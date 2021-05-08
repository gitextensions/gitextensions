using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Submodules;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        // Top-level nodes used to group SubmoduleNodes
        private class SubmoduleFolderNode : Node
        {
            private string _name;

            public SubmoduleFolderNode(Tree tree, string name)
                : base(tree)
            {
                _name = name;
            }

            protected override string DisplayText()
            {
                return string.Format(_name);
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.FolderClosed);
                SetNodeFont(FontStyle.Italic);
            }
        }

        // Node representing a submodule
        private class SubmoduleNode : Node
        {
            public SubmoduleInfo Info { get; set; }
            public bool IsCurrent { get; }
            public IReadOnlyList<GitItemStatus>? GitStatus { get; }
            public string LocalPath { get; }
            public string SuperPath { get; }
            public string SubmoduleName { get; }
            public string BranchText { get; }

            public SubmoduleNode(Tree tree, SubmoduleInfo submoduleInfo, bool isCurrent, IReadOnlyList<GitItemStatus>? gitStatus, string localPath, string superPath)
                : base(tree)
            {
                Info = submoduleInfo;
                IsCurrent = isCurrent;
                GitStatus = gitStatus;
                LocalPath = localPath;
                SuperPath = superPath;

                // Extract submodule name and branch
                // e.g. Info.Text = "Externals/conemu-inside [no branch]"
                // Note that the branch portion won't be there if the user hasn't yet init'd + updated the submodule.
                var pathAndBranch = Info.Text.Split(Delimiters.Space, 2);
                Trace.Assert(pathAndBranch.Length >= 1);
                SubmoduleName = pathAndBranch[0].SubstringAfterLast('/'); // Remove path
                BranchText = pathAndBranch.Length == 2 ? " " + pathAndBranch[1] : "";
            }

            public void RefreshDetails()
            {
                ApplyStatus();
            }

            public bool CanOpen => !IsCurrent;

            protected override string DisplayText()
            {
                return SubmoduleName + BranchText + Info.Detailed?.AddedAndRemovedText;
            }

            protected override string NodeName()
            {
                return SubmoduleName;
            }

            public void Open()
            {
                if (Info.Detailed?.RawStatus is not null)
                {
                    UICommands.BrowseSetWorkingDir(Info.Path, ObjectId.WorkTreeId, Info.Detailed.RawStatus.OldCommit);
                    return;
                }

                UICommands.BrowseSetWorkingDir(Info.Path);
            }

            public void LaunchGitExtensions()
            {
                GitUICommands.LaunchBrowse(workingDir: Info.Path.EnsureTrailingPathSeparator(), ObjectId.WorkTreeId, Info?.Detailed?.RawStatus?.OldCommit);
            }

            internal override void OnSelected()
            {
                if (Tree.IgnoreSelectionChangedEvent)
                {
                    return;
                }

                base.OnSelected();
            }

            internal override void OnDoubleClick()
            {
                Open();
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                if (IsCurrent)
                {
                    TreeViewNode.NodeFont = new Font(AppSettings.Font, FontStyle.Bold);
                }

                // Note that status is applied also after the tree is created, when status is applied
                ApplyStatus();
            }

            private void ApplyStatus()
            {
                TreeViewNode.ToolTipText = DisplayText();
                TreeViewNode.ImageKey = GetSubmoduleItemImage(Info?.Detailed);
                TreeViewNode.SelectedImageKey = TreeViewNode.ImageKey;

                return;

                // NOTE: Copied and adapted from FormBrowse.GetSubmoduleItemImage
                static string GetSubmoduleItemImage(DetailedSubmoduleInfo? details)
                {
                    return (details?.Status, details?.IsDirty) switch
                    {
                        (SubmoduleStatus.FastForward, true) => nameof(Images.SubmoduleRevisionUpDirty),
                        (SubmoduleStatus.FastForward, false) => nameof(Images.SubmoduleRevisionUp),
                        (SubmoduleStatus.Rewind, true) => nameof(Images.SubmoduleRevisionDownDirty),
                        (SubmoduleStatus.Rewind, false) => nameof(Images.SubmoduleRevisionDown),
                        (SubmoduleStatus.NewerTime, true) => nameof(Images.SubmoduleRevisionSemiUpDirty),
                        (SubmoduleStatus.NewerTime, false) => nameof(Images.SubmoduleRevisionSemiUp),
                        (SubmoduleStatus.OlderTime, true) => nameof(Images.SubmoduleRevisionSemiDownDirty),
                        (SubmoduleStatus.OlderTime, false) => nameof(Images.SubmoduleRevisionSemiDown),
                        (_, true) => nameof(Images.SubmoduleDirty),
                        (_, false) => nameof(Images.FileStatusModified),
                        _ => nameof(Images.FolderSubmodule)
                    };
                }
            }

            internal async Task SetStatusToolTipAsync(CancellationToken token)
            {
                string toolTip;
                if (Info.Detailed?.RawStatus is not null)
                {
                    // Prefer submodule status, shows ahead/behind
                    toolTip = LocalizationHelpers.ProcessSubmoduleStatus(
                        new GitModule(Info.Path),
                        Info.Detailed.RawStatus,
                        moduleIsParent: false,
                        limitOutput: true);
                }
                else if (GitStatus is not null)
                {
                    var changeCount = new ArtificialCommitChangeCount();
                    changeCount.Update(GitStatus);
                    toolTip = changeCount.GetSummary();
                }
                else
                {
                    // No data need to be set
                    return;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                token.ThrowIfCancellationRequested();
                TreeViewNode.ToolTipText = toolTip;
            }
        }

        // Used temporarily to facilitate building a tree
        private class DummyNode : Node
        {
            public DummyNode() : base(null)
            {
            }
        }

        private sealed class SubmoduleTree : Tree
        {
            private SubmoduleStatusEventArgs? _currentSubmoduleInfo;
            private Nodes? _currentNodes = null;

            public SubmoduleTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
                : base(treeNode, uiCommands)
            {
                SubmoduleStatusProvider.Default.StatusUpdating += Provider_StatusUpdating;
                SubmoduleStatusProvider.Default.StatusUpdated += Provider_StatusUpdated;
            }

            protected override Task OnAttachedAsync()
            {
                var e = _currentSubmoduleInfo;
                if (e is not null)
                {
                    OnStatusUpdated(e);
                }

                return Task.CompletedTask;
            }

            protected override Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                return Task.FromResult(new Nodes(null));
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
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
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
                        var infos = e.Info.AllSubmodules.ToDictionary(info => info.Path, info => info);
                        Validates.NotNull(e.Info.TopProject);
                        infos[e.Info.TopProject.Path] = e.Info.TopProject;
                        var nodes = _currentNodes.DepthEnumerator<SubmoduleNode>().ToList();
                        foreach (var node in nodes)
                        {
                            if (infos.ContainsKey(node.Info.Path))
                            {
                                node.Info = infos[node.Info.Path];
                                infos.Remove(node.Info.Path);
                            }
                            else
                            {
                                // structure no longer matching
                                Debug.Assert(true, $"Status info with {1 + e.Info.AllSubmodules.Count} records do not match current nodes ({nodes.Count})");
                                _currentNodes = null;
                                break;
                            }
                        }

                        if (infos.Count > 0)
                        {
                            Debug.Fail($"{infos.Count} status info records remains after matching current nodes, structure seem to mismatch ({nodes.Count}/{e.Info.AllSubmodules.Count})");
                            _currentNodes = null;
                        }
                    }

                    if (_currentNodes is null)
                    {
                        await ReloadNodesAsync(token =>
                        {
                            cts = CancellationTokenSource.CreateLinkedTokenSource(e.Token, token);
                            loadNodesTask = LoadNodesAsync(e.Info, cts.Token);
                            return loadNodesTask;
                        }).ConfigureAwait(false);
                    }

                    if (cts is not null && loadNodesTask is not null)
                    {
                        _currentNodes = await loadNodesTask;
                    }

                    if (_currentNodes is not null)
                    {
                        var token = cts?.Token ?? e.Token;
                        await LoadNodeDetailsAsync(_currentNodes, token).ConfigureAwaitRunInline();
                        LoadNodeToolTips(_currentNodes, token);
                    }

                    Interlocked.CompareExchange(ref _currentSubmoduleInfo, null, e);
            }).FileAndForget();
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
                token.ThrowIfCancellationRequested();

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

                var threadModule = (GitModule?)result.Module;

                Validates.NotNull(threadModule);

                var submoduleNodes = new List<SubmoduleNode>();

                // We always want to display submodules rooted from the top project.
                CreateSubmoduleNodes(result, threadModule, ref submoduleNodes);

                var nodes = new Nodes(this);
                AddTopAndNodesToTree(ref nodes, submoduleNodes, threadModule, result);
                return nodes;
            }

            private void CreateSubmoduleNodes(SubmoduleInfoResult result, GitModule threadModule, ref List<SubmoduleNode> nodes)
            {
                // result.OurSubmodules/AllSubmodules contain a recursive list of submodules, but don't provide info about the super
                // project path. So we deduce these by substring matching paths against an ordered list of all paths.
                var modulePaths = result.AllSubmodules.Select(info => info.Path).ToList();

                // Add current and parent module paths
                var parentModule = threadModule;
                while (parentModule is not null)
                {
                    modulePaths.Add(parentModule.WorkingDir);
                    parentModule = parentModule.SuperprojectModule;
                }

                // Sort descending so we find the nearest outer folder first
                modulePaths = modulePaths.OrderByDescending(path => path).ToList();

                foreach (var submoduleInfo in result.AllSubmodules)
                {
                    string superPath = GetSubmoduleSuperPath(submoduleInfo.Path);
                    string localPath = Path.GetDirectoryName(submoduleInfo.Path.Substring(superPath.Length)).ToPosixPath();

                    var isCurrent = submoduleInfo.Bold;
                    nodes.Add(new SubmoduleNode(this,
                        submoduleInfo,
                        isCurrent,
                        isCurrent ? result.CurrentSubmoduleStatus : null,
                        localPath,
                        superPath));
                }

                return;

                string GetSubmoduleSuperPath(string submodulePath)
                {
                    var superPath = modulePaths.Find(path => submodulePath != path && submodulePath.Contains(path));
                    Validates.NotNull(superPath);
                    return superPath;
                }
            }

            private string GetNodeRelativePath(GitModule topModule, SubmoduleNode node)
            {
                return node.SuperPath.SubstringAfter(topModule.WorkingDir).ToPosixPath() + node.LocalPath;
            }

            private void AddTopAndNodesToTree(
                ref Nodes nodes,
                List<SubmoduleNode> submoduleNodes,
                GitModule threadModule,
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

                var topModule = threadModule.GetTopModule();

                // Build a mapping of top-module-relative path to node
                var pathToNodes = new Dictionary<string, Node>();

                // Add existing SubmoduleNodes
                foreach (var node in submoduleNodes)
                {
                    pathToNodes[GetNodeRelativePath(topModule, node)] = node;
                }

                // Create and add missing SubmoduleFolderNodes
                foreach (var node in submoduleNodes)
                {
                    var parts = GetNodeRelativePath(topModule, node).Split(Delimiters.ForwardSlash);
                    for (int i = 0; i < parts.Length - 1; ++i)
                    {
                        var path = string.Join("/", parts.Take(i + 1));
                        if (!pathToNodes.ContainsKey(path))
                        {
                            pathToNodes[path] = new SubmoduleFolderNode(this, parts[i]);
                        }
                    }
                }

                // Now build the tree
                var rootNode = new DummyNode();
                var nodesInTree = new HashSet<Node>();
                foreach (var node in submoduleNodes)
                {
                    Node parentNode = rootNode;
                    var parts = GetNodeRelativePath(topModule, node).Split(Delimiters.ForwardSlash);
                    for (int i = 0; i < parts.Length; ++i)
                    {
                        var path = string.Join("/", parts.Take(i + 1));
                        var nodeToAdd = pathToNodes[path];

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
                var topModuleNode = new SubmoduleNode(
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

                // Reset all changes.
                module.Reset(ResetMode.Hard);

                // Also delete new files, if requested.
                if (resetType == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    module.Clean(CleanMode.OnlyNonIgnored, directories: true);
                }
            }

            public void StashSubmodule(IWin32Window owner, SubmoduleNode node)
            {
                var uiCmds = new GitUICommands(new GitModule(node.Info.Path));
                uiCmds.StashSave(owner, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            public void CommitSubmodule(IWin32Window owner, SubmoduleNode node)
            {
                var submodulCommands = new GitUICommands(node.Info.Path.EnsureTrailingPathSeparator());
                submodulCommands.StartCommitDialog(owner);
            }
        }
    }
}
