namespace GitUI.CommandsDialogs
{
    partial class FormPush
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ForcePushOptionPanel = new FlowLayoutPanel();
            ckForceWithLease = new CheckBox();
            ForcePushBranches = new CheckBox();
            toolTip1 = new ToolTip(components);
            PushToUrl = new RadioButton();
            PushToRemote = new RadioButton();
            Push = new Button();
            TabControlTagBranch = new TabControl();
            BranchTab = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            _NO_TRANSLATE_Branch = new ComboBox();
            labelTo = new Label();
            RemoteBranch = new ComboBox();
            ShowOptions = new LinkLabel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label2 = new Label();
            RecursiveSubmodules = new ComboBox();
            ReplaceTrackingReference = new CheckBox();
            _createPullRequestCB = new CheckBox();
            labelFrom = new Label();
            TagTab = new TabPage();
            ForcePushTags = new CheckBox();
            label1 = new Label();
            TagComboBox = new ComboBox();
            MultipleBranchTab = new TabPage();
            BranchGrid = new DataGridView();
            LocalColumn = new DataGridViewTextBoxColumn();
            RemoteColumn = new DataGridViewTextBoxColumn();
            NewColumn = new DataGridViewTextBoxColumn();
            PushColumn = new DataGridViewCheckBoxColumn();
            ForceColumn = new DataGridViewCheckBoxColumn();
            DeleteColumn = new DataGridViewCheckBoxColumn();
            LoadSSHKey = new Button();
            groupBox2 = new GroupBox();
            _NO_TRANSLATE_Remotes = new ComboBox();
            AddRemote = new Button();
            PushDestination = new ComboBox();
            folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            Pull = new Button();
            PushOptionsPanel = new Panel();
            menuPushSelection = new ContextMenuStrip(components);
            unselectAllToolStripMenuItem = new ToolStripMenuItem();
            selectTrackedToolStripMenuItem = new ToolStripMenuItem();
            selectAllToolStripMenuItem = new ToolStripMenuItem();
            ForcePushOptionPanel.SuspendLayout();
            TabControlTagBranch.SuspendLayout();
            BranchTab.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            PushOptionsPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            TagTab.SuspendLayout();
            MultipleBranchTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(BranchGrid)).BeginInit();
            groupBox2.SuspendLayout();
            menuPushSelection.SuspendLayout();
            SuspendLayout();
            // 
            // ForcePushOptionPanel
            // 
            ForcePushOptionPanel.AutoSize = true;
            ForcePushOptionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ForcePushOptionPanel.Controls.Add(ckForceWithLease);
            ForcePushOptionPanel.Controls.Add(ForcePushBranches);
            ForcePushOptionPanel.Location = new Point(1, 3);
            ForcePushOptionPanel.Margin = new Padding(0);
            ForcePushOptionPanel.Name = "ForcePushOptionPanel";
            ForcePushOptionPanel.Size = new Size(194, 23);
            ForcePushOptionPanel.TabIndex = 0;
            // 
            // ckForceWithLease
            // 
            ckForceWithLease.AutoSize = true;
            ckForceWithLease.Location = new Point(0, 3);
            ckForceWithLease.Margin = new Padding(0, 3, 0, 3);
            ckForceWithLease.Name = "ckForceWithLease";
            ckForceWithLease.Size = new Size(109, 17);
            ckForceWithLease.TabIndex = 0;
            ckForceWithLease.Text = "&Force with lease";
            ckForceWithLease.UseVisualStyleBackColor = true;
            ckForceWithLease.CheckedChanged += ForceWithLeaseCheckedChanged;
            // 
            // ForcePushBranches
            // 
            ForcePushBranches.AutoSize = true;
            ForcePushBranches.Location = new Point(112, 3);
            ForcePushBranches.Name = "ForcePushBranches";
            ForcePushBranches.Size = new Size(79, 17);
            ForcePushBranches.TabIndex = 1;
            ForcePushBranches.Text = "F&orce push";
            ForcePushBranches.UseVisualStyleBackColor = true;
            ForcePushBranches.CheckedChanged += ForcePushBranchesCheckedChanged;
            // 
            // PushToUrl
            // 
            PushToUrl.AutoSize = true;
            PushToUrl.Location = new Point(14, 49);
            PushToUrl.Name = "PushToUrl";
            PushToUrl.Size = new Size(38, 17);
            PushToUrl.TabIndex = 3;
            PushToUrl.Text = "U&rl";
            toolTip1.SetToolTip(PushToUrl, "Url to push to");
            PushToUrl.UseVisualStyleBackColor = true;
            PushToUrl.CheckedChanged += PushToUrlCheckedChanged;
            // 
            // PushToRemote
            // 
            PushToRemote.AutoSize = true;
            PushToRemote.Checked = true;
            PushToRemote.Location = new Point(14, 19);
            PushToRemote.Name = "PushToRemote";
            PushToRemote.Size = new Size(62, 17);
            PushToRemote.TabIndex = 0;
            PushToRemote.TabStop = true;
            PushToRemote.Text = "&Remote";
            toolTip1.SetToolTip(PushToRemote, "Remote repository to push to");
            PushToRemote.UseVisualStyleBackColor = true;
            // 
            // Push
            // 
            Push.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Push.DialogResult = DialogResult.OK;
            Push.Image = Properties.Images.ArrowUp;
            Push.ImageAlign = ContentAlignment.MiddleLeft;
            Push.Location = new Point(470, 253);
            Push.Name = "Push";
            Push.Size = new Size(101, 25);
            Push.TabIndex = 4;
            Push.UseVisualStyleBackColor = true;
            Push.Click += PushClick;
            // 
            // TabControlTagBranch
            // 
            TabControlTagBranch.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TabControlTagBranch.Controls.Add(BranchTab);
            TabControlTagBranch.Controls.Add(TagTab);
            TabControlTagBranch.Controls.Add(MultipleBranchTab);
            TabControlTagBranch.Location = new Point(12, 98);
            TabControlTagBranch.Name = "TabControlTagBranch";
            TabControlTagBranch.SelectedIndex = 0;
            TabControlTagBranch.ShowToolTips = true;
            TabControlTagBranch.Size = new Size(560, 149);
            TabControlTagBranch.TabIndex = 1;
            TabControlTagBranch.Selected += TabControlTagBranch_Selected;
            // 
            // BranchTab
            // 
            BranchTab.Controls.Add(tableLayoutPanel1);
            BranchTab.Controls.Add(ShowOptions);
            BranchTab.Controls.Add(PushOptionsPanel);
            BranchTab.Controls.Add(labelFrom);
            BranchTab.Location = new Point(4, 22);
            BranchTab.Name = "BranchTab";
            BranchTab.Padding = new Padding(3);
            BranchTab.Size = new Size(552, 123);
            BranchTab.TabIndex = 0;
            BranchTab.Text = "Push branches";
            BranchTab.ToolTipText = "Push branches and commits to remote repository.";
            BranchTab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_Branch, 0, 0);
            tableLayoutPanel1.Controls.Add(labelTo, 1, 0);
            tableLayoutPanel1.Controls.Add(RemoteBranch, 2, 0);
            tableLayoutPanel1.Location = new Point(124, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(417, 22);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // _NO_TRANSLATE_Branch
            // 
            _NO_TRANSLATE_Branch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_Branch.AutoCompleteSource = AutoCompleteSource.ListItems;
            _NO_TRANSLATE_Branch.Dock = DockStyle.Fill;
            _NO_TRANSLATE_Branch.FormattingEnabled = true;
            _NO_TRANSLATE_Branch.Location = new Point(0, 0);
            _NO_TRANSLATE_Branch.Margin = new Padding(0);
            _NO_TRANSLATE_Branch.Name = "_NO_TRANSLATE_Branch";
            _NO_TRANSLATE_Branch.Size = new Size(197, 21);
            _NO_TRANSLATE_Branch.TabIndex = 0;
            _NO_TRANSLATE_Branch.SelectedIndexChanged += _NO_TRANSLATE_Branch_SelectedIndexChanged;
            _NO_TRANSLATE_Branch.SelectedValueChanged += BranchSelectedValueChanged;
            _NO_TRANSLATE_Branch.Enter += _NO_TRANSLATE_Branch_Enter;
            // 
            // labelTo
            // 
            labelTo.AutoSize = true;
            labelTo.Location = new Point(200, 0);
            labelTo.Name = "labelTo";
            labelTo.RightToLeft = RightToLeft.Yes;
            labelTo.Size = new Size(17, 22);
            labelTo.TabIndex = 1;
            labelTo.Text = "&to";
            labelTo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // RemoteBranch
            // 
            RemoteBranch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            RemoteBranch.AutoCompleteSource = AutoCompleteSource.ListItems;
            RemoteBranch.Dock = DockStyle.Fill;
            RemoteBranch.FormattingEnabled = true;
            RemoteBranch.Location = new Point(220, 0);
            RemoteBranch.Margin = new Padding(0);
            RemoteBranch.Name = "RemoteBranch";
            RemoteBranch.Size = new Size(197, 21);
            RemoteBranch.TabIndex = 2;
            // 
            // ShowOptions
            // 
            ShowOptions.AutoSize = true;
            ShowOptions.Location = new Point(6, 40);
            ShowOptions.Name = "ShowOptions";
            ShowOptions.Size = new Size(71, 13);
            ShowOptions.TabIndex = 2;
            ShowOptions.TabStop = true;
            ShowOptions.Text = "Show options";
            ShowOptions.LinkClicked += ShowOptions_LinkClicked;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(RecursiveSubmodules);
            flowLayoutPanel1.Location = new Point(319, 2);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(215, 25);
            flowLayoutPanel1.TabIndex = 2;
            flowLayoutPanel1.WrapContents = false;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(0, 6);
            label2.Margin = new Padding(0);
            label2.Name = "label2";
            label2.Size = new Size(113, 13);
            label2.TabIndex = 0;
            label2.Text = "Recursive &submodules";
            // 
            // RecursiveSubmodules
            // 
            RecursiveSubmodules.Anchor = AnchorStyles.Left;
            RecursiveSubmodules.DropDownStyle = ComboBoxStyle.DropDownList;
            RecursiveSubmodules.Items.AddRange(new object[] {
            "None",
            "Check",
            "On-demand"});
            RecursiveSubmodules.Location = new Point(115, 2);
            RecursiveSubmodules.Margin = new Padding(2);
            RecursiveSubmodules.Name = "RecursiveSubmodules";
            RecursiveSubmodules.Size = new Size(98, 21);
            RecursiveSubmodules.TabIndex = 1;
            // 
            // ReplaceTrackingReference
            // 
            ReplaceTrackingReference.AutoSize = true;
            ReplaceTrackingReference.Location = new Point(1, 29);
            ReplaceTrackingReference.Name = "ReplaceTrackingReference";
            ReplaceTrackingReference.Size = new Size(155, 17);
            ReplaceTrackingReference.TabIndex = 1;
            ReplaceTrackingReference.Text = "R&eplace tracking reference";
            ReplaceTrackingReference.UseVisualStyleBackColor = true;
            // 
            // _createPullRequestCB
            // 
            _createPullRequestCB.AutoSize = true;
            _createPullRequestCB.Location = new Point(1, 52);
            _createPullRequestCB.Name = "_createPullRequestCB";
            _createPullRequestCB.Size = new Size(171, 17);
            _createPullRequestCB.TabIndex = 3;
            _createPullRequestCB.Text = "&Create pull request after push";
            _createPullRequestCB.UseVisualStyleBackColor = true;
            // 
            // labelFrom
            // 
            labelFrom.AutoSize = true;
            labelFrom.Location = new Point(6, 15);
            labelFrom.Name = "labelFrom";
            labelFrom.Size = new Size(79, 13);
            labelFrom.TabIndex = 0;
            labelFrom.Text = "&Branch to push";
            // 
            // TagTab
            // 
            TagTab.Controls.Add(ForcePushTags);
            TagTab.Controls.Add(label1);
            TagTab.Controls.Add(TagComboBox);
            TagTab.Location = new Point(4, 22);
            TagTab.Name = "TagTab";
            TagTab.Padding = new Padding(3);
            TagTab.Size = new Size(552, 123);
            TagTab.TabIndex = 1;
            TagTab.Text = "Push tags";
            TagTab.ToolTipText = "Push tags to remote repository";
            TagTab.UseVisualStyleBackColor = true;
            // 
            // ForcePushTags
            // 
            ForcePushTags.AutoSize = true;
            ForcePushTags.Location = new Point(124, 43);
            ForcePushTags.Name = "ForcePushTags";
            ForcePushTags.Size = new Size(79, 17);
            ForcePushTags.TabIndex = 2;
            ForcePushTags.Text = "&Force push";
            ForcePushTags.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 15);
            label1.Name = "label1";
            label1.Size = new Size(64, 13);
            label1.TabIndex = 0;
            label1.Text = "&Tag to push";
            // 
            // TagComboBox
            // 
            TagComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TagComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            TagComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            TagComboBox.FormattingEnabled = true;
            TagComboBox.Location = new Point(124, 12);
            TagComboBox.Name = "TagComboBox";
            TagComboBox.Size = new Size(262, 21);
            TagComboBox.TabIndex = 1;
            // 
            // MultipleBranchTab
            // 
            MultipleBranchTab.Controls.Add(BranchGrid);
            MultipleBranchTab.Location = new Point(4, 22);
            MultipleBranchTab.Name = "MultipleBranchTab";
            MultipleBranchTab.Padding = new Padding(3);
            MultipleBranchTab.Size = new Size(552, 123);
            MultipleBranchTab.TabIndex = 2;
            MultipleBranchTab.Text = "Push multiple branches";
            MultipleBranchTab.UseVisualStyleBackColor = true;
            // 
            // BranchGrid
            // 
            BranchGrid.AllowUserToAddRows = false;
            BranchGrid.AllowUserToDeleteRows = false;
            BranchGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            BranchGrid.Columns.AddRange(new DataGridViewColumn[] {
            LocalColumn,
            RemoteColumn,
            NewColumn,
            PushColumn,
            ForceColumn,
            DeleteColumn});
            BranchGrid.Dock = DockStyle.Fill;
            BranchGrid.Location = new Point(3, 3);
            BranchGrid.Name = "BranchGrid";
            BranchGrid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            BranchGrid.RowHeadersVisible = false;
            BranchGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            BranchGrid.Size = new Size(546, 117);
            BranchGrid.TabIndex = 0;
            BranchGrid.CurrentCellDirtyStateChanged += BranchGrid_CurrentCellDirtyStateChanged;
            BranchGrid.DataBindingComplete += BranchGrid_DataBindingComplete;
            BranchGrid.CellPainting += BranchGrid_CellPainting;
            // 
            // LocalColumn
            // 
            LocalColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            LocalColumn.HeaderText = "Local Branch";
            LocalColumn.Name = "LocalColumn";
            // 
            // RemoteColumn
            // 
            RemoteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            RemoteColumn.HeaderText = "Remote Branch";
            RemoteColumn.Name = "RemoteColumn";
            // 
            // NewColumn
            // 
            NewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            NewColumn.HeaderText = "Ahead/Behind";
            NewColumn.Name = "NewColumn";
            NewColumn.ReadOnly = true;
            NewColumn.Width = 97;
            // 
            // PushColumn
            // 
            PushColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            PushColumn.HeaderText = "Push";
            PushColumn.Name = "PushColumn";
            PushColumn.Width = 36;
            // 
            // ForceColumn
            // 
            ForceColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            ForceColumn.HeaderText = "Force";
            ForceColumn.Name = "ForceColumn";
            ForceColumn.Width = 101;
            // 
            // DeleteColumn
            // 
            DeleteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            DeleteColumn.HeaderText = "Delete Remote Branch";
            DeleteColumn.Name = "DeleteColumn";
            DeleteColumn.Width = 108;
            // 
            // LoadSSHKey
            // 
            LoadSSHKey.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LoadSSHKey.Image = Properties.Images.Putty;
            LoadSSHKey.ImageAlign = ContentAlignment.MiddleLeft;
            LoadSSHKey.Location = new Point(315, 253);
            LoadSSHKey.Name = "LoadSSHKey";
            LoadSSHKey.Size = new Size(137, 25);
            LoadSSHKey.TabIndex = 3;
            LoadSSHKey.Text = "Load SSH key";
            LoadSSHKey.UseVisualStyleBackColor = true;
            LoadSSHKey.Click += LoadSshKeyClick;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(PushToRemote);
            groupBox2.Controls.Add(_NO_TRANSLATE_Remotes);
            groupBox2.Controls.Add(AddRemote);
            groupBox2.Controls.Add(PushToUrl);
            groupBox2.Controls.Add(PushDestination);
            groupBox2.Controls.Add(folderBrowserButton1);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(556, 80);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "Push to";
            // 
            // _NO_TRANSLATE_Remotes
            // 
            _NO_TRANSLATE_Remotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_Remotes.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_Remotes.AutoCompleteSource = AutoCompleteSource.ListItems;
            _NO_TRANSLATE_Remotes.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_Remotes.FormattingEnabled = true;
            _NO_TRANSLATE_Remotes.Location = new Point(128, 19);
            _NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            _NO_TRANSLATE_Remotes.Size = new Size(262, 21);
            _NO_TRANSLATE_Remotes.TabIndex = 1;
            _NO_TRANSLATE_Remotes.SelectedIndexChanged += RemotesUpdated;
            _NO_TRANSLATE_Remotes.TextUpdate += RemotesUpdated;
            _NO_TRANSLATE_Remotes.Validated += RemotesValidated;
            // 
            // AddRemote
            // 
            AddRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddRemote.Image = Properties.Images.Remotes;
            AddRemote.ImageAlign = ContentAlignment.MiddleLeft;
            AddRemote.Location = new Point(396, 17);
            AddRemote.Name = "AddRemote";
            AddRemote.Size = new Size(150, 25);
            AddRemote.TabIndex = 2;
            AddRemote.Text = "&Manage remotes";
            AddRemote.UseVisualStyleBackColor = true;
            AddRemote.Click += AddRemoteClick;
            // 
            // PushDestination
            // 
            PushDestination.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PushDestination.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            PushDestination.AutoCompleteSource = AutoCompleteSource.ListItems;
            PushDestination.Enabled = false;
            PushDestination.FormattingEnabled = true;
            PushDestination.Location = new Point(128, 48);
            PushDestination.Name = "PushDestination";
            PushDestination.Size = new Size(262, 21);
            PushDestination.TabIndex = 4;
            // 
            // folderBrowserButton1
            // 
            folderBrowserButton1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            folderBrowserButton1.AutoSize = true;
            folderBrowserButton1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            folderBrowserButton1.Enabled = false;
            folderBrowserButton1.Location = new Point(496, 47);
            folderBrowserButton1.Name = "folderBrowserButton1";
            folderBrowserButton1.PathShowingControl = PushDestination;
            folderBrowserButton1.Size = new Size(0, 0);
            folderBrowserButton1.TabIndex = 5;
            // 
            // Pull
            // 
            Pull.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Pull.Location = new Point(11, 253);
            Pull.Name = "Pull";
            Pull.Size = new Size(101, 25);
            Pull.TabIndex = 2;
            Pull.Text = "P&ull";
            Pull.UseVisualStyleBackColor = true;
            Pull.Click += PullClick;
            // 
            // PushOptionsPanel
            // 
            PushOptionsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PushOptionsPanel.Controls.Add(flowLayoutPanel1);
            PushOptionsPanel.Controls.Add(ReplaceTrackingReference);
            PushOptionsPanel.Controls.Add(ForcePushOptionPanel);
            PushOptionsPanel.Controls.Add(_createPullRequestCB);
            PushOptionsPanel.Location = new Point(8, 40);
            PushOptionsPanel.Name = "PushOptionsPanel";
            PushOptionsPanel.Size = new Size(537, 83);
            PushOptionsPanel.TabIndex = 3;
            PushOptionsPanel.Visible = false;
            // 
            // contextMenuStripPush
            // 
            menuPushSelection.Items.AddRange(new ToolStripItem[] {
            unselectAllToolStripMenuItem,
            selectTrackedToolStripMenuItem,
            selectAllToolStripMenuItem});
            menuPushSelection.Name = "menuPushSelection";
            menuPushSelection.Size = new Size(181, 92);
            // 
            // unselectAllToolStripMenuItem
            // 
            unselectAllToolStripMenuItem.Name = "unselectAllToolStripMenuItem";
            unselectAllToolStripMenuItem.Size = new Size(180, 22);
            unselectAllToolStripMenuItem.Text = "Unselect all";
            unselectAllToolStripMenuItem.Click += unselectAllToolStripMenuItem_Click;
            // 
            // selectTrackedToolStripMenuItem
            // 
            selectTrackedToolStripMenuItem.Name = "selectTrackedToolStripMenuItem";
            selectTrackedToolStripMenuItem.Size = new Size(180, 22);
            selectTrackedToolStripMenuItem.Text = "Select tracked";
            selectTrackedToolStripMenuItem.Click += selectTrackedToolStripMenuItem_Click;
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.Size = new Size(180, 22);
            selectAllToolStripMenuItem.Text = "Select all";
            selectAllToolStripMenuItem.Click += selectAllToolStripMenuItem_Click;
            // 
            // FormPush
            // 
            AcceptButton = Push;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(584, 290);
            Controls.Add(TabControlTagBranch);
            Controls.Add(LoadSSHKey);
            Controls.Add(groupBox2);
            Controls.Add(Push);
            Controls.Add(Pull);
            MinimizeBox = false;
            MinimumSize = new Size(600, 329);
            Name = "FormPush";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Push";
            Load += FormPushLoad;
            ForcePushOptionPanel.ResumeLayout(false);
            ForcePushOptionPanel.PerformLayout();
            TabControlTagBranch.ResumeLayout(false);
            BranchTab.ResumeLayout(false);
            BranchTab.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            PushOptionsPanel.ResumeLayout(false);
            PushOptionsPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            TagTab.ResumeLayout(false);
            TagTab.PerformLayout();
            MultipleBranchTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(BranchGrid)).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            menuPushSelection.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Button Push;
        private ComboBox PushDestination;
        private CheckBox ForcePushTags;
        private Button Pull;
        private GroupBox groupBox2;
        private RadioButton PushToUrl;
        private RadioButton PushToRemote;
        private Button AddRemote;
        private ComboBox _NO_TRANSLATE_Remotes;
        private Button LoadSSHKey;
        private ComboBox TagComboBox;
        private Label label1;
        private TabControl TabControlTagBranch;
        private TabPage BranchTab;
        private TabPage TagTab;
        private ToolTip toolTip1;
        private TabPage MultipleBranchTab;
        private DataGridView BranchGrid;
        private DataGridViewTextBoxColumn LocalColumn;
        private DataGridViewTextBoxColumn RemoteColumn;
        private DataGridViewTextBoxColumn NewColumn;
        private DataGridViewCheckBoxColumn PushColumn;
        private DataGridViewCheckBoxColumn ForceColumn;
        private DataGridViewCheckBoxColumn DeleteColumn;
        private LinkLabel ShowOptions;
        private CheckBox ForcePushBranches;
        private CheckBox _createPullRequestCB;
        private Label labelTo;
        private ComboBox RemoteBranch;
        private Label labelFrom;
        private ComboBox _NO_TRANSLATE_Branch;
        private CheckBox ReplaceTrackingReference;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label2;
        private ComboBox RecursiveSubmodules;
        private UserControls.FolderBrowserButton folderBrowserButton1;
        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox ckForceWithLease;
        private FlowLayoutPanel ForcePushOptionPanel;
        private Panel PushOptionsPanel;
        private ContextMenuStrip menuPushSelection;
        private ToolStripMenuItem unselectAllToolStripMenuItem;
        private ToolStripMenuItem selectTrackedToolStripMenuItem;
        private ToolStripMenuItem selectAllToolStripMenuItem;
    }
}
