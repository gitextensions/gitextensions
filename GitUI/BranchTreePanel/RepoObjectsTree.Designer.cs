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
            this.components = new System.ComponentModel.Container();
            this.treeMain = new GitUI.UserControls.NativeTreeView();
            this.menuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyContextMenuItem = new GitUI.UserControls.RevisionGrid.CopyContextMenuItem();
            this.filterForSelectedRefsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuBtnManageRemotesFromRootNode = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnFetchAllRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnPruneAllRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnManageRemotes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnEnableRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnEnableRemoteAndFetch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnDisableRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchAllBranchesFromARemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnPruneAllBranchesFromARemote = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBtnOpenRemoteUrlInBrowser = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnOpenSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnOpenGESubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnManageSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnUpdateSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnSynchronizeSubmodules = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnResetSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnCommitSubmodule = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnRemoteBranchFetchAndCheckout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnPullFromRemoteBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchRebase = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnFetchOneBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnDeleteAllBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnStashAllFromRootNode = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnStashStagedFromRootNode = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnManageStashFromRootNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnOpenStash = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnApplyStash = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnPopStash = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnDropStash = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnCollapse = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnExpand = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnubtnMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnubtnMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.runScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMainMenuSpacer1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiMainMenuSpacer2 = new System.Windows.Forms.ToolStripSeparator();
            this.repoTreePanel = new System.Windows.Forms.TableLayoutPanel();
            this.leftPanelToolStrip = new GitUI.ToolStripEx();
            this.tsbCollapseAll = new System.Windows.Forms.ToolStripButton();
            this.tsbShowBranches = new System.Windows.Forms.ToolStripButton();
            this.tsbShowRemotes = new System.Windows.Forms.ToolStripButton();
            this.tsbShowTags = new System.Windows.Forms.ToolStripButton();
            this.tsbShowStashes = new System.Windows.Forms.ToolStripButton();
            this.tsbShowSubmodules = new System.Windows.Forms.ToolStripButton();
            this.branchSearchPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnSearch = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuMain.SuspendLayout();
            this.repoTreePanel.SuspendLayout();
            this.leftPanelToolStrip.SuspendLayout();
            this.branchSearchPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeMain
            // 
            this.treeMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeMain.ContextMenuStrip = this.menuMain;
            this.treeMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeMain.FullRowSelect = true;
            this.treeMain.Location = new System.Drawing.Point(0, 53);
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
            this.copyContextMenuItem,
            this.filterForSelectedRefsMenuItem,
            this.toolStripSeparator2,
            this.mnuBtnManageRemotesFromRootNode,
            this.mnuBtnFetchAllRemotes,
            this.mnuBtnPruneAllRemotes,
            this.toolStripSeparator3,
            this.mnubtnManageRemotes,
            this.mnubtnEnableRemote,
            this.mnubtnEnableRemoteAndFetch,
            this.mnubtnDisableRemote,
            this.mnubtnFetchAllBranchesFromARemote,
            this.mnuBtnPruneAllBranchesFromARemote,
            this.mnuBtnOpenRemoteUrlInBrowser,
            this.toolStripSeparator5,
            this.mnubtnOpenSubmodule,
            this.mnubtnOpenGESubmodule,
            this.mnubtnManageSubmodules,
            this.mnubtnUpdateSubmodule,
            this.mnubtnSynchronizeSubmodules,
            this.mnubtnResetSubmodule,
            this.mnubtnStashSubmodule,
            this.mnubtnCommitSubmodule,
            this.toolStripSeparator8,
            this.mnubtnRemoteBranchFetchAndCheckout,
            this.mnubtnPullFromRemoteBranch,
            this.mnubtnFetchRebase,
            this.mnubtnFetchCreateBranch,
            this.mnubtnFetchOneBranch,
            this.toolStripSeparator6,
            this.mnubtnCreateBranch,
            this.mnubtnDeleteAllBranches,
            this.toolStripSeparator10,
            this.mnubtnStashAllFromRootNode,
            this.mnubtnStashStagedFromRootNode,
            this.mnubtnManageStashFromRootNode,
            this.toolStripSeparator7,
            this.mnubtnOpenStash,
            this.mnubtnApplyStash,
            this.mnubtnPopStash,
            this.mnubtnDropStash,
            this.toolStripSeparator9,
            this.mnubtnCollapse,
            this.mnubtnExpand,
            this.toolStripSeparator4,
            this.mnubtnMoveUp,
            this.mnubtnMoveDown,
            this.toolStripSeparator1,
            this.runScriptToolStripMenuItem});
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(263, 848);
            this.menuMain.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            this.menuMain.Opened += new System.EventHandler(this.contextMenu_Opened);
            // 
            // copyContextMenuItem
            // 
            this.copyContextMenuItem.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyContextMenuItem.Name = "copyContextMenuItem";
            this.copyContextMenuItem.Size = new System.Drawing.Size(266, 26);
            this.copyContextMenuItem.Text = "&Copy to clipboard";
            // 
            // filterForSelectedRefsMenuItem
            // 
            this.filterForSelectedRefsMenuItem.Image = global::GitUI.Properties.Images.ShowThisBranchOnly;
            this.filterForSelectedRefsMenuItem.Name = "filterForSelectedRefsMenuItem";
            this.filterForSelectedRefsMenuItem.Size = new System.Drawing.Size(266, 26);
            this.filterForSelectedRefsMenuItem.Text = "&Filter for selected";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(263, 6);
            // 
            // mnuBtnManageRemotesFromRootNode
            // 
            this.mnuBtnManageRemotesFromRootNode.Image = global::GitUI.Properties.Images.Remotes;
            this.mnuBtnManageRemotesFromRootNode.Name = "mnuBtnManageRemotesFromRootNode";
            this.mnuBtnManageRemotesFromRootNode.Size = new System.Drawing.Size(266, 26);
            this.mnuBtnManageRemotesFromRootNode.Text = "&Manage...";
            this.mnuBtnManageRemotesFromRootNode.ToolTipText = "Manage remotes";
            // 
            // mnuBtnFetchAllRemotes
            // 
            this.mnuBtnFetchAllRemotes.Image = global::GitUI.Properties.Images.PullFetchAll;
            this.mnuBtnFetchAllRemotes.Name = "mnuBtnFetchAllRemotes";
            this.mnuBtnFetchAllRemotes.Size = new System.Drawing.Size(266, 26);
            this.mnuBtnFetchAllRemotes.Text = "Fetch all remotes";
            // 
            // mnuBtnPruneAllRemotes
            // 
            this.mnuBtnPruneAllRemotes.Image = global::GitUI.Properties.Images.PullFetchPruneAll;
            this.mnuBtnPruneAllRemotes.Name = "mnuBtnPruneAllRemotes";
            this.mnuBtnPruneAllRemotes.Size = new System.Drawing.Size(266, 26);
            this.mnuBtnPruneAllRemotes.Text = "Fetch and prune all remotes";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnManageRemotes
            // 
            this.mnubtnManageRemotes.Image = global::GitUI.Properties.Images.Remotes;
            this.mnubtnManageRemotes.Name = "mnubtnManageRemotes";
            this.mnubtnManageRemotes.Size = new System.Drawing.Size(266, 26);
            this.mnubtnManageRemotes.Text = "&Manage...";
            this.mnubtnManageRemotes.ToolTipText = "Manage remotes";
            // 
            // mnubtnEnableRemote
            // 
            this.mnubtnEnableRemote.Image = global::GitUI.Properties.Images.EyeOpened;
            this.mnubtnEnableRemote.Name = "mnubtnEnableRemote";
            this.mnubtnEnableRemote.Size = new System.Drawing.Size(266, 26);
            this.mnubtnEnableRemote.Text = "&Activate";
            // 
            // mnubtnEnableRemoteAndFetch
            // 
            this.mnubtnEnableRemoteAndFetch.Image = global::GitUI.Properties.Images.RemoteEnableAndFetch;
            this.mnubtnEnableRemoteAndFetch.Name = "mnubtnEnableRemoteAndFetch";
            this.mnubtnEnableRemoteAndFetch.Size = new System.Drawing.Size(266, 26);
            this.mnubtnEnableRemoteAndFetch.Text = "A&ctivate and fetch";
            // 
            // mnubtnDisableRemote
            // 
            this.mnubtnDisableRemote.Image = global::GitUI.Properties.Images.EyeClosed;
            this.mnubtnDisableRemote.Name = "mnubtnDisableRemote";
            this.mnubtnDisableRemote.Size = new System.Drawing.Size(266, 26);
            this.mnubtnDisableRemote.Text = "&Deactivate";
            // 
            // mnubtnFetchAllBranchesFromARemote
            // 
            this.mnubtnFetchAllBranchesFromARemote.Image = global::GitUI.Properties.Images.PullFetch;
            this.mnubtnFetchAllBranchesFromARemote.Name = "mnubtnFetchAllBranchesFromARemote";
            this.mnubtnFetchAllBranchesFromARemote.Size = new System.Drawing.Size(266, 26);
            this.mnubtnFetchAllBranchesFromARemote.Text = "&Fetch";
            // 
            // mnuBtnPruneAllBranchesFromARemote
            // 
            this.mnuBtnPruneAllBranchesFromARemote.Image = global::GitUI.Properties.Images.PullFetchPrune;
            this.mnuBtnPruneAllBranchesFromARemote.Name = "mnuBtnPruneAllBranchesFromARemote";
            this.mnuBtnPruneAllBranchesFromARemote.Size = new System.Drawing.Size(266, 26);
            this.mnuBtnPruneAllBranchesFromARemote.Text = "Fetch and prune";
            // 
            // mnuBtnOpenRemoteUrlInBrowser
            // 
            this.mnuBtnOpenRemoteUrlInBrowser.Image = global::GitUI.Properties.Images.Globe;
            this.mnuBtnOpenRemoteUrlInBrowser.Name = "mnuBtnOpenRemoteUrlInBrowser";
            this.mnuBtnOpenRemoteUrlInBrowser.Size = new System.Drawing.Size(266, 26);
            this.mnuBtnOpenRemoteUrlInBrowser.Text = "Open remote Url";
            this.mnuBtnOpenRemoteUrlInBrowser.ToolTipText = "Redirects you to the actual repository page";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnOpenSubmodule
            // 
            this.mnubtnOpenSubmodule.Image = global::GitUI.Properties.Images.FolderOpen;
            this.mnubtnOpenSubmodule.Name = "mnubtnOpenSubmodule";
            this.mnubtnOpenSubmodule.Size = new System.Drawing.Size(266, 26);
            this.mnubtnOpenSubmodule.Text = "&Open";
            this.mnubtnOpenSubmodule.ToolTipText = "Open selected submodule";
            // 
            // mnubtnOpenGESubmodule
            // 
            this.mnubtnOpenGESubmodule.Image = global::GitUI.Properties.Images.GitExtensionsLogo16;
            this.mnubtnOpenGESubmodule.Name = "mnubtnOpenGESubmodule";
            this.mnubtnOpenGESubmodule.Size = new System.Drawing.Size(266, 26);
            this.mnubtnOpenGESubmodule.Text = "O&pen";
            this.mnubtnOpenGESubmodule.ToolTipText = "Open selected submodule in a new instance";
            // 
            // mnubtnManageSubmodules
            // 
            this.mnubtnManageSubmodules.Image = global::GitUI.Properties.Images.SubmodulesManage;
            this.mnubtnManageSubmodules.Name = "mnubtnManageSubmodules";
            this.mnubtnManageSubmodules.Size = new System.Drawing.Size(266, 26);
            this.mnubtnManageSubmodules.Text = "&Manage...";
            this.mnubtnManageSubmodules.ToolTipText = "Manage submodules";
            // 
            // mnubtnUpdateSubmodule
            // 
            this.mnubtnUpdateSubmodule.Image = global::GitUI.Properties.Images.SubmodulesUpdate;
            this.mnubtnUpdateSubmodule.Name = "mnubtnUpdateSubmodule";
            this.mnubtnUpdateSubmodule.Size = new System.Drawing.Size(266, 26);
            this.mnubtnUpdateSubmodule.Text = "&Update";
            this.mnubtnUpdateSubmodule.ToolTipText = "Update selected submodule recursively";
            // 
            // mnubtnSynchronizeSubmodules
            // 
            this.mnubtnSynchronizeSubmodules.Image = global::GitUI.Properties.Images.SubmodulesSync;
            this.mnubtnSynchronizeSubmodules.Name = "mnubtnSynchronizeSubmodules";
            this.mnubtnSynchronizeSubmodules.Size = new System.Drawing.Size(266, 26);
            this.mnubtnSynchronizeSubmodules.Text = "Synchronize";
            this.mnubtnSynchronizeSubmodules.ToolTipText = "Synchronize selected submodule recursively";
            // 
            // mnubtnResetSubmodule
            // 
            this.mnubtnResetSubmodule.Image = global::GitUI.Properties.Images.ResetWorkingDirChanges;
            this.mnubtnResetSubmodule.Name = "mnubtnResetSubmodule";
            this.mnubtnResetSubmodule.Size = new System.Drawing.Size(266, 26);
            this.mnubtnResetSubmodule.Text = "&Reset";
            this.mnubtnResetSubmodule.ToolTipText = "Reset selected submodule";
            // 
            // mnubtnStashSubmodule
            // 
            this.mnubtnStashSubmodule.Image = global::GitUI.Properties.Images.Stash;
            this.mnubtnStashSubmodule.Name = "mnubtnStashSubmodule";
            this.mnubtnStashSubmodule.Size = new System.Drawing.Size(266, 26);
            this.mnubtnStashSubmodule.Text = "&Stash";
            this.mnubtnStashSubmodule.ToolTipText = "Stash changes in selected submodule";
            // 
            // mnubtnCommitSubmodule
            // 
            this.mnubtnCommitSubmodule.Image = global::GitUI.Properties.Images.RepoStateDirtySubmodules;
            this.mnubtnCommitSubmodule.Name = "mnubtnCommitSubmodule";
            this.mnubtnCommitSubmodule.Size = new System.Drawing.Size(266, 26);
            this.mnubtnCommitSubmodule.Text = "&Commit";
            this.mnubtnCommitSubmodule.ToolTipText = "Commit changes in selected submodule";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnRemoteBranchFetchAndCheckout
            // 
            this.mnubtnRemoteBranchFetchAndCheckout.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.mnubtnRemoteBranchFetchAndCheckout.Name = "mnubtnRemoteBranchFetchAndCheckout";
            this.mnubtnRemoteBranchFetchAndCheckout.Size = new System.Drawing.Size(266, 26);
            this.mnubtnRemoteBranchFetchAndCheckout.Text = "&Fetch && Checkout";
            this.mnubtnRemoteBranchFetchAndCheckout.ToolTipText = "Fetch then checkout this remote branch";
            // 
            // mnubtnPullFromRemoteBranch
            // 
            this.mnubtnPullFromRemoteBranch.Image = global::GitUI.Properties.Images.Pull;
            this.mnubtnPullFromRemoteBranch.Name = "mnubtnPullFromRemoteBranch";
            this.mnubtnPullFromRemoteBranch.Size = new System.Drawing.Size(266, 26);
            this.mnubtnPullFromRemoteBranch.Text = "Fetch && Merge (&Pull)";
            this.mnubtnPullFromRemoteBranch.ToolTipText = "Fetch then merge this remote branch into current branch";
            // 
            // mnubtnFetchRebase
            // 
            this.mnubtnFetchRebase.Image = global::GitUI.Properties.Images.Rebase;
            this.mnubtnFetchRebase.Name = "mnubtnFetchRebase";
            this.mnubtnFetchRebase.Size = new System.Drawing.Size(266, 26);
            this.mnubtnFetchRebase.Text = "Fetch && Re&base";
            this.mnubtnFetchRebase.ToolTipText = "Fetch then rebase current branch on this remote branch";
            // 
            // mnubtnFetchCreateBranch
            // 
            this.mnubtnFetchCreateBranch.Image = global::GitUI.Properties.Images.Branch;
            this.mnubtnFetchCreateBranch.Name = "mnubtnFetchCreateBranch";
            this.mnubtnFetchCreateBranch.Size = new System.Drawing.Size(266, 26);
            this.mnubtnFetchCreateBranch.Text = "Fetc&h && Create Branch";
            this.mnubtnFetchCreateBranch.ToolTipText = "Fetch then create a local branch from the remote branch";
            // 
            // mnubtnFetchOneBranch
            // 
            this.mnubtnFetchOneBranch.Image = global::GitUI.Properties.Images.Stage;
            this.mnubtnFetchOneBranch.Name = "mnubtnFetchOneBranch";
            this.mnubtnFetchOneBranch.Size = new System.Drawing.Size(266, 26);
            this.mnubtnFetchOneBranch.Text = "Fe&tch";
            this.mnubtnFetchOneBranch.ToolTipText = "Fetch this remote branch";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnCreateBranch
            // 
            this.mnubtnCreateBranch.Image = global::GitUI.Properties.Images.BranchCreate;
            this.mnubtnCreateBranch.Name = "mnubtnCreateBranch";
            this.mnubtnCreateBranch.Size = new System.Drawing.Size(266, 26);
            this.mnubtnCreateBranch.Text = "Create Branch...";
            this.mnubtnCreateBranch.ToolTipText = "Create a local branch";
            // 
            // mnubtnDeleteAllBranches
            // 
            this.mnubtnDeleteAllBranches.Image = global::GitUI.Properties.Images.BranchDelete;
            this.mnubtnDeleteAllBranches.Name = "mnubtnDeleteAllBranches";
            this.mnubtnDeleteAllBranches.Size = new System.Drawing.Size(266, 26);
            this.mnubtnDeleteAllBranches.Text = "Delete All";
            this.mnubtnDeleteAllBranches.ToolTipText = "Delete all child branches, which must all be fully merged in its upstream branch o" +
    "r in HEAD";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnStashAllFromRootNode
            // 
            this.mnubtnStashAllFromRootNode.Name = "mnubtnStashAllFromRootNode";
            this.mnubtnStashAllFromRootNode.Size = new System.Drawing.Size(266, 26);
            this.mnubtnStashAllFromRootNode.Text = "&Stash";
            // 
            // mnubtnStashStagedFromRootNode
            // 
            this.mnubtnStashStagedFromRootNode.Name = "mnubtnStashStagedFromRootNode";
            this.mnubtnStashStagedFromRootNode.Size = new System.Drawing.Size(266, 26);
            this.mnubtnStashStagedFromRootNode.Text = "S&tash staged";
            // 
            // mnubtnManageStashFromRootNode
            // 
            this.mnubtnManageStashFromRootNode.Name = "mnubtnManageStashFromRootNode";
            this.mnubtnManageStashFromRootNode.Size = new System.Drawing.Size(266, 26);
            this.mnubtnManageStashFromRootNode.Text = "&Manage stashes...";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnOpenStash
            // 
            this.mnubtnOpenStash.Name = "mnubtnOpenStash";
            this.mnubtnOpenStash.Size = new System.Drawing.Size(266, 26);
            this.mnubtnOpenStash.Text = "&Open stash";
            this.mnubtnOpenStash.ToolTipText = "Open this stash";
            // 
            // mnubtnApplyStash
            // 
            this.mnubtnApplyStash.Name = "mnubtnApplyStash";
            this.mnubtnApplyStash.Size = new System.Drawing.Size(266, 26);
            this.mnubtnApplyStash.Text = "&Apply stash";
            this.mnubtnApplyStash.ToolTipText = "Apply this stash";
            // 
            // mnubtnPopStash
            // 
            this.mnubtnPopStash.Name = "mnubtnPopStash";
            this.mnubtnPopStash.Size = new System.Drawing.Size(266, 26);
            this.mnubtnPopStash.Text = "&Pop stash";
            this.mnubtnPopStash.ToolTipText = "Pop this stash";
            // 
            // mnubtnDropStash
            // 
            this.mnubtnDropStash.Name = "mnubtnDropStash";
            this.mnubtnDropStash.Size = new System.Drawing.Size(266, 26);
            this.mnubtnDropStash.Text = "&Drop stash...";
            this.mnubtnDropStash.ToolTipText = "Drop this stash";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnCollapse
            // 
            this.mnubtnCollapse.Image = global::GitUI.Properties.Images.CollapseAll;
            this.mnubtnCollapse.Name = "mnubtnCollapse";
            this.mnubtnCollapse.Size = new System.Drawing.Size(266, 26);
            this.mnubtnCollapse.Text = "Collapse";
            this.mnubtnCollapse.ToolTipText = "Collapse all subnodes";
            // 
            // mnubtnExpand
            // 
            this.mnubtnExpand.Image = global::GitUI.Properties.Images.ExpandAll;
            this.mnubtnExpand.Name = "mnubtnExpand";
            this.mnubtnExpand.Size = new System.Drawing.Size(266, 26);
            this.mnubtnExpand.Text = "Expand";
            this.mnubtnExpand.ToolTipText = "Expand all subnodes";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(263, 6);
            // 
            // mnubtnMoveUp
            // 
            this.mnubtnMoveUp.Image = global::GitUI.Properties.Images.ArrowUp;
            this.mnubtnMoveUp.Name = "mnubtnMoveUp";
            this.mnubtnMoveUp.Size = new System.Drawing.Size(266, 26);
            this.mnubtnMoveUp.Text = "Move Up";
            this.mnubtnMoveUp.ToolTipText = "Move node up";
            // 
            // mnubtnMoveDown
            // 
            this.mnubtnMoveDown.Image = global::GitUI.Properties.Images.ArrowDown;
            this.mnubtnMoveDown.Name = "mnubtnMoveDown";
            this.mnubtnMoveDown.Size = new System.Drawing.Size(266, 26);
            this.mnubtnMoveDown.Text = "Move Down";
            this.mnubtnMoveDown.ToolTipText = "Move node down";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(263, 6);
            // 
            // runScriptToolStripMenuItem
            // 
            this.runScriptToolStripMenuItem.Image = global::GitUI.Properties.Images.Console;
            this.runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            this.runScriptToolStripMenuItem.Size = new System.Drawing.Size(266, 26);
            this.runScriptToolStripMenuItem.Text = "Run script";
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
            this.tsbShowSubmodules,
            this.tsbShowStashes});
            this.leftPanelToolStrip.Location = new System.Drawing.Point(0, 0);
            this.leftPanelToolStrip.Name = "leftPanelToolStrip";
            this.leftPanelToolStrip.Size = new System.Drawing.Size(300, 27);
            this.leftPanelToolStrip.TabIndex = 5;
            // 
            // tsbCollapseAll
            // 
            this.tsbCollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCollapseAll.Image = global::GitUI.Properties.Images.CollapseAll;
            this.tsbCollapseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCollapseAll.Name = "tsbCollapseAll";
            this.tsbCollapseAll.Size = new System.Drawing.Size(29, 24);
            this.tsbCollapseAll.Click += new System.EventHandler(this.btnCollapseAll_Click);
            // 
            // tsbShowBranches
            // 
            this.tsbShowBranches.CheckOnClick = true;
            this.tsbShowBranches.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShowBranches.Image = global::GitUI.Properties.Images.BranchLocalRoot;
            this.tsbShowBranches.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShowBranches.Name = "tsbShowBranches";
            this.tsbShowBranches.Size = new System.Drawing.Size(29, 24);
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
            this.tsbShowRemotes.Size = new System.Drawing.Size(29, 24);
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
            this.tsbShowTags.Size = new System.Drawing.Size(29, 24);
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
            this.tsbShowSubmodules.Size = new System.Drawing.Size(29, 24);
            this.tsbShowSubmodules.Text = "&Submodules";
            this.tsbShowSubmodules.Click += new System.EventHandler(this.tsbShowSubmodules_Click);
            // 
            // tsbShowStashes
            // 
            this.tsbShowStashes.CheckOnClick = true;
            this.tsbShowStashes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShowStashes.Image = global::GitUI.Properties.Images.Stash;
            this.tsbShowStashes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShowStashes.Name = "tsbShowStashes";
            this.tsbShowStashes.Size = new System.Drawing.Size(29, 24);
            this.tsbShowStashes.Text = "St&ashes";
            this.tsbShowStashes.Click += new System.EventHandler(this.tsbShowStashes_Click);
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
            this.branchSearchPanel.Location = new System.Drawing.Point(0, 27);
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
            // RepoObjectsTree
            // 
            this.Controls.Add(this.repoTreePanel);
            this.MinimumSize = new System.Drawing.Size(190, 0);
            this.Name = "RepoObjectsTree";
            this.Size = new System.Drawing.Size(300, 350);
            this.menuMain.ResumeLayout(false);
            this.repoTreePanel.ResumeLayout(false);
            this.repoTreePanel.PerformLayout();
            this.leftPanelToolStrip.ResumeLayout(false);
            this.leftPanelToolStrip.PerformLayout();
            this.branchSearchPanel.ResumeLayout(false);
            this.branchSearchPanel.PerformLayout();
            this.ResumeLayout(false);

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
