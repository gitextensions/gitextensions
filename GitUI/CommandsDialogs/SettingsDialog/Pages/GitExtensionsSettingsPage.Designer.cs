namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GitExtensionsSettingsPage
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
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.chkWriteCommitMessageInCommitWindow = new System.Windows.Forms.CheckBox();
            this.chkPlaySpecialStartupSound = new System.Windows.Forms.CheckBox();
            this.chkCloseProcessDialog = new System.Windows.Forms.CheckBox();
            this.chkShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.chkStartWithRecentWorkingDir = new System.Windows.Forms.CheckBox();
            this.label23 = new System.Windows.Forms.Label();
            this.SmtpServer = new System.Windows.Forms.TextBox();
            this.RevisionGridQuickSearchTimeout = new System.Windows.Forms.NumericUpDown();
            this.chkStashUntrackedFiles = new System.Windows.Forms.CheckBox();
            this.label24 = new System.Windows.Forms.Label();
            this.chkUsePatienceDiffAlgorithm = new System.Windows.Forms.CheckBox();
            this.chkShowErrorsWhenStagingFiles = new System.Windows.Forms.CheckBox();
            this.chkFollowRenamesInFileHistory = new System.Windows.Forms.CheckBox();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.chkCheckForUncommittedChangesInCheckoutBranch = new System.Windows.Forms.CheckBox();
            this.chkShowGitStatusInToolbar = new System.Windows.Forms.CheckBox();
            this.chkShowCurrentChangesInRevisionGraph = new System.Windows.Forms.CheckBox();
            this.chkAutoRefreshRevisionGrid = new System.Windows.Forms.CheckBox();
            this.chkUseFastChecks = new System.Windows.Forms.CheckBox();
            this.chkShowStashCountInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.groupBox12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox12
            // 
            this.groupBox12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox12.Controls.Add(this.chkWriteCommitMessageInCommitWindow);
            this.groupBox12.Controls.Add(this.chkPlaySpecialStartupSound);
            this.groupBox12.Controls.Add(this.chkCloseProcessDialog);
            this.groupBox12.Controls.Add(this.chkShowGitCommandLine);
            this.groupBox12.Controls.Add(this.chkStartWithRecentWorkingDir);
            this.groupBox12.Controls.Add(this.label23);
            this.groupBox12.Controls.Add(this.SmtpServer);
            this.groupBox12.Controls.Add(this.RevisionGridQuickSearchTimeout);
            this.groupBox12.Controls.Add(this.chkStashUntrackedFiles);
            this.groupBox12.Controls.Add(this.label24);
            this.groupBox12.Controls.Add(this.chkUsePatienceDiffAlgorithm);
            this.groupBox12.Controls.Add(this.chkShowErrorsWhenStagingFiles);
            this.groupBox12.Controls.Add(this.chkFollowRenamesInFileHistory);
            this.groupBox12.Location = new System.Drawing.Point(3, 206);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(581, 288);
            this.groupBox12.TabIndex = 1;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Behaviour";
            // 
            // chkWriteCommitMessageInCommitWindow
            // 
            this.chkWriteCommitMessageInCommitWindow.AutoSize = true;
            this.chkWriteCommitMessageInCommitWindow.Location = new System.Drawing.Point(10, 184);
            this.chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
            this.chkWriteCommitMessageInCommitWindow.Size = new System.Drawing.Size(329, 34);
            this.chkWriteCommitMessageInCommitWindow.TabIndex = 8;
            this.chkWriteCommitMessageInCommitWindow.Text = "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
    "d during commit)";
            this.chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
            // 
            // chkPlaySpecialStartupSound
            // 
            this.chkPlaySpecialStartupSound.AutoSize = true;
            this.chkPlaySpecialStartupSound.Location = new System.Drawing.Point(320, 162);
            this.chkPlaySpecialStartupSound.Name = "chkPlaySpecialStartupSound";
            this.chkPlaySpecialStartupSound.Size = new System.Drawing.Size(166, 19);
            this.chkPlaySpecialStartupSound.TabIndex = 7;
            this.chkPlaySpecialStartupSound.Text = "Play Special Startup Sound";
            this.chkPlaySpecialStartupSound.UseVisualStyleBackColor = true;
            // 
            // chkCloseProcessDialog
            // 
            this.chkCloseProcessDialog.AutoSize = true;
            this.chkCloseProcessDialog.Location = new System.Drawing.Point(10, 30);
            this.chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            this.chkCloseProcessDialog.Size = new System.Drawing.Size(260, 19);
            this.chkCloseProcessDialog.TabIndex = 0;
            this.chkCloseProcessDialog.Text = "Close Process dialog when process succeeds";
            this.chkCloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // chkShowGitCommandLine
            // 
            this.chkShowGitCommandLine.AutoSize = true;
            this.chkShowGitCommandLine.Location = new System.Drawing.Point(10, 52);
            this.chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            this.chkShowGitCommandLine.Size = new System.Drawing.Size(290, 19);
            this.chkShowGitCommandLine.TabIndex = 1;
            this.chkShowGitCommandLine.Text = "Show console window when executing git process";
            this.chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            this.chkStartWithRecentWorkingDir.AutoSize = true;
            this.chkStartWithRecentWorkingDir.Location = new System.Drawing.Point(10, 162);
            this.chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            this.chkStartWithRecentWorkingDir.Size = new System.Drawing.Size(196, 19);
            this.chkStartWithRecentWorkingDir.TabIndex = 6;
            this.chkStartWithRecentWorkingDir.Text = "Open last working dir on startup";
            this.chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(7, 263);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(227, 15);
            this.label23.TabIndex = 11;
            this.label23.Text = "SMTP server for sending patches by email";
            // 
            // SmtpServer
            // 
            this.SmtpServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SmtpServer.Location = new System.Drawing.Point(316, 259);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(242, 23);
            this.SmtpServer.TabIndex = 12;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            this.RevisionGridQuickSearchTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(316, 228);
            this.RevisionGridQuickSearchTimeout.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Name = "RevisionGridQuickSearchTimeout";
            this.RevisionGridQuickSearchTimeout.Size = new System.Drawing.Size(123, 23);
            this.RevisionGridQuickSearchTimeout.TabIndex = 10;
            this.RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // chkStashUntrackedFiles
            // 
            this.chkStashUntrackedFiles.AutoSize = true;
            this.chkStashUntrackedFiles.Location = new System.Drawing.Point(10, 118);
            this.chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            this.chkStashUntrackedFiles.Size = new System.Drawing.Size(188, 19);
            this.chkStashUntrackedFiles.TabIndex = 4;
            this.chkStashUntrackedFiles.Text = "Include untracked files in stash";
            this.chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(7, 232);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(216, 15);
            this.label24.TabIndex = 9;
            this.label24.Text = "Revision grid quick search timeout [ms]";
            // 
            // chkUsePatienceDiffAlgorithm
            // 
            this.chkUsePatienceDiffAlgorithm.AutoSize = true;
            this.chkUsePatienceDiffAlgorithm.Location = new System.Drawing.Point(10, 74);
            this.chkUsePatienceDiffAlgorithm.Name = "chkUsePatienceDiffAlgorithm";
            this.chkUsePatienceDiffAlgorithm.Size = new System.Drawing.Size(169, 19);
            this.chkUsePatienceDiffAlgorithm.TabIndex = 2;
            this.chkUsePatienceDiffAlgorithm.Text = "Use patience diff algorithm";
            this.chkUsePatienceDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            this.chkShowErrorsWhenStagingFiles.AutoSize = true;
            this.chkShowErrorsWhenStagingFiles.Location = new System.Drawing.Point(10, 96);
            this.chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            this.chkShowErrorsWhenStagingFiles.Size = new System.Drawing.Size(186, 19);
            this.chkShowErrorsWhenStagingFiles.TabIndex = 3;
            this.chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            this.chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            this.chkFollowRenamesInFileHistory.AutoSize = true;
            this.chkFollowRenamesInFileHistory.Location = new System.Drawing.Point(10, 140);
            this.chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            this.chkFollowRenamesInFileHistory.Size = new System.Drawing.Size(259, 19);
            this.chkFollowRenamesInFileHistory.TabIndex = 5;
            this.chkFollowRenamesInFileHistory.Text = "Follow renames in file history (experimental)";
            this.chkFollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // groupBox11
            // 
            this.groupBox11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox11.Controls.Add(this.chkCheckForUncommittedChangesInCheckoutBranch);
            this.groupBox11.Controls.Add(this.chkShowGitStatusInToolbar);
            this.groupBox11.Controls.Add(this.chkShowCurrentChangesInRevisionGraph);
            this.groupBox11.Controls.Add(this.chkAutoRefreshRevisionGrid);
            this.groupBox11.Controls.Add(this.chkUseFastChecks);
            this.groupBox11.Controls.Add(this.chkShowStashCountInBrowseWindow);
            this.groupBox11.Controls.Add(this.label12);
            this.groupBox11.Controls.Add(this._NO_TRANSLATE_MaxCommits);
            this.groupBox11.Location = new System.Drawing.Point(3, 3);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(581, 197);
            this.groupBox11.TabIndex = 0;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Performance";
            // 
            // chkCheckForUncommittedChangesInCheckoutBranch
            // 
            this.chkCheckForUncommittedChangesInCheckoutBranch.AutoSize = true;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Location = new System.Drawing.Point(11, 133);
            this.chkCheckForUncommittedChangesInCheckoutBranch.Name = "chkCheckForUncommittedChangesInCheckoutBranch";
            this.chkCheckForUncommittedChangesInCheckoutBranch.Size = new System.Drawing.Size(341, 19);
            this.chkCheckForUncommittedChangesInCheckoutBranch.TabIndex = 5;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Text = "Check for uncommitted changes in checkout branch dialog";
            this.chkCheckForUncommittedChangesInCheckoutBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowGitStatusInToolbar
            // 
            this.chkShowGitStatusInToolbar.AutoSize = true;
            this.chkShowGitStatusInToolbar.Location = new System.Drawing.Point(11, 23);
            this.chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            this.chkShowGitStatusInToolbar.Size = new System.Drawing.Size(489, 19);
            this.chkShowGitStatusInToolbar.TabIndex = 0;
            this.chkShowGitStatusInToolbar.Text = "Show repository status in browse dialog (number of changes in toolbar, restart re" +
    "quired)";
            this.chkShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentChangesInRevisionGraph
            // 
            this.chkShowCurrentChangesInRevisionGraph.AutoSize = true;
            this.chkShowCurrentChangesInRevisionGraph.Location = new System.Drawing.Point(11, 45);
            this.chkShowCurrentChangesInRevisionGraph.Name = "chkShowCurrentChangesInRevisionGraph";
            this.chkShowCurrentChangesInRevisionGraph.Size = new System.Drawing.Size(335, 19);
            this.chkShowCurrentChangesInRevisionGraph.TabIndex = 1;
            this.chkShowCurrentChangesInRevisionGraph.Text = "Show current working dir changes in revision graph (slow!)";
            this.chkShowCurrentChangesInRevisionGraph.UseVisualStyleBackColor = true;
            // 
            // chkAutoRefreshRevisionGrid
            // 
            this.chkAutoRefreshRevisionGrid.AutoSize = true;
            this.chkAutoRefreshRevisionGrid.Location = new System.Drawing.Point(11, 89);
            this.chkAutoRefreshRevisionGrid.Name = "chkAutoRefreshRevisionGrid";
            this.chkAutoRefreshRevisionGrid.Size = new System.Drawing.Size(313, 19);
            this.chkAutoRefreshRevisionGrid.TabIndex = 3;
            this.chkAutoRefreshRevisionGrid.Text = "Refresh repository view automatically on index change";
            this.chkAutoRefreshRevisionGrid.UseVisualStyleBackColor = true;
            // 
            // chkUseFastChecks
            // 
            this.chkUseFastChecks.AutoSize = true;
            this.chkUseFastChecks.Location = new System.Drawing.Point(11, 67);
            this.chkUseFastChecks.Name = "chkUseFastChecks";
            this.chkUseFastChecks.Size = new System.Drawing.Size(297, 19);
            this.chkUseFastChecks.TabIndex = 2;
            this.chkUseFastChecks.Text = "Use FileSystemWatcher to check if index is changed";
            this.chkUseFastChecks.UseVisualStyleBackColor = true;
            // 
            // chkShowStashCountInBrowseWindow
            // 
            this.chkShowStashCountInBrowseWindow.AutoSize = true;
            this.chkShowStashCountInBrowseWindow.Location = new System.Drawing.Point(11, 111);
            this.chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            this.chkShowStashCountInBrowseWindow.Size = new System.Drawing.Size(289, 19);
            this.chkShowStashCountInBrowseWindow.TabIndex = 4;
            this.chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            this.chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 164);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(296, 15);
            this.label12.TabIndex = 6;
            this.label12.Text = "Limit number of commits that will be loaded at startup";
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(321, 160);
            this._NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            this._NO_TRANSLATE_MaxCommits.Size = new System.Drawing.Size(123, 23);
            this._NO_TRANSLATE_MaxCommits.TabIndex = 7;
            this._NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // GitExtensionsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox12);
            this.Controls.Add(this.groupBox11);
            this.Name = "GitExtensionsSettingsPage";
            this.Size = new System.Drawing.Size(587, 502);
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.CheckBox chkWriteCommitMessageInCommitWindow;
        private System.Windows.Forms.CheckBox chkPlaySpecialStartupSound;
        private System.Windows.Forms.CheckBox chkCloseProcessDialog;
        private System.Windows.Forms.CheckBox chkShowGitCommandLine;
        private System.Windows.Forms.CheckBox chkStartWithRecentWorkingDir;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox SmtpServer;
        private System.Windows.Forms.NumericUpDown RevisionGridQuickSearchTimeout;
        private System.Windows.Forms.CheckBox chkStashUntrackedFiles;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.CheckBox chkUsePatienceDiffAlgorithm;
        private System.Windows.Forms.CheckBox chkShowErrorsWhenStagingFiles;
        private System.Windows.Forms.CheckBox chkFollowRenamesInFileHistory;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.CheckBox chkCheckForUncommittedChangesInCheckoutBranch;
        private System.Windows.Forms.CheckBox chkShowGitStatusInToolbar;
        private System.Windows.Forms.CheckBox chkShowCurrentChangesInRevisionGraph;
        private System.Windows.Forms.CheckBox chkUseFastChecks;
        private System.Windows.Forms.CheckBox chkShowStashCountInBrowseWindow;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_MaxCommits;
        private System.Windows.Forms.CheckBox chkAutoRefreshRevisionGrid;
    }
}
