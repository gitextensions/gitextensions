using System.ComponentModel;
using System.Windows.Forms;
using GitUI.UserControls;

namespace GitUI.BranchTreePanel
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
            if (disposing && (components is not null))
            {
                components.Dispose();
            }

            if (_doubleClickDecorator is not null)
            {
                _doubleClickDecorator.BeforeDoubleClickExpandCollapse -= BeforeDoubleClickExpandCollapse;
                _doubleClickDecorator = null;
            }

            if (_explorerNavigationDecorator is not null)
            {
                _explorerNavigationDecorator.AfterSelect -= OnNodeSelected;
                _explorerNavigationDecorator = null;
            }

            if (disposing)
            {
                _branchesTree?.Dispose();
                _remotesTree?.Dispose();
                _tagTree?.Dispose();
                _submoduleTree?.Dispose();
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
            this.treeMain = new GitUI.UserControls.NativeTreeView();
            this.menuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiMainMenuSpacer1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiMainMenuSpacer2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnCollapse = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnExpand = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBranch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnFilterLocalBranchInRevisionGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.runScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFilterRemoteBranchInRevisionGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTags = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuRemotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuBtnManageRemotesFromRootNode = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnFetchAllRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnPruneAllRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemote = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnRemoteBranchFetchAndCheckout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnPullFromRemoteBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchRebase = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchOneBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnFetchAllBranchesFromARemote = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuBranchPath = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnDeleteAllBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRemoteRepoNode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnManageRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSpacer3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnEnableRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnEnableRemoteAndFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnDisableRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnPruneAllBranchesFromARemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnOpenRemoteUrlInBrowser = new System.Windows.Forms.ToolStripMenuItem();
            this.repoTreePanel = new System.Windows.Forms.TableLayoutPanel();
            this.leftPanelToolStrip = new GitUI.ToolStripEx();
            this.tsbCollapseAll = new System.Windows.Forms.ToolStripButton();
            this.tsbShowBranches = new System.Windows.Forms.ToolStripButton();
            this.tsbShowRemotes = new System.Windows.Forms.ToolStripButton();
            this.tsbShowTags = new System.Windows.Forms.ToolStripButton();
            this.tsbShowSubmodules = new System.Windows.Forms.ToolStripButton();
            this.branchSearchPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnSearch = new System.Windows.Forms.Button();
            this.menuSettings = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuSubmodule = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnManageSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnOpenSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnOpenGESubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnUpdateSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnSynchronizeSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnResetSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnCommitSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAllSubmodules = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuMain.SuspendLayout();
            this.menuBranch.SuspendLayout();
            this.menuRemotes.SuspendLayout();
            this.menuRemote.SuspendLayout();
            this.menuBranchPath.SuspendLayout();
            this.menuRemoteRepoNode.SuspendLayout();
            this.repoTreePanel.SuspendLayout();
            this.leftPanelToolStrip.SuspendLayout();
            this.branchSearchPanel.SuspendLayout();
            this.menuSubmodule.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeMain
            // 
            this.treeMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeMain.ContextMenuStrip = this.menuMain;
            this.treeMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMain.FullRowSelect = true;
            this.treeMain.Location = new System.Drawing.Point(0, 51);
            this.treeMain.Margin = new System.Windows.Forms.Padding(0);
            this.treeMain.Name = "treeMain";
            this.treeMain.PathSeparator = "/";
            this.treeMain.ShowNodeToolTips = true;
            this.treeMain.Size = new System.Drawing.Size(300, 324);
            this.treeMain.TabIndex = 3;
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnCollapse,
            this.mnubtnExpand,
            this.mnubtnMoveUp,
            this.mnubtnMoveDown});
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(139, 92);
            this.menuMain.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnCollapseAll
            // 
            this.mnubtnCollapse.Image = global::GitUI.Properties.Images.CollapseAll;
            this.mnubtnCollapse.Name = "mnubtnCollapse";
            this.mnubtnCollapse.Size = new System.Drawing.Size(138, 22);
            this.mnubtnCollapse.Text = "Collapse";
            this.mnubtnCollapse.ToolTipText = "Collapse all subnodes";
            // 
            // mnubtnExpandAll
            // 
            this.mnubtnExpand.Image = global::GitUI.Properties.Images.ExpandAll;
            this.mnubtnExpand.Name = "mnubtnExpand";
            this.mnubtnExpand.Size = new System.Drawing.Size(138, 22);
            this.mnubtnExpand.Text = "Expand";
            this.mnubtnExpand.ToolTipText = "Expand all subnodes";
            // 
            // mnubtnMoveUp
            // 
            this.mnubtnMoveUp.Image = global::GitUI.Properties.Images.ArrowUp;
            this.mnubtnMoveUp.Name = "mnubtnMoveUp";
            this.mnubtnMoveUp.Size = new System.Drawing.Size(138, 22);
            this.mnubtnMoveUp.Text = "Move Up";
            this.mnubtnMoveUp.ToolTipText = "Move node up";
            // 
            // mnubtnMoveDown
            // 
            this.mnubtnMoveDown.Image = global::GitUI.Properties.Images.ArrowDown;
            this.mnubtnMoveDown.Name = "mnubtnMoveDown";
            this.mnubtnMoveDown.Size = new System.Drawing.Size(138, 22);
            this.mnubtnMoveDown.Text = "Move Down";
            this.mnubtnMoveDown.ToolTipText = "Move node down";
            // 
            // tsmiMainMenuSpacer1
            // 
            this.tsmiMainMenuSpacer1.Name = "tsmiMainMenuSpacer1";
            this.tsmiMainMenuSpacer1.Size = new System.Drawing.Size(133, 6);
            // 
            // tsmiMainMenuSpacer2
            // 
            this.tsmiMainMenuSpacer2.Name = "tsmiMainMenuSpacer2";
            this.tsmiMainMenuSpacer2.Size = new System.Drawing.Size(133, 6);
            // 
            // menuBranch
            // 
            this.menuBranch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnFilterLocalBranchInRevisionGrid,
            this.runScriptToolStripMenuItem});
            this.menuBranch.Name = "contextmenuBranch";
            this.menuBranch.Size = new System.Drawing.Size(192, 48);
            this.menuBranch.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnFilterLocalBranchInRevisionGrid
            // 
            this.mnubtnFilterLocalBranchInRevisionGrid.Image = global::GitUI.Properties.Images.ShowThisBranchOnly;
            this.mnubtnFilterLocalBranchInRevisionGrid.Name = "mnubtnFilterLocalBranchInRevisionGrid";
            this.mnubtnFilterLocalBranchInRevisionGrid.Size = new System.Drawing.Size(191, 22);
            this.mnubtnFilterLocalBranchInRevisionGrid.Text = "Show this branch only";
            // 
            // runScriptToolStripMenuItem
            // 
            this.runScriptToolStripMenuItem.Image = global::GitUI.Properties.Images.Console;
            this.runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            this.runScriptToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.runScriptToolStripMenuItem.Text = "Run script";
            // 
            // mnubtnFilterRemoteBranchInRevisionGrid
            // 
            this.mnubtnFilterRemoteBranchInRevisionGrid.Image = global::GitUI.Properties.Images.ShowThisBranchOnly;
            this.mnubtnFilterRemoteBranchInRevisionGrid.Name = "mnubtnFilterRemoteBranchInRevisionGrid";
            this.mnubtnFilterRemoteBranchInRevisionGrid.Size = new System.Drawing.Size(193, 22);
            this.mnubtnFilterRemoteBranchInRevisionGrid.Text = "Show this branch only";
            // 
            // menuTags
            // 
            this.menuTags.Name = "contextmenuTags";
            this.menuTags.Size = new System.Drawing.Size(61, 4);
            // 
            // menuRemotes
            // 
            this.menuRemotes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuBtnManageRemotesFromRootNode,
            this.mnuBtnFetchAllRemotes,
            this.mnuBtnPruneAllRemotes});
            this.menuRemotes.Name = "contextmenuRemotes";
            this.menuRemotes.Size = new System.Drawing.Size(222, 70);
            this.menuRemotes.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnuBtnManageRemotesFromRootNode
            // 
            this.mnuBtnManageRemotesFromRootNode.Image = global::GitUI.Properties.Images.Remotes;
            this.mnuBtnManageRemotesFromRootNode.Name = "mnuBtnManageRemotesFromRootNode";
            this.mnuBtnManageRemotesFromRootNode.Size = new System.Drawing.Size(221, 22);
            this.mnuBtnManageRemotesFromRootNode.Text = "&Manage...";
            this.mnuBtnManageRemotesFromRootNode.ToolTipText = "Manage remotes";
            // 
            // mnuBtnFetchAllRemotes
            // 
            this.mnuBtnFetchAllRemotes.Image = global::GitUI.Properties.Images.PullFetchAll;
            this.mnuBtnFetchAllRemotes.Name = "mnuBtnFetchAllRemotes";
            this.mnuBtnFetchAllRemotes.Size = new System.Drawing.Size(221, 22);
            this.mnuBtnFetchAllRemotes.Text = "Fetch all remotes";
            // 
            // mnuBtnPruneAllRemotes
            // 
            this.mnuBtnPruneAllRemotes.Image = global::GitUI.Properties.Images.PullFetchPruneAll;
            this.mnuBtnPruneAllRemotes.Name = "mnuBtnPruneAllRemotes";
            this.mnuBtnPruneAllRemotes.Size = new System.Drawing.Size(221, 22);
            this.mnuBtnPruneAllRemotes.Text = "Fetch and prune all remotes";
            // 
            // menuRemote
            // 
            this.menuRemote.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnRemoteBranchFetchAndCheckout,
            this.mnubtnPullFromRemoteBranch,
            this.mnubtnFetchRebase,
            this.mnubtnFetchCreateBranch,
            this.mnubtnFetchOneBranch,
            this.toolStripSeparator1,
            this.mnubtnFilterRemoteBranchInRevisionGrid});
            this.menuRemote.Name = "contextmenuRemote";
            this.menuRemote.Size = new System.Drawing.Size(194, 142);
            this.menuRemote.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnRemoteBranchFetchAndCheckout
            // 
            this.mnubtnRemoteBranchFetchAndCheckout.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.mnubtnRemoteBranchFetchAndCheckout.Name = "mnubtnRemoteBranchFetchAndCheckout";
            this.mnubtnRemoteBranchFetchAndCheckout.Size = new System.Drawing.Size(193, 22);
            this.mnubtnRemoteBranchFetchAndCheckout.Text = "&Fetch && Checkout";
            this.mnubtnRemoteBranchFetchAndCheckout.ToolTipText = "Checkout this branch";
            // 
            // mnubtnPullFromRemoteBranch
            // 
            this.mnubtnPullFromRemoteBranch.Image = global::GitUI.Properties.Images.Pull;
            this.mnubtnPullFromRemoteBranch.Name = "mnubtnPullFromRemoteBranch";
            this.mnubtnPullFromRemoteBranch.Size = new System.Drawing.Size(193, 22);
            this.mnubtnPullFromRemoteBranch.Text = "Fetch && Merge (&Pull)";
            // 
            // mnubtnFetchRebase
            // 
            this.mnubtnFetchRebase.Image = global::GitUI.Properties.Images.Rebase;
            this.mnubtnFetchRebase.Name = "mnubtnFetchRebase";
            this.mnubtnFetchRebase.Size = new System.Drawing.Size(193, 22);
            this.mnubtnFetchRebase.Text = "Fetch && Re&base";
            this.mnubtnFetchRebase.ToolTipText = "Fetch & Rebase current branch to this branch";
            // 
            // mnubtnFetchCreateBranch
            // 
            this.mnubtnFetchCreateBranch.Image = global::GitUI.Properties.Images.Branch;
            this.mnubtnFetchCreateBranch.Name = "mnubtnFetchCreateBranch";
            this.mnubtnFetchCreateBranch.Size = new System.Drawing.Size(193, 22);
            this.mnubtnFetchCreateBranch.Text = "Fetc&h && Create Branch";
            this.mnubtnFetchCreateBranch.ToolTipText = "Fetch then create a local branch from the remote branch";
            // 
            // mnubtnFetchOneBranch
            // 
            this.mnubtnFetchOneBranch.Image = global::GitUI.Properties.Images.Stage;
            this.mnubtnFetchOneBranch.Name = "mnubtnFetchOneBranch";
            this.mnubtnFetchOneBranch.Size = new System.Drawing.Size(193, 22);
            this.mnubtnFetchOneBranch.Text = "Fe&tch";
            this.mnubtnFetchOneBranch.ToolTipText = "Fetch the new remote branch";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(190, 6);
            // 
            // mnubtnFetchAllBranchesFromARemote
            // 
            this.mnubtnFetchAllBranchesFromARemote.Image = global::GitUI.Properties.Images.PullFetch;
            this.mnubtnFetchAllBranchesFromARemote.Name = "mnubtnFetchAllBranchesFromARemote";
            this.mnubtnFetchAllBranchesFromARemote.Size = new System.Drawing.Size(170, 22);
            this.mnubtnFetchAllBranchesFromARemote.Text = "&Fetch";
            // 
            // menuTag
            // 
            this.menuTag.Name = "contextmenuTag";
            this.menuTag.Size = new System.Drawing.Size(61, 4);
            this.menuTag.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // menuBranchPath
            // 
            this.menuBranchPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnCreateBranch,
            this.mnubtnDeleteAllBranches});
            this.menuBranchPath.Name = "contextmenuBranch";
            this.menuBranchPath.Size = new System.Drawing.Size(158, 48);
            this.menuBranchPath.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnCreateBranch
            // 
            this.mnubtnCreateBranch.Image = global::GitUI.Properties.Images.BranchCreate;
            this.mnubtnCreateBranch.Name = "mnubtnCreateBranch";
            this.mnubtnCreateBranch.Size = new System.Drawing.Size(157, 22);
            this.mnubtnCreateBranch.Text = "Create Branch...";
            this.mnubtnCreateBranch.ToolTipText = "Create a local branch";
            // 
            // mnubtnDeleteAllBranches
            // 
            this.mnubtnDeleteAllBranches.Image = global::GitUI.Properties.Images.BranchDelete;
            this.mnubtnDeleteAllBranches.Name = "mnubtnDeleteAllBranches";
            this.mnubtnDeleteAllBranches.Size = new System.Drawing.Size(157, 22);
            this.mnubtnDeleteAllBranches.Text = "Delete All";
            this.mnubtnDeleteAllBranches.ToolTipText = "Delete all child branchs, which must all be fully merged in its upstream branch o" +
    "r in HEAD";
            // 
            // menuRemoteRepoNode
            // 
            this.menuRemoteRepoNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnManageRemotes,
            this.tsmiSpacer3,
            this.mnubtnEnableRemote,
            this.mnubtnEnableRemoteAndFetch,
            this.mnubtnDisableRemote,
            this.mnubtnFetchAllBranchesFromARemote,
            this.mnuBtnPruneAllBranchesFromARemote,
            this.mnuBtnOpenRemoteUrlInBrowser});
            this.menuRemoteRepoNode.Name = "contextmenuRemote";
            this.menuRemoteRepoNode.Size = new System.Drawing.Size(171, 164);
            this.menuRemoteRepoNode.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnManageRemotes
            // 
            this.mnubtnManageRemotes.Image = global::GitUI.Properties.Images.Remotes;
            this.mnubtnManageRemotes.Name = "mnubtnManageRemotes";
            this.mnubtnManageRemotes.Size = new System.Drawing.Size(170, 22);
            this.mnubtnManageRemotes.Text = "&Manage...";
            this.mnubtnManageRemotes.ToolTipText = "Manage remotes";
            // 
            // tsmiSpacer3
            // 
            this.tsmiSpacer3.Name = "tsmiSpacer3";
            this.tsmiSpacer3.Size = new System.Drawing.Size(167, 6);
            // 
            // mnubtnEnableRemote
            // 
            this.mnubtnEnableRemote.Image = global::GitUI.Properties.Images.EyeOpened;
            this.mnubtnEnableRemote.Name = "mnubtnEnableRemote";
            this.mnubtnEnableRemote.Size = new System.Drawing.Size(170, 22);
            this.mnubtnEnableRemote.Text = "&Activate";
            // 
            // mnubtnEnableRemoteAndFetch
            // 
            this.mnubtnEnableRemoteAndFetch.Image = global::GitUI.Properties.Images.RemoteEnableAndFetch;
            this.mnubtnEnableRemoteAndFetch.Name = "mnubtnEnableRemoteAndFetch";
            this.mnubtnEnableRemoteAndFetch.Size = new System.Drawing.Size(170, 22);
            this.mnubtnEnableRemoteAndFetch.Text = "A&ctivate and fetch";
            // 
            // mnubtnDisableRemote
            // 
            this.mnubtnDisableRemote.Image = global::GitUI.Properties.Images.EyeClosed;
            this.mnubtnDisableRemote.Name = "mnubtnDisableRemote";
            this.mnubtnDisableRemote.Size = new System.Drawing.Size(170, 22);
            this.mnubtnDisableRemote.Text = "&Deactivate";
            // 
            // mnuBtnPruneAllBranchesFromARemote
            // 
            this.mnuBtnPruneAllBranchesFromARemote.Image = global::GitUI.Properties.Images.PullFetchPrune;
            this.mnuBtnPruneAllBranchesFromARemote.Name = "mnuBtnPruneAllBranchesFromARemote";
            this.mnuBtnPruneAllBranchesFromARemote.Size = new System.Drawing.Size(170, 22);
            this.mnuBtnPruneAllBranchesFromARemote.Text = "Fetch and prune";
            // 
            // mnuBtnOpenRemoteUrlInBrowser
            // 
            this.mnuBtnOpenRemoteUrlInBrowser.Image = global::GitUI.Properties.Images.Globe;
            this.mnuBtnOpenRemoteUrlInBrowser.Name = "mnuBtnOpenRemoteUrlInBrowser";
            this.mnuBtnOpenRemoteUrlInBrowser.Size = new System.Drawing.Size(170, 22);
            this.mnuBtnOpenRemoteUrlInBrowser.Text = "Open remote Url";
            this.mnuBtnOpenRemoteUrlInBrowser.ToolTipText = "Redirects you to the actual repository page";
            // 
            // repoTreePanel
            // 
            this.repoTreePanel.ColumnCount = 1;
            this.repoTreePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.repoTreePanel.Controls.Add(this.leftPanelToolStrip, 0, 0);
            this.repoTreePanel.Controls.Add(this.branchSearchPanel, 0, 1);
            this.repoTreePanel.Controls.Add(this.treeMain, 0, 2);
            this.repoTreePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.repoTreePanel.Location = new System.Drawing.Point(0, 0);
            this.repoTreePanel.Margin = new System.Windows.Forms.Padding(0);
            this.repoTreePanel.Name = "repoTreePanel";
            this.repoTreePanel.RowCount = 3;
            this.repoTreePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.repoTreePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.repoTreePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.repoTreePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.repoTreePanel.Size = new System.Drawing.Size(300, 350);
            this.repoTreePanel.TabIndex = 4;
            // 
            // leftPanelToolStrip
            // 
            this.leftPanelToolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.leftPanelToolStrip.ClickThrough = true;
            this.leftPanelToolStrip.DrawBorder = false;
            this.leftPanelToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.leftPanelToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbCollapseAll,
            this.tsbShowBranches,
            this.tsbShowRemotes,
            this.tsbShowTags,
            this.tsbShowSubmodules});
            this.leftPanelToolStrip.Location = new System.Drawing.Point(0, 0);
            this.leftPanelToolStrip.Name = "leftPanelToolStrip";
            this.leftPanelToolStrip.Size = new System.Drawing.Size(300, 25);
            this.leftPanelToolStrip.TabIndex = 5;
            // 
            // tsbCollapseAll
            // 
            this.tsbCollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCollapseAll.Image = global::GitUI.Properties.Images.CollapseAll;
            this.tsbCollapseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCollapseAll.Name = "tsbCollapseAll";
            this.tsbCollapseAll.Size = new System.Drawing.Size(23, 22);
            this.tsbCollapseAll.Click += new System.EventHandler(this.btnCollapseAll_Click);
            // 
            // tsbShowBranches
            // 
            this.tsbShowBranches.CheckOnClick = true;
            this.tsbShowBranches.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShowBranches.Image = global::GitUI.Properties.Images.BranchLocalRoot;
            this.tsbShowBranches.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShowBranches.Name = "tsbShowBranches";
            this.tsbShowBranches.Size = new System.Drawing.Size(23, 22);
            this.tsbShowBranches.Text = "&Branches";
            this.tsbShowBranches.Click += new System.EventHandler(this.tsbShowBranches_Click);
            // 
            // tsbShowRemotes
            // 
            this.tsbShowRemotes.CheckOnClick = true;
            this.tsbShowRemotes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShowRemotes.Image = global::GitUI.Properties.Images.BranchRemoteRoot;
            this.tsbShowRemotes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShowRemotes.Name = "tsbShowRemotes";
            this.tsbShowRemotes.Size = new System.Drawing.Size(23, 22);
            this.tsbShowRemotes.Text = "&Remotes";
            this.tsbShowRemotes.Click += new System.EventHandler(this.tsbShowRemotes_Click);
            // 
            // tsbShowTags
            // 
            this.tsbShowTags.CheckOnClick = true;
            this.tsbShowTags.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShowTags.Image = global::GitUI.Properties.Images.TagHorizontal;
            this.tsbShowTags.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShowTags.Name = "tsbShowTags";
            this.tsbShowTags.Size = new System.Drawing.Size(23, 22);
            this.tsbShowTags.Text = "&Tags";
            this.tsbShowTags.Click += new System.EventHandler(this.tsbShowTags_Click);
            // 
            // tsbShowSubmodules
            // 
            this.tsbShowSubmodules.CheckOnClick = true;
            this.tsbShowSubmodules.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShowSubmodules.Image = global::GitUI.Properties.Images.FolderSubmodule;
            this.tsbShowSubmodules.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShowSubmodules.Name = "tsbShowSubmodules";
            this.tsbShowSubmodules.Size = new System.Drawing.Size(23, 22);
            this.tsbShowSubmodules.Text = "&Submodules";
            this.tsbShowSubmodules.Click += new System.EventHandler(this.tsbShowSubmodules_Click);
            // 
            // branchSearchPanel
            // 
            this.branchSearchPanel.AutoSize = true;
            this.branchSearchPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.branchSearchPanel.BackColor = System.Drawing.SystemColors.Control;
            this.branchSearchPanel.ColumnCount = 5;
            this.branchSearchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.branchSearchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.branchSearchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.branchSearchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.branchSearchPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.branchSearchPanel.Controls.Add(this.btnSearch, 2, 0);
            this.branchSearchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchSearchPanel.Location = new System.Drawing.Point(0, 25);
            this.branchSearchPanel.Margin = new System.Windows.Forms.Padding(0);
            this.branchSearchPanel.Name = "branchSearchPanel";
            this.branchSearchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.branchSearchPanel.Size = new System.Drawing.Size(300, 26);
            this.branchSearchPanel.TabIndex = 4;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSearch.AutoSize = true;
            this.btnSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Image = global::GitUI.Properties.Images.Preview;
            this.btnSearch.Location = new System.Drawing.Point(274, 0);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(0);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Padding = new System.Windows.Forms.Padding(2);
            this.btnSearch.Size = new System.Drawing.Size(26, 26);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.OnBtnSearchClicked);
            // 
            // menuSettings
            // 
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(61, 4);
            // 
            // menuSubmodule
            // 
            this.menuSubmodule.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnManageSubmodules,
            this.mnubtnOpenSubmodule,
            this.mnubtnOpenGESubmodule,
            this.mnubtnUpdateSubmodule,
            this.mnubtnSynchronizeSubmodules,
            this.mnubtnResetSubmodule,
            this.mnubtnStashSubmodule,
            this.mnubtnCommitSubmodule});
            this.menuSubmodule.Name = "contextmenuSubmodule";
            this.menuSubmodule.Size = new System.Drawing.Size(139, 92);
            this.menuSubmodule.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnManageSubmodules
            // 
            this.mnubtnManageSubmodules.Image = global::GitUI.Properties.Images.SubmodulesManage;
            this.mnubtnManageSubmodules.Name = "mnubtnManageSubmodules";
            this.mnubtnManageSubmodules.Size = new System.Drawing.Size(138, 22);
            this.mnubtnManageSubmodules.Text = "&Manage...";
            this.mnubtnManageSubmodules.ToolTipText = "Manage submodules";
            // 
            // mnubtnOpenSubmodule
            // 
            this.mnubtnOpenSubmodule.Image = global::GitUI.Properties.Images.FolderOpen;
            this.mnubtnOpenSubmodule.Name = "mnubtnOpenSubmodule";
            this.mnubtnOpenSubmodule.Size = new System.Drawing.Size(138, 22);
            this.mnubtnOpenSubmodule.Text = "&Open";
            this.mnubtnOpenSubmodule.ToolTipText = "Open selected submodule";
            // 
            // mnubtnOpenGESubmodule
            // 
            this.mnubtnOpenGESubmodule.Image = global::GitUI.Properties.Images.GitExtensionsLogo16;
            this.mnubtnOpenGESubmodule.Name = "mnubtnOpenGESubmodule";
            this.mnubtnOpenGESubmodule.Size = new System.Drawing.Size(138, 22);
            this.mnubtnOpenGESubmodule.Text = "O&pen";
            this.mnubtnOpenGESubmodule.ToolTipText = "Open selected submodule in a new instance";
            // 
            // mnubtnUpdateSubmodule
            // 
            this.mnubtnUpdateSubmodule.Image = global::GitUI.Properties.Images.SubmodulesUpdate;
            this.mnubtnUpdateSubmodule.Name = "mnubtnUpdateSubmodule";
            this.mnubtnUpdateSubmodule.Size = new System.Drawing.Size(138, 22);
            this.mnubtnUpdateSubmodule.Text = "&Update";
            this.mnubtnUpdateSubmodule.ToolTipText = "Update selected submodule recursively";
            // 
            // mnubtnSynchronizeSubmodules
            // 
            this.mnubtnSynchronizeSubmodules.Image = global::GitUI.Properties.Images.SubmodulesSync;
            this.mnubtnSynchronizeSubmodules.Name = "mnubtnSynchronizeSubmodules";
            this.mnubtnSynchronizeSubmodules.Size = new System.Drawing.Size(138, 22);
            this.mnubtnSynchronizeSubmodules.Text = "Synchronize";
            this.mnubtnSynchronizeSubmodules.ToolTipText = "Synchronize selected submodule recursively";
            // 
            // mnubtnResetSubmodule
            // 
            this.mnubtnResetSubmodule.Image = global::GitUI.Properties.Images.ResetWorkingDirChanges;
            this.mnubtnResetSubmodule.Name = "mnubtnResetSubmodule";
            this.mnubtnResetSubmodule.Size = new System.Drawing.Size(138, 22);
            this.mnubtnResetSubmodule.Text = "&Reset";
            this.mnubtnResetSubmodule.ToolTipText = "Reset selected submodule";
            // 
            // mnubtnStashSubmodule
            // 
            this.mnubtnStashSubmodule.Image = global::GitUI.Properties.Images.Stash;
            this.mnubtnStashSubmodule.Name = "mnubtnStashSubmodule";
            this.mnubtnStashSubmodule.Size = new System.Drawing.Size(138, 22);
            this.mnubtnStashSubmodule.Text = "&Stash";
            this.mnubtnStashSubmodule.ToolTipText = "Stash changes in selected submodule";
            // 
            // mnubtnCommitSubmodule
            // 
            this.mnubtnCommitSubmodule.Image = global::GitUI.Properties.Images.RepoStateDirtySubmodules;
            this.mnubtnCommitSubmodule.Name = "mnubtnCommitSubmodule";
            this.mnubtnCommitSubmodule.Size = new System.Drawing.Size(138, 22);
            this.mnubtnCommitSubmodule.Text = "&Commit";
            this.mnubtnCommitSubmodule.ToolTipText = "Commit changes in selected submodule";
            // 
            // menuAllSubmodules
            // 
            this.menuAllSubmodules.Name = "contextmenuSubmodules";
            this.menuAllSubmodules.Size = new System.Drawing.Size(61, 4);
            this.menuAllSubmodules.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // RepoObjectsTree
            // 
            this.Controls.Add(this.repoTreePanel);
            this.MinimumSize = new System.Drawing.Size(190, 0);
            this.Name = "RepoObjectsTree";
            this.Size = new System.Drawing.Size(300, 350);
            this.menuMain.ResumeLayout(false);
            this.menuBranch.ResumeLayout(false);
            this.menuRemotes.ResumeLayout(false);
            this.menuRemote.ResumeLayout(false);
            this.menuBranchPath.ResumeLayout(false);
            this.menuRemoteRepoNode.ResumeLayout(false);
            this.repoTreePanel.ResumeLayout(false);
            this.repoTreePanel.PerformLayout();
            this.leftPanelToolStrip.ResumeLayout(false);
            this.leftPanelToolStrip.PerformLayout();
            this.branchSearchPanel.ResumeLayout(false);
            this.branchSearchPanel.PerformLayout();
            this.menuSubmodule.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

#nullable disable

        private NativeTreeView treeMain;
        private ContextMenuStrip menuBranch;
        private ToolStripMenuItem mnubtnFilterLocalBranchInRevisionGrid;
        private ToolStripMenuItem mnubtnFilterRemoteBranchInRevisionGrid;
        private ContextMenuStrip menuTags;
        private ContextMenuStrip menuRemotes;
        private ContextMenuStrip menuRemote;
        private ToolStripMenuItem mnubtnFetchAllBranchesFromARemote;
        private ToolStripMenuItem mnubtnPullFromRemoteBranch;
        private ContextMenuStrip menuTag;
        private ContextMenuStrip menuBranchPath;
        private ToolStripMenuItem mnubtnDeleteAllBranches;
        private ToolStripMenuItem mnubtnCreateBranch;
        private ContextMenuStrip menuMain;
        private ToolStripMenuItem mnubtnCollapse;
        private ToolStripMenuItem mnubtnExpand;
        private ContextMenuStrip menuRemoteRepoNode;
        private ToolStripMenuItem mnubtnFetchOneBranch;
        private TableLayoutPanel repoTreePanel;
        private TableLayoutPanel branchSearchPanel;
        private Button btnSearch;
        private ContextMenuStrip menuSettings;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnubtnManageRemotes;
        private ToolStripMenuItem mnuBtnManageRemotesFromRootNode;
        private ToolStripMenuItem mnubtnRemoteBranchFetchAndCheckout;
        private ToolStripMenuItem mnubtnFetchRebase;
        private ToolStripMenuItem mnubtnFetchCreateBranch;
        private ToolStripSeparator tsmiMainMenuSpacer1;
        private ToolStripSeparator tsmiMainMenuSpacer2;
        private ToolStripMenuItem mnubtnEnableRemote;
        private ToolStripMenuItem mnubtnEnableRemoteAndFetch;
        private ToolStripMenuItem mnubtnDisableRemote;
        private ToolStripSeparator tsmiSpacer3;
        private ToolTip toolTip;
        private ToolStripMenuItem mnuBtnPruneAllRemotes;
        private ToolStripMenuItem mnuBtnPruneAllBranchesFromARemote;
        private ToolStripMenuItem mnuBtnFetchAllRemotes;
        private ContextMenuStrip menuSubmodule;
        private ContextMenuStrip menuAllSubmodules;
        private ToolStripMenuItem mnubtnManageSubmodules;
        private ToolStripMenuItem mnubtnSynchronizeSubmodules;
        private ToolStripMenuItem mnubtnUpdateSubmodule;
        private ToolStripMenuItem mnubtnOpenSubmodule;
        private ToolStripMenuItem mnubtnOpenGESubmodule;
        private ToolStripMenuItem mnubtnResetSubmodule;
        private ToolStripMenuItem mnubtnStashSubmodule;
        private ToolStripMenuItem mnubtnCommitSubmodule;
        private ToolStripMenuItem runScriptToolStripMenuItem;
        private ToolStripMenuItem mnubtnMoveUp;
        private ToolStripMenuItem mnubtnMoveDown;
        private ToolStripMenuItem mnuBtnOpenRemoteUrlInBrowser;
        private ToolStripEx leftPanelToolStrip;
        private ToolStripButton tsbCollapseAll;
        private ToolStripButton tsbShowBranches;
        private ToolStripButton tsbShowRemotes;
        private ToolStripButton tsbShowTags;
        private ToolStripButton tsbShowSubmodules;

#nullable restore
    }
}
