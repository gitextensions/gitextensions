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
            this.GitTree = new System.Windows.Forms.TreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.Commits = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.FileChanges = new System.Windows.Forms.DataGridView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.FileText = new ICSharpCode.TextEditor.TextEditorControl();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.Workingdir = new System.Windows.Forms.Label();
            this.CurrentBranch = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.branchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitBashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitGUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitcommandLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.guidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.Commits.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FileChanges)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // GitTree
            // 
            this.GitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitTree.Location = new System.Drawing.Point(0, 0);
            this.GitTree.Name = "GitTree";
            this.GitTree.Size = new System.Drawing.Size(208, 482);
            this.GitTree.TabIndex = 0;
            this.GitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTree_BeforeExpand);
            this.GitTree.DoubleClick += new System.EventHandler(this.GitTree_DoubleClick);
            this.GitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.GitTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(750, 482);
            this.splitContainer1.SplitterDistance = 208;
            this.splitContainer1.TabIndex = 1;
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
            this.splitContainer3.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer3.Size = new System.Drawing.Size(538, 482);
            this.splitContainer3.SplitterDistance = 171;
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
            this.tabControl2.Size = new System.Drawing.Size(538, 171);
            this.tabControl2.TabIndex = 1;
            // 
            // Commits
            // 
            this.Commits.Controls.Add(this.RevisionGrid);
            this.Commits.Location = new System.Drawing.Point(4, 22);
            this.Commits.Name = "Commits";
            this.Commits.Padding = new System.Windows.Forms.Padding(3);
            this.Commits.Size = new System.Drawing.Size(530, 145);
            this.Commits.TabIndex = 0;
            this.Commits.Text = "Commits";
            this.Commits.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.FileChanges);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(530, 145);
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
            this.FileChanges.Size = new System.Drawing.Size(524, 139);
            this.FileChanges.TabIndex = 0;
            this.FileChanges.SelectionChanged += new System.EventHandler(this.FileChanges_SelectionChanged);
            this.FileChanges.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.FileChanges_CellContentClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(538, 307);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.FileText);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(530, 281);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "View";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.IsReadOnly = false;
            this.FileText.Location = new System.Drawing.Point(3, 3);
            this.FileText.Name = "FileText";
            this.FileText.Size = new System.Drawing.Size(524, 275);
            this.FileText.TabIndex = 0;
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
            this.splitContainer2.Panel1.Controls.Add(this.Workingdir);
            this.splitContainer2.Panel1.Controls.Add(this.CurrentBranch);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(750, 519);
            this.splitContainer2.SplitterDistance = 33;
            this.splitContainer2.TabIndex = 2;
            // 
            // Workingdir
            // 
            this.Workingdir.AutoSize = true;
            this.Workingdir.Location = new System.Drawing.Point(198, 10);
            this.Workingdir.Name = "Workingdir";
            this.Workingdir.Size = new System.Drawing.Size(10, 13);
            this.Workingdir.TabIndex = 3;
            this.Workingdir.Text = " ";
            this.Workingdir.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CurrentBranch
            // 
            this.CurrentBranch.AutoSize = true;
            this.CurrentBranch.Location = new System.Drawing.Point(12, 10);
            this.CurrentBranch.Name = "CurrentBranch";
            this.CurrentBranch.Size = new System.Drawing.Size(0, 13);
            this.CurrentBranch.TabIndex = 2;
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
            this.checkoutToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.viewDiffToolStripMenuItem,
            this.addFilesToolStripMenuItem,
            this.branchToolStripMenuItem,
            this.cloneToolStripMenuItem,
            this.commitToolStripMenuItem,
            this.initNewRepositoryToolStripMenuItem,
            this.pushToolStripMenuItem,
            this.pullToolStripMenuItem,
            this.patchToolStripMenuItem,
            this.formatPatchToolStripMenuItem,
            this.applyPatchToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.checkoutToolStripMenuItem.Text = "Checkout revision";
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.checkoutToolStripMenuItem_Click);
            // 
            // viewDiffToolStripMenuItem
            // 
            this.viewDiffToolStripMenuItem.Name = "viewDiffToolStripMenuItem";
            this.viewDiffToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.viewDiffToolStripMenuItem.Text = "View diff";
            this.viewDiffToolStripMenuItem.Click += new System.EventHandler(this.viewDiffToolStripMenuItem_Click);
            // 
            // addFilesToolStripMenuItem
            // 
            this.addFilesToolStripMenuItem.Name = "addFilesToolStripMenuItem";
            this.addFilesToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addFilesToolStripMenuItem.Text = "AddFiles";
            this.addFilesToolStripMenuItem.Click += new System.EventHandler(this.addFilesToolStripMenuItem_Click);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.branchToolStripMenuItem.Text = "Branch";
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.branchToolStripMenuItem_Click);
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
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "Init new repository";
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.initNewRepositoryToolStripMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.pushToolStripMenuItem.Text = "Push";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.pushToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.pullToolStripMenuItem.Text = "Pull";
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.pullToolStripMenuItem_Click);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.patchToolStripMenuItem.Text = "View patch file";
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.patchToolStripMenuItem_Click);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.formatPatchToolStripMenuItem.Text = "Format patch";
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.formatPatchToolStripMenuItem_Click);
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.applyPatchToolStripMenuItem.Text = "Apply patch";
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.applyPatchToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
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
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.gitcommandLogToolStripMenuItem.Text = "Gitcommand log";
            this.gitcommandLogToolStripMenuItem.Click += new System.EventHandler(this.gitcommandLogToolStripMenuItem_Click);
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
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(3, 3);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.Size = new System.Drawing.Size(524, 139);
            this.RevisionGrid.TabIndex = 0;
            this.RevisionGrid.DoubleClick += new System.EventHandler(this.RevisionGrid_DoubleClick);
            this.RevisionGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RevisionGrid_MouseDoubleClick);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.checkoutBranchToolStripMenuItem_Click);
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
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.Commits.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FileChanges)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView GitTree;
        private System.Windows.Forms.SplitContainer splitContainer1;
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
    }
}