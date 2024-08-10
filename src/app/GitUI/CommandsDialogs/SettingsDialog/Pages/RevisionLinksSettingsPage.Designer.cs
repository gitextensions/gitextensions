namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    sealed partial class RevisionLinksSettingsPage
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
            splitContainer1 = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            toolStripManageCategories = new ToolStripEx();
            Add = new ToolStripSplitButton();
            toolStripSeparator1 = new ToolStripSeparator();
            Remove = new ToolStripButton();
            _NO_TRANSLATE_Categories = new ListBox();
            CategoriesLabel = new Label();
            LinksGrid = new DataGridView();
            CaptionCol = new DataGridViewTextBoxColumn();
            URICol = new DataGridViewTextBoxColumn();
            panel1 = new Panel();
            label6 = new Label();
            detailPanel = new Panel();
            tableLayoutPanel3 = new TableLayoutPanel();
            remoteGrp = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            lblUseRemotes = new Label();
            _NO_TRANSLATE_RemotePatern = new TextBox();
            lblSearchRemotePattern = new Label();
            lblRemoteSearchIn = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            chxURL = new CheckBox();
            chxPushURL = new CheckBox();
            flowLayoutPanel4 = new FlowLayoutPanel();
            _NO_TRANSLATE_UseRemotes = new TextBox();
            chkOnlyFirstRemote = new CheckBox();
            revisionDataGrp = new GroupBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            _NO_TRANSLATE_SearchPatternEdit = new TextBox();
            _NO_TRANSLATE_NestedPatternEdit = new TextBox();
            label2 = new Label();
            label5 = new Label();
            nestedPatternLab = new Label();
            flowLayoutPanel3 = new FlowLayoutPanel();
            MessageChx = new CheckBox();
            LocalBranchChx = new CheckBox();
            RemoteBranchChx = new CheckBox();
            label1 = new Label();
            flowLayoutPanel2 = new FlowLayoutPanel();
            _NO_TRANSLATE_Name = new TextBox();
            EnabledChx = new CheckBox();
            gotoUserManualControl1 = new UserControls.GotoUserManualControl();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            toolStripManageCategories.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LinksGrid).BeginInit();
            panel1.SuspendLayout();
            detailPanel.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            remoteGrp.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel4.SuspendLayout();
            revisionDataGrp.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.AutoScaleDimensions = new SizeF(96F, 96F);
            splitContainer1.AutoScaleMode = AutoScaleMode.Dpi;
            splitContainer1.BackColor = SystemColors.Control;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(2, 3, 2, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = SystemColors.Control;
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            splitContainer1.Panel1.Controls.Add(_NO_TRANSLATE_Categories);
            splitContainer1.Panel1.Controls.Add(CategoriesLabel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(LinksGrid);
            splitContainer1.Panel2.Controls.Add(panel1);
            splitContainer1.Panel2.Controls.Add(detailPanel);
            splitContainer1.Size = new Size(1520, 788);
            splitContainer1.SplitterDistance = 192;
            splitContainer1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.BackColor = SystemColors.Control;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(toolStripManageCategories, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Bottom;
            tableLayoutPanel1.Location = new Point(0, 762);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(192, 26);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // toolStripManageCategories
            // 
            toolStripManageCategories.AllowMerge = false;
            toolStripManageCategories.GripMargin = new Padding(0);
            toolStripManageCategories.GripStyle = ToolStripGripStyle.Hidden;
            toolStripManageCategories.Items.AddRange(new ToolStripItem[] { Add, toolStripSeparator1, Remove });
            toolStripManageCategories.Location = new Point(0, 1);
            toolStripManageCategories.Margin = new Padding(0, 1, 0, 0);
            toolStripManageCategories.Name = "toolStripManageCategories";
            toolStripManageCategories.Size = new Size(192, 25);
            toolStripManageCategories.TabIndex = 5;
            // 
            // Add
            // 
            Add.BackColor = SystemColors.Control;
            Add.Image = Properties.Resources.FileStatusAdded;
            Add.ImageTransparentColor = Color.Magenta;
            Add.Name = "Add";
            Add.Size = new Size(61, 22);
            Add.Text = "Add";
            Add.ButtonClick += Add_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // Remove
            // 
            Remove.Image = Properties.Resources.FileStatusRemoved;
            Remove.ImageTransparentColor = Color.Magenta;
            Remove.Name = "Remove";
            Remove.Size = new Size(70, 22);
            Remove.Text = "Remove";
            Remove.Click += Remove_Click;
            // 
            // _NO_TRANSLATE_Categories
            // 
            _NO_TRANSLATE_Categories.Dock = DockStyle.Fill;
            _NO_TRANSLATE_Categories.FormattingEnabled = true;
            _NO_TRANSLATE_Categories.IntegralHeight = false;
            _NO_TRANSLATE_Categories.ItemHeight = 15;
            _NO_TRANSLATE_Categories.Location = new Point(0, 19);
            _NO_TRANSLATE_Categories.Margin = new Padding(2, 3, 2, 3);
            _NO_TRANSLATE_Categories.Name = "_NO_TRANSLATE_Categories";
            _NO_TRANSLATE_Categories.Size = new Size(192, 769);
            _NO_TRANSLATE_Categories.TabIndex = 1;
            _NO_TRANSLATE_Categories.SelectedIndexChanged += _NO_TRANSLATE_Categories_SelectedIndexChanged;
            // 
            // CategoriesLabel
            // 
            CategoriesLabel.AutoSize = true;
            CategoriesLabel.Dock = DockStyle.Top;
            CategoriesLabel.ImeMode = ImeMode.NoControl;
            CategoriesLabel.Location = new Point(0, 0);
            CategoriesLabel.Margin = new Padding(2, 0, 2, 0);
            CategoriesLabel.Name = "CategoriesLabel";
            CategoriesLabel.Padding = new Padding(0, 0, 0, 4);
            CategoriesLabel.Size = new Size(63, 19);
            CategoriesLabel.TabIndex = 0;
            CategoriesLabel.Text = "Categories";
            // 
            // LinksGrid
            // 
            LinksGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LinksGrid.Columns.AddRange(new DataGridViewColumn[] { CaptionCol, URICol });
            LinksGrid.Dock = DockStyle.Fill;
            LinksGrid.Location = new Point(0, 322);
            LinksGrid.Margin = new Padding(2, 3, 2, 3);
            LinksGrid.MultiSelect = false;
            LinksGrid.Name = "LinksGrid";
            LinksGrid.RowHeadersVisible = false;
            LinksGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            LinksGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            LinksGrid.Size = new Size(1324, 466);
            LinksGrid.TabIndex = 8;
            // 
            // CaptionCol
            // 
            CaptionCol.HeaderText = "Caption";
            CaptionCol.Name = "CaptionCol";
            // 
            // URICol
            // 
            URICol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            URICol.HeaderText = "URI";
            URICol.Name = "URICol";
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(label6);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 304);
            panel1.MinimumSize = new Size(0, 18);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(0, 0, 0, 3);
            panel1.Size = new Size(1324, 18);
            panel1.TabIndex = 7;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ImeMode = ImeMode.NoControl;
            label6.Location = new Point(0, 0);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(34, 15);
            label6.TabIndex = 21;
            label6.Text = "Links";
            // 
            // detailPanel
            // 
            detailPanel.AutoSize = true;
            detailPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            detailPanel.BackColor = SystemColors.Control;
            detailPanel.Controls.Add(tableLayoutPanel3);
            detailPanel.Dock = DockStyle.Top;
            detailPanel.Location = new Point(0, 0);
            detailPanel.Name = "detailPanel";
            detailPanel.Padding = new Padding(0, 3, 0, 6);
            detailPanel.Size = new Size(1324, 304);
            detailPanel.TabIndex = 6;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.BackColor = SystemColors.Control;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(remoteGrp, 0, 2);
            tableLayoutPanel3.Controls.Add(revisionDataGrp, 0, 3);
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(flowLayoutPanel2, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Top;
            tableLayoutPanel3.Location = new Point(0, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(1324, 269);
            tableLayoutPanel3.TabIndex = 14;
            // 
            // remoteGrp
            // 
            remoteGrp.AutoSize = true;
            remoteGrp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.SetColumnSpan(remoteGrp, 2);
            remoteGrp.Controls.Add(tableLayoutPanel2);
            remoteGrp.Dock = DockStyle.Fill;
            remoteGrp.Location = new Point(3, 32);
            remoteGrp.Name = "remoteGrp";
            remoteGrp.Size = new Size(1318, 117);
            remoteGrp.TabIndex = 28;
            remoteGrp.TabStop = false;
            remoteGrp.Text = "Remote data";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.BackColor = SystemColors.Control;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(lblUseRemotes, 0, 0);
            tableLayoutPanel2.Controls.Add(_NO_TRANSLATE_RemotePatern, 1, 2);
            tableLayoutPanel2.Controls.Add(lblSearchRemotePattern, 0, 2);
            tableLayoutPanel2.Controls.Add(lblRemoteSearchIn, 0, 1);
            tableLayoutPanel2.Controls.Add(flowLayoutPanel1, 1, 1);
            tableLayoutPanel2.Controls.Add(flowLayoutPanel4, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Top;
            tableLayoutPanel2.Location = new Point(3, 19);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(1312, 95);
            tableLayoutPanel2.TabIndex = 17;
            // 
            // lblUseRemotes
            // 
            lblUseRemotes.Anchor = AnchorStyles.Left;
            lblUseRemotes.AutoSize = true;
            lblUseRemotes.ImeMode = ImeMode.NoControl;
            lblUseRemotes.Location = new Point(2, 10);
            lblUseRemotes.Margin = new Padding(2, 0, 2, 0);
            lblUseRemotes.Name = "lblUseRemotes";
            lblUseRemotes.Size = new Size(72, 15);
            lblUseRemotes.TabIndex = 24;
            lblUseRemotes.Text = "Use remotes";
            // 
            // _NO_TRANSLATE_RemotePatern
            // 
            _NO_TRANSLATE_RemotePatern.Dock = DockStyle.Fill;
            _NO_TRANSLATE_RemotePatern.Location = new Point(89, 69);
            _NO_TRANSLATE_RemotePatern.Margin = new Padding(2, 3, 2, 3);
            _NO_TRANSLATE_RemotePatern.Name = "_NO_TRANSLATE_RemotePatern";
            _NO_TRANSLATE_RemotePatern.Size = new Size(1221, 23);
            _NO_TRANSLATE_RemotePatern.TabIndex = 22;
            _NO_TRANSLATE_RemotePatern.Leave += _NO_TRANSLATE_RemotePatern_Leave;
            // 
            // lblSearchRemotePattern
            // 
            lblSearchRemotePattern.Anchor = AnchorStyles.Left;
            lblSearchRemotePattern.AutoSize = true;
            lblSearchRemotePattern.ImeMode = ImeMode.NoControl;
            lblSearchRemotePattern.Location = new Point(2, 73);
            lblSearchRemotePattern.Margin = new Padding(2, 0, 2, 0);
            lblSearchRemotePattern.Name = "lblSearchRemotePattern";
            lblSearchRemotePattern.Size = new Size(83, 15);
            lblSearchRemotePattern.TabIndex = 19;
            lblSearchRemotePattern.Text = "Search pattern";
            // 
            // lblRemoteSearchIn
            // 
            lblRemoteSearchIn.Anchor = AnchorStyles.Left;
            lblRemoteSearchIn.AutoSize = true;
            lblRemoteSearchIn.ImeMode = ImeMode.NoControl;
            lblRemoteSearchIn.Location = new Point(2, 43);
            lblRemoteSearchIn.Margin = new Padding(2, 0, 2, 0);
            lblRemoteSearchIn.Name = "lblRemoteSearchIn";
            lblRemoteSearchIn.Size = new Size(55, 15);
            lblRemoteSearchIn.TabIndex = 15;
            lblRemoteSearchIn.Text = "Search in";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(chxURL);
            flowLayoutPanel1.Controls.Add(chxPushURL);
            flowLayoutPanel1.Location = new Point(90, 38);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(135, 25);
            flowLayoutPanel1.TabIndex = 23;
            // 
            // chxURL
            // 
            chxURL.AutoSize = true;
            chxURL.Location = new Point(3, 3);
            chxURL.Name = "chxURL";
            chxURL.Size = new Size(47, 19);
            chxURL.TabIndex = 0;
            chxURL.Text = "URL";
            chxURL.UseVisualStyleBackColor = true;
            chxURL.CheckedChanged += chxURL_CheckedChanged;
            // 
            // chxPushURL
            // 
            chxPushURL.AutoSize = true;
            chxPushURL.Location = new Point(56, 3);
            chxPushURL.Name = "chxPushURL";
            chxPushURL.Size = new Size(76, 19);
            chxPushURL.TabIndex = 2;
            chxPushURL.Text = "Push URL";
            chxPushURL.UseVisualStyleBackColor = true;
            chxPushURL.CheckedChanged += chxPushURL_CheckedChanged;
            // 
            // flowLayoutPanel4
            // 
            flowLayoutPanel4.AutoSize = true;
            flowLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel4.BackColor = SystemColors.Control;
            flowLayoutPanel4.Controls.Add(_NO_TRANSLATE_UseRemotes);
            flowLayoutPanel4.Controls.Add(chkOnlyFirstRemote);
            flowLayoutPanel4.Dock = DockStyle.Fill;
            flowLayoutPanel4.Location = new Point(90, 3);
            flowLayoutPanel4.Name = "flowLayoutPanel4";
            flowLayoutPanel4.Size = new Size(1219, 29);
            flowLayoutPanel4.TabIndex = 25;
            // 
            // _NO_TRANSLATE_UseRemotes
            // 
            _NO_TRANSLATE_UseRemotes.Location = new Point(2, 3);
            _NO_TRANSLATE_UseRemotes.Margin = new Padding(2, 3, 2, 3);
            _NO_TRANSLATE_UseRemotes.Name = "_NO_TRANSLATE_UseRemotes";
            _NO_TRANSLATE_UseRemotes.Size = new Size(218, 23);
            _NO_TRANSLATE_UseRemotes.TabIndex = 26;
            _NO_TRANSLATE_UseRemotes.Leave += _NO_TRANSLATE_UseRemotes_Leave;
            // 
            // chkOnlyFirstRemote
            // 
            chkOnlyFirstRemote.Anchor = AnchorStyles.Left;
            chkOnlyFirstRemote.AutoSize = true;
            chkOnlyFirstRemote.Location = new Point(225, 5);
            chkOnlyFirstRemote.Name = "chkOnlyFirstRemote";
            chkOnlyFirstRemote.Size = new Size(152, 19);
            chkOnlyFirstRemote.TabIndex = 27;
            chkOnlyFirstRemote.Text = "Only use the first match";
            chkOnlyFirstRemote.UseVisualStyleBackColor = true;
            chkOnlyFirstRemote.CheckedChanged += chkOnlyFirstRemote_CheckedChanged;
            // 
            // revisionDataGrp
            // 
            revisionDataGrp.AutoSize = true;
            revisionDataGrp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.SetColumnSpan(revisionDataGrp, 2);
            revisionDataGrp.Controls.Add(tableLayoutPanel4);
            revisionDataGrp.Dock = DockStyle.Fill;
            revisionDataGrp.Location = new Point(3, 155);
            revisionDataGrp.Name = "revisionDataGrp";
            revisionDataGrp.Size = new Size(1318, 111);
            revisionDataGrp.TabIndex = 27;
            revisionDataGrp.TabStop = false;
            revisionDataGrp.Text = "Revision data";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel4.BackColor = SystemColors.Control;
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(_NO_TRANSLATE_SearchPatternEdit, 1, 1);
            tableLayoutPanel4.Controls.Add(_NO_TRANSLATE_NestedPatternEdit, 1, 2);
            tableLayoutPanel4.Controls.Add(label2, 0, 1);
            tableLayoutPanel4.Controls.Add(label5, 0, 0);
            tableLayoutPanel4.Controls.Add(nestedPatternLab, 0, 2);
            tableLayoutPanel4.Controls.Add(flowLayoutPanel3, 1, 0);
            tableLayoutPanel4.Dock = DockStyle.Top;
            tableLayoutPanel4.Location = new Point(3, 19);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 3;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new Size(1312, 89);
            tableLayoutPanel4.TabIndex = 16;
            // 
            // _NO_TRANSLATE_SearchPatternEdit
            // 
            _NO_TRANSLATE_SearchPatternEdit.Dock = DockStyle.Fill;
            _NO_TRANSLATE_SearchPatternEdit.Location = new Point(91, 34);
            _NO_TRANSLATE_SearchPatternEdit.Margin = new Padding(2, 3, 2, 3);
            _NO_TRANSLATE_SearchPatternEdit.Name = "_NO_TRANSLATE_SearchPatternEdit";
            _NO_TRANSLATE_SearchPatternEdit.Size = new Size(1219, 23);
            _NO_TRANSLATE_SearchPatternEdit.TabIndex = 22;
            _NO_TRANSLATE_SearchPatternEdit.Leave += _NO_TRANSLATE_SearchPatternEdit_Leave;
            // 
            // _NO_TRANSLATE_NestedPatternEdit
            // 
            _NO_TRANSLATE_NestedPatternEdit.Dock = DockStyle.Fill;
            _NO_TRANSLATE_NestedPatternEdit.Location = new Point(91, 63);
            _NO_TRANSLATE_NestedPatternEdit.Margin = new Padding(2, 3, 2, 3);
            _NO_TRANSLATE_NestedPatternEdit.Name = "_NO_TRANSLATE_NestedPatternEdit";
            _NO_TRANSLATE_NestedPatternEdit.Size = new Size(1219, 23);
            _NO_TRANSLATE_NestedPatternEdit.TabIndex = 21;
            _NO_TRANSLATE_NestedPatternEdit.Leave += _NO_TRANSLATE_NestedPatternEdit_Leave;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.ImeMode = ImeMode.NoControl;
            label2.Location = new Point(2, 38);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(83, 15);
            label2.TabIndex = 19;
            label2.Text = "Search pattern";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.ImeMode = ImeMode.NoControl;
            label5.Location = new Point(2, 8);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(55, 15);
            label5.TabIndex = 15;
            label5.Text = "Search in";
            // 
            // nestedPatternLab
            // 
            nestedPatternLab.Anchor = AnchorStyles.Left;
            nestedPatternLab.AutoSize = true;
            nestedPatternLab.ImeMode = ImeMode.NoControl;
            nestedPatternLab.Location = new Point(2, 67);
            nestedPatternLab.Margin = new Padding(2, 0, 2, 0);
            nestedPatternLab.Name = "nestedPatternLab";
            nestedPatternLab.Size = new Size(85, 15);
            nestedPatternLab.TabIndex = 20;
            nestedPatternLab.Text = "Nested pattern";
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.AutoSize = true;
            flowLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel3.BackColor = SystemColors.Control;
            flowLayoutPanel3.Controls.Add(MessageChx);
            flowLayoutPanel3.Controls.Add(LocalBranchChx);
            flowLayoutPanel3.Controls.Add(RemoteBranchChx);
            flowLayoutPanel3.Location = new Point(92, 3);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(357, 25);
            flowLayoutPanel3.TabIndex = 23;
            // 
            // MessageChx
            // 
            MessageChx.AutoSize = true;
            MessageChx.Location = new Point(3, 3);
            MessageChx.Name = "MessageChx";
            MessageChx.Size = new Size(72, 19);
            MessageChx.TabIndex = 0;
            MessageChx.Text = "Message";
            MessageChx.UseVisualStyleBackColor = true;
            MessageChx.CheckedChanged += MessageChx_CheckedChanged;
            // 
            // LocalBranchChx
            // 
            LocalBranchChx.AutoSize = true;
            LocalBranchChx.Location = new Point(81, 3);
            LocalBranchChx.Name = "LocalBranchChx";
            LocalBranchChx.Size = new Size(127, 19);
            LocalBranchChx.TabIndex = 1;
            LocalBranchChx.Text = "Local branch name";
            LocalBranchChx.UseVisualStyleBackColor = true;
            LocalBranchChx.CheckedChanged += LocalBranchChx_CheckedChanged;
            // 
            // RemoteBranchChx
            // 
            RemoteBranchChx.AutoSize = true;
            RemoteBranchChx.Location = new Point(214, 3);
            RemoteBranchChx.Name = "RemoteBranchChx";
            RemoteBranchChx.Size = new Size(140, 19);
            RemoteBranchChx.TabIndex = 2;
            RemoteBranchChx.Text = "Remote branch name";
            RemoteBranchChx.UseVisualStyleBackColor = true;
            RemoteBranchChx.CheckedChanged += RemoteBranchChx_CheckedChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.ImeMode = ImeMode.NoControl;
            label1.Location = new Point(2, 7);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 7;
            label1.Text = "Name";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.BackColor = SystemColors.Control;
            flowLayoutPanel2.Controls.Add(_NO_TRANSLATE_Name);
            flowLayoutPanel2.Controls.Add(EnabledChx);
            flowLayoutPanel2.Controls.Add(gotoUserManualControl1);
            flowLayoutPanel2.Location = new Point(43, 0);
            flowLayoutPanel2.Margin = new Padding(0);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(426, 29);
            flowLayoutPanel2.TabIndex = 24;
            // 
            // _NO_TRANSLATE_Name
            // 
            _NO_TRANSLATE_Name.Location = new Point(2, 3);
            _NO_TRANSLATE_Name.Margin = new Padding(2, 3, 2, 3);
            _NO_TRANSLATE_Name.Name = "_NO_TRANSLATE_Name";
            _NO_TRANSLATE_Name.Size = new Size(272, 23);
            _NO_TRANSLATE_Name.TabIndex = 11;
            _NO_TRANSLATE_Name.Leave += _NO_TRANSLATE_Name_Leave;
            // 
            // EnabledChx
            // 
            EnabledChx.Anchor = AnchorStyles.Left;
            EnabledChx.AutoSize = true;
            EnabledChx.Location = new Point(279, 5);
            EnabledChx.Name = "EnabledChx";
            EnabledChx.Size = new Size(68, 19);
            EnabledChx.TabIndex = 22;
            EnabledChx.Text = "Enabled";
            EnabledChx.UseVisualStyleBackColor = true;
            EnabledChx.CheckedChanged += EnabledChx_CheckedChanged;
            // 
            // gotoUserManualControl1
            // 
            gotoUserManualControl1.AutoSize = true;
            gotoUserManualControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gotoUserManualControl1.Dock = DockStyle.Right;
            gotoUserManualControl1.Location = new Point(353, 3);
            gotoUserManualControl1.ManualSectionAnchorName = "revision-links";
            gotoUserManualControl1.ManualSectionSubfolder = "settings";
            gotoUserManualControl1.MinimumSize = new Size(70, 20);
            gotoUserManualControl1.Name = "gotoUserManualControl1";
            gotoUserManualControl1.Size = new Size(70, 23);
            gotoUserManualControl1.TabIndex = 26;
            // 
            // RevisionLinksSettingsPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = SystemColors.Control;
            Controls.Add(splitContainer1);
            Name = "RevisionLinksSettingsPage";
            Size = new Size(1520, 788);
            Text = "Revision links";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            toolStripManageCategories.ResumeLayout(false);
            toolStripManageCategories.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LinksGrid).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            detailPanel.ResumeLayout(false);
            detailPanel.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            remoteGrp.ResumeLayout(false);
            remoteGrp.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel4.ResumeLayout(false);
            flowLayoutPanel4.PerformLayout();
            revisionDataGrp.ResumeLayout(false);
            revisionDataGrp.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel1;
        private ListBox _NO_TRANSLATE_Categories;
        private Label CategoriesLabel;
        private Panel detailPanel;
        private TableLayoutPanel tableLayoutPanel3;
        private Panel panel1;
        private Label label6;
        private DataGridViewTextBoxColumn CaptionCol;
        private DataGridViewTextBoxColumn URICol;
        private DataGridView LinksGrid;
        private GroupBox revisionDataGrp;
        private TableLayoutPanel tableLayoutPanel4;
        private TextBox _NO_TRANSLATE_SearchPatternEdit;
        private Label label2;
        private Label label5;
        private Label nestedPatternLab;
        private FlowLayoutPanel flowLayoutPanel3;
        private CheckBox MessageChx;
        private CheckBox LocalBranchChx;
        private CheckBox RemoteBranchChx;
        private Label label1;
        private FlowLayoutPanel flowLayoutPanel2;
        private TextBox _NO_TRANSLATE_Name;
        private CheckBox EnabledChx;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private GroupBox remoteGrp;
        private TableLayoutPanel tableLayoutPanel2;
        private TextBox _NO_TRANSLATE_RemotePatern;
        private Label lblSearchRemotePattern;
        private Label lblRemoteSearchIn;
        private FlowLayoutPanel flowLayoutPanel1;
        private CheckBox chxURL;
        private CheckBox chxPushURL;
        private TextBox _NO_TRANSLATE_NestedPatternEdit;
        private Label lblUseRemotes;
        private FlowLayoutPanel flowLayoutPanel4;
        private TextBox _NO_TRANSLATE_UseRemotes;
        private CheckBox chkOnlyFirstRemote;
        private GitUI.ToolStripEx toolStripManageCategories;
        private ToolStripSplitButton Add;
        private ToolStripButton Remove;
        private ToolStripSeparator toolStripSeparator1;
    }
}
