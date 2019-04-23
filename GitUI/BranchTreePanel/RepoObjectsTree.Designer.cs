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
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (_doubleClickDecorator != null)
            {
                _doubleClickDecorator.BeforeDoubleClickExpandCollapse -= BeforeDoubleClickExpandCollapse;
                _doubleClickDecorator = null;
            }

            if (_explorerNavigationDecorator != null)
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
            this.mnubtnCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBranch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnFilterLocalBranchInRevisionGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFilterRemoteBranchInRevisionGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTags = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuRemotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuBtnManageRemotesFromRootNode = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuRemoteRepoNode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnManageRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSpacer3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnEnableRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnEnableRemoteAndFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnDisableRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnPruneAllBranchesFromARemote = new System.Windows.Forms.ToolStripMenuItem();
            this.repoTreePanel = new System.Windows.Forms.TableLayoutPanel();
            this.branchSearchPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblSearchBranch = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnCollapseAll = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.menuSettings = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tsmiShowBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowTags = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnFetchAllRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSubmodule = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnUpdateSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAllSubmodules = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnubtnMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnManageSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnSynchronizeSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMain.SuspendLayout();
            this.menuBranch.SuspendLayout();
            this.menuRemotes.SuspendLayout();
            this.menuRemote.SuspendLayout();
            this.menuTag.SuspendLayout();
            this.menuBranchPath.SuspendLayout();
            this.menuRemoteRepoNode.SuspendLayout();
            this.repoTreePanel.SuspendLayout();
            this.branchSearchPanel.SuspendLayout();
            this.menuSettings.SuspendLayout();
            this.menuSubmodule.SuspendLayout();
            this.menuAllSubmodules.SuspendLayout();
            this.mnubtnOpenSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.SuspendLayout();
            // 
            // treeMain
            // 
            this.treeMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeMain.ContextMenuStrip = this.menuMain;
            this.treeMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMain.FullRowSelect = true;
            this.treeMain.Location = new System.Drawing.Point(0, 26);
            this.treeMain.Margin = new System.Windows.Forms.Padding(0);
            this.treeMain.Name = "treeMain";
            this.treeMain.PathSeparator = "/";
            this.treeMain.ShowNodeToolTips = true;
            this.treeMain.Size = new System.Drawing.Size(300, 350);
            this.treeMain.TabIndex = 3;
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnCollapseAll,
            this.mnubtnExpandAll,
            this.mnubtnMoveUp,
            this.mnubtnMoveDown});
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(137, 76);
            this.menuMain.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
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
            // mnubtnCollapseAll
            // 
            this.mnubtnCollapseAll.Image = global::GitUI.Properties.Images.CollapseAll;
            this.mnubtnCollapseAll.Name = "mnubtnCollapseAll";
            this.mnubtnCollapseAll.Size = new System.Drawing.Size(136, 22);
            this.mnubtnCollapseAll.Text = "Collapse All";
            this.mnubtnCollapseAll.ToolTipText = "Collapse all nodes";
            // 
            // mnubtnExpandAll
            // 
            this.mnubtnExpandAll.Image = global::GitUI.Properties.Images.ExpandAll;
            this.mnubtnExpandAll.Name = "mnubtnExpandAll";
            this.mnubtnExpandAll.Size = new System.Drawing.Size(136, 22);
            this.mnubtnExpandAll.Text = "Expand All";
            this.mnubtnExpandAll.ToolTipText = "Expand all nodes";
            // 
            // menuBranch
            // 
            this.menuBranch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnFilterLocalBranchInRevisionGrid});
            this.menuBranch.Name = "contextmenuBranch";
            this.menuBranch.Size = new System.Drawing.Size(192, 76);
            this.menuBranch.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);

            // 
            // mnubtnFilterLocalBranchInRevisionGrid
            // 
            this.mnubtnFilterLocalBranchInRevisionGrid.Image = global::GitUI.Properties.Images.ShowThisBranchOnly;
            this.mnubtnFilterLocalBranchInRevisionGrid.Name = "mnubtnFilterLocalBranchInRevisionGrid";
            this.mnubtnFilterLocalBranchInRevisionGrid.Size = new System.Drawing.Size(191, 22);
            this.mnubtnFilterLocalBranchInRevisionGrid.Text = "Show this branch only";
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
            this.menuRemotes.Size = new System.Drawing.Size(173, 26);
            this.menuRemotes.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnuBtnManageRemotesFromRootNode
            // 
            this.mnuBtnManageRemotesFromRootNode.Image = global::GitUI.Properties.Images.Remotes;
            this.mnuBtnManageRemotesFromRootNode.Name = "mnuBtnManageRemotesFromRootNode";
            this.mnuBtnManageRemotesFromRootNode.Size = new System.Drawing.Size(172, 22);
            this.mnuBtnManageRemotesFromRootNode.Text = "&Manage...";
            this.mnuBtnManageRemotesFromRootNode.ToolTipText = "Manage remotes";
            // 
            // pruneAllRemotesToolStripMenuItem
            // 
            this.mnuBtnPruneAllRemotes.Image = global::GitUI.Properties.Images.PullFetchPruneAll;
            this.mnuBtnPruneAllRemotes.Name = "mnuBtnPruneAllRemotes";
            this.mnuBtnPruneAllRemotes.Size = new System.Drawing.Size(428, 38);
            this.mnuBtnPruneAllRemotes.Text = "Fetch and prune all remotes";
            // 
            // menuRemote
            // 
            this.menuRemote.Items.AddRange(new ToolStripItem[] {
            this.mnubtnRemoteBranchFetchAndCheckout,
            this.mnubtnPullFromRemoteBranch,
            this.mnubtnFetchRebase,
            this.mnubtnFetchCreateBranch,
            this.mnubtnFetchOneBranch,
            this.toolStripSeparator1,
            this.mnubtnFilterRemoteBranchInRevisionGrid });
            this.menuRemote.Name = "contextmenuRemote";
            this.menuRemote.Size = new System.Drawing.Size(194, 280);
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
            this.mnubtnFetchAllBranchesFromARemote.Size = new System.Drawing.Size(172, 22);
            this.mnubtnFetchAllBranchesFromARemote.Text = "&Fetch";
            // 
            // menuTag
            // 
            this.menuTag.Name = "contextmenuTag";
            this.menuTag.Size = new System.Drawing.Size(158, 76);
            this.menuTag.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // menuBranchPath
            // 
            this.menuBranchPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnDeleteAllBranches});
            this.menuBranchPath.Name = "contextmenuBranch";
            this.menuBranchPath.Size = new System.Drawing.Size(125, 26);
            this.menuBranchPath.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnDeleteAllBranches
            // 
            this.mnubtnDeleteAllBranches.Image = global::GitUI.Properties.Images.BranchDelete;
            this.mnubtnDeleteAllBranches.Name = "mnubtnDeleteAllBranches";
            this.mnubtnDeleteAllBranches.Size = new System.Drawing.Size(124, 22);
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
            this.mnuBtnPruneAllBranchesFromARemote});
            this.menuRemoteRepoNode.Name = "contextmenuRemote";
            this.menuRemoteRepoNode.Size = new System.Drawing.Size(173, 120);
            this.menuRemoteRepoNode.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnManageRemotes
            // 
            this.mnubtnManageRemotes.Image = global::GitUI.Properties.Images.Remotes;
            this.mnubtnManageRemotes.Name = "mnubtnManageRemotes";
            this.mnubtnManageRemotes.Size = new System.Drawing.Size(172, 22);
            this.mnubtnManageRemotes.Text = "&Manage...";
            this.mnubtnManageRemotes.ToolTipText = "Manage remotes";
            // 
            // tsmiSpacer3
            // 
            this.tsmiSpacer3.Name = "tsmiSpacer3";
            this.tsmiSpacer3.Size = new System.Drawing.Size(169, 6);
            // 
            // mnubtnEnableRemote
            // 
            this.mnubtnEnableRemote.Image = global::GitUI.Properties.Images.EyeOpened;
            this.mnubtnEnableRemote.Name = "mnubtnEnableRemote";
            this.mnubtnEnableRemote.Size = new System.Drawing.Size(172, 22);
            this.mnubtnEnableRemote.Text = "&Activate";
            // 
            // mnubtnEnableRemoteAndFetch
            // 
            this.mnubtnEnableRemoteAndFetch.Image = global::GitUI.Properties.Images.RemoteEnableAndFetch;
            this.mnubtnEnableRemoteAndFetch.Name = "mnubtnEnableRemoteAndFetch";
            this.mnubtnEnableRemoteAndFetch.Size = new System.Drawing.Size(172, 22);
            this.mnubtnEnableRemoteAndFetch.Text = "A&ctivate and fetch";
            // 
            // mnubtnDisableRemote
            // 
            this.mnubtnDisableRemote.Image = global::GitUI.Properties.Images.EyeClosed;
            this.mnubtnDisableRemote.Name = "mnubtnDisableRemote";
            this.mnubtnDisableRemote.Size = new System.Drawing.Size(172, 22);
            this.mnubtnDisableRemote.Text = "&Deactivate";
            // 
            // mnuBtnPruneAllBranchesFromARemote
            // 
            this.mnuBtnPruneAllBranchesFromARemote.Image = global::GitUI.Properties.Images.PullFetchPrune;
            this.mnuBtnPruneAllBranchesFromARemote.Name = "mnuBtnPruneAllBranchesFromARemote";
            this.mnuBtnPruneAllBranchesFromARemote.Size = new System.Drawing.Size(302, 38);
            this.mnuBtnPruneAllBranchesFromARemote.Text = "Fetch and prune";
            // 
            // repoTreePanel
            // 
            this.repoTreePanel.ColumnCount = 1;
            this.repoTreePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.repoTreePanel.Controls.Add(this.treeMain, 0, 1);
            this.repoTreePanel.Controls.Add(this.branchSearchPanel, 0, 0);
            this.repoTreePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.repoTreePanel.Location = new System.Drawing.Point(0, 0);
            this.repoTreePanel.Margin = new System.Windows.Forms.Padding(0);
            this.repoTreePanel.Name = "repoTreePanel";
            this.repoTreePanel.RowCount = 2;
            this.repoTreePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.repoTreePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.repoTreePanel.Size = new System.Drawing.Size(300, 350);
            this.repoTreePanel.TabIndex = 4;
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
            this.branchSearchPanel.Controls.Add(this.lblSearchBranch, 0, 0);
            this.branchSearchPanel.Controls.Add(this.btnSearch, 2, 0);
            this.branchSearchPanel.Controls.Add(this.btnCollapseAll, 3, 0);
            this.branchSearchPanel.Controls.Add(this.btnSettings, 4, 0);
            this.branchSearchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchSearchPanel.Location = new System.Drawing.Point(0, 0);
            this.branchSearchPanel.Margin = new System.Windows.Forms.Padding(0);
            this.branchSearchPanel.Name = "branchSearchPanel";
            this.branchSearchPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.branchSearchPanel.Size = new System.Drawing.Size(300, 26);
            this.branchSearchPanel.TabIndex = 4;
            // 
            // lblSearchBranch
            // 
            this.lblSearchBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSearchBranch.AutoSize = true;
            this.lblSearchBranch.Location = new System.Drawing.Point(3, 6);
            this.lblSearchBranch.Name = "lblSearchBranch";
            this.lblSearchBranch.Size = new System.Drawing.Size(44, 13);
            this.lblSearchBranch.TabIndex = 0;
            this.lblSearchBranch.Text = "Search:";
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSearch.AutoSize = true;
            this.btnSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Image = global::GitUI.Properties.Images.Preview;
            this.btnSearch.Location = new System.Drawing.Point(228, 0);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(0);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Padding = new System.Windows.Forms.Padding(2);
            this.btnSearch.Size = new System.Drawing.Size(26, 26);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.OnBtnSearchClicked);
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCollapseAll.AutoSize = true;
            this.btnCollapseAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCollapseAll.FlatAppearance.BorderSize = 0;
            this.btnCollapseAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapseAll.Image = global::GitUI.Properties.Images.CollapseAll;
            this.btnCollapseAll.Location = new System.Drawing.Point(254, 0);
            this.btnCollapseAll.Margin = new System.Windows.Forms.Padding(0);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Padding = new System.Windows.Forms.Padding(2);
            this.btnCollapseAll.Size = new System.Drawing.Size(26, 26);
            this.btnCollapseAll.TabIndex = 3;
            this.btnCollapseAll.UseVisualStyleBackColor = true;
            this.btnCollapseAll.Click += new System.EventHandler(this.btnCollapseAll_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSettings.AutoSize = true;
            this.btnSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSettings.ContextMenuStrip = this.menuSettings;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Image = global::GitUI.Properties.Images.DocumentTree;
            this.btnSettings.Location = new System.Drawing.Point(280, 0);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Padding = new System.Windows.Forms.Padding(2);
            this.btnSettings.Size = new System.Drawing.Size(20, 26);
            this.btnSettings.TabIndex = 4;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.OnBtnSettingsClicked);
            // 
            // menuSettings
            // 
            this.menuSettings.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiShowBranches,
            this.tsmiShowRemotes,
            this.tsmiShowTags,
            this.tsmiShowSubmodules});
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(155, 70);
            // 
            // tsmiShowBranches
            // 
            this.tsmiShowBranches.CheckOnClick = true;
            this.tsmiShowBranches.Image = global::GitUI.Properties.Images.BranchLocalRoot;
            this.tsmiShowBranches.Name = "tsmiShowBranches";
            this.tsmiShowBranches.Size = new System.Drawing.Size(154, 22);
            this.tsmiShowBranches.Text = "&Branches";
            this.tsmiShowBranches.Click += new System.EventHandler(this.tsmiShowBranches_Click);
            // 
            // tsmiShowTags
            // 
            this.tsmiShowTags.CheckOnClick = true;
            this.tsmiShowTags.Image = global::GitUI.Properties.Images.TagHorizontal;
            this.tsmiShowTags.Name = "tsmiShowTags";
            this.tsmiShowTags.Size = new System.Drawing.Size(154, 22);
            this.tsmiShowTags.Text = "&Tags";
            this.tsmiShowTags.Click += new System.EventHandler(this.tsmiShowTags_Click);
            // 
            // tsmiShowRemotes
            // 
            this.tsmiShowRemotes.CheckOnClick = true;
            this.tsmiShowRemotes.Image = global::GitUI.Properties.Images.BranchRemoteRoot;
            this.tsmiShowRemotes.Name = "tsmiShowRemotes";
            this.tsmiShowRemotes.Size = new System.Drawing.Size(154, 22);
            this.tsmiShowRemotes.Text = "&Remotes";
            this.tsmiShowRemotes.Click += new System.EventHandler(this.tsmiShowRemotes_Click);
            // 
            // mnuBtnFetchAllRemotes
            // 
            this.mnuBtnFetchAllRemotes.Image = global::GitUI.Properties.Images.PullFetchAll;
            this.mnuBtnFetchAllRemotes.Name = "mnuBtnFetchAllRemotes";
            this.mnuBtnFetchAllRemotes.Size = new System.Drawing.Size(428, 38);
            this.mnuBtnFetchAllRemotes.Text = "Fetch all remotes";
            // 
            // tsmiShowRemotes
            // 
            this.tsmiShowSubmodules.CheckOnClick = true;
            this.tsmiShowSubmodules.Image = global::GitUI.Properties.Images.FolderSubmodule;
            this.tsmiShowSubmodules.Name = "tsmiShowSubmodules";
            this.tsmiShowSubmodules.Size = new System.Drawing.Size(154, 22);
            this.tsmiShowSubmodules.Text = "&Submodules";
            this.tsmiShowSubmodules.Click += new System.EventHandler(this.tsmiShowSubmodules_Click);
            // 
            // menuSubmodule
            // 
            this.menuSubmodule.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnubtnManageSubmodules,
            this.mnubtnOpenSubmodule,
            this.mnubtnUpdateSubmodule,
            this.mnubtnSynchronizeSubmodules});
            this.menuSubmodule.Name = "contextmenuSubmodule";
            this.menuSubmodule.Size = new System.Drawing.Size(177, 26);
            this.menuSubmodule.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // mnubtnOpenSubmodule
            // 
            this.mnubtnOpenSubmodule.Image = global::GitUI.Properties.Images.FolderOpen;
            this.mnubtnOpenSubmodule.Name = "mnubtnOpenSubmodule";
            this.mnubtnOpenSubmodule.Size = new System.Drawing.Size(342, 38);
            this.mnubtnOpenSubmodule.Text = "&Open";
            this.mnubtnOpenSubmodule.ToolTipText = "Open selected submodule";
            // 
            // mnubtnUpdateSubmodule
            // 
            this.mnubtnUpdateSubmodule.Image = global::GitUI.Properties.Images.SubmodulesUpdate;
            this.mnubtnUpdateSubmodule.Name = "mnubtnUpdateSubmodule";
            this.mnubtnUpdateSubmodule.Size = new System.Drawing.Size(176, 22);
            this.mnubtnUpdateSubmodule.Text = "&Update";
            this.mnubtnUpdateSubmodule.ToolTipText = "Update selected submodule recursively";
            // 
            // menuAllSubmodules
            // 
            this.menuAllSubmodules.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { });
            this.menuAllSubmodules.Name = "contextmenuSubmodules";
            this.menuAllSubmodules.Size = new System.Drawing.Size(196, 26);
            this.menuAllSubmodules.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
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
            // mnubtnManageSubmodules
            // 
            this.mnubtnManageSubmodules.Image = global::GitUI.Properties.Images.SubmodulesManage;
            this.mnubtnManageSubmodules.Name = "mnubtnManageSubmodules";
            this.mnubtnManageSubmodules.Size = new System.Drawing.Size(195, 22);
            this.mnubtnManageSubmodules.Text = "&Manage...";
            this.mnubtnManageSubmodules.ToolTipText = "Manage submodules";
            // 
            // mnubtnSynchronizeSubmodules
            // 
            this.mnubtnSynchronizeSubmodules.Image = global::GitUI.Properties.Images.SubmodulesSync;
            this.mnubtnSynchronizeSubmodules.Name = "mnubtnSynchronizeSubmodules";
            this.mnubtnSynchronizeSubmodules.Size = new System.Drawing.Size(195, 22);
            this.mnubtnSynchronizeSubmodules.Text = "Synchronize";
            this.mnubtnSynchronizeSubmodules.ToolTipText = "Synchronize selected submodule recursively";
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
            this.menuTag.ResumeLayout(false);
            this.menuBranchPath.ResumeLayout(false);
            this.menuRemoteRepoNode.ResumeLayout(false);
            this.repoTreePanel.ResumeLayout(false);
            this.repoTreePanel.PerformLayout();
            this.branchSearchPanel.ResumeLayout(false);
            this.branchSearchPanel.PerformLayout();
            this.menuSettings.ResumeLayout(false);
            this.menuSubmodule.ResumeLayout(false);
            this.menuAllSubmodules.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

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
        private ContextMenuStrip menuMain;
        private ToolStripMenuItem mnubtnCollapseAll;
        private ToolStripMenuItem mnubtnExpandAll;
        private ContextMenuStrip menuRemoteRepoNode;
        private ToolStripMenuItem mnubtnFetchOneBranch;
        private TableLayoutPanel repoTreePanel;
        private TableLayoutPanel branchSearchPanel;
        private Label lblSearchBranch;
        private Button btnSearch;
        private Button btnCollapseAll;
        private Button btnSettings;
        private ContextMenuStrip menuSettings;
        private ToolStripMenuItem tsmiShowTags;
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
        private ToolStripMenuItem tsmiShowBranches;
        private ToolStripMenuItem tsmiShowRemotes;
        private ToolTip toolTip;
        private ToolStripMenuItem mnuBtnPruneAllRemotes;
        private ToolStripMenuItem mnuBtnPruneAllBranchesFromARemote;
        private ToolStripMenuItem mnuBtnFetchAllRemotes;
        private ToolStripMenuItem tsmiShowSubmodules;
        private ContextMenuStrip menuSubmodule;
        private ContextMenuStrip menuAllSubmodules;
        private ToolStripMenuItem mnubtnManageSubmodules;
        private ToolStripMenuItem mnubtnSynchronizeSubmodules;
        private ToolStripMenuItem mnubtnUpdateSubmodule;
        private ToolStripMenuItem mnubtnOpenSubmodule;
        private ToolStripMenuItem mnubtnMoveUp;
        private ToolStripMenuItem mnubtnMoveDown;
    }
}
