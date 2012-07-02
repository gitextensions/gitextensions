namespace GitUI
{
    partial class FormPush
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.AutoPullOnRejected = new System.Windows.Forms.CheckBox();
            this.Push = new System.Windows.Forms.Button();
            this.TabControlTagBranch = new System.Windows.Forms.TabControl();
            this.BranchTab = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ShowOptions = new System.Windows.Forms.LinkLabel();
            this.PushOptionsPanel = new System.Windows.Forms.Panel();
            this.PushAllBranches = new System.Windows.Forms.CheckBox();
            this.RecursiveSubmodulesCheck = new System.Windows.Forms.CheckBox();
            this.ForcePushBranches = new System.Windows.Forms.CheckBox();
            this._createPullRequestCB = new System.Windows.Forms.CheckBox();
            this.labelTo = new System.Windows.Forms.Label();
            this.RemoteBranch = new System.Windows.Forms.ComboBox();
            this.labelFrom = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Branch = new System.Windows.Forms.ComboBox();
            this.TagTab = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.TagOptionsPanel = new System.Windows.Forms.Panel();
            this.PushAllTags = new System.Windows.Forms.CheckBox();
            this.ForcePushTags = new System.Windows.Forms.CheckBox();
            this.ShowTagOptions = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.TagComboBox = new System.Windows.Forms.ComboBox();
            this.MultipleBranchTab = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.BranchGrid = new System.Windows.Forms.DataGridView();
            this.LocalColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RemoteColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PushColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ForceColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DeleteColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AddRemote = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this.BrowseSource = new System.Windows.Forms.Button();
            this.PushDestination = new System.Windows.Forms.ComboBox();
            this.Pull = new System.Windows.Forms.Button();
            this.ReplaceTrackingReference = new System.Windows.Forms.CheckBox();
            this.TabControlTagBranch.SuspendLayout();
            this.BranchTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.PushOptionsPanel.SuspendLayout();
            this.TagTab.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.TagOptionsPanel.SuspendLayout();
            this.MultipleBranchTab.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BranchGrid)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // PullFromUrl
            // 
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 49);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(49, 24);
            this.PullFromUrl.TabIndex = 1;
            this.PullFromUrl.Text = "Url";
            this.toolTip1.SetToolTip(this.PullFromUrl, "Url to push to");
            this.PullFromUrl.UseVisualStyleBackColor = true;
            this.PullFromUrl.CheckedChanged += new System.EventHandler(this.PullFromUrlCheckedChanged);
            // 
            // PullFromRemote
            // 
            this.PullFromRemote.AutoSize = true;
            this.PullFromRemote.Checked = true;
            this.PullFromRemote.Location = new System.Drawing.Point(7, 19);
            this.PullFromRemote.Name = "PullFromRemote";
            this.PullFromRemote.Size = new System.Drawing.Size(82, 24);
            this.PullFromRemote.TabIndex = 0;
            this.PullFromRemote.TabStop = true;
            this.PullFromRemote.Text = "Remote";
            this.toolTip1.SetToolTip(this.PullFromRemote, "Remote repository to push to");
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemoteCheckedChanged);
            // 
            // AutoPullOnRejected
            // 
            this.AutoPullOnRejected.AutoSize = true;
            this.AutoPullOnRejected.Location = new System.Drawing.Point(235, 28);
            this.AutoPullOnRejected.Name = "AutoPullOnRejected";
            this.AutoPullOnRejected.Size = new System.Drawing.Size(171, 24);
            this.AutoPullOnRejected.TabIndex = 23;
            this.AutoPullOnRejected.Text = "Auto pull on rejected";
            this.toolTip1.SetToolTip(this.AutoPullOnRejected, "Auto pull on non fast forward  rejected");
            this.AutoPullOnRejected.UseVisualStyleBackColor = true;
            // 
            // Push
            // 
            this.Push.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Push.Image = global::GitUI.Properties.Resources.ArrowUp;
            this.Push.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Push.Location = new System.Drawing.Point(530, 210);
            this.Push.Name = "Push";
            this.Push.Size = new System.Drawing.Size(101, 25);
            this.Push.TabIndex = 9;
            this.Push.Text = "&Push";
            this.Push.UseVisualStyleBackColor = true;
            this.Push.Click += new System.EventHandler(this.PushClick);
            // 
            // TabControlTagBranch
            // 
            this.TabControlTagBranch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControlTagBranch.Controls.Add(this.BranchTab);
            this.TabControlTagBranch.Controls.Add(this.TagTab);
            this.TabControlTagBranch.Controls.Add(this.MultipleBranchTab);
            this.TabControlTagBranch.HotTrack = true;
            this.TabControlTagBranch.ItemSize = new System.Drawing.Size(57, 18);
            this.TabControlTagBranch.Location = new System.Drawing.Point(12, 98);
            this.TabControlTagBranch.Multiline = true;
            this.TabControlTagBranch.Name = "TabControlTagBranch";
            this.TabControlTagBranch.SelectedIndex = 0;
            this.TabControlTagBranch.ShowToolTips = true;
            this.TabControlTagBranch.Size = new System.Drawing.Size(624, 102);
            this.TabControlTagBranch.TabIndex = 6;
            this.TabControlTagBranch.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabControlTagBranch_Selected);
            // 
            // BranchTab
            // 
            this.BranchTab.BackColor = System.Drawing.Color.Transparent;
            this.BranchTab.Controls.Add(this.groupBox1);
            this.BranchTab.Location = new System.Drawing.Point(4, 22);
            this.BranchTab.Name = "BranchTab";
            this.BranchTab.Padding = new System.Windows.Forms.Padding(3);
            this.BranchTab.Size = new System.Drawing.Size(616, 76);
            this.BranchTab.TabIndex = 0;
            this.BranchTab.Text = "Push branches";
            this.BranchTab.ToolTipText = "Push branches and commits to remote repository.";
            this.BranchTab.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ShowOptions);
            this.groupBox1.Controls.Add(this.PushOptionsPanel);
            this.groupBox1.Controls.Add(this.labelTo);
            this.groupBox1.Controls.Add(this.RemoteBranch);
            this.groupBox1.Controls.Add(this.labelFrom);
            this.groupBox1.Controls.Add(this._NO_TRANSLATE_Branch);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(610, 70);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Branch";
            // 
            // ShowOptions
            // 
            this.ShowOptions.AutoSize = true;
            this.ShowOptions.Location = new System.Drawing.Point(124, 46);
            this.ShowOptions.Name = "ShowOptions";
            this.ShowOptions.Size = new System.Drawing.Size(99, 20);
            this.ShowOptions.TabIndex = 26;
            this.ShowOptions.TabStop = true;
            this.ShowOptions.Text = "Show options";
            this.ShowOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowOptions_LinkClicked);
            // 
            // PushOptionsPanel
            // 
            this.PushOptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PushOptionsPanel.Controls.Add(this.ReplaceTrackingReference);
            this.PushOptionsPanel.Controls.Add(this.PushAllBranches);
            this.PushOptionsPanel.Controls.Add(this.RecursiveSubmodulesCheck);
            this.PushOptionsPanel.Controls.Add(this.ForcePushBranches);
            this.PushOptionsPanel.Controls.Add(this.AutoPullOnRejected);
            this.PushOptionsPanel.Controls.Add(this._createPullRequestCB);
            this.PushOptionsPanel.Location = new System.Drawing.Point(127, 46);
            this.PushOptionsPanel.Name = "PushOptionsPanel";
            this.PushOptionsPanel.Size = new System.Drawing.Size(479, 21);
            this.PushOptionsPanel.TabIndex = 25;
            this.PushOptionsPanel.Visible = false;
            // 
            // PushAllBranches
            // 
            this.PushAllBranches.AutoSize = true;
            this.PushAllBranches.Location = new System.Drawing.Point(0, 5);
            this.PushAllBranches.Name = "PushAllBranches";
            this.PushAllBranches.Size = new System.Drawing.Size(144, 24);
            this.PushAllBranches.TabIndex = 2;
            this.PushAllBranches.Text = "Push &all branches";
            this.PushAllBranches.UseVisualStyleBackColor = true;
            this.PushAllBranches.CheckedChanged += new System.EventHandler(this.PushAllBranchesCheckedChanged);
            // 
            // RecursiveSubmodulesCheck
            // 
            this.RecursiveSubmodulesCheck.AutoSize = true;
            this.RecursiveSubmodulesCheck.Checked = true;
            this.RecursiveSubmodulesCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RecursiveSubmodulesCheck.Location = new System.Drawing.Point(0, 50);
            this.RecursiveSubmodulesCheck.Name = "RecursiveSubmodulesCheck";
            this.RecursiveSubmodulesCheck.Size = new System.Drawing.Size(218, 24);
            this.RecursiveSubmodulesCheck.TabIndex = 24;
            this.RecursiveSubmodulesCheck.Text = "Recursive submodules check";
            this.RecursiveSubmodulesCheck.UseVisualStyleBackColor = true;
            // 
            // ForcePushBranches
            // 
            this.ForcePushBranches.AutoSize = true;
            this.ForcePushBranches.Location = new System.Drawing.Point(0, 28);
            this.ForcePushBranches.Name = "ForcePushBranches";
            this.ForcePushBranches.Size = new System.Drawing.Size(101, 24);
            this.ForcePushBranches.TabIndex = 3;
            this.ForcePushBranches.Text = "&Force Push";
            this.ForcePushBranches.UseVisualStyleBackColor = true;
            this.ForcePushBranches.CheckedChanged += new System.EventHandler(this.ForcePushBranchesCheckedChanged);
            // 
            // _createPullRequestCB
            // 
            this._createPullRequestCB.AutoSize = true;
            this._createPullRequestCB.Location = new System.Drawing.Point(235, 5);
            this._createPullRequestCB.Name = "_createPullRequestCB";
            this._createPullRequestCB.Size = new System.Drawing.Size(226, 24);
            this._createPullRequestCB.TabIndex = 22;
            this._createPullRequestCB.Text = "Create pull request after push";
            this._createPullRequestCB.UseVisualStyleBackColor = true;
            // 
            // labelTo
            // 
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(306, 21);
            this.labelTo.Name = "labelTo";
            this.labelTo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.labelTo.Size = new System.Drawing.Size(23, 20);
            this.labelTo.TabIndex = 21;
            this.labelTo.Text = "to";
            this.labelTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RemoteBranch
            // 
            this.RemoteBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.RemoteBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.RemoteBranch.FormattingEnabled = true;
            this.RemoteBranch.Location = new System.Drawing.Point(348, 19);
            this.RemoteBranch.Name = "RemoteBranch";
            this.RemoteBranch.Size = new System.Drawing.Size(252, 28);
            this.RemoteBranch.TabIndex = 1;
            // 
            // labelFrom
            // 
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(6, 22);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(107, 20);
            this.labelFrom.TabIndex = 17;
            this.labelFrom.Text = "Branch to push";
            // 
            // _NO_TRANSLATE_Branch
            // 
            this._NO_TRANSLATE_Branch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Branch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_Branch.FormattingEnabled = true;
            this._NO_TRANSLATE_Branch.Location = new System.Drawing.Point(127, 19);
            this._NO_TRANSLATE_Branch.Name = "_NO_TRANSLATE_Branch";
            this._NO_TRANSLATE_Branch.Size = new System.Drawing.Size(173, 28);
            this._NO_TRANSLATE_Branch.TabIndex = 0;
            this._NO_TRANSLATE_Branch.SelectedValueChanged += new System.EventHandler(this.BranchSelectedValueChanged);
            // 
            // TagTab
            // 
            this.TagTab.BackColor = System.Drawing.Color.Transparent;
            this.TagTab.Controls.Add(this.groupBox3);
            this.TagTab.Location = new System.Drawing.Point(4, 22);
            this.TagTab.Name = "TagTab";
            this.TagTab.Padding = new System.Windows.Forms.Padding(3);
            this.TagTab.Size = new System.Drawing.Size(616, 78);
            this.TagTab.TabIndex = 1;
            this.TagTab.Text = "Push tags";
            this.TagTab.ToolTipText = "Push tags to remote repository";
            this.TagTab.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.TagOptionsPanel);
            this.groupBox3.Controls.Add(this.ShowTagOptions);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.TagComboBox);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(610, 72);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tag";
            // 
            // TagOptionsPanel
            // 
            this.TagOptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TagOptionsPanel.Controls.Add(this.PushAllTags);
            this.TagOptionsPanel.Controls.Add(this.ForcePushTags);
            this.TagOptionsPanel.Location = new System.Drawing.Point(160, 44);
            this.TagOptionsPanel.Name = "TagOptionsPanel";
            this.TagOptionsPanel.Size = new System.Drawing.Size(447, 29);
            this.TagOptionsPanel.TabIndex = 28;
            this.TagOptionsPanel.Visible = false;
            // 
            // PushAllTags
            // 
            this.PushAllTags.AutoSize = true;
            this.PushAllTags.Location = new System.Drawing.Point(0, 3);
            this.PushAllTags.Name = "PushAllTags";
            this.PushAllTags.Size = new System.Drawing.Size(113, 24);
            this.PushAllTags.TabIndex = 19;
            this.PushAllTags.Text = "Push &all tags";
            this.PushAllTags.UseVisualStyleBackColor = true;
            // 
            // ForcePushTags
            // 
            this.ForcePushTags.AutoSize = true;
            this.ForcePushTags.Location = new System.Drawing.Point(0, 26);
            this.ForcePushTags.Name = "ForcePushTags";
            this.ForcePushTags.Size = new System.Drawing.Size(101, 24);
            this.ForcePushTags.TabIndex = 22;
            this.ForcePushTags.Text = "&Force Push";
            this.ForcePushTags.UseVisualStyleBackColor = true;
            this.ForcePushTags.CheckedChanged += new System.EventHandler(this.ForcePushTagsCheckedChanged);
            // 
            // ShowTagOptions
            // 
            this.ShowTagOptions.AutoSize = true;
            this.ShowTagOptions.Location = new System.Drawing.Point(157, 45);
            this.ShowTagOptions.Name = "ShowTagOptions";
            this.ShowTagOptions.Size = new System.Drawing.Size(99, 20);
            this.ShowTagOptions.TabIndex = 27;
            this.ShowTagOptions.TabStop = true;
            this.ShowTagOptions.Text = "Show options";
            this.ShowTagOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowTagOptions_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 20);
            this.label1.TabIndex = 17;
            this.label1.Text = "Tag to push";
            // 
            // TagComboBox
            // 
            this.TagComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.TagComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.TagComboBox.FormattingEnabled = true;
            this.TagComboBox.Location = new System.Drawing.Point(160, 19);
            this.TagComboBox.Name = "TagComboBox";
            this.TagComboBox.Size = new System.Drawing.Size(297, 28);
            this.TagComboBox.TabIndex = 18;
            // 
            // MultipleBranchTab
            // 
            this.MultipleBranchTab.Controls.Add(this.groupBox4);
            this.MultipleBranchTab.Location = new System.Drawing.Point(4, 22);
            this.MultipleBranchTab.Name = "MultipleBranchTab";
            this.MultipleBranchTab.Padding = new System.Windows.Forms.Padding(3);
            this.MultipleBranchTab.Size = new System.Drawing.Size(616, 78);
            this.MultipleBranchTab.TabIndex = 2;
            this.MultipleBranchTab.Text = "Push multiple branches";
            this.MultipleBranchTab.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.BranchGrid);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(610, 72);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Select Branches to Push";
            // 
            // BranchGrid
            // 
            this.BranchGrid.AllowUserToAddRows = false;
            this.BranchGrid.AllowUserToDeleteRows = false;
            this.BranchGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BranchGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LocalColumn,
            this.RemoteColumn,
            this.NewColumn,
            this.PushColumn,
            this.ForceColumn,
            this.DeleteColumn});
            this.BranchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchGrid.Location = new System.Drawing.Point(3, 23);
            this.BranchGrid.Name = "BranchGrid";
            this.BranchGrid.RowHeadersVisible = false;
            this.BranchGrid.Size = new System.Drawing.Size(604, 46);
            this.BranchGrid.TabIndex = 0;
            this.BranchGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.BranchGrid_CurrentCellDirtyStateChanged);
            // 
            // LocalColumn
            // 
            this.LocalColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.LocalColumn.DataPropertyName = "Local";
            this.LocalColumn.HeaderText = "Local Branch";
            this.LocalColumn.Name = "LocalColumn";
            // 
            // RemoteColumn
            // 
            this.RemoteColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RemoteColumn.DataPropertyName = "Remote";
            this.RemoteColumn.HeaderText = "Remote Branch";
            this.RemoteColumn.Name = "RemoteColumn";
            // 
            // NewColumn
            // 
            this.NewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.NewColumn.DataPropertyName = "New";
            this.NewColumn.HeaderText = "New at Remote";
            this.NewColumn.Name = "NewColumn";
            this.NewColumn.ReadOnly = true;
            this.NewColumn.Width = 125;
            // 
            // PushColumn
            // 
            this.PushColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.PushColumn.DataPropertyName = "Push";
            this.PushColumn.HeaderText = "Push";
            this.PushColumn.Name = "PushColumn";
            this.PushColumn.Width = 45;
            // 
            // ForceColumn
            // 
            this.ForceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ForceColumn.DataPropertyName = "Force";
            this.ForceColumn.HeaderText = "Push (Force Rewind)";
            this.ForceColumn.Name = "ForceColumn";
            this.ForceColumn.Width = 133;
            // 
            // DeleteColumn
            // 
            this.DeleteColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.DeleteColumn.DataPropertyName = "Delete";
            this.DeleteColumn.HeaderText = "Delete Remote Branch";
            this.DeleteColumn.Name = "DeleteColumn";
            this.DeleteColumn.Width = 148;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(382, 210);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(137, 25);
            this.LoadSSHKey.TabIndex = 8;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.PullFromUrl);
            this.groupBox2.Controls.Add(this.PullFromRemote);
            this.groupBox2.Controls.Add(this.AddRemote);
            this.groupBox2.Controls.Add(this._NO_TRANSLATE_Remotes);
            this.groupBox2.Controls.Add(this.BrowseSource);
            this.groupBox2.Controls.Add(this.PushDestination);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(624, 80);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Push to";
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Location = new System.Drawing.Point(514, 17);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(101, 25);
            this.AddRemote.TabIndex = 3;
            this.AddRemote.Text = "Manage remotes";
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemoteClick);
            // 
            // _NO_TRANSLATE_Remotes
            // 
            this._NO_TRANSLATE_Remotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Remotes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Remotes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_Remotes.FormattingEnabled = true;
            this._NO_TRANSLATE_Remotes.Location = new System.Drawing.Point(128, 19);
            this._NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            this._NO_TRANSLATE_Remotes.Size = new System.Drawing.Size(380, 28);
            this._NO_TRANSLATE_Remotes.TabIndex = 2;
            this._NO_TRANSLATE_Remotes.SelectedIndexChanged += new System.EventHandler(this.RemotesUpdated);
            this._NO_TRANSLATE_Remotes.TextUpdate += new System.EventHandler(this.RemotesUpdated);
            this._NO_TRANSLATE_Remotes.Validated += new System.EventHandler(this.RemotesValidated);
            // 
            // BrowseSource
            // 
            this.BrowseSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseSource.Enabled = false;
            this.BrowseSource.Location = new System.Drawing.Point(514, 46);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(101, 25);
            this.BrowseSource.TabIndex = 5;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSourceClick);
            // 
            // PushDestination
            // 
            this.PushDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PushDestination.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.PushDestination.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.PushDestination.Enabled = false;
            this.PushDestination.FormattingEnabled = true;
            this.PushDestination.Location = new System.Drawing.Point(128, 48);
            this.PushDestination.Name = "PushDestination";
            this.PushDestination.Size = new System.Drawing.Size(380, 28);
            this.PushDestination.TabIndex = 4;
            // 
            // Pull
            // 
            this.Pull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Pull.Location = new System.Drawing.Point(12, 210);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(101, 25);
            this.Pull.TabIndex = 7;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.PullClick);
            // 
            // ReplaceTrackingReference
            // 
            this.ReplaceTrackingReference.AutoSize = true;
            this.ReplaceTrackingReference.Location = new System.Drawing.Point(235, 50);
            this.ReplaceTrackingReference.Name = "ReplaceTrackingReference";
            this.ReplaceTrackingReference.Size = new System.Drawing.Size(207, 24);
            this.ReplaceTrackingReference.TabIndex = 25;
            this.ReplaceTrackingReference.Text = "Replace tracking reference";
            this.ReplaceTrackingReference.UseVisualStyleBackColor = true;
            // 
            // FormPush
            // 
            this.AcceptButton = this.Push;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 245);
            this.Controls.Add(this.TabControlTagBranch);
            this.Controls.Add(this.LoadSSHKey);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.Push);
            this.Controls.Add(this.Pull);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(660, 285);
            this.Name = "FormPush";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Push";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPush_FormClosing);
            this.Load += new System.EventHandler(this.FormPushLoad);
            this.TabControlTagBranch.ResumeLayout(false);
            this.BranchTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.PushOptionsPanel.ResumeLayout(false);
            this.PushOptionsPanel.PerformLayout();
            this.TagTab.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.TagOptionsPanel.ResumeLayout(false);
            this.TagOptionsPanel.PerformLayout();
            this.MultipleBranchTab.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BranchGrid)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.Button Push;
        private System.Windows.Forms.ComboBox PushDestination;
        private System.Windows.Forms.CheckBox ForcePushTags;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton PullFromUrl;
        private System.Windows.Forms.RadioButton PullFromRemote;
        private System.Windows.Forms.Button AddRemote;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Remotes;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.ComboBox TagComboBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl TabControlTagBranch;
        private System.Windows.Forms.TabPage BranchTab;
        private System.Windows.Forms.TabPage TagTab;
        private System.Windows.Forms.CheckBox PushAllTags;
        private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TabPage MultipleBranchTab;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.DataGridView BranchGrid;
		private System.Windows.Forms.DataGridViewTextBoxColumn LocalColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn RemoteColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn NewColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn PushColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ForceColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DeleteColumn;
        private System.Windows.Forms.Panel TagOptionsPanel;
        private System.Windows.Forms.LinkLabel ShowTagOptions;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel ShowOptions;
        private System.Windows.Forms.Panel PushOptionsPanel;
        private System.Windows.Forms.CheckBox PushAllBranches;
        private System.Windows.Forms.CheckBox RecursiveSubmodulesCheck;
        private System.Windows.Forms.CheckBox ForcePushBranches;
        private System.Windows.Forms.CheckBox AutoPullOnRejected;
        private System.Windows.Forms.CheckBox _createPullRequestCB;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.ComboBox RemoteBranch;
        private System.Windows.Forms.Label labelFrom;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Branch;
        private System.Windows.Forms.CheckBox ReplaceTrackingReference;
    }
}