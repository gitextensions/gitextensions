namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class CommitDialogSettingsPage
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxBehaviour = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelBehaviour = new System.Windows.Forms.TableLayoutPanel();
            this.cbRememberAmendCommitState = new System.Windows.Forms.CheckBox();
            this.chkAutocomplete = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages =
                new System.Windows.Forms.NumericUpDown();
            this.lblCommitDialogNumberOfPreviousMessages = new System.Windows.Forms.Label();
            this.chkShowErrorsWhenStagingFiles = new System.Windows.Forms.CheckBox();
            this.chkWriteCommitMessageInCommitWindow = new System.Windows.Forms.CheckBox();
            this.grpAdditionalButtons = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkShowCommitAndPush = new System.Windows.Forms.CheckBox();
            this.chkShowResetWorkTreeChanges = new System.Windows.Forms.CheckBox();
            this.chkShowResetAllChanges = new System.Windows.Forms.CheckBox();
            this.chkEnsureCommitMessageSecondLineEmpty = new System.Windows.Forms.CheckBox();
            this.groupBoxBehaviour.SuspendLayout();
            this.tableLayoutPanelBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this
                ._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).BeginInit();
            this.grpAdditionalButtons.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxBehaviour
            // 
            this.groupBoxBehaviour.AutoSize = true;
            this.groupBoxBehaviour.Controls.Add(this.tableLayoutPanelBehaviour);
            this.groupBoxBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxBehaviour.Location = new System.Drawing.Point(0, 0);
            this.groupBoxBehaviour.Name = "groupBoxBehaviour";
            this.groupBoxBehaviour.Size = new System.Drawing.Size(1014, 294);
            this.groupBoxBehaviour.TabIndex = 56;
            this.groupBoxBehaviour.TabStop = false;
            this.groupBoxBehaviour.Text = "Behaviour";
            // 
            // tableLayoutPanelBehaviour
            // 
            this.tableLayoutPanelBehaviour.AutoSize = true;
            this.tableLayoutPanelBehaviour.ColumnCount = 2;
            this.tableLayoutPanelBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBehaviour.Controls.Add(this.cbRememberAmendCommitState, 0, 6);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkAutocomplete, 0, 0);
            this.tableLayoutPanelBehaviour.Controls.Add(
                this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages, 1, 4);
            this.tableLayoutPanelBehaviour.Controls.Add(
                this.lblCommitDialogNumberOfPreviousMessages, 0, 4);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkShowErrorsWhenStagingFiles, 0, 1);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkWriteCommitMessageInCommitWindow, 0,
                3);
            this.tableLayoutPanelBehaviour.Controls.Add(this.grpAdditionalButtons, 0, 7);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkEnsureCommitMessageSecondLineEmpty,
                0, 2);
            this.tableLayoutPanelBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelBehaviour.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanelBehaviour.Name = "tableLayoutPanelBehaviour";
            this.tableLayoutPanelBehaviour.RowCount = 8;
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.Size = new System.Drawing.Size(1008, 272);
            this.tableLayoutPanelBehaviour.TabIndex = 57;
            // 
            // cbRememberAmendCommitState
            // 
            this.cbRememberAmendCommitState.AutoSize = true;
            this.cbRememberAmendCommitState.Location = new System.Drawing.Point(3, 147);
            this.cbRememberAmendCommitState.Name = "cbRememberAmendCommitState";
            this.cbRememberAmendCommitState.Size = new System.Drawing.Size(351, 19);
            this.cbRememberAmendCommitState.TabIndex = 5;
            this.cbRememberAmendCommitState.Text =
                "Remember \'Amend commit\' checkbox on commit form close";
            this.cbRememberAmendCommitState.UseVisualStyleBackColor = true;
            // 
            // chkAutocomplete
            // 
            this.chkAutocomplete.AutoSize = true;
            this.chkAutocomplete.Location = new System.Drawing.Point(3, 3);
            this.chkAutocomplete.Name = "chkAutocomplete";
            this.chkAutocomplete.Size = new System.Drawing.Size(253, 19);
            this.chkAutocomplete.TabIndex = 0;
            this.chkAutocomplete.Text = "Provide auto-completion in commit dialog";
            this.chkAutocomplete.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages
            // 
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Location =
                new System.Drawing.Point(360, 118);
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Maximum =
                new decimal(new int[] {999, 0, 0, 0});
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Minimum =
                new decimal(new int[] {1, 0, 0, 0});
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Name =
                "_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages";
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Size =
                new System.Drawing.Size(123, 23);
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.TabIndex = 4;
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value =
                new decimal(new int[] {1, 0, 0, 0});
            // 
            // lblCommitDialogNumberOfPreviousMessages
            // 
            this.lblCommitDialogNumberOfPreviousMessages.Anchor =
                System.Windows.Forms.AnchorStyles.Left;
            this.lblCommitDialogNumberOfPreviousMessages.AutoSize = true;
            this.lblCommitDialogNumberOfPreviousMessages.Location =
                new System.Drawing.Point(3, 122);
            this.lblCommitDialogNumberOfPreviousMessages.Name =
                "lblCommitDialogNumberOfPreviousMessages";
            this.lblCommitDialogNumberOfPreviousMessages.Size = new System.Drawing.Size(261, 15);
            this.lblCommitDialogNumberOfPreviousMessages.TabIndex = 2;
            this.lblCommitDialogNumberOfPreviousMessages.Text =
                "Number of previous messages in commit dialog";
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            this.chkShowErrorsWhenStagingFiles.AutoSize = true;
            this.chkShowErrorsWhenStagingFiles.Location = new System.Drawing.Point(3, 28);
            this.chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            this.chkShowErrorsWhenStagingFiles.Size = new System.Drawing.Size(186, 19);
            this.chkShowErrorsWhenStagingFiles.TabIndex = 1;
            this.chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            this.chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // chkWriteCommitMessageInCommitWindow
            // 
            this.chkWriteCommitMessageInCommitWindow.AutoSize = true;
            this.chkWriteCommitMessageInCommitWindow.Location = new System.Drawing.Point(3, 78);
            this.chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
            this.chkWriteCommitMessageInCommitWindow.Size = new System.Drawing.Size(329, 34);
            this.chkWriteCommitMessageInCommitWindow.TabIndex = 3;
            this.chkWriteCommitMessageInCommitWindow.Text =
                "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
                "d during commit)";
            this.chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
            // 
            // grpAdditionalButtons
            // 
            this.grpAdditionalButtons.AutoSize = true;
            this.grpAdditionalButtons.AutoSizeMode =
                System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelBehaviour.SetColumnSpan(this.grpAdditionalButtons, 2);
            this.grpAdditionalButtons.Controls.Add(this.flowLayoutPanel1);
            this.grpAdditionalButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAdditionalButtons.Location = new System.Drawing.Point(3, 172);
            this.grpAdditionalButtons.Name = "grpAdditionalButtons";
            this.grpAdditionalButtons.Size = new System.Drawing.Size(1002, 97);
            this.grpAdditionalButtons.TabIndex = 6;
            this.grpAdditionalButtons.TabStop = false;
            this.grpAdditionalButtons.Text = "Show additional buttons in commit button area";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.chkShowCommitAndPush);
            this.flowLayoutPanel1.Controls.Add(this.chkShowResetWorkTreeChanges);
            this.flowLayoutPanel1.Controls.Add(this.chkShowResetAllChanges);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 19);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(996, 75);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // chkShowCommitAndPush
            // 
            this.chkShowCommitAndPush.AutoSize = true;
            this.chkShowCommitAndPush.Location = new System.Drawing.Point(3, 3);
            this.chkShowCommitAndPush.Name = "chkShowCommitAndPush";
            this.chkShowCommitAndPush.Size = new System.Drawing.Size(112, 19);
            this.chkShowCommitAndPush.TabIndex = 0;
            this.chkShowCommitAndPush.Text = "Commit && Push";
            this.chkShowCommitAndPush.UseVisualStyleBackColor = true;
            // 
            // chkShowResetWorkTreeChanges
            // 
            this.chkShowResetWorkTreeChanges.AutoSize = true;
            this.chkShowResetWorkTreeChanges.Location = new System.Drawing.Point(3, 28);
            this.chkShowResetWorkTreeChanges.Name = "chkShowResetWorkTreeChanges";
            this.chkShowResetWorkTreeChanges.Size = new System.Drawing.Size(156, 19);
            this.chkShowResetWorkTreeChanges.TabIndex = 1;
            this.chkShowResetWorkTreeChanges.Text = "Reset Unstaged Changes";
            this.chkShowResetWorkTreeChanges.UseVisualStyleBackColor = true;
            // 
            // chkShowResetAllChanges
            // 
            this.chkShowResetAllChanges.AutoSize = true;
            this.chkShowResetAllChanges.Location = new System.Drawing.Point(3, 53);
            this.chkShowResetAllChanges.Name = "chkShowResetAllChanges";
            this.chkShowResetAllChanges.Size = new System.Drawing.Size(120, 19);
            this.chkShowResetAllChanges.TabIndex = 2;
            this.chkShowResetAllChanges.Text = "Reset All Changes";
            this.chkShowResetAllChanges.UseVisualStyleBackColor = true;
            // 
            // chkEnsureCommitMessageSecondLineEmpty
            // 
            this.chkEnsureCommitMessageSecondLineEmpty.AutoSize = true;
            this.chkEnsureCommitMessageSecondLineEmpty.Location = new System.Drawing.Point(3, 53);
            this.chkEnsureCommitMessageSecondLineEmpty.Name =
                "chkEnsureCommitMessageSecondLineEmpty";
            this.chkEnsureCommitMessageSecondLineEmpty.Size = new System.Drawing.Size(300, 19);
            this.chkEnsureCommitMessageSecondLineEmpty.TabIndex = 2;
            this.chkEnsureCommitMessageSecondLineEmpty.Text =
                "Ensure the second line of commit message is empty";
            this.chkEnsureCommitMessageSecondLineEmpty.UseVisualStyleBackColor = true;
            // 
            // CommitDialogSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxBehaviour);
            this.Name = "CommitDialogSettingsPage";
            this.Size = new System.Drawing.Size(1014, 950);
            this.groupBoxBehaviour.ResumeLayout(false);
            this.groupBoxBehaviour.PerformLayout();
            this.tableLayoutPanelBehaviour.ResumeLayout(false);
            this.tableLayoutPanelBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this
                ._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).EndInit();
            this.grpAdditionalButtons.ResumeLayout(false);
            this.grpAdditionalButtons.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBehaviour;
        private System.Windows.Forms.CheckBox chkWriteCommitMessageInCommitWindow;
        private System.Windows.Forms.CheckBox chkShowErrorsWhenStagingFiles;
        private System.Windows.Forms.Label lblCommitDialogNumberOfPreviousMessages;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBehaviour;
        private System.Windows.Forms.GroupBox grpAdditionalButtons;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox chkShowCommitAndPush;
        private System.Windows.Forms.CheckBox chkShowResetWorkTreeChanges;
        private System.Windows.Forms.CheckBox chkShowResetAllChanges;
        private System.Windows.Forms.CheckBox chkEnsureCommitMessageSecondLineEmpty;
        private System.Windows.Forms.CheckBox chkAutocomplete;
        private System.Windows.Forms.CheckBox cbRememberAmendCommitState;
    }
}
