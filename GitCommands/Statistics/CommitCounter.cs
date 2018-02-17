﻿using System;
using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitCommands.Statistics
{
    public static class CommitCounter
    {
        public static Tuple<Dictionary<string, int>, int> GroupAllCommitsByContributor(IGitModule module)
        {
            return GroupAllCommitsByContributor(module, DateTime.MinValue, DateTime.MaxValue);
        }

        private static Tuple<Dictionary<string, int>, int> GroupAllCommitsByContributor(IGitModule module, DateTime since, DateTime until)
        {
            var sinceParam = since != DateTime.MinValue ? GetDateParameter(since, "since") : "";
            var untilParam = until != DateTime.MaxValue ? GetDateParameter(since, "until") : "";

            var unformattedCommitsPerContributor =
                module.RunGitCmd(
                        "shortlog --all -s -n --no-merges" + sinceParam + untilParam)
                    .Split('\n');

            return ParseCommitsPerContributor(unformattedCommitsPerContributor);
        }

        private static Tuple<Dictionary<string, int>, int> ParseCommitsPerContributor(
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

                if (!int.TryParse(commitCount.Substring(0, tab), out var count))
                    continue;

                var contributor = commitCount.Substring(tab + 1);

                totalCommits += count;

                if (!commitsPerContributor.TryGetValue(contributor, out var oldCount))
                    commitsPerContributor.Add(contributor, count);
                else
                    // Sometimes this happen because of wrong encoding
                    commitsPerContributor[contributor] = oldCount + count;
            }
            return Tuple.Create(commitsPerContributor, totalCommits);
        }

        private static string GetDateParameter(DateTime sinceDate, string paramName)
        {
            return string.Format(" --{1}=\"{0}\"", sinceDate.ToString("yyyy-MM-dd hh:mm:ss"), paramName);
        }
    }
}