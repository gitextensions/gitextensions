using System.ComponentModel;
using System.Windows.Forms;
using GitUI.UserControls;

namespace GitUI.LeftPanel
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
                _stashTree?.Dispose();
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
            components = new System.ComponentModel.Container();
            treeMain = new GitUI.UserControls.NativeTreeView();
            menuMain = new ContextMenuStrip(components);
            copyContextMenuItem = new GitUI.UserControls.RevisionGrid.CopyContextMenuItem();
            filterForSelectedRefsMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            mnuBtnManageRemotesFromRootNode = new ToolStripMenuItem();
            mnuBtnFetchAllRemotes = new ToolStripMenuItem();
            mnuBtnPruneAllRemotes = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            mnubtnManageRemotes = new ToolStripMenuItem();
            mnubtnEnableRemote = new ToolStripMenuItem();
            mnubtnEnableRemoteAndFetch = new ToolStripMenuItem();
            mnubtnDisableRemote = new ToolStripMenuItem();
            mnubtnFetchAllBranchesFromARemote = new ToolStripMenuItem();
            mnuBtnPruneAllBranchesFromARemote = new ToolStripMenuItem();
            mnuBtnOpenRemoteUrlInBrowser = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            mnubtnOpenSubmodule = new ToolStripMenuItem();
            mnubtnOpenGESubmodule = new ToolStripMenuItem();
            mnubtnManageSubmodules = new ToolStripMenuItem();
            mnubtnUpdateSubmodule = new ToolStripMenuItem();
            mnubtnSynchronizeSubmodules = new ToolStripMenuItem();
            mnubtnResetSubmodule = new ToolStripMenuItem();
            mnubtnStashSubmodule = new ToolStripMenuItem();
            mnubtnCommitSubmodule = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            mnubtnRemoteBranchFetchAndCheckout = new ToolStripMenuItem();
            mnubtnPullFromRemoteBranch = new ToolStripMenuItem();
            mnubtnFetchRebase = new ToolStripMenuItem();
            mnubtnFetchCreateBranch = new ToolStripMenuItem();
            mnubtnFetchOneBranch = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnubtnCreateBranch = new ToolStripMenuItem();
            mnubtnDeleteAllBranches = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            mnubtnStashAllFromRootNode = new ToolStripMenuItem();
            mnubtnStashStagedFromRootNode = new ToolStripMenuItem();
            mnubtnManageStashFromRootNode = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            mnubtnOpenStash = new ToolStripMenuItem();
            mnubtnApplyStash = new ToolStripMenuItem();
            mnubtnPopStash = new ToolStripMenuItem();
            mnubtnDropStash = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            mnubtnCollapse = new ToolStripMenuItem();
            mnubtnExpand = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            mnubtnMoveUp = new ToolStripMenuItem();
            mnubtnMoveDown = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            runScriptToolStripMenuItem = new ToolStripMenuItem();
            tsmiMainMenuSpacer1 = new ToolStripSeparator();
            tsmiMainMenuSpacer2 = new ToolStripSeparator();
            repoTreePanel = new TableLayoutPanel();
            leftPanelToolStrip = new GitUI.ToolStripEx();
            tsbCollapseAll = new ToolStripButton();
            tsbShowBranches = new ToolStripButton();
            tsbShowRemotes = new ToolStripButton();
            tsbShowTags = new ToolStripButton();
            tsbShowStashes = new ToolStripButton();
            tsbShowSubmodules = new ToolStripButton();
            branchSearchPanel = new TableLayoutPanel();
            btnSearch = new Button();
            toolTip = new ToolTip(components);
            menuMain.SuspendLayout();
            repoTreePanel.SuspendLayout();
            leftPanelToolStrip.SuspendLayout();
            branchSearchPanel.SuspendLayout();
            SuspendLayout();
            // 
            // treeMain
            // 
            treeMain.BorderStyle = BorderStyle.None;
            treeMain.ContextMenuStrip = menuMain;
            treeMain.Dock = DockStyle.Fill;
            treeMain.FullRowSelect = true;
            treeMain.Location = new Point(0, 53);
            treeMain.Margin = new Padding(0);
            treeMain.Name = "treeMain";
            treeMain.PathSeparator = "/";
            treeMain.ShowNodeToolTips = true;
            treeMain.Size = new Size(300, 324);
            treeMain.TabIndex = 3;
            // 
            // menuMain
            // 
            menuMain.Items.AddRange(new ToolStripItem[] {
            copyContextMenuItem,
            filterForSelectedRefsMenuItem,
            toolStripSeparator2,
            mnuBtnManageRemotesFromRootNode,
            mnuBtnFetchAllRemotes,
            mnuBtnPruneAllRemotes,
            toolStripSeparator3,
            mnubtnManageRemotes,
            mnubtnEnableRemote,
            mnubtnEnableRemoteAndFetch,
            mnubtnDisableRemote,
            mnubtnFetchAllBranchesFromARemote,
            mnuBtnPruneAllBranchesFromARemote,
            mnuBtnOpenRemoteUrlInBrowser,
            toolStripSeparator5,
            mnubtnOpenSubmodule,
            mnubtnOpenGESubmodule,
            mnubtnManageSubmodules,
            mnubtnUpdateSubmodule,
            mnubtnSynchronizeSubmodules,
            mnubtnResetSubmodule,
            mnubtnStashSubmodule,
            mnubtnCommitSubmodule,
            toolStripSeparator8,
            mnubtnRemoteBranchFetchAndCheckout,
            mnubtnPullFromRemoteBranch,
            mnubtnFetchRebase,
            mnubtnFetchCreateBranch,
            mnubtnFetchOneBranch,
            toolStripSeparator6,
            mnubtnCreateBranch,
            mnubtnDeleteAllBranches,
            toolStripSeparator10,
            mnubtnStashAllFromRootNode,
            mnubtnStashStagedFromRootNode,
            mnubtnManageStashFromRootNode,
            toolStripSeparator7,
            mnubtnOpenStash,
            mnubtnApplyStash,
            mnubtnPopStash,
            mnubtnDropStash,
            toolStripSeparator9,
            mnubtnCollapse,
            mnubtnExpand,
            toolStripSeparator4,
            mnubtnMoveUp,
            mnubtnMoveDown,
            toolStripSeparator1,
            runScriptToolStripMenuItem});
            menuMain.Name = "menuMain";
            menuMain.Size = new Size(263, 848);
            menuMain.Opening += contextMenu_Opening;
            menuMain.Opened += contextMenu_Opened;
            // 
            // copyContextMenuItem
            // 
            copyContextMenuItem.Image = Properties.Images.CopyToClipboard;
            copyContextMenuItem.Name = "copyContextMenuItem";
            copyContextMenuItem.Size = new Size(266, 26);
            copyContextMenuItem.Text = "&Copy to clipboard";
            // 
            // filterForSelectedRefsMenuItem
            // 
            filterForSelectedRefsMenuItem.Image = Properties.Images.ShowThisBranchOnly;
            filterForSelectedRefsMenuItem.Name = "filterForSelectedRefsMenuItem";
            filterForSelectedRefsMenuItem.Size = new Size(266, 26);
            filterForSelectedRefsMenuItem.Text = "&Filter for selected";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(263, 6);
            // 
            // mnuBtnManageRemotesFromRootNode
            // 
            mnuBtnManageRemotesFromRootNode.Image = Properties.Images.Remotes;
            mnuBtnManageRemotesFromRootNode.Name = "mnuBtnManageRemotesFromRootNode";
            mnuBtnManageRemotesFromRootNode.Size = new Size(266, 26);
            mnuBtnManageRemotesFromRootNode.Text = "&Manage...";
            mnuBtnManageRemotesFromRootNode.ToolTipText = "Manage remotes";
            // 
            // mnuBtnFetchAllRemotes
            // 
            mnuBtnFetchAllRemotes.Image = Properties.Images.PullFetchAll;
            mnuBtnFetchAllRemotes.Name = "mnuBtnFetchAllRemotes";
            mnuBtnFetchAllRemotes.Size = new Size(266, 26);
            mnuBtnFetchAllRemotes.Text = "Fetch all remotes";
            // 
            // mnuBtnPruneAllRemotes
            // 
            mnuBtnPruneAllRemotes.Image = Properties.Images.PullFetchPruneAll;
            mnuBtnPruneAllRemotes.Name = "mnuBtnPruneAllRemotes";
            mnuBtnPruneAllRemotes.Size = new Size(266, 26);
            mnuBtnPruneAllRemotes.Text = "Fetch and prune all remotes";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(263, 6);
            // 
            // mnubtnManageRemotes
            // 
            mnubtnManageRemotes.Image = Properties.Images.Remotes;
            mnubtnManageRemotes.Name = "mnubtnManageRemotes";
            mnubtnManageRemotes.Size = new Size(266, 26);
            mnubtnManageRemotes.Text = "&Manage...";
            mnubtnManageRemotes.ToolTipText = "Manage remotes";
            // 
            // mnubtnEnableRemote
            // 
            mnubtnEnableRemote.Image = Properties.Images.EyeOpened;
            mnubtnEnableRemote.Name = "mnubtnEnableRemote";
            mnubtnEnableRemote.Size = new Size(266, 26);
            mnubtnEnableRemote.Text = "&Activate";
            // 
            // mnubtnEnableRemoteAndFetch
            // 
            mnubtnEnableRemoteAndFetch.Image = Properties.Images.RemoteEnableAndFetch;
            mnubtnEnableRemoteAndFetch.Name = "mnubtnEnableRemoteAndFetch";
            mnubtnEnableRemoteAndFetch.Size = new Size(266, 26);
            mnubtnEnableRemoteAndFetch.Text = "A&ctivate and fetch";
            // 
            // mnubtnDisableRemote
            // 
            mnubtnDisableRemote.Image = Properties.Images.EyeClosed;
            mnubtnDisableRemote.Name = "mnubtnDisableRemote";
            mnubtnDisableRemote.Size = new Size(266, 26);
            mnubtnDisableRemote.Text = "&Deactivate";
            // 
            // mnubtnFetchAllBranchesFromARemote
            // 
            mnubtnFetchAllBranchesFromARemote.Image = Properties.Images.PullFetch;
            mnubtnFetchAllBranchesFromARemote.Name = "mnubtnFetchAllBranchesFromARemote";
            mnubtnFetchAllBranchesFromARemote.Size = new Size(266, 26);
            mnubtnFetchAllBranchesFromARemote.Text = "&Fetch";
            // 
            // mnuBtnPruneAllBranchesFromARemote
            // 
            mnuBtnPruneAllBranchesFromARemote.Image = Properties.Images.PullFetchPrune;
            mnuBtnPruneAllBranchesFromARemote.Name = "mnuBtnPruneAllBranchesFromARemote";
            mnuBtnPruneAllBranchesFromARemote.Size = new Size(266, 26);
            mnuBtnPruneAllBranchesFromARemote.Text = "Fetch and prune";
            // 
            // mnuBtnOpenRemoteUrlInBrowser
            // 
            mnuBtnOpenRemoteUrlInBrowser.Image = Properties.Images.Globe;
            mnuBtnOpenRemoteUrlInBrowser.Name = "mnuBtnOpenRemoteUrlInBrowser";
            mnuBtnOpenRemoteUrlInBrowser.Size = new Size(266, 26);
            mnuBtnOpenRemoteUrlInBrowser.Text = "Open remote Url";
            mnuBtnOpenRemoteUrlInBrowser.ToolTipText = "Redirects you to the actual repository page";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(263, 6);
            // 
            // mnubtnOpenSubmodule
            // 
            mnubtnOpenSubmodule.Image = Properties.Images.FolderOpen;
            mnubtnOpenSubmodule.Name = "mnubtnOpenSubmodule";
            mnubtnOpenSubmodule.Size = new Size(266, 26);
            mnubtnOpenSubmodule.Text = "&Open";
            mnubtnOpenSubmodule.ToolTipText = "Open selected submodule";
            // 
            // mnubtnOpenGESubmodule
            // 
            mnubtnOpenGESubmodule.Image = Properties.Images.GitExtensionsLogo16;
            mnubtnOpenGESubmodule.Name = "mnubtnOpenGESubmodule";
            mnubtnOpenGESubmodule.Size = new Size(266, 26);
            mnubtnOpenGESubmodule.Text = "O&pen";
            mnubtnOpenGESubmodule.ToolTipText = "Open selected submodule in a new instance";
            // 
            // mnubtnManageSubmodules
            // 
            mnubtnManageSubmodules.Image = Properties.Images.SubmodulesManage;
            mnubtnManageSubmodules.Name = "mnubtnManageSubmodules";
            mnubtnManageSubmodules.Size = new Size(266, 26);
            mnubtnManageSubmodules.Text = "&Manage...";
            mnubtnManageSubmodules.ToolTipText = "Manage submodules";
            // 
            // mnubtnUpdateSubmodule
            // 
            mnubtnUpdateSubmodule.Image = Properties.Images.SubmodulesUpdate;
            mnubtnUpdateSubmodule.Name = "mnubtnUpdateSubmodule";
            mnubtnUpdateSubmodule.Size = new Size(266, 26);
            mnubtnUpdateSubmodule.Text = "&Update";
            mnubtnUpdateSubmodule.ToolTipText = "Update selected submodule recursively";
            // 
            // mnubtnSynchronizeSubmodules
            // 
            mnubtnSynchronizeSubmodules.Image = Properties.Images.SubmodulesSync;
            mnubtnSynchronizeSubmodules.Name = "mnubtnSynchronizeSubmodules";
            mnubtnSynchronizeSubmodules.Size = new Size(266, 26);
            mnubtnSynchronizeSubmodules.Text = "Synchronize";
            mnubtnSynchronizeSubmodules.ToolTipText = "Synchronize selected submodule recursively";
            // 
            // mnubtnResetSubmodule
            // 
            mnubtnResetSubmodule.Image = Properties.Images.ResetWorkingDirChanges;
            mnubtnResetSubmodule.Name = "mnubtnResetSubmodule";
            mnubtnResetSubmodule.Size = new Size(266, 26);
            mnubtnResetSubmodule.Text = "&Reset";
            mnubtnResetSubmodule.ToolTipText = "Reset selected submodule";
            // 
            // mnubtnStashSubmodule
            // 
            mnubtnStashSubmodule.Image = Properties.Images.Stash;
            mnubtnStashSubmodule.Name = "mnubtnStashSubmodule";
            mnubtnStashSubmodule.Size = new Size(266, 26);
            mnubtnStashSubmodule.Text = "&Stash";
            mnubtnStashSubmodule.ToolTipText = "Stash changes in selected submodule";
            // 
            // mnubtnCommitSubmodule
            // 
            mnubtnCommitSubmodule.Image = Properties.Images.RepoStateDirtySubmodules;
            mnubtnCommitSubmodule.Name = "mnubtnCommitSubmodule";
            mnubtnCommitSubmodule.Size = new Size(266, 26);
            mnubtnCommitSubmodule.Text = "&Commit";
            mnubtnCommitSubmodule.ToolTipText = "Commit changes in selected submodule";
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(263, 6);
            // 
            // mnubtnRemoteBranchFetchAndCheckout
            // 
            mnubtnRemoteBranchFetchAndCheckout.Image = Properties.Images.BranchCheckout;
            mnubtnRemoteBranchFetchAndCheckout.Name = "mnubtnRemoteBranchFetchAndCheckout";
            mnubtnRemoteBranchFetchAndCheckout.Size = new Size(266, 26);
            mnubtnRemoteBranchFetchAndCheckout.Text = "&Fetch && Checkout";
            mnubtnRemoteBranchFetchAndCheckout.ToolTipText = "Fetch then checkout this remote branch";
            // 
            // mnubtnPullFromRemoteBranch
            // 
            mnubtnPullFromRemoteBranch.Image = Properties.Images.Pull;
            mnubtnPullFromRemoteBranch.Name = "mnubtnPullFromRemoteBranch";
            mnubtnPullFromRemoteBranch.Size = new Size(266, 26);
            mnubtnPullFromRemoteBranch.Text = "Fetch && Merge (&Pull)";
            mnubtnPullFromRemoteBranch.ToolTipText = "Fetch then merge this remote branch into current branch";
            // 
            // mnubtnFetchRebase
            // 
            mnubtnFetchRebase.Image = Properties.Images.Rebase;
            mnubtnFetchRebase.Name = "mnubtnFetchRebase";
            mnubtnFetchRebase.Size = new Size(266, 26);
            mnubtnFetchRebase.Text = "Fetch && Re&base";
            mnubtnFetchRebase.ToolTipText = "Fetch then rebase current branch on this remote branch";
            // 
            // mnubtnFetchCreateBranch
            // 
            mnubtnFetchCreateBranch.Image = Properties.Images.Branch;
            mnubtnFetchCreateBranch.Name = "mnubtnFetchCreateBranch";
            mnubtnFetchCreateBranch.Size = new Size(266, 26);
            mnubtnFetchCreateBranch.Text = "Fetc&h && Create Branch";
            mnubtnFetchCreateBranch.ToolTipText = "Fetch then create a local branch from the remote branch";
            // 
            // mnubtnFetchOneBranch
            // 
            mnubtnFetchOneBranch.Image = Properties.Images.Stage;
            mnubtnFetchOneBranch.Name = "mnubtnFetchOneBranch";
            mnubtnFetchOneBranch.Size = new Size(266, 26);
            mnubtnFetchOneBranch.Text = "Fe&tch";
            mnubtnFetchOneBranch.ToolTipText = "Fetch this remote branch";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(263, 6);
            // 
            // mnubtnCreateBranch
            // 
            mnubtnCreateBranch.Image = Properties.Images.BranchCreate;
            mnubtnCreateBranch.Name = "mnubtnCreateBranch";
            mnubtnCreateBranch.Size = new Size(266, 26);
            mnubtnCreateBranch.Text = "Create Branch...";
            mnubtnCreateBranch.ToolTipText = "Create a local branch";
            // 
            // mnubtnDeleteAllBranches
            // 
            mnubtnDeleteAllBranches.Image = Properties.Images.BranchDelete;
            mnubtnDeleteAllBranches.Name = "mnubtnDeleteAllBranches";
            mnubtnDeleteAllBranches.Size = new Size(266, 26);
            mnubtnDeleteAllBranches.Text = "Delete All";
            mnubtnDeleteAllBranches.ToolTipText = "Delete all child branches, which must all be fully merged in its upstream branch o" +
    "r in HEAD";
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(263, 6);
            // 
            // mnubtnStashAllFromRootNode
            // 
            mnubtnStashAllFromRootNode.Name = "mnubtnStashAllFromRootNode";
            mnubtnStashAllFromRootNode.Size = new Size(266, 26);
            mnubtnStashAllFromRootNode.Text = "&Stash";
            // 
            // mnubtnStashStagedFromRootNode
            // 
            mnubtnStashStagedFromRootNode.Name = "mnubtnStashStagedFromRootNode";
            mnubtnStashStagedFromRootNode.Size = new Size(266, 26);
            mnubtnStashStagedFromRootNode.Text = "S&tash staged";
            // 
            // mnubtnManageStashFromRootNode
            // 
            mnubtnManageStashFromRootNode.Name = "mnubtnManageStashFromRootNode";
            mnubtnManageStashFromRootNode.Size = new Size(266, 26);
            mnubtnManageStashFromRootNode.Text = "&Manage stashes...";
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(263, 6);
            // 
            // mnubtnOpenStash
            // 
            mnubtnOpenStash.Name = "mnubtnOpenStash";
            mnubtnOpenStash.Size = new Size(266, 26);
            mnubtnOpenStash.Text = "&Open stash";
            mnubtnOpenStash.ToolTipText = "Open this stash";
            // 
            // mnubtnApplyStash
            // 
            mnubtnApplyStash.Name = "mnubtnApplyStash";
            mnubtnApplyStash.Size = new Size(266, 26);
            mnubtnApplyStash.Text = "&Apply stash";
            mnubtnApplyStash.ToolTipText = "Apply this stash";
            // 
            // mnubtnPopStash
            // 
            mnubtnPopStash.Name = "mnubtnPopStash";
            mnubtnPopStash.Size = new Size(266, 26);
            mnubtnPopStash.Text = "&Pop stash";
            mnubtnPopStash.ToolTipText = "Pop this stash";
            // 
            // mnubtnDropStash
            // 
            mnubtnDropStash.Name = "mnubtnDropStash";
            mnubtnDropStash.Size = new Size(266, 26);
            mnubtnDropStash.Text = "&Drop stash...";
            mnubtnDropStash.ToolTipText = "Drop this stash";
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(263, 6);
            // 
            // mnubtnCollapse
            // 
            mnubtnCollapse.Image = Properties.Images.CollapseAll;
            mnubtnCollapse.Name = "mnubtnCollapse";
            mnubtnCollapse.Size = new Size(266, 26);
            mnubtnCollapse.Text = "Collapse";
            mnubtnCollapse.ToolTipText = "Collapse all subnodes";
            // 
            // mnubtnExpand
            // 
            mnubtnExpand.Image = Properties.Images.ExpandAll;
            mnubtnExpand.Name = "mnubtnExpand";
            mnubtnExpand.Size = new Size(266, 26);
            mnubtnExpand.Text = "Expand";
            mnubtnExpand.ToolTipText = "Expand all subnodes";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(263, 6);
            // 
            // mnubtnMoveUp
            // 
            mnubtnMoveUp.Image = Properties.Images.ArrowUp;
            mnubtnMoveUp.Name = "mnubtnMoveUp";
            mnubtnMoveUp.Size = new Size(266, 26);
            mnubtnMoveUp.Text = "Move Up";
            mnubtnMoveUp.ToolTipText = "Move node up";
            // 
            // mnubtnMoveDown
            // 
            mnubtnMoveDown.Image = Properties.Images.ArrowDown;
            mnubtnMoveDown.Name = "mnubtnMoveDown";
            mnubtnMoveDown.Size = new Size(266, 26);
            mnubtnMoveDown.Text = "Move Down";
            mnubtnMoveDown.ToolTipText = "Move node down";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(263, 6);
            // 
            // runScriptToolStripMenuItem
            // 
            runScriptToolStripMenuItem.Image = Properties.Images.Console;
            runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            runScriptToolStripMenuItem.Size = new Size(266, 26);
            runScriptToolStripMenuItem.Text = "Run script";
            // 
            // tsmiMainMenuSpacer1
            // 
            tsmiMainMenuSpacer1.Name = "tsmiMainMenuSpacer1";
            tsmiMainMenuSpacer1.Size = new Size(133, 6);
            // 
            // tsmiMainMenuSpacer2
            // 
            tsmiMainMenuSpacer2.Name = "tsmiMainMenuSpacer2";
            tsmiMainMenuSpacer2.Size = new Size(133, 6);
            // 
            // repoTreePanel
            // 
            repoTreePanel.ColumnCount = 1;
            repoTreePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            repoTreePanel.Controls.Add(leftPanelToolStrip, 0, 0);
            repoTreePanel.Controls.Add(branchSearchPanel, 0, 1);
            repoTreePanel.Controls.Add(treeMain, 0, 2);
            repoTreePanel.Dock = DockStyle.Fill;
            repoTreePanel.Location = new Point(0, 0);
            repoTreePanel.Margin = new Padding(0);
            repoTreePanel.Name = "repoTreePanel";
            repoTreePanel.RowCount = 3;
            repoTreePanel.RowStyles.Add(new RowStyle());
            repoTreePanel.RowStyles.Add(new RowStyle());
            repoTreePanel.RowStyles.Add(new RowStyle());
            repoTreePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            repoTreePanel.Size = new Size(300, 350);
            repoTreePanel.TabIndex = 4;
            // 
            // leftPanelToolStrip
            // 
            leftPanelToolStrip.BackColor = SystemColors.Control;
            leftPanelToolStrip.ClickThrough = true;
            leftPanelToolStrip.DrawBorder = false;
            leftPanelToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            leftPanelToolStrip.Items.AddRange(new ToolStripItem[] {
            tsbCollapseAll,
            tsbShowBranches,
            tsbShowRemotes,
            tsbShowTags,
            tsbShowSubmodules,
            tsbShowStashes});
            leftPanelToolStrip.Location = new Point(0, 0);
            leftPanelToolStrip.Name = "leftPanelToolStrip";
            leftPanelToolStrip.Size = new Size(300, 27);
            leftPanelToolStrip.TabIndex = 5;
            // 
            // tsbCollapseAll
            // 
            tsbCollapseAll.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCollapseAll.Image = Properties.Images.CollapseAll;
            tsbCollapseAll.ImageTransparentColor = Color.Magenta;
            tsbCollapseAll.Name = "tsbCollapseAll";
            tsbCollapseAll.Size = new Size(29, 24);
            tsbCollapseAll.Click += btnCollapseAll_Click;
            // 
            // tsbShowBranches
            // 
            tsbShowBranches.CheckOnClick = true;
            tsbShowBranches.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShowBranches.Image = Properties.Images.BranchLocalRoot;
            tsbShowBranches.ImageTransparentColor = Color.Magenta;
            tsbShowBranches.Name = "tsbShowBranches";
            tsbShowBranches.Size = new Size(29, 24);
            tsbShowBranches.Text = "&Branches";
            tsbShowBranches.Click += tsbShowBranches_Click;
            // 
            // tsbShowRemotes
            // 
            tsbShowRemotes.CheckOnClick = true;
            tsbShowRemotes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShowRemotes.Image = Properties.Images.BranchRemoteRoot;
            tsbShowRemotes.ImageTransparentColor = Color.Magenta;
            tsbShowRemotes.Name = "tsbShowRemotes";
            tsbShowRemotes.Size = new Size(29, 24);
            tsbShowRemotes.Text = "&Remotes";
            tsbShowRemotes.Click += tsbShowRemotes_Click;
            // 
            // tsbShowTags
            // 
            tsbShowTags.CheckOnClick = true;
            tsbShowTags.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShowTags.Image = Properties.Images.TagHorizontal;
            tsbShowTags.ImageTransparentColor = Color.Magenta;
            tsbShowTags.Name = "tsbShowTags";
            tsbShowTags.Size = new Size(29, 24);
            tsbShowTags.Text = "&Tags";
            tsbShowTags.Click += tsbShowTags_Click;
            // 
            // tsbShowSubmodules
            // 
            tsbShowSubmodules.CheckOnClick = true;
            tsbShowSubmodules.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShowSubmodules.Image = Properties.Images.FolderSubmodule;
            tsbShowSubmodules.ImageTransparentColor = Color.Magenta;
            tsbShowSubmodules.Name = "tsbShowSubmodules";
            tsbShowSubmodules.Size = new Size(29, 24);
            tsbShowSubmodules.Text = "&Submodules";
            tsbShowSubmodules.Click += tsbShowSubmodules_Click;
            // 
            // tsbShowStashes
            // 
            tsbShowStashes.CheckOnClick = true;
            tsbShowStashes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShowStashes.Image = Properties.Images.Stash;
            tsbShowStashes.ImageTransparentColor = Color.Magenta;
            tsbShowStashes.Name = "tsbShowStashes";
            tsbShowStashes.Size = new Size(29, 24);
            tsbShowStashes.Text = "St&ashes";
            tsbShowStashes.Click += tsbShowStashes_Click;
            // 
            // branchSearchPanel
            // 
            branchSearchPanel.AutoSize = true;
            branchSearchPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            branchSearchPanel.BackColor = SystemColors.Control;
            branchSearchPanel.ColumnCount = 5;
            branchSearchPanel.ColumnStyles.Add(new ColumnStyle());
            branchSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            branchSearchPanel.ColumnStyles.Add(new ColumnStyle());
            branchSearchPanel.ColumnStyles.Add(new ColumnStyle());
            branchSearchPanel.ColumnStyles.Add(new ColumnStyle());
            branchSearchPanel.Controls.Add(btnSearch, 2, 0);
            branchSearchPanel.Dock = DockStyle.Fill;
            branchSearchPanel.Location = new Point(0, 27);
            branchSearchPanel.Margin = new Padding(0);
            branchSearchPanel.Name = "branchSearchPanel";
            branchSearchPanel.RowStyles.Add(new RowStyle());
            branchSearchPanel.Size = new Size(300, 26);
            branchSearchPanel.TabIndex = 4;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Left;
            btnSearch.AutoSize = true;
            btnSearch.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Image = Properties.Images.Preview;
            btnSearch.Location = new Point(274, 0);
            btnSearch.Margin = new Padding(0);
            btnSearch.Name = "btnSearch";
            btnSearch.Padding = new Padding(2);
            btnSearch.Size = new Size(26, 26);
            btnSearch.TabIndex = 2;
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += OnBtnSearchClicked;
            // 
            // RepoObjectsTree
            // 
            Controls.Add(repoTreePanel);
            MinimumSize = new Size(190, 0);
            Name = "RepoObjectsTree";
            Size = new Size(300, 350);
            menuMain.ResumeLayout(false);
            repoTreePanel.ResumeLayout(false);
            repoTreePanel.PerformLayout();
            leftPanelToolStrip.ResumeLayout(false);
            leftPanelToolStrip.PerformLayout();
            branchSearchPanel.ResumeLayout(false);
            branchSearchPanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

#nullable disable

        private NativeTreeView treeMain;
        private ContextMenuStrip menuMain;
        private ToolStripMenuItem mnubtnCollapse;
        private ToolStripMenuItem mnubtnExpand;
        private TableLayoutPanel repoTreePanel;
        private TableLayoutPanel branchSearchPanel;
        private Button btnSearch;
        private ToolStripSeparator tsmiMainMenuSpacer1;
        private ToolStripSeparator tsmiMainMenuSpacer2;
        private ToolTip toolTip;
        private ToolStripMenuItem mnubtnMoveUp;
        private ToolStripMenuItem mnubtnMoveDown;
        private ToolStripEx leftPanelToolStrip;
        private ToolStripButton tsbCollapseAll;
        private ToolStripButton tsbShowBranches;
        private ToolStripButton tsbShowRemotes;
        private ToolStripButton tsbShowTags;
        private ToolStripButton tsbShowStashes;
        private ToolStripButton tsbShowSubmodules;
        private UserControls.RevisionGrid.CopyContextMenuItem copyContextMenuItem;
        private ToolStripMenuItem filterForSelectedRefsMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuBtnManageRemotesFromRootNode;
        private ToolStripMenuItem mnuBtnFetchAllRemotes;
        private ToolStripMenuItem mnuBtnPruneAllRemotes;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem mnubtnManageRemotes;
        private ToolStripMenuItem mnubtnEnableRemote;
        private ToolStripMenuItem mnubtnEnableRemoteAndFetch;
        private ToolStripMenuItem mnubtnDisableRemote;
        private ToolStripMenuItem mnubtnFetchAllBranchesFromARemote;
        private ToolStripMenuItem mnuBtnPruneAllBranchesFromARemote;
        private ToolStripMenuItem mnuBtnOpenRemoteUrlInBrowser;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem mnubtnManageSubmodules;
        private ToolStripMenuItem mnubtnOpenSubmodule;
        private ToolStripMenuItem mnubtnOpenGESubmodule;
        private ToolStripMenuItem mnubtnUpdateSubmodule;
        private ToolStripMenuItem mnubtnSynchronizeSubmodules;
        private ToolStripMenuItem mnubtnResetSubmodule;
        private ToolStripMenuItem mnubtnStashSubmodule;
        private ToolStripMenuItem mnubtnCommitSubmodule;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem mnubtnRemoteBranchFetchAndCheckout;
        private ToolStripMenuItem mnubtnPullFromRemoteBranch;
        private ToolStripMenuItem mnubtnFetchRebase;
        private ToolStripMenuItem mnubtnFetchCreateBranch;
        private ToolStripMenuItem mnubtnFetchOneBranch;
        private ToolStripMenuItem mnubtnCreateBranch;
        private ToolStripMenuItem mnubtnDeleteAllBranches;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem mnubtnStashAllFromRootNode;
        private ToolStripMenuItem mnubtnStashStagedFromRootNode;
        private ToolStripMenuItem mnubtnManageStashFromRootNode;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem mnubtnOpenStash;
        private ToolStripMenuItem mnubtnApplyStash;
        private ToolStripMenuItem mnubtnPopStash;
        private ToolStripMenuItem mnubtnDropStash;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem runScriptToolStripMenuItem;

#nullable restore
    }
}
