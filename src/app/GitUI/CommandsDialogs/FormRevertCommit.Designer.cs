namespace GitUI.CommandsDialogs
{
    partial class FormRevertCommit
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
            commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            ParentsLabel = new Label();
            ParentsList = new UserControls.NativeListView();
            columnHeader1 = ((ColumnHeader)(new ColumnHeader()));
            columnHeader2 = ((ColumnHeader)(new ColumnHeader()));
            columnHeader3 = ((ColumnHeader)(new ColumnHeader()));
            columnHeader4 = ((ColumnHeader)(new ColumnHeader()));
            AutoCommit = new CheckBox();
            Revert = new Button();
            BranchInfo = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            parentListPanel = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            parentListPanel.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // commitSummaryUserControl1
            // 
            commitSummaryUserControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            commitSummaryUserControl1.AutoSize = true;
            commitSummaryUserControl1.Location = new Point(20, 27);
            commitSummaryUserControl1.Margin = new Padding(20, 4, 4, 4);
            commitSummaryUserControl1.MinimumSize = new Size(550, 200);
            commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            commitSummaryUserControl1.Revision = null;
            commitSummaryUserControl1.Size = new Size(658, 200);
            commitSummaryUserControl1.TabIndex = 16;
            // 
            // ParentsLabel
            // 
            ParentsLabel.AutoSize = true;
            ParentsLabel.Location = new Point(4, 0);
            ParentsLabel.Margin = new Padding(4, 0, 4, 0);
            ParentsLabel.Name = "ParentsLabel";
            ParentsLabel.Size = new Size(298, 23);
            ParentsLabel.TabIndex = 14;
            ParentsLabel.Text = "This commit is a merge, select &parent:";
            // 
            // ParentsList
            // 
            ParentsList.BorderStyle = BorderStyle.FixedSingle;
            ParentsList.Columns.AddRange(new ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            columnHeader3,
            columnHeader4});
            ParentsList.Dock = DockStyle.Fill;
            ParentsList.FullRowSelect = true;
            ParentsList.HideSelection = false;
            ParentsList.Location = new Point(4, 27);
            ParentsList.Margin = new Padding(4);
            ParentsList.MultiSelect = false;
            ParentsList.Name = "ParentsList";
            ParentsList.Size = new Size(668, 75);
            ParentsList.TabIndex = 15;
            ParentsList.UseCompatibleStateImageBehavior = false;
            ParentsList.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "No.";
            columnHeader1.Width = 50;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Message";
            columnHeader2.Width = 291;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Author";
            columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Date";
            columnHeader4.Width = 80;
            // 
            // AutoCommit
            // 
            AutoCommit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AutoCommit.AutoSize = true;
            AutoCommit.Location = new Point(4, 10);
            AutoCommit.Margin = new Padding(4);
            AutoCommit.Name = "AutoCommit";
            AutoCommit.Size = new Size(265, 27);
            AutoCommit.TabIndex = 11;
            AutoCommit.Text = "&Automatically create a commit";
            AutoCommit.UseVisualStyleBackColor = true;
            // 
            // Revert
            // 
            Revert.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Revert.AutoSize = true;
            Revert.Location = new Point(501, 4);
            Revert.Margin = new Padding(4);
            Revert.Name = "Revert";
            Revert.Size = new Size(171, 33);
            Revert.TabIndex = 10;
            Revert.Text = "&Revert this commit";
            Revert.UseVisualStyleBackColor = true;
            Revert.Click += Revert_Click;
            // 
            // BranchInfo
            // 
            BranchInfo.AutoSize = true;
            BranchInfo.Location = new Point(4, 0);
            BranchInfo.Margin = new Padding(4, 0, 4, 0);
            BranchInfo.Name = "BranchInfo";
            BranchInfo.Size = new Size(157, 23);
            BranchInfo.TabIndex = 5;
            BranchInfo.Text = "Revert this commit:";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(BranchInfo, 0, 0);
            tableLayoutPanel1.Controls.Add(commitSummaryUserControl1, 0, 1);
            tableLayoutPanel1.Controls.Add(parentListPanel, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(682, 390);
            tableLayoutPanel1.TabIndex = 17;
            // 
            // parentListPanel
            // 
            parentListPanel.AutoSize = true;
            parentListPanel.ColumnCount = 1;
            parentListPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            parentListPanel.Controls.Add(ParentsLabel, 0, 0);
            parentListPanel.Controls.Add(ParentsList, 0, 1);
            parentListPanel.Dock = DockStyle.Fill;
            parentListPanel.Location = new Point(3, 234);
            parentListPanel.Name = "parentListPanel";
            parentListPanel.RowCount = 2;
            parentListPanel.RowStyles.Add(new RowStyle());
            parentListPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            parentListPanel.Size = new Size(676, 106);
            parentListPanel.TabIndex = 17;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(Revert, 1, 0);
            tableLayoutPanel3.Controls.Add(AutoCommit, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 346);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(676, 41);
            tableLayoutPanel3.TabIndex = 18;
            // 
            // FormRevertCommit
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(682, 390);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(700, 330);
            Name = "FormRevertCommit";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Revert commit";
            Load += FormRevertCommit_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            parentListPanel.ResumeLayout(false);
            parentListPanel.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Button Revert;
        private CheckBox AutoCommit;
        private Label ParentsLabel;
        private UserControls.NativeListView ParentsList;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private Label BranchInfo;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel parentListPanel;
        private TableLayoutPanel tableLayoutPanel3;
    }
}
