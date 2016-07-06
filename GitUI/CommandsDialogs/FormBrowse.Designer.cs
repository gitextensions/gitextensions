using System.Windows.Forms;
using GitUI.Editor;

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
            this.toolPanel = new System.Windows.Forms.SplitContainer();
            this.UserMenuToolStrip = new GitUI.ToolStripEx();
            this.ToolStrip = new GitUI.ToolStripEx();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonLevelUp = new System.Windows.Forms.ToolStripSplitButton();
            this._NO_TRANSLATE_Workingdir = new System.Windows.Forms.ToolStripSplitButton();
            this.branchSelect = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitStash = new System.Windows.Forms.ToolStripSplitButton();
            this.stashChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashPopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.viewStashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPull = new System.Windows.Forms.ToolStripSplitButton();
            this.mergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.pullToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setNextPullActionAsDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonPush = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.GitBash = new System.Windows.Forms.ToolStripButton();
            this.EditSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripBranchFilterComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripBranchFilterDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripRevisionFilterLabel = new System.Windows.Forms.ToolStripLabel();
            this.toggleSplitViewLayout = new System.Windows.Forms.ToolStripButton();
            this.toolStripRevisionFilterTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.ShowFirstParent = new System.Windows.Forms.ToolStripButton();
            this.toolStripRevisionFilterDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.CommitInfoTabControl = new System.Windows.Forms.TabControl();
            this.CommitInfoTabPage = new System.Windows.Forms.TabPage();
            this.RevisionInfo = new GitUI.CommitInfo.CommitInfo();
            this.TreeTabPage = new System.Windows.Forms.TabPage();
            this.FileTreeSplitContainer = new System.Windows.Forms.SplitContainer();
            this.GitTree = new System.Windows.Forms.TreeView();
            this.FileTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToThisRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator30 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFilenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeOpenContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeCleanWorkingTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator31 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.editCheckedOutFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileText = new GitUI.Editor.FileViewer();
            this.DiffTabPage = new System.Windows.Forms.TabPage();
            this.DiffSplitContainer = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parentOfALocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parentOfBLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToFirstToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToSecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToParentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickSelectedDiffFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFilenameToClipboardToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffShowInFileTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator33 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryDiffToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findInDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.FilterToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.TreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneSVNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStripSeparator44 = new System.Windows.Forms.ToolStripSeparator();
            this.editgitignoreToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editgitattributesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editmailmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSparse = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.gitMaintenanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verifyGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteIndexlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editLocalGitConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repoSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.gitBashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitGUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kGitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.formatPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
            this.SvnFetchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SvnRebaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SvnDcommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator46 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator41 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator42 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.PuTTYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAuthenticationAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateOrImportKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._repositoryHostsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._forkCloneRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._viewPullRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._createPullRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.pluginSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changelogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.translateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportAnIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitcountPerUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitcommandLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.menuStrip1 = new GitUI.MenuStripEx();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.toolStripMenuItemReflog = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.toolPanel)).BeginInit();
            this.toolPanel.Panel1.SuspendLayout();
            this.toolPanel.Panel2.SuspendLayout();
            this.toolPanel.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.CommitInfoTabControl.SuspendLayout();
            this.CommitInfoTabPage.SuspendLayout();
            this.TreeTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FileTreeSplitContainer)).BeginInit();
            this.FileTreeSplitContainer.Panel1.SuspendLayout();
            this.FileTreeSplitContainer.Panel2.SuspendLayout();
            this.FileTreeSplitContainer.SuspendLayout();
            this.FileTreeContextMenu.SuspendLayout();
            this.DiffTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DiffSplitContainer)).BeginInit();
            this.DiffSplitContainer.Panel1.SuspendLayout();
            this.DiffSplitContainer.Panel2.SuspendLayout();
            this.DiffSplitContainer.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.TreeContextMenu.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // toolPanel
            // 
            this.toolPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.toolPanel.IsSplitterFixed = true;
            this.toolPanel.Location = new System.Drawing.Point(0, 24);
            this.toolPanel.Name = "toolPanel";
            this.toolPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // toolPanel.Panel1
            // 
            this.toolPanel.Panel1.Controls.Add(this.UserMenuToolStrip);
            this.toolPanel.Panel1.Controls.Add(this.ToolStrip);
            // 
            // toolPanel.Panel2
            // 
            this.toolPanel.Panel2.Controls.Add(this.MainSplitContainer);
            this.toolPanel.Size = new System.Drawing.Size(923, 527);
            this.toolPanel.SplitterDistance = 25;
            this.toolPanel.SplitterWidth = 1;
            this.toolPanel.TabIndex = 4;
            // 
            // UserMenuToolStrip
            // 
            this.UserMenuToolStrip.ClickThrough = true;
            this.UserMenuToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.UserMenuToolStrip.Location = new System.Drawing.Point(836, 24);
            this.UserMenuToolStrip.Name = "UserMenuToolStrip";
            this.UserMenuToolStrip.Size = new System.Drawing.Size(111, 25);
            this.UserMenuToolStrip.TabIndex = 5;
            this.UserMenuToolStrip.Visible = false;
            // 
            // ToolStrip
            // 
            this.ToolStrip.ClickThrough = true;
            this.ToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshButton,
            this.toolStripSeparator17,
            this.toolStripButtonLevelUp,
            this._NO_TRANSLATE_Workingdir,
            this.branchSelect,
            this.toolStripSeparator1,
            this.toolStripSplitStash,
            this.toolStripButton1,
            this.toolStripButtonPull,
            this.toolStripButtonPush,
            this.toolStripSeparator2,
            this.GitBash,
            this.EditSettings,
            this.toolStripSeparator5,
            this.toolStripLabel1,
            this.toolStripBranchFilterComboBox,
            this.toolStripBranchFilterDropDownButton,
            this.toolStripSeparator19,
            this.toolStripRevisionFilterLabel,
            this.toggleSplitViewLayout,
            this.toolStripRevisionFilterTextBox,
            this.toolStripRevisionFilterDropDownButton,
            this.ShowFirstParent});
            this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStrip.Size = new System.Drawing.Size(923, 25);
            this.ToolStrip.TabIndex = 4;
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.White;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.ToolTipText = "Refresh";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButtonClick);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonLevelUp
            // 
            this.toolStripButtonLevelUp.AutoToolTip = false;
            this.toolStripButtonLevelUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLevelUp.Image = global::GitUI.Properties.Resources.levelUp;
            this.toolStripButtonLevelUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLevelUp.Name = "toolStripButtonLevelUp";
            this.toolStripButtonLevelUp.Size = new System.Drawing.Size(32, 22);
            this.toolStripButtonLevelUp.Text = "Go to superproject";
            this.toolStripButtonLevelUp.ToolTipText = "Go to superproject";
            this.toolStripButtonLevelUp.ButtonClick += new System.EventHandler(this.toolStripButtonLevelUp_ButtonClick);
            this.toolStripButtonLevelUp.DropDownOpening += new System.EventHandler(this.toolStripButtonLevelUp_DropDownOpening);
            // 
            // _NO_TRANSLATE_Workingdir
            // 
            this._NO_TRANSLATE_Workingdir.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._NO_TRANSLATE_Workingdir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._NO_TRANSLATE_Workingdir.Name = "_NO_TRANSLATE_Workingdir";
            this._NO_TRANSLATE_Workingdir.Size = new System.Drawing.Size(83, 22);
            this._NO_TRANSLATE_Workingdir.Text = "WorkingDir";
            this._NO_TRANSLATE_Workingdir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._NO_TRANSLATE_Workingdir.ToolTipText = "Change working directory";
            this._NO_TRANSLATE_Workingdir.ButtonClick += new System.EventHandler(this.WorkingdirClick);
            this._NO_TRANSLATE_Workingdir.DropDownOpening += new System.EventHandler(this.WorkingdirDropDownOpening);
            this._NO_TRANSLATE_Workingdir.MouseUp += new System.Windows.Forms.MouseEventHandler(this._NO_TRANSLATE_Workingdir_MouseUp);
            // 
            // branchSelect
            // 
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
            this.stashPopToolStripMenuItem,
            this.toolStripSeparator9,
            this.viewStashToolStripMenuItem});
            this.toolStripSplitStash.Image = global::GitUI.Properties.Resources.stash;
            this.toolStripSplitStash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitStash.Name = "toolStripSplitStash";
            this.toolStripSplitStash.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitStash.ToolTipText = "Stash changes";
            this.toolStripSplitStash.ButtonClick += new System.EventHandler(this.ToolStripSplitStashButtonClick);
            // 
            // stashChangesToolStripMenuItem
            // 
            this.stashChangesToolStripMenuItem.Name = "stashChangesToolStripMenuItem";
            this.stashChangesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.stashChangesToolStripMenuItem.Text = "Stash";
            this.stashChangesToolStripMenuItem.ToolTipText = "Stash changes";
            this.stashChangesToolStripMenuItem.Click += new System.EventHandler(this.StashChangesToolStripMenuItemClick);
            // 
            // stashPopToolStripMenuItem
            // 
            this.stashPopToolStripMenuItem.Name = "stashPopToolStripMenuItem";
            this.stashPopToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.stashPopToolStripMenuItem.Text = "Stash pop";
            this.stashPopToolStripMenuItem.ToolTipText = "Apply and drop single stash";
            this.stashPopToolStripMenuItem.Click += new System.EventHandler(this.StashPopToolStripMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(126, 6);
            // 
            // viewStashToolStripMenuItem
            // 
            this.viewStashToolStripMenuItem.Name = "viewStashToolStripMenuItem";
            this.viewStashToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.viewStashToolStripMenuItem.Text = "View stash";
            this.viewStashToolStripMenuItem.ToolTipText = "View stash";
            this.viewStashToolStripMenuItem.Click += new System.EventHandler(this.ViewStashToolStripMenuItemClick);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = global::GitUI.Properties.Resources.IconClean;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(71, 22);
            this.toolStripButton1.Text = "Commit";
            this.toolStripButton1.Click += new System.EventHandler(this.ToolStripButton1Click);
            // 
            // toolStripButtonPull
            // 
            this.toolStripButtonPull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPull.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mergeToolStripMenuItem,
            this.rebaseToolStripMenuItem1,
            this.fetchToolStripMenuItem,
            this.toolStripSeparator14,
            this.pullToolStripMenuItem1,
            this.fetchAllToolStripMenuItem,
            this.setNextPullActionAsDefaultToolStripMenuItem});
            this.toolStripButtonPull.Image = global::GitUI.Properties.Resources.Icon_4;
            this.toolStripButtonPull.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPull.Name = "toolStripButtonPull";
            this.toolStripButtonPull.Size = new System.Drawing.Size(32, 22);
            this.toolStripButtonPull.Text = "Pull";
            this.toolStripButtonPull.ButtonClick += new System.EventHandler(this.ToolStripButtonPullClick);
            this.toolStripButtonPull.DropDownOpened += new System.EventHandler(this.toolStripButtonPull_DropDownOpened);
            // 
            // mergeToolStripMenuItem
            // 
            this.mergeToolStripMenuItem.Image = global::GitUI.Properties.Resources.PullMerge;
            this.mergeToolStripMenuItem.Name = "mergeToolStripMenuItem";
            this.mergeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.mergeToolStripMenuItem.Text = "Pull - merge";
            this.mergeToolStripMenuItem.Click += new System.EventHandler(this.mergeToolStripMenuItem_Click);
            // 
            // rebaseToolStripMenuItem1
            // 
            this.rebaseToolStripMenuItem1.Image = global::GitUI.Properties.Resources.PullRebase;
            this.rebaseToolStripMenuItem1.Name = "rebaseToolStripMenuItem1";
            this.rebaseToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this.rebaseToolStripMenuItem1.Text = "Pull - rebase";
            this.rebaseToolStripMenuItem1.Click += new System.EventHandler(this.rebaseToolStripMenuItem1_Click);
            // 
            // fetchToolStripMenuItem
            // 
            this.fetchToolStripMenuItem.Image = global::GitUI.Properties.Resources.PullFetch;
            this.fetchToolStripMenuItem.Name = "fetchToolStripMenuItem";
            this.fetchToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fetchToolStripMenuItem.Text = "Fetch";
            this.fetchToolStripMenuItem.Click += new System.EventHandler(this.fetchToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(236, 6);
            // 
            // pullToolStripMenuItem1
            // 
            this.pullToolStripMenuItem1.Image = global::GitUI.Properties.Resources.Icon_4;
            this.pullToolStripMenuItem1.Name = "pullToolStripMenuItem1";
            this.pullToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this.pullToolStripMenuItem1.Text = "Open pull dialog...";
            this.pullToolStripMenuItem1.Click += new System.EventHandler(this.pullToolStripMenuItem1_Click);
            // 
            // fetchAllToolStripMenuItem
            // 
            this.fetchAllToolStripMenuItem.Image = global::GitUI.Properties.Resources.PullFetchAll;
            this.fetchAllToolStripMenuItem.Name = "fetchAllToolStripMenuItem";
            this.fetchAllToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fetchAllToolStripMenuItem.Text = "Fetch all";
            this.fetchAllToolStripMenuItem.Click += new System.EventHandler(this.fetchAllToolStripMenuItem_Click);
            // 
            // setNextPullActionAsDefaultToolStripMenuItem
            // 
            this.setNextPullActionAsDefaultToolStripMenuItem.Checked = true;
            this.setNextPullActionAsDefaultToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.setNextPullActionAsDefaultToolStripMenuItem.Name = "setNextPullActionAsDefaultToolStripMenuItem";
            this.setNextPullActionAsDefaultToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.setNextPullActionAsDefaultToolStripMenuItem.Text = "Set the next selection as default";
            this.setNextPullActionAsDefaultToolStripMenuItem.Click += new System.EventHandler(this.dontSetAsDefaultToolStripMenuItem_Click);
            // 
            // toolStripButtonPush
            // 
            this.toolStripButtonPush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPush.Image = global::GitUI.Properties.Resources.Icon_3;
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
            // GitBash
            // 
            this.GitBash.Image = global::GitUI.Properties.Resources.bash;
            this.GitBash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GitBash.Name = "GitBash";
            this.GitBash.Size = new System.Drawing.Size(23, 22);
            this.GitBash.ToolTipText = "Git bash";
            this.GitBash.Click += new System.EventHandler(this.GitBashClick);
            // 
            // EditSettings
            // 
            this.EditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.EditSettings.Image = global::GitUI.Properties.Resources.Icon_71;
            this.EditSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.EditSettings.Name = "EditSettings";
            this.EditSettings.Size = new System.Drawing.Size(23, 22);
            this.EditSettings.ToolTipText = "Settings";
            this.EditSettings.Click += new System.EventHandler(this.SettingsClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel1.Text = "Branches:";
            // 
            // toolStripBranchFilterComboBox
            // 
            this.toolStripBranchFilterComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.toolStripBranchFilterComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.toolStripBranchFilterComboBox.AutoSize = false;
            this.toolStripBranchFilterComboBox.DropDownWidth = 300;
            this.toolStripBranchFilterComboBox.Name = "toolStripBranchFilterComboBox";
            this.toolStripBranchFilterComboBox.Size = new System.Drawing.Size(150, 23);
            // 
            // toolStripBranchFilterDropDownButton
            // 
            this.toolStripBranchFilterDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBranchFilterDropDownButton.Image = global::GitUI.Properties.Resources.Icon_77;
            this.toolStripBranchFilterDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripBranchFilterDropDownButton.Name = "toolStripBranchFilterDropDownButton";
            this.toolStripBranchFilterDropDownButton.Size = new System.Drawing.Size(29, 22);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripRevisionFilterLabel
            // 
            this.toolStripRevisionFilterLabel.Name = "toolStripRevisionFilterLabel";
            this.toolStripRevisionFilterLabel.Size = new System.Drawing.Size(36, 22);
            this.toolStripRevisionFilterLabel.Text = "Filter:";
            // 
            // toggleSplitViewLayout
            // 
            this.toggleSplitViewLayout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toggleSplitViewLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleSplitViewLayout.Image = global::GitUI.Properties.Resources.SplitViewLayout;
            this.toggleSplitViewLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleSplitViewLayout.Name = "toggleSplitViewLayout";
            this.toggleSplitViewLayout.Size = new System.Drawing.Size(23, 22);
            this.toggleSplitViewLayout.ToolTipText = "Toggle split view layout";
            this.toggleSplitViewLayout.Click += new System.EventHandler(this.toggleSplitViewLayout_Click);
            // 
            // toolStripRevisionFilterTextBox
            // 
            this.toolStripRevisionFilterTextBox.ForeColor = System.Drawing.Color.Black;
            this.toolStripRevisionFilterTextBox.Name = "toolStripRevisionFilterTextBox";
            this.toolStripRevisionFilterTextBox.Size = new System.Drawing.Size(97, 25);
            // 
            // ShowFirstParent
            // 
            this.ShowFirstParent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowFirstParent.Image = global::GitUI.Properties.Resources.IconShowFirstParent;
            this.ShowFirstParent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowFirstParent.Name = "ShowFirstParent";
            this.ShowFirstParent.Size = new System.Drawing.Size(23, 22);
            this.ShowFirstParent.ToolTipText = "Show first parents";
            // 
            // toolStripRevisionFilterDropDownButton
            // 
            this.toolStripRevisionFilterDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRevisionFilterDropDownButton.Image = global::GitUI.Properties.Resources.Icon_77;
            this.toolStripRevisionFilterDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRevisionFilterDropDownButton.Name = "toolStripRevisionFilterDropDownButton";
            this.toolStripRevisionFilterDropDownButton.Size = new System.Drawing.Size(29, 22);
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.RevisionGrid);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.CommitInfoTabControl);
            this.MainSplitContainer.Panel2MinSize = 0;
            this.MainSplitContainer.Size = new System.Drawing.Size(923, 501);
            this.MainSplitContainer.SplitterDistance = 208;
            this.MainSplitContainer.TabIndex = 1;
            this.MainSplitContainer.TabStop = false;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.RevisionGraphDrawStyle = GitUI.RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
            this.RevisionGrid.ShowUncommitedChangesIfPossible = true;
            this.RevisionGrid.Size = new System.Drawing.Size(923, 208);
            this.RevisionGrid.TabIndex = 0;
            // 
            // CommitInfoTabControl
            // 
            this.CommitInfoTabControl.Controls.Add(this.CommitInfoTabPage);
            this.CommitInfoTabControl.Controls.Add(this.TreeTabPage);
            this.CommitInfoTabControl.Controls.Add(this.DiffTabPage);
            this.CommitInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.CommitInfoTabControl.Name = "CommitInfoTabControl";
            this.CommitInfoTabControl.SelectedIndex = 0;
            this.CommitInfoTabControl.Size = new System.Drawing.Size(923, 289);
            this.CommitInfoTabControl.TabIndex = 0;
            this.CommitInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // CommitInfoTabPage
            // 
            this.CommitInfoTabPage.Controls.Add(this.RevisionInfo);
            this.CommitInfoTabPage.Location = new System.Drawing.Point(4, 22);
            this.CommitInfoTabPage.Margin = new System.Windows.Forms.Padding(15);
            this.CommitInfoTabPage.Name = "CommitInfoTabPage";
            this.CommitInfoTabPage.Size = new System.Drawing.Size(915, 263);
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
            this.RevisionInfo.Margin = new System.Windows.Forms.Padding(10);
            this.RevisionInfo.Name = "RevisionInfo";
            this.RevisionInfo.ShowBranchesAsLinks = true;
            this.RevisionInfo.Size = new System.Drawing.Size(915, 263);
            this.RevisionInfo.TabIndex = 1;
            this.RevisionInfo.CommandClick += new System.EventHandler<GitUI.CommitInfo.CommandEventArgs>(this.RevisionInfo_CommandClick);
            // 
            // TreeTabPage
            // 
            this.TreeTabPage.Controls.Add(this.FileTreeSplitContainer);
            this.TreeTabPage.Location = new System.Drawing.Point(4, 22);
            this.TreeTabPage.Name = "TreeTabPage";
            this.TreeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.TreeTabPage.Size = new System.Drawing.Size(915, 263);
            this.TreeTabPage.TabIndex = 0;
            this.TreeTabPage.Text = "File tree";
            this.TreeTabPage.UseVisualStyleBackColor = true;
            // 
            // FileTreeSplitContainer
            // 
            this.FileTreeSplitContainer.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::GitUI.Properties.Settings.Default, "FormBrowse_FileTreeSplitContainer_SplitterDistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FileTreeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileTreeSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.FileTreeSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.FileTreeSplitContainer.Name = "FileTreeSplitContainer";
            // 
            // FileTreeSplitContainer.Panel1
            // 
            this.FileTreeSplitContainer.Panel1.Controls.Add(this.GitTree);
            // 
            // FileTreeSplitContainer.Panel2
            // 
            this.FileTreeSplitContainer.Panel2.Controls.Add(this.FileText);
            this.FileTreeSplitContainer.Size = new System.Drawing.Size(909, 257);
            this.FileTreeSplitContainer.SplitterDistance = global::GitUI.Properties.Settings.Default.FormBrowse_FileTreeSplitContainer_SplitterDistance;
            this.FileTreeSplitContainer.TabIndex = 1;
            // 
            // GitTree
            // 
            this.GitTree.ContextMenuStrip = this.FileTreeContextMenu;
            this.GitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitTree.HideSelection = false;
            this.GitTree.Location = new System.Drawing.Point(0, 0);
            this.GitTree.Name = "GitTree";
            this.GitTree.Size = new System.Drawing.Size(215, 257);
            this.GitTree.TabIndex = 0;
            this.GitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTreeBeforeExpand);
            this.GitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.GitTree_AfterSelect);
            this.GitTree.DoubleClick += new System.EventHandler(this.GitTreeDoubleClick);
            this.GitTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GitTreeKeyDown);
            this.GitTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GitTreeMouseDown);
            // 
            // FileTreeContextMenu
            // 
            this.FileTreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.resetToThisRevisionToolStripMenuItem,
            this.toolStripSeparator30,
            this.copyFilenameToClipboardToolStripMenuItem,
            this.fileTreeOpenContainingFolderToolStripMenuItem,
            this.fileTreeArchiveToolStripMenuItem,
            this.fileTreeCleanWorkingTreeToolStripMenuItem,
            this.toolStripSeparator31,
            this.fileHistoryToolStripMenuItem,
            this.blameToolStripMenuItem1,
            this.findToolStripMenuItem,
            this.toolStripSeparator20,
            this.editCheckedOutFileToolStripMenuItem,
            this.openFileToolStripMenuItem,
            this.openFileWithToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.toolStripSeparator18,
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem});
            this.FileTreeContextMenu.Name = "FileTreeContextMenu";
            this.FileTreeContextMenu.Size = new System.Drawing.Size(297, 358);
            this.FileTreeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.FileTreeContextMenu_Opening);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsOnClick);
            // 
            // resetToThisRevisionToolStripMenuItem
            // 
            this.resetToThisRevisionToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetFileTo;
            this.resetToThisRevisionToolStripMenuItem.Name = "resetToThisRevisionToolStripMenuItem";
            this.resetToThisRevisionToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.resetToThisRevisionToolStripMenuItem.Text = "Reset to selected revision";
            this.resetToThisRevisionToolStripMenuItem.Click += new System.EventHandler(this.ResetToThisRevisionOnClick);
            // 
            // toolStripSeparator30
            // 
            this.toolStripSeparator30.Name = "toolStripSeparator30";
            this.toolStripSeparator30.Size = new System.Drawing.Size(293, 6);
            // 
            // copyFilenameToClipboardToolStripMenuItem
            // 
            this.copyFilenameToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem.Name = "copyFilenameToClipboardToolStripMenuItem";
            this.copyFilenameToClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.copyFilenameToClipboardToolStripMenuItem.Text = "Copy full path";
            this.copyFilenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem_Click);
            // 
            // fileTreeOpenContainingFolderToolStripMenuItem
            // 
            this.fileTreeOpenContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.fileTreeOpenContainingFolderToolStripMenuItem.Name = "fileTreeOpenContainingFolderToolStripMenuItem";
            this.fileTreeOpenContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileTreeOpenContainingFolderToolStripMenuItem.Text = "Open containing folder";
            this.fileTreeOpenContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.fileTreeOpenContainingFolderToolStripMenuItem_Click);
            // 
            // fileTreeArchiveToolStripMenuItem
            // 
            this.fileTreeArchiveToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconArchiveRevision;
            this.fileTreeArchiveToolStripMenuItem.Name = "fileTreeArchiveToolStripMenuItem";
            this.fileTreeArchiveToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileTreeArchiveToolStripMenuItem.Text = "Archive...";
            this.fileTreeArchiveToolStripMenuItem.Click += new System.EventHandler(this.fileTreeArchiveToolStripMenuItem_Click);
            // 
            // fileTreeCleanWorkingTreeToolStripMenuItem
            // 
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCleanupRepo;
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Name = "fileTreeCleanWorkingTreeToolStripMenuItem";
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Text = "Clean working directory...";
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Click += new System.EventHandler(this.fileTreeCleanWorkingTreeToolStripMenuItem_Click);
            // 
            // toolStripSeparator31
            // 
            this.toolStripSeparator31.Name = "toolStripSeparator31";
            this.toolStripSeparator31.Size = new System.Drawing.Size(293, 6);
            // 
            // fileHistoryToolStripMenuItem
            // 
            this.fileHistoryToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            this.fileHistoryToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileHistoryToolStripMenuItem.Text = "File history";
            this.fileHistoryToolStripMenuItem.Click += new System.EventHandler(this.fileHistoryItem_Click);
            // 
            // blameToolStripMenuItem1
            // 
            this.blameToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconBlame;
            this.blameToolStripMenuItem1.Name = "blameToolStripMenuItem1";
            this.blameToolStripMenuItem1.Size = new System.Drawing.Size(296, 22);
            this.blameToolStripMenuItem1.Text = "Blame";
            this.blameToolStripMenuItem1.Click += new System.EventHandler(this.blameMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFind;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.FindFileOnClick);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(293, 6);
            // 
            // editCheckedOutFileToolStripMenuItem
            // 
            this.editCheckedOutFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconEditFile;
            this.editCheckedOutFileToolStripMenuItem.Name = "editCheckedOutFileToolStripMenuItem";
            this.editCheckedOutFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.editCheckedOutFileToolStripMenuItem.Text = "Edit working directory file";
            this.editCheckedOutFileToolStripMenuItem.Click += new System.EventHandler(this.editCheckedOutFileToolStripMenuItem_Click);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openFileToolStripMenuItem.Text = "Open this revision (temp file)";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.OpenOnClick);
            // 
            // openFileWithToolStripMenuItem
            // 
            this.openFileWithToolStripMenuItem.Name = "openFileWithToolStripMenuItem";
            this.openFileWithToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openFileWithToolStripMenuItem.Text = "Open this revision with... (temp file)";
            this.openFileWithToolStripMenuItem.Click += new System.EventHandler(this.OpenWithOnClick);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openWithToolStripMenuItem.Text = "Open working directory file with...";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(293, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.expandAllToolStripMenuItem.Text = "Expand all (takes a while on large trees)";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse all";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.Location = new System.Drawing.Point(0, 0);
            this.FileText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FileText.Name = "FileText";
            this.FileText.Size = new System.Drawing.Size(690, 257);
            this.FileText.TabIndex = 0;
            // 
            // DiffTabPage
            // 
            this.DiffTabPage.Controls.Add(this.DiffSplitContainer);
            this.DiffTabPage.Location = new System.Drawing.Point(4, 22);
            this.DiffTabPage.Name = "DiffTabPage";
            this.DiffTabPage.Size = new System.Drawing.Size(915, 263);
            this.DiffTabPage.TabIndex = 1;
            this.DiffTabPage.Text = "Diff";
            this.DiffTabPage.UseVisualStyleBackColor = true;
            // 
            // DiffSplitContainer
            // 
            this.DiffSplitContainer.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::GitUI.Properties.Settings.Default, "FormBrowse_DiffSplitContainer_SplitterDistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DiffSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.DiffSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.DiffSplitContainer.Name = "DiffSplitContainer";
            // 
            // DiffSplitContainer.Panel1
            // 
            this.DiffSplitContainer.Panel1.Controls.Add(this.DiffFiles);
            // 
            // DiffSplitContainer.Panel2
            // 
            this.DiffSplitContainer.Panel2.Controls.Add(this.DiffText);
            this.DiffSplitContainer.Size = new System.Drawing.Size(915, 263);
            this.DiffSplitContainer.SplitterDistance = global::GitUI.Properties.Settings.Default.FormBrowse_DiffSplitContainer_SplitterDistance;
            this.DiffSplitContainer.TabIndex = 0;
            // 
            // DiffFiles
            // 
            this.DiffFiles.ContextMenuStrip = this.DiffContextMenu;
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(215, 263);
            this.DiffFiles.TabIndex = 1;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFilesSelectedIndexChanged);
            this.DiffFiles.DataSourceChanged += new System.EventHandler(this.DiffFiles_DataSourceChanged);
            this.DiffFiles.DoubleClick += new System.EventHandler(this.DiffFilesDoubleClick);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.saveAsToolStripMenuItem1,
            this.resetFileToToolStripMenuItem,
            this.cherryPickSelectedDiffFileToolStripMenuItem,
            this.toolStripSeparator32,
            this.copyFilenameToClipboardToolStripMenuItem1,
            this.openContainingFolderToolStripMenuItem,
            this.diffShowInFileTreeToolStripMenuItem,
            this.toolStripSeparator33,
            this.fileHistoryDiffToolstripMenuItem,
            this.blameToolStripMenuItem,
            this.findInDiffToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(211, 236);
            this.DiffContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.DiffContextMenu_Opening);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aBToolStripMenuItem,
            this.aLocalToolStripMenuItem,
            this.bLocalToolStripMenuItem,
            this.parentOfALocalToolStripMenuItem,
            this.parentOfBLocalToolStripMenuItem});
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconDiffTool;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.DropDownOpening += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_DropDownOpening);
            // 
            // aBToolStripMenuItem
            // 
            this.aBToolStripMenuItem.Name = "aBToolStripMenuItem";
            this.aBToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.aBToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.aBToolStripMenuItem.Text = "A <--> B";
            this.aBToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // aLocalToolStripMenuItem
            // 
            this.aLocalToolStripMenuItem.Name = "aLocalToolStripMenuItem";
            this.aLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.aLocalToolStripMenuItem.Text = "A <--> Working directory";
            this.aLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // bLocalToolStripMenuItem
            // 
            this.bLocalToolStripMenuItem.Name = "bLocalToolStripMenuItem";
            this.bLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.bLocalToolStripMenuItem.Text = "B <--> Working directory";
            this.bLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // parentOfALocalToolStripMenuItem
            // 
            this.parentOfALocalToolStripMenuItem.Name = "parentOfALocalToolStripMenuItem";
            this.parentOfALocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.parentOfALocalToolStripMenuItem.Text = "A\'s parent <--> Working directory";
            this.parentOfALocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // parentOfBLocalToolStripMenuItem
            // 
            this.parentOfBLocalToolStripMenuItem.Name = "parentOfBLocalToolStripMenuItem";
            this.parentOfBLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.parentOfBLocalToolStripMenuItem.Text = "B\'s parent <--> Working directory";
            this.parentOfBLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem1
            // 
            this.saveAsToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            this.saveAsToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.saveAsToolStripMenuItem1.Text = "Save (B) as...";
            this.saveAsToolStripMenuItem1.Click += new System.EventHandler(this.saveAsToolStripMenuItem1_Click);
            // 
            // resetFileToToolStripMenuItem
            // 
            this.resetFileToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetFileToFirstToolStripMenuItem,
            this.resetFileToSecondToolStripMenuItem,
            this.resetFileToSelectedToolStripMenuItem,
            this.resetFileToParentToolStripMenuItem});
            this.resetFileToToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetFileTo;
            this.resetFileToToolStripMenuItem.Name = "resetFileToToolStripMenuItem";
            this.resetFileToToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.resetFileToToolStripMenuItem.Text = "Reset file(s) to";
            this.resetFileToToolStripMenuItem.DropDownOpening += new System.EventHandler(this.resetFileToToolStripMenuItem_DropDownOpening);
            // 
            // resetFileToFirstToolStripMenuItem
            // 
            this.resetFileToFirstToolStripMenuItem.Name = "resetFileToFirstToolStripMenuItem";
            this.resetFileToFirstToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToFirstToolStripMenuItem.Text = "First";
            this.resetFileToFirstToolStripMenuItem.Click += new System.EventHandler(this.resetFileToFirstToolStripMenuItem_Click);
            // 
            // resetFileToSecondToolStripMenuItem
            // 
            this.resetFileToSecondToolStripMenuItem.Name = "resetFileToSecondToolStripMenuItem";
            this.resetFileToSecondToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToSecondToolStripMenuItem.Text = "Second";
            this.resetFileToSecondToolStripMenuItem.Click += new System.EventHandler(this.resetFileToSecondToolStripMenuItem_Click);
            // 
            // resetFileToSelectedToolStripMenuItem
            // 
            this.resetFileToSelectedToolStripMenuItem.Name = "resetFileToSelectedToolStripMenuItem";
            this.resetFileToSelectedToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToSelectedToolStripMenuItem.Text = "Selected";
            this.resetFileToSelectedToolStripMenuItem.Click += new System.EventHandler(this.resetFileToSelectedToolStripMenuItem_Click);
            // 
            // resetFileToParentToolStripMenuItem
            // 
            this.resetFileToParentToolStripMenuItem.Name = "resetFileToParentToolStripMenuItem";
            this.resetFileToParentToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToParentToolStripMenuItem.Text = "Parent";
            this.resetFileToParentToolStripMenuItem.Click += new System.EventHandler(this.resetFileToParentToolStripMenuItem_Click);
            // 
            // cherryPickSelectedDiffFileToolStripMenuItem
            // 
            this.cherryPickSelectedDiffFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCherryPick;
            this.cherryPickSelectedDiffFileToolStripMenuItem.Name = "cherryPickSelectedDiffFileToolStripMenuItem";
            this.cherryPickSelectedDiffFileToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.cherryPickSelectedDiffFileToolStripMenuItem.Text = "Cherry pick file\'s changes";
            this.cherryPickSelectedDiffFileToolStripMenuItem.Click += new System.EventHandler(this.cherryPickSelectedDiffFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(207, 6);
            // 
            // copyFilenameToClipboardToolStripMenuItem1
            // 
            this.copyFilenameToClipboardToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem1.Name = "copyFilenameToClipboardToolStripMenuItem1";
            this.copyFilenameToClipboardToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.copyFilenameToClipboardToolStripMenuItem1.Text = "Copy full path(s)";
            this.copyFilenameToClipboardToolStripMenuItem1.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem1_Click);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open containing folder(s)";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // diffShowInFileTreeToolStripMenuItem
            // 
            this.diffShowInFileTreeToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFileTree;
            this.diffShowInFileTreeToolStripMenuItem.Name = "diffShowInFileTreeToolStripMenuItem";
            this.diffShowInFileTreeToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.diffShowInFileTreeToolStripMenuItem.Text = "Show in File tree";
            this.diffShowInFileTreeToolStripMenuItem.Click += new System.EventHandler(this.diffShowInFileTreeToolStripMenuItem_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(207, 6);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            this.fileHistoryDiffToolstripMenuItem.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            this.fileHistoryDiffToolstripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.fileHistoryDiffToolstripMenuItem.Text = "File history";
            this.fileHistoryDiffToolstripMenuItem.Click += new System.EventHandler(this.fileHistoryDiffToolstripMenuItem_Click);
            // 
            // blameToolStripMenuItem
            // 
            this.blameToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBlame;
            this.blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            this.blameToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.blameToolStripMenuItem.Text = "Blame";
            this.blameToolStripMenuItem.Click += new System.EventHandler(this.blameToolStripMenuItem_Click);
            // 
            // findInDiffToolStripMenuItem
            // 
            this.findInDiffToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFind;
            this.findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            this.findInDiffToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findInDiffToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.findInDiffToolStripMenuItem.Text = "Find";
            this.findInDiffToolStripMenuItem.Click += new System.EventHandler(this.findInDiffToolStripMenuItem_Click);
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffText.Name = "DiffText";
            this.DiffText.Size = new System.Drawing.Size(696, 263);
            this.DiffText.TabIndex = 0;
            // 
            // FilterToolTip
            // 
            this.FilterToolTip.AutomaticDelay = 0;
            this.FilterToolTip.ShowAlways = true;
            this.FilterToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.FilterToolTip.ToolTipTitle = "RegEx";
            this.FilterToolTip.UseAnimation = false;
            this.FilterToolTip.UseFading = false;
            // 
            // TreeContextMenu
            // 
            this.TreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.TreeContextMenu.Name = "TreeContextMenu";
            this.TreeContextMenu.Size = new System.Drawing.Size(99, 26);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 551);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.statusStrip.Size = new System.Drawing.Size(923, 22);
            this.statusStrip.TabIndex = 4;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(14, 17);
            this.toolStripStatusLabel1.Text = "X";
            this.toolStripStatusLabel1.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.initNewRepositoryToolStripMenuItem,
            this.openToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.toolStripSeparator12,
            this.cloneToolStripMenuItem,
            this.cloneSVNToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.fileToolStripMenuItem.Text = "Start";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.FileToolStripMenuItemDropDownOpening);
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRepoCreate;
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "Create new repository...";
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.InitNewRepositoryToolStripMenuItemClick);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRepoOpen;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.recentToolStripMenuItem.Image = global::GitUI.Properties.Resources.RecentRepositories;
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.recentToolStripMenuItem.Text = "Recent Repositories";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(83, 22);
            this.toolStripMenuItem2.Text = "...";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(195, 6);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCloneRepoGit;
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.cloneToolStripMenuItem.Text = "Clone repository...";
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.CloneToolStripMenuItemClick);
            // 
            // cloneSVNToolStripMenuItem
            // 
            this.cloneSVNToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCloneRepoSvn;
            this.cloneSVNToolStripMenuItem.Name = "cloneSVNToolStripMenuItem";
            this.cloneSVNToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.cloneSVNToolStripMenuItem.Text = "Clone SVN repository...";
            this.cloneSVNToolStripMenuItem.Click += new System.EventHandler(this.CloneSvnToolStripMenuItemClick);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(195, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.closeToolStripMenuItem.Text = "Close (go to Dashboard)";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItemClick);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.refreshToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // refreshDashboardToolStripMenuItem
            // 
            this.refreshDashboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.refreshDashboardToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.refreshDashboardToolStripMenuItem.Name = "refreshDashboardToolStripMenuItem";
            this.refreshDashboardToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.refreshDashboardToolStripMenuItem.Text = "Refresh";
            this.refreshDashboardToolStripMenuItem.Click += new System.EventHandler(this.RefreshDashboardToolStripMenuItemClick);
            // 
            // fileExplorerToolStripMenuItem
            // 
            this.fileExplorerToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.fileExplorerToolStripMenuItem.Name = "fileExplorerToolStripMenuItem";
            this.fileExplorerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.fileExplorerToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.fileExplorerToolStripMenuItem.Text = "File Explorer";
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
            this.toolStripSeparator44,
            this.editgitignoreToolStripMenuItem1,
            this.editgitattributesToolStripMenuItem,
            this.editmailmapToolStripMenuItem,
            this.menuitemSparse,
            this.toolStripSeparator4,
            this.gitMaintenanceToolStripMenuItem,
            this.repoSettingsToolStripMenuItem,
            this.toolStripSeparator13,
            this.closeToolStripMenuItem});
            this.repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
            this.repositoryToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.repositoryToolStripMenuItem.Text = "Repository";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(218, 6);
            // 
            // manageRemoteRepositoriesToolStripMenuItem1
            // 
            this.manageRemoteRepositoriesToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconRemotes;
            this.manageRemoteRepositoriesToolStripMenuItem1.Name = "manageRemoteRepositoriesToolStripMenuItem1";
            this.manageRemoteRepositoriesToolStripMenuItem1.Size = new System.Drawing.Size(221, 22);
            this.manageRemoteRepositoriesToolStripMenuItem1.Text = "Remote repositories...";
            this.manageRemoteRepositoriesToolStripMenuItem1.Click += new System.EventHandler(this.ManageRemoteRepositoriesToolStripMenuItemClick);
            // 
            // toolStripSeparator43
            // 
            this.toolStripSeparator43.Name = "toolStripSeparator43";
            this.toolStripSeparator43.Size = new System.Drawing.Size(218, 6);
            // 
            // manageSubmodulesToolStripMenuItem
            // 
            this.manageSubmodulesToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconSubmodulesManage;
            this.manageSubmodulesToolStripMenuItem.Name = "manageSubmodulesToolStripMenuItem";
            this.manageSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.manageSubmodulesToolStripMenuItem.Text = "Submodules...";
            this.manageSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.ManageSubmodulesToolStripMenuItemClick);
            // 
            // updateAllSubmodulesToolStripMenuItem
            // 
            this.updateAllSubmodulesToolStripMenuItem.Name = "updateAllSubmodulesToolStripMenuItem";
            this.updateAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.updateAllSubmodulesToolStripMenuItem.Text = "Update all submodules";
            this.updateAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.UpdateAllSubmodulesToolStripMenuItemClick);
            // 
            // synchronizeAllSubmodulesToolStripMenuItem
            // 
            this.synchronizeAllSubmodulesToolStripMenuItem.Name = "synchronizeAllSubmodulesToolStripMenuItem";
            this.synchronizeAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.synchronizeAllSubmodulesToolStripMenuItem.Text = "Synchronize all submodules";
            this.synchronizeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.SynchronizeAllSubmodulesToolStripMenuItemClick);
            // 
            // toolStripSeparator44
            // 
            this.toolStripSeparator44.Name = "toolStripSeparator44";
            this.toolStripSeparator44.Size = new System.Drawing.Size(218, 6);
            // 
            // editgitignoreToolStripMenuItem1
            // 
            this.editgitignoreToolStripMenuItem1.Name = "editgitignoreToolStripMenuItem1";
            this.editgitignoreToolStripMenuItem1.Size = new System.Drawing.Size(221, 22);
            this.editgitignoreToolStripMenuItem1.Text = "Edit .gitignore";
            this.editgitignoreToolStripMenuItem1.Click += new System.EventHandler(this.EditGitignoreToolStripMenuItem1Click);
            // 
            // editgitattributesToolStripMenuItem
            // 
            this.editgitattributesToolStripMenuItem.Name = "editgitattributesToolStripMenuItem";
            this.editgitattributesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.editgitattributesToolStripMenuItem.Text = "Edit .gitattributes";
            this.editgitattributesToolStripMenuItem.Click += new System.EventHandler(this.editgitattributesToolStripMenuItem_Click);
            // 
            // editmailmapToolStripMenuItem
            // 
            this.editmailmapToolStripMenuItem.Name = "editmailmapToolStripMenuItem";
            this.editmailmapToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.editmailmapToolStripMenuItem.Text = "Edit .mailmap";
            this.editmailmapToolStripMenuItem.Click += new System.EventHandler(this.EditMailMapToolStripMenuItemClick);
            // 
            // menuitemSparse
            // 
            this.menuitemSparse.Name = "menuitemSparse";
            this.menuitemSparse.Size = new System.Drawing.Size(221, 22);
            this.menuitemSparse.Text = "Sparse Working Copy";
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
            this.verifyGitDatabaseToolStripMenuItem,
            this.deleteIndexlockToolStripMenuItem,
            this.editLocalGitConfigToolStripMenuItem});
            this.gitMaintenanceToolStripMenuItem.Image = global::GitUI.Properties.Resources.Icon_82;
            this.gitMaintenanceToolStripMenuItem.Name = "gitMaintenanceToolStripMenuItem";
            this.gitMaintenanceToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.gitMaintenanceToolStripMenuItem.Text = "Git maintenance";
            // 
            // compressGitDatabaseToolStripMenuItem
            // 
            this.compressGitDatabaseToolStripMenuItem.Name = "compressGitDatabaseToolStripMenuItem";
            this.compressGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.compressGitDatabaseToolStripMenuItem.Text = "Compress git database";
            this.compressGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.CompressGitDatabaseToolStripMenuItemClick);
            // 
            // verifyGitDatabaseToolStripMenuItem
            // 
            this.verifyGitDatabaseToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRecoverLostObjects;
            this.verifyGitDatabaseToolStripMenuItem.Name = "verifyGitDatabaseToolStripMenuItem";
            this.verifyGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.verifyGitDatabaseToolStripMenuItem.Text = "Recover lost objects...";
            this.verifyGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.VerifyGitDatabaseToolStripMenuItemClick);
            // 
            // deleteIndexlockToolStripMenuItem
            // 
            this.deleteIndexlockToolStripMenuItem.Name = "deleteIndexlockToolStripMenuItem";
            this.deleteIndexlockToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.deleteIndexlockToolStripMenuItem.Text = "Delete index.lock";
            this.deleteIndexlockToolStripMenuItem.Click += new System.EventHandler(this.deleteIndexlockToolStripMenuItem_Click);
            // 
            // editLocalGitConfigToolStripMenuItem
            // 
            this.editLocalGitConfigToolStripMenuItem.Name = "editLocalGitConfigToolStripMenuItem";
            this.editLocalGitConfigToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.editLocalGitConfigToolStripMenuItem.Text = "Edit .git/config";
            this.editLocalGitConfigToolStripMenuItem.Click += new System.EventHandler(this.EditLocalGitConfigToolStripMenuItemClick);
            // 
            // repoSettingsToolStripMenuItem
            // 
            this.repoSettingsToolStripMenuItem.Name = "repoSettingsToolStripMenuItem";
            this.repoSettingsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.repoSettingsToolStripMenuItem.Text = "Repository settings";
            this.repoSettingsToolStripMenuItem.Click += new System.EventHandler(this.RepoSettingsToolStripMenuItemClick);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(218, 6);
            // 
            // gitBashToolStripMenuItem
            // 
            this.gitBashToolStripMenuItem.Image = global::GitUI.Properties.Resources.bash;
            this.gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            this.gitBashToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.gitBashToolStripMenuItem.Text = "Git bash";
            this.gitBashToolStripMenuItem.Click += new System.EventHandler(this.GitBashToolStripMenuItemClick1);
            // 
            // gitGUIToolStripMenuItem
            // 
            this.gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            this.gitGUIToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.gitGUIToolStripMenuItem.Text = "Git GUI";
            this.gitGUIToolStripMenuItem.Click += new System.EventHandler(this.GitGuiToolStripMenuItemClick);
            // 
            // kGitToolStripMenuItem
            // 
            this.kGitToolStripMenuItem.Name = "kGitToolStripMenuItem";
            this.kGitToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.kGitToolStripMenuItem.Text = "GitK";
            this.kGitToolStripMenuItem.Click += new System.EventHandler(this.KGitToolStripMenuItemClick);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitToolStripMenuItem,
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
            this.patchToolStripMenuItem,
            this.toolStripSeparator24,
            this.SvnFetchToolStripMenuItem,
            this.SvnRebaseToolStripMenuItem,
            this.SvnDcommitToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // commitToolStripMenuItem
            // 
            this.commitToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconClean;
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            this.commitToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.commitToolStripMenuItem.Text = "Commit...";
            this.commitToolStripMenuItem.Click += new System.EventHandler(this.CommitToolStripMenuItemClick);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.Image = global::GitUI.Properties.Resources.Icon_4;
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.pullToolStripMenuItem.Text = "Pull...";
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.PullToolStripMenuItemClick);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Image = global::GitUI.Properties.Resources.Icon_3;
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.pushToolStripMenuItem.Text = "Push...";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.PushToolStripMenuItemClick);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(210, 6);
            // 
            // stashToolStripMenuItem
            // 
            this.stashToolStripMenuItem.Image = global::GitUI.Properties.Resources.stash;
            this.stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            this.stashToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.stashToolStripMenuItem.Text = "Stash changes...";
            this.stashToolStripMenuItem.Click += new System.EventHandler(this.StashToolStripMenuItemClick);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.resetToolStripMenuItem.Text = "Reset changes...";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.ResetToolStripMenuItem_Click);
            // 
            // cleanupToolStripMenuItem
            // 
            this.cleanupToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCleanupRepo;
            this.cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
            this.cleanupToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.cleanupToolStripMenuItem.Text = "Clean working directory...";
            this.cleanupToolStripMenuItem.Click += new System.EventHandler(this.CleanupToolStripMenuItemClick);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(210, 6);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchCreate;
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.branchToolStripMenuItem.Text = "Create branch...";
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.CreateBranchToolStripMenuItemClick);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchDelete;
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch...";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.DeleteBranchToolStripMenuItemClick);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchCheckout;
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+.";
            this.checkoutBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemPeriod)));
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch...";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.CheckoutBranchToolStripMenuItemClick);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconMerge;
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.mergeBranchToolStripMenuItem.Text = "Merge branches...";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.MergeBranchToolStripMenuItemClick);
            // 
            // rebaseToolStripMenuItem
            // 
            this.rebaseToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRebase;
            this.rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            this.rebaseToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.rebaseToolStripMenuItem.Text = "Rebase...";
            this.rebaseToolStripMenuItem.Click += new System.EventHandler(this.RebaseToolStripMenuItemClick);
            // 
            // runMergetoolToolStripMenuItem
            // 
            this.runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            this.runMergetoolToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.runMergetoolToolStripMenuItem.Text = "Solve mergeconflicts...";
            this.runMergetoolToolStripMenuItem.Click += new System.EventHandler(this.RunMergetoolToolStripMenuItemClick);
            // 
            // toolStripSeparator45
            // 
            this.toolStripSeparator45.Name = "toolStripSeparator45";
            this.toolStripSeparator45.Size = new System.Drawing.Size(210, 6);
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconTagCreate;
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.tagToolStripMenuItem.Text = "Create tag...";
            this.tagToolStripMenuItem.Click += new System.EventHandler(this.TagToolStripMenuItemClick);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconTagDelete;
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.deleteTagToolStripMenuItem.Text = "Delete tag...";
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.DeleteTagToolStripMenuItemClick);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(210, 6);
            // 
            // cherryPickToolStripMenuItem
            // 
            this.cherryPickToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCherryPick;
            this.cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            this.cherryPickToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.cherryPickToolStripMenuItem.Text = "Cherry pick...";
            this.cherryPickToolStripMenuItem.Click += new System.EventHandler(this.CherryPickToolStripMenuItemClick);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconArchiveRevision;
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.archiveToolStripMenuItem.Text = "Archive revision...";
            this.archiveToolStripMenuItem.Click += new System.EventHandler(this.ArchiveToolStripMenuItemClick);
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.Image = global::GitUI.Properties.Resources.RevisionCheckout;
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.checkoutToolStripMenuItem.Text = "Checkout revision...";
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.CheckoutToolStripMenuItemClick);
            // 
            // bisectToolStripMenuItem
            // 
            this.bisectToolStripMenuItem.Name = "bisectToolStripMenuItem";
            this.bisectToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.bisectToolStripMenuItem.Text = "Bisect...";
            this.bisectToolStripMenuItem.Click += new System.EventHandler(this.BisectClick);
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(210, 6);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.formatPatchToolStripMenuItem.Text = "Format patch...";
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.FormatPatchToolStripMenuItemClick);
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.applyPatchToolStripMenuItem.Text = "Apply patch...";
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.ApplyPatchToolStripMenuItemClick);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.patchToolStripMenuItem.Text = "View patch file...";
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.PatchToolStripMenuItemClick);
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            this.toolStripSeparator24.Size = new System.Drawing.Size(210, 6);
            // 
            // SvnFetchToolStripMenuItem
            // 
            this.SvnFetchToolStripMenuItem.Name = "SvnFetchToolStripMenuItem";
            this.SvnFetchToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.SvnFetchToolStripMenuItem.Text = "SVN Fetch";
            this.SvnFetchToolStripMenuItem.Click += new System.EventHandler(this.SvnFetchToolStripMenuItem_Click);
            // 
            // SvnRebaseToolStripMenuItem
            // 
            this.SvnRebaseToolStripMenuItem.Name = "SvnRebaseToolStripMenuItem";
            this.SvnRebaseToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.SvnRebaseToolStripMenuItem.Text = "SVN Rebase";
            this.SvnRebaseToolStripMenuItem.Click += new System.EventHandler(this.SvnRebaseToolStripMenuItem_Click);
            // 
            // SvnDcommitToolStripMenuItem
            // 
            this.SvnDcommitToolStripMenuItem.Name = "SvnDcommitToolStripMenuItem";
            this.SvnDcommitToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.SvnDcommitToolStripMenuItem.Text = "SVN DCommit";
            this.SvnDcommitToolStripMenuItem.Click += new System.EventHandler(this.SvnDcommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator46
            // 
            this.toolStripSeparator46.Name = "toolStripSeparator46";
            this.toolStripSeparator46.Size = new System.Drawing.Size(268, 6);
            // 
            // toolStripSeparator41
            // 
            this.toolStripSeparator41.Name = "toolStripSeparator41";
            this.toolStripSeparator41.Size = new System.Drawing.Size(165, 6);
            // 
            // toolStripSeparator42
            // 
            this.toolStripSeparator42.Name = "toolStripSeparator42";
            this.toolStripSeparator42.Size = new System.Drawing.Size(110, 6);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(165, 6);
            // 
            // PuTTYToolStripMenuItem
            // 
            this.PuTTYToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAuthenticationAgentToolStripMenuItem,
            this.generateOrImportKeyToolStripMenuItem});
            this.PuTTYToolStripMenuItem.Image = global::GitUI.Properties.Resources.putty;
            this.PuTTYToolStripMenuItem.Name = "PuTTYToolStripMenuItem";
            this.PuTTYToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.PuTTYToolStripMenuItem.Text = "PuTTY";
            // 
            // startAuthenticationAgentToolStripMenuItem
            // 
            this.startAuthenticationAgentToolStripMenuItem.Image = global::GitUI.Properties.Resources.pageant_16;
            this.startAuthenticationAgentToolStripMenuItem.Name = "startAuthenticationAgentToolStripMenuItem";
            this.startAuthenticationAgentToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.startAuthenticationAgentToolStripMenuItem.Text = "Start authentication agent";
            this.startAuthenticationAgentToolStripMenuItem.Click += new System.EventHandler(this.StartAuthenticationAgentToolStripMenuItemClick);
            // 
            // generateOrImportKeyToolStripMenuItem
            // 
            this.generateOrImportKeyToolStripMenuItem.Image = global::GitUI.Properties.Resources.puttygen;
            this.generateOrImportKeyToolStripMenuItem.Name = "generateOrImportKeyToolStripMenuItem";
            this.generateOrImportKeyToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.generateOrImportKeyToolStripMenuItem.Text = "Generate or import key";
            this.generateOrImportKeyToolStripMenuItem.Click += new System.EventHandler(this.GenerateOrImportKeyToolStripMenuItemClick);
            // 
            // _repositoryHostsToolStripMenuItem
            // 
            this._repositoryHostsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._forkCloneRepositoryToolStripMenuItem,
            this._viewPullRequestsToolStripMenuItem,
            this._createPullRequestsToolStripMenuItem});
            this._repositoryHostsToolStripMenuItem.Name = "_repositoryHostsToolStripMenuItem";
            this._repositoryHostsToolStripMenuItem.Size = new System.Drawing.Size(114, 20);
            this._repositoryHostsToolStripMenuItem.Text = "(Repository hosts)";
            // 
            // _forkCloneRepositoryToolStripMenuItem
            // 
            this._forkCloneRepositoryToolStripMenuItem.Name = "_forkCloneRepositoryToolStripMenuItem";
            this._forkCloneRepositoryToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this._forkCloneRepositoryToolStripMenuItem.Text = "Fork/Clone repository...";
            this._forkCloneRepositoryToolStripMenuItem.Click += new System.EventHandler(this._forkCloneMenuItem_Click);
            // 
            // _viewPullRequestsToolStripMenuItem
            // 
            this._viewPullRequestsToolStripMenuItem.Name = "_viewPullRequestsToolStripMenuItem";
            this._viewPullRequestsToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this._viewPullRequestsToolStripMenuItem.Text = "View pull requests...";
            this._viewPullRequestsToolStripMenuItem.Click += new System.EventHandler(this._viewPullRequestsToolStripMenuItem_Click);
            // 
            // _createPullRequestsToolStripMenuItem
            // 
            this._createPullRequestsToolStripMenuItem.Name = "_createPullRequestsToolStripMenuItem";
            this._createPullRequestsToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this._createPullRequestsToolStripMenuItem.Text = "Create pull requests...";
            this._createPullRequestsToolStripMenuItem.Click += new System.EventHandler(this._createPullRequestToolStripMenuItem_Click);
            // 
            // dashboardToolStripMenuItem
            // 
            this.dashboardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshDashboardToolStripMenuItem,
            this.toolStripSeparator42});
            this.dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            this.dashboardToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.dashboardToolStripMenuItem.Text = "Dashboard";
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator15,
            this.pluginSettingsToolStripMenuItem});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            this.pluginsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.pluginsToolStripMenuItem_DropDownOpening);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(113, 6);
            // 
            // pluginSettingsToolStripMenuItem
            // 
            this.pluginSettingsToolStripMenuItem.Name = "pluginSettingsToolStripMenuItem";
            this.pluginSettingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.pluginSettingsToolStripMenuItem.Text = "Settings";
            this.pluginSettingsToolStripMenuItem.Click += new System.EventHandler(this.PluginSettingsToolStripMenuItemClick);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Image = global::GitUI.Properties.Resources.Icon_71;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userManualToolStripMenuItem,
            this.changelogToolStripMenuItem,
            this.toolStripSeparator3,
            this.translateToolStripMenuItem,
            this.toolStripSeparator16,
            this.donateToolStripMenuItem,
            this.reportAnIssueToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // userManualToolStripMenuItem
            // 
            this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            this.userManualToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.userManualToolStripMenuItem.Text = "User Manual";
            this.userManualToolStripMenuItem.Click += new System.EventHandler(this.UserManualToolStripMenuItemClick);
            // 
            // changelogToolStripMenuItem
            // 
            this.changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            this.changelogToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.changelogToolStripMenuItem.Text = "Changelog";
            this.changelogToolStripMenuItem.Click += new System.EventHandler(this.ChangelogToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(167, 6);
            // 
            // translateToolStripMenuItem
            // 
            this.translateToolStripMenuItem.Name = "translateToolStripMenuItem";
            this.translateToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.translateToolStripMenuItem.Text = "Translate";
            this.translateToolStripMenuItem.Click += new System.EventHandler(this.TranslateToolStripMenuItemClick);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(167, 6);
            // 
            // donateToolStripMenuItem
            // 
            this.donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            this.donateToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.donateToolStripMenuItem.Text = "Donate";
            this.donateToolStripMenuItem.Click += new System.EventHandler(this.DonateToolStripMenuItemClick);
            // 
            // reportAnIssueToolStripMenuItem
            // 
            this.reportAnIssueToolStripMenuItem.Name = "reportAnIssueToolStripMenuItem";
            this.reportAnIssueToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.reportAnIssueToolStripMenuItem.Text = "Report an issue";
            this.reportAnIssueToolStripMenuItem.Click += new System.EventHandler(this.reportAnIssueToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::GitUI.Properties.Resources.Info;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitBashToolStripMenuItem,
            this.gitGUIToolStripMenuItem,
            this.kGitToolStripMenuItem,
            this.toolStripSeparator6,
            this.PuTTYToolStripMenuItem,
            this.toolStripSeparator41,
            this.commitcountPerUserToolStripMenuItem,
            this.gitcommandLogToolStripMenuItem,
            this.toolStripSeparator7,
            this.settingsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // commitcountPerUserToolStripMenuItem
            // 
            this.commitcountPerUserToolStripMenuItem.Image = global::GitUI.Properties.Resources.statistic;
            this.commitcountPerUserToolStripMenuItem.Name = "commitcountPerUserToolStripMenuItem";
            this.commitcountPerUserToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.commitcountPerUserToolStripMenuItem.Text = "Commits per user";
            this.commitcountPerUserToolStripMenuItem.Click += new System.EventHandler(this.CommitcountPerUserToolStripMenuItemClick);
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.Image = global::GitUI.Properties.Resources.New;
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.gitcommandLogToolStripMenuItem.Text = "Gitcommand log";
            this.gitcommandLogToolStripMenuItem.Click += new System.EventHandler(this.GitcommandLogToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(165, 6);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ClickThrough = true;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dashboardToolStripMenuItem,
            this.repositoryToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this._repositoryHostsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(923, 24);
            this.menuStrip1.TabIndex = 3;
            // 
            // toolStripMenuItemReflog
            // 
            this.toolStripMenuItemReflog.Name = "toolStripMenuItemReflog";
            this.toolStripMenuItemReflog.Size = new System.Drawing.Size(254, 26);
            this.toolStripMenuItemReflog.Text = "Show reflog...";
            this.toolStripMenuItemReflog.Click += new System.EventHandler(this.toolStripMenuItemReflog_Click);
            // 
            // FormBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(923, 573);
            this.Controls.Add(this.toolPanel);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip);
            this.Name = "FormBrowse";
            this.Text = "Git Extensions";
            this.Activated += new System.EventHandler(this.FormBrowse_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBrowseFormClosing);
            this.Load += new System.EventHandler(this.BrowseLoad);
            this.toolPanel.Panel1.ResumeLayout(false);
            this.toolPanel.Panel1.PerformLayout();
            this.toolPanel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.toolPanel)).EndInit();
            this.toolPanel.ResumeLayout(false);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.CommitInfoTabControl.ResumeLayout(false);
            this.CommitInfoTabPage.ResumeLayout(false);
            this.TreeTabPage.ResumeLayout(false);
            this.FileTreeSplitContainer.Panel1.ResumeLayout(false);
            this.FileTreeSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FileTreeSplitContainer)).EndInit();
            this.FileTreeSplitContainer.ResumeLayout(false);
            this.FileTreeContextMenu.ResumeLayout(false);
            this.DiffTabPage.ResumeLayout(false);
            this.DiffSplitContainer.Panel1.ResumeLayout(false);
            this.DiffSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DiffSplitContainer)).EndInit();
            this.DiffSplitContainer.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.TreeContextMenu.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView GitTree;
        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.TabControl CommitInfoTabControl;
        private System.Windows.Forms.TabPage TreeTabPage;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private GitUI.RevisionGrid RevisionGrid;
        private System.Windows.Forms.SplitContainer FileTreeSplitContainer;
        private ToolStripEx ToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSplitButton _NO_TRANSLATE_Workingdir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton GitBash;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton EditSettings;
        private System.Windows.Forms.ToolStripButton RefreshButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripTextBox toolStripRevisionFilterTextBox;
        private System.Windows.Forms.TabPage DiffTabPage;
        private System.Windows.Forms.SplitContainer DiffSplitContainer;
        private FileStatusList DiffFiles;
        private System.Windows.Forms.ToolStripButton toolStripButtonPush;
        private FileViewer FileText;
        private FileViewer DiffText;
        private System.Windows.Forms.TabPage CommitInfoTabPage;
        private CommitInfo.CommitInfo RevisionInfo;
        private System.Windows.Forms.ToolStripLabel toolStripRevisionFilterLabel;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitStash;
        private System.Windows.Forms.ToolStripMenuItem stashChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stashPopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem viewStashToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip TreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private ContextMenuStrip FileTreeContextMenu;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem resetToThisRevisionToolStripMenuItem;
        private ToolStripMenuItem openFileToolStripMenuItem;
        private ToolStripMenuItem openFileWithToolStripMenuItem;
        private ToolStripMenuItem fileHistoryToolStripMenuItem;
        private ToolStripMenuItem findToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem;
        private ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem1;
        private ToolStripMenuItem saveAsToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox toolStripBranchFilterComboBox;
        private ToolStripDropDownButton toolStripBranchFilterDropDownButton;
        private ToolStripDropDownButton toolStripRevisionFilterDropDownButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem openWithToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripSeparator toolStripSeparator30;
        private ToolStripSeparator toolStripSeparator31;
        private ToolStripSeparator toolStripSeparator32;
        private ToolStripSeparator toolStripSeparator33;
        private ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private ToolStripSplitButton branchSelect;
        private ToolStripButton toggleSplitViewLayout;
        private ToolStripMenuItem editCheckedOutFileToolStripMenuItem;
        private SplitContainer toolPanel;
        private ToolStripEx UserMenuToolStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem refreshToolStripMenuItem;
        private ToolStripMenuItem refreshDashboardToolStripMenuItem;
        private ToolStripMenuItem recentToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem fileExplorerToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem repositoryToolStripMenuItem;
        private ToolStripMenuItem gitBashToolStripMenuItem;
        private ToolStripMenuItem gitGUIToolStripMenuItem;
        private ToolStripMenuItem kGitToolStripMenuItem;
        private ToolStripMenuItem commandsToolStripMenuItem;
        private ToolStripMenuItem applyPatchToolStripMenuItem;
        private ToolStripMenuItem archiveToolStripMenuItem;
        private ToolStripMenuItem bisectToolStripMenuItem;
        private ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private ToolStripMenuItem checkoutToolStripMenuItem;
        private ToolStripMenuItem cherryPickToolStripMenuItem;
        private ToolStripMenuItem cleanupToolStripMenuItem;
        private ToolStripMenuItem cloneToolStripMenuItem;
        private ToolStripMenuItem commitToolStripMenuItem;
        private ToolStripMenuItem branchToolStripMenuItem;
        private ToolStripMenuItem tagToolStripMenuItem;
        private ToolStripMenuItem deleteBranchToolStripMenuItem;
        private ToolStripMenuItem deleteTagToolStripMenuItem;
        private ToolStripMenuItem formatPatchToolStripMenuItem;
        private ToolStripMenuItem initNewRepositoryToolStripMenuItem;
        private ToolStripMenuItem mergeBranchToolStripMenuItem;
        private ToolStripMenuItem pullToolStripMenuItem;
        private ToolStripMenuItem pushToolStripMenuItem;
        private ToolStripMenuItem rebaseToolStripMenuItem;
        private ToolStripMenuItem runMergetoolToolStripMenuItem;
        private ToolStripMenuItem stashToolStripMenuItem;
        private ToolStripMenuItem patchToolStripMenuItem;
        private ToolStripMenuItem manageRemoteRepositoriesToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem PuTTYToolStripMenuItem;
        private ToolStripMenuItem startAuthenticationAgentToolStripMenuItem;
        private ToolStripMenuItem generateOrImportKeyToolStripMenuItem;
        private ToolStripMenuItem _repositoryHostsToolStripMenuItem;
        private ToolStripMenuItem _forkCloneRepositoryToolStripMenuItem;
        private ToolStripMenuItem _viewPullRequestsToolStripMenuItem;
        private ToolStripMenuItem _createPullRequestsToolStripMenuItem;
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
        private ToolStripMenuItem verifyGitDatabaseToolStripMenuItem;
        private ToolStripMenuItem deleteIndexlockToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem editgitignoreToolStripMenuItem1;
        private ToolStripMenuItem editgitattributesToolStripMenuItem;
        private ToolStripMenuItem editmailmapToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem userManualToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem translateToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem donateToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private MenuStripEx menuStrip1;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private ToolStripMenuItem fileTreeOpenContainingFolderToolStripMenuItem;
        private ToolStripMenuItem fileTreeArchiveToolStripMenuItem;
        private ToolStripMenuItem fileTreeCleanWorkingTreeToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator21;
        private ToolStripSeparator toolStripSeparator25;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripSeparator toolStripSeparator23;
        private ToolStripMenuItem cloneSVNToolStripMenuItem;
        private ToolStripMenuItem SvnRebaseToolStripMenuItem;
        private ToolStripMenuItem SvnDcommitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator24;
        private ToolStripMenuItem SvnFetchToolStripMenuItem;
        private ToolStripMenuItem blameToolStripMenuItem;
        private ToolStripMenuItem blameToolStripMenuItem1;
        private ToolStripMenuItem expandAllToolStripMenuItem;
        private ToolStripMenuItem findInDiffToolStripMenuItem;        
        private ToolStripMenuItem collapseAllToolStripMenuItem;
        private ToolStripSplitButton toolStripButtonLevelUp;
        private ToolStripSplitButton toolStripButtonPull;
        private ToolStripMenuItem mergeToolStripMenuItem;
        private ToolStripMenuItem rebaseToolStripMenuItem1;
        private ToolStripMenuItem fetchToolStripMenuItem;
        private ToolStripMenuItem pullToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripMenuItem setNextPullActionAsDefaultToolStripMenuItem;
        private ToolStripMenuItem fetchAllToolStripMenuItem;
        private ToolStripMenuItem resetFileToToolStripMenuItem;
        private ToolStripMenuItem resetFileToFirstToolStripMenuItem;
        private ToolStripMenuItem resetFileToParentToolStripMenuItem;
        private ToolStripMenuItem resetFileToSecondToolStripMenuItem;
        private ToolStripMenuItem resetFileToSelectedToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;        
        private ToolStripMenuItem aBToolStripMenuItem;
        private ToolStripMenuItem aLocalToolStripMenuItem;
        private ToolStripMenuItem bLocalToolStripMenuItem;
        private ToolStripMenuItem parentOfALocalToolStripMenuItem;
        private ToolStripMenuItem parentOfBLocalToolStripMenuItem;        
        private ToolStripMenuItem reportAnIssueToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator41;
        private ToolStripSeparator toolStripSeparator42;
        private ToolStripSeparator toolStripSeparator43;
        private ToolStripSeparator toolStripSeparator44;
        private ToolStripSeparator toolStripSeparator45;
        private ToolStripSeparator toolStripSeparator46;
        private ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private ToolStripMenuItem commitcountPerUserToolStripMenuItem;
        private ToolStripMenuItem gitcommandLogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem cherryPickSelectedDiffFileToolStripMenuItem;
        private ToolStripMenuItem menuitemSparse;
        private ToolStripButton ShowFirstParent;
        private ToolTip FilterToolTip;
        private ToolStripMenuItem toolStripMenuItemReflog;
    }
}
