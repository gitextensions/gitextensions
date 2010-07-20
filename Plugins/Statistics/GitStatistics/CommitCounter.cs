using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitStatistics
{
    public class CommitCounter
    {
        private readonly IGitUIEventArgs _gitUiEventArgs;

        public int TotalCommits;
        public Dictionary<string, int> UserCommitCount = new Dictionary<string, int>();

        public CommitCounter(IGitUIEventArgs gitUiEventArgs)
        {
            _gitUiEventArgs = gitUiEventArgs;
        }

        public void Count()
        {
            var userCommitCounts =
                _gitUiEventArgs.GitUICommands
                    .CommandLineCommand("cmd.exe",
                                        string.Format(
                                            "/c \"\"{0}\" log --all --pretty=short | \"{1}\" shortlog --all -s -n\"",
                                            _gitUiEventArgs.GitCommand, _gitUiEventArgs.GitCommand))
                    .Split('\n');

            foreach (var userCommitCount in userCommitCounts)
            {
                var commitCount = userCommitCount.Trim(); //remove whitespaces at start and end
                var tab = commitCount.IndexOfAny(new[] {' ', '\t'}); //find space or tab

                if (tab <= 0)
                    continue;
                int count;
                if (!int.TryParse(commitCount.Substring(0, tab), out count))
                    continue;

                var user = commitCount.Substring(tab + 1);
                TotalCommits += count;
                UserCommitCount.Add(user, count);
            }
        }
    }
}