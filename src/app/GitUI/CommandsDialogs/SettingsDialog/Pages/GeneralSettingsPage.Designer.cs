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
            TableLayoutPanel tlpnlMain;
            groupBoxTelemetry = new GroupBox();
            tlpnlTelemetry = new TableLayoutPanel();
            chkTelemetry = new CheckBox();
            llblTelemetryPrivacyLink = new LinkLabel();
            groupBoxPerformance = new GroupBox();
            tlpnlPerformance = new TableLayoutPanel();
            chkShowAheadBehindDataInBrowseWindow = new CheckBox();
            chkCheckForUncommittedChangesInCheckoutBranch = new CheckBox();
            chkShowGitStatusInToolbar = new CheckBox();
            chkShowGitStatusForArtificialCommits = new CheckBox();
            chkShowStashCountInBrowseWindow = new CheckBox();
            chkShowSubmoduleStatusInBrowse = new CheckBox();
            lblCommitsLimit = new CheckBox();
            _NO_TRANSLATE_MaxCommits = new NumericUpDown();
            groupBoxBehaviour = new GroupBox();
            tlpnlBehaviour = new TableLayoutPanel();
            chkFollowRenamesInFileHistoryExact = new CheckBox();
            RevisionGridQuickSearchTimeout = new NumericUpDown();
            btnDefaultDestinationBrowse = new Button();
            lblQuickSearchTimeout = new Label();
            chkCloseProcessDialog = new CheckBox();
            cbDefaultCloneDestination = new ComboBox();
            chkShowGitCommandLine = new CheckBox();
            lblDefaultCloneDestination = new Label();
            chkUseHistogramDiffAlgorithm = new CheckBox();
            chkStashUntrackedFiles = new CheckBox();
            chkStartWithRecentWorkingDir = new CheckBox();
            chkFollowRenamesInFileHistory = new CheckBox();
            lblDefaultPullAction = new Label();
            cboDefaultPullAction = new ComboBox();
            chkUpdateModules = new CheckBox();
            tlpnlMain = new TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            groupBoxTelemetry.SuspendLayout();
            tlpnlTelemetry.SuspendLayout();
            groupBoxPerformance.SuspendLayout();
            tlpnlPerformance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_MaxCommits)).BeginInit();
            groupBoxBehaviour.SuspendLayout();
            tlpnlBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(RevisionGridQuickSearchTimeout)).BeginInit();
            SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(groupBoxTelemetry, 0, 2);
            tlpnlMain.Controls.Add(groupBoxPerformance, 0, 0);
            tlpnlMain.Controls.Add(groupBoxBehaviour, 0, 1);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.Size = new Size(1263, 507);
            tlpnlMain.TabIndex = 0;
            // 
            // groupBoxTelemetry
            // 
            groupBoxTelemetry.AutoSize = true;
            groupBoxTelemetry.Controls.Add(tlpnlTelemetry);
            groupBoxTelemetry.Dock = DockStyle.Fill;
            groupBoxTelemetry.Location = new Point(3, 503);
            groupBoxTelemetry.Name = "groupBoxTelemetry";
            groupBoxTelemetry.Padding = new Padding(8);
            groupBoxTelemetry.Size = new Size(1257, 52);
            groupBoxTelemetry.TabIndex = 3;
            groupBoxTelemetry.TabStop = false;
            groupBoxTelemetry.Text = "Telemetry";
            // 
            // tlpnlTelemetry
            // 
            tlpnlTelemetry.AutoSize = true;
            tlpnlTelemetry.ColumnCount = 2;
            tlpnlTelemetry.ColumnStyles.Add(new ColumnStyle());
            tlpnlTelemetry.ColumnStyles.Add(new ColumnStyle());
            tlpnlTelemetry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tlpnlTelemetry.Controls.Add(chkTelemetry, 0, 0);
            tlpnlTelemetry.Controls.Add(llblTelemetryPrivacyLink, 1, 0);
            tlpnlTelemetry.Dock = DockStyle.Top;
            tlpnlTelemetry.Location = new Point(8, 21);
            tlpnlTelemetry.Name = "tlpnlTelemetry";
            tlpnlTelemetry.RowCount = 1;
            tlpnlTelemetry.RowStyles.Add(new RowStyle());
            tlpnlTelemetry.Size = new Size(1241, 23);
            tlpnlTelemetry.TabIndex = 0;
            // 
            // chkTelemetry
            // 
            chkTelemetry.AutoSize = true;
            chkTelemetry.Dock = DockStyle.Fill;
            chkTelemetry.Location = new Point(3, 3);
            chkTelemetry.Name = "chkTelemetry";
            chkTelemetry.Size = new Size(128, 17);
            chkTelemetry.TabIndex = 0;
            chkTelemetry.Text = "Yes, I allow telemetry!";
            chkTelemetry.UseVisualStyleBackColor = true;
            // 
            // llblTelemetryPrivacyLink
            // 
            llblTelemetryPrivacyLink.Dock = DockStyle.Fill;
            llblTelemetryPrivacyLink.Location = new Point(137, 0);
            llblTelemetryPrivacyLink.Name = "llblTelemetryPrivacyLink";
            llblTelemetryPrivacyLink.Size = new Size(1357, 23);
            llblTelemetryPrivacyLink.TabIndex = 1;
            llblTelemetryPrivacyLink.TabStop = true;
            llblTelemetryPrivacyLink.Text = "Why and what is captured?";
            llblTelemetryPrivacyLink.TextAlign = ContentAlignment.MiddleLeft;
            llblTelemetryPrivacyLink.LinkClicked += LlblTelemetryPrivacyLink_LinkClicked;
            // 
            // groupBoxPerformance
            // 
            groupBoxPerformance.AutoSize = true;
            groupBoxPerformance.Controls.Add(tlpnlPerformance);
            groupBoxPerformance.Dock = DockStyle.Fill;
            groupBoxPerformance.Location = new Point(3, 3);
            groupBoxPerformance.Name = "groupBoxPerformance";
            groupBoxPerformance.Padding = new Padding(8);
            groupBoxPerformance.Size = new Size(1257, 216);
            groupBoxPerformance.TabIndex = 0;
            groupBoxPerformance.TabStop = false;
            groupBoxPerformance.Text = "Performance";
            // 
            // tlpnlPerformance
            // 
            tlpnlPerformance.AutoSize = true;
            tlpnlPerformance.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlPerformance.ColumnCount = 2;
            tlpnlPerformance.ColumnStyles.Add(new ColumnStyle());
            tlpnlPerformance.ColumnStyles.Add(new ColumnStyle());
            tlpnlPerformance.Controls.Add(chkShowAheadBehindDataInBrowseWindow, 0, 4);
            tlpnlPerformance.Controls.Add(chkCheckForUncommittedChangesInCheckoutBranch, 0, 5);
            tlpnlPerformance.Controls.Add(chkShowGitStatusInToolbar, 0, 0);
            tlpnlPerformance.Controls.Add(chkShowGitStatusForArtificialCommits, 0, 1);
            tlpnlPerformance.Controls.Add(chkShowStashCountInBrowseWindow, 0, 3);
            tlpnlPerformance.Controls.Add(chkShowSubmoduleStatusInBrowse, 0, 2);
            tlpnlPerformance.Controls.Add(lblCommitsLimit, 0, 6);
            tlpnlPerformance.Controls.Add(_NO_TRANSLATE_MaxCommits, 1, 6);
            tlpnlPerformance.Dock = DockStyle.Top;
            tlpnlPerformance.Location = new Point(8, 21);
            tlpnlPerformance.Name = "tlpnlPerformance";
            tlpnlPerformance.RowCount = 7;
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.RowStyles.Add(new RowStyle());
            tlpnlPerformance.Size = new Size(1241, 187);
            tlpnlPerformance.TabIndex = 0;
            // 
            // chkShowAheadBehindDataInBrowseWindow
            // 
            chkShowAheadBehindDataInBrowseWindow.AutoSize = true;
            chkShowAheadBehindDataInBrowseWindow.Dock = DockStyle.Fill;
            chkShowAheadBehindDataInBrowseWindow.Location = new Point(3, 118);
            chkShowAheadBehindDataInBrowseWindow.Name = "chkShowAheadBehindDataInBrowseWindow";
            chkShowAheadBehindDataInBrowseWindow.Size = new Size(347, 17);
            chkShowAheadBehindDataInBrowseWindow.TabIndex = 6;
            chkShowAheadBehindDataInBrowseWindow.Text = "Show ahead and behind information on status bar in browse window";
            chkShowAheadBehindDataInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkCheckForUncommittedChangesInCheckoutBranch
            // 
            chkCheckForUncommittedChangesInCheckoutBranch.AutoSize = true;
            chkCheckForUncommittedChangesInCheckoutBranch.Dock = DockStyle.Fill;
            chkCheckForUncommittedChangesInCheckoutBranch.Location = new Point(3, 141);
            chkCheckForUncommittedChangesInCheckoutBranch.Name = "chkCheckForUncommittedChangesInCheckoutBranch";
            chkCheckForUncommittedChangesInCheckoutBranch.Size = new Size(347, 17);
            chkCheckForUncommittedChangesInCheckoutBranch.TabIndex = 7;
            chkCheckForUncommittedChangesInCheckoutBranch.Text = "Check for uncommitted changes in checkout branch dialog";
            chkCheckForUncommittedChangesInCheckoutBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowGitStatusInToolbar
            // 
            chkShowGitStatusInToolbar.AutoSize = true;
            tlpnlPerformance.SetColumnSpan(chkShowGitStatusInToolbar, 2);
            chkShowGitStatusInToolbar.Dock = DockStyle.Fill;
            chkShowGitStatusInToolbar.Location = new Point(3, 3);
            chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            chkShowGitStatusInToolbar.Size = new Size(1235, 17);
            chkShowGitStatusInToolbar.TabIndex = 0;
            chkShowGitStatusInToolbar.Text = "Show number of changed files on commit button";
            chkShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            chkShowGitStatusInToolbar.CheckedChanged += ShowGitStatus_CheckedChanged;
            // 
            // chkShowGitStatusForArtificialCommits
            // 
            chkShowGitStatusForArtificialCommits.AutoSize = true;
            tlpnlPerformance.SetColumnSpan(chkShowGitStatusForArtificialCommits, 2);
            chkShowGitStatusForArtificialCommits.Dock = DockStyle.Fill;
            chkShowGitStatusForArtificialCommits.Location = new Point(3, 26);
            chkShowGitStatusForArtificialCommits.Name = "chkShowGitStatusForArtificialCommits";
            chkShowGitStatusForArtificialCommits.Size = new Size(1235, 17);
            chkShowGitStatusForArtificialCommits.TabIndex = 1;
            chkShowGitStatusForArtificialCommits.Text = "Show number of changed files for artificial commits";
            chkShowGitStatusForArtificialCommits.UseVisualStyleBackColor = true;
            chkShowGitStatusForArtificialCommits.CheckedChanged += ShowGitStatus_CheckedChanged;
            // 
            // chkShowStashCountInBrowseWindow
            // 
            chkShowStashCountInBrowseWindow.AutoSize = true;
            chkShowStashCountInBrowseWindow.Dock = DockStyle.Fill;
            chkShowStashCountInBrowseWindow.Location = new Point(3, 95);
            chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            chkShowStashCountInBrowseWindow.Size = new Size(347, 17);
            chkShowStashCountInBrowseWindow.TabIndex = 5;
            chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkShowSubmoduleStatusInBrowse
            // 
            chkShowSubmoduleStatusInBrowse.AutoSize = true;
            chkShowSubmoduleStatusInBrowse.Dock = DockStyle.Fill;
            chkShowSubmoduleStatusInBrowse.Location = new Point(3, 49);
            chkShowSubmoduleStatusInBrowse.Name = "chkShowSubmoduleStatusInBrowse";
            chkShowSubmoduleStatusInBrowse.Size = new Size(347, 17);
            chkShowSubmoduleStatusInBrowse.TabIndex = 2;
            chkShowSubmoduleStatusInBrowse.Text = "Show submodule status in browse menu";
            chkShowSubmoduleStatusInBrowse.UseVisualStyleBackColor = true;
            // 
            // lblCommitsLimit
            // 
            lblCommitsLimit.AutoSize = true;
            lblCommitsLimit.Dock = DockStyle.Fill;
            lblCommitsLimit.Location = new Point(3, 164);
            lblCommitsLimit.Name = "lblCommitsLimit";
            lblCommitsLimit.Size = new Size(347, 20);
            lblCommitsLimit.TabIndex = 7;
            lblCommitsLimit.Text = "Limit number of commits to be loaded";
            lblCommitsLimit.CheckedChanged += lblCommitsLimit_CheckedChanged;
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            _NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            _NO_TRANSLATE_MaxCommits.Location = new Point(356, 164);
            _NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            _NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            _NO_TRANSLATE_MaxCommits.Size = new Size(85, 20);
            _NO_TRANSLATE_MaxCommits.TabIndex = 8;
            _NO_TRANSLATE_MaxCommits.TextAlign = HorizontalAlignment.Right;
            _NO_TRANSLATE_MaxCommits.ThousandsSeparator = true;
            _NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // groupBoxBehaviour
            // 
            groupBoxBehaviour.AutoSize = true;
            groupBoxBehaviour.Controls.Add(tlpnlBehaviour);
            groupBoxBehaviour.Dock = DockStyle.Fill;
            groupBoxBehaviour.Location = new Point(3, 225);
            groupBoxBehaviour.Name = "groupBoxBehaviour";
            groupBoxBehaviour.Padding = new Padding(8);
            groupBoxBehaviour.Size = new Size(1257, 272);
            groupBoxBehaviour.TabIndex = 1;
            groupBoxBehaviour.TabStop = false;
            groupBoxBehaviour.Text = "Behaviour";
            // 
            // tlpnlBehaviour
            // 
            tlpnlBehaviour.AutoSize = true;
            tlpnlBehaviour.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlBehaviour.ColumnCount = 3;
            tlpnlBehaviour.ColumnStyles.Add(new ColumnStyle());
            tlpnlBehaviour.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlBehaviour.ColumnStyles.Add(new ColumnStyle());
            tlpnlBehaviour.Controls.Add(chkFollowRenamesInFileHistoryExact, 1, 5);
            tlpnlBehaviour.Controls.Add(RevisionGridQuickSearchTimeout, 1, 9);
            tlpnlBehaviour.Controls.Add(btnDefaultDestinationBrowse, 2, 7);
            tlpnlBehaviour.Controls.Add(lblQuickSearchTimeout, 0, 9);
            tlpnlBehaviour.Controls.Add(chkCloseProcessDialog, 0, 0);
            tlpnlBehaviour.Controls.Add(cbDefaultCloneDestination, 1, 7);
            tlpnlBehaviour.Controls.Add(chkShowGitCommandLine, 0, 1);
            tlpnlBehaviour.Controls.Add(lblDefaultCloneDestination, 0, 7);
            tlpnlBehaviour.Controls.Add(chkUseHistogramDiffAlgorithm, 0, 2);
            tlpnlBehaviour.Controls.Add(chkStashUntrackedFiles, 0, 3);
            tlpnlBehaviour.Controls.Add(chkStartWithRecentWorkingDir, 0, 6);
            tlpnlBehaviour.Controls.Add(chkFollowRenamesInFileHistory, 0, 5);
            tlpnlBehaviour.Controls.Add(lblDefaultPullAction, 0, 8);
            tlpnlBehaviour.Controls.Add(cboDefaultPullAction, 1, 8);
            tlpnlBehaviour.Controls.Add(chkUpdateModules, 0, 4);
            tlpnlBehaviour.Dock = DockStyle.Top;
            tlpnlBehaviour.Location = new Point(8, 21);
            tlpnlBehaviour.Name = "tlpnlBehaviour";
            tlpnlBehaviour.RowCount = 10;
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.RowStyles.Add(new RowStyle());
            tlpnlBehaviour.Size = new Size(1241, 243);
            tlpnlBehaviour.TabIndex = 0;
            // 
            // chkFollowRenamesInFileHistoryExact
            // 
            chkFollowRenamesInFileHistoryExact.AutoSize = true;
            chkFollowRenamesInFileHistoryExact.Dock = DockStyle.Fill;
            chkFollowRenamesInFileHistoryExact.Location = new Point(273, 118);
            chkFollowRenamesInFileHistoryExact.Name = "chkFollowRenamesInFileHistoryExact";
            chkFollowRenamesInFileHistoryExact.Size = new Size(907, 17);
            chkFollowRenamesInFileHistoryExact.TabIndex = 6;
            chkFollowRenamesInFileHistoryExact.Text = "Follow exact renames and copies only";
            chkFollowRenamesInFileHistoryExact.UseVisualStyleBackColor = true;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            RevisionGridQuickSearchTimeout.Location = new Point(273, 220);
            RevisionGridQuickSearchTimeout.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            RevisionGridQuickSearchTimeout.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            RevisionGridQuickSearchTimeout.Name = "RevisionGridQuickSearchTimeout";
            RevisionGridQuickSearchTimeout.Size = new Size(85, 20);
            RevisionGridQuickSearchTimeout.TabIndex = 11;
            RevisionGridQuickSearchTimeout.TextAlign = HorizontalAlignment.Right;
            RevisionGridQuickSearchTimeout.ThousandsSeparator = true;
            RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // btnDefaultDestinationBrowse
            // 
            btnDefaultDestinationBrowse.AutoSize = true;
            btnDefaultDestinationBrowse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDefaultDestinationBrowse.Location = new Point(1186, 164);
            btnDefaultDestinationBrowse.Name = "btnDefaultDestinationBrowse";
            btnDefaultDestinationBrowse.Size = new Size(52, 23);
            btnDefaultDestinationBrowse.TabIndex = 9;
            btnDefaultDestinationBrowse.Text = "Browse";
            btnDefaultDestinationBrowse.UseVisualStyleBackColor = true;
            btnDefaultDestinationBrowse.Click += DefaultCloneDestinationBrowseClick;
            // 
            // lblQuickSearchTimeout
            // 
            lblQuickSearchTimeout.AutoSize = true;
            lblQuickSearchTimeout.Dock = DockStyle.Fill;
            lblQuickSearchTimeout.Location = new Point(3, 217);
            lblQuickSearchTimeout.Name = "lblQuickSearchTimeout";
            lblQuickSearchTimeout.Size = new Size(264, 26);
            lblQuickSearchTimeout.TabIndex = 11;
            lblQuickSearchTimeout.Text = "Revision grid quick search timeout [ms]";
            lblQuickSearchTimeout.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkCloseProcessDialog
            // 
            chkCloseProcessDialog.AutoSize = true;
            chkCloseProcessDialog.Dock = DockStyle.Fill;
            chkCloseProcessDialog.Location = new Point(3, 3);
            chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            chkCloseProcessDialog.Size = new Size(264, 17);
            chkCloseProcessDialog.TabIndex = 0;
            chkCloseProcessDialog.Text = "Close Process dialog when process succeeds";
            chkCloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // cbDefaultCloneDestination
            // 
            cbDefaultCloneDestination.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbDefaultCloneDestination.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            cbDefaultCloneDestination.Dock = DockStyle.Fill;
            cbDefaultCloneDestination.FormattingEnabled = true;
            cbDefaultCloneDestination.Location = new Point(273, 164);
            cbDefaultCloneDestination.Name = "cbDefaultCloneDestination";
            cbDefaultCloneDestination.Size = new Size(907, 21);
            cbDefaultCloneDestination.TabIndex = 8;
            // 
            // chkShowGitCommandLine
            // 
            chkShowGitCommandLine.AutoSize = true;
            chkShowGitCommandLine.Dock = DockStyle.Fill;
            chkShowGitCommandLine.Location = new Point(3, 26);
            chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            chkShowGitCommandLine.Size = new Size(264, 17);
            chkShowGitCommandLine.TabIndex = 1;
            chkShowGitCommandLine.Text = "Show console window when executing git process";
            chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // lblDefaultCloneDestination
            // 
            lblDefaultCloneDestination.AutoSize = true;
            lblDefaultCloneDestination.Dock = DockStyle.Fill;
            lblDefaultCloneDestination.Location = new Point(3, 161);
            lblDefaultCloneDestination.Name = "lblDefaultCloneDestination";
            lblDefaultCloneDestination.Size = new Size(264, 29);
            lblDefaultCloneDestination.TabIndex = 8;
            lblDefaultCloneDestination.Text = "Default clone destination";
            lblDefaultCloneDestination.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkUseHistogramDiffAlgorithm
            // 
            chkUseHistogramDiffAlgorithm.AutoSize = true;
            chkUseHistogramDiffAlgorithm.Dock = DockStyle.Fill;
            chkUseHistogramDiffAlgorithm.Location = new Point(3, 49);
            chkUseHistogramDiffAlgorithm.Name = "chkUseHistogramDiffAlgorithm";
            chkUseHistogramDiffAlgorithm.Size = new Size(264, 17);
            chkUseHistogramDiffAlgorithm.TabIndex = 2;
            chkUseHistogramDiffAlgorithm.Text = "Use histogram diff algorithm";
            chkUseHistogramDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkStashUntrackedFiles
            // 
            chkStashUntrackedFiles.AutoSize = true;
            chkStashUntrackedFiles.Dock = DockStyle.Fill;
            chkStashUntrackedFiles.Location = new Point(3, 72);
            chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            chkStashUntrackedFiles.Size = new Size(264, 17);
            chkStashUntrackedFiles.TabIndex = 3;
            chkStashUntrackedFiles.Text = "Include untracked files in autostash";
            chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            chkStartWithRecentWorkingDir.AutoSize = true;
            chkStartWithRecentWorkingDir.Dock = DockStyle.Fill;
            chkStartWithRecentWorkingDir.Location = new Point(3, 141);
            chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            chkStartWithRecentWorkingDir.Size = new Size(264, 17);
            chkStartWithRecentWorkingDir.TabIndex = 7;
            chkStartWithRecentWorkingDir.Text = "Open last working directory on startup";
            chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            chkFollowRenamesInFileHistory.AutoSize = true;
            chkFollowRenamesInFileHistory.Dock = DockStyle.Fill;
            chkFollowRenamesInFileHistory.Location = new Point(3, 118);
            chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            chkFollowRenamesInFileHistory.Size = new Size(264, 17);
            chkFollowRenamesInFileHistory.TabIndex = 5;
            chkFollowRenamesInFileHistory.Text = "Follow renames in file history";
            chkFollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // lblDefaultPullAction
            // 
            lblDefaultPullAction.AutoSize = true;
            lblDefaultPullAction.Dock = DockStyle.Fill;
            lblDefaultPullAction.Location = new Point(3, 190);
            lblDefaultPullAction.Name = "lblDefaultPullAction";
            lblDefaultPullAction.Size = new Size(264, 27);
            lblDefaultPullAction.TabIndex = 14;
            lblDefaultPullAction.Text = "Default pull action";
            lblDefaultPullAction.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboDefaultPullAction
            // 
            cboDefaultPullAction.FormattingEnabled = true;
            cboDefaultPullAction.Location = new Point(273, 193);
            cboDefaultPullAction.Name = "cboDefaultPullAction";
            cboDefaultPullAction.Size = new Size(121, 21);
            cboDefaultPullAction.TabIndex = 10;
            // 
            // chkUpdateModules
            // 
            chkUpdateModules.AutoSize = true;
            chkUpdateModules.Location = new Point(3, 95);
            chkUpdateModules.Name = "chkUpdateModules";
            chkUpdateModules.Size = new Size(183, 17);
            chkUpdateModules.TabIndex = 4;
            chkUpdateModules.Text = "Update submodules on checkout";
            chkUpdateModules.ThreeState = true;
            chkUpdateModules.UseVisualStyleBackColor = true;
            // 
            // GeneralSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            Controls.Add(tlpnlMain);
            Name = "GeneralSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1279, 523);
            Text = "General";
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            groupBoxTelemetry.ResumeLayout(false);
            groupBoxTelemetry.PerformLayout();
            tlpnlTelemetry.ResumeLayout(false);
            tlpnlTelemetry.PerformLayout();
            groupBoxPerformance.ResumeLayout(false);
            groupBoxPerformance.PerformLayout();
            tlpnlPerformance.ResumeLayout(false);
            tlpnlPerformance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_MaxCommits)).EndInit();
            groupBoxBehaviour.ResumeLayout(false);
            groupBoxBehaviour.PerformLayout();
            tlpnlBehaviour.ResumeLayout(false);
            tlpnlBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(RevisionGridQuickSearchTimeout)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox groupBoxBehaviour;
        private CheckBox chkCloseProcessDialog;
        private CheckBox chkShowGitCommandLine;
        private CheckBox chkStartWithRecentWorkingDir;
        private NumericUpDown RevisionGridQuickSearchTimeout;
        private CheckBox chkStashUntrackedFiles;
        private Label lblQuickSearchTimeout;
        private CheckBox chkUseHistogramDiffAlgorithm;
        private CheckBox chkFollowRenamesInFileHistory;
        private GroupBox groupBoxPerformance;
        private CheckBox chkCheckForUncommittedChangesInCheckoutBranch;
        private CheckBox chkShowGitStatusInToolbar;
        private CheckBox chkShowGitStatusForArtificialCommits;
        private CheckBox chkShowStashCountInBrowseWindow;
        private CheckBox chkShowSubmoduleStatusInBrowse;
        private CheckBox lblCommitsLimit;
        private NumericUpDown _NO_TRANSLATE_MaxCommits;
        private Label lblDefaultCloneDestination;
        private ComboBox cbDefaultCloneDestination;
        private Button btnDefaultDestinationBrowse;
        private TableLayoutPanel tlpnlPerformance;
        private TableLayoutPanel tlpnlBehaviour;
        private CheckBox chkFollowRenamesInFileHistoryExact;
        private CheckBox chkShowAheadBehindDataInBrowseWindow;
        private GroupBox groupBoxTelemetry;
        private TableLayoutPanel tlpnlTelemetry;
        private CheckBox chkTelemetry;
        private LinkLabel llblTelemetryPrivacyLink;
        private Label lblDefaultPullAction;
        private ComboBox cboDefaultPullAction;
        private CheckBox chkUpdateModules;
    }
}
