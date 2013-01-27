using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    // "branches"
    public partial class RepoObjectsTree
    {
        /// <summary>branches tracker</summary>
        ListWatcher<BranchNode> _branchesWatcher;

        /// <summary>Readies branches for a new repo.</summary>
        void NewRepoBranches()
        {
            _branchesWatcher = new ListWatcher<BranchNode>(() =>
                {
                    var branchNames = git.GetBranchNames().ToArray();
                    return BranchNode.GetBranchTree(branchNames);
                },
                branches => treeMain.Update(() => ResetBranchNodes(branches))
            );
        }

        /// <summary>Async loads the repo's branches into the tree.</summary>
        Task LoadBranches()
        {
            return _branchesWatcher.CheckUpdateAsync();
        }

        /// <summary>Applies the specified <see cref="BranchNode"/>s to the <see cref="TreeView"/>.</summary>
        void ResetBranchNodes(ICollection<BranchNode> branches)
        {
            nodeBranches.Nodes.Clear();
            nodeBranches.Text = string.Format("branches ({0})", branches.Count);

            // todo: cache: per Repo, on BranchNode.FullPath

            foreach (BranchNode node in branches)
            {
                AddBranchNode(nodeBranches.Nodes, node);
            }
        }

        /// <summary>Adds a <see cref="BranchNode"/>, and recursivley, its children.</summary>
        void AddBranchNode(TreeNodeCollection nodes, BranchNode branchNode)
        {
            TreeNode treeNode = nodes.Add(branchNode.FullPath, branchNode.Name);
            treeNode.Tag = branchNode;
            BranchPath branchPath = branchNode as BranchPath;
            if (branchPath != null)
            {
                ApplyBranchPathStyle(treeNode);
                foreach (BranchNode child in branchPath.Children)
                {// recurse children
                    AddBranchNode(treeNode.Nodes, child);
                }
            }
            else
            {
                ApplyBranchStyle(treeNode);
            }
        }

        #region Styles

        /// <summary>Applies a style for a <see cref="Branch"/>.</summary>
        void ApplyBranchStyle(TreeNode node)
        {
            ApplyBranchNodeStyle(node);
            if (((Branch)node.Tag).IsActive)
            {
                node.NodeFont = new Font(node.NodeFont, FontStyle.Bold);
            }

            // ...
        }

        /// <summary>Applies a style for a <see cref="BranchPath"/>.</summary>
        void ApplyBranchPathStyle(TreeNode node)
        {
            ApplyBranchNodeStyle(node);
            // ...
        }

        /// <summary>Apples the style for a <see cref="BranchNode"/>. 
        /// <remarks>Should be invoked from a more specific style.</remarks></summary>
        void ApplyBranchNodeStyle(TreeNode node)
        {
            ApplyTreeNodeStyle(node);
            // ...
        }

        #endregion Styles

        #region private classes

        /// <summary>base class for a branch node</summary>
        abstract class BranchNode
        {
            protected static char Separator = '/';
            protected static string SeparatorStr = Separator.ToString();

            public string Name { get; private set; }
            /// <summary>do NOT access this member directly. <remarks>use <see cref="FullPath"/></remarks></summary>
            string _FullPath;
            public string FullPath
            {
                get
                {
                    return _FullPath ?? (_FullPath = (Parent != null)
                                                         ? string.Format("{0}{1}{2}",
                                                                Parent.FullPath, SeparatorStr, Name)
                                                         : Name);
                }
            }
            public BranchPath Parent { get; private set; }
            BranchPath _Root;
            public BranchPath Root { get { return _Root ?? (_Root = GetRoot(this)); } }
            internal int Level { get; private set; }

            public override int GetHashCode()
            {
                return FullPath.GetHashCode();
            }

            protected bool Equals(BranchNode other)
            {
                if (other == null) { return false; }
                return other == this || string.Equals(_FullPath, other._FullPath);
            }

            public override bool Equals(object obj)
            {
                BranchNode branchNode = obj as BranchNode;
                return branchNode != null && Equals(branchNode);
            }

            protected BranchNode(string name, int level, BranchPath parent)
            {
                Name = name;
                Parent = parent;
                Level = level;
            }

            public override string ToString()
            {
                return Name;
            }

            /// <summary>Gets the root <see cref="BranchPath"/> or this, if it is the root.</summary>
            protected static BranchPath GetRoot(BranchNode node)
            {
                return (node.Parent != null)
                    ? GetRoot(node.Parent)
                    : node as BranchPath;
            }

            /// <summary>Gets the name of the branch or path part.</summary>
            protected static string GetName(string branch)
            {
                return IsOrHasParent(branch)
                    ? branch.Substring(EndOfParentPath(branch))
                    : branch;
            }

            protected static bool IsParentOf(BranchNode parent, string branch)
            {
                return string.Equals(GetFullParentPath(branch), parent.FullPath);
            }

            /// <summary>Indicates whether a branch HAS or IS a parent.</summary>
            protected static bool IsOrHasParent(string branch)
            {
                return branch.Contains(SeparatorStr);
            }

            /// <summary>Indicates whether the specified</summary>
            static bool IsInFamilyTree(BranchPath parent, string branch, out BranchPath commonAncestor)
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

            /// <summary>Gets the full path of the parent for the specified <paramref name="branch"/>.</summary>
            protected static string GetFullParentPath(string branch)
            {
                return branch.Substring(0, EndOfParentPath(branch));
            }

            /// <summary>Gets the end index of the parent of the specified <paramref name="branch"/>.</summary>
            protected static int EndOfParentPath(string branch)
            {
                return branch.LastIndexOf(Separator);
            }

            /// <summary>Gets the hierarchical branch tree from the specified list of <paramref name="branches"/>.</summary>
            public static BranchList GetBranchTree(IEnumerable<string> branches)
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
                BranchPath currentParent = null;
                List<BranchNode> nodes = new List<BranchNode>();

                foreach (string branch in branches)
                {
                    if (IsOrHasParent(branch) == false)
                    {// just a plain branch (master)
                        nodes.Add(new Branch(branch, 1, activeBranch));
                    }
                    // (else has/is parent)

                    else if (currentParent == null)
                    {// (has/is parent) -> return all parents and branch
                        nodes.Add(GetChildren(null, branch, activeBranch, out currentParent));
                    }
                    // (else currentParent NOT null)

                    else if (IsInFamilyTree(currentParent, branch, out currentParent))
                    {
                        GetChildren(currentParent, branch, activeBranch, out currentParent);
                    }
                    else
                    {
                        nodes.Add(GetChildren(null, branch, activeBranch, out currentParent));
                    }
                }
                return new BranchList(activeBranch, nodes, rawBranches);
            }

            /// <summary>Gets the children of the specified <paramref name="parent"/> node OR
            /// Creates a new lineage of <see cref="BranchNode"/>s.</summary>
            static BranchPath GetChildren(BranchPath parent, string branch, string activeBranch, out BranchPath currentParent)
            {
                var splits = branch.Split(Separator);
                int nParents = splits.Length - 1;

                currentParent = parent // common ancestor
                    ?? new BranchPath(splits[0], 0);

                // benchmarks/-main
                // benchmarks/get-branches

                // start adding children at Parent.Level +1
                for (int i = currentParent.Level + 1; i < nParents; i++)
                {// parent0:parentN
                    currentParent = currentParent.AddChild(
                        new BranchPath(splits[i], i, currentParent));
                }
                // child
                currentParent.AddChild(new Branch(splits[splits.Length - 1], nParents, activeBranch, currentParent));
                return currentParent.Root;
            }

            /// <summary>Finds a specified <see cref="BranchNode"/> with the given full name.</summary>
            /// <typeparam name="TBranchNode">Type of <see cref="BranchNode"/> to search for.</typeparam>
            public static TBranchNode Find<TBranchNode>(IEnumerable<BranchNode> nodes, string fullPath)
                  where TBranchNode : BranchNode
            {
                if (fullPath == null) { return null; }

                foreach (var branchNode in nodes)
                {
                    if (branchNode.FullPath.Equals(fullPath))
                    {
                        return branchNode as TBranchNode;
                    }
                    BranchPath branchPath = branchNode as BranchPath;
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
        }

        /// <summary>branch</summary>
        class Branch : BranchNode
        {
            public Branch(string branch, int level, string activeBranchPath = null, BranchPath parent = null)
                : base(GetName(branch), level, parent)
            {
                IsActive = Equals(activeBranchPath, FullPath);
            }

            public bool IsActive { get; private set; }

            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>part of a path leading to a branch(s)</summary>
        class BranchPath : BranchNode
        {
            public List<BranchNode> Children { get; private set; }
            public BranchPath AddChild(BranchPath path)
            {
                Children.Add(path);
                return path;
            }

            public void AddChild(Branch branch)
            {
                Children.Add(branch);
            }

            /// <summary>Creates a new <see cref="BranchPath"/>.</summary>
            /// <param name="name">Short name for the path.</param>
            /// <param name="level">Level of the node in the tree.</param>
            /// <param name="parent">Parent node. Leave NULL if this is a Root.</param>
            public BranchPath(string name, int level, BranchPath parent = null)
                : base(name, level, parent)
            {
                Children = new List<BranchNode>();
            }

            public override bool Equals(object obj)
            {
                BranchPath other = obj as BranchPath;
                return other != null && string.Equals(Name, other.Name);
            }

            public override string ToString()
            {
                return string.Format("{0}{1}", Name, SeparatorStr);
            }
        }

        /// <summary>list of <see cref="BranchNode"/>s, including the <see cref="Active"/> branch, if applicable</summary>
        class BranchList : Collection<BranchNode>
        {
            IEnumerable<string> _Branches;

            public BranchList(BranchNode active, IList<BranchNode> nodes, IEnumerable<string> branches)
                : base(nodes)
            {
                Active = active;
                _Branches = branches;
            }
            public BranchList(string active, IList<BranchNode> nodes, IEnumerable<string> branches)
                : this(BranchNode.Find<Branch>(nodes, active), nodes, branches) { }

            /// <summary>Gets the current active branch. <remarks>May be null, if HEAD is detached.</remarks></summary>
            public BranchNode Active { get; private set; }

            /// <summary>Gets the list of branch names from the raw output.</summary>
            public IEnumerable<string> Branches { get { return _Branches; } }

            public override int GetHashCode() { return Branches.GetHash(); }
        }

        #endregion private classes


    }
}
