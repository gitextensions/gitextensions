using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandDialogs;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using Microsoft;

namespace GitUI.LeftPanel;

internal sealed class FavoritesTree : BaseRefTree
{
    private readonly IRevisionGridInfo _revisionGridInfo;
    private readonly FavoriteBranchesCache _favoritesCache;
    private FileSystemWatcher _configWatcher;

    public FavoritesTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource, IRevisionGridInfo revisionGridInfo)
        : base(treeNode, uiCommands, refsSource, RefsFilter.Remotes | RefsFilter.Heads)
    {
        _revisionGridInfo = revisionGridInfo;

        // Whilst the UICommands may change, the service registrations are constant.
        IServiceProvider serviceProvider = uiCommands.UICommands;
        _favoritesCache = new FavoriteBranchesCache(serviceProvider);
    }

    public void Add(NodeBase node)
    {
        StopFileWatcher();

        if (node is BaseRevisionNode baseRevisionNode && baseRevisionNode.ObjectId is not null)
        {
            _favoritesCache.Add(baseRevisionNode.ObjectId, baseRevisionNode.FullPath);
        }

        Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
        ResumeFileWatcher();
    }

    public void Remove(NodeBase node)
    {
        StopFileWatcher();

        RemoveNode(node);

        Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
        ResumeFileWatcher();
    }

    private void RemoveNode(NodeBase node)
    {
        if (node is BaseRevisionNode { ObjectId: not null } baseRevisionNode)
        {
            _favoritesCache.Remove(baseRevisionNode.ObjectId, baseRevisionNode.FullPath);
        }
        else
        {
            if (node is FavoriteNode favoriteNode)
            {
                if (favoriteNode.HasChildren)
                {
                    foreach (Node childNode in favoriteNode.Nodes)
                    {
                        RemoveNode(childNode);
                    }
                }
                else
                {
                    _favoritesCache.Remove(favoriteNode.ObjectId, favoriteNode.FullPath);
                }
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        StopFileWatcher();
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> gitRefs, CancellationToken token)
    {
        StopFileWatcher();
        _favoritesCache.Location = UICommands.Module.WorkingDirGitDir;
        _favoritesCache.Load();

        FavoriteNode remoteNode = new(this, TranslatedStrings.Remotes, nameof(Images.BranchRemoteRoot)) { Visible = true };
        FavoriteNode localNode = new(this, TranslatedStrings.Local, nameof(Images.BranchLocalRoot)) { Visible = true };
        FavoriteNode untraceableNode = new(this, "Untraceable", nameof(Images.Warning)) { Visible = true };

        Dictionary<string, BaseRevisionNode> pathToNodesRemote = [];
        Dictionary<string, BaseRevisionNode> pathToNodeLocal = [];
        Dictionary<string, BaseRevisionNode> pathToUntraceable = [];

        IEnumerable<IGitRef> favGitRefs = _favoritesCache.Synchronize(gitRefs, out IList<BranchIdentifier>? noMatches);

        foreach (IGitRef branch in favGitRefs)
        {
            token.ThrowIfCancellationRequested();

            if (branch.IsRemote)
            {
                CreateRemoteBranchTree(branch, pathToNodesRemote, remoteNode.Nodes);
            }
            else if (branch.IsHead)
            {
                CreateLocalBranchTree(branch, pathToNodeLocal, localNode.Nodes);
            }
        }

        foreach (BranchIdentifier identifier in noMatches)
        {
            CreateUntraceableBranchTree(identifier, pathToUntraceable, untraceableNode.Nodes);
        }

        // noMatches.ForEach(p => _favoritesCache.Remove(p.ObjectId, p.Name));

        _favoritesCache.Save();

        Nodes nodes = new(this);
        nodes.AddNode(localNode);
        nodes.AddNode(remoteNode);

        if (untraceableNode.HasChildren)
        {
            nodes.AddNode(untraceableNode);
        }

        ResumeFileWatcher();

        return nodes;
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            TreeViewNode.Collapse();
        }
    }

    private void CreateUntraceableBranchTree(BranchIdentifier branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes)
    {
        Validates.NotNull(branch.ObjectId);

        FavoriteNode branchNode = new(this, branch.Name, nameof(Images.BranchDelete))
        {
            ObjectId = branch.ObjectId
        };

        BaseRevisionNode parent = branchNode.CreateRootNode(pathToNodes,
            (tree, parentPath) => new FavoriteNode(this, parentPath, nameof(Images.BranchFolder)));

        if (parent is not null)
        {
            nodes.AddNode(parent);
        }
    }

    private void CreateLocalBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes)
    {
        Validates.NotNull(branch.ObjectId);

        string currentBranch = _revisionGridInfo.GetCurrentBranch();

        LocalBranchNode branchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, true);

        BaseRevisionNode parent = branchNode.CreateRootNode(pathToNodes, (tree, parentPath) => new BranchPathNode(tree, parentPath));

        if (parent is not null)
        {
            nodes.AddNode(parent);
        }
    }

    private void CreateRemoteBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes)
    {
        Validates.NotNull(branch.ObjectId);

        ConfigFileRemoteSettingsManager remotesManager = new(() => Module);
        Dictionary<string, Remote> remoteByName = ThreadHelper.JoinableTaskFactory.Run(Module.GetRemotesAsync).ToDictionary(r => r.Name);

        string remoteName = branch.Name.SubstringUntil('/');

        if (remoteByName.TryGetValue(remoteName, out Remote remote))
        {
            RemoteBranchNode branchNode = new(this, branch.ObjectId, branch.Name, true);

            BaseRevisionNode parent = branchNode.CreateRootNode(pathToNodes, (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));

            if (parent is not null)
            {
                nodes.AddNode(parent);
            }
        }

        BaseRevisionNode CreateRemoteBranchPathNode(Tree tree, string parentPath, Remote remote)
        {
            if (string.Equals(parentPath, remote.Name, StringComparison.Ordinal))
            {
                return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true);
            }

            return new BasePathNode(tree, parentPath);
        }
    }

    private void ResumeFileWatcher()
    {
        StopFileWatcher();
        _favoritesCache.Location = UICommands.Module.WorkingDirGitDir;
        string configFile = _favoritesCache.ConfigFile;
        string directoryName = Path.GetDirectoryName(configFile);

        if (!Directory.Exists(directoryName))
        {
            return;
        }

        _configWatcher = new FileSystemWatcher { Path = directoryName, Filter = Path.GetFileName(configFile), NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size };

        _configWatcher.Changed += OnConfigFileChanged;
        _configWatcher.Created += OnConfigFileChanged;
        _configWatcher.Deleted += OnConfigFileChanged;
        _configWatcher.Renamed += OnConfigFileRenamed;

        _configWatcher.EnableRaisingEvents = true;
    }

    private void StopFileWatcher()
    {
        if (_configWatcher is not null)
        {
            _configWatcher.EnableRaisingEvents = false;
            _configWatcher.Changed -= OnConfigFileChanged;
            _configWatcher.Created -= OnConfigFileChanged;
            _configWatcher.Deleted -= OnConfigFileChanged;
            _configWatcher.Renamed -= OnConfigFileRenamed;
            _configWatcher.Dispose();
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        ReloadFavoritesAndUpdate();
    }

    private void ReloadFavoritesAndUpdate()
    {
        Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
    }

    private void OnConfigFileRenamed(object sender, RenamedEventArgs e)
    {
        ReloadFavoritesAndUpdate();
    }
}
