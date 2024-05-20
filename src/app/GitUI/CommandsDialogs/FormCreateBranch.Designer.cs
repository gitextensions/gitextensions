namespace GitUI.CommandsDialogs
{
    partial class FormCreateBranch
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
            label2 = new Label();
            commitPickerSmallControl1 = new GitUI.UserControls.CommitPickerSmallControl();
            chkbxCheckoutAfterCreate = new CheckBox();
            label1 = new Label();
            BranchNameTextBox = new TextBox();
            Orphan = new CheckBox();
            ClearOrphan = new CheckBox();
            Ok = new Button();
            toolTip = new ToolTip(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBox1 = new GroupBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tableLayoutPanel1);
            MainPanel.Size = new Size(570, 325);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Ok);
            ControlsPanel.Location = new Point(0, 325);
            ControlsPanel.Size = new Size(570, 41);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 31);
            label2.Margin = new Padding(3);
            label2.Name = "label2";
            label2.Size = new Size(160, 22);
            label2.TabIndex = 2;
            label2.Text = "Create b&ranch at this revision";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // commitPickerSmallControl1
            // 
            commitPickerSmallControl1.AutoSize = true;
            commitPickerSmallControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitPickerSmallControl1.Dock = DockStyle.Fill;
            commitPickerSmallControl1.Location = new Point(169, 31);
            commitPickerSmallControl1.MinimumSize = new Size(100, 26);
            commitPickerSmallControl1.Name = "commitPickerSmallControl1";
            commitPickerSmallControl1.Size = new Size(380, 26);
            commitPickerSmallControl1.TabIndex = 3;
            commitPickerSmallControl1.SelectedObjectIdChanged += commitPickerSmallControl1_SelectedObjectIdChanged;
            // 
            // chkbxCheckoutAfterCreate
            // 
            chkbxCheckoutAfterCreate.AutoSize = true;
            chkbxCheckoutAfterCreate.Checked = true;
            chkbxCheckoutAfterCreate.CheckState = CheckState.Checked;
            chkbxCheckoutAfterCreate.Dock = DockStyle.Fill;
            chkbxCheckoutAfterCreate.Location = new Point(169, 59);
            chkbxCheckoutAfterCreate.Name = "chkbxCheckoutAfterCreate";
            chkbxCheckoutAfterCreate.Size = new Size(380, 22);
            chkbxCheckoutAfterCreate.TabIndex = 5;
            chkbxCheckoutAfterCreate.Text = "Checkout &after create";
            chkbxCheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 3);
            label1.Margin = new Padding(3);
            label1.Name = "label1";
            label1.Size = new Size(160, 22);
            label1.TabIndex = 0;
            label1.Text = "&Branch name";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BranchNameTextBox
            // 
            BranchNameTextBox.Dock = DockStyle.Fill;
            BranchNameTextBox.Location = new Point(169, 3);
            BranchNameTextBox.Name = "BranchNameTextBox";
            BranchNameTextBox.Size = new Size(380, 23);
            BranchNameTextBox.TabIndex = 1;
            BranchNameTextBox.Leave += BranchNameTextBox_Leave;
            // 
            // Orphan
            // 
            Orphan.AutoSize = true;
            Orphan.Location = new Point(11, 3);
            Orphan.Name = "Orphan";
            Orphan.Size = new Size(101, 19);
            Orphan.TabIndex = 1;
            Orphan.Text = "Create or&phan";
            toolTip.SetToolTip(Orphan, "New branch will have NO parents");
            Orphan.UseVisualStyleBackColor = true;
            Orphan.CheckedChanged += Orphan_CheckedChanged;
            // 
            // ClearOrphan
            // 
            ClearOrphan.AutoSize = true;
            ClearOrphan.Checked = true;
            ClearOrphan.CheckState = CheckState.Checked;
            ClearOrphan.Enabled = false;
            ClearOrphan.Location = new Point(118, 3);
            ClearOrphan.Name = "ClearOrphan";
            ClearOrphan.Size = new Size(204, 19);
            ClearOrphan.TabIndex = 3;
            ClearOrphan.Text = "Clear &working directory and index";
            toolTip.SetToolTip(ClearOrphan, "Remove files from the working directory and from the index");
            ClearOrphan.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            Ok.AutoSize = true;
            Ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Ok.DialogResult = DialogResult.OK;
            Ok.Image = Properties.Images.BranchCreate;
            Ok.ImageAlign = ContentAlignment.TopLeft;
            Ok.Location = new Point(450, 8);
            Ok.MinimumSize = new Size(75, 23);
            Ok.Name = "Ok";
            Ok.Size = new Size(107, 25);
            Ok.TabIndex = 7;
            Ok.Text = "&Create branch";
            Ok.TextAlign = ContentAlignment.MiddleRight;
            Ok.TextImageRelation = TextImageRelation.ImageBeforeText;
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(groupBox1, 0, 4);
            tableLayoutPanel1.Controls.Add(commitPickerSmallControl1, 1, 1);
            tableLayoutPanel1.Controls.Add(BranchNameTextBox, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(chkbxCheckoutAfterCreate, 1, 2);
            tableLayoutPanel1.Controls.Add(commitSummaryUserControl1, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(9, 9);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(552, 301);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(groupBox1, 2);
            groupBox1.Controls.Add(flowLayoutPanel1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 241);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(8);
            groupBox1.Size = new Size(546, 57);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Orphan";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(Orphan);
            flowLayoutPanel1.Controls.Add(ClearOrphan);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(8, 24);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(8, 0, 0, 0);
            flowLayoutPanel1.Size = new Size(530, 25);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // commitSummaryUserControl1
            // 
            commitSummaryUserControl1.AutoSize = true;
            commitSummaryUserControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(commitSummaryUserControl1, 2);
            commitSummaryUserControl1.Dock = DockStyle.Fill;
            commitSummaryUserControl1.Location = new Point(2, 86);
            commitSummaryUserControl1.Margin = new Padding(2, 2, 2, 2);
            commitSummaryUserControl1.MinimumSize = new Size(293, 107);
            commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            commitSummaryUserControl1.Revision = null;
            commitSummaryUserControl1.Size = new Size(548, 150);
            commitSummaryUserControl1.TabIndex = 7;
            // 
            // FormCreateBranch
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(570, 366);
            HelpButton = true;
            ManualSectionAnchorName = "create-branch";
            ManualSectionSubfolder = "branches";
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(580, 405);
            Name = "FormCreateBranch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create branch";
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox Orphan;
        private ToolTip toolTip;
        private CheckBox ClearOrphan;
        private TextBox BranchNameTextBox;
        private Label label1;
        private Label label2;
        private UserControls.CommitPickerSmallControl commitPickerSmallControl1;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel1;
        private Button Ok;
        private CheckBox chkbxCheckoutAfterCreate;
        private FlowLayoutPanel flowLayoutPanel1;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl1;
    }
}
