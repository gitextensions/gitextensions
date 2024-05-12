using GitExtensions.Extensibility.Git;
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
            splitContainer1 = new SplitContainer();
            GridChangedFiles = new DataGridView();
            FileNameA = new DataGridViewTextBoxColumn();
            typeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            File = new DataGridViewTextBoxColumn();
            patchBindingSource = new BindingSource(components);
            ChangesList = new GitUI.Editor.FileViewer();
            BrowsePatch = new Button();
            PatchFileNameEdit = new TextBox();
            labelPatch = new Label();
            changedFileBindingSource = new BindingSource(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();

            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(GridChangedFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(patchBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(changedFileBindingSource)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(3, 40);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(GridChangedFiles);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(ChangesList);
            splitContainer1.Size = new Size(683, 458);
            splitContainer1.SplitterDistance = 270;
            splitContainer1.TabIndex = 1;
            // 
            // GridChangedFiles
            // 
            GridChangedFiles.AllowUserToAddRows = false;
            GridChangedFiles.AllowUserToDeleteRows = false;
            GridChangedFiles.AutoGenerateColumns = false;
            GridChangedFiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridChangedFiles.Columns.AddRange(new DataGridViewColumn[] {
            FileNameA,
            typeDataGridViewTextBoxColumn,
            File});
            GridChangedFiles.DataSource = patchBindingSource;
            GridChangedFiles.Dock = DockStyle.Fill;
            GridChangedFiles.Location = new Point(0, 0);
            GridChangedFiles.MultiSelect = false;
            GridChangedFiles.Name = "GridChangedFiles";
            GridChangedFiles.ReadOnly = true;
            GridChangedFiles.RowHeadersVisible = false;
            GridChangedFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            GridChangedFiles.Size = new Size(683, 270);
            GridChangedFiles.TabIndex = 0;
            GridChangedFiles.SelectionChanged += GridChangedFiles_SelectionChanged;
            // 
            // FileNameA
            // 
            FileNameA.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            FileNameA.HeaderText = "Filename";
            FileNameA.Name = "FileNameA";
            FileNameA.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            typeDataGridViewTextBoxColumn.HeaderText = "Change";
            typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // File
            // 
            File.HeaderText = "Type";
            File.Name = "File";
            File.ReadOnly = true;
            // 
            // patchBindingSource
            // 
            patchBindingSource.DataSource = typeof(Patch);
            // 
            // ChangesList
            // 
            ChangesList.Dock = DockStyle.Fill;
            ChangesList.Location = new Point(0, 0);
            ChangesList.Margin = new Padding(3, 2, 3, 2);
            ChangesList.Name = "ChangesList";
            ChangesList.Size = new Size(683, 184);
            ChangesList.TabIndex = 1;
            // 
            // BrowsePatch
            // 
            BrowsePatch.Anchor = AnchorStyles.Left;
            BrowsePatch.Location = new Point(580, 3);
            BrowsePatch.Name = "BrowsePatch";
            BrowsePatch.Size = new Size(100, 25);
            BrowsePatch.TabIndex = 5;
            BrowsePatch.Text = "Browse";
            BrowsePatch.UseVisualStyleBackColor = true;
            BrowsePatch.Click += BrowsePatch_Click;
            // 
            // PatchFileNameEdit
            // 
            PatchFileNameEdit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            PatchFileNameEdit.AutoCompleteSource = AutoCompleteSource.FileSystem;
            PatchFileNameEdit.Dock = DockStyle.Fill;
            PatchFileNameEdit.Location = new Point(46, 3);
            PatchFileNameEdit.Name = "PatchFileNameEdit";
            PatchFileNameEdit.Size = new Size(528, 23);
            PatchFileNameEdit.TabIndex = 3;
            // 
            // labelPatch
            // 
            labelPatch.Anchor = AnchorStyles.Left;
            labelPatch.AutoSize = true;
            labelPatch.Location = new Point(3, 8);
            labelPatch.Name = "labelPatch";
            labelPatch.Size = new Size(37, 15);
            labelPatch.TabIndex = 1;
            labelPatch.Text = "Patch";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(splitContainer1, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(689, 501);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(BrowsePatch, 2, 0);
            tableLayoutPanel2.Controls.Add(labelPatch, 0, 0);
            tableLayoutPanel2.Controls.Add(PatchFileNameEdit, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(683, 31);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // FormViewPatch
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(689, 501);
            Controls.Add(tableLayoutPanel1);
            Name = "FormViewPatch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "View patch file";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();

            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(GridChangedFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(patchBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(changedFileBindingSource)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private DataGridView GridChangedFiles;
        private BindingSource changedFileBindingSource;
        private Button BrowsePatch;
        private TextBox PatchFileNameEdit;
        private Label labelPatch;
        private BindingSource patchBindingSource;
        private DataGridViewTextBoxColumn FileNameA;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn File;
        private FileViewer ChangesList;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
    }
}

