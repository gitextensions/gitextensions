using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GitCommands.Statistics
{
    public sealed class ImpactLoader : IDisposable
    {
        public delegate void UpdateEventHandler(Commit commit);

        public event EventHandler Exited;
        public event EventHandler Error;
        public event UpdateEventHandler Updated;

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

            public static DataPoint operator +(DataPoint d1, DataPoint d2)
            {
                DataPoint temp = new DataPoint();
                temp.Commits = d1.Commits + d2.Commits;
                temp.AddedLines = d1.AddedLines + d2.AddedLines;
                temp.DeletedLines = d1.DeletedLines + d2.DeletedLines;
                return temp;
            }
        }

        public struct Commit
        {
            public DateTime week;
            public string author;
            public DataPoint data;
        }

        private GitCommandsInstance git;

        private Thread backgroundThread = null;

        ~ImpactLoader()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
                backgroundThread = null;
            }
            if (git != null)
            {
                git.Dispose();
                git = null;
            }
        }

        public void Execute()
        {
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
            }
            backgroundThread = new Thread(new ThreadStart(execute));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        private void execute()
        {
            try
            {
                string command = "log --pretty=tformat:\"--- %ad --- %an\" --numstat --date=iso -C --all --no-merges";

                git = new GitCommandsInstance();
                git.StreamOutput = true;
                git.CollectOutput = false;
                Process p = git.CmdStartProcess(Settings.GitCommand, command);

                // Read line
                string line = p.StandardOutput.ReadLine();

                // Analyze commit listing
                while (true)
                {
                    Commit commit = new Commit();

                    // Reached the end ?
                    if (line == null)
                        break;

                    // Look for commit delimiters
                    if (!line.StartsWith("--- "))
                    {
                        line = p.StandardOutput.ReadLine();
                        continue;
                    }

                    // Strip "--- " 
                    line = line.Substring(4);

                    // Split date and author
                    string[] header = line.Split(new string[] { " --- " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (header.Length != 2)
                        continue;

                    // Save author in variable
                    string author = commit.author = header[1];

                    // Parse commit date
                    DateTime date = DateTime.Parse(header[0]).Date;
                    // Calculate first day of the commit week
                    date = commit.week = date.AddDays(-(int)date.DayOfWeek);

                    // Reset commit data
                    commit.data.Commits = 1;
                    commit.data.AddedLines = 0;
                    commit.data.DeletedLines = 0;

                    // Parse commit lines
                    while ((line = p.StandardOutput.ReadLine()) != null && !line.StartsWith("--- "))
                    {
                        // Skip empty line
                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] file_line = line.Split('\t');
                        if (file_line.Length >= 2)
                        {
                            if (file_line[0] != "-")
                                commit.data.AddedLines += int.Parse(file_line[0]);
                            if (file_line[1] != "-")
                                commit.data.DeletedLines += int.Parse(file_line[1]);
                        }
                    }

                    if (Updated != null)
                        Updated(commit);
                }
            }
            catch (ThreadAbortException)
            {
                //Silently ignore this exception...
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error(this, EventArgs.Empty);
                MessageBox.Show("Cannot load commit log." + Environment.NewLine + Environment.NewLine + ex.ToString());
                return;
            }

            if (Exited != null)
                Exited(this, EventArgs.Empty);
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
