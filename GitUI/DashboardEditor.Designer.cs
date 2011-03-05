using GitCommands.Repository;

namespace GitUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardEditor));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._NO_TRANSLATE_Categories = new System.Windows.Forms.ListBox();
            this.CategoriesLabel = new System.Windows.Forms.Label();
            this.Remove = new System.Windows.Forms.Button();
            this.Add = new System.Windows.Forms.Button();
            this.RepositoriesGrid = new System.Windows.Forms.DataGridView();
            this.pathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.RepositoriesType = new System.Windows.Forms.RadioButton();
            this._NO_TRANSLATE_RssFeed = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.RssFeedType = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Caption = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RepositoriesGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._NO_TRANSLATE_Categories);
            this.splitContainer2.Panel1.Controls.Add(this.CategoriesLabel);
            this.splitContainer2.Panel1.Controls.Add(this.Remove);
            this.splitContainer2.Panel1.Controls.Add(this.Add);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.RepositoriesGrid);
            this.splitContainer2.Panel2.Controls.Add(this.RepositoriesType);
            this.splitContainer2.Panel2.Controls.Add(this._NO_TRANSLATE_RssFeed);
            this.splitContainer2.Panel2.Controls.Add(this.label3);
            this.splitContainer2.Panel2.Controls.Add(this.RssFeedType);
            this.splitContainer2.Panel2.Controls.Add(this.label2);
            this.splitContainer2.Panel2.Controls.Add(this._NO_TRANSLATE_Caption);
            this.splitContainer2.Panel2.Controls.Add(this.label1);
            // 
            // _NO_TRANSLATE_Categories
            // 
            resources.ApplyResources(this._NO_TRANSLATE_Categories, "_NO_TRANSLATE_Categories");
            this._NO_TRANSLATE_Categories.FormattingEnabled = true;
            this._NO_TRANSLATE_Categories.Name = "_NO_TRANSLATE_Categories";
            this._NO_TRANSLATE_Categories.SelectedIndexChanged += new System.EventHandler(this.Categories_SelectedIndexChanged);
            // 
            // CategoriesLabel
            // 
            resources.ApplyResources(this.CategoriesLabel, "CategoriesLabel");
            this.CategoriesLabel.Name = "CategoriesLabel";
            // 
            // Remove
            // 
            resources.ApplyResources(this.Remove, "Remove");
            this.Remove.Name = "Remove";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // Add
            // 
            resources.ApplyResources(this.Add, "Add");
            this.Add.Name = "Add";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // RepositoriesGrid
            // 
            resources.ApplyResources(this.RepositoriesGrid, "RepositoriesGrid");
            this.RepositoriesGrid.AutoGenerateColumns = false;
            this.RepositoriesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.RepositoriesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathDataGridViewTextBoxColumn,
            this.Title,
            this.descriptionDataGridViewTextBoxColumn});
            this.RepositoriesGrid.DataSource = this.repositoryBindingSource;
            this.RepositoriesGrid.MultiSelect = false;
            this.RepositoriesGrid.Name = "RepositoriesGrid";
            this.RepositoriesGrid.RowHeadersVisible = false;
            this.RepositoriesGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.RepositoriesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.RepositoriesGrid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.RepositoriesGrid_UserDeletingRow);
            this.RepositoriesGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.RepositoriesGrid_CellValidating);
            // 
            // pathDataGridViewTextBoxColumn
            // 
            this.pathDataGridViewTextBoxColumn.DataPropertyName = "Path";
            resources.ApplyResources(this.pathDataGridViewTextBoxColumn, "pathDataGridViewTextBoxColumn");
            this.pathDataGridViewTextBoxColumn.Name = "pathDataGridViewTextBoxColumn";
            // 
            // Title
            // 
            this.Title.DataPropertyName = "Title";
            resources.ApplyResources(this.Title, "Title");
            this.Title.Name = "Title";
            // 
            // descriptionDataGridViewTextBoxColumn
            // 
            this.descriptionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
            resources.ApplyResources(this.descriptionDataGridViewTextBoxColumn, "descriptionDataGridViewTextBoxColumn");
            this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
            // 
            // repositoryBindingSource
            // 
            this.repositoryBindingSource.DataSource = typeof(GitCommands.Repository.Repository);
            // 
            // RepositoriesType
            // 
            resources.ApplyResources(this.RepositoriesType, "RepositoriesType");
            this.RepositoriesType.Name = "RepositoriesType";
            this.RepositoriesType.TabStop = true;
            this.RepositoriesType.UseVisualStyleBackColor = true;
            this.RepositoriesType.Validating += new System.ComponentModel.CancelEventHandler(this.RepositoriesType_Validating);
            this.RepositoriesType.CheckedChanged += new System.EventHandler(this.RepositoriesType_CheckedChanged);
            // 
            // _NO_TRANSLATE_RssFeed
            // 
            resources.ApplyResources(this._NO_TRANSLATE_RssFeed, "_NO_TRANSLATE_RssFeed");
            this._NO_TRANSLATE_RssFeed.Name = "_NO_TRANSLATE_RssFeed";
            this._NO_TRANSLATE_RssFeed.TextChanged += new System.EventHandler(this.RssFeed_TextChanged);
            this._NO_TRANSLATE_RssFeed.Validating += new System.ComponentModel.CancelEventHandler(this.RssFeed_Validating);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // RssFeedType
            // 
            resources.ApplyResources(this.RssFeedType, "RssFeedType");
            this.RssFeedType.Name = "RssFeedType";
            this.RssFeedType.TabStop = true;
            this.RssFeedType.UseVisualStyleBackColor = true;
            this.RssFeedType.Validating += new System.ComponentModel.CancelEventHandler(this.RssFeedType_Validating);
            this.RssFeedType.CheckedChanged += new System.EventHandler(this.RssFeedType_CheckedChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // _NO_TRANSLATE_Caption
            // 
            resources.ApplyResources(this._NO_TRANSLATE_Caption, "_NO_TRANSLATE_Caption");
            this._NO_TRANSLATE_Caption.Name = "_NO_TRANSLATE_Caption";
            this._NO_TRANSLATE_Caption.TextChanged += new System.EventHandler(this.Caption_TextChanged);
            this._NO_TRANSLATE_Caption.Validating += new System.ComponentModel.CancelEventHandler(this.Caption_Validating);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // DashboardEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Name = "DashboardEditor";
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RepositoriesGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.ListBox _NO_TRANSLATE_Categories;
        private System.Windows.Forms.Label CategoriesLabel;
        private System.Windows.Forms.DataGridView RepositoriesGrid;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Caption;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_RssFeed;
        private System.Windows.Forms.RadioButton RssFeedType;
        private System.Windows.Forms.RadioButton RepositoriesType;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
    }
}
