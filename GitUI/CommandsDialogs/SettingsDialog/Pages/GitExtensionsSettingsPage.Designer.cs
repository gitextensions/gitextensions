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
            this.btnDefaultDestinationBrowse = new System.Windows.Forms.Button();
            this.cbDefaultCloneDestination = new System.Windows.Forms.ComboBox();
            this.lblDefaultCloneDestination = new System.Windows.Forms.Label();
            this.chkWriteCommitMessageInCommitWindow = new System.Windows.Forms.CheckBox();
            this.chkPlaySpecialStartupSound = new System.Windows.Forms.CheckBox();
            this.chkCloseProcessDialog = new System.Windows.Forms.CheckBox();
            this.chkShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.chkStartWithRecentWorkingDir = new System.Windows.Forms.CheckBox();
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
            this.chkUseFastChecks = new System.Windows.Forms.CheckBox();
            this.chkShowStashCountInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkUseSSL = new System.Windows.Forms.CheckBox();
            this.SmtpServerPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SmtpServer = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.groupBox12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox12
            // 
            this.groupBox12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox12.Controls.Add(this.btnDefaultDestinationBrowse);
            this.groupBox12.Controls.Add(this.cbDefaultCloneDestination);
            this.groupBox12.Controls.Add(this.lblDefaultCloneDestination);
            this.groupBox12.Controls.Add(this.chkWriteCommitMessageInCommitWindow);
            this.groupBox12.Controls.Add(this.chkPlaySpecialStartupSound);
            this.groupBox12.Controls.Add(this.chkCloseProcessDialog);
            this.groupBox12.Controls.Add(this.chkShowGitCommandLine);
            this.groupBox12.Controls.Add(this.chkStartWithRecentWorkingDir);
            this.groupBox12.Controls.Add(this.RevisionGridQuickSearchTimeout);
            this.groupBox12.Controls.Add(this.chkStashUntrackedFiles);
            this.groupBox12.Controls.Add(this.label24);
            this.groupBox12.Controls.Add(this.chkUsePatienceDiffAlgorithm);
            this.groupBox12.Controls.Add(this.chkShowErrorsWhenStagingFiles);
            this.groupBox12.Controls.Add(this.chkFollowRenamesInFileHistory);
            this.groupBox12.Location = new System.Drawing.Point(3, 186);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(581, 296);
            this.groupBox12.TabIndex = 56;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Behaviour";
            // 
            // btnDefaultDestinationBrowse
            // 
            this.btnDefaultDestinationBrowse.Location = new System.Drawing.Point(449, 230);
            this.btnDefaultDestinationBrowse.Name = "btnDefaultDestinationBrowse";
            this.btnDefaultDestinationBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDefaultDestinationBrowse.TabIndex = 57;
            this.btnDefaultDestinationBrowse.Text = "Browse";
            this.btnDefaultDestinationBrowse.UseVisualStyleBackColor = true;
            this.btnDefaultDestinationBrowse.Click += new System.EventHandler(this.DefaultCloneDestinationBrowseClick);
            // 
            // cbDefaultCloneDestination
            // 
            this.cbDefaultCloneDestination.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbDefaultCloneDestination.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.cbDefaultCloneDestination.FormattingEnabled = true;
            this.cbDefaultCloneDestination.Location = new System.Drawing.Point(155, 230);
            this.cbDefaultCloneDestination.Name = "cbDefaultCloneDestination";
            this.cbDefaultCloneDestination.Size = new System.Drawing.Size(288, 23);
            this.cbDefaultCloneDestination.TabIndex = 56;
            // 
            // lblDefaultCloneDestination
            // 
            this.lblDefaultCloneDestination.AutoSize = true;
            this.lblDefaultCloneDestination.Location = new System.Drawing.Point(7, 233);
            this.lblDefaultCloneDestination.Name = "lblDefaultCloneDestination";
            this.lblDefaultCloneDestination.Size = new System.Drawing.Size(139, 15);
            this.lblDefaultCloneDestination.TabIndex = 55;
            this.lblDefaultCloneDestination.Text = "Default clone destination";
            // 
            // chkWriteCommitMessageInCommitWindow
            // 
            this.chkWriteCommitMessageInCommitWindow.AutoSize = true;
            this.chkWriteCommitMessageInCommitWindow.Location = new System.Drawing.Point(10, 187);
            this.chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
            this.chkWriteCommitMessageInCommitWindow.Size = new System.Drawing.Size(329, 34);
            this.chkWriteCommitMessageInCommitWindow.TabIndex = 54;
            this.chkWriteCommitMessageInCommitWindow.Text = "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
    "d during commit)";
            this.chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
            // 
            // chkPlaySpecialStartupSound
            // 
            this.chkPlaySpecialStartupSound.AutoSize = true;
            this.chkPlaySpecialStartupSound.Location = new System.Drawing.Point(320, 158);
            this.chkPlaySpecialStartupSound.Name = "chkPlaySpecialStartupSound";
            this.chkPlaySpecialStartupSound.Size = new System.Drawing.Size(166, 19);
            this.chkPlaySpecialStartupSound.TabIndex = 53;
            this.chkPlaySpecialStartupSound.Text = "Play Special Startup Sound";
            this.chkPlaySpecialStartupSound.UseVisualStyleBackColor = true;
            // 
            // chkCloseProcessDialog
            // 
            this.chkCloseProcessDialog.AutoSize = true;
            this.chkCloseProcessDialog.Location = new System.Drawing.Point(10, 20);
            this.chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            this.chkCloseProcessDialog.Size = new System.Drawing.Size(260, 19);
            this.chkCloseProcessDialog.TabIndex = 9;
            this.chkCloseProcessDialog.Text = "Close Process dialog when process succeeds";
            this.chkCloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // chkShowGitCommandLine
            // 
            this.chkShowGitCommandLine.AutoSize = true;
            this.chkShowGitCommandLine.Location = new System.Drawing.Point(10, 43);
            this.chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            this.chkShowGitCommandLine.Size = new System.Drawing.Size(290, 19);
            this.chkShowGitCommandLine.TabIndex = 11;
            this.chkShowGitCommandLine.Text = "Show console window when executing git process";
            this.chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            this.chkStartWithRecentWorkingDir.AutoSize = true;
            this.chkStartWithRecentWorkingDir.Location = new System.Drawing.Point(10, 158);
            this.chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            this.chkStartWithRecentWorkingDir.Size = new System.Drawing.Size(196, 19);
            this.chkStartWithRecentWorkingDir.TabIndex = 52;
            this.chkStartWithRecentWorkingDir.Text = "Open last working dir on startup";
            this.chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            this.RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(320, 263);
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
            this.RevisionGridQuickSearchTimeout.TabIndex = 33;
            this.RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // chkStashUntrackedFiles
            // 
            this.chkStashUntrackedFiles.AutoSize = true;
            this.chkStashUntrackedFiles.Location = new System.Drawing.Point(10, 110);
            this.chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            this.chkStashUntrackedFiles.Size = new System.Drawing.Size(188, 19);
            this.chkStashUntrackedFiles.TabIndex = 51;
            this.chkStashUntrackedFiles.Text = "Include untracked files in stash";
            this.chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(7, 265);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(216, 15);
            this.label24.TabIndex = 32;
            this.label24.Text = "Revision grid quick search timeout [ms]";
            // 
            // chkUsePatienceDiffAlgorithm
            // 
            this.chkUsePatienceDiffAlgorithm.AutoSize = true;
            this.chkUsePatienceDiffAlgorithm.Location = new System.Drawing.Point(10, 66);
            this.chkUsePatienceDiffAlgorithm.Name = "chkUsePatienceDiffAlgorithm";
            this.chkUsePatienceDiffAlgorithm.Size = new System.Drawing.Size(169, 19);
            this.chkUsePatienceDiffAlgorithm.TabIndex = 43;
            this.chkUsePatienceDiffAlgorithm.Text = "Use patience diff algorithm";
            this.chkUsePatienceDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            this.chkShowErrorsWhenStagingFiles.AutoSize = true;
            this.chkShowErrorsWhenStagingFiles.Location = new System.Drawing.Point(10, 87);
            this.chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            this.chkShowErrorsWhenStagingFiles.Size = new System.Drawing.Size(186, 19);
            this.chkShowErrorsWhenStagingFiles.TabIndex = 34;
            this.chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            this.chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            this.chkFollowRenamesInFileHistory.AutoSize = true;
            this.chkFollowRenamesInFileHistory.Location = new System.Drawing.Point(10, 133);
            this.chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            this.chkFollowRenamesInFileHistory.Size = new System.Drawing.Size(259, 19);
            this.chkFollowRenamesInFileHistory.TabIndex = 26;
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
            this.groupBox11.Controls.Add(this.chkUseFastChecks);
            this.groupBox11.Controls.Add(this.chkShowStashCountInBrowseWindow);
            this.groupBox11.Controls.Add(this.label12);
            this.groupBox11.Controls.Add(this._NO_TRANSLATE_MaxCommits);
            this.groupBox11.Location = new System.Drawing.Point(3, 3);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(581, 177);
            this.groupBox11.TabIndex = 55;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Performance";
            // 
            // chkCheckForUncommittedChangesInCheckoutBranch
            // 
            this.chkCheckForUncommittedChangesInCheckoutBranch.AutoSize = true;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Location = new System.Drawing.Point(10, 111);
            this.chkCheckForUncommittedChangesInCheckoutBranch.Name = "chkCheckForUncommittedChangesInCheckoutBranch";
            this.chkCheckForUncommittedChangesInCheckoutBranch.Size = new System.Drawing.Size(341, 19);
            this.chkCheckForUncommittedChangesInCheckoutBranch.TabIndex = 39;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Text = "Check for uncommitted changes in checkout branch dialog";
            this.chkCheckForUncommittedChangesInCheckoutBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowGitStatusInToolbar
            // 
            this.chkShowGitStatusInToolbar.AutoSize = true;
            this.chkShowGitStatusInToolbar.Location = new System.Drawing.Point(10, 23);
            this.chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            this.chkShowGitStatusInToolbar.Size = new System.Drawing.Size(489, 19);
            this.chkShowGitStatusInToolbar.TabIndex = 31;
            this.chkShowGitStatusInToolbar.Text = "Show repository status in browse dialog (number of changes in toolbar, restart re" +
    "quired)";
            this.chkShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentChangesInRevisionGraph
            // 
            this.chkShowCurrentChangesInRevisionGraph.AutoSize = true;
            this.chkShowCurrentChangesInRevisionGraph.Location = new System.Drawing.Point(10, 43);
            this.chkShowCurrentChangesInRevisionGraph.Name = "chkShowCurrentChangesInRevisionGraph";
            this.chkShowCurrentChangesInRevisionGraph.Size = new System.Drawing.Size(335, 19);
            this.chkShowCurrentChangesInRevisionGraph.TabIndex = 36;
            this.chkShowCurrentChangesInRevisionGraph.Text = "Show current working dir changes in revision graph (slow!)";
            this.chkShowCurrentChangesInRevisionGraph.UseVisualStyleBackColor = true;
            // 
            // chkUseFastChecks
            // 
            this.chkUseFastChecks.AutoSize = true;
            this.chkUseFastChecks.Location = new System.Drawing.Point(10, 65);
            this.chkUseFastChecks.Name = "chkUseFastChecks";
            this.chkUseFastChecks.Size = new System.Drawing.Size(297, 19);
            this.chkUseFastChecks.TabIndex = 12;
            this.chkUseFastChecks.Text = "Use FileSystemWatcher to check if index is changed";
            this.chkUseFastChecks.UseVisualStyleBackColor = true;
            // 
            // chkShowStashCountInBrowseWindow
            // 
            this.chkShowStashCountInBrowseWindow.AutoSize = true;
            this.chkShowStashCountInBrowseWindow.Location = new System.Drawing.Point(10, 87);
            this.chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            this.chkShowStashCountInBrowseWindow.Size = new System.Drawing.Size(289, 19);
            this.chkShowStashCountInBrowseWindow.TabIndex = 38;
            this.chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            this.chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 143);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(296, 15);
            this.label12.TabIndex = 0;
            this.label12.Text = "Limit number of commits that will be loaded at startup";
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(320, 141);
            this._NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            this._NO_TRANSLATE_MaxCommits.Size = new System.Drawing.Size(123, 23);
            this._NO_TRANSLATE_MaxCommits.TabIndex = 2;
            this._NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkUseSSL);
            this.groupBox1.Controls.Add(this.SmtpServerPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.SmtpServer);
            this.groupBox1.Controls.Add(this.label23);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Location = new System.Drawing.Point(3, 488);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(581, 68);
            this.groupBox1.TabIndex = 57;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Email settings for sending patches";
            // 
            // chkUseSSL
            // 
            this.chkUseSSL.AutoSize = true;
            this.chkUseSSL.Location = new System.Drawing.Point(422, 32);
            this.chkUseSSL.Name = "chkUseSSL";
            this.chkUseSSL.Size = new System.Drawing.Size(90, 19);
            this.chkUseSSL.TabIndex = 27;
            this.chkUseSSL.Text = "Use SSL/TLS";
            this.chkUseSSL.UseVisualStyleBackColor = true;
            this.chkUseSSL.CheckedChanged += new System.EventHandler(this.chkUseSSL_CheckedChanged);
            // 
            // SmtpServerPort
            // 
            this.SmtpServerPort.Location = new System.Drawing.Point(351, 30);
            this.SmtpServerPort.Name = "SmtpServerPort";
            this.SmtpServerPort.Size = new System.Drawing.Size(49, 23);
            this.SmtpServerPort.TabIndex = 21;
            this.SmtpServerPort.Text = "587";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(316, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 15);
            this.label2.TabIndex = 22;
            this.label2.Text = "Port";
            // 
            // SmtpServer
            // 
            this.SmtpServer.Location = new System.Drawing.Point(130, 30);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(177, 23);
            this.SmtpServer.TabIndex = 19;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(19, 33);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(105, 15);
            this.label23.TabIndex = 20;
            this.label23.Text = "SMTP server name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(296, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Limit number of commits that will be loaded at startup";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(320, 141);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(123, 23);
            this.numericUpDown1.TabIndex = 2;
            this.numericUpDown1.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // GitExtensionsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox12);
            this.Controls.Add(this.groupBox11);
            this.Name = "GitExtensionsSettingsPage";
            this.Size = new System.Drawing.Size(587, 559);
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.CheckBox chkWriteCommitMessageInCommitWindow;
        private System.Windows.Forms.CheckBox chkPlaySpecialStartupSound;
        private System.Windows.Forms.CheckBox chkCloseProcessDialog;
        private System.Windows.Forms.CheckBox chkShowGitCommandLine;
        private System.Windows.Forms.CheckBox chkStartWithRecentWorkingDir;
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkUseSSL;
        private System.Windows.Forms.TextBox SmtpServerPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SmtpServer;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label lblDefaultCloneDestination;
        private System.Windows.Forms.ComboBox cbDefaultCloneDestination;
        private System.Windows.Forms.Button btnDefaultDestinationBrowse;
    }
}
