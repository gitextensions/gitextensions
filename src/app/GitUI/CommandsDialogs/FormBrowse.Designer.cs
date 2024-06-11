using System.Drawing;
using System.Windows.Forms;
using GitExtensions.Extensibility.Git;
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
            components = new System.ComponentModel.Container();
            ToolStripSeparator toolStripSeparator14;
            ToolStripSeparator toolStripSeparator11;
            ToolStripMain = new GitUI.ToolStripEx();
            RefreshButton = new ToolStripButton();
            toolStripSeparator0 = new ToolStripSeparator();
            toggleLeftPanel = new ToolStripButton();
            toggleSplitViewLayout = new ToolStripButton();
            menuCommitInfoPosition = new ToolStripSplitButton();
            commitInfoBelowMenuItem = new ToolStripMenuItem();
            commitInfoLeftwardMenuItem = new ToolStripMenuItem();
            commitInfoRightwardMenuItem = new ToolStripMenuItem();
            toolStripSeparator17 = new ToolStripSeparator();
            toolStripButtonLevelUp = new ToolStripSplitButton();
            _NO_TRANSLATE_WorkingDir = new WorkingDirectoryToolStripSplitButton();
            branchSelect = new ToolStripSplitButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripSplitStash = new ToolStripSplitButton();
            stashChangesToolStripMenuItem = new ToolStripMenuItem();
            stashStagedToolStripMenuItem = new ToolStripMenuItem();
            stashPopToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            manageStashesToolStripMenuItem = new ToolStripMenuItem();
            createAStashToolStripMenuItem = new ToolStripMenuItem();
            toolStripButtonCommit = new ToolStripButton();
            toolStripButtonPull = new ToolStripSplitButton();
            mergeToolStripMenuItem = new ToolStripMenuItem();
            rebaseToolStripMenuItem1 = new ToolStripMenuItem();
            fetchToolStripMenuItem = new ToolStripMenuItem();
            pullToolStripMenuItem1 = new ToolStripMenuItem();
            fetchAllToolStripMenuItem = new ToolStripMenuItem();
            fetchPruneAllToolStripMenuItem = new ToolStripMenuItem();
            setDefaultPullButtonActionToolStripMenuItem = new ToolStripMenuItem();
            toolStripButtonPush = new GitUI.CommandsDialogs.ToolStripPushButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripFileExplorer = new ToolStripButton();
            userShell = new ToolStripSplitButton();
            EditSettings = new ToolStripButton();
            MainSplitContainer = new SplitContainer();
            repoObjectsTree = new GitUI.LeftPanel.RepoObjectsTree();
            RightSplitContainer = new SplitContainer();
            RevisionsSplitContainer = new SplitContainer();
            RevisionGridContainer = new Panel();
            RevisionGrid = new GitUI.RevisionGridControl();
            notificationBarBisectInProgress = new GitUI.UserControls.InteractiveGitActionControl();
            notificationBarGitActionInProgress = new GitUI.UserControls.InteractiveGitActionControl();
            CommitInfoTabControl = new GitUI.CommandsDialogs.FullBleedTabControl();
            CommitInfoTabPage = new TabPage();
            RevisionInfo = new GitUI.CommitInfo.CommitInfo();
            TreeTabPage = new TabPage();
            fileTree = new GitUI.CommandsDialogs.RevisionFileTreeControl();
            DiffTabPage = new TabPage();
            revisionDiff = new GitUI.CommandsDialogs.RevisionDiffControl();
            GpgInfoTabPage = new TabPage();
            revisionGpgInfo1 = new GitUI.CommandsDialogs.RevisionGpgInfoControl();
            FilterToolTip = new ToolTip(components);
            fileToolStripMenuItem = new GitUI.CommandsDialogs.Menus.StartToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            refreshToolStripMenuItem = new ToolStripMenuItem();
            refreshDashboardToolStripMenuItem = new ToolStripMenuItem();
            fileExplorerToolStripMenuItem = new ToolStripMenuItem();
            repositoryToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            manageRemoteRepositoriesToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparator43 = new ToolStripSeparator();
            manageSubmodulesToolStripMenuItem = new ToolStripMenuItem();
            updateAllSubmodulesToolStripMenuItem = new ToolStripMenuItem();
            synchronizeAllSubmodulesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            manageWorktreeToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator44 = new ToolStripSeparator();
            editgitignoreToolStripMenuItem1 = new ToolStripMenuItem();
            editgitinfoexcludeToolStripMenuItem = new ToolStripMenuItem();
            editGitAttributesToolStripMenuItem = new ToolStripMenuItem();
            editmailmapToolStripMenuItem = new ToolStripMenuItem();
            menuitemSparse = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            gitMaintenanceToolStripMenuItem = new ToolStripMenuItem();
            compressGitDatabaseToolStripMenuItem = new ToolStripMenuItem();
            recoverLostObjectsToolStripMenuItem = new ToolStripMenuItem();
            deleteIndexLockToolStripMenuItem = new ToolStripMenuItem();
            editLocalGitConfigToolStripMenuItem = new ToolStripMenuItem();
            repoSettingsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            commandsToolStripMenuItem = new ToolStripMenuItem();
            commitToolStripMenuItem = new ToolStripMenuItem();
            undoLastCommitToolStripMenuItem = new ToolStripMenuItem();
            pullToolStripMenuItem = new ToolStripMenuItem();
            pushToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator21 = new ToolStripSeparator();
            stashToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            cleanupToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator25 = new ToolStripSeparator();
            branchToolStripMenuItem = new ToolStripMenuItem();
            deleteBranchToolStripMenuItem = new ToolStripMenuItem();
            checkoutBranchToolStripMenuItem = new ToolStripMenuItem();
            mergeBranchToolStripMenuItem = new ToolStripMenuItem();
            rebaseToolStripMenuItem = new ToolStripMenuItem();
            runMergetoolToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator45 = new ToolStripSeparator();
            tagToolStripMenuItem = new ToolStripMenuItem();
            deleteTagToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator23 = new ToolStripSeparator();
            cherryPickToolStripMenuItem = new ToolStripMenuItem();
            archiveToolStripMenuItem = new ToolStripMenuItem();
            checkoutToolStripMenuItem = new ToolStripMenuItem();
            bisectToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemReflog = new ToolStripMenuItem();
            toolStripSeparator22 = new ToolStripSeparator();
            formatPatchToolStripMenuItem = new ToolStripMenuItem();
            applyPatchToolStripMenuItem = new ToolStripMenuItem();
            patchToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator46 = new ToolStripSeparator();
            toolStripSeparator42 = new ToolStripSeparator();
            _repositoryHostsToolStripMenuItem = new ToolStripMenuItem();
            _forkCloneRepositoryToolStripMenuItem = new ToolStripMenuItem();
            _viewPullRequestsToolStripMenuItem = new ToolStripMenuItem();
            _createPullRequestsToolStripMenuItem = new ToolStripMenuItem();
            dashboardToolStripMenuItem = new ToolStripMenuItem();
            pluginsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator15 = new ToolStripSeparator();
            pluginSettingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new GitUI.CommandsDialogs.Menus.HelpToolStripMenuItem();
            toolsToolStripMenuItem = new GitUI.CommandsDialogs.Menus.ToolsToolStripMenuItem();
            mainMenuStrip = new GitUI.MenuStripEx();
            gitItemBindingSource = new BindingSource(components);
            gitRevisionBindingSource = new BindingSource(components);
            toolPanel = new ToolStripContainer();
            _addUpstreamRemoteToolStripMenuItem = new ToolStripMenuItem();
            ToolStripFilters = new GitUI.UserControls.FilterToolBar();
            ToolStripScripts = new GitUI.ToolStripEx();
            pluginsLoadingToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            toolStripSeparator11 = new ToolStripSeparator();
            ToolStripMain.SuspendLayout();
            ToolStripFilters.SuspendLayout();
            ToolStripScripts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(MainSplitContainer)).BeginInit();
            MainSplitContainer.Panel1.SuspendLayout();
            MainSplitContainer.Panel2.SuspendLayout();
            MainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(RightSplitContainer)).BeginInit();
            RightSplitContainer.Panel1.SuspendLayout();
            RightSplitContainer.Panel2.SuspendLayout();
            RightSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(RevisionsSplitContainer)).BeginInit();
            RevisionsSplitContainer.Panel1.SuspendLayout();
            RevisionsSplitContainer.SuspendLayout();
            RevisionGridContainer.SuspendLayout();
            CommitInfoTabControl.SuspendLayout();
            CommitInfoTabPage.SuspendLayout();
            TreeTabPage.SuspendLayout();
            DiffTabPage.SuspendLayout();
            GpgInfoTabPage.SuspendLayout();
            mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(gitRevisionBindingSource)).BeginInit();
            toolPanel.ContentPanel.SuspendLayout();
            toolPanel.TopToolStripPanel.SuspendLayout();
            toolPanel.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(236, 6);
            // 
            // ToolStripMain
            // 
            ToolStripMain.ClickThrough = true;
            ToolStripMain.Dock = DockStyle.None;
            ToolStripMain.DrawBorder = false;
            ToolStripMain.GripEnabled = false;
            ToolStripMain.GripMargin = new Padding(0);
            ToolStripMain.ImeMode = ImeMode.NoControl;
            ToolStripMain.Items.AddRange(new ToolStripItem[] {
            RefreshButton,
            toolStripSeparator0,
            toggleLeftPanel,
            toggleSplitViewLayout,
            menuCommitInfoPosition,
            toolStripSeparator17,
            toolStripButtonLevelUp,
            _NO_TRANSLATE_WorkingDir,
            branchSelect,
            toolStripSeparator1,
            toolStripButtonPull,
            toolStripButtonPush,
            toolStripButtonCommit,
            toolStripSplitStash,
            toolStripSeparator2,
            toolStripFileExplorer,
            userShell,
            EditSettings});
            ToolStripMain.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolStripMain.Location = new Point(3, 0);
            ToolStripMain.Name = "ToolStripMain";
            ToolStripMain.Padding = new Padding(0);
            ToolStripMain.Size = new Size(479, 25);
            ToolStripMain.TabIndex = 0;
            ToolStripMain.Text = "Standard";
            // 
            // RefreshButton
            // 
            RefreshButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshButton.Image = Properties.Images.ReloadRevisions;
            RefreshButton.ImageTransparentColor = Color.White;
            RefreshButton.Name = "RefreshButton";
            RefreshButton.Size = new Size(23, 22);
            RefreshButton.ToolTipText = "Refresh";
            RefreshButton.Click += RefreshToolStripMenuItemClick;
            // 
            // toolStripSeparator0
            // 
            toolStripSeparator0.Name = "toolStripSeparator0";
            toolStripSeparator0.Size = new Size(6, 25);
            // 
            // toggleLeftPanel
            // 
            toggleLeftPanel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toggleLeftPanel.Image = Properties.Images.LayoutSidebarLeft;
            toggleLeftPanel.Name = "toggleLeftPanel";
            toggleLeftPanel.Size = new Size(23, 22);
            toggleLeftPanel.ToolTipText = "Toggle left panel";
            toggleLeftPanel.Click += toggleLeftPanel_Click;
            // 
            // toggleSplitViewLayout
            // 
            toggleSplitViewLayout.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toggleSplitViewLayout.Image = Properties.Images.LayoutFooter;
            toggleSplitViewLayout.Name = "toggleSplitViewLayout";
            toggleSplitViewLayout.Size = new Size(23, 22);
            toggleSplitViewLayout.ToolTipText = "Toggle split view layout";
            toggleSplitViewLayout.Click += toggleSplitViewLayout_Click;
            // 
            // menuCommitInfoPosition
            // 
            menuCommitInfoPosition.DisplayStyle = ToolStripItemDisplayStyle.Image;
            menuCommitInfoPosition.DropDownItems.AddRange(new ToolStripItem[] {
            commitInfoBelowMenuItem,
            commitInfoLeftwardMenuItem,
            commitInfoRightwardMenuItem});
            menuCommitInfoPosition.Image = Properties.Images.LayoutFooterTab;
            menuCommitInfoPosition.Name = "menuCommitInfoPosition";
            menuCommitInfoPosition.Size = new Size(32, 22);
            menuCommitInfoPosition.ToolTipText = "Commit info position";
            menuCommitInfoPosition.Click += CommitInfoPositionClick;
            // 
            // commitInfoBelowMenuItem
            // 
            commitInfoBelowMenuItem.Image = Properties.Images.LayoutFooterTab;
            commitInfoBelowMenuItem.Name = "commitInfoBelowMenuItem";
            commitInfoBelowMenuItem.Size = new Size(259, 22);
            commitInfoBelowMenuItem.Text = "Commit info &below graph";
            commitInfoBelowMenuItem.Click += CommitInfoBelowClick;
            // 
            // commitInfoLeftwardMenuItem
            // 
            commitInfoLeftwardMenuItem.Image = Properties.Images.LayoutSidebarTopLeft;
            commitInfoLeftwardMenuItem.Name = "commitInfoLeftwardMenuItem";
            commitInfoLeftwardMenuItem.Size = new Size(259, 22);
            commitInfoLeftwardMenuItem.Text = "Commit info &left of graph";
            commitInfoLeftwardMenuItem.Click += CommitInfoLeftwardClick;
            // 
            // commitInfoRightwardMenuItem
            // 
            commitInfoRightwardMenuItem.Image = Properties.Images.LayoutSidebarTopRight;
            commitInfoRightwardMenuItem.Name = "commitInfoRightwardMenuItem";
            commitInfoRightwardMenuItem.Size = new Size(259, 22);
            commitInfoRightwardMenuItem.Text = "Commit info &right of graph";
            commitInfoRightwardMenuItem.Click += CommitInfoRightwardClick;
            // 
            // toolStripSeparator17
            // 
            toolStripSeparator17.Name = "toolStripSeparator17";
            toolStripSeparator17.Size = new Size(6, 25);
            // 
            // toolStripButtonLevelUp
            // 
            toolStripButtonLevelUp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonLevelUp.Image = Properties.Images.SubmodulesManage;
            toolStripButtonLevelUp.ImageTransparentColor = Color.Magenta;
            toolStripButtonLevelUp.Name = "toolStripButtonLevelUp";
            toolStripButtonLevelUp.Size = new Size(32, 22);
            toolStripButtonLevelUp.ToolTipText = "Submodules";
            toolStripButtonLevelUp.ButtonClick += toolStripButtonLevelUp_ButtonClick;
            // 
            // _NO_TRANSLATE_WorkingDir
            // 
            _NO_TRANSLATE_WorkingDir.Size = new Size(83, 22);
            _NO_TRANSLATE_WorkingDir.Text = "WorkingDir";
            _NO_TRANSLATE_WorkingDir.ToolTipText = "Change working directory";
            // 
            // branchSelect
            // 
            branchSelect.Image = Properties.Resources.branch;
            branchSelect.ImageTransparentColor = Color.Magenta;
            branchSelect.Name = "branchSelect";
            branchSelect.Size = new Size(60, 22);
            branchSelect.Text = "Branch";
            branchSelect.ToolTipText = "Change current branch";
            branchSelect.ButtonClick += CurrentBranchClick;
            branchSelect.DropDownOpening += CurrentBranchDropDownOpening;
            branchSelect.MouseUp += branchSelect_MouseUp;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolStripSplitStash
            // 
            toolStripSplitStash.DropDownItems.AddRange(new ToolStripItem[] {
            stashChangesToolStripMenuItem,
            stashStagedToolStripMenuItem,
            stashPopToolStripMenuItem,
            toolStripSeparator9,
            manageStashesToolStripMenuItem,
            createAStashToolStripMenuItem});
            toolStripSplitStash.Image = Properties.Images.Stash;
            toolStripSplitStash.ImageTransparentColor = Color.Magenta;
            toolStripSplitStash.Name = "toolStripSplitStash";
            toolStripSplitStash.Size = new Size(32, 22);
            toolStripSplitStash.ToolTipText = "Manage stashes";
            toolStripSplitStash.ButtonClick += ToolStripSplitStashButtonClick;
            // 
            // stashChangesToolStripMenuItem
            // 
            stashChangesToolStripMenuItem.Name = "stashChangesToolStripMenuItem";
            stashChangesToolStripMenuItem.Size = new Size(167, 22);
            stashChangesToolStripMenuItem.Text = "&Stash";
            stashChangesToolStripMenuItem.ToolTipText = "Stash changes";
            stashChangesToolStripMenuItem.Click += StashChangesToolStripMenuItemClick;
            // 
            // stashStagedToolStripMenuItem
            // 
            stashStagedToolStripMenuItem.Name = "stashStagedToolStripMenuItem";
            stashStagedToolStripMenuItem.Size = new Size(167, 22);
            stashStagedToolStripMenuItem.Text = "S&tash staged";
            stashStagedToolStripMenuItem.ToolTipText = "Stash staged changes";
            stashStagedToolStripMenuItem.Click += StashStagedToolStripMenuItemClick;
            // 
            // stashPopToolStripMenuItem
            // 
            stashPopToolStripMenuItem.Name = "stashPopToolStripMenuItem";
            stashPopToolStripMenuItem.Size = new Size(167, 22);
            stashPopToolStripMenuItem.Text = "Stash &pop";
            stashPopToolStripMenuItem.ToolTipText = "Apply and drop single stash";
            stashPopToolStripMenuItem.Click += StashPopToolStripMenuItemClick;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(164, 6);
            // 
            // manageStashesToolStripMenuItem
            // 
            manageStashesToolStripMenuItem.Name = "manageStashesToolStripMenuItem";
            manageStashesToolStripMenuItem.Size = new Size(167, 22);
            manageStashesToolStripMenuItem.Text = "&Manage stashes...";
            manageStashesToolStripMenuItem.ToolTipText = "Manage stashes";
            manageStashesToolStripMenuItem.Click += ManageStashesToolStripMenuItemClick;
            // 
            // createAStashToolStripMenuItem
            // 
            createAStashToolStripMenuItem.Name = "createAStashToolStripMenuItem";
            createAStashToolStripMenuItem.Size = new Size(167, 22);
            createAStashToolStripMenuItem.Text = "&Create a stash...";
            createAStashToolStripMenuItem.Click += CreateStashToolStripMenuItemClick;
            // 
            // toolStripButtonCommit
            // 
            toolStripButtonCommit.Image = Properties.Images.RepoStateClean;
            toolStripButtonCommit.ImageAlign = ContentAlignment.MiddleLeft;
            toolStripButtonCommit.ImageTransparentColor = Color.Magenta;
            toolStripButtonCommit.Name = "toolStripButtonCommit";
            toolStripButtonCommit.Size = new Size(71, 22);
            toolStripButtonCommit.Text = "Commit";
            toolStripButtonCommit.TextAlign = ContentAlignment.MiddleLeft;
            toolStripButtonCommit.Click += CommitToolStripMenuItemClick;
            // 
            // toolStripButtonPull
            // 
            toolStripButtonPull.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonPull.DropDownItems.AddRange(new ToolStripItem[] {
            pullToolStripMenuItem1,
            toolStripSeparator11,
            mergeToolStripMenuItem,
            rebaseToolStripMenuItem1,
            fetchToolStripMenuItem,
            fetchAllToolStripMenuItem,
            fetchPruneAllToolStripMenuItem,
            toolStripSeparator14,
            setDefaultPullButtonActionToolStripMenuItem});
            toolStripButtonPull.Image = Properties.Images.Pull;
            toolStripButtonPull.ImageTransparentColor = Color.Magenta;
            toolStripButtonPull.Name = "toolStripButtonPull";
            toolStripButtonPull.Size = new Size(32, 22);
            toolStripButtonPull.Text = "Pull";
            toolStripButtonPull.ButtonClick += ToolStripButtonPullClick;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(236, 6);
            // 
            // mergeToolStripMenuItem
            // 
            mergeToolStripMenuItem.Image = Properties.Images.PullMerge;
            mergeToolStripMenuItem.Name = "mergeToolStripMenuItem";
            mergeToolStripMenuItem.Size = new Size(239, 22);
            mergeToolStripMenuItem.Text = "Pull - &merge";
            mergeToolStripMenuItem.Click += mergeToolStripMenuItem_Click;
            // 
            // rebaseToolStripMenuItem1
            // 
            rebaseToolStripMenuItem1.Image = Properties.Images.PullRebase;
            rebaseToolStripMenuItem1.Name = "rebaseToolStripMenuItem1";
            rebaseToolStripMenuItem1.Size = new Size(239, 22);
            rebaseToolStripMenuItem1.Text = "Pull - &rebase";
            rebaseToolStripMenuItem1.Click += rebaseToolStripMenuItem1_Click;
            // 
            // fetchToolStripMenuItem
            // 
            fetchToolStripMenuItem.Image = Properties.Images.PullFetch;
            fetchToolStripMenuItem.Name = "fetchToolStripMenuItem";
            fetchToolStripMenuItem.Size = new Size(239, 22);
            fetchToolStripMenuItem.Text = "&Fetch";
            fetchToolStripMenuItem.ToolTipText = "Fetch branches and tags";
            fetchToolStripMenuItem.Click += fetchToolStripMenuItem_Click;
            // 
            // pullToolStripMenuItem1
            // 
            pullToolStripMenuItem1.Image = Properties.Images.Pull;
            pullToolStripMenuItem1.Name = "pullToolStripMenuItem1";
            pullToolStripMenuItem1.Size = new Size(239, 22);
            pullToolStripMenuItem1.Text = "Open &pull dialog...";
            pullToolStripMenuItem1.Click += pullToolStripMenuItem1_Click;
            // 
            // fetchAllToolStripMenuItem
            // 
            fetchAllToolStripMenuItem.Image = Properties.Images.PullFetchAll;
            fetchAllToolStripMenuItem.Name = "fetchAllToolStripMenuItem";
            fetchAllToolStripMenuItem.Size = new Size(239, 22);
            fetchAllToolStripMenuItem.Text = "Fetch &all";
            fetchAllToolStripMenuItem.ToolTipText = "Fetch branches and tags from all remote repositories";
            fetchAllToolStripMenuItem.Click += fetchAllToolStripMenuItem_Click;
            // 
            // fetchPruneAllToolStripMenuItem
            // 
            fetchPruneAllToolStripMenuItem.Image = Properties.Images.PullFetchPruneAll;
            fetchPruneAllToolStripMenuItem.Name = "fetchPruneAllToolStripMenuItem";
            fetchPruneAllToolStripMenuItem.Size = new Size(239, 22);
            fetchPruneAllToolStripMenuItem.Text = "F&etch and prune all";
            fetchPruneAllToolStripMenuItem.ToolTipText = "Fetch branches and tags from all remote repositories also prune deleted branches";
            fetchPruneAllToolStripMenuItem.Click += fetchPruneAllToolStripMenuItem_Click;
            // 
            // setDefaultPullButtonActionToolStripMenuItem
            // 
            setDefaultPullButtonActionToolStripMenuItem.Name = "setDefaultPullButtonActionToolStripMenuItem";
            setDefaultPullButtonActionToolStripMenuItem.Size = new Size(239, 22);
            setDefaultPullButtonActionToolStripMenuItem.Text = "Set &default Pull button action";
            // 
            // toolStripButtonPush
            // 
            toolStripButtonPush.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonPush.Image = Properties.Images.Push;
            toolStripButtonPush.ImageTransparentColor = Color.Magenta;
            toolStripButtonPush.Name = "toolStripButtonPush";
            toolStripButtonPush.Size = new Size(23, 22);
            toolStripButtonPush.Text = "Push";
            toolStripButtonPush.Click += ToolStripButtonPushClick;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // toolStripFileExplorer
            // 
            toolStripFileExplorer.Enabled = false;
            toolStripFileExplorer.Image = Properties.Images.BrowseFileExplorer;
            toolStripFileExplorer.ImageTransparentColor = Color.Gray;
            toolStripFileExplorer.Name = "toolStripFileExplorer";
            toolStripFileExplorer.Size = new Size(23, 22);
            toolStripFileExplorer.ToolTipText = "File Explorer";
            toolStripFileExplorer.Click += FileExplorerToolStripMenuItemClick;
            // 
            // userShell
            // 
            userShell.Image = Properties.Images.GitForWindows;
            userShell.ImageTransparentColor = Color.Magenta;
            userShell.Name = "userShell";
            userShell.Size = new Size(23, 22);
            userShell.ToolTipText = "Git bash";
            userShell.Click += userShell_Click;
            // 
            // EditSettings
            // 
            EditSettings.DisplayStyle = ToolStripItemDisplayStyle.Image;
            EditSettings.Image = Properties.Images.Settings;
            EditSettings.Name = "EditSettings";
            EditSettings.Size = new Size(23, 22);
            EditSettings.ToolTipText = "Settings";
            EditSettings.Click += OnShowSettingsClick;
            // 
            // MainSplitContainer
            // 
            MainSplitContainer.Dock = DockStyle.Fill;
            MainSplitContainer.FixedPanel = FixedPanel.Panel1;
            MainSplitContainer.Location = new Point(0, 0);
            MainSplitContainer.Margin = new Padding(0);
            MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            MainSplitContainer.Panel1.Controls.Add(repoObjectsTree);
            MainSplitContainer.Panel1.Padding = new Padding(1);
            MainSplitContainer.Panel1MinSize = 192;
            // 
            // MainSplitContainer.Panel2
            // 
            MainSplitContainer.Panel2.Controls.Add(RightSplitContainer);
            MainSplitContainer.Size = new Size(923, 502);
            MainSplitContainer.SplitterWidth = 6;
            MainSplitContainer.TabIndex = 1;
            // 
            // repoObjectsTree
            // 
            repoObjectsTree.Dock = DockStyle.Fill;
            repoObjectsTree.Location = new Point(0, 0);
            repoObjectsTree.MinimumSize = new Size(190, 0);
            repoObjectsTree.Margin = new Padding(0);
            repoObjectsTree.Name = "repoObjectsTree";
            repoObjectsTree.Size = new Size(267, 502);
            repoObjectsTree.TabIndex = 0;
            // 
            // RightSplitContainer
            // 
            RightSplitContainer.Dock = DockStyle.Fill;
            RightSplitContainer.Location = new Point(0, 0);
            RightSplitContainer.Margin = new Padding(0);
            RightSplitContainer.Name = "RightSplitContainer";
            RightSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // RightSplitContainer.Panel1
            // 
            RightSplitContainer.Panel1.Controls.Add(RevisionsSplitContainer);
            // 
            // RightSplitContainer.Panel2
            // 
            RightSplitContainer.Panel2.Controls.Add(CommitInfoTabControl);
            RightSplitContainer.Panel2MinSize = 0;
            RightSplitContainer.Size = new Size(650, 502);
            RightSplitContainer.SplitterDistance = 209;
            RightSplitContainer.SplitterWidth = 6;
            RightSplitContainer.TabIndex = 1;
            RightSplitContainer.TabStop = false;
            // 
            // RevisionsSplitContainer
            // 
            RevisionsSplitContainer.Dock = DockStyle.Fill;
            RevisionsSplitContainer.FixedPanel = FixedPanel.Panel2;
            RevisionsSplitContainer.Location = new Point(0, 0);
            RevisionsSplitContainer.Margin = new Padding(0);
            RevisionsSplitContainer.Name = "RevisionsSplitContainer";
            RevisionsSplitContainer.Panel1.Padding = new Padding(1);
            RevisionsSplitContainer.Panel2.Padding = new Padding(1);
            // 
            // RevisionsSplitContainer.Panel1
            // 
            RevisionsSplitContainer.Size = new Size(650, 209);
            RevisionsSplitContainer.SplitterDistance = 350;
            RevisionsSplitContainer.SplitterWidth = 6;
            RevisionsSplitContainer.Panel1.Controls.Add(RevisionGridContainer);
            RevisionsSplitContainer.TabIndex = 0;
            // 
            // RevisionGridContainer
            // 
            RevisionGridContainer.Controls.Add(RevisionGrid);
            RevisionGridContainer.Controls.Add(notificationBarBisectInProgress);
            RevisionGridContainer.Controls.Add(notificationBarGitActionInProgress);
            RevisionGridContainer.Dock = DockStyle.Fill;
            RevisionGridContainer.Location = new Point(0, 0);
            RevisionGridContainer.Name = "RevisionGridContainer";
            RevisionGridContainer.Size = new Size(606, 205);
            RevisionGridContainer.TabIndex = 2;
            // 
            // RevisionGrid
            // 
            RevisionGrid.Dock = DockStyle.Fill;
            RevisionGrid.Location = new Point(0, 0);
            RevisionGrid.Name = "RevisionGrid";
            RevisionGrid.Size = new Size(350, 209);
            RevisionGrid.ShowBuildServerInfo = true;
            RevisionGrid.TabIndex = 2;
            // 
            // notificationBarBisectInProgress
            // 
            notificationBarBisectInProgress.Dock = DockStyle.Top;
            notificationBarBisectInProgress.Location = new Point(0, 33);
            notificationBarBisectInProgress.MinimumSize = new Size(0, 33);
            notificationBarBisectInProgress.Name = "notificationBarBisectInProgress";
            notificationBarBisectInProgress.Size = new Size(561, 33);
            notificationBarBisectInProgress.TabIndex = 1;
            notificationBarBisectInProgress.Visible = false;
            // 
            // notificationBarGitActionInProgress
            // 
            notificationBarGitActionInProgress.Dock = DockStyle.Top;
            notificationBarGitActionInProgress.Location = new Point(0, 0);
            notificationBarGitActionInProgress.MinimumSize = new Size(0, 33);
            notificationBarGitActionInProgress.Name = "notificationBarGitActionInProgress";
            notificationBarGitActionInProgress.Size = new Size(561, 33);
            notificationBarGitActionInProgress.TabIndex = 0;
            notificationBarGitActionInProgress.Visible = false;
            // 
            // CommitInfoTabControl
            // 
            CommitInfoTabControl.Controls.Add(CommitInfoTabPage);
            CommitInfoTabControl.Controls.Add(DiffTabPage);
            CommitInfoTabControl.Controls.Add(TreeTabPage);
            CommitInfoTabControl.Controls.Add(GpgInfoTabPage);
            CommitInfoTabControl.Dock = DockStyle.Fill;
            CommitInfoTabControl.Location = new Point(0, 0);
            CommitInfoTabControl.Margin = new Padding(0);
            CommitInfoTabControl.Name = "CommitInfoTabControl";
            CommitInfoTabControl.Padding = new Point(0, 0);
            CommitInfoTabControl.SelectedIndex = 0;
            CommitInfoTabControl.Size = new Size(650, 287);
            CommitInfoTabControl.TabIndex = 0;
            CommitInfoTabControl.SelectedIndexChanged += CommitInfoTabControl_SelectedIndexChanged;
            // 
            // CommitInfoTabPage
            // 
            CommitInfoTabPage.Controls.Add(RevisionInfo);
            CommitInfoTabPage.Location = new Point(1, 21);
            CommitInfoTabPage.Margin = new Padding(0);
            CommitInfoTabPage.Name = "CommitInfoTabPage";
            CommitInfoTabPage.Size = new Size(646, 264);
            CommitInfoTabPage.TabIndex = 2;
            CommitInfoTabPage.Text = "Commit";
            CommitInfoTabPage.UseVisualStyleBackColor = true;
            // 
            // RevisionInfo
            // 
            RevisionInfo.BackColor = SystemColors.Window;
            RevisionInfo.Cursor = Cursors.IBeam;
            RevisionInfo.Dock = DockStyle.Fill;
            RevisionInfo.Location = new Point(0, 0);
            RevisionInfo.Margin = new Padding(0);
            RevisionInfo.Name = "RevisionInfo";
            RevisionInfo.ShowBranchesAsLinks = true;
            RevisionInfo.Size = new Size(646, 264);
            RevisionInfo.TabIndex = 0;
            RevisionInfo.CommandClicked += new System.EventHandler<ResourceManager.CommandEventArgs>(RevisionInfo_CommandClicked);
            // 
            // TreeTabPage
            // 
            TreeTabPage.Controls.Add(fileTree);
            TreeTabPage.Location = new Point(1, 21);
            TreeTabPage.Margin = new Padding(0);
            TreeTabPage.Name = "TreeTabPage";
            TreeTabPage.Size = new Size(646, 264);
            TreeTabPage.TabIndex = 0;
            TreeTabPage.Text = "File tree";
            TreeTabPage.UseVisualStyleBackColor = true;
            // 
            // fileTree
            // 
            fileTree.Dock = DockStyle.Fill;
            fileTree.Location = new Point(0, 0);
            fileTree.Margin = new Padding(0);
            fileTree.Name = "fileTree";
            fileTree.Size = new Size(646, 264);
            fileTree.TabIndex = 0;
            // 
            // DiffTabPage
            // 
            DiffTabPage.Controls.Add(revisionDiff);
            DiffTabPage.Location = new Point(1, 21);
            DiffTabPage.Margin = new Padding(0);
            DiffTabPage.Name = "DiffTabPage";
            DiffTabPage.Size = new Size(646, 264);
            DiffTabPage.TabIndex = 1;
            DiffTabPage.Text = "Diff";
            DiffTabPage.UseVisualStyleBackColor = true;
            // 
            // revisionDiff
            // 
            revisionDiff.Dock = DockStyle.Fill;
            revisionDiff.Location = new Point(0, 0);
            revisionDiff.Margin = new Padding(0);
            revisionDiff.Name = "revisionDiff";
            revisionDiff.Size = new Size(646, 264);
            revisionDiff.TabIndex = 0;
            // 
            // GpgInfoTabPage
            // 
            GpgInfoTabPage.Controls.Add(revisionGpgInfo1);
            GpgInfoTabPage.Location = new Point(1, 21);
            GpgInfoTabPage.Margin = new Padding(0);
            GpgInfoTabPage.Name = "GpgInfoTabPage";
            GpgInfoTabPage.Size = new Size(646, 264);
            GpgInfoTabPage.TabIndex = 3;
            GpgInfoTabPage.Text = "GPG";
            GpgInfoTabPage.UseVisualStyleBackColor = true;
            // 
            // revisionGpgInfo1
            // 
            revisionGpgInfo1.Dock = DockStyle.Fill;
            revisionGpgInfo1.Location = new Point(0, 0);
            revisionGpgInfo1.Margin = new Padding(0);
            revisionGpgInfo1.Name = "revisionGpgInfo1";
            revisionGpgInfo1.Padding = new Padding(6);
            revisionGpgInfo1.Size = new Size(646, 264);
            revisionGpgInfo1.TabIndex = 0;
            // 
            // FilterToolTip
            // 
            FilterToolTip.AutomaticDelay = 100;
            FilterToolTip.ShowAlways = true;
            FilterToolTip.ToolTipIcon = ToolTipIcon.Error;
            FilterToolTip.ToolTipTitle = "RegEx";
            FilterToolTip.UseAnimation = false;
            FilterToolTip.UseFading = false;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(43, 19);
            fileToolStripMenuItem.GitModuleChanged += new System.EventHandler<GitModuleEventArgs>(SetGitModule);
            fileToolStripMenuItem.RecentRepositoriesCleared += fileToolStripMenuItem_RecentRepositoriesCleared;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Image = Properties.Images.DashboardFolderGit;
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(221, 22);
            closeToolStripMenuItem.Text = "&Close (go to Dashboard)";
            closeToolStripMenuItem.Click += CloseToolStripMenuItemClick;
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Image = Properties.Images.ReloadRevisions;
            refreshToolStripMenuItem.ImageTransparentColor = Color.Transparent;
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.Size = new Size(221, 22);
            refreshToolStripMenuItem.Text = "&Refresh";
            refreshToolStripMenuItem.Click += RefreshToolStripMenuItemClick;
            // 
            // refreshDashboardToolStripMenuItem
            // 
            refreshDashboardToolStripMenuItem.Image = Properties.Images.ReloadRevisions;
            refreshDashboardToolStripMenuItem.ImageTransparentColor = Color.Transparent;
            refreshDashboardToolStripMenuItem.Name = "refreshDashboardToolStripMenuItem";
            refreshDashboardToolStripMenuItem.Size = new Size(113, 22);
            refreshDashboardToolStripMenuItem.Text = "&Refresh";
            refreshDashboardToolStripMenuItem.Click += RefreshDashboardToolStripMenuItemClick;
            // 
            // fileExplorerToolStripMenuItem
            // 
            fileExplorerToolStripMenuItem.Image = Properties.Images.BrowseFileExplorer;
            fileExplorerToolStripMenuItem.Name = "fileExplorerToolStripMenuItem";
            fileExplorerToolStripMenuItem.ShortcutKeys = ((Keys)(((Keys.Control | Keys.Shift) 
            | Keys.O)));
            fileExplorerToolStripMenuItem.Size = new Size(221, 22);
            fileExplorerToolStripMenuItem.Text = "File E&xplorer";
            fileExplorerToolStripMenuItem.Click += FileExplorerToolStripMenuItemClick;
            // 
            // repositoryToolStripMenuItem
            // 
            repositoryToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            refreshToolStripMenuItem,
            fileExplorerToolStripMenuItem,
            toolStripSeparator8,
            manageRemoteRepositoriesToolStripMenuItem1,
            toolStripSeparator43,
            manageSubmodulesToolStripMenuItem,
            updateAllSubmodulesToolStripMenuItem,
            synchronizeAllSubmodulesToolStripMenuItem,
            toolStripSeparator10,
            manageWorktreeToolStripMenuItem,
            toolStripSeparator44,
            editgitignoreToolStripMenuItem1,
            editgitinfoexcludeToolStripMenuItem,
            editGitAttributesToolStripMenuItem,
            editmailmapToolStripMenuItem,
            menuitemSparse,
            toolStripSeparator4,
            gitMaintenanceToolStripMenuItem,
            repoSettingsToolStripMenuItem,
            toolStripSeparator13,
            closeToolStripMenuItem});
            repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
            repositoryToolStripMenuItem.Size = new Size(75, 20);
            repositoryToolStripMenuItem.Text = "&Repository";
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(218, 6);
            // 
            // manageRemoteRepositoriesToolStripMenuItem1
            // 
            manageRemoteRepositoriesToolStripMenuItem1.Image = Properties.Images.Remotes;
            manageRemoteRepositoriesToolStripMenuItem1.Name = "manageRemoteRepositoriesToolStripMenuItem1";
            manageRemoteRepositoriesToolStripMenuItem1.Size = new Size(221, 22);
            manageRemoteRepositoriesToolStripMenuItem1.Text = "Remo&te repositories...";
            manageRemoteRepositoriesToolStripMenuItem1.Click += ManageRemoteRepositoriesToolStripMenuItemClick;
            // 
            // toolStripSeparator43
            // 
            toolStripSeparator43.Name = "toolStripSeparator43";
            toolStripSeparator43.Size = new Size(218, 6);
            // 
            // manageSubmodulesToolStripMenuItem
            // 
            manageSubmodulesToolStripMenuItem.Image = Properties.Images.SubmodulesManage;
            manageSubmodulesToolStripMenuItem.Name = "manageSubmodulesToolStripMenuItem";
            manageSubmodulesToolStripMenuItem.Size = new Size(221, 22);
            manageSubmodulesToolStripMenuItem.Text = "Manage &submodules...";
            manageSubmodulesToolStripMenuItem.Click += ManageSubmodulesToolStripMenuItemClick;
            // 
            // updateAllSubmodulesToolStripMenuItem
            // 
            updateAllSubmodulesToolStripMenuItem.Image = Properties.Images.SubmodulesUpdate;
            updateAllSubmodulesToolStripMenuItem.Name = "updateAllSubmodulesToolStripMenuItem";
            updateAllSubmodulesToolStripMenuItem.Size = new Size(221, 22);
            updateAllSubmodulesToolStripMenuItem.Text = "&Update all submodules";
            updateAllSubmodulesToolStripMenuItem.Click += UpdateAllSubmodulesToolStripMenuItemClick;
            // 
            // synchronizeAllSubmodulesToolStripMenuItem
            // 
            synchronizeAllSubmodulesToolStripMenuItem.Image = Properties.Images.SubmodulesSync;
            synchronizeAllSubmodulesToolStripMenuItem.Name = "synchronizeAllSubmodulesToolStripMenuItem";
            synchronizeAllSubmodulesToolStripMenuItem.Size = new Size(221, 22);
            synchronizeAllSubmodulesToolStripMenuItem.Text = "Synchronize all su&bmodules";
            synchronizeAllSubmodulesToolStripMenuItem.Click += SynchronizeAllSubmodulesToolStripMenuItemClick;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(218, 6);
            // 
            // manageWorktreeToolStripMenuItem
            // 
            manageWorktreeToolStripMenuItem.Image = Properties.Images.WorkTree;
            manageWorktreeToolStripMenuItem.Name = "manageWorktreeToolStripMenuItem";
            manageWorktreeToolStripMenuItem.Size = new Size(180, 22);
            manageWorktreeToolStripMenuItem.Text = "Manage &worktrees...";
            manageWorktreeToolStripMenuItem.Click += manageWorktreeToolStripMenuItem_Click;
            // 
            // toolStripSeparator44
            // 
            toolStripSeparator44.Name = "toolStripSeparator44";
            toolStripSeparator44.Size = new Size(218, 6);
            // 
            // editgitignoreToolStripMenuItem1
            // 
            editgitignoreToolStripMenuItem1.Image = Properties.Images.EditGitIgnore;
            editgitignoreToolStripMenuItem1.Name = "editgitignoreToolStripMenuItem1";
            editgitignoreToolStripMenuItem1.Size = new Size(221, 22);
            editgitignoreToolStripMenuItem1.Text = "Edit .git&ignore";
            editgitignoreToolStripMenuItem1.Click += EditGitignoreToolStripMenuItem1Click;
            // 
            // editgitinfoexcludeToolStripMenuItem
            // 
            editgitinfoexcludeToolStripMenuItem.Name = "editgitinfoexcludeToolStripMenuItem";
            editgitinfoexcludeToolStripMenuItem.Size = new Size(221, 22);
            editgitinfoexcludeToolStripMenuItem.Text = "Edit .git/info/&exclude";
            editgitinfoexcludeToolStripMenuItem.Click += EditGitInfoExcludeToolStripMenuItemClick;
            // 
            // editGitAttributesToolStripMenuItem
            // 
            editGitAttributesToolStripMenuItem.Name = "editGitAttributesToolStripMenuItem";
            editGitAttributesToolStripMenuItem.Size = new Size(221, 22);
            editGitAttributesToolStripMenuItem.Text = "Edit .git&attributes";
            editGitAttributesToolStripMenuItem.Click += editGitAttributesToolStripMenuItem_Click;
            // 
            // editmailmapToolStripMenuItem
            // 
            editmailmapToolStripMenuItem.Name = "editmailmapToolStripMenuItem";
            editmailmapToolStripMenuItem.Size = new Size(221, 22);
            editmailmapToolStripMenuItem.Text = "Edit .&mailmap";
            editmailmapToolStripMenuItem.Click += EditMailMapToolStripMenuItemClick;
            // 
            // menuitemSparse
            // 
            menuitemSparse.Name = "menuitemSparse";
            menuitemSparse.Size = new Size(221, 22);
            menuitemSparse.Text = "Sparse Wor&king Copy";
            menuitemSparse.Click += menuitemSparseWorkingCopy_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(218, 6);
            // 
            // gitMaintenanceToolStripMenuItem
            // 
            gitMaintenanceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            compressGitDatabaseToolStripMenuItem,
            recoverLostObjectsToolStripMenuItem,
            deleteIndexLockToolStripMenuItem,
            editLocalGitConfigToolStripMenuItem});
            gitMaintenanceToolStripMenuItem.Image = Properties.Images.Maintenance;
            gitMaintenanceToolStripMenuItem.Name = "gitMaintenanceToolStripMenuItem";
            gitMaintenanceToolStripMenuItem.Size = new Size(221, 22);
            gitMaintenanceToolStripMenuItem.Text = "&Git maintenance";
            // 
            // compressGitDatabaseToolStripMenuItem
            // 
            compressGitDatabaseToolStripMenuItem.Image = Properties.Images.CompressGitDatabase;
            compressGitDatabaseToolStripMenuItem.Name = "compressGitDatabaseToolStripMenuItem";
            compressGitDatabaseToolStripMenuItem.Size = new Size(194, 22);
            compressGitDatabaseToolStripMenuItem.Text = "&Compress git database";
            compressGitDatabaseToolStripMenuItem.Click += CompressGitDatabaseToolStripMenuItemClick;
            // 
            // recoverLostObjectsToolStripMenuItem
            // 
            recoverLostObjectsToolStripMenuItem.Image = Properties.Images.RecoverLostObjects;
            recoverLostObjectsToolStripMenuItem.Name = "recoverLostObjectsToolStripMenuItem";
            recoverLostObjectsToolStripMenuItem.Size = new Size(194, 22);
            recoverLostObjectsToolStripMenuItem.Text = "&Recover lost objects...";
            recoverLostObjectsToolStripMenuItem.Click += recoverLostObjectsToolStripMenuItemClick;
            // 
            // deleteIndexLockToolStripMenuItem
            // 
            deleteIndexLockToolStripMenuItem.Image = Properties.Images.DeleteIndexLock;
            deleteIndexLockToolStripMenuItem.Name = "deleteIndexLockToolStripMenuItem";
            deleteIndexLockToolStripMenuItem.Size = new Size(194, 22);
            deleteIndexLockToolStripMenuItem.Text = "&Delete index.lock";
            deleteIndexLockToolStripMenuItem.Click += deleteIndexLockToolStripMenuItem_Click;
            // 
            // editLocalGitConfigToolStripMenuItem
            // 
            editLocalGitConfigToolStripMenuItem.Image = Properties.Images.EditGitConfig;
            editLocalGitConfigToolStripMenuItem.Name = "editLocalGitConfigToolStripMenuItem";
            editLocalGitConfigToolStripMenuItem.Size = new Size(194, 22);
            editLocalGitConfigToolStripMenuItem.Text = "&Edit .git/config";
            editLocalGitConfigToolStripMenuItem.Click += EditLocalGitConfigToolStripMenuItemClick;
            // 
            // repoSettingsToolStripMenuItem
            // 
            repoSettingsToolStripMenuItem.Image = Properties.Images.Settings;
            repoSettingsToolStripMenuItem.Name = "repoSettingsToolStripMenuItem";
            repoSettingsToolStripMenuItem.Size = new Size(221, 22);
            repoSettingsToolStripMenuItem.Text = "Rep&ository settings...";
            repoSettingsToolStripMenuItem.Click += RepoSettingsToolStripMenuItemClick;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(218, 6);
            // 
            // commandsToolStripMenuItem
            // 
            commandsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            commitToolStripMenuItem,
            undoLastCommitToolStripMenuItem,
            pullToolStripMenuItem,
            pushToolStripMenuItem,
            toolStripSeparator21,
            stashToolStripMenuItem,
            resetToolStripMenuItem,
            cleanupToolStripMenuItem,
            toolStripSeparator25,
            branchToolStripMenuItem,
            deleteBranchToolStripMenuItem,
            checkoutBranchToolStripMenuItem,
            mergeBranchToolStripMenuItem,
            rebaseToolStripMenuItem,
            runMergetoolToolStripMenuItem,
            toolStripSeparator45,
            tagToolStripMenuItem,
            deleteTagToolStripMenuItem,
            toolStripSeparator23,
            cherryPickToolStripMenuItem,
            archiveToolStripMenuItem,
            checkoutToolStripMenuItem,
            bisectToolStripMenuItem,
            toolStripMenuItemReflog,
            toolStripSeparator22,
            formatPatchToolStripMenuItem,
            applyPatchToolStripMenuItem,
            patchToolStripMenuItem});
            commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            commandsToolStripMenuItem.Size = new Size(81, 20);
            commandsToolStripMenuItem.Text = "&Commands";
            // 
            // commitToolStripMenuItem
            // 
            commitToolStripMenuItem.Image = Properties.Images.RepoStateClean;
            commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            commitToolStripMenuItem.Size = new Size(213, 22);
            commitToolStripMenuItem.Text = "&Commit...";
            commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
            // 
            // undoLastCommitToolStripMenuItem
            // 
            undoLastCommitToolStripMenuItem.Image = Properties.Images.ResetFileTo;
            undoLastCommitToolStripMenuItem.Name = "undoLastCommitToolStripMenuItem";
            undoLastCommitToolStripMenuItem.Size = new Size(213, 22);
            undoLastCommitToolStripMenuItem.Text = "&Undo last commit...";
            undoLastCommitToolStripMenuItem.Click += undoLastCommitToolStripMenuItem_Click;
            // 
            // pullToolStripMenuItem
            // 
            pullToolStripMenuItem.Image = Properties.Images.Pull;
            pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            pullToolStripMenuItem.Size = new Size(213, 22);
            pullToolStripMenuItem.Text = "Pull&/Fetch...";
            pullToolStripMenuItem.Click += PullToolStripMenuItemClick;
            // 
            // pushToolStripMenuItem
            // 
            pushToolStripMenuItem.Image = Properties.Images.Push;
            pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            pushToolStripMenuItem.Size = new Size(213, 22);
            pushToolStripMenuItem.Text = "&Push...";
            pushToolStripMenuItem.Click += PushToolStripMenuItemClick;
            // 
            // toolStripSeparator21
            // 
            toolStripSeparator21.Name = "toolStripSeparator21";
            toolStripSeparator21.Size = new Size(210, 6);
            // 
            // stashToolStripMenuItem
            // 
            stashToolStripMenuItem.Image = Properties.Images.Stash;
            stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            stashToolStripMenuItem.Size = new Size(213, 22);
            stashToolStripMenuItem.Text = "Ma&nage stashes...";
            stashToolStripMenuItem.Click += StashToolStripMenuItemClick;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Image = Properties.Images.ResetWorkingDirChanges;
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new Size(213, 22);
            resetToolStripMenuItem.Text = "&Reset changes...";
            resetToolStripMenuItem.Click += ResetToolStripMenuItem_Click;
            // 
            // cleanupToolStripMenuItem
            // 
            cleanupToolStripMenuItem.Image = Properties.Images.CleanupRepo;
            cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
            cleanupToolStripMenuItem.Size = new Size(213, 22);
            cleanupToolStripMenuItem.Text = "Clean &working directory...";
            cleanupToolStripMenuItem.Click += CleanupToolStripMenuItemClick;
            // 
            // toolStripSeparator25
            // 
            toolStripSeparator25.Name = "toolStripSeparator25";
            toolStripSeparator25.Size = new Size(210, 6);
            // 
            // branchToolStripMenuItem
            // 
            branchToolStripMenuItem.Image = Properties.Images.BranchCreate;
            branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            branchToolStripMenuItem.Size = new Size(213, 22);
            branchToolStripMenuItem.Text = "Create &branch...";
            branchToolStripMenuItem.Click += CreateBranchToolStripMenuItemClick;
            // 
            // deleteBranchToolStripMenuItem
            // 
            deleteBranchToolStripMenuItem.Image = Properties.Images.BranchDelete;
            deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            deleteBranchToolStripMenuItem.Size = new Size(213, 22);
            deleteBranchToolStripMenuItem.Text = "De&lete branch...";
            deleteBranchToolStripMenuItem.Click += DeleteBranchToolStripMenuItemClick;
            // 
            // checkoutBranchToolStripMenuItem
            // 
            checkoutBranchToolStripMenuItem.Image = Properties.Images.BranchCheckout;
            checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            checkoutBranchToolStripMenuItem.Size = new Size(213, 22);
            checkoutBranchToolStripMenuItem.Text = "Chec&kout branch...";
            checkoutBranchToolStripMenuItem.Click += CheckoutBranchToolStripMenuItemClick;
            // 
            // mergeBranchToolStripMenuItem
            // 
            mergeBranchToolStripMenuItem.Image = Properties.Images.Merge;
            mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            mergeBranchToolStripMenuItem.Size = new Size(213, 22);
            mergeBranchToolStripMenuItem.Text = "&Merge branches...";
            mergeBranchToolStripMenuItem.Click += MergeBranchToolStripMenuItemClick;
            // 
            // rebaseToolStripMenuItem
            // 
            rebaseToolStripMenuItem.Image = Properties.Images.Rebase;
            rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            rebaseToolStripMenuItem.Size = new Size(213, 22);
            rebaseToolStripMenuItem.Text = "R&ebase...";
            rebaseToolStripMenuItem.Click += RebaseToolStripMenuItemClick;
            // 
            // runMergetoolToolStripMenuItem
            // 
            runMergetoolToolStripMenuItem.Image = Properties.Images.Unmerged;
            runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            runMergetoolToolStripMenuItem.Size = new Size(213, 22);
            runMergetoolToolStripMenuItem.Text = "&Solve merge conflicts...";
            runMergetoolToolStripMenuItem.Click += RunMergetoolToolStripMenuItemClick;
            // 
            // toolStripSeparator45
            // 
            toolStripSeparator45.Name = "toolStripSeparator45";
            toolStripSeparator45.Size = new Size(210, 6);
            // 
            // tagToolStripMenuItem
            // 
            tagToolStripMenuItem.Image = Properties.Images.TagCreate;
            tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            tagToolStripMenuItem.Size = new Size(213, 22);
            tagToolStripMenuItem.Text = "Create &tag...";
            tagToolStripMenuItem.Click += TagToolStripMenuItemClick;
            // 
            // deleteTagToolStripMenuItem
            // 
            deleteTagToolStripMenuItem.Image = Properties.Images.TagDelete;
            deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            deleteTagToolStripMenuItem.Size = new Size(213, 22);
            deleteTagToolStripMenuItem.Text = "&Delete tag...";
            deleteTagToolStripMenuItem.Click += DeleteTagToolStripMenuItemClick;
            // 
            // toolStripSeparator23
            // 
            toolStripSeparator23.Name = "toolStripSeparator23";
            toolStripSeparator23.Size = new Size(210, 6);
            // 
            // cherryPickToolStripMenuItem
            // 
            cherryPickToolStripMenuItem.Image = Properties.Images.CherryPick;
            cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            cherryPickToolStripMenuItem.Size = new Size(213, 22);
            cherryPickToolStripMenuItem.Text = "Cherr&y pick...";
            cherryPickToolStripMenuItem.Click += CherryPickToolStripMenuItemClick;
            // 
            // archiveToolStripMenuItem
            // 
            archiveToolStripMenuItem.Image = Properties.Images.ArchiveRevision;
            archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            archiveToolStripMenuItem.Size = new Size(213, 22);
            archiveToolStripMenuItem.Text = "Archi&ve revision...";
            archiveToolStripMenuItem.Click += ArchiveToolStripMenuItemClick;
            // 
            // checkoutToolStripMenuItem
            // 
            checkoutToolStripMenuItem.Image = Properties.Images.Checkout;
            checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            checkoutToolStripMenuItem.Size = new Size(213, 22);
            checkoutToolStripMenuItem.Text = "Check&out revision...";
            checkoutToolStripMenuItem.Click += CheckoutToolStripMenuItemClick;
            // 
            // bisectToolStripMenuItem
            // 
            bisectToolStripMenuItem.Image = Properties.Images.Bisect;
            bisectToolStripMenuItem.Name = "bisectToolStripMenuItem";
            bisectToolStripMenuItem.Size = new Size(213, 22);
            bisectToolStripMenuItem.Text = "B&isect...";
            bisectToolStripMenuItem.Click += BisectClick;
            // 
            // toolStripMenuItemReflog
            // 
            toolStripMenuItemReflog.Image = Properties.Images.Book;
            toolStripMenuItemReflog.Name = "toolStripMenuItemReflog";
            toolStripMenuItemReflog.Size = new Size(213, 22);
            toolStripMenuItemReflog.Text = "Show reflo&g...";
            toolStripMenuItemReflog.Click += toolStripMenuItemReflog_Click;
            // 
            // toolStripSeparator22
            // 
            toolStripSeparator22.Name = "toolStripSeparator22";
            toolStripSeparator22.Size = new Size(210, 6);
            // 
            // formatPatchToolStripMenuItem
            // 
            formatPatchToolStripMenuItem.Image = Properties.Images.PatchFormat;
            formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            formatPatchToolStripMenuItem.Size = new Size(213, 22);
            formatPatchToolStripMenuItem.Text = "&Format patch...";
            formatPatchToolStripMenuItem.Click += FormatPatchToolStripMenuItemClick;
            // 
            // applyPatchToolStripMenuItem
            // 
            applyPatchToolStripMenuItem.Image = Properties.Images.PatchApply;
            applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            applyPatchToolStripMenuItem.Size = new Size(213, 22);
            applyPatchToolStripMenuItem.Text = "&Apply patch...";
            applyPatchToolStripMenuItem.Click += ApplyPatchToolStripMenuItemClick;
            // 
            // patchToolStripMenuItem
            // 
            patchToolStripMenuItem.Image = Properties.Images.PatchView;
            patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            patchToolStripMenuItem.Size = new Size(213, 22);
            patchToolStripMenuItem.Text = "View patc&h file...";
            patchToolStripMenuItem.Click += PatchToolStripMenuItemClick;
            // 
            // toolStripSeparator46
            // 
            toolStripSeparator46.Name = "toolStripSeparator46";
            toolStripSeparator46.Size = new Size(268, 6);
            // 
            // toolStripSeparator42
            // 
            toolStripSeparator42.Name = "toolStripSeparator42";
            toolStripSeparator42.Size = new Size(110, 6);
            // 
            // _repositoryHostsToolStripMenuItem
            // 
            _repositoryHostsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            _forkCloneRepositoryToolStripMenuItem,
            _viewPullRequestsToolStripMenuItem,
            _createPullRequestsToolStripMenuItem,
            _addUpstreamRemoteToolStripMenuItem});
            _repositoryHostsToolStripMenuItem.Name = "_repositoryHostsToolStripMenuItem";
            _repositoryHostsToolStripMenuItem.Size = new Size(114, 20);
            _repositoryHostsToolStripMenuItem.Text = "(Repository hosts)";
            // 
            // _forkCloneRepositoryToolStripMenuItem
            // 
            _forkCloneRepositoryToolStripMenuItem.Name = "_forkCloneRepositoryToolStripMenuItem";
            _forkCloneRepositoryToolStripMenuItem.Size = new Size(198, 22);
            _forkCloneRepositoryToolStripMenuItem.Text = "&Fork/Clone repository...";
            _forkCloneRepositoryToolStripMenuItem.Click += _forkCloneMenuItem_Click;
            // 
            // _viewPullRequestsToolStripMenuItem
            // 
            _viewPullRequestsToolStripMenuItem.Name = "_viewPullRequestsToolStripMenuItem";
            _viewPullRequestsToolStripMenuItem.Size = new Size(198, 22);
            _viewPullRequestsToolStripMenuItem.Text = "View &pull requests...";
            _viewPullRequestsToolStripMenuItem.Click += _viewPullRequestsToolStripMenuItem_Click;
            // 
            // _createPullRequestsToolStripMenuItem
            // 
            _createPullRequestsToolStripMenuItem.Name = "_createPullRequestsToolStripMenuItem";
            _createPullRequestsToolStripMenuItem.Size = new Size(198, 22);
            _createPullRequestsToolStripMenuItem.Text = "&Create pull requests...";
            _createPullRequestsToolStripMenuItem.Click += _createPullRequestToolStripMenuItem_Click;
            // 
            // dashboardToolStripMenuItem
            // 
            dashboardToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            refreshDashboardToolStripMenuItem,
            toolStripSeparator42});
            dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            dashboardToolStripMenuItem.Size = new Size(76, 20);
            dashboardToolStripMenuItem.Text = "&Dashboard";
            // 
            // pluginsToolStripMenuItem
            // 
            pluginsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pluginsLoadingToolStripMenuItem, toolStripSeparator15, pluginSettingsToolStripMenuItem });
            pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            pluginsToolStripMenuItem.Size = new Size(58, 20);
            pluginsToolStripMenuItem.Text = "&Plugins";
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(150, 6);
            // 
            // pluginSettingsToolStripMenuItem
            // 
            pluginSettingsToolStripMenuItem.Image = Properties.Images.Settings;
            pluginSettingsToolStripMenuItem.Name = "pluginSettingsToolStripMenuItem";
            pluginSettingsToolStripMenuItem.Size = new Size(153, 22);
            pluginSettingsToolStripMenuItem.Text = "Plugins &settings...";
            pluginSettingsToolStripMenuItem.Click += PluginSettingsToolStripMenuItemClick;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 19);
            toolsToolStripMenuItem.Text = "&Tools";
            toolsToolStripMenuItem.SettingsChanged += new System.EventHandler<GitUI.CommandsDialogs.Menus.SettingsChangedEventArgs>(toolsToolStripMenuItem_SettingsChanged);
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.ClickThrough = true;
            mainMenuStrip.Items.AddRange(new ToolStripItem[] {
            fileToolStripMenuItem,
            dashboardToolStripMenuItem,
            repositoryToolStripMenuItem,
            commandsToolStripMenuItem,
            _repositoryHostsToolStripMenuItem,
            pluginsToolStripMenuItem,
            toolsToolStripMenuItem,
            helpToolStripMenuItem});
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Size = new Size(923, 24);
            mainMenuStrip.Padding = new Padding(4);
            mainMenuStrip.TabIndex = 0;
            // 
            // toolPanel
            // 
            toolPanel.BottomToolStripPanelVisible = false;
            // 
            // toolPanel.ContentPanel
            // 
            toolPanel.ContentPanel.Controls.Add(MainSplitContainer);
            toolPanel.ContentPanel.Margin = new Padding(0);
            toolPanel.ContentPanel.Padding = new Padding(6);
            toolPanel.ContentPanel.Size = new Size(1846, 1023);
            toolPanel.Dock = DockStyle.Fill;
            toolPanel.LeftToolStripPanelVisible = false;
            toolPanel.Location = new Point(0, 24);
            toolPanel.Margin = new Padding(0);
            toolPanel.Name = "toolPanel";
            toolPanel.Padding = new Padding(0);
            toolPanel.TopToolStripPanel.Padding = new Padding(4, 0, 4, 0);
            toolPanel.RightToolStripPanelVisible = false;
            toolPanel.Size = new Size(923, 527);
            toolPanel.TabIndex = 1;
            // 
            // toolPanel.TopToolStripPanel
            // 
            toolPanel.TopToolStripPanel.Controls.Add(ToolStripMain);
            toolPanel.TopToolStripPanel.Controls.Add(ToolStripFilters);
            toolPanel.TopToolStripPanel.Controls.Add(ToolStripScripts);
            // 
            // ToolStripFilters
            // 
            ToolStripFilters.Name = "ToolStripFilters";
            ToolStripFilters.TabIndex = 1;
            ToolStripFilters.Text = "Filters";
            // 
            // addUpstreamRemoteToolStripMenuItem
            // 
            _addUpstreamRemoteToolStripMenuItem.Name = "_addUpstreamRemoteToolStripMenuItem";
            _addUpstreamRemoteToolStripMenuItem.Size = new Size(360, 38);
            _addUpstreamRemoteToolStripMenuItem.Text = "&Add upstream remote";
            _addUpstreamRemoteToolStripMenuItem.Click += _addUpstreamRemoteToolStripMenuItem_Click;
            // 
            // toolStripScripts
            // 
            ToolStripScripts.ClickThrough = true;
            ToolStripScripts.Dock = DockStyle.None;
            ToolStripScripts.DrawBorder = false;
            ToolStripScripts.GripEnabled = false;
            ToolStripScripts.GripMargin = new Padding(0);
            ToolStripScripts.ImeMode = ImeMode.NoControl;
            ToolStripScripts.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolStripScripts.Location = new Point(890, 0);
            ToolStripScripts.Name = "ToolStripScripts";
            ToolStripScripts.Padding = new Padding(0);
            ToolStripScripts.Size = new Size(43, 25);
            ToolStripScripts.TabIndex = 2;
            ToolStripScripts.Text = "Scripts";
            // 
            // pluginsLoadingToolStripMenuItem
            // 
            pluginsLoadingToolStripMenuItem.Enabled = false;
            pluginsLoadingToolStripMenuItem.Name = "pluginsLoadingToolStripMenuItem";
            pluginsLoadingToolStripMenuItem.Size = new Size(180, 22);
            pluginsLoadingToolStripMenuItem.Text = "Loading...";
            // 
            // FormBrowse
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            ClientSize = new Size(923, 573);
            Controls.Add(toolPanel);
            Controls.Add(mainMenuStrip);
            Name = "FormBrowse";
            Text = "Git Extensions";
            ToolStripMain.ResumeLayout(false);
            ToolStripMain.PerformLayout();
            MainSplitContainer.Panel1.ResumeLayout(false);
            MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(MainSplitContainer)).EndInit();
            MainSplitContainer.ResumeLayout(false);
            RightSplitContainer.Panel1.ResumeLayout(false);
            RightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(RightSplitContainer)).EndInit();
            RightSplitContainer.ResumeLayout(false);
            RevisionsSplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(RevisionsSplitContainer)).EndInit();
            RevisionsSplitContainer.ResumeLayout(false);
            RevisionGridContainer.ResumeLayout(false);
            CommitInfoTabControl.ResumeLayout(false);
            CommitInfoTabPage.ResumeLayout(false);
            TreeTabPage.ResumeLayout(false);
            DiffTabPage.ResumeLayout(false);
            GpgInfoTabPage.ResumeLayout(false);
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(gitRevisionBindingSource)).EndInit();
            toolPanel.ContentPanel.ResumeLayout(false);
            toolPanel.TopToolStripPanel.ResumeLayout(false);
            toolPanel.TopToolStripPanel.PerformLayout();
            toolPanel.ResumeLayout(false);
            toolPanel.PerformLayout();
            ToolStripFilters.ResumeLayout(false);
            ToolStripFilters.PerformLayout();
            ToolStripScripts.ResumeLayout(false);
            ToolStripScripts.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private GitUI.LeftPanel.RepoObjectsTree repoObjectsTree;
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
        private WorkingDirectoryToolStripSplitButton _NO_TRANSLATE_WorkingDir;
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
        private ToolStripButton toggleLeftPanel;
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
        private ToolStripMenuItem pluginsLoadingToolStripMenuItem;
    }
}
