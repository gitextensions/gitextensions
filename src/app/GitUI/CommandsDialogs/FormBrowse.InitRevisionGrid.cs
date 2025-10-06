using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse revisiong grid control

    private void InitRevisionGrid(ObjectId? selectedId, ObjectId? firstId, bool isFileHistoryMode)
    {
        RevisionGrid.IndexWatcher.Changed += (_, args) =>
        {
            bool indexChanged = args.IsIndexChanged;
            this.InvokeAndForget(() =>
                RefreshButton.Image = indexChanged && AppSettings.ShowGitStatusInBrowseToolbar && Module.IsValidGitWorkingDir()
                    ? Images.ReloadRevisionsDirty
                    : Images.ReloadRevisions);
        };

        RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();

        RevisionGrid.FilterChanged += (sender, e) =>
        {
            Text = _appTitleGenerator.Generate(Module.WorkingDir, Module.IsValidGitWorkingDir(), RevisionGrid.CurrentBranch.Value, TranslatedStrings.NoBranch, e.PathFilter);

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
                CommitInfoTabControl.SelectedTab = DiffTabPage;
            }

            if (sender is null || MainSplitContainer.Panel1Collapsed)
            {
                // - the event is either not originated from the revision grid, or
                // - the left panel is hidden
                return;
            }

            RefreshLeftPanel(e.GetRefs, e.GetStashRevs, e.ForceRefresh);
        };

        RevisionGrid.RevisionsLoaded += (sender, e) =>
        {
            if (sender is null || MainSplitContainer.Panel1Collapsed)
            {
                // - the event is either not originated from the revision grid, or
                // - the left panel is hidden
                return;
            }

            repoObjectsTree.RefreshRevisionsLoaded();
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

        RevisionGrid.SelectInLeftPanel = SelectInLeftPanel;

        return;

        void SelectInLeftPanel(string gitRef)
        {
            if (MainSplitContainer.Panel1Collapsed)
            {
                toggleLeftPanel.PerformClick();
            }

            repoObjectsTree.SelectGitRef(gitRef);
            repoObjectsTree.Focus();
        }
    }
}
