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
            if (disposing && (components != null))
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
            this.components = new System.ComponentModel.Container();
            this.Patches = new System.Windows.Forms.DataGridView();
            this.patchFileBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.subjectDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.Patches)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.patchFileBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // Patches
            // 
            this.Patches.AllowUserToAddRows = false;
            this.Patches.AllowUserToDeleteRows = false;
            this.Patches.AutoGenerateColumns = false;
            this.Patches.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Patches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Patches.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileName,
            this.subjectDataGridViewTextBoxColumn,
            this.authorDataGridViewTextBoxColumn,
            this.dateDataGridViewTextBoxColumn,
            this.Status});
            this.Patches.DataSource = this.patchFileBindingSource;
            this.Patches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Patches.Location = new System.Drawing.Point(0, 0);
            this.Patches.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Patches.Name = "Patches";
            this.Patches.ReadOnly = true;
            this.Patches.RowHeadersVisible = false;
            this.Patches.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Patches.Size = new System.Drawing.Size(675, 406);
            this.Patches.TabIndex = 0;
            this.Patches.DoubleClick += new System.EventHandler(this.Patches_DoubleClick);
            // 
            // patchFileBindingSource
            // 
            this.patchFileBindingSource.DataSource = typeof(GitCommands.Patches.PatchFile);
            // 
            // FileName
            // 
            this.FileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.FileName.HeaderText = "Name";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            // 
            // subjectDataGridViewTextBoxColumn
            // 
            this.subjectDataGridViewTextBoxColumn.HeaderText = "Subject";
            this.subjectDataGridViewTextBoxColumn.Name = "subjectDataGridViewTextBoxColumn";
            this.subjectDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn
            // 
            this.authorDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.authorDataGridViewTextBoxColumn.HeaderText = "Author";
            this.authorDataGridViewTextBoxColumn.Name = "authorDataGridViewTextBoxColumn";
            this.authorDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dateDataGridViewTextBoxColumn.HeaderText = "Date";
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            this.dateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Status
            // 
            this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            // 
            // PatchGrid
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            
            this.Controls.Add(this.Patches);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PatchGrid";
            this.Size = new System.Drawing.Size(675, 406);
            ((System.ComponentModel.ISupportInitialize)(this.Patches)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.patchFileBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView Patches;
        private System.Windows.Forms.BindingSource patchFileBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn subjectDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
    }
}
