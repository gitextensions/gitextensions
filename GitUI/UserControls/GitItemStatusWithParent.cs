using GitCommands;

namespace GitUI.UserControls
{
    public sealed class GitItemStatusWithParent
    {
        public GitItemStatusWithParent(GitRevision parent, GitItemStatus item)
        {
            ParentRevision = parent;
            Item = item;
        }

        public GitRevision ParentRevision { get; }
        public GitItemStatus Item { get; }
    }
}