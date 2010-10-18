using System;
using System.Collections.Generic;

namespace GitCommands.Statistics
{
    public class CommitCounter
    {
        public static Tuple<Dictionary<string, int>, int> GroupAllCommitsByContributer()
        {
            return GroupAllCommitsByContributer(DateTime.MinValue, DateTime.MaxValue);
        }

        public static Tuple<Dictionary<string, int>, int> GroupAllCommitsByContributer(DateTime since, DateTime until)
        {
            var sinceParam = since != DateTime.MinValue ? GetDateParameter(since, "since") : "";
            var untilParam = until != DateTime.MaxValue ? GetDateParameter(since, "until") : "";

            var unformattedCommitsPerContributor =
                GitCommandHelpers
                    .RunCmd(
                        Settings.GitCommand,
                        "shortlog --all -s -n --no-merges" + sinceParam + untilParam)
                    .Split('\n');

            return ParseCommitsPerContributor(unformattedCommitsPerContributor);
        }

        public static Tuple<Dictionary<string, int>, int> ParseCommitsPerContributor(
            IEnumerable<string> unformattedCommitsPerContributor)
        {
            var commitsPerContributor = new Dictionary<string, int>();
            var delimiter = new[] {' ', '\t'};
            var totalCommits = 0;

            foreach (var userCommitCount in unformattedCommitsPerContributor)
            {
                var commitCount = userCommitCount.Trim(); //remove whitespaces at start and end

                var tab = commitCount.IndexOfAny(delimiter); //find space or tab

                if (tab <= 0)
                    continue;

                int count;
                if (!int.TryParse(commitCount.Substring(0, tab), out count))
                    continue;

                var contributor = commitCount.Substring(tab + 1);

                totalCommits += count;
                commitsPerContributor.Add(contributor, count);
            }
            return Tuple.Create(commitsPerContributor, totalCommits);
        }

        private static string GetDateParameter(DateTime sinceDate, string paramName)
        {
            return string.Format(" --{1}=\"{0}\"", sinceDate.ToString("yyyy-MM-dd hh:mm:ss"), paramName);
        }
    }
}