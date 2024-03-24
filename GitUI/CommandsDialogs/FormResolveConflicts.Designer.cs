using GitCommands.Git;

namespace GitUI.CommandsDialogs
{
    partial class FormResolveConflicts
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
            label1 = new Label();
            ConflictedFiles = new DataGridView();
            FileName = new DataGridViewTextBoxColumn();
            authorDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            ConflictedFilesContextMenu = new ContextMenuStrip(components);
            OpenMergetool = new ToolStripMenuItem();
            customMergetool = new ToolStripMenuItem();
            ContextMarkAsSolved = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            ContextChooseLocal = new ToolStripMenuItem();
            ContextChooseRemote = new ToolStripMenuItem();
            ContextChooseBase = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            ContextOpenLocalWith = new ToolStripMenuItem();
            ContextOpenRemoteWith = new ToolStripMenuItem();
            ContextOpenBaseWith = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            ContextSaveLocalAs = new ToolStripMenuItem();
            ContextSaveRemoteAs = new ToolStripMenuItem();
            ContextSaveBaseAs = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            openToolStripMenuItem = new ToolStripMenuItem();
            openWithToolStripMenuItem = new ToolStripMenuItem();
            openFolderToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            fileHistoryToolStripMenuItem = new ToolStripMenuItem();
            gitItemBindingSource = new BindingSource(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            labelLocalCurrent = new Label();
            localFileName = new Label();
            baseFileName = new Label();
            labelBase = new Label();
            labelRemoteIncoming = new Label();
            remoteFileName = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            merge = new Button();
            conflictDescription = new Label();
            pictureBox1 = new PictureBox();
            startMergetool = new Button();
            openMergeToolBtn = new Button();
            Rescan = new Button();
            Reset = new Button();
            tableLayoutPanel4 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tableLayoutPanel5 = new TableLayoutPanel();
            gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            progressBar = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(ConflictedFiles)).BeginInit();
            toolTip = new ToolTip(components);
            ConflictedFilesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            tableLayoutPanel4.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(162, 16);
            label1.TabIndex = 0;
            label1.Text = "Unresolved merge conflicts";
            // 
            // ConflictedFiles
            // 
            ConflictedFiles.AllowUserToAddRows = false;
            ConflictedFiles.AllowUserToDeleteRows = false;
            ConflictedFiles.AllowUserToResizeColumns = false;
            ConflictedFiles.AllowUserToResizeRows = false;
            ConflictedFiles.AutoGenerateColumns = false;
            ConflictedFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ConflictedFiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ConflictedFiles.Columns.AddRange(new DataGridViewColumn[] {
            FileName,
            authorDataGridViewTextBoxColumn1});
            ConflictedFiles.ContextMenuStrip = ConflictedFilesContextMenu;
            ConflictedFiles.DataSource = gitItemBindingSource;
            ConflictedFiles.Dock = DockStyle.Fill;
            ConflictedFiles.Location = new Point(3, 19);
            ConflictedFiles.Name = "ConflictedFiles";
            ConflictedFiles.ReadOnly = true;
            ConflictedFiles.RowHeadersVisible = false;
            ConflictedFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ConflictedFiles.Size = new Size(455, 221);
            ConflictedFiles.TabIndex = 1;
            ConflictedFiles.CellMouseDown += ConflictedFiles_CellMouseDown;
            ConflictedFiles.SelectionChanged += ConflictedFiles_SelectionChanged;
            ConflictedFiles.DoubleClick += ConflictedFiles_DoubleClick;
            ConflictedFiles.KeyDown += ConflictedFiles_KeyDown;
            // 
            // FileName
            // 
            FileName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            FileName.HeaderText = "Filename";
            FileName.Name = "FileName";
            FileName.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn1
            // 
            authorDataGridViewTextBoxColumn1.HeaderText = "Author";
            authorDataGridViewTextBoxColumn1.Name = "authorDataGridViewTextBoxColumn1";
            authorDataGridViewTextBoxColumn1.ReadOnly = true;
            authorDataGridViewTextBoxColumn1.Visible = false;
            // 
            // ConflictedFilesContextMenu
            // 
            ConflictedFilesContextMenu.Items.AddRange(new ToolStripItem[] {
            OpenMergetool,
            customMergetool,
            ContextMarkAsSolved,
            toolStripSeparator3,
            ContextChooseLocal,
            ContextChooseRemote,
            ContextChooseBase,
            toolStripSeparator1,
            ContextOpenLocalWith,
            ContextOpenRemoteWith,
            ContextOpenBaseWith,
            toolStripSeparator2,
            ContextSaveLocalAs,
            ContextSaveRemoteAs,
            ContextSaveBaseAs,
            toolStripMenuItem1,
            openToolStripMenuItem,
            openWithToolStripMenuItem,
            openFolderToolStripMenuItem,
            toolStripSeparator4,
            fileHistoryToolStripMenuItem});
            ConflictedFilesContextMenu.Name = "ConflictedFilesContextMenu";
            ConflictedFilesContextMenu.Size = new Size(196, 342);
            ConflictedFilesContextMenu.Opening += ConflictedFilesContextMenu_Opening;
            // 
            // OpenMergetool
            // 
            OpenMergetool.Name = "OpenMergetool";
            OpenMergetool.Size = new Size(195, 22);
            OpenMergetool.Text = "Open in mergetool";
            OpenMergetool.Click += OpenMergetool_Click;
            // 
            // customMergetool
            // 
            customMergetool.Name = "customMergetool";
            customMergetool.Size = new Size(195, 22);
            customMergetool.Text = "Open in &mergetool";
            customMergetool.Click += customMergetool_Click;
            // 
            // ContextMarkAsSolved
            // 
            ContextMarkAsSolved.Name = "ContextMarkAsSolved";
            ContextMarkAsSolved.Size = new Size(195, 22);
            ContextMarkAsSolved.Text = "Mark conflict as solved";
            ContextMarkAsSolved.Click += ContextMarkAsSolved_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(192, 6);
            // 
            // ContextChooseLocal
            // 
            ContextChooseLocal.Name = "ContextChooseLocal";
            ContextChooseLocal.ShortcutKeys = ((Keys)((Keys.Control | Keys.D1)));
            ContextChooseLocal.Size = new Size(195, 22);
            ContextChooseLocal.Text = "Choose local";
            ContextChooseLocal.Click += ContextChooseLocal_Click;
            // 
            // ContextChooseRemote
            // 
            ContextChooseRemote.Name = "ContextChooseRemote";
            ContextChooseRemote.ShortcutKeys = ((Keys)((Keys.Control | Keys.D2)));
            ContextChooseRemote.Size = new Size(195, 22);
            ContextChooseRemote.Text = "Choose remote";
            ContextChooseRemote.Click += ContextChooseRemote_Click;
            // 
            // ContextChooseBase
            // 
            ContextChooseBase.Name = "ContextChooseBase";
            ContextChooseBase.ShortcutKeys = ((Keys)((Keys.Control | Keys.D3)));
            ContextChooseBase.Size = new Size(195, 22);
            ContextChooseBase.Text = "Choose base";
            ContextChooseBase.Click += ContextChooseBase_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(192, 6);
            // 
            // ContextOpenLocalWith
            // 
            ContextOpenLocalWith.Name = "ContextOpenLocalWith";
            ContextOpenLocalWith.Size = new Size(195, 22);
            ContextOpenLocalWith.Text = "Open local with";
            ContextOpenLocalWith.Click += ContextOpenLocalWith_Click;
            // 
            // ContextOpenRemoteWith
            // 
            ContextOpenRemoteWith.Name = "ContextOpenRemoteWith";
            ContextOpenRemoteWith.Size = new Size(195, 22);
            ContextOpenRemoteWith.Text = "Open remote with";
            ContextOpenRemoteWith.Click += ContextOpenRemoteWith_Click;
            // 
            // ContextOpenBaseWith
            // 
            ContextOpenBaseWith.Name = "ContextOpenBaseWith";
            ContextOpenBaseWith.Size = new Size(195, 22);
            ContextOpenBaseWith.Text = "Open base with";
            ContextOpenBaseWith.Click += ContextOpenBaseWith_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(192, 6);
            // 
            // ContextSaveLocalAs
            // 
            ContextSaveLocalAs.Name = "ContextSaveLocalAs";
            ContextSaveLocalAs.Size = new Size(195, 22);
            ContextSaveLocalAs.Text = "Save local as";
            ContextSaveLocalAs.Click += ContextSaveLocalAs_Click;
            // 
            // ContextSaveRemoteAs
            // 
            ContextSaveRemoteAs.Name = "ContextSaveRemoteAs";
            ContextSaveRemoteAs.Size = new Size(195, 22);
            ContextSaveRemoteAs.Text = "Save remote as";
            ContextSaveRemoteAs.Click += ContextSaveRemoteAs_Click;
            // 
            // ContextSaveBaseAs
            // 
            ContextSaveBaseAs.Name = "ContextSaveBaseAs";
            ContextSaveBaseAs.Size = new Size(195, 22);
            ContextSaveBaseAs.Text = "Save base as";
            ContextSaveBaseAs.Click += ContextSaveBaseAs_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(192, 6);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(195, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openWithToolStripMenuItem
            // 
            openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            openWithToolStripMenuItem.Size = new Size(195, 22);
            openWithToolStripMenuItem.Text = "Open With";
            openWithToolStripMenuItem.Click += openWithToolStripMenuItem_Click;
            // 
            // openFolderToolStripMenuItem
            // 
            openFolderToolStripMenuItem.Image = Properties.Images.BrowseFileExplorer;
            openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            openFolderToolStripMenuItem.Size = new Size(195, 22);
            openFolderToolStripMenuItem.Text = "Show in folder";
            openFolderToolStripMenuItem.Click += openFolderToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(192, 6);
            // 
            // fileHistoryToolStripMenuItem
            // 
            fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            fileHistoryToolStripMenuItem.Size = new Size(195, 22);
            fileHistoryToolStripMenuItem.Text = "File history";
            fileHistoryToolStripMenuItem.Click += fileHistoryToolStripMenuItem_Click;
            // 
            // gitItemBindingSource
            // 
            gitItemBindingSource.DataSource = typeof(GitItem);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel4.SetColumnSpan(tableLayoutPanel1, 2);
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(labelLocalCurrent, 0, 0);
            tableLayoutPanel1.Controls.Add(localFileName, 1, 0);
            tableLayoutPanel1.Controls.Add(baseFileName, 1, 1);
            tableLayoutPanel1.Controls.Add(labelBase, 0, 1);
            tableLayoutPanel1.Controls.Add(labelRemoteIncoming, 0, 2);
            tableLayoutPanel1.Controls.Add(remoteFileName, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 296);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(467, 66);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // labelLocalCurrent
            // 
            labelLocalCurrent.Anchor = AnchorStyles.Left;
            labelLocalCurrent.AutoSize = true;
            labelLocalCurrent.Location = new Point(3, 0);
            labelLocalCurrent.Name = "labelLocalCurrent";
            labelLocalCurrent.Padding = new Padding(0, 3, 0, 3);
            labelLocalCurrent.Size = new Size(80, 21);
            labelLocalCurrent.TabIndex = 1;
            labelLocalCurrent.Text = "Local/current";
            // 
            // localFileName
            // 
            localFileName.Anchor = AnchorStyles.Left;
            localFileName.AutoSize = true;
            localFileName.Location = new Point(61, 3);
            localFileName.Name = "localFileName";
            localFileName.Size = new Size(20, 16);
            localFileName.TabIndex = 3;
            localFileName.Text = "...";
            // 
            // baseFileName
            // 
            baseFileName.Anchor = AnchorStyles.Left;
            baseFileName.AutoSize = true;
            baseFileName.Location = new Point(61, 25);
            baseFileName.Name = "baseFileName";
            baseFileName.Size = new Size(20, 16);
            baseFileName.TabIndex = 4;
            baseFileName.Text = "...";
            // 
            // labelBase
            // 
            labelBase.Anchor = AnchorStyles.Left;
            labelBase.AutoSize = true;
            labelBase.Location = new Point(3, 22);
            labelBase.Name = "labelBase";
            labelBase.Padding = new Padding(0, 3, 0, 3);
            labelBase.Size = new Size(35, 22);
            labelBase.TabIndex = 2;
            labelBase.Text = "Base";
            // 
            // labelRemoteIncoming
            // 
            labelRemoteIncoming.Anchor = AnchorStyles.Left;
            labelRemoteIncoming.AutoSize = true;
            labelRemoteIncoming.Location = new Point(3, 44);
            labelRemoteIncoming.Name = "labelRemoteIncoming";
            labelRemoteIncoming.Padding = new Padding(0, 3, 0, 3);
            labelRemoteIncoming.Size = new Size(104, 21);
            labelRemoteIncoming.TabIndex = 5;
            labelRemoteIncoming.Text = "Remote/incoming";
            // 
            // remoteFileName
            // 
            remoteFileName.Anchor = AnchorStyles.Left;
            remoteFileName.AutoSize = true;
            remoteFileName.Location = new Point(61, 47);
            remoteFileName.Name = "remoteFileName";
            remoteFileName.Size = new Size(20, 16);
            remoteFileName.TabIndex = 6;
            remoteFileName.Text = "...";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(merge, 2, 0);
            tableLayoutPanel3.Controls.Add(conflictDescription, 1, 0);
            tableLayoutPanel3.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 243);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(461, 27);
            tableLayoutPanel3.TabIndex = 6;
            // 
            // merge
            // 
            merge.ContextMenuStrip = ConflictedFilesContextMenu;
            merge.Image = Properties.Images.Select;
            merge.ImageAlign = ContentAlignment.MiddleLeft;
            merge.Location = new Point(331, 0);
            merge.Margin = new Padding(0, 0, 0, 2);
            merge.Name = "merge";
            merge.Size = new Size(130, 25);
            merge.TabIndex = 2;
            merge.TabStop = false;
            merge.Text = "Merge";
            merge.UseVisualStyleBackColor = true;
            merge.Click += merge_Click;
            // 
            // conflictDescription
            // 
            conflictDescription.Anchor = AnchorStyles.Left;
            conflictDescription.AutoSize = true;
            conflictDescription.Cursor = Cursors.Arrow;
            conflictDescription.Location = new Point(19, 5);
            conflictDescription.Name = "conflictDescription";
            conflictDescription.Size = new Size(64, 16);
            conflictDescription.TabIndex = 2;
            conflictDescription.Text = "Select file";
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Left;
            pictureBox1.BackgroundImage = Properties.Images.Information;
            pictureBox1.BackgroundImageLayout = ImageLayout.None;
            pictureBox1.Location = new Point(0, 5);
            pictureBox1.Margin = new Padding(0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(16, 16);
            pictureBox1.TabIndex = 9;
            pictureBox1.TabStop = false;
            // 
            // startMergetool
            // 
            startMergetool.Location = new Point(3, 34);
            startMergetool.Name = "startMergetool";
            startMergetool.Size = new Size(140, 25);
            startMergetool.TabIndex = 4;
            startMergetool.Text = "Start mergetool";
            startMergetool.UseVisualStyleBackColor = true;
            startMergetool.Click += Mergetool_Click;
            // 
            // openMergeToolBtn
            // 
            openMergeToolBtn.Location = new Point(3, 3);
            openMergeToolBtn.Name = "openMergeToolBtn";
            openMergeToolBtn.Size = new Size(140, 25);
            openMergeToolBtn.TabIndex = 3;
            openMergeToolBtn.Text = "Open in mergetool";
            openMergeToolBtn.UseVisualStyleBackColor = true;
            openMergeToolBtn.Click += OpenMergetool_Click;
            // 
            // Rescan
            // 
            Rescan.Location = new Point(3, 65);
            Rescan.Name = "Rescan";
            Rescan.Size = new Size(140, 25);
            Rescan.TabIndex = 5;
            Rescan.Text = "Rescan merge conflicts";
            Rescan.UseVisualStyleBackColor = true;
            Rescan.Click += Rescan_Click;
            // 
            // Reset
            // 
            Reset.Location = new Point(3, 96);
            Reset.Name = "Reset";
            Reset.Size = new Size(140, 25);
            Reset.TabIndex = 6;
            Reset.Text = "&Reset";
            Reset.UseVisualStyleBackColor = true;
            Reset.Click += Reset_Click;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.Controls.Add(tableLayoutPanel1, 0, 1);
            tableLayoutPanel4.Controls.Add(flowLayoutPanel1, 1, 0);
            tableLayoutPanel4.Controls.Add(tableLayoutPanel5, 0, 0);
            tableLayoutPanel4.Controls.Add(gotoUserManualControl1, 0, 2);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(0, 0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 3;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel4.Size = new Size(619, 392);
            tableLayoutPanel4.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(openMergeToolBtn);
            flowLayoutPanel1.Controls.Add(startMergetool);
            flowLayoutPanel1.Controls.Add(Rescan);
            flowLayoutPanel1.Controls.Add(Reset);
            flowLayoutPanel1.Controls.Add(progressBar);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(470, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(146, 290);
            flowLayoutPanel1.TabIndex = 0;
            flowLayoutPanel1.WrapContents = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(3, 127);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(140, 24);
            progressBar.TabIndex = 7;
            progressBar.Visible = false;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(tableLayoutPanel3, 0, 2);
            tableLayoutPanel5.Controls.Add(ConflictedFiles, 0, 1);
            tableLayoutPanel5.Controls.Add(label1, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 4;
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel5.Size = new Size(461, 290);
            tableLayoutPanel5.TabIndex = 1;
            // 
            // gotoUserManualControl1
            // 
            gotoUserManualControl1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            gotoUserManualControl1.AutoSize = true;
            gotoUserManualControl1.Location = new Point(3, 369);
            gotoUserManualControl1.ManualSectionAnchorName = "handle-merge-conflicts";
            gotoUserManualControl1.ManualSectionSubfolder = "modify_history";
            gotoUserManualControl1.MinimumSize = new Size(70, 20);
            gotoUserManualControl1.Name = "gotoUserManualControl1";
            gotoUserManualControl1.Size = new Size(70, 20);
            gotoUserManualControl1.TabIndex = 2;
            // 
            // FormResolveConflicts
            // 
            AcceptButton = merge;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(619, 392);
            Controls.Add(tableLayoutPanel4);
            Margin = new Padding(4);
            MinimumSize = new Size(450, 299);
            Name = "FormResolveConflicts";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Resolve merge conflicts";
            Load += FormResolveConflicts_Load;
            ((System.ComponentModel.ISupportInitialize)(ConflictedFiles)).EndInit();
            ConflictedFilesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private BindingSource gitItemBindingSource;
        private ContextMenuStrip ConflictedFilesContextMenu;
        private ToolStripMenuItem ContextChooseBase;
        private ToolStripMenuItem ContextChooseLocal;
        private ToolStripMenuItem ContextChooseRemote;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem ContextOpenBaseWith;
        private ToolStripMenuItem ContextOpenLocalWith;
        private ToolStripMenuItem ContextOpenRemoteWith;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem OpenMergetool;
        private ToolStripMenuItem customMergetool;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem ContextSaveBaseAs;
        private ToolStripMenuItem ContextSaveLocalAs;
        private ToolStripMenuItem ContextSaveRemoteAs;
        private ToolStripMenuItem ContextMarkAsSolved;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem openWithToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem openFolderToolStripMenuItem;
        private Label labelLocalCurrent;
        private TableLayoutPanel tableLayoutPanel1;
        private Label labelBase;
        private Label localFileName;
        private Label baseFileName;
        private Label labelRemoteIncoming;
        private Label remoteFileName;
        private Label conflictDescription;
        private DataGridView ConflictedFiles;
        private Button openMergeToolBtn;
        private Button Rescan;
        private Button Reset;
        private Button startMergetool;
        private TableLayoutPanel tableLayoutPanel3;
        private Button merge;
        private PictureBox pictureBox1;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem fileHistoryToolStripMenuItem;
        private DataGridViewTextBoxColumn FileName;
        private DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn1;
        private TableLayoutPanel tableLayoutPanel4;
        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel5;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private ProgressBar progressBar;
        private ToolTip toolTip;
    }
}
