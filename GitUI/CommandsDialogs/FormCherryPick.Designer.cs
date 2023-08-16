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
            Pick = new Button();
            btnChooseRevision = new Button();
            ParentsLabel = new Label();
            ParentsList = new UserControls.NativeListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            checkAddReference = new CheckBox();
            AutoCommit = new CheckBox();
            BranchInfo = new Label();
            commitSummaryUserControl1 = new UserControls.CommitSummaryUserControl();
            mainTableLayoutPanel = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label2 = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            mainTableLayoutPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(mainTableLayoutPanel);
            MainPanel.Size = new Size(637, 390);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Pick);
            ControlsPanel.Location = new Point(0, 390);
            ControlsPanel.Size = new Size(637, 41);
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(435, 0);
            label2.Name = "label2";
            label2.Size = new Size(138, 30);
            label2.TabIndex = 33;
            label2.Text = "C&hoose another revision:";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Pick
            // 
            Pick.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Pick.Location = new Point(515, 8);
            Pick.Name = "Pick";
            Pick.Size = new Size(109, 25);
            Pick.TabIndex = 10;
            Pick.Text = "&Cherry pick";
            Pick.UseVisualStyleBackColor = true;
            Pick.Click += Revert_Click;
            // 
            // btnChooseRevision
            // 
            btnChooseRevision.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChooseRevision.Image = Properties.Images.SelectRevision;
            btnChooseRevision.Location = new Point(579, 3);
            btnChooseRevision.Name = "btnChooseRevision";
            btnChooseRevision.Size = new Size(25, 24);
            btnChooseRevision.TabIndex = 34;
            btnChooseRevision.UseVisualStyleBackColor = true;
            btnChooseRevision.Click += btnChooseRevision_Click;
            // 
            // ParentsLabel
            // 
            ParentsLabel.AutoSize = true;
            ParentsLabel.Dock = DockStyle.Fill;
            ParentsLabel.Location = new Point(3, 217);
            ParentsLabel.Name = "ParentsLabel";
            ParentsLabel.Size = new Size(607, 15);
            ParentsLabel.TabIndex = 12;
            ParentsLabel.Text = "This commit is a merge, select &parent:";
            // 
            // ParentsList
            // 
            ParentsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            ParentsList.Dock = DockStyle.Fill;
            ParentsList.FullRowSelect = true;
            ParentsList.Location = new Point(6, 238);
            ParentsList.Margin = new Padding(6);
            ParentsList.MultiSelect = false;
            ParentsList.Name = "ParentsList";
            ParentsList.Size = new Size(601, 72);
            ParentsList.TabIndex = 13;
            ParentsList.UseCompatibleStateImageBehavior = false;
            ParentsList.View = View.Details;
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
            // checkAddReference
            // 
            checkAddReference.AutoSize = true;
            checkAddReference.Dock = DockStyle.Fill;
            checkAddReference.Location = new Point(3, 319);
            checkAddReference.Name = "checkAddReference";
            checkAddReference.Size = new Size(607, 19);
            checkAddReference.TabIndex = 14;
            checkAddReference.Text = "A&dd commit reference to commit message";
            checkAddReference.UseVisualStyleBackColor = true;
            // 
            // AutoCommit
            // 
            AutoCommit.AutoSize = true;
            AutoCommit.Dock = DockStyle.Fill;
            AutoCommit.Location = new Point(3, 344);
            AutoCommit.Name = "AutoCommit";
            AutoCommit.Size = new Size(607, 19);
            AutoCommit.TabIndex = 11;
            AutoCommit.Text = "&Automatically create a commit";
            AutoCommit.UseVisualStyleBackColor = true;
            // 
            // BranchInfo
            // 
            BranchInfo.AutoSize = true;
            BranchInfo.Dock = DockStyle.Fill;
            BranchInfo.Location = new Point(3, 0);
            BranchInfo.Name = "BranchInfo";
            BranchInfo.Size = new Size(607, 15);
            BranchInfo.TabIndex = 5;
            BranchInfo.Text = "Cherry pick this commit:";
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
            commitSummaryUserControl1.Size = new Size(594, 160);
            commitSummaryUserControl1.TabIndex = 15;
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.AutoSize = true;
            mainTableLayoutPanel.ColumnCount = 1;
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.Controls.Add(BranchInfo, 0, 0);
            mainTableLayoutPanel.Controls.Add(commitSummaryUserControl1, 0, 1);
            mainTableLayoutPanel.Controls.Add(flowLayoutPanel1, 0, 2);
            mainTableLayoutPanel.Controls.Add(ParentsLabel, 0, 3);
            mainTableLayoutPanel.Controls.Add(ParentsList, 0, 4);
            mainTableLayoutPanel.Controls.Add(checkAddReference, 0, 5);
            mainTableLayoutPanel.Controls.Add(AutoCommit, 0, 6);
            mainTableLayoutPanel.Dock = DockStyle.Fill;
            mainTableLayoutPanel.Location = new Point(12, 12);
            mainTableLayoutPanel.Margin = new Padding(0);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 5;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.Size = new Size(613, 366);
            mainTableLayoutPanel.TabIndex = 35;
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
            flowLayoutPanel1.Size = new Size(607, 30);
            flowLayoutPanel1.TabIndex = 16;
            // 
            // FormCherryPick
            // 
            AcceptButton = Pick;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new Size(637, 431);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(653, 365);
            Name = "FormCherryPick";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Cherry pick commit";
            FormClosing += Form_Closing;
            Load += Form_Load;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Pick;
        private System.Windows.Forms.CheckBox AutoCommit;
        private UserControls.NativeListView ParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label ParentsLabel;
        private System.Windows.Forms.CheckBox checkAddReference;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private System.Windows.Forms.Button btnChooseRevision;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
