using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Text.RegularExpressions;
using PatchApply;

namespace GitUI
{
    public partial class FormBrowse : Form
    {
        public FormBrowse()
        {
            InitializeComponent();
            RevisionGrid.SelectionChanged += new EventHandler(RevisionGrid_SelectionChanged);
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IGitItem item = (IGitItem)e.Node.Tag;
            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    EditorOptions.SetSyntax(FileText, ((GitItem)item).FileName);
                    FileText.Text = GitCommands.GitCommands.GetFileText(item.Guid);
                    FileText.Refresh();
                }
        }

        private void Browse_Load(object sender, EventArgs e)
        {
            if (!GitCommands.Settings.ValidWorkingDir())
            {
                openToolStripMenuItem_Click(sender, e);
            }

            InternalInitialize(false);
            RevisionGrid.Focus();
        }

        private ToolStripItem warning;

        protected void Initialize()
        {
            try
            {
                InternalInitialize(true);
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InternalInitialize(bool hard)
        {
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            CurrentBranch.Text = selectedHead;

            if (hard)
                ShowRevisions();

            Workingdir.Text = GitCommands.Settings.WorkingDir;

            if (GitCommands.Settings.ValidWorkingDir() && GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (warning == null)
                {
                    warning = ToolStrip.Items.Add("There are unresolved merge conflicts!");
                    warning.BackColor = Color.Salmon;
                    warning.Click += new EventHandler(warning_Click);
                }
            }
            else
            {
                if (warning != null)
                {
                    warning.Click -= new EventHandler(warning_Click);
                    ToolStrip.Items.Remove(warning);
                    warning = null;
                }
            }
        }


        private void ShowRevisions()
        {
            RevisionGrid.RefreshRevisions();
            GitTree.Nodes.Clear();
            if (RevisionGrid.GetRevisions().Count > 0)
                LoadInTreeSingle(RevisionGrid.GetRevisions()[0], GitTree.Nodes);
        }

        protected void LoadInTreeSingle(IGitItem item, TreeNodeCollection node)
        {
            List<IGitItem> list = new List<IGitItem>();
            list.Add(item);
            LoadInTree(list, node);
            if (node.Count > 0)
                node[0].Expand();
        }

        protected void LoadInTree(List<IGitItem> items, TreeNodeCollection node)
        {
            foreach (IGitItem item in items)
            {
                TreeNode subNode = node.Add(item.Name);
                subNode.Tag = item;

                if (item is GitItem)
                {
                    if (((GitItem)item).ItemType == "tree")
                        subNode.Nodes.Add(new TreeNode());
                }
                else
                {
                    subNode.Nodes.Add(new TreeNode());
                }
                //LoadInTree(item.SubItems, subNode.Nodes);
            }
        }

        private void GitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            IGitItem item = (IGitItem)e.Node.Tag;

            e.Node.Nodes.Clear();
            //item.SubItems = GitCommands.GitCommands.GetTree(item.Guid);
            LoadInTree(item.SubItems, e.Node.Nodes);

        }

        private void RevisionGrid_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (RevisionGrid.GetRevisions().Count == 0) 
                    return;
                IGitItem revision = RevisionGrid.GetRevisions()[0];

                //List<GitItem> items = GitCommands.GitCommands.GetTree(revision.TreeGuid);
                GitTree.Nodes.Clear();
                LoadInTreeSingle(revision, GitTree.Nodes);
            }
            catch
            { 
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open open = new Open();
            open.ShowDialog();
            Initialize();

        }

        private void checkoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCheckout form = new FormCheckout();
            form.ShowDialog();
            Initialize();
        }

        private void FileText_TextChanged(object sender, EventArgs e)
        {
        }

        private void FileChanges_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void GitTree_DoubleClick(object sender, EventArgs e)
        {
            if (GitTree.SelectedNode == null || !(GitTree.SelectedNode.Tag is IGitItem)) return;

            IGitItem item = (IGitItem)GitTree.SelectedNode.Tag;
            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    FormFileHistory form = new FormFileHistory(((GitItem)item).FileName);
                    form.ShowDialog();

                }

        }

        private void viewDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormDiff diff = new FormDiff();
            diff.ShowDialog();
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAddFiles addFiles = new FormAddFiles();
            addFiles.ShowDialog();
        }

        private void branchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBranch form = new FormBranch();
            form.ShowDialog();
            Initialize();
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormClone form = new FormClone();
            form.ShowDialog();
            Initialize();
        }

        private void commitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCommit form = new FormCommit();
            form.ShowDialog();
            if (form.NeedRefresh)
                Initialize();
        }

        private void initNewRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormInit().ShowDialog();
            Initialize();
        }

        private void pushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GitCommands.Push cmd = new GitCommands.Push(new GitCommands.PushDto());
            //cmd.Execute();
            //MessageBox.Show(cmd.Dto.Result);
            //Initialize();
            new FormPush().ShowDialog();
            Initialize();
        }

        private void pullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GitCommands.Pull cmd = new GitCommands.Pull(new GitCommands.PullDto());
            //cmd.Execute();
            //MessageBox.Show(cmd.Dto.Result);
            //Initialize();

            new FormPull().ShowDialog();
            Initialize();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.ShowDialog();
        }

        private void patchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewPatch applyPatch = new ViewPatch();
            applyPatch.ShowDialog();
            Initialize();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettigns form = new FormSettigns();
            form.ShowDialog();
        }

        private void applyPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MergePatch form = new MergePatch();
            form.ShowDialog();
            Initialize();
        }

        private void gitBashToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunBash();

        }

        private void gitGUIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGui();
        }

        private void formatPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormFormatPath().ShowDialog();
        }

        private void gitcommandLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new GitLogForm().ShowDialog();
        }

        private void RevisionGrid_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void RevisionGrid_DoubleClick(object sender, EventArgs e)
        {
            FormDiff form = new FormDiff();
            form.ShowDialog();
        }

        private void checkoutBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormCheckoutBranck().ShowDialog();
            Initialize();

        }

        private void stashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormStash().ShowDialog();
            Initialize();
        }

        private void runMergetoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
            Initialize();
        }

        void warning_Click(object sender, EventArgs e)
        {
            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are unresolved mergeconflicts, run mergetool now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
                    if (MessageBox.Show("When all mergeconflicts are resolved, you can commit.\nDo you want to commit now?", "Commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        FormCommit frm = new FormCommit();
                        frm.ShowDialog();
                    }
                    Initialize();
                }
            }
            
        }


        private void WarningText_Click(object sender, EventArgs e)
        {

        }

        private void Workingdir_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void CurrentBranch_Click(object sender, EventArgs e)
        {
            checkoutBranchToolStripMenuItem_Click(sender, e);
        }

        private void deleteBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDeleteBranch().ShowDialog();
            Initialize();
        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cherryPickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormCherryPick().ShowDialog();
            Initialize();
        }

        private void mergeBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormMergeBranch().ShowDialog();
            Initialize();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            commitToolStripMenuItem_Click(sender, e);
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void Workingdir_Click_1(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void CurrentBranch_Click_1(object sender, EventArgs e)
        {
            checkoutBranchToolStripMenuItem_Click(sender, e);
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            addFilesToolStripMenuItem_Click(sender, e);
        }

        private void CreateBranch_Click(object sender, EventArgs e)
        {
            branchToolStripMenuItem_Click(sender, e);
        }

        private void GitBash_Click(object sender, EventArgs e)
        {
            gitBashToolStripMenuItem_Click_1(sender, e);
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            settingsToolStripMenuItem_Click(sender, e);
        }

        private void tagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormTag().ShowDialog();
            Initialize();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            refreshToolStripMenuItem_Click(sender, e);
        }

        private void commitcountPerUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormCommitCount().ShowDialog();
        }

        private void kGitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGitK();
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDonate().ShowDialog();
        }

        private void deleteTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDeleteTag().ShowDialog();
            Initialize();
        }

        private void editgitignoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormGitIgnore().ShowDialog();
        }

        private void FormBrowse_FormClosing(object sender, FormClosingEventArgs e)
        {
            

        }


    }
}
