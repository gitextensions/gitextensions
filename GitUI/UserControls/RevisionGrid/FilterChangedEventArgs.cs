using System;
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
            ShowFirstParent = filter.ShowFirstParent;
            ShowReflogReferences = filter.ShowReflogReferences;
            PathFilter = filter.PathFilter;
        }

        public bool ShowAllBranches { get; }
        public bool ShowCurrentBranchOnly { get; }
        public bool ShowFilteredBranches { get; }
        public bool ShowFirstParent { get; }
        public bool ShowReflogReferences { get; }

        /// <summary>
        ///  Gets the currently applied Path filter.
        /// </summary>
        public string PathFilter { get; }
    }
}
