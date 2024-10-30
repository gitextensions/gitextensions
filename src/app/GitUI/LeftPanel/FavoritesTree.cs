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
    private readonly FavoriteBranchesCache _favoriteBranchesCache = new();
    private FileSystemWatcher _configWatcher;

    public FavoritesTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource, IRevisionGridInfo revisionGridInfo)
        : base(treeNode, uiCommands, refsSource, RefsFilter.Remotes | RefsFilter.Heads)
    {
        _revisionGridInfo = revisionGridInfo;
    }

    public void Add(NodeBase node)
    {
        StopFileWatcher();

        if (node is BaseRevisionNode baseRevisionNode && baseRevisionNode.ObjectId != null)
        {
            _favoriteBranchesCache.Add(baseRevisionNode.ObjectId, baseRevisionNode.FullPath);
        }

        Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
        ResumeFileWatcher();
    }

    public void Remove(NodeBase node)
    {
        StopFileWatcher();

        if (node is BaseRevisionNode baseRevisionNode && baseRevisionNode.ObjectId != null)
        {
            _favoriteBranchesCache.Remove(baseRevisionNode.ObjectId, baseRevisionNode.FullPath);
        }

        Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
        ResumeFileWatcher();
    }

    public override void Dispose()
    {
        base.Dispose();
        StopFileWatcher();
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
    {
        StopFileWatcher();
        _favoriteBranchesCache.Location = UICommands.Module.WorkingDirGitDir;
        FavoriteNode remoteNode = new(this, TranslatedStrings.Remotes, nameof(Images.BranchRemoteRoot)) { Visible = true };
        FavoriteNode localNode = new(this, TranslatedStrings.Local, nameof(Images.BranchLocalRoot)) { Visible = true };

        Dictionary<string, BaseRevisionNode> pathToNodesRemote = [];
        Dictionary<string, BaseRevisionNode> pathToNodeLocal = [];

        foreach (IGitRef branch in branches)
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

        _favoriteBranchesCache.CleanUp(branches);

        Nodes nodes = new(this);
        nodes.AddNode(localNode);
        nodes.AddNode(remoteNode);
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

    private void CreateLocalBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes)
    {
        Validates.NotNull(branch.ObjectId);

        if (!_favoriteBranchesCache.Contains(branch.ObjectId, branch.Name))
        {
            return;
        }

        string currentBranch = _revisionGridInfo.GetCurrentBranch();

        LocalBranchNode branchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, true);

        BaseRevisionNode parent = branchNode.CreateRootNode(pathToNodes, CreatePathNode);

        if (parent is not null)
        {
            nodes.AddNode(parent);
        }

        BaseRevisionNode CreatePathNode(Tree tree, string parentPath)
        {
            BranchPathNode branchPathNode = new(tree, parentPath);

            return branchPathNode;
        }
    }

    private void CreateRemoteBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes)
    {
        Validates.NotNull(branch.ObjectId);

        if (!_favoriteBranchesCache.Contains(branch.ObjectId, branch.Name))
        {
            return;
        }

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
            if (parentPath == remote.Name)
            {
                return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true);
            }

            return new BasePathNode(tree, parentPath);
        }
    }

    private void ResumeFileWatcher()
    {
        StopFileWatcher();
        _favoriteBranchesCache.Location = UICommands.Module.WorkingDirGitDir;
        string configFile = _favoriteBranchesCache.ConfigFile;
        _configWatcher = new FileSystemWatcher { Path = Path.GetDirectoryName(configFile), Filter = Path.GetFileName(configFile), NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName };

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
        _favoriteBranchesCache.Load();
        Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
    }

    private void OnConfigFileRenamed(object sender, RenamedEventArgs e)
    {
        ReloadFavoritesAndUpdate();
    }
}
