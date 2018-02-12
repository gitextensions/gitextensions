namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class DashboardEditor
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Remove = new System.Windows.Forms.Button();
            this.Add = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Categories = new System.Windows.Forms.ListBox();
            this.CategoriesLabel = new System.Windows.Forms.Label();
            this.RepositoriesGrid = new System.Windows.Forms.DataGridView();
            this.pathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Caption = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RepositoriesGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_Categories);
            this.splitContainer1.Panel1.Controls.Add(this.CategoriesLabel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.RepositoriesGrid);
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Panel2.Enabled = false;
            this.splitContainer1.Size = new System.Drawing.Size(780, 492);
            this.splitContainer1.SplitterDistance = 192;
            this.splitContainer1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.Remove, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Add, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 463);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(192, 29);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.AutoSize = true;
            this.Remove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Remove.Location = new System.Drawing.Point(134, 3);
            this.Remove.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(56, 23);
            this.Remove.TabIndex = 4;
            this.Remove.Text = "Remove";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // Add
            // 
            this.Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Add.AutoSize = true;
            this.Add.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Add.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Add.Location = new System.Drawing.Point(2, 3);
            this.Add.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(36, 23);
            this.Add.TabIndex = 3;
            this.Add.Text = "Add";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // _NO_TRANSLATE_Categories
            // 
            this._NO_TRANSLATE_Categories.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_Categories.FormattingEnabled = true;
            this._NO_TRANSLATE_Categories.IntegralHeight = false;
            this._NO_TRANSLATE_Categories.Location = new System.Drawing.Point(0, 27);
            this._NO_TRANSLATE_Categories.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_Categories.Name = "_NO_TRANSLATE_Categories";
            this._NO_TRANSLATE_Categories.Size = new System.Drawing.Size(192, 465);
            this._NO_TRANSLATE_Categories.TabIndex = 1;
            this._NO_TRANSLATE_Categories.SelectedIndexChanged += new System.EventHandler(this.Categories_SelectedIndexChanged);
            // 
            // CategoriesLabel
            // 
            this.CategoriesLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.CategoriesLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CategoriesLabel.Location = new System.Drawing.Point(0, 0);
            this.CategoriesLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CategoriesLabel.Name = "CategoriesLabel";
            this.CategoriesLabel.Size = new System.Drawing.Size(192, 27);
            this.CategoriesLabel.TabIndex = 0;
            this.CategoriesLabel.Text = "Categories";
            // 
            // RepositoriesGrid
            // 
            this.RepositoriesGrid.AutoGenerateColumns = false;
            this.RepositoriesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.RepositoriesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathDataGridViewTextBoxColumn,
            this.Title,
            this.descriptionDataGridViewTextBoxColumn});
            this.RepositoriesGrid.DataSource = this.repositoryBindingSource;
            this.RepositoriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RepositoriesGrid.Location = new System.Drawing.Point(0, 42);
            this.RepositoriesGrid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.RepositoriesGrid.MultiSelect = false;
            this.RepositoriesGrid.Name = "RepositoriesGrid";
            this.RepositoriesGrid.RowHeadersVisible = false;
            this.RepositoriesGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.RepositoriesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.RepositoriesGrid.Size = new System.Drawing.Size(584, 450);
            this.RepositoriesGrid.TabIndex = 0;
            this.RepositoriesGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.RepositoriesGrid_CellValidating);
            this.RepositoriesGrid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.RepositoriesGrid_UserDeletingRow);
            // 
            // pathDataGridViewTextBoxColumn
            // 
            this.pathDataGridViewTextBoxColumn.DataPropertyName = "Path";
            this.pathDataGridViewTextBoxColumn.HeaderText = "Path";
            this.pathDataGridViewTextBoxColumn.Name = "pathDataGridViewTextBoxColumn";
            this.pathDataGridViewTextBoxColumn.Width = 150;
            // 
            // Title
            // 
            this.Title.DataPropertyName = "Title";
            this.Title.HeaderText = "Title";
            this.Title.Name = "Title";
            this.Title.Width = 150;
            // 
            // descriptionDataGridViewTextBoxColumn
            // 
            this.descriptionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
            this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
            this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
            // 
            // repositoryBindingSource
            // 
            this.repositoryBindingSource.DataSource = typeof(GitCommands.Repository.Repository);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this._NO_TRANSLATE_Caption);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(584, 42);
            this.panel2.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(2, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Caption";
            // 
            // _NO_TRANSLATE_Caption
            // 
            this._NO_TRANSLATE_Caption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Caption.Location = new System.Drawing.Point(102, 8);
            this._NO_TRANSLATE_Caption.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_Caption.Name = "_NO_TRANSLATE_Caption";
            this._NO_TRANSLATE_Caption.Size = new System.Drawing.Size(480, 21);
            this._NO_TRANSLATE_Caption.TabIndex = 9;
            this._NO_TRANSLATE_Caption.TextChanged += new System.EventHandler(this.Caption_TextChanged);
            this._NO_TRANSLATE_Caption.Validating += new System.ComponentModel.CancelEventHandler(this.Caption_Validating);
            // 
            // DashboardEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "DashboardEditor";
            this.Size = new System.Drawing.Size(780, 492);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RepositoriesGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox _NO_TRANSLATE_Categories;
        private System.Windows.Forms.Label CategoriesLabel;
        private System.Windows.Forms.DataGridView RepositoriesGrid;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Caption;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
