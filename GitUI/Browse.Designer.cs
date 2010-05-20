namespace GitUI
{
    partial class FormBrowse
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBrowse));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.Workingdir = new System.Windows.Forms.ToolStripSplitButton();
            this.CurrentBranch = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitStash = new System.Windows.Forms.ToolStripSplitButton();
            this.stashChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashPopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.viewStashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPull = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPush = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.GitBash = new System.Windows.Forms.ToolStripButton();
            this.EditSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.CommitInfo = new System.Windows.Forms.TabPage();
            this.RevisionInfo = new GitUI.CommitInfo();
            this.Tree = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.GitTree = new System.Windows.Forms.TreeView();
            this.FileText = new GitUI.FileViewer();
            this.Diff = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffText = new GitUI.FileViewer();
            this.TreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitBashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitGUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kGitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.branchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runMergetoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageRemoteRepositoriesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.PuTTYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAuthenticationAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateOrImportKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.updateAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.updateAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncronizeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.openSubmoduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.gitMaintenanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verifyGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.editgitignoreToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editmailmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitcountPerUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitcommandLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changelogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.CommitInfo.SuspendLayout();
            this.Tree.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.Diff.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.TreeContextMenu.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.AccessibleDescription = null;
            this.splitContainer2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.BackgroundImage = null;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Font = null;
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.AccessibleDescription = null;
            this.splitContainer2.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
            this.splitContainer2.Panel1.BackgroundImage = null;
            this.splitContainer2.Panel1.Controls.Add(this.ToolStrip);
            this.splitContainer2.Panel1.Font = null;
            this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AccessibleDescription = null;
            this.splitContainer2.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
            this.splitContainer2.Panel2.BackgroundImage = null;
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Panel2.Font = null;
            this.splitContainer2.TabStop = false;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // ToolStrip
            // 
            this.ToolStrip.AccessibleDescription = null;
            this.ToolStrip.AccessibleName = null;
            resources.ApplyResources(this.ToolStrip, "ToolStrip");
            this.ToolStrip.BackgroundImage = null;
            this.ToolStrip.Font = null;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshButton,
            this.toolStripLabel1,
            this.Workingdir,
            this.CurrentBranch,
            this.toolStripSeparator1,
            this.toolStripSplitStash,
            this.toolStripButton1,
            this.toolStripButtonPull,
            this.toolStripButtonPush,
            this.toolStripSeparator2,
            this.GitBash,
            this.EditSettings,
            this.toolStripSeparator5,
            this.toolStripLabel2,
            this.toolStripTextBoxFilter});
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolStrip_ItemClicked);
            // 
            // RefreshButton
            // 
            this.RefreshButton.AccessibleDescription = null;
            this.RefreshButton.AccessibleName = null;
            resources.ApplyResources(this.RefreshButton, "RefreshButton");
            this.RefreshButton.BackgroundImage = null;
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.AccessibleDescription = null;
            this.toolStripLabel1.AccessibleName = null;
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            this.toolStripLabel1.BackgroundImage = null;
            this.toolStripLabel1.Name = "toolStripLabel1";
            // 
            // Workingdir
            // 
            this.Workingdir.AccessibleDescription = null;
            this.Workingdir.AccessibleName = null;
            resources.ApplyResources(this.Workingdir, "Workingdir");
            this.Workingdir.BackgroundImage = null;
            this.Workingdir.Name = "Workingdir";
            this.Workingdir.ButtonClick += new System.EventHandler(this.Workingdir_Click_1);
            this.Workingdir.DropDownOpening += new System.EventHandler(this.Workingdir_DropDownOpening);
            // 
            // CurrentBranch
            // 
            this.CurrentBranch.AccessibleDescription = null;
            this.CurrentBranch.AccessibleName = null;
            resources.ApplyResources(this.CurrentBranch, "CurrentBranch");
            this.CurrentBranch.BackgroundImage = null;
            this.CurrentBranch.Name = "CurrentBranch";
            this.CurrentBranch.Click += new System.EventHandler(this.CurrentBranch_Click_1);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.AccessibleDescription = null;
            this.toolStripSeparator1.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // toolStripSplitStash
            // 
            this.toolStripSplitStash.AccessibleDescription = null;
            this.toolStripSplitStash.AccessibleName = null;
            resources.ApplyResources(this.toolStripSplitStash, "toolStripSplitStash");
            this.toolStripSplitStash.BackgroundImage = null;
            this.toolStripSplitStash.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitStash.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stashChangesToolStripMenuItem,
            this.stashPopToolStripMenuItem,
            this.toolStripSeparator9,
            this.viewStashToolStripMenuItem});
            this.toolStripSplitStash.Image = global::GitUI.Properties.Resources.stash1;
            this.toolStripSplitStash.Name = "toolStripSplitStash";
            this.toolStripSplitStash.ButtonClick += new System.EventHandler(this.toolStripSplitStash_ButtonClick);
            // 
            // stashChangesToolStripMenuItem
            // 
            this.stashChangesToolStripMenuItem.AccessibleDescription = null;
            this.stashChangesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.stashChangesToolStripMenuItem, "stashChangesToolStripMenuItem");
            this.stashChangesToolStripMenuItem.BackgroundImage = null;
            this.stashChangesToolStripMenuItem.Name = "stashChangesToolStripMenuItem";
            this.stashChangesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.stashChangesToolStripMenuItem.Click += new System.EventHandler(this.stashChangesToolStripMenuItem_Click);
            // 
            // stashPopToolStripMenuItem
            // 
            this.stashPopToolStripMenuItem.AccessibleDescription = null;
            this.stashPopToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.stashPopToolStripMenuItem, "stashPopToolStripMenuItem");
            this.stashPopToolStripMenuItem.BackgroundImage = null;
            this.stashPopToolStripMenuItem.Name = "stashPopToolStripMenuItem";
            this.stashPopToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.stashPopToolStripMenuItem.Click += new System.EventHandler(this.stashPopToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.AccessibleDescription = null;
            this.toolStripSeparator9.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator9, "toolStripSeparator9");
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            // 
            // viewStashToolStripMenuItem
            // 
            this.viewStashToolStripMenuItem.AccessibleDescription = null;
            this.viewStashToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.viewStashToolStripMenuItem, "viewStashToolStripMenuItem");
            this.viewStashToolStripMenuItem.BackgroundImage = null;
            this.viewStashToolStripMenuItem.Name = "viewStashToolStripMenuItem";
            this.viewStashToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.viewStashToolStripMenuItem.Click += new System.EventHandler(this.viewStashToolStripMenuItem_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.AccessibleDescription = null;
            this.toolStripButton1.AccessibleName = null;
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.BackgroundImage = null;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButtonPull
            // 
            this.toolStripButtonPull.AccessibleDescription = null;
            this.toolStripButtonPull.AccessibleName = null;
            resources.ApplyResources(this.toolStripButtonPull, "toolStripButtonPull");
            this.toolStripButtonPull.BackgroundImage = null;
            this.toolStripButtonPull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPull.Image = global::GitUI.Properties.Resources._4;
            this.toolStripButtonPull.Name = "toolStripButtonPull";
            this.toolStripButtonPull.Click += new System.EventHandler(this.toolStripButtonPull_Click);
            // 
            // toolStripButtonPush
            // 
            this.toolStripButtonPush.AccessibleDescription = null;
            this.toolStripButtonPush.AccessibleName = null;
            resources.ApplyResources(this.toolStripButtonPush, "toolStripButtonPush");
            this.toolStripButtonPush.BackgroundImage = null;
            this.toolStripButtonPush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPush.Image = global::GitUI.Properties.Resources._31;
            this.toolStripButtonPush.Name = "toolStripButtonPush";
            this.toolStripButtonPush.Click += new System.EventHandler(this.toolStripButtonPush_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.AccessibleDescription = null;
            this.toolStripSeparator2.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // GitBash
            // 
            this.GitBash.AccessibleDescription = null;
            this.GitBash.AccessibleName = null;
            resources.ApplyResources(this.GitBash, "GitBash");
            this.GitBash.BackgroundImage = null;
            this.GitBash.Name = "GitBash";
            this.GitBash.Click += new System.EventHandler(this.GitBash_Click);
            // 
            // EditSettings
            // 
            this.EditSettings.AccessibleDescription = null;
            this.EditSettings.AccessibleName = null;
            resources.ApplyResources(this.EditSettings, "EditSettings");
            this.EditSettings.BackgroundImage = null;
            this.EditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.EditSettings.Name = "EditSettings";
            this.EditSettings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.AccessibleDescription = null;
            this.toolStripSeparator5.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.AccessibleDescription = null;
            this.toolStripLabel2.AccessibleName = null;
            resources.ApplyResources(this.toolStripLabel2, "toolStripLabel2");
            this.toolStripLabel2.BackgroundImage = null;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Click += new System.EventHandler(this.toolStripLabel2_Click);
            // 
            // toolStripTextBoxFilter
            // 
            this.toolStripTextBoxFilter.AccessibleDescription = null;
            this.toolStripTextBoxFilter.AccessibleName = null;
            resources.ApplyResources(this.toolStripTextBoxFilter, "toolStripTextBoxFilter");
            this.toolStripTextBoxFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.toolStripTextBoxFilter.ForeColor = System.Drawing.Color.Black;
            this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            this.toolStripTextBoxFilter.Leave += new System.EventHandler(this.toolStripTextBoxFilter_Leave);
            this.toolStripTextBoxFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBoxFilter_KeyPress);
            this.toolStripTextBoxFilter.Click += new System.EventHandler(this.toolStripTextBox1_Click);
            // 
            // splitContainer3
            // 
            this.splitContainer3.AccessibleDescription = null;
            this.splitContainer3.AccessibleName = null;
            resources.ApplyResources(this.splitContainer3, "splitContainer3");
            this.splitContainer3.BackgroundImage = null;
            this.splitContainer3.Font = null;
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.AccessibleDescription = null;
            this.splitContainer3.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer3.Panel1, "splitContainer3.Panel1");
            this.splitContainer3.Panel1.BackgroundImage = null;
            this.splitContainer3.Panel1.Controls.Add(this.RevisionGrid);
            this.splitContainer3.Panel1.Font = null;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.AccessibleDescription = null;
            this.splitContainer3.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer3.Panel2, "splitContainer3.Panel2");
            this.splitContainer3.Panel2.BackgroundImage = null;
            this.splitContainer3.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer3.Panel2.Font = null;
            this.splitContainer3.TabStop = false;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.AccessibleDescription = null;
            this.RevisionGrid.AccessibleName = null;
            resources.ApplyResources(this.RevisionGrid, "RevisionGrid");
            this.RevisionGrid.BackgroundImage = null;
            this.RevisionGrid.currentCheckout = "\nfatal: Not a git repository\n";
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.Font = null;
            this.RevisionGrid.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RevisionGrid.DoubleClick += new System.EventHandler(this.RevisionGrid_DoubleClick);
            this.RevisionGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RevisionGrid_MouseDoubleClick);
            // 
            // tabControl1
            // 
            this.tabControl1.AccessibleDescription = null;
            this.tabControl1.AccessibleName = null;
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.BackgroundImage = null;
            this.tabControl1.Controls.Add(this.CommitInfo);
            this.tabControl1.Controls.Add(this.Tree);
            this.tabControl1.Controls.Add(this.Diff);
            this.tabControl1.Font = null;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // CommitInfo
            // 
            this.CommitInfo.AccessibleDescription = null;
            this.CommitInfo.AccessibleName = null;
            resources.ApplyResources(this.CommitInfo, "CommitInfo");
            this.CommitInfo.BackgroundImage = null;
            this.CommitInfo.Controls.Add(this.RevisionInfo);
            this.CommitInfo.Font = null;
            this.CommitInfo.Name = "CommitInfo";
            this.CommitInfo.UseVisualStyleBackColor = true;
            // 
            // RevisionInfo
            // 
            this.RevisionInfo.AccessibleDescription = null;
            this.RevisionInfo.AccessibleName = null;
            resources.ApplyResources(this.RevisionInfo, "RevisionInfo");
            this.RevisionInfo.BackColor = System.Drawing.SystemColors.Window;
            this.RevisionInfo.BackgroundImage = null;
            this.RevisionInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RevisionInfo.Font = null;
            this.RevisionInfo.Name = "RevisionInfo";
            // 
            // Tree
            // 
            this.Tree.AccessibleDescription = null;
            this.Tree.AccessibleName = null;
            resources.ApplyResources(this.Tree, "Tree");
            this.Tree.BackgroundImage = null;
            this.Tree.Controls.Add(this.splitContainer4);
            this.Tree.Font = null;
            this.Tree.Name = "Tree";
            this.Tree.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.AccessibleDescription = null;
            this.splitContainer4.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4, "splitContainer4");
            this.splitContainer4.BackgroundImage = null;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Font = null;
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.AccessibleDescription = null;
            this.splitContainer4.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4.Panel1, "splitContainer4.Panel1");
            this.splitContainer4.Panel1.BackgroundImage = null;
            this.splitContainer4.Panel1.Controls.Add(this.GitTree);
            this.splitContainer4.Panel1.Font = null;
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.AccessibleDescription = null;
            this.splitContainer4.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4.Panel2, "splitContainer4.Panel2");
            this.splitContainer4.Panel2.BackgroundImage = null;
            this.splitContainer4.Panel2.Controls.Add(this.FileText);
            this.splitContainer4.Panel2.Font = null;
            // 
            // GitTree
            // 
            this.GitTree.AccessibleDescription = null;
            this.GitTree.AccessibleName = null;
            resources.ApplyResources(this.GitTree, "GitTree");
            this.GitTree.BackgroundImage = null;
            this.GitTree.Font = null;
            this.GitTree.HideSelection = false;
            this.GitTree.Name = "GitTree";
            this.GitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTree_BeforeExpand);
            this.GitTree.DoubleClick += new System.EventHandler(this.GitTree_DoubleClick);
            this.GitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.GitTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GitTree_MouseDown);
            // 
            // FileText
            // 
            this.FileText.AccessibleDescription = null;
            this.FileText.AccessibleName = null;
            resources.ApplyResources(this.FileText, "FileText");
            this.FileText.BackgroundImage = null;
            this.FileText.Font = null;
            this.FileText.IgnoreWhitespaceChanges = false;
            this.FileText.Name = "FileText";
            this.FileText.NumberOfVisibleLines = 3;
            this.FileText.ScrollPos = 0;
            this.FileText.ShowEntireFile = false;
            this.FileText.TreatAllFilesAsText = false;
            // 
            // Diff
            // 
            this.Diff.AccessibleDescription = null;
            this.Diff.AccessibleName = null;
            resources.ApplyResources(this.Diff, "Diff");
            this.Diff.BackgroundImage = null;
            this.Diff.Controls.Add(this.splitContainer1);
            this.Diff.Font = null;
            this.Diff.Name = "Diff";
            this.Diff.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.AccessibleDescription = null;
            this.splitContainer1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BackgroundImage = null;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Font = null;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AccessibleDescription = null;
            this.splitContainer1.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.BackgroundImage = null;
            this.splitContainer1.Panel1.Controls.Add(this.DiffFiles);
            this.splitContainer1.Panel1.Font = null;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AccessibleDescription = null;
            this.splitContainer1.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.BackgroundImage = null;
            this.splitContainer1.Panel2.Controls.Add(this.DiffText);
            this.splitContainer1.Panel2.Font = null;
            // 
            // DiffFiles
            // 
            this.DiffFiles.AccessibleDescription = null;
            this.DiffFiles.AccessibleName = null;
            resources.ApplyResources(this.DiffFiles, "DiffFiles");
            this.DiffFiles.BackgroundImage = null;
            this.DiffFiles.ContextMenuStrip = this.DiffContextMenu;
            this.DiffFiles.Font = null;
            this.DiffFiles.GitItemStatusses = null;
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.SelectedItem = null;
            this.DiffFiles.DoubleClick += new System.EventHandler(this.DiffFiles_DoubleClick);
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFiles_SelectedIndexChanged);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.AccessibleDescription = null;
            this.DiffContextMenu.AccessibleName = null;
            resources.ApplyResources(this.DiffContextMenu, "DiffContextMenu");
            this.DiffContextMenu.BackgroundImage = null;
            this.DiffContextMenu.Font = null;
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.AccessibleDescription = null;
            this.openWithDifftoolToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openWithDifftoolToolStripMenuItem, "openWithDifftoolToolStripMenuItem");
            this.openWithDifftoolToolStripMenuItem.BackgroundImage = null;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // DiffText
            // 
            this.DiffText.AccessibleDescription = null;
            this.DiffText.AccessibleName = null;
            resources.ApplyResources(this.DiffText, "DiffText");
            this.DiffText.BackgroundImage = null;
            this.DiffText.Font = null;
            this.DiffText.IgnoreWhitespaceChanges = false;
            this.DiffText.Name = "DiffText";
            this.DiffText.NumberOfVisibleLines = 3;
            this.DiffText.ScrollPos = 0;
            this.DiffText.ShowEntireFile = false;
            this.DiffText.TreatAllFilesAsText = false;
            // 
            // TreeContextMenu
            // 
            this.TreeContextMenu.AccessibleDescription = null;
            this.TreeContextMenu.AccessibleName = null;
            resources.ApplyResources(this.TreeContextMenu, "TreeContextMenu");
            this.TreeContextMenu.BackgroundImage = null;
            this.TreeContextMenu.Font = null;
            this.TreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.TreeContextMenu.Name = "TreeContextMenu";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.AccessibleDescription = null;
            this.saveToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.BackgroundImage = null;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // menuStrip1
            // 
            this.menuStrip1.AccessibleDescription = null;
            this.menuStrip1.AccessibleName = null;
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.BackgroundImage = null;
            this.menuStrip1.Font = null;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.gitToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this.remotesToolStripMenuItem,
            this.submodulesToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.AccessibleDescription = null;
            this.fileToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            this.fileToolStripMenuItem.BackgroundImage = null;
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.toolStripSeparator12,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.AccessibleDescription = null;
            this.openToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.BackgroundImage = null;
            this.openToolStripMenuItem.Image = global::GitUI.Properties.Resources._40;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.AccessibleDescription = null;
            this.closeToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.closeToolStripMenuItem, "closeToolStripMenuItem");
            this.closeToolStripMenuItem.BackgroundImage = null;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.AccessibleDescription = null;
            this.refreshToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.refreshToolStripMenuItem, "refreshToolStripMenuItem");
            this.refreshToolStripMenuItem.BackgroundImage = null;
            this.refreshToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.AccessibleDescription = null;
            this.recentToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.recentToolStripMenuItem, "recentToolStripMenuItem");
            this.recentToolStripMenuItem.BackgroundImage = null;
            this.recentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.AccessibleDescription = null;
            this.toolStripMenuItem2.AccessibleName = null;
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.BackgroundImage = null;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeyDisplayString = null;
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.AccessibleDescription = null;
            this.toolStripSeparator12.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator12, "toolStripSeparator12");
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.AccessibleDescription = null;
            this.exitToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.BackgroundImage = null;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // gitToolStripMenuItem
            // 
            this.gitToolStripMenuItem.AccessibleDescription = null;
            this.gitToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.gitToolStripMenuItem, "gitToolStripMenuItem");
            this.gitToolStripMenuItem.BackgroundImage = null;
            this.gitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitBashToolStripMenuItem,
            this.gitGUIToolStripMenuItem,
            this.kGitToolStripMenuItem});
            this.gitToolStripMenuItem.Name = "gitToolStripMenuItem";
            this.gitToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // gitBashToolStripMenuItem
            // 
            this.gitBashToolStripMenuItem.AccessibleDescription = null;
            this.gitBashToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.gitBashToolStripMenuItem, "gitBashToolStripMenuItem");
            this.gitBashToolStripMenuItem.BackgroundImage = null;
            this.gitBashToolStripMenuItem.Image = global::GitUI.Properties.Resources._26;
            this.gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            this.gitBashToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.gitBashToolStripMenuItem.Click += new System.EventHandler(this.gitBashToolStripMenuItem_Click_1);
            // 
            // gitGUIToolStripMenuItem
            // 
            this.gitGUIToolStripMenuItem.AccessibleDescription = null;
            this.gitGUIToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.gitGUIToolStripMenuItem, "gitGUIToolStripMenuItem");
            this.gitGUIToolStripMenuItem.BackgroundImage = null;
            this.gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            this.gitGUIToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.gitGUIToolStripMenuItem.Click += new System.EventHandler(this.gitGUIToolStripMenuItem_Click);
            // 
            // kGitToolStripMenuItem
            // 
            this.kGitToolStripMenuItem.AccessibleDescription = null;
            this.kGitToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.kGitToolStripMenuItem, "kGitToolStripMenuItem");
            this.kGitToolStripMenuItem.BackgroundImage = null;
            this.kGitToolStripMenuItem.Name = "kGitToolStripMenuItem";
            this.kGitToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.kGitToolStripMenuItem.Click += new System.EventHandler(this.kGitToolStripMenuItem_Click);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.AccessibleDescription = null;
            this.commandsToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.commandsToolStripMenuItem, "commandsToolStripMenuItem");
            this.commandsToolStripMenuItem.BackgroundImage = null;
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyPatchToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.checkoutToolStripMenuItem,
            this.cherryPickToolStripMenuItem,
            this.cleanupToolStripMenuItem,
            this.cloneToolStripMenuItem,
            this.commitToolStripMenuItem,
            this.branchToolStripMenuItem,
            this.tagToolStripMenuItem,
            this.deleteBranchToolStripMenuItem,
            this.deleteTagToolStripMenuItem,
            this.formatPatchToolStripMenuItem,
            this.initNewRepositoryToolStripMenuItem,
            this.mergeBranchToolStripMenuItem,
            this.pullToolStripMenuItem,
            this.pushToolStripMenuItem,
            this.rebaseToolStripMenuItem,
            this.runMergetoolToolStripMenuItem,
            this.stashToolStripMenuItem,
            this.viewDiffToolStripMenuItem,
            this.patchToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.AccessibleDescription = null;
            this.applyPatchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.applyPatchToolStripMenuItem, "applyPatchToolStripMenuItem");
            this.applyPatchToolStripMenuItem.BackgroundImage = null;
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.applyPatchToolStripMenuItem_Click);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.AccessibleDescription = null;
            this.archiveToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.archiveToolStripMenuItem, "archiveToolStripMenuItem");
            this.archiveToolStripMenuItem.BackgroundImage = null;
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.archiveToolStripMenuItem.Click += new System.EventHandler(this.archiveToolStripMenuItem_Click);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.AccessibleDescription = null;
            this.checkoutBranchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.checkoutBranchToolStripMenuItem, "checkoutBranchToolStripMenuItem");
            this.checkoutBranchToolStripMenuItem.BackgroundImage = null;
            this.checkoutBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources._33;
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.checkoutBranchToolStripMenuItem_Click);
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.AccessibleDescription = null;
            this.checkoutToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.checkoutToolStripMenuItem, "checkoutToolStripMenuItem");
            this.checkoutToolStripMenuItem.BackgroundImage = null;
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.checkoutToolStripMenuItem_Click);
            // 
            // cherryPickToolStripMenuItem
            // 
            this.cherryPickToolStripMenuItem.AccessibleDescription = null;
            this.cherryPickToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.cherryPickToolStripMenuItem, "cherryPickToolStripMenuItem");
            this.cherryPickToolStripMenuItem.BackgroundImage = null;
            this.cherryPickToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            this.cherryPickToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.cherryPickToolStripMenuItem.Click += new System.EventHandler(this.cherryPickToolStripMenuItem_Click);
            // 
            // cleanupToolStripMenuItem
            // 
            this.cleanupToolStripMenuItem.AccessibleDescription = null;
            this.cleanupToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.cleanupToolStripMenuItem, "cleanupToolStripMenuItem");
            this.cleanupToolStripMenuItem.BackgroundImage = null;
            this.cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
            this.cleanupToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.cleanupToolStripMenuItem.Click += new System.EventHandler(this.cleanupToolStripMenuItem_Click);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.AccessibleDescription = null;
            this.cloneToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.cloneToolStripMenuItem, "cloneToolStripMenuItem");
            this.cloneToolStripMenuItem.BackgroundImage = null;
            this.cloneToolStripMenuItem.Image = global::GitUI.Properties.Resources._46;
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.cloneToolStripMenuItem_Click);
            // 
            // commitToolStripMenuItem
            // 
            this.commitToolStripMenuItem.AccessibleDescription = null;
            this.commitToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.commitToolStripMenuItem, "commitToolStripMenuItem");
            this.commitToolStripMenuItem.BackgroundImage = null;
            this.commitToolStripMenuItem.Image = global::GitUI.Properties.Resources._10;
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            this.commitToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.commitToolStripMenuItem.Click += new System.EventHandler(this.commitToolStripMenuItem_Click);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.AccessibleDescription = null;
            this.branchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.branchToolStripMenuItem, "branchToolStripMenuItem");
            this.branchToolStripMenuItem.BackgroundImage = null;
            this.branchToolStripMenuItem.Image = global::GitUI.Properties.Resources._33;
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.branchToolStripMenuItem_Click);
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.AccessibleDescription = null;
            this.tagToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.tagToolStripMenuItem, "tagToolStripMenuItem");
            this.tagToolStripMenuItem.BackgroundImage = null;
            this.tagToolStripMenuItem.Image = global::GitUI.Properties.Resources._352;
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.tagToolStripMenuItem.Click += new System.EventHandler(this.tagToolStripMenuItem_Click);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.AccessibleDescription = null;
            this.deleteBranchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.deleteBranchToolStripMenuItem, "deleteBranchToolStripMenuItem");
            this.deleteBranchToolStripMenuItem.BackgroundImage = null;
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchToolStripMenuItem_Click);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.AccessibleDescription = null;
            this.deleteTagToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.deleteTagToolStripMenuItem, "deleteTagToolStripMenuItem");
            this.deleteTagToolStripMenuItem.BackgroundImage = null;
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.deleteTagToolStripMenuItem_Click);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.AccessibleDescription = null;
            this.formatPatchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.formatPatchToolStripMenuItem, "formatPatchToolStripMenuItem");
            this.formatPatchToolStripMenuItem.BackgroundImage = null;
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.formatPatchToolStripMenuItem_Click);
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.AccessibleDescription = null;
            this.initNewRepositoryToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.initNewRepositoryToolStripMenuItem, "initNewRepositoryToolStripMenuItem");
            this.initNewRepositoryToolStripMenuItem.BackgroundImage = null;
            this.initNewRepositoryToolStripMenuItem.Image = global::GitUI.Properties.Resources._14;
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.initNewRepositoryToolStripMenuItem_Click);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.AccessibleDescription = null;
            this.mergeBranchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.mergeBranchToolStripMenuItem, "mergeBranchToolStripMenuItem");
            this.mergeBranchToolStripMenuItem.BackgroundImage = null;
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.mergeBranchToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.AccessibleDescription = null;
            this.pullToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.pullToolStripMenuItem, "pullToolStripMenuItem");
            this.pullToolStripMenuItem.BackgroundImage = null;
            this.pullToolStripMenuItem.Image = global::GitUI.Properties.Resources._4;
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.pullToolStripMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.AccessibleDescription = null;
            this.pushToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.pushToolStripMenuItem, "pushToolStripMenuItem");
            this.pushToolStripMenuItem.BackgroundImage = null;
            this.pushToolStripMenuItem.Image = global::GitUI.Properties.Resources._3;
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.pushToolStripMenuItem_Click);
            // 
            // rebaseToolStripMenuItem
            // 
            this.rebaseToolStripMenuItem.AccessibleDescription = null;
            this.rebaseToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.rebaseToolStripMenuItem, "rebaseToolStripMenuItem");
            this.rebaseToolStripMenuItem.BackgroundImage = null;
            this.rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            this.rebaseToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.rebaseToolStripMenuItem.Click += new System.EventHandler(this.rebaseToolStripMenuItem_Click);
            // 
            // runMergetoolToolStripMenuItem
            // 
            this.runMergetoolToolStripMenuItem.AccessibleDescription = null;
            this.runMergetoolToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.runMergetoolToolStripMenuItem, "runMergetoolToolStripMenuItem");
            this.runMergetoolToolStripMenuItem.BackgroundImage = null;
            this.runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            this.runMergetoolToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.runMergetoolToolStripMenuItem.Click += new System.EventHandler(this.runMergetoolToolStripMenuItem_Click);
            // 
            // stashToolStripMenuItem
            // 
            this.stashToolStripMenuItem.AccessibleDescription = null;
            this.stashToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.stashToolStripMenuItem, "stashToolStripMenuItem");
            this.stashToolStripMenuItem.BackgroundImage = null;
            this.stashToolStripMenuItem.Image = global::GitUI.Properties.Resources.stash1;
            this.stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            this.stashToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.stashToolStripMenuItem.Click += new System.EventHandler(this.stashToolStripMenuItem_Click);
            // 
            // viewDiffToolStripMenuItem
            // 
            this.viewDiffToolStripMenuItem.AccessibleDescription = null;
            this.viewDiffToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.viewDiffToolStripMenuItem, "viewDiffToolStripMenuItem");
            this.viewDiffToolStripMenuItem.BackgroundImage = null;
            this.viewDiffToolStripMenuItem.Name = "viewDiffToolStripMenuItem";
            this.viewDiffToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.viewDiffToolStripMenuItem.Click += new System.EventHandler(this.viewDiffToolStripMenuItem_Click);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.AccessibleDescription = null;
            this.patchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.patchToolStripMenuItem, "patchToolStripMenuItem");
            this.patchToolStripMenuItem.BackgroundImage = null;
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.patchToolStripMenuItem_Click);
            // 
            // remotesToolStripMenuItem
            // 
            this.remotesToolStripMenuItem.AccessibleDescription = null;
            this.remotesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.remotesToolStripMenuItem, "remotesToolStripMenuItem");
            this.remotesToolStripMenuItem.BackgroundImage = null;
            this.remotesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageRemoteRepositoriesToolStripMenuItem1,
            this.toolStripSeparator6,
            this.PuTTYToolStripMenuItem});
            this.remotesToolStripMenuItem.Name = "remotesToolStripMenuItem";
            this.remotesToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // manageRemoteRepositoriesToolStripMenuItem1
            // 
            this.manageRemoteRepositoriesToolStripMenuItem1.AccessibleDescription = null;
            this.manageRemoteRepositoriesToolStripMenuItem1.AccessibleName = null;
            resources.ApplyResources(this.manageRemoteRepositoriesToolStripMenuItem1, "manageRemoteRepositoriesToolStripMenuItem1");
            this.manageRemoteRepositoriesToolStripMenuItem1.BackgroundImage = null;
            this.manageRemoteRepositoriesToolStripMenuItem1.Name = "manageRemoteRepositoriesToolStripMenuItem1";
            this.manageRemoteRepositoriesToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.manageRemoteRepositoriesToolStripMenuItem1.Click += new System.EventHandler(this.manageRemoteRepositoriesToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.AccessibleDescription = null;
            this.toolStripSeparator6.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            // 
            // PuTTYToolStripMenuItem
            // 
            this.PuTTYToolStripMenuItem.AccessibleDescription = null;
            this.PuTTYToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.PuTTYToolStripMenuItem, "PuTTYToolStripMenuItem");
            this.PuTTYToolStripMenuItem.BackgroundImage = null;
            this.PuTTYToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAuthenticationAgentToolStripMenuItem,
            this.generateOrImportKeyToolStripMenuItem});
            this.PuTTYToolStripMenuItem.Image = global::GitUI.Properties.Resources.putty;
            this.PuTTYToolStripMenuItem.Name = "PuTTYToolStripMenuItem";
            this.PuTTYToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // startAuthenticationAgentToolStripMenuItem
            // 
            this.startAuthenticationAgentToolStripMenuItem.AccessibleDescription = null;
            this.startAuthenticationAgentToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.startAuthenticationAgentToolStripMenuItem, "startAuthenticationAgentToolStripMenuItem");
            this.startAuthenticationAgentToolStripMenuItem.BackgroundImage = null;
            this.startAuthenticationAgentToolStripMenuItem.Image = global::GitUI.Properties.Resources.pageant;
            this.startAuthenticationAgentToolStripMenuItem.Name = "startAuthenticationAgentToolStripMenuItem";
            this.startAuthenticationAgentToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.startAuthenticationAgentToolStripMenuItem.Click += new System.EventHandler(this.startAuthenticationAgentToolStripMenuItem_Click);
            // 
            // generateOrImportKeyToolStripMenuItem
            // 
            this.generateOrImportKeyToolStripMenuItem.AccessibleDescription = null;
            this.generateOrImportKeyToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.generateOrImportKeyToolStripMenuItem, "generateOrImportKeyToolStripMenuItem");
            this.generateOrImportKeyToolStripMenuItem.BackgroundImage = null;
            this.generateOrImportKeyToolStripMenuItem.Image = global::GitUI.Properties.Resources.puttygen;
            this.generateOrImportKeyToolStripMenuItem.Name = "generateOrImportKeyToolStripMenuItem";
            this.generateOrImportKeyToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.generateOrImportKeyToolStripMenuItem.Click += new System.EventHandler(this.generateOrImportKeyToolStripMenuItem_Click);
            // 
            // submodulesToolStripMenuItem
            // 
            this.submodulesToolStripMenuItem.AccessibleDescription = null;
            this.submodulesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.submodulesToolStripMenuItem, "submodulesToolStripMenuItem");
            this.submodulesToolStripMenuItem.BackgroundImage = null;
            this.submodulesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageSubmodulesToolStripMenuItem,
            this.toolStripSeparator8,
            this.updateAllSubmodulesRecursiveToolStripMenuItem,
            this.initializeAllSubmodulesRecursiveToolStripMenuItem,
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem,
            this.toolStripSeparator14,
            this.updateAllSubmodulesToolStripMenuItem,
            this.initializeAllSubmodulesToolStripMenuItem,
            this.syncronizeAllSubmodulesToolStripMenuItem,
            this.toolStripSeparator10,
            this.openSubmoduleToolStripMenuItem});
            this.submodulesToolStripMenuItem.Name = "submodulesToolStripMenuItem";
            this.submodulesToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // manageSubmodulesToolStripMenuItem
            // 
            this.manageSubmodulesToolStripMenuItem.AccessibleDescription = null;
            this.manageSubmodulesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.manageSubmodulesToolStripMenuItem, "manageSubmodulesToolStripMenuItem");
            this.manageSubmodulesToolStripMenuItem.BackgroundImage = null;
            this.manageSubmodulesToolStripMenuItem.Name = "manageSubmodulesToolStripMenuItem";
            this.manageSubmodulesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.manageSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.manageSubmodulesToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.AccessibleDescription = null;
            this.toolStripSeparator8.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            // 
            // updateAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.updateAllSubmodulesRecursiveToolStripMenuItem.AccessibleDescription = null;
            this.updateAllSubmodulesRecursiveToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.updateAllSubmodulesRecursiveToolStripMenuItem, "updateAllSubmodulesRecursiveToolStripMenuItem");
            this.updateAllSubmodulesRecursiveToolStripMenuItem.BackgroundImage = null;
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Name = "updateAllSubmodulesRecursiveToolStripMenuItem";
            this.updateAllSubmodulesRecursiveToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.updateAllSubmodulesRecursiveToolStripMenuItem_Click);
            // 
            // initializeAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.AccessibleDescription = null;
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.initializeAllSubmodulesRecursiveToolStripMenuItem, "initializeAllSubmodulesRecursiveToolStripMenuItem");
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.BackgroundImage = null;
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Name = "initializeAllSubmodulesRecursiveToolStripMenuItem";
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.initializeAllSubmodulesRecursiveToolStripMenuItem_Click);
            // 
            // synchronizeAlSubmodulesRecursiveToolStripMenuItem
            // 
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.AccessibleDescription = null;
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.synchronizeAlSubmodulesRecursiveToolStripMenuItem, "synchronizeAlSubmodulesRecursiveToolStripMenuItem");
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.BackgroundImage = null;
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.Name = "synchronizeAlSubmodulesRecursiveToolStripMenuItem";
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.synchronizeAlSubmodulesRecursiveToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.AccessibleDescription = null;
            this.toolStripSeparator14.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator14, "toolStripSeparator14");
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            // 
            // updateAllSubmodulesToolStripMenuItem
            // 
            this.updateAllSubmodulesToolStripMenuItem.AccessibleDescription = null;
            this.updateAllSubmodulesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.updateAllSubmodulesToolStripMenuItem, "updateAllSubmodulesToolStripMenuItem");
            this.updateAllSubmodulesToolStripMenuItem.BackgroundImage = null;
            this.updateAllSubmodulesToolStripMenuItem.Name = "updateAllSubmodulesToolStripMenuItem";
            this.updateAllSubmodulesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.updateAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.updateAllSubmodulesToolStripMenuItem_Click);
            // 
            // initializeAllSubmodulesToolStripMenuItem
            // 
            this.initializeAllSubmodulesToolStripMenuItem.AccessibleDescription = null;
            this.initializeAllSubmodulesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.initializeAllSubmodulesToolStripMenuItem, "initializeAllSubmodulesToolStripMenuItem");
            this.initializeAllSubmodulesToolStripMenuItem.BackgroundImage = null;
            this.initializeAllSubmodulesToolStripMenuItem.Name = "initializeAllSubmodulesToolStripMenuItem";
            this.initializeAllSubmodulesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.initializeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.initializeAllSubmodulesToolStripMenuItem_Click);
            // 
            // syncronizeAllSubmodulesToolStripMenuItem
            // 
            this.syncronizeAllSubmodulesToolStripMenuItem.AccessibleDescription = null;
            this.syncronizeAllSubmodulesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.syncronizeAllSubmodulesToolStripMenuItem, "syncronizeAllSubmodulesToolStripMenuItem");
            this.syncronizeAllSubmodulesToolStripMenuItem.BackgroundImage = null;
            this.syncronizeAllSubmodulesToolStripMenuItem.Name = "syncronizeAllSubmodulesToolStripMenuItem";
            this.syncronizeAllSubmodulesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.syncronizeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.syncronizeAllSubmodulesToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.AccessibleDescription = null;
            this.toolStripSeparator10.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            // 
            // openSubmoduleToolStripMenuItem
            // 
            this.openSubmoduleToolStripMenuItem.AccessibleDescription = null;
            this.openSubmoduleToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openSubmoduleToolStripMenuItem, "openSubmoduleToolStripMenuItem");
            this.openSubmoduleToolStripMenuItem.BackgroundImage = null;
            this.openSubmoduleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator11});
            this.openSubmoduleToolStripMenuItem.Name = "openSubmoduleToolStripMenuItem";
            this.openSubmoduleToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openSubmoduleToolStripMenuItem.DropDownOpening += new System.EventHandler(this.openSubmoduleToolStripMenuItem_DropDownOpening);
            this.openSubmoduleToolStripMenuItem.Click += new System.EventHandler(this.openSubmoduleToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.AccessibleDescription = null;
            this.toolStripSeparator11.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator11, "toolStripSeparator11");
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.AccessibleDescription = null;
            this.pluginsToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.pluginsToolStripMenuItem, "pluginsToolStripMenuItem");
            this.pluginsToolStripMenuItem.BackgroundImage = null;
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.toolStripSeparator15});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.AccessibleDescription = null;
            this.settingsToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.BackgroundImage = null;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.AccessibleDescription = null;
            this.toolStripSeparator15.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator15, "toolStripSeparator15");
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.AccessibleDescription = null;
            this.settingsToolStripMenuItem1.AccessibleName = null;
            resources.ApplyResources(this.settingsToolStripMenuItem1, "settingsToolStripMenuItem1");
            this.settingsToolStripMenuItem1.BackgroundImage = null;
            this.settingsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitMaintenanceToolStripMenuItem,
            this.toolStripSeparator4,
            this.editgitignoreToolStripMenuItem1,
            this.editmailmapToolStripMenuItem,
            this.toolStripSeparator13,
            this.settingsToolStripMenuItem2});
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.ShortcutKeyDisplayString = null;
            // 
            // gitMaintenanceToolStripMenuItem
            // 
            this.gitMaintenanceToolStripMenuItem.AccessibleDescription = null;
            this.gitMaintenanceToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.gitMaintenanceToolStripMenuItem, "gitMaintenanceToolStripMenuItem");
            this.gitMaintenanceToolStripMenuItem.BackgroundImage = null;
            this.gitMaintenanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressGitDatabaseToolStripMenuItem,
            this.verifyGitDatabaseToolStripMenuItem});
            this.gitMaintenanceToolStripMenuItem.Image = global::GitUI.Properties.Resources._82;
            this.gitMaintenanceToolStripMenuItem.Name = "gitMaintenanceToolStripMenuItem";
            this.gitMaintenanceToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // compressGitDatabaseToolStripMenuItem
            // 
            this.compressGitDatabaseToolStripMenuItem.AccessibleDescription = null;
            this.compressGitDatabaseToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.compressGitDatabaseToolStripMenuItem, "compressGitDatabaseToolStripMenuItem");
            this.compressGitDatabaseToolStripMenuItem.BackgroundImage = null;
            this.compressGitDatabaseToolStripMenuItem.Name = "compressGitDatabaseToolStripMenuItem";
            this.compressGitDatabaseToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.compressGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.compressGitDatabaseToolStripMenuItem_Click);
            // 
            // verifyGitDatabaseToolStripMenuItem
            // 
            this.verifyGitDatabaseToolStripMenuItem.AccessibleDescription = null;
            this.verifyGitDatabaseToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.verifyGitDatabaseToolStripMenuItem, "verifyGitDatabaseToolStripMenuItem");
            this.verifyGitDatabaseToolStripMenuItem.BackgroundImage = null;
            this.verifyGitDatabaseToolStripMenuItem.Name = "verifyGitDatabaseToolStripMenuItem";
            this.verifyGitDatabaseToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.verifyGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.verifyGitDatabaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.AccessibleDescription = null;
            this.toolStripSeparator4.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // editgitignoreToolStripMenuItem1
            // 
            this.editgitignoreToolStripMenuItem1.AccessibleDescription = null;
            this.editgitignoreToolStripMenuItem1.AccessibleName = null;
            resources.ApplyResources(this.editgitignoreToolStripMenuItem1, "editgitignoreToolStripMenuItem1");
            this.editgitignoreToolStripMenuItem1.BackgroundImage = null;
            this.editgitignoreToolStripMenuItem1.Name = "editgitignoreToolStripMenuItem1";
            this.editgitignoreToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.editgitignoreToolStripMenuItem1.Click += new System.EventHandler(this.editgitignoreToolStripMenuItem1_Click);
            // 
            // editmailmapToolStripMenuItem
            // 
            this.editmailmapToolStripMenuItem.AccessibleDescription = null;
            this.editmailmapToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.editmailmapToolStripMenuItem, "editmailmapToolStripMenuItem");
            this.editmailmapToolStripMenuItem.BackgroundImage = null;
            this.editmailmapToolStripMenuItem.Name = "editmailmapToolStripMenuItem";
            this.editmailmapToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.editmailmapToolStripMenuItem.Click += new System.EventHandler(this.editmailmapToolStripMenuItem_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.AccessibleDescription = null;
            this.toolStripSeparator13.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator13, "toolStripSeparator13");
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            // 
            // settingsToolStripMenuItem2
            // 
            this.settingsToolStripMenuItem2.AccessibleDescription = null;
            this.settingsToolStripMenuItem2.AccessibleName = null;
            resources.ApplyResources(this.settingsToolStripMenuItem2, "settingsToolStripMenuItem2");
            this.settingsToolStripMenuItem2.BackgroundImage = null;
            this.settingsToolStripMenuItem2.Image = global::GitUI.Properties.Resources._71;
            this.settingsToolStripMenuItem2.Name = "settingsToolStripMenuItem2";
            this.settingsToolStripMenuItem2.ShortcutKeyDisplayString = null;
            this.settingsToolStripMenuItem2.Click += new System.EventHandler(this.settingsToolStripMenuItem2_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.AccessibleDescription = null;
            this.helpToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            this.helpToolStripMenuItem.BackgroundImage = null;
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitcountPerUserToolStripMenuItem,
            this.gitcommandLogToolStripMenuItem,
            this.toolStripSeparator7,
            this.userManualToolStripMenuItem,
            this.changelogToolStripMenuItem,
            this.toolStripSeparator3,
            this.donateToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // commitcountPerUserToolStripMenuItem
            // 
            this.commitcountPerUserToolStripMenuItem.AccessibleDescription = null;
            this.commitcountPerUserToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.commitcountPerUserToolStripMenuItem, "commitcountPerUserToolStripMenuItem");
            this.commitcountPerUserToolStripMenuItem.BackgroundImage = null;
            this.commitcountPerUserToolStripMenuItem.Image = global::GitUI.Properties.Resources._53;
            this.commitcountPerUserToolStripMenuItem.Name = "commitcountPerUserToolStripMenuItem";
            this.commitcountPerUserToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.commitcountPerUserToolStripMenuItem.Click += new System.EventHandler(this.commitcountPerUserToolStripMenuItem_Click);
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.AccessibleDescription = null;
            this.gitcommandLogToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.gitcommandLogToolStripMenuItem, "gitcommandLogToolStripMenuItem");
            this.gitcommandLogToolStripMenuItem.BackgroundImage = null;
            this.gitcommandLogToolStripMenuItem.Image = global::GitUI.Properties.Resources._21;
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.gitcommandLogToolStripMenuItem.Click += new System.EventHandler(this.gitcommandLogToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.AccessibleDescription = null;
            this.toolStripSeparator7.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            // 
            // userManualToolStripMenuItem
            // 
            this.userManualToolStripMenuItem.AccessibleDescription = null;
            this.userManualToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.userManualToolStripMenuItem, "userManualToolStripMenuItem");
            this.userManualToolStripMenuItem.BackgroundImage = null;
            this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            this.userManualToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.userManualToolStripMenuItem.Click += new System.EventHandler(this.userManualToolStripMenuItem_Click);
            // 
            // changelogToolStripMenuItem
            // 
            this.changelogToolStripMenuItem.AccessibleDescription = null;
            this.changelogToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.changelogToolStripMenuItem, "changelogToolStripMenuItem");
            this.changelogToolStripMenuItem.BackgroundImage = null;
            this.changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            this.changelogToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.changelogToolStripMenuItem.Click += new System.EventHandler(this.changelogToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.AccessibleDescription = null;
            this.toolStripSeparator3.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // donateToolStripMenuItem
            // 
            this.donateToolStripMenuItem.AccessibleDescription = null;
            this.donateToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.donateToolStripMenuItem, "donateToolStripMenuItem");
            this.donateToolStripMenuItem.BackgroundImage = null;
            this.donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            this.donateToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.donateToolStripMenuItem.Click += new System.EventHandler(this.donateToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.AccessibleDescription = null;
            this.aboutToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.BackgroundImage = null;
            this.aboutToolStripMenuItem.Image = global::GitUI.Properties.Resources._49;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // FormBrowse
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Font = null;
            this.Name = "FormBrowse";
            this.Load += new System.EventHandler(this.Browse_Load);
            this.Shown += new System.EventHandler(this.FormBrowse_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBrowse_FormClosing);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.CommitInfo.ResumeLayout(false);
            this.Tree.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.Diff.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.TreeContextMenu.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView GitTree;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Tree;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutToolStripMenuItem;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.ToolStripMenuItem viewDiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem branchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem initNewRepositoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pushToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pullToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem patchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyPatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitBashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitGUIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem formatPatchToolStripMenuItem;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.ToolStripMenuItem gitcommandLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runMergetoolToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.ToolStripMenuItem deleteBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSplitButton Workingdir;
        private System.Windows.Forms.ToolStripButton CurrentBranch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton GitBash;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton EditSettings;
        private System.Windows.Forms.ToolStripMenuItem tagToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton RefreshButton;
        private System.Windows.Forms.ToolStripMenuItem commitcountPerUserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kGitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem donateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editgitignoreToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem archiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editmailmapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem gitMaintenanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compressGitDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verifyGitDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageRemoteRepositoriesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem PuTTYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startAuthenticationAgentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateOrImportKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
        private System.Windows.Forms.TabPage Diff;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FileStatusList DiffFiles;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem changelogToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonPull;
        private System.Windows.Forms.ToolStripButton toolStripButtonPush;
        private FileViewer FileText;
        private FileViewer DiffText;
        private System.Windows.Forms.TabPage CommitInfo;
        private CommitInfo RevisionInfo;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripMenuItem submodulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageSubmodulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem updateAllSubmodulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem initializeAllSubmodulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem syncronizeAllSubmodulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitStash;
        private System.Windows.Forms.ToolStripMenuItem stashChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stashPopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem viewStashToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem openSubmoduleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem updateAllSubmodulesRecursiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem initializeAllSubmodulesRecursiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem synchronizeAlSubmodulesRecursiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip TreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cleanupToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
    }
}