using GitUIPluginInterfaces;
using ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BranchTree
{
    public partial class BranchTreeForm : GitExtensionsFormBase
    {
        private GitUIBaseEventArgs gitUiCommands;

        private readonly IGitModule _gitCommands;
        private readonly string[] _remotes;

        public BranchTreeForm()
        {
            InitializeComponent();
            Translate();
        }

        public BranchTreeForm(GitUIBaseEventArgs gitUiCommands) : this()
        {
            this.gitUiCommands = gitUiCommands;
            this._gitCommands = gitUiCommands != null ? gitUiCommands.GitModule : null;
            _remotes = _gitCommands.GetRemotes();

            var br = _gitCommands.RunGitCmd("branch -r").Split('\n');
            Branches.BeginUpdate();
            Branches.Nodes.Clear();

            var a = new Dictionary<string, string[]>();

            foreach (var branch in br)
            {
                if (string.IsNullOrWhiteSpace(branch))
                    continue;
                var path = branch.Split('/');
                a.Add(branch, path);
            }
            var b = a.OrderByDescending(el => el.Value.Length);
            foreach (var item in b)
            {
                addOrUpdateNode(Branches.Nodes, item.Value, item.Key);
            }

            Branches.EndUpdate();
            Branches.NodeMouseDoubleClick += Branches_NodeMouseDoubleClick;
        }

        private void Branches_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var branchTag = e.Node.Tag as string;

            if (string.IsNullOrEmpty(branchTag))
            {
                return;
            }

            foreach (var item in _remotes)
            {
                if (branchTag.StartsWith(item+"/")) {
                    branchTag = branchTag.Remove(0, item.Length + 1);
                }
            }
            
            _gitCommands.RunGitCmd("checkout "+ branchTag);
        }

        private void addOrUpdateNode(TreeNodeCollection nodes, string[] children, string fullPath)
        {
            if (children.Length == 0)
            {
                return;
            }

            TreeNode treeNode = null;

            if (children.Length == 1)
            {
                treeNode = new TreeNode();
                treeNode.Text = children[0];
                treeNode.Name = children[0];
                treeNode.Tag = fullPath;
                nodes.Add(treeNode);
                return;
            }

            var index = nodes.IndexOfKey(children[0]);

            if (index < 0)
            {
                treeNode = new TreeNode();
                treeNode.Text = children[0];
                treeNode.Name = children[0];
                nodes.Add(treeNode);
            }
            else
            {
                treeNode = nodes[index];
            }

            var nextLevelChildren = new string[children.Length - 1];
            Array.ConstrainedCopy(children, 1, nextLevelChildren, 0, children.Length - 1);
            addOrUpdateNode(treeNode.Nodes, nextLevelChildren, fullPath);
        }

    }
}
