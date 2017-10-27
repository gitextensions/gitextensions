using System.Collections.Generic;
using GitCommands;
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

            if ((xGitItem.IsTree || xGitItem.IsCommit) && yGitItem.IsBlob)
                return -1;
            if (xGitItem.IsBlob && (yGitItem.IsTree || yGitItem.IsCommit))
                return 1;
            return xGitItem.Name.CompareTo(yGitItem.Name);
        }
    }
}
