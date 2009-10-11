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
            this.Workingdir = new System.Windows.Forms.ToolStripButton();
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
            this.NoGit = new System.Windows.Forms.Panel();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Open = new System.Windows.Forms.Button();
            this.Init = new System.Windows.Forms.Button();
            this.Clone = new System.Windows.Forms.Button();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.RecentRepositoriesGroupBox = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Donate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.CommitInfo = new System.Windows.Forms.TabPage();
            this.RevisionInfo = new System.Windows.Forms.RichTextBox();
            this.Tree = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.GitTree = new System.Windows.Forms.TreeView();
            this.FileText = new GitUI.FileViewer();
            this.Commit = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new System.Windows.Forms.ListBox();
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
            this.updateAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncronizeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.updateAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.changelogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            this.NoGit.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.CommitInfo.SuspendLayout();
            this.Tree.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.Commit.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.TreeContextMenu.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 24);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.ToolStrip);
            this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.NoGit);
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(796, 549);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 2;
            this.splitContainer2.TabStop = false;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // ToolStrip
            // 
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
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Size = new System.Drawing.Size(796, 25);
            this.ToolStrip.TabIndex = 4;
            this.ToolStrip.Text = "toolStrip1";
            this.ToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolStrip_ItemClicked);
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = ((System.Drawing.Image)(resources.GetObject("RefreshButton.Image")));
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.White;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.ToolTipText = "Refresh";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(0, 22);
            // 
            // Workingdir
            // 
            this.Workingdir.Image = ((System.Drawing.Image)(resources.GetObject("Workingdir.Image")));
            this.Workingdir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Workingdir.Name = "Workingdir";
            this.Workingdir.Size = new System.Drawing.Size(87, 22);
            this.Workingdir.Text = "WorkingDir";
            this.Workingdir.ToolTipText = "Change working directory";
            this.Workingdir.Click += new System.EventHandler(this.Workingdir_Click_1);
            // 
            // CurrentBranch
            // 
            this.CurrentBranch.Image = ((System.Drawing.Image)(resources.GetObject("CurrentBranch.Image")));
            this.CurrentBranch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CurrentBranch.Name = "CurrentBranch";
            this.CurrentBranch.Size = new System.Drawing.Size(64, 22);
            this.CurrentBranch.Text = "Branch";
            this.CurrentBranch.ToolTipText = "Switch branch";
            this.CurrentBranch.Click += new System.EventHandler(this.CurrentBranch_Click_1);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSplitStash
            // 
            this.toolStripSplitStash.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitStash.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stashChangesToolStripMenuItem,
            this.stashPopToolStripMenuItem,
            this.toolStripSeparator9,
            this.viewStashToolStripMenuItem});
            this.toolStripSplitStash.Image = global::GitUI.Properties.Resources.stash1;
            this.toolStripSplitStash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitStash.Name = "toolStripSplitStash";
            this.toolStripSplitStash.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitStash.Text = "toolStripSplitButton1";
            this.toolStripSplitStash.ToolTipText = "Stash changes";
            this.toolStripSplitStash.ButtonClick += new System.EventHandler(this.toolStripSplitStash_ButtonClick);
            // 
            // stashChangesToolStripMenuItem
            // 
            this.stashChangesToolStripMenuItem.Name = "stashChangesToolStripMenuItem";
            this.stashChangesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.stashChangesToolStripMenuItem.Text = "Stash";
            this.stashChangesToolStripMenuItem.ToolTipText = "Stash changes";
            this.stashChangesToolStripMenuItem.Click += new System.EventHandler(this.stashChangesToolStripMenuItem_Click);
            // 
            // stashPopToolStripMenuItem
            // 
            this.stashPopToolStripMenuItem.Name = "stashPopToolStripMenuItem";
            this.stashPopToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.stashPopToolStripMenuItem.Text = "Stash pop";
            this.stashPopToolStripMenuItem.ToolTipText = "Apply and drop single stash";
            this.stashPopToolStripMenuItem.Click += new System.EventHandler(this.stashPopToolStripMenuItem_Click);
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
            this.viewStashToolStripMenuItem.Click += new System.EventHandler(this.viewStashToolStripMenuItem_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(71, 22);
            this.toolStripButton1.Text = "Commit";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButtonPull
            // 
            this.toolStripButtonPull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPull.Image = global::GitUI.Properties.Resources._4;
            this.toolStripButtonPull.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPull.Name = "toolStripButtonPull";
            this.toolStripButtonPull.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPull.Text = "Pull";
            this.toolStripButtonPull.Click += new System.EventHandler(this.toolStripButtonPull_Click);
            // 
            // toolStripButtonPush
            // 
            this.toolStripButtonPush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPush.Image = global::GitUI.Properties.Resources._31;
            this.toolStripButtonPush.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPush.Name = "toolStripButtonPush";
            this.toolStripButtonPush.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPush.Text = "Push";
            this.toolStripButtonPush.Click += new System.EventHandler(this.toolStripButtonPush_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // GitBash
            // 
            this.GitBash.Image = ((System.Drawing.Image)(resources.GetObject("GitBash.Image")));
            this.GitBash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GitBash.Name = "GitBash";
            this.GitBash.Size = new System.Drawing.Size(23, 22);
            this.GitBash.ToolTipText = "Git bash";
            this.GitBash.Click += new System.EventHandler(this.GitBash_Click);
            // 
            // EditSettings
            // 
            this.EditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.EditSettings.Image = ((System.Drawing.Image)(resources.GetObject("EditSettings.Image")));
            this.EditSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.EditSettings.Name = "EditSettings";
            this.EditSettings.Size = new System.Drawing.Size(23, 22);
            this.EditSettings.ToolTipText = "Settings";
            this.EditSettings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel2.Text = "Filter:";
            this.toolStripLabel2.Click += new System.EventHandler(this.toolStripLabel2_Click);
            // 
            // toolStripTextBoxFilter
            // 
            this.toolStripTextBoxFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            this.toolStripTextBoxFilter.Size = new System.Drawing.Size(120, 25);
            this.toolStripTextBoxFilter.Leave += new System.EventHandler(this.toolStripTextBoxFilter_Leave);
            this.toolStripTextBoxFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBoxFilter_KeyPress);
            this.toolStripTextBoxFilter.Click += new System.EventHandler(this.toolStripTextBox1_Click);
            // 
            // NoGit
            // 
            this.NoGit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.NoGit.Controls.Add(this.splitContainer5);
            this.NoGit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoGit.Location = new System.Drawing.Point(0, 0);
            this.NoGit.Name = "NoGit";
            this.NoGit.Size = new System.Drawing.Size(796, 520);
            this.NoGit.TabIndex = 1;
            this.NoGit.Paint += new System.Windows.Forms.PaintEventHandler(this.NoGit_Paint);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.label2);
            this.splitContainer5.Size = new System.Drawing.Size(796, 520);
            this.splitContainer5.SplitterDistance = 254;
            this.splitContainer5.TabIndex = 8;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.splitContainer7);
            this.splitContainer6.Size = new System.Drawing.Size(254, 520);
            this.splitContainer6.SplitterDistance = 126;
            this.splitContainer6.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Open);
            this.groupBox1.Controls.Add(this.Init);
            this.groupBox1.Controls.Add(this.Clone);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(254, 126);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Getting started";
            // 
            // Open
            // 
            this.Open.Location = new System.Drawing.Point(48, 19);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(143, 28);
            this.Open.TabIndex = 7;
            this.Open.Text = "Open repository";
            this.Open.UseVisualStyleBackColor = true;
            this.Open.Click += new System.EventHandler(this.Open_Click);
            // 
            // Init
            // 
            this.Init.Location = new System.Drawing.Point(48, 87);
            this.Init.Name = "Init";
            this.Init.Size = new System.Drawing.Size(143, 28);
            this.Init.TabIndex = 4;
            this.Init.Text = "Create new repository";
            this.Init.UseVisualStyleBackColor = true;
            this.Init.Click += new System.EventHandler(this.Init_Click);
            // 
            // Clone
            // 
            this.Clone.Location = new System.Drawing.Point(48, 53);
            this.Clone.Name = "Clone";
            this.Clone.Size = new System.Drawing.Size(143, 28);
            this.Clone.TabIndex = 6;
            this.Clone.Text = "Clone repository";
            this.Clone.UseVisualStyleBackColor = true;
            this.Clone.Click += new System.EventHandler(this.Clone_Click);
            // 
            // splitContainer7
            // 
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.Controls.Add(this.RecentRepositoriesGroupBox);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer7.Size = new System.Drawing.Size(254, 390);
            this.splitContainer7.SplitterDistance = 324;
            this.splitContainer7.TabIndex = 0;
            // 
            // RecentRepositoriesGroupBox
            // 
            this.RecentRepositoriesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecentRepositoriesGroupBox.Location = new System.Drawing.Point(0, 0);
            this.RecentRepositoriesGroupBox.Name = "RecentRepositoriesGroupBox";
            this.RecentRepositoriesGroupBox.Size = new System.Drawing.Size(254, 324);
            this.RecentRepositoriesGroupBox.TabIndex = 0;
            this.RecentRepositoriesGroupBox.TabStop = false;
            this.RecentRepositoriesGroupBox.Text = "Recent repositories";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Donate);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(254, 62);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Donate";
            // 
            // Donate
            // 
            this.Donate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Donate.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Donate.Location = new System.Drawing.Point(48, 20);
            this.Donate.Name = "Donate";
            this.Donate.Size = new System.Drawing.Size(143, 30);
            this.Donate.TabIndex = 0;
            this.Donate.Text = "Donate";
            this.Donate.UseVisualStyleBackColor = true;
            this.Donate.Click += new System.EventHandler(this.Donate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "The current working dir is not a git repository.";
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.RevisionGrid);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer3.Size = new System.Drawing.Size(796, 520);
            this.splitContainer3.SplitterDistance = 230;
            this.splitContainer3.TabIndex = 1;
            this.splitContainer3.TabStop = false;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.currentCheckout = "\nfatal: Not a git repository\n";
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RevisionGrid.Size = new System.Drawing.Size(796, 230);
            this.RevisionGrid.TabIndex = 0;
            this.RevisionGrid.DoubleClick += new System.EventHandler(this.RevisionGrid_DoubleClick);
            this.RevisionGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RevisionGrid_MouseDoubleClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.CommitInfo);
            this.tabControl1.Controls.Add(this.Tree);
            this.tabControl1.Controls.Add(this.Commit);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(796, 286);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // CommitInfo
            // 
            this.CommitInfo.Controls.Add(this.RevisionInfo);
            this.CommitInfo.Location = new System.Drawing.Point(4, 22);
            this.CommitInfo.Margin = new System.Windows.Forms.Padding(15);
            this.CommitInfo.Name = "CommitInfo";
            this.CommitInfo.Size = new System.Drawing.Size(788, 260);
            this.CommitInfo.TabIndex = 2;
            this.CommitInfo.Text = "Commit";
            this.CommitInfo.UseVisualStyleBackColor = true;
            // 
            // RevisionInfo
            // 
            this.RevisionInfo.BackColor = System.Drawing.SystemColors.Window;
            this.RevisionInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RevisionInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RevisionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionInfo.Location = new System.Drawing.Point(0, 0);
            this.RevisionInfo.Margin = new System.Windows.Forms.Padding(10);
            this.RevisionInfo.Name = "RevisionInfo";
            this.RevisionInfo.ReadOnly = true;
            this.RevisionInfo.Size = new System.Drawing.Size(788, 260);
            this.RevisionInfo.TabIndex = 1;
            this.RevisionInfo.Text = "";
            this.RevisionInfo.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RevisionInfo_LinkClicked);
            // 
            // Tree
            // 
            this.Tree.Controls.Add(this.splitContainer4);
            this.Tree.Location = new System.Drawing.Point(4, 22);
            this.Tree.Name = "Tree";
            this.Tree.Padding = new System.Windows.Forms.Padding(3);
            this.Tree.Size = new System.Drawing.Size(788, 260);
            this.Tree.TabIndex = 0;
            this.Tree.Text = "File tree";
            this.Tree.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Location = new System.Drawing.Point(3, 3);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.GitTree);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.FileText);
            this.splitContainer4.Size = new System.Drawing.Size(782, 254);
            this.splitContainer4.SplitterDistance = 213;
            this.splitContainer4.TabIndex = 1;
            // 
            // GitTree
            // 
            this.GitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitTree.HideSelection = false;
            this.GitTree.Location = new System.Drawing.Point(0, 0);
            this.GitTree.Name = "GitTree";
            this.GitTree.Size = new System.Drawing.Size(213, 254);
            this.GitTree.TabIndex = 0;
            this.GitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTree_BeforeExpand);
            this.GitTree.DoubleClick += new System.EventHandler(this.GitTree_DoubleClick);
            this.GitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.GitTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GitTree_MouseDown);
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.Location = new System.Drawing.Point(0, 0);
            this.FileText.Name = "FileText";
            this.FileText.ScrollPos = 0;
            this.FileText.Size = new System.Drawing.Size(565, 254);
            this.FileText.TabIndex = 0;
            // 
            // Commit
            // 
            this.Commit.Controls.Add(this.splitContainer1);
            this.Commit.Location = new System.Drawing.Point(4, 22);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(788, 260);
            this.Commit.TabIndex = 1;
            this.Commit.Text = "Diff";
            this.Commit.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.DiffFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.DiffText);
            this.splitContainer1.Size = new System.Drawing.Size(788, 260);
            this.splitContainer1.SplitterDistance = 217;
            this.splitContainer1.TabIndex = 0;
            // 
            // DiffFiles
            // 
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.FormattingEnabled = true;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(217, 251);
            this.DiffFiles.Sorted = true;
            this.DiffFiles.TabIndex = 1;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFiles_SelectedIndexChanged);
            this.DiffFiles.DoubleClick += new System.EventHandler(this.DiffFiles_DoubleClick);
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Name = "DiffText";
            this.DiffText.ScrollPos = 0;
            this.DiffText.Size = new System.Drawing.Size(567, 260);
            this.DiffText.TabIndex = 0;
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
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.gitToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this.remotesToolStripMenuItem,
            this.submodulesToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(796, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.toolStripSeparator12,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::GitUI.Properties.Resources._40;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
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
            this.toolStripSeparator12.Size = new System.Drawing.Size(174, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // gitToolStripMenuItem
            // 
            this.gitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitBashToolStripMenuItem,
            this.gitGUIToolStripMenuItem,
            this.kGitToolStripMenuItem});
            this.gitToolStripMenuItem.Name = "gitToolStripMenuItem";
            this.gitToolStripMenuItem.Size = new System.Drawing.Size(34, 20);
            this.gitToolStripMenuItem.Text = "Git";
            // 
            // gitBashToolStripMenuItem
            // 
            this.gitBashToolStripMenuItem.Image = global::GitUI.Properties.Resources._26;
            this.gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            this.gitBashToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.gitBashToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.gitBashToolStripMenuItem.Text = "Git bash";
            this.gitBashToolStripMenuItem.Click += new System.EventHandler(this.gitBashToolStripMenuItem_Click_1);
            // 
            // gitGUIToolStripMenuItem
            // 
            this.gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            this.gitGUIToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.gitGUIToolStripMenuItem.Text = "Git GUI";
            this.gitGUIToolStripMenuItem.Click += new System.EventHandler(this.gitGUIToolStripMenuItem_Click);
            // 
            // kGitToolStripMenuItem
            // 
            this.kGitToolStripMenuItem.Name = "kGitToolStripMenuItem";
            this.kGitToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.kGitToolStripMenuItem.Text = "GitK";
            this.kGitToolStripMenuItem.Click += new System.EventHandler(this.kGitToolStripMenuItem_Click);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyPatchToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.checkoutToolStripMenuItem,
            this.cherryPickToolStripMenuItem,
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
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.applyPatchToolStripMenuItem.Text = "Apply patch";
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.applyPatchToolStripMenuItem_Click);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.archiveToolStripMenuItem.Text = "Archive";
            this.archiveToolStripMenuItem.Click += new System.EventHandler(this.archiveToolStripMenuItem_Click);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources._33;
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.checkoutBranchToolStripMenuItem_Click);
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.checkoutToolStripMenuItem.Text = "Checkout revision";
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.checkoutToolStripMenuItem_Click);
            // 
            // cherryPickToolStripMenuItem
            // 
            this.cherryPickToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            this.cherryPickToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.cherryPickToolStripMenuItem.Text = "Cherry pick";
            this.cherryPickToolStripMenuItem.Click += new System.EventHandler(this.cherryPickToolStripMenuItem_Click);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Image = global::GitUI.Properties.Resources._46;
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.cloneToolStripMenuItem.Text = "Clone repository";
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.cloneToolStripMenuItem_Click);
            // 
            // commitToolStripMenuItem
            // 
            this.commitToolStripMenuItem.Image = global::GitUI.Properties.Resources._10;
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            this.commitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
            this.commitToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.commitToolStripMenuItem.Text = "Commit";
            this.commitToolStripMenuItem.Click += new System.EventHandler(this.commitToolStripMenuItem_Click);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.Image = global::GitUI.Properties.Resources._33;
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.branchToolStripMenuItem.Text = "Create branch";
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.branchToolStripMenuItem_Click);
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.Image = global::GitUI.Properties.Resources._352;
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.tagToolStripMenuItem.Text = "Create tag";
            this.tagToolStripMenuItem.Click += new System.EventHandler(this.tagToolStripMenuItem_Click);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchToolStripMenuItem_Click);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.deleteTagToolStripMenuItem.Text = "Delete tag";
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.deleteTagToolStripMenuItem_Click);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.formatPatchToolStripMenuItem.Text = "Format patch";
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.formatPatchToolStripMenuItem_Click);
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Image = global::GitUI.Properties.Resources._14;
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "Init new repository";
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.initNewRepositoryToolStripMenuItem_Click);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.mergeBranchToolStripMenuItem.Text = "Merge branches";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.mergeBranchToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.Image = global::GitUI.Properties.Resources._4;
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.pullToolStripMenuItem.Text = "Pull";
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.pullToolStripMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Image = global::GitUI.Properties.Resources._3;
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.pushToolStripMenuItem.Text = "Push";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.pushToolStripMenuItem_Click);
            // 
            // rebaseToolStripMenuItem
            // 
            this.rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            this.rebaseToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.rebaseToolStripMenuItem.Text = "Rebase";
            this.rebaseToolStripMenuItem.Click += new System.EventHandler(this.rebaseToolStripMenuItem_Click);
            // 
            // runMergetoolToolStripMenuItem
            // 
            this.runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            this.runMergetoolToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.runMergetoolToolStripMenuItem.Text = "Solve mergeconflicts";
            this.runMergetoolToolStripMenuItem.Click += new System.EventHandler(this.runMergetoolToolStripMenuItem_Click);
            // 
            // stashToolStripMenuItem
            // 
            this.stashToolStripMenuItem.Image = global::GitUI.Properties.Resources.stash1;
            this.stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            this.stashToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.stashToolStripMenuItem.Text = "Stash changes";
            this.stashToolStripMenuItem.Click += new System.EventHandler(this.stashToolStripMenuItem_Click);
            // 
            // viewDiffToolStripMenuItem
            // 
            this.viewDiffToolStripMenuItem.Name = "viewDiffToolStripMenuItem";
            this.viewDiffToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.viewDiffToolStripMenuItem.Text = "View diff";
            this.viewDiffToolStripMenuItem.Click += new System.EventHandler(this.viewDiffToolStripMenuItem_Click);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.patchToolStripMenuItem.Text = "View patch file";
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.patchToolStripMenuItem_Click);
            // 
            // remotesToolStripMenuItem
            // 
            this.remotesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageRemoteRepositoriesToolStripMenuItem1,
            this.toolStripSeparator6,
            this.PuTTYToolStripMenuItem});
            this.remotesToolStripMenuItem.Name = "remotesToolStripMenuItem";
            this.remotesToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.remotesToolStripMenuItem.Text = "Remotes";
            // 
            // manageRemoteRepositoriesToolStripMenuItem1
            // 
            this.manageRemoteRepositoriesToolStripMenuItem1.Name = "manageRemoteRepositoriesToolStripMenuItem1";
            this.manageRemoteRepositoriesToolStripMenuItem1.Size = new System.Drawing.Size(222, 22);
            this.manageRemoteRepositoriesToolStripMenuItem1.Text = "Manage remote repositories";
            this.manageRemoteRepositoriesToolStripMenuItem1.Click += new System.EventHandler(this.manageRemoteRepositoriesToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(219, 6);
            // 
            // PuTTYToolStripMenuItem
            // 
            this.PuTTYToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAuthenticationAgentToolStripMenuItem,
            this.generateOrImportKeyToolStripMenuItem});
            this.PuTTYToolStripMenuItem.Image = global::GitUI.Properties.Resources.putty;
            this.PuTTYToolStripMenuItem.Name = "PuTTYToolStripMenuItem";
            this.PuTTYToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.PuTTYToolStripMenuItem.Text = "PuTTY";
            // 
            // startAuthenticationAgentToolStripMenuItem
            // 
            this.startAuthenticationAgentToolStripMenuItem.Image = global::GitUI.Properties.Resources.pageant;
            this.startAuthenticationAgentToolStripMenuItem.Name = "startAuthenticationAgentToolStripMenuItem";
            this.startAuthenticationAgentToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.startAuthenticationAgentToolStripMenuItem.Text = "Start authentication agent";
            this.startAuthenticationAgentToolStripMenuItem.Click += new System.EventHandler(this.startAuthenticationAgentToolStripMenuItem_Click);
            // 
            // generateOrImportKeyToolStripMenuItem
            // 
            this.generateOrImportKeyToolStripMenuItem.Image = global::GitUI.Properties.Resources.puttygen;
            this.generateOrImportKeyToolStripMenuItem.Name = "generateOrImportKeyToolStripMenuItem";
            this.generateOrImportKeyToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.generateOrImportKeyToolStripMenuItem.Text = "Generate or import key";
            this.generateOrImportKeyToolStripMenuItem.Click += new System.EventHandler(this.generateOrImportKeyToolStripMenuItem_Click);
            // 
            // submodulesToolStripMenuItem
            // 
            this.submodulesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageSubmodulesToolStripMenuItem,
            this.toolStripSeparator8,
            this.updateAllSubmodulesToolStripMenuItem,
            this.initializeAllSubmodulesToolStripMenuItem,
            this.syncronizeAllSubmodulesToolStripMenuItem,
            this.toolStripSeparator14,
            this.updateAllSubmodulesRecursiveToolStripMenuItem,
            this.initializeAllSubmodulesRecursiveToolStripMenuItem,
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem,
            this.toolStripSeparator10,
            this.openSubmoduleToolStripMenuItem});
            this.submodulesToolStripMenuItem.Name = "submodulesToolStripMenuItem";
            this.submodulesToolStripMenuItem.Size = new System.Drawing.Size(85, 20);
            this.submodulesToolStripMenuItem.Text = "Submodules";
            // 
            // manageSubmodulesToolStripMenuItem
            // 
            this.manageSubmodulesToolStripMenuItem.Name = "manageSubmodulesToolStripMenuItem";
            this.manageSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.manageSubmodulesToolStripMenuItem.Text = "Manage submodules";
            this.manageSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.manageSubmodulesToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(265, 6);
            // 
            // updateAllSubmodulesToolStripMenuItem
            // 
            this.updateAllSubmodulesToolStripMenuItem.Name = "updateAllSubmodulesToolStripMenuItem";
            this.updateAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.updateAllSubmodulesToolStripMenuItem.Text = "Update all submodules";
            this.updateAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.updateAllSubmodulesToolStripMenuItem_Click);
            // 
            // initializeAllSubmodulesToolStripMenuItem
            // 
            this.initializeAllSubmodulesToolStripMenuItem.Name = "initializeAllSubmodulesToolStripMenuItem";
            this.initializeAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.initializeAllSubmodulesToolStripMenuItem.Text = "Initialize all submodules";
            this.initializeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.initializeAllSubmodulesToolStripMenuItem_Click);
            // 
            // syncronizeAllSubmodulesToolStripMenuItem
            // 
            this.syncronizeAllSubmodulesToolStripMenuItem.Name = "syncronizeAllSubmodulesToolStripMenuItem";
            this.syncronizeAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.syncronizeAllSubmodulesToolStripMenuItem.Text = "Synchronize all submodules";
            this.syncronizeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.syncronizeAllSubmodulesToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(265, 6);
            // 
            // updateAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Name = "updateAllSubmodulesRecursiveToolStripMenuItem";
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Text = "Update all submodules recursive";
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.updateAllSubmodulesRecursiveToolStripMenuItem_Click);
            // 
            // initializeAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Name = "initializeAllSubmodulesRecursiveToolStripMenuItem";
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Text = "Initialize all submodules recursive";
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.initializeAllSubmodulesRecursiveToolStripMenuItem_Click);
            // 
            // synchronizeAlSubmodulesRecursiveToolStripMenuItem
            // 
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.Name = "synchronizeAlSubmodulesRecursiveToolStripMenuItem";
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.Text = "Synchronize al submodules recursive";
            this.synchronizeAlSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.synchronizeAlSubmodulesRecursiveToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(265, 6);
            // 
            // openSubmoduleToolStripMenuItem
            // 
            this.openSubmoduleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator11});
            this.openSubmoduleToolStripMenuItem.Name = "openSubmoduleToolStripMenuItem";
            this.openSubmoduleToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.openSubmoduleToolStripMenuItem.Text = "Browse submodule";
            this.openSubmoduleToolStripMenuItem.DropDownOpening += new System.EventHandler(this.openSubmoduleToolStripMenuItem_DropDownOpening);
            this.openSubmoduleToolStripMenuItem.Click += new System.EventHandler(this.openSubmoduleToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(57, 6);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.toolStripSeparator15});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(113, 6);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitMaintenanceToolStripMenuItem,
            this.toolStripSeparator4,
            this.editgitignoreToolStripMenuItem1,
            this.editmailmapToolStripMenuItem,
            this.toolStripSeparator13,
            this.settingsToolStripMenuItem2});
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem1.Text = "Settings";
            // 
            // gitMaintenanceToolStripMenuItem
            // 
            this.gitMaintenanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressGitDatabaseToolStripMenuItem,
            this.verifyGitDatabaseToolStripMenuItem});
            this.gitMaintenanceToolStripMenuItem.Image = global::GitUI.Properties.Resources._82;
            this.gitMaintenanceToolStripMenuItem.Name = "gitMaintenanceToolStripMenuItem";
            this.gitMaintenanceToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.gitMaintenanceToolStripMenuItem.Text = "Git maintenance";
            // 
            // compressGitDatabaseToolStripMenuItem
            // 
            this.compressGitDatabaseToolStripMenuItem.Name = "compressGitDatabaseToolStripMenuItem";
            this.compressGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.compressGitDatabaseToolStripMenuItem.Text = "Compress git database";
            this.compressGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.compressGitDatabaseToolStripMenuItem_Click);
            // 
            // verifyGitDatabaseToolStripMenuItem
            // 
            this.verifyGitDatabaseToolStripMenuItem.Name = "verifyGitDatabaseToolStripMenuItem";
            this.verifyGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.verifyGitDatabaseToolStripMenuItem.Text = "Recover lost objects";
            this.verifyGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.verifyGitDatabaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(158, 6);
            // 
            // editgitignoreToolStripMenuItem1
            // 
            this.editgitignoreToolStripMenuItem1.Name = "editgitignoreToolStripMenuItem1";
            this.editgitignoreToolStripMenuItem1.Size = new System.Drawing.Size(161, 22);
            this.editgitignoreToolStripMenuItem1.Text = "Edit .gitignore";
            this.editgitignoreToolStripMenuItem1.Click += new System.EventHandler(this.editgitignoreToolStripMenuItem1_Click);
            // 
            // editmailmapToolStripMenuItem
            // 
            this.editmailmapToolStripMenuItem.Name = "editmailmapToolStripMenuItem";
            this.editmailmapToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.editmailmapToolStripMenuItem.Text = "Edit .mailmap";
            this.editmailmapToolStripMenuItem.Click += new System.EventHandler(this.editmailmapToolStripMenuItem_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(158, 6);
            // 
            // settingsToolStripMenuItem2
            // 
            this.settingsToolStripMenuItem2.Image = global::GitUI.Properties.Resources._71;
            this.settingsToolStripMenuItem2.Name = "settingsToolStripMenuItem2";
            this.settingsToolStripMenuItem2.Size = new System.Drawing.Size(161, 22);
            this.settingsToolStripMenuItem2.Text = "Settings";
            this.settingsToolStripMenuItem2.Click += new System.EventHandler(this.settingsToolStripMenuItem2_Click);
            // 
            // helpToolStripMenuItem
            // 
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
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // commitcountPerUserToolStripMenuItem
            // 
            this.commitcountPerUserToolStripMenuItem.Image = global::GitUI.Properties.Resources._53;
            this.commitcountPerUserToolStripMenuItem.Name = "commitcountPerUserToolStripMenuItem";
            this.commitcountPerUserToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.commitcountPerUserToolStripMenuItem.Text = "Commits per user";
            this.commitcountPerUserToolStripMenuItem.Click += new System.EventHandler(this.commitcountPerUserToolStripMenuItem_Click);
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.Image = global::GitUI.Properties.Resources._21;
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.gitcommandLogToolStripMenuItem.Text = "Gitcommand log";
            this.gitcommandLogToolStripMenuItem.Click += new System.EventHandler(this.gitcommandLogToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(165, 6);
            // 
            // changelogToolStripMenuItem
            // 
            this.changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            this.changelogToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.changelogToolStripMenuItem.Text = "Changelog";
            this.changelogToolStripMenuItem.Click += new System.EventHandler(this.changelogToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(165, 6);
            // 
            // donateToolStripMenuItem
            // 
            this.donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            this.donateToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.donateToolStripMenuItem.Text = "Donate";
            this.donateToolStripMenuItem.Click += new System.EventHandler(this.donateToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::GitUI.Properties.Resources._49;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.aboutToolStripMenuItem.Text = "About";
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
            // userManualToolStripMenuItem
            // 
            this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            this.userManualToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.userManualToolStripMenuItem.Text = "User Manual";
            this.userManualToolStripMenuItem.Click += new System.EventHandler(this.userManualToolStripMenuItem_Click);
            // 
            // FormBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(796, 573);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormBrowse";
            this.Text = "Git Extensions";
            this.Load += new System.EventHandler(this.Browse_Load);
            this.Shown += new System.EventHandler(this.FormBrowse_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBrowse_FormClosing);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.NoGit.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            this.splitContainer6.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            this.splitContainer7.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.CommitInfo.ResumeLayout(false);
            this.Tree.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.Commit.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripButton Workingdir;
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
        private System.Windows.Forms.TabPage Commit;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox DiffFiles;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem changelogToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonPull;
        private System.Windows.Forms.ToolStripButton toolStripButtonPush;
        private FileViewer FileText;
        private FileViewer DiffText;
        private System.Windows.Forms.Panel NoGit;
        private System.Windows.Forms.Button Open;
        private System.Windows.Forms.Button Clone;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Init;
        private System.Windows.Forms.TabPage CommitInfo;
        private System.Windows.Forms.RichTextBox RevisionInfo;
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
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox RecentRepositoriesGroupBox;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Donate;
        private System.Windows.Forms.ContextMenuStrip TreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
    }
}