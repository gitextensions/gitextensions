namespace PatchApply
{
    partial class ViewPatch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewPatch));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.GridChangedFiles = new System.Windows.Forms.DataGridView();
            this.FileNameA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.patchBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ChangesList = new GitUI.FileViewer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.BrowsePatch = new System.Windows.Forms.Button();
            this.PatchFileNameEdit = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.changedFileBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridChangedFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.patchBindingSource)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.changedFileBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.AccessibleDescription = null;
            this.splitContainer1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BackgroundImage = null;
            this.splitContainer1.Font = null;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AccessibleDescription = null;
            this.splitContainer1.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.BackgroundImage = null;
            this.splitContainer1.Panel1.Controls.Add(this.GridChangedFiles);
            this.splitContainer1.Panel1.Font = null;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AccessibleDescription = null;
            this.splitContainer1.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.BackgroundImage = null;
            this.splitContainer1.Panel2.Controls.Add(this.ChangesList);
            this.splitContainer1.Panel2.Font = null;
            // 
            // GridChangedFiles
            // 
            this.GridChangedFiles.AccessibleDescription = null;
            this.GridChangedFiles.AccessibleName = null;
            this.GridChangedFiles.AllowUserToAddRows = false;
            this.GridChangedFiles.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.GridChangedFiles, "GridChangedFiles");
            this.GridChangedFiles.AutoGenerateColumns = false;
            this.GridChangedFiles.BackgroundImage = null;
            this.GridChangedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridChangedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileNameA,
            this.typeDataGridViewTextBoxColumn,
            this.File});
            this.GridChangedFiles.DataSource = this.patchBindingSource;
            this.GridChangedFiles.Font = null;
            this.GridChangedFiles.MultiSelect = false;
            this.GridChangedFiles.Name = "GridChangedFiles";
            this.GridChangedFiles.ReadOnly = true;
            this.GridChangedFiles.RowHeadersVisible = false;
            this.GridChangedFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.GridChangedFiles.SelectionChanged += new System.EventHandler(this.GridChangedFiles_SelectionChanged);
            // 
            // FileNameA
            // 
            this.FileNameA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FileNameA.DataPropertyName = "FileNameA";
            resources.ApplyResources(this.FileNameA, "FileNameA");
            this.FileNameA.Name = "FileNameA";
            this.FileNameA.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            resources.ApplyResources(this.typeDataGridViewTextBoxColumn, "typeDataGridViewTextBoxColumn");
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            this.typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // File
            // 
            this.File.DataPropertyName = "File";
            resources.ApplyResources(this.File, "File");
            this.File.Name = "File";
            this.File.ReadOnly = true;
            // 
            // patchBindingSource
            // 
            this.patchBindingSource.DataSource = typeof(PatchApply.Patch);
            // 
            // ChangesList
            // 
            this.ChangesList.AccessibleDescription = null;
            this.ChangesList.AccessibleName = null;
            resources.ApplyResources(this.ChangesList, "ChangesList");
            this.ChangesList.BackgroundImage = null;
            this.ChangesList.Font = null;
            this.ChangesList.IgnoreWhitespaceChanges = false;
            this.ChangesList.Name = "ChangesList";
            this.ChangesList.NumberOfVisibleLines = 3;
            this.ChangesList.ScrollPos = 0;
            this.ChangesList.ShowEntireFile = false;
            this.ChangesList.TreatAllFilesAsText = false;
            // 
            // splitContainer4
            // 
            this.splitContainer4.AccessibleDescription = null;
            this.splitContainer4.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4, "splitContainer4");
            this.splitContainer4.BackgroundImage = null;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Font = null;
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.AccessibleDescription = null;
            this.splitContainer4.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4.Panel1, "splitContainer4.Panel1");
            this.splitContainer4.Panel1.BackgroundImage = null;
            this.splitContainer4.Panel1.Controls.Add(this.BrowsePatch);
            this.splitContainer4.Panel1.Controls.Add(this.PatchFileNameEdit);
            this.splitContainer4.Panel1.Controls.Add(this.label1);
            this.splitContainer4.Panel1.Font = null;
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.AccessibleDescription = null;
            this.splitContainer4.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4.Panel2, "splitContainer4.Panel2");
            this.splitContainer4.Panel2.BackgroundImage = null;
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer4.Panel2.Font = null;
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.AccessibleDescription = null;
            this.BrowsePatch.AccessibleName = null;
            resources.ApplyResources(this.BrowsePatch, "BrowsePatch");
            this.BrowsePatch.BackgroundImage = null;
            this.BrowsePatch.Font = null;
            this.BrowsePatch.Name = "BrowsePatch";
            this.BrowsePatch.UseVisualStyleBackColor = true;
            this.BrowsePatch.Click += new System.EventHandler(this.BrowsePatch_Click);
            // 
            // PatchFileNameEdit
            // 
            this.PatchFileNameEdit.AccessibleDescription = null;
            this.PatchFileNameEdit.AccessibleName = null;
            resources.ApplyResources(this.PatchFileNameEdit, "PatchFileNameEdit");
            this.PatchFileNameEdit.BackgroundImage = null;
            this.PatchFileNameEdit.Font = null;
            this.PatchFileNameEdit.Name = "PatchFileNameEdit";
            this.PatchFileNameEdit.TextChanged += new System.EventHandler(this.PatchFileNameEdit_TextChanged);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // ViewPatch
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.splitContainer4);
            this.Font = null;
            this.Name = "ViewPatch";
            this.Load += new System.EventHandler(this.ViewPatch_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridChangedFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.patchBindingSource)).EndInit();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.changedFileBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView GridChangedFiles;
        private System.Windows.Forms.BindingSource changedFileBindingSource;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Button BrowsePatch;
        private System.Windows.Forms.TextBox PatchFileNameEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource patchBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileNameA;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn File;
        private GitUI.FileViewer ChangesList;
    }
}

