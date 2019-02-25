using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Submodules;
using GitUI.Properties;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        // If true, display submodules as a tree of folders and nodes; otherwise, display a single node per module
        // as in the Submodules menu.
        // TODO: Possibly expose this as a user option, or just get rid of it.
        private static bool UseFolderTree = true;

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
            private DetailedSubmoduleInfo _details;

            public SubmoduleInfo Info { get; }
            public bool IsCurrent { get; }
            public string LocalPath { get; }
            public string SuperPath { get; }
            public string SubmoduleName { get; }
            public string BranchText { get; }

            public SubmoduleNode(Tree tree, SubmoduleInfo submoduleInfo, bool isCurrent, string localPath, string superPath)
                : base(tree)
            {
                Info = submoduleInfo;
                IsCurrent = isCurrent;
                LocalPath = localPath;
                SuperPath = superPath;

                // Extract submodule name and branch
                // e.g. Info.Text = "Externals/conemu-inside [no branch]"
                // Note that the branch portion won't be there if the user hasn't yet init'd + updated the submodule.
                var pathAndBranch = Info.Text.Split(new char[] { ' ' }, 2);
                Trace.Assert(pathAndBranch.Length >= 1);
                SubmoduleName = pathAndBranch[0].SubstringAfterLast('/'); // Remove path
                BranchText = pathAndBranch.Length == 2 ? " " + pathAndBranch[1] : "";
            }

            public async Task LoadDetailsAsync(CancellationToken token)
            {
                token.ThrowIfCancellationRequested();

                if (Info.Detailed == null)
                {
                    return;
                }

                _details = await Info.Detailed.GetValueAsync(token);
                if (_details != null)
                {
                    await Tree.TreeViewNode.TreeView.InvokeAsync(() =>
                    {
                        ApplyText();
                        ApplyStyle();
                    }, token);
                }
            }

            public bool CanOpen => !IsCurrent;

            protected override string DisplayText()
            {
                if (!UseFolderTree)
                {
                    return Info.Text;
                }

                return SubmoduleName + BranchText + _details?.AddedAndRemovedText;
            }

            protected override string NodeName()
            {
                return SubmoduleName;
            }

            public void Open()
            {
                UICommands.BrowseSetWorkingDir(Info.Path);
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

                Trace.Assert(TreeViewNode != null);

                if (IsCurrent)
                {
                    TreeViewNode.NodeFont = new Font(AppSettings.Font, FontStyle.Bold);
                }

                if (_details == null)
                {
                    TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.FolderSubmodule);
                }
                else
                {
                    TreeViewNode.ImageKey = GetSubmoduleItemImage(_details);
                }

                TreeViewNode.SelectedImageKey = TreeViewNode.ImageKey;

                return;

                // NOTE: Copied and adapated from FormBrowse.GetSubmoduleItemImage
                string GetSubmoduleItemImage(DetailedSubmoduleInfo details)
                {
                    if (details.Status == null)
                    {
                        return nameof(Images.FolderSubmodule);
                    }

                    if (details.Status == SubmoduleStatus.FastForward)
                    {
                        return details.IsDirty ? nameof(Images.SubmoduleRevisionUpDirty) : nameof(Images.SubmoduleRevisionUp);
                    }

                    if (details.Status == SubmoduleStatus.Rewind)
                    {
                        return details.IsDirty ? nameof(Images.SubmoduleRevisionDownDirty) : nameof(Images.SubmoduleRevisionDown);
                    }

                    if (details.Status == SubmoduleStatus.NewerTime)
                    {
                        return details.IsDirty ? nameof(Images.SubmoduleRevisionSemiUpDirty) : nameof(Images.SubmoduleRevisionSemiUp);
                    }

                    if (details.Status == SubmoduleStatus.OlderTime)
                    {
                        return details.IsDirty ? nameof(Images.SubmoduleRevisionSemiDownDirty) : nameof(Images.SubmoduleRevisionSemiDown);
                    }

                    return details.IsDirty ? nameof(Images.SubmoduleDirty) : nameof(Images.FileStatusModified);
                }
            }
        }

        // Used temporarily to faciliate building a tree
        private class DummyNode : Node
        {
            public DummyNode() : base(null)
            {
            }
        }

        private sealed class SubmoduleTree : Tree
        {
            private SubmoduleInfo _topProjectInfo;

            public SubmoduleTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
                : base(treeNode, uiCommands)
            {
                SubmoduleStatusProvider.Default.StatusUpdated += Provider_StatusUpdated;
            }

            protected override void PostRepositoryChanged()
            {
                // Do nothing, we don't care about this event.
            }

            public override async Task RefreshTreeAsync()
            {
                // Will call LoadNodesAsync with the last cached SubmoduleInfoResult
                await Task.Run(() => SubmoduleStatusProvider.Default.ResendCachedStatus());
            }

            private void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
                if (TreeViewNode.TreeView == null)
                {
                    return;
                }

                ReloadNodesAsync((token) => LoadNodesAsync(e.Info, token)).FileAndForget();
            }

            private async Task LoadNodesAsync(SubmoduleInfoResult info, CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();

                FillSubmoduleTree(info);
            }

            private async Task LoadNodeDetailsAsync(CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                var tasks = Nodes.DepthEnumerator<SubmoduleNode>().Select(node => node.LoadDetailsAsync(token)).ToList();
                await Task.WhenAll(tasks);
            }

            protected override void PostFillTreeViewNode(CancellationToken token, bool firstTime)
            {
                if (firstTime)
                {
                    TreeViewNode.ExpandAll();
                }

                TreeViewNode.Text = Strings.Submodules;

                ThreadHelper.JoinableTaskFactory.RunAsync(async () => await LoadNodeDetailsAsync(token)).FileAndForget();
            }

            private void FillSubmoduleTree(SubmoduleInfoResult result)
            {
                _topProjectInfo = result.TopProject;

                var threadModule = (GitModule)result.Module;

                var nodes = new List<SubmoduleNode>();

                // We always want to display submodules rooted from the top project. If the currently open project is the top project,
                // OurSubmodules contains all child submodules recursively; otherwise, if we're currently in a submodule, SuperSubmodules
                // contains all submodule info relative to the top project.
                if (result.SuperSubmodules?.Count > 0)
                {
                    CreateSubmoduleNodes(result.SuperSubmodules, threadModule, ref nodes);
                }
                else
                {
                    CreateSubmoduleNodes(result.OurSubmodules, threadModule, ref nodes);
                }

                AddNodesToTree(nodes, threadModule);
            }

            private void CreateSubmoduleNodes(IEnumerable<SubmoduleInfo> submodules, GitModule threadModule, ref List<SubmoduleNode> nodes)
            {
                // result.OurSubmodules/SuperSubmodules contain a recursive list of submodules, but don't provide info about the super
                // project path. So we deduce these by substring matching paths against an ordered list of all paths.
                var modulePaths = submodules.Select(info => info.Path).ToList();

                // Add current and parent module paths
                var parentModule = threadModule;
                while (parentModule != null)
                {
                    modulePaths.Add(parentModule.WorkingDir);
                    parentModule = parentModule.SuperprojectModule;
                }

                // Sort descending so we find the nearest outer folder first
                modulePaths = modulePaths.OrderByDescending(path => path).ToList();

                foreach (var submoduleInfo in submodules)
                {
                    string superPath = GetSubmoduleSuperPath(submoduleInfo.Path);
                    string localPath = PathUtil.GetDirectoryName(submoduleInfo.Path.Substring(superPath.Length).ToPosixPath());

                    var isCurrent = submoduleInfo.Bold;
                    nodes.Add(new SubmoduleNode(this, submoduleInfo, isCurrent, localPath, superPath));
                }

                return;

                string GetSubmoduleSuperPath(string submodulePath)
                {
                    var superPath = modulePaths.Find(path => submodulePath != path && submodulePath.Contains(path));
                    Trace.Assert(superPath != null);
                    return superPath;
                }
            }

            private string GetNodeRelativePath(GitModule topModule, SubmoduleNode node)
            {
                return node.SuperPath.SubstringAfter(topModule.WorkingDir).ToPosixPath() + node.LocalPath;
            }

            private void AddNodesToTree(List<SubmoduleNode> nodes, GitModule threadModule)
            {
                if (!UseFolderTree)
                {
                    Nodes.AddNodes(nodes);
                    return;
                }

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
                foreach (var node in nodes)
                {
                    pathToNodes[GetNodeRelativePath(topModule, node)] = node;
                }

                // Create and add missing SubmoduleFolderNodes
                foreach (var node in nodes)
                {
                    var parts = GetNodeRelativePath(topModule, node).Split('/');
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
                var nodesInTree = new List<Node>();
                foreach (var node in nodes)
                {
                    Node parentNode = rootNode;
                    var parts = GetNodeRelativePath(topModule, node).Split('/');
                    for (int i = 0; i < parts.Length; ++i)
                    {
                        var path = string.Join("/", parts.Take(i + 1));
                        var nodeToAdd = pathToNodes[path];

                        // If node is not already in the tree, add it
                        if (nodesInTree.FirstOrDefault(n => n == nodeToAdd) == default(Node))
                        {
                            parentNode.Nodes.AddNode(nodeToAdd);
                            nodesInTree.Add(nodeToAdd);
                        }

                        parentNode = nodeToAdd;
                    }
                }

                // Add top-module node, and move children of root to it
                var topModuleNode = new SubmoduleNode(this, _topProjectInfo, _topProjectInfo.Bold, "", _topProjectInfo.Path);
                topModuleNode.Nodes.AddNodes(rootNode.Nodes);
                Nodes.AddNode(topModuleNode);
            }

            public void UpdateSubmodule(IWin32Window owner, SubmoduleNode node)
            {
                UICommands.StartUpdateSubmoduleDialog(owner, node.LocalPath, node.SuperPath);
            }

            public void OpenSubmodule(IWin32Window owner, SubmoduleNode node)
            {
                node.Open();
            }

            public void ManageSubmodules(IWin32Window owner)
            {
                UICommands.StartSubmodulesDialog(owner);
            }

            public void SynchronizeSubmodules(IWin32Window owner)
            {
                UICommands.StartSyncSubmodulesDialog(owner);
            }
        }
    }
}