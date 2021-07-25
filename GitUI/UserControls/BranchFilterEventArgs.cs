namespace GitUI.UserControls
{
    internal class BranchFilterEventArgs
    {
        public BranchFilterEventArgs(string filter, bool requireRefresh)
        {
            Filter = filter;
            RequireRefresh = requireRefresh;
        }

        public string Filter { get; }
        public bool RequireRefresh { get; }
    }
}
