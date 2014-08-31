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
            this.groupBoxBehaviour = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelBehaviour = new System.Windows.Forms.TableLayoutPanel();
            this.RevisionGridQuickSearchTimeout = new System.Windows.Forms.NumericUpDown();
            this.btnDefaultDestinationBrowse = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.chkCloseProcessDialog = new System.Windows.Forms.CheckBox();
            this.cbDefaultCloneDestination = new System.Windows.Forms.ComboBox();
            this.chkShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.lblDefaultCloneDestination = new System.Windows.Forms.Label();
            this.chkUsePatienceDiffAlgorithm = new System.Windows.Forms.CheckBox();
            this.chkPlaySpecialStartupSound = new System.Windows.Forms.CheckBox();
            this.chkStashUntrackedFiles = new System.Windows.Forms.CheckBox();
            this.chkStartWithRecentWorkingDir = new System.Windows.Forms.CheckBox();
            this.chkFollowRenamesInFileHistory = new System.Windows.Forms.CheckBox();
            this.groupBoxPerformance = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelPerformance = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.chkCheckForUncommittedChangesInCheckoutBranch = new System.Windows.Forms.CheckBox();
            this.chkShowGitStatusInToolbar = new System.Windows.Forms.CheckBox();
            this.chkShowStashCountInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.chkUseFastChecks = new System.Windows.Forms.CheckBox();
            this.chkShowCurrentChangesInRevisionGraph = new System.Windows.Forms.CheckBox();
            this.groupBoxEmailSettings = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelEmailSettings = new System.Windows.Forms.TableLayoutPanel();
            this.labelSmtpServerName = new System.Windows.Forms.Label();
            this.tableLayoutPanelSmtpServer = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.SmtpServer = new System.Windows.Forms.TextBox();
            this.SmtpServerPort = new System.Windows.Forms.TextBox();
            this.chkUseSSL = new System.Windows.Forms.CheckBox();
            this.panelSpacer1 = new System.Windows.Forms.Panel();
            this.panelSpacer2 = new System.Windows.Forms.Panel();
            this.groupBoxBehaviour.SuspendLayout();
            this.tableLayoutPanelBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            this.groupBoxPerformance.SuspendLayout();
            this.tableLayoutPanelPerformance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.groupBoxEmailSettings.SuspendLayout();
            this.tableLayoutPanelEmailSettings.SuspendLayout();
            this.tableLayoutPanelSmtpServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxBehaviour
            // 
            this.groupBoxBehaviour.AutoSize = true;
            this.groupBoxBehaviour.Controls.Add(this.tableLayoutPanelBehaviour);
            this.groupBoxBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxBehaviour.Location = new System.Drawing.Point(0, 186);
            this.groupBoxBehaviour.Name = "groupBoxBehaviour";
            this.groupBoxBehaviour.Size = new System.Drawing.Size(976, 236);
            this.groupBoxBehaviour.TabIndex = 56;
            this.groupBoxBehaviour.TabStop = false;
            this.groupBoxBehaviour.Text = "Behaviour";
            // 
            // tableLayoutPanelBehaviour
            // 
            this.tableLayoutPanelBehaviour.AutoSize = true;
            this.tableLayoutPanelBehaviour.ColumnCount = 3;
            this.tableLayoutPanelBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBehaviour.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBehaviour.Controls.Add(this.RevisionGridQuickSearchTimeout, 1, 7);
            this.tableLayoutPanelBehaviour.Controls.Add(this.btnDefaultDestinationBrowse, 2, 6);
            this.tableLayoutPanelBehaviour.Controls.Add(this.label24, 0, 7);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkCloseProcessDialog, 0, 0);
            this.tableLayoutPanelBehaviour.Controls.Add(this.cbDefaultCloneDestination, 1, 6);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkShowGitCommandLine, 0, 1);
            this.tableLayoutPanelBehaviour.Controls.Add(this.lblDefaultCloneDestination, 0, 6);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkUsePatienceDiffAlgorithm, 0, 2);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkPlaySpecialStartupSound, 1, 5);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkStashUntrackedFiles, 0, 3);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkStartWithRecentWorkingDir, 0, 5);
            this.tableLayoutPanelBehaviour.Controls.Add(this.chkFollowRenamesInFileHistory, 0, 4);
            this.tableLayoutPanelBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelBehaviour.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanelBehaviour.Name = "tableLayoutPanelBehaviour";
            this.tableLayoutPanelBehaviour.RowCount = 8;
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBehaviour.Size = new System.Drawing.Size(970, 214);
            this.tableLayoutPanelBehaviour.TabIndex = 61;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            this.RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(322, 188);
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
            this.RevisionGridQuickSearchTimeout.TabIndex = 21;
            this.RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // btnDefaultDestinationBrowse
            // 
            this.btnDefaultDestinationBrowse.Location = new System.Drawing.Point(871, 159);
            this.btnDefaultDestinationBrowse.Name = "btnDefaultDestinationBrowse";
            this.btnDefaultDestinationBrowse.Size = new System.Drawing.Size(96, 23);
            this.btnDefaultDestinationBrowse.TabIndex = 19;
            this.btnDefaultDestinationBrowse.Text = "Browse";
            this.btnDefaultDestinationBrowse.UseVisualStyleBackColor = true;
            this.btnDefaultDestinationBrowse.Click += new System.EventHandler(this.DefaultCloneDestinationBrowseClick);
            // 
            // label24
            // 
            this.label24.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(3, 191);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(234, 16);
            this.label24.TabIndex = 20;
            this.label24.Text = "Revision grid quick search timeout [ms]";
            // 
            // chkCloseProcessDialog
            // 
            this.chkCloseProcessDialog.AutoSize = true;
            this.chkCloseProcessDialog.Location = new System.Drawing.Point(3, 3);
            this.chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            this.chkCloseProcessDialog.Size = new System.Drawing.Size(283, 20);
            this.chkCloseProcessDialog.TabIndex = 10;
            this.chkCloseProcessDialog.Text = "Close Process dialog when process succeeds";
            this.chkCloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // cbDefaultCloneDestination
            // 
            this.cbDefaultCloneDestination.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbDefaultCloneDestination.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.cbDefaultCloneDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDefaultCloneDestination.FormattingEnabled = true;
            this.cbDefaultCloneDestination.Location = new System.Drawing.Point(322, 159);
            this.cbDefaultCloneDestination.Name = "cbDefaultCloneDestination";
            this.cbDefaultCloneDestination.Size = new System.Drawing.Size(543, 24);
            this.cbDefaultCloneDestination.TabIndex = 18;
            this.cbDefaultCloneDestination.DropDown += new System.EventHandler(this.defaultCloneDropDown);
            // 
            // chkShowGitCommandLine
            // 
            this.chkShowGitCommandLine.AutoSize = true;
            this.chkShowGitCommandLine.Location = new System.Drawing.Point(3, 29);
            this.chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            this.chkShowGitCommandLine.Size = new System.Drawing.Size(313, 20);
            this.chkShowGitCommandLine.TabIndex = 11;
            this.chkShowGitCommandLine.Text = "Show console window when executing git process";
            this.chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // lblDefaultCloneDestination
            // 
            this.lblDefaultCloneDestination.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDefaultCloneDestination.AutoSize = true;
            this.lblDefaultCloneDestination.Location = new System.Drawing.Point(3, 162);
            this.lblDefaultCloneDestination.Name = "lblDefaultCloneDestination";
            this.lblDefaultCloneDestination.Size = new System.Drawing.Size(148, 16);
            this.lblDefaultCloneDestination.TabIndex = 17;
            this.lblDefaultCloneDestination.Text = "Default clone destination";
            // 
            // chkUsePatienceDiffAlgorithm
            // 
            this.chkUsePatienceDiffAlgorithm.AutoSize = true;
            this.chkUsePatienceDiffAlgorithm.Location = new System.Drawing.Point(3, 55);
            this.chkUsePatienceDiffAlgorithm.Name = "chkUsePatienceDiffAlgorithm";
            this.chkUsePatienceDiffAlgorithm.Size = new System.Drawing.Size(180, 20);
            this.chkUsePatienceDiffAlgorithm.TabIndex = 12;
            this.chkUsePatienceDiffAlgorithm.Text = "Use patience diff algorithm";
            this.chkUsePatienceDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkPlaySpecialStartupSound
            // 
            this.chkPlaySpecialStartupSound.AutoSize = true;
            this.chkPlaySpecialStartupSound.Location = new System.Drawing.Point(322, 133);
            this.chkPlaySpecialStartupSound.Name = "chkPlaySpecialStartupSound";
            this.chkPlaySpecialStartupSound.Size = new System.Drawing.Size(181, 20);
            this.chkPlaySpecialStartupSound.TabIndex = 16;
            this.chkPlaySpecialStartupSound.Text = "Play Special Startup Sound";
            this.chkPlaySpecialStartupSound.UseVisualStyleBackColor = true;
            // 
            // chkStashUntrackedFiles
            // 
            this.chkStashUntrackedFiles.AutoSize = true;
            this.chkStashUntrackedFiles.Location = new System.Drawing.Point(3, 81);
            this.chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            this.chkStashUntrackedFiles.Size = new System.Drawing.Size(203, 20);
            this.chkStashUntrackedFiles.TabIndex = 13;
            this.chkStashUntrackedFiles.Text = "Include untracked files in stash";
            this.chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            this.chkStartWithRecentWorkingDir.AutoSize = true;
            this.chkStartWithRecentWorkingDir.Location = new System.Drawing.Point(3, 133);
            this.chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            this.chkStartWithRecentWorkingDir.Size = new System.Drawing.Size(211, 20);
            this.chkStartWithRecentWorkingDir.TabIndex = 15;
            this.chkStartWithRecentWorkingDir.Text = "Open last working directory on startup";
            this.chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            this.chkFollowRenamesInFileHistory.AutoSize = true;
            this.chkFollowRenamesInFileHistory.Location = new System.Drawing.Point(3, 107);
            this.chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            this.chkFollowRenamesInFileHistory.Size = new System.Drawing.Size(283, 20);
            this.chkFollowRenamesInFileHistory.TabIndex = 14;
            this.chkFollowRenamesInFileHistory.Text = "Follow renames in file history (experimental)";
            this.chkFollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // groupBoxPerformance
            // 
            this.groupBoxPerformance.AutoSize = true;
            this.groupBoxPerformance.Controls.Add(this.tableLayoutPanelPerformance);
            this.groupBoxPerformance.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxPerformance.Location = new System.Drawing.Point(0, 0);
            this.groupBoxPerformance.Name = "groupBoxPerformance";
            this.groupBoxPerformance.Size = new System.Drawing.Size(976, 181);
            this.groupBoxPerformance.TabIndex = 55;
            this.groupBoxPerformance.TabStop = false;
            this.groupBoxPerformance.Text = "Performance";
            // 
            // tableLayoutPanelPerformance
            // 
            this.tableLayoutPanelPerformance.AutoSize = true;
            this.tableLayoutPanelPerformance.ColumnCount = 2;
            this.tableLayoutPanelPerformance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPerformance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPerformance.Controls.Add(this._NO_TRANSLATE_MaxCommits, 1, 5);
            this.tableLayoutPanelPerformance.Controls.Add(this.label12, 0, 5);
            this.tableLayoutPanelPerformance.Controls.Add(this.chkCheckForUncommittedChangesInCheckoutBranch, 0, 4);
            this.tableLayoutPanelPerformance.Controls.Add(this.chkShowGitStatusInToolbar, 0, 0);
            this.tableLayoutPanelPerformance.Controls.Add(this.chkShowStashCountInBrowseWindow, 0, 3);
            this.tableLayoutPanelPerformance.Controls.Add(this.chkUseFastChecks, 0, 2);
            this.tableLayoutPanelPerformance.Controls.Add(this.chkShowCurrentChangesInRevisionGraph, 0, 1);
            this.tableLayoutPanelPerformance.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelPerformance.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanelPerformance.Name = "tableLayoutPanelPerformance";
            this.tableLayoutPanelPerformance.RowCount = 6;
            this.tableLayoutPanelPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerformance.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerformance.Size = new System.Drawing.Size(970, 159);
            this.tableLayoutPanelPerformance.TabIndex = 0;
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(375, 133);
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
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 136);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(318, 16);
            this.label12.TabIndex = 6;
            this.label12.Text = "Limit number of commits that will be loaded at startup";
            // 
            // chkCheckForUncommittedChangesInCheckoutBranch
            // 
            this.chkCheckForUncommittedChangesInCheckoutBranch.AutoSize = true;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Location = new System.Drawing.Point(3, 107);
            this.chkCheckForUncommittedChangesInCheckoutBranch.Name = "chkCheckForUncommittedChangesInCheckoutBranch";
            this.chkCheckForUncommittedChangesInCheckoutBranch.Size = new System.Drawing.Size(359, 20);
            this.chkCheckForUncommittedChangesInCheckoutBranch.TabIndex = 5;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Text = "Check for uncommitted changes in checkout branch dialog";
            this.chkCheckForUncommittedChangesInCheckoutBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowGitStatusInToolbar
            // 
            this.chkShowGitStatusInToolbar.AutoSize = true;
            this.tableLayoutPanelPerformance.SetColumnSpan(this.chkShowGitStatusInToolbar, 2);
            this.chkShowGitStatusInToolbar.Location = new System.Drawing.Point(3, 3);
            this.chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            this.chkShowGitStatusInToolbar.Size = new System.Drawing.Size(536, 20);
            this.chkShowGitStatusInToolbar.TabIndex = 1;
            this.chkShowGitStatusInToolbar.Text = "Show repository status in browse dialog (number of changes in toolbar, restart re" +
    "quired)";
            this.chkShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            // 
            // chkShowStashCountInBrowseWindow
            // 
            this.chkShowStashCountInBrowseWindow.AutoSize = true;
            this.chkShowStashCountInBrowseWindow.Location = new System.Drawing.Point(3, 81);
            this.chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            this.chkShowStashCountInBrowseWindow.Size = new System.Drawing.Size(315, 20);
            this.chkShowStashCountInBrowseWindow.TabIndex = 4;
            this.chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            this.chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkUseFastChecks
            // 
            this.chkUseFastChecks.AutoSize = true;
            this.chkUseFastChecks.Location = new System.Drawing.Point(3, 55);
            this.chkUseFastChecks.Name = "chkUseFastChecks";
            this.chkUseFastChecks.Size = new System.Drawing.Size(323, 20);
            this.chkUseFastChecks.TabIndex = 3;
            this.chkUseFastChecks.Text = "Use FileSystemWatcher to check if index is changed";
            this.chkUseFastChecks.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentChangesInRevisionGraph
            // 
            this.chkShowCurrentChangesInRevisionGraph.AutoSize = true;
            this.chkShowCurrentChangesInRevisionGraph.Location = new System.Drawing.Point(3, 29);
            this.chkShowCurrentChangesInRevisionGraph.Name = "chkShowCurrentChangesInRevisionGraph";
            this.chkShowCurrentChangesInRevisionGraph.Size = new System.Drawing.Size(366, 20);
            this.chkShowCurrentChangesInRevisionGraph.TabIndex = 2;
            this.chkShowCurrentChangesInRevisionGraph.Text = "Show current working directory changes in revision graph (slow!)";
            this.chkShowCurrentChangesInRevisionGraph.UseVisualStyleBackColor = true;
            // 
            // groupBoxEmailSettings
            // 
            this.groupBoxEmailSettings.AutoSize = true;
            this.groupBoxEmailSettings.Controls.Add(this.tableLayoutPanelEmailSettings);
            this.groupBoxEmailSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxEmailSettings.Location = new System.Drawing.Point(0, 427);
            this.groupBoxEmailSettings.Name = "groupBoxEmailSettings";
            this.groupBoxEmailSettings.Size = new System.Drawing.Size(976, 51);
            this.groupBoxEmailSettings.TabIndex = 57;
            this.groupBoxEmailSettings.TabStop = false;
            this.groupBoxEmailSettings.Text = "Email settings for sending patches";
            // 
            // tableLayoutPanelEmailSettings
            // 
            this.tableLayoutPanelEmailSettings.AutoSize = true;
            this.tableLayoutPanelEmailSettings.ColumnCount = 3;
            this.tableLayoutPanelEmailSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelEmailSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelEmailSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelEmailSettings.Controls.Add(this.labelSmtpServerName, 0, 0);
            this.tableLayoutPanelEmailSettings.Controls.Add(this.tableLayoutPanelSmtpServer, 2, 0);
            this.tableLayoutPanelEmailSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelEmailSettings.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanelEmailSettings.Name = "tableLayoutPanelEmailSettings";
            this.tableLayoutPanelEmailSettings.RowCount = 1;
            this.tableLayoutPanelEmailSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelEmailSettings.Size = new System.Drawing.Size(970, 29);
            this.tableLayoutPanelEmailSettings.TabIndex = 61;
            // 
            // labelSmtpServerName
            // 
            this.labelSmtpServerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSmtpServerName.AutoSize = true;
            this.labelSmtpServerName.Location = new System.Drawing.Point(3, 6);
            this.labelSmtpServerName.Name = "labelSmtpServerName";
            this.labelSmtpServerName.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.labelSmtpServerName.Size = new System.Drawing.Size(127, 16);
            this.labelSmtpServerName.TabIndex = 30;
            this.labelSmtpServerName.Text = "SMTP server name";
            // 
            // tableLayoutPanelSmtpServer
            // 
            this.tableLayoutPanelSmtpServer.AutoSize = true;
            this.tableLayoutPanelSmtpServer.ColumnCount = 4;
            this.tableLayoutPanelSmtpServer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSmtpServer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSmtpServer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSmtpServer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSmtpServer.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanelSmtpServer.Controls.Add(this.SmtpServer, 0, 0);
            this.tableLayoutPanelSmtpServer.Controls.Add(this.SmtpServerPort, 2, 0);
            this.tableLayoutPanelSmtpServer.Controls.Add(this.chkUseSSL, 3, 0);
            this.tableLayoutPanelSmtpServer.Dock = System.Windows.Forms.DockStyle.Left;
            this.tableLayoutPanelSmtpServer.Location = new System.Drawing.Point(133, 0);
            this.tableLayoutPanelSmtpServer.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelSmtpServer.Name = "tableLayoutPanelSmtpServer";
            this.tableLayoutPanelSmtpServer.RowCount = 1;
            this.tableLayoutPanelSmtpServer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSmtpServer.Size = new System.Drawing.Size(384, 29);
            this.tableLayoutPanelSmtpServer.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(188, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 16);
            this.label2.TabIndex = 32;
            this.label2.Text = "Port";
            // 
            // SmtpServer
            // 
            this.SmtpServer.Location = new System.Drawing.Point(3, 3);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(179, 23);
            this.SmtpServer.TabIndex = 31;
            // 
            // SmtpServerPort
            // 
            this.SmtpServerPort.Location = new System.Drawing.Point(225, 3);
            this.SmtpServerPort.Name = "SmtpServerPort";
            this.SmtpServerPort.Size = new System.Drawing.Size(49, 23);
            this.SmtpServerPort.TabIndex = 33;
            this.SmtpServerPort.Text = "587";
            // 
            // chkUseSSL
            // 
            this.chkUseSSL.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseSSL.AutoSize = true;
            this.chkUseSSL.Location = new System.Drawing.Point(280, 4);
            this.chkUseSSL.Name = "chkUseSSL";
            this.chkUseSSL.Size = new System.Drawing.Size(101, 20);
            this.chkUseSSL.TabIndex = 34;
            this.chkUseSSL.Text = "Use SSL/TLS";
            this.chkUseSSL.UseVisualStyleBackColor = true;
            this.chkUseSSL.CheckedChanged += new System.EventHandler(this.chkUseSSL_CheckedChanged);
            // 
            // panelSpacer1
            // 
            this.panelSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSpacer1.Location = new System.Drawing.Point(0, 181);
            this.panelSpacer1.Name = "panelSpacer1";
            this.panelSpacer1.Size = new System.Drawing.Size(976, 5);
            this.panelSpacer1.TabIndex = 58;
            // 
            // panelSpacer2
            // 
            this.panelSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSpacer2.Location = new System.Drawing.Point(0, 422);
            this.panelSpacer2.Name = "panelSpacer2";
            this.panelSpacer2.Size = new System.Drawing.Size(976, 5);
            this.panelSpacer2.TabIndex = 60;
            // 
            // GitExtensionsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxEmailSettings);
            this.Controls.Add(this.panelSpacer2);
            this.Controls.Add(this.groupBoxBehaviour);
            this.Controls.Add(this.panelSpacer1);
            this.Controls.Add(this.groupBoxPerformance);
            this.Name = "GitExtensionsSettingsPage";
            this.Size = new System.Drawing.Size(976, 538);
            this.groupBoxBehaviour.ResumeLayout(false);
            this.groupBoxBehaviour.PerformLayout();
            this.tableLayoutPanelBehaviour.ResumeLayout(false);
            this.tableLayoutPanelBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            this.groupBoxPerformance.ResumeLayout(false);
            this.groupBoxPerformance.PerformLayout();
            this.tableLayoutPanelPerformance.ResumeLayout(false);
            this.tableLayoutPanelPerformance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.groupBoxEmailSettings.ResumeLayout(false);
            this.groupBoxEmailSettings.PerformLayout();
            this.tableLayoutPanelEmailSettings.ResumeLayout(false);
            this.tableLayoutPanelEmailSettings.PerformLayout();
            this.tableLayoutPanelSmtpServer.ResumeLayout(false);
            this.tableLayoutPanelSmtpServer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBehaviour;
        private System.Windows.Forms.CheckBox chkPlaySpecialStartupSound;
        private System.Windows.Forms.CheckBox chkCloseProcessDialog;
        private System.Windows.Forms.CheckBox chkShowGitCommandLine;
        private System.Windows.Forms.CheckBox chkStartWithRecentWorkingDir;
        private System.Windows.Forms.NumericUpDown RevisionGridQuickSearchTimeout;
        private System.Windows.Forms.CheckBox chkStashUntrackedFiles;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.CheckBox chkUsePatienceDiffAlgorithm;
        private System.Windows.Forms.CheckBox chkFollowRenamesInFileHistory;
        private System.Windows.Forms.GroupBox groupBoxPerformance;
        private System.Windows.Forms.CheckBox chkCheckForUncommittedChangesInCheckoutBranch;
        private System.Windows.Forms.CheckBox chkShowGitStatusInToolbar;
        private System.Windows.Forms.CheckBox chkShowCurrentChangesInRevisionGraph;
        private System.Windows.Forms.CheckBox chkUseFastChecks;
        private System.Windows.Forms.CheckBox chkShowStashCountInBrowseWindow;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_MaxCommits;
        private System.Windows.Forms.GroupBox groupBoxEmailSettings;
        private System.Windows.Forms.CheckBox chkUseSSL;
        private System.Windows.Forms.TextBox SmtpServerPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SmtpServer;
        private System.Windows.Forms.Label labelSmtpServerName;
        private System.Windows.Forms.Label lblDefaultCloneDestination;
        private System.Windows.Forms.ComboBox cbDefaultCloneDestination;
        private System.Windows.Forms.Button btnDefaultDestinationBrowse;
        private System.Windows.Forms.Panel panelSpacer1;
        private System.Windows.Forms.Panel panelSpacer2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPerformance;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBehaviour;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelEmailSettings;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSmtpServer;
    }
}
