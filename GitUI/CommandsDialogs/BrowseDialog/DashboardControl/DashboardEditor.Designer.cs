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
            this.RepositoriesType = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Caption = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_RssFeed = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RssFeedType = new System.Windows.Forms.RadioButton();
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
#endif
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 460);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(192, 32);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.AutoSize = true;
            this.Remove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Remove.Location = new System.Drawing.Point(126, 3);
            this.Remove.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(64, 26);
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
            this.Add.Size = new System.Drawing.Size(40, 26);
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
            this._NO_TRANSLATE_Categories.ItemHeight = 16;
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
            this.RepositoriesGrid.Location = new System.Drawing.Point(0, 131);
            this.RepositoriesGrid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.RepositoriesGrid.MultiSelect = false;
            this.RepositoriesGrid.Name = "RepositoriesGrid";
            this.RepositoriesGrid.RowHeadersVisible = false;
            this.RepositoriesGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.RepositoriesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.RepositoriesGrid.Size = new System.Drawing.Size(584, 361);
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
            this.panel2.Controls.Add(this.RepositoriesType);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this._NO_TRANSLATE_Caption);
            this.panel2.Controls.Add(this._NO_TRANSLATE_RssFeed);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.RssFeedType);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(584, 131);
            this.panel2.TabIndex = 6;
            // 
            // RepositoriesType
            // 
            this.RepositoriesType.AutoSize = true;
            this.RepositoriesType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RepositoriesType.Location = new System.Drawing.Point(102, 69);
            this.RepositoriesType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.RepositoriesType.Name = "RepositoriesType";
            this.RepositoriesType.Size = new System.Drawing.Size(96, 20);
            this.RepositoriesType.TabIndex = 8;
            this.RepositoriesType.Text = "Repositories";
            this.RepositoriesType.UseVisualStyleBackColor = true;
            this.RepositoriesType.Validating += new System.ComponentModel.CancelEventHandler(this.RepositoriesType_Validating);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(2, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 16);
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
            this._NO_TRANSLATE_Caption.Size = new System.Drawing.Size(480, 23);
            this._NO_TRANSLATE_Caption.TabIndex = 9;
            this._NO_TRANSLATE_Caption.TextChanged += new System.EventHandler(this.Caption_TextChanged);
            this._NO_TRANSLATE_Caption.Validating += new System.ComponentModel.CancelEventHandler(this.Caption_Validating);
            // 
            // _NO_TRANSLATE_RssFeed
            // 
            this._NO_TRANSLATE_RssFeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_RssFeed.Location = new System.Drawing.Point(102, 99);
            this._NO_TRANSLATE_RssFeed.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_RssFeed.Name = "_NO_TRANSLATE_RssFeed";
            this._NO_TRANSLATE_RssFeed.Size = new System.Drawing.Size(480, 23);
            this._NO_TRANSLATE_RssFeed.TabIndex = 12;
            this._NO_TRANSLATE_RssFeed.TextChanged += new System.EventHandler(this.RssFeed_TextChanged);
            this._NO_TRANSLATE_RssFeed.Validating += new System.ComponentModel.CancelEventHandler(this.RssFeed_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(2, 43);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Type";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(2, 101);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 16);
            this.label3.TabIndex = 11;
            this.label3.Text = "RSS feed";
            // 
            // RssFeedType
            // 
            this.RssFeedType.AutoSize = true;
            this.RssFeedType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RssFeedType.Location = new System.Drawing.Point(102, 41);
            this.RssFeedType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.RssFeedType.Name = "RssFeedType";
            this.RssFeedType.Size = new System.Drawing.Size(82, 20);
            this.RssFeedType.TabIndex = 7;
            this.RssFeedType.Text = "RSS Feed";
            this.RssFeedType.UseVisualStyleBackColor = true;
            this.RssFeedType.CheckedChanged += new System.EventHandler(this.RssFeedType_CheckedChanged);
            this.RssFeedType.Validating += new System.ComponentModel.CancelEventHandler(this.RssFeedType_Validating);
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
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
#endif
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
        private System.Windows.Forms.RadioButton RepositoriesType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Caption;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_RssFeed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton RssFeedType;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
