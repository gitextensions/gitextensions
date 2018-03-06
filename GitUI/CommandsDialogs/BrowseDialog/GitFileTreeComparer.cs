using System.Collections.Generic;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public class GitFileTreeComparer : IComparer<IGitItem>
    {
        public int Compare(IGitItem x, IGitItem y)
        {
            var xGitItem = x as GitItem;
            var yGitItem = y as GitItem;
            if (xGitItem == null && yGitItem == null)
            {
                return 0;
            }

            if (xGitItem == null)
            {
                return 1;
            }

            if (yGitItem == null)
            {
                return -1;
            }

            if ((xGitItem.ObjectType == GitObjectType.Tree || xGitItem.ObjectType == GitObjectType.Commit) && yGitItem.ObjectType == GitObjectType.Blob)
            {
                return -1;
            }

            if (xGitItem.ObjectType == GitObjectType.Blob && (yGitItem.ObjectType == GitObjectType.Tree || yGitItem.ObjectType == GitObjectType.Commit))
            {
                return 1;
            }

            return xGitItem.Name.CompareTo(yGitItem.Name);
        }
    }
}
