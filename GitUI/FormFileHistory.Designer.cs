namespace GitUI
{
    partial class FormFileHistory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFileHistory));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.FileChanges = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gitItemBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ViewTab = new System.Windows.Forms.TabPage();
            this.View = new GitUI.FileViewer();
            this.DiffTab = new System.Windows.Forms.TabPage();
            this.Blame = new System.Windows.Forms.TabPage();
            this.BlameGrid = new System.Windows.Forms.DataGridView();
            this.authorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gitBlameBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.subItemsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.Diff = new GitUI.FileViewer();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FileChanges)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.DiffTab.SuspendLayout();
            this.Blame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BlameGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitBlameBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subItemsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.FileChanges);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(750, 446);
            this.splitContainer1.SplitterDistance = 112;
            this.splitContainer1.TabIndex = 0;
            // 
            // FileChanges
            // 
            this.FileChanges.AllowUserToAddRows = false;
            this.FileChanges.AllowUserToDeleteRows = false;
            this.FileChanges.AutoGenerateColumns = false;
            this.FileChanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FileChanges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.Author,
            this.Date});
            this.FileChanges.DataSource = this.gitItemBindingSource1;
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Location = new System.Drawing.Point(0, 0);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.ReadOnly = true;
            this.FileChanges.RowHeadersVisible = false;
            this.FileChanges.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.FileChanges.Size = new System.Drawing.Size(750, 112);
            this.FileChanges.TabIndex = 1;
            this.FileChanges.DoubleClick += new System.EventHandler(this.FileChanges_DoubleClick);
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
            // Author
            // 
            this.Author.DataPropertyName = "Author";
            this.Author.HeaderText = "Author";
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.Width = 150;
            // 
            // Date
            // 
            this.Date.DataPropertyName = "Date";
            this.Date.HeaderText = "Date";
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.Width = 180;
            // 
            // gitItemBindingSource1
            // 
            this.gitItemBindingSource1.DataSource = typeof(GitCommands.GitItem);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.ViewTab);
            this.tabControl1.Controls.Add(this.DiffTab);
            this.tabControl1.Controls.Add(this.Blame);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(750, 330);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // ViewTab
            // 
            this.ViewTab.Controls.Add(this.View);
            this.ViewTab.Location = new System.Drawing.Point(4, 22);
            this.ViewTab.Name = "ViewTab";
            this.ViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.ViewTab.Size = new System.Drawing.Size(742, 304);
            this.ViewTab.TabIndex = 0;
            this.ViewTab.Text = "View";
            this.ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Location = new System.Drawing.Point(3, 3);
            this.View.Name = "View";
            this.View.ScrollPos = 0;
            this.View.Size = new System.Drawing.Size(736, 298);
            this.View.TabIndex = 0;
            // 
            // DiffTab
            // 
            this.DiffTab.Controls.Add(this.Diff);
            this.DiffTab.Location = new System.Drawing.Point(4, 22);
            this.DiffTab.Name = "DiffTab";
            this.DiffTab.Padding = new System.Windows.Forms.Padding(3);
            this.DiffTab.Size = new System.Drawing.Size(742, 304);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Blame
            // 
            this.Blame.Controls.Add(this.BlameGrid);
            this.Blame.Location = new System.Drawing.Point(4, 22);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(742, 304);
            this.Blame.TabIndex = 2;
            this.Blame.Text = "Blame";
            this.Blame.UseVisualStyleBackColor = true;
            this.Blame.Click += new System.EventHandler(this.Blame_Click);
            // 
            // BlameGrid
            // 
            this.BlameGrid.AllowUserToAddRows = false;
            this.BlameGrid.AllowUserToDeleteRows = false;
            this.BlameGrid.AutoGenerateColumns = false;
            this.BlameGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.BlameGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.BlameGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.BlameGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.BlameGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BlameGrid.ColumnHeadersVisible = false;
            this.BlameGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.authorDataGridViewTextBoxColumn,
            this.TextColumn});
            this.BlameGrid.DataSource = this.gitBlameBindingSource;
            this.BlameGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameGrid.GridColor = System.Drawing.SystemColors.Window;
            this.BlameGrid.Location = new System.Drawing.Point(0, 0);
            this.BlameGrid.MultiSelect = false;
            this.BlameGrid.Name = "BlameGrid";
            this.BlameGrid.ReadOnly = true;
            this.BlameGrid.RowHeadersVisible = false;
            this.BlameGrid.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.BlameGrid.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.BlameGrid.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.BlameGrid.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.BlameGrid.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.BlameGrid.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.BlameGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.BlameGrid.Size = new System.Drawing.Size(742, 304);
            this.BlameGrid.TabIndex = 0;
            this.BlameGrid.DoubleClick += new System.EventHandler(this.BlameGrid_DoubleClick);
            this.BlameGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.BlameGrid_CellPainting);
            // 
            // authorDataGridViewTextBoxColumn
            // 
            this.authorDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.authorDataGridViewTextBoxColumn.DataPropertyName = "Author";
            this.authorDataGridViewTextBoxColumn.FillWeight = 193.0556F;
            this.authorDataGridViewTextBoxColumn.HeaderText = "Author";
            this.authorDataGridViewTextBoxColumn.Name = "authorDataGridViewTextBoxColumn";
            this.authorDataGridViewTextBoxColumn.ReadOnly = true;
            this.authorDataGridViewTextBoxColumn.Width = 250;
            // 
            // TextColumn
            // 
            this.TextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TextColumn.DataPropertyName = "Text";
            this.TextColumn.HeaderText = "Text";
            this.TextColumn.Name = "TextColumn";
            this.TextColumn.ReadOnly = true;
            // 
            // gitBlameBindingSource
            // 
            this.gitBlameBindingSource.DataSource = typeof(GitCommands.GitBlame);
            // 
            // subItemsBindingSource
            // 
            this.subItemsBindingSource.DataMember = "SubItems";
            this.subItemsBindingSource.DataSource = this.gitItemBindingSource1;
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.Location = new System.Drawing.Point(3, 3);
            this.Diff.Name = "Diff";
            this.Diff.ScrollPos = 0;
            this.Diff.Size = new System.Drawing.Size(736, 298);
            this.Diff.TabIndex = 0;
            // 
            // FormFileHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 446);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormFileHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File History";
            this.Load += new System.EventHandler(this.FormFileHistory_Load);
            this.Shown += new System.EventHandler(this.FormFileHistory_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FileChanges)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.DiffTab.ResumeLayout(false);
            this.Blame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BlameGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitBlameBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subItemsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView FileChanges;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ViewTab;
        private System.Windows.Forms.TabPage DiffTab;
        private System.Windows.Forms.TabPage Blame;
        private System.Windows.Forms.BindingSource gitItemBindingSource1;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.BindingSource subItemsBindingSource;
        private System.Windows.Forms.DataGridView BlameGrid;
        private System.Windows.Forms.BindingSource gitBlameBindingSource;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TextColumn;
        private FileViewer View;
        private FileViewer Diff;
    }
}