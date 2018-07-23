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
            this.Tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Unshallow = new System.Windows.Forms.CheckBox();
            this.Pull = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.helpImageDisplayUserControl1 = new GitUI.Help.HelpImageDisplayUserControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this.AddRemote = new System.Windows.Forms.Button();
            this.comboBoxPullSource = new System.Windows.Forms.ComboBox();
            this.folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Merge = new System.Windows.Forms.RadioButton();
            this.Rebase = new System.Windows.Forms.RadioButton();
            this.Fetch = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.localBranch = new System.Windows.Forms.TextBox();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.ReachableTags = new System.Windows.Forms.RadioButton();
            this.NoTags = new System.Windows.Forms.RadioButton();
            this.AllTags = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Stash = new System.Windows.Forms.Button();
            this.AutoStash = new System.Windows.Forms.CheckBox();
            this.Prune = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // PullFromUrl
            // 
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 47);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(38, 17);
            this.PullFromUrl.TabIndex = 3;
            this.PullFromUrl.Text = "Url";
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
            this.PullFromRemote.Size = new System.Drawing.Size(62, 17);
            this.PullFromRemote.TabIndex = 0;
            this.PullFromRemote.TabStop = true;
            this.PullFromRemote.Text = "Remote";
            this.Tooltip.SetToolTip(this.PullFromRemote, "Remote repository to pull from");
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemoteCheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Local branch";
            this.Tooltip.SetToolTip(this.label1, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Remote branch";
            this.Tooltip.SetToolTip(this.label2, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // Unshallow
            // 
            this.Unshallow.AutoSize = true;
            this.Unshallow.Location = new System.Drawing.Point(7, 387);
            this.Unshallow.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
            this.Unshallow.Name = "Unshallow";
            this.Unshallow.Size = new System.Drawing.Size(126, 17);
            this.Unshallow.TabIndex = 20;
            this.Unshallow.Text = "Download full history";
            this.Tooltip.SetToolTip(this.Unshallow, "Fetches as much history from the remote source as possible.\nIf full history is available (if the source is not a shallow clone itself), then this repo will be a shallow clone no more.\n\nActual command line (if checked): --unshallow");
            this.Unshallow.Visible = false;
            // 
            // Pull
            // 
            this.Pull.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Pull.Image = global::GitUI.Properties.Images.ArrowDown;
            this.Pull.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Pull.Location = new System.Drawing.Point(489, 8);
            this.Pull.MinimumSize = new System.Drawing.Size(120, 25);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(124, 25);
            this.Pull.TabIndex = 40;
            this.Pull.Text = "&Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.PullClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.helpImageDisplayUserControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(674, 479);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // helpImageDisplayUserControl1
            // 
            this.helpImageDisplayUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpImageDisplayUserControl1.AutoSize = true;
            this.helpImageDisplayUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.helpImageDisplayUserControl1.Image1 = null;
            this.helpImageDisplayUserControl1.Image2 = null;
            this.helpImageDisplayUserControl1.IsExpanded = true;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.helpImageDisplayUserControl1.Location = new System.Drawing.Point(3, 3);
            this.helpImageDisplayUserControl1.MinimumSize = new System.Drawing.Size(40, 83);
            this.helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";
            this.helpImageDisplayUserControl1.Size = new System.Drawing.Size(40, 473);
            this.helpImageDisplayUserControl1.TabIndex = 10;
            this.helpImageDisplayUserControl1.UniqueIsExpandedSettingsId = "Pull";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(49, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(622, 473);
            this.panel2.TabIndex = 19;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.groupBox4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.Unshallow, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.Prune, 0, 5);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 7;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(622, 473);
            this.tableLayoutPanel2.TabIndex = 42;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.PullFromRemote);
            this.groupBox2.Controls.Add(this._NO_TRANSLATE_Remotes);
            this.groupBox2.Controls.Add(this.AddRemote);
            this.groupBox2.Controls.Add(this.PullFromUrl);
            this.groupBox2.Controls.Add(this.comboBoxPullSource);
            this.groupBox2.Controls.Add(this.folderBrowserButton1);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(616, 76);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pull from";
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
            this._NO_TRANSLATE_Remotes.Size = new System.Drawing.Size(306, 21);
            this._NO_TRANSLATE_Remotes.TabIndex = 1;
            this._NO_TRANSLATE_Remotes.TextChanged += new System.EventHandler(this.Remotes_TextChanged);
            this._NO_TRANSLATE_Remotes.Validating += new System.ComponentModel.CancelEventHandler(this.RemotesValidating);
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Image = global::GitUI.Properties.Images.Remotes;
            this.AddRemote.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddRemote.Location = new System.Drawing.Point(460, 14);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(150, 25);
            this.AddRemote.TabIndex = 2;
            this.AddRemote.Text = "Manage remotes";
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
            this.comboBoxPullSource.Size = new System.Drawing.Size(306, 21);
            this.comboBoxPullSource.TabIndex = 4;
            this.comboBoxPullSource.Validating += new System.ComponentModel.CancelEventHandler(this.PullSourceValidating);
            // 
            // folderBrowserButton1
            // 
            this.folderBrowserButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButton1.AutoSize = true;
            this.folderBrowserButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.folderBrowserButton1.Enabled = false;
            this.folderBrowserButton1.Location = new System.Drawing.Point(460, 45);
            this.folderBrowserButton1.Name = "folderBrowserButton1";
            this.folderBrowserButton1.PathShowingControl = this.comboBoxPullSource;
            this.folderBrowserButton1.Size = new System.Drawing.Size(100, 25);
            this.folderBrowserButton1.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(3, 169);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(616, 103);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge options";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.Merge);
            this.flowLayoutPanel1.Controls.Add(this.Rebase);
            this.flowLayoutPanel1.Controls.Add(this.Fetch);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(610, 83);
            this.flowLayoutPanel1.TabIndex = 11;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // Merge
            // 
            this.Merge.AutoSize = true;
            this.Merge.Image = global::GitUI.Properties.Images.Merge;
            this.Merge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Merge.Location = new System.Drawing.Point(3, 3);
            this.Merge.Name = "Merge";
            this.Merge.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Merge.Size = new System.Drawing.Size(239, 19);
            this.Merge.TabIndex = 8;
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
            this.Rebase.Location = new System.Drawing.Point(3, 28);
            this.Rebase.Name = "Rebase";
            this.Rebase.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Rebase.Size = new System.Drawing.Size(468, 19);
            this.Rebase.TabIndex = 9;
            this.Rebase.Text = "&Rebase current branch on top of remote branch, creates linear history (use with " +
    "caution)";
            this.Rebase.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Rebase.UseVisualStyleBackColor = true;
            this.Rebase.CheckedChanged += new System.EventHandler(this.RebaseCheckedChanged);
            // 
            // Fetch
            // 
            this.Fetch.AutoSize = true;
            this.Fetch.Location = new System.Drawing.Point(3, 53);
            this.Fetch.Name = "Fetch";
            this.Fetch.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.Fetch.Size = new System.Drawing.Size(225, 19);
            this.Fetch.TabIndex = 10;
            this.Fetch.Text = "Do not merge, only &fetch remote changes";
            this.Fetch.UseVisualStyleBackColor = true;
            this.Fetch.CheckedChanged += new System.EventHandler(this.FetchCheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.localBranch);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.Branches);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(3, 85);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(616, 78);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Branch";
            // 
            // localBranch
            // 
            this.localBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.localBranch.Location = new System.Drawing.Point(150, 14);
            this.localBranch.Name = "localBranch";
            this.localBranch.Size = new System.Drawing.Size(306, 21);
            this.localBranch.TabIndex = 9;
            this.localBranch.Leave += new System.EventHandler(this.localBranch_Leave);
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(150, 46);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(306, 21);
            this.Branches.TabIndex = 6;
            this.Branches.DropDown += new System.EventHandler(this.BranchesDropDown);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.flowLayoutPanel2);
            this.groupBox4.Location = new System.Drawing.Point(3, 278);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(616, 104);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Tag options";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.ReachableTags);
            this.flowLayoutPanel2.Controls.Add(this.NoTags);
            this.flowLayoutPanel2.Controls.Add(this.AllTags);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(610, 84);
            this.flowLayoutPanel2.TabIndex = 16;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // ReachableTags
            // 
            this.ReachableTags.AutoSize = true;
            this.ReachableTags.Checked = true;
            this.ReachableTags.Location = new System.Drawing.Point(3, 3);
            this.ReachableTags.Name = "ReachableTags";
            this.ReachableTags.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.ReachableTags.Size = new System.Drawing.Size(365, 19);
            this.ReachableTags.TabIndex = 17;
            this.ReachableTags.TabStop = true;
            this.ReachableTags.Text = "Follow tagopt, if not specified, fetch tags reachable from remote HEAD";
            this.ReachableTags.UseVisualStyleBackColor = true;
            // 
            // NoTags
            // 
            this.NoTags.AutoSize = true;
            this.NoTags.Location = new System.Drawing.Point(3, 28);
            this.NoTags.Name = "NoTags";
            this.NoTags.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.NoTags.Size = new System.Drawing.Size(86, 19);
            this.NoTags.TabIndex = 18;
            this.NoTags.Text = "Fetch no tag";
            this.NoTags.UseVisualStyleBackColor = true;
            // 
            // AllTags
            // 
            this.AllTags.AutoSize = true;
            this.AllTags.Location = new System.Drawing.Point(3, 53);
            this.AllTags.Name = "AllTags";
            this.AllTags.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.AllTags.Size = new System.Drawing.Size(89, 19);
            this.AllTags.TabIndex = 19;
            this.AllTags.Text = "Fetch all tags";
            this.AllTags.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.Pull, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.Mergetool, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.Stash, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.AutoStash, 2, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 430);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(616, 41);
            this.tableLayoutPanel3.TabIndex = 42;
            // 
            // Mergetool
            // 
            this.Mergetool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Mergetool.Location = new System.Drawing.Point(3, 8);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(141, 25);
            this.Mergetool.TabIndex = 11;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // Stash
            // 
            this.Stash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Stash.Location = new System.Drawing.Point(150, 8);
            this.Stash.Name = "Stash";
            this.Stash.Size = new System.Drawing.Size(132, 25);
            this.Stash.TabIndex = 12;
            this.Stash.Text = "Stash changes";
            this.Stash.UseVisualStyleBackColor = true;
            this.Stash.Click += new System.EventHandler(this.StashClick);
            // 
            // AutoStash
            // 
            this.AutoStash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AutoStash.AutoSize = true;
            this.AutoStash.Location = new System.Drawing.Point(288, 12);
            this.AutoStash.Name = "AutoStash";
            this.AutoStash.Size = new System.Drawing.Size(78, 17);
            this.AutoStash.TabIndex = 13;
            this.AutoStash.Text = "Auto stash";
            this.AutoStash.UseVisualStyleBackColor = true;
            // 
            // Prune
            // 
            this.Prune.AutoSize = true;
            this.Prune.Location = new System.Drawing.Point(7, 408);
            this.Prune.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
            this.Prune.Name = "Prune";
            this.Prune.Size = new System.Drawing.Size(138, 17);
            this.Prune.TabIndex = 20;
            this.Prune.Text = "Prune remote branches";
            this.Tooltip.SetToolTip(this.Prune, "Removes remote tracking branches that no longer exist on the remote (e.g. if some" +
        "one else deleted them).\r\n\r\nActual command line (if checked): --prune\r\n");
            // 
            // FormPull
            // 
            this.AcceptButton = this.Pull;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(674, 479);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(690, 508);
            this.Name = "FormPull";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pull";
            this.Load += new System.EventHandler(this.FormPullLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.RadioButton Merge;
        private System.Windows.Forms.RadioButton Rebase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Fetch;
        private System.Windows.Forms.ComboBox comboBoxPullSource;
        private System.Windows.Forms.Button Stash;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Remotes;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton PullFromUrl;
        private System.Windows.Forms.RadioButton PullFromRemote;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox AutoStash;
        private System.Windows.Forms.ToolTip Tooltip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox localBranch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Help.HelpImageDisplayUserControl helpImageDisplayUserControl1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.RadioButton ReachableTags;
        private System.Windows.Forms.RadioButton NoTags;
        private System.Windows.Forms.RadioButton AllTags;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button AddRemote;
        private UserControls.FolderBrowserButton folderBrowserButton1;
        private System.Windows.Forms.CheckBox Unshallow;
        private System.Windows.Forms.CheckBox Prune;
    }
}