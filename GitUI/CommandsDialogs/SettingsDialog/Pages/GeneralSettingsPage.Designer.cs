namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GeneralSettingsPage
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
            System.Windows.Forms.TableLayoutPanel tlpnlMain;
            this.groupBoxPerformance = new System.Windows.Forms.GroupBox();
            this.tlpnlPerformance = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowAheadBehindDataInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.chkCheckForUncommittedChangesInCheckoutBranch = new System.Windows.Forms.CheckBox();
            this.chkShowGitStatusInToolbar = new System.Windows.Forms.CheckBox();
            this.chkShowGitStatusForArtificialCommits = new System.Windows.Forms.CheckBox();
            this.chkShowStashCountInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.chkShowSubmoduleStatusInBrowse = new System.Windows.Forms.CheckBox();
            this.chkUseFastChecks = new System.Windows.Forms.CheckBox();
            this.lblCommitsLimit = new System.Windows.Forms.Label();
            this.chkSortByAuthorDate = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.groupBoxEmailSettings = new System.Windows.Forms.GroupBox();
            this.tlpnlEmailSettings = new System.Windows.Forms.TableLayoutPanel();
            this.SmtpServer = new System.Windows.Forms.TextBox();
            this.lblSmtpServerPort = new System.Windows.Forms.Label();
            this.chkUseSSL = new System.Windows.Forms.CheckBox();
            this.SmtpServerPort = new System.Windows.Forms.TextBox();
            this.lblSmtpServerName = new System.Windows.Forms.Label();
            this.groupBoxBehaviour = new System.Windows.Forms.GroupBox();
            this.tlpnlBehaviour = new System.Windows.Forms.TableLayoutPanel();
            this.chkFollowRenamesInFileHistoryExact = new System.Windows.Forms.CheckBox();
            this.RevisionGridQuickSearchTimeout = new System.Windows.Forms.NumericUpDown();
            this.btnDefaultDestinationBrowse = new System.Windows.Forms.Button();
            this.lblQuickSearchTimeout = new System.Windows.Forms.Label();
            this.chkCloseProcessDialog = new System.Windows.Forms.CheckBox();
            this.cbDefaultCloneDestination = new System.Windows.Forms.ComboBox();
            this.chkShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.lblDefaultCloneDestination = new System.Windows.Forms.Label();
            this.chkUsePatienceDiffAlgorithm = new System.Windows.Forms.CheckBox();
            this.chkStashUntrackedFiles = new System.Windows.Forms.CheckBox();
            this.chkStartWithRecentWorkingDir = new System.Windows.Forms.CheckBox();
            this.chkFollowRenamesInFileHistory = new System.Windows.Forms.CheckBox();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.groupBoxPerformance.SuspendLayout();
            this.tlpnlPerformance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.groupBoxEmailSettings.SuspendLayout();
            this.tlpnlEmailSettings.SuspendLayout();
            this.groupBoxBehaviour.SuspendLayout();
            this.tlpnlBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tlpnlMain.Controls.Add(this.groupBoxPerformance, 0, 0);
            tlpnlMain.Controls.Add(this.groupBoxEmailSettings, 0, 2);
            tlpnlMain.Controls.Add(this.groupBoxBehaviour, 0, 1);
            tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlMain.Location = new System.Drawing.Point(12, 12);
            tlpnlMain.Margin = new System.Windows.Forms.Padding(4);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlMain.Size = new System.Drawing.Size(1162, 352);
            tlpnlMain.TabIndex = 0;
            // 
            // groupBoxPerformance
            // 
            this.groupBoxPerformance.AutoSize = true;
            this.groupBoxPerformance.Controls.Add(this.tlpnlPerformance);
            this.groupBoxPerformance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxPerformance.Location = new System.Drawing.Point(4, 4);
            this.groupBoxPerformance.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxPerformance.Name = "groupBoxPerformance";
            this.groupBoxPerformance.Padding = new System.Windows.Forms.Padding(12);
            this.groupBoxPerformance.Size = new System.Drawing.Size(1154, 331);
            this.groupBoxPerformance.TabIndex = 0;
            this.groupBoxPerformance.TabStop = false;
            this.groupBoxPerformance.Text = "Performance";
            // 
            // tlpnlPerformance
            // 
            this.tlpnlPerformance.AutoSize = true;
            this.tlpnlPerformance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlPerformance.ColumnCount = 2;
            this.tlpnlPerformance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlPerformance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlPerformance.Controls.Add(this.chkShowAheadBehindDataInBrowseWindow, 0, 5);
            this.tlpnlPerformance.Controls.Add(this.chkCheckForUncommittedChangesInCheckoutBranch, 0, 6);
            this.tlpnlPerformance.Controls.Add(this.chkShowGitStatusInToolbar, 0, 0);
            this.tlpnlPerformance.Controls.Add(this.chkShowGitStatusForArtificialCommits, 0, 1);
            this.tlpnlPerformance.Controls.Add(this.chkShowStashCountInBrowseWindow, 0, 4);
            this.tlpnlPerformance.Controls.Add(this.chkShowSubmoduleStatusInBrowse, 0, 2);
            this.tlpnlPerformance.Controls.Add(this.chkUseFastChecks, 0, 3);
            this.tlpnlPerformance.Controls.Add(this.lblCommitsLimit, 0, 7);
            this.tlpnlPerformance.Controls.Add(this.chkSortByAuthorDate, 0, 8);
            this.tlpnlPerformance.Controls.Add(this._NO_TRANSLATE_MaxCommits, 1, 7);
            this.tlpnlPerformance.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpnlPerformance.Location = new System.Drawing.Point(12, 31);
            this.tlpnlPerformance.Margin = new System.Windows.Forms.Padding(4);
            this.tlpnlPerformance.Name = "tlpnlPerformance";
            this.tlpnlPerformance.RowCount = 8;
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlPerformance.Size = new System.Drawing.Size(1130, 288);
            this.tlpnlPerformance.TabIndex = 0;
            // 
            // chkShowAheadBehindDataInBrowseWindow
            // 
            this.chkShowAheadBehindDataInBrowseWindow.AutoSize = true;
            this.chkShowAheadBehindDataInBrowseWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowAheadBehindDataInBrowseWindow.Location = new System.Drawing.Point(4, 164);
            this.chkShowAheadBehindDataInBrowseWindow.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowAheadBehindDataInBrowseWindow.Name = "chkShowAheadBehindDataInBrowseWindow";
            this.chkShowAheadBehindDataInBrowseWindow.Size = new System.Drawing.Size(514, 24);
            this.chkShowAheadBehindDataInBrowseWindow.TabIndex = 10;
            this.chkShowAheadBehindDataInBrowseWindow.Text = "Show ahead and behind information on status bar in browse window";
            this.chkShowAheadBehindDataInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkCheckForUncommittedChangesInCheckoutBranch
            // 
            this.chkCheckForUncommittedChangesInCheckoutBranch.AutoSize = true;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Location = new System.Drawing.Point(4, 196);
            this.chkCheckForUncommittedChangesInCheckoutBranch.Margin = new System.Windows.Forms.Padding(4);
            this.chkCheckForUncommittedChangesInCheckoutBranch.Name = "chkCheckForUncommittedChangesInCheckoutBranch";
            this.chkCheckForUncommittedChangesInCheckoutBranch.Size = new System.Drawing.Size(514, 24);
            this.chkCheckForUncommittedChangesInCheckoutBranch.TabIndex = 6;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Text = "Check for uncommitted changes in checkout branch dialog";
            this.chkCheckForUncommittedChangesInCheckoutBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowGitStatusInToolbar
            // 
            this.chkShowGitStatusInToolbar.AutoSize = true;
            this.tlpnlPerformance.SetColumnSpan(this.chkShowGitStatusInToolbar, 2);
            this.chkShowGitStatusInToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowGitStatusInToolbar.Location = new System.Drawing.Point(4, 4);
            this.chkShowGitStatusInToolbar.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            this.chkShowGitStatusInToolbar.Size = new System.Drawing.Size(1122, 24);
            this.chkShowGitStatusInToolbar.TabIndex = 0;
            this.chkShowGitStatusInToolbar.Text = "Show number of changed files on commit button";
            this.chkShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            this.chkShowGitStatusInToolbar.CheckedChanged += new System.EventHandler(this.ShowGitStatus_CheckedChanged);
            // 
            // chkShowGitStatusForArtificialCommits
            // 
            this.chkShowGitStatusForArtificialCommits.AutoSize = true;
            this.tlpnlPerformance.SetColumnSpan(this.chkShowGitStatusForArtificialCommits, 2);
            this.chkShowGitStatusForArtificialCommits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowGitStatusForArtificialCommits.Location = new System.Drawing.Point(4, 36);
            this.chkShowGitStatusForArtificialCommits.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowGitStatusForArtificialCommits.Name = "chkShowGitStatusForArtificialCommits";
            this.chkShowGitStatusForArtificialCommits.Size = new System.Drawing.Size(1122, 24);
            this.chkShowGitStatusForArtificialCommits.TabIndex = 1;
            this.chkShowGitStatusForArtificialCommits.Text = "Show number of changed files for artificial commits";
            this.chkShowGitStatusForArtificialCommits.UseVisualStyleBackColor = true;
            this.chkShowGitStatusForArtificialCommits.CheckedChanged += new System.EventHandler(this.ShowGitStatus_CheckedChanged);
            // 
            // chkShowStashCountInBrowseWindow
            // 
            this.chkShowStashCountInBrowseWindow.AutoSize = true;
            this.chkShowStashCountInBrowseWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowStashCountInBrowseWindow.Location = new System.Drawing.Point(4, 132);
            this.chkShowStashCountInBrowseWindow.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            this.chkShowStashCountInBrowseWindow.Size = new System.Drawing.Size(514, 24);
            this.chkShowStashCountInBrowseWindow.TabIndex = 5;
            this.chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            this.chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkShowSubmoduleStatusInBrowse
            // 
            this.chkShowSubmoduleStatusInBrowse.AutoSize = true;
            this.chkShowSubmoduleStatusInBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowSubmoduleStatusInBrowse.Location = new System.Drawing.Point(4, 68);
            this.chkShowSubmoduleStatusInBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowSubmoduleStatusInBrowse.Name = "chkShowSubmoduleStatusInBrowse";
            this.chkShowSubmoduleStatusInBrowse.Size = new System.Drawing.Size(514, 24);
            this.chkShowSubmoduleStatusInBrowse.TabIndex = 2;
            this.chkShowSubmoduleStatusInBrowse.Text = "Show submodule status in browse menu";
            this.chkShowSubmoduleStatusInBrowse.UseVisualStyleBackColor = true;
            // 
            // chkUseFastChecks
            // 
            this.chkUseFastChecks.AutoSize = true;
            this.chkUseFastChecks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkUseFastChecks.Location = new System.Drawing.Point(4, 100);
            this.chkUseFastChecks.Margin = new System.Windows.Forms.Padding(4);
            this.chkUseFastChecks.Name = "chkUseFastChecks";
            this.chkUseFastChecks.Size = new System.Drawing.Size(514, 24);
            this.chkUseFastChecks.TabIndex = 4;
            this.chkUseFastChecks.Text = "Use FileSystemWatcher to check if index is changed";
            this.chkUseFastChecks.UseVisualStyleBackColor = true;
            // 
            // lblCommitsLimit
            // 
            this.lblCommitsLimit.AutoSize = true;
            this.lblCommitsLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommitsLimit.Location = new System.Drawing.Point(4, 224);
            this.lblCommitsLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCommitsLimit.Name = "lblCommitsLimit";
            this.lblCommitsLimit.Size = new System.Drawing.Size(514, 34);
            this.lblCommitsLimit.TabIndex = 7;
            this.lblCommitsLimit.Text = "Limit number of commits that will be loaded at startup";
            this.lblCommitsLimit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkSortByAuthorDate
            // 
            this.chkSortByAuthorDate.AutoSize = true;
            this.chkSortByAuthorDate.Location = new System.Drawing.Point(3, 261);
            this.chkSortByAuthorDate.Name = "chkSortByAuthorDate";
            this.chkSortByAuthorDate.Size = new System.Drawing.Size(171, 24);
            this.chkSortByAuthorDate.TabIndex = 11;
            this.chkSortByAuthorDate.Text = "Sort by author date";
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(526, 228);
            this._NO_TRANSLATE_MaxCommits.Margin = new System.Windows.Forms.Padding(4);
            this._NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            this._NO_TRANSLATE_MaxCommits.Size = new System.Drawing.Size(128, 26);
            this._NO_TRANSLATE_MaxCommits.TabIndex = 8;
            this._NO_TRANSLATE_MaxCommits.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._NO_TRANSLATE_MaxCommits.ThousandsSeparator = true;
            this._NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // groupBoxEmailSettings
            // 
            this.groupBoxEmailSettings.AutoSize = true;
            this.groupBoxEmailSettings.Controls.Add(this.tlpnlEmailSettings);
            this.groupBoxEmailSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxEmailSettings.Location = new System.Drawing.Point(4, 658);
            this.groupBoxEmailSettings.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxEmailSettings.Name = "groupBoxEmailSettings";
            this.groupBoxEmailSettings.Padding = new System.Windows.Forms.Padding(12);
            this.groupBoxEmailSettings.Size = new System.Drawing.Size(1154, 143);
            this.groupBoxEmailSettings.TabIndex = 2;
            this.groupBoxEmailSettings.TabStop = false;
            this.groupBoxEmailSettings.Text = "Email settings for sending patches";
            // 
            // tlpnlEmailSettings
            // 
            this.tlpnlEmailSettings.AutoSize = true;
            this.tlpnlEmailSettings.ColumnCount = 3;
            this.tlpnlEmailSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlEmailSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlEmailSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlEmailSettings.Controls.Add(this.SmtpServer, 2, 0);
            this.tlpnlEmailSettings.Controls.Add(this.lblSmtpServerPort, 0, 1);
            this.tlpnlEmailSettings.Controls.Add(this.chkUseSSL, 0, 2);
            this.tlpnlEmailSettings.Controls.Add(this.SmtpServerPort, 2, 1);
            this.tlpnlEmailSettings.Controls.Add(this.lblSmtpServerName, 0, 0);
            this.tlpnlEmailSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpnlEmailSettings.Location = new System.Drawing.Point(12, 31);
            this.tlpnlEmailSettings.Margin = new System.Windows.Forms.Padding(4);
            this.tlpnlEmailSettings.Name = "tlpnlEmailSettings";
            this.tlpnlEmailSettings.RowCount = 3;
            this.tlpnlEmailSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlEmailSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlEmailSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlEmailSettings.Size = new System.Drawing.Size(1130, 100);
            this.tlpnlEmailSettings.TabIndex = 0;
            // 
            // SmtpServer
            // 
            this.SmtpServer.Location = new System.Drawing.Point(170, 4);
            this.SmtpServer.Margin = new System.Windows.Forms.Padding(4);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(266, 26);
            this.SmtpServer.TabIndex = 0;
            // 
            // lblSmtpServerPort
            // 
            this.lblSmtpServerPort.AutoSize = true;
            this.lblSmtpServerPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSmtpServerPort.Location = new System.Drawing.Point(4, 34);
            this.lblSmtpServerPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSmtpServerPort.Name = "lblSmtpServerPort";
            this.lblSmtpServerPort.Size = new System.Drawing.Size(158, 34);
            this.lblSmtpServerPort.TabIndex = 1;
            this.lblSmtpServerPort.Text = "Port";
            this.lblSmtpServerPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkUseSSL
            // 
            this.chkUseSSL.AutoSize = true;
            this.chkUseSSL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkUseSSL.Location = new System.Drawing.Point(4, 72);
            this.chkUseSSL.Margin = new System.Windows.Forms.Padding(4);
            this.chkUseSSL.Name = "chkUseSSL";
            this.chkUseSSL.Size = new System.Drawing.Size(158, 24);
            this.chkUseSSL.TabIndex = 3;
            this.chkUseSSL.Text = "Use SSL/TLS";
            this.chkUseSSL.UseVisualStyleBackColor = true;
            this.chkUseSSL.CheckedChanged += new System.EventHandler(this.chkUseSSL_CheckedChanged);
            // 
            // SmtpServerPort
            // 
            this.SmtpServerPort.Location = new System.Drawing.Point(170, 38);
            this.SmtpServerPort.Margin = new System.Windows.Forms.Padding(4);
            this.SmtpServerPort.Name = "SmtpServerPort";
            this.SmtpServerPort.Size = new System.Drawing.Size(72, 26);
            this.SmtpServerPort.TabIndex = 2;
            this.SmtpServerPort.Text = "587";
            // 
            // lblSmtpServerName
            // 
            this.lblSmtpServerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSmtpServerName.AutoSize = true;
            this.lblSmtpServerName.Location = new System.Drawing.Point(4, 7);
            this.lblSmtpServerName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSmtpServerName.Name = "lblSmtpServerName";
            this.lblSmtpServerName.Padding = new System.Windows.Forms.Padding(0, 0, 15, 0);
            this.lblSmtpServerName.Size = new System.Drawing.Size(158, 20);
            this.lblSmtpServerName.TabIndex = 0;
            this.lblSmtpServerName.Text = "SMTP server name";
            this.lblSmtpServerName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxBehaviour
            // 
            this.groupBoxBehaviour.AutoSize = true;
            this.groupBoxBehaviour.Controls.Add(this.tlpnlBehaviour);
            this.groupBoxBehaviour.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxBehaviour.Location = new System.Drawing.Point(4, 343);
            this.groupBoxBehaviour.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxBehaviour.Name = "groupBoxBehaviour";
            this.groupBoxBehaviour.Padding = new System.Windows.Forms.Padding(12);
            this.groupBoxBehaviour.Size = new System.Drawing.Size(1154, 307);
            this.groupBoxBehaviour.TabIndex = 1;
            this.groupBoxBehaviour.TabStop = false;
            this.groupBoxBehaviour.Text = "Behaviour";
            // 
            // tlpnlBehaviour
            // 
            this.tlpnlBehaviour.AutoSize = true;
            this.tlpnlBehaviour.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlBehaviour.ColumnCount = 3;
            this.tlpnlBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlBehaviour.Controls.Add(this.chkFollowRenamesInFileHistoryExact, 1, 4);
            this.tlpnlBehaviour.Controls.Add(this.RevisionGridQuickSearchTimeout, 1, 7);
            this.tlpnlBehaviour.Controls.Add(this.btnDefaultDestinationBrowse, 2, 6);
            this.tlpnlBehaviour.Controls.Add(this.lblQuickSearchTimeout, 0, 7);
            this.tlpnlBehaviour.Controls.Add(this.chkCloseProcessDialog, 0, 0);
            this.tlpnlBehaviour.Controls.Add(this.cbDefaultCloneDestination, 1, 6);
            this.tlpnlBehaviour.Controls.Add(this.chkShowGitCommandLine, 0, 1);
            this.tlpnlBehaviour.Controls.Add(this.lblDefaultCloneDestination, 0, 6);
            this.tlpnlBehaviour.Controls.Add(this.chkUsePatienceDiffAlgorithm, 0, 2);
            this.tlpnlBehaviour.Controls.Add(this.chkStashUntrackedFiles, 0, 3);
            this.tlpnlBehaviour.Controls.Add(this.chkStartWithRecentWorkingDir, 0, 5);
            this.tlpnlBehaviour.Controls.Add(this.chkFollowRenamesInFileHistory, 0, 4);
            this.tlpnlBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpnlBehaviour.Location = new System.Drawing.Point(12, 31);
            this.tlpnlBehaviour.Margin = new System.Windows.Forms.Padding(4);
            this.tlpnlBehaviour.Name = "tlpnlBehaviour";
            this.tlpnlBehaviour.RowCount = 8;
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.Size = new System.Drawing.Size(1130, 264);
            this.tlpnlBehaviour.TabIndex = 0;
            // 
            // chkFollowRenamesInFileHistoryExact
            // 
            this.chkFollowRenamesInFileHistoryExact.AutoSize = true;
            this.chkFollowRenamesInFileHistoryExact.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFollowRenamesInFileHistoryExact.Location = new System.Drawing.Point(397, 132);
            this.chkFollowRenamesInFileHistoryExact.Margin = new System.Windows.Forms.Padding(4);
            this.chkFollowRenamesInFileHistoryExact.Name = "chkFollowRenamesInFileHistoryExact";
            this.chkFollowRenamesInFileHistoryExact.Size = new System.Drawing.Size(649, 24);
            this.chkFollowRenamesInFileHistoryExact.TabIndex = 5;
            this.chkFollowRenamesInFileHistoryExact.Text = "Follow exact renames and copies only";
            this.chkFollowRenamesInFileHistoryExact.UseVisualStyleBackColor = true;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            this.RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(397, 234);
            this.RevisionGridQuickSearchTimeout.Margin = new System.Windows.Forms.Padding(4);
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
            this.RevisionGridQuickSearchTimeout.Size = new System.Drawing.Size(128, 26);
            this.RevisionGridQuickSearchTimeout.TabIndex = 12;
            this.RevisionGridQuickSearchTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.RevisionGridQuickSearchTimeout.ThousandsSeparator = true;
            this.RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // btnDefaultDestinationBrowse
            // 
            this.btnDefaultDestinationBrowse.AutoSize = true;
            this.btnDefaultDestinationBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDefaultDestinationBrowse.Location = new System.Drawing.Point(1054, 196);
            this.btnDefaultDestinationBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.btnDefaultDestinationBrowse.Name = "btnDefaultDestinationBrowse";
            this.btnDefaultDestinationBrowse.Size = new System.Drawing.Size(72, 30);
            this.btnDefaultDestinationBrowse.TabIndex = 10;
            this.btnDefaultDestinationBrowse.Text = "Browse";
            this.btnDefaultDestinationBrowse.UseVisualStyleBackColor = true;
            this.btnDefaultDestinationBrowse.Click += new System.EventHandler(this.DefaultCloneDestinationBrowseClick);
            // 
            // lblQuickSearchTimeout
            // 
            this.lblQuickSearchTimeout.AutoSize = true;
            this.lblQuickSearchTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblQuickSearchTimeout.Location = new System.Drawing.Point(4, 230);
            this.lblQuickSearchTimeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblQuickSearchTimeout.Name = "lblQuickSearchTimeout";
            this.lblQuickSearchTimeout.Size = new System.Drawing.Size(385, 34);
            this.lblQuickSearchTimeout.TabIndex = 11;
            this.lblQuickSearchTimeout.Text = "Revision grid quick search timeout [ms]";
            this.lblQuickSearchTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkCloseProcessDialog
            // 
            this.chkCloseProcessDialog.AutoSize = true;
            this.chkCloseProcessDialog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCloseProcessDialog.Location = new System.Drawing.Point(4, 4);
            this.chkCloseProcessDialog.Margin = new System.Windows.Forms.Padding(4);
            this.chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            this.chkCloseProcessDialog.Size = new System.Drawing.Size(385, 24);
            this.chkCloseProcessDialog.TabIndex = 0;
            this.chkCloseProcessDialog.Text = "Close Process dialog when process succeeds";
            this.chkCloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // cbDefaultCloneDestination
            // 
            this.cbDefaultCloneDestination.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbDefaultCloneDestination.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.cbDefaultCloneDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDefaultCloneDestination.FormattingEnabled = true;
            this.cbDefaultCloneDestination.Location = new System.Drawing.Point(397, 196);
            this.cbDefaultCloneDestination.Margin = new System.Windows.Forms.Padding(4);
            this.cbDefaultCloneDestination.Name = "cbDefaultCloneDestination";
            this.cbDefaultCloneDestination.Size = new System.Drawing.Size(649, 28);
            this.cbDefaultCloneDestination.TabIndex = 9;
            // 
            // chkShowGitCommandLine
            // 
            this.chkShowGitCommandLine.AutoSize = true;
            this.chkShowGitCommandLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowGitCommandLine.Location = new System.Drawing.Point(4, 36);
            this.chkShowGitCommandLine.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            this.chkShowGitCommandLine.Size = new System.Drawing.Size(385, 24);
            this.chkShowGitCommandLine.TabIndex = 1;
            this.chkShowGitCommandLine.Text = "Show console window when executing git process";
            this.chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // lblDefaultCloneDestination
            // 
            this.lblDefaultCloneDestination.AutoSize = true;
            this.lblDefaultCloneDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDefaultCloneDestination.Location = new System.Drawing.Point(4, 192);
            this.lblDefaultCloneDestination.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefaultCloneDestination.Name = "lblDefaultCloneDestination";
            this.lblDefaultCloneDestination.Size = new System.Drawing.Size(385, 38);
            this.lblDefaultCloneDestination.TabIndex = 8;
            this.lblDefaultCloneDestination.Text = "Default clone destination";
            this.lblDefaultCloneDestination.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkUsePatienceDiffAlgorithm
            // 
            this.chkUsePatienceDiffAlgorithm.AutoSize = true;
            this.chkUsePatienceDiffAlgorithm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkUsePatienceDiffAlgorithm.Location = new System.Drawing.Point(4, 68);
            this.chkUsePatienceDiffAlgorithm.Margin = new System.Windows.Forms.Padding(4);
            this.chkUsePatienceDiffAlgorithm.Name = "chkUsePatienceDiffAlgorithm";
            this.chkUsePatienceDiffAlgorithm.Size = new System.Drawing.Size(385, 24);
            this.chkUsePatienceDiffAlgorithm.TabIndex = 2;
            this.chkUsePatienceDiffAlgorithm.Text = "Use patience diff algorithm";
            this.chkUsePatienceDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkStashUntrackedFiles
            // 
            this.chkStashUntrackedFiles.AutoSize = true;
            this.chkStashUntrackedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkStashUntrackedFiles.Location = new System.Drawing.Point(4, 100);
            this.chkStashUntrackedFiles.Margin = new System.Windows.Forms.Padding(4);
            this.chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            this.chkStashUntrackedFiles.Size = new System.Drawing.Size(385, 24);
            this.chkStashUntrackedFiles.TabIndex = 3;
            this.chkStashUntrackedFiles.Text = "Include untracked files in stash";
            this.chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            this.chkStartWithRecentWorkingDir.AutoSize = true;
            this.chkStartWithRecentWorkingDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkStartWithRecentWorkingDir.Location = new System.Drawing.Point(4, 164);
            this.chkStartWithRecentWorkingDir.Margin = new System.Windows.Forms.Padding(4);
            this.chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            this.chkStartWithRecentWorkingDir.Size = new System.Drawing.Size(385, 24);
            this.chkStartWithRecentWorkingDir.TabIndex = 6;
            this.chkStartWithRecentWorkingDir.Text = "Open last working directory on startup";
            this.chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            this.chkFollowRenamesInFileHistory.AutoSize = true;
            this.chkFollowRenamesInFileHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFollowRenamesInFileHistory.Location = new System.Drawing.Point(4, 132);
            this.chkFollowRenamesInFileHistory.Margin = new System.Windows.Forms.Padding(4);
            this.chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            this.chkFollowRenamesInFileHistory.Size = new System.Drawing.Size(385, 24);
            this.chkFollowRenamesInFileHistory.TabIndex = 4;
            this.chkFollowRenamesInFileHistory.Text = "Follow renames in file history (experimental)";
            this.chkFollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // GeneralSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(tlpnlMain);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GeneralSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Size = new System.Drawing.Size(1186, 376);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.groupBoxPerformance.ResumeLayout(false);
            this.groupBoxPerformance.PerformLayout();
            this.tlpnlPerformance.ResumeLayout(false);
            this.tlpnlPerformance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.groupBoxEmailSettings.ResumeLayout(false);
            this.groupBoxEmailSettings.PerformLayout();
            this.tlpnlEmailSettings.ResumeLayout(false);
            this.tlpnlEmailSettings.PerformLayout();
            this.groupBoxBehaviour.ResumeLayout(false);
            this.groupBoxBehaviour.PerformLayout();
            this.tlpnlBehaviour.ResumeLayout(false);
            this.tlpnlBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBehaviour;
        private System.Windows.Forms.CheckBox chkCloseProcessDialog;
        private System.Windows.Forms.CheckBox chkShowGitCommandLine;
        private System.Windows.Forms.CheckBox chkStartWithRecentWorkingDir;
        private System.Windows.Forms.NumericUpDown RevisionGridQuickSearchTimeout;
        private System.Windows.Forms.CheckBox chkStashUntrackedFiles;
        private System.Windows.Forms.Label lblQuickSearchTimeout;
        private System.Windows.Forms.CheckBox chkUsePatienceDiffAlgorithm;
        private System.Windows.Forms.CheckBox chkFollowRenamesInFileHistory;
        private System.Windows.Forms.GroupBox groupBoxPerformance;
        private System.Windows.Forms.CheckBox chkCheckForUncommittedChangesInCheckoutBranch;
        private System.Windows.Forms.CheckBox chkShowGitStatusInToolbar;
        private System.Windows.Forms.CheckBox chkShowGitStatusForArtificialCommits;
        private System.Windows.Forms.CheckBox chkSortByAuthorDate;
        private System.Windows.Forms.CheckBox chkUseFastChecks;
        private System.Windows.Forms.CheckBox chkShowStashCountInBrowseWindow;
        private System.Windows.Forms.CheckBox chkShowSubmoduleStatusInBrowse;
        private System.Windows.Forms.Label lblCommitsLimit;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_MaxCommits;
        private System.Windows.Forms.GroupBox groupBoxEmailSettings;
        private System.Windows.Forms.CheckBox chkUseSSL;
        private System.Windows.Forms.TextBox SmtpServerPort;
        private System.Windows.Forms.Label lblSmtpServerPort;
        private System.Windows.Forms.TextBox SmtpServer;
        private System.Windows.Forms.Label lblSmtpServerName;
        private System.Windows.Forms.Label lblDefaultCloneDestination;
        private System.Windows.Forms.ComboBox cbDefaultCloneDestination;
        private System.Windows.Forms.Button btnDefaultDestinationBrowse;
        private System.Windows.Forms.TableLayoutPanel tlpnlPerformance;
        private System.Windows.Forms.TableLayoutPanel tlpnlBehaviour;
        private System.Windows.Forms.TableLayoutPanel tlpnlEmailSettings;
        private System.Windows.Forms.CheckBox chkFollowRenamesInFileHistoryExact;
        private System.Windows.Forms.CheckBox chkShowAheadBehindDataInBrowseWindow;
    }
}
