namespace GitUI.CommandsDialogs
{
    partial class FormCherryPick
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
            btnPick = new Button();
            btnAbort = new Button();
            btnChooseRevision = new Button();
            lblParents = new Label();
            lvParentsList = new UserControls.NativeListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            cbxAddReference = new CheckBox();
            cbxAutoCommit = new CheckBox();
            lblBranchInfo = new Label();
            commitSummaryUserControl1 = new UserControls.CommitSummaryUserControl();
            tlPnlMain = new TableLayoutPanel();
            chooseRevPanel = new FlowLayoutPanel();
            lblAnotherRev = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlPnlMain.SuspendLayout();
            chooseRevPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.AutoSize = true;
            MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MainPanel.Controls.Add(tlPnlMain);
            MainPanel.Size = new Size(614, 372);
            MainPanel.TabIndex = 0;
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(btnAbort);
            ControlsPanel.Controls.Add(btnPick);
            ControlsPanel.Location = new Point(0, 372);
            ControlsPanel.Size = new Size(614, 41);
            ControlsPanel.TabIndex = 1;
            // 
            // btnPick
            // 
            btnPick.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPick.Location = new Point(411, 8);
            btnPick.Name = "btnPick";
            btnPick.Size = new Size(109, 25);
            btnPick.TabIndex = 0;
            btnPick.Text = "&Cherry pick";
            btnPick.UseVisualStyleBackColor = true;
            btnPick.Click += btnPick_Click;
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
            // btnChooseRevision
            // 
            btnChooseRevision.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChooseRevision.Image = Properties.Images.SelectRevision;
            btnChooseRevision.Location = new Point(556, 3);
            btnChooseRevision.Name = "btnChooseRevision";
            btnChooseRevision.Size = new Size(25, 24);
            btnChooseRevision.TabIndex = 5;
            btnChooseRevision.UseVisualStyleBackColor = true;
            btnChooseRevision.Click += btnChooseRevision_Click;
            // 
            // lblParents
            // 
            lblParents.AutoSize = true;
            lblParents.Dock = DockStyle.Fill;
            lblParents.Location = new Point(3, 217);
            lblParents.Name = "lblParents";
            lblParents.Size = new Size(584, 15);
            lblParents.TabIndex = 6;
            lblParents.Text = "This commit is a merge, select &parent:";
            // 
            // lvParentsList
            // 
            lvParentsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            lvParentsList.Dock = DockStyle.Fill;
            lvParentsList.FullRowSelect = true;
            lvParentsList.Location = new Point(6, 238);
            lvParentsList.Margin = new Padding(6);
            lvParentsList.MultiSelect = false;
            lvParentsList.Name = "lvParentsList";
            lvParentsList.Size = new Size(578, 54);
            lvParentsList.TabIndex = 7;
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
            // cbxAddReference
            // 
            cbxAddReference.AutoSize = true;
            cbxAddReference.Dock = DockStyle.Fill;
            cbxAddReference.Location = new Point(3, 326);
            cbxAddReference.Name = "cbxAddReference";
            cbxAddReference.Size = new Size(584, 19);
            cbxAddReference.TabIndex = 9;
            cbxAddReference.Text = "A&dd commit reference to commit message";
            cbxAddReference.UseVisualStyleBackColor = true;
            // 
            // cbxAutoCommit
            // 
            cbxAutoCommit.AutoSize = true;
            cbxAutoCommit.Dock = DockStyle.Fill;
            cbxAutoCommit.Location = new Point(3, 301);
            cbxAutoCommit.Name = "cbxAutoCommit";
            cbxAutoCommit.Size = new Size(584, 19);
            cbxAutoCommit.TabIndex = 8;
            cbxAutoCommit.Text = "&Automatically create a commit";
            cbxAutoCommit.UseVisualStyleBackColor = true;
            // 
            // lblBranchInfo
            // 
            lblBranchInfo.AutoSize = true;
            lblBranchInfo.Dock = DockStyle.Fill;
            lblBranchInfo.Location = new Point(3, 0);
            lblBranchInfo.Name = "lblBranchInfo";
            lblBranchInfo.Size = new Size(584, 15);
            lblBranchInfo.TabIndex = 1;
            lblBranchInfo.Text = "Cherry pick this commit:";
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
            tlPnlMain.Controls.Add(lblBranchInfo, 0, 0);
            tlPnlMain.Controls.Add(commitSummaryUserControl1, 0, 1);
            tlPnlMain.Controls.Add(chooseRevPanel, 0, 2);
            tlPnlMain.Controls.Add(lblParents, 0, 3);
            tlPnlMain.Controls.Add(lvParentsList, 0, 4);
            tlPnlMain.Controls.Add(cbxAutoCommit, 0, 5);
            tlPnlMain.Controls.Add(cbxAddReference, 0, 6);
            tlPnlMain.Dock = DockStyle.Fill;
            tlPnlMain.Location = new Point(12, 12);
            tlPnlMain.Margin = new Padding(0);
            tlPnlMain.Name = "tlPnlMain";
            tlPnlMain.RowCount = 7;
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.RowStyles.Add(new RowStyle());
            tlPnlMain.Size = new Size(590, 348);
            tlPnlMain.TabIndex = 0;
            // 
            // chooseRevPanel
            // 
            chooseRevPanel.AutoSize = true;
            chooseRevPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chooseRevPanel.Controls.Add(btnChooseRevision);
            chooseRevPanel.Controls.Add(lblAnotherRev);
            chooseRevPanel.Dock = DockStyle.Fill;
            chooseRevPanel.FlowDirection = FlowDirection.RightToLeft;
            chooseRevPanel.Location = new Point(3, 184);
            chooseRevPanel.Name = "chooseRevPanel";
            chooseRevPanel.Size = new Size(584, 30);
            chooseRevPanel.TabIndex = 3;
            // 
            // lblAnotherRev
            // 
            lblAnotherRev.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lblAnotherRev.AutoSize = true;
            lblAnotherRev.Location = new Point(412, 0);
            lblAnotherRev.Name = "lblAnotherRev";
            lblAnotherRev.Size = new Size(138, 30);
            lblAnotherRev.TabIndex = 4;
            lblAnotherRev.Text = "C&hoose another revision:";
            lblAnotherRev.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormCherryPick
            // 
            AcceptButton = btnPick;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            CancelButton = btnAbort;
            ClientSize = new Size(614, 413);
            HelpButton = true;
            ManualSectionAnchorName = "cherry-pick-commit";
            ManualSectionSubfolder = "modify_history";
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(630, 370);
            Name = "FormCherryPick";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Cherry pick commit";
            FormClosing += Form_Closing;
            Load += Form_Load;
            Shown += Form_Shown;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            tlPnlMain.ResumeLayout(false);
            tlPnlMain.PerformLayout();
            chooseRevPanel.ResumeLayout(false);
            chooseRevPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBranchInfo;
        private Label lblAnotherRev;
        private Button btnPick;
        private Button btnAbort;
        private CheckBox cbxAutoCommit;
        private Label lblParents;
        private UserControls.NativeListView lvParentsList;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private CheckBox cbxAddReference;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private Button btnChooseRevision;
        private TableLayoutPanel tlPnlMain;
        private FlowLayoutPanel chooseRevPanel;
    }
}
