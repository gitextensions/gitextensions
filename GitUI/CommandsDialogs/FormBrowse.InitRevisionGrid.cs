using System.Linq;
using GitCommands;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    partial class FormBrowse
    {
        // This file is dedicated to init logic for FormBrowse revisiong grid control

        private void InitRevisionGrid(ObjectId? selectedId, ObjectId? firstId, bool isBlame)
        {
            RevisionGrid.IndexWatcher.Changed += (_, args) =>
            {
                bool indexChanged = args.IsIndexChanged;
                this.InvokeAsync(
                        () =>
                        {
                            RefreshButton.Image = indexChanged && AppSettings.ShowGitStatusInBrowseToolbar && Module.IsValidGitWorkingDir()
                                ? Images.ReloadRevisionsDirty
                                : Images.ReloadRevisions;
                        })
                    .FileAndForget();
            };
            RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();
            RevisionGrid.FilterChanged += (sender, e) =>
            {
                Text = _appTitleGenerator.Generate(Module.WorkingDir, Module.IsValidGitWorkingDir(), RevisionGrid.CurrentBranch.Value, TranslatedStrings.NoBranch, e.PathFilter);

                // PathFilter is a free text field and may contain wildcards, quoting is optional.
                // This is will adjust the string at least for paths added from context menus.
                string? path = e.PathFilter;
                if (path is not null && path.Length > 1 && path[0] == '"' && path[path.Length - 1] == '"')
                {
                    path = path[1..(path.Length - 1)];
                }

                revisionDiff.FallbackFollowedFile = path;
                fileTree.FallbackFollowedFile = path;
            };
            RevisionGrid.GridLoading += (sender, e) =>
            {
                // The FileTree tab should be shown at first start, in "filehistory" mode
                if (isBlame)
                {
                    CommitInfoTabControl.SelectedTab = TreeTabPage;
                    isBlame = false;
                }

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
                repoObjectsTree.Refresh(isFiltering, e.ForceRefresh, e.GetRefs);
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
