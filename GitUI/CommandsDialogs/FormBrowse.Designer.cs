using System.Drawing;
using System.Windows.Forms;
using GitUI.CommandsDialogs.Menus;

namespace GitUI.CommandsDialogs
{
    partial class FormBrowse
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
            this.ToolStripMain = new GitUI.ToolStripEx();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator0 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleBranchTreePanel = new System.Windows.Forms.ToolStripButton();
            this.toggleSplitViewLayout = new System.Windows.Forms.ToolStripButton();
            this.menuCommitInfoPosition = new System.Windows.Forms.ToolStripSplitButton();
            this.commitInfoBelowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitInfoLeftwardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitInfoRightwardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonLevelUp = new System.Windows.Forms.ToolStripSplitButton();
            this._NO_TRANSLATE_WorkingDir = new System.Windows.Forms.ToolStripSplitButton();
            this.branchSelect = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitStash = new System.Windows.Forms.ToolStripSplitButton();
            this.stashChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashStagedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashPopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.manageStashesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createAStashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonCommit = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPull = new System.Windows.Forms.ToolStripSplitButton();
            this.mergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pullToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchPruneAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDefaultPullButtonActionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonPush = new GitUI.CommandsDialogs.ToolStripPushButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripFileExplorer = new System.Windows.Forms.ToolStripButton();
            this.userShell = new System.Windows.Forms.ToolStripSplitButton();
            this.EditSettings = new System.Windows.Forms.ToolStripButton();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.repoObjectsTree = new GitUI.BranchTreePanel.RepoObjectsTree();
            this.RightSplitContainer = new System.Windows.Forms.SplitContainer();
            this.RevisionsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.RevisionGridContainer = new System.Windows.Forms.Panel();
            this.RevisionGrid = new GitUI.RevisionGridControl();
            this.notificationBarBisectInProgress = new GitUI.UserControls.InteractiveGitActionControl();
            this.notificationBarGitActionInProgress = new GitUI.UserControls.InteractiveGitActionControl();
            this.CommitInfoTabControl = new GitUI.CommandsDialogs.FullBleedTabControl();
            this.CommitInfoTabPage = new System.Windows.Forms.TabPage();
            this.RevisionInfo = new GitUI.CommitInfo.CommitInfo();
            this.TreeTabPage = new System.Windows.Forms.TabPage();
            this.fileTree = new GitUI.CommandsDialogs.RevisionFileTreeControl();
            this.DiffTabPage = new System.Windows.Forms.TabPage();
            this.revisionDiff = new GitUI.CommandsDialogs.RevisionDiffControl();
            this.GpgInfoTabPage = new System.Windows.Forms.TabPage();
            this.revisionGpgInfo1 = new GitUI.CommandsDialogs.RevisionGpgInfoControl();
            this.FilterToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.fileToolStripMenuItem = new GitUI.CommandsDialogs.Menus.StartToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshDashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.manageRemoteRepositoriesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator43 = new System.Windows.Forms.ToolStripSeparator();
            this.manageSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synchronizeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.manageWorktreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator44 = new System.Windows.Forms.ToolStripSeparator();
            this.editgitignoreToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editgitinfoexcludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editGitAttributesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editmailmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSparse = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.gitMaintenanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recoverLostObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteIndexLockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editLocalGitConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repoSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoLastCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.stashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator25 = new System.Windows.Forms.ToolStripSeparator();
            this.branchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runMergetoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator45 = new System.Windows.Forms.ToolStripSeparator();
            this.tagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            this.cherryPickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemReflog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.formatPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator46 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator42 = new System.Windows.Forms.ToolStripSeparator();
            this._repositoryHostsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._forkCloneRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._viewPullRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._createPullRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.pluginSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new GitUI.CommandsDialogs.Menus.HelpToolStripMenuItem();
            this.toolsToolStripMenuItem = new GitUI.CommandsDialogs.Menus.ToolsToolStripMenuItem();
            this.mainMenuStrip = new GitUI.MenuStripEx();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.toolPanel = new System.Windows.Forms.ToolStripContainer();
            this._addUpstreamRemoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripFilters = new GitUI.UserControls.FilterToolBar();
            this.ToolStripScripts = new GitUI.ToolStripEx();
            toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMain.SuspendLayout();
            this.ToolStripFilters.SuspendLayout();
            this.ToolStripScripts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RightSplitContainer)).BeginInit();
            this.RightSplitContainer.Panel1.SuspendLayout();
            this.RightSplitContainer.Panel2.SuspendLayout();
            this.RightSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionsSplitContainer)).BeginInit();
            this.RevisionsSplitContainer.Panel1.SuspendLayout();
            this.RevisionsSplitContainer.SuspendLayout();
            this.RevisionGridContainer.SuspendLayout();
            this.CommitInfoTabControl.SuspendLayout();
            this.CommitInfoTabPage.SuspendLayout();
            this.TreeTabPage.SuspendLayout();
            this.DiffTabPage.SuspendLayout();
            this.GpgInfoTabPage.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.toolPanel.ContentPanel.SuspendLayout();
            this.toolPanel.TopToolStripPanel.SuspendLayout();
            this.toolPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(236, 6);
            // 
            // ToolStripMain
            // 
            this.ToolStripMain.ClickThrough = true;
            this.ToolStripMain.Dock = System.Windows.Forms.DockStyle.None;
            this.ToolStripMain.DrawBorder = false;
            this.ToolStripMain.GripEnabled = false;
            this.ToolStripMain.GripMargin = new System.Windows.Forms.Padding(0);
            this.ToolStripMain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshButton,
            this.toolStripSeparator0,
            this.toggleBranchTreePanel,
            this.toggleSplitViewLayout,
            this.menuCommitInfoPosition,
            this.toolStripSeparator17,
            this.toolStripButtonLevelUp,
            this._NO_TRANSLATE_WorkingDir,
            this.branchSelect,
            this.toolStripSeparator1,
            this.toolStripButtonPull,
            this.toolStripButtonPush,
            this.toolStripButtonCommit,
            this.toolStripSplitStash,
            this.toolStripSeparator2,
            this.toolStripFileExplorer,
            this.userShell,
            this.EditSettings});
            this.ToolStripMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStripMain.Location = new System.Drawing.Point(3, 0);
            this.ToolStripMain.Name = "ToolStripMain";
            this.ToolStripMain.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStripMain.Size = new System.Drawing.Size(479, 25);
            this.ToolStripMain.TabIndex = 0;
            this.ToolStripMain.Text = "Standard";
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = global::GitUI.Properties.Images.ReloadRevisions;
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.White;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.ToolTipText = "Refresh";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // toolStripSeparator0
            // 
            this.toolStripSeparator0.Name = "toolStripSeparator0";
            this.toolStripSeparator0.Size = new System.Drawing.Size(6, 25);
            // 
            // toggleBranchTreePanel
            // 
            this.toggleBranchTreePanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleBranchTreePanel.Image = global::GitUI.Properties.Images.LayoutSidebarLeft;
            this.toggleBranchTreePanel.Name = "toggleBranchTreePanel";
            this.toggleBranchTreePanel.Size = new System.Drawing.Size(23, 22);
            this.toggleBranchTreePanel.ToolTipText = "Toggle left panel";
            this.toggleBranchTreePanel.Click += new System.EventHandler(this.toggleBranchTreePanel_Click);
            // 
            // toggleSplitViewLayout
            // 
            this.toggleSplitViewLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleSplitViewLayout.Image = global::GitUI.Properties.Images.LayoutFooter;
            this.toggleSplitViewLayout.Name = "toggleSplitViewLayout";
            this.toggleSplitViewLayout.Size = new System.Drawing.Size(23, 22);
            this.toggleSplitViewLayout.ToolTipText = "Toggle split view layout";
            this.toggleSplitViewLayout.Click += new System.EventHandler(this.toggleSplitViewLayout_Click);
            // 
            // menuCommitInfoPosition
            // 
            this.menuCommitInfoPosition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.menuCommitInfoPosition.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitInfoBelowMenuItem,
            this.commitInfoLeftwardMenuItem,
            this.commitInfoRightwardMenuItem});
            this.menuCommitInfoPosition.Image = global::GitUI.Properties.Images.LayoutFooterTab;
            this.menuCommitInfoPosition.Name = "menuCommitInfoPosition";
            this.menuCommitInfoPosition.Size = new System.Drawing.Size(32, 22);
            this.menuCommitInfoPosition.ToolTipText = "Commit info position";
            this.menuCommitInfoPosition.Click += new System.EventHandler(this.CommitInfoPositionClick);
            // 
            // commitInfoBelowMenuItem
            // 
            this.commitInfoBelowMenuItem.Image = global::GitUI.Properties.Images.LayoutFooterTab;
            this.commitInfoBelowMenuItem.Name = "commitInfoBelowMenuItem";
            this.commitInfoBelowMenuItem.Size = new System.Drawing.Size(259, 22);
            this.commitInfoBelowMenuItem.Text = "Commit info &below graph";
            this.commitInfoBelowMenuItem.Click += new System.EventHandler(this.CommitInfoBelowClick);
            // 
            // commitInfoLeftwardMenuItem
            // 
            this.commitInfoLeftwardMenuItem.Image = global::GitUI.Properties.Images.LayoutSidebarTopLeft;
            this.commitInfoLeftwardMenuItem.Name = "commitInfoLeftwardMenuItem";
            this.commitInfoLeftwardMenuItem.Size = new System.Drawing.Size(259, 22);
            this.commitInfoLeftwardMenuItem.Text = "Commit info &left of graph";
            this.commitInfoLeftwardMenuItem.Click += new System.EventHandler(this.CommitInfoLeftwardClick);
            // 
            // commitInfoRightwardMenuItem
            // 
            this.commitInfoRightwardMenuItem.Image = global::GitUI.Properties.Images.LayoutSidebarTopRight;
            this.commitInfoRightwardMenuItem.Name = "commitInfoRightwardMenuItem";
            this.commitInfoRightwardMenuItem.Size = new System.Drawing.Size(259, 22);
            this.commitInfoRightwardMenuItem.Text = "Commit info &right of graph";
            this.commitInfoRightwardMenuItem.Click += new System.EventHandler(this.CommitInfoRightwardClick);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonLevelUp
            // 
            this.toolStripButtonLevelUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLevelUp.Image = global::GitUI.Properties.Images.SubmodulesManage;
            this.toolStripButtonLevelUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLevelUp.Name = "toolStripButtonLevelUp";
            this.toolStripButtonLevelUp.Size = new System.Drawing.Size(32, 22);
            this.toolStripButtonLevelUp.ToolTipText = "Submodules";
            this.toolStripButtonLevelUp.ButtonClick += new System.EventHandler(this.toolStripButtonLevelUp_ButtonClick);
            // 
            // _NO_TRANSLATE_WorkingDir
            // 
            this._NO_TRANSLATE_WorkingDir.Image = global::GitUI.Properties.Resources.RepoOpen;
            this._NO_TRANSLATE_WorkingDir.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._NO_TRANSLATE_WorkingDir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._NO_TRANSLATE_WorkingDir.Name = "_NO_TRANSLATE_WorkingDir";
            this._NO_TRANSLATE_WorkingDir.Size = new System.Drawing.Size(83, 22);
            this._NO_TRANSLATE_WorkingDir.Text = "WorkingDir";
            this._NO_TRANSLATE_WorkingDir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._NO_TRANSLATE_WorkingDir.ToolTipText = "Change working directory";
            this._NO_TRANSLATE_WorkingDir.ButtonClick += new System.EventHandler(this.WorkingDirClick);
            this._NO_TRANSLATE_WorkingDir.DropDownOpening += new System.EventHandler(this.WorkingDirDropDownOpening);
            this._NO_TRANSLATE_WorkingDir.MouseUp += new System.Windows.Forms.MouseEventHandler(this._NO_TRANSLATE_WorkingDir_MouseUp);
            // 
            // branchSelect
            // 
            this.branchSelect.Image = global::GitUI.Properties.Resources.branch;
            this.branchSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.branchSelect.Name = "branchSelect";
            this.branchSelect.Size = new System.Drawing.Size(60, 22);
            this.branchSelect.Text = "Branch";
            this.branchSelect.ToolTipText = "Change current branch";
            this.branchSelect.ButtonClick += new System.EventHandler(this.CurrentBranchClick);
            this.branchSelect.DropDownOpening += new System.EventHandler(this.CurrentBranchDropDownOpening);
            this.branchSelect.MouseUp += new System.Windows.Forms.MouseEventHandler(this.branchSelect_MouseUp);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSplitStash
            // 
            this.toolStripSplitStash.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stashChangesToolStripMenuItem,
            this.stashStagedToolStripMenuItem,
            this.stashPopToolStripMenuItem,
            this.toolStripSeparator9,
            this.manageStashesToolStripMenuItem,
            this.createAStashToolStripMenuItem});
            this.toolStripSplitStash.Image = global::GitUI.Properties.Images.Stash;
            this.toolStripSplitStash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitStash.Name = "toolStripSplitStash";
            this.toolStripSplitStash.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitStash.ToolTipText = "Manage stashes";
            this.toolStripSplitStash.ButtonClick += new System.EventHandler(this.ToolStripSplitStashButtonClick);
            // 
            // stashChangesToolStripMenuItem
            // 
            this.stashChangesToolStripMenuItem.Name = "stashChangesToolStripMenuItem";
            this.stashChangesToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.stashChangesToolStripMenuItem.Text = "&Stash";
            this.stashChangesToolStripMenuItem.ToolTipText = "Stash changes";
            this.stashChangesToolStripMenuItem.Click += new System.EventHandler(this.StashChangesToolStripMenuItemClick);
            // 
            // stashStagedToolStripMenuItem
            // 
            this.stashStagedToolStripMenuItem.Name = "stashStagedToolStripMenuItem";
            this.stashStagedToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.stashStagedToolStripMenuItem.Text = "S&tash staged";
            this.stashStagedToolStripMenuItem.ToolTipText = "Stash staged changes";
            this.stashStagedToolStripMenuItem.Click += new System.EventHandler(this.StashStagedToolStripMenuItemClick);
            // 
            // stashPopToolStripMenuItem
            // 
            this.stashPopToolStripMenuItem.Name = "stashPopToolStripMenuItem";
            this.stashPopToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.stashPopToolStripMenuItem.Text = "Stash &pop";
            this.stashPopToolStripMenuItem.ToolTipText = "Apply and drop single stash";
            this.stashPopToolStripMenuItem.Click += new System.EventHandler(this.StashPopToolStripMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(164, 6);
            // 
            // manageStashesToolStripMenuItem
            // 
            this.manageStashesToolStripMenuItem.Name = "manageStashesToolStripMenuItem";
            this.manageStashesToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.manageStashesToolStripMenuItem.Text = "&Manage stashes...";
            this.manageStashesToolStripMenuItem.ToolTipText = "Manage stashes";
            this.manageStashesToolStripMenuItem.Click += new System.EventHandler(this.ManageStashesToolStripMenuItemClick);
            // 
            // createAStashToolStripMenuItem
            // 
            this.createAStashToolStripMenuItem.Name = "createAStashToolStripMenuItem";
            this.createAStashToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.createAStashToolStripMenuItem.Text = "&Create a stash...";
            this.createAStashToolStripMenuItem.Click += new System.EventHandler(this.CreateStashToolStripMenuItemClick);
            // 
            // toolStripButtonCommit
            // 
            this.toolStripButtonCommit.Image = global::GitUI.Properties.Images.RepoStateClean;
            this.toolStripButtonCommit.ImageAlign = ContentAlignment.MiddleLeft;
            this.toolStripButtonCommit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCommit.Name = "toolStripButtonCommit";
            this.toolStripButtonCommit.Size = new System.Drawing.Size(71, 22);
            this.toolStripButtonCommit.Text = "Commit";
            this.toolStripButtonCommit.TextAlign = ContentAlignment.MiddleLeft;
            this.toolStripButtonCommit.Click += new System.EventHandler(this.CommitToolStripMenuItemClick);
            // 
            // toolStripButtonPull
            // 
            this.toolStripButtonPull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPull.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pullToolStripMenuItem1,
            toolStripSeparator11,
            this.mergeToolStripMenuItem,
            this.rebaseToolStripMenuItem1,
            this.fetchToolStripMenuItem,
            this.fetchAllToolStripMenuItem,
            this.fetchPruneAllToolStripMenuItem,
            toolStripSeparator14,
            this.setDefaultPullButtonActionToolStripMenuItem});
            this.toolStripButtonPull.Image = global::GitUI.Properties.Images.Pull;
            this.toolStripButtonPull.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPull.Name = "toolStripButtonPull";
            this.toolStripButtonPull.Size = new System.Drawing.Size(32, 22);
            this.toolStripButtonPull.Text = "Pull";
            this.toolStripButtonPull.ButtonClick += new System.EventHandler(this.ToolStripButtonPullClick);
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(236, 6);
            // 
            // mergeToolStripMenuItem
            // 
            this.mergeToolStripMenuItem.Image = global::GitUI.Properties.Images.PullMerge;
            this.mergeToolStripMenuItem.Name = "mergeToolStripMenuItem";
            this.mergeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.mergeToolStripMenuItem.Text = "Pull - &merge";
            this.mergeToolStripMenuItem.Click += new System.EventHandler(this.mergeToolStripMenuItem_Click);
            // 
            // rebaseToolStripMenuItem1
            // 
            this.rebaseToolStripMenuItem1.Image = global::GitUI.Properties.Images.PullRebase;
            this.rebaseToolStripMenuItem1.Name = "rebaseToolStripMenuItem1";
            this.rebaseToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this.rebaseToolStripMenuItem1.Text = "Pull - &rebase";
            this.rebaseToolStripMenuItem1.Click += new System.EventHandler(this.rebaseToolStripMenuItem1_Click);
            // 
            // fetchToolStripMenuItem
            // 
            this.fetchToolStripMenuItem.Image = global::GitUI.Properties.Images.PullFetch;
            this.fetchToolStripMenuItem.Name = "fetchToolStripMenuItem";
            this.fetchToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fetchToolStripMenuItem.Text = "&Fetch";
            this.fetchToolStripMenuItem.ToolTipText = "Fetch branches and tags";
            this.fetchToolStripMenuItem.Click += new System.EventHandler(this.fetchToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem1
            // 
            this.pullToolStripMenuItem1.Image = global::GitUI.Properties.Images.Pull;
            this.pullToolStripMenuItem1.Name = "pullToolStripMenuItem1";
            this.pullToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this.pullToolStripMenuItem1.Text = "Open &pull dialog...";
            this.pullToolStripMenuItem1.Click += new System.EventHandler(this.pullToolStripMenuItem1_Click);
            // 
            // fetchAllToolStripMenuItem
            // 
            this.fetchAllToolStripMenuItem.Image = global::GitUI.Properties.Images.PullFetchAll;
            this.fetchAllToolStripMenuItem.Name = "fetchAllToolStripMenuItem";
            this.fetchAllToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fetchAllToolStripMenuItem.Text = "Fetch &all";
            this.fetchAllToolStripMenuItem.ToolTipText = "Fetch branches and tags from all remote repositories";
            this.fetchAllToolStripMenuItem.Click += new System.EventHandler(this.fetchAllToolStripMenuItem_Click);
            // 
            // fetchPruneAllToolStripMenuItem
            // 
            this.fetchPruneAllToolStripMenuItem.Image = global::GitUI.Properties.Images.PullFetchPruneAll;
            this.fetchPruneAllToolStripMenuItem.Name = "fetchPruneAllToolStripMenuItem";
            this.fetchPruneAllToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fetchPruneAllToolStripMenuItem.Text = "F&etch and prune all";
            this.fetchPruneAllToolStripMenuItem.ToolTipText = "Fetch branches and tags from all remote repositories also prune deleted refs";
            this.fetchPruneAllToolStripMenuItem.Click += new System.EventHandler(this.fetchPruneAllToolStripMenuItem_Click);
            // 
            // setDefaultPullButtonActionToolStripMenuItem
            // 
            this.setDefaultPullButtonActionToolStripMenuItem.Name = "setDefaultPullButtonActionToolStripMenuItem";
            this.setDefaultPullButtonActionToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.setDefaultPullButtonActionToolStripMenuItem.Text = "Set &default Pull button action";
            // 
            // toolStripButtonPush
            // 
            this.toolStripButtonPush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPush.Image = global::GitUI.Properties.Images.Push;
            this.toolStripButtonPush.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPush.Name = "toolStripButtonPush";
            this.toolStripButtonPush.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPush.Text = "Push";
            this.toolStripButtonPush.Click += new System.EventHandler(this.ToolStripButtonPushClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripFileExplorer
            // 
            this.toolStripFileExplorer.Enabled = false;
            this.toolStripFileExplorer.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.toolStripFileExplorer.ImageTransparentColor = System.Drawing.Color.Gray;
            this.toolStripFileExplorer.Name = "toolStripFileExplorer";
            this.toolStripFileExplorer.Size = new System.Drawing.Size(23, 22);
            this.toolStripFileExplorer.ToolTipText = "File Explorer";
            this.toolStripFileExplorer.Click += new System.EventHandler(this.FileExplorerToolStripMenuItemClick);
            // 
            // userShell
            // 
            this.userShell.Image = global::GitUI.Properties.Images.GitForWindows;
            this.userShell.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.userShell.Name = "userShell";
            this.userShell.Size = new System.Drawing.Size(23, 22);
            this.userShell.ToolTipText = "Git bash";
            this.userShell.Click += new System.EventHandler(this.userShell_Click);
            // 
            // EditSettings
            // 
            this.EditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.EditSettings.Image = global::GitUI.Properties.Images.Settings;
            this.EditSettings.Name = "EditSettings";
            this.EditSettings.Size = new System.Drawing.Size(23, 22);
            this.EditSettings.ToolTipText = "Settings";
            this.EditSettings.Click += new System.EventHandler(this.OnShowSettingsClick);
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.repoObjectsTree);
            this.MainSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(1);
            this.MainSplitContainer.Panel1MinSize = 192;
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.RightSplitContainer);
            this.MainSplitContainer.Size = new System.Drawing.Size(923, 502);
            this.MainSplitContainer.SplitterWidth = 6;
            this.MainSplitContainer.TabIndex = 1;
            // 
            // repoObjectsTree
            // 
            this.repoObjectsTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.repoObjectsTree.Location = new System.Drawing.Point(0, 0);
            this.repoObjectsTree.MinimumSize = new System.Drawing.Size(190, 0);
            this.repoObjectsTree.Margin = new System.Windows.Forms.Padding(0);
            this.repoObjectsTree.Name = "repoObjectsTree";
            this.repoObjectsTree.Size = new System.Drawing.Size(267, 502);
            this.repoObjectsTree.TabIndex = 0;
            // 
            // RightSplitContainer
            // 
            this.RightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.RightSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.RightSplitContainer.Name = "RightSplitContainer";
            this.RightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // RightSplitContainer.Panel1
            // 
            this.RightSplitContainer.Panel1.Controls.Add(this.RevisionsSplitContainer);
            // 
            // RightSplitContainer.Panel2
            // 
            this.RightSplitContainer.Panel2.Controls.Add(this.CommitInfoTabControl);
            this.RightSplitContainer.Panel2MinSize = 0;
            this.RightSplitContainer.Size = new System.Drawing.Size(650, 502);
            this.RightSplitContainer.SplitterDistance = 209;
            this.RightSplitContainer.SplitterWidth = 6;
            this.RightSplitContainer.TabIndex = 1;
            this.RightSplitContainer.TabStop = false;
            // 
            // RevisionsSplitContainer
            // 
            this.RevisionsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.RevisionsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.RevisionsSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.RevisionsSplitContainer.Name = "RevisionsSplitContainer";
            this.RevisionsSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(1);
            this.RevisionsSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(1);
            // 
            // RevisionsSplitContainer.Panel1
            // 
            this.RevisionsSplitContainer.Size = new System.Drawing.Size(650, 209);
            this.RevisionsSplitContainer.SplitterDistance = 350;
            this.RevisionsSplitContainer.SplitterWidth = 6;
            this.RevisionsSplitContainer.Panel1.Controls.Add(this.RevisionGridContainer);
            this.RevisionsSplitContainer.TabIndex = 0;
            // 
            // RevisionGridContainer
            // 
            this.RevisionGridContainer.Controls.Add(this.RevisionGrid);
            this.RevisionGridContainer.Controls.Add(this.notificationBarBisectInProgress);
            this.RevisionGridContainer.Controls.Add(this.notificationBarGitActionInProgress);
            this.RevisionGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGridContainer.Location = new System.Drawing.Point(0, 0);
            this.RevisionGridContainer.Name = "RevisionGridContainer";
            this.RevisionGridContainer.Size = new System.Drawing.Size(606, 205);
            this.RevisionGridContainer.TabIndex = 2;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.Size = new System.Drawing.Size(350, 209);
            this.RevisionGrid.ShowBuildServerInfo = true;
            this.RevisionGrid.TabIndex = 2;
            // 
            // notificationBarBisectInProgress
            // 
            this.notificationBarBisectInProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.notificationBarBisectInProgress.Location = new System.Drawing.Point(0, 33);
            this.notificationBarBisectInProgress.MinimumSize = new System.Drawing.Size(0, 33);
            this.notificationBarBisectInProgress.Name = "notificationBarBisectInProgress";
            this.notificationBarBisectInProgress.Size = new System.Drawing.Size(561, 33);
            this.notificationBarBisectInProgress.TabIndex = 1;
            this.notificationBarBisectInProgress.Visible = false;
            // 
            // notificationBarGitActionInProgress
            // 
            this.notificationBarGitActionInProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.notificationBarGitActionInProgress.Location = new System.Drawing.Point(0, 0);
            this.notificationBarGitActionInProgress.MinimumSize = new System.Drawing.Size(0, 33);
            this.notificationBarGitActionInProgress.Name = "notificationBarGitActionInProgress";
            this.notificationBarGitActionInProgress.Size = new System.Drawing.Size(561, 33);
            this.notificationBarGitActionInProgress.TabIndex = 0;
            this.notificationBarGitActionInProgress.Visible = false;
            // 
            // CommitInfoTabControl
            // 
            this.CommitInfoTabControl.Controls.Add(this.CommitInfoTabPage);
            this.CommitInfoTabControl.Controls.Add(this.DiffTabPage);
            this.CommitInfoTabControl.Controls.Add(this.TreeTabPage);
            this.CommitInfoTabControl.Controls.Add(this.GpgInfoTabPage);
            this.CommitInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.CommitInfoTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.CommitInfoTabControl.Name = "CommitInfoTabControl";
            this.CommitInfoTabControl.Padding = new System.Drawing.Point(0, 0);
            this.CommitInfoTabControl.SelectedIndex = 0;
            this.CommitInfoTabControl.Size = new System.Drawing.Size(650, 287);
            this.CommitInfoTabControl.TabIndex = 0;
            this.CommitInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.CommitInfoTabControl_SelectedIndexChanged);
            // 
            // CommitInfoTabPage
            // 
            this.CommitInfoTabPage.Controls.Add(this.RevisionInfo);
            this.CommitInfoTabPage.Location = new System.Drawing.Point(1, 21);
            this.CommitInfoTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.CommitInfoTabPage.Name = "CommitInfoTabPage";
            this.CommitInfoTabPage.Size = new System.Drawing.Size(646, 264);
            this.CommitInfoTabPage.TabIndex = 2;
            this.CommitInfoTabPage.Text = "Commit";
            this.CommitInfoTabPage.UseVisualStyleBackColor = true;
            // 
            // RevisionInfo
            // 
            this.RevisionInfo.BackColor = System.Drawing.SystemColors.Window;
            this.RevisionInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RevisionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionInfo.Location = new System.Drawing.Point(0, 0);
            this.RevisionInfo.Margin = new System.Windows.Forms.Padding(0);
            this.RevisionInfo.Name = "RevisionInfo";
            this.RevisionInfo.ShowBranchesAsLinks = true;
            this.RevisionInfo.Size = new System.Drawing.Size(646, 264);
            this.RevisionInfo.TabIndex = 0;
            this.RevisionInfo.CommandClicked += new System.EventHandler<ResourceManager.CommandEventArgs>(this.RevisionInfo_CommandClicked);
            // 
            // TreeTabPage
            // 
            this.TreeTabPage.Controls.Add(this.fileTree);
            this.TreeTabPage.Location = new System.Drawing.Point(1, 21);
            this.TreeTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.TreeTabPage.Name = "TreeTabPage";
            this.TreeTabPage.Size = new System.Drawing.Size(646, 264);
            this.TreeTabPage.TabIndex = 0;
            this.TreeTabPage.Text = "File tree";
            this.TreeTabPage.UseVisualStyleBackColor = true;
            // 
            // fileTree
            // 
            this.fileTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTree.Location = new System.Drawing.Point(0, 0);
            this.fileTree.Margin = new System.Windows.Forms.Padding(0);
            this.fileTree.Name = "fileTree";
            this.fileTree.Size = new System.Drawing.Size(646, 264);
            this.fileTree.TabIndex = 0;
            // 
            // DiffTabPage
            // 
            this.DiffTabPage.Controls.Add(this.revisionDiff);
            this.DiffTabPage.Location = new System.Drawing.Point(1, 21);
            this.DiffTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.DiffTabPage.Name = "DiffTabPage";
            this.DiffTabPage.Size = new System.Drawing.Size(646, 264);
            this.DiffTabPage.TabIndex = 1;
            this.DiffTabPage.Text = "Diff";
            this.DiffTabPage.UseVisualStyleBackColor = true;
            // 
            // revisionDiff
            // 
            this.revisionDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.revisionDiff.Location = new System.Drawing.Point(0, 0);
            this.revisionDiff.Margin = new System.Windows.Forms.Padding(0);
            this.revisionDiff.Name = "revisionDiff";
            this.revisionDiff.Size = new System.Drawing.Size(646, 264);
            this.revisionDiff.TabIndex = 0;
            // 
            // GpgInfoTabPage
            // 
            this.GpgInfoTabPage.Controls.Add(this.revisionGpgInfo1);
            this.GpgInfoTabPage.Location = new System.Drawing.Point(1, 21);
            this.GpgInfoTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.GpgInfoTabPage.Name = "GpgInfoTabPage";
            this.GpgInfoTabPage.Size = new System.Drawing.Size(646, 264);
            this.GpgInfoTabPage.TabIndex = 3;
            this.GpgInfoTabPage.Text = "GPG";
            this.GpgInfoTabPage.UseVisualStyleBackColor = true;
            // 
            // revisionGpgInfo1
            // 
            this.revisionGpgInfo1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.revisionGpgInfo1.Location = new System.Drawing.Point(0, 0);
            this.revisionGpgInfo1.Margin = new System.Windows.Forms.Padding(0);
            this.revisionGpgInfo1.Name = "revisionGpgInfo1";
            this.revisionGpgInfo1.Padding = new System.Windows.Forms.Padding(6);
            this.revisionGpgInfo1.Size = new System.Drawing.Size(646, 264);
            this.revisionGpgInfo1.TabIndex = 0;
            // 
            // FilterToolTip
            // 
            this.FilterToolTip.AutomaticDelay = 100;
            this.FilterToolTip.ShowAlways = true;
            this.FilterToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.FilterToolTip.ToolTipTitle = "RegEx";
            this.FilterToolTip.UseAnimation = false;
            this.FilterToolTip.UseFading = false;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(43, 19);
            this.fileToolStripMenuItem.GitModuleChanged += new System.EventHandler<GitCommands.Git.GitModuleEventArgs>(this.SetGitModule);
            this.fileToolStripMenuItem.RecentRepositoriesCleared += new System.EventHandler(this.fileToolStripMenuItem_RecentRepositoriesCleared);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = global::GitUI.Properties.Images.DashboardFolderGit;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.closeToolStripMenuItem.Text = "&Close (go to Dashboard)";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItemClick);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::GitUI.Properties.Images.ReloadRevisions;
            this.refreshToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.refreshToolStripMenuItem.Text = "&Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // refreshDashboardToolStripMenuItem
            // 
            this.refreshDashboardToolStripMenuItem.Image = global::GitUI.Properties.Images.ReloadRevisions;
            this.refreshDashboardToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.refreshDashboardToolStripMenuItem.Name = "refreshDashboardToolStripMenuItem";
            this.refreshDashboardToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.refreshDashboardToolStripMenuItem.Text = "&Refresh";
            this.refreshDashboardToolStripMenuItem.Click += new System.EventHandler(this.RefreshDashboardToolStripMenuItemClick);
            // 
            // fileExplorerToolStripMenuItem
            // 
            this.fileExplorerToolStripMenuItem.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.fileExplorerToolStripMenuItem.Name = "fileExplorerToolStripMenuItem";
            this.fileExplorerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.fileExplorerToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.fileExplorerToolStripMenuItem.Text = "File E&xplorer";
            this.fileExplorerToolStripMenuItem.Click += new System.EventHandler(this.FileExplorerToolStripMenuItemClick);
            // 
            // repositoryToolStripMenuItem
            // 
            this.repositoryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.fileExplorerToolStripMenuItem,
            this.toolStripSeparator8,
            this.manageRemoteRepositoriesToolStripMenuItem1,
            this.toolStripSeparator43,
            this.manageSubmodulesToolStripMenuItem,
            this.updateAllSubmodulesToolStripMenuItem,
            this.synchronizeAllSubmodulesToolStripMenuItem,
            this.toolStripSeparator10,
            this.manageWorktreeToolStripMenuItem,
            this.toolStripSeparator44,
            this.editgitignoreToolStripMenuItem1,
            this.editgitinfoexcludeToolStripMenuItem,
            this.editGitAttributesToolStripMenuItem,
            this.editmailmapToolStripMenuItem,
            this.menuitemSparse,
            this.toolStripSeparator4,
            this.gitMaintenanceToolStripMenuItem,
            this.repoSettingsToolStripMenuItem,
            this.toolStripSeparator13,
            this.closeToolStripMenuItem});
            this.repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
            this.repositoryToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.repositoryToolStripMenuItem.Text = "&Repository";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(218, 6);
            // 
            // manageRemoteRepositoriesToolStripMenuItem1
            // 
            this.manageRemoteRepositoriesToolStripMenuItem1.Image = global::GitUI.Properties.Images.Remotes;
            this.manageRemoteRepositoriesToolStripMenuItem1.Name = "manageRemoteRepositoriesToolStripMenuItem1";
            this.manageRemoteRepositoriesToolStripMenuItem1.Size = new System.Drawing.Size(221, 22);
            this.manageRemoteRepositoriesToolStripMenuItem1.Text = "Remo&te repositories...";
            this.manageRemoteRepositoriesToolStripMenuItem1.Click += new System.EventHandler(this.ManageRemoteRepositoriesToolStripMenuItemClick);
            // 
            // toolStripSeparator43
            // 
            this.toolStripSeparator43.Name = "toolStripSeparator43";
            this.toolStripSeparator43.Size = new System.Drawing.Size(218, 6);
            // 
            // manageSubmodulesToolStripMenuItem
            // 
            this.manageSubmodulesToolStripMenuItem.Image = global::GitUI.Properties.Images.SubmodulesManage;
            this.manageSubmodulesToolStripMenuItem.Name = "manageSubmodulesToolStripMenuItem";
            this.manageSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.manageSubmodulesToolStripMenuItem.Text = "Manage &submodules...";
            this.manageSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.ManageSubmodulesToolStripMenuItemClick);
            // 
            // updateAllSubmodulesToolStripMenuItem
            // 
            this.updateAllSubmodulesToolStripMenuItem.Image = global::GitUI.Properties.Images.SubmodulesUpdate;
            this.updateAllSubmodulesToolStripMenuItem.Name = "updateAllSubmodulesToolStripMenuItem";
            this.updateAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.updateAllSubmodulesToolStripMenuItem.Text = "&Update all submodules";
            this.updateAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.UpdateAllSubmodulesToolStripMenuItemClick);
            // 
            // synchronizeAllSubmodulesToolStripMenuItem
            // 
            this.synchronizeAllSubmodulesToolStripMenuItem.Image = global::GitUI.Properties.Images.SubmodulesSync;
            this.synchronizeAllSubmodulesToolStripMenuItem.Name = "synchronizeAllSubmodulesToolStripMenuItem";
            this.synchronizeAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.synchronizeAllSubmodulesToolStripMenuItem.Text = "Synchronize all su&bmodules";
            this.synchronizeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.SynchronizeAllSubmodulesToolStripMenuItemClick);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(218, 6);
            // 
            // manageWorktreeToolStripMenuItem
            // 
            this.manageWorktreeToolStripMenuItem.Image = global::GitUI.Properties.Images.WorkTree;
            this.manageWorktreeToolStripMenuItem.Name = "manageWorktreeToolStripMenuItem";
            this.manageWorktreeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.manageWorktreeToolStripMenuItem.Text = "Manage &worktrees...";
            this.manageWorktreeToolStripMenuItem.Click += new System.EventHandler(this.manageWorktreeToolStripMenuItem_Click);
            // 
            // toolStripSeparator44
            // 
            this.toolStripSeparator44.Name = "toolStripSeparator44";
            this.toolStripSeparator44.Size = new System.Drawing.Size(218, 6);
            // 
            // editgitignoreToolStripMenuItem1
            // 
            this.editgitignoreToolStripMenuItem1.Image = global::GitUI.Properties.Images.EditGitIgnore;
            this.editgitignoreToolStripMenuItem1.Name = "editgitignoreToolStripMenuItem1";
            this.editgitignoreToolStripMenuItem1.Size = new System.Drawing.Size(221, 22);
            this.editgitignoreToolStripMenuItem1.Text = "Edit .git&ignore";
            this.editgitignoreToolStripMenuItem1.Click += new System.EventHandler(this.EditGitignoreToolStripMenuItem1Click);
            // 
            // editgitinfoexcludeToolStripMenuItem
            // 
            this.editgitinfoexcludeToolStripMenuItem.Name = "editgitinfoexcludeToolStripMenuItem";
            this.editgitinfoexcludeToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.editgitinfoexcludeToolStripMenuItem.Text = "Edit .git/info/&exclude";
            this.editgitinfoexcludeToolStripMenuItem.Click += new System.EventHandler(this.EditGitInfoExcludeToolStripMenuItemClick);
            // 
            // editGitAttributesToolStripMenuItem
            // 
            this.editGitAttributesToolStripMenuItem.Name = "editGitAttributesToolStripMenuItem";
            this.editGitAttributesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.editGitAttributesToolStripMenuItem.Text = "Edit .git&attributes";
            this.editGitAttributesToolStripMenuItem.Click += new System.EventHandler(this.editGitAttributesToolStripMenuItem_Click);
            // 
            // editmailmapToolStripMenuItem
            // 
            this.editmailmapToolStripMenuItem.Name = "editmailmapToolStripMenuItem";
            this.editmailmapToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.editmailmapToolStripMenuItem.Text = "Edit .&mailmap";
            this.editmailmapToolStripMenuItem.Click += new System.EventHandler(this.EditMailMapToolStripMenuItemClick);
            // 
            // menuitemSparse
            // 
            this.menuitemSparse.Name = "menuitemSparse";
            this.menuitemSparse.Size = new System.Drawing.Size(221, 22);
            this.menuitemSparse.Text = "Sparse Wor&king Copy";
            this.menuitemSparse.Click += new System.EventHandler(this.menuitemSparseWorkingCopy_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(218, 6);
            // 
            // gitMaintenanceToolStripMenuItem
            // 
            this.gitMaintenanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressGitDatabaseToolStripMenuItem,
            this.recoverLostObjectsToolStripMenuItem,
            this.deleteIndexLockToolStripMenuItem,
            this.editLocalGitConfigToolStripMenuItem});
            this.gitMaintenanceToolStripMenuItem.Image = global::GitUI.Properties.Images.Maintenance;
            this.gitMaintenanceToolStripMenuItem.Name = "gitMaintenanceToolStripMenuItem";
            this.gitMaintenanceToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.gitMaintenanceToolStripMenuItem.Text = "&Git maintenance";
            // 
            // compressGitDatabaseToolStripMenuItem
            // 
            this.compressGitDatabaseToolStripMenuItem.Image = global::GitUI.Properties.Images.CompressGitDatabase;
            this.compressGitDatabaseToolStripMenuItem.Name = "compressGitDatabaseToolStripMenuItem";
            this.compressGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.compressGitDatabaseToolStripMenuItem.Text = "&Compress git database";
            this.compressGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.CompressGitDatabaseToolStripMenuItemClick);
            // 
            // recoverLostObjectsToolStripMenuItem
            // 
            this.recoverLostObjectsToolStripMenuItem.Image = global::GitUI.Properties.Images.RecoverLostObjects;
            this.recoverLostObjectsToolStripMenuItem.Name = "recoverLostObjectsToolStripMenuItem";
            this.recoverLostObjectsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.recoverLostObjectsToolStripMenuItem.Text = "&Recover lost objects...";
            this.recoverLostObjectsToolStripMenuItem.Click += new System.EventHandler(this.recoverLostObjectsToolStripMenuItemClick);
            // 
            // deleteIndexLockToolStripMenuItem
            // 
            this.deleteIndexLockToolStripMenuItem.Image = global::GitUI.Properties.Images.DeleteIndexLock;
            this.deleteIndexLockToolStripMenuItem.Name = "deleteIndexLockToolStripMenuItem";
            this.deleteIndexLockToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.deleteIndexLockToolStripMenuItem.Text = "&Delete index.lock";
            this.deleteIndexLockToolStripMenuItem.Click += new System.EventHandler(this.deleteIndexLockToolStripMenuItem_Click);
            // 
            // editLocalGitConfigToolStripMenuItem
            // 
            this.editLocalGitConfigToolStripMenuItem.Image = global::GitUI.Properties.Images.EditGitConfig;
            this.editLocalGitConfigToolStripMenuItem.Name = "editLocalGitConfigToolStripMenuItem";
            this.editLocalGitConfigToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.editLocalGitConfigToolStripMenuItem.Text = "&Edit .git/config";
            this.editLocalGitConfigToolStripMenuItem.Click += new System.EventHandler(this.EditLocalGitConfigToolStripMenuItemClick);
            // 
            // repoSettingsToolStripMenuItem
            // 
            this.repoSettingsToolStripMenuItem.Image = global::GitUI.Properties.Images.Settings;
            this.repoSettingsToolStripMenuItem.Name = "repoSettingsToolStripMenuItem";
            this.repoSettingsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.repoSettingsToolStripMenuItem.Text = "Rep&ository settings";
            this.repoSettingsToolStripMenuItem.Click += new System.EventHandler(this.RepoSettingsToolStripMenuItemClick);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(218, 6);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitToolStripMenuItem,
            this.undoLastCommitToolStripMenuItem,
            this.pullToolStripMenuItem,
            this.pushToolStripMenuItem,
            this.toolStripSeparator21,
            this.stashToolStripMenuItem,
            this.resetToolStripMenuItem,
            this.cleanupToolStripMenuItem,
            this.toolStripSeparator25,
            this.branchToolStripMenuItem,
            this.deleteBranchToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.mergeBranchToolStripMenuItem,
            this.rebaseToolStripMenuItem,
            this.runMergetoolToolStripMenuItem,
            this.toolStripSeparator45,
            this.tagToolStripMenuItem,
            this.deleteTagToolStripMenuItem,
            this.toolStripSeparator23,
            this.cherryPickToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.checkoutToolStripMenuItem,
            this.bisectToolStripMenuItem,
            this.toolStripMenuItemReflog,
            this.toolStripSeparator22,
            this.formatPatchToolStripMenuItem,
            this.applyPatchToolStripMenuItem,
            this.patchToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "&Commands";
            // 
            // commitToolStripMenuItem
            // 
            this.commitToolStripMenuItem.Image = global::GitUI.Properties.Images.RepoStateClean;
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            this.commitToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.commitToolStripMenuItem.Text = "&Commit...";
            this.commitToolStripMenuItem.Click += new System.EventHandler(this.CommitToolStripMenuItemClick);
            // 
            // undoLastCommitToolStripMenuItem
            // 
            this.undoLastCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetFileTo;
            this.undoLastCommitToolStripMenuItem.Name = "undoLastCommitToolStripMenuItem";
            this.undoLastCommitToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.undoLastCommitToolStripMenuItem.Text = "&Undo last commit...";
            this.undoLastCommitToolStripMenuItem.Click += new System.EventHandler(this.undoLastCommitToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.Image = global::GitUI.Properties.Images.Pull;
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.pullToolStripMenuItem.Text = "Pull&/Fetch...";
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.PullToolStripMenuItemClick);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Image = global::GitUI.Properties.Images.Push;
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.pushToolStripMenuItem.Text = "&Push...";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.PushToolStripMenuItemClick);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(210, 6);
            // 
            // stashToolStripMenuItem
            // 
            this.stashToolStripMenuItem.Image = global::GitUI.Properties.Images.Stash;
            this.stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            this.stashToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.stashToolStripMenuItem.Text = "Ma&nage stashes...";
            this.stashToolStripMenuItem.Click += new System.EventHandler(this.StashToolStripMenuItemClick);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetWorkingDirChanges;
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.resetToolStripMenuItem.Text = "&Reset changes...";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.ResetToolStripMenuItem_Click);
            // 
            // cleanupToolStripMenuItem
            // 
            this.cleanupToolStripMenuItem.Image = global::GitUI.Properties.Images.CleanupRepo;
            this.cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
            this.cleanupToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.cleanupToolStripMenuItem.Text = "Clean &working directory...";
            this.cleanupToolStripMenuItem.Click += new System.EventHandler(this.CleanupToolStripMenuItemClick);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(210, 6);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchCreate;
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.branchToolStripMenuItem.Text = "Create &branch...";
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.CreateBranchToolStripMenuItemClick);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchDelete;
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.deleteBranchToolStripMenuItem.Text = "De&lete branch...";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.DeleteBranchToolStripMenuItemClick);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Chec&kout branch...";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.CheckoutBranchToolStripMenuItemClick);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.Merge;
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.mergeBranchToolStripMenuItem.Text = "&Merge branches...";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.MergeBranchToolStripMenuItemClick);
            // 
            // rebaseToolStripMenuItem
            // 
            this.rebaseToolStripMenuItem.Image = global::GitUI.Properties.Images.Rebase;
            this.rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            this.rebaseToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.rebaseToolStripMenuItem.Text = "R&ebase...";
            this.rebaseToolStripMenuItem.Click += new System.EventHandler(this.RebaseToolStripMenuItemClick);
            // 
            // runMergetoolToolStripMenuItem
            // 
            this.runMergetoolToolStripMenuItem.Image = global::GitUI.Properties.Images.Conflict;
            this.runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            this.runMergetoolToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.runMergetoolToolStripMenuItem.Text = "&Solve merge conflicts...";
            this.runMergetoolToolStripMenuItem.Click += new System.EventHandler(this.RunMergetoolToolStripMenuItemClick);
            // 
            // toolStripSeparator45
            // 
            this.toolStripSeparator45.Name = "toolStripSeparator45";
            this.toolStripSeparator45.Size = new System.Drawing.Size(210, 6);
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.Image = global::GitUI.Properties.Images.TagCreate;
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.tagToolStripMenuItem.Text = "Create &tag...";
            this.tagToolStripMenuItem.Click += new System.EventHandler(this.TagToolStripMenuItemClick);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Image = global::GitUI.Properties.Images.TagDelete;
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.deleteTagToolStripMenuItem.Text = "&Delete tag...";
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.DeleteTagToolStripMenuItemClick);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(210, 6);
            // 
            // cherryPickToolStripMenuItem
            // 
            this.cherryPickToolStripMenuItem.Image = global::GitUI.Properties.Images.CherryPick;
            this.cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            this.cherryPickToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.cherryPickToolStripMenuItem.Text = "Cherr&y pick...";
            this.cherryPickToolStripMenuItem.Click += new System.EventHandler(this.CherryPickToolStripMenuItemClick);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.Image = global::GitUI.Properties.Images.ArchiveRevision;
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.archiveToolStripMenuItem.Text = "Archi&ve revision...";
            this.archiveToolStripMenuItem.Click += new System.EventHandler(this.ArchiveToolStripMenuItemClick);
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.Image = global::GitUI.Properties.Images.Checkout;
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.checkoutToolStripMenuItem.Text = "Check&out revision...";
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.CheckoutToolStripMenuItemClick);
            // 
            // bisectToolStripMenuItem
            // 
            this.bisectToolStripMenuItem.Image = global::GitUI.Properties.Images.Bisect;
            this.bisectToolStripMenuItem.Name = "bisectToolStripMenuItem";
            this.bisectToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.bisectToolStripMenuItem.Text = "B&isect...";
            this.bisectToolStripMenuItem.Click += new System.EventHandler(this.BisectClick);
            // 
            // toolStripMenuItemReflog
            // 
            this.toolStripMenuItemReflog.Image = global::GitUI.Properties.Images.Book;
            this.toolStripMenuItemReflog.Name = "toolStripMenuItemReflog";
            this.toolStripMenuItemReflog.Size = new System.Drawing.Size(213, 22);
            this.toolStripMenuItemReflog.Text = "Show reflo&g...";
            this.toolStripMenuItemReflog.Click += new System.EventHandler(this.toolStripMenuItemReflog_Click);
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(210, 6);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.Image = global::GitUI.Properties.Images.PatchFormat;
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.formatPatchToolStripMenuItem.Text = "&Format patch...";
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.FormatPatchToolStripMenuItemClick);
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.Image = global::GitUI.Properties.Images.PatchApply;
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.applyPatchToolStripMenuItem.Text = "&Apply patch...";
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.ApplyPatchToolStripMenuItemClick);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.Image = global::GitUI.Properties.Images.PatchView;
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.patchToolStripMenuItem.Text = "View patc&h file...";
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.PatchToolStripMenuItemClick);
            // 
            // toolStripSeparator46
            // 
            this.toolStripSeparator46.Name = "toolStripSeparator46";
            this.toolStripSeparator46.Size = new System.Drawing.Size(268, 6);
            // 
            // toolStripSeparator42
            // 
            this.toolStripSeparator42.Name = "toolStripSeparator42";
            this.toolStripSeparator42.Size = new System.Drawing.Size(110, 6);
            // 
            // _repositoryHostsToolStripMenuItem
            // 
            this._repositoryHostsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._forkCloneRepositoryToolStripMenuItem,
            this._viewPullRequestsToolStripMenuItem,
            this._createPullRequestsToolStripMenuItem,
            this._addUpstreamRemoteToolStripMenuItem});
            this._repositoryHostsToolStripMenuItem.Name = "_repositoryHostsToolStripMenuItem";
            this._repositoryHostsToolStripMenuItem.Size = new System.Drawing.Size(114, 20);
            this._repositoryHostsToolStripMenuItem.Text = "(Repository hosts)";
            // 
            // _forkCloneRepositoryToolStripMenuItem
            // 
            this._forkCloneRepositoryToolStripMenuItem.Name = "_forkCloneRepositoryToolStripMenuItem";
            this._forkCloneRepositoryToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this._forkCloneRepositoryToolStripMenuItem.Text = "&Fork/Clone repository...";
            this._forkCloneRepositoryToolStripMenuItem.Click += new System.EventHandler(this._forkCloneMenuItem_Click);
            // 
            // _viewPullRequestsToolStripMenuItem
            // 
            this._viewPullRequestsToolStripMenuItem.Name = "_viewPullRequestsToolStripMenuItem";
            this._viewPullRequestsToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this._viewPullRequestsToolStripMenuItem.Text = "View &pull requests...";
            this._viewPullRequestsToolStripMenuItem.Click += new System.EventHandler(this._viewPullRequestsToolStripMenuItem_Click);
            // 
            // _createPullRequestsToolStripMenuItem
            // 
            this._createPullRequestsToolStripMenuItem.Name = "_createPullRequestsToolStripMenuItem";
            this._createPullRequestsToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this._createPullRequestsToolStripMenuItem.Text = "&Create pull requests...";
            this._createPullRequestsToolStripMenuItem.Click += new System.EventHandler(this._createPullRequestToolStripMenuItem_Click);
            // 
            // dashboardToolStripMenuItem
            // 
            this.dashboardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshDashboardToolStripMenuItem,
            this.toolStripSeparator42});
            this.dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            this.dashboardToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.dashboardToolStripMenuItem.Text = "&Dashboard";
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator15,
            this.pluginSettingsToolStripMenuItem});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "&Plugins";
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(150, 6);
            // 
            // pluginSettingsToolStripMenuItem
            // 
            this.pluginSettingsToolStripMenuItem.Image = global::GitUI.Properties.Images.Settings;
            this.pluginSettingsToolStripMenuItem.Name = "pluginSettingsToolStripMenuItem";
            this.pluginSettingsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.pluginSettingsToolStripMenuItem.Text = "Plugin &Settings";
            this.pluginSettingsToolStripMenuItem.Click += new System.EventHandler(this.PluginSettingsToolStripMenuItemClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 19);
            this.toolsToolStripMenuItem.Text = "&Tools";
            this.toolsToolStripMenuItem.SettingsChanged += new System.EventHandler<GitUI.CommandsDialogs.Menus.SettingsChangedEventArgs>(this.toolsToolStripMenuItem_SettingsChanged);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ClickThrough = true;
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dashboardToolStripMenuItem,
            this.repositoryToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this._repositoryHostsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(923, 24);
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(4);
            this.mainMenuStrip.TabIndex = 0;
            // 
            // toolPanel
            // 
            this.toolPanel.BottomToolStripPanelVisible = false;
            // 
            // toolPanel.ContentPanel
            // 
            this.toolPanel.ContentPanel.Controls.Add(this.MainSplitContainer);
            this.toolPanel.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.toolPanel.ContentPanel.Padding = new System.Windows.Forms.Padding(6);
            this.toolPanel.ContentPanel.Size = new System.Drawing.Size(1846, 1023);
            this.toolPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolPanel.LeftToolStripPanelVisible = false;
            this.toolPanel.Location = new System.Drawing.Point(0, 24);
            this.toolPanel.Margin = new System.Windows.Forms.Padding(0);
            this.toolPanel.Name = "toolPanel";
            this.toolPanel.Padding = new System.Windows.Forms.Padding(0);
            this.toolPanel.TopToolStripPanel.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.toolPanel.RightToolStripPanelVisible = false;
            this.toolPanel.Size = new System.Drawing.Size(923, 527);
            this.toolPanel.TabIndex = 1;
            // 
            // toolPanel.TopToolStripPanel
            // 
            this.toolPanel.TopToolStripPanel.Controls.Add(this.ToolStripMain);
            this.toolPanel.TopToolStripPanel.Controls.Add(this.ToolStripFilters);
            this.toolPanel.TopToolStripPanel.Controls.Add(this.ToolStripScripts);
            // 
            // ToolStripFilters
            // 
            this.ToolStripFilters.Name = "ToolStripFilters";
            this.ToolStripFilters.TabIndex = 1;
            this.ToolStripFilters.Text = "Filters";
            // 
            // addUpstreamRemoteToolStripMenuItem
            // 
            this._addUpstreamRemoteToolStripMenuItem.Name = "_addUpstreamRemoteToolStripMenuItem";
            this._addUpstreamRemoteToolStripMenuItem.Size = new System.Drawing.Size(360, 38);
            this._addUpstreamRemoteToolStripMenuItem.Text = "&Add upstream remote";
            this._addUpstreamRemoteToolStripMenuItem.Click += new System.EventHandler(this._addUpstreamRemoteToolStripMenuItem_Click);
            // 
            // toolStripScripts
            // 
            this.ToolStripScripts.ClickThrough = true;
            this.ToolStripScripts.Dock = System.Windows.Forms.DockStyle.None;
            this.ToolStripScripts.DrawBorder = false;
            this.ToolStripScripts.GripEnabled = false;
            this.ToolStripScripts.GripMargin = new System.Windows.Forms.Padding(0);
            this.ToolStripScripts.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolStripScripts.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStripScripts.Location = new System.Drawing.Point(890, 0);
            this.ToolStripScripts.Name = "ToolStripScripts";
            this.ToolStripScripts.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStripScripts.Size = new System.Drawing.Size(43, 25);
            this.ToolStripScripts.TabIndex = 2;
            this.ToolStripScripts.Text = "Scripts";
            // 
            // FormBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(923, 573);
            this.Controls.Add(this.toolPanel);
            this.Controls.Add(this.mainMenuStrip);
            this.Name = "FormBrowse";
            this.Text = "Git Extensions";
            this.ToolStripMain.ResumeLayout(false);
            this.ToolStripMain.PerformLayout();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.RightSplitContainer.Panel1.ResumeLayout(false);
            this.RightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RightSplitContainer)).EndInit();
            this.RightSplitContainer.ResumeLayout(false);
            this.RevisionsSplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RevisionsSplitContainer)).EndInit();
            this.RevisionsSplitContainer.ResumeLayout(false);
            this.RevisionGridContainer.ResumeLayout(false);
            this.CommitInfoTabControl.ResumeLayout(false);
            this.CommitInfoTabPage.ResumeLayout(false);
            this.TreeTabPage.ResumeLayout(false);
            this.DiffTabPage.ResumeLayout(false);
            this.GpgInfoTabPage.ResumeLayout(false);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.toolPanel.ContentPanel.ResumeLayout(false);
            this.toolPanel.TopToolStripPanel.ResumeLayout(false);
            this.toolPanel.TopToolStripPanel.PerformLayout();
            this.toolPanel.ResumeLayout(false);
            this.toolPanel.PerformLayout();
            this.ToolStripFilters.ResumeLayout(false);
            this.ToolStripFilters.PerformLayout();
            this.ToolStripScripts.ResumeLayout(false);
            this.ToolStripScripts.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        internal SplitContainer MainSplitContainer;
        private SplitContainer RightSplitContainer;
        private SplitContainer RevisionsSplitContainer;

        private FullBleedTabControl CommitInfoTabControl;
        private TabPage CommitInfoTabPage;
        private TabPage DiffTabPage;
        private TabPage TreeTabPage;
        private TabPage GpgInfoTabPage;

        private BindingSource gitRevisionBindingSource;
        private BindingSource gitItemBindingSource;
        private GitUI.RevisionGridControl RevisionGrid;
        private CommitInfo.CommitInfo RevisionInfo;
        private GitUI.BranchTreePanel.RepoObjectsTree repoObjectsTree;
        private ToolTip FilterToolTip;
        private RevisionFileTreeControl fileTree;
        private RevisionDiffControl revisionDiff;
        private ToolStripContainer toolPanel;
        private RevisionGpgInfoControl revisionGpgInfo1;

        private MenuStripEx mainMenuStrip;
        private ToolStripEx ToolStripMain;
        private GitUI.UserControls.FilterToolBar ToolStripFilters;
        private ToolStripEx ToolStripScripts;

        private ToolStripButton toolStripButtonCommit;
        private ToolStripSplitButton _NO_TRANSLATE_WorkingDir;
        private ToolStripSeparator toolStripSeparator0;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSplitButton userShell;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton EditSettings;
        private ToolStripButton RefreshButton;
        private ToolStripPushButton toolStripButtonPush;
        private ToolStripSplitButton toolStripSplitStash;
        private ToolStripMenuItem stashChangesToolStripMenuItem;
        private ToolStripMenuItem stashStagedToolStripMenuItem;
        private ToolStripMenuItem stashPopToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem manageStashesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripSplitButton branchSelect;
        private ToolStripButton toggleBranchTreePanel;
        private ToolStripButton toggleSplitViewLayout;
        private GitUI.CommandsDialogs.Menus.StartToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem refreshToolStripMenuItem;
        private ToolStripMenuItem refreshDashboardToolStripMenuItem;
        private ToolStripMenuItem fileExplorerToolStripMenuItem;
        private ToolStripMenuItem repositoryToolStripMenuItem;
        private ToolStripMenuItem commandsToolStripMenuItem;
        private ToolStripMenuItem applyPatchToolStripMenuItem;
        private ToolStripMenuItem archiveToolStripMenuItem;
        private ToolStripMenuItem bisectToolStripMenuItem;
        private ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private ToolStripMenuItem checkoutToolStripMenuItem;
        private ToolStripMenuItem cherryPickToolStripMenuItem;
        private ToolStripMenuItem cleanupToolStripMenuItem;
        private ToolStripMenuItem commitToolStripMenuItem;
        private ToolStripMenuItem branchToolStripMenuItem;
        private ToolStripMenuItem tagToolStripMenuItem;
        private ToolStripMenuItem deleteBranchToolStripMenuItem;
        private ToolStripMenuItem deleteTagToolStripMenuItem;
        private ToolStripMenuItem formatPatchToolStripMenuItem;
        private ToolStripMenuItem mergeBranchToolStripMenuItem;
        private ToolStripMenuItem pullToolStripMenuItem;
        private ToolStripMenuItem pushToolStripMenuItem;
        private ToolStripMenuItem rebaseToolStripMenuItem;
        private ToolStripMenuItem runMergetoolToolStripMenuItem;
        private ToolStripMenuItem stashToolStripMenuItem;
        private ToolStripMenuItem patchToolStripMenuItem;
        private ToolStripMenuItem manageRemoteRepositoriesToolStripMenuItem1;
        private ToolStripMenuItem _repositoryHostsToolStripMenuItem;
        private ToolStripMenuItem _forkCloneRepositoryToolStripMenuItem;
        private ToolStripMenuItem _viewPullRequestsToolStripMenuItem;
        private ToolStripMenuItem _createPullRequestsToolStripMenuItem;
        private ToolStripMenuItem _addUpstreamRemoteToolStripMenuItem;
        private ToolStripMenuItem dashboardToolStripMenuItem;
        private ToolStripMenuItem manageSubmodulesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem updateAllSubmodulesToolStripMenuItem;
        private ToolStripMenuItem synchronizeAllSubmodulesToolStripMenuItem;
        private ToolStripMenuItem pluginsToolStripMenuItem;
        private ToolStripMenuItem pluginSettingsToolStripMenuItem;
        private ToolStripMenuItem repoSettingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem gitMaintenanceToolStripMenuItem;
        private ToolStripMenuItem editLocalGitConfigToolStripMenuItem;
        private ToolStripMenuItem compressGitDatabaseToolStripMenuItem;
        private ToolStripMenuItem recoverLostObjectsToolStripMenuItem;
        private ToolStripMenuItem deleteIndexLockToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem editgitignoreToolStripMenuItem1;
        private ToolStripMenuItem editGitAttributesToolStripMenuItem;
        private ToolStripMenuItem editmailmapToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private GitUI.CommandsDialogs.Menus.HelpToolStripMenuItem helpToolStripMenuItem;
        private GitUI.CommandsDialogs.Menus.ToolsToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator21;
        private ToolStripSeparator toolStripSeparator25;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripSeparator toolStripSeparator23;
        private ToolStripSplitButton toolStripButtonLevelUp;
        private ToolStripSplitButton toolStripButtonPull;
        private ToolStripMenuItem mergeToolStripMenuItem;
        private ToolStripMenuItem rebaseToolStripMenuItem1;
        private ToolStripMenuItem fetchToolStripMenuItem;
        private ToolStripMenuItem pullToolStripMenuItem1;
        private ToolStripMenuItem setDefaultPullButtonActionToolStripMenuItem;
        private ToolStripMenuItem fetchAllToolStripMenuItem;
        private ToolStripMenuItem fetchPruneAllToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator42;
        private ToolStripSeparator toolStripSeparator43;
        private ToolStripSeparator toolStripSeparator44;
        private ToolStripSeparator toolStripSeparator45;
        private ToolStripSeparator toolStripSeparator46;
        private ToolStripMenuItem menuitemSparse;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem editgitinfoexcludeToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItemReflog;
        private ToolStripMenuItem manageWorktreeToolStripMenuItem;
        private ToolStripButton toolStripFileExplorer;
        private ToolStripMenuItem createAStashToolStripMenuItem;
        private ToolStripMenuItem undoLastCommitToolStripMenuItem;
        private ToolStripSplitButton menuCommitInfoPosition;
        private ToolStripMenuItem commitInfoBelowMenuItem;
        private ToolStripMenuItem commitInfoLeftwardMenuItem;
        private ToolStripMenuItem commitInfoRightwardMenuItem;
        private Panel RevisionGridContainer;
        private UserControls.InteractiveGitActionControl notificationBarBisectInProgress;
        private UserControls.InteractiveGitActionControl notificationBarGitActionInProgress;
    }
}
