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
        event PropertyChangedEventHandler? FilterChanged;

        /// <summary>
        ///  Applies a branch filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="requireRefresh">
        ///  <see langword="true"/> to refresh the grid, if the filter applied successfully; <see langword="false"/> to not refresh the grid at all.
        /// </param>
        void SetAndApplyBranchFilter(string filter, bool requireRefresh);

        /// <summary>
        ///  Applies a revision filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <exception cref="InvalidOperationException">Invalid 'diff contains' filter.</exception>
        void SetAndApplyRevisionFilter(RevisionFilter filter);

        void ShowRevisionFilterDialog();

        void ToggleShowFirstParent();

        void ToggleShowReflogReferences();
    }
}
