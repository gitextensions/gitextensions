using System;
using System.Collections.Generic;

namespace GitCommands.Statistics
{
    public class CommitCounter
    {
        public static Tuple<Dictionary<string, int>, int> GroupAllCommitsByContributer()
        {
            var commitsPerContributor = new Dictionary<string, int>();
            var unformattedCommitsPerContributor =
                GitCommands
                    .RunCmd(Settings.GitCommand, "shortlog --all -s -n --no-merges")
                    .Split('\n');

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
    }
}