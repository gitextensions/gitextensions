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
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.branchesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.currentBranchPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.linkCurrentBranch = new System.Windows.Forms.LinkLabel();
            this.linkHead = new System.Windows.Forms.LinkLabel();
            this.gridReflog = new System.Windows.Forms.DataGridView();
            this.Sha = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Ref = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Action = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripReflog = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copySha1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createABranchOnThisCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCurrentBranchOnThisCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblDirtyWorkingDirectory = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.branchesPanel.SuspendLayout();
            this.currentBranchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReflog)).BeginInit();
            this.contextMenuStripReflog.SuspendLayout();
            this.SuspendLayout();
            // 
            // Branches
            // 
            this.Branches.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(86, 4);
            this.Branches.Margin = new System.Windows.Forms.Padding(4);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(272, 24);
            this.Branches.TabIndex = 6;
            this.Branches.SelectedIndexChanged += new System.EventHandler(this.Branches_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Reference:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.branchesPanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.currentBranchPanel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.gridReflog, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblDirtyWorkingDirectory, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(782, 555);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // branchesPanel
            // 
            this.branchesPanel.AutoSize = true;
            this.branchesPanel.Controls.Add(this.label2);
            this.branchesPanel.Controls.Add(this.Branches);
            this.branchesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchesPanel.Location = new System.Drawing.Point(4, 38);
            this.branchesPanel.Margin = new System.Windows.Forms.Padding(4);
            this.branchesPanel.Name = "branchesPanel";
            this.branchesPanel.Size = new System.Drawing.Size(362, 32);
            this.branchesPanel.TabIndex = 32;
            // 
            // currentBranchPanel
            // 
            this.currentBranchPanel.Controls.Add(this.label1);
            this.currentBranchPanel.Controls.Add(this.linkCurrentBranch);
            this.currentBranchPanel.Controls.Add(this.linkHead);
            this.currentBranchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.currentBranchPanel.Location = new System.Drawing.Point(374, 38);
            this.currentBranchPanel.Margin = new System.Windows.Forms.Padding(4);
            this.currentBranchPanel.Name = "currentBranchPanel";
            this.currentBranchPanel.Size = new System.Drawing.Size(453, 32);
            this.currentBranchPanel.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 7, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Display reflog for:";
            // 
            // linkCurrentBranch
            // 
            this.linkCurrentBranch.AutoSize = true;
            this.linkCurrentBranch.Location = new System.Drawing.Point(126, 7);
            this.linkCurrentBranch.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.linkCurrentBranch.Name = "linkCurrentBranch";
            this.linkCurrentBranch.Size = new System.Drawing.Size(150, 17);
            this.linkCurrentBranch.TabIndex = 8;
            this.linkCurrentBranch.TabStop = true;
            this.linkCurrentBranch.Text = "(Current branch name)";
            this.linkCurrentBranch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkCurrentBranch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCurrentBranch_LinkClicked);
            // 
            // linkHead
            // 
            this.linkHead.AutoSize = true;
            this.linkHead.Location = new System.Drawing.Point(282, 7);
            this.linkHead.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.linkHead.Name = "linkHead";
            this.linkHead.Size = new System.Drawing.Size(43, 17);
            this.linkHead.TabIndex = 8;
            this.linkHead.TabStop = true;
            this.linkHead.Text = "HEAD";
            this.linkHead.Click += new System.EventHandler(this.linkHead_Click);
            // 
            // gridReflog
            // 
            this.gridReflog.AllowUserToAddRows = false;
            this.gridReflog.AllowUserToDeleteRows = false;
            this.gridReflog.AllowUserToResizeRows = false;
            this.gridReflog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridReflog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            this.gridReflog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Sha,
            this.Ref,
            this.Action});
            this.tableLayoutPanel1.SetColumnSpan(this.gridReflog, 2);
            this.gridReflog.ContextMenuStrip = this.contextMenuStripReflog;
            this.gridReflog.Location = new System.Drawing.Point(3, 77);
            this.gridReflog.MultiSelect = false;
            this.gridReflog.Name = "gridReflog";
            this.gridReflog.ReadOnly = true;
            this.gridReflog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridReflog.Size = new System.Drawing.Size(825, 509);
            this.gridReflog.TabIndex = 33;
            this.gridReflog.MouseClick += new System.Windows.Forms.MouseEventHandler(this.gridReflog_MouseClick);
            this.gridReflog.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gridReflog_MouseMove);
            // 
            // Sha
            // 
            this.Sha.HeaderText = "SHA-1";
            this.Sha.Name = "Sha";
            this.Sha.ReadOnly = true;
            this.Sha.Width = 5;
            // 
            // Ref
            // 
            this.Ref.HeaderText = "Ref";
            this.Ref.Name = "Ref";
            this.Ref.ReadOnly = true;
            this.Ref.Width = 5;
            // 
            // Action
            // 
            this.Action.HeaderText = "Action";
            this.Action.Name = "Action";
            this.Action.ReadOnly = true;
            this.Action.Width = 5;
            // 
            // contextMenuStripReflog
            // 
            this.contextMenuStripReflog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copySha1ToolStripMenuItem,
            this.createABranchOnThisCommitToolStripMenuItem,
            this.resetCurrentBranchOnThisCommitToolStripMenuItem});
            this.contextMenuStripReflog.Name = "contextMenuStripReflog";
            this.contextMenuStripReflog.Size = new System.Drawing.Size(323, 82);
            // 
            // copySha1ToolStripMenuItem
            // 
            this.copySha1ToolStripMenuItem.Image = global::GitUI.Properties.Images.CommitId;
            this.copySha1ToolStripMenuItem.Name = "copySha1ToolStripMenuItem";
            this.copySha1ToolStripMenuItem.Size = new System.Drawing.Size(322, 26);
            this.copySha1ToolStripMenuItem.Text = "Copy SHA-1";
            this.copySha1ToolStripMenuItem.Click += new System.EventHandler(this.copySha1ToolStripMenuItem_Click);
            // 
            // createABranchOnThisCommitToolStripMenuItem
            // 
            this.createABranchOnThisCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchCreate;
            this.createABranchOnThisCommitToolStripMenuItem.Name = "createABranchOnThisCommitToolStripMenuItem";
            this.createABranchOnThisCommitToolStripMenuItem.Size = new System.Drawing.Size(322, 26);
            this.createABranchOnThisCommitToolStripMenuItem.Text = "Create a branch on this commit...";
            this.createABranchOnThisCommitToolStripMenuItem.Click += new System.EventHandler(this.createABranchOnThisCommitToolStripMenuItem_Click);
            // 
            // resetCurrentBranchOnThisCommitToolStripMenuItem
            // 
            this.resetCurrentBranchOnThisCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetCurrentBranchToHere;
            this.resetCurrentBranchOnThisCommitToolStripMenuItem.Name = "resetCurrentBranchOnThisCommitToolStripMenuItem";
            this.resetCurrentBranchOnThisCommitToolStripMenuItem.Size = new System.Drawing.Size(322, 26);
            this.resetCurrentBranchOnThisCommitToolStripMenuItem.Text = "Reset current branch to this commit...";
            this.resetCurrentBranchOnThisCommitToolStripMenuItem.Click += new System.EventHandler(this.resetCurrentBranchOnThisCommitToolStripMenuItem_Click);
            // 
            // lblDirtyWorkingDirectory
            // 
            this.lblDirtyWorkingDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDirtyWorkingDirectory.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblDirtyWorkingDirectory, 2);
            this.lblDirtyWorkingDirectory.ForeColor = System.Drawing.Color.Red;
            this.lblDirtyWorkingDirectory.Location = new System.Drawing.Point(3, 0);
            this.lblDirtyWorkingDirectory.Name = "lblDirtyWorkingDirectory";
            this.lblDirtyWorkingDirectory.Size = new System.Drawing.Size(825, 34);
            this.lblDirtyWorkingDirectory.TabIndex = 34;
            this.lblDirtyWorkingDirectory.Text = "Warning: you\'ve got changes in your working directory that could be lost if you w" +
    "ant to reset the current branch to another commit.\r\nStash them before if you don" +
    "\'t want to lose them.";
            this.lblDirtyWorkingDirectory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormReflog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(782, 555);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "FormReflog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reflog";
            this.Load += new System.EventHandler(this.FormReflog_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.branchesPanel.ResumeLayout(false);
            this.branchesPanel.PerformLayout();
            this.currentBranchPanel.ResumeLayout(false);
            this.currentBranchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridReflog)).EndInit();
            this.contextMenuStripReflog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel branchesPanel;
        private System.Windows.Forms.DataGridView gridReflog;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sha;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ref;
        private System.Windows.Forms.DataGridViewTextBoxColumn Action;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripReflog;
        private System.Windows.Forms.ToolStripMenuItem createABranchOnThisCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCurrentBranchOnThisCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copySha1ToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel currentBranchPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkCurrentBranch;
        private System.Windows.Forms.LinkLabel linkHead;
        private System.Windows.Forms.Label lblDirtyWorkingDirectory;
    }
}