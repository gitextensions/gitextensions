namespace GitUI.CommandsDialogs;

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
        lblCreateBranch = new Label();
        commitPicker = new GitUI.UserControls.CommitPickerSmallControl();
        chkCheckoutAfterCreate = new CheckBox();
        label1 = new Label();
        BranchNameTextBox = new TextBox();
        chkCreateOrphan = new CheckBox();
        chkClearOrphan = new CheckBox();
        cmdOk = new Button();
        toolTip = new ToolTip(components);
        tableLayout = new TableLayoutPanel();
        grpOrphan = new GroupBox();
        flowLayoutPanel1 = new FlowLayoutPanel();
        commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
        MainPanel.SuspendLayout();
        ControlsPanel.SuspendLayout();
        tableLayout.SuspendLayout();
        grpOrphan.SuspendLayout();
        flowLayoutPanel1.SuspendLayout();
        SuspendLayout();
        // 
        // MainPanel
        // 
        MainPanel.Controls.Add(tableLayout);
        MainPanel.Size = new Size(570, 354);
        // 
        // ControlsPanel
        // 
        ControlsPanel.Controls.Add(cmdOk);
        ControlsPanel.Location = new Point(0, 325);
        ControlsPanel.Size = new Size(570, 41);
        // 
        // lblCreateBranch
        // 
        lblCreateBranch.AutoSize = true;
        lblCreateBranch.Dock = DockStyle.Fill;
        lblCreateBranch.Location = new Point(3, 31);
        lblCreateBranch.Margin = new Padding(3);
        lblCreateBranch.Name = "lblCreateBranch";
        lblCreateBranch.Size = new Size(160, 22);
        lblCreateBranch.TabIndex = 2;
        lblCreateBranch.Text = "Create b&ranch at this revision";
        lblCreateBranch.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // commitPicker
        // 
        commitPicker.AutoSize = true;
        commitPicker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        commitPicker.Dock = DockStyle.Fill;
        commitPicker.Location = new Point(169, 31);
        commitPicker.MinimumSize = new Size(100, 26);
        commitPicker.Name = "commitPicker";
        commitPicker.Size = new Size(374, 26);
        commitPicker.TabIndex = 3;
        commitPicker.SelectedObjectIdChanged += commitPicker_SelectedObjectIdChanged;
        // 
        // chkCheckoutAfterCreate
        // 
        chkCheckoutAfterCreate.AutoSize = true;
        chkCheckoutAfterCreate.Checked = true;
        chkCheckoutAfterCreate.CheckState = CheckState.Checked;
        chkCheckoutAfterCreate.Dock = DockStyle.Fill;
        chkCheckoutAfterCreate.Location = new Point(169, 59);
        chkCheckoutAfterCreate.Name = "chkCheckoutAfterCreate";
        chkCheckoutAfterCreate.Size = new Size(374, 22);
        chkCheckoutAfterCreate.TabIndex = 5;
        chkCheckoutAfterCreate.Text = "Checkout &after create";
        chkCheckoutAfterCreate.UseVisualStyleBackColor = true;
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
        BranchNameTextBox.Size = new Size(374, 23);
        BranchNameTextBox.TabIndex = 1;
        BranchNameTextBox.Leave += BranchNameTextBox_Leave;
        // 
        // chkCreateOrphan
        // 
        chkCreateOrphan.AutoSize = true;
        chkCreateOrphan.Location = new Point(11, 3);
        chkCreateOrphan.Name = "chkCreateOrphan";
        chkCreateOrphan.Size = new Size(101, 19);
        chkCreateOrphan.TabIndex = 1;
        chkCreateOrphan.Text = "Create or&phan";
        toolTip.SetToolTip(chkCreateOrphan, "New branch will have NO parents");
        chkCreateOrphan.UseVisualStyleBackColor = true;
        chkCreateOrphan.CheckedChanged += chkCreateOrphan_CheckedChanged;
        // 
        // chkClearOrphan
        // 
        chkClearOrphan.AutoSize = true;
        chkClearOrphan.Checked = true;
        chkClearOrphan.CheckState = CheckState.Checked;
        chkClearOrphan.Enabled = false;
        chkClearOrphan.Location = new Point(118, 3);
        chkClearOrphan.Name = "chkClearOrphan";
        chkClearOrphan.Size = new Size(204, 19);
        chkClearOrphan.TabIndex = 3;
        chkClearOrphan.Text = "Clear &working directory and index";
        toolTip.SetToolTip(chkClearOrphan, "Remove files from the working directory and from the index");
        chkClearOrphan.UseVisualStyleBackColor = true;
        // 
        // cmdOk
        // 
        cmdOk.AutoSize = true;
        cmdOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        cmdOk.DialogResult = DialogResult.OK;
        cmdOk.Image = Properties.Images.BranchCreate;
        cmdOk.ImageAlign = ContentAlignment.TopLeft;
        cmdOk.Location = new Point(450, 8);
        cmdOk.MinimumSize = new Size(75, 23);
        cmdOk.Name = "cmdOk";
        cmdOk.Size = new Size(107, 25);
        cmdOk.TabIndex = 7;
        cmdOk.Text = "&Create branch";
        cmdOk.TextAlign = ContentAlignment.MiddleRight;
        cmdOk.TextImageRelation = TextImageRelation.ImageBeforeText;
        cmdOk.UseVisualStyleBackColor = true;
        cmdOk.Click += cmdOk_Click;
        // 
        // tableLayout
        // 
        tableLayout.AutoSize = true;
        tableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayout.ColumnCount = 2;
        tableLayout.ColumnStyles.Add(new ColumnStyle());
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.Controls.Add(grpOrphan, 0, 4);
        tableLayout.Controls.Add(commitPicker, 1, 1);
        tableLayout.Controls.Add(BranchNameTextBox, 1, 0);
        tableLayout.Controls.Add(label1, 0, 0);
        tableLayout.Controls.Add(lblCreateBranch, 0, 1);
        tableLayout.Controls.Add(chkCheckoutAfterCreate, 1, 2);
        tableLayout.Controls.Add(commitSummaryUserControl1, 0, 3);
        tableLayout.Dock = DockStyle.Fill;
        tableLayout.Location = new Point(12, 12);
        tableLayout.Margin = new Padding(0);
        tableLayout.Name = "tableLayout";
        tableLayout.RowCount = 6;
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tableLayout.RowStyles.Add(new RowStyle());
        tableLayout.RowStyles.Add(new RowStyle());
        tableLayout.RowStyles.Add(new RowStyle());
        tableLayout.Size = new Size(546, 330);
        tableLayout.TabIndex = 0;
        // 
        // grpOrphan
        // 
        grpOrphan.AutoSize = true;
        grpOrphan.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayout.SetColumnSpan(grpOrphan, 2);
        grpOrphan.Controls.Add(flowLayoutPanel1);
        grpOrphan.Dock = DockStyle.Fill;
        grpOrphan.Location = new Point(3, 241);
        grpOrphan.Name = "grpOrphan";
        grpOrphan.Padding = new Padding(8);
        grpOrphan.Size = new Size(540, 57);
        grpOrphan.TabIndex = 6;
        grpOrphan.TabStop = false;
        grpOrphan.Text = "Orphan";
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanel1.Controls.Add(chkCreateOrphan);
        flowLayoutPanel1.Controls.Add(chkClearOrphan);
        flowLayoutPanel1.Dock = DockStyle.Fill;
        flowLayoutPanel1.Location = new Point(8, 24);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Padding = new Padding(8, 0, 0, 0);
        flowLayoutPanel1.Size = new Size(524, 25);
        flowLayoutPanel1.TabIndex = 1;
        // 
        // commitSummaryUserControl1
        // 
        commitSummaryUserControl1.AutoSize = true;
        commitSummaryUserControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayout.SetColumnSpan(commitSummaryUserControl1, 2);
        commitSummaryUserControl1.Dock = DockStyle.Fill;
        commitSummaryUserControl1.Location = new Point(2, 86);
        commitSummaryUserControl1.Margin = new Padding(2, 2, 2, 2);
        commitSummaryUserControl1.MinimumSize = new Size(293, 107);
        commitSummaryUserControl1.Name = "commitSummaryUserControl1";
        commitSummaryUserControl1.Revision = null;
        commitSummaryUserControl1.Size = new Size(542, 150);
        commitSummaryUserControl1.TabIndex = 7;
        // 
        // FormCreateBranch
        // 
        AcceptButton = cmdOk;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(570, 386);
        HelpButton = true;
        ManualSectionAnchorName = "create-branch";
        ManualSectionSubfolder = "branches";
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(580, 425);
        Name = "FormCreateBranch";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Create branch";
        MainPanel.ResumeLayout(false);
        MainPanel.PerformLayout();
        ControlsPanel.ResumeLayout(false);
        ControlsPanel.PerformLayout();
        tableLayout.ResumeLayout(false);
        tableLayout.PerformLayout();
        grpOrphan.ResumeLayout(false);
        grpOrphan.PerformLayout();
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private CheckBox chkCreateOrphan;
    private ToolTip toolTip;
    private CheckBox chkClearOrphan;
    private TextBox BranchNameTextBox;
    private Label label1;
    private Label lblCreateBranch;
    private UserControls.CommitPickerSmallControl commitPicker;
    private GroupBox grpOrphan;
    private TableLayoutPanel tableLayout;
    private Button cmdOk;
    private CheckBox chkCheckoutAfterCreate;
    private FlowLayoutPanel flowLayoutPanel1;
    private UserControls.CommitSummaryUserControl commitSummaryUserControl1;
}
