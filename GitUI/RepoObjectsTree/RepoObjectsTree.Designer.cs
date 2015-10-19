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
            this.treeMain = new System.Windows.Forms.TreeView();
            this.menuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnReload = new System.Windows.Forms.ToolStripMenuItem();
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.menuBranch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnBranchDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnBranchDeleteForce = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnBranchCheckout = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSubmodules = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuTags = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStashes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnStashSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnClearStashes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnRemotesAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemotesRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemote = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnNewCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnNewFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolbtnRemotePull = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemoteRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemoteFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemotePrune = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnRemoteRename = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStash = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnStashPop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashApply = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashDrop = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSubmodule = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuBranchPath = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnDeleteAllBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnDeleteAllBranchesForce = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemoteBranchTracked = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnTrackedPull = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnTrackedFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnTrackedCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnTrackedUnTrack = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnTrackedDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemoteBranchStale = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnStaleRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemoteBranchNew = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuRemoteBranchUnTracked = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnUntrackedTrack = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnUntrackedFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemoteRepoNode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuBtnCheckoutLocal = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMain.SuspendLayout();
            this.menuBranch.SuspendLayout();
            this.menuStashes.SuspendLayout();
            this.menuRemotes.SuspendLayout();
            this.menuRemote.SuspendLayout();
            this.menuStash.SuspendLayout();
            this.menuBranchPath.SuspendLayout();
            this.menuRemoteBranchTracked.SuspendLayout();
            this.menuRemoteBranchStale.SuspendLayout();
            this.menuRemoteBranchUnTracked.SuspendLayout();
            this.menuRemoteRepoNode.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeMain
            // 
            this.treeMain.ContextMenuStrip = this.menuMain;
            this.treeMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMain.FullRowSelect = true;
            this.treeMain.Location = new System.Drawing.Point(0, 0);
            this.treeMain.Name = "treeMain";
            this.treeMain.ShowNodeToolTips = true;
            this.treeMain.Size = new System.Drawing.Size(200, 350);
            this.treeMain.TabIndex = 3;
            this.treeMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnNodeSelected);
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnCollapseAll,
            this.mnubtnExpandAll,
            this.mnubtnReload});
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(137, 70);
            // 
            // mnubtnCollapseAll
            // 
            this.mnubtnCollapseAll.Image = global::GitUI.Properties.Resources.CollapseAll;
            this.mnubtnCollapseAll.Name = "mnubtnCollapseAll";
            this.mnubtnCollapseAll.Size = new System.Drawing.Size(136, 22);
            this.mnubtnCollapseAll.Text = "Collapse All";
            this.mnubtnCollapseAll.ToolTipText = "Collapse all nodes";
            // 
            // mnubtnExpandAll
            // 
            this.mnubtnExpandAll.Image = global::GitUI.Properties.Resources.ExpandAll;
            this.mnubtnExpandAll.Name = "mnubtnExpandAll";
            this.mnubtnExpandAll.Size = new System.Drawing.Size(136, 22);
            this.mnubtnExpandAll.Text = "Expand All";
            this.mnubtnExpandAll.ToolTipText = "Expand all nodes";
            // 
            // mnubtnReload
            // 
            this.mnubtnReload.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.mnubtnReload.Name = "mnubtnReload";
            this.mnubtnReload.Size = new System.Drawing.Size(136, 22);
            this.mnubtnReload.Text = "Reload";
            this.mnubtnReload.ToolTipText = "Reload the tree";
            // 
            // imgList
            // 
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgList.ImageSize = new System.Drawing.Size(16, 16);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // menuBranch
            // 
            this.menuBranch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuBtnCheckoutLocal,
            this.mnubtnBranchDelete,
            this.mnubtnBranchDeleteForce});
            this.menuBranch.Name = "contextmenuBranch";
            this.menuBranch.Size = new System.Drawing.Size(153, 92);
            // 
            // mnubtnBranchDelete
            // 
            this.mnubtnBranchDelete.Image = global::GitUI.Properties.Resources.IconBranchDelete;
            this.mnubtnBranchDelete.Name = "mnubtnBranchDelete";
            this.mnubtnBranchDelete.Size = new System.Drawing.Size(152, 22);
            this.mnubtnBranchDelete.Text = "Delete";
            this.mnubtnBranchDelete.ToolTipText = "Delete the branch, which must be fully merged in its upstream branch or in HEAD";
            // 
            // mnubtnBranchDeleteForce
            // 
            this.mnubtnBranchDeleteForce.Image = global::GitUI.Properties.Resources.DeleteRed;
            this.mnubtnBranchDeleteForce.Name = "mnubtnBranchDeleteForce";
            this.mnubtnBranchDeleteForce.Size = new System.Drawing.Size(152, 22);
            this.mnubtnBranchDeleteForce.Text = "Force Delete";
            this.mnubtnBranchDeleteForce.ToolTipText = "Delete the branch, regardless of its merged status";
            // 
            // mnubtnBranchCheckout
            // 
            this.mnubtnBranchCheckout.Image = global::GitUI.Properties.Resources.IconBranchCheckout;
            this.mnubtnBranchCheckout.Name = "mnubtnBranchCheckout";
            this.mnubtnBranchCheckout.Size = new System.Drawing.Size(157, 22);
            this.mnubtnBranchCheckout.Text = "Checkout";
            this.mnubtnBranchCheckout.ToolTipText = "Checkout this branch";
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
            this.menuStashes.Size = new System.Drawing.Size(125, 48);
            // 
            // mnubtnStashSave
            // 
            this.mnubtnStashSave.Image = global::GitUI.Properties.Resources.IconSave;
            this.mnubtnStashSave.Name = "mnubtnStashSave";
            this.mnubtnStashSave.Size = new System.Drawing.Size(124, 22);
            this.mnubtnStashSave.Text = "Save";
            this.mnubtnStashSave.ToolTipText = "Save local changes to a new stash then revert changes";
            // 
            // mnubtnClearStashes
            // 
            this.mnubtnClearStashes.Image = global::GitUI.Properties.Resources.StashesClear;
            this.mnubtnClearStashes.Name = "mnubtnClearStashes";
            this.mnubtnClearStashes.Size = new System.Drawing.Size(124, 22);
            this.mnubtnClearStashes.Text = "Delete All";
            this.mnubtnClearStashes.ToolTipText = "Delete all stashes";
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
            this.mnubtnRemotesAdd.Image = global::GitUI.Properties.Resources.AddConnection;
            this.mnubtnRemotesAdd.Name = "mnubtnRemotesAdd";
            this.mnubtnRemotesAdd.Size = new System.Drawing.Size(134, 22);
            this.mnubtnRemotesAdd.Text = "Add...";
            // 
            // mnubtnRemotesRemoveAll
            // 
            this.mnubtnRemotesRemoveAll.Image = global::GitUI.Properties.Resources.StashesClear;
            this.mnubtnRemotesRemoveAll.Name = "mnubtnRemotesRemoveAll";
            this.mnubtnRemotesRemoveAll.Size = new System.Drawing.Size(134, 22);
            this.mnubtnRemotesRemoveAll.Text = "Remove All";
            // 
            // menuRemote
            // 
            this.menuRemote.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnBranchCheckout,
            this.mnubtnNewCreateBranch,
            this.mnubtnNewFetch,
            this.toolbtnRemotePull,
            this.mnubtnRemoteRemove});
            this.menuRemote.Name = "contextmenuRemote";
            this.menuRemote.Size = new System.Drawing.Size(158, 114);
            // 
            // mnubtnNewCreateBranch
            // 
            this.mnubtnNewCreateBranch.Image = global::GitUI.Properties.Resources.BranchFrom;
            this.mnubtnNewCreateBranch.Name = "mnubtnNewCreateBranch";
            this.mnubtnNewCreateBranch.Size = new System.Drawing.Size(157, 22);
            this.mnubtnNewCreateBranch.Text = "Create Branch...";
            this.mnubtnNewCreateBranch.ToolTipText = "Fetch then create a local branch from the remote branch";
            // 
            // mnubtnNewFetch
            // 
            this.mnubtnNewFetch.Image = global::GitUI.Properties.Resources.IconStage;
            this.mnubtnNewFetch.Name = "mnubtnNewFetch";
            this.mnubtnNewFetch.Size = new System.Drawing.Size(157, 22);
            this.mnubtnNewFetch.Text = "Fetch";
            this.mnubtnNewFetch.ToolTipText = "Fetch the new remote branch";
            // 
            // toolbtnRemotePull
            // 
            this.toolbtnRemotePull.Image = global::GitUI.Properties.Resources.Icon_4;
            this.toolbtnRemotePull.Name = "toolbtnRemotePull";
            this.toolbtnRemotePull.Size = new System.Drawing.Size(157, 22);
            this.toolbtnRemotePull.Text = "Pull";
            // 
            // mnubtnRemoteRemove
            // 
            this.mnubtnRemoteRemove.Image = global::GitUI.Properties.Resources.DeleteRed;
            this.mnubtnRemoteRemove.Name = "mnubtnRemoteRemove";
            this.mnubtnRemoteRemove.Size = new System.Drawing.Size(157, 22);
            this.mnubtnRemoteRemove.Text = "Delete";
            this.mnubtnRemoteRemove.ToolTipText = "Remove the remote and all tracking branches";
            // 
            // mnubtnRemoteFetch
            // 
            this.mnubtnRemoteFetch.Image = global::GitUI.Properties.Resources.IconStage;
            this.mnubtnRemoteFetch.Name = "mnubtnRemoteFetch";
            this.mnubtnRemoteFetch.Size = new System.Drawing.Size(103, 22);
            this.mnubtnRemoteFetch.Text = "Fetch";
            // 
            // mnubtnRemotePrune
            // 
            this.mnubtnRemotePrune.Image = global::GitUI.Properties.Resources.Cut;
            this.mnubtnRemotePrune.Name = "mnubtnRemotePrune";
            this.mnubtnRemotePrune.Size = new System.Drawing.Size(117, 22);
            this.mnubtnRemotePrune.Text = "Prune";
            // 
            // mnubtnRemoteRename
            // 
            this.mnubtnRemoteRename.Image = global::GitUI.Properties.Resources.Rename;
            this.mnubtnRemoteRename.Name = "mnubtnRemoteRename";
            this.mnubtnRemoteRename.Size = new System.Drawing.Size(117, 22);
            this.mnubtnRemoteRename.Text = "Rename";
            // 
            // menuStash
            // 
            this.menuStash.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnStashPop,
            this.mnubtnStashApply,
            this.mnubtnStashDrop});
            this.menuStash.Name = "contextmenuStash";
            this.menuStash.Size = new System.Drawing.Size(108, 70);
            // 
            // mnubtnStashPop
            // 
            this.mnubtnStashPop.Image = global::GitUI.Properties.Resources.StashPop;
            this.mnubtnStashPop.Name = "mnubtnStashPop";
            this.mnubtnStashPop.Size = new System.Drawing.Size(107, 22);
            this.mnubtnStashPop.Text = "Pop";
            this.mnubtnStashPop.ToolTipText = "Apply to current working tree, then delete";
            // 
            // mnubtnStashApply
            // 
            this.mnubtnStashApply.Image = global::GitUI.Properties.Resources.ApplyChanges;
            this.mnubtnStashApply.Name = "mnubtnStashApply";
            this.mnubtnStashApply.Size = new System.Drawing.Size(107, 22);
            this.mnubtnStashApply.Text = "Apply";
            this.mnubtnStashApply.ToolTipText = "Apply to current working tree";
            // 
            // mnubtnStashDrop
            // 
            this.mnubtnStashDrop.Image = global::GitUI.Properties.Resources.DeleteRed;
            this.mnubtnStashDrop.Name = "mnubtnStashDrop";
            this.mnubtnStashDrop.Size = new System.Drawing.Size(107, 22);
            this.mnubtnStashDrop.Text = "Delete";
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
            // menuBranchPath
            // 
            this.menuBranchPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnDeleteAllBranches,
            this.mnubtnDeleteAllBranchesForce});
            this.menuBranchPath.Name = "contextmenuBranch";
            this.menuBranchPath.Size = new System.Drawing.Size(157, 48);
            // 
            // mnubtnDeleteAllBranches
            // 
            this.mnubtnDeleteAllBranches.Image = global::GitUI.Properties.Resources.IconBranchDelete;
            this.mnubtnDeleteAllBranches.Name = "mnubtnDeleteAllBranches";
            this.mnubtnDeleteAllBranches.Size = new System.Drawing.Size(156, 22);
            this.mnubtnDeleteAllBranches.Text = "Delete All";
            this.mnubtnDeleteAllBranches.ToolTipText = "Delete all child branchs, which must all be fully merged in its upstream branch o" +
    "r in HEAD";
            // 
            // mnubtnDeleteAllBranchesForce
            // 
            this.mnubtnDeleteAllBranchesForce.Image = global::GitUI.Properties.Resources.DeleteRed;
            this.mnubtnDeleteAllBranchesForce.Name = "mnubtnDeleteAllBranchesForce";
            this.mnubtnDeleteAllBranchesForce.Size = new System.Drawing.Size(156, 22);
            this.mnubtnDeleteAllBranchesForce.Text = "Force Delete All";
            this.mnubtnDeleteAllBranchesForce.ToolTipText = "Delete all child branches, regardless of their merged status";
            // 
            // menuRemoteBranchTracked
            // 
            this.menuRemoteBranchTracked.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnTrackedPull,
            this.mnubtnTrackedFetch,
            this.mnubtnTrackedCreateBranch,
            this.mnubtnTrackedUnTrack,
            this.mnubtnTrackedDelete});
            this.menuRemoteBranchTracked.Name = "menuRemoteBranchTracked";
            this.menuRemoteBranchTracked.Size = new System.Drawing.Size(158, 114);
            // 
            // mnubtnTrackedPull
            // 
            this.mnubtnTrackedPull.Image = global::GitUI.Properties.Resources.Icon_4;
            this.mnubtnTrackedPull.Name = "mnubtnTrackedPull";
            this.mnubtnTrackedPull.Size = new System.Drawing.Size(157, 22);
            this.mnubtnTrackedPull.Text = "Pull";
            this.mnubtnTrackedPull.ToolTipText = "Download updates and merge into it\'s local branch";
            // 
            // mnubtnTrackedFetch
            // 
            this.mnubtnTrackedFetch.Image = global::GitUI.Properties.Resources.IconStage;
            this.mnubtnTrackedFetch.Name = "mnubtnTrackedFetch";
            this.mnubtnTrackedFetch.Size = new System.Drawing.Size(157, 22);
            this.mnubtnTrackedFetch.Text = "Fetch";
            this.mnubtnTrackedFetch.ToolTipText = "Download updates from the remote branch";
            // 
            // mnubtnTrackedCreateBranch
            // 
            this.mnubtnTrackedCreateBranch.Image = global::GitUI.Properties.Resources.BranchFrom;
            this.mnubtnTrackedCreateBranch.Name = "mnubtnTrackedCreateBranch";
            this.mnubtnTrackedCreateBranch.Size = new System.Drawing.Size(157, 22);
            this.mnubtnTrackedCreateBranch.Text = "Create Branch...";
            this.mnubtnTrackedCreateBranch.ToolTipText = "Create a local branch from the remote branch";
            // 
            // mnubtnTrackedUnTrack
            // 
            this.mnubtnTrackedUnTrack.Image = global::GitUI.Properties.Resources.DeleteSoft;
            this.mnubtnTrackedUnTrack.Name = "mnubtnTrackedUnTrack";
            this.mnubtnTrackedUnTrack.Size = new System.Drawing.Size(157, 22);
            this.mnubtnTrackedUnTrack.Text = "Un-Track";
            this.mnubtnTrackedUnTrack.ToolTipText = "Un-track the remote branch and remove the local copy";
            // 
            // mnubtnTrackedDelete
            // 
            this.mnubtnTrackedDelete.Image = global::GitUI.Properties.Resources.DeleteRed;
            this.mnubtnTrackedDelete.Name = "mnubtnTrackedDelete";
            this.mnubtnTrackedDelete.Size = new System.Drawing.Size(157, 22);
            this.mnubtnTrackedDelete.Text = "Delete";
            this.mnubtnTrackedDelete.ToolTipText = "Delete the branch on the remote repository";
            // 
            // menuRemoteBranchStale
            // 
            this.menuRemoteBranchStale.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnStaleRemove});
            this.menuRemoteBranchStale.Name = "menuRemoteBranchTracked";
            this.menuRemoteBranchStale.Size = new System.Drawing.Size(118, 26);
            // 
            // mnubtnStaleRemove
            // 
            this.mnubtnStaleRemove.Image = global::GitUI.Properties.Resources.DeleteRed;
            this.mnubtnStaleRemove.Name = "mnubtnStaleRemove";
            this.mnubtnStaleRemove.Size = new System.Drawing.Size(117, 22);
            this.mnubtnStaleRemove.Text = "Remove";
            this.mnubtnStaleRemove.ToolTipText = "Remove the local copy";
            // 
            // menuRemoteBranchNew
            // 
            this.menuRemoteBranchNew.Name = "menuRemoteBranchTracked";
            this.menuRemoteBranchNew.Size = new System.Drawing.Size(61, 4);
            // 
            // menuRemoteBranchUnTracked
            // 
            this.menuRemoteBranchUnTracked.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnUntrackedTrack,
            this.mnubtnUntrackedFetch});
            this.menuRemoteBranchUnTracked.Name = "menuRemoteBranchTracked";
            this.menuRemoteBranchUnTracked.Size = new System.Drawing.Size(149, 48);
            // 
            // mnubtnUntrackedTrack
            // 
            this.mnubtnUntrackedTrack.Image = global::GitUI.Properties.Resources.BranchNew;
            this.mnubtnUntrackedTrack.Name = "mnubtnUntrackedTrack";
            this.mnubtnUntrackedTrack.Size = new System.Drawing.Size(148, 22);
            this.mnubtnUntrackedTrack.Text = "Track";
            this.mnubtnUntrackedTrack.ToolTipText = "Locally track the remote branch";
            // 
            // mnubtnUntrackedFetch
            // 
            this.mnubtnUntrackedFetch.Image = global::GitUI.Properties.Resources.IconStage;
            this.mnubtnUntrackedFetch.Name = "mnubtnUntrackedFetch";
            this.mnubtnUntrackedFetch.Size = new System.Drawing.Size(148, 22);
            this.mnubtnUntrackedFetch.Text = "Fetch && Track";
            this.mnubtnUntrackedFetch.ToolTipText = "Fetch the remote branch and track it locally";
            // 
            // menuRemoteRepoNode
            // 
            this.menuRemoteRepoNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnRemoteFetch});
            this.menuRemoteRepoNode.Name = "contextmenuRemote";
            this.menuRemoteRepoNode.Size = new System.Drawing.Size(104, 26);
            // 
            // mnuBtnCheckoutLocal
            // 
            this.mnuBtnCheckoutLocal.Image = global::GitUI.Properties.Resources.IconBranchCheckout;
            this.mnuBtnCheckoutLocal.Name = "mnuBtnCheckoutLocal";
            this.mnuBtnCheckoutLocal.Size = new System.Drawing.Size(152, 22);
            this.mnuBtnCheckoutLocal.Text = "Checkout";
            this.mnuBtnCheckoutLocal.ToolTipText = "Checkout this branch";
            // 
            // RepoObjectsTree
            // 
            this.Controls.Add(this.treeMain);
            this.Name = "RepoObjectsTree";
            this.Size = new System.Drawing.Size(200, 350);
            this.menuMain.ResumeLayout(false);
            this.menuBranch.ResumeLayout(false);
            this.menuStashes.ResumeLayout(false);
            this.menuRemotes.ResumeLayout(false);
            this.menuRemote.ResumeLayout(false);
            this.menuStash.ResumeLayout(false);
            this.menuBranchPath.ResumeLayout(false);
            this.menuRemoteBranchTracked.ResumeLayout(false);
            this.menuRemoteBranchStale.ResumeLayout(false);
            this.menuRemoteBranchUnTracked.ResumeLayout(false);
            this.menuRemoteRepoNode.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeView treeMain;
        private ContextMenuStrip menuBranch;
        private ToolStripMenuItem mnubtnBranchCheckout;
        private ToolStripMenuItem mnubtnBranchDelete;
        private ToolStripMenuItem mnubtnBranchDeleteForce;
        private ContextMenuStrip menuSubmodules;
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
        private ToolStripMenuItem mnubtnStashPop;
        private ToolStripMenuItem mnubtnStashApply;
        private ToolStripMenuItem mnubtnStashDrop;
        private ContextMenuStrip menuSubmodule;
        private ContextMenuStrip menuTag;
        private ImageList imgList;
        private ContextMenuStrip menuBranchPath;
        private ToolStripMenuItem mnubtnDeleteAllBranches;
        private ToolStripMenuItem mnubtnDeleteAllBranchesForce;
        private ContextMenuStrip menuMain;
        private ToolStripMenuItem mnubtnCollapseAll;
        private ToolStripMenuItem mnubtnExpandAll;
        private ToolStripMenuItem mnubtnReload;
        private ContextMenuStrip menuRemoteBranchTracked;
        private ContextMenuStrip menuRemoteBranchStale;
        private ContextMenuStrip menuRemoteBranchNew;
        private ContextMenuStrip menuRemoteBranchUnTracked;
        private ToolStripMenuItem mnubtnTrackedPull;
        private ToolStripMenuItem mnubtnTrackedFetch;
        private ToolStripMenuItem mnubtnTrackedCreateBranch;
        private ToolStripMenuItem mnubtnStaleRemove;
        private ToolStripMenuItem mnubtnTrackedUnTrack;
        private ToolStripMenuItem mnubtnTrackedDelete;
        private ToolStripMenuItem mnubtnUntrackedTrack;
        private ToolStripMenuItem mnubtnUntrackedFetch;
        private ContextMenuStrip menuRemoteRepoNode;
        private ToolStripMenuItem mnubtnNewCreateBranch;
        private ToolStripMenuItem mnubtnNewFetch;
        private ToolStripMenuItem mnuBtnCheckoutLocal;
    }
}
