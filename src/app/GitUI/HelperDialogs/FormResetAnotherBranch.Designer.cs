namespace GitUI.HelperDialogs;

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
        commitSummaryUserControl = new GitUI.UserControls.CommitSummaryUserControl();
        ForceReset = new CheckBox();
        pictureBox1 = new PictureBox();
        lblResetBranchWarning = new Label();
        Branches = new ComboBox();
        tableLayoutPanel1 = new TableLayoutPanel();
        flowLayoutPanel1 = new FlowLayoutPanel();
        tlpnlWarning = new TableLayoutPanel();
        cbxCheckoutBranch = new CheckBox();
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
        BranchInfo.Location = new Point(6, 47);
        BranchInfo.Name = "BranchInfo";
        BranchInfo.Size = new Size(517, 15);
        BranchInfo.TabIndex = 1;
        BranchInfo.Text = "Reset local &branch:";
        // 
        // Ok
        // 
        Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Ok.Location = new Point(328, 3);
        Ok.Name = "Ok";
        Ok.Size = new Size(91, 25);
        Ok.TabIndex = 0;
        Ok.Text = "OK";
        Ok.UseVisualStyleBackColor = true;
        Ok.Click += Ok_Click;
        // 
        // Cancel
        // 
        Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Cancel.DialogResult = DialogResult.Cancel;
        Cancel.Location = new Point(425, 3);
        Cancel.Name = "Cancel";
        Cancel.Size = new Size(91, 25);
        Cancel.TabIndex = 1;
        Cancel.Text = "Cancel";
        Cancel.UseVisualStyleBackColor = true;
        Cancel.Click += Cancel_Click;
        // 
        // commitSummaryUserControl
        // 
        commitSummaryUserControl.AutoSize = true;
        commitSummaryUserControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        commitSummaryUserControl.Dock = DockStyle.Fill;
        commitSummaryUserControl.Location = new Point(4, 92);
        commitSummaryUserControl.Margin = new Padding(1);
        commitSummaryUserControl.MinimumSize = new Size(493, 150);
        commitSummaryUserControl.Name = "commitSummaryUserControl";
        commitSummaryUserControl.Revision = null;
        commitSummaryUserControl.Size = new Size(521, 150);
        commitSummaryUserControl.TabIndex = 3;
        commitSummaryUserControl.TabStop = false;
        // 
        // ForceReset
        // 
        ForceReset.AutoSize = true;
        ForceReset.Dock = DockStyle.Fill;
        ForceReset.Location = new Point(6, 271);
        ForceReset.Name = "ForceReset";
        ForceReset.Size = new Size(517, 19);
        ForceReset.TabIndex = 5;
        ForceReset.Text = "&Force reset for a non-fast-forward reset";
        ForceReset.UseVisualStyleBackColor = true;
        ForceReset.CheckedChanged += Validate;
        // 
        // pictureBox1
        // 
        pictureBox1.Dock = DockStyle.Fill;
        pictureBox1.Image = Properties.Images.Warning;
        pictureBox1.InitialImage = Properties.Images.Warning;
        pictureBox1.Location = new Point(3, 3);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(16, 24);
        pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
        pictureBox1.TabIndex = 11;
        pictureBox1.TabStop = false;
        // 
        // lblResetBranchWarning
        // 
        lblResetBranchWarning.AutoSize = true;
        lblResetBranchWarning.Dock = DockStyle.Fill;
        lblResetBranchWarning.ForeColor = SystemColors.WindowText;
        lblResetBranchWarning.Location = new Point(25, 0);
        lblResetBranchWarning.Name = "lblResetBranchWarning";
        lblResetBranchWarning.Size = new Size(491, 30);
        lblResetBranchWarning.TabIndex = 0;
        lblResetBranchWarning.Text = "You can only reset a branch safely if there is a direct path from it to selected revision.\r\nForcing a branch to reset if it has not been merged might leave some commits unreachable.";
        // 
        // Branches
        // 
        Branches.Dock = DockStyle.Fill;
        Branches.Location = new Point(11, 65);
        Branches.Margin = new Padding(8, 3, 8, 3);
        Branches.Name = "Branches";
        Branches.Size = new Size(507, 23);
        Branches.TabIndex = 2;
        Branches.DropDownClosed += Validate;
        Branches.TextChanged += Validate;
        Branches.Enter += Validate;
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
        tableLayoutPanel1.Controls.Add(ForceReset, 0, 6);
        tableLayoutPanel1.Controls.Add(commitSummaryUserControl, 0, 4);
        tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 7);
        tableLayoutPanel1.Controls.Add(tlpnlWarning, 0, 0);
        tableLayoutPanel1.Controls.Add(cbxCheckoutBranch, 0, 5);
        tableLayoutPanel1.Dock = DockStyle.Top;
        tableLayoutPanel1.Location = new Point(8, 8);
        tableLayoutPanel1.Margin = new Padding(2);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.Padding = new Padding(3);
        tableLayoutPanel1.RowCount = 8;
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.Size = new Size(519, 331);
        tableLayoutPanel1.TabIndex = 0;
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanel1.Controls.Add(Cancel);
        flowLayoutPanel1.Controls.Add(Ok);
        flowLayoutPanel1.Dock = DockStyle.Fill;
        flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
        flowLayoutPanel1.Location = new Point(5, 295);
        flowLayoutPanel1.Margin = new Padding(2);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(519, 31);
        flowLayoutPanel1.TabIndex = 6;
        // 
        // tlpnlWarning
        // 
        tlpnlWarning.AutoSize = true;
        tlpnlWarning.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlWarning.ColumnCount = 2;
        tlpnlWarning.ColumnStyles.Add(new ColumnStyle());
        tlpnlWarning.ColumnStyles.Add(new ColumnStyle());
        tlpnlWarning.Controls.Add(pictureBox1, 0, 0);
        tlpnlWarning.Controls.Add(lblResetBranchWarning, 1, 0);
        tlpnlWarning.Dock = DockStyle.Fill;
        tlpnlWarning.Location = new Point(5, 5);
        tlpnlWarning.Margin = new Padding(2);
        tlpnlWarning.Name = "tlpnlWarning";
        tlpnlWarning.RowCount = 1;
        tlpnlWarning.RowStyles.Add(new RowStyle());
        tlpnlWarning.Size = new Size(519, 30);
        tlpnlWarning.TabIndex = 0;
        // 
        // cbxCheckoutBranch
        // 
        cbxCheckoutBranch.AutoSize = true;
        cbxCheckoutBranch.Dock = DockStyle.Fill;
        cbxCheckoutBranch.Location = new Point(6, 246);
        cbxCheckoutBranch.Name = "cbxCheckoutBranch";
        cbxCheckoutBranch.Size = new Size(517, 19);
        cbxCheckoutBranch.TabIndex = 4;
        cbxCheckoutBranch.Text = "Chec&kout branch after reset";
        cbxCheckoutBranch.UseVisualStyleBackColor = true;
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
    private Label lblResetBranchWarning;
    private ComboBox Branches;
    private TableLayoutPanel tableLayoutPanel1;
    private FlowLayoutPanel flowLayoutPanel1;
    private TableLayoutPanel tlpnlWarning;
    private CheckBox cbxCheckoutBranch;
}
