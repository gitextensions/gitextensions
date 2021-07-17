using System.Collections.Generic;
using System.IO;
using GitCommands.Git.Commands;
using GitExtUtils;
using Microsoft;

namespace GitCommands.Git.WorkTrees
{
    /// <summary>
    /// Provides a parser for output of <see cref="GitCommandHelpers.ListWorkTreeCmd"/> command.
    /// </summary>
    public class ListWorkTreeOutputParser
    {
        /// <summary>
        /// Parse the output from <see cref="GitCommandHelpers.ListWorkTreeCmd"/> command
        /// </summary>
        /// <param name="listWorkTreeCommandOutput">An output of <see cref="GitCommandHelpers.ListWorkTreeCmd"/> command.</param>
        /// <returns>list with the parsed GitWorkTree.</returns>
        /// <seealso href="https://git-scm.com/docs/git-worktree"/>
        public static List<GitWorkTree> Parse(string listWorkTreeCommandOutput)
        {
            // Here are the 3 types of lines return by the `worktree list --porcelain` that should be handled:
            //
            // 1:
            // worktree /path/to/bare-source
            // bare
            //
            // 2:
            // worktree /path/to/linked-worktree
            // HEAD abcd1234abcd1234abcd1234abcd1234abcd1234
            // branch refs/heads/master
            //
            // 3:
            // worktree /path/to/other-linked-worktree
            // HEAD 1234abc1234abc1234abc1234abc1234abc1234a
            // detached.
            List<GitWorkTree> result = new();
            GitWorkTree? currentWorktree = null;
            foreach (string line in listWorkTreeCommandOutput.LazySplit('\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] strings = line.Split(' ');
                switch (strings[0])
                {
                    case "worktree":
                        currentWorktree = new GitWorkTree { Path = line.Substring(9) };
                        currentWorktree.IsDeleted = !Directory.Exists(currentWorktree.Path);
                        result.Add(currentWorktree);
                        break;
                    case "HEAD":
                        Validates.NotNull(currentWorktree);
                        currentWorktree.Sha1 = strings[1];
                        break;
                    case "bare":
                        Validates.NotNull(currentWorktree);
                        currentWorktree.Type = HeadType.Bare;
                        break;
                    case "branch":
                        Validates.NotNull(currentWorktree);
                        currentWorktree.Type = HeadType.Branch;
                        currentWorktree.CompleteBranchName = strings[1];
                        currentWorktree.BranchName = strings[1].Replace(GitRefName.RefsHeadsPrefix, string.Empty);
                        break;
                    case "detached":
                        Validates.NotNull(currentWorktree);
                        currentWorktree.Type = HeadType.Detached;
                        break;
                }
            }

            return result;
        }
    }
}
