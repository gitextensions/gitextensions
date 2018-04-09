using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormViewPatch
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
            this.BrowsePatch = new System.Windows.Forms.Button();
            this.PatchFileNameEdit = new System.Windows.Forms.TextBox();
            this.labelPatch = new System.Windows.Forms.Label();
            this.changedFileBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();

            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridChangedFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.patchBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.changedFileBindingSource)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(3, 40);
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
            this.splitContainer1.Size = new System.Drawing.Size(683, 458);
            this.splitContainer1.SplitterDistance = 270;
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
            this.GridChangedFiles.Size = new System.Drawing.Size(683, 270);
            this.GridChangedFiles.TabIndex = 0;
            this.GridChangedFiles.SelectionChanged += new System.EventHandler(this.GridChangedFiles_SelectionChanged);
            // 
            // FileNameA
            // 
            this.FileNameA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FileNameA.HeaderText = "Filename";
            this.FileNameA.Name = "FileNameA";
            this.FileNameA.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.HeaderText = "Change";
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            this.typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // File
            // 
            this.File.HeaderText = "Type";
            this.File.Name = "File";
            this.File.ReadOnly = true;
            // 
            // patchBindingSource
            // 
            this.patchBindingSource.DataSource = typeof(GitCommands.Patches.Patch);
            // 
            // ChangesList
            // 
            this.ChangesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangesList.Location = new System.Drawing.Point(0, 0);
            this.ChangesList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ChangesList.Name = "ChangesList";
            this.ChangesList.Size = new System.Drawing.Size(683, 184);
            this.ChangesList.TabIndex = 1;
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.BrowsePatch.Location = new System.Drawing.Point(580, 3);
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
            this.PatchFileNameEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PatchFileNameEdit.Location = new System.Drawing.Point(46, 3);
            this.PatchFileNameEdit.Name = "PatchFileNameEdit";
            this.PatchFileNameEdit.Size = new System.Drawing.Size(528, 23);
            this.PatchFileNameEdit.TabIndex = 3;
            // 
            // labelPatch
            // 
            this.labelPatch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelPatch.AutoSize = true;
            this.labelPatch.Location = new System.Drawing.Point(3, 8);
            this.labelPatch.Name = "labelPatch";
            this.labelPatch.Size = new System.Drawing.Size(37, 15);
            this.labelPatch.TabIndex = 1;
            this.labelPatch.Text = "Patch";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(689, 501);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.BrowsePatch, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelPatch, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.PatchFileNameEdit, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(683, 31);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // FormViewPatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(689, 501);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormViewPatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "View patch file";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();

            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridChangedFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.patchBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.changedFileBindingSource)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView GridChangedFiles;
        private System.Windows.Forms.BindingSource changedFileBindingSource;
        private System.Windows.Forms.Button BrowsePatch;
        private System.Windows.Forms.TextBox PatchFileNameEdit;
        private System.Windows.Forms.Label labelPatch;
        private System.Windows.Forms.BindingSource patchBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileNameA;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn File;
        private FileViewer ChangesList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}

