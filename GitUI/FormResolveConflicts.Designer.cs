namespace GitUI
{
    partial class FormResolveConflicts
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ConflictedFiles = new System.Windows.Forms.DataGridView();
            this.ConflictedFilesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenMergetool = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMarkAsSolved = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextChooseBase = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextChooseLocal = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextChooseRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextOpenBaseWith = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextOpenLocalWith = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextOpenRemoteWith = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextSaveBaseAs = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextSaveLocalAs = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextSaveRemoteAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.localFileName = new System.Windows.Forms.Label();
            this.baseFileName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.remoteFileName = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.merge = new System.Windows.Forms.Button();
            this.conflictDescription = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.startMergetool = new System.Windows.Forms.Button();
            this.openMergeToolBtn = new System.Windows.Forms.Button();
            this.Rescan = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.guidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commitGuidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.subItemsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).BeginInit();
            this.ConflictedFilesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subItemsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.startMergetool);
            this.splitContainer1.Panel2.Controls.Add(this.openMergeToolBtn);
            this.splitContainer1.Panel2.Controls.Add(this.Rescan);
            this.splitContainer1.Panel2.Controls.Add(this.Reset);
            this.splitContainer1.Size = new System.Drawing.Size(620, 388);
            this.splitContainer1.SplitterDistance = 445;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer2.Size = new System.Drawing.Size(445, 388);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Unresolved merge conflicts";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.ConflictedFiles, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 53F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(445, 359);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // ConflictedFiles
            // 
            this.ConflictedFiles.AllowUserToAddRows = false;
            this.ConflictedFiles.AllowUserToDeleteRows = false;
            this.ConflictedFiles.AllowUserToResizeColumns = false;
            this.ConflictedFiles.AllowUserToResizeRows = false;
            this.ConflictedFiles.AutoGenerateColumns = false;
            this.ConflictedFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ConflictedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConflictedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileName,
            this.authorDataGridViewTextBoxColumn1});
            this.ConflictedFiles.ContextMenuStrip = this.ConflictedFilesContextMenu;
            this.ConflictedFiles.DataSource = this.gitItemBindingSource;
            this.ConflictedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConflictedFiles.Location = new System.Drawing.Point(3, 3);
            this.ConflictedFiles.MultiSelect = false;
            this.ConflictedFiles.Name = "ConflictedFiles";
            this.ConflictedFiles.ReadOnly = true;
            this.ConflictedFiles.RowHeadersVisible = false;
            this.ConflictedFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ConflictedFiles.Size = new System.Drawing.Size(439, 225);
            this.ConflictedFiles.TabIndex = 5;
            this.ConflictedFiles.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ConflictedFiles_CellMouseDown);
            this.ConflictedFiles.SelectionChanged += new System.EventHandler(this.ConflictedFiles_SelectionChanged);
            this.ConflictedFiles.DoubleClick += new System.EventHandler(this.ConflictedFiles_DoubleClick);
            this.ConflictedFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConflictedFiles_KeyDown);
            // 
            // ConflictedFilesContextMenu
            // 
            this.ConflictedFilesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenMergetool,
            this.ContextMarkAsSolved,
            this.toolStripSeparator3,
            this.ContextChooseBase,
            this.ContextChooseLocal,
            this.ContextChooseRemote,
            this.toolStripSeparator1,
            this.ContextOpenBaseWith,
            this.ContextOpenLocalWith,
            this.ContextOpenRemoteWith,
            this.toolStripSeparator2,
            this.ContextSaveBaseAs,
            this.ContextSaveLocalAs,
            this.ContextSaveRemoteAs,
            this.toolStripMenuItem1,
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.toolStripSeparator4,
            this.fileHistoryToolStripMenuItem});
            this.ConflictedFilesContextMenu.Name = "ConflictedFilesContextMenu";
            this.ConflictedFilesContextMenu.Size = new System.Drawing.Size(230, 370);
            this.ConflictedFilesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ConflictedFilesContextMenu_Opening);
            // 
            // OpenMergetool
            // 
            this.OpenMergetool.Name = "OpenMergetool";
            this.OpenMergetool.Size = new System.Drawing.Size(229, 24);
            this.OpenMergetool.Text = "Open in mergetool";
            this.OpenMergetool.Click += new System.EventHandler(this.OpenMergetool_Click);
            // 
            // ContextMarkAsSolved
            // 
            this.ContextMarkAsSolved.Name = "ContextMarkAsSolved";
            this.ContextMarkAsSolved.Size = new System.Drawing.Size(229, 24);
            this.ContextMarkAsSolved.Text = "Mark conflict as solved";
            this.ContextMarkAsSolved.Click += new System.EventHandler(this.ContextMarkAsSolved_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(226, 6);
            // 
            // ContextChooseBase
            // 
            this.ContextChooseBase.Name = "ContextChooseBase";
            this.ContextChooseBase.Size = new System.Drawing.Size(229, 24);
            this.ContextChooseBase.Text = "Choose base";
            this.ContextChooseBase.Click += new System.EventHandler(this.ContextChooseBase_Click);
            // 
            // ContextChooseLocal
            // 
            this.ContextChooseLocal.Name = "ContextChooseLocal";
            this.ContextChooseLocal.Size = new System.Drawing.Size(229, 24);
            this.ContextChooseLocal.Text = "Choose local";
            this.ContextChooseLocal.Click += new System.EventHandler(this.ContextChooseLocal_Click);
            // 
            // ContextChooseRemote
            // 
            this.ContextChooseRemote.Name = "ContextChooseRemote";
            this.ContextChooseRemote.Size = new System.Drawing.Size(229, 24);
            this.ContextChooseRemote.Text = "Choose remote";
            this.ContextChooseRemote.Click += new System.EventHandler(this.ContextChooseRemote_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(226, 6);
            // 
            // ContextOpenBaseWith
            // 
            this.ContextOpenBaseWith.Name = "ContextOpenBaseWith";
            this.ContextOpenBaseWith.Size = new System.Drawing.Size(229, 24);
            this.ContextOpenBaseWith.Text = "Open base with";
            this.ContextOpenBaseWith.Click += new System.EventHandler(this.ContextOpenBaseWith_Click);
            // 
            // ContextOpenLocalWith
            // 
            this.ContextOpenLocalWith.Name = "ContextOpenLocalWith";
            this.ContextOpenLocalWith.Size = new System.Drawing.Size(229, 24);
            this.ContextOpenLocalWith.Text = "Open local with";
            this.ContextOpenLocalWith.Click += new System.EventHandler(this.ContextOpenLocalWith_Click);
            // 
            // ContextOpenRemoteWith
            // 
            this.ContextOpenRemoteWith.Name = "ContextOpenRemoteWith";
            this.ContextOpenRemoteWith.Size = new System.Drawing.Size(229, 24);
            this.ContextOpenRemoteWith.Text = "Open remote with";
            this.ContextOpenRemoteWith.Click += new System.EventHandler(this.ContextOpenRemoteWith_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
            // 
            // ContextSaveBaseAs
            // 
            this.ContextSaveBaseAs.Name = "ContextSaveBaseAs";
            this.ContextSaveBaseAs.Size = new System.Drawing.Size(229, 24);
            this.ContextSaveBaseAs.Text = "Save base as";
            this.ContextSaveBaseAs.Click += new System.EventHandler(this.ContextSaveBaseAs_Click);
            // 
            // ContextSaveLocalAs
            // 
            this.ContextSaveLocalAs.Name = "ContextSaveLocalAs";
            this.ContextSaveLocalAs.Size = new System.Drawing.Size(229, 24);
            this.ContextSaveLocalAs.Text = "Save local as";
            this.ContextSaveLocalAs.Click += new System.EventHandler(this.ContextSaveLocalAs_Click);
            // 
            // ContextSaveRemoteAs
            // 
            this.ContextSaveRemoteAs.Name = "ContextSaveRemoteAs";
            this.ContextSaveRemoteAs.Size = new System.Drawing.Size(229, 24);
            this.ContextSaveRemoteAs.Text = "Save remote as";
            this.ContextSaveRemoteAs.Click += new System.EventHandler(this.ContextSaveRemoteAs_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(226, 6);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(229, 24);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(229, 24);
            this.openWithToolStripMenuItem.Text = "Open With";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(226, 6);
            // 
            // fileHistoryToolStripMenuItem
            // 
            this.fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            this.fileHistoryToolStripMenuItem.Size = new System.Drawing.Size(229, 24);
            this.fileHistoryToolStripMenuItem.Text = "File history";
            this.fileHistoryToolStripMenuItem.Click += new System.EventHandler(this.fileHistoryToolStripMenuItem_Click);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 59F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.localFileName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.baseFileName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.remoteFileName, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 284);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(445, 75);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 20);
            this.label7.TabIndex = 1;
            this.label7.Text = "Local";
            // 
            // localFileName
            // 
            this.localFileName.AutoSize = true;
            this.localFileName.Location = new System.Drawing.Point(62, 0);
            this.localFileName.Name = "localFileName";
            this.localFileName.Size = new System.Drawing.Size(18, 20);
            this.localFileName.TabIndex = 3;
            this.localFileName.Text = "...";
            // 
            // baseFileName
            // 
            this.baseFileName.AutoSize = true;
            this.baseFileName.Location = new System.Drawing.Point(62, 25);
            this.baseFileName.Name = "baseFileName";
            this.baseFileName.Size = new System.Drawing.Size(18, 20);
            this.baseFileName.TabIndex = 4;
            this.baseFileName.Text = "...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Base";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 25);
            this.label5.TabIndex = 5;
            this.label5.Text = "Remote";
            // 
            // remoteFileName
            // 
            this.remoteFileName.AutoSize = true;
            this.remoteFileName.Location = new System.Drawing.Point(62, 50);
            this.remoteFileName.Name = "remoteFileName";
            this.remoteFileName.Size = new System.Drawing.Size(18, 20);
            this.remoteFileName.TabIndex = 6;
            this.remoteFileName.Text = "...";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel3.Controls.Add(this.merge, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.conflictDescription, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 231);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(445, 53);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // merge
            // 
            this.merge.ContextMenuStrip = this.ConflictedFilesContextMenu;
            this.merge.Image = global::GitUI.Properties.Resources.Select;
            this.merge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.merge.Location = new System.Drawing.Point(315, 0);
            this.merge.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.merge.Name = "merge";
            this.merge.Size = new System.Drawing.Size(130, 25);
            this.merge.TabIndex = 8;
            this.merge.TabStop = false;
            this.merge.Text = "Merge";
            this.merge.UseVisualStyleBackColor = true;
            this.merge.Click += new System.EventHandler(this.merge_Click);
            // 
            // conflictDescription
            // 
            this.conflictDescription.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.conflictDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conflictDescription.Location = new System.Drawing.Point(19, 0);
            this.conflictDescription.Name = "conflictDescription";
            this.conflictDescription.Size = new System.Drawing.Size(293, 53);
            this.conflictDescription.TabIndex = 2;
            this.conflictDescription.Text = "Select file";
            this.conflictDescription.Click += new System.EventHandler(this.conflictDescription_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::GitUI.Properties.Resources.Info;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 53);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // startMergetool
            // 
            this.startMergetool.Location = new System.Drawing.Point(2, 33);
            this.startMergetool.Name = "startMergetool";
            this.startMergetool.Size = new System.Drawing.Size(166, 25);
            this.startMergetool.TabIndex = 10;
            this.startMergetool.Text = "Start mergetool";
            this.startMergetool.UseVisualStyleBackColor = true;
            this.startMergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // openMergeToolBtn
            // 
            this.openMergeToolBtn.Location = new System.Drawing.Point(2, 4);
            this.openMergeToolBtn.Name = "openMergeToolBtn";
            this.openMergeToolBtn.Size = new System.Drawing.Size(166, 25);
            this.openMergeToolBtn.TabIndex = 1;
            this.openMergeToolBtn.Text = "Open in mergetool";
            this.openMergeToolBtn.UseVisualStyleBackColor = true;
            this.openMergeToolBtn.Click += new System.EventHandler(this.OpenMergetool_Click);
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(2, 62);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(166, 25);
            this.Rescan.TabIndex = 7;
            this.Rescan.Text = "Rescan mergeconflicts";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(2, 92);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(166, 25);
            this.Reset.TabIndex = 9;
            this.Reset.Text = "Abort";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // guidDataGridViewTextBoxColumn
            // 
            this.guidDataGridViewTextBoxColumn.DataPropertyName = "Guid";
            this.guidDataGridViewTextBoxColumn.HeaderText = "Guid";
            this.guidDataGridViewTextBoxColumn.Name = "guidDataGridViewTextBoxColumn";
            this.guidDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // commitGuidDataGridViewTextBoxColumn
            // 
            this.commitGuidDataGridViewTextBoxColumn.DataPropertyName = "CommitGuid";
            this.commitGuidDataGridViewTextBoxColumn.HeaderText = "CommitGuid";
            this.commitGuidDataGridViewTextBoxColumn.Name = "commitGuidDataGridViewTextBoxColumn";
            this.commitGuidDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // itemTypeDataGridViewTextBoxColumn
            // 
            this.itemTypeDataGridViewTextBoxColumn.DataPropertyName = "ItemType";
            this.itemTypeDataGridViewTextBoxColumn.HeaderText = "ItemType";
            this.itemTypeDataGridViewTextBoxColumn.Name = "itemTypeDataGridViewTextBoxColumn";
            this.itemTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn
            // 
            this.authorDataGridViewTextBoxColumn.DataPropertyName = "Author";
            this.authorDataGridViewTextBoxColumn.HeaderText = "Author";
            this.authorDataGridViewTextBoxColumn.Name = "authorDataGridViewTextBoxColumn";
            this.authorDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.DataPropertyName = "Date";
            this.dateDataGridViewTextBoxColumn.HeaderText = "Date";
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            this.dateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // fileNameDataGridViewTextBoxColumn
            // 
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn.HeaderText = "FileName";
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // modeDataGridViewTextBoxColumn
            // 
            this.modeDataGridViewTextBoxColumn.DataPropertyName = "Mode";
            this.modeDataGridViewTextBoxColumn.HeaderText = "Mode";
            this.modeDataGridViewTextBoxColumn.Name = "modeDataGridViewTextBoxColumn";
            this.modeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // subItemsBindingSource
            // 
            this.subItemsBindingSource.DataMember = "SubItems";
            this.subItemsBindingSource.DataSource = this.gitItemBindingSource;
            // 
            // FileName
            // 
            this.FileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FileName.DataPropertyName = "FileName";
            this.FileName.HeaderText = "Filename";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn1
            // 
            this.authorDataGridViewTextBoxColumn1.DataPropertyName = "Author";
            this.authorDataGridViewTextBoxColumn1.HeaderText = "Author";
            this.authorDataGridViewTextBoxColumn1.Name = "authorDataGridViewTextBoxColumn1";
            this.authorDataGridViewTextBoxColumn1.ReadOnly = true;
            this.authorDataGridViewTextBoxColumn1.Visible = false;
            // 
            // FormResolveConflicts
            // 
            this.AcceptButton = this.merge;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 388);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(450, 300);
            this.Name = "FormResolveConflicts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resolve merge conflicts";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormResolveConflicts_FormClosing);
            this.Load += new System.EventHandler(this.FormResolveConflicts_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).EndInit();
            this.ConflictedFilesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subItemsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.ContextMenuStrip ConflictedFilesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ContextChooseBase;
        private System.Windows.Forms.ToolStripMenuItem ContextChooseLocal;
        private System.Windows.Forms.ToolStripMenuItem ContextChooseRemote;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ContextOpenBaseWith;
        private System.Windows.Forms.ToolStripMenuItem ContextOpenLocalWith;
        private System.Windows.Forms.ToolStripMenuItem ContextOpenRemoteWith;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem OpenMergetool;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem ContextSaveBaseAs;
        private System.Windows.Forms.ToolStripMenuItem ContextSaveLocalAs;
        private System.Windows.Forms.ToolStripMenuItem ContextSaveRemoteAs;
        private System.Windows.Forms.ToolStripMenuItem ContextMarkAsSolved;
        private System.Windows.Forms.DataGridViewTextBoxColumn guidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn commitGuidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn modeDataGridViewTextBoxColumn;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label localFileName;
        private System.Windows.Forms.Label baseFileName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label remoteFileName;
        private System.Windows.Forms.Label conflictDescription;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView ConflictedFiles;
        private System.Windows.Forms.Button openMergeToolBtn;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.Button startMergetool;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button merge;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.BindingSource subItemsBindingSource;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn1;
    }
}