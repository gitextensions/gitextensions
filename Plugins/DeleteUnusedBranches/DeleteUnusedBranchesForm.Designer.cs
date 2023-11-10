namespace GitExtensions.Plugins.DeleteUnusedBranches
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
            StatusStrip statusStrip1;
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteUnusedBranchesForm));
            lblStatus = new ToolStripStatusLabel();
            pnlBranchesArea = new Panel();
            imgLoading = new PictureBox();
            BranchesGrid = new DataGridView();
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
            nameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            dateDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            checkBoxHeaderCell = new DataGridViewCheckBoxHeaderCell();
            Author = new DataGridViewTextBoxColumn();
            Message = new DataGridViewTextBoxColumn();
            branchBindingSource = new BindingSource(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            buttonSettings = new Button();
            Cancel = new Button();
            Delete = new Button();
            instructionLabel = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            regexFilter = new TextBox();
            IncludeRemoteBranches = new CheckBox();
            _NO_TRANSLATE_Remote = new TextBox();
            useRegexFilter = new CheckBox();
            label1 = new Label();
            label2 = new Label();
            mergedIntoBranch = new TextBox();
            olderThanDays = new NumericUpDown();
            RefreshBtn = new Button();
            includeUnmergedBranches = new CheckBox();
            useRegexCaseInsensitive = new CheckBox();
            regexDoesNotMatch = new CheckBox();
            statusStrip1 = new StatusStrip();
            statusStrip1.SuspendLayout();
            pnlBranchesArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(imgLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(BranchesGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(branchBindingSource)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(olderThanDays)).BeginInit();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] {
            lblStatus});
            statusStrip1.Location = new Point(0, 399);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(760, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 17);
            // 
            // pnlBranchesArea
            // 
            pnlBranchesArea.Controls.Add(imgLoading);
            pnlBranchesArea.Controls.Add(BranchesGrid);
            pnlBranchesArea.Dock = DockStyle.Fill;
            pnlBranchesArea.Location = new Point(3, 275);
            pnlBranchesArea.Name = "pnlBranchesArea";
            pnlBranchesArea.Size = new Size(754, 84);
            pnlBranchesArea.TabIndex = 2;
            // 
            // imgLoading
            // 
            imgLoading.BackColor = SystemColors.AppWorkspace;
            imgLoading.Dock = DockStyle.Fill;
            imgLoading.Location = new Point(0, 0);
            imgLoading.Name = "imgLoading";
            imgLoading.Size = new Size(754, 84);
            imgLoading.SizeMode = PictureBoxSizeMode.CenterImage;
            imgLoading.TabIndex = 1;
            imgLoading.TabStop = false;
            imgLoading.Visible = false;
            // 
            // BranchesGrid
            // 
            BranchesGrid.AllowUserToAddRows = false;
            BranchesGrid.AllowUserToDeleteRows = false;
            BranchesGrid.AutoGenerateColumns = false;
            BranchesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            BranchesGrid.Columns.AddRange(new DataGridViewColumn[] {
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn,
            nameDataGridViewTextBoxColumn,
            dateDataGridViewTextBoxColumn,
            Author,
            Message});
            BranchesGrid.DataSource = branchBindingSource;
            BranchesGrid.Dock = DockStyle.Fill;
            BranchesGrid.Location = new Point(0, 0);
            BranchesGrid.Name = "BranchesGrid";
            BranchesGrid.RowHeadersVisible = false;
            BranchesGrid.Size = new Size(754, 84);
            BranchesGrid.TabIndex = 0;
            BranchesGrid.CellContentClick += BranchesGrid_CellContentClick;
            // 
            // _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn
            // 
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.FillWeight = 20F;
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.Name = "_NO_TRANSLATE_deleteDataGridViewCheckBoxColumn";
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.HeaderCell = checkBoxHeaderCell;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            nameDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.Padding = new Padding(5, 0, 5, 0);
            nameDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle1;
            nameDataGridViewTextBoxColumn.FillWeight = 2F;
            nameDataGridViewTextBoxColumn.HeaderText = "Name";
            nameDataGridViewTextBoxColumn.MinimumWidth = 100;
            nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            dateDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dateDataGridViewTextBoxColumn.FillWeight = 300F;
            dateDataGridViewTextBoxColumn.HeaderText = "Last activity";
            dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            dateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Author
            // 
            Author.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Padding = new Padding(5, 0, 5, 0);
            Author.DefaultCellStyle = dataGridViewCellStyle2;
            Author.FillWeight = 2F;
            Author.HeaderText = "Last author";
            Author.Name = "Author";
            Author.ReadOnly = true;
            // 
            // Message
            // 
            Message.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Message.HeaderText = "Last message";
            Message.Name = "Message";
            Message.ReadOnly = true;
            Message.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // branchBindingSource
            // 
            branchBindingSource.DataSource = typeof(DeleteUnusedBranches.Branch);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pnlBranchesArea, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 3);
            tableLayoutPanel1.Controls.Add(instructionLabel, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 247F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tableLayoutPanel1.Size = new Size(760, 399);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 4;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Controls.Add(buttonSettings, 0, 0);
            tableLayoutPanel2.Controls.Add(Cancel, 3, 0);
            tableLayoutPanel2.Controls.Add(Delete, 2, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 365);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(754, 31);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // buttonSettings
            // 
            buttonSettings.Location = new Point(3, 3);
            buttonSettings.Name = "buttonSettings";
            buttonSettings.Size = new Size(75, 25);
            buttonSettings.TabIndex = 2;
            buttonSettings.Text = "Settings";
            buttonSettings.UseVisualStyleBackColor = true;
            buttonSettings.Click += buttonSettings_Click;
            // 
            // Cancel
            // 
            Cancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(676, 3);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 25);
            Cancel.TabIndex = 0;
            Cancel.Text = "Close";
            Cancel.UseVisualStyleBackColor = true;
            // 
            // Delete
            // 
            Delete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Delete.Location = new Point(595, 3);
            Delete.Name = "Delete";
            Delete.Size = new Size(75, 25);
            Delete.TabIndex = 1;
            Delete.Text = "Delete";
            Delete.UseVisualStyleBackColor = true;
            Delete.Click += Delete_Click;
            // 
            // instructionLabel
            // 
            instructionLabel.Anchor = AnchorStyles.Left;
            instructionLabel.AutoSize = true;
            instructionLabel.Location = new Point(3, 5);
            instructionLabel.Name = "instructionLabel";
            instructionLabel.Size = new Size(19, 15);
            instructionLabel.TabIndex = 1;
            instructionLabel.Text = "....";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(regexFilter, 1, 3);
            tableLayoutPanel3.Controls.Add(IncludeRemoteBranches, 0, 2);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_Remote, 1, 2);
            tableLayoutPanel3.Controls.Add(useRegexFilter, 0, 3);
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(label2, 0, 1);
            tableLayoutPanel3.Controls.Add(mergedIntoBranch, 1, 1);
            tableLayoutPanel3.Controls.Add(olderThanDays, 1, 0);
            tableLayoutPanel3.Controls.Add(useRegexCaseInsensitive, 1, 4);
            tableLayoutPanel3.Controls.Add(RefreshBtn, 1, 7);
            tableLayoutPanel3.Controls.Add(includeUnmergedBranches, 0, 6);
            tableLayoutPanel3.Controls.Add(regexDoesNotMatch, 1, 5);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 28);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 8;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(754, 241);
            tableLayoutPanel3.TabIndex = 3;
            // 
            // regexFilter
            // 
            regexFilter.Location = new Point(380, 93);
            regexFilter.Name = "regexFilter";
            regexFilter.Size = new Size(218, 23);
            regexFilter.TabIndex = 6;
            regexFilter.Text = "/(feature|develop)/";
            regexFilter.TextChanged += ClearResults;
            // 
            // IncludeRemoteBranches
            // 
            IncludeRemoteBranches.AutoSize = true;
            IncludeRemoteBranches.Location = new Point(3, 63);
            IncludeRemoteBranches.Name = "IncludeRemoteBranches";
            IncludeRemoteBranches.Size = new Size(180, 19);
            IncludeRemoteBranches.TabIndex = 3;
            IncludeRemoteBranches.Text = "Delete remote branches from";
            IncludeRemoteBranches.UseVisualStyleBackColor = true;
            IncludeRemoteBranches.CheckedChanged += ClearResults;
            // 
            // _NO_TRANSLATE_Remote
            // 
            _NO_TRANSLATE_Remote.Location = new Point(380, 63);
            _NO_TRANSLATE_Remote.Name = "_NO_TRANSLATE_Remote";
            _NO_TRANSLATE_Remote.Size = new Size(218, 23);
            _NO_TRANSLATE_Remote.TabIndex = 4;
            _NO_TRANSLATE_Remote.Text = "origin";
            _NO_TRANSLATE_Remote.TextChanged += ClearResults;
            // 
            // useRegexFilter
            // 
            useRegexFilter.AutoSize = true;
            useRegexFilter.Location = new Point(3, 93);
            useRegexFilter.Name = "useRegexFilter";
            useRegexFilter.Size = new Size(168, 19);
            useRegexFilter.TabIndex = 5;
            useRegexFilter.Text = "Use regex to filter branches";
            useRegexFilter.UseVisualStyleBackColor = true;
            useRegexFilter.CheckedChanged += ClearResults;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 5);
            label1.Margin = new Padding(3, 5, 3, 0);
            label1.Name = "label1";
            label1.Size = new Size(183, 15);
            label1.TabIndex = 9;
            label1.Text = "Delete branches older than x days";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 35);
            label2.Margin = new Padding(3, 5, 3, 0);
            label2.Name = "label2";
            label2.Size = new Size(225, 15);
            label2.TabIndex = 10;
            label2.Text = "Delete branches fully merged into branch";
            // 
            // mergedIntoBranch
            // 
            mergedIntoBranch.Location = new Point(380, 33);
            mergedIntoBranch.Name = "mergedIntoBranch";
            mergedIntoBranch.Size = new Size(218, 23);
            mergedIntoBranch.TabIndex = 12;
            mergedIntoBranch.TextChanged += ClearResults;
            // 
            // olderThanDays
            // 
            olderThanDays.Location = new Point(380, 3);
            olderThanDays.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            olderThanDays.Name = "olderThanDays";
            olderThanDays.Size = new Size(120, 23);
            olderThanDays.TabIndex = 13;
            olderThanDays.ValueChanged += ClearResults;
            // 
            // RefreshBtn
            // 
            RefreshBtn.Location = new Point(380, 213);
            RefreshBtn.Name = "RefreshBtn";
            RefreshBtn.Size = new Size(124, 23);
            RefreshBtn.TabIndex = 7;
            RefreshBtn.Text = "Search branches";
            RefreshBtn.UseVisualStyleBackColor = true;
            RefreshBtn.Click += Refresh_Click;
            // 
            // includeUnmergedBranches
            // 
            includeUnmergedBranches.AutoSize = true;
            includeUnmergedBranches.Location = new Point(3, 183);
            includeUnmergedBranches.Name = "includeUnmergedBranches";
            includeUnmergedBranches.Size = new Size(174, 19);
            includeUnmergedBranches.TabIndex = 14;
            includeUnmergedBranches.Text = "Include unmerged branches";
            includeUnmergedBranches.UseVisualStyleBackColor = true;
            includeUnmergedBranches.CheckedChanged += includeUnmergedBranches_CheckedChanged;
            // 
            // useRegexCaseInsensitive
            // 
            useRegexCaseInsensitive.AutoSize = true;
            useRegexCaseInsensitive.Location = new Point(380, 123);
            useRegexCaseInsensitive.Name = "useRegexCaseInsensitive";
            useRegexCaseInsensitive.Size = new Size(109, 19);
            useRegexCaseInsensitive.TabIndex = 15;
            useRegexCaseInsensitive.Text = "Case insensitive";
            useRegexCaseInsensitive.UseVisualStyleBackColor = true;
            // 
            // regexDoesNotMatch
            // 
            regexDoesNotMatch.AutoSize = true;
            regexDoesNotMatch.Location = new Point(380, 153);
            regexDoesNotMatch.Name = "regexDoesNotMatch";
            regexDoesNotMatch.Size = new Size(110, 19);
            regexDoesNotMatch.TabIndex = 16;
            regexDoesNotMatch.Text = "Does not match";
            regexDoesNotMatch.UseVisualStyleBackColor = true;
            // 
            // DeleteUnusedBranchesForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = Cancel;
            ClientSize = new Size(760, 500);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(statusStrip1);
            MinimumSize = new Size(600, 400);
            Name = "DeleteUnusedBranchesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Delete obsolete branches";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            pnlBranchesArea.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(imgLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(BranchesGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(branchBindingSource)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(olderThanDays)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DataGridView BranchesGrid;
        private BindingSource branchBindingSource;
        private TableLayoutPanel tableLayoutPanel1;
        private Label instructionLabel;
        private TableLayoutPanel tableLayoutPanel2;
        private Button Cancel;
        private Button Delete;
        private Button buttonSettings;
        private CheckBox IncludeRemoteBranches;
        private TextBox _NO_TRANSLATE_Remote;
        private TableLayoutPanel tableLayoutPanel3;
        private TextBox regexFilter;
        private CheckBox useRegexFilter;
        private Button RefreshBtn;
        private Label label1;
        private Label label2;
        private TextBox mergedIntoBranch;
        private NumericUpDown olderThanDays;
        private CheckBox includeUnmergedBranches;
        private PictureBox imgLoading;
        private Panel pnlBranchesArea;
        private ToolStripStatusLabel lblStatus;
        private DataGridViewCheckBoxColumn _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn;
        private DeleteUnusedBranches.DataGridViewCheckBoxHeaderCell checkBoxHeaderCell;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn Author;
        private DataGridViewTextBoxColumn Message;
        private CheckBox useRegexCaseInsensitive;
        private CheckBox regexDoesNotMatch;
    }
}
