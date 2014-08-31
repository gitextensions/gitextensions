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
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages = new System.Windows.Forms.NumericUpDown();
            this.lblCommitDialogNumberOfPreviousMessages = new System.Windows.Forms.Label();
            this.chkShowErrorsWhenStagingFiles = new System.Windows.Forms.CheckBox();
            this.chkWriteCommitMessageInCommitWindow = new System.Windows.Forms.CheckBox();
            this.grpAdditionalButtons = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkShowCommitAndPush = new System.Windows.Forms.CheckBox();
            this.chkShowResetUnstagedChanges = new System.Windows.Forms.CheckBox();
            this.chkShowResetAllChanges = new System.Windows.Forms.CheckBox();
            this.groupBoxBehaviour.SuspendLayout();
            this.tableLayoutPanelBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).BeginInit();
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
            this.groupBoxBehaviour.Size = new System.Drawing.Size(1146, 201);
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
            this.tableLayoutPanelBehaviour.Controls.Add(this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages, 1, 2);
            this.tableLayoutPanelBehaviour.Controls.Add(this.lblCommitDialogNumberOfPreviousMessages, 0, 2);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkShowErrorsWhenStagingFiles, 0, 0);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkWriteCommitMessageInCommitWindow, 0, 1);
            this.tableLayoutPanelBehaviour.Controls.Add(this.grpAdditionalButtons, 0, 4);
            this.tableLayoutPanelBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelBehaviour.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanelBehaviour.Name = "tableLayoutPanelBehaviour";
            this.tableLayoutPanelBehaviour.RowCount = 5;
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelBehaviour.Size = new System.Drawing.Size(1140, 181);
            this.tableLayoutPanelBehaviour.TabIndex = 57;
            // 
            // _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages
            // 
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Location = new System.Drawing.Point(307, 62);
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
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Size = new System.Drawing.Size(123, 21);
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.TabIndex = 3;
            this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblCommitDialogNumberOfPreviousMessages
            // 
            this.lblCommitDialogNumberOfPreviousMessages.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCommitDialogNumberOfPreviousMessages.AutoSize = true;
            this.lblCommitDialogNumberOfPreviousMessages.Location = new System.Drawing.Point(3, 66);
            this.lblCommitDialogNumberOfPreviousMessages.Name = "lblCommitDialogNumberOfPreviousMessages";
            this.lblCommitDialogNumberOfPreviousMessages.Size = new System.Drawing.Size(229, 13);
            this.lblCommitDialogNumberOfPreviousMessages.TabIndex = 2;
            this.lblCommitDialogNumberOfPreviousMessages.Text = "Number of previous messages in commit dialog";
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            this.chkShowErrorsWhenStagingFiles.AutoSize = true;
            this.chkShowErrorsWhenStagingFiles.Location = new System.Drawing.Point(3, 3);
            this.chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            this.chkShowErrorsWhenStagingFiles.Size = new System.Drawing.Size(173, 17);
            this.chkShowErrorsWhenStagingFiles.TabIndex = 0;
            this.chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            this.chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // chkWriteCommitMessageInCommitWindow
            // 
            this.chkWriteCommitMessageInCommitWindow.AutoSize = true;
            this.chkWriteCommitMessageInCommitWindow.Location = new System.Drawing.Point(3, 26);
            this.chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
            this.chkWriteCommitMessageInCommitWindow.Size = new System.Drawing.Size(298, 30);
            this.chkWriteCommitMessageInCommitWindow.TabIndex = 1;
            this.chkWriteCommitMessageInCommitWindow.Text = "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
    "d during commit)";
            this.chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
            // 
            // grpAdditionalButtons
            // 
            this.grpAdditionalButtons.AutoSize = true;
            this.grpAdditionalButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelBehaviour.SetColumnSpan(this.grpAdditionalButtons, 2);
            this.grpAdditionalButtons.Controls.Add(this.flowLayoutPanel1);
            this.grpAdditionalButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAdditionalButtons.Location = new System.Drawing.Point(3, 89);
            this.grpAdditionalButtons.Name = "grpAdditionalButtons";
            this.grpAdditionalButtons.Size = new System.Drawing.Size(1134, 89);
            this.grpAdditionalButtons.TabIndex = 5;
            this.grpAdditionalButtons.TabStop = false;
            this.grpAdditionalButtons.Text = "Show additional buttons in commit button area";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.chkShowCommitAndPush);
            this.flowLayoutPanel1.Controls.Add(this.chkShowResetUnstagedChanges);
            this.flowLayoutPanel1.Controls.Add(this.chkShowResetAllChanges);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1128, 69);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // chkShowCommitAndPush
            // 
            this.chkShowCommitAndPush.AutoSize = true;
            this.chkShowCommitAndPush.Location = new System.Drawing.Point(3, 3);
            this.chkShowCommitAndPush.Name = "chkShowCommitAndPush";
            this.chkShowCommitAndPush.Size = new System.Drawing.Size(97, 17);
            this.chkShowCommitAndPush.TabIndex = 0;
            this.chkShowCommitAndPush.Text = "Commit && Push";
            this.chkShowCommitAndPush.UseVisualStyleBackColor = true;
            // 
            // chkShowResetUnstagedChanges
            // 
            this.chkShowResetUnstagedChanges.AutoSize = true;
            this.chkShowResetUnstagedChanges.Location = new System.Drawing.Point(3, 26);
            this.chkShowResetUnstagedChanges.Name = "chkShowResetUnstagedChanges";
            this.chkShowResetUnstagedChanges.Size = new System.Drawing.Size(148, 17);
            this.chkShowResetUnstagedChanges.TabIndex = 1;
            this.chkShowResetUnstagedChanges.Text = "Reset Unstaged Changes";
            this.chkShowResetUnstagedChanges.UseVisualStyleBackColor = true;
            // 
            // chkShowResetAllChanges
            // 
            this.chkShowResetAllChanges.AutoSize = true;
            this.chkShowResetAllChanges.Location = new System.Drawing.Point(3, 49);
            this.chkShowResetAllChanges.Name = "chkShowResetAllChanges";
            this.chkShowResetAllChanges.Size = new System.Drawing.Size(113, 17);
            this.chkShowResetAllChanges.TabIndex = 2;
            this.chkShowResetAllChanges.Text = "Reset All Changes";
            this.chkShowResetAllChanges.UseVisualStyleBackColor = true;
            // 
            // CommitDialogSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxBehaviour);
            this.Name = "CommitDialogSettingsPage";
            this.Size = new System.Drawing.Size(1146, 421);
            this.groupBoxBehaviour.ResumeLayout(false);
            this.groupBoxBehaviour.PerformLayout();
            this.tableLayoutPanelBehaviour.ResumeLayout(false);
            this.tableLayoutPanelBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).EndInit();
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
        private System.Windows.Forms.CheckBox chkShowResetUnstagedChanges;
        private System.Windows.Forms.CheckBox chkShowResetAllChanges;
    }
}
