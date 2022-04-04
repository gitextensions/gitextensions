namespace GitUI.CommandsDialogs
{
    partial class FormPull
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.lblLocalBranch = new System.Windows.Forms.Label();
            this.lblRemoteBranch = new System.Windows.Forms.Label();
            this.Prune = new System.Windows.Forms.CheckBox();
            this.PruneTags = new System.Windows.Forms.CheckBox();
            this.Unshallow = new System.Windows.Forms.CheckBox();
            this.Pull = new System.Windows.Forms.Button();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.PanelLeftImage = new GitUI.Help.HelpImageDisplayUserControl();
            this.PanelRight = new System.Windows.Forms.Panel();
            this.PanelRightInner = new System.Windows.Forms.TableLayoutPanel();
            this.GroupPullFrom = new System.Windows.Forms.GroupBox();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this.AddRemote = new System.Windows.Forms.Button();
            this.comboBoxPullSource = new System.Windows.Forms.ComboBox();
            this.folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            this.GroupMergeOptions = new System.Windows.Forms.GroupBox();
            this.PanelMergeOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.Merge = new System.Windows.Forms.RadioButton();
            this.Rebase = new System.Windows.Forms.RadioButton();
            this.Fetch = new System.Windows.Forms.RadioButton();
            this.GroupBranch = new System.Windows.Forms.GroupBox();
            this.localBranch = new System.Windows.Forms.TextBox();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.GroupTagOptions = new System.Windows.Forms.GroupBox();
            this.PanelTagOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.ReachableTags = new System.Windows.Forms.RadioButton();
            this.NoTags = new System.Windows.Forms.RadioButton();
            this.AllTags = new System.Windows.Forms.RadioButton();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Stash = new System.Windows.Forms.Button();
            this.AutoStash = new System.Windows.Forms.CheckBox();
            this.lblSpacer = new System.Windows.Forms.Label();
            this.MainPanel.SuspendLayout();
            this.ControlsPanel.SuspendLayout();
            this.MainLayout.SuspendLayout();
            this.PanelRight.SuspendLayout();
            this.PanelRightInner.SuspendLayout();
            this.GroupPullFrom.SuspendLayout();
            this.GroupMergeOptions.SuspendLayout();
            this.PanelMergeOptions.SuspendLayout();
            this.GroupBranch.SuspendLayout();
            this.GroupTagOptions.SuspendLayout();
            this.PanelTagOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.MainLayout);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(9);
            this.MainPanel.Size = new System.Drawing.Size(674, 484);
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Controls.Add(this.Pull);
            this.ControlsPanel.Controls.Add(this.lblSpacer);
            this.ControlsPanel.Controls.Add(this.AutoStash);
            this.ControlsPanel.Controls.Add(this.Stash);
            this.ControlsPanel.Controls.Add(this.Mergetool);
            this.ControlsPanel.Location = new System.Drawing.Point(0, 484);
            this.ControlsPanel.Size = new System.Drawing.Size(674, 41);
            // 
            // PullFromUrl
            // 
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 47);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(46, 19);
            this.PullFromUrl.TabIndex = 8;
            this.PullFromUrl.Text = "&URL";
            this.Tooltip.SetToolTip(this.PullFromUrl, "Url to pull from");
            this.PullFromUrl.UseVisualStyleBackColor = true;
            this.PullFromUrl.CheckedChanged += new System.EventHandler(this.PullFromUrlCheckedChanged);
            // 
            // PullFromRemote
            // 
            this.PullFromRemote.AutoSize = true;
            this.PullFromRemote.Checked = true;
            this.PullFromRemote.Location = new System.Drawing.Point(7, 19);
            this.PullFromRemote.Name = "PullFromRemote";
            this.PullFromRemote.Size = new System.Drawing.Size(66, 19);
            this.PullFromRemote.TabIndex = 5;
            this.PullFromRemote.TabStop = true;
            this.PullFromRemote.Text = "&Remote";
            this.Tooltip.SetToolTip(this.PullFromRemote, "Remote repository to pull from");
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemoteCheckedChanged);
            // 
            // lblLocalBranch
            // 
            this.lblLocalBranch.AutoSize = true;
            this.lblLocalBranch.Location = new System.Drawing.Point(9, 22);
            this.lblLocalBranch.Name = "lblLocalBranch";
            this.lblLocalBranch.Size = new System.Drawing.Size(75, 15);
            this.lblLocalBranch.TabIndex = 11;
            this.lblLocalBranch.Text = "&Local branch";
            this.Tooltip.SetToolTip(this.lblLocalBranch, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // lblRemoteBranch
            // 
            this.lblRemoteBranch.AutoSize = true;
            this.lblRemoteBranch.Location = new System.Drawing.Point(8, 49);
            this.lblRemoteBranch.Name = "lblRemoteBranch";
            this.lblRemoteBranch.Size = new System.Drawing.Size(88, 15);
            this.lblRemoteBranch.TabIndex = 13;
            this.lblRemoteBranch.Text = "Rem&ote branch";
            this.Tooltip.SetToolTip(this.lblRemoteBranch, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // Prune
            // 
            this.Prune.AutoSize = true;
            this.Prune.Location = new System.Drawing.Point(7, 437);
            this.Prune.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
            this.Prune.Name = "Prune";
            this.Prune.Size = new System.Drawing.Size(149, 19);
            this.Prune.TabIndex = 26;
            this.Prune.Text = "&Prune remote branches";
            this.Tooltip.SetToolTip(this.Prune, "Removes remote tracking branches that no longer exist on the remote (e.g. if some" +
        "one else deleted them).\r\n\r\nActual command line (if checked): --prune --force\r\n");
            this.Prune.CheckedChanged += new System.EventHandler(this.Prune_CheckedChanged);
            // 
            // PruneTags
            // 
            this.PruneTags.AutoSize = true;
            this.PruneTags.Enabled = false;
            this.PruneTags.Location = new System.Drawing.Point(7, 460);
            this.PruneTags.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
            this.PruneTags.Name = "PruneTags";
            this.PruneTags.Size = new System.Drawing.Size(197, 19);
            this.PruneTags.TabIndex = 27;
            this.PruneTags.Text = "Prune remote branches an&d tags";
            this.Tooltip.SetToolTip(this.PruneTags, "Before fetching, remove any local tags that no longer exist on the remote if --pr" +
        "une is enabled.");
            this.PruneTags.CheckedChanged += new System.EventHandler(this.PruneTags_CheckedChanged);
            // 
            // Unshallow
            // 
            this.Unshallow.AutoSize = true;
            this.Unshallow.Location = new System.Drawing.Point(7, 414);
            this.Unshallow.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
            this.Unshallow.Name = "Unshallow";
            this.Unshallow.Size = new System.Drawing.Size(139, 19);
            this.Unshallow.TabIndex = 25;
            this.Unshallow.Text = "Do&wnload full history";
            this.Unshallow.Visible = false;
            // 
            // Pull
            // 
            this.Pull.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Pull.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Pull.Image = global::GitUI.Properties.Images.ArrowDown;
            this.Pull.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Pull.Location = new System.Drawing.Point(537, 8);
            this.Pull.MinimumSize = new System.Drawing.Size(120, 25);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(124, 25);
            this.Pull.TabIndex = 32;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.PullClick);
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Controls.Add(this.PanelLeftImage, 0, 0);
            this.MainLayout.Controls.Add(this.PanelRight, 1, 0);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(9, 9);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 1;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Size = new System.Drawing.Size(656, 466);
            this.MainLayout.TabIndex = 0;
            // 
            // PanelLeftImage
            // 
            this.PanelLeftImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelLeftImage.AutoSize = true;
            this.PanelLeftImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelLeftImage.Image1 = null;
            this.PanelLeftImage.Image2 = null;
            this.PanelLeftImage.IsExpanded = true;
            this.PanelLeftImage.IsOnHoverShowImage2 = false;
            this.PanelLeftImage.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.PanelLeftImage.Location = new System.Drawing.Point(3, 3);
            this.PanelLeftImage.MinimumSize = new System.Drawing.Size(40, 85);
            this.PanelLeftImage.Name = "PanelLeftImage";
            this.PanelLeftImage.Size = new System.Drawing.Size(40, 460);
            this.PanelLeftImage.TabIndex = 1;
            this.PanelLeftImage.UniqueIsExpandedSettingsId = "Pull";
            // 
            // PanelRight
            // 
            this.PanelRight.Controls.Add(this.PanelRightInner);
            this.PanelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRight.Location = new System.Drawing.Point(49, 3);
            this.PanelRight.Name = "PanelRight";
            this.PanelRight.Size = new System.Drawing.Size(604, 460);
            this.PanelRight.TabIndex = 2;
            // 
            // PanelRightInner
            // 
            this.PanelRightInner.ColumnCount = 1;
            this.PanelRightInner.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PanelRightInner.Controls.Add(this.GroupPullFrom, 0, 0);
            this.PanelRightInner.Controls.Add(this.GroupMergeOptions, 0, 2);
            this.PanelRightInner.Controls.Add(this.GroupBranch, 0, 1);
            this.PanelRightInner.Controls.Add(this.GroupTagOptions, 0, 3);
            this.PanelRightInner.Controls.Add(this.Unshallow, 0, 4);
            this.PanelRightInner.Controls.Add(this.Prune, 0, 5);
            this.PanelRightInner.Controls.Add(this.PruneTags, 0, 6);
            this.PanelRightInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRightInner.Location = new System.Drawing.Point(0, 0);
            this.PanelRightInner.Margin = new System.Windows.Forms.Padding(0);
            this.PanelRightInner.Name = "PanelRightInner";
            this.PanelRightInner.RowCount = 8;
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelRightInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.PanelRightInner.Size = new System.Drawing.Size(604, 460);
            this.PanelRightInner.TabIndex = 3;
            // 
            // GroupPullFrom
            // 
            this.GroupPullFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupPullFrom.AutoSize = true;
            this.GroupPullFrom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GroupPullFrom.Controls.Add(this.PullFromRemote);
            this.GroupPullFrom.Controls.Add(this._NO_TRANSLATE_Remotes);
            this.GroupPullFrom.Controls.Add(this.AddRemote);
            this.GroupPullFrom.Controls.Add(this.PullFromUrl);
            this.GroupPullFrom.Controls.Add(this.comboBoxPullSource);
            this.GroupPullFrom.Controls.Add(this.folderBrowserButton1);
            this.GroupPullFrom.Location = new System.Drawing.Point(3, 3);
            this.GroupPullFrom.Name = "GroupPullFrom";
            this.GroupPullFrom.Size = new System.Drawing.Size(598, 91);
            this.GroupPullFrom.TabIndex = 4;
            this.GroupPullFrom.TabStop = false;
            this.GroupPullFrom.Text = "Pull from";
            // 
            // _NO_TRANSLATE_Remotes
            // 
            this._NO_TRANSLATE_Remotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Remotes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Remotes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_Remotes.FormattingEnabled = true;
            this._NO_TRANSLATE_Remotes.Location = new System.Drawing.Point(148, 16);
            this._NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            this._NO_TRANSLATE_Remotes.Size = new System.Drawing.Size(288, 23);
            this._NO_TRANSLATE_Remotes.TabIndex = 6;
            this._NO_TRANSLATE_Remotes.TextChanged += new System.EventHandler(this.Remotes_TextChanged);
            this._NO_TRANSLATE_Remotes.Validating += new System.ComponentModel.CancelEventHandler(this.RemotesValidating);
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Image = global::GitUI.Properties.Images.Remotes;
            this.AddRemote.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddRemote.Location = new System.Drawing.Point(442, 14);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(150, 25);
            this.AddRemote.TabIndex = 7;
            this.AddRemote.Text = "Mana&ge remotes";
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemoteClick);
            // 
            // comboBoxPullSource
            // 
            this.comboBoxPullSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPullSource.Enabled = false;
            this.comboBoxPullSource.FormattingEnabled = true;
            this.comboBoxPullSource.Location = new System.Drawing.Point(148, 46);
            this.comboBoxPullSource.Name = "comboBoxPullSource";
            this.comboBoxPullSource.Size = new System.Drawing.Size(288, 23);
            this.comboBoxPullSource.TabIndex = 9;
            this.comboBoxPullSource.Validating += new System.ComponentModel.CancelEventHandler(this.PullSourceValidating);
            // 
            // folderBrowserButton1
            // 
            this.folderBrowserButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButton1.AutoSize = true;
            this.folderBrowserButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.folderBrowserButton1.Enabled = false;
            this.folderBrowserButton1.Location = new System.Drawing.Point(542, 45);
            this.folderBrowserButton1.Name = "folderBrowserButton1";
            this.folderBrowserButton1.PathShowingControl = this.comboBoxPullSource;
            this.folderBrowserButton1.Size = new System.Drawing.Size(0, 0);
            this.folderBrowserButton1.TabIndex = 5;
            // 
            // GroupMergeOptions
            // 
            this.GroupMergeOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupMergeOptions.AutoSize = true;
            this.GroupMergeOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GroupMergeOptions.Controls.Add(this.PanelMergeOptions);
            this.GroupMergeOptions.Location = new System.Drawing.Point(3, 197);
            this.GroupMergeOptions.Name = "GroupMergeOptions";
            this.GroupMergeOptions.Size = new System.Drawing.Size(598, 103);
            this.GroupMergeOptions.TabIndex = 15;
            this.GroupMergeOptions.TabStop = false;
            this.GroupMergeOptions.Text = "Merge options";
            // 
            // PanelMergeOptions
            // 
            this.PanelMergeOptions.AutoSize = true;
            this.PanelMergeOptions.Controls.Add(this.Merge);
            this.PanelMergeOptions.Controls.Add(this.Rebase);
            this.PanelMergeOptions.Controls.Add(this.Fetch);
            this.PanelMergeOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelMergeOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelMergeOptions.Location = new System.Drawing.Point(3, 19);
            this.PanelMergeOptions.Name = "PanelMergeOptions";
            this.PanelMergeOptions.Size = new System.Drawing.Size(592, 81);
            this.PanelMergeOptions.TabIndex = 16;
            this.PanelMergeOptions.WrapContents = false;
            // 
            // Merge
            // 
            this.Merge.AutoSize = true;
            this.Merge.Image = global::GitUI.Properties.Images.Merge;
            this.Merge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Merge.Location = new System.Drawing.Point(3, 3);
            this.Merge.Name = "Merge";
            this.Merge.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Merge.Size = new System.Drawing.Size(261, 21);
            this.Merge.TabIndex = 17;
            this.Merge.TabStop = true;
            this.Merge.Text = "&Merge remote branch into current branch";
            this.Merge.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Merge.UseVisualStyleBackColor = true;
            this.Merge.CheckedChanged += new System.EventHandler(this.MergeCheckedChanged);
            // 
            // Rebase
            // 
            this.Rebase.AutoSize = true;
            this.Rebase.Image = global::GitUI.Properties.Images.Rebase;
            this.Rebase.Location = new System.Drawing.Point(3, 30);
            this.Rebase.Name = "Rebase";
            this.Rebase.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Rebase.Size = new System.Drawing.Size(504, 21);
            this.Rebase.TabIndex = 18;
            this.Rebase.Text = "R&ebase current branch on top of remote branch, creates linear history (use with " +
    "caution)";
            this.Rebase.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Rebase.UseVisualStyleBackColor = true;
            this.Rebase.CheckedChanged += new System.EventHandler(this.RebaseCheckedChanged);
            // 
            // Fetch
            // 
            this.Fetch.AutoSize = true;
            this.Fetch.Location = new System.Drawing.Point(3, 57);
            this.Fetch.Name = "Fetch";
            this.Fetch.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Fetch.Size = new System.Drawing.Size(245, 21);
            this.Fetch.TabIndex = 19;
            this.Fetch.Text = "Do not merge, only &fetch remote changes";
            this.Fetch.UseVisualStyleBackColor = true;
            this.Fetch.CheckedChanged += new System.EventHandler(this.FetchCheckedChanged);
            // 
            // GroupBranch
            // 
            this.GroupBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBranch.AutoSize = true;
            this.GroupBranch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GroupBranch.Controls.Add(this.localBranch);
            this.GroupBranch.Controls.Add(this.lblLocalBranch);
            this.GroupBranch.Controls.Add(this.Branches);
            this.GroupBranch.Controls.Add(this.lblRemoteBranch);
            this.GroupBranch.Location = new System.Drawing.Point(3, 100);
            this.GroupBranch.Name = "GroupBranch";
            this.GroupBranch.Size = new System.Drawing.Size(598, 91);
            this.GroupBranch.TabIndex = 10;
            this.GroupBranch.TabStop = false;
            this.GroupBranch.Text = "Branch";
            // 
            // localBranch
            // 
            this.localBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.localBranch.Location = new System.Drawing.Point(150, 14);
            this.localBranch.Name = "localBranch";
            this.localBranch.Size = new System.Drawing.Size(288, 23);
            this.localBranch.TabIndex = 12;
            this.localBranch.Leave += new System.EventHandler(this.localBranch_Leave);
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(150, 46);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(288, 23);
            this.Branches.TabIndex = 14;
            this.Branches.DropDown += new System.EventHandler(this.BranchesDropDown);
            // 
            // GroupTagOptions
            // 
            this.GroupTagOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupTagOptions.AutoSize = true;
            this.GroupTagOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GroupTagOptions.Controls.Add(this.PanelTagOptions);
            this.GroupTagOptions.Location = new System.Drawing.Point(3, 306);
            this.GroupTagOptions.Name = "GroupTagOptions";
            this.GroupTagOptions.Size = new System.Drawing.Size(598, 103);
            this.GroupTagOptions.TabIndex = 20;
            this.GroupTagOptions.TabStop = false;
            this.GroupTagOptions.Text = "Tag options";
            // 
            // PanelTagOptions
            // 
            this.PanelTagOptions.AutoSize = true;
            this.PanelTagOptions.Controls.Add(this.ReachableTags);
            this.PanelTagOptions.Controls.Add(this.NoTags);
            this.PanelTagOptions.Controls.Add(this.AllTags);
            this.PanelTagOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTagOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelTagOptions.Location = new System.Drawing.Point(3, 19);
            this.PanelTagOptions.Name = "PanelTagOptions";
            this.PanelTagOptions.Size = new System.Drawing.Size(592, 81);
            this.PanelTagOptions.TabIndex = 21;
            this.PanelTagOptions.WrapContents = false;
            // 
            // ReachableTags
            // 
            this.ReachableTags.AutoSize = true;
            this.ReachableTags.Checked = true;
            this.ReachableTags.Location = new System.Drawing.Point(3, 3);
            this.ReachableTags.Name = "ReachableTags";
            this.ReachableTags.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.ReachableTags.Size = new System.Drawing.Size(398, 21);
            this.ReachableTags.TabIndex = 22;
            this.ReachableTags.TabStop = true;
            this.ReachableTags.Text = "Follow &tagopt, if not specified, fetch tags reachable from remote HEAD";
            this.ReachableTags.UseVisualStyleBackColor = true;
            // 
            // NoTags
            // 
            this.NoTags.AutoSize = true;
            this.NoTags.Location = new System.Drawing.Point(3, 30);
            this.NoTags.Name = "NoTags";
            this.NoTags.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.NoTags.Size = new System.Drawing.Size(91, 21);
            this.NoTags.TabIndex = 23;
            this.NoTags.Text = "Fetch &no tag";
            this.NoTags.UseVisualStyleBackColor = true;
            // 
            // AllTags
            // 
            this.AllTags.AutoSize = true;
            this.AllTags.Location = new System.Drawing.Point(3, 57);
            this.AllTags.Name = "AllTags";
            this.AllTags.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.AllTags.Size = new System.Drawing.Size(94, 21);
            this.AllTags.TabIndex = 24;
            this.AllTags.Text = "Fetch &all tags";
            this.AllTags.UseVisualStyleBackColor = true;
            // 
            // Mergetool
            // 
            this.Mergetool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Mergetool.Location = new System.Drawing.Point(58, 8);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(141, 25);
            this.Mergetool.TabIndex = 29;
            this.Mergetool.Text = "&Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // Stash
            // 
            this.Stash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Stash.Location = new System.Drawing.Point(205, 8);
            this.Stash.Name = "Stash";
            this.Stash.Size = new System.Drawing.Size(132, 25);
            this.Stash.TabIndex = 30;
            this.Stash.Text = "Stash &changes";
            this.Stash.UseVisualStyleBackColor = true;
            this.Stash.Click += new System.EventHandler(this.StashClick);
            // 
            // AutoStash
            // 
            this.AutoStash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AutoStash.AutoSize = true;
            this.AutoStash.Location = new System.Drawing.Point(343, 11);
            this.AutoStash.Name = "AutoStash";
            this.AutoStash.Size = new System.Drawing.Size(82, 19);
            this.AutoStash.TabIndex = 31;
            this.AutoStash.Text = "Auto stas&h";
            this.AutoStash.UseVisualStyleBackColor = true;
            // 
            // lblSpacer
            // 
            this.lblSpacer.Location = new System.Drawing.Point(431, 5);
            this.lblSpacer.Name = "lblSpacer";
            this.lblSpacer.Size = new System.Drawing.Size(100, 23);
            this.lblSpacer.TabIndex = 33;
            // 
            // FormPull
            // 
            this.AcceptButton = this.Pull;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(674, 525);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(690, 564);
            this.Name = "FormPull";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pull";
            this.Load += new System.EventHandler(this.FormPullLoad);
            this.MainPanel.ResumeLayout(false);
            this.ControlsPanel.ResumeLayout(false);
            this.ControlsPanel.PerformLayout();
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.PanelRight.ResumeLayout(false);
            this.PanelRightInner.ResumeLayout(false);
            this.PanelRightInner.PerformLayout();
            this.GroupPullFrom.ResumeLayout(false);
            this.GroupPullFrom.PerformLayout();
            this.GroupMergeOptions.ResumeLayout(false);
            this.GroupMergeOptions.PerformLayout();
            this.PanelMergeOptions.ResumeLayout(false);
            this.PanelMergeOptions.PerformLayout();
            this.GroupBranch.ResumeLayout(false);
            this.GroupBranch.PerformLayout();
            this.GroupTagOptions.ResumeLayout(false);
            this.GroupTagOptions.PerformLayout();
            this.PanelTagOptions.ResumeLayout(false);
            this.PanelTagOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label lblRemoteBranch;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.RadioButton Merge;
        private System.Windows.Forms.RadioButton Rebase;
        private System.Windows.Forms.GroupBox GroupMergeOptions;
        private System.Windows.Forms.RadioButton Fetch;
        private System.Windows.Forms.ComboBox comboBoxPullSource;
        private System.Windows.Forms.Button Stash;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Remotes;
        private System.Windows.Forms.GroupBox GroupPullFrom;
        private System.Windows.Forms.RadioButton PullFromUrl;
        private System.Windows.Forms.RadioButton PullFromRemote;
        private System.Windows.Forms.GroupBox GroupBranch;
        private System.Windows.Forms.CheckBox AutoStash;
        private System.Windows.Forms.ToolTip Tooltip;
        private System.Windows.Forms.Label lblLocalBranch;
        private System.Windows.Forms.TextBox localBranch;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.Panel PanelRight;
        private System.Windows.Forms.TableLayoutPanel PanelRightInner;
        private System.Windows.Forms.FlowLayoutPanel PanelMergeOptions;
        private Help.HelpImageDisplayUserControl PanelLeftImage;
        private System.Windows.Forms.FlowLayoutPanel PanelTagOptions;
        private System.Windows.Forms.RadioButton ReachableTags;
        private System.Windows.Forms.RadioButton NoTags;
        private System.Windows.Forms.RadioButton AllTags;
        private System.Windows.Forms.GroupBox GroupTagOptions;
        private System.Windows.Forms.Button AddRemote;
        private UserControls.FolderBrowserButton folderBrowserButton1;
        private System.Windows.Forms.CheckBox Unshallow;
        private System.Windows.Forms.CheckBox Prune;
        private System.Windows.Forms.CheckBox PruneTags;
        private System.Windows.Forms.Label lblSpacer;
    }
}
