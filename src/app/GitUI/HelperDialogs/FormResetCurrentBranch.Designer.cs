namespace GitUI.HelperDialogs
{
    partial class FormResetCurrentBranch
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
            GroupBox gbResetType;
            tableLayoutPanel2 = new TableLayoutPanel();
            Soft = new RadioButton();
            Mixed = new RadioButton();
            Keep = new RadioButton();
            Merge = new RadioButton();
            Hard = new RadioButton();
            _NO_TRANSLATE_BranchInfo = new Label();
            commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            Cancel = new Button();
            Ok = new Button();
            gbResetType = new GroupBox();
            gbResetType.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // gbResetType
            // 
            gbResetType.AutoSize = true;
            gbResetType.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbResetType.Controls.Add(tableLayoutPanel2);
            gbResetType.Dock = DockStyle.Fill;
            gbResetType.Location = new Point(5, 170);
            gbResetType.Margin = new Padding(2);
            gbResetType.Name = "gbResetType";
            gbResetType.Padding = new Padding(11);
            gbResetType.Size = new Size(459, 245);
            gbResetType.TabIndex = 0;
            gbResetType.TabStop = false;
            gbResetType.Text = "Reset type";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(Soft, 0, 0);
            tableLayoutPanel2.Controls.Add(Mixed, 0, 1);
            tableLayoutPanel2.Controls.Add(Keep, 0, 2);
            tableLayoutPanel2.Controls.Add(Merge, 0, 3);
            tableLayoutPanel2.Controls.Add(Hard, 0, 4);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(11, 24);
            tableLayoutPanel2.Margin = new Padding(2);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(437, 210);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // Soft
            // 
            Soft.AutoSize = true;
            Soft.BackColor = Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            Soft.Checked = true;
            Soft.Dock = DockStyle.Fill;
            Soft.ForeColor = Color.Black;
            Soft.Location = new Point(3, 3);
            Soft.Name = "Soft";
            Soft.Padding = new Padding(3);
            Soft.Size = new Size(431, 36);
            Soft.TabIndex = 0;
            Soft.Text = "&Soft: leave working directory and index untouched";
            Soft.UseVisualStyleBackColor = false;
            // 
            // Mixed
            // 
            Mixed.AutoSize = true;
            Mixed.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            Mixed.Dock = DockStyle.Fill;
            Mixed.ForeColor = Color.Black;
            Mixed.Location = new Point(3, 45);
            Mixed.Name = "Mixed";
            Mixed.Padding = new Padding(3);
            Mixed.Size = new Size(431, 36);
            Mixed.TabIndex = 1;
            Mixed.TabStop = true;
            Mixed.Text = "Mi&xed: leave working directory untouched, reset index";
            Mixed.UseVisualStyleBackColor = false;
            // 
            // Keep
            // 
            Keep.AutoSize = true;
            Keep.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            Keep.Dock = DockStyle.Fill;
            Keep.ForeColor = Color.Black;
            Keep.Location = new Point(3, 87);
            Keep.Name = "Keep";
            Keep.Padding = new Padding(3);
            Keep.Size = new Size(431, 36);
            Keep.TabIndex = 2;
            Keep.Text = "&Keep: update working directory to the commit \r\n(abort if there are local changes)" +
    ", reset index";
            Keep.UseVisualStyleBackColor = false;
            // 
            // Merge
            // 
            Merge.AutoSize = true;
            Merge.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            Merge.Dock = DockStyle.Fill;
            Merge.ForeColor = Color.Black;
            Merge.Location = new Point(3, 129);
            Merge.Name = "Merge";
            Merge.Padding = new Padding(3);
            Merge.Size = new Size(431, 36);
            Merge.TabIndex = 3;
            Merge.Text = "&Merge: update working directory to the commit and keep local changes \r\n(abort if " +
    "there are conflicts), reset index";
            Merge.UseVisualStyleBackColor = false;
            // 
            // Hard
            // 
            Hard.AutoSize = true;
            Hard.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            Hard.Dock = DockStyle.Fill;
            Hard.ForeColor = Color.Black;
            Hard.Location = new Point(3, 171);
            Hard.Name = "Hard";
            Hard.Padding = new Padding(3);
            Hard.Size = new Size(431, 36);
            Hard.TabIndex = 4;
            Hard.Text = "&Hard: reset working directory and index\r\n(discard ALL local changes, even uncommi" +
    "tted changes)";
            Hard.UseVisualStyleBackColor = false;
            // 
            // _NO_TRANSLATE_BranchInfo
            // 
            _NO_TRANSLATE_BranchInfo.AutoSize = true;
            _NO_TRANSLATE_BranchInfo.Dock = DockStyle.Fill;
            _NO_TRANSLATE_BranchInfo.Location = new Point(7, 3);
            _NO_TRANSLATE_BranchInfo.Margin = new Padding(4, 0, 4, 0);
            _NO_TRANSLATE_BranchInfo.Name = "_NO_TRANSLATE_BranchInfo";
            _NO_TRANSLATE_BranchInfo.Size = new Size(455, 13);
            _NO_TRANSLATE_BranchInfo.TabIndex = 5;
            _NO_TRANSLATE_BranchInfo.Text = "##Reset branch \'{0}\' to:";
            // 
            // commitSummaryUserControl1
            // 
            commitSummaryUserControl1.AutoSize = true;
            commitSummaryUserControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitSummaryUserControl1.Dock = DockStyle.Fill;
            commitSummaryUserControl1.Location = new Point(4, 17);
            commitSummaryUserControl1.Margin = new Padding(1);
            commitSummaryUserControl1.MinimumSize = new Size(440, 147);
            commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            commitSummaryUserControl1.Revision = null;
            commitSummaryUserControl1.Size = new Size(461, 150);
            commitSummaryUserControl1.TabIndex = 6;
            commitSummaryUserControl1.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_BranchInfo, 0, 0);
            tableLayoutPanel1.Controls.Add(commitSummaryUserControl1, 0, 1);
            tableLayoutPanel1.Controls.Add(gbResetType, 0, 2);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(5, 5);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(3);
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(469, 459);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(Cancel);
            flowLayoutPanel1.Controls.Add(Ok);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(4, 426);
            flowLayoutPanel1.Margin = new Padding(1);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(461, 29);
            flowLayoutPanel1.TabIndex = 10;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(368, 2);
            Cancel.Margin = new Padding(2);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(91, 25);
            Cancel.TabIndex = 2;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // Ok
            // 
            Ok.Location = new Point(273, 2);
            Ok.Margin = new Padding(2);
            Ok.Name = "Ok";
            Ok.Size = new Size(91, 25);
            Ok.TabIndex = 1;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // FormResetCurrentBranch
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CancelButton = Cancel;
            ClientSize = new Size(479, 469);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormResetCurrentBranch";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reset current branch";
            HelpButtonClicked += FormResetCurrentBranch_HelpButtonClicked;
            Load += FormResetCurrentBranch_Load;
            gbResetType.ResumeLayout(false);
            gbResetType.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label _NO_TRANSLATE_BranchInfo;
        private RadioButton Hard;
        private RadioButton Mixed;
        private RadioButton Soft;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private RadioButton Keep;
        private RadioButton Merge;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button Cancel;
        private Button Ok;
    }
}
