using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

partial class FileStatusList
{
    private Action? _blame;
    private Action? _cherryPickChanges;
    private Action? _filterFileInGrid;
    private Func<bool>? _getSupportLinePatching;
    private Action<bool>? _openInFileTreeTab_AsBlame;
    private Action? _refreshParent;
    private Action? _stage;
    private Action? _unstage;

    /// <summary>
    ///  Binds worktree/index actions supplied by a staging consumer.
    /// </summary>
    public void BindContextMenu(Action refreshParent, bool canAutoRefresh, Action? stage, Action? unstage)
    {
        _refreshParent = refreshParent;
        _stage = stage;
        _unstage = unstage;
    }

    /// <summary>
    ///  Binds the stash-level cherry-pick action.
    /// </summary>
    public void BindContextMenu(Action cherryPickChanges, Func<bool> getSupportLinePatching)
    {
        _cherryPickChanges = cherryPickChanges;
        _getSupportLinePatching = getSupportLinePatching;
    }

    /// <summary>
    ///  Binds revision-diff actions supplied by the owning view.
    /// </summary>
    public void BindContextMenu(
        Action? blame,
        Action? cherryPickChanges,
        Action filterFileInGrid,
        Action refreshParent,
        Action<bool>? openInFileTreeTab_AsBlame,
        Func<GitRevision?>? getCurrentRevision,
        Func<int> getLineNumber,
        Func<string>? getSelectedText,
        Func<bool> getSupportLinePatching)
    {
        _blame = blame;
        _cherryPickChanges = cherryPickChanges;
        _getSupportLinePatching = getSupportLinePatching;
        _filterFileInGrid = filterFileInGrid;
        _openInFileTreeTab_AsBlame = openInFileTreeTab_AsBlame;
        _refreshParent = refreshParent;
    }

    private void WireContextMenu()
    {
        ItemContextMenu.Opening += ItemContextMenu_Opening;
        tsmiStageFile.Click += (_, _) => _stage?.Invoke();
        tsmiUnstageFile.Click += (_, _) => _unstage?.Invoke();
        tsmiCherryPickChanges.Click += (_, _) => _cherryPickChanges?.Invoke();
        tsmiOpenWorkingDirectoryFile.Click += OpenWorkingDirectoryFile_Click;
        tsmiCopyPaths.Click += CopyPaths_Click;
        tsmiShowInFolder.Click += ShowInFolder_Click;
        tsmiShowInFileTree.Click += (_, _) => _openInFileTreeTab_AsBlame?.Invoke(false);
        tsmiFilterFileInGrid.Click += (_, _) => _filterFileInGrid?.Invoke();
        tsmiFileHistory.Click += (_, _) => StartFileHistoryDialog(showBlame: false);
        tsmiBlame.Click += Blame_Click;
        _collapseAll.Click += (_, _) => SetTreeExpansion(expanded: false, rootOnly: false);
        _collapseRootFolders.Click += (_, _) => SetTreeExpansion(expanded: false, rootOnly: true);
        _expandAll.Click += (_, _) => SetTreeExpansion(expanded: true, rootOnly: false);
        _selectAll.Click += (_, _) => SelectAll();
    }

    private void Blame_Click(object? sender, EventArgs e)
    {
        if (_blame is not null)
        {
            _blame();
        }
        else
        {
            StartFileHistoryDialog(showBlame: true);
        }
    }

    private void CopyPaths_Click(object? sender, EventArgs e)
    {
        string[] paths = SelectedFolder is RelativePath folder
            ? [folder.Value]
            : [.. SelectedGitItems.Select(item => item.Name)];
        if (paths.Length > 0)
        {
            ClipboardUtil.TrySetText(string.Join(Environment.NewLine, paths));
        }
    }

    private string? GetSelectedAbsolutePath()
    {
        string? relativePath = SelectedFolder?.Value;
        if (relativePath is null && SelectedGitItems is [GitItemStatus item])
        {
            relativePath = item.Name;
        }

        if (relativePath is null || !TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return null;
        }

        return new FullPathResolver(() => commands.Module.WorkingDir).Resolve(relativePath)?.ToNativePath();
    }

    private WinFormsShims.IWin32Window? GetOwner()
        => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

    private void ItemContextMenu_Opening(object? sender, EventArgs e)
    {
        IReadOnlyList<GitItemStatus> selectedItems = SelectedGitItems;
        bool hasItems = selectedItems.Count > 0;
        bool hasSingleItem = selectedItems.Count == 1;
        bool hasPath = hasItems || SelectedFolder is not null;
        string? absolutePath = GetSelectedAbsolutePath();
        bool workingFileExists = absolutePath is not null && File.Exists(absolutePath);

        tsmiStageFile.IsVisible = _stage is not null;
        tsmiStageFile.IsEnabled = _stage is not null
                                  && selectedItems.Any(item => !item.IsAssumeUnchanged && !item.IsSkipWorktree);
        tsmiUnstageFile.IsVisible = _unstage is not null;
        tsmiUnstageFile.IsEnabled = _unstage is not null && hasItems;
        tsmiCherryPickChanges.IsVisible = _cherryPickChanges is not null;
        tsmiCherryPickChanges.IsEnabled = hasSingleItem && (_getSupportLinePatching?.Invoke() ?? false);
        sepGit.IsVisible = tsmiStageFile.IsVisible || tsmiUnstageFile.IsVisible || tsmiCherryPickChanges.IsVisible;

        tsmiOpenWorkingDirectoryFile.IsVisible = workingFileExists;
        tsmiCopyPaths.IsEnabled = hasPath;
        tsmiShowInFolder.IsEnabled = absolutePath is not null
                                     && (File.Exists(absolutePath) || Directory.Exists(absolutePath));
        sepBrowse.IsVisible = hasPath;
        tsmiShowInFileTree.IsVisible = !_isFileTreeMode && _openInFileTreeTab_AsBlame is not null;
        tsmiShowInFileTree.IsEnabled = hasSingleItem;
        tsmiFilterFileInGrid.IsVisible = _filterFileInGrid is not null;
        tsmiFilterFileInGrid.IsEnabled = hasPath;
        tsmiFileHistory.IsEnabled = hasSingleItem && selectedItems[0].IsTracked;
        tsmiBlame.IsEnabled = hasSingleItem && selectedItems[0].IsTracked;

        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            sepScripts.IsVisible = ItemContextMenu.AddUserScripts(
                tsmiRunScript,
                ExecuteCommand,
                script => script.OnEvent == ScriptEvent.ShowInFileList,
                commands);
        }
        else
        {
            ItemContextMenu.RemoveUserScripts(tsmiRunScript);
            sepScripts.IsVisible = false;
        }

        _treeContextMenuSeparator.IsVisible = _isFileTreeMode;
        _collapseAll.IsVisible = _isFileTreeMode;
        _collapseRootFolders.IsVisible = _isFileTreeMode;
        _expandAll.IsVisible = _isFileTreeMode;
        _selectAll.IsVisible = _isFileTreeMode;
    }

    private void OpenWorkingDirectoryFile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is string path && File.Exists(path))
        {
            OsShellUtil.Open(path);
        }
    }

    private void SetTreeExpansion(bool expanded, bool rootOnly)
    {
        foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>())
        {
            SetExpansion(node);
        }

        return;

        void SetExpansion(FileTreeNode node)
        {
            if (node.IsFolder)
            {
                node.IsExpanded = expanded;
            }

            if (!rootOnly)
            {
                foreach (FileTreeNode child in node.Children)
                {
                    SetExpansion(child);
                }
            }
        }
    }

    private void ShowInFolder_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is not string path)
        {
            return;
        }

        string? directory = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
        {
            OsShellUtil.Open(directory);
        }
    }

    private void StartFileHistoryDialog(bool showBlame)
    {
        if (SelectedGitItems is not [GitItemStatus { IsTracked: true } item])
        {
            return;
        }

        UICommands.StartFileHistoryDialog(
            GetOwner(),
            item.Name,
            SelectedFileStatusItem?.SecondRevision,
            showBlame: showBlame);
    }
}
