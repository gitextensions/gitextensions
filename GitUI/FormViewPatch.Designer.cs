using GitUI.Editor;

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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.GridChangedFiles = new System.Windows.Forms.DataGridView();
            this.FileNameA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.patchBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ChangesList = new GitUI.Editor.FileViewer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.BrowsePatch = new System.Windows.Forms.Button();
            this.PatchFileNameEdit = new System.Windows.Forms.TextBox();
            this.labelPatch = new System.Windows.Forms.Label();
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
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.GridChangedFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ChangesList);
            this.splitContainer1.Size = new System.Drawing.Size(689, 457);
            this.splitContainer1.SplitterDistance = 161;
            this.splitContainer1.TabIndex = 1;
            // 
            // GridChangedFiles
            // 
            this.GridChangedFiles.AllowUserToAddRows = false;
            this.GridChangedFiles.AllowUserToDeleteRows = false;
            this.GridChangedFiles.AutoGenerateColumns = false;
            this.GridChangedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridChangedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileNameA,
            this.typeDataGridViewTextBoxColumn,
            this.File});
            this.GridChangedFiles.DataSource = this.patchBindingSource;
            this.GridChangedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridChangedFiles.Location = new System.Drawing.Point(0, 0);
            this.GridChangedFiles.MultiSelect = false;
            this.GridChangedFiles.Name = "GridChangedFiles";
            this.GridChangedFiles.ReadOnly = true;
            this.GridChangedFiles.RowHeadersVisible = false;
            this.GridChangedFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.GridChangedFiles.Size = new System.Drawing.Size(689, 161);
            this.GridChangedFiles.TabIndex = 0;
            this.GridChangedFiles.SelectionChanged += new System.EventHandler(this.GridChangedFiles_SelectionChanged);
            // 
            // FileNameA
            // 
            this.FileNameA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FileNameA.DataPropertyName = "FileNameA";
            this.FileNameA.HeaderText = "Filename";
            this.FileNameA.Name = "FileNameA";
            this.FileNameA.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            this.typeDataGridViewTextBoxColumn.HeaderText = "Change";
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            this.typeDataGridViewTextBoxColumn.ReadOnly = true;
            this.typeDataGridViewTextBoxColumn.Width = 70;
            // 
            // File
            // 
            this.File.DataPropertyName = "File";
            this.File.HeaderText = "Type";
            this.File.Name = "File";
            this.File.ReadOnly = true;
            this.File.Width = 50;
            // 
            // patchBindingSource
            // 
            this.patchBindingSource.DataSource = typeof(PatchApply.Patch);
            // 
            // ChangesList
            // 
            this.ChangesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangesList.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.ChangesList.IgnoreWhitespaceChanges = false;
            this.ChangesList.IsReadOnly = true;
            this.ChangesList.Location = new System.Drawing.Point(0, 0);
            this.ChangesList.Name = "ChangesList";
            this.ChangesList.NumberOfVisibleLines = 3;
            this.ChangesList.ScrollPos = 0;
            this.ChangesList.ShowEntireFile = false;
            this.ChangesList.ShowLineNumbers = true;
            this.ChangesList.Size = new System.Drawing.Size(689, 292);
            this.ChangesList.TabIndex = 1;
            this.ChangesList.TreatAllFilesAsText = false;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.BrowsePatch);
            this.splitContainer4.Panel1.Controls.Add(this.PatchFileNameEdit);
            this.splitContainer4.Panel1.Controls.Add(this.labelPatch);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer4.Size = new System.Drawing.Size(689, 501);
            this.splitContainer4.SplitterDistance = 40;
            this.splitContainer4.TabIndex = 2;
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Location = new System.Drawing.Point(356, 7);
            this.BrowsePatch.Name = "BrowsePatch";
            this.BrowsePatch.Size = new System.Drawing.Size(100, 25);
            this.BrowsePatch.TabIndex = 5;
            this.BrowsePatch.Text = "Browse";
            this.BrowsePatch.UseVisualStyleBackColor = true;
            this.BrowsePatch.Click += new System.EventHandler(this.BrowsePatch_Click);
            // 
            // PatchFileNameEdit
            // 
            this.PatchFileNameEdit.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.PatchFileNameEdit.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.PatchFileNameEdit.Location = new System.Drawing.Point(93, 9);
            this.PatchFileNameEdit.Name = "PatchFileNameEdit";
            this.PatchFileNameEdit.Size = new System.Drawing.Size(256, 21);
            this.PatchFileNameEdit.TabIndex = 3;
            this.PatchFileNameEdit.TextChanged += new System.EventHandler(this.PatchFileNameEdit_TextChanged);
            // 
            // labelPatch
            // 
            this.labelPatch.AutoSize = true;
            this.labelPatch.Location = new System.Drawing.Point(3, 13);
            this.labelPatch.Name = "labelPatch";
            this.labelPatch.Size = new System.Drawing.Size(34, 13);
            this.labelPatch.TabIndex = 1;
            this.labelPatch.Text = "Patch";
            // 
            // ViewPatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 501);
            this.Controls.Add(this.splitContainer4);
            this.Name = "ViewPatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "View patch file";
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
        private System.Windows.Forms.Label labelPatch;
        private System.Windows.Forms.BindingSource patchBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileNameA;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn File;
        private FileViewer ChangesList;
    }
}

