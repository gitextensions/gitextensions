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
            Revert = new Button();
            btnAbort = new Button();
            ParentsLabel = new Label();
            lvParentsList = new UserControls.NativeListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            AutoCommit = new CheckBox();
            BranchInfo = new Label();
            commitSummaryUserControl1 = new UserControls.CommitSummaryUserControl();
            tlPnlMain = new TableLayoutPanel();
            ControlsPanel.SuspendLayout();
            MainPanel.SuspendLayout();
            tlPnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.AutoSize = true;
            MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MainPanel.Controls.Add(tlPnlMain);
            MainPanel.Size = new Size(614, 311);
            MainPanel.TabIndex = 0;
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(btnAbort);
            ControlsPanel.Controls.Add(Revert);
            ControlsPanel.Location = new Point(0, 379);
            ControlsPanel.Size = new Size(614, 41);
            ControlsPanel.TabIndex = 1;
            // 
            // Revert
            // 
            Revert.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Revert.Location = new Point(403, 8);
            Revert.Name = "Revert";
            Revert.Size = new Size(117, 25);
            Revert.TabIndex = 0;
            Revert.Text = "&Revert this commit";
            Revert.UseVisualStyleBackColor = true;
            Revert.Click += Revert_Click;
            // 
            // btnAbort
            // 
            btnAbort.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAbort.Location = new Point(526, 8);
            btnAbort.Name = "btnAbort";
            btnAbort.Size = new Size(75, 25);
            btnAbort.TabIndex = 1;
            btnAbort.Text = "A&bort";
            btnAbort.UseVisualStyleBackColor = true;
            btnAbort.Click += btnAbort_Click;
            // 
            // ParentsLabel
            // 
            ParentsLabel.AutoSize = true;
            ParentsLabel.Dock = DockStyle.Fill;
            ParentsLabel.Location = new Point(3, 181);
            ParentsLabel.Name = "ParentsLabel";
            ParentsLabel.Size = new Size(584, 15);
            ParentsLabel.TabIndex = 3;
            ParentsLabel.Text = "This commit is a merge, select &parent:";
            // 
            // lvParentsList
            // 
            lvParentsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            lvParentsList.Dock = DockStyle.Fill;
            lvParentsList.FullRowSelect = true;
            lvParentsList.Location = new Point(6, 202);
            lvParentsList.Margin = new Padding(6);
            lvParentsList.MultiSelect = false;
            lvParentsList.Name = "lvParentsList";
            lvParentsList.Size = new Size(578, 54);
            lvParentsList.TabIndex = 4;
            lvParentsList.UseCompatibleStateImageBehavior = false;
            lvParentsList.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "No.";
            columnHeader1.Width = 43;
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
            AutoCommit.AutoSize = true;
            AutoCommit.Dock = DockStyle.Fill;
            AutoCommit.Location = new Point(3, 265);
            AutoCommit.Name = "AutoCommit";
            AutoCommit.Size = new Size(584, 19);
            AutoCommit.TabIndex = 5;
            AutoCommit.Text = "&Automatically create a commit";
            AutoCommit.UseVisualStyleBackColor = true;
            // 
            // BranchInfo
            // 
            BranchInfo.AutoSize = true;
            BranchInfo.Dock = DockStyle.Fill;
            BranchInfo.Location = new Point(3, 0);
            BranchInfo.Name = "BranchInfo";
            BranchInfo.Size = new Size(584, 15);
            BranchInfo.TabIndex = 1;
            BranchInfo.Text = "Revert this commit:";
            // 
            // commitSummaryUserControl1
            // 
            commitSummaryUserControl1.AutoSize = true;
            commitSummaryUserControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitSummaryUserControl1.Dock = DockStyle.Fill;
            commitSummaryUserControl1.Location = new Point(16, 18);
            commitSummaryUserControl1.Margin = new Padding(16, 3, 3, 3);
            commitSummaryUserControl1.MinimumSize = new Size(440, 160);
            commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            commitSummaryUserControl1.Revision = null;
            commitSummaryUserControl1.Size = new Size(571, 160);
            commitSummaryUserControl1.TabIndex = 2;
            commitSummaryUserControl1.TabStop = false;
            // 
            // tlPnlMain
            // 
            tlPnlMain.AutoSize = true;
            tlPnlMain.ColumnCount = 1;
            tlPnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlPnlMain.Controls.Add(BranchInfo, 0, 0);
            tlPnlMain.Controls.Add(commitSummaryUserControl1, 0, 1);
            tlPnlMain.Controls.Add(ParentsLabel, 0, 2);
            tlPnlMain.Controls.Add(lvParentsList, 0, 3);
            tlPnlMain.Controls.Add(AutoCommit, 0, 4);
            tlPnlMain.Dock = DockStyle.Fill;
            tlPnlMain.Location = new Point(12, 12);
            tlPnlMain.Margin = new Padding(0);
            tlPnlMain.Name = "tlPnlMain";
            tlPnlMain.RowCount = 5;
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.Size = new Size(590, 287);
            tlPnlMain.TabIndex = 0;
            // 
            // FormRevertCommit
            // 
            AcceptButton = Revert;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            CancelButton = btnAbort;
            ClientSize = new Size(614, 352);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(630, 362);
            Name = "FormRevertCommit";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Revert commit";
            Load += Form_Load;
            Shown += Form_Shown;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            tlPnlMain.ResumeLayout(false);
            tlPnlMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label BranchInfo;
        private Button Revert;
        private Button btnAbort;
        private CheckBox AutoCommit;
        private Label ParentsLabel;
        private UserControls.NativeListView lvParentsList;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private TableLayoutPanel tlPnlMain;
    }
}
