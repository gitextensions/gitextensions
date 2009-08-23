namespace GitUI
{
    partial class FormCommit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCommit));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.workingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetAlltrackedChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.eToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.Unstaged = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChangeString = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UnstagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ResetChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitItemStatusBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.UnstageFiles = new System.Windows.Forms.Button();
            this.AddFiles = new System.Windows.Forms.Button();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.filesListedToCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stageAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unstageAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.stageChunkOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Staged = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChangeString2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.SelectedDiff = new GitUI.FileViewer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.Cancel = new System.Windows.Forms.Button();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.Amend = new System.Windows.Forms.Button();
            this.AddManyFiles = new System.Windows.Forms.Button();
            this.Commit = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.Scan = new System.Windows.Forms.Button();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.splitContainer8 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.Message = new GitUI.EditNetSpell();
            this.OutPut = new System.Windows.Forms.RichTextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Unstaged)).BeginInit();
            this.UnstagedFileContext.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Staged)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            this.splitContainer8.Panel1.SuspendLayout();
            this.splitContainer8.Panel2.SuspendLayout();
            this.splitContainer8.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.Controls.Add(this.Ok);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(895, 648);
            this.splitContainer1.SplitterDistance = 389;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer2.Size = new System.Drawing.Size(389, 648);
            this.splitContainer2.SplitterDistance = 286;
            this.splitContainer2.TabIndex = 3;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.Loading);
            this.splitContainer4.Panel2.Controls.Add(this.Unstaged);
            this.splitContainer4.Size = new System.Drawing.Size(389, 286);
            this.splitContainer4.SplitterDistance = 25;
            this.splitContainer4.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.workingToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(389, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // workingToolStripMenuItem
            // 
            this.workingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteSelectedFilesToolStripMenuItem,
            this.resetSelectedFilesToolStripMenuItem,
            this.resetAlltrackedChangesToolStripMenuItem,
            this.toolStripSeparator1,
            this.eToolStripMenuItem,
            this.deleteAllUntrackedFilesToolStripMenuItem});
            this.workingToolStripMenuItem.Name = "workingToolStripMenuItem";
            this.workingToolStripMenuItem.Size = new System.Drawing.Size(128, 20);
            this.workingToolStripMenuItem.Text = "Working dir changes";
            // 
            // deleteSelectedFilesToolStripMenuItem
            // 
            this.deleteSelectedFilesToolStripMenuItem.Name = "deleteSelectedFilesToolStripMenuItem";
            this.deleteSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.deleteSelectedFilesToolStripMenuItem.Text = "Delete selected files";
            this.deleteSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedFilesToolStripMenuItem_Click);
            // 
            // resetSelectedFilesToolStripMenuItem
            // 
            this.resetSelectedFilesToolStripMenuItem.Name = "resetSelectedFilesToolStripMenuItem";
            this.resetSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.resetSelectedFilesToolStripMenuItem.Text = "Reset selected files";
            this.resetSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.resetSelectedFilesToolStripMenuItem_Click);
            // 
            // resetAlltrackedChangesToolStripMenuItem
            // 
            this.resetAlltrackedChangesToolStripMenuItem.Name = "resetAlltrackedChangesToolStripMenuItem";
            this.resetAlltrackedChangesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.resetAlltrackedChangesToolStripMenuItem.Text = "Reset all (tracked) changes";
            this.resetAlltrackedChangesToolStripMenuItem.Click += new System.EventHandler(this.resetAlltrackedChangesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(211, 6);
            // 
            // eToolStripMenuItem
            // 
            this.eToolStripMenuItem.Name = "eToolStripMenuItem";
            this.eToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.eToolStripMenuItem.Text = "Edit ignored files";
            this.eToolStripMenuItem.Click += new System.EventHandler(this.eToolStripMenuItem_Click);
            // 
            // deleteAllUntrackedFilesToolStripMenuItem
            // 
            this.deleteAllUntrackedFilesToolStripMenuItem.Name = "deleteAllUntrackedFilesToolStripMenuItem";
            this.deleteAllUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.deleteAllUntrackedFilesToolStripMenuItem.Text = "Delete all untracked files";
            this.deleteAllUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.deleteAllUntrackedFilesToolStripMenuItem_Click);
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(389, 257);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 2;
            this.Loading.TabStop = false;
            // 
            // Unstaged
            // 
            this.Unstaged.AllowUserToAddRows = false;
            this.Unstaged.AllowUserToDeleteRows = false;
            this.Unstaged.AutoGenerateColumns = false;
            this.Unstaged.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Unstaged.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn1,
            this.ChangeString});
            this.Unstaged.ContextMenuStrip = this.UnstagedFileContext;
            this.Unstaged.DataSource = this.gitItemStatusBindingSource;
            this.Unstaged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Unstaged.Location = new System.Drawing.Point(0, 0);
            this.Unstaged.Name = "Unstaged";
            this.Unstaged.ReadOnly = true;
            this.Unstaged.RowHeadersVisible = false;
            this.Unstaged.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Unstaged.Size = new System.Drawing.Size(389, 257);
            this.Unstaged.TabIndex = 0;
            this.Unstaged.DoubleClick += new System.EventHandler(this.Unstaged_DoubleClick);
            this.Unstaged.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Unstaged_CellMouseDown);
            this.Unstaged.SelectionChanged += new System.EventHandler(this.Untracked_SelectionChanged);
            this.Unstaged.Click += new System.EventHandler(this.Unstaged_Click);
            // 
            // nameDataGridViewTextBoxColumn1
            // 
            this.nameDataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameDataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn1.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn1.Name = "nameDataGridViewTextBoxColumn1";
            this.nameDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // ChangeString
            // 
            this.ChangeString.DataPropertyName = "ChangeString";
            this.ChangeString.HeaderText = "Change";
            this.ChangeString.Name = "ChangeString";
            this.ChangeString.ReadOnly = true;
            // 
            // UnstagedFileContext
            // 
            this.UnstagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ResetChanges,
            this.deleteFileToolStripMenuItem});
            this.UnstagedFileContext.Name = "UnstagedFileContext";
            this.UnstagedFileContext.Size = new System.Drawing.Size(169, 48);
            // 
            // ResetChanges
            // 
            this.ResetChanges.Name = "ResetChanges";
            this.ResetChanges.Size = new System.Drawing.Size(168, 22);
            this.ResetChanges.Text = "Reset file changes";
            this.ResetChanges.Click += new System.EventHandler(this.ResetSoft_Click);
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.deleteFileToolStripMenuItem.Text = "Delete file";
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.deleteFileToolStripMenuItem_Click);
            // 
            // gitItemStatusBindingSource
            // 
            this.gitItemStatusBindingSource.DataSource = typeof(GitCommands.GitItemStatus);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.progressBar);
            this.splitContainer5.Panel1.Controls.Add(this.UnstageFiles);
            this.splitContainer5.Panel1.Controls.Add(this.AddFiles);
            this.splitContainer5.Panel1.Controls.Add(this.menuStrip2);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.Staged);
            this.splitContainer5.Size = new System.Drawing.Size(389, 358);
            this.splitContainer5.SplitterDistance = 25;
            this.splitContainer5.TabIndex = 1;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(125, 1);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(262, 25);
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;
            // 
            // UnstageFiles
            // 
            this.UnstageFiles.Image = global::GitUI.Properties.Resources._3;
            this.UnstageFiles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UnstageFiles.Location = new System.Drawing.Point(124, 3);
            this.UnstageFiles.Name = "UnstageFiles";
            this.UnstageFiles.Size = new System.Drawing.Size(133, 23);
            this.UnstageFiles.TabIndex = 1;
            this.UnstageFiles.Text = "Unstage selected files";
            this.UnstageFiles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.UnstageFiles.UseVisualStyleBackColor = true;
            this.UnstageFiles.Click += new System.EventHandler(this.UnstageFiles_Click);
            // 
            // AddFiles
            // 
            this.AddFiles.Image = global::GitUI.Properties.Resources._4;
            this.AddFiles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddFiles.Location = new System.Drawing.Point(263, 3);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(123, 23);
            this.AddFiles.TabIndex = 4;
            this.AddFiles.Text = "Stage selected files";
            this.AddFiles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.Stage_Click);
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filesListedToCommitToolStripMenuItem});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(389, 24);
            this.menuStrip2.TabIndex = 5;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // filesListedToCommitToolStripMenuItem
            // 
            this.filesListedToCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stageAllToolStripMenuItem,
            this.unstageAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.stageChunkOfFileToolStripMenuItem});
            this.filesListedToCommitToolStripMenuItem.Name = "filesListedToCommitToolStripMenuItem";
            this.filesListedToCommitToolStripMenuItem.Size = new System.Drawing.Size(101, 20);
            this.filesListedToCommitToolStripMenuItem.Text = "Files to commit";
            // 
            // stageAllToolStripMenuItem
            // 
            this.stageAllToolStripMenuItem.Name = "stageAllToolStripMenuItem";
            this.stageAllToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.stageAllToolStripMenuItem.Text = "Stage all";
            this.stageAllToolStripMenuItem.Click += new System.EventHandler(this.stageAllToolStripMenuItem_Click);
            // 
            // unstageAllToolStripMenuItem
            // 
            this.unstageAllToolStripMenuItem.Name = "unstageAllToolStripMenuItem";
            this.unstageAllToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.unstageAllToolStripMenuItem.Text = "Unstage all";
            this.unstageAllToolStripMenuItem.Click += new System.EventHandler(this.unstageAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(169, 6);
            // 
            // stageChunkOfFileToolStripMenuItem
            // 
            this.stageChunkOfFileToolStripMenuItem.Name = "stageChunkOfFileToolStripMenuItem";
            this.stageChunkOfFileToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.stageChunkOfFileToolStripMenuItem.Text = "Stage chunk of file";
            this.stageChunkOfFileToolStripMenuItem.Click += new System.EventHandler(this.stageChunkOfFileToolStripMenuItem_Click);
            // 
            // Staged
            // 
            this.Staged.AllowUserToAddRows = false;
            this.Staged.AllowUserToDeleteRows = false;
            this.Staged.AutoGenerateColumns = false;
            this.Staged.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Staged.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.ChangeString2});
            this.Staged.DataSource = this.gitItemStatusBindingSource;
            this.Staged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Staged.Location = new System.Drawing.Point(0, 0);
            this.Staged.Name = "Staged";
            this.Staged.ReadOnly = true;
            this.Staged.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.Staged.RowHeadersVisible = false;
            this.Staged.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Staged.Size = new System.Drawing.Size(389, 329);
            this.Staged.TabIndex = 0;
            this.Staged.SelectionChanged += new System.EventHandler(this.Tracked_SelectionChanged);
            this.Staged.Click += new System.EventHandler(this.Staged_Click);
            this.Staged.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Staged_CellContentClick);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // ChangeString2
            // 
            this.ChangeString2.DataPropertyName = "ChangeString";
            this.ChangeString2.HeaderText = "Change";
            this.ChangeString2.Name = "ChangeString2";
            this.ChangeString2.ReadOnly = true;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(334, 10);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "Commit";
            this.Ok.UseVisualStyleBackColor = true;
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
            this.splitContainer3.Panel1.Controls.Add(this.SelectedDiff);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer6);
            this.splitContainer3.Size = new System.Drawing.Size(502, 648);
            this.splitContainer3.SplitterDistance = 316;
            this.splitContainer3.TabIndex = 0;
            // 
            // SelectedDiff
            // 
            this.SelectedDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedDiff.Location = new System.Drawing.Point(0, 0);
            this.SelectedDiff.Name = "SelectedDiff";
            this.SelectedDiff.Size = new System.Drawing.Size(502, 316);
            this.SelectedDiff.TabIndex = 0;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.SolveMergeconflicts);
            this.splitContainer6.Panel1.Controls.Add(this.Amend);
            this.splitContainer6.Panel1.Controls.Add(this.AddManyFiles);
            this.splitContainer6.Panel1.Controls.Add(this.Commit);
            this.splitContainer6.Panel1.Controls.Add(this.Reset);
            this.splitContainer6.Panel1.Controls.Add(this.Scan);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.splitContainer7);
            this.splitContainer6.Size = new System.Drawing.Size(502, 328);
            this.splitContainer6.SplitterDistance = 134;
            this.splitContainer6.TabIndex = 6;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(232, 104);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(129, 23);
            this.Cancel.TabIndex = 9;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(3, 168);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(129, 42);
            this.SolveMergeconflicts.TabIndex = 8;
            this.SolveMergeconflicts.Text = "There are unresolved mergeconflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeconflicts_Click);
            // 
            // Amend
            // 
            this.Amend.Location = new System.Drawing.Point(4, 29);
            this.Amend.Name = "Amend";
            this.Amend.Size = new System.Drawing.Size(127, 23);
            this.Amend.TabIndex = 7;
            this.Amend.Text = "Amend to last commit";
            this.Amend.UseVisualStyleBackColor = true;
            this.Amend.Click += new System.EventHandler(this.Amend_Click);
            // 
            // AddManyFiles
            // 
            this.AddManyFiles.Location = new System.Drawing.Point(4, 139);
            this.AddManyFiles.Name = "AddManyFiles";
            this.AddManyFiles.Size = new System.Drawing.Size(127, 23);
            this.AddManyFiles.TabIndex = 6;
            this.AddManyFiles.Text = "Add many files";
            this.AddManyFiles.UseVisualStyleBackColor = true;
            this.AddManyFiles.Click += new System.EventHandler(this.AddManyFiles_Click);
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(3, 3);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(129, 23);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(4, 110);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(128, 23);
            this.Reset.TabIndex = 5;
            this.Reset.Text = "Reset changed HARD";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // Scan
            // 
            this.Scan.Location = new System.Drawing.Point(3, 68);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(129, 23);
            this.Scan.TabIndex = 3;
            this.Scan.Text = "Rescan changes";
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
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
            this.splitContainer7.Panel1.Controls.Add(this.splitContainer8);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.OutPut);
            this.splitContainer7.Panel2.Controls.Add(this.Cancel);
            this.splitContainer7.Size = new System.Drawing.Size(364, 328);
            this.splitContainer7.SplitterDistance = 194;
            this.splitContainer7.TabIndex = 0;
            // 
            // splitContainer8
            // 
            this.splitContainer8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer8.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer8.Location = new System.Drawing.Point(0, 0);
            this.splitContainer8.Name = "splitContainer8";
            this.splitContainer8.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer8.Panel1
            // 
            this.splitContainer8.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer8.Panel2
            // 
            this.splitContainer8.Panel2.Controls.Add(this.Message);
            this.splitContainer8.Size = new System.Drawing.Size(364, 194);
            this.splitContainer8.SplitterDistance = 25;
            this.splitContainer8.TabIndex = 0;
            this.splitContainer8.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer8_SplitterMoved);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Commit message";
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Location = new System.Drawing.Point(0, 0);
            this.Message.MistakeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(364, 165);
            this.Message.TabIndex = 0;
            this.Message.Load += new System.EventHandler(this.Message_Load);
            // 
            // OutPut
            // 
            this.OutPut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutPut.Location = new System.Drawing.Point(0, 0);
            this.OutPut.Name = "OutPut";
            this.OutPut.ReadOnly = true;
            this.OutPut.Size = new System.Drawing.Size(364, 130);
            this.OutPut.TabIndex = 0;
            this.OutPut.Text = "";
            // 
            // FormCommit
            // 
            this.AcceptButton = this.Commit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(895, 648);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormCommit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commit";
            this.Load += new System.EventHandler(this.FormCommit_Load);
            this.Shown += new System.EventHandler(this.FormCommit_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCommit_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Unstaged)).EndInit();
            this.UnstagedFileContext.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).EndInit();
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Staged)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            this.splitContainer6.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            this.splitContainer7.ResumeLayout(false);
            this.splitContainer8.Panel1.ResumeLayout(false);
            this.splitContainer8.Panel1.PerformLayout();
            this.splitContainer8.Panel2.ResumeLayout(false);
            this.splitContainer8.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.RichTextBox OutPut;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView Unstaged;
        private System.Windows.Forms.DataGridView Staged;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button Scan;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.BindingSource gitItemStatusBindingSource;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isDeletedDataGridViewCheckBoxColumn1;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isChangedDataGridViewCheckBoxColumn1;
        private System.Windows.Forms.Button UnstageFiles;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.SplitContainer splitContainer8;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChangeString;
        private System.Windows.Forms.Button AddManyFiles;
        private System.Windows.Forms.PictureBox Loading;
        private System.Windows.Forms.Button Amend;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChangeString2;
        private System.Windows.Forms.ContextMenuStrip UnstagedFileContext;
        private System.Windows.Forms.ToolStripMenuItem ResetChanges;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem workingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetAlltrackedChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem filesListedToCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stageAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unstageAllToolStripMenuItem;
        private FileViewer SelectedDiff;
        private EditNetSpell Message;
        private System.Windows.Forms.ToolStripMenuItem deleteAllUntrackedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem stageChunkOfFileToolStripMenuItem;
        private System.Windows.Forms.Button Cancel;
    }
}