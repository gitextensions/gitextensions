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
            this.ConflictedFiles = new System.Windows.Forms.DataGridView();
            this.Namex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SolveConflictButton = new System.Windows.Forms.DataGridViewButtonColumn();
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
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Rescan = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.guidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commitGuidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.guidDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commitGuidDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemTypeDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modeDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).BeginInit();
            this.ConflictedFilesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(723, 396);
            this.splitContainer1.SplitterDistance = 587;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
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
            this.splitContainer2.Panel2.Controls.Add(this.ConflictedFiles);
            this.splitContainer2.Size = new System.Drawing.Size(587, 396);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Unresolved merge conflicts";
            // 
            // ConflictedFiles
            // 
            this.ConflictedFiles.AllowUserToAddRows = false;
            this.ConflictedFiles.AllowUserToDeleteRows = false;
            this.ConflictedFiles.AutoGenerateColumns = false;
            this.ConflictedFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ConflictedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConflictedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Namex,
            this.SolveConflictButton,
            this.guidDataGridViewTextBoxColumn1,
            this.commitGuidDataGridViewTextBoxColumn1,
            this.itemTypeDataGridViewTextBoxColumn1,
            this.nameDataGridViewTextBoxColumn1,
            this.authorDataGridViewTextBoxColumn1,
            this.dateDataGridViewTextBoxColumn1,
            this.fileNameDataGridViewTextBoxColumn1,
            this.modeDataGridViewTextBoxColumn1});
            this.ConflictedFiles.ContextMenuStrip = this.ConflictedFilesContextMenu;
            this.ConflictedFiles.DataSource = this.gitItemBindingSource;
            this.ConflictedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConflictedFiles.Location = new System.Drawing.Point(0, 0);
            this.ConflictedFiles.MultiSelect = false;
            this.ConflictedFiles.Name = "ConflictedFiles";
            this.ConflictedFiles.ReadOnly = true;
            this.ConflictedFiles.RowHeadersVisible = false;
            this.ConflictedFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ConflictedFiles.Size = new System.Drawing.Size(587, 367);
            this.ConflictedFiles.TabIndex = 0;
            this.ConflictedFiles.DoubleClick += new System.EventHandler(this.ConflictedFiles_DoubleClick);
            this.ConflictedFiles.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ConflictedFiles_CellMouseDown);
            this.ConflictedFiles.SelectionChanged += new System.EventHandler(this.ConflictedFiles_SelectionChanged);
            // 
            // Namex
            // 
            this.Namex.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Namex.DataPropertyName = "FileName";
            this.Namex.HeaderText = "Filename";
            this.Namex.Name = "Namex";
            this.Namex.ReadOnly = true;
            // 
            // SolveConflictButton
            // 
            this.SolveConflictButton.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.SolveConflictButton.HeaderText = "";
            this.SolveConflictButton.Name = "SolveConflictButton";
            this.SolveConflictButton.ReadOnly = true;
            this.SolveConflictButton.Text = "Solve";
            this.SolveConflictButton.UseColumnTextForButtonValue = true;
            this.SolveConflictButton.Width = 60;
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
            this.ContextSaveRemoteAs});
            this.ConflictedFilesContextMenu.Name = "ConflictedFilesContextMenu";
            this.ConflictedFilesContextMenu.Size = new System.Drawing.Size(219, 264);
            this.ConflictedFilesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ConflictedFilesContextMenu_Opening);
            // 
            // OpenMergetool
            // 
            this.OpenMergetool.Name = "OpenMergetool";
            this.OpenMergetool.Size = new System.Drawing.Size(218, 22);
            this.OpenMergetool.Text = "Open in mergetool";
            this.OpenMergetool.Click += new System.EventHandler(this.OpenMergetool_Click);
            // 
            // ContextMarkAsSolved
            // 
            this.ContextMarkAsSolved.Name = "ContextMarkAsSolved";
            this.ContextMarkAsSolved.Size = new System.Drawing.Size(218, 22);
            this.ContextMarkAsSolved.Text = "Mark conlfict as solved";
            this.ContextMarkAsSolved.Click += new System.EventHandler(this.ContextMarkAsSolved_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(215, 6);
            // 
            // ContextChooseBase
            // 
            this.ContextChooseBase.Name = "ContextChooseBase";
            this.ContextChooseBase.Size = new System.Drawing.Size(218, 22);
            this.ContextChooseBase.Text = "Choose base";
            this.ContextChooseBase.Click += new System.EventHandler(this.ContextChooseBase_Click);
            // 
            // ContextChooseLocal
            // 
            this.ContextChooseLocal.Name = "ContextChooseLocal";
            this.ContextChooseLocal.Size = new System.Drawing.Size(218, 22);
            this.ContextChooseLocal.Text = "Choose local";
            this.ContextChooseLocal.Click += new System.EventHandler(this.ContextChooseLocal_Click);
            // 
            // ContextChooseRemote
            // 
            this.ContextChooseRemote.Name = "ContextChooseRemote";
            this.ContextChooseRemote.Size = new System.Drawing.Size(218, 22);
            this.ContextChooseRemote.Text = "Choose remote";
            this.ContextChooseRemote.Click += new System.EventHandler(this.ContextChooseRemote_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(215, 6);
            // 
            // ContextOpenBaseWith
            // 
            this.ContextOpenBaseWith.Name = "ContextOpenBaseWith";
            this.ContextOpenBaseWith.Size = new System.Drawing.Size(218, 22);
            this.ContextOpenBaseWith.Text = "Open base with";
            this.ContextOpenBaseWith.Click += new System.EventHandler(this.ContextOpenBaseWith_Click);
            // 
            // ContextOpenLocalWith
            // 
            this.ContextOpenLocalWith.Name = "ContextOpenLocalWith";
            this.ContextOpenLocalWith.Size = new System.Drawing.Size(218, 22);
            this.ContextOpenLocalWith.Text = "Open local with";
            this.ContextOpenLocalWith.Click += new System.EventHandler(this.ContextOpenLocalWith_Click);
            // 
            // ContextOpenRemoteWith
            // 
            this.ContextOpenRemoteWith.Name = "ContextOpenRemoteWith";
            this.ContextOpenRemoteWith.Size = new System.Drawing.Size(218, 22);
            this.ContextOpenRemoteWith.Text = "Open remote with";
            this.ContextOpenRemoteWith.Click += new System.EventHandler(this.ContextOpenRemoteWith_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(215, 6);
            // 
            // ContextSaveBaseAs
            // 
            this.ContextSaveBaseAs.Name = "ContextSaveBaseAs";
            this.ContextSaveBaseAs.Size = new System.Drawing.Size(218, 22);
            this.ContextSaveBaseAs.Text = "Save base as";
            this.ContextSaveBaseAs.Click += new System.EventHandler(this.ContextSaveBaseAs_Click);
            // 
            // ContextSaveLocalAs
            // 
            this.ContextSaveLocalAs.Name = "ContextSaveLocalAs";
            this.ContextSaveLocalAs.Size = new System.Drawing.Size(218, 22);
            this.ContextSaveLocalAs.Text = "Save local as";
            this.ContextSaveLocalAs.Click += new System.EventHandler(this.ContextSaveLocalAs_Click);
            // 
            // ContextSaveRemoteAs
            // 
            this.ContextSaveRemoteAs.Name = "ContextSaveRemoteAs";
            this.ContextSaveRemoteAs.Size = new System.Drawing.Size(218, 22);
            this.ContextSaveRemoteAs.Text = "Save remote as";
            this.ContextSaveRemoteAs.Click += new System.EventHandler(this.ContextSaveRemoteAs_Click);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.button1);
            this.splitContainer3.Panel1.Controls.Add(this.Mergetool);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.Rescan);
            this.splitContainer3.Panel2.Controls.Add(this.Reset);
            this.splitContainer3.Size = new System.Drawing.Size(132, 396);
            this.splitContainer3.SplitterDistance = 329;
            this.splitContainer3.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Open in mergetool";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(3, 32);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(125, 23);
            this.Mergetool.TabIndex = 0;
            this.Mergetool.Text = "Run mergetool";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(3, 3);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(125, 23);
            this.Rescan.TabIndex = 1;
            this.Rescan.Text = "Rescan mergeconflicts";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(3, 32);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(125, 23);
            this.Reset.TabIndex = 6;
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
            // guidDataGridViewTextBoxColumn1
            // 
            this.guidDataGridViewTextBoxColumn1.DataPropertyName = "Guid";
            this.guidDataGridViewTextBoxColumn1.HeaderText = "Guid";
            this.guidDataGridViewTextBoxColumn1.Name = "guidDataGridViewTextBoxColumn1";
            this.guidDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // commitGuidDataGridViewTextBoxColumn1
            // 
            this.commitGuidDataGridViewTextBoxColumn1.DataPropertyName = "CommitGuid";
            this.commitGuidDataGridViewTextBoxColumn1.HeaderText = "CommitGuid";
            this.commitGuidDataGridViewTextBoxColumn1.Name = "commitGuidDataGridViewTextBoxColumn1";
            this.commitGuidDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // itemTypeDataGridViewTextBoxColumn1
            // 
            this.itemTypeDataGridViewTextBoxColumn1.DataPropertyName = "ItemType";
            this.itemTypeDataGridViewTextBoxColumn1.HeaderText = "ItemType";
            this.itemTypeDataGridViewTextBoxColumn1.Name = "itemTypeDataGridViewTextBoxColumn1";
            this.itemTypeDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn1
            // 
            this.nameDataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn1.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn1.Name = "nameDataGridViewTextBoxColumn1";
            this.nameDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn1
            // 
            this.authorDataGridViewTextBoxColumn1.DataPropertyName = "Author";
            this.authorDataGridViewTextBoxColumn1.HeaderText = "Author";
            this.authorDataGridViewTextBoxColumn1.Name = "authorDataGridViewTextBoxColumn1";
            this.authorDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn1
            // 
            this.dateDataGridViewTextBoxColumn1.DataPropertyName = "Date";
            this.dateDataGridViewTextBoxColumn1.HeaderText = "Date";
            this.dateDataGridViewTextBoxColumn1.Name = "dateDataGridViewTextBoxColumn1";
            this.dateDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // fileNameDataGridViewTextBoxColumn1
            // 
            this.fileNameDataGridViewTextBoxColumn1.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn1.HeaderText = "FileName";
            this.fileNameDataGridViewTextBoxColumn1.Name = "fileNameDataGridViewTextBoxColumn1";
            this.fileNameDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // modeDataGridViewTextBoxColumn1
            // 
            this.modeDataGridViewTextBoxColumn1.DataPropertyName = "Mode";
            this.modeDataGridViewTextBoxColumn1.HeaderText = "Mode";
            this.modeDataGridViewTextBoxColumn1.Name = "modeDataGridViewTextBoxColumn1";
            this.modeDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // FormResolveConflicts
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 396);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormResolveConflicts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resolve merge conflicts";
            this.Load += new System.EventHandler(this.FormResolveConflicts_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormResolveConflicts_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).EndInit();
            this.ConflictedFilesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView ConflictedFiles;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button Reset;
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
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Namex;
        private System.Windows.Forms.DataGridViewButtonColumn SolveConflictButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn guidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn commitGuidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn modeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn guidDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn commitGuidDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemTypeDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn modeDataGridViewTextBoxColumn1;
    }
}