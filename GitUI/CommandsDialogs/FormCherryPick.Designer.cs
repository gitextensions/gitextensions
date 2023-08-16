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
            Pick = new Button();
            btnChooseRevision = new Button();
            label2 = new Label();
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
            commitInformationPanel = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            panelParentsList = new TableLayoutPanel();
            mainTableLayoutPanel.SuspendLayout();
            commitInformationPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            panelParentsList.SuspendLayout();
            SuspendLayout();
            // 
            // Pick
            // 
            Pick.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Pick.Location = new Point(475, 22);
            Pick.Name = "Pick";
            tableLayoutPanel3.SetRowSpan(Pick, 2);
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
            btnChooseRevision.Location = new Point(69, 33);
            btnChooseRevision.Name = "btnChooseRevision";
            btnChooseRevision.Size = new Size(25, 24);
            btnChooseRevision.TabIndex = 34;
            btnChooseRevision.UseVisualStyleBackColor = true;
            btnChooseRevision.Click += btnChooseRevision_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(91, 30);
            label2.TabIndex = 33;
            label2.Text = "C&hoose another\r\nrevision:";
            // 
            // ParentsLabel
            // 
            ParentsLabel.AutoSize = true;
            ParentsLabel.Location = new Point(3, 0);
            ParentsLabel.Name = "ParentsLabel";
            ParentsLabel.Size = new Size(206, 15);
            ParentsLabel.TabIndex = 12;
            ParentsLabel.Text = "This commit is a merge, select &parent:";
            // 
            // ParentsList
            // 
            ParentsList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ParentsList.BorderStyle = BorderStyle.FixedSingle;
            ParentsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            ParentsList.FullRowSelect = true;
            ParentsList.Location = new Point(0, 15);
            ParentsList.Margin = new Padding(0);
            ParentsList.MultiSelect = false;
            ParentsList.Name = "ParentsList";
            ParentsList.Size = new Size(587, 106);
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
            checkAddReference.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkAddReference.AutoSize = true;
            checkAddReference.Location = new Point(3, 28);
            checkAddReference.Name = "checkAddReference";
            checkAddReference.Size = new Size(253, 19);
            checkAddReference.TabIndex = 14;
            checkAddReference.Text = "A&dd commit reference to commit message";
            checkAddReference.UseVisualStyleBackColor = true;
            // 
            // AutoCommit
            // 
            AutoCommit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AutoCommit.AutoSize = true;
            AutoCommit.Location = new Point(3, 3);
            AutoCommit.Name = "AutoCommit";
            AutoCommit.Size = new Size(189, 19);
            AutoCommit.TabIndex = 11;
            AutoCommit.Text = "&Automatically create a commit";
            AutoCommit.UseVisualStyleBackColor = true;
            // 
            // BranchInfo
            // 
            BranchInfo.AutoSize = true;
            BranchInfo.Location = new Point(3, 0);
            BranchInfo.Name = "BranchInfo";
            BranchInfo.Size = new Size(137, 15);
            BranchInfo.TabIndex = 5;
            BranchInfo.Text = "Cherry pick this commit:";
            // 
            // commitSummaryUserControl1
            // 
            commitSummaryUserControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            commitSummaryUserControl1.AutoSize = true;
            commitSummaryUserControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitSummaryUserControl1.Location = new Point(16, 3);
            commitSummaryUserControl1.Margin = new Padding(16, 3, 3, 3);
            commitSummaryUserControl1.MinimumSize = new Size(440, 160);
            commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            commitSummaryUserControl1.Revision = null;
            commitSummaryUserControl1.Size = new Size(467, 160);
            commitSummaryUserControl1.TabIndex = 15;
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.AutoSize = true;
            mainTableLayoutPanel.ColumnCount = 1;
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.Controls.Add(BranchInfo, 0, 0);
            mainTableLayoutPanel.Controls.Add(commitInformationPanel, 0, 1);
            mainTableLayoutPanel.Controls.Add(tableLayoutPanel3, 0, 3);
            mainTableLayoutPanel.Controls.Add(panelParentsList, 0, 2);
            mainTableLayoutPanel.Dock = DockStyle.Fill;
            mainTableLayoutPanel.Location = new Point(0, 0);
            mainTableLayoutPanel.Margin = new Padding(2, 2, 2, 2);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 4;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.Size = new Size(591, 364);
            mainTableLayoutPanel.TabIndex = 35;
            // 
            // commitInformationPanel
            // 
            commitInformationPanel.AutoSize = true;
            commitInformationPanel.ColumnCount = 2;
            commitInformationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            commitInformationPanel.ColumnStyles.Add(new ColumnStyle());
            commitInformationPanel.Controls.Add(commitSummaryUserControl1, 0, 0);
            commitInformationPanel.Controls.Add(flowLayoutPanel1, 1, 0);
            commitInformationPanel.Dock = DockStyle.Fill;
            commitInformationPanel.Location = new Point(2, 17);
            commitInformationPanel.Margin = new Padding(2, 2, 2, 2);
            commitInformationPanel.Name = "commitInformationPanel";
            commitInformationPanel.RowCount = 1;
            commitInformationPanel.RowStyles.Add(new RowStyle());
            commitInformationPanel.Size = new Size(587, 166);
            commitInformationPanel.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(btnChooseRevision);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(488, 2);
            flowLayoutPanel1.Margin = new Padding(2, 2, 2, 2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(97, 162);
            flowLayoutPanel1.TabIndex = 16;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(Pick, 1, 0);
            tableLayoutPanel3.Controls.Add(checkAddReference, 0, 1);
            tableLayoutPanel3.Controls.Add(AutoCommit, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(2, 312);
            tableLayoutPanel3.Margin = new Padding(2, 2, 2, 2);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel3.Size = new Size(587, 50);
            tableLayoutPanel3.TabIndex = 17;
            // 
            // panelParentsList
            // 
            panelParentsList.ColumnCount = 1;
            panelParentsList.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelParentsList.Controls.Add(ParentsLabel, 0, 0);
            panelParentsList.Controls.Add(ParentsList, 0, 1);
            panelParentsList.Dock = DockStyle.Fill;
            panelParentsList.Location = new Point(2, 187);
            panelParentsList.Margin = new Padding(2, 2, 2, 2);
            panelParentsList.Name = "panelParentsList";
            panelParentsList.RowCount = 2;
            panelParentsList.RowStyles.Add(new RowStyle());
            panelParentsList.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelParentsList.Size = new Size(587, 121);
            panelParentsList.TabIndex = 18;
            // 
            // FormCherryPick
            // 
            AcceptButton = Pick;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new Size(591, 364);
            Controls.Add(mainTableLayoutPanel);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(603, 296);
            Name = "FormCherryPick";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Cherry pick commit";
            FormClosing += Form_Closing;
            Load += Form_Load;
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            commitInformationPanel.ResumeLayout(false);
            commitInformationPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            panelParentsList.ResumeLayout(false);
            panelParentsList.PerformLayout();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel commitInformationPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel panelParentsList;
    }
}
