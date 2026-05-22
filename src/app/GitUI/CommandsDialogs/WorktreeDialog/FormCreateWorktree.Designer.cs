namespace GitUI.CommandsDialogs.WorktreeDialog;

partial class FormCreateWorktree
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        btnCreateWorktree = new Button();
        txtNewBranchName = new TextBox();
        rbCreateNewBranch = new RadioButton();
        rbCheckoutExistingBranch = new RadioButton();
        chkOpenWorktree = new CheckBox();
        txtWorktreeDirectory = new TextBox();
        lblNewWorktreeFolder = new Label();
        cbxBranches = new ComboBox();
        btnBrowseWorktreeDir = new GitUI.UserControls.FolderBrowserButton();
        gbxWhatToCheckout = new GroupBox();
        tlpnlCheckout = new TableLayoutPanel();
        tlpnlMain = new TableLayoutPanel();
        MainPanel.SuspendLayout();
        ControlsPanel.SuspendLayout();
        gbxWhatToCheckout.SuspendLayout();
        tlpnlCheckout.SuspendLayout();
        tlpnlMain.SuspendLayout();
        SuspendLayout();
        //
        // MainPanel
        //
        MainPanel.AutoSize = true;
        MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MainPanel.Controls.Add(tlpnlMain);
        MainPanel.Size = new Size(608, 167);
        //
        // ControlsPanel
        //
        ControlsPanel.Controls.Add(btnCreateWorktree);
        ControlsPanel.Location = new Point(0, 167);
        ControlsPanel.Size = new Size(608, 41);
        //
        // btnCreateWorktree
        //
        btnCreateWorktree.AutoSize = true;
        btnCreateWorktree.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnCreateWorktree.DialogResult = DialogResult.OK;
        btnCreateWorktree.Location = new Point(470, 8);
        btnCreateWorktree.MinimumSize = new Size(75, 23);
        btnCreateWorktree.Name = "btnCreateWorktree";
        btnCreateWorktree.Size = new Size(125, 25);
        btnCreateWorktree.TabIndex = 0;
        btnCreateWorktree.Text = "&Create the new worktree";
        btnCreateWorktree.UseVisualStyleBackColor = true;
        btnCreateWorktree.Click += btnCreateWorktree_Click;
        //
        // txtNewBranchName
        //
        txtNewBranchName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtNewBranchName.Location = new Point(174, 34);
        txtNewBranchName.Name = "txtNewBranchName";
        txtNewBranchName.Size = new Size(419, 21);
        txtNewBranchName.TabIndex = 3;
        txtNewBranchName.TextChanged += UpdateWorktreePathAndValidateWorktreeOptions;
        //
        // rbCreateNewBranch
        //
        rbCreateNewBranch.AutoSize = true;
        rbCreateNewBranch.Location = new Point(3, 30);
        rbCreateNewBranch.Dock = DockStyle.Fill;
        rbCreateNewBranch.Name = "rbCreateNewBranch";
        rbCreateNewBranch.Size = new Size(135, 30);
        rbCreateNewBranch.TabIndex = 2;
        rbCreateNewBranch.TabStop = true;
        rbCreateNewBranch.Text = "Create a &new branch:\r\n(from current commit)";
        rbCreateNewBranch.UseVisualStyleBackColor = true;
        rbCreateNewBranch.Click += UpdateWorktreePathAndValidateWorktreeOptions;
        //
        // rbCheckoutExistingBranch
        //
        rbCheckoutExistingBranch.AutoSize = true;
        rbCheckoutExistingBranch.Checked = true;
        rbCheckoutExistingBranch.Dock = DockStyle.Fill;
        rbCheckoutExistingBranch.Location = new Point(3, 3);
        rbCheckoutExistingBranch.Name = "rbCheckoutExistingBranch";
        rbCheckoutExistingBranch.Size = new Size(165, 17);
        rbCheckoutExistingBranch.TabIndex = 0;
        rbCheckoutExistingBranch.TabStop = true;
        rbCheckoutExistingBranch.Text = "Checkout an &existing branch:";
        rbCheckoutExistingBranch.UseVisualStyleBackColor = true;
        rbCheckoutExistingBranch.Click += UpdateWorktreePathAndValidateWorktreeOptions;
        //
        // chkOpenWorktree
        //
        chkOpenWorktree.AutoSize = true;
        chkOpenWorktree.Checked = true;
        chkOpenWorktree.CheckState = CheckState.Checked;
        tlpnlMain.SetColumnSpan(chkOpenWorktree, 3);
        chkOpenWorktree.Location = new Point(3, 123);
        chkOpenWorktree.Name = "chkOpenWorktree";
        chkOpenWorktree.Size = new Size(228, 17);
        chkOpenWorktree.TabIndex = 4;
        chkOpenWorktree.Text = "&Open the new worktree after the creation";
        chkOpenWorktree.UseVisualStyleBackColor = true;
        //
        // txtWorktreeDirectory
        //
        txtWorktreeDirectory.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtWorktreeDirectory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        txtWorktreeDirectory.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
        txtWorktreeDirectory.Location = new Point(133, 94);
        txtWorktreeDirectory.Margin = new Padding(8, 3, 3, 3);
        txtWorktreeDirectory.Name = "txtWorktreeDirectory";
        txtWorktreeDirectory.Size = new Size(356, 21);
        txtWorktreeDirectory.TabIndex = 2;
        txtWorktreeDirectory.TextChanged += txtWorktreeDirectory_TextChanged;
        //
        // 
        // lblNewWorktreeFolder
        // 
        lblNewWorktreeFolder.AutoSize = true;
        lblNewWorktreeFolder.Dock = DockStyle.Fill;
        lblNewWorktreeFolder.Location = new Point(3, 98);
        lblNewWorktreeFolder.Name = "lblNewWorktreeFolder";
        lblNewWorktreeFolder.Size = new Size(124, 13);
        lblNewWorktreeFolder.TabIndex = 1;
        lblNewWorktreeFolder.Text = "New worktree &directory:";
        lblNewWorktreeFolder.TextAlign = ContentAlignment.MiddleLeft;
        //
        // cbxBranches
        //
        cbxBranches.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cbxBranches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cbxBranches.AutoCompleteSource = AutoCompleteSource.ListItems;
        cbxBranches.DropDownStyle = ComboBoxStyle.DropDownList;
        cbxBranches.FormattingEnabled = true;
        cbxBranches.Location = new Point(174, 3);
        cbxBranches.Name = "cbxBranches";
        cbxBranches.Size = new Size(419, 21);
        cbxBranches.TabIndex = 1;
        cbxBranches.SelectedIndexChanged += UpdateWorktreePathAndValidateWorktreeOptions;
        cbxBranches.KeyUp += cbxBranches_KeyUp;
        //
        // btnBrowseWorktreeDir
        //
        btnBrowseWorktreeDir.Anchor = AnchorStyles.Right;
        btnBrowseWorktreeDir.Location = new Point(495, 92);
        btnBrowseWorktreeDir.Name = "btnBrowseWorktreeDir";
        btnBrowseWorktreeDir.PathShowingControl = txtWorktreeDirectory;
        btnBrowseWorktreeDir.Size = new Size(110, 25);
        btnBrowseWorktreeDir.TabIndex = 3;
        // 
        // gbxWhatToCheckout
        //
        gbxWhatToCheckout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        gbxWhatToCheckout.AutoSize = true;
        gbxWhatToCheckout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlMain.SetColumnSpan(gbxWhatToCheckout, 3);
        gbxWhatToCheckout.Controls.Add(tlpnlCheckout);
        gbxWhatToCheckout.Location = new Point(3, 3);
        gbxWhatToCheckout.Name = "gbxWhatToCheckout";
        gbxWhatToCheckout.Size = new Size(602, 83);
        gbxWhatToCheckout.TabIndex = 0;
        gbxWhatToCheckout.TabStop = false;
        gbxWhatToCheckout.Text = "What to checkout:";
        //
        // tlpnlCheckout
        //
        tlpnlCheckout.AutoSize = true;
        tlpnlCheckout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlCheckout.ColumnCount = 2;
        tlpnlCheckout.ColumnStyles.Add(new ColumnStyle());
        tlpnlCheckout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpnlCheckout.Controls.Add(txtNewBranchName, 1, 1);
        tlpnlCheckout.Controls.Add(rbCheckoutExistingBranch, 0, 0);
        tlpnlCheckout.Controls.Add(rbCreateNewBranch, 0, 1);
        tlpnlCheckout.Controls.Add(cbxBranches, 1, 0);
        tlpnlCheckout.Dock = DockStyle.Fill;
        tlpnlCheckout.Location = new Point(3, 17);
        tlpnlCheckout.Margin = new Padding(0);
        tlpnlCheckout.Name = "tlpnlCheckout";
        tlpnlCheckout.RowCount = 2;
        tlpnlCheckout.RowStyles.Add(new RowStyle());
        tlpnlCheckout.RowStyles.Add(new RowStyle());
        tlpnlCheckout.Size = new Size(596, 63);
        tlpnlCheckout.TabIndex = 0;
        // 
        // tlpnlMain
        //
        tlpnlMain.AutoSize = true;
        tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlMain.ColumnCount = 3;
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.Controls.Add(gbxWhatToCheckout, 0, 0);
        tlpnlMain.Controls.Add(lblNewWorktreeFolder, 0, 1);
        tlpnlMain.Controls.Add(txtWorktreeDirectory, 1, 1);
        tlpnlMain.Controls.Add(btnBrowseWorktreeDir, 2, 1);
        tlpnlMain.Controls.Add(chkOpenWorktree, 0, 2);
        tlpnlMain.Dock = DockStyle.Fill;
        tlpnlMain.Location = new Point(12, 12);
        tlpnlMain.Margin = new Padding(0);
        tlpnlMain.Name = "tlpnlMain";
        tlpnlMain.RowCount = 3;
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.Size = new Size(584, 143);
        tlpnlMain.TabIndex = 0;
        //
        // FormCreateWorktree
        //
        AcceptButton = btnCreateWorktree;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(608, 208);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormCreateWorktree";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Create a new worktree";
        Load += FormCreateWorktree_Load;
        MainPanel.ResumeLayout(false);
        MainPanel.PerformLayout();
        ControlsPanel.ResumeLayout(false);
        ControlsPanel.PerformLayout();
        gbxWhatToCheckout.ResumeLayout(false);
        gbxWhatToCheckout.PerformLayout();
        tlpnlCheckout.ResumeLayout(false);
        tlpnlCheckout.PerformLayout();
        tlpnlMain.ResumeLayout(false);
        tlpnlMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private Button btnCreateWorktree;
    private ComboBox cbxBranches;
    private UserControls.FolderBrowserButton btnBrowseWorktreeDir;
    private TextBox txtWorktreeDirectory;
    private Label lblNewWorktreeFolder;
    private CheckBox chkOpenWorktree;
    private RadioButton rbCheckoutExistingBranch;
    private RadioButton rbCreateNewBranch;
    private TextBox txtNewBranchName;
    private GroupBox gbxWhatToCheckout;
    private TableLayoutPanel tlpnlCheckout;
    private TableLayoutPanel tlpnlMain;
}
