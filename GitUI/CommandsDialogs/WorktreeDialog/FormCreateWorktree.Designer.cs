namespace GitUI.CommandsDialogs.WorktreeDialog
{
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
            createWorktreeButton = new Button();
            textBoxNewBranchName = new TextBox();
            radioButtonCreateNewBranch = new RadioButton();
            radioButtonCheckoutExistingBranch = new RadioButton();
            openWorktreeCheckBox = new CheckBox();
            newWorktreeDirectory = new TextBox();
            label2 = new Label();
            comboBoxBranches = new ComboBox();
            folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            groupBoxWhatToCheckout = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBoxWhatToCheckout.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            //
            // createWorktreeButton
            //
            createWorktreeButton.Anchor = AnchorStyles.Bottom;
            createWorktreeButton.AutoSize = true;
            createWorktreeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(createWorktreeButton, 3);
            createWorktreeButton.DialogResult = DialogResult.OK;
            createWorktreeButton.Location = new Point(232, 176);
            createWorktreeButton.Name = "createWorktreeButton";
            createWorktreeButton.Padding = new Padding(3);
            createWorktreeButton.Size = new Size(144, 29);
            createWorktreeButton.TabIndex = 3;
            createWorktreeButton.Text = "Create the new worktree";
            createWorktreeButton.UseVisualStyleBackColor = true;
            createWorktreeButton.Click += createWorktreeButton_Click;
            //
            // textBoxNewBranchName
            //
            textBoxNewBranchName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxNewBranchName.Location = new Point(174, 34);
            textBoxNewBranchName.Name = "textBoxNewBranchName";
            textBoxNewBranchName.Size = new Size(419, 21);
            textBoxNewBranchName.TabIndex = 11;
            textBoxNewBranchName.TextChanged += UpdateWorktreePathAndValidateWorktreeOptions;
            //
            // radioButtonCreateNewBranch
            //
            radioButtonCreateNewBranch.AutoSize = true;
            radioButtonCreateNewBranch.Location = new Point(3, 30);
            radioButtonCreateNewBranch.Name = "radioButtonCreateNewBranch";
            radioButtonCreateNewBranch.Size = new Size(135, 30);
            radioButtonCreateNewBranch.TabIndex = 10;
            radioButtonCreateNewBranch.TabStop = true;
            radioButtonCreateNewBranch.Text = "Create a new branch:\r\n (from current commit) ";
            radioButtonCreateNewBranch.UseVisualStyleBackColor = true;
            radioButtonCreateNewBranch.Click += UpdateWorktreePathAndValidateWorktreeOptions;
            //
            // radioButtonCheckoutExistingBranch
            //
            radioButtonCheckoutExistingBranch.AutoSize = true;
            radioButtonCheckoutExistingBranch.Checked = true;
            radioButtonCheckoutExistingBranch.Location = new Point(3, 3);
            radioButtonCheckoutExistingBranch.Name = "radioButtonCheckoutExistingBranch";
            radioButtonCheckoutExistingBranch.Size = new Size(165, 17);
            radioButtonCheckoutExistingBranch.TabIndex = 10;
            radioButtonCheckoutExistingBranch.TabStop = true;
            radioButtonCheckoutExistingBranch.Text = "Checkout an existing branch:";
            radioButtonCheckoutExistingBranch.UseVisualStyleBackColor = true;
            radioButtonCheckoutExistingBranch.Click += UpdateWorktreePathAndValidateWorktreeOptions;
            //
            // openWorktreeCheckBox
            //
            openWorktreeCheckBox.AutoSize = true;
            openWorktreeCheckBox.Checked = true;
            openWorktreeCheckBox.CheckState = CheckState.Checked;
            tableLayoutPanel1.SetColumnSpan(openWorktreeCheckBox, 3);
            openWorktreeCheckBox.Location = new Point(3, 123);
            openWorktreeCheckBox.Name = "openWorktreeCheckBox";
            openWorktreeCheckBox.Size = new Size(228, 17);
            openWorktreeCheckBox.TabIndex = 9;
            openWorktreeCheckBox.Text = "Open the new worktree after the creation";
            openWorktreeCheckBox.UseVisualStyleBackColor = true;
            //
            // newWorktreeDirectory
            //
            newWorktreeDirectory.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            newWorktreeDirectory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            newWorktreeDirectory.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            newWorktreeDirectory.Location = new Point(133, 94);
            newWorktreeDirectory.Name = "newWorktreeDirectory";
            newWorktreeDirectory.Size = new Size(356, 21);
            newWorktreeDirectory.TabIndex = 7;
            newWorktreeDirectory.TextChanged += ValidateWorktreeOptions;
            //
            // label2
            //
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 98);
            label2.Name = "label2";
            label2.Size = new Size(124, 13);
            label2.TabIndex = 6;
            label2.Text = "New worktree directory:";
            //
            // comboBoxBranches
            //
            comboBoxBranches.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxBranches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxBranches.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxBranches.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBranches.FormattingEnabled = true;
            comboBoxBranches.Location = new Point(174, 3);
            comboBoxBranches.Name = "comboBoxBranches";
            comboBoxBranches.Size = new Size(419, 21);
            comboBoxBranches.TabIndex = 2;
            comboBoxBranches.SelectedIndexChanged += UpdateWorktreePathAndValidateWorktreeOptions;
            comboBoxBranches.KeyUp += comboBoxBranches_KeyUp;
            //
            // folderBrowserButton1
            //
            folderBrowserButton1.Anchor = AnchorStyles.Left;
            folderBrowserButton1.Location = new Point(495, 92);
            folderBrowserButton1.Name = "folderBrowserButton1";
            folderBrowserButton1.PathShowingControl = newWorktreeDirectory;
            folderBrowserButton1.Size = new Size(110, 25);
            folderBrowserButton1.TabIndex = 8;
            //
            // groupBoxWhatToCheckout
            //
            groupBoxWhatToCheckout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxWhatToCheckout.AutoSize = true;
            groupBoxWhatToCheckout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(groupBoxWhatToCheckout, 3);
            groupBoxWhatToCheckout.Controls.Add(tableLayoutPanel2);
            groupBoxWhatToCheckout.Location = new Point(3, 3);
            groupBoxWhatToCheckout.Name = "groupBoxWhatToCheckout";
            groupBoxWhatToCheckout.Size = new Size(602, 83);
            groupBoxWhatToCheckout.TabIndex = 12;
            groupBoxWhatToCheckout.TabStop = false;
            groupBoxWhatToCheckout.Text = "What to checkout: ";
            //
            // tableLayoutPanel2
            //
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(textBoxNewBranchName, 1, 1);
            tableLayoutPanel2.Controls.Add(radioButtonCheckoutExistingBranch, 0, 0);
            tableLayoutPanel2.Controls.Add(radioButtonCreateNewBranch, 0, 1);
            tableLayoutPanel2.Controls.Add(comboBoxBranches, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 17);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(596, 63);
            tableLayoutPanel2.TabIndex = 14;
            //
            // tableLayoutPanel1
            //
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(groupBoxWhatToCheckout, 0, 0);
            tableLayoutPanel1.Controls.Add(openWorktreeCheckBox, 0, 2);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(folderBrowserButton1, 2, 1);
            tableLayoutPanel1.Controls.Add(newWorktreeDirectory, 1, 1);
            tableLayoutPanel1.Controls.Add(createWorktreeButton, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(608, 208);
            tableLayoutPanel1.TabIndex = 13;
            //
            // FormCreateWorktree
            //
            AcceptButton = createWorktreeButton;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(608, 208);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCreateWorktree";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create a new worktree";
            Load += FormCreateWorktree_Load;
            groupBoxWhatToCheckout.ResumeLayout(false);
            groupBoxWhatToCheckout.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Button createWorktreeButton;
        private ComboBox comboBoxBranches;
        private UserControls.FolderBrowserButton folderBrowserButton1;
        private TextBox newWorktreeDirectory;
        private Label label2;
        private CheckBox openWorktreeCheckBox;
        private RadioButton radioButtonCheckoutExistingBranch;
        private RadioButton radioButtonCreateNewBranch;
        private TextBox textBoxNewBranchName;
        private GroupBox groupBoxWhatToCheckout;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel1;
    }
}