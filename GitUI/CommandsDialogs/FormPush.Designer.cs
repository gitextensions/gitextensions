namespace GitUI.CommandsDialogs
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
            this.PushToUrl = new System.Windows.Forms.RadioButton();
            this.PushToRemote = new System.Windows.Forms.RadioButton();
            this.Push = new System.Windows.Forms.Button();
            this.TabControlTagBranch = new System.Windows.Forms.TabControl();
            this.BranchTab = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ShowOptions = new System.Windows.Forms.LinkLabel();
            this.PushOptionsPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.RecursiveSubmodules = new System.Windows.Forms.ComboBox();
            this.ReplaceTrackingReference = new System.Windows.Forms.CheckBox();
            this.PushAllBranches = new System.Windows.Forms.CheckBox();
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
            this.folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            this.PushDestination = new System.Windows.Forms.ComboBox();
            this.AddRemote = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this.Pull = new System.Windows.Forms.Button();
            this.TabControlTagBranch.SuspendLayout();
            this.BranchTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.PushOptionsPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.TagTab.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.TagOptionsPanel.SuspendLayout();
            this.MultipleBranchTab.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BranchGrid)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // PushToUrl
            // 
            this.PushToUrl.AutoSize = true;
            this.PushToUrl.Location = new System.Drawing.Point(9, 61);
            this.PushToUrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PushToUrl.Name = "PushToUrl";
            this.PushToUrl.Size = new System.Drawing.Size(53, 27);
            this.PushToUrl.TabIndex = 1;
            this.PushToUrl.Text = "Url";
            this.toolTip1.SetToolTip(this.PushToUrl, "Url to push to");
            this.PushToUrl.UseVisualStyleBackColor = true;
            this.PushToUrl.CheckedChanged += new System.EventHandler(this.PushToUrlCheckedChanged);
            // 
            // PushToRemote
            // 
            this.PushToRemote.AutoSize = true;
            this.PushToRemote.Checked = true;
            this.PushToRemote.Location = new System.Drawing.Point(9, 24);
            this.PushToRemote.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PushToRemote.Name = "PushToRemote";
            this.PushToRemote.Size = new System.Drawing.Size(90, 27);
            this.PushToRemote.TabIndex = 0;
            this.PushToRemote.TabStop = true;
            this.PushToRemote.Text = "Remote";
            this.toolTip1.SetToolTip(this.PushToRemote, "Remote repository to push to");
            this.PushToRemote.UseVisualStyleBackColor = true;
            this.PushToRemote.CheckedChanged += new System.EventHandler(this.PushToRemoteCheckedChanged);
            // 
            // Push
            // 
            this.Push.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Push.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Push.Image = global::GitUI.Properties.Resources.ArrowUp;
            this.Push.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Push.Location = new System.Drawing.Point(662, 239);
            this.Push.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Push.Name = "Push";
            this.Push.Size = new System.Drawing.Size(126, 31);
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
            this.TabControlTagBranch.Location = new System.Drawing.Point(15, 122);
            this.TabControlTagBranch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabControlTagBranch.Multiline = true;
            this.TabControlTagBranch.Name = "TabControlTagBranch";
            this.TabControlTagBranch.SelectedIndex = 0;
            this.TabControlTagBranch.ShowToolTips = true;
            this.TabControlTagBranch.Size = new System.Drawing.Size(780, 106);
            this.TabControlTagBranch.TabIndex = 6;
            this.TabControlTagBranch.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabControlTagBranch_Selected);
            // 
            // BranchTab
            // 
            this.BranchTab.BackColor = System.Drawing.Color.Transparent;
            this.BranchTab.Controls.Add(this.groupBox1);
            this.BranchTab.Location = new System.Drawing.Point(4, 22);
            this.BranchTab.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BranchTab.Name = "BranchTab";
            this.BranchTab.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BranchTab.Size = new System.Drawing.Size(772, 80);
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
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(764, 72);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Branch";
            // 
            // ShowOptions
            // 
            this.ShowOptions.AutoSize = true;
            this.ShowOptions.Location = new System.Drawing.Point(155, 58);
            this.ShowOptions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ShowOptions.Name = "ShowOptions";
            this.ShowOptions.Size = new System.Drawing.Size(113, 23);
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
            this.PushOptionsPanel.Controls.Add(this.flowLayoutPanel1);
            this.PushOptionsPanel.Controls.Add(this.ReplaceTrackingReference);
            this.PushOptionsPanel.Controls.Add(this.PushAllBranches);
            this.PushOptionsPanel.Controls.Add(this.ForcePushBranches);
            this.PushOptionsPanel.Controls.Add(this._createPullRequestCB);
            this.PushOptionsPanel.Location = new System.Drawing.Point(159, 58);
            this.PushOptionsPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PushOptionsPanel.Name = "PushOptionsPanel";
            this.PushOptionsPanel.Size = new System.Drawing.Size(602, 10);
            this.PushOptionsPanel.TabIndex = 25;
            this.PushOptionsPanel.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.RecursiveSubmodules);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(272, 6);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(304, 35);
            this.flowLayoutPanel1.TabIndex = 27;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(178, 23);
            this.label2.TabIndex = 26;
            this.label2.Text = "Recursive submodules";
            // 
            // RecursiveSubmodules
            // 
            this.RecursiveSubmodules.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.RecursiveSubmodules.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RecursiveSubmodules.Items.AddRange(new object[] {
            "None",
            "Check",
            "On-demand"});
            this.RecursiveSubmodules.Location = new System.Drawing.Point(180, 2);
            this.RecursiveSubmodules.Margin = new System.Windows.Forms.Padding(2);
            this.RecursiveSubmodules.Name = "RecursiveSubmodules";
            this.RecursiveSubmodules.Size = new System.Drawing.Size(122, 31);
            this.RecursiveSubmodules.TabIndex = 27;
            // 
            // ReplaceTrackingReference
            // 
            this.ReplaceTrackingReference.AutoSize = true;
            this.ReplaceTrackingReference.Location = new System.Drawing.Point(0, 81);
            this.ReplaceTrackingReference.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ReplaceTrackingReference.Name = "ReplaceTrackingReference";
            this.ReplaceTrackingReference.Size = new System.Drawing.Size(233, 27);
            this.ReplaceTrackingReference.TabIndex = 25;
            this.ReplaceTrackingReference.Text = "Replace tracking reference";
            this.ReplaceTrackingReference.UseVisualStyleBackColor = true;
            // 
            // PushAllBranches
            // 
            this.PushAllBranches.AutoSize = true;
            this.PushAllBranches.Location = new System.Drawing.Point(0, 11);
            this.PushAllBranches.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PushAllBranches.Name = "PushAllBranches";
            this.PushAllBranches.Size = new System.Drawing.Size(165, 27);
            this.PushAllBranches.TabIndex = 2;
            this.PushAllBranches.Text = "Push &all branches";
            this.PushAllBranches.UseVisualStyleBackColor = true;
            this.PushAllBranches.CheckedChanged += new System.EventHandler(this.PushAllBranchesCheckedChanged);
            // 
            // ForcePushBranches
            // 
            this.ForcePushBranches.AutoSize = true;
            this.ForcePushBranches.Location = new System.Drawing.Point(0, 46);
            this.ForcePushBranches.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ForcePushBranches.Name = "ForcePushBranches";
            this.ForcePushBranches.Size = new System.Drawing.Size(115, 27);
            this.ForcePushBranches.TabIndex = 3;
            this.ForcePushBranches.Text = "&Force Push";
            this.ForcePushBranches.UseVisualStyleBackColor = true;
            this.ForcePushBranches.CheckedChanged += new System.EventHandler(this.ForcePushBranchesCheckedChanged);
            // 
            // _createPullRequestCB
            // 
            this._createPullRequestCB.AutoSize = true;
            this._createPullRequestCB.Location = new System.Drawing.Point(276, 46);
            this._createPullRequestCB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._createPullRequestCB.Name = "_createPullRequestCB";
            this._createPullRequestCB.Size = new System.Drawing.Size(259, 27);
            this._createPullRequestCB.TabIndex = 22;
            this._createPullRequestCB.Text = "Create pull request after push";
            this._createPullRequestCB.UseVisualStyleBackColor = true;
            // 
            // labelTo
            // 
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(382, 26);
            this.labelTo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTo.Name = "labelTo";
            this.labelTo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.labelTo.Size = new System.Drawing.Size(26, 23);
            this.labelTo.TabIndex = 21;
            this.labelTo.Text = "to";
            this.labelTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RemoteBranch
            // 
            this.RemoteBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.RemoteBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.RemoteBranch.FormattingEnabled = true;
            this.RemoteBranch.Location = new System.Drawing.Point(435, 24);
            this.RemoteBranch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RemoteBranch.Name = "RemoteBranch";
            this.RemoteBranch.Size = new System.Drawing.Size(314, 31);
            this.RemoteBranch.TabIndex = 1;
            // 
            // labelFrom
            // 
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(8, 28);
            this.labelFrom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(126, 23);
            this.labelFrom.TabIndex = 17;
            this.labelFrom.Text = "Branch to push";
            // 
            // _NO_TRANSLATE_Branch
            // 
            this._NO_TRANSLATE_Branch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Branch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_Branch.FormattingEnabled = true;
            this._NO_TRANSLATE_Branch.Location = new System.Drawing.Point(159, 24);
            this._NO_TRANSLATE_Branch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._NO_TRANSLATE_Branch.Name = "_NO_TRANSLATE_Branch";
            this._NO_TRANSLATE_Branch.Size = new System.Drawing.Size(215, 31);
            this._NO_TRANSLATE_Branch.TabIndex = 0;
            this._NO_TRANSLATE_Branch.SelectedValueChanged += new System.EventHandler(this.BranchSelectedValueChanged);
            // 
            // TagTab
            // 
            this.TagTab.BackColor = System.Drawing.Color.Transparent;
            this.TagTab.Controls.Add(this.groupBox3);
            this.TagTab.Location = new System.Drawing.Point(4, 22);
            this.TagTab.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TagTab.Name = "TagTab";
            this.TagTab.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TagTab.Size = new System.Drawing.Size(772, 92);
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
            this.groupBox3.Location = new System.Drawing.Point(4, 4);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Size = new System.Drawing.Size(764, 84);
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
            this.TagOptionsPanel.Location = new System.Drawing.Point(200, 55);
            this.TagOptionsPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TagOptionsPanel.Name = "TagOptionsPanel";
            this.TagOptionsPanel.Size = new System.Drawing.Size(562, 30);
            this.TagOptionsPanel.TabIndex = 28;
            this.TagOptionsPanel.Visible = false;
            // 
            // PushAllTags
            // 
            this.PushAllTags.AutoSize = true;
            this.PushAllTags.Location = new System.Drawing.Point(0, 4);
            this.PushAllTags.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PushAllTags.Name = "PushAllTags";
            this.PushAllTags.Size = new System.Drawing.Size(128, 27);
            this.PushAllTags.TabIndex = 19;
            this.PushAllTags.Text = "Push &all tags";
            this.PushAllTags.UseVisualStyleBackColor = true;
            // 
            // ForcePushTags
            // 
            this.ForcePushTags.AutoSize = true;
            this.ForcePushTags.Location = new System.Drawing.Point(0, 32);
            this.ForcePushTags.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ForcePushTags.Name = "ForcePushTags";
            this.ForcePushTags.Size = new System.Drawing.Size(115, 27);
            this.ForcePushTags.TabIndex = 22;
            this.ForcePushTags.Text = "&Force Push";
            this.ForcePushTags.UseVisualStyleBackColor = true;
            this.ForcePushTags.CheckedChanged += new System.EventHandler(this.ForcePushTagsCheckedChanged);
            // 
            // ShowTagOptions
            // 
            this.ShowTagOptions.AutoSize = true;
            this.ShowTagOptions.Location = new System.Drawing.Point(196, 56);
            this.ShowTagOptions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ShowTagOptions.Name = "ShowTagOptions";
            this.ShowTagOptions.Size = new System.Drawing.Size(113, 23);
            this.ShowTagOptions.TabIndex = 27;
            this.ShowTagOptions.TabStop = true;
            this.ShowTagOptions.Text = "Show options";
            this.ShowTagOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowTagOptions_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 23);
            this.label1.TabIndex = 17;
            this.label1.Text = "Tag to push";
            // 
            // TagComboBox
            // 
            this.TagComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.TagComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.TagComboBox.FormattingEnabled = true;
            this.TagComboBox.Location = new System.Drawing.Point(200, 24);
            this.TagComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TagComboBox.Name = "TagComboBox";
            this.TagComboBox.Size = new System.Drawing.Size(370, 31);
            this.TagComboBox.TabIndex = 18;
            // 
            // MultipleBranchTab
            // 
            this.MultipleBranchTab.Controls.Add(this.groupBox4);
            this.MultipleBranchTab.Location = new System.Drawing.Point(4, 22);
            this.MultipleBranchTab.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MultipleBranchTab.Name = "MultipleBranchTab";
            this.MultipleBranchTab.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MultipleBranchTab.Size = new System.Drawing.Size(772, 92);
            this.MultipleBranchTab.TabIndex = 2;
            this.MultipleBranchTab.Text = "Push multiple branches";
            this.MultipleBranchTab.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.BranchGrid);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(4, 4);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Size = new System.Drawing.Size(764, 84);
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
            this.BranchGrid.Location = new System.Drawing.Point(4, 27);
            this.BranchGrid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BranchGrid.Name = "BranchGrid";
            this.BranchGrid.RowHeadersVisible = false;
            this.BranchGrid.Size = new System.Drawing.Size(756, 53);
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
            this.NewColumn.Width = 140;
            // 
            // PushColumn
            // 
            this.PushColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.PushColumn.DataPropertyName = "Push";
            this.PushColumn.HeaderText = "Push";
            this.PushColumn.Name = "PushColumn";
            this.PushColumn.Width = 53;
            // 
            // ForceColumn
            // 
            this.ForceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ForceColumn.DataPropertyName = "Force";
            this.ForceColumn.HeaderText = "Push (Force Rewind)";
            this.ForceColumn.Name = "ForceColumn";
            this.ForceColumn.Width = 152;
            // 
            // DeleteColumn
            // 
            this.DeleteColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.DeleteColumn.DataPropertyName = "Delete";
            this.DeleteColumn.HeaderText = "Delete Remote Branch";
            this.DeleteColumn.Name = "DeleteColumn";
            this.DeleteColumn.Width = 168;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(478, 239);
            this.LoadSSHKey.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(171, 31);
            this.LoadSSHKey.TabIndex = 8;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.folderBrowserButton1);
            this.groupBox2.Controls.Add(this.PushToUrl);
            this.groupBox2.Controls.Add(this.PushToRemote);
            this.groupBox2.Controls.Add(this.AddRemote);
            this.groupBox2.Controls.Add(this._NO_TRANSLATE_Remotes);
            this.groupBox2.Controls.Add(this.PushDestination);
            this.groupBox2.Location = new System.Drawing.Point(15, 15);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(780, 100);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Push to";
            // 
            // folderBrowserButton1
            // 
            this.folderBrowserButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButton1.Enabled = false;
            this.folderBrowserButton1.Location = new System.Drawing.Point(580, 59);
            this.folderBrowserButton1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.folderBrowserButton1.Name = "folderBrowserButton1";
            this.folderBrowserButton1.PathShowingControl = this.PushDestination;
            this.folderBrowserButton1.Size = new System.Drawing.Size(188, 31);
            this.folderBrowserButton1.TabIndex = 6;
            // 
            // PushDestination
            // 
            this.PushDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PushDestination.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.PushDestination.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.PushDestination.Enabled = false;
            this.PushDestination.FormattingEnabled = true;
            this.PushDestination.Location = new System.Drawing.Point(160, 60);
            this.PushDestination.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PushDestination.Name = "PushDestination";
            this.PushDestination.Size = new System.Drawing.Size(412, 31);
            this.PushDestination.TabIndex = 4;
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Image = global::GitUI.Properties.Resources.IconRemotes;
            this.AddRemote.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddRemote.Location = new System.Drawing.Point(580, 21);
            this.AddRemote.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(188, 31);
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
            this._NO_TRANSLATE_Remotes.Location = new System.Drawing.Point(160, 24);
            this._NO_TRANSLATE_Remotes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            this._NO_TRANSLATE_Remotes.Size = new System.Drawing.Size(412, 31);
            this._NO_TRANSLATE_Remotes.TabIndex = 2;
            this._NO_TRANSLATE_Remotes.SelectedIndexChanged += new System.EventHandler(this.RemotesUpdated);
            this._NO_TRANSLATE_Remotes.TextUpdate += new System.EventHandler(this.RemotesUpdated);
            this._NO_TRANSLATE_Remotes.Validated += new System.EventHandler(this.RemotesValidated);
            // 
            // Pull
            // 
            this.Pull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Pull.Location = new System.Drawing.Point(15, 239);
            this.Pull.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(126, 31);
            this.Pull.TabIndex = 7;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.PullClick);
            // 
            // FormPush
            // 
            this.AcceptButton = this.Push;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(815, 296);
            this.Controls.Add(this.TabControlTagBranch);
            this.Controls.Add(this.LoadSSHKey);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.Push);
            this.Controls.Add(this.Pull);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(819, 343);
            this.Name = "FormPush";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Push";
            this.Load += new System.EventHandler(this.FormPushLoad);
            this.TabControlTagBranch.ResumeLayout(false);
            this.BranchTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.PushOptionsPanel.ResumeLayout(false);
            this.PushOptionsPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
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

        private System.Windows.Forms.Button Push;
        private System.Windows.Forms.ComboBox PushDestination;
        private System.Windows.Forms.CheckBox ForcePushTags;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton PushToUrl;
        private System.Windows.Forms.RadioButton PushToRemote;
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
        private System.Windows.Forms.CheckBox ForcePushBranches;
        private System.Windows.Forms.CheckBox _createPullRequestCB;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.ComboBox RemoteBranch;
        private System.Windows.Forms.Label labelFrom;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Branch;
        private System.Windows.Forms.CheckBox ReplaceTrackingReference;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox RecursiveSubmodules;
        private UserControls.FolderBrowserButton folderBrowserButton1;
    }
}