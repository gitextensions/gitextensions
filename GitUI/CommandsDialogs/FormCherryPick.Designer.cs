using System.Windows.Forms;

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
            Label label2;
            btnPick = new Button();
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
            tlpnlMain = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnAbort = new Button();
            label2 = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlpnlMain.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tlpnlMain);
            MainPanel.Size = new Size(614, 379);
            MainPanel.TabIndex = 0;
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(btnAbort);
            ControlsPanel.Controls.Add(btnPick);
            ControlsPanel.Location = new Point(0, 379);
            ControlsPanel.Size = new Size(614, 41);
            ControlsPanel.TabIndex = 1;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(412, 0);
            label2.Name = "label2";
            label2.Size = new Size(138, 30);
            label2.TabIndex = 0;
            label2.Text = "C&hoose another revision:";
            label2.TextAlign = ContentAlignment.MiddleCenter;
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
            // btnChooseRevision
            // 
            btnChooseRevision.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChooseRevision.Image = Properties.Images.SelectRevision;
            btnChooseRevision.Location = new Point(556, 3);
            btnChooseRevision.Name = "btnChooseRevision";
            btnChooseRevision.Size = new Size(25, 24);
            btnChooseRevision.TabIndex = 1;
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
            lblParents.TabIndex = 3;
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
            lvParentsList.Size = new Size(578, 61);
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
            // cbxAddReference
            // 
            cbxAddReference.AutoSize = true;
            cbxAddReference.Dock = DockStyle.Fill;
            cbxAddReference.Location = new Point(3, 308);
            cbxAddReference.Name = "cbxAddReference";
            cbxAddReference.Size = new Size(584, 19);
            cbxAddReference.TabIndex = 6;
            cbxAddReference.Text = "A&dd commit reference to commit message";
            cbxAddReference.UseVisualStyleBackColor = true;
            // 
            // cbxAutoCommit
            // 
            cbxAutoCommit.AutoSize = true;
            cbxAutoCommit.Dock = DockStyle.Fill;
            cbxAutoCommit.Location = new Point(3, 333);
            cbxAutoCommit.Name = "cbxAutoCommit";
            cbxAutoCommit.Size = new Size(584, 19);
            cbxAutoCommit.TabIndex = 5;
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
            lblBranchInfo.TabIndex = 0;
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
            commitSummaryUserControl1.TabIndex = 1;
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlMain.Controls.Add(lblBranchInfo, 0, 0);
            tlpnlMain.Controls.Add(commitSummaryUserControl1, 0, 1);
            tlpnlMain.Controls.Add(flowLayoutPanel1, 0, 2);
            tlpnlMain.Controls.Add(lblParents, 0, 3);
            tlpnlMain.Controls.Add(lvParentsList, 0, 4);
            tlpnlMain.Controls.Add(cbxAutoCommit, 0, 5);
            tlpnlMain.Controls.Add(cbxAddReference, 0, 6);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(12, 12);
            tlpnlMain.Margin = new Padding(0);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 5;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.Size = new Size(590, 355);
            tlpnlMain.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(btnChooseRevision);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(3, 184);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(584, 30);
            flowLayoutPanel1.TabIndex = 2;
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
            // FormCherryPick
            // 
            AcceptButton = btnPick;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new Size(614, 420);
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
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblBranchInfo;
        private System.Windows.Forms.Button btnPick;
        private System.Windows.Forms.CheckBox cbxAutoCommit;
        private UserControls.NativeListView lvParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label lblParents;
        private System.Windows.Forms.CheckBox cbxAddReference;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private System.Windows.Forms.Button btnChooseRevision;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Button btnAbort;
    }
}
