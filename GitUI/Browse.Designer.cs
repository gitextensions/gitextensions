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
            this.WarningText = new System.Windows.Forms.Label();
            this.Workingdir = new System.Windows.Forms.Label();
            this.CurrentBranch = new System.Windows.Forms.Label();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.Commits = new System.Windows.Forms.TabPage();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.FileChanges = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.guidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.GitTree = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.FileText = new ICSharpCode.TextEditor.TextEditorControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.branchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runMergetoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitBashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitGUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitcommandLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.Commits.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FileChanges)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
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
            this.splitContainer2.Panel1.Controls.Add(this.WarningText);
            this.splitContainer2.Panel1.Controls.Add(this.Workingdir);
            this.splitContainer2.Panel1.Controls.Add(this.CurrentBranch);
            this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(750, 519);
            this.splitContainer2.SplitterDistance = 33;
            this.splitContainer2.TabIndex = 2;
            // 
            // WarningText
            // 
            this.WarningText.AutoSize = true;
            this.WarningText.BackColor = System.Drawing.Color.LightSalmon;
            this.WarningText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningText.ForeColor = System.Drawing.Color.Black;
            this.WarningText.Location = new System.Drawing.Point(505, 10);
            this.WarningText.Name = "WarningText";
            this.WarningText.Size = new System.Drawing.Size(0, 13);
            this.WarningText.TabIndex = 4;
            this.WarningText.Click += new System.EventHandler(this.WarningText_Click);
            // 
            // Workingdir
            // 
            this.Workingdir.AutoSize = true;
            this.Workingdir.Location = new System.Drawing.Point(328, 10);
            this.Workingdir.Name = "Workingdir";
            this.Workingdir.Size = new System.Drawing.Size(10, 13);
            this.Workingdir.TabIndex = 3;
            this.Workingdir.Text = " ";
            this.Workingdir.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Workingdir.Click += new System.EventHandler(this.Workingdir_Click);
            // 
            // CurrentBranch
            // 
            this.CurrentBranch.AutoSize = true;
            this.CurrentBranch.Location = new System.Drawing.Point(12, 10);
            this.CurrentBranch.Name = "CurrentBranch";
            this.CurrentBranch.Size = new System.Drawing.Size(0, 13);
            this.CurrentBranch.TabIndex = 2;
            this.CurrentBranch.Click += new System.EventHandler(this.CurrentBranch_Click);
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
            this.splitContainer3.Panel1.Controls.Add(this.tabControl2);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(750, 482);
            this.splitContainer3.SplitterDistance = 214;
            this.splitContainer3.TabIndex = 1;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.Commits);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(750, 214);
            this.tabControl2.TabIndex = 1;
            // 
            // Commits
            // 
            this.Commits.Controls.Add(this.RevisionGrid);
            this.Commits.Location = new System.Drawing.Point(4, 22);
            this.Commits.Name = "Commits";
            this.Commits.Padding = new System.Windows.Forms.Padding(3);
            this.Commits.Size = new System.Drawing.Size(742, 188);
            this.Commits.TabIndex = 0;
            this.Commits.Text = "Commits";
            this.Commits.UseVisualStyleBackColor = true;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(3, 3);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.Size = new System.Drawing.Size(736, 182);
            this.RevisionGrid.TabIndex = 0;
            this.RevisionGrid.DoubleClick += new System.EventHandler(this.RevisionGrid_DoubleClick);
            this.RevisionGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RevisionGrid_MouseDoubleClick);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.FileChanges);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(742, 188);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "File changes";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // FileChanges
            // 
            this.FileChanges.AllowUserToAddRows = false;
            this.FileChanges.AllowUserToDeleteRows = false;
            this.FileChanges.AutoGenerateColumns = false;
            this.FileChanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FileChanges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.guidDataGridViewTextBoxColumn});
            this.FileChanges.DataSource = this.gitItemBindingSource;
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Location = new System.Drawing.Point(3, 3);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.ReadOnly = true;
            this.FileChanges.RowHeadersVisible = false;
            this.FileChanges.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.FileChanges.Size = new System.Drawing.Size(736, 182);
            this.FileChanges.TabIndex = 0;
            this.FileChanges.SelectionChanged += new System.EventHandler(this.FileChanges_SelectionChanged);
            this.FileChanges.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.FileChanges_CellContentClick);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // guidDataGridViewTextBoxColumn
            // 
            this.guidDataGridViewTextBoxColumn.DataPropertyName = "Guid";
            this.guidDataGridViewTextBoxColumn.HeaderText = "Guid";
            this.guidDataGridViewTextBoxColumn.Name = "guidDataGridViewTextBoxColumn";
            this.guidDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.GitTree);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer4.Size = new System.Drawing.Size(750, 264);
            this.splitContainer4.SplitterDistance = 225;
            this.splitContainer4.TabIndex = 1;
            // 
            // GitTree
            // 
            this.GitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitTree.Location = new System.Drawing.Point(0, 0);
            this.GitTree.Name = "GitTree";
            this.GitTree.Size = new System.Drawing.Size(225, 264);
            this.GitTree.TabIndex = 0;
            this.GitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTree_BeforeExpand);
            this.GitTree.DoubleClick += new System.EventHandler(this.GitTree_DoubleClick);
            this.GitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(521, 264);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.FileText);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(513, 238);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "View file";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.IsReadOnly = false;
            this.FileText.Location = new System.Drawing.Point(3, 3);
            this.FileText.Name = "FileText";
            this.FileText.Size = new System.Drawing.Size(507, 232);
            this.FileText.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this.gitToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(750, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFilesToolStripMenuItem,
            this.applyPatchToolStripMenuItem,
            this.branchToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.checkoutToolStripMenuItem,
            this.cherryPickToolStripMenuItem,
            this.deleteBranchToolStripMenuItem,
            this.cloneToolStripMenuItem,
            this.commitToolStripMenuItem,
            this.formatPatchToolStripMenuItem,
            this.initNewRepositoryToolStripMenuItem,
            this.mergeBranchToolStripMenuItem,
            this.pullToolStripMenuItem,
            this.runMergetoolToolStripMenuItem,
            this.pushToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.stashToolStripMenuItem,
            this.viewDiffToolStripMenuItem,
            this.patchToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // addFilesToolStripMenuItem
            // 
            this.addFilesToolStripMenuItem.Name = "addFilesToolStripMenuItem";
            this.addFilesToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addFilesToolStripMenuItem.Text = "AddFiles";
            this.addFilesToolStripMenuItem.Click += new System.EventHandler(this.addFilesToolStripMenuItem_Click);
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.applyPatchToolStripMenuItem.Text = "Apply patch";
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.applyPatchToolStripMenuItem_Click);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.branchToolStripMenuItem.Text = "Branch";
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.branchToolStripMenuItem_Click);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.checkoutBranchToolStripMenuItem_Click);
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.checkoutToolStripMenuItem.Text = "Checkout revision";
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.checkoutToolStripMenuItem_Click);
            // 
            // cherryPickToolStripMenuItem
            // 
            this.cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            this.cherryPickToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.cherryPickToolStripMenuItem.Text = "Cherry pick";
            this.cherryPickToolStripMenuItem.Click += new System.EventHandler(this.cherryPickToolStripMenuItem_Click);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchToolStripMenuItem_Click);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.cloneToolStripMenuItem.Text = "Clone";
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.cloneToolStripMenuItem_Click);
            // 
            // commitToolStripMenuItem
            // 
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            this.commitToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.commitToolStripMenuItem.Text = "Commit";
            this.commitToolStripMenuItem.Click += new System.EventHandler(this.commitToolStripMenuItem_Click);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.formatPatchToolStripMenuItem.Text = "Format patch";
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.formatPatchToolStripMenuItem_Click);
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "Init new repository";
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.initNewRepositoryToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.pullToolStripMenuItem.Text = "Pull";
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.pullToolStripMenuItem_Click);
            // 
            // runMergetoolToolStripMenuItem
            // 
            this.runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            this.runMergetoolToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.runMergetoolToolStripMenuItem.Text = "Run mergetool";
            this.runMergetoolToolStripMenuItem.Click += new System.EventHandler(this.runMergetoolToolStripMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.pushToolStripMenuItem.Text = "Push";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.pushToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // stashToolStripMenuItem
            // 
            this.stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            this.stashToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.stashToolStripMenuItem.Text = "Stash";
            this.stashToolStripMenuItem.Click += new System.EventHandler(this.stashToolStripMenuItem_Click);
            // 
            // viewDiffToolStripMenuItem
            // 
            this.viewDiffToolStripMenuItem.Name = "viewDiffToolStripMenuItem";
            this.viewDiffToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.viewDiffToolStripMenuItem.Text = "View diff";
            this.viewDiffToolStripMenuItem.Click += new System.EventHandler(this.viewDiffToolStripMenuItem_Click);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.patchToolStripMenuItem.Text = "View patch file";
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.patchToolStripMenuItem_Click);
            // 
            // gitToolStripMenuItem
            // 
            this.gitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitBashToolStripMenuItem,
            this.gitGUIToolStripMenuItem});
            this.gitToolStripMenuItem.Name = "gitToolStripMenuItem";
            this.gitToolStripMenuItem.Size = new System.Drawing.Size(34, 20);
            this.gitToolStripMenuItem.Text = "Git";
            // 
            // gitBashToolStripMenuItem
            // 
            this.gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            this.gitBashToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.gitBashToolStripMenuItem.Text = "Git bash";
            this.gitBashToolStripMenuItem.Click += new System.EventHandler(this.gitBashToolStripMenuItem_Click_1);
            // 
            // gitGUIToolStripMenuItem
            // 
            this.gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            this.gitGUIToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.gitGUIToolStripMenuItem.Text = "Git GUI";
            this.gitGUIToolStripMenuItem.Click += new System.EventHandler(this.gitGUIToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitcommandLogToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.gitcommandLogToolStripMenuItem.Text = "Gitcommand log";
            this.gitcommandLogToolStripMenuItem.Click += new System.EventHandler(this.gitcommandLogToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.mergeBranchToolStripMenuItem.Text = "Merge branches";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.mergeBranchToolStripMenuItem_Click);
            // 
            // FormBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 543);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormBrowse";
            this.Text = "Browse";
            this.Load += new System.EventHandler(this.Browse_Load);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.Commits.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FileChanges)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView GitTree;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage Commits;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DataGridView FileChanges;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.ToolStripMenuItem viewDiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem branchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem initNewRepositoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pushToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pullToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn guidDataGridViewTextBoxColumn;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem patchToolStripMenuItem;
        private ICSharpCode.TextEditor.TextEditorControl FileText;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.Label CurrentBranch;
        private System.Windows.Forms.ToolStripMenuItem applyPatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitBashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitGUIToolStripMenuItem;
        private System.Windows.Forms.Label Workingdir;
        private System.Windows.Forms.ToolStripMenuItem formatPatchToolStripMenuItem;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.ToolStripMenuItem gitcommandLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stashToolStripMenuItem;
        private System.Windows.Forms.Label WarningText;
        private System.Windows.Forms.ToolStripMenuItem runMergetoolToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.ToolStripMenuItem deleteBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBranchToolStripMenuItem;
    }
}