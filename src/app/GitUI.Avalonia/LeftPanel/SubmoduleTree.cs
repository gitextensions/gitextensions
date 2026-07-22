using Avalonia.Controls;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;

using ResourceManager;

namespace GitUI.LeftPanel;

internal sealed class SubmoduleTree : Tree
{
    private readonly StringComparer _pathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private ISubmoduleStatusProvider? _submoduleStatusProvider;

    public SubmoduleTree(RepoObjectsTree owner)
        : base(owner, RepoTreeKind.Submodules, TranslatedStrings.Submodules, Images.FolderSubmodule)
    {
        Complete(TranslatedStrings.Submodules, Images.FolderSubmodule, count: 0, expanded: true);
    }

    public void Attach(ISubmoduleStatusProvider? provider)
    {
        if (ReferenceEquals(_submoduleStatusProvider, provider))
        {
            return;
        }

        Detach();
        _submoduleStatusProvider = provider;
        if (_submoduleStatusProvider is not null)
        {
            _submoduleStatusProvider.StatusUpdated += Provider_StatusUpdated;
        }
    }

    public void Detach()
    {
        if (_submoduleStatusProvider is not null)
        {
            _submoduleStatusProvider.StatusUpdated -= Provider_StatusUpdated;
            _submoduleStatusProvider = null;
        }
    }

    public void Load(SubmoduleInfoResult result)
    {
        if (result.TopProject is null)
        {
            return;
        }

        HashSet<string> expanded =
        [
            .. DescendantsAndSelf()
                .Where(node => node.TreeViewNode.IsExpanded)
                .Select(node => $"{node.GetType().Name}:{node.SearchText}"),
        ];
        HashSet<string> selected = OwnerControl.CaptureSelectedNodeIdentities(this);
        bool firstLoad = TreeViewNode.Items.Count == 0;
        TreeViewNode.Items.Clear();

        string topPath = NormalizePath(result.TopProject.Path);
        SubmoduleNode topNode = new(
            this,
            this,
            result.TopProject,
            result.TopProject.Bold,
            result.TopProject.Bold ? result.CurrentSubmoduleStatus : null,
            string.Empty,
            result.TopProject.Path);
        TreeViewNode.Items.Add(topNode.TreeViewNode);

        Dictionary<string, SubmoduleNode> moduleNodes = new(_pathComparer)
        {
            [topPath] = topNode,
        };

        foreach (SubmoduleInfo info in result.AllSubmodules.OrderBy(info => NormalizePath(info.Path).Length))
        {
            string path = NormalizePath(info.Path);
            KeyValuePair<string, SubmoduleNode> parent = moduleNodes
                .Where(pair => IsChildPath(pair.Key, path))
                .OrderByDescending(pair => pair.Key.Length)
                .FirstOrDefault();
            if (parent.Value is null)
            {
                continue;
            }

            string relativePath = Path.GetRelativePath(parent.Key, path).Replace(Path.DirectorySeparatorChar, '/');
            string[] parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            NodeBase parentNode = parent.Value;
            foreach (string folder in parts[..^1])
            {
                SubmoduleFolderNode? folderNode = parentNode.TreeViewNode.Items
                    .Cast<TreeViewItem>()
                    .Select(item => item.Tag)
                    .OfType<SubmoduleFolderNode>()
                    .FirstOrDefault(node => _pathComparer.Equals(node.Name, folder));
                if (folderNode is null)
                {
                    folderNode = new SubmoduleFolderNode(this, parentNode, folder);
                    parentNode.TreeViewNode.Items.Add(folderNode.TreeViewNode);
                }

                parentNode = folderNode;
            }

            SubmoduleNode node = new(
                this,
                parentNode,
                info,
                info.Bold,
                info.Bold ? result.CurrentSubmoduleStatus : null,
                relativePath,
                parent.Key);
            parentNode.TreeViewNode.Items.Add(node.TreeViewNode);
            moduleNodes[path] = node;
        }

        CompactSingleChildFolderChains(topNode.TreeViewNode.Items.Cast<TreeViewItem>());
        Complete(TranslatedStrings.Submodules, Images.FolderSubmodule, result.AllSubmodules.Count, expanded: true);

        foreach (NodeBase node in DescendantsAndSelf())
        {
            node.TreeViewNode.IsExpanded = firstLoad
                || expanded.Contains($"{node.GetType().Name}:{node.SearchText}");
        }

        OwnerControl.RestoreSelectedNodes(this, selected);
    }

    public void UpdateSubmodule(IWin32Window owner, SubmoduleNode node)
        => UICommands.StartUpdateSubmoduleDialog(owner, node.LocalPath, node.SuperPath);

    public void OpenSubmodule(SubmoduleNode node)
        => node.Open();

    public void OpenSubmoduleInGitExtensions(SubmoduleNode node)
        => node.LaunchGitExtensions();

    public void ManageSubmodules(IWin32Window owner)
        => UICommands.StartSubmodulesDialog(owner);

    public void SynchronizeSubmodules(IWin32Window owner)
        => UICommands.StartSyncSubmodulesDialog(owner);

    public void ResetSubmodule(IWin32Window owner, SubmoduleNode node)
    {
        CommandsDialogs.FormResetChanges.ActionEnum resetType = CommandsDialogs.FormResetChanges.ShowResetDialog(owner, true, true);
        if (resetType == CommandsDialogs.FormResetChanges.ActionEnum.Cancel)
        {
            return;
        }

        GitModule module = new(UICommands.GetRequiredService<IGitExecutorProvider>(), node.Info.Path);
        module.ResetAllChanges(clean: resetType == CommandsDialogs.FormResetChanges.ActionEnum.ResetAndDelete);
    }

    public void StashSubmodule(IWin32Window owner, SubmoduleNode node)
        => UICommands.WithWorkingDirectory(node.Info.Path).StashSave(owner, GitCommands.AppSettings.IncludeUntrackedFilesInManualStash);

    public void CommitSubmodule(IWin32Window owner, SubmoduleNode node)
        => UICommands.WithWorkingDirectory(node.Info.Path.EnsureTrailingPathSeparator()).StartCommitDialog(owner);

    private void Provider_StatusUpdated(object? sender, SubmoduleStatusEventArgs e)
    {
        if (e.Token.IsCancellationRequested)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Load(e.Info);
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!e.Token.IsCancellationRequested)
                {
                    Load(e.Info);
                }
            });
        }
    }

    private string NormalizePath(string path)
    {
        string normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return Path.TrimEndingDirectorySeparator(normalized);
    }

    private bool IsChildPath(string parent, string child)
    {
        if (child.Length <= parent.Length || !child.StartsWith(parent, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            return false;
        }

        return child[parent.Length] is '/' or '\\';
    }

    private static void CompactSingleChildFolderChains(IEnumerable<TreeViewItem> items)
    {
        foreach (TreeViewItem item in items.ToArray())
        {
            if (item.Tag is SubmoduleFolderNode folderNode)
            {
                folderNode.CompactSingleChildFolders();
            }

            CompactSingleChildFolderChains(item.Items.Cast<TreeViewItem>());
        }
    }
}
