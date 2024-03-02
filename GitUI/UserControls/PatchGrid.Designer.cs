namespace GitUI
{
    partial class PatchGrid
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Patches = new DataGridView();
            patchFileBindingSource = new BindingSource(components);
            Action = new DataGridViewTextBoxColumn();
            FileName = new DataGridViewTextBoxColumn();
            subjectDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            authorDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            dateDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            CommitHash = new DataGridViewTextBoxColumn();
            Status = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(Patches)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(patchFileBindingSource)).BeginInit();
            SuspendLayout();
            // 
            // Patches
            // 
            Patches.AllowUserToAddRows = false;
            Patches.AllowUserToDeleteRows = false;
            Patches.AutoGenerateColumns = false;
            Patches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Patches.BackgroundColor = SystemColors.ControlLight;
            Patches.BorderStyle = BorderStyle.None;
            Patches.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Patches.Columns.AddRange(new DataGridViewColumn[] {
            Status,
            Action,
            FileName,
            subjectDataGridViewTextBoxColumn,
            authorDataGridViewTextBoxColumn,
            dateDataGridViewTextBoxColumn,
            CommitHash});
            Patches.DataSource = patchFileBindingSource;
            Patches.Dock = DockStyle.Fill;
            Patches.Location = new Point(0, 0);
            Patches.Margin = new Padding(3, 2, 3, 2);
            Patches.Name = "Patches";
            Patches.ReadOnly = true;
            Patches.RowHeadersVisible = false;
            Patches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Patches.Size = new Size(675, 406);
            Patches.TabIndex = 0;
            Patches.DoubleClick += Patches_DoubleClick;
            // 
            // patchFileBindingSource
            // 
            patchFileBindingSource.DataSource = typeof(GitUI.PatchFile);
            // 
            // FileName
            // 
            FileName.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            FileName.HeaderText = "Name";
            FileName.Name = "FileName";
            FileName.ReadOnly = true;
            FileName.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // Action
            // 
            Action.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Action.HeaderText = "Action";
            Action.Name = "Action";
            Action.ReadOnly = true;
            Action.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // subjectDataGridViewTextBoxColumn
            // 
            subjectDataGridViewTextBoxColumn.HeaderText = "Subject";
            subjectDataGridViewTextBoxColumn.Name = "subjectDataGridViewTextBoxColumn";
            subjectDataGridViewTextBoxColumn.ReadOnly = true;
            subjectDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // authorDataGridViewTextBoxColumn
            // 
            authorDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            authorDataGridViewTextBoxColumn.HeaderText = "Author";
            authorDataGridViewTextBoxColumn.Name = "authorDataGridViewTextBoxColumn";
            authorDataGridViewTextBoxColumn.ReadOnly = true;
            authorDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            dateDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dateDataGridViewTextBoxColumn.HeaderText = "Date";
            dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            dateDataGridViewTextBoxColumn.ReadOnly = true;
            dateDataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // CommitHash
            // 
            CommitHash.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            CommitHash.HeaderText = "Commit hash";
            CommitHash.Name = "CommitHash";
            CommitHash.ReadOnly = true;
            CommitHash.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // Status
            // 
            Status.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Status.HeaderText = "Status";
            Status.Name = "Status";
            Status.ReadOnly = true;
            Status.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // PatchGrid
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(Patches);
            Margin = new Padding(3, 2, 3, 2);
            Name = "PatchGrid";
            Size = new Size(675, 406);
            ((System.ComponentModel.ISupportInitialize)(Patches)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(patchFileBindingSource)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private DataGridView Patches;
        private BindingSource patchFileBindingSource;
        private DataGridViewTextBoxColumn Action;
        private DataGridViewTextBoxColumn FileName;
        private DataGridViewTextBoxColumn subjectDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn CommitHash;
        private DataGridViewTextBoxColumn Status;
    }
}
