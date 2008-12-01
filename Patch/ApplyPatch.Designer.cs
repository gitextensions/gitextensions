namespace PatchApply
{
    partial class ApplyPatch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyPatch));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.GridChangedFiles = new System.Windows.Forms.DataGridView();
            this.ChangesList = new System.Windows.Forms.RichTextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.FileToPatchEdit = new ICSharpCode.TextEditor.TextEditorControl();
            this.PatchedFileEdit = new ICSharpCode.TextEditor.TextEditorControl();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.ApplyToDir = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.BrowsePatch = new System.Windows.Forms.Button();
            this.ApplyToDirEdit = new System.Windows.Forms.TextBox();
            this.PatchFileNameEdit = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.changedFileBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.patchBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.FileNameA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Apply = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridChangedFiles)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.changedFileBindingSource)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.patchBindingSource)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(689, 411);
            this.splitContainer1.SplitterDistance = 120;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.GridChangedFiles);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.ChangesList);
            this.splitContainer5.Size = new System.Drawing.Size(689, 120);
            this.splitContainer5.SplitterDistance = 377;
            this.splitContainer5.TabIndex = 0;
            // 
            // GridChangedFiles
            // 
            this.GridChangedFiles.AllowUserToAddRows = false;
            this.GridChangedFiles.AllowUserToDeleteRows = false;
            this.GridChangedFiles.AllowUserToOrderColumns = true;
            this.GridChangedFiles.AutoGenerateColumns = false;
            this.GridChangedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridChangedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileNameA,
            this.typeDataGridViewTextBoxColumn,
            this.Rate,
            this.File,
            this.Apply});
            this.GridChangedFiles.DataSource = this.patchBindingSource;
            this.GridChangedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridChangedFiles.Location = new System.Drawing.Point(0, 0);
            this.GridChangedFiles.MultiSelect = false;
            this.GridChangedFiles.Name = "GridChangedFiles";
            this.GridChangedFiles.RowHeadersVisible = false;
            this.GridChangedFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.GridChangedFiles.Size = new System.Drawing.Size(377, 120);
            this.GridChangedFiles.TabIndex = 0;
            this.GridChangedFiles.SelectionChanged += new System.EventHandler(this.GridChangedFiles_SelectionChanged);
            // 
            // ChangesList
            // 
            this.ChangesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangesList.Location = new System.Drawing.Point(0, 0);
            this.ChangesList.Name = "ChangesList";
            this.ChangesList.Size = new System.Drawing.Size(308, 120);
            this.ChangesList.TabIndex = 0;
            this.ChangesList.Text = "";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.FileToPatchEdit);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.PatchedFileEdit);
            this.splitContainer2.Size = new System.Drawing.Size(689, 287);
            this.splitContainer2.SplitterDistance = 339;
            this.splitContainer2.TabIndex = 0;
            // 
            // FileToPatchEdit
            // 
            this.FileToPatchEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileToPatchEdit.IsReadOnly = false;
            this.FileToPatchEdit.Location = new System.Drawing.Point(0, 0);
            this.FileToPatchEdit.Name = "FileToPatchEdit";
            this.FileToPatchEdit.Size = new System.Drawing.Size(339, 287);
            this.FileToPatchEdit.TabIndex = 0;
            // 
            // PatchedFileEdit
            // 
            this.PatchedFileEdit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.patchBindingSource, "Text", true));
            this.PatchedFileEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PatchedFileEdit.IsIconBarVisible = true;
            this.PatchedFileEdit.IsReadOnly = false;
            this.PatchedFileEdit.Location = new System.Drawing.Point(0, 0);
            this.PatchedFileEdit.Name = "PatchedFileEdit";
            this.PatchedFileEdit.Size = new System.Drawing.Size(346, 287);
            this.PatchedFileEdit.TabIndex = 0;
            this.PatchedFileEdit.Click += new System.EventHandler(this.PatchedFileEdit_Click);
            this.PatchedFileEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PatchedFileEdit_MouseMove);
            this.PatchedFileEdit.Leave += new System.EventHandler(this.PatchedFileEdit_Leave);
            this.PatchedFileEdit.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PatchedFileEdit_Scroll);
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
            this.splitContainer4.Panel1.Controls.Add(this.ApplyToDir);
            this.splitContainer4.Panel1.Controls.Add(this.LoadButton);
            this.splitContainer4.Panel1.Controls.Add(this.BrowsePatch);
            this.splitContainer4.Panel1.Controls.Add(this.ApplyToDirEdit);
            this.splitContainer4.Panel1.Controls.Add(this.PatchFileNameEdit);
            this.splitContainer4.Panel1.Controls.Add(this.label2);
            this.splitContainer4.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer4.Size = new System.Drawing.Size(689, 501);
            this.splitContainer4.SplitterDistance = 86;
            this.splitContainer4.TabIndex = 2;
            // 
            // ApplyToDir
            // 
            this.ApplyToDir.Location = new System.Drawing.Point(337, 55);
            this.ApplyToDir.Name = "ApplyToDir";
            this.ApplyToDir.Size = new System.Drawing.Size(75, 24);
            this.ApplyToDir.TabIndex = 8;
            this.ApplyToDir.Text = "Browse";
            this.ApplyToDir.UseVisualStyleBackColor = true;
            this.ApplyToDir.Click += new System.EventHandler(this.ApplyToDir_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(611, 29);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(75, 23);
            this.LoadButton.TabIndex = 6;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Location = new System.Drawing.Point(338, 29);
            this.BrowsePatch.Name = "BrowsePatch";
            this.BrowsePatch.Size = new System.Drawing.Size(75, 23);
            this.BrowsePatch.TabIndex = 5;
            this.BrowsePatch.Text = "Browse";
            this.BrowsePatch.UseVisualStyleBackColor = true;
            this.BrowsePatch.Click += new System.EventHandler(this.BrowsePatch_Click);
            // 
            // ApplyToDirEdit
            // 
            this.ApplyToDirEdit.Location = new System.Drawing.Point(75, 54);
            this.ApplyToDirEdit.Name = "ApplyToDirEdit";
            this.ApplyToDirEdit.Size = new System.Drawing.Size(256, 20);
            this.ApplyToDirEdit.TabIndex = 4;
            this.ApplyToDirEdit.TextChanged += new System.EventHandler(this.ApplyToDirEdit_TextChanged);
            // 
            // PatchFileNameEdit
            // 
            this.PatchFileNameEdit.Location = new System.Drawing.Point(75, 29);
            this.PatchFileNameEdit.Name = "PatchFileNameEdit";
            this.PatchFileNameEdit.Size = new System.Drawing.Size(256, 20);
            this.PatchFileNameEdit.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Apply to";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Patch";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator1,
            this.toolStripButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(689, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "toolStripButton3";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // patchBindingSource
            // 
            this.patchBindingSource.DataSource = typeof(PatchApply.Patch);
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
            // Rate
            // 
            this.Rate.DataPropertyName = "Rate";
            this.Rate.HeaderText = "Rate";
            this.Rate.Name = "Rate";
            this.Rate.ReadOnly = true;
            this.Rate.Width = 50;
            // 
            // File
            // 
            this.File.DataPropertyName = "File";
            this.File.HeaderText = "Type";
            this.File.Name = "File";
            this.File.ReadOnly = true;
            this.File.Width = 50;
            // 
            // Apply
            // 
            this.Apply.DataPropertyName = "Apply";
            this.Apply.HeaderText = "Apply";
            this.Apply.Name = "Apply";
            this.Apply.Width = 50;
            // 
            // ApplyPatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 501);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ApplyPatch";
            this.Text = "ApplyPatch";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplyPatch_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridChangedFiles)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.changedFileBindingSource)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.patchBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView GridChangedFiles;
        private System.Windows.Forms.RichTextBox ChangesList;
        private System.Windows.Forms.BindingSource changedFileBindingSource;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Button BrowsePatch;
        private System.Windows.Forms.TextBox ApplyToDirEdit;
        private System.Windows.Forms.TextBox PatchFileNameEdit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button LoadButton;
        private ICSharpCode.TextEditor.TextEditorControl FileToPatchEdit;
        private System.Windows.Forms.BindingSource patchBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileNameADataGridViewTextBoxColumn;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private ICSharpCode.TextEditor.TextEditorControl PatchedFileEdit;
        private System.Windows.Forms.Button ApplyToDir;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileNameA;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rate;
        private System.Windows.Forms.DataGridViewTextBoxColumn File;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Apply;
    }
}

