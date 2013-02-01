using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    partial class RepoObjectsTree
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuBranches = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnNewBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolbarMain = new System.Windows.Forms.ToolStrip();
            this.toolbtnExpandAll = new System.Windows.Forms.ToolStripButton();
            this.toolbtnCollapseAll = new System.Windows.Forms.ToolStripButton();
            this.treeMain = new System.Windows.Forms.TreeView();
            this.menuBranch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnBranchCheckout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnBranchCreateFrom = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnBranchDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnBranchDeleteForce = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSubmodules = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuTags = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStashes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnStashSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnClearStashes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnRemotesAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemotesRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemote = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnRemoteRename = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemotePrune = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemoteRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemoteFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolbtnRemotePull = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStash = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnStashShowDiff = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashPop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashApply = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashDrop = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSubmodule = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuBranches.SuspendLayout();
            this.toolbarMain.SuspendLayout();
            this.menuBranch.SuspendLayout();
            this.menuStashes.SuspendLayout();
            this.menuRemotes.SuspendLayout();
            this.menuRemote.SuspendLayout();
            this.menuStash.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuBranches
            // 
            this.menuBranches.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnNewBranch});
            this.menuBranches.Name = "contextmenuBranches";
            this.menuBranches.Size = new System.Drawing.Size(148, 26);
            // 
            // mnubtnNewBranch
            // 
            this.mnubtnNewBranch.Image = global::GitUI.Properties.Resources.IconBranchCreate;
            this.mnubtnNewBranch.Name = "mnubtnNewBranch";
            this.mnubtnNewBranch.Size = new System.Drawing.Size(147, 22);
            this.mnubtnNewBranch.Text = "New Branch...";
            this.mnubtnNewBranch.ToolTipText = "Create a new branch";
            // 
            // toolbarMain
            // 
            this.toolbarMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolbtnExpandAll,
            this.toolbtnCollapseAll});
            this.toolbarMain.Location = new System.Drawing.Point(0, 0);
            this.toolbarMain.Name = "toolbarMain";
            this.toolbarMain.Size = new System.Drawing.Size(200, 25);
            this.toolbarMain.TabIndex = 2;
            this.toolbarMain.Text = "toolStrip1";
            // 
            // toolbtnExpandAll
            // 
            this.toolbtnExpandAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolbtnExpandAll.Image = global::GitUI.Properties.Resources.ExpandAll;
            this.toolbtnExpandAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolbtnExpandAll.Name = "toolbtnExpandAll";
            this.toolbtnExpandAll.Size = new System.Drawing.Size(23, 22);
            this.toolbtnExpandAll.Text = "Expand All";
            this.toolbtnExpandAll.ToolTipText = "Expand All";
            this.toolbtnExpandAll.Click += new System.EventHandler(this.ExpandAll_Click);
            // 
            // toolbtnCollapseAll
            // 
            this.toolbtnCollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolbtnCollapseAll.Image = global::GitUI.Properties.Resources.CollapseAll;
            this.toolbtnCollapseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolbtnCollapseAll.Name = "toolbtnCollapseAll";
            this.toolbtnCollapseAll.Size = new System.Drawing.Size(23, 22);
            this.toolbtnCollapseAll.Text = "Collapse All";
            this.toolbtnCollapseAll.ToolTipText = "Collapse All";
            this.toolbtnCollapseAll.Click += new System.EventHandler(this.CollapseAll_Click);
            // 
            // treeMain
            // 
            this.treeMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMain.Location = new System.Drawing.Point(0, 25);
            this.treeMain.Name = "treeMain";
            this.treeMain.Size = new System.Drawing.Size(200, 325);
            this.treeMain.TabIndex = 3;
            this.treeMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnNodeSelected);
            this.treeMain.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnNodeDoubleClick);
            // 
            // menuBranch
            // 
            this.menuBranch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnBranchCheckout,
            this.mnubtnBranchCreateFrom,
            this.mnubtnBranchDelete,
            this.mnubtnBranchDeleteForce});
            this.menuBranch.Name = "contextmenuBranch";
            this.menuBranch.Size = new System.Drawing.Size(158, 92);
            // 
            // mnubtnBranchCheckout
            // 
            this.mnubtnBranchCheckout.Name = "mnubtnBranchCheckout";
            this.mnubtnBranchCheckout.Size = new System.Drawing.Size(157, 22);
            this.mnubtnBranchCheckout.Text = "Checkout";
            // 
            // mnubtnBranchCreateFrom
            // 
            this.mnubtnBranchCreateFrom.Name = "mnubtnBranchCreateFrom";
            this.mnubtnBranchCreateFrom.Size = new System.Drawing.Size(157, 22);
            this.mnubtnBranchCreateFrom.Text = "Create branch...";
            this.mnubtnBranchCreateFrom.ToolTipText = "Create a new branch from the current branch";
            // 
            // mnubtnBranchDelete
            // 
            this.mnubtnBranchDelete.Name = "mnubtnBranchDelete";
            this.mnubtnBranchDelete.Size = new System.Drawing.Size(157, 22);
            this.mnubtnBranchDelete.Text = "Delete";
            this.mnubtnBranchDelete.ToolTipText = "Delete the branch, which must be fully merged in its upstream branch or in HEAD";
            // 
            // mnubtnBranchDeleteForce
            // 
            this.mnubtnBranchDeleteForce.Name = "mnubtnBranchDeleteForce";
            this.mnubtnBranchDeleteForce.Size = new System.Drawing.Size(157, 22);
            this.mnubtnBranchDeleteForce.Text = "Force Delete";
            this.mnubtnBranchDeleteForce.ToolTipText = "Delete the branch, regardless of its merged status";
            // 
            // menuSubmodules
            // 
            this.menuSubmodules.Name = "contextmenuSubmodules";
            this.menuSubmodules.Size = new System.Drawing.Size(61, 4);
            // 
            // menuTags
            // 
            this.menuTags.Name = "contextmenuTags";
            this.menuTags.Size = new System.Drawing.Size(61, 4);
            // 
            // menuStashes
            // 
            this.menuStashes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnStashSave,
            this.mnubtnClearStashes});
            this.menuStashes.Name = "contextmenuStashes";
            this.menuStashes.Size = new System.Drawing.Size(102, 48);
            // 
            // mnubtnStashSave
            // 
            this.mnubtnStashSave.Image = global::GitUI.Properties.Resources.IconSave;
            this.mnubtnStashSave.Name = "mnubtnStashSave";
            this.mnubtnStashSave.Size = new System.Drawing.Size(101, 22);
            this.mnubtnStashSave.Text = "Save";
            this.mnubtnStashSave.ToolTipText = "Save local changes to a new stash then revert changes";
            // 
            // mnubtnClearStashes
            // 
            this.mnubtnClearStashes.Image = global::GitUI.Properties.Resources.StashesClear;
            this.mnubtnClearStashes.Name = "mnubtnClearStashes";
            this.mnubtnClearStashes.Size = new System.Drawing.Size(101, 22);
            this.mnubtnClearStashes.Text = "Clear";
            this.mnubtnClearStashes.ToolTipText = "Remove all the stashes";
            // 
            // menuRemotes
            // 
            this.menuRemotes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnRemotesAdd,
            this.mnubtnRemotesRemoveAll});
            this.menuRemotes.Name = "contextmenuRemotes";
            this.menuRemotes.Size = new System.Drawing.Size(135, 48);
            // 
            // mnubtnRemotesAdd
            // 
            this.mnubtnRemotesAdd.Name = "mnubtnRemotesAdd";
            this.mnubtnRemotesAdd.Size = new System.Drawing.Size(134, 22);
            this.mnubtnRemotesAdd.Text = "Add...";
            // 
            // mnubtnRemotesRemoveAll
            // 
            this.mnubtnRemotesRemoveAll.Name = "mnubtnRemotesRemoveAll";
            this.mnubtnRemotesRemoveAll.Size = new System.Drawing.Size(134, 22);
            this.mnubtnRemotesRemoveAll.Text = "Remove All";
            // 
            // menuRemote
            // 
            this.menuRemote.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnRemoteRename,
            this.mnubtnRemotePrune,
            this.mnubtnRemoteRemove,
            this.mnubtnRemoteFetch,
            this.toolbtnRemotePull});
            this.menuRemote.Name = "contextmenuRemote";
            this.menuRemote.Size = new System.Drawing.Size(118, 114);
            // 
            // mnubtnRemoteRename
            // 
            this.mnubtnRemoteRename.Name = "mnubtnRemoteRename";
            this.mnubtnRemoteRename.Size = new System.Drawing.Size(117, 22);
            this.mnubtnRemoteRename.Text = "Rename";
            // 
            // mnubtnRemotePrune
            // 
            this.mnubtnRemotePrune.Name = "mnubtnRemotePrune";
            this.mnubtnRemotePrune.Size = new System.Drawing.Size(117, 22);
            this.mnubtnRemotePrune.Text = "Prune";
            // 
            // mnubtnRemoteRemove
            // 
            this.mnubtnRemoteRemove.Name = "mnubtnRemoteRemove";
            this.mnubtnRemoteRemove.Size = new System.Drawing.Size(117, 22);
            this.mnubtnRemoteRemove.Text = "Remove";
            this.mnubtnRemoteRemove.ToolTipText = "Remove the remote and all tracking branches";
            // 
            // mnubtnRemoteFetch
            // 
            this.mnubtnRemoteFetch.Name = "mnubtnRemoteFetch";
            this.mnubtnRemoteFetch.Size = new System.Drawing.Size(117, 22);
            this.mnubtnRemoteFetch.Text = "Fetch";
            // 
            // toolbtnRemotePull
            // 
            this.toolbtnRemotePull.Name = "toolbtnRemotePull";
            this.toolbtnRemotePull.Size = new System.Drawing.Size(117, 22);
            this.toolbtnRemotePull.Text = "Pull";
            // 
            // menuStash
            // 
            this.menuStash.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnStashShowDiff,
            this.mnubtnStashPop,
            this.mnubtnStashApply,
            this.mnubtnStashBranch,
            this.mnubtnStashDrop});
            this.menuStash.Name = "contextmenuStash";
            this.menuStash.Size = new System.Drawing.Size(158, 114);
            // 
            // mnubtnStashShowDiff
            // 
            this.mnubtnStashShowDiff.Name = "mnubtnStashShowDiff";
            this.mnubtnStashShowDiff.Size = new System.Drawing.Size(157, 22);
            this.mnubtnStashShowDiff.Text = "Show Diff";
            this.mnubtnStashShowDiff.ToolTipText = "Show the diff compared its parent";
            // 
            // mnubtnStashPop
            // 
            this.mnubtnStashPop.Name = "mnubtnStashPop";
            this.mnubtnStashPop.Size = new System.Drawing.Size(157, 22);
            this.mnubtnStashPop.Text = "Pop";
            this.mnubtnStashPop.ToolTipText = "Apply to current working tree, then remove";
            // 
            // mnubtnStashApply
            // 
            this.mnubtnStashApply.Name = "mnubtnStashApply";
            this.mnubtnStashApply.Size = new System.Drawing.Size(157, 22);
            this.mnubtnStashApply.Text = "Apply";
            this.mnubtnStashApply.ToolTipText = "Apply to current working tree";
            // 
            // mnubtnStashBranch
            // 
            this.mnubtnStashBranch.Name = "mnubtnStashBranch";
            this.mnubtnStashBranch.Size = new System.Drawing.Size(157, 22);
            this.mnubtnStashBranch.Text = "Create branch...";
            this.mnubtnStashBranch.ToolTipText = "Create and checkout a new branch with the stash applied to its parent";
            // 
            // mnubtnStashDrop
            // 
            this.mnubtnStashDrop.Name = "mnubtnStashDrop";
            this.mnubtnStashDrop.Size = new System.Drawing.Size(157, 22);
            this.mnubtnStashDrop.Text = "Drop";
            this.mnubtnStashDrop.ToolTipText = "Remove the stash";
            // 
            // menuSubmodule
            // 
            this.menuSubmodule.Name = "contextmenuSubmodule";
            this.menuSubmodule.Size = new System.Drawing.Size(61, 4);
            // 
            // menuTag
            // 
            this.menuTag.Name = "contextmenuTag";
            this.menuTag.Size = new System.Drawing.Size(61, 4);
            // 
            // RepoObjectsTree
            // 
            this.Controls.Add(this.treeMain);
            this.Controls.Add(this.toolbarMain);
            this.Name = "RepoObjectsTree";
            this.Size = new System.Drawing.Size(200, 350);
            this.menuBranches.ResumeLayout(false);
            this.toolbarMain.ResumeLayout(false);
            this.toolbarMain.PerformLayout();
            this.menuBranch.ResumeLayout(false);
            this.menuStashes.ResumeLayout(false);
            this.menuRemotes.ResumeLayout(false);
            this.menuRemote.ResumeLayout(false);
            this.menuStash.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolbarMain;
        private ToolStripButton toolbtnExpandAll;
        private ToolStripButton toolbtnCollapseAll;
        private TreeView treeMain;
        private ContextMenuStrip menuBranch;
        private ToolStripMenuItem mnubtnBranchCheckout;
        private ToolStripMenuItem mnubtnBranchCreateFrom;
        private ToolStripMenuItem mnubtnBranchDelete;
        private ToolStripMenuItem mnubtnBranchDeleteForce;
        private ContextMenuStrip menuSubmodules;
        private ContextMenuStrip menuBranches;
        private ToolStripMenuItem mnubtnNewBranch;
        private ContextMenuStrip menuTags;
        private ContextMenuStrip menuStashes;
        private ToolStripMenuItem mnubtnStashSave;
        private ToolStripMenuItem mnubtnClearStashes;
        private ContextMenuStrip menuRemotes;
        private ToolStripMenuItem mnubtnRemotesAdd;
        private ToolStripMenuItem mnubtnRemotesRemoveAll;
        private ContextMenuStrip menuRemote;
        private ToolStripMenuItem mnubtnRemoteRename;
        private ToolStripMenuItem mnubtnRemotePrune;
        private ToolStripMenuItem mnubtnRemoteRemove;
        private ToolStripMenuItem mnubtnRemoteFetch;
        private ToolStripMenuItem toolbtnRemotePull;
        private ContextMenuStrip menuStash;
        private ToolStripMenuItem mnubtnStashShowDiff;
        private ToolStripMenuItem mnubtnStashPop;
        private ToolStripMenuItem mnubtnStashApply;
        private ToolStripMenuItem mnubtnStashBranch;
        private ToolStripMenuItem mnubtnStashDrop;
        private ContextMenuStrip menuSubmodule;
        private ContextMenuStrip menuTag;
    }
}
