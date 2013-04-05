using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.UserControls
{
    // "branches"
    public partial class RepoObjectsTree
    {
        static readonly string branchesKey = Guid.NewGuid().ToString();
        static readonly string branchKey = Guid.NewGuid().ToString();
        static readonly string branchPathKey = Guid.NewGuid().ToString();

        static void OnReloadBranches(ICollection<BaseBranchNode> items, RootNode<BaseBranchNode> rootNode)
        {
            // todo: cache: per Repo, on BranchNode.FullPath

            BranchesNode branchesNode = (BranchesNode)rootNode;

            rootNode.TreeNode.Text = string.Format("{0} ({1})", Strings.branches, branchesNode.Branches.Count());
        }

        /// <summary>Adds a <see cref="BaseBranchNode"/>, and recursivley, its children.</summary>
        TreeNode OnAddBranchNode(TreeNodeCollection nodes, BaseBranchNode branchNode)
        {
            bool isBranch = branchNode is BranchNode;
            TreeNode treeNode = new TreeNode(branchNode.Name)
            {
                Name = branchNode.FullPath,
                ContextMenuStrip = isBranch ? menuBranch : menuBranchPath,
                ImageKey = isBranch ? branchKey : branchPathKey,
                SelectedImageKey = isBranch ? branchKey : branchPathKey,
            };

            nodes.Add(treeNode);
            branchNode.TreeNode = treeNode;

            BranchPathNode branchPath = branchNode as BranchPathNode;
            if (branchPath != null)
            {
                foreach (BaseBranchNode child in branchPath.Children)
                {// recurse children
                    OnAddBranchNode(treeNode.Nodes, child);
                }
            }

            return null;// return null bypass duplicate call to ApplyStyle
        }

        #region private classes

        /// <summary>base class for a branch node</summary>
        abstract class BaseBranchNode : Node
        {
            /// <summary>Short name of the branch/branch path. <example>"issue1344"</example></summary>
            public string Name { get; private set; }
            /// <summary>do NOT access this member directly. <remarks>use <see cref="FullPath"/></remarks></summary>
            string _FullPath;
            /// <summary>Full path of the branch. <example>"issues/issue1344"</example></summary>
            public string FullPath
            {
                get
                {
                    return _FullPath ?? (_FullPath = (Parent != null)
                                                         ? string.Format("{0}{1}{2}",
                                                                Parent.FullPath, BranchesNode.SeparatorStr, Name)
                                                         : Name);
                }
            }
            /// <summary>Parent of this <see cref="BaseBranchNode"/>.</summary>
            public BranchPathNode Parent { get; private set; }
            BranchPathNode _Root;
            /// <summary>Root <see cref="BaseBranchNode"/>.</summary>
            public BranchPathNode Root { get { return _Root ?? (_Root = BranchesNode.GetRoot(this)); } }
            internal int Level { get; private set; }

            List<BranchPathNode> _Parents;
            /// <summary>Gets the list of parent <see cref="BranchPathNode"/>s, youngest to oldest.</summary>
            public IEnumerable<BranchPathNode> Parents
            {
                get
                {
                    if (_Parents == null)
                    {
                        if (Parent == null)
                        {
                            _Parents = Enumerable.Empty<BranchPathNode>().ToList();
                        }
                        else
                        {
                            _Parents = new List<BranchPathNode> { Parent };
                            _Parents.AddRange(Parent.Parents);
                        }
                    }
                    return _Parents;
                }
            }

            public override int GetHashCode()
            {
                return FullPath.GetHashCode();
            }

            /// <summary>Two <see cref="BaseBranchNode"/> instances are equal
            ///  if their <see cref="FullPath"/> values are equal.</summary>
            protected bool Equals(BaseBranchNode other)
            {
                if (other == null) { return false; }
                return (other == this)
                    ||
                       string.Equals(FullPath, other.FullPath);
            }

            public override bool Equals(object obj)
            {
                BaseBranchNode branchNode = obj as BaseBranchNode;
                return branchNode != null && Equals(branchNode);
            }

            protected BaseBranchNode(GitUICommands uiCommands,
                string name, int level, BranchPathNode parent)
                : base(uiCommands)
            {
                Name = name;
                Parent = parent;
                Level = level;
            }

            public override string ToString() { return Name; }

            //static bool HasSameParents(string branch, BranchPath other, out BranchPath newPath)
            //{
            //    bool hasSameParents = string.Equals(other.FullPath, GetFullParentPath(branch));
            //    if (hasSameParents == false)
            //    {
            //        newPath = null;
            //        return false;
            //    }
            //    else
            //    {
            //        newPath = new BranchPath(other, branch);
            //        return true;
            //    }
            //}
        }

        /// <summary>Local branch node.</summary>
        sealed class BranchNode : BaseBranchNode
        {
            public BranchNode(GitUICommands uiCommands,
                string branch, int level, string activeBranchPath = null,
                BranchPathNode parent = null, bool isLocal = true)
                : base(uiCommands, BranchesNode.GetName(branch), level, parent)
            {
                IsActive = Equals(activeBranchPath, FullPath);
                IsDraggable = true;
                AllowDrop = true;
            }

            /// <summary>true if this <see cref="BranchNode"/> is the active branch.</summary>
            public bool IsActive { get; private set; }

            /// <summary>Gets the branch's short name.</summary>
            public override string ToString() { return Name; }

            /// <summary>Indicates whether this <see cref="BranchNode"/> is setup for remote tracking.</summary>
            public bool IsTrackingSetup(ConfigFile config)
            {// NOT (not whitespace)
                return !string.IsNullOrWhiteSpace(config.GetValue(GitRef.RemoteSettingName(FullPath)));
            }

            /// <summary>Styles the <see cref="Node.TreeNode"/>.</summary>
            internal override void ApplyStyle()
            {
                base.ApplyStyle();
                if (IsActive)
                {
                    TreeNode.NodeFont = new Font(TreeNode.NodeFont, FontStyle.Bold);
                }
            }

            public override bool Equals(object obj)
            {
                if (base.Equals(obj))
                {
                    BranchNode branchNode = obj as BranchNode;
                    return branchNode != null && IsActive == branchNode.IsActive;
                }

                return false;
            }

            /// <summary>Checkout the branch.</summary>
            internal override void OnDoubleClick()
            {
                base.OnDoubleClick();
                Checkout();
            }

            internal override void OnSelected()
            {
                base.OnSelected();
                UiCommands.BrowseGoToRef(FullPath);
            }
            protected override IEnumerable<DragDropAction> CreateDragDropActions()
            {
                var stashDD = new DragDropAction<StashNode>(
                    (draggedStash) => IsActive,
                    (draggedStash) =>
                    {
                        // normal -> Pop
                        // Alt -> Apply
                        UiCommands.StartStashDialog();
                    });

                var branchDD = new DragDropAction<BranchNode>(draggedBranch =>
                {
                    string activeBranch = UiCommands.Module.GetSelectedBranch();
                    if (Equals(FullPath, activeBranch))
                    {// target is active -> merge dropped
                        return true;
                    }
                    if (Equals(draggedBranch.FullPath, activeBranch))
                    {// dragged is active -> merge dragged
                        return true;
                    }
                    return false;
                }, draggedBranch =>
                {
                    string activeBranch = UiCommands.Module.GetSelectedBranch();
                    if (Equals(FullPath, activeBranch))
                    {// target is active -> merge dropped
                        UiCommands.StartMergeBranchDialog(draggedBranch.FullPath);
                    }
                    if (Equals(draggedBranch.FullPath, activeBranch))
                    {// dropped is active -> merge target
                        UiCommands.StartMergeBranchDialog(FullPath);
                    }
                });

                // RemoteBranch onto (local) branch -> pull
                var remoteBranchDD = new DragDropAction<RemoteBranchNode>(
                    remoteBranch =>
                    {
                        if (Git.IsDirtyDir())
                        {// disallow pull onto the current branch if it's "dirty"
                            // show: "commit changes first"
                            return false;
                        }
                        return remoteBranch.Value.PullConfigs.Any(pull => Equals(pull.LocalBranch, FullPath))
                            && Git.IsBranchBehind(FullPath, remoteBranch.Value);
                    },
                    remoteBranch =>
                    {
                        // checkout this branch
                        if (false)
                        {// can fast-forward pull

                        }
                    });

                return new DragDropAction[] { stashDD, branchDD, remoteBranchDD };
            }

            public void Checkout()
            {
                UiCommands.StartCheckoutBranch(FullPath, false);
            }

            public void CreateBranch()
            {
                UiCommands.StartCreateBranchDialog();
            }

            public void Delete()
            {
                UiCommands.StartDeleteBranchDialog(FullPath);
            }

            public void DeleteForce()
            {
                var branchHead = GitRef.CreateBranchRef(UiCommands.Module, null, FullPath);
                var cmd = new GitDeleteBranchCmd(new GitRef[] { branchHead }, true);
                UiCommands.StartCommandLineProcessDialog(cmd, null);
            }
        }

        //class RemoteBranchNode : BranchNode
        //{
        //    /// <summary>Name of the remote for this remote branch. <example>"origin"</example></summary>
        //    public string Remote { get; private set; }
        //    /// <summary>Full name of the branch, excluding the remote name. <example>"issues/issue1344"</example></summary>
        //    public string FullBranchName { get; private set; }

        //    public RemoteBranchNode(GitUICommands uiCommands,
        //        string remote, string branch, int level, string activeBranchPath = null, BranchPathNode parent = null)
        //        : base(uiCommands, branch, level, activeBranchPath, parent, isLocal: false)
        //    {
        //        Remote = remote;
        //        FullBranchName = FullPath.Substring(FullPath.IndexOf(BranchesNode.Separator));
        //    }

        //!if (targetBranch.IsRemote)
        //{// local branch -> remote branch = push
        //    uiCommands.StartPushDialog(
        //        new GitPushAction(
        //            ((RemoteBranchNode)targetBranch).Remote,
        //            draggedBranch.FullPath,
        //            targetBranch.FullPath));
        //}
        //}

        /// <summary>Part of a path leading to local branch(es)</summary>
        class BranchPathNode : BaseBranchNode
        {
            public IList<BaseBranchNode> Children { get; private set; }
            public BranchPathNode AddChild(BranchPathNode path)
            {
                Children.Add(path);
                return path;
            }

            public void AddChild(BranchNode branch)
            {
                Children.Add(branch);
            }

            /// <summary>Creates a new <see cref="BranchPathNode"/>.</summary>
            /// <param name="name">Short name for the path.</param>
            /// <param name="level">Level of the node in the tree.</param>
            /// <param name="parent">Parent node. Leave NULL if this is a Root.</param>
            public BranchPathNode(GitUICommands uiCommands,
                string name, int level, BranchPathNode parent = null)
                : base(uiCommands, name, level, parent)
            {
                Children = new List<BaseBranchNode>();
            }

            public override bool Equals(object obj)
            {
                BranchPathNode other = obj as BranchPathNode;
                return other != null &&
                    base.Equals(other) &&
                    Children.SequenceEqual(other.Children);
            }

            public override string ToString()
            {
                return string.Format("{0}{1}", Name, BranchesNode.SeparatorStr);
            }

            public T Recurse<T>(T seed, Func<BaseBranchNode, T, T> aggregate)
            {
                T egg = seed;

                foreach (BaseBranchNode baseBranchNode in Children)
                {
                    egg = aggregate(baseBranchNode, seed);
                    BranchPathNode branchPath = baseBranchNode as BranchPathNode;
                    if (branchPath != null)
                    {
                        egg = branchPath.Recurse(egg, aggregate);
                    }
                }
                return egg;
            }

            internal override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeNode.NodeFont = new Font(TreeNode.NodeFont, FontStyle.Italic);
            }

            public void CreateWithin()
            {
                throw new NotImplementedException();
            }

            public void DeleteAll()
            {
                throw new NotImplementedException();
            }

            public void DeleteAllForce()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>Root "branches" node.</summary>
        ///// <summary>list of <see cref="BaseBranchNode"/>s, including the <see cref="Active"/> branch, if applicable</summary>
        class BranchesNode : RootNode<BaseBranchNode>
        {
            public BranchesNode(TreeNode rootNode, GitUICommands uiCommands,
                Func<ICollection<BaseBranchNode>> getBranches, Func<TreeNodeCollection, BaseBranchNode, TreeNode> onAddBranchNode)
                : base(
                rootNode,
                uiCommands,
                getBranches,
                null,
                OnReloadBranches,
                onAddBranchNode) { }

            /// <summary>Gets the current active branch. <remarks>May be null, if HEAD is detached.</remarks></summary>
            public BaseBranchNode Active { get; private set; }

            /// <summary>Gets the list of branch names from the raw output.</summary>
            public IEnumerable<string> Branches { get; private set; }

            public override int GetHashCode() { return Branches.GetHash(); }
            /// <summary>'/'</summary>
            public static char Separator = GitModule.RefSeparator;
            /// <summary>"/"</summary>
            public static string SeparatorStr = GitModule.RefSep;

            /// <summary>Gets the root <see cref="BranchPathNode"/> or this, if it is the root.</summary>
            public static BranchPathNode GetRoot(BaseBranchNode node)
            {
                return (node.Parent != null)
                           ? GetRoot(node.Parent)
                           : node as BranchPathNode;
            }

            /// <summary>Gets the name of the branch or path part.</summary>
            public static string GetName(string branch)
            {
                return IsOrHasParent(branch)
                           ? branch.Substring(EndOfParentPath(branch))
                           : branch;
            }

            static bool IsParentOf(BaseBranchNode parent, string branch)
            {
                return String.Equals((string)GetFullParentPath(branch), parent.FullPath);
            }

            /// <summary>Indicates whether a branch HAS or IS a parent.</summary>
            static bool IsOrHasParent(string branch)
            {
                return branch.Contains(SeparatorStr);
            }

            /// <summary>Indicates whether the specified</summary>
            static bool IsInFamilyTree(BranchPathNode parent, string branch, out BranchPathNode commonAncestor)
            {
                if (IsParentOf(parent, branch))
                {
                    commonAncestor = parent;
                    return true;
                }
                commonAncestor = null;
                return
                    (parent.Parent != null)
                    &&
                    IsInFamilyTree(parent.Parent, branch, out commonAncestor);
            }

            /// <summary>Gets the full path of the parent for the specified <paramref name="branch"/>.</summary>
            static string GetFullParentPath(string branch)
            {
                return branch.Substring(0, EndOfParentPath(branch));
            }

            /// <summary>Gets the end index of the parent of the specified <paramref name="branch"/>.</summary>
            static int EndOfParentPath(string branch)
            {
                return branch.LastIndexOf(Separator);
            }

            /// <summary>Gets the hierarchical branch tree from the specified list of <paramref name="branches"/>.</summary>
            public static ICollection<BaseBranchNode> GetBranchTree(GitUICommands uiCommands, IEnumerable<string> branches)
            {
                // (input)
                // a-branch
                // develop/crazy-branch
                // develop/features/feat-next
                // develop/features/feat-next2
                // develop/issues/iss444
                // develop/wild-branch
                // issues/iss111
                // master
                // 
                // ->
                // (output)
                // 0 a-branch
                // 0 develop/
                // 1   features/
                // 2      feat-next
                // 2      feat-next2
                // 1   issues/
                // 2      iss444
                // 1   wild-branch
                // 1   wilds/
                // 2      card
                // 0 issues/
                // 1     iss111
                // 0 master

                #region get active branch and scrub

                var rawBranches = branches.ToList();
                var branchList = rawBranches.ToList();
                string activeBranch =
                    branchList.FirstOrDefault(
                        branch => branch.StartsWith(GitModule.ActiveBranchIndicatorStr));
                if (activeBranch != null)
                {
                    activeBranch = activeBranch.TrimStart(GitModule.ActiveBranchIndicator, ' ');
                    branchList[0] = activeBranch;
                }
                branches = branchList;
                #endregion get active branch and scrub

                branches = branches.OrderBy(branch => branch);// orderby name
                BranchPathNode currentParent = null;
                List<BaseBranchNode> nodes = new List<BaseBranchNode>();

                foreach (string branch in branches)
                {
                    if (IsOrHasParent(branch) == false)
                    {// just a plain branch (master)
                        nodes.Add(new BranchNode(uiCommands, branch, 1, activeBranch));
                    }
                    // (else has/is parent)

                    else if (currentParent == null)
                    {// (has/is parent) -> return all parents and branch
                        nodes.Add(GetChildren(uiCommands, null, branch, activeBranch, out currentParent));
                    }
                    // (else currentParent NOT null)

                    else if (IsInFamilyTree(currentParent, branch, out currentParent))
                    {
                        GetChildren(uiCommands, currentParent, branch, activeBranch, out currentParent);
                    }
                    else
                    {
                        nodes.Add(GetChildren(uiCommands, null, branch, activeBranch, out currentParent));
                    }
                }
                return nodes;
            }

            /// <summary>Gets the children of the specified <paramref name="parent"/> node OR
            /// Creates a new lineage of <see cref="BaseBranchNode"/>s.</summary>
            static BranchPathNode GetChildren(GitUICommands uiCommands, BranchPathNode parent, string branch, string activeBranch, out BranchPathNode currentParent)
            {
                string[] splits = branch.Split(Separator);// issues/iss1334 -> issues, iss1334
                int nParents = splits.Length - 1;

                currentParent = parent // common ancestor
                                ?? new BranchPathNode(uiCommands, splits[0], 0);

                // benchmarks/-main
                // benchmarks/get-branches

                // start adding children at Parent.Level +1
                for (int i = currentParent.Level + 1; i < nParents; i++)
                {// parent0:parentN
                    currentParent = currentParent.AddChild(
                        new BranchPathNode(uiCommands, splits[i], i, currentParent));
                }
                // child
                currentParent.AddChild(new BranchNode(uiCommands, splits[splits.Length - 1], nParents, activeBranch, currentParent));
                return currentParent.Root;
            }

            /// <summary>Finds a specified <see cref="BaseBranchNode"/> with the given full name.</summary>
            /// <typeparam name="TBranchNode">Type of <see cref="BaseBranchNode"/> to search for.</typeparam>
            public static TBranchNode Find<TBranchNode>(IEnumerable<BaseBranchNode> nodes, string fullPath)
                where TBranchNode : BaseBranchNode
            {
                if (fullPath == null) { return null; }

                foreach (var branchNode in nodes)
                {
                    if (branchNode.FullPath.Equals(fullPath))
                    {
                        return branchNode as TBranchNode;
                    }
                    BranchPathNode branchPath = branchNode as BranchPathNode;
                    if (branchPath != null)
                    {
                        var match = Find<TBranchNode>(branchPath.Children, fullPath);
                        if (match != null)
                        {
                            return match;
                        }
                    }
                }
                return null;
            }

            /// <summary>Occurs on the background thread immediately before <see cref="RootNode{TChild}.ReloadNodes"/> is called.</summary>
            protected override void OnReloading(ICollection<BaseBranchNode> olds, ICollection<BaseBranchNode> news)
            {
                base.OnReloading(olds, news);

                // recursive get branches...
                List<string> branches = new List<string>();

                Func<List<string>, BaseBranchNode, List<string>> getChildBranches = ((seed, node) =>
                {
                    BranchPathNode branchPath = node as BranchPathNode;
                    if (branchPath != null)
                    {// BranchPath
                        return branchPath.Recurse(
                              seed,
                              (bbp, count) => (from branchNode in branchPath.Children
                                               let branch = branchNode as BranchNode
                                               where branch != null
                                               select branch.FullPath).ToList());
                    }// else Branch
                    return new List<string> { node.FullPath };
                });


                foreach (BaseBranchNode baseBranchNode in news)
                {
                    branches.AddRange(getChildBranches(branches, baseBranchNode));
                }

                Branches = branches;

                SetFavorites(news);
            }

            void SetFavorites(ICollection<BaseBranchNode> news)
            {
                favorites.Clear();
                var favs = (from section in UiCommands.Module.GetLocalConfig().ConfigSections
                            where Equals(section.SectionName, "branch")
                            let value = section.GetValue("fav")
                            where value.IsNotNullOrWhitespace()
                            select section.SubSection).ToList();
                if (favs.Any() == false)
                {// no branches config'd -> return
                    return;
                } // else...

                Func<List<BranchNode>, BaseBranchNode, List<BranchNode>> getFavBranches = ((seed, node) =>
                {
                    BranchPathNode branchPath = node as BranchPathNode;
                    if (branchPath != null)
                    {
                        // BranchPath
                        return branchPath.Recurse(
                            seed,
                            (bbp, favors) => (from branchNode in branchPath.Children
                                              let branch = branchNode as BranchNode
                                              where branch != null
                                              let isFav = favs.Any(fav => Equals(fav, branchNode.FullPath))
                                              where isFav
                                              select branch).ToList());
                    } // else (Branch)
                    BranchNode branchNod = node as BranchNode;
                    if (branchNod != null && favs.Any(fav => Equals(fav, branchNod.FullPath)))
                    {// branch AND 
                        return new List<BranchNode> { branchNod };
                    }

                    return new List<BranchNode>();
                });

                foreach (BaseBranchNode baseBranchNode in news)
                {
                    favorites.AddRange(getFavBranches(favorites, baseBranchNode));
                }
            }

            List<BranchNode> favorites = new List<BranchNode>();

            protected override IEnumerable<DragDropAction> CreateDragDropActions()
            {
                return new DragDropAction[]
                {
                    new DragDropAction<BranchNode>(
                        branch => true,
                        branch =>
                        {
                            using (FormCheckoutBranch branchForm = new FormCheckoutBranch(
                                    UiCommands, branch.FullPath, false))
                            {
                                branchForm.ShowDialog();
                            }
                        }), 
                };
            }

            public void CreateBranch()
            {
                throw new NotImplementedException();
            }
        }

        #endregion private classes

    }
}
