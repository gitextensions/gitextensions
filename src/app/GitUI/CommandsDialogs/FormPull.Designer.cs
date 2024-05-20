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
            components = new System.ComponentModel.Container();
            Tooltip = new ToolTip(components);
            PullFromUrl = new RadioButton();
            PullFromRemote = new RadioButton();
            lblLocalBranch = new Label();
            lblRemoteBranch = new Label();
            Prune = new CheckBox();
            PruneTags = new CheckBox();
            Unshallow = new CheckBox();
            Pull = new Button();
            MainLayout = new TableLayoutPanel();
            PanelLeftImage = new GitUI.Help.HelpImageDisplayUserControl();
            PanelRight = new Panel();
            PanelRightInner = new TableLayoutPanel();
            GroupPullFrom = new GroupBox();
            _NO_TRANSLATE_Remotes = new ComboBox();
            AddRemote = new Button();
            comboBoxPullSource = new ComboBox();
            folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            GroupMergeOptions = new GroupBox();
            PanelMergeOptions = new FlowLayoutPanel();
            Merge = new RadioButton();
            Rebase = new RadioButton();
            Fetch = new RadioButton();
            GroupBranch = new GroupBox();
            localBranch = new TextBox();
            Branches = new ComboBox();
            GroupTagOptions = new GroupBox();
            PanelTagOptions = new FlowLayoutPanel();
            ReachableTags = new RadioButton();
            NoTags = new RadioButton();
            AllTags = new RadioButton();
            Mergetool = new Button();
            Stash = new Button();
            AutoStash = new CheckBox();
            lblSpacer = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            MainLayout.SuspendLayout();
            PanelRight.SuspendLayout();
            PanelRightInner.SuspendLayout();
            GroupPullFrom.SuspendLayout();
            GroupMergeOptions.SuspendLayout();
            PanelMergeOptions.SuspendLayout();
            GroupBranch.SuspendLayout();
            GroupTagOptions.SuspendLayout();
            PanelTagOptions.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(MainLayout);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(674, 484);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Pull);
            ControlsPanel.Controls.Add(lblSpacer);
            ControlsPanel.Controls.Add(AutoStash);
            ControlsPanel.Controls.Add(Stash);
            ControlsPanel.Controls.Add(Mergetool);
            ControlsPanel.Location = new Point(0, 484);
            ControlsPanel.Size = new Size(674, 41);
            // 
            // PullFromUrl
            // 
            PullFromUrl.AutoSize = true;
            PullFromUrl.Location = new Point(7, 47);
            PullFromUrl.Name = "PullFromUrl";
            PullFromUrl.Size = new Size(46, 19);
            PullFromUrl.TabIndex = 8;
            PullFromUrl.Text = "&URL";
            Tooltip.SetToolTip(PullFromUrl, "Url to pull from");
            PullFromUrl.UseVisualStyleBackColor = true;
            PullFromUrl.CheckedChanged += PullFromUrlCheckedChanged;
            // 
            // PullFromRemote
            // 
            PullFromRemote.AutoSize = true;
            PullFromRemote.Checked = true;
            PullFromRemote.Location = new Point(7, 19);
            PullFromRemote.Name = "PullFromRemote";
            PullFromRemote.Size = new Size(66, 19);
            PullFromRemote.TabIndex = 5;
            PullFromRemote.TabStop = true;
            PullFromRemote.Text = "&Remote";
            Tooltip.SetToolTip(PullFromRemote, "Remote repository to pull from");
            PullFromRemote.UseVisualStyleBackColor = true;
            PullFromRemote.CheckedChanged += PullFromRemoteCheckedChanged;
            // 
            // lblLocalBranch
            // 
            lblLocalBranch.AutoSize = true;
            lblLocalBranch.Location = new Point(9, 22);
            lblLocalBranch.Name = "lblLocalBranch";
            lblLocalBranch.Size = new Size(75, 15);
            lblLocalBranch.TabIndex = 11;
            lblLocalBranch.Text = "&Local branch";
            Tooltip.SetToolTip(lblLocalBranch, "Local branch to create or reset to the remote branch selected.");
            // 
            // lblRemoteBranch
            // 
            lblRemoteBranch.AutoSize = true;
            lblRemoteBranch.Location = new Point(8, 49);
            lblRemoteBranch.Name = "lblRemoteBranch";
            lblRemoteBranch.Size = new Size(88, 15);
            lblRemoteBranch.TabIndex = 13;
            lblRemoteBranch.Text = "Rem&ote branch";
            Tooltip.SetToolTip(lblRemoteBranch, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // Prune
            // 
            Prune.AutoSize = true;
            Prune.Location = new Point(7, 437);
            Prune.Margin = new Padding(7, 2, 2, 2);
            Prune.Name = "Prune";
            Prune.Size = new Size(149, 19);
            Prune.TabIndex = 26;
            Prune.Text = "&Prune remote branches";
            Tooltip.SetToolTip(Prune, "Removes remote tracking branches that no longer exist on the remote (e.g. if some" +
        "one else deleted them).\r\n\r\nActual command line (if checked): --prune --force\r\n");
            Prune.CheckedChanged += Prune_CheckedChanged;
            // 
            // PruneTags
            // 
            PruneTags.AutoSize = true;
            PruneTags.Enabled = false;
            PruneTags.Location = new Point(7, 460);
            PruneTags.Margin = new Padding(7, 2, 2, 2);
            PruneTags.Name = "PruneTags";
            PruneTags.Size = new Size(197, 19);
            PruneTags.TabIndex = 27;
            PruneTags.Text = "Prune remote branches an&d tags";
            Tooltip.SetToolTip(PruneTags, "Before fetching, remove any local tags that no longer exist on the remote if --pr" +
        "une is enabled.");
            PruneTags.CheckedChanged += PruneTags_CheckedChanged;
            // 
            // Unshallow
            // 
            Unshallow.AutoSize = true;
            Unshallow.Location = new Point(7, 414);
            Unshallow.Margin = new Padding(7, 2, 2, 2);
            Unshallow.Name = "Unshallow";
            Unshallow.Size = new Size(139, 19);
            Unshallow.TabIndex = 25;
            Unshallow.Text = "Do&wnload full history";
            Unshallow.Visible = false;
            // 
            // Pull
            // 
            Pull.Anchor = AnchorStyles.Right;
            Pull.DialogResult = DialogResult.OK;
            Pull.Image = Properties.Images.ArrowDown;
            Pull.ImageAlign = ContentAlignment.MiddleLeft;
            Pull.Location = new Point(537, 8);
            Pull.MinimumSize = new Size(120, 25);
            Pull.Name = "Pull";
            Pull.Size = new Size(124, 25);
            Pull.TabIndex = 32;
            Pull.Text = "Pull";
            Pull.UseVisualStyleBackColor = true;
            Pull.Click += PullClick;
            // 
            // MainLayout
            // 
            MainLayout.ColumnCount = 2;
            MainLayout.ColumnStyles.Add(new ColumnStyle());
            MainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            MainLayout.Controls.Add(PanelLeftImage, 0, 0);
            MainLayout.Controls.Add(PanelRight, 1, 0);
            MainLayout.Dock = DockStyle.Fill;
            MainLayout.Location = new Point(9, 9);
            MainLayout.Margin = new Padding(0);
            MainLayout.Name = "MainLayout";
            MainLayout.RowCount = 1;
            MainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            MainLayout.Size = new Size(656, 466);
            MainLayout.TabIndex = 0;
            // 
            // PanelLeftImage
            // 
            PanelLeftImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PanelLeftImage.AutoSize = true;
            PanelLeftImage.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PanelLeftImage.Image1 = null;
            PanelLeftImage.Image2 = null;
            PanelLeftImage.IsExpanded = true;
            PanelLeftImage.IsOnHoverShowImage2 = false;
            PanelLeftImage.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            PanelLeftImage.Location = new Point(3, 3);
            PanelLeftImage.MinimumSize = new Size(40, 85);
            PanelLeftImage.Name = "PanelLeftImage";
            PanelLeftImage.Size = new Size(40, 460);
            PanelLeftImage.TabIndex = 1;
            PanelLeftImage.UniqueIsExpandedSettingsId = "Pull";
            // 
            // PanelRight
            // 
            PanelRight.Controls.Add(PanelRightInner);
            PanelRight.Dock = DockStyle.Fill;
            PanelRight.Location = new Point(49, 3);
            PanelRight.Name = "PanelRight";
            PanelRight.Size = new Size(604, 460);
            PanelRight.TabIndex = 2;
            // 
            // PanelRightInner
            // 
            PanelRightInner.ColumnCount = 1;
            PanelRightInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            PanelRightInner.Controls.Add(GroupPullFrom, 0, 0);
            PanelRightInner.Controls.Add(GroupMergeOptions, 0, 2);
            PanelRightInner.Controls.Add(GroupBranch, 0, 1);
            PanelRightInner.Controls.Add(GroupTagOptions, 0, 3);
            PanelRightInner.Controls.Add(Unshallow, 0, 4);
            PanelRightInner.Controls.Add(Prune, 0, 5);
            PanelRightInner.Controls.Add(PruneTags, 0, 6);
            PanelRightInner.Dock = DockStyle.Fill;
            PanelRightInner.Location = new Point(0, 0);
            PanelRightInner.Margin = new Padding(0);
            PanelRightInner.Name = "PanelRightInner";
            PanelRightInner.RowCount = 8;
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle());
            PanelRightInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            PanelRightInner.Size = new Size(604, 460);
            PanelRightInner.TabIndex = 3;
            // 
            // GroupPullFrom
            // 
            GroupPullFrom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GroupPullFrom.AutoSize = true;
            GroupPullFrom.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GroupPullFrom.Controls.Add(PullFromRemote);
            GroupPullFrom.Controls.Add(_NO_TRANSLATE_Remotes);
            GroupPullFrom.Controls.Add(AddRemote);
            GroupPullFrom.Controls.Add(PullFromUrl);
            GroupPullFrom.Controls.Add(comboBoxPullSource);
            GroupPullFrom.Controls.Add(folderBrowserButton1);
            GroupPullFrom.Location = new Point(3, 3);
            GroupPullFrom.Name = "GroupPullFrom";
            GroupPullFrom.Size = new Size(598, 91);
            GroupPullFrom.TabIndex = 4;
            GroupPullFrom.TabStop = false;
            GroupPullFrom.Text = "Pull from";
            // 
            // _NO_TRANSLATE_Remotes
            // 
            _NO_TRANSLATE_Remotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_Remotes.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_Remotes.AutoCompleteSource = AutoCompleteSource.ListItems;
            _NO_TRANSLATE_Remotes.FormattingEnabled = true;
            _NO_TRANSLATE_Remotes.Location = new Point(148, 16);
            _NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            _NO_TRANSLATE_Remotes.Size = new Size(288, 23);
            _NO_TRANSLATE_Remotes.TabIndex = 6;
            _NO_TRANSLATE_Remotes.TextChanged += Remotes_TextChanged;
            _NO_TRANSLATE_Remotes.Validating += RemotesValidating;
            // 
            // AddRemote
            // 
            AddRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddRemote.Image = Properties.Images.Remotes;
            AddRemote.ImageAlign = ContentAlignment.MiddleLeft;
            AddRemote.Location = new Point(442, 14);
            AddRemote.Name = "AddRemote";
            AddRemote.Size = new Size(150, 25);
            AddRemote.TabIndex = 7;
            AddRemote.Text = "Mana&ge remotes";
            AddRemote.UseVisualStyleBackColor = true;
            AddRemote.Click += AddRemoteClick;
            // 
            // comboBoxPullSource
            // 
            comboBoxPullSource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxPullSource.Enabled = false;
            comboBoxPullSource.FormattingEnabled = true;
            comboBoxPullSource.Location = new Point(148, 46);
            comboBoxPullSource.Name = "comboBoxPullSource";
            comboBoxPullSource.Size = new Size(288, 23);
            comboBoxPullSource.TabIndex = 9;
            comboBoxPullSource.Validating += PullSourceValidating;
            // 
            // folderBrowserButton1
            // 
            folderBrowserButton1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            folderBrowserButton1.AutoSize = true;
            folderBrowserButton1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            folderBrowserButton1.Enabled = false;
            folderBrowserButton1.Location = new Point(542, 45);
            folderBrowserButton1.Name = "folderBrowserButton1";
            folderBrowserButton1.PathShowingControl = comboBoxPullSource;
            folderBrowserButton1.Size = new Size(0, 0);
            folderBrowserButton1.TabIndex = 5;
            // 
            // GroupMergeOptions
            // 
            GroupMergeOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GroupMergeOptions.AutoSize = true;
            GroupMergeOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GroupMergeOptions.Controls.Add(PanelMergeOptions);
            GroupMergeOptions.Location = new Point(3, 197);
            GroupMergeOptions.Name = "GroupMergeOptions";
            GroupMergeOptions.Size = new Size(598, 103);
            GroupMergeOptions.TabIndex = 15;
            GroupMergeOptions.TabStop = false;
            GroupMergeOptions.Text = "Merge options";
            // 
            // PanelMergeOptions
            // 
            PanelMergeOptions.AutoSize = true;
            PanelMergeOptions.Controls.Add(Merge);
            PanelMergeOptions.Controls.Add(Rebase);
            PanelMergeOptions.Controls.Add(Fetch);
            PanelMergeOptions.Dock = DockStyle.Fill;
            PanelMergeOptions.FlowDirection = FlowDirection.TopDown;
            PanelMergeOptions.Location = new Point(3, 19);
            PanelMergeOptions.Name = "PanelMergeOptions";
            PanelMergeOptions.Size = new Size(592, 81);
            PanelMergeOptions.TabIndex = 16;
            PanelMergeOptions.WrapContents = false;
            // 
            // Merge
            // 
            Merge.AutoSize = true;
            Merge.Image = Properties.Images.Merge;
            Merge.ImageAlign = ContentAlignment.MiddleLeft;
            Merge.Location = new Point(3, 3);
            Merge.Name = "Merge";
            Merge.Padding = new Padding(0, 1, 0, 1);
            Merge.Size = new Size(261, 21);
            Merge.TabIndex = 17;
            Merge.TabStop = true;
            Merge.Text = "&Merge remote branch into current branch";
            Merge.TextImageRelation = TextImageRelation.ImageBeforeText;
            Merge.UseVisualStyleBackColor = true;
            Merge.CheckedChanged += MergeCheckedChanged;
            // 
            // Rebase
            // 
            Rebase.AutoSize = true;
            Rebase.Image = Properties.Images.Rebase;
            Rebase.Location = new Point(3, 30);
            Rebase.Name = "Rebase";
            Rebase.Padding = new Padding(0, 1, 0, 1);
            Rebase.Size = new Size(504, 21);
            Rebase.TabIndex = 18;
            Rebase.Text = "R&ebase current branch on top of remote branch, creates linear history (use with " +
    "caution)";
            Rebase.TextImageRelation = TextImageRelation.ImageBeforeText;
            Rebase.UseVisualStyleBackColor = true;
            Rebase.CheckedChanged += RebaseCheckedChanged;
            // 
            // Fetch
            // 
            Fetch.AutoSize = true;
            Fetch.Location = new Point(3, 57);
            Fetch.Name = "Fetch";
            Fetch.Padding = new Padding(0, 1, 0, 1);
            Fetch.Size = new Size(245, 21);
            Fetch.TabIndex = 19;
            Fetch.Text = "Do not merge, only &fetch remote changes";
            Fetch.UseVisualStyleBackColor = true;
            Fetch.CheckedChanged += FetchCheckedChanged;
            // 
            // GroupBranch
            // 
            GroupBranch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GroupBranch.AutoSize = true;
            GroupBranch.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GroupBranch.Controls.Add(localBranch);
            GroupBranch.Controls.Add(lblLocalBranch);
            GroupBranch.Controls.Add(Branches);
            GroupBranch.Controls.Add(lblRemoteBranch);
            GroupBranch.Location = new Point(3, 100);
            GroupBranch.Name = "GroupBranch";
            GroupBranch.Size = new Size(598, 91);
            GroupBranch.TabIndex = 10;
            GroupBranch.TabStop = false;
            GroupBranch.Text = "Branch";
            // 
            // localBranch
            // 
            localBranch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            localBranch.Location = new Point(150, 14);
            localBranch.Name = "localBranch";
            localBranch.Size = new Size(288, 23);
            localBranch.TabIndex = 12;
            localBranch.Leave += localBranch_Leave;
            // 
            // Branches
            // 
            Branches.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Branches.FormattingEnabled = true;
            Branches.Location = new Point(150, 46);
            Branches.Name = "Branches";
            Branches.Size = new Size(288, 23);
            Branches.TabIndex = 14;
            Branches.DropDown += BranchesDropDown;
            // 
            // GroupTagOptions
            // 
            GroupTagOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GroupTagOptions.AutoSize = true;
            GroupTagOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GroupTagOptions.Controls.Add(PanelTagOptions);
            GroupTagOptions.Location = new Point(3, 306);
            GroupTagOptions.Name = "GroupTagOptions";
            GroupTagOptions.Size = new Size(598, 103);
            GroupTagOptions.TabIndex = 20;
            GroupTagOptions.TabStop = false;
            GroupTagOptions.Text = "Tag options";
            // 
            // PanelTagOptions
            // 
            PanelTagOptions.AutoSize = true;
            PanelTagOptions.Controls.Add(ReachableTags);
            PanelTagOptions.Controls.Add(NoTags);
            PanelTagOptions.Controls.Add(AllTags);
            PanelTagOptions.Dock = DockStyle.Fill;
            PanelTagOptions.FlowDirection = FlowDirection.TopDown;
            PanelTagOptions.Location = new Point(3, 19);
            PanelTagOptions.Name = "PanelTagOptions";
            PanelTagOptions.Size = new Size(592, 81);
            PanelTagOptions.TabIndex = 21;
            PanelTagOptions.WrapContents = false;
            // 
            // ReachableTags
            // 
            ReachableTags.AutoSize = true;
            ReachableTags.Checked = true;
            ReachableTags.Location = new Point(3, 3);
            ReachableTags.Name = "ReachableTags";
            ReachableTags.Padding = new Padding(0, 1, 0, 1);
            ReachableTags.Size = new Size(398, 21);
            ReachableTags.TabIndex = 22;
            ReachableTags.TabStop = true;
            ReachableTags.Text = "Follow &tagopt, if not specified, fetch tags reachable from remote HEAD";
            ReachableTags.UseVisualStyleBackColor = true;
            // 
            // NoTags
            // 
            NoTags.AutoSize = true;
            NoTags.Location = new Point(3, 30);
            NoTags.Name = "NoTags";
            NoTags.Padding = new Padding(0, 1, 0, 1);
            NoTags.Size = new Size(91, 21);
            NoTags.TabIndex = 23;
            NoTags.Text = "Fetch &no tag";
            NoTags.UseVisualStyleBackColor = true;
            // 
            // AllTags
            // 
            AllTags.AutoSize = true;
            AllTags.Location = new Point(3, 57);
            AllTags.Name = "AllTags";
            AllTags.Padding = new Padding(0, 1, 0, 1);
            AllTags.Size = new Size(94, 21);
            AllTags.TabIndex = 24;
            AllTags.Text = "Fetch &all tags";
            AllTags.UseVisualStyleBackColor = true;
            // 
            // Mergetool
            // 
            Mergetool.Anchor = AnchorStyles.Left;
            Mergetool.Location = new Point(58, 8);
            Mergetool.Name = "Mergetool";
            Mergetool.Size = new Size(141, 25);
            Mergetool.TabIndex = 29;
            Mergetool.Text = "&Solve conflicts";
            Mergetool.UseVisualStyleBackColor = true;
            Mergetool.Click += MergetoolClick;
            // 
            // Stash
            // 
            Stash.Anchor = AnchorStyles.Left;
            Stash.Location = new Point(205, 8);
            Stash.Name = "Stash";
            Stash.Size = new Size(132, 25);
            Stash.TabIndex = 30;
            Stash.Text = "Stash &changes";
            Stash.UseVisualStyleBackColor = true;
            Stash.Click += StashClick;
            // 
            // AutoStash
            // 
            AutoStash.Anchor = AnchorStyles.Left;
            AutoStash.AutoSize = true;
            AutoStash.Location = new Point(343, 11);
            AutoStash.Name = "AutoStash";
            AutoStash.Size = new Size(82, 19);
            AutoStash.TabIndex = 31;
            AutoStash.Text = "Auto stas&h";
            AutoStash.UseVisualStyleBackColor = true;
            // 
            // lblSpacer
            // 
            lblSpacer.Location = new Point(431, 5);
            lblSpacer.Name = "lblSpacer";
            lblSpacer.Size = new Size(100, 23);
            lblSpacer.TabIndex = 33;
            // 
            // FormPull
            // 
            AcceptButton = Pull;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(674, 525);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(690, 564);
            Name = "FormPull";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Pull";
            Load += FormPullLoad;
            MainPanel.ResumeLayout(false);
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            MainLayout.ResumeLayout(false);
            MainLayout.PerformLayout();
            PanelRight.ResumeLayout(false);
            PanelRightInner.ResumeLayout(false);
            PanelRightInner.PerformLayout();
            GroupPullFrom.ResumeLayout(false);
            GroupPullFrom.PerformLayout();
            GroupMergeOptions.ResumeLayout(false);
            GroupMergeOptions.PerformLayout();
            PanelMergeOptions.ResumeLayout(false);
            PanelMergeOptions.PerformLayout();
            GroupBranch.ResumeLayout(false);
            GroupBranch.PerformLayout();
            GroupTagOptions.ResumeLayout(false);
            GroupTagOptions.PerformLayout();
            PanelTagOptions.ResumeLayout(false);
            PanelTagOptions.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ComboBox Branches;
        private Label lblRemoteBranch;
        private Button Pull;
        private Button Mergetool;
        private RadioButton Merge;
        private RadioButton Rebase;
        private GroupBox GroupMergeOptions;
        private RadioButton Fetch;
        private ComboBox comboBoxPullSource;
        private Button Stash;
        private ComboBox _NO_TRANSLATE_Remotes;
        private GroupBox GroupPullFrom;
        private RadioButton PullFromUrl;
        private RadioButton PullFromRemote;
        private GroupBox GroupBranch;
        private CheckBox AutoStash;
        private ToolTip Tooltip;
        private Label lblLocalBranch;
        private TextBox localBranch;
        private TableLayoutPanel MainLayout;
        private Panel PanelRight;
        private TableLayoutPanel PanelRightInner;
        private FlowLayoutPanel PanelMergeOptions;
        private Help.HelpImageDisplayUserControl PanelLeftImage;
        private FlowLayoutPanel PanelTagOptions;
        private RadioButton ReachableTags;
        private RadioButton NoTags;
        private RadioButton AllTags;
        private GroupBox GroupTagOptions;
        private Button AddRemote;
        private UserControls.FolderBrowserButton folderBrowserButton1;
        private CheckBox Unshallow;
        private CheckBox Prune;
        private CheckBox PruneTags;
        private Label lblSpacer;
    }
}
