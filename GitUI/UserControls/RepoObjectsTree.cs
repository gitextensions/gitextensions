using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : UserControl
    {
        GitModule git;
        GitUICommands uiCommands;

        Lazy<TreeNode> nodeTags;
        Lazy<TreeNode> nodeBranches;

        public RepoObjectsTree()
        {
            InitializeComponent();

            nodeBranches = GetNodeLazy("branches");
            nodeTags = GetNodeLazy("tags");

        }

        Lazy<TreeNode> GetNodeLazy(string node)
        {
            return new Lazy<TreeNode>(() => treeMain.Nodes.Find(node, false)[0]);
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload(GitModule git, GitUICommands uiCommands)
        {
            this.git = git;
            this.uiCommands = uiCommands;

            #region Branches ----------------------
            // todo: async CancellationToken(s)
            // todo: task exception handling
            Task.Factory.
                StartNew(() => BranchNode.GetBranchesTree(git.GetBranchNames())).
                ContinueWith(
                    taskGetBranches => ApplyBranchNodes(taskGetBranches.Result),
                    TaskScheduler.FromCurrentSynchronizationContext()
                );

            #endregion Branches ----------------------
        }

        void ApplyBranchNodes(IEnumerable<BranchNode> branchNodes)
        {
            TreeNodeCollection nodes = nodeBranches.Value.Nodes;
            TreeNode previous = nodeBranches.Value;
            var LevelLastNode = new Dictionary<int, TreeNode>();

            /* 0 a-branch
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
            foreach (var node in branchNodes)
            {
                if (node.Level == 1)
                {// root (e.g. a-branch, develop)
                    previous = Add(nodes, node, LevelLastNode);
                }
                else if (node.Level - previous.Level == 0)
                {// sibling (e.g. feat-next and feat-next2)
                    previous = Add(previous.Parent.Nodes, node, LevelLastNode);
                }
                else if (node.Level - previous.Level == 1)
                {// child (e.g. iss444)
                    previous = Add(previous.Nodes, node, LevelLastNode);
                }
                else
                {// previous is last child (e.g. feat-next2)
                    previous = FindParent(previous, node);
                    previous = Add(previous.Parent.Parent.Nodes, node, LevelLastNode);
                }
            }
        }

        static TreeNode FindParent(TreeNode previous, BranchNode node)
        {
            // 0 develop/
            // 1   features/
            // 2      feat-next
            // 2      feat-next2
            // 1   issues/
            if (node.Level - previous.Level == -1)
            {
                return previous.Parent.Parent;
            }
            return FindParent(previous.Parent, node);
        }

        static TreeNode Add(TreeNodeCollection nodes, BranchNode node, IDictionary<int, TreeNode> levelsLastNode)
        {
            TreeNode treeNode = nodes.Add(node.FullPath, node.Name);
            levelsLastNode[node.Level] = treeNode;
            return treeNode;
        }

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
            internal int Level { get; private set; }

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
            static bool IsInFamilyTree(BranchPath parent, string branch, ref int level, out BranchPath commonAncestor)
            {
                if (IsParentOf(parent, branch))
                {
                    level = 0;
                    commonAncestor = parent;
                    return true;
                }
                commonAncestor = null;
                level -= -1;
                return parent.Parent != null && IsInFamilyTree(parent.Parent, branch, ref level, out commonAncestor);
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
                int level = 0;

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
                        nodes.AddRange(GetBranchNodes(branch, out currentParent));
                    }
                    // (else currentParent NOT null)

                    else if (IsInFamilyTree(currentParent, branch, ref level, out currentParent))
                    {
                        nodes.AddRange(GetBranchNodes(branch, out currentParent));
                    }
                    else
                    {
                        nodes.AddRange(GetBranchNodes(branch, out currentParent));
                    }
                }
                return nodes;
            }

            static List<BranchNode> GetBranchNodes(string branch, out BranchPath currentParent)
            {
                currentParent = null;
                var nodes = new List<BranchNode>();
                var splits = branch.Split(Separator);
                int nParents = splits.Length - 1;
                for (int i = 0; i < nParents; i++)
                {
                    currentParent = new BranchPath(splits[i], i + 1, currentParent);
                    nodes.Add(currentParent);
                }
                string last = splits.Last();
                nodes.Add(new Branch(last, nParents + 1, currentParent));
                return nodes;
            }
        }

        class Branch : BranchNode
        {
            public Branch(string branch, int level, BranchPath parent = null)
                : base(GetName(branch), level, parent) { }

            public override string ToString()
            {
                return Name;
            }
        }

        class BranchPath : BranchNode
        {
            public List<BranchPath> Children { get; private set; }

            /// <summary>Root parent (has no parent).</summary>
            public BranchPath(string name, int level)
                : this(name, level, null) { }

            /// <summary>Parent (has parent).</summary>
            public BranchPath(string branch, int level, BranchPath parent)
                : base(branch, level, parent) { }

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

        void ApplyTreeRootsStyle()
        {
            treeMain.BeginUpdate();
            foreach (TreeNode node in treeMain.Nodes)
            {
                node.NodeFont = Settings.Font;
            }
            treeMain.EndUpdate();
        }

        static TreeNode Create(string text)
        {
            return new TreeNode(text)
            {
                NodeFont = Settings.Font
            };
        }

        void treeMainExpandAll_Click(object sender, EventArgs e)
        {
            treeMain.ExpandAll();
        }

        void treeMainCollapseAll_Click(object sender, EventArgs e)
        {
            treeMain.CollapseAll();
        }
    }
}
