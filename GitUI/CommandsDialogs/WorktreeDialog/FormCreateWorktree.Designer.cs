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
            this.createWorktreeButton = new System.Windows.Forms.Button();
            this.textBoxNewBranchName = new System.Windows.Forms.TextBox();
            this.radioButtonCreateNewBranch = new System.Windows.Forms.RadioButton();
            this.radioButtonCheckoutExistingBranch = new System.Windows.Forms.RadioButton();
            this.openWorktreeCheckBox = new System.Windows.Forms.CheckBox();
            this.newWorktreeDirectory = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxBranches = new System.Windows.Forms.ComboBox();
            this.folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            this.groupBoxWhatToCheckout = new System.Windows.Forms.GroupBox();
            this.groupBoxWhatToCheckout.SuspendLayout();
            this.SuspendLayout();
            // 
            // createWorktreeButton
            // 
            this.createWorktreeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.createWorktreeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.createWorktreeButton.Location = new System.Drawing.Point(171, 197);
            this.createWorktreeButton.Name = "createWorktreeButton";
            this.createWorktreeButton.Size = new System.Drawing.Size(246, 28);
            this.createWorktreeButton.TabIndex = 3;
            this.createWorktreeButton.Text = "Create the new worktree";
            this.createWorktreeButton.UseVisualStyleBackColor = true;
            this.createWorktreeButton.Click += new System.EventHandler(this.createWorktreeButton_Click);
            // 
            // textBoxNewBranchName
            // 
            this.textBoxNewBranchName.Location = new System.Drawing.Point(191, 70);
            this.textBoxNewBranchName.Name = "textBoxNewBranchName";
            this.textBoxNewBranchName.Size = new System.Drawing.Size(393, 21);
            this.textBoxNewBranchName.TabIndex = 11;
            this.textBoxNewBranchName.TextChanged += new System.EventHandler(this.UpdateWorktreePathAndValidateWorktreeOptions);
            // 
            // radioButtonCreateNewBranch
            // 
            this.radioButtonCreateNewBranch.AutoSize = true;
            this.radioButtonCreateNewBranch.Location = new System.Drawing.Point(13, 71);
            this.radioButtonCreateNewBranch.Name = "radioButtonCreateNewBranch";
            this.radioButtonCreateNewBranch.Size = new System.Drawing.Size(135, 30);
            this.radioButtonCreateNewBranch.TabIndex = 10;
            this.radioButtonCreateNewBranch.TabStop = true;
            this.radioButtonCreateNewBranch.Text = "Create a new branch:\r\n (from current commit) ";
            this.radioButtonCreateNewBranch.UseVisualStyleBackColor = true;
            this.radioButtonCreateNewBranch.Click += new System.EventHandler(this.UpdateWorktreePathAndValidateWorktreeOptions);
            // 
            // radioButtonCheckoutExistingBranch
            // 
            this.radioButtonCheckoutExistingBranch.AutoSize = true;
            this.radioButtonCheckoutExistingBranch.Location = new System.Drawing.Point(13, 21);
            this.radioButtonCheckoutExistingBranch.Name = "radioButtonCheckoutExistingBranch";
            this.radioButtonCheckoutExistingBranch.Size = new System.Drawing.Size(165, 17);
            this.radioButtonCheckoutExistingBranch.TabIndex = 10;
            this.radioButtonCheckoutExistingBranch.TabStop = true;
            this.radioButtonCheckoutExistingBranch.Text = "Checkout an existing branch:";
            this.radioButtonCheckoutExistingBranch.UseVisualStyleBackColor = true;
            this.radioButtonCheckoutExistingBranch.Click += new System.EventHandler(this.UpdateWorktreePathAndValidateWorktreeOptions);
            // 
            // openWorktreeCheckBox
            // 
            this.openWorktreeCheckBox.AutoSize = true;
            this.openWorktreeCheckBox.Checked = true;
            this.openWorktreeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.openWorktreeCheckBox.Location = new System.Drawing.Point(19, 162);
            this.openWorktreeCheckBox.Name = "openWorktreeCheckBox";
            this.openWorktreeCheckBox.Size = new System.Drawing.Size(228, 17);
            this.openWorktreeCheckBox.TabIndex = 9;
            this.openWorktreeCheckBox.Text = "Open the new worktree after the creation";
            this.openWorktreeCheckBox.UseVisualStyleBackColor = true;
            // 
            // newWorktreeDirectory
            // 
            this.newWorktreeDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.newWorktreeDirectory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.newWorktreeDirectory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.newWorktreeDirectory.Location = new System.Drawing.Point(146, 121);
            this.newWorktreeDirectory.Name = "newWorktreeDirectory";
            this.newWorktreeDirectory.Size = new System.Drawing.Size(338, 21);
            this.newWorktreeDirectory.TabIndex = 7;
            this.newWorktreeDirectory.TextChanged += new System.EventHandler(this.ValidateWorktreeOptions);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "New worktree directory:";
            // 
            // comboBoxBranches
            // 
            this.comboBoxBranches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxBranches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxBranches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxBranches.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBranches.FormattingEnabled = true;
            this.comboBoxBranches.Location = new System.Drawing.Point(191, 20);
            this.comboBoxBranches.Name = "comboBoxBranches";
            this.comboBoxBranches.Size = new System.Drawing.Size(393, 21);
            this.comboBoxBranches.TabIndex = 2;
            this.comboBoxBranches.SelectedIndexChanged += new System.EventHandler(this.UpdateWorktreePathAndValidateWorktreeOptions);
            this.comboBoxBranches.SelectionChangeCommitted += new System.EventHandler(this.comboBoxBranches_SelectionChangeCommitted);
            this.comboBoxBranches.TextChanged += new System.EventHandler(this.comboBoxBranches_TextChanged);
            this.comboBoxBranches.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxBranches_KeyUp);
            // 
            // folderBrowserButton1
            // 
            this.folderBrowserButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButton1.Location = new System.Drawing.Point(488, 119);
            this.folderBrowserButton1.Name = "folderBrowserButton1";
            this.folderBrowserButton1.PathShowingControl = this.newWorktreeDirectory;
            this.folderBrowserButton1.Size = new System.Drawing.Size(110, 25);
            this.folderBrowserButton1.TabIndex = 8;
            // 
            // groupBoxWhatToCheckout
            // 
            this.groupBoxWhatToCheckout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWhatToCheckout.Controls.Add(this.textBoxNewBranchName);
            this.groupBoxWhatToCheckout.Controls.Add(this.radioButtonCreateNewBranch);
            this.groupBoxWhatToCheckout.Controls.Add(this.radioButtonCheckoutExistingBranch);
            this.groupBoxWhatToCheckout.Controls.Add(this.comboBoxBranches);
            this.groupBoxWhatToCheckout.Location = new System.Drawing.Point(6, 3);
            this.groupBoxWhatToCheckout.Name = "groupBoxWhatToCheckout";
            this.groupBoxWhatToCheckout.Size = new System.Drawing.Size(590, 112);
            this.groupBoxWhatToCheckout.TabIndex = 12;
            this.groupBoxWhatToCheckout.TabStop = false;
            this.groupBoxWhatToCheckout.Text = "What to checkout: ";
            // 
            // FormCreateWorktree
            // 
            this.AcceptButton = this.createWorktreeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(604, 236);
            this.Controls.Add(this.groupBoxWhatToCheckout);
            this.Controls.Add(this.openWorktreeCheckBox);
            this.Controls.Add(this.folderBrowserButton1);
            this.Controls.Add(this.newWorktreeDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.createWorktreeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCreateWorktree";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create a new worktree";
            this.Load += new System.EventHandler(this.FormCreateWorktree_Load);
            this.groupBoxWhatToCheckout.ResumeLayout(false);
            this.groupBoxWhatToCheckout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createWorktreeButton;
        private System.Windows.Forms.ComboBox comboBoxBranches;
        private UserControls.FolderBrowserButton folderBrowserButton1;
        private System.Windows.Forms.TextBox newWorktreeDirectory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox openWorktreeCheckBox;
        private System.Windows.Forms.RadioButton radioButtonCheckoutExistingBranch;
        private System.Windows.Forms.RadioButton radioButtonCreateNewBranch;
        private System.Windows.Forms.TextBox textBoxNewBranchName;
        private System.Windows.Forms.GroupBox groupBoxWhatToCheckout;
    }
}