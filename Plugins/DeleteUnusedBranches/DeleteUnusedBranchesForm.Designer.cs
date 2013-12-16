namespace DeleteUnusedBranches
{
    partial class DeleteUnusedBranchesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteUnusedBranchesForm));
            this.BranchesGrid = new System.Windows.Forms.DataGridView();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.regexFilter = new System.Windows.Forms.TextBox();
            this.IncludeRemoteBranches = new System.Windows.Forms.CheckBox();
            this.remote = new System.Windows.Forms.TextBox();
            this.useRegexFilter = new System.Windows.Forms.CheckBox();
            this.Refresh = new System.Windows.Forms.Button();
            this.refreshHint = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.mergedIntoBranch = new System.Windows.Forms.TextBox();
            this.olderThanDays = new System.Windows.Forms.NumericUpDown();
            this.includeUnmergedBranches = new System.Windows.Forms.CheckBox();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deleteDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.branchBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.BranchesGrid)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olderThanDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.branchBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BranchesGrid
            // 
            this.BranchesGrid.AllowUserToAddRows = false;
            this.BranchesGrid.AllowUserToDeleteRows = false;
            this.BranchesGrid.AutoGenerateColumns = false;
            this.BranchesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BranchesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.dateDataGridViewTextBoxColumn,
            this.deleteDataGridViewCheckBoxColumn,
            this.Result});
            this.BranchesGrid.DataSource = this.branchBindingSource;
            this.BranchesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchesGrid.Location = new System.Drawing.Point(3, 218);
            this.BranchesGrid.Name = "BranchesGrid";
            this.BranchesGrid.RowHeadersVisible = false;
            this.BranchesGrid.Size = new System.Drawing.Size(754, 163);
            this.BranchesGrid.TabIndex = 0;
            // 
            // Result
            // 
            this.Result.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Result.DataPropertyName = "Result";
            this.Result.FillWeight = 2F;
            this.Result.HeaderText = "Result";
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            this.Result.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.BranchesGrid, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.instructionLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(760, 421);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.buttonSettings, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Cancel, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.Delete, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 387);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(754, 31);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // buttonSettings
            // 
            this.buttonSettings.Location = new System.Drawing.Point(3, 3);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(75, 25);
            this.buttonSettings.TabIndex = 2;
            this.buttonSettings.Text = "Settings";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.buttonSettings_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(676, 3);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 25);
            this.Cancel.TabIndex = 0;
            this.Cancel.Text = "Close";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Delete
            // 
            this.Delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Delete.Location = new System.Drawing.Point(595, 3);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 25);
            this.Delete.TabIndex = 1;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // instructionLabel
            // 
            this.instructionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(3, 6);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(19, 13);
            this.instructionLabel.TabIndex = 1;
            this.instructionLabel.Text = "....";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.regexFilter, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.IncludeRemoteBranches, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.remote, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.useRegexFilter, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.Refresh, 1, 5);
            this.tableLayoutPanel3.Controls.Add(this.refreshHint, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.mergedIntoBranch, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.olderThanDays, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.includeUnmergedBranches, 0, 4);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 28);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 6;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(754, 184);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // regexFilter
            // 
            this.regexFilter.Location = new System.Drawing.Point(380, 93);
            this.regexFilter.Name = "regexFilter";
            this.regexFilter.Size = new System.Drawing.Size(218, 20);
            this.regexFilter.TabIndex = 6;
            this.regexFilter.Text = "/(feature|develop)/";
            this.regexFilter.TextChanged += new System.EventHandler(this.regexFilter_TextChanged);
            // 
            // IncludeRemoteBranches
            // 
            this.IncludeRemoteBranches.AutoSize = true;
            this.IncludeRemoteBranches.Location = new System.Drawing.Point(3, 63);
            this.IncludeRemoteBranches.Name = "IncludeRemoteBranches";
            this.IncludeRemoteBranches.Size = new System.Drawing.Size(162, 17);
            this.IncludeRemoteBranches.TabIndex = 3;
            this.IncludeRemoteBranches.Text = "Delete remote branches from";
            this.IncludeRemoteBranches.UseVisualStyleBackColor = true;
            this.IncludeRemoteBranches.CheckedChanged += new System.EventHandler(this.IncludeRemoteBranches_CheckedChanged);
            // 
            // remote
            // 
            this.remote.Location = new System.Drawing.Point(380, 63);
            this.remote.Name = "remote";
            this.remote.Size = new System.Drawing.Size(218, 20);
            this.remote.TabIndex = 4;
            this.remote.Text = "origin";
            this.remote.TextChanged += new System.EventHandler(this.remote_TextChanged);
            // 
            // useRegexFilter
            // 
            this.useRegexFilter.AutoSize = true;
            this.useRegexFilter.Location = new System.Drawing.Point(3, 93);
            this.useRegexFilter.Name = "useRegexFilter";
            this.useRegexFilter.Size = new System.Drawing.Size(155, 17);
            this.useRegexFilter.TabIndex = 5;
            this.useRegexFilter.Text = "Use regex to filter branches";
            this.useRegexFilter.UseVisualStyleBackColor = true;
            this.useRegexFilter.CheckedChanged += new System.EventHandler(this.useRegexFilter_CheckedChanged);
            // 
            // Refresh
            // 
            this.Refresh.Location = new System.Drawing.Point(380, 153);
            this.Refresh.Name = "Refresh";
            this.Refresh.Size = new System.Drawing.Size(124, 23);
            this.Refresh.TabIndex = 7;
            this.Refresh.Text = "Search branches";
            this.Refresh.UseVisualStyleBackColor = true;
            this.Refresh.Click += new System.EventHandler(this.Refresh_Click);
            // 
            // refreshHint
            // 
            this.refreshHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.refreshHint.AutoSize = true;
            this.refreshHint.Location = new System.Drawing.Point(3, 171);
            this.refreshHint.Name = "refreshHint";
            this.refreshHint.Size = new System.Drawing.Size(0, 13);
            this.refreshHint.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Delete branches older than x days";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Delete branches fully merged into branch";
            // 
            // mergedIntoBranch
            // 
            this.mergedIntoBranch.Location = new System.Drawing.Point(380, 33);
            this.mergedIntoBranch.Name = "mergedIntoBranch";
            this.mergedIntoBranch.Size = new System.Drawing.Size(218, 20);
            this.mergedIntoBranch.TabIndex = 12;
            this.mergedIntoBranch.TextChanged += new System.EventHandler(this.mergedIntoBranch_TextChanged);
            // 
            // olderThanDays
            // 
            this.olderThanDays.Location = new System.Drawing.Point(380, 3);
            this.olderThanDays.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.olderThanDays.Name = "olderThanDays";
            this.olderThanDays.Size = new System.Drawing.Size(120, 20);
            this.olderThanDays.TabIndex = 13;
            this.olderThanDays.ValueChanged += new System.EventHandler(this.olderThanDays_ValueChanged);
            // 
            // includeUnmergedBranches
            // 
            this.includeUnmergedBranches.AutoSize = true;
            this.includeUnmergedBranches.Location = new System.Drawing.Point(3, 123);
            this.includeUnmergedBranches.Name = "includeUnmergedBranches";
            this.includeUnmergedBranches.Size = new System.Drawing.Size(158, 17);
            this.includeUnmergedBranches.TabIndex = 14;
            this.includeUnmergedBranches.Text = "Include unmerged branches";
            this.includeUnmergedBranches.UseVisualStyleBackColor = true;
            this.includeUnmergedBranches.CheckedChanged += new System.EventHandler(this.includeUnmergedBranches_CheckedChanged);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.FillWeight = 2F;
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dateDataGridViewTextBoxColumn.DataPropertyName = "Date";
            this.dateDataGridViewTextBoxColumn.FillWeight = 300F;
            this.dateDataGridViewTextBoxColumn.HeaderText = "Last activity";
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            this.dateDataGridViewTextBoxColumn.ReadOnly = true;
            this.dateDataGridViewTextBoxColumn.Width = 175;
            // 
            // deleteDataGridViewCheckBoxColumn
            // 
            this.deleteDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.deleteDataGridViewCheckBoxColumn.DataPropertyName = "Delete";
            this.deleteDataGridViewCheckBoxColumn.FillWeight = 20F;
            this.deleteDataGridViewCheckBoxColumn.HeaderText = "Delete";
            this.deleteDataGridViewCheckBoxColumn.Name = "deleteDataGridViewCheckBoxColumn";
            this.deleteDataGridViewCheckBoxColumn.Width = 50;
            // 
            // branchBindingSource
            // 
            this.branchBindingSource.DataSource = typeof(DeleteUnusedBranches.Branch);
            // 
            // DeleteUnusedBranchesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(760, 421);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "DeleteUnusedBranchesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Delete obsolete branches";
            ((System.ComponentModel.ISupportInitialize)(this.BranchesGrid)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olderThanDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.branchBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView BranchesGrid;
        private System.Windows.Forms.BindingSource branchBindingSource;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn deleteDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.CheckBox IncludeRemoteBranches;
        private System.Windows.Forms.TextBox remote;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox regexFilter;
        private System.Windows.Forms.CheckBox useRegexFilter;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.Label refreshHint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mergedIntoBranch;
        private System.Windows.Forms.NumericUpDown olderThanDays;
        private System.Windows.Forms.CheckBox includeUnmergedBranches;
    }
}