using Avalonia.Controls;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;

using ResourceManager;

namespace GitUI.LeftPanel;

internal sealed class SubmoduleTree : Tree
{
    private ISubmoduleStatusProvider? _submoduleStatusProvider;
    private SubmoduleStatusEventArgs? _currentSubmoduleInfo;
    private Nodes? _currentNodes;
    private readonly StringComparer _pathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public SubmoduleTree(RepoObjectsTree owner)
        : base(owner, RepoTreeKind.Submodules, TranslatedStrings.Submodules, Images.FolderSubmodule)
    {
        Complete(TranslatedStrings.Submodules, Images.FolderSubmodule, expanded: true);
    }

    public override void Dispose()
    {
        base.Dispose();
        Detach();
    }

    private void Provider_StatusUpdating(object? sender, EventArgs e)
        => _currentNodes = null;

    private void Provider_StatusUpdated(object? sender, SubmoduleStatusEventArgs e)
    {
        if (e.Token.IsCancellationRequested)
        {
            return;
        }

        _currentSubmoduleInfo = e;
        if (Dispatcher.UIThread.CheckAccess())
        {
            OnStatusUpdated(e);
        }
        else
        {
            Dispatcher.UIThread.Post(() => OnStatusUpdated(e));
        }
    }

    private void OnStatusUpdated(SubmoduleStatusEventArgs e)
    {
        if (e.Token.IsCancellationRequested || !ReferenceEquals(_currentSubmoduleInfo, e))
        {
            return;
        }

        // Structure is up-to-date, update status
        bool structureMatches = !e.StructureUpdated && _currentNodes is not null && TryUpdateExistingNodes(e.Info);
        if (!structureMatches)
        {
            // structure no longer matching
            // This normally occurs with illegal paths
            // Load the nodes in the tree
            // Module.GetRefs() is not used for submodules
            Load(e.Info);
        }

        if (_currentNodes is not null)
        {
            _ = LoadNodeDetailsAsync(_currentNodes, e.Token);
            LoadNodeToolTips(_currentNodes, e.Token);
        }

        Interlocked.CompareExchange(ref _currentSubmoduleInfo, null, e);
    }

    public void Load(SubmoduleInfoResult result)
        => OwnerControl.UpdateNodes(() =>
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
        Nodes.Clear();

        Nodes loadedNodes = FillSubmoduleTree(result);
        Nodes.AddNodes(loadedNodes);
        _currentNodes = Nodes;
        Complete(TranslatedStrings.Submodules, Images.FolderSubmodule, expanded: true);

        foreach (NodeBase node in DescendantsAndSelf())
        {
            node.TreeViewNode.IsExpanded = firstLoad
                || expanded.Contains($"{node.GetType().Name}:{node.SearchText}");
        }

        OwnerControl.RestoreSelectedNodes(this, selected);
    });

    private async Task<Nodes> LoadNodesAsync(SubmoduleInfoResult info, CancellationToken token)
    {
        await Task.CompletedTask;
        token.ThrowIfCancellationRequested();

        return FillSubmoduleTree(info);
    }

    private async Task LoadNodeDetailsAsync(Nodes loadedNodes, CancellationToken token)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            token.ThrowIfCancellationRequested();
            loadedNodes.DepthEnumerator<SubmoduleNode>().ForEach(node => node.RefreshDetails());
        });
    }

    private void LoadNodeToolTips(Nodes loadedNodes, CancellationToken token)
    {
        if (UICommands.GetService(typeof(IGitExecutorProvider)) is null)
        {
            return;
        }

        foreach (SubmoduleNode node in loadedNodes.DepthEnumerator<SubmoduleNode>())
        {
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            ThreadHelper.FileAndForget(async () =>
            {
                try
                {
                    await node.SetStatusToolTipAsync(token);
                }
                catch (OperationCanceledException)
                {
                }
                //// Comment out to debug BugReporter
                ////catch (GitExtUtils.ExternalOperationException)
                ////{
                ////}
            });
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
        }
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            foreach (NodeBase node in DescendantsAndSelf())
            {
                node.TreeViewNode.IsExpanded = true;
            }
        }
    }

    private Nodes FillSubmoduleTree(SubmoduleInfoResult result)
    {
        if (result.TopProject is null || result.Module is null)
        {
            return new Nodes(this);
        }

        IGitModule threadModule = result.Module;
        List<SubmoduleNode> submoduleNodes = [];

        // We always want to display submodules rooted from the top project.
        CreateSubmoduleNodes(result, threadModule, ref submoduleNodes);

        return AddTopAndNodesToTree(submoduleNodes, threadModule, result);
    }

    private void CreateSubmoduleNodes(SubmoduleInfoResult result, IGitModule threadModule, ref List<SubmoduleNode> nodes)
    {
        // result.OurSubmodules/AllSubmodules contain a recursive list of submodules, but don't provide info about the super
        // project path. So we deduce these by substring matching paths against an ordered list of all paths.
        List<string> modulePaths = [.. result.AllSubmodules.Select(info => NormalizePath(info.Path))];

        // Add current and parent module paths
        IGitModule? parentModule = threadModule;

        while (parentModule is not null)
        {
            modulePaths.Add(NormalizePath(parentModule.WorkingDir));
            parentModule = parentModule.SuperprojectModule;
        }

        // Sort descending so we find the nearest outer folder first
        modulePaths = [.. modulePaths.OrderByDescending(path => path, _pathComparer)];

        foreach (SubmoduleInfo submoduleInfo in result.AllSubmodules)
        {
            string submodulePath = NormalizePath(submoduleInfo.Path);
            string? superPath = GetSubmoduleSuperPath(submodulePath);

            if (superPath is null)
            {
                continue;
            }

            string localPath = Path.GetRelativePath(superPath, submodulePath).ToPosixPath();

            bool isCurrent = submoduleInfo.Bold;

            nodes.Add(new SubmoduleNode(this,
                this,
                submoduleInfo,
                isCurrent,
                isCurrent ? result.CurrentSubmoduleStatus : null,
                localPath!,
                superPath));
        }

        return;

        string? GetSubmoduleSuperPath(string submodulePath) =>
            modulePaths.Find(path => !_pathComparer.Equals(submodulePath, path) && IsChildPath(path, submodulePath));
    }

    private static string GetNodeRelativePath(IGitModule topModule, SubmoduleNode node)
    {
        return Path.GetRelativePath(
            NormalizePath(topModule.WorkingDir),
            NormalizePath(node.Info.Path)).ToPosixPath();
    }

    private Nodes AddTopAndNodesToTree(
        List<SubmoduleNode> submoduleNodes,
        IGitModule threadModule,
        SubmoduleInfoResult result)
    {
        // Create tree of SubmoduleFolderNode for each path directory and add input SubmoduleNodes as leaves.

        // Example of (SuperPath + LocalPath).ToPosixPath() for all nodes:
        //
        // C:/code/gitextensions2/Externals/conemu-inside
        // C:/code/gitextensions2/Externals/Git.hub
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor/gitextensions
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor/gitextensions/Externals/conemu-inside
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor/gitextensions/Externals/Git.hub
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor/gitextensions/Externals/ICSharpCode.TextEditor
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor/gitextensions/Externals/NBug
        // C:/code/gitextensions2/Externals/ICSharpCode.TextEditor/gitextensions/GitExtensionsDoc
        // C:/code/gitextensions2/Externals/NBug
        // C:/code/gitextensions2/GitExtensionsDoc
        //
        // What we want to do is first remove the topModule portion, "C:/code/gitextensions2/", and
        // then build our tree by breaking up each path into parts, separated by '/'.
        //
        // Note that when we break up the paths, some parts are just directories, the others are submodule nodes:
        //
        // Externals / ICSharpCode.TextEditor / gitextensions / Externals / Git.hub
        //  folder          submodule             submodule      folder     submodule
        //
        // Input 'nodes' is an array of SubmoduleNodes for all the submodules; now we need to create SubmoduleFolderNodes
        // and insert everything into a tree.

        IGitModule topModule = threadModule.GetTopModule();

        // Build a mapping of top-module-relative path to node
        Dictionary<string, Node> pathToNodes = new(_pathComparer);

        // Add existing SubmoduleNodes
        foreach (SubmoduleNode node in submoduleNodes)
        {
            pathToNodes[GetNodeRelativePath(topModule, node)] = node;
        }

        // Create and add missing SubmoduleFolderNodes
        foreach (SubmoduleNode node in submoduleNodes)
        {
            string[] parts = GetNodeRelativePath(topModule, node).Split(Delimiters.ForwardSlash);

            for (int i = 0; i < parts.Length - 1; ++i)
            {
                string path = string.Join("/", parts.Take(i + 1));

                if (!pathToNodes.ContainsKey(path))
                {
                    pathToNodes[path] = new SubmoduleFolderNode(this, this, parts[i]);
                }
            }
        }

        // Add top-module node
        SubmoduleNode topModuleNode = new(
            this,
            this,
            result.TopProject!,
            result.TopProject!.Bold,
            result.TopProject.Bold ? result.CurrentSubmoduleStatus : null,
            "",
            result.TopProject.Path);

        // Now build the tree
        HashSet<Node> nodesInTree = [];
        foreach (SubmoduleNode node in submoduleNodes)
        {
            NodeBase parentNode = topModuleNode;
            string[] parts = GetNodeRelativePath(topModule, node).Split(Delimiters.ForwardSlash);

            for (int i = 0; i < parts.Length; ++i)
            {
                string path = string.Join("/", parts.Take(i + 1));
                Node nodeToAdd = pathToNodes[path];

                // If node is not already in the tree, add it
                if (!nodesInTree.Contains(nodeToAdd))
                {
                    nodeToAdd.Reparent(parentNode);
                    parentNode.AddChild(nodeToAdd);
                    nodesInTree.Add(nodeToAdd);
                }

                parentNode = nodeToAdd;
            }
        }

        // Compact chains of single-child folder nodes for a cleaner display
        CompactSingleChildFolderChains(topModuleNode.TreeViewNode.Items.Cast<TreeViewItem>());

        Nodes nodes = new(this);
        nodes.AddNode(topModuleNode);

        return nodes;
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
            _submoduleStatusProvider.StatusUpdating += Provider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated += Provider_StatusUpdated;
        }
    }

    public void Detach()
    {
        if (_submoduleStatusProvider is not null)
        {
            _submoduleStatusProvider.StatusUpdating -= Provider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated -= Provider_StatusUpdated;
            _submoduleStatusProvider = null;
        }
    }

    private bool TryUpdateExistingNodes(SubmoduleInfoResult info)
    {
        if (info.TopProject is null || _currentNodes is null)
        {
            return false;
        }

        Dictionary<string, SubmoduleInfo> infos = info.AllSubmodules.ToDictionary(item => NormalizePath(item.Path), item => item, _pathComparer);
        infos[NormalizePath(info.TopProject.Path)] = info.TopProject;
        SubmoduleNode[] nodes = [.. _currentNodes.DepthEnumerator<SubmoduleNode>()];
        foreach (SubmoduleNode node in nodes)
        {
            if (!infos.Remove(NormalizePath(node.Info.Path), out SubmoduleInfo? current))
            {
                return false;
            }

            node.Info = current;
        }

        return infos.Count == 0;
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
        => UICommands.WithWorkingDirectory(node.Info.Path).StashSave(owner, AppSettings.IncludeUntrackedFilesInManualStash);

    public void CommitSubmodule(IWin32Window owner, SubmoduleNode node)
        => UICommands.WithWorkingDirectory(node.Info.Path.EnsureTrailingPathSeparator()).StartCommitDialog(owner);

    private static string NormalizePath(string path)
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
