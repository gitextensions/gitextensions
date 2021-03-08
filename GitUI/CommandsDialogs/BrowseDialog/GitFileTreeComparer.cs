using System.Collections.Generic;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public class GitFileTreeComparer : IComparer<IGitItem>
    {
        public int Compare(IGitItem x, IGitItem y)
        {
            return (x as GitItem, y as GitItem) switch
            {
                (null, null) => 0,
                (null, _) => 1,
                (_, null) => -1,
                var (xGitItem, yGitItem) => (xGitItem.ObjectType, yGitItem.ObjectType) switch
                {
                    (GitObjectType.Tree or GitObjectType.Commit, GitObjectType.Blob) => -1,
                    (GitObjectType.Blob, GitObjectType.Tree or GitObjectType.Commit) => 1,
                    _ => xGitItem.Name.CompareTo(yGitItem.Name)
                }
            };
        }
    }
}
