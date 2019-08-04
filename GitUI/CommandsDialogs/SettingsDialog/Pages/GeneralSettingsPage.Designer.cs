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
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
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
            this.groupBoxTelemetry = new System.Windows.Forms.GroupBox();
            this.tlpnlTelemetry = new System.Windows.Forms.TableLayoutPanel();
            this.chkTelemetry = new System.Windows.Forms.CheckBox();
            this.llblTelemetryPrivacyLink = new System.Windows.Forms.LinkLabel();
            this.lblDefaultPullAction = new System.Windows.Forms.Label();
            this.cboDefaultPullAction = new System.Windows.Forms.ComboBox();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.groupBoxPerformance.SuspendLayout();
            this.tlpnlPerformance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.groupBoxBehaviour.SuspendLayout();
            this.tlpnlBehaviour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            this.groupBoxTelemetry.SuspendLayout();
            this.tlpnlTelemetry.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlMain.Controls.Add(this.groupBoxTelemetry, 0, 2);
            tlpnlMain.Controls.Add(this.groupBoxPerformance, 0, 0);
            tlpnlMain.Controls.Add(this.groupBoxBehaviour, 0, 1);
            tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlMain.Location = new System.Drawing.Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlMain.Size = new System.Drawing.Size(1324, 873);
            tlpnlMain.TabIndex = 0;
            // 
            // groupBoxPerformance
            // 
            this.groupBoxPerformance.AutoSize = true;
            this.groupBoxPerformance.Controls.Add(this.tlpnlPerformance);
            this.groupBoxPerformance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxPerformance.Location = new System.Drawing.Point(3, 3);
            this.groupBoxPerformance.Name = "groupBoxPerformance";
            this.groupBoxPerformance.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxPerformance.Size = new System.Drawing.Size(1318, 239);
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
            this.tlpnlPerformance.Controls.Add(this._NO_TRANSLATE_MaxCommits, 1, 7);
            this.tlpnlPerformance.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpnlPerformance.Location = new System.Drawing.Point(8, 21);
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
            this.tlpnlPerformance.Size = new System.Drawing.Size(1302, 210);
            this.tlpnlPerformance.TabIndex = 0;
            // 
            // chkShowAheadBehindDataInBrowseWindow
            // 
            this.chkShowAheadBehindDataInBrowseWindow.AutoSize = true;
            this.chkShowAheadBehindDataInBrowseWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowAheadBehindDataInBrowseWindow.Location = new System.Drawing.Point(3, 141);
            this.chkShowAheadBehindDataInBrowseWindow.Name = "chkShowAheadBehindDataInBrowseWindow";
            this.chkShowAheadBehindDataInBrowseWindow.Size = new System.Drawing.Size(347, 17);
            this.chkShowAheadBehindDataInBrowseWindow.TabIndex = 10;
            this.chkShowAheadBehindDataInBrowseWindow.Text = "Show ahead and behind information on status bar in browse window";
            this.chkShowAheadBehindDataInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkCheckForUncommittedChangesInCheckoutBranch
            // 
            this.chkCheckForUncommittedChangesInCheckoutBranch.AutoSize = true;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Location = new System.Drawing.Point(3, 164);
            this.chkCheckForUncommittedChangesInCheckoutBranch.Name = "chkCheckForUncommittedChangesInCheckoutBranch";
            this.chkCheckForUncommittedChangesInCheckoutBranch.Size = new System.Drawing.Size(347, 17);
            this.chkCheckForUncommittedChangesInCheckoutBranch.TabIndex = 6;
            this.chkCheckForUncommittedChangesInCheckoutBranch.Text = "Check for uncommitted changes in checkout branch dialog";
            this.chkCheckForUncommittedChangesInCheckoutBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowGitStatusInToolbar
            // 
            this.chkShowGitStatusInToolbar.AutoSize = true;
            this.tlpnlPerformance.SetColumnSpan(this.chkShowGitStatusInToolbar, 2);
            this.chkShowGitStatusInToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowGitStatusInToolbar.Location = new System.Drawing.Point(3, 3);
            this.chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            this.chkShowGitStatusInToolbar.Size = new System.Drawing.Size(1296, 17);
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
            this.chkShowGitStatusForArtificialCommits.Location = new System.Drawing.Point(3, 26);
            this.chkShowGitStatusForArtificialCommits.Name = "chkShowGitStatusForArtificialCommits";
            this.chkShowGitStatusForArtificialCommits.Size = new System.Drawing.Size(1296, 17);
            this.chkShowGitStatusForArtificialCommits.TabIndex = 1;
            this.chkShowGitStatusForArtificialCommits.Text = "Show number of changed files for artificial commits";
            this.chkShowGitStatusForArtificialCommits.UseVisualStyleBackColor = true;
            this.chkShowGitStatusForArtificialCommits.CheckedChanged += new System.EventHandler(this.ShowGitStatus_CheckedChanged);
            // 
            // chkShowStashCountInBrowseWindow
            // 
            this.chkShowStashCountInBrowseWindow.AutoSize = true;
            this.chkShowStashCountInBrowseWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowStashCountInBrowseWindow.Location = new System.Drawing.Point(3, 118);
            this.chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            this.chkShowStashCountInBrowseWindow.Size = new System.Drawing.Size(347, 17);
            this.chkShowStashCountInBrowseWindow.TabIndex = 5;
            this.chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            this.chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // chkShowSubmoduleStatusInBrowse
            // 
            this.chkShowSubmoduleStatusInBrowse.AutoSize = true;
            this.chkShowSubmoduleStatusInBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowSubmoduleStatusInBrowse.Location = new System.Drawing.Point(3, 49);
            this.chkShowSubmoduleStatusInBrowse.Name = "chkShowSubmoduleStatusInBrowse";
            this.chkShowSubmoduleStatusInBrowse.Size = new System.Drawing.Size(347, 17);
            this.chkShowSubmoduleStatusInBrowse.TabIndex = 2;
            this.chkShowSubmoduleStatusInBrowse.Text = "Show submodule status in browse menu";
            this.chkShowSubmoduleStatusInBrowse.UseVisualStyleBackColor = true;
            // 
            // chkUseFastChecks
            // 
            this.chkUseFastChecks.AutoSize = true;
            this.chkUseFastChecks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkUseFastChecks.Location = new System.Drawing.Point(3, 95);
            this.chkUseFastChecks.Name = "chkUseFastChecks";
            this.chkUseFastChecks.Size = new System.Drawing.Size(347, 17);
            this.chkUseFastChecks.TabIndex = 4;
            this.chkUseFastChecks.Text = "Use FileSystemWatcher to check if index is changed";
            this.chkUseFastChecks.UseVisualStyleBackColor = true;
            // 
            // lblCommitsLimit
            // 
            this.lblCommitsLimit.AutoSize = true;
            this.lblCommitsLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommitsLimit.Location = new System.Drawing.Point(3, 184);
            this.lblCommitsLimit.Name = "lblCommitsLimit";
            this.lblCommitsLimit.Size = new System.Drawing.Size(347, 26);
            this.lblCommitsLimit.TabIndex = 7;
            this.lblCommitsLimit.Text = "Limit number of commits that will be loaded at startup";
            this.lblCommitsLimit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(356, 187);
            this._NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            this._NO_TRANSLATE_MaxCommits.Size = new System.Drawing.Size(85, 20);
            this._NO_TRANSLATE_MaxCommits.TabIndex = 8;
            this._NO_TRANSLATE_MaxCommits.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._NO_TRANSLATE_MaxCommits.ThousandsSeparator = true;
            this._NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // groupBoxBehaviour
            // 
            this.groupBoxBehaviour.AutoSize = true;
            this.groupBoxBehaviour.Controls.Add(this.tlpnlBehaviour);
            this.groupBoxBehaviour.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxBehaviour.Location = new System.Drawing.Point(3, 248);
            this.groupBoxBehaviour.Name = "groupBoxBehaviour";
            this.groupBoxBehaviour.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxBehaviour.Size = new System.Drawing.Size(1188, 249);
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
            this.tlpnlBehaviour.Controls.Add(this.RevisionGridQuickSearchTimeout, 1, 8);
            this.tlpnlBehaviour.Controls.Add(this.btnDefaultDestinationBrowse, 2, 6);
            this.tlpnlBehaviour.Controls.Add(this.lblQuickSearchTimeout, 0, 8);
            this.tlpnlBehaviour.Controls.Add(this.chkCloseProcessDialog, 0, 0);
            this.tlpnlBehaviour.Controls.Add(this.cbDefaultCloneDestination, 1, 6);
            this.tlpnlBehaviour.Controls.Add(this.chkShowGitCommandLine, 0, 1);
            this.tlpnlBehaviour.Controls.Add(this.lblDefaultCloneDestination, 0, 6);
            this.tlpnlBehaviour.Controls.Add(this.chkUsePatienceDiffAlgorithm, 0, 2);
            this.tlpnlBehaviour.Controls.Add(this.chkStashUntrackedFiles, 0, 3);
            this.tlpnlBehaviour.Controls.Add(this.chkStartWithRecentWorkingDir, 0, 5);
            this.tlpnlBehaviour.Controls.Add(this.chkFollowRenamesInFileHistory, 0, 4);
            this.tlpnlBehaviour.Controls.Add(this.lblDefaultPullAction, 0, 7);
            this.tlpnlBehaviour.Controls.Add(this.cboDefaultPullAction, 1, 7);
            this.tlpnlBehaviour.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpnlBehaviour.Location = new System.Drawing.Point(8, 21);
            this.tlpnlBehaviour.Name = "tlpnlBehaviour";
            this.tlpnlBehaviour.RowCount = 9;
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlBehaviour.Size = new System.Drawing.Size(1172, 220);
            this.tlpnlBehaviour.TabIndex = 0;
            // 
            // chkFollowRenamesInFileHistoryExact
            // 
            this.chkFollowRenamesInFileHistoryExact.AutoSize = true;
            this.chkFollowRenamesInFileHistoryExact.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFollowRenamesInFileHistoryExact.Location = new System.Drawing.Point(273, 95);
            this.chkFollowRenamesInFileHistoryExact.Name = "chkFollowRenamesInFileHistoryExact";
            this.chkFollowRenamesInFileHistoryExact.Size = new System.Drawing.Size(968, 17);
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
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(273, 170);
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
            this.RevisionGridQuickSearchTimeout.Size = new System.Drawing.Size(85, 20);
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
            this.btnDefaultDestinationBrowse.Location = new System.Drawing.Point(1247, 141);
            this.btnDefaultDestinationBrowse.Name = "btnDefaultDestinationBrowse";
            this.btnDefaultDestinationBrowse.Size = new System.Drawing.Size(52, 23);
            this.btnDefaultDestinationBrowse.TabIndex = 10;
            this.btnDefaultDestinationBrowse.Text = "Browse";
            this.btnDefaultDestinationBrowse.UseVisualStyleBackColor = true;
            this.btnDefaultDestinationBrowse.Click += new System.EventHandler(this.DefaultCloneDestinationBrowseClick);
            // 
            // lblQuickSearchTimeout
            // 
            this.lblQuickSearchTimeout.AutoSize = true;
            this.lblQuickSearchTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblQuickSearchTimeout.Location = new System.Drawing.Point(3, 167);
            this.lblQuickSearchTimeout.Name = "lblQuickSearchTimeout";
            this.lblQuickSearchTimeout.Size = new System.Drawing.Size(264, 26);
            this.lblQuickSearchTimeout.TabIndex = 11;
            this.lblQuickSearchTimeout.Text = "Revision grid quick search timeout [ms]";
            this.lblQuickSearchTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkCloseProcessDialog
            // 
            this.chkCloseProcessDialog.AutoSize = true;
            this.chkCloseProcessDialog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCloseProcessDialog.Location = new System.Drawing.Point(3, 3);
            this.chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            this.chkCloseProcessDialog.Size = new System.Drawing.Size(264, 17);
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
            this.cbDefaultCloneDestination.Location = new System.Drawing.Point(273, 141);
            this.cbDefaultCloneDestination.Name = "cbDefaultCloneDestination";
            this.cbDefaultCloneDestination.Size = new System.Drawing.Size(968, 21);
            this.cbDefaultCloneDestination.TabIndex = 9;
            // 
            // chkShowGitCommandLine
            // 
            this.chkShowGitCommandLine.AutoSize = true;
            this.chkShowGitCommandLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowGitCommandLine.Location = new System.Drawing.Point(3, 26);
            this.chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            this.chkShowGitCommandLine.Size = new System.Drawing.Size(264, 17);
            this.chkShowGitCommandLine.TabIndex = 1;
            this.chkShowGitCommandLine.Text = "Show console window when executing git process";
            this.chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // lblDefaultCloneDestination
            // 
            this.lblDefaultCloneDestination.AutoSize = true;
            this.lblDefaultCloneDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDefaultCloneDestination.Location = new System.Drawing.Point(3, 138);
            this.lblDefaultCloneDestination.Name = "lblDefaultCloneDestination";
            this.lblDefaultCloneDestination.Size = new System.Drawing.Size(264, 29);
            this.lblDefaultCloneDestination.TabIndex = 8;
            this.lblDefaultCloneDestination.Text = "Default clone destination";
            this.lblDefaultCloneDestination.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkUsePatienceDiffAlgorithm
            // 
            this.chkUsePatienceDiffAlgorithm.AutoSize = true;
            this.chkUsePatienceDiffAlgorithm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkUsePatienceDiffAlgorithm.Location = new System.Drawing.Point(3, 49);
            this.chkUsePatienceDiffAlgorithm.Name = "chkUsePatienceDiffAlgorithm";
            this.chkUsePatienceDiffAlgorithm.Size = new System.Drawing.Size(264, 17);
            this.chkUsePatienceDiffAlgorithm.TabIndex = 2;
            this.chkUsePatienceDiffAlgorithm.Text = "Use patience diff algorithm";
            this.chkUsePatienceDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkStashUntrackedFiles
            // 
            this.chkStashUntrackedFiles.AutoSize = true;
            this.chkStashUntrackedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkStashUntrackedFiles.Location = new System.Drawing.Point(3, 72);
            this.chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            this.chkStashUntrackedFiles.Size = new System.Drawing.Size(264, 17);
            this.chkStashUntrackedFiles.TabIndex = 3;
            this.chkStashUntrackedFiles.Text = "Include untracked files in stash";
            this.chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            this.chkStartWithRecentWorkingDir.AutoSize = true;
            this.chkStartWithRecentWorkingDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkStartWithRecentWorkingDir.Location = new System.Drawing.Point(3, 118);
            this.chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            this.chkStartWithRecentWorkingDir.Size = new System.Drawing.Size(264, 17);
            this.chkStartWithRecentWorkingDir.TabIndex = 6;
            this.chkStartWithRecentWorkingDir.Text = "Open last working directory on startup";
            this.chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            this.chkFollowRenamesInFileHistory.AutoSize = true;
            this.chkFollowRenamesInFileHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFollowRenamesInFileHistory.Location = new System.Drawing.Point(3, 95);
            this.chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            this.chkFollowRenamesInFileHistory.Size = new System.Drawing.Size(264, 17);
            this.chkFollowRenamesInFileHistory.TabIndex = 4;
            this.chkFollowRenamesInFileHistory.Text = "Follow renames in file history (experimental)";
            this.chkFollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // groupBoxTelemetry
            // 
            this.groupBoxTelemetry.AutoSize = true;
            this.groupBoxTelemetry.Controls.Add(this.tlpnlTelemetry);
            this.groupBoxTelemetry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTelemetry.Location = new System.Drawing.Point(3, 476);
            this.groupBoxTelemetry.Name = "groupBoxTelemetry";
            this.groupBoxTelemetry.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxTelemetry.Size = new System.Drawing.Size(1318, 52);
            this.groupBoxTelemetry.TabIndex = 3;
            this.groupBoxTelemetry.TabStop = false;
            this.groupBoxTelemetry.Text = "Telemetry";
            // 
            // tlpnlTelemetry
            // 
            this.tlpnlTelemetry.AutoSize = true;
            this.tlpnlTelemetry.ColumnCount = 2;
            this.tlpnlTelemetry.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlTelemetry.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlTelemetry.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlTelemetry.Controls.Add(this.chkTelemetry, 0, 0);
            this.tlpnlTelemetry.Controls.Add(this.llblTelemetryPrivacyLink, 1, 0);
            this.tlpnlTelemetry.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpnlTelemetry.Location = new System.Drawing.Point(8, 21);
            this.tlpnlTelemetry.Name = "tlpnlTelemetry";
            this.tlpnlTelemetry.RowCount = 1;
            this.tlpnlTelemetry.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlTelemetry.Size = new System.Drawing.Size(1302, 23);
            this.tlpnlTelemetry.TabIndex = 0;
            // 
            // chkTelemetry
            // 
            this.chkTelemetry.AutoSize = true;
            this.chkTelemetry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkTelemetry.Location = new System.Drawing.Point(3, 3);
            this.chkTelemetry.Name = "chkTelemetry";
            this.chkTelemetry.Size = new System.Drawing.Size(128, 17);
            this.chkTelemetry.TabIndex = 0;
            this.chkTelemetry.Text = "Yes, I allow telemetry!";
            this.chkTelemetry.UseVisualStyleBackColor = true;
            // 
            // llblTelemetryPrivacyLink
            // 
            this.llblTelemetryPrivacyLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.llblTelemetryPrivacyLink.Location = new System.Drawing.Point(137, 0);
            this.llblTelemetryPrivacyLink.Name = "llblTelemetryPrivacyLink";
            this.llblTelemetryPrivacyLink.Size = new System.Drawing.Size(1162, 23);
            this.llblTelemetryPrivacyLink.TabIndex = 1;
            this.llblTelemetryPrivacyLink.TabStop = true;
            this.llblTelemetryPrivacyLink.Text = "Why and what is captured?";
            this.llblTelemetryPrivacyLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.llblTelemetryPrivacyLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblTelemetryPrivacyLink_LinkClicked);
            // 
            // lblDefaultPullAction
            // 
            this.lblDefaultPullAction.AutoSize = true;
            this.lblDefaultPullAction.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDefaultPullAction.Location = new System.Drawing.Point(3, 167);
            this.lblDefaultPullAction.Name = "lblDefaultPullAction";
            this.lblDefaultPullAction.Size = new System.Drawing.Size(264, 27);
            this.lblDefaultPullAction.TabIndex = 13;
            this.lblDefaultPullAction.Text = "Default pull action";
            this.lblDefaultPullAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboDefaultPullAction
            // 
            this.cboDefaultPullAction.FormattingEnabled = true;
            this.cboDefaultPullAction.Location = new System.Drawing.Point(273, 170);
            this.cboDefaultPullAction.Name = "cboDefaultPullAction";
            this.cboDefaultPullAction.Size = new System.Drawing.Size(121, 21);
            this.cboDefaultPullAction.TabIndex = 14;
            // 
            // GeneralSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(tlpnlMain);
            this.Name = "GeneralSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1340, 889);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.groupBoxPerformance.ResumeLayout(false);
            this.groupBoxPerformance.PerformLayout();
            this.tlpnlPerformance.ResumeLayout(false);
            this.tlpnlPerformance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.groupBoxBehaviour.ResumeLayout(false);
            this.groupBoxBehaviour.PerformLayout();
            this.tlpnlBehaviour.ResumeLayout(false);
            this.tlpnlBehaviour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            this.groupBoxTelemetry.ResumeLayout(false);
            this.groupBoxTelemetry.PerformLayout();
            this.tlpnlTelemetry.ResumeLayout(false);
            this.tlpnlTelemetry.PerformLayout();
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
        private System.Windows.Forms.CheckBox chkUseFastChecks;
        private System.Windows.Forms.CheckBox chkShowStashCountInBrowseWindow;
        private System.Windows.Forms.CheckBox chkShowSubmoduleStatusInBrowse;
        private System.Windows.Forms.Label lblCommitsLimit;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_MaxCommits;
        private System.Windows.Forms.Label lblDefaultCloneDestination;
        private System.Windows.Forms.ComboBox cbDefaultCloneDestination;
        private System.Windows.Forms.Button btnDefaultDestinationBrowse;
        private System.Windows.Forms.TableLayoutPanel tlpnlPerformance;
        private System.Windows.Forms.TableLayoutPanel tlpnlBehaviour;
        private System.Windows.Forms.CheckBox chkFollowRenamesInFileHistoryExact;
        private System.Windows.Forms.CheckBox chkShowAheadBehindDataInBrowseWindow;
        private System.Windows.Forms.GroupBox groupBoxTelemetry;
        private System.Windows.Forms.TableLayoutPanel tlpnlTelemetry;
        private System.Windows.Forms.CheckBox chkTelemetry;
        private System.Windows.Forms.LinkLabel llblTelemetryPrivacyLink;
        private System.Windows.Forms.Label lblDefaultPullAction;
        private System.Windows.Forms.ComboBox cboDefaultPullAction;
    }
}
