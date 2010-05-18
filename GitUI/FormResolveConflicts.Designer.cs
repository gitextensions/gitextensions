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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormResolveConflicts));
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
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Rescan = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.Namex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SolveConflictButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.guidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commitGuidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            resources.ApplyResources(this.modeDataGridViewTextBoxColumn, "modeDataGridViewTextBoxColumn");
            this.modeDataGridViewTextBoxColumn.Name = "modeDataGridViewTextBoxColumn";
            this.modeDataGridViewTextBoxColumn.ReadOnly = true;
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).BeginInit();
            this.ConflictedFilesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConflictedFiles
            // 
            this.ConflictedFiles.AllowUserToAddRows = false;
            this.ConflictedFiles.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.ConflictedFiles, "ConflictedFiles");
            this.ConflictedFiles.AutoGenerateColumns = false;
            this.ConflictedFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ConflictedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConflictedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Namex,
            this.SolveConflictButton});
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
            // ConflictedFilesContextMenu
            // 
            resources.ApplyResources(this.ConflictedFilesContextMenu, "ConflictedFilesContextMenu");
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
            this.ConflictedFilesContextMenu.Size = new System.Drawing.Size(196, 264);
            this.ConflictedFilesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ConflictedFilesContextMenu_Opening);
            // 
            // OpenMergetool
            // 
            resources.ApplyResources(this.OpenMergetool, "OpenMergetool");
            this.OpenMergetool.Name = "OpenMergetool";
            this.OpenMergetool.Size = new System.Drawing.Size(195, 22);
            this.OpenMergetool.Click += new System.EventHandler(this.OpenMergetool_Click);
            // 
            // ContextMarkAsSolved
            // 
            resources.ApplyResources(this.ContextMarkAsSolved, "ContextMarkAsSolved");
            this.ContextMarkAsSolved.Name = "ContextMarkAsSolved";
            this.ContextMarkAsSolved.Size = new System.Drawing.Size(195, 22);
            this.ContextMarkAsSolved.Click += new System.EventHandler(this.ContextMarkAsSolved_Click);
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(192, 6);
            // 
            // ContextChooseBase
            // 
            resources.ApplyResources(this.ContextChooseBase, "ContextChooseBase");
            this.ContextChooseBase.Name = "ContextChooseBase";
            this.ContextChooseBase.Size = new System.Drawing.Size(195, 22);
            this.ContextChooseBase.Click += new System.EventHandler(this.ContextChooseBase_Click);
            // 
            // ContextChooseLocal
            // 
            resources.ApplyResources(this.ContextChooseLocal, "ContextChooseLocal");
            this.ContextChooseLocal.Name = "ContextChooseLocal";
            this.ContextChooseLocal.Size = new System.Drawing.Size(195, 22);
            this.ContextChooseLocal.Click += new System.EventHandler(this.ContextChooseLocal_Click);
            // 
            // ContextChooseRemote
            // 
            resources.ApplyResources(this.ContextChooseRemote, "ContextChooseRemote");
            this.ContextChooseRemote.Name = "ContextChooseRemote";
            this.ContextChooseRemote.Size = new System.Drawing.Size(195, 22);
            this.ContextChooseRemote.Click += new System.EventHandler(this.ContextChooseRemote_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
            // 
            // ContextOpenBaseWith
            // 
            resources.ApplyResources(this.ContextOpenBaseWith, "ContextOpenBaseWith");
            this.ContextOpenBaseWith.Name = "ContextOpenBaseWith";
            this.ContextOpenBaseWith.Size = new System.Drawing.Size(195, 22);
            this.ContextOpenBaseWith.Click += new System.EventHandler(this.ContextOpenBaseWith_Click);
            // 
            // ContextOpenLocalWith
            // 
            resources.ApplyResources(this.ContextOpenLocalWith, "ContextOpenLocalWith");
            this.ContextOpenLocalWith.Name = "ContextOpenLocalWith";
            this.ContextOpenLocalWith.Size = new System.Drawing.Size(195, 22);
            this.ContextOpenLocalWith.Click += new System.EventHandler(this.ContextOpenLocalWith_Click);
            // 
            // ContextOpenRemoteWith
            // 
            resources.ApplyResources(this.ContextOpenRemoteWith, "ContextOpenRemoteWith");
            this.ContextOpenRemoteWith.Name = "ContextOpenRemoteWith";
            this.ContextOpenRemoteWith.Size = new System.Drawing.Size(195, 22);
            this.ContextOpenRemoteWith.Click += new System.EventHandler(this.ContextOpenRemoteWith_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(192, 6);
            // 
            // ContextSaveBaseAs
            // 
            resources.ApplyResources(this.ContextSaveBaseAs, "ContextSaveBaseAs");
            this.ContextSaveBaseAs.Name = "ContextSaveBaseAs";
            this.ContextSaveBaseAs.Size = new System.Drawing.Size(195, 22);
            this.ContextSaveBaseAs.Click += new System.EventHandler(this.ContextSaveBaseAs_Click);
            // 
            // ContextSaveLocalAs
            // 
            resources.ApplyResources(this.ContextSaveLocalAs, "ContextSaveLocalAs");
            this.ContextSaveLocalAs.Name = "ContextSaveLocalAs";
            this.ContextSaveLocalAs.Size = new System.Drawing.Size(195, 22);
            this.ContextSaveLocalAs.Click += new System.EventHandler(this.ContextSaveLocalAs_Click);
            // 
            // ContextSaveRemoteAs
            // 
            resources.ApplyResources(this.ContextSaveRemoteAs, "ContextSaveRemoteAs");
            this.ContextSaveRemoteAs.Name = "ContextSaveRemoteAs";
            this.ContextSaveRemoteAs.Size = new System.Drawing.Size(195, 22);
            this.ContextSaveRemoteAs.Click += new System.EventHandler(this.ContextSaveRemoteAs_Click);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(723, 396);
            this.splitContainer1.SplitterDistance = 587;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
            this.splitContainer2.Panel2.Controls.Add(this.ConflictedFiles);
            this.splitContainer2.Size = new System.Drawing.Size(587, 396);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 0;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            // 
            // splitContainer3
            // 
            resources.ApplyResources(this.splitContainer3, "splitContainer3");
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            resources.ApplyResources(this.splitContainer3.Panel1, "splitContainer3.Panel1");
            this.splitContainer3.Panel1.Controls.Add(this.button1);
            this.splitContainer3.Panel1.Controls.Add(this.Mergetool);
            // 
            // splitContainer3.Panel2
            // 
            resources.ApplyResources(this.splitContainer3.Panel2, "splitContainer3.Panel2");
            this.splitContainer3.Panel2.Controls.Add(this.Rescan);
            this.splitContainer3.Panel2.Controls.Add(this.Reset);
            this.splitContainer3.Size = new System.Drawing.Size(132, 396);
            this.splitContainer3.SplitterDistance = 329;
            this.splitContainer3.TabIndex = 7;
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 23);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Mergetool
            // 
            resources.ApplyResources(this.Mergetool, "Mergetool");
            this.Mergetool.Location = new System.Drawing.Point(3, 32);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(125, 23);
            this.Mergetool.TabIndex = 0;
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Rescan
            // 
            resources.ApplyResources(this.Rescan, "Rescan");
            this.Rescan.Location = new System.Drawing.Point(3, 3);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(125, 23);
            this.Rescan.TabIndex = 1;
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // Reset
            // 
            resources.ApplyResources(this.Reset, "Reset");
            this.Reset.Location = new System.Drawing.Point(3, 32);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(125, 23);
            this.Reset.TabIndex = 6;
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // Namex
            // 
            this.Namex.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Namex.DataPropertyName = "FileName";
            resources.ApplyResources(this.Namex, "Namex");
            this.Namex.Name = "Namex";
            this.Namex.ReadOnly = true;
            // 
            // SolveConflictButton
            // 
            this.SolveConflictButton.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.SolveConflictButton, "SolveConflictButton");
            this.SolveConflictButton.Name = "SolveConflictButton";
            this.SolveConflictButton.ReadOnly = true;
            this.SolveConflictButton.Text = "Solve";
            this.SolveConflictButton.UseColumnTextForButtonValue = true;
            this.SolveConflictButton.Width = 60;
            // 
            // guidDataGridViewTextBoxColumn
            // 
            this.guidDataGridViewTextBoxColumn.DataPropertyName = "Guid";
            resources.ApplyResources(this.guidDataGridViewTextBoxColumn, "guidDataGridViewTextBoxColumn");
            this.guidDataGridViewTextBoxColumn.Name = "guidDataGridViewTextBoxColumn";
            this.guidDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // commitGuidDataGridViewTextBoxColumn
            // 
            this.commitGuidDataGridViewTextBoxColumn.DataPropertyName = "CommitGuid";
            resources.ApplyResources(this.commitGuidDataGridViewTextBoxColumn, "commitGuidDataGridViewTextBoxColumn");
            this.commitGuidDataGridViewTextBoxColumn.Name = "commitGuidDataGridViewTextBoxColumn";
            this.commitGuidDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // itemTypeDataGridViewTextBoxColumn
            // 
            this.itemTypeDataGridViewTextBoxColumn.DataPropertyName = "ItemType";
            resources.ApplyResources(this.itemTypeDataGridViewTextBoxColumn, "itemTypeDataGridViewTextBoxColumn");
            this.itemTypeDataGridViewTextBoxColumn.Name = "itemTypeDataGridViewTextBoxColumn";
            this.itemTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            resources.ApplyResources(this.nameDataGridViewTextBoxColumn, "nameDataGridViewTextBoxColumn");
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn
            // 
            this.authorDataGridViewTextBoxColumn.DataPropertyName = "Author";
            resources.ApplyResources(this.authorDataGridViewTextBoxColumn, "authorDataGridViewTextBoxColumn");
            this.authorDataGridViewTextBoxColumn.Name = "authorDataGridViewTextBoxColumn";
            this.authorDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.DataPropertyName = "Date";
            resources.ApplyResources(this.dateDataGridViewTextBoxColumn, "dateDataGridViewTextBoxColumn");
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            this.dateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // fileNameDataGridViewTextBoxColumn
            // 
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            resources.ApplyResources(this.fileNameDataGridViewTextBoxColumn, "fileNameDataGridViewTextBoxColumn");
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // modeDataGridViewTextBoxColumn
            // 
            this.modeDataGridViewTextBoxColumn.DataPropertyName = "Mode";
            resources.ApplyResources(this.modeDataGridViewTextBoxColumn, "modeDataGridViewTextBoxColumn");
            this.modeDataGridViewTextBoxColumn.Name = "modeDataGridViewTextBoxColumn";
            this.modeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // FormResolveConflicts
            // 
            this.AcceptButton = this.button1;
            resources.ApplyResources(this, "$this");
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 396);
            this.Controls.Add(this.splitContainer1);
            //this.Icon = global::GitUI.Properties.Resources.cow_head;
            this.Name = "FormResolveConflicts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormResolveConflicts_FormClosing);
            this.Load += new System.EventHandler(this.FormResolveConflicts_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).EndInit();
            this.ConflictedFilesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
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
    }
}