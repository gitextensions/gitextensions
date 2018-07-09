using GitCommands.Git;

namespace GitUI.CommandsDialogs
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
            this.label1 = new System.Windows.Forms.Label();
            this.ConflictedFiles = new System.Windows.Forms.DataGridView();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConflictedFilesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenMergetool = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMarkAsSolved = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextChooseLocal = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextChooseRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextChooseBase = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextOpenLocalWith = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextOpenRemoteWith = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextOpenBaseWith = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextSaveLocalAs = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextSaveRemoteAs = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextSaveBaseAs = new System.Windows.Forms.ToolStripMenuItem();
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
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).BeginInit();
            this.ConflictedFilesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Unresolved merge conflicts";
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
            this.ConflictedFiles.Location = new System.Drawing.Point(3, 19);
            this.ConflictedFiles.Name = "ConflictedFiles";
            this.ConflictedFiles.ReadOnly = true;
            this.ConflictedFiles.RowHeadersVisible = false;
            this.ConflictedFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ConflictedFiles.Size = new System.Drawing.Size(455, 221);
            this.ConflictedFiles.TabIndex = 1;
            this.ConflictedFiles.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ConflictedFiles_CellMouseDown);
            this.ConflictedFiles.SelectionChanged += new System.EventHandler(this.ConflictedFiles_SelectionChanged);
            this.ConflictedFiles.DoubleClick += new System.EventHandler(this.ConflictedFiles_DoubleClick);
            this.ConflictedFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConflictedFiles_KeyDown);
            // 
            // FileName
            // 
            this.FileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FileName.HeaderText = "Filename";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn1
            // 
            this.authorDataGridViewTextBoxColumn1.HeaderText = "Author";
            this.authorDataGridViewTextBoxColumn1.Name = "authorDataGridViewTextBoxColumn1";
            this.authorDataGridViewTextBoxColumn1.ReadOnly = true;
            this.authorDataGridViewTextBoxColumn1.Visible = false;
            // 
            // ConflictedFilesContextMenu
            // 
            this.ConflictedFilesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenMergetool,
            this.ContextMarkAsSolved,
            this.toolStripSeparator3,
            this.ContextChooseLocal,
            this.ContextChooseRemote,
            this.ContextChooseBase,
            this.toolStripSeparator1,
            this.ContextOpenLocalWith,
            this.ContextOpenRemoteWith,
            this.ContextOpenBaseWith,
            this.toolStripSeparator2,
            this.ContextSaveLocalAs,
            this.ContextSaveRemoteAs,
            this.ContextSaveBaseAs,
            this.toolStripMenuItem1,
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.toolStripSeparator4,
            this.fileHistoryToolStripMenuItem});
            this.ConflictedFilesContextMenu.Name = "ConflictedFilesContextMenu";
            this.ConflictedFilesContextMenu.Size = new System.Drawing.Size(196, 342);
            this.ConflictedFilesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ConflictedFilesContextMenu_Opening);
            // 
            // OpenMergetool
            // 
            this.OpenMergetool.Name = "OpenMergetool";
            this.OpenMergetool.Size = new System.Drawing.Size(195, 22);
            this.OpenMergetool.Text = "Open in mergetool";
            this.OpenMergetool.Click += new System.EventHandler(this.OpenMergetool_Click);
            // 
            // ContextMarkAsSolved
            // 
            this.ContextMarkAsSolved.Name = "ContextMarkAsSolved";
            this.ContextMarkAsSolved.Size = new System.Drawing.Size(195, 22);
            this.ContextMarkAsSolved.Text = "Mark conflict as solved";
            this.ContextMarkAsSolved.Click += new System.EventHandler(this.ContextMarkAsSolved_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(192, 6);
            // 
            // ContextChooseLocal
            // 
            this.ContextChooseLocal.Name = "ContextChooseLocal";
            this.ContextChooseLocal.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.ContextChooseLocal.Size = new System.Drawing.Size(195, 22);
            this.ContextChooseLocal.Text = "Choose local";
            this.ContextChooseLocal.Click += new System.EventHandler(this.ContextChooseLocal_Click);
            // 
            // ContextChooseRemote
            // 
            this.ContextChooseRemote.Name = "ContextChooseRemote";
            this.ContextChooseRemote.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
            this.ContextChooseRemote.Size = new System.Drawing.Size(195, 22);
            this.ContextChooseRemote.Text = "Choose remote";
            this.ContextChooseRemote.Click += new System.EventHandler(this.ContextChooseRemote_Click);
            // 
            // ContextChooseBase
            // 
            this.ContextChooseBase.Name = "ContextChooseBase";
            this.ContextChooseBase.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D3)));
            this.ContextChooseBase.Size = new System.Drawing.Size(195, 22);
            this.ContextChooseBase.Text = "Choose base";
            this.ContextChooseBase.Click += new System.EventHandler(this.ContextChooseBase_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
            // 
            // ContextOpenLocalWith
            // 
            this.ContextOpenLocalWith.Name = "ContextOpenLocalWith";
            this.ContextOpenLocalWith.Size = new System.Drawing.Size(195, 22);
            this.ContextOpenLocalWith.Text = "Open local with";
            this.ContextOpenLocalWith.Click += new System.EventHandler(this.ContextOpenLocalWith_Click);
            // 
            // ContextOpenRemoteWith
            // 
            this.ContextOpenRemoteWith.Name = "ContextOpenRemoteWith";
            this.ContextOpenRemoteWith.Size = new System.Drawing.Size(195, 22);
            this.ContextOpenRemoteWith.Text = "Open remote with";
            this.ContextOpenRemoteWith.Click += new System.EventHandler(this.ContextOpenRemoteWith_Click);
            // 
            // ContextOpenBaseWith
            // 
            this.ContextOpenBaseWith.Name = "ContextOpenBaseWith";
            this.ContextOpenBaseWith.Size = new System.Drawing.Size(195, 22);
            this.ContextOpenBaseWith.Text = "Open base with";
            this.ContextOpenBaseWith.Click += new System.EventHandler(this.ContextOpenBaseWith_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(192, 6);
            // 
            // ContextSaveLocalAs
            // 
            this.ContextSaveLocalAs.Name = "ContextSaveLocalAs";
            this.ContextSaveLocalAs.Size = new System.Drawing.Size(195, 22);
            this.ContextSaveLocalAs.Text = "Save local as";
            this.ContextSaveLocalAs.Click += new System.EventHandler(this.ContextSaveLocalAs_Click);
            // 
            // ContextSaveRemoteAs
            // 
            this.ContextSaveRemoteAs.Name = "ContextSaveRemoteAs";
            this.ContextSaveRemoteAs.Size = new System.Drawing.Size(195, 22);
            this.ContextSaveRemoteAs.Text = "Save remote as";
            this.ContextSaveRemoteAs.Click += new System.EventHandler(this.ContextSaveRemoteAs_Click);
            // 
            // ContextSaveBaseAs
            // 
            this.ContextSaveBaseAs.Name = "ContextSaveBaseAs";
            this.ContextSaveBaseAs.Size = new System.Drawing.Size(195, 22);
            this.ContextSaveBaseAs.Text = "Save base as";
            this.ContextSaveBaseAs.Click += new System.EventHandler(this.ContextSaveBaseAs_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(192, 6);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.openWithToolStripMenuItem.Text = "Open With";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
            // 
            // fileHistoryToolStripMenuItem
            // 
            this.fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            this.fileHistoryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.fileHistoryToolStripMenuItem.Text = "File history";
            this.fileHistoryToolStripMenuItem.Click += new System.EventHandler(this.fileHistoryToolStripMenuItem_Click);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitItem);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.localFileName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.baseFileName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.remoteFileName, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 296);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(467, 66);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label7.Size = new System.Drawing.Size(37, 22);
            this.label7.TabIndex = 1;
            this.label7.Text = "Local";
            // 
            // localFileName
            // 
            this.localFileName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.localFileName.AutoSize = true;
            this.localFileName.Location = new System.Drawing.Point(61, 3);
            this.localFileName.Name = "localFileName";
            this.localFileName.Size = new System.Drawing.Size(20, 16);
            this.localFileName.TabIndex = 3;
            this.localFileName.Text = "...";
            // 
            // baseFileName
            // 
            this.baseFileName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.baseFileName.AutoSize = true;
            this.baseFileName.Location = new System.Drawing.Point(61, 25);
            this.baseFileName.Name = "baseFileName";
            this.baseFileName.Size = new System.Drawing.Size(20, 16);
            this.baseFileName.TabIndex = 4;
            this.baseFileName.Text = "...";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 22);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label2.Size = new System.Drawing.Size(35, 22);
            this.label2.TabIndex = 2;
            this.label2.Text = "Base";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 44);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label5.Size = new System.Drawing.Size(52, 22);
            this.label5.TabIndex = 5;
            this.label5.Text = "Remote";
            // 
            // remoteFileName
            // 
            this.remoteFileName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.remoteFileName.AutoSize = true;
            this.remoteFileName.Location = new System.Drawing.Point(61, 47);
            this.remoteFileName.Name = "remoteFileName";
            this.remoteFileName.Size = new System.Drawing.Size(20, 16);
            this.remoteFileName.TabIndex = 6;
            this.remoteFileName.Text = "...";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.merge, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.conflictDescription, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 243);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(461, 27);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // merge
            // 
            this.merge.ContextMenuStrip = this.ConflictedFilesContextMenu;
            this.merge.Image = global::GitUI.Properties.Images.Select;
            this.merge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.merge.Location = new System.Drawing.Point(331, 0);
            this.merge.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.merge.Name = "merge";
            this.merge.Size = new System.Drawing.Size(130, 25);
            this.merge.TabIndex = 2;
            this.merge.TabStop = false;
            this.merge.Text = "Merge";
            this.merge.UseVisualStyleBackColor = true;
            this.merge.Click += new System.EventHandler(this.merge_Click);
            // 
            // conflictDescription
            // 
            this.conflictDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.conflictDescription.AutoSize = true;
            this.conflictDescription.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.conflictDescription.Location = new System.Drawing.Point(19, 5);
            this.conflictDescription.Name = "conflictDescription";
            this.conflictDescription.Size = new System.Drawing.Size(64, 16);
            this.conflictDescription.TabIndex = 2;
            this.conflictDescription.Text = "Select file";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pictureBox1.BackgroundImage = global::GitUI.Properties.Images.Information;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Location = new System.Drawing.Point(0, 5);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // startMergetool
            // 
            this.startMergetool.Location = new System.Drawing.Point(3, 34);
            this.startMergetool.Name = "startMergetool";
            this.startMergetool.Size = new System.Drawing.Size(140, 25);
            this.startMergetool.TabIndex = 4;
            this.startMergetool.Text = "Start mergetool";
            this.startMergetool.UseVisualStyleBackColor = true;
            this.startMergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // openMergeToolBtn
            // 
            this.openMergeToolBtn.Location = new System.Drawing.Point(3, 3);
            this.openMergeToolBtn.Name = "openMergeToolBtn";
            this.openMergeToolBtn.Size = new System.Drawing.Size(140, 25);
            this.openMergeToolBtn.TabIndex = 3;
            this.openMergeToolBtn.Text = "Open in mergetool";
            this.openMergeToolBtn.UseVisualStyleBackColor = true;
            this.openMergeToolBtn.Click += new System.EventHandler(this.OpenMergetool_Click);
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(3, 65);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(140, 25);
            this.Rescan.TabIndex = 5;
            this.Rescan.Text = "Rescan merge conflicts";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(3, 96);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(140, 25);
            this.Reset.TabIndex = 6;
            this.Reset.Text = "Abort";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.gotoUserManualControl1, 0, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(619, 392);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.openMergeToolBtn);
            this.flowLayoutPanel1.Controls.Add(this.startMergetool);
            this.flowLayoutPanel1.Controls.Add(this.Rescan);
            this.flowLayoutPanel1.Controls.Add(this.Reset);
            this.flowLayoutPanel1.Controls.Add(this.progressBar);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(470, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(146, 290);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(3, 127);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(140, 24);
            this.progressBar.TabIndex = 7;
            this.progressBar.Visible = false;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.ConflictedFiles, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 4;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(461, 290);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gotoUserManualControl1.AutoSize = true;
            this.gotoUserManualControl1.Location = new System.Drawing.Point(3, 369);
            this.gotoUserManualControl1.ManualSectionAnchorName = null;
            this.gotoUserManualControl1.ManualSectionSubfolder = "merge_conflicts";
            this.gotoUserManualControl1.MinimumSize = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.TabIndex = 2;
            // 
            // FormResolveConflicts
            // 
            this.AcceptButton = this.merge;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(619, 392);
            this.Controls.Add(this.tableLayoutPanel4);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(450, 299);
            this.Name = "FormResolveConflicts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resolve merge conflicts";
            this.Load += new System.EventHandler(this.FormResolveConflicts_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).EndInit();
            this.ConflictedFilesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

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
        private System.Windows.Forms.DataGridView ConflictedFiles;
        private System.Windows.Forms.Button openMergeToolBtn;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.Button startMergetool;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button merge;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}