using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    // "branches" implementation
    public partial class RepoObjectsTree
    {
        /// <summary>current nodes being displayed</summary>
        IList<BranchNode> currentNodes;
        /// <summary>aggregate hash for <see cref="currentNodes"/></summary>
        int branchNodesHash;

        /// <summary>Gets the branches hierarchical tree from the specified list of <paramref name="branches"/>.
        /// <remarks>If applicable, uses a cached tree.</remarks></summary>
        IList<BranchNode> GetBranchTree(IEnumerable<string> branches)
        {
            if (currentNodes != null)
            {// already populated..
                branches = branches as string[] ?? branches.ToArray();
                if (branchNodesHash == branches.GetHash()) //! may be faulty assumption
                {// matching hashes (same branches) -> return current nodes
                    return currentNodes;
                }
            }

            currentNodes = BranchNode.GetBranchesTree(branches);
            branchNodesHash = currentNodes.GetHash();

            return currentNodes;
        }

        /// <summary>Applies the specified <see cref="BranchNode"/>s to the <see cref="TreeView"/>.</summary>
        void ResetBranchNodes(IEnumerable<BranchNode> branchNodes)
        {
            nodeBranches.Value.Nodes.Clear();
            // todo: cache: per Repo, on BranchNode.FullPath

            foreach (BranchNode node in branchNodes)
            {
                Add(nodeBranches.Value.Nodes, node);
            }
        }

        /// <summary>Adds a <see cref="BranchNode"/>, and recursivley, its children.</summary>
        void Add(TreeNodeCollection nodes, BranchNode branchNode)
        {
            TreeNode treeNode = nodes.Add(branchNode.FullPath, branchNode.Name);
            BranchPath branchPath = branchNode as BranchPath;
            if (branchPath != null)
            {
                ApplyBranchPathStyle(treeNode);
                foreach (BranchNode child in branchPath.Children)
                {// recurse children
                    Add(treeNode.Nodes, child);
                }
            }
            else
            {
                ApplyBranchStyle(treeNode);
            }
        }

        /// <summary>Applies a style for a <see cref="Branch"/>.</summary>
        void ApplyBranchStyle(TreeNode node)
        {
            ApplyBranchNodeStyle(node);
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

        /// <summary>base class for a branch node</summary>
        abstract class BranchNode
        {
            protected static char Separator = '/';
            protected static string SeparatorStr = Separator.ToString();

            public string Name { get; private set; }
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
                return _FullPath.GetHashCode();
            }

            protected bool Equals(BranchNode other)
            {
                return ReferenceEquals(this, other) || string.Equals(_FullPath, other._FullPath);
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

            /// <summary>Gets the branches hierarchical tree from the specified list of <paramref name="branches"/>.</summary>
            public static IList<BranchNode> GetBranchesTree(IEnumerable<string> branches)
            {
                /* (input)
                 * a-branch
                 * develop/crazy-branch
                 * develop/features/feat-next
                 * develop/features/feat-next2
                 * develop/issues/iss444
                 * develop/wild-branch
                 * issues/iss111
                 * master
                 * 
                 * ->
                 * (output)
                 * 0 a-branch
                 * 0 develop/
                 * 1   features/
                 * 2      feat-next
                 * 2      feat-next2
                 * 1   issues/
                 * 2      iss444
                 * 1   wild-branch
                 * 1   wilds/
                 * 2      card
                 * 0 issues/
                 * 1     iss111
                 * 0 master
                 */

                branches = branches.OrderBy(branch => branch);// orderby name

                BranchPath currentParent = null;

                List<BranchNode> nodes = new List<BranchNode>();

                foreach (string branch in branches)
                {
                    if (IsOrHasParent(branch) == false)
                    {// just a plain branch (master)
                        nodes.Add(new Branch(branch, 1));
                    }
                    // (else has/is parent)

                    else if (currentParent == null)
                    {// (has/is parent) -> return all parents and branch
                        nodes.Add(GetBranchNodes(null, branch, out currentParent));
                    }
                    // (else currentParent NOT null)

                    else if (IsInFamilyTree(currentParent, branch, out currentParent))
                    {
                        GetBranchNodes(currentParent, branch, out currentParent);
                    }
                    else
                    {
                        nodes.Add(GetBranchNodes(null, branch, out currentParent));
                    }
                }
                return nodes;
            }

            static BranchPath GetBranchNodes(BranchPath parent, string branch, out BranchPath currentParent)
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
                currentParent.AddChild(new Branch(splits[splits.Length - 1], nParents, currentParent));
                return currentParent.Root;
            }
        }

        /// <summary>branch</summary>
        class Branch : BranchNode
        {
            public Branch(string branch, int level, BranchPath parent = null)
                : base(GetName(branch), level, parent) { }

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


            /// <summary>Root parent (has no parent).</summary>
            public BranchPath(string name, int level)
                : this(name, level, null) { }

            /// <summary>Parent (has parent).</summary>
            public BranchPath(string name, int level, BranchPath parent)
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
    }
}
