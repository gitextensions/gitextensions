using System;
using System.Collections.Generic;

namespace GitCommands.Statistics
{
    public class CommitCounter
    {
        public static Tuple<Dictionary<string, int>, int> GroupAllCommitsByUser()
        {
            var commitsPerUser = new Dictionary<string, int>();
            var unformattedCommitsPerUser =
                GitCommands
                    .RunCmd("cmd.exe",
                            string.Format(
                                "/c \"\"{0}\" log --all --pretty=short | \"{0}\" shortlog --all -s -n\"",
                                Settings.GitCommand))
                    .Split('\n');

            var delimiter = new[] {' ', '\t'};
            var totalCommits = 0;

            foreach (var userCommitCount in unformattedCommitsPerUser)
            {
                var commitCount = userCommitCount.Trim(); //remove whitespaces at start and end

                var tab = commitCount.IndexOfAny(delimiter); //find space or tab

                if (tab <= 0)
                    continue;

                int count;
                if (!int.TryParse(commitCount.Substring(0, tab), out count))
                    continue;

                var user = commitCount.Substring(tab + 1);

                totalCommits += count;
                if (!commitsPerUser.ContainsKey(user))
                    commitsPerUser.Add(user, 0);
                commitsPerUser[user] += count;
            }
            return Tuple.Create(commitsPerUser, totalCommits);
        }
    }
}