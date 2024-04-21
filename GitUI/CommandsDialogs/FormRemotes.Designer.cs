
namespace GitUI.CommandsDialogs
{
    partial class FormRemotes
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
            ListViewGroup listViewGroup2 = new ListViewGroup("ListViewGroup", HorizontalAlignment.Left);
            flpnlRemoteManagement = new TableLayoutPanel();
            pnlMgtDetails = new Panel();
            tblpnlMgtDetails = new TableLayoutPanel();
            label1 = new Label();
            RemoteName = new TextBox();
            label2 = new Label();
            Url = new UserControls.CaseSensitiveComboBox();
            folderBrowserButtonUrl = new UserControls.FolderBrowserButton();
            checkBoxSepPushUrl = new CheckBox();
            labelPushUrl = new Label();
            comboBoxPushUrl = new UserControls.CaseSensitiveComboBox();
            folderBrowserButtonPushUrl = new UserControls.FolderBrowserButton();
            pnlMgtPuttySsh = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            label3 = new Label();
            PuttySshKey = new TextBox();
            SshBrowse = new Button();
            flowLayoutPanel3 = new FlowLayoutPanel();
            TestConnection = new Button();
            LoadSSHKey = new Button();
            lblMgtPuttyPanelHeader = new Label();
            lblHeaderLine2 = new Label();
            flowLayoutPanel2 = new FlowLayoutPanel();
            Save = new Button();
            pnlManagementContainer = new Panel();
            gbMgtPanel = new GroupBox();
            panelButtons = new FlowLayoutPanel();
            New = new Button();
            Delete = new Button();
            btnToggleState = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            panel1 = new Panel();
            Remotes = new UserControls.NativeListView();
            columnHeader1 = new ColumnHeader();
            tabPage2 = new TabPage();
            tableLayoutPanel2 = new TableLayoutPanel();
            panelDetails = new Panel();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            DefaultMergeWithCombo = new ComboBox();
            RemoteRepositoryCombo = new ComboBox();
            LocalBranchNameEdit = new TextBox();
            SaveDefaultPushPull = new Button();
            RemoteBranches = new DataGridView();
            BranchName = new DataGridViewTextBoxColumn();
            RemoteCombo = new DataGridViewTextBoxColumn();
            MergeWith = new DataGridViewTextBoxColumn();
            toolTip1 = new ToolTip(components);
            flpnlRemoteManagement.SuspendLayout();
            pnlMgtDetails.SuspendLayout();
            tblpnlMgtDetails.SuspendLayout();
            pnlMgtPuttySsh.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            pnlManagementContainer.SuspendLayout();
            gbMgtPanel.SuspendLayout();
            panelButtons.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            panel1.SuspendLayout();
            tabPage2.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panelDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RemoteBranches).BeginInit();
            SuspendLayout();
            // 
            // flpnlRemoteManagement
            // 
            flpnlRemoteManagement.AutoSize = true;
            flpnlRemoteManagement.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpnlRemoteManagement.ColumnCount = 1;
            flpnlRemoteManagement.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            flpnlRemoteManagement.Controls.Add(pnlMgtDetails, 0, 0);
            flpnlRemoteManagement.Controls.Add(pnlMgtPuttySsh, 0, 1);
            flpnlRemoteManagement.Controls.Add(flowLayoutPanel2, 0, 2);
            flpnlRemoteManagement.Dock = DockStyle.Fill;
            flpnlRemoteManagement.Location = new Point(3, 19);
            flpnlRemoteManagement.Name = "flpnlRemoteManagement";
            flpnlRemoteManagement.RowCount = 3;
            flpnlRemoteManagement.RowStyles.Add(new RowStyle());
            flpnlRemoteManagement.RowStyles.Add(new RowStyle());
            flpnlRemoteManagement.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            flpnlRemoteManagement.Size = new Size(514, 278);
            flpnlRemoteManagement.TabIndex = 0;
            // 
            // pnlMgtDetails
            // 
            pnlMgtDetails.AutoSize = true;
            pnlMgtDetails.Controls.Add(tblpnlMgtDetails);
            pnlMgtDetails.Dock = DockStyle.Top;
            pnlMgtDetails.Location = new Point(3, 3);
            pnlMgtDetails.Name = "pnlMgtDetails";
            pnlMgtDetails.Padding = new Padding(0, 0, 0, 8);
            pnlMgtDetails.Size = new Size(508, 138);
            pnlMgtDetails.TabIndex = 0;
            pnlMgtDetails.Text = "Details";
            // 
            // tblpnlMgtDetails
            // 
            tblpnlMgtDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tblpnlMgtDetails.AutoSize = true;
            tblpnlMgtDetails.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tblpnlMgtDetails.ColumnCount = 3;
            tblpnlMgtDetails.ColumnStyles.Add(new ColumnStyle());
            tblpnlMgtDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblpnlMgtDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tblpnlMgtDetails.Controls.Add(label1, 0, 0);
            tblpnlMgtDetails.Controls.Add(RemoteName, 1, 0);
            tblpnlMgtDetails.Controls.Add(label2, 0, 1);
            tblpnlMgtDetails.Controls.Add(Url, 1, 1);
            tblpnlMgtDetails.Controls.Add(folderBrowserButtonUrl, 2, 1);
            tblpnlMgtDetails.Controls.Add(checkBoxSepPushUrl, 0, 2);
            tblpnlMgtDetails.Controls.Add(labelPushUrl, 0, 3);
            tblpnlMgtDetails.Controls.Add(comboBoxPushUrl, 1, 3);
            tblpnlMgtDetails.Controls.Add(folderBrowserButtonPushUrl, 2, 3);
            tblpnlMgtDetails.Location = new Point(16, 11);
            tblpnlMgtDetails.Name = "tblpnlMgtDetails";
            tblpnlMgtDetails.RowCount = 4;
            tblpnlMgtDetails.RowStyles.Add(new RowStyle());
            tblpnlMgtDetails.RowStyles.Add(new RowStyle());
            tblpnlMgtDetails.RowStyles.Add(new RowStyle());
            tblpnlMgtDetails.RowStyles.Add(new RowStyle());
            tblpnlMgtDetails.Size = new Size(470, 116);
            tblpnlMgtDetails.TabIndex = 11;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.MinimumSize = new Size(100, 0);
            label1.Name = "label1";
            label1.Size = new Size(100, 29);
            label1.TabIndex = 0;
            label1.Text = "Name";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // RemoteName
            // 
            RemoteName.Dock = DockStyle.Fill;
            RemoteName.Location = new Point(109, 3);
            RemoteName.Name = "RemoteName";
            RemoteName.Size = new Size(248, 23);
            RemoteName.TabIndex = 1;
            RemoteName.TextChanged += RemoteName_TextChanged;
            RemoteName.Enter += RemoteName_Enter;
            // 
            // label2
            // 
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 29);
            label2.Name = "label2";
            label2.Size = new Size(100, 31);
            label2.TabIndex = 2;
            label2.Text = "Url";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Url
            // 
            Url.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Url.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Url.AutoCompleteSource = AutoCompleteSource.ListItems;
            Url.FormattingEnabled = true;
            Url.Location = new Point(109, 33);
            Url.Name = "Url";
            Url.Size = new Size(248, 23);
            Url.TabIndex = 3;
            Url.Enter += Url_Enter;
            // 
            // folderBrowserButtonUrl
            // 
            folderBrowserButtonUrl.AutoSize = true;
            folderBrowserButtonUrl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            folderBrowserButtonUrl.Dock = DockStyle.Fill;
            folderBrowserButtonUrl.Location = new Point(363, 32);
            folderBrowserButtonUrl.MinimumSize = new Size(104, 25);
            folderBrowserButtonUrl.Name = "folderBrowserButtonUrl";
            folderBrowserButtonUrl.PathShowingControl = Url;
            folderBrowserButtonUrl.Size = new Size(104, 25);
            folderBrowserButtonUrl.TabIndex = 4;
            // 
            // checkBoxSepPushUrl
            // 
            checkBoxSepPushUrl.AutoSize = true;
            tblpnlMgtDetails.SetColumnSpan(checkBoxSepPushUrl, 2);
            checkBoxSepPushUrl.Dock = DockStyle.Fill;
            checkBoxSepPushUrl.Location = new Point(3, 63);
            checkBoxSepPushUrl.Name = "checkBoxSepPushUrl";
            checkBoxSepPushUrl.Padding = new Padding(24, 0, 0, 0);
            checkBoxSepPushUrl.Size = new Size(354, 19);
            checkBoxSepPushUrl.TabIndex = 5;
            checkBoxSepPushUrl.Text = "Separate Push Url";
            checkBoxSepPushUrl.UseVisualStyleBackColor = true;
            checkBoxSepPushUrl.CheckedChanged += checkBoxSepPushUrl_CheckedChanged;
            // 
            // labelPushUrl
            // 
            labelPushUrl.Dock = DockStyle.Fill;
            labelPushUrl.Location = new Point(3, 85);
            labelPushUrl.Name = "labelPushUrl";
            labelPushUrl.Size = new Size(100, 31);
            labelPushUrl.TabIndex = 6;
            labelPushUrl.Text = "Push Url";
            labelPushUrl.TextAlign = ContentAlignment.MiddleLeft;
            labelPushUrl.Visible = false;
            // 
            // comboBoxPushUrl
            // 
            comboBoxPushUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            comboBoxPushUrl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxPushUrl.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxPushUrl.FormattingEnabled = true;
            comboBoxPushUrl.Location = new Point(109, 89);
            comboBoxPushUrl.Name = "comboBoxPushUrl";
            comboBoxPushUrl.Size = new Size(248, 23);
            comboBoxPushUrl.TabIndex = 7;
            comboBoxPushUrl.Visible = false;
            comboBoxPushUrl.Enter += ComboBoxPushUrl_Enter;
            // 
            // folderBrowserButtonPushUrl
            // 
            folderBrowserButtonPushUrl.AutoSize = true;
            folderBrowserButtonPushUrl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            folderBrowserButtonPushUrl.Dock = DockStyle.Fill;
            folderBrowserButtonPushUrl.Location = new Point(363, 88);
            folderBrowserButtonPushUrl.MinimumSize = new Size(104, 25);
            folderBrowserButtonPushUrl.Name = "folderBrowserButtonPushUrl";
            folderBrowserButtonPushUrl.PathShowingControl = comboBoxPushUrl;
            folderBrowserButtonPushUrl.Size = new Size(104, 25);
            folderBrowserButtonPushUrl.TabIndex = 8;
            folderBrowserButtonPushUrl.Visible = false;
            // 
            // pnlMgtPuttySsh
            // 
            pnlMgtPuttySsh.AutoSize = true;
            pnlMgtPuttySsh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlMgtPuttySsh.Controls.Add(tableLayoutPanel1);
            pnlMgtPuttySsh.Controls.Add(lblMgtPuttyPanelHeader);
            pnlMgtPuttySsh.Controls.Add(lblHeaderLine2);
            pnlMgtPuttySsh.Dock = DockStyle.Top;
            pnlMgtPuttySsh.Location = new Point(3, 147);
            pnlMgtPuttySsh.Name = "pnlMgtPuttySsh";
            pnlMgtPuttySsh.Padding = new Padding(0, 0, 0, 8);
            pnlMgtPuttySsh.Size = new Size(508, 91);
            pnlMgtPuttySsh.TabIndex = 1;
            pnlMgtPuttySsh.Text = "PuTTY SSH";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label3, 0, 0);
            tableLayoutPanel1.Controls.Add(PuttySshKey, 1, 0);
            tableLayoutPanel1.Controls.Add(SshBrowse, 2, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel3, 1, 1);
            tableLayoutPanel1.Location = new Point(16, 18);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(470, 62);
            tableLayoutPanel1.TabIndex = 12;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(3, 0);
            label3.MinimumSize = new Size(100, 0);
            label3.Name = "label3";
            label3.Size = new Size(100, 31);
            label3.TabIndex = 0;
            label3.Text = "Private key file";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PuttySshKey
            // 
            PuttySshKey.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            PuttySshKey.Location = new Point(109, 4);
            PuttySshKey.Name = "PuttySshKey";
            PuttySshKey.Size = new Size(248, 23);
            PuttySshKey.TabIndex = 1;
            // 
            // SshBrowse
            // 
            SshBrowse.AutoSize = true;
            SshBrowse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SshBrowse.Dock = DockStyle.Fill;
            SshBrowse.Image = Properties.Images.FileNew;
            SshBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            SshBrowse.Location = new Point(363, 3);
            SshBrowse.MinimumSize = new Size(104, 25);
            SshBrowse.Name = "SshBrowse";
            SshBrowse.Size = new Size(104, 25);
            SshBrowse.TabIndex = 2;
            SshBrowse.Text = " Browse...";
            SshBrowse.TextImageRelation = TextImageRelation.ImageBeforeText;
            SshBrowse.UseVisualStyleBackColor = true;
            SshBrowse.Click += SshBrowseClick;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.AutoSize = true;
            flowLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(flowLayoutPanel3, 2);
            flowLayoutPanel3.Controls.Add(TestConnection);
            flowLayoutPanel3.Controls.Add(LoadSSHKey);
            flowLayoutPanel3.Dock = DockStyle.Fill;
            flowLayoutPanel3.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel3.Location = new Point(106, 31);
            flowLayoutPanel3.Margin = new Padding(0);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(364, 31);
            flowLayoutPanel3.TabIndex = 3;
            // 
            // TestConnection
            // 
            TestConnection.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TestConnection.AutoSize = true;
            TestConnection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TestConnection.Image = Properties.Images.Putty;
            TestConnection.ImageAlign = ContentAlignment.MiddleLeft;
            TestConnection.Location = new Point(201, 3);
            TestConnection.MinimumSize = new Size(160, 25);
            TestConnection.Name = "TestConnection";
            TestConnection.Size = new Size(160, 25);
            TestConnection.TabIndex = 1;
            TestConnection.Text = "Test connection";
            TestConnection.UseVisualStyleBackColor = true;
            TestConnection.Click += TestConnectionClick;
            // 
            // LoadSSHKey
            // 
            LoadSSHKey.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LoadSSHKey.AutoSize = true;
            LoadSSHKey.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            LoadSSHKey.Image = Properties.Images.Putty;
            LoadSSHKey.ImageAlign = ContentAlignment.MiddleLeft;
            LoadSSHKey.Location = new Point(35, 3);
            LoadSSHKey.MinimumSize = new Size(160, 25);
            LoadSSHKey.Name = "LoadSSHKey";
            LoadSSHKey.Size = new Size(160, 25);
            LoadSSHKey.TabIndex = 0;
            LoadSSHKey.Text = "Load SSH key";
            LoadSSHKey.UseVisualStyleBackColor = true;
            LoadSSHKey.Click += LoadSshKeyClick;
            // 
            // lblMgtPuttyPanelHeader
            // 
            lblMgtPuttyPanelHeader.AutoSize = true;
            lblMgtPuttyPanelHeader.Location = new Point(8, 0);
            lblMgtPuttyPanelHeader.Name = "lblMgtPuttyPanelHeader";
            lblMgtPuttyPanelHeader.Size = new Size(64, 15);
            lblMgtPuttyPanelHeader.TabIndex = 0;
            lblMgtPuttyPanelHeader.Text = "PuTTY SSH";
            // 
            // lblHeaderLine2
            // 
            lblHeaderLine2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblHeaderLine2.BorderStyle = BorderStyle.Fixed3D;
            lblHeaderLine2.Location = new Point(16, 8);
            lblHeaderLine2.Name = "lblHeaderLine2";
            lblHeaderLine2.Size = new Size(481, 3);
            lblHeaderLine2.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.Controls.Add(Save);
            flowLayoutPanel2.Dock = DockStyle.Top;
            flowLayoutPanel2.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new Point(3, 244);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Padding = new Padding(0, 0, 20, 0);
            flowLayoutPanel2.Size = new Size(508, 31);
            flowLayoutPanel2.TabIndex = 2;
            // 
            // Save
            // 
            Save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Save.Image = Properties.Images.Save;
            Save.ImageAlign = ContentAlignment.MiddleLeft;
            Save.Location = new Point(355, 3);
            Save.Name = "Save";
            Save.Size = new Size(130, 25);
            Save.TabIndex = 0;
            Save.Text = "Save changes";
            Save.UseVisualStyleBackColor = true;
            Save.Click += SaveClick;
            // 
            // pnlManagementContainer
            // 
            pnlManagementContainer.Controls.Add(gbMgtPanel);
            pnlManagementContainer.Dock = DockStyle.Fill;
            pnlManagementContainer.Location = new Point(197, 3);
            pnlManagementContainer.Name = "pnlManagementContainer";
            pnlManagementContainer.Padding = new Padding(8, 4, 8, 8);
            pnlManagementContainer.Size = new Size(536, 297);
            pnlManagementContainer.TabIndex = 0;
            // 
            // gbMgtPanel
            // 
            gbMgtPanel.AutoSize = true;
            gbMgtPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbMgtPanel.Controls.Add(flpnlRemoteManagement);
            gbMgtPanel.Dock = DockStyle.Top;
            gbMgtPanel.Location = new Point(8, 4);
            gbMgtPanel.Name = "gbMgtPanel";
            gbMgtPanel.Size = new Size(520, 300);
            gbMgtPanel.TabIndex = 0;
            gbMgtPanel.TabStop = false;
            gbMgtPanel.Text = "Create New Remote";
            // 
            // panelButtons
            // 
            panelButtons.AutoSize = true;
            panelButtons.Controls.Add(New);
            panelButtons.Controls.Add(Delete);
            panelButtons.Controls.Add(btnToggleState);
            panelButtons.Dock = DockStyle.Right;
            panelButtons.FlowDirection = FlowDirection.TopDown;
            panelButtons.Location = new Point(150, 8);
            panelButtons.Margin = new Padding(8);
            panelButtons.Name = "panelButtons";
            panelButtons.Padding = new Padding(4, 0, 0, 0);
            panelButtons.Size = new Size(36, 281);
            panelButtons.TabIndex = 1;
            // 
            // New
            // 
            New.Image = Properties.Images.RemoteAdd;
            New.Location = new Point(7, 3);
            New.Name = "New";
            New.Size = new Size(26, 26);
            New.TabIndex = 0;
            toolTip1.SetToolTip(New, "Add new remote");
            New.UseVisualStyleBackColor = true;
            New.Click += NewClick;
            // 
            // Delete
            // 
            Delete.Image = Properties.Images.RemoteDelete;
            Delete.Location = new Point(7, 35);
            Delete.Name = "Delete";
            Delete.Size = new Size(26, 26);
            Delete.TabIndex = 1;
            toolTip1.SetToolTip(Delete, "Delete the selected remote");
            Delete.UseVisualStyleBackColor = true;
            Delete.Click += DeleteClick;
            // 
            // btnToggleState
            // 
            btnToggleState.Image = Properties.Images.EyeOpened;
            btnToggleState.Location = new Point(7, 67);
            btnToggleState.Name = "btnToggleState";
            btnToggleState.Size = new Size(26, 26);
            btnToggleState.TabIndex = 3;
            toolTip1.SetToolTip(btnToggleState, "Enable or disable the selected remote");
            btnToggleState.UseVisualStyleBackColor = true;
            btnToggleState.Click += btnToggleState_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(744, 331);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(pnlManagementContainer);
            tabPage1.Controls.Add(panel1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(736, 303);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Remote repositories";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(Remotes);
            panel1.Controls.Add(panelButtons);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(8);
            panel1.Size = new Size(194, 297);
            panel1.TabIndex = 0;
            // 
            // Remotes
            // 
            Remotes.Activation = ItemActivation.OneClick;
            Remotes.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            Remotes.Dock = DockStyle.Fill;
            Remotes.FullRowSelect = true;
            listViewGroup2.Header = "ListViewGroup";
            listViewGroup2.Name = "listViewGroup1";
            Remotes.Groups.AddRange(new ListViewGroup[] { listViewGroup2 });
            Remotes.HeaderStyle = ColumnHeaderStyle.None;
            Remotes.LabelWrap = false;
            Remotes.Location = new Point(8, 8);
            Remotes.MultiSelect = false;
            Remotes.Name = "Remotes";
            Remotes.Size = new Size(142, 281);
            Remotes.TabIndex = 1;
            Remotes.TileSize = new Size(136, 18);
            Remotes.UseCompatibleStateImageBehavior = false;
            Remotes.View = View.Details;
            Remotes.MouseUp += Remotes_MouseUp;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "";
            columnHeader1.Width = 120;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(tableLayoutPanel2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(736, 303);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Default pull behavior (fetch & merge)";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(panelDetails, 0, 1);
            tableLayoutPanel2.Controls.Add(RemoteBranches, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(730, 297);
            tableLayoutPanel2.TabIndex = 12;
            // 
            // panelDetails
            // 
            panelDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelDetails.Controls.Add(label4);
            panelDetails.Controls.Add(label5);
            panelDetails.Controls.Add(label6);
            panelDetails.Controls.Add(DefaultMergeWithCombo);
            panelDetails.Controls.Add(RemoteRepositoryCombo);
            panelDetails.Controls.Add(LocalBranchNameEdit);
            panelDetails.Controls.Add(SaveDefaultPushPull);
            panelDetails.Location = new Point(3, 173);
            panelDetails.Name = "panelDetails";
            panelDetails.Size = new Size(724, 121);
            panelDetails.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(36, 11);
            label4.Name = "label4";
            label4.Size = new Size(108, 15);
            label4.TabIndex = 0;
            label4.Text = "Local branch name";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(36, 37);
            label5.Name = "label5";
            label5.Size = new Size(104, 15);
            label5.TabIndex = 1;
            label5.Text = "Remote repository";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(36, 64);
            label6.Name = "label6";
            label6.Size = new Size(108, 15);
            label6.TabIndex = 2;
            label6.Text = "Default merge with";
            // 
            // DefaultMergeWithCombo
            // 
            DefaultMergeWithCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            DefaultMergeWithCombo.FormattingEnabled = true;
            DefaultMergeWithCombo.Location = new Point(158, 61);
            DefaultMergeWithCombo.Name = "DefaultMergeWithCombo";
            DefaultMergeWithCombo.Size = new Size(513, 23);
            DefaultMergeWithCombo.TabIndex = 2;
            DefaultMergeWithCombo.DropDown += DefaultMergeWithComboDropDown;
            DefaultMergeWithCombo.Validated += DefaultMergeWithComboValidated;
            // 
            // RemoteRepositoryCombo
            // 
            RemoteRepositoryCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            RemoteRepositoryCombo.FormattingEnabled = true;
            RemoteRepositoryCombo.Location = new Point(158, 34);
            RemoteRepositoryCombo.Name = "RemoteRepositoryCombo";
            RemoteRepositoryCombo.Size = new Size(513, 23);
            RemoteRepositoryCombo.TabIndex = 1;
            RemoteRepositoryCombo.Validated += RemoteRepositoryComboValidated;
            // 
            // LocalBranchNameEdit
            // 
            LocalBranchNameEdit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LocalBranchNameEdit.Location = new Point(158, 8);
            LocalBranchNameEdit.Name = "LocalBranchNameEdit";
            LocalBranchNameEdit.Size = new Size(513, 23);
            LocalBranchNameEdit.TabIndex = 0;
            // 
            // SaveDefaultPushPull
            // 
            SaveDefaultPushPull.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveDefaultPushPull.Image = Properties.Images.Save;
            SaveDefaultPushPull.ImageAlign = ContentAlignment.MiddleLeft;
            SaveDefaultPushPull.Location = new Point(541, 88);
            SaveDefaultPushPull.MinimumSize = new Size(130, 25);
            SaveDefaultPushPull.Name = "SaveDefaultPushPull";
            SaveDefaultPushPull.Size = new Size(130, 25);
            SaveDefaultPushPull.TabIndex = 3;
            SaveDefaultPushPull.Text = "Save changes";
            SaveDefaultPushPull.TextImageRelation = TextImageRelation.ImageBeforeText;
            SaveDefaultPushPull.UseVisualStyleBackColor = true;
            SaveDefaultPushPull.Click += SaveDefaultPushPullClick;
            // 
            // RemoteBranches
            // 
            RemoteBranches.AllowUserToAddRows = false;
            RemoteBranches.AllowUserToDeleteRows = false;
            RemoteBranches.AllowUserToResizeRows = false;
            RemoteBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            RemoteBranches.BackgroundColor = SystemColors.Window;
            RemoteBranches.BorderStyle = BorderStyle.None;
            RemoteBranches.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            RemoteBranches.Columns.AddRange(new DataGridViewColumn[] { BranchName, RemoteCombo, MergeWith });
            RemoteBranches.Dock = DockStyle.Fill;
            RemoteBranches.GridColor = SystemColors.ControlLight;
            RemoteBranches.Location = new Point(3, 3);
            RemoteBranches.MultiSelect = false;
            RemoteBranches.Name = "RemoteBranches";
            RemoteBranches.ReadOnly = true;
            RemoteBranches.RowHeadersVisible = false;
            RemoteBranches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RemoteBranches.Size = new Size(724, 164);
            RemoteBranches.TabIndex = 0;
            RemoteBranches.DataError += RemoteBranchesDataError;
            // 
            // BranchName
            // 
            BranchName.HeaderText = "Local branch name";
            BranchName.Name = "BranchName";
            BranchName.ReadOnly = true;
            // 
            // RemoteCombo
            // 
            RemoteCombo.HeaderText = "Remote repository";
            RemoteCombo.Name = "RemoteCombo";
            RemoteCombo.ReadOnly = true;
            // 
            // MergeWith
            // 
            MergeWith.HeaderText = "Default merge with";
            MergeWith.Name = "MergeWith";
            MergeWith.ReadOnly = true;
            // 
            // FormRemotes
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(744, 331);
            Controls.Add(tabControl1);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(760, 370);
            Name = "FormRemotes";
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Remote repositories";
            flpnlRemoteManagement.ResumeLayout(false);
            flpnlRemoteManagement.PerformLayout();
            pnlMgtDetails.ResumeLayout(false);
            pnlMgtDetails.PerformLayout();
            tblpnlMgtDetails.ResumeLayout(false);
            tblpnlMgtDetails.PerformLayout();
            pnlMgtPuttySsh.ResumeLayout(false);
            pnlMgtPuttySsh.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            pnlManagementContainer.ResumeLayout(false);
            pnlManagementContainer.PerformLayout();
            gbMgtPanel.ResumeLayout(false);
            gbMgtPanel.PerformLayout();
            panelButtons.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            panelDetails.ResumeLayout(false);
            panelDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)RemoteBranches).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private UserControls.NativeListView Remotes;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button LoadSSHKey;
        private TextBox PuttySshKey;
        private Button TestConnection;
        private Label label3;
        private Button SshBrowse;
        private Button Delete;
        private Button New;
        private Button Save;
        private TextBox RemoteName;
        private Label label1;
        private Label label2;
        private GitUI.UserControls.CaseSensitiveComboBox Url;
        private CheckBox checkBoxSepPushUrl;
        private Label labelPushUrl;
        private GitUI.UserControls.CaseSensitiveComboBox comboBoxPushUrl;
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel2;
        private DataGridView RemoteBranches;
        private Button SaveDefaultPushPull;
        private TextBox LocalBranchNameEdit;
        private ComboBox RemoteRepositoryCombo;
        private ComboBox DefaultMergeWithCombo;
        private Label label6;
        private Label label5;
        private Label label4;
        private UserControls.FolderBrowserButton folderBrowserButtonUrl;
        private UserControls.FolderBrowserButton folderBrowserButtonPushUrl;
        private FlowLayoutPanel panelButtons;
        private GroupBox gbMgtPanel;
        private Panel pnlMgtPuttySsh;
        private Label lblMgtPuttyPanelHeader;
        private TableLayoutPanel flpnlRemoteManagement;
        private FlowLayoutPanel flowLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label lblHeaderLine2;
        private Panel pnlMgtDetails;
        private TableLayoutPanel tblpnlMgtDetails;
        private Panel pnlManagementContainer;
        private DataGridViewTextBoxColumn BranchName;
        private DataGridViewTextBoxColumn RemoteCombo;
        private DataGridViewTextBoxColumn MergeWith;
        private ToolTip toolTip1;
        private Button btnToggleState;
        private ColumnHeader columnHeader1;
        private Panel panelDetails;
    }
}
