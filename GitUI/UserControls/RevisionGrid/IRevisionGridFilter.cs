using System;
using System.ComponentModel;

namespace GitUI.UserControls.RevisionGrid
{
    /// <summary>
    ///  Defines filtering functionality that affects presentation of information in the revision grid control.
    /// </summary>
    public interface IRevisionGridFilter
    {
        /// <summary>
        ///  Occurs whenever filter changes.
        /// </summary>
        event EventHandler<FilterChangedEventArgs>? FilterChanged;

        /// <summary>
        ///  Applies a branch filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        void SetAndApplyBranchFilter(string filter);

        /// <summary>
        ///  Applies a revision filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <exception cref="InvalidOperationException">Invalid 'diff contains' filter.</exception>
        void SetAndApplyRevisionFilter(RevisionFilter filter);

        void ShowAllBranches();

        void ShowCurrentBranchOnly();

        void ShowFilteredBranches();

        void ShowRevisionFilterDialog();

        void ToggleShowFirstParent();

        void ToggleShowReflogReferences();
    }
}
