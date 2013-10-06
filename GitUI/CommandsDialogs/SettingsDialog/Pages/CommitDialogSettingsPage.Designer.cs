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
            this.lblCommitDialogNumberOfPreviousMessages = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages = new System.Windows.Forms.NumericUpDown();
            this.chkWriteCommitMessageInCommitWindow = new System.Windows.Forms.CheckBox();
            this.chkShowErrorsWhenStagingFiles = new System.Windows.Forms.CheckBox();
            this.groupBoxBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxBehaviour
            // 
            this.groupBoxBehaviour.Controls.Add(this.lblCommitDialogNumberOfPreviousMessages);
            this.groupBoxBehaviour.Controls.Add(this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages);
            this.groupBoxBehaviour.Controls.Add(this.chkWriteCommitMessageInCommitWindow);
            this.groupBoxBehaviour.Controls.Add(this.chkShowErrorsWhenStagingFiles);
            this.groupBoxBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxBehaviour.Location = new System.Drawing.Point(0, 0);
            this.groupBoxBehaviour.Name = "groupBoxBehaviour";
            this.groupBoxBehaviour.Size = new System.Drawing.Size(1303, 123);
            this.groupBoxBehaviour.TabIndex = 56;
            this.groupBoxBehaviour.TabStop = false;
            this.groupBoxBehaviour.Text = "Behaviour";
            // 
            // lblCommitDialogNumberOfPreviousMessages
            // 
            this.lblCommitDialogNumberOfPreviousMessages.AutoSize = true;
            this.lblCommitDialogNumberOfPreviousMessages.Location = new System.Drawing.Point(7, 86);
            this.lblCommitDialogNumberOfPreviousMessages.Name = "lblCommitDialogNumberOfPreviousMessages";
            this.lblCommitDialogNumberOfPreviousMessages.Size = new System.Drawing.Size(261, 15);
            this.lblCommitDialogNumberOfPreviousMessages.TabIndex = 9;
            this.lblCommitDialogNumberOfPreviousMessages.Text = "Number of previous messages in commit dialog";
            // 
            // _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages
            // 
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Location = new System.Drawing.Point(320, 84);
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Name = "_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages";
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Size = new System.Drawing.Size(123, 23);
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.TabIndex = 10;
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // chkWriteCommitMessageInCommitWindow
            // 
            this.chkWriteCommitMessageInCommitWindow.AutoSize = true;
            this.chkWriteCommitMessageInCommitWindow.Location = new System.Drawing.Point(10, 47);
            this.chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
            this.chkWriteCommitMessageInCommitWindow.Size = new System.Drawing.Size(329, 34);
            this.chkWriteCommitMessageInCommitWindow.TabIndex = 8;
            this.chkWriteCommitMessageInCommitWindow.Text = "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
    "d during commit)";
            this.chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            this.chkShowErrorsWhenStagingFiles.AutoSize = true;
            this.chkShowErrorsWhenStagingFiles.Location = new System.Drawing.Point(10, 22);
            this.chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            this.chkShowErrorsWhenStagingFiles.Size = new System.Drawing.Size(186, 19);
            this.chkShowErrorsWhenStagingFiles.TabIndex = 3;
            this.chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            this.chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // CommitDialogSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxBehaviour);
            this.Name = "CommitDialogSettingsPage";
            this.Size = new System.Drawing.Size(1303, 856);
            this.groupBoxBehaviour.ResumeLayout(false);
            this.groupBoxBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBehaviour;
        private System.Windows.Forms.CheckBox chkWriteCommitMessageInCommitWindow;
        private System.Windows.Forms.CheckBox chkShowErrorsWhenStagingFiles;
        private System.Windows.Forms.Label lblCommitDialogNumberOfPreviousMessages;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages;
    }
}
