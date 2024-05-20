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
            if (disposing && (components is not null))
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
            groupBoxBehaviour = new GroupBox();
            tableLayoutPanelBehaviour = new TableLayoutPanel();
            cbRememberAmendCommitState = new CheckBox();
            chkAutocomplete = new CheckBox();
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages =
                new NumericUpDown();
            lblCommitDialogNumberOfPreviousMessages = new Label();
            chkShowErrorsWhenStagingFiles = new CheckBox();
            chkWriteCommitMessageInCommitWindow = new CheckBox();
            grpAdditionalButtons = new GroupBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            chkShowCommitAndPush = new CheckBox();
            chkShowResetWorkTreeChanges = new CheckBox();
            chkShowResetAllChanges = new CheckBox();
            chkEnsureCommitMessageSecondLineEmpty = new CheckBox();
            groupBoxBehaviour.SuspendLayout();
            tableLayoutPanelBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this
                ._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).BeginInit();
            grpAdditionalButtons.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxBehaviour
            // 
            groupBoxBehaviour.AutoSize = true;
            groupBoxBehaviour.Controls.Add(tableLayoutPanelBehaviour);
            groupBoxBehaviour.Dock = DockStyle.Top;
            groupBoxBehaviour.Location = new Point(0, 0);
            groupBoxBehaviour.Name = "groupBoxBehaviour";
            groupBoxBehaviour.Size = new Size(1014, 294);
            groupBoxBehaviour.TabIndex = 56;
            groupBoxBehaviour.TabStop = false;
            groupBoxBehaviour.Text = "Behaviour";
            // 
            // tableLayoutPanelBehaviour
            // 
            tableLayoutPanelBehaviour.AutoSize = true;
            tableLayoutPanelBehaviour.ColumnCount = 2;
            tableLayoutPanelBehaviour.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelBehaviour.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelBehaviour.Controls.Add(cbRememberAmendCommitState, 0, 6);
            tableLayoutPanelBehaviour.Controls.Add(chkAutocomplete, 0, 0);
            tableLayoutPanelBehaviour.Controls.Add(
                _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages, 1, 4);
            tableLayoutPanelBehaviour.Controls.Add(
                lblCommitDialogNumberOfPreviousMessages, 0, 4);
            tableLayoutPanelBehaviour.Controls.Add(chkShowErrorsWhenStagingFiles, 0, 1);
            tableLayoutPanelBehaviour.Controls.Add(chkWriteCommitMessageInCommitWindow, 0,
                3);
            tableLayoutPanelBehaviour.Controls.Add(grpAdditionalButtons, 0, 7);
            tableLayoutPanelBehaviour.Controls.Add(chkEnsureCommitMessageSecondLineEmpty,
                0, 2);
            tableLayoutPanelBehaviour.Dock = DockStyle.Top;
            tableLayoutPanelBehaviour.Location = new Point(3, 19);
            tableLayoutPanelBehaviour.Name = "tableLayoutPanelBehaviour";
            tableLayoutPanelBehaviour.RowCount = 8;
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
            tableLayoutPanelBehaviour.Size = new Size(1008, 272);
            tableLayoutPanelBehaviour.TabIndex = 57;
            // 
            // cbRememberAmendCommitState
            // 
            cbRememberAmendCommitState.AutoSize = true;
            cbRememberAmendCommitState.Location = new Point(3, 147);
            cbRememberAmendCommitState.Name = "cbRememberAmendCommitState";
            cbRememberAmendCommitState.Size = new Size(351, 19);
            cbRememberAmendCommitState.TabIndex = 5;
            cbRememberAmendCommitState.Text =
                "Remember \'Amend commit\' checkbox on commit form close";
            cbRememberAmendCommitState.UseVisualStyleBackColor = true;
            // 
            // chkAutocomplete
            // 
            chkAutocomplete.AutoSize = true;
            chkAutocomplete.Location = new Point(3, 3);
            chkAutocomplete.Name = "chkAutocomplete";
            chkAutocomplete.Size = new Size(253, 19);
            chkAutocomplete.TabIndex = 0;
            chkAutocomplete.Text = "Provide auto-completion in commit dialog";
            chkAutocomplete.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages
            // 
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Location =
                new Point(360, 118);
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Maximum =
                new decimal(new int[] { 999, 0, 0, 0 });
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Minimum =
                new decimal(new int[] { 1, 0, 0, 0 });
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Name =
                "_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages";
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Size =
                new Size(123, 23);
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.TabIndex = 4;
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value =
                new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblCommitDialogNumberOfPreviousMessages
            // 
            lblCommitDialogNumberOfPreviousMessages.Anchor =
                AnchorStyles.Left;
            lblCommitDialogNumberOfPreviousMessages.AutoSize = true;
            lblCommitDialogNumberOfPreviousMessages.Location =
                new Point(3, 122);
            lblCommitDialogNumberOfPreviousMessages.Name =
                "lblCommitDialogNumberOfPreviousMessages";
            lblCommitDialogNumberOfPreviousMessages.Size = new Size(261, 15);
            lblCommitDialogNumberOfPreviousMessages.TabIndex = 2;
            lblCommitDialogNumberOfPreviousMessages.Text =
                "Number of previous messages in commit dialog";
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            chkShowErrorsWhenStagingFiles.AutoSize = true;
            chkShowErrorsWhenStagingFiles.Location = new Point(3, 28);
            chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            chkShowErrorsWhenStagingFiles.Size = new Size(186, 19);
            chkShowErrorsWhenStagingFiles.TabIndex = 1;
            chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // chkWriteCommitMessageInCommitWindow
            // 
            chkWriteCommitMessageInCommitWindow.AutoSize = true;
            chkWriteCommitMessageInCommitWindow.Location = new Point(3, 78);
            chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
            chkWriteCommitMessageInCommitWindow.Size = new Size(329, 34);
            chkWriteCommitMessageInCommitWindow.TabIndex = 3;
            chkWriteCommitMessageInCommitWindow.Text =
                "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
                "d during commit)";
            chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
            // 
            // grpAdditionalButtons
            // 
            grpAdditionalButtons.AutoSize = true;
            grpAdditionalButtons.AutoSizeMode =
                AutoSizeMode.GrowAndShrink;
            tableLayoutPanelBehaviour.SetColumnSpan(grpAdditionalButtons, 2);
            grpAdditionalButtons.Controls.Add(flowLayoutPanel1);
            grpAdditionalButtons.Dock = DockStyle.Fill;
            grpAdditionalButtons.Location = new Point(3, 172);
            grpAdditionalButtons.Name = "grpAdditionalButtons";
            grpAdditionalButtons.Size = new Size(1002, 97);
            grpAdditionalButtons.TabIndex = 6;
            grpAdditionalButtons.TabStop = false;
            grpAdditionalButtons.Text = "Show additional buttons in commit button area";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(chkShowCommitAndPush);
            flowLayoutPanel1.Controls.Add(chkShowResetWorkTreeChanges);
            flowLayoutPanel1.Controls.Add(chkShowResetAllChanges);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(3, 19);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(996, 75);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // chkShowCommitAndPush
            // 
            chkShowCommitAndPush.AutoSize = true;
            chkShowCommitAndPush.Location = new Point(3, 3);
            chkShowCommitAndPush.Name = "chkShowCommitAndPush";
            chkShowCommitAndPush.Size = new Size(112, 19);
            chkShowCommitAndPush.TabIndex = 0;
            chkShowCommitAndPush.Text = "Commit && Push";
            chkShowCommitAndPush.UseVisualStyleBackColor = true;
            // 
            // chkShowResetWorkTreeChanges
            // 
            chkShowResetWorkTreeChanges.AutoSize = true;
            chkShowResetWorkTreeChanges.Location = new Point(3, 28);
            chkShowResetWorkTreeChanges.Name = "chkShowResetWorkTreeChanges";
            chkShowResetWorkTreeChanges.Size = new Size(156, 19);
            chkShowResetWorkTreeChanges.TabIndex = 1;
            chkShowResetWorkTreeChanges.Text = "Reset Unstaged Changes";
            chkShowResetWorkTreeChanges.UseVisualStyleBackColor = true;
            // 
            // chkShowResetAllChanges
            // 
            chkShowResetAllChanges.AutoSize = true;
            chkShowResetAllChanges.Location = new Point(3, 53);
            chkShowResetAllChanges.Name = "chkShowResetAllChanges";
            chkShowResetAllChanges.Size = new Size(120, 19);
            chkShowResetAllChanges.TabIndex = 2;
            chkShowResetAllChanges.Text = "Reset All Changes";
            chkShowResetAllChanges.UseVisualStyleBackColor = true;
            // 
            // chkEnsureCommitMessageSecondLineEmpty
            // 
            chkEnsureCommitMessageSecondLineEmpty.AutoSize = true;
            chkEnsureCommitMessageSecondLineEmpty.Location = new Point(3, 53);
            chkEnsureCommitMessageSecondLineEmpty.Name =
                "chkEnsureCommitMessageSecondLineEmpty";
            chkEnsureCommitMessageSecondLineEmpty.Size = new Size(300, 19);
            chkEnsureCommitMessageSecondLineEmpty.TabIndex = 2;
            chkEnsureCommitMessageSecondLineEmpty.Text =
                "Ensure the second line of commit message is empty";
            chkEnsureCommitMessageSecondLineEmpty.UseVisualStyleBackColor = true;
            // 
            // CommitDialogSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            Controls.Add(groupBoxBehaviour);
            Name = "CommitDialogSettingsPage";
            Size = new Size(1014, 950);
            Text = "Commit dialog";
            groupBoxBehaviour.ResumeLayout(false);
            groupBoxBehaviour.PerformLayout();
            tableLayoutPanelBehaviour.ResumeLayout(false);
            tableLayoutPanelBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this
                ._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).EndInit();
            grpAdditionalButtons.ResumeLayout(false);
            grpAdditionalButtons.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxBehaviour;
        private CheckBox chkWriteCommitMessageInCommitWindow;
        private CheckBox chkShowErrorsWhenStagingFiles;
        private Label lblCommitDialogNumberOfPreviousMessages;
        private NumericUpDown _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages;
        private TableLayoutPanel tableLayoutPanelBehaviour;
        private GroupBox grpAdditionalButtons;
        private FlowLayoutPanel flowLayoutPanel1;
        private CheckBox chkShowCommitAndPush;
        private CheckBox chkShowResetWorkTreeChanges;
        private CheckBox chkShowResetAllChanges;
        private CheckBox chkEnsureCommitMessageSecondLineEmpty;
        private CheckBox chkAutocomplete;
        private CheckBox cbRememberAmendCommitState;
    }
}
