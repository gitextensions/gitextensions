namespace GitUI.CommandsDialogs
{
    partial class FormReflog
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
            Branches = new ComboBox();
            label2 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            branchesPanel = new FlowLayoutPanel();
            currentBranchPanel = new FlowLayoutPanel();
            label1 = new Label();
            linkCurrentBranch = new LinkLabel();
            linkHead = new LinkLabel();
            gridReflog = new DataGridView();
            Sha = new DataGridViewTextBoxColumn();
            Ref = new DataGridViewTextBoxColumn();
            Action = new DataGridViewTextBoxColumn();
            contextMenuStripReflog = new ContextMenuStrip(components);
            copySha1ToolStripMenuItem = new ToolStripMenuItem();
            createABranchOnThisCommitToolStripMenuItem = new ToolStripMenuItem();
            resetCurrentBranchOnThisCommitToolStripMenuItem = new ToolStripMenuItem();
            lblDirtyWorkingDirectory = new Label();
            tableLayoutPanel1.SuspendLayout();
            branchesPanel.SuspendLayout();
            currentBranchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(gridReflog)).BeginInit();
            contextMenuStripReflog.SuspendLayout();
            SuspendLayout();
            // 
            // Branches
            // 
            Branches.Anchor = AnchorStyles.Left;
            Branches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Branches.AutoCompleteSource = AutoCompleteSource.ListItems;
            Branches.DropDownStyle = ComboBoxStyle.DropDownList;
            Branches.FormattingEnabled = true;
            Branches.Location = new Point(86, 4);
            Branches.Margin = new Padding(4);
            Branches.Name = "Branches";
            Branches.Size = new Size(272, 24);
            Branches.TabIndex = 6;
            Branches.SelectedIndexChanged += Branches_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(4, 7);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(74, 17);
            label2.TabIndex = 5;
            label2.Text = "Reference:";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(branchesPanel, 0, 1);
            tableLayoutPanel1.Controls.Add(currentBranchPanel, 1, 1);
            tableLayoutPanel1.Controls.Add(gridReflog, 0, 2);
            tableLayoutPanel1.Controls.Add(lblDirtyWorkingDirectory, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(782, 555);
            tableLayoutPanel1.TabIndex = 19;
            // 
            // branchesPanel
            // 
            branchesPanel.AutoSize = true;
            branchesPanel.Controls.Add(label2);
            branchesPanel.Controls.Add(Branches);
            branchesPanel.Dock = DockStyle.Fill;
            branchesPanel.Location = new Point(4, 38);
            branchesPanel.Margin = new Padding(4);
            branchesPanel.Name = "branchesPanel";
            branchesPanel.Size = new Size(362, 32);
            branchesPanel.TabIndex = 32;
            // 
            // currentBranchPanel
            // 
            currentBranchPanel.Controls.Add(label1);
            currentBranchPanel.Controls.Add(linkCurrentBranch);
            currentBranchPanel.Controls.Add(linkHead);
            currentBranchPanel.Dock = DockStyle.Fill;
            currentBranchPanel.Location = new Point(374, 38);
            currentBranchPanel.Margin = new Padding(4);
            currentBranchPanel.Name = "currentBranchPanel";
            currentBranchPanel.Size = new Size(453, 32);
            currentBranchPanel.TabIndex = 8;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(4, 7);
            label1.Margin = new Padding(4, 7, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(115, 17);
            label1.TabIndex = 7;
            label1.Text = "Display reflog for:";
            // 
            // linkCurrentBranch
            // 
            linkCurrentBranch.AutoSize = true;
            linkCurrentBranch.Location = new Point(126, 7);
            linkCurrentBranch.Margin = new Padding(3, 7, 3, 0);
            linkCurrentBranch.Name = "linkCurrentBranch";
            linkCurrentBranch.Size = new Size(150, 17);
            linkCurrentBranch.TabIndex = 8;
            linkCurrentBranch.TabStop = true;
            linkCurrentBranch.Text = "(Current branch name)";
            linkCurrentBranch.TextAlign = ContentAlignment.MiddleLeft;
            linkCurrentBranch.LinkClicked += linkCurrentBranch_LinkClicked;
            // 
            // linkHead
            // 
            linkHead.AutoSize = true;
            linkHead.Location = new Point(282, 7);
            linkHead.Margin = new Padding(3, 7, 3, 0);
            linkHead.Name = "linkHead";
            linkHead.Size = new Size(43, 17);
            linkHead.TabIndex = 8;
            linkHead.TabStop = true;
            linkHead.Text = "HEAD";
            linkHead.Click += linkHead_Click;
            // 
            // gridReflog
            // 
            gridReflog.AllowUserToAddRows = false;
            gridReflog.AllowUserToDeleteRows = false;
            gridReflog.AllowUserToResizeRows = false;
            gridReflog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridReflog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            gridReflog.Columns.AddRange(new DataGridViewColumn[] {
            Sha,
            Ref,
            Action});
            tableLayoutPanel1.SetColumnSpan(gridReflog, 2);
            gridReflog.ContextMenuStrip = contextMenuStripReflog;
            gridReflog.Location = new Point(3, 77);
            gridReflog.MultiSelect = false;
            gridReflog.Name = "gridReflog";
            gridReflog.ReadOnly = true;
            gridReflog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridReflog.Size = new Size(825, 509);
            gridReflog.TabIndex = 33;
            gridReflog.MouseClick += gridReflog_MouseClick;
            gridReflog.MouseMove += gridReflog_MouseMove;
            // 
            // Sha
            // 
            Sha.HeaderText = "SHA-1";
            Sha.Name = "Sha";
            Sha.ReadOnly = true;
            Sha.Width = 5;
            // 
            // Ref
            // 
            Ref.HeaderText = "Ref";
            Ref.Name = "Ref";
            Ref.ReadOnly = true;
            Ref.Width = 5;
            // 
            // Action
            // 
            Action.HeaderText = "Action";
            Action.Name = "Action";
            Action.ReadOnly = true;
            Action.Width = 5;
            // 
            // contextMenuStripReflog
            // 
            contextMenuStripReflog.Items.AddRange(new ToolStripItem[] {
            copySha1ToolStripMenuItem,
            createABranchOnThisCommitToolStripMenuItem,
            resetCurrentBranchOnThisCommitToolStripMenuItem});
            contextMenuStripReflog.Name = "contextMenuStripReflog";
            contextMenuStripReflog.Size = new Size(323, 82);
            // 
            // copySha1ToolStripMenuItem
            // 
            copySha1ToolStripMenuItem.Image = Properties.Images.CommitId;
            copySha1ToolStripMenuItem.Name = "copySha1ToolStripMenuItem";
            copySha1ToolStripMenuItem.Size = new Size(322, 26);
            copySha1ToolStripMenuItem.Text = "Copy SHA-1";
            copySha1ToolStripMenuItem.Click += copySha1ToolStripMenuItem_Click;
            // 
            // createABranchOnThisCommitToolStripMenuItem
            // 
            createABranchOnThisCommitToolStripMenuItem.Image = Properties.Images.BranchCreate;
            createABranchOnThisCommitToolStripMenuItem.Name = "createABranchOnThisCommitToolStripMenuItem";
            createABranchOnThisCommitToolStripMenuItem.Size = new Size(322, 26);
            createABranchOnThisCommitToolStripMenuItem.Text = "Create a branch on this commit...";
            createABranchOnThisCommitToolStripMenuItem.Click += createABranchOnThisCommitToolStripMenuItem_Click;
            // 
            // resetCurrentBranchOnThisCommitToolStripMenuItem
            // 
            resetCurrentBranchOnThisCommitToolStripMenuItem.Image = Properties.Images.ResetCurrentBranchToHere;
            resetCurrentBranchOnThisCommitToolStripMenuItem.Name = "resetCurrentBranchOnThisCommitToolStripMenuItem";
            resetCurrentBranchOnThisCommitToolStripMenuItem.Size = new Size(322, 26);
            resetCurrentBranchOnThisCommitToolStripMenuItem.Text = "Reset current branch to this commit...";
            resetCurrentBranchOnThisCommitToolStripMenuItem.Click += resetCurrentBranchOnThisCommitToolStripMenuItem_Click;
            // 
            // lblDirtyWorkingDirectory
            // 
            lblDirtyWorkingDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblDirtyWorkingDirectory.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(lblDirtyWorkingDirectory, 2);
            lblDirtyWorkingDirectory.ForeColor = Color.Red;
            lblDirtyWorkingDirectory.Location = new Point(3, 0);
            lblDirtyWorkingDirectory.Name = "lblDirtyWorkingDirectory";
            lblDirtyWorkingDirectory.Size = new Size(825, 34);
            lblDirtyWorkingDirectory.TabIndex = 34;
            lblDirtyWorkingDirectory.Text = "Warning: you\'ve got changes in your working directory that could be lost if you w" +
    "ant to reset the current branch to another commit.\r\nStash them before if you don" +
    "\'t want to lose them.";
            lblDirtyWorkingDirectory.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormReflog
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(782, 555);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4);
            MinimizeBox = false;
            MinimumSize = new Size(400, 200);
            Name = "FormReflog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reflog";
            Load += FormReflog_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            branchesPanel.ResumeLayout(false);
            branchesPanel.PerformLayout();
            currentBranchPanel.ResumeLayout(false);
            currentBranchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(gridReflog)).EndInit();
            contextMenuStripReflog.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private Label label2;
        private ComboBox Branches;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel branchesPanel;
        private DataGridView gridReflog;
        private DataGridViewTextBoxColumn Sha;
        private DataGridViewTextBoxColumn Ref;
        private DataGridViewTextBoxColumn Action;
        private ContextMenuStrip contextMenuStripReflog;
        private ToolStripMenuItem createABranchOnThisCommitToolStripMenuItem;
        private ToolStripMenuItem resetCurrentBranchOnThisCommitToolStripMenuItem;
        private ToolStripMenuItem copySha1ToolStripMenuItem;
        private FlowLayoutPanel currentBranchPanel;
        private Label label1;
        private LinkLabel linkCurrentBranch;
        private LinkLabel linkHead;
        private Label lblDirtyWorkingDirectory;
    }
}