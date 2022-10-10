using GitUI.UserControls.RevisionGrid;

namespace GitUI
{
    public class FilterChangedEventArgs : EventArgs
    {
        public FilterChangedEventArgs(FilterInfo filter)
        {
            ShowAllBranches = filter.IsShowAllBranchesChecked;
            ShowCurrentBranchOnly = filter.IsShowCurrentBranchOnlyChecked;
            ShowFilteredBranches = filter.IsShowFilteredBranchesChecked;
            ShowOnlyFirstParent = filter.ShowOnlyFirstParent;
            ShowReflogReferences = filter.ShowReflogReferences;
            HasFilter = filter.HasFilter;
            PathFilter = filter.PathFilter;
            BranchFilter = filter.BranchFilter;
            MessageFilter = filter.Message;
            CommitterFilter = filter.Committer;
            AuthorFilter = filter.Author;
            DiffContentFilter = filter.DiffContent;
            FilterSummary = filter.GetSummary();
        }

        public bool ShowAllBranches { get; }
        public bool ShowCurrentBranchOnly { get; }
        public bool ShowFilteredBranches { get; }
        public bool ShowOnlyFirstParent { get; }
        public bool ShowReflogReferences { get; }
        public bool HasFilter { get; }

        /// <summary>
        ///  Gets the currently applied Path filter.
        /// </summary>
        public string PathFilter { get; }

        /// <summary>
        ///  Gets the currently applied Branch filter.
        /// </summary>
        public string BranchFilter { get; }

        /// <summary>
        ///  Gets the currently applied Message filter.
        /// </summary>
        public string MessageFilter { get; }

        /// <summary>
        ///  Gets the currently applied Committer filter.
        /// </summary>
        public string CommitterFilter { get; }

        /// <summary>
        ///  Gets the currently applied Author filter.
        /// </summary>
        public string AuthorFilter { get; }

        /// <summary>
        ///  Gets the currently applied Diff contains filter.
        /// </summary>
        public string DiffContentFilter { get; }

        /// <summary>
        ///  Gets a summary of the current filter.
        /// </summary>
        public string FilterSummary { get; }
    }
}
