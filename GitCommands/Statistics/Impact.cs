using System;
using System.Collections.Generic;

namespace GitCommands.Statistics
{
    public class Impact
    {
        public struct DataPoint
        {
            public int Commits, AddedLines, DeletedLines;

            public int ChangedLines
            {
                get { return AddedLines + DeletedLines; }
            }

            public DataPoint(int commits, int added, int deleted) 
            {
                this.Commits = commits;
                this.AddedLines = added;
                this.DeletedLines = deleted;
            }
        }

        public static SortedDictionary<DateTime, Dictionary<string, DataPoint>> GetImpact()
        {
            SortedDictionary<DateTime, Dictionary<string, DataPoint>> impacts =
                new SortedDictionary<DateTime, Dictionary<string, DataPoint>>();

            // --- 2010-11-03 16:01:58 +0100 --- Author A
            //
            // 17	4	GitUI/FormRebase.Designer.cs
            // 1	1	GitUI/FormRebase.cs
            // --- 2010-11-03 15:57:29 +0100 --- Author B
            //
            // 4	1	GitCommands/Git/GitCommandsHelper.cs

            string[] log = GitCommandHelpers.RunCmd(GitCommands.Settings.GitCommand,
                            "log --pretty=tformat:\"--- %ad --- %an\" --numstat --date=iso -C").Split('\n');

            // Analyze commit listing
            for (int i = 0; i < log.Length; i++)
            {
                // Look for commit delimiters
                if (!log[i].StartsWith("--- "))
                    continue;
                    
                // Strip "--- " 
                log[i] = log[i].Substring(4);

                // Split date and author
                string[] header = log[i].Split(new string[]{" --- "}, 2, StringSplitOptions.RemoveEmptyEntries);
                if (header.Length != 2)
                    continue;

                // Save author in variable
                string author = header[1];

                // Parse commit date
                DateTime date = DateTime.Parse(header[0]).Date;
                // Calculate first day of the commit week
                date = date.AddDays(-(int)date.DayOfWeek);

                // Skip empty line
                i++;

                // Parse commit lines
                int added = 0;
                int deleted = 0;
                while (i < log.Length - 1 && !log[i + 1].StartsWith("--- "))
                {
                    // Jump to next lines that does not start with "--- "
                    i++;

                    string[] line = log[i].Split('\t');
                    if (line.Length >= 2)
                    {
                        if (line[0] != "-")
                            added += int.Parse(line[0]);
                        if (line[1] != "-")
                            deleted += int.Parse(line[1]);

                    }
                }

                // If week does not exist yet in the impact dictionary
                if (!impacts.ContainsKey(date))
                {
                    // Create it
                    impacts.Add(date, new Dictionary<string, DataPoint>());
                }

                // If author does not exist yet for this week in the impact dictionary
                if (!impacts[date].ContainsKey(author))
                {
                    // Create it
                    impacts[date].Add(author, new DataPoint(1, added, deleted));
                }
                else
                {
                    // Otherwise just add the changes
                    impacts[date][author] = new DataPoint(impacts[date][author].Commits + 1,
                                                            impacts[date][author].AddedLines + added,
                                                            impacts[date][author].DeletedLines + deleted);

                }
            }

            return impacts;
        }

        public static Dictionary<string, DataPoint> GetAuthors(SortedDictionary<DateTime, Dictionary<string, DataPoint>> impact)
        {
            Dictionary<string, DataPoint> authors = new Dictionary<string, DataPoint>();

            foreach (var week in impact)
            {
                foreach (var pair in week.Value)
                {
                    string author = pair.Key;

                    if (!authors.ContainsKey(author)) 
                        authors.Add(author, new DataPoint(0, 0, 0));

                    authors[author] = new DataPoint(authors[author].Commits + pair.Value.Commits,
                                                    authors[author].AddedLines + pair.Value.AddedLines,
                                                    authors[author].DeletedLines + pair.Value.DeletedLines);

                }
            }

            return authors;
        }

        public static void AddIntermediateEmptyWeeks(
            ref SortedDictionary<DateTime, Dictionary<string, DataPoint>> impact, Dictionary<string, DataPoint> authors)
        {
            foreach (var author_data in authors)
            {
                string author = author_data.Key;

                // Determine first and last commit week of each author
                DateTime start = new DateTime(), end = new DateTime();
                bool start_found = false;
                foreach (var week in impact)
                {
                    if (week.Value.ContainsKey(author))
                    {
                        if (!start_found)
                        {
                            start = week.Key;
                            start_found = true;
                        }
                        end = week.Key;
                    }
                }
                if (!start_found)
                    continue;

                // Add 0 commits weeks in between
                foreach (var week in impact)
                    if (!week.Value.ContainsKey(author) &&
                        week.Key > start && week.Key < end)
                        week.Value.Add(author, new DataPoint(0, 0, 0));
            }
        }
    }
}
