using System.Collections.Generic;
using GitCommands;

namespace GitUI
{
    public class GitFileTreeComparer : IComparer<IGitItem>
    {
        public int Compare(IGitItem x, IGitItem y)
        {
            var xGitItem = (GitItem)x;
            var yGitItem = (GitItem)y;

            if ((xGitItem.ItemType == "tree" || xGitItem.ItemType == "commit") && yGitItem.ItemType == "blob")
                return -1;
            if (xGitItem.ItemType == "blob" && (yGitItem.ItemType == "tree" || yGitItem.ItemType == "commit"))
                return 1;
            return xGitItem.Name.CompareTo(yGitItem.Name);
        }
    }
}
