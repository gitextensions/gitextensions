﻿using System.Diagnostics;
using GitCommands;
using GitCommands.Submodules;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI.LeftPanel
{
    internal sealed class SubmoduleTree : Tree
    {
        private readonly ISubmoduleStatusProvider _submoduleStatusProvider;
        private SubmoduleStatusEventArgs? _currentSubmoduleInfo;
        private Nodes? _currentNodes = null;

        public SubmoduleTree(TreeNode treeNode, IGitUICommandsSource commandsSource)
            : base(treeNode, commandsSource)
        {
            _submoduleStatusProvider = UICommands.GetRequiredService<ISubmoduleStatusProvider>();
            _submoduleStatusProvider.StatusUpdating += Provider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated += Provider_StatusUpdated;
        }

        public override void Dispose()
        {
            base.Dispose();

            _submoduleStatusProvider.StatusUpdating -= Provider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated -= Provider_StatusUpdated;
        }

        private void Provider_StatusUpdating(object sender, EventArgs e)
        {
            _currentNodes = null;
        }

        private void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
        {
            _currentSubmoduleInfo = e;

            if (IsAttached)
            {
                OnStatusUpdated(e);
            }
        }

        private void OnStatusUpdated(SubmoduleStatusEventArgs e)
        {
            TreeViewNode.TreeView.InvokeAndForget(async () =>
            {
                CancellationTokenSource? cts = null;
                Task<Nodes>? loadNodesTask = null;

                if (e.StructureUpdated)
                {
                    _currentNodes = null;
                }

                if (_currentNodes is not null)
                {
                    // Structure is up-to-date, update status
                    Dictionary<string, SubmoduleInfo> infos = e.Info.AllSubmodules.ToDictionary(info => info.Path, info => info);
                    Validates.NotNull(e.Info.TopProject);
                    infos[e.Info.TopProject.Path] = e.Info.TopProject;
                    List<SubmoduleNode> nodes = _currentNodes.DepthEnumerator<SubmoduleNode>().ToList();

                    foreach (SubmoduleNode node in nodes)
                    {
                        if (infos.ContainsKey(node.Info.Path))
                        {
                            node.Info = infos[node.Info.Path];
                            infos.Remove(node.Info.Path);
                        }
                        else
                        {
                            // structure no longer matching
                            DebugHelpers.Assert(true, $"Status info with {1 + e.Info.AllSubmodules.Count} records do not match current nodes ({nodes.Count})");
                            _currentNodes = null;
                            break;
                        }
                    }

                    if (infos.Count > 0)
                    {
                        // This normally occurs with illegal paths
                        Trace.WriteLine($"{infos.Count} status info records remains after matching current nodes, structure seem to mismatch ({nodes.Count}/{e.Info.AllSubmodules.Count}: {string.Join(",", infos.Keys.ToList())})");

                        _currentNodes = null;
                    }
                }

                if (_currentNodes is null)
                {
                    // Module.GetRefs() is not used for submodules
                    JoinableTask joinableTask = ReloadNodesDetached((token, _) =>
                        {
                            cts = CancellationTokenSource.CreateLinkedTokenSource(e.Token, token);
                            loadNodesTask = LoadNodesAsync(e.Info, cts.Token);
                            return loadNodesTask;
                        },
                        getRefs: null);
                    await joinableTask.JoinAsync(e.Token);
                }

                if (cts is not null && loadNodesTask is not null)
                {
                    _currentNodes = await loadNodesTask;
                }

                if (_currentNodes is not null)
                {
                    CancellationToken token = cts?.Token ?? e.Token;
                    try
                    {
                        await LoadNodeDetailsAsync(_currentNodes, token).ConfigureAwaitRunInline();
                        LoadNodeToolTips(_currentNodes, token);
                    }
                    catch (Exception) when (token.IsCancellationRequested)
                    {
                    }
                }

                Interlocked.CompareExchange(ref _currentSubmoduleInfo, null, e);
            });
        }

        private async Task<Nodes> LoadNodesAsync(SubmoduleInfoResult info, CancellationToken token)
        {
            await TaskScheduler.Default;
            token.ThrowIfCancellationRequested();

            return FillSubmoduleTree(info);
        }

        private async Task LoadNodeDetailsAsync(Nodes loadedNodes, CancellationToken token)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            if (TreeViewNode.TreeView is not null)
            {
                TreeViewNode.TreeView.BeginUpdate();

                try
                {
                    loadedNodes.DepthEnumerator<SubmoduleNode>().ForEach(node => node.RefreshDetails());
                }
                finally
                {
                    TreeViewNode.TreeView.EndUpdate();
                }
            }
        }

        private void LoadNodeToolTips(Nodes loadedNodes, CancellationToken token)
        {
            if (TreeViewNode.TreeView is null)
            {
                return;
            }

            loadedNodes.DepthEnumerator<SubmoduleNode>()
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                    .ForEach(async node =>
                    {
                        try
                        {
                            await node.SetStatusToolTipAsync(token).ConfigureAwait(false);
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

        protected override void PostFillTreeViewNode(bool firstTime)
        {
            if (firstTime)
            {
                TreeViewNode.ExpandAll();
            }
        }

        private Nodes FillSubmoduleTree(SubmoduleInfoResult result)
        {
            Validates.NotNull(result.TopProject);

            GitModule threadModule = (GitModule?)result.Module;

            Validates.NotNull(threadModule);

            List<SubmoduleNode> submoduleNodes = [];

            // We always want to display submodules rooted from the top project.
            CreateSubmoduleNodes(result, threadModule, ref submoduleNodes);

            Nodes nodes = new(this);
            AddTopAndNodesToTree(ref nodes, submoduleNodes, threadModule, result);
            return nodes;
        }

        private void CreateSubmoduleNodes(SubmoduleInfoResult result, IGitModule threadModule, ref List<SubmoduleNode> nodes)
        {
            // result.OurSubmodules/AllSubmodules contain a recursive list of submodules, but don't provide info about the super
            // project path. So we deduce these by substring matching paths against an ordered list of all paths.
            List<string> modulePaths = result.AllSubmodules.Select(info => info.Path).ToList();

            // Add current and parent module paths
            IGitModule parentModule = threadModule;

            while (parentModule is not null)
            {
                modulePaths.Add(parentModule.WorkingDir);
                parentModule = parentModule.SuperprojectModule;
            }

            // Sort descending so we find the nearest outer folder first
            modulePaths = modulePaths.OrderByDescending(path => path).ToList();

            foreach (SubmoduleInfo submoduleInfo in result.AllSubmodules)
            {
                string? superPath = GetSubmoduleSuperPath(submoduleInfo.Path);

                if (!Directory.Exists(superPath))
                {
                    MessageBoxes.SubmoduleDirectoryDoesNotExist(owner: null, superPath ?? submoduleInfo.Path, submoduleInfo.Text);
                    continue;
                }

                string localPath = Path.GetDirectoryName(submoduleInfo.Path[superPath.Length..]).ToPosixPath();

                bool isCurrent = submoduleInfo.Bold;

                nodes.Add(new SubmoduleNode(this,
                    submoduleInfo,
                    isCurrent,
                    isCurrent ? result.CurrentSubmoduleStatus : null,
                    localPath,
                    superPath));
            }

            return;

            string? GetSubmoduleSuperPath(string submodulePath) =>
                modulePaths.Find(path => submodulePath != path && submodulePath.Contains(path));
        }

        private static string GetNodeRelativePath(IGitModule topModule, SubmoduleNode node)
        {
            return node.SuperPath.SubstringAfter(topModule.WorkingDir).ToPosixPath() + node.LocalPath;
        }

        private void AddTopAndNodesToTree(
            ref Nodes nodes,
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
            Dictionary<string, Node> pathToNodes = [];

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
                        pathToNodes[path] = new SubmoduleFolderNode(this, parts[i]);
                    }
                }
            }

            // Now build the tree
            DummyNode rootNode = new();
            HashSet<Node> nodesInTree = [];
            foreach (SubmoduleNode node in submoduleNodes)
            {
                Node parentNode = rootNode;
                string[] parts = GetNodeRelativePath(topModule, node).Split(Delimiters.ForwardSlash);

                for (int i = 0; i < parts.Length; ++i)
                {
                    string path = string.Join("/", parts.Take(i + 1));
                    Node nodeToAdd = pathToNodes[path];

                    // If node is not already in the tree, add it
                    if (!nodesInTree.Contains(nodeToAdd))
                    {
                        parentNode.Nodes.AddNode(nodeToAdd);
                        nodesInTree.Add(nodeToAdd);
                    }

                    parentNode = nodeToAdd;
                }
            }

            Validates.NotNull(result.TopProject);

            // Add top-module node, and move children of root to it
            SubmoduleNode topModuleNode = new(
                this,
                result.TopProject,
                result.TopProject.Bold,
                result.TopProject.Bold ? result.CurrentSubmoduleStatus : null,
                "",
                result.TopProject.Path);
            topModuleNode.Nodes.AddNodes(rootNode.Nodes);
            nodes.AddNode(topModuleNode);
        }

        public void UpdateSubmodule(IWin32Window owner, SubmoduleNode node)
        {
            UICommands.StartUpdateSubmoduleDialog(owner, node.LocalPath, node.SuperPath);
        }

        public void OpenSubmodule(IWin32Window owner, SubmoduleNode node)
        {
            node.Open();
        }

        public void OpenSubmoduleInGitExtensions(IWin32Window owner, SubmoduleNode node)
        {
            node.LaunchGitExtensions();
        }

        public void ManageSubmodules(IWin32Window owner)
        {
            UICommands.StartSubmodulesDialog(owner);
        }

        public void SynchronizeSubmodules(IWin32Window owner)
        {
            UICommands.StartSyncSubmodulesDialog(owner);
        }

        public void ResetSubmodule(IWin32Window owner, SubmoduleNode node)
        {
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(owner, true, true);

            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

            GitModule module = new(node.Info.Path);
            module.ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete);
        }

        public void StashSubmodule(IWin32Window owner, SubmoduleNode node)
        {
            GitUICommands uiCmds = UICommands.WithWorkingDirectory(node.Info.Path);
            uiCmds.StashSave(owner, AppSettings.IncludeUntrackedFilesInManualStash);
        }

        public void CommitSubmodule(IWin32Window owner, SubmoduleNode node)
        {
            GitUICommands submodulCommands = UICommands.WithWorkingDirectory(node.Info.Path.EnsureTrailingPathSeparator());
            submodulCommands.StartCommitDialog(owner);
        }
    }
}
