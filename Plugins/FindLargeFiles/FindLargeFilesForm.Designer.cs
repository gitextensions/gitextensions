namespace GitExtensions.Plugins.FindLargeFiles
{
    partial class FindLargeFilesForm
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
            if (disposing && (components is not null))
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindLargeFilesForm));
            BranchesGrid = new DataGridView();
            sHADataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            pathDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            sizeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            CompressedSize = new DataGridViewTextBoxColumn();
            commitCountDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            lastCommitDateDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
            gitObjectBindingSource = new BindingSource(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            Cancel = new Button();
            Delete = new Button();
            pbRevisions = new ProgressBar();
            label1 = new Label();
            branchBindingSource = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)(BranchesGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(gitObjectBindingSource)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(branchBindingSource)).BeginInit();
            SuspendLayout();
            // 
            // BranchesGrid
            // 
            BranchesGrid.AllowUserToAddRows = false;
            BranchesGrid.AllowUserToDeleteRows = false;
            BranchesGrid.AutoGenerateColumns = false;
            BranchesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            BranchesGrid.Columns.AddRange(new DataGridViewColumn[] {
            sHADataGridViewTextBoxColumn,
            pathDataGridViewTextBoxColumn,
            sizeDataGridViewTextBoxColumn,
            CompressedSize,
            commitCountDataGridViewTextBoxColumn,
            lastCommitDateDataGridViewTextBoxColumn,
            dataGridViewCheckBoxColumn1});
            BranchesGrid.DataSource = gitObjectBindingSource;
            BranchesGrid.Dock = DockStyle.Fill;
            BranchesGrid.Location = new Point(3, 28);
            BranchesGrid.Name = "BranchesGrid";
            BranchesGrid.ReadOnly = true;
            BranchesGrid.RowHeadersVisible = false;
            BranchesGrid.Size = new Size(754, 353);
            BranchesGrid.TabIndex = 0;
            // 
            // sHADataGridViewTextBoxColumn
            // 
            sHADataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            sHADataGridViewTextBoxColumn.HeaderText = "SHA";
            sHADataGridViewTextBoxColumn.Name = "sHADataGridViewTextBoxColumn";
            sHADataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // pathDataGridViewTextBoxColumn
            // 
            pathDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            pathDataGridViewTextBoxColumn.HeaderText = "Path";
            pathDataGridViewTextBoxColumn.Name = "pathDataGridViewTextBoxColumn";
            pathDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // sizeDataGridViewTextBoxColumn
            // 
            sizeDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            sizeDataGridViewTextBoxColumn.HeaderText = "Size";
            sizeDataGridViewTextBoxColumn.Name = "sizeDataGridViewTextBoxColumn";
            sizeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // CompressedSize
            // 
            CompressedSize.HeaderText = "Compressed size";
            CompressedSize.Name = "CompressedSize";
            CompressedSize.ReadOnly = true;
            // 
            // commitCountDataGridViewTextBoxColumn
            // 
            commitCountDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            commitCountDataGridViewTextBoxColumn.HeaderText = "Commit count";
            commitCountDataGridViewTextBoxColumn.Name = "commitCountDataGridViewTextBoxColumn";
            commitCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // lastCommitDateDataGridViewTextBoxColumn
            // 
            lastCommitDateDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            lastCommitDateDataGridViewTextBoxColumn.HeaderText = "Last commit date";
            lastCommitDateDataGridViewTextBoxColumn.Name = "lastCommitDateDataGridViewTextBoxColumn";
            lastCommitDateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            dataGridViewCheckBoxColumn1.HeaderText = "Delete";
            dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            dataGridViewCheckBoxColumn1.ReadOnly = true;
            // 
            // gitObjectBindingSource
            // 
            gitObjectBindingSource.DataSource = typeof(FindLargeFiles.GitObject);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(BranchesGrid, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tableLayoutPanel1.Size = new Size(760, 421);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tableLayoutPanel2.Controls.Add(Cancel, 2, 0);
            tableLayoutPanel2.Controls.Add(Delete, 1, 0);
            tableLayoutPanel2.Controls.Add(pbRevisions, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 387);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(754, 31);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Dock = DockStyle.Left;
            Cancel.Location = new Point(607, 3);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 25);
            Cancel.TabIndex = 0;
            Cancel.Text = "Close";
            Cancel.UseVisualStyleBackColor = true;
            // 
            // Delete
            // 
            Delete.Dock = DockStyle.Right;
            Delete.Location = new Point(526, 3);
            Delete.Name = "Delete";
            Delete.Size = new Size(75, 25);
            Delete.TabIndex = 1;
            Delete.Text = "Delete";
            Delete.UseVisualStyleBackColor = true;
            Delete.Click += Delete_Click;
            // 
            // pbRevisions
            // 
            pbRevisions.Dock = DockStyle.Fill;
            pbRevisions.Location = new Point(2, 2);
            pbRevisions.Margin = new Padding(2);
            pbRevisions.Name = "pbRevisions";
            pbRevisions.Size = new Size(450, 27);
            pbRevisions.TabIndex = 2;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 6);
            label1.Name = "label1";
            label1.Size = new Size(538, 13);
            label1.TabIndex = 1;
            label1.Text = "Reset local changes before deleting files. Choose files to delete. Force push for re" +
    "placing data on remote repository.";
            // 
            // branchBindingSource
            // 
            branchBindingSource.DataSource = typeof(FindLargeFiles.GitObject);
            // 
            // FindLargeFilesForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = Cancel;
            ClientSize = new Size(760, 421);
            Controls.Add(tableLayoutPanel1);
            Name = "FindLargeFilesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Find large files";
            ((System.ComponentModel.ISupportInitialize)(BranchesGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(gitObjectBindingSource)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(branchBindingSource)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private DataGridView BranchesGrid;
        private BindingSource branchBindingSource;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button Cancel;
        private Button Delete;
        private ProgressBar pbRevisions;
        private BindingSource gitObjectBindingSource;
        private DataGridViewTextBoxColumn sHADataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn pathDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn sizeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn CompressedSize;
        private DataGridViewTextBoxColumn commitCountDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn lastCommitDateDataGridViewTextBoxColumn;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
    }
}
