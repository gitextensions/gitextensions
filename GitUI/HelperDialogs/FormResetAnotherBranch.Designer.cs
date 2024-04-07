namespace GitUI.HelperDialogs
{
    partial class FormResetAnotherBranch
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
            BranchInfo = new Label();
            Ok = new Button();
            Cancel = new Button();
            commitSummaryUserControl = new UserControls.CommitSummaryUserControl();
            ForceReset = new CheckBox();
            pictureBox1 = new PictureBox();
            labelResetBranchWarning = new Label();
            Branches = new ComboBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tlpnlWarning = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tlpnlWarning.SuspendLayout();
            SuspendLayout();
            // 
            // BranchInfo
            // 
            BranchInfo.AutoSize = true;
            BranchInfo.Dock = DockStyle.Fill;
            BranchInfo.Location = new Point(6, 39);
            BranchInfo.Name = "BranchInfo";
            BranchInfo.Size = new Size(509, 13);
            BranchInfo.TabIndex = 1;
            BranchInfo.Text = "Reset local &branch:";
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Ok.Location = new Point(320, 3);
            Ok.Name = "Ok";
            Ok.Size = new Size(91, 25);
            Ok.TabIndex = 4;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(417, 3);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(91, 25);
            Cancel.TabIndex = 5;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // commitSummaryUserControl
            // 
            commitSummaryUserControl.AutoSize = true;
            commitSummaryUserControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitSummaryUserControl.Dock = DockStyle.Fill;
            commitSummaryUserControl.Location = new Point(4, 80);
            commitSummaryUserControl.Margin = new Padding(1);
            commitSummaryUserControl.MinimumSize = new Size(493, 150);
            commitSummaryUserControl.Name = "commitSummaryUserControl";
            commitSummaryUserControl.Revision = null;
            commitSummaryUserControl.Size = new Size(513, 150);
            commitSummaryUserControl.TabIndex = 0;
            commitSummaryUserControl.TabStop = false;
            // 
            // ForceReset
            // 
            ForceReset.AutoSize = true;
            ForceReset.Dock = DockStyle.Fill;
            ForceReset.Location = new Point(6, 234);
            ForceReset.Name = "ForceReset";
            ForceReset.Size = new Size(509, 17);
            ForceReset.TabIndex = 3;
            ForceReset.Text = "&Force reset for a non-fast-forward reset";
            ForceReset.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = Properties.Images.Warning;
            pictureBox1.InitialImage = Properties.Images.Warning;
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(16, 16);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.TabIndex = 11;
            pictureBox1.TabStop = false;
            // 
            // labelResetBranchWarning
            // 
            labelResetBranchWarning.ForeColor = Color.Black;
            labelResetBranchWarning.Location = new Point(25, 0);
            labelResetBranchWarning.Name = "labelResetBranchWarning";
            labelResetBranchWarning.Size = new Size(250, 20);
            labelResetBranchWarning.TabIndex = 0;
            labelResetBranchWarning.Text = "You can only reset a branch safely if there is a direct path from it to selected " +
    "revision.\r\nForcing a branch to reset if it has not been merged might leave some " +
    "commits unreachable.";
            // 
            // Branches
            // 
            Branches.Dock = DockStyle.Fill;
            Branches.Location = new Point(11, 55);
            Branches.Margin = new Padding(8, 3, 8, 3);
            Branches.Name = "Branches";
            Branches.Size = new Size(499, 21);
            Branches.TabIndex = 2;
            Branches.KeyUp += Branches_KeyUp;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(BranchInfo, 0, 2);
            tableLayoutPanel1.Controls.Add(Branches, 0, 3);
            tableLayoutPanel1.Controls.Add(ForceReset, 0, 5);
            tableLayoutPanel1.Controls.Add(commitSummaryUserControl, 0, 4);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 6);
            tableLayoutPanel1.Controls.Add(tlpnlWarning, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(8, 8);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(3);
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(519, 292);
            tableLayoutPanel1.TabIndex = 12;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(Cancel);
            flowLayoutPanel1.Controls.Add(Ok);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(5, 256);
            flowLayoutPanel1.Margin = new Padding(2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(511, 31);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // tlpnlWarning
            // 
            tlpnlWarning.AutoSize = true;
            tlpnlWarning.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlWarning.ColumnCount = 2;
            tlpnlWarning.ColumnStyles.Add(new ColumnStyle());
            tlpnlWarning.ColumnStyles.Add(new ColumnStyle());
            tlpnlWarning.Controls.Add(pictureBox1, 0, 0);
            tlpnlWarning.Controls.Add(labelResetBranchWarning, 1, 0);
            tlpnlWarning.Dock = DockStyle.Fill;
            tlpnlWarning.Location = new Point(5, 5);
            tlpnlWarning.Margin = new Padding(2);
            tlpnlWarning.Name = "tlpnlWarning";
            tlpnlWarning.RowCount = 1;
            tlpnlWarning.RowStyles.Add(new RowStyle());
            tlpnlWarning.Size = new Size(511, 22);
            tlpnlWarning.TabIndex = 4;
            // 
            // FormResetAnotherBranch
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CancelButton = Cancel;
            ClientSize = new Size(535, 381);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormResetAnotherBranch";
            Padding = new Padding(8);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reset branch";
            Load += FormResetAnotherBranch_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            tlpnlWarning.ResumeLayout(false);
            tlpnlWarning.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label BranchInfo;
        private Button Ok;
        private Button Cancel;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl;
        private CheckBox ForceReset;
        private PictureBox pictureBox1;
        private Label labelResetBranchWarning;
        private ComboBox Branches;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tlpnlWarning;
    }
}
