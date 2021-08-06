namespace GitUI.UserControls
{
    public class RevisionFilter
    {
        // TODO: consider using a bit vector or an enum

        public RevisionFilter(string filter, bool byCommit, bool byCommitter, bool byAuthor, bool byDiffContent)
        {
            Filter = filter;
            FilterByCommit = byCommit;
            FilterByCommitter = byCommitter;
            FilterByAuthor = byAuthor;
            FilterByDiffContent = byDiffContent;
        }

        public string Filter { get; }
        public bool FilterByCommit { get; }
        public bool FilterByCommitter { get; }
        public bool FilterByAuthor { get; }
        public bool FilterByDiffContent { get; }
    }
}
