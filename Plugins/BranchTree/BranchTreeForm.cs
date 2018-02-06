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
        TreeNode _tagTreeNode, _localTreeNode, _remoteTreeNode;
        private readonly IGitUICommands _gitUiCommands;
               

        public BranchTreeForm()
        {
            InitializeComponent();
            Translate();
        }

        public BranchTreeForm(GitUIBaseEventArgs gitUiCommands) : this()
        {
            _gitUiCommands = gitUiCommands.GitUICommands;
            var gitModule = gitUiCommands != null ? gitUiCommands.GitModule : null;
            
            BranchesTreeView.BeginUpdate();
            BranchesTreeView.Nodes.Clear();

            _localTreeNode = BranchesTreeView.Nodes.Add("Local");
            _remoteTreeNode = BranchesTreeView.Nodes.Add("Remote");
            _tagTreeNode = BranchesTreeView.Nodes.Add("Tag");

            InitLocalBranches(_localTreeNode, gitModule);
            InitRemoteBranches(_remoteTreeNode, gitModule);
            InitTag(_tagTreeNode, gitModule);
          
            BranchesTreeView.EndUpdate();
            BranchesTreeView.NodeMouseDoubleClick += Branches_NodeMouseDoubleClick;
        }

        private void InitTag(TreeNode root, IGitModule _gitModule)
        {
            var tags = _gitModule.GetRefs(true, false)
                .OrderBy(r => r.Name);

            foreach (var tag in tags)
            {
                AddOrUpdateNode(root.Nodes, tag.Name.Split('/'), tag.CompleteName);
            }
        }

        private void InitRemoteBranches(TreeNode root, IGitModule gitModule)
        {
            var branches = gitModule.GetRefs()
                   .Where(branch => branch.IsRemote && !branch.IsTag)
                   .OrderBy(r => r.Name);                   

            var remotes = gitModule.GetRemotes(allowEmpty: true);
            
            foreach (var branch in branches)
            {
                var remote = branch.Name.Split('/').First();

                if (!remotes.Contains(remote))
                {
                    continue;
                }

                AddOrUpdateNode(root.Nodes, branch.Name.Split('/'), branch.CompleteName);
            }
        }

        private void InitLocalBranches(TreeNode root, IGitModule gitModule)
        {
            var localBranches = gitModule.RunGitCmd("branch")
                .Split('\n')
                .Where(branch => !string.IsNullOrWhiteSpace(branch))// first is ""
                .OrderByDescending(branch => branch.Contains('*'))// * for current branch
                .ThenBy(r => r)
                .Select(line => line.Trim());// trim justify space
            foreach (var localBranch in localBranches)
            {
                AddOrUpdateNode(root.Nodes, localBranch.Split('/'), localBranch);
            }
            
        }
                

        private void Branches_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                MessageBox.Show("Quick switching coming soon!", "Sorry but", MessageBoxButtons.OK);
            }
        }

        private void AddOrUpdateNode(TreeNodeCollection nodes, string[] children, string fullPath)
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
            AddOrUpdateNode(treeNode.Nodes, nextLevelChildren, fullPath);
        }

    }
}
