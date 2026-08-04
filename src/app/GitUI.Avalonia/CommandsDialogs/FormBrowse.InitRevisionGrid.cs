using Avalonia.Controls;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse revisiong grid control

    private void InitRevisionGrid(ObjectId selectedId, ObjectId firstId, bool isFileHistoryMode)
    {
        RevisionGrid.ArtificialChanged += (_, _) => RefreshGitStatusMonitor();

        RevisionGrid.IndexWatcher.Changed += (_, args) =>
        {
            bool indexChanged = args.IsIndexChanged;
            Dispatcher.UIThread.Post(() =>
                ((Image)RefreshButton.Content!).Source =
                    indexChanged && AppSettings.ShowGitStatusInBrowseToolbar && Module.IsValidGitWorkingDir()
                        ? Images.ReloadRevisionsDirty
                        : Images.ReloadRevisions);
        };

        RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus?.OnMenuCommandsPropertyChanged();

        RevisionGrid.FilterChanged += (sender, e) =>
        {
            IAppTitleGenerator appTitleGenerator = UICommands.GetRequiredService<IAppTitleGenerator>();
            Title = appTitleGenerator.Generate(Module.WorkingDir, Module.IsValidGitWorkingDir(), RevisionGrid.GetCurrentBranch(), TranslatedStrings.NoBranch, e.PathFilter);

            // PathFilter is a free text field and may contain wildcards, quoting is optional.
            // This is will adjust the string at least for paths added from context menus.
            string? path = e.PathFilter;
            if (path?.Length is > 1 && path[0] == '"' && path[^1] == '"')
            {
                path = path[1..^1];
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                RelativePath relativePath = RelativePath.From(path);
                revisionDiff.FallbackFollowedFile = relativePath;
                fileTree.FallbackFollowedFile = relativePath;
            }
        };

        bool firstTimeInFileHistoryMode = isFileHistoryMode;
        RevisionGrid.RevisionsLoading += (sender, e) =>
        {
            // Open diff in "filehistory" mode
            if (firstTimeInFileHistoryMode)
            {
                firstTimeInFileHistoryMode = false;
                CommitInfoTabControl.SelectedItem = DiffTabPage;
            }

            if (sender is null || !leftPanel.IsVisible)
            {
                // - the event is either not originated from the revision grid, or
                // - the left panel is hidden
                return;
            }

            RefreshLeftPanel(sender, e);
        };

        RevisionGrid.RevisionsLoaded += (sender, e) =>
        {
            _repositoryHistoryUIService?.TriggerBranchNameCacheUpdate();

            if (sender is null || !leftPanel.IsVisible)
            {
                // - the event is either not originated from the revision grid, or
                // - the left panel is hidden
                return;
            }

            // The Avalonia tree receives the completed ref set atomically from RefreshLeftPanel.
        };

        RevisionGrid.SelectionChanged += (sender, e) =>
        {
            _selectedRevisionUpdatedTargets = UpdateTargets.None;

            // Avalonia clears an empty file tree at the revision boundary so a later tab switch keeps keyboard focus.
            if (RevisionGrid.SelectedRevision is null)
            {
                fileTree.Clear();
            }

            RevisionGrid_SelectionChanged(sender, e);
        };

        RevisionGrid.SelectedId = selectedId.IsZero ? firstId : selectedId;

        // force focus of file list
        // The native grid does not expose the original dedicated artificial/head toggle event.

        // The native tree does not yet expose the ref-node lookup required by SelectInLeftPanel,
        // so the grid keeps this optional callback unset instead of selecting the wrong surface.
    }
}
