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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Remove = new System.Windows.Forms.Button();
            this.Add = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Categories = new System.Windows.Forms.ListBox();
            this.CategoriesLabel = new System.Windows.Forms.Label();
            this.LinksGrid = new System.Windows.Forms.DataGridView();
            this.CaptionCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.URICol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.detailPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.remoteGrp = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblUseRemotes = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_RemotePatern = new System.Windows.Forms.TextBox();
            this.lblSearchRemotePattern = new System.Windows.Forms.Label();
            this.lblRemoteSearchIn = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chxURL = new System.Windows.Forms.CheckBox();
            this.chxPushURL = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this._NO_TRANSLATE_UseRemotes = new System.Windows.Forms.TextBox();
            this.chkOnlyFirstRemote = new System.Windows.Forms.CheckBox();
            this.revisionDataGrp = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_SearchPatternEdit = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_NestedPatternEdit = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.nestedPatternLab = new System.Windows.Forms.Label();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.MessageChx = new System.Windows.Forms.CheckBox();
            this.LocalBranchChx = new System.Windows.Forms.CheckBox();
            this.RemoteBranchChx = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this._NO_TRANSLATE_Name = new System.Windows.Forms.TextBox();
            this.EnabledChx = new System.Windows.Forms.CheckBox();
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
            this.panel1.SuspendLayout();
            this.detailPanel.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.remoteGrp.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.revisionDataGrp.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_Categories);
            this.splitContainer1.Panel1.Controls.Add(this.CategoriesLabel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LinksGrid);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.detailPanel);
            this.splitContainer1.Size = new System.Drawing.Size(974, 452);
            this.splitContainer1.SplitterDistance = 192;
            this.splitContainer1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.Remove, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Add, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 423);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(192, 29);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.AutoSize = true;
            this.Remove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Remove.Location = new System.Drawing.Point(134, 3);
            this.Remove.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(56, 23);
            this.Remove.TabIndex = 4;
            this.Remove.Text = "Remove";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // Add
            // 
            this.Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Add.AutoSize = true;
            this.Add.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Add.BackColor = System.Drawing.SystemColors.Control;
            this.Add.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Add.Location = new System.Drawing.Point(2, 3);
            this.Add.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(36, 23);
            this.Add.TabIndex = 3;
            this.Add.Text = "Add";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // _NO_TRANSLATE_Categories
            // 
            this._NO_TRANSLATE_Categories.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_Categories.FormattingEnabled = true;
            this._NO_TRANSLATE_Categories.IntegralHeight = false;
            this._NO_TRANSLATE_Categories.Location = new System.Drawing.Point(0, 22);
            this._NO_TRANSLATE_Categories.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_Categories.Name = "_NO_TRANSLATE_Categories";
            this._NO_TRANSLATE_Categories.Size = new System.Drawing.Size(192, 430);
            this._NO_TRANSLATE_Categories.TabIndex = 1;
            this._NO_TRANSLATE_Categories.SelectedIndexChanged += new System.EventHandler(this._NO_TRANSLATE_Categories_SelectedIndexChanged);
            // 
            // CategoriesLabel
            // 
            this.CategoriesLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.CategoriesLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CategoriesLabel.Location = new System.Drawing.Point(0, 0);
            this.CategoriesLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CategoriesLabel.Name = "CategoriesLabel";
            this.CategoriesLabel.Size = new System.Drawing.Size(192, 22);
            this.CategoriesLabel.TabIndex = 0;
            this.CategoriesLabel.Text = "Categories";
            // 
            // LinksGrid
            // 
            this.LinksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.LinksGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CaptionCol,
            this.URICol});
            this.LinksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinksGrid.Location = new System.Drawing.Point(0, 275);
            this.LinksGrid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.LinksGrid.MultiSelect = false;
            this.LinksGrid.Name = "LinksGrid";
            this.LinksGrid.RowHeadersVisible = false;
            this.LinksGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.LinksGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.LinksGrid.Size = new System.Drawing.Size(778, 177);
            this.LinksGrid.TabIndex = 8;
            // 
            // CaptionCol
            // 
            this.CaptionCol.HeaderText = "Caption";
            this.CaptionCol.Name = "CaptionCol";
            // 
            // URICol
            // 
            this.URICol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.URICol.HeaderText = "URI";
            this.URICol.Name = "URICol";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.label6);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 260);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.panel1.Size = new System.Drawing.Size(778, 15);
            this.panel1.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(206, -1);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Links";
            // 
            // detailPanel
            // 
            this.detailPanel.AutoSize = true;
            this.detailPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.detailPanel.BackColor = System.Drawing.SystemColors.Control;
            this.detailPanel.Controls.Add(this.tableLayoutPanel3);
            this.detailPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailPanel.Location = new System.Drawing.Point(0, 0);
            this.detailPanel.Name = "detailPanel";
            this.detailPanel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 6);
            this.detailPanel.Size = new System.Drawing.Size(778, 260);
            this.detailPanel.TabIndex = 6;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.remoteGrp, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.revisionDataGrp, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(778, 251);
            this.tableLayoutPanel3.TabIndex = 14;
            // 
            // remoteGrp
            // 
            this.remoteGrp.AutoSize = true;
            this.remoteGrp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.SetColumnSpan(this.remoteGrp, 2);
            this.remoteGrp.Controls.Add(this.tableLayoutPanel2);
            this.remoteGrp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteGrp.Location = new System.Drawing.Point(3, 30);
            this.remoteGrp.Name = "remoteGrp";
            this.remoteGrp.Size = new System.Drawing.Size(772, 109);
            this.remoteGrp.TabIndex = 28;
            this.remoteGrp.TabStop = false;
            this.remoteGrp.Text = "Remote data";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblUseRemotes, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._NO_TRANSLATE_RemotePatern, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblSearchRemotePattern, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblRemoteSearchIn, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel4, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(766, 89);
            this.tableLayoutPanel2.TabIndex = 17;
            // 
            // lblUseRemotes
            // 
            this.lblUseRemotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblUseRemotes.AutoSize = true;
            this.lblUseRemotes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblUseRemotes.Location = new System.Drawing.Point(2, 10);
            this.lblUseRemotes.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblUseRemotes.Name = "lblUseRemotes";
            this.lblUseRemotes.Size = new System.Drawing.Size(67, 13);
            this.lblUseRemotes.TabIndex = 24;
            this.lblUseRemotes.Text = "Use remotes";
            // 
            // _NO_TRANSLATE_RemotePatern
            // 
            this._NO_TRANSLATE_RemotePatern.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_RemotePatern.Location = new System.Drawing.Point(85, 65);
            this._NO_TRANSLATE_RemotePatern.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_RemotePatern.Name = "_NO_TRANSLATE_RemotePatern";
            this._NO_TRANSLATE_RemotePatern.Size = new System.Drawing.Size(679, 21);
            this._NO_TRANSLATE_RemotePatern.TabIndex = 22;
            this._NO_TRANSLATE_RemotePatern.Leave += new System.EventHandler(this._NO_TRANSLATE_RemotePatern_Leave);
            // 
            // lblSearchRemotePattern
            // 
            this.lblSearchRemotePattern.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSearchRemotePattern.AutoSize = true;
            this.lblSearchRemotePattern.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblSearchRemotePattern.Location = new System.Drawing.Point(2, 69);
            this.lblSearchRemotePattern.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSearchRemotePattern.Name = "lblSearchRemotePattern";
            this.lblSearchRemotePattern.Size = new System.Drawing.Size(79, 13);
            this.lblSearchRemotePattern.TabIndex = 19;
            this.lblSearchRemotePattern.Text = "Search pattern";
            // 
            // lblRemoteSearchIn
            // 
            this.lblRemoteSearchIn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRemoteSearchIn.AutoSize = true;
            this.lblRemoteSearchIn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblRemoteSearchIn.Location = new System.Drawing.Point(2, 41);
            this.lblRemoteSearchIn.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRemoteSearchIn.Name = "lblRemoteSearchIn";
            this.lblRemoteSearchIn.Size = new System.Drawing.Size(51, 13);
            this.lblRemoteSearchIn.TabIndex = 15;
            this.lblRemoteSearchIn.Text = "Search in";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.chxURL);
            this.flowLayoutPanel1.Controls.Add(this.chxPushURL);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(86, 36);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(128, 23);
            this.flowLayoutPanel1.TabIndex = 23;
            // 
            // chxURL
            // 
            this.chxURL.AutoSize = true;
            this.chxURL.Location = new System.Drawing.Point(3, 3);
            this.chxURL.Name = "chxURL";
            this.chxURL.Size = new System.Drawing.Size(45, 17);
            this.chxURL.TabIndex = 0;
            this.chxURL.Text = "URL";
            this.chxURL.UseVisualStyleBackColor = true;
            this.chxURL.CheckedChanged += new System.EventHandler(this.chxURL_CheckedChanged);
            // 
            // chxPushURL
            // 
            this.chxPushURL.AutoSize = true;
            this.chxPushURL.Location = new System.Drawing.Point(54, 3);
            this.chxPushURL.Name = "chxPushURL";
            this.chxPushURL.Size = new System.Drawing.Size(71, 17);
            this.chxPushURL.TabIndex = 2;
            this.chxPushURL.Text = "Push URL";
            this.chxPushURL.UseVisualStyleBackColor = true;
            this.chxPushURL.CheckedChanged += new System.EventHandler(this.chxPushURL_CheckedChanged);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.AutoSize = true;
            this.flowLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel4.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanel4.Controls.Add(this._NO_TRANSLATE_UseRemotes);
            this.flowLayoutPanel4.Controls.Add(this.chkOnlyFirstRemote);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(86, 3);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(677, 27);
            this.flowLayoutPanel4.TabIndex = 25;
            // 
            // _NO_TRANSLATE_UseRemotes
            // 
            this._NO_TRANSLATE_UseRemotes.Location = new System.Drawing.Point(2, 3);
            this._NO_TRANSLATE_UseRemotes.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_UseRemotes.Name = "_NO_TRANSLATE_UseRemotes";
            this._NO_TRANSLATE_UseRemotes.Size = new System.Drawing.Size(218, 21);
            this._NO_TRANSLATE_UseRemotes.TabIndex = 26;
            this._NO_TRANSLATE_UseRemotes.Leave += new System.EventHandler(this._NO_TRANSLATE_UseRemotes_Leave);
            // 
            // chkOnlyFirstRemote
            // 
            this.chkOnlyFirstRemote.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkOnlyFirstRemote.AutoSize = true;
            this.chkOnlyFirstRemote.Location = new System.Drawing.Point(225, 5);
            this.chkOnlyFirstRemote.Name = "chkOnlyFirstRemote";
            this.chkOnlyFirstRemote.Size = new System.Drawing.Size(141, 17);
            this.chkOnlyFirstRemote.TabIndex = 27;
            this.chkOnlyFirstRemote.Text = "Only use the first match";
            this.chkOnlyFirstRemote.UseVisualStyleBackColor = true;
            this.chkOnlyFirstRemote.CheckedChanged += new System.EventHandler(this.chkOnlyFirstRemote_CheckedChanged);
            // 
            // revisionDataGrp
            // 
            this.revisionDataGrp.AutoSize = true;
            this.revisionDataGrp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.SetColumnSpan(this.revisionDataGrp, 2);
            this.revisionDataGrp.Controls.Add(this.tableLayoutPanel4);
            this.revisionDataGrp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.revisionDataGrp.Location = new System.Drawing.Point(3, 145);
            this.revisionDataGrp.Name = "revisionDataGrp";
            this.revisionDataGrp.Size = new System.Drawing.Size(772, 103);
            this.revisionDataGrp.TabIndex = 27;
            this.revisionDataGrp.TabStop = false;
            this.revisionDataGrp.Text = "Revision data";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this._NO_TRANSLATE_SearchPatternEdit, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this._NO_TRANSLATE_NestedPatternEdit, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.nestedPatternLab, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.flowLayoutPanel3, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(766, 83);
            this.tableLayoutPanel4.TabIndex = 16;
            // 
            // _NO_TRANSLATE_SearchPatternEdit
            // 
            this._NO_TRANSLATE_SearchPatternEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_SearchPatternEdit.Location = new System.Drawing.Point(86, 32);
            this._NO_TRANSLATE_SearchPatternEdit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_SearchPatternEdit.Name = "_NO_TRANSLATE_SearchPatternEdit";
            this._NO_TRANSLATE_SearchPatternEdit.Size = new System.Drawing.Size(678, 21);
            this._NO_TRANSLATE_SearchPatternEdit.TabIndex = 22;
            this._NO_TRANSLATE_SearchPatternEdit.Leave += new System.EventHandler(this._NO_TRANSLATE_SearchPatternEdit_Leave);
            // 
            // _NO_TRANSLATE_NestedPatternEdit
            // 
            this._NO_TRANSLATE_NestedPatternEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_NestedPatternEdit.Location = new System.Drawing.Point(86, 59);
            this._NO_TRANSLATE_NestedPatternEdit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_NestedPatternEdit.Name = "_NO_TRANSLATE_NestedPatternEdit";
            this._NO_TRANSLATE_NestedPatternEdit.Size = new System.Drawing.Size(678, 21);
            this._NO_TRANSLATE_NestedPatternEdit.TabIndex = 21;
            this._NO_TRANSLATE_NestedPatternEdit.Leave += new System.EventHandler(this._NO_TRANSLATE_NestedPatternEdit_Leave);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(2, 36);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Search pattern";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(2, 8);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Search in";
            // 
            // nestedPatternLab
            // 
            this.nestedPatternLab.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nestedPatternLab.AutoSize = true;
            this.nestedPatternLab.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.nestedPatternLab.Location = new System.Drawing.Point(2, 63);
            this.nestedPatternLab.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nestedPatternLab.Name = "nestedPatternLab";
            this.nestedPatternLab.Size = new System.Drawing.Size(80, 13);
            this.nestedPatternLab.TabIndex = 20;
            this.nestedPatternLab.Text = "Nested pattern";
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanel3.Controls.Add(this.MessageChx);
            this.flowLayoutPanel3.Controls.Add(this.LocalBranchChx);
            this.flowLayoutPanel3.Controls.Add(this.RemoteBranchChx);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(87, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(329, 23);
            this.flowLayoutPanel3.TabIndex = 23;
            // 
            // MessageChx
            // 
            this.MessageChx.AutoSize = true;
            this.MessageChx.Location = new System.Drawing.Point(3, 3);
            this.MessageChx.Name = "MessageChx";
            this.MessageChx.Size = new System.Drawing.Size(68, 17);
            this.MessageChx.TabIndex = 0;
            this.MessageChx.Text = "Message";
            this.MessageChx.UseVisualStyleBackColor = true;
            this.MessageChx.CheckedChanged += new System.EventHandler(this.MessageChx_CheckedChanged);
            // 
            // LocalBranchChx
            // 
            this.LocalBranchChx.AutoSize = true;
            this.LocalBranchChx.Location = new System.Drawing.Point(77, 3);
            this.LocalBranchChx.Name = "LocalBranchChx";
            this.LocalBranchChx.Size = new System.Drawing.Size(115, 17);
            this.LocalBranchChx.TabIndex = 1;
            this.LocalBranchChx.Text = "Local branch name";
            this.LocalBranchChx.UseVisualStyleBackColor = true;
            this.LocalBranchChx.CheckedChanged += new System.EventHandler(this.LocalBranchChx_CheckedChanged);
            // 
            // RemoteBranchChx
            // 
            this.RemoteBranchChx.AutoSize = true;
            this.RemoteBranchChx.Location = new System.Drawing.Point(198, 3);
            this.RemoteBranchChx.Name = "RemoteBranchChx";
            this.RemoteBranchChx.Size = new System.Drawing.Size(128, 17);
            this.RemoteBranchChx.TabIndex = 2;
            this.RemoteBranchChx.Text = "Remote branch name";
            this.RemoteBranchChx.UseVisualStyleBackColor = true;
            this.RemoteBranchChx.CheckedChanged += new System.EventHandler(this.RemoteBranchChx_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(2, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Name";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanel2.Controls.Add(this._NO_TRANSLATE_Name);
            this.flowLayoutPanel2.Controls.Add(this.EnabledChx);
            this.flowLayoutPanel2.Controls.Add(this.gotoUserManualControl1);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(38, 0);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(422, 27);
            this.flowLayoutPanel2.TabIndex = 24;
            // 
            // _NO_TRANSLATE_Name
            // 
            this._NO_TRANSLATE_Name.Location = new System.Drawing.Point(2, 3);
            this._NO_TRANSLATE_Name.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_Name.Name = "_NO_TRANSLATE_Name";
            this._NO_TRANSLATE_Name.Size = new System.Drawing.Size(272, 21);
            this._NO_TRANSLATE_Name.TabIndex = 11;
            this._NO_TRANSLATE_Name.Leave += new System.EventHandler(this._NO_TRANSLATE_Name_Leave);
            // 
            // EnabledChx
            // 
            this.EnabledChx.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.EnabledChx.AutoSize = true;
            this.EnabledChx.Location = new System.Drawing.Point(279, 5);
            this.EnabledChx.Name = "EnabledChx";
            this.EnabledChx.Size = new System.Drawing.Size(64, 17);
            this.EnabledChx.TabIndex = 22;
            this.EnabledChx.Text = "Enabled";
            this.EnabledChx.UseVisualStyleBackColor = true;
            this.EnabledChx.CheckedChanged += new System.EventHandler(this.EnabledChx_CheckedChanged);
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.AutoSize = true;
            this.gotoUserManualControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gotoUserManualControl1.Dock = System.Windows.Forms.DockStyle.Right;
            this.gotoUserManualControl1.Location = new System.Drawing.Point(349, 3);
            this.gotoUserManualControl1.ManualSectionAnchorName = "git-extensions-revision-links";
            this.gotoUserManualControl1.ManualSectionSubfolder = "settings";
            this.gotoUserManualControl1.MinimumSize = new System.Drawing.Size(70, 20);
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(70, 21);
            this.gotoUserManualControl1.TabIndex = 26;
            // 
            // RevisionLinksSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.splitContainer1);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Name = "RevisionLinksSettingsPage";
            this.Size = new System.Drawing.Size(974, 452);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.detailPanel.ResumeLayout(false);
            this.detailPanel.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.remoteGrp.ResumeLayout(false);
            this.remoteGrp.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.revisionDataGrp.ResumeLayout(false);
            this.revisionDataGrp.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.ListBox _NO_TRANSLATE_Categories;
        private System.Windows.Forms.Label CategoriesLabel;
        private System.Windows.Forms.Panel detailPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridViewTextBoxColumn CaptionCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn URICol;
        private System.Windows.Forms.DataGridView LinksGrid;
        private System.Windows.Forms.GroupBox revisionDataGrp;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_SearchPatternEdit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label nestedPatternLab;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.CheckBox MessageChx;
        private System.Windows.Forms.CheckBox LocalBranchChx;
        private System.Windows.Forms.CheckBox RemoteBranchChx;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Name;
        private System.Windows.Forms.CheckBox EnabledChx;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
        private System.Windows.Forms.GroupBox remoteGrp;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_RemotePatern;
        private System.Windows.Forms.Label lblSearchRemotePattern;
        private System.Windows.Forms.Label lblRemoteSearchIn;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox chxURL;
        private System.Windows.Forms.CheckBox chxPushURL;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_NestedPatternEdit;
        private System.Windows.Forms.Label lblUseRemotes;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_UseRemotes;
        private System.Windows.Forms.CheckBox chkOnlyFirstRemote;
    }
}
