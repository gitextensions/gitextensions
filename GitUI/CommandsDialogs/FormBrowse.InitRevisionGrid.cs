using GitCommands;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    partial class FormBrowse
    {
        // This file is dedicated to init logic for FormBrowse revisiong grid control

        private void InitRevisionGrid(ObjectId? selectedId, ObjectId? firstId)
        {
            RevisionGrid.IndexWatcher.Changed += (_, args) =>
            {
                bool indexChanged = args.IsIndexChanged;
                this.InvokeAsync(
                        () =>
                        {
                            RefreshButton.Image = indexChanged && AppSettings.UseFastChecks && Module.IsValidGitWorkingDir()
                                ? Images.ReloadRevisionsDirty
                                : Images.ReloadRevisions;
                        })
                    .FileAndForget();
            };
            RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();
            RevisionGrid.PathFilterChanged += revisionGrid_PathFilterChanged;
            RevisionGrid.RefFilterOptionsChanged += (sender, e) =>
            {
                if (e.RefFilterOptions.HasFlag(RefFilterOptions.All | RefFilterOptions.Boundary))
                {
                    // This means show all branches
                    ToolStripFilters.ClearFilters();
                    AppSettings.BranchFilterEnabled =
                        AppSettings.ShowCurrentBranchOnly = false;
                }
            };
            RevisionGrid.RevisionGraphLoaded += (sender, e) =>
            {
                if (sender is null || MainSplitContainer.Panel1Collapsed)
                {
                    // - the event is either not originated from the revision grid, or
                    // - the left panel is hidden
                    return;
                }

                // Apply filtering when:
                // 1. don't show reflogs, and
                // 2. one of the following
                //      a) show the current branch only, or
                //      b) filter on specific branch
                bool isFiltering = !AppSettings.ShowReflogReferences
                                && (AppSettings.ShowCurrentBranchOnly || AppSettings.BranchFilterEnabled);
                repoObjectsTree.ToggleFilterMode(isFiltering);
            };
            RevisionGrid.SelectionChanged += (sender, e) =>
            {
                _selectedRevisionUpdatedTargets = UpdateTargets.None;
                RefreshSelection();
            };
            RevisionGrid.ToggledBetweenArtificialAndHeadCommits += (s, e) =>
            {
                if (!revisionDiff.Visible)
                {
                    CommitInfoTabControl.SelectedTab = DiffTabPage;
                }

                if (revisionDiff.Visible)
                {
                    // force focus of file list
                    revisionDiff.SwitchFocus(alreadyContainedFocus: false);
                }
            };

            RevisionGrid.SelectedId = selectedId;
            RevisionGrid.FirstId = firstId;
        }
    }
}
