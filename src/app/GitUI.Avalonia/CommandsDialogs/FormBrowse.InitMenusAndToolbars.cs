using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Properties;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse menus and toolbars

    internal static readonly string FetchPullToolbarShortcutsPrefix = "pull_shortcut_";

    private void InitMenusAndToolbars(string? revFilter, string? pathFilter)
    {
        if (_hasRuntimeCommands)
        {
            commandsToolStripMenuItem.SubmenuOpened += CommandsToolStripMenuItem_SubmenuOpened;
        }

        InitFilters();

        ((Image)recoverLostObjectsToolStripMenuItem.Icon!).Source = Images.RecoverLostObjects.AdaptLightness(); // Repository->Git maintenance->Recover lost objects
        branchSelect.Icon = Images.Branch.AdaptLightness(); // main toolbar

        pullToolStripMenuItem1.Tag = GitPullAction.None;
        mergeToolStripMenuItem.Tag = GitPullAction.Merge;
        rebaseToolStripMenuItem1.Tag = GitPullAction.Rebase;
        fetchToolStripMenuItem.Tag = GitPullAction.Fetch;
        fetchAllToolStripMenuItem.Tag = GitPullAction.FetchAll;
        fetchPruneAllToolStripMenuItem.Tag = GitPullAction.FetchPruneAll;

        UpdateCommitButtonAndGetBrush(status: null, AppSettings.ShowGitStatusInBrowseToolbar);

        FillNextPullActionAsDefaultToolStripMenuItems();
        RefreshDefaultPullAction();

        FillUserShells(defaultShell: "Git bash");

        InsertFetchPullShortcuts();

        ((MenuFlyout)toolStripButtonPull.Flyout!).Opening += (_, _) => UpdateFetchAllVisibility();

        // Layout engine bug (?) which may change the order of toolbars
        // if the 1st one becomes longer than the 2nd toolbar's Location.X
        // the layout engine will be place the 2nd toolbar first
        // 1. Clear all toolbars
        // 2. Add all the toolbars back in a reverse order, every added toolbar pushing existing ones to the right
        // 3. Assert all toolbars on the same row
        // 4. Assert the correct order of toolbars
        // Avalonia's ordered WrapPanel does not have the ToolStripPanel location bug, so it needs no reorder pass.

        UpdateTooltipWithShortcut(toggleLeftPanel, Command.ToggleLeftPanel);
        UpdateTooltipWithShortcut(toolStripButtonCommit, Command.Commit);
        UpdateTooltipWithShortcut(EditSettings, Command.OpenSettings);
        UpdateTooltipWithShortcut(branchSelect, Command.CheckoutBranch);
        UpdateTooltipWithShortcut(RefreshButton, new KeyGesture(Key.F5));
        UpdateTooltipWithShortcut(userShell, Command.GitBash);

        return;

        void InitFilters()
        {
            // ToolStripFilters.RefreshRevisionFunction() is init in UICommands_PostRepositoryChanged

            if (!string.IsNullOrWhiteSpace(revFilter))
            {
                ToolStripFilters.SetRevisionFilter(revFilter);
            }

            if (!string.IsNullOrWhiteSpace(pathFilter))
            {
                RevisionGrid.SetAndApplyPathFilter(pathFilter.QuoteNE());
            }
        }
    }

    private void UpdateWorktreeToolStripVisibility()
    {
        if (!Module.IsValidGitWorkingDir())
        {
            toolStripWorktrees.IsVisible = false;
            return;
        }

        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            await TaskScheduler.Default;
            IReadOnlyList<GitWorktree> worktrees = Module.GetWorktrees();

            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _worktrees = worktrees;
            toolStripWorktrees.IsVisible = worktrees.Count > 1;
        });
    }

    private void UpdateTooltipWithShortcut(Control button, Command command)
        => UpdateTooltipWithShortcut(button, KeysMapper.ToKeyGesture(GetShortcutKeys(command)));

    private static void UpdateTooltipWithShortcut(Control button, KeyGesture? keys)
    {
        string text = ToolTip.GetTip(button)?.ToString() ?? button.Name ?? string.Empty;
        ToolTip.SetTip(button, keys is null ? text : $"{text} ({keys})");
    }

    private void InsertFetchPullShortcuts()
    {
        int i = ToolStripMain.Children.IndexOf(toolStripButtonPull);
        ToolStripMain.Children.Insert(i++, CreateCorrespondingToolbarButton(fetchToolStripMenuItem, Images.PullFetch, Command.QuickFetch));
        ToolStripMain.Children.Insert(i++, CreateCorrespondingToolbarButton(fetchAllToolStripMenuItem, Images.PullFetchAll));
        ToolStripMain.Children.Insert(i++, CreateCorrespondingToolbarButton(fetchPruneAllToolStripMenuItem, Images.PullFetchPruneAll));
        ToolStripMain.Children.Insert(i++, CreateCorrespondingToolbarButton(mergeToolStripMenuItem, Images.PullMerge, Command.QuickPull));
        ToolStripMain.Children.Insert(i++, CreateCorrespondingToolbarButton(rebaseToolStripMenuItem1, Images.PullRebase));
        ToolStripMain.Children.Insert(i, CreateCorrespondingToolbarButton(pullToolStripMenuItem1, Images.Pull, Command.PullOrFetch));

        IconButton CreateCorrespondingToolbarButton(
            MenuItem toolStripMenuItem,
            Avalonia.Media.IImage image,
            Command? command = null)
        {
            string toolTipText = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(toolStripMenuItem.Header?.ToString() ?? string.Empty);
            IconButton clonedToolStripMenuItem = new()
            {
                Icon = image,
                Name = FetchPullToolbarShortcutsPrefix + toolStripMenuItem.Name,
                Content = toolTipText,
            };
            clonedToolStripMenuItem.Classes.Add("gitextensions-toolbar-button");
            clonedToolStripMenuItem.Classes.Add("gitextensions-icon-only");
            Avalonia.Automation.AutomationProperties.SetName(clonedToolStripMenuItem, toolTipText);
            ToolTip.SetTip(
                clonedToolStripMenuItem,
                command.HasValue
                    ? $"{toolTipText} ({GetShortcutKeyTooltipString(command.Value)})"
                    : toolTipText);

            clonedToolStripMenuItem.Click += (_, _) => toolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            return clonedToolStripMenuItem;
        }
    }

    private void FillNextPullActionAsDefaultToolStripMenuItems()
    {
        // Show both Check and Image margins in a menu
        // Prevent submenu from closing while options are changed
        // Avalonia's native menu owns its margins and dismissal behavior; the actions remain independently selectable.
        (MenuItem Item, GitPullAction Action)[] items =
        [
            (defaultPullDialogToolStripMenuItem, GitPullAction.None),
            (defaultPullMergeToolStripMenuItem, GitPullAction.Merge),
            (defaultPullRebaseToolStripMenuItem, GitPullAction.Rebase),
            (defaultPullFetchToolStripMenuItem, GitPullAction.Fetch),
            (defaultPullFetchAllToolStripMenuItem, GitPullAction.FetchAll),
            (defaultPullFetchPruneAllToolStripMenuItem, GitPullAction.FetchPruneAll),
        ];

        foreach ((MenuItem item, GitPullAction action) in items)
        {
            item.Tag = action;
            item.Click += SetDefaultPullActionMenuItemClick;
        }

        void SetDefaultPullActionMenuItemClick(object? sender, EventArgs eventArgs)
        {
            MenuItem clickedMenuItem = (MenuItem)sender!;
            AppSettings.DefaultPullAction = (GitPullAction)clickedMenuItem.Tag!;
            RefreshDefaultPullAction();
        }
    }

    private void FillUserShells(string defaultShell)
    {
        userShell.IsVisible = !_hasRuntimeCommands
            || UICommands.GetService(typeof(ITerminalLauncher)) is ITerminalLauncher;
        ToolTip.SetTip(userShell, defaultShell);

        // a user may have a specific shell configured in settings, but the shell is no longer available
        // set the first available shell as default
        // The portable toolbar exposes the configured platform terminal through one native launcher button.
    }

    private void RefreshDefaultPullAction()
    {
        if (setDefaultPullButtonActionToolStripMenuItem is null)
        {
            // We may get called while instantiating the form
            return;
        }

        GitPullAction action = AppSettings.DefaultPullAction;
        foreach (MenuItem menuItem in setDefaultPullButtonActionToolStripMenuItem.Items.OfType<MenuItem>())
        {
            menuItem.IsChecked = menuItem.Tag is GitPullAction itemAction && itemAction == action;
        }

        toolStripButtonPull.Icon = action switch
        {
            GitPullAction.Fetch => Images.PullFetch,
            GitPullAction.FetchAll => Images.PullFetchAll,
            GitPullAction.FetchPruneAll => Images.PullFetchPruneAll,
            GitPullAction.Rebase => Images.PullRebase,
            GitPullAction.Merge => Images.PullMerge,
            _ => Images.Pull,
        };

        ToolTip.SetTip(toolStripButtonPull, action switch
        {
            GitPullAction.Fetch => _pullFetch.Text,
            GitPullAction.FetchAll => _pullFetchAll.Text,
            GitPullAction.FetchPruneAll => _pullFetchPruneAll.Text,
            GitPullAction.Rebase => _pullRebase.Text,
            GitPullAction.Merge => _pullMerge.Text,
            _ => _pullOpenDialog.Text,
        });
        UpdateTooltipWithShortcut(toolStripButtonPull, Command.QuickPullOrFetch);
    }

    /// <summary>
    ///  Hides "Fetch all" item when there is only one remote,
    ///  since it is redundant with the single-remote "Fetch" command.
    /// </summary>
    private void UpdateFetchAllVisibility()
    {
        bool hasMultipleRemotes = Module.IsValidGitWorkingDir() && Module.GetRemoteNames().Count > 1;

        // Toolbar button drop down menu
        fetchAllToolStripMenuItem.IsVisible = hasMultipleRemotes;

        // Update the "set default pull action" submenu items
        defaultPullFetchAllToolStripMenuItem.IsVisible = hasMultipleRemotes;
    }

    private Avalonia.Media.IBrush UpdateCommitButtonAndGetBrush(
        IReadOnlyList<GitItemStatus>? status,
        bool showCount)
    {
        RepoStateVisualiser repoStateVisualiser = new();
        (Avalonia.Media.IImage image, Avalonia.Media.IBrush brush) = repoStateVisualiser.Invoke(status);

        if (showCount)
        {
            toolStripButtonCommit.Icon = image;
            toolStripButtonCommit.Content = status is null
                ? _commitButtonText.Text
                : $"{_commitButtonText} ({status.Count})";
        }
        else
        {
            toolStripButtonCommit.Icon = RepoStateVisualiser.Clean.Item1;
            toolStripButtonCommit.Content = _commitButtonText.Text;
        }

        return brush;
    }
}
