
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            this.flpnlRemoteManagement = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.Save = new System.Windows.Forms.Button();
            this.pnlMgtPuttySsh = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.PuttySshKey = new System.Windows.Forms.TextBox();
            this.SshBrowse = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.TestConnection = new System.Windows.Forms.Button();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.lblMgtPuttyPanelHeader = new System.Windows.Forms.Label();
            this.lblHeaderLine2 = new System.Windows.Forms.Label();
            this.pnlMgtDetails = new System.Windows.Forms.Panel();
            this.tblpnlMgtDetails = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserButtonPushUrl = new GitUI.UserControls.FolderBrowserButton();
            this.comboBoxPushUrl = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Url = new System.Windows.Forms.ComboBox();
            this.labelPushUrl = new System.Windows.Forms.Label();
            this.folderBrowserButtonUrl = new GitUI.UserControls.FolderBrowserButton();
            this.checkBoxSepPushUrl = new System.Windows.Forms.CheckBox();
            this.RemoteName = new System.Windows.Forms.TextBox();
            this.pnlManagementContainer = new System.Windows.Forms.Panel();
            this.gbMgtPanel = new System.Windows.Forms.GroupBox();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.New = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.btnToggleState = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Remotes = new UserControls.NativeListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.RemoteBranches = new System.Windows.Forms.DataGridView();
            this.BranchName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RemoteCombo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MergeWith = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SaveDefaultPushPull = new System.Windows.Forms.Button();
            this.LocalBranchNameEdit = new System.Windows.Forms.TextBox();
            this.RemoteRepositoryCombo = new System.Windows.Forms.ComboBox();
            this.DefaultMergeWithCombo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.UpdateBranch = new System.Windows.Forms.Button();
            this.Prune = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flpnlRemoteManagement.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.pnlMgtPuttySsh.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.pnlMgtDetails.SuspendLayout();
            this.tblpnlMgtDetails.SuspendLayout();
            this.pnlManagementContainer.SuspendLayout();
            this.gbMgtPanel.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flpnlRemoteManagement
            // 
            this.flpnlRemoteManagement.AutoSize = true;
            this.flpnlRemoteManagement.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpnlRemoteManagement.ColumnCount = 1;
            this.flpnlRemoteManagement.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.flpnlRemoteManagement.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.flpnlRemoteManagement.Controls.Add(this.pnlMgtPuttySsh, 0, 1);
            this.flpnlRemoteManagement.Controls.Add(this.pnlMgtDetails, 0, 0);
            this.flpnlRemoteManagement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlRemoteManagement.Location = new System.Drawing.Point(3, 16);
            this.flpnlRemoteManagement.Name = "flpnlRemoteManagement";
            this.flpnlRemoteManagement.RowCount = 3;
            this.flpnlRemoteManagement.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.flpnlRemoteManagement.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.flpnlRemoteManagement.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.flpnlRemoteManagement.Size = new System.Drawing.Size(514, 274);
            this.flpnlRemoteManagement.TabIndex = 0;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.Save);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 240);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.flowLayoutPanel2.Size = new System.Drawing.Size(508, 31);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Image = global::GitUI.Properties.Images.Save;
            this.Save.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Save.Location = new System.Drawing.Point(355, 3);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(130, 25);
            this.Save.TabIndex = 0;
            this.Save.Text = "Save changes";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // pnlMgtPuttySsh
            // 
            this.pnlMgtPuttySsh.AutoSize = true;
            this.pnlMgtPuttySsh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlMgtPuttySsh.Controls.Add(this.tableLayoutPanel1);
            this.pnlMgtPuttySsh.Controls.Add(this.lblMgtPuttyPanelHeader);
            this.pnlMgtPuttySsh.Controls.Add(this.lblHeaderLine2);
            this.pnlMgtPuttySsh.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMgtPuttySsh.Location = new System.Drawing.Point(3, 145);
            this.pnlMgtPuttySsh.Name = "pnlMgtPuttySsh";
            this.pnlMgtPuttySsh.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlMgtPuttySsh.Size = new System.Drawing.Size(508, 89);
            this.pnlMgtPuttySsh.TabIndex = 1;
            this.pnlMgtPuttySsh.Text = "PuTTY SSH";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.PuttySshKey, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.SshBrowse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 18);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(470, 60);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // PuttySshKey
            // 
            this.PuttySshKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttySshKey.Location = new System.Drawing.Point(109, 3);
            this.PuttySshKey.Name = "PuttySshKey";
            this.PuttySshKey.Size = new System.Drawing.Size(248, 20);
            this.PuttySshKey.TabIndex = 1;
            // 
            // SshBrowse
            // 
            this.SshBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SshBrowse.Image = global::GitUI.Properties.Images.FileNew;
            this.SshBrowse.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SshBrowse.Location = new System.Drawing.Point(363, 3);
            this.SshBrowse.MinimumSize = new System.Drawing.Size(104, 0);
            this.SshBrowse.Name = "SshBrowse";
            this.SshBrowse.Size = new System.Drawing.Size(104, 25);
            this.SshBrowse.TabIndex = 2;
            this.SshBrowse.Text = " Browse...";
            this.SshBrowse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.SshBrowse.UseVisualStyleBackColor = true;
            this.SshBrowse.Click += new System.EventHandler(this.SshBrowseClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.MinimumSize = new System.Drawing.Size(100, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 31);
            this.label3.TabIndex = 0;
            this.label3.Text = "Private key file";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel3, 2);
            this.flowLayoutPanel3.Controls.Add(this.TestConnection);
            this.flowLayoutPanel3.Controls.Add(this.LoadSSHKey);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(106, 31);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(364, 29);
            this.flowLayoutPanel3.TabIndex = 3;
            // 
            // TestConnection
            // 
            this.TestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TestConnection.Image = global::GitUI.Properties.Images.Putty;
            this.TestConnection.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TestConnection.Location = new System.Drawing.Point(201, 3);
            this.TestConnection.Name = "TestConnection";
            this.TestConnection.Size = new System.Drawing.Size(160, 23);
            this.TestConnection.TabIndex = 1;
            this.TestConnection.Text = "Test connection";
            this.TestConnection.UseVisualStyleBackColor = true;
            this.TestConnection.Click += new System.EventHandler(this.TestConnectionClick);
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Images.Putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(35, 3);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(160, 23);
            this.LoadSSHKey.TabIndex = 0;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // lblMgtPuttyPanelHeader
            // 
            this.lblMgtPuttyPanelHeader.AutoSize = true;
            this.lblMgtPuttyPanelHeader.Location = new System.Drawing.Point(8, 0);
            this.lblMgtPuttyPanelHeader.Name = "lblMgtPuttyPanelHeader";
            this.lblMgtPuttyPanelHeader.Size = new System.Drawing.Size(66, 13);
            this.lblMgtPuttyPanelHeader.TabIndex = 0;
            this.lblMgtPuttyPanelHeader.Text = "PuTTY SSH";
            // 
            // lblHeaderLine2
            // 
            this.lblHeaderLine2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHeaderLine2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblHeaderLine2.Location = new System.Drawing.Point(16, 8);
            this.lblHeaderLine2.Name = "lblHeaderLine2";
            this.lblHeaderLine2.Size = new System.Drawing.Size(481, 3);
            this.lblHeaderLine2.TabIndex = 1;
            // 
            // pnlMgtDetails
            // 
            this.pnlMgtDetails.AutoSize = true;
            this.pnlMgtDetails.Controls.Add(this.tblpnlMgtDetails);
            this.pnlMgtDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMgtDetails.Location = new System.Drawing.Point(3, 3);
            this.pnlMgtDetails.Name = "pnlMgtDetails";
            this.pnlMgtDetails.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlMgtDetails.Size = new System.Drawing.Size(508, 136);
            this.pnlMgtDetails.TabIndex = 0;
            this.pnlMgtDetails.Text = "Details";
            // 
            // tblpnlMgtDetails
            // 
            this.tblpnlMgtDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblpnlMgtDetails.AutoSize = true;
            this.tblpnlMgtDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblpnlMgtDetails.ColumnCount = 3;
            this.tblpnlMgtDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblpnlMgtDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblpnlMgtDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tblpnlMgtDetails.Controls.Add(this.label1, 0, 0);
            this.tblpnlMgtDetails.Controls.Add(this.folderBrowserButtonPushUrl, 2, 3);
            this.tblpnlMgtDetails.Controls.Add(this.label2, 0, 1);
            this.tblpnlMgtDetails.Controls.Add(this.Url, 1, 1);
            this.tblpnlMgtDetails.Controls.Add(this.comboBoxPushUrl, 1, 3);
            this.tblpnlMgtDetails.Controls.Add(this.labelPushUrl, 0, 3);
            this.tblpnlMgtDetails.Controls.Add(this.folderBrowserButtonUrl, 2, 1);
            this.tblpnlMgtDetails.Controls.Add(this.checkBoxSepPushUrl, 0, 2);
            this.tblpnlMgtDetails.Controls.Add(this.RemoteName, 1, 0);
            this.tblpnlMgtDetails.Location = new System.Drawing.Point(16, 11);
            this.tblpnlMgtDetails.Name = "tblpnlMgtDetails";
            this.tblpnlMgtDetails.RowCount = 4;
            this.tblpnlMgtDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblpnlMgtDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblpnlMgtDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblpnlMgtDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblpnlMgtDetails.Size = new System.Drawing.Size(470, 114);
            this.tblpnlMgtDetails.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.MinimumSize = new System.Drawing.Size(100, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // folderBrowserButtonPushUrl
            // 
            this.folderBrowserButtonPushUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderBrowserButtonPushUrl.Location = new System.Drawing.Point(363, 86);
            this.folderBrowserButtonPushUrl.Name = "folderBrowserButtonPushUrl";
            this.folderBrowserButtonPushUrl.PathShowingControl = this.comboBoxPushUrl;
            this.folderBrowserButtonPushUrl.Size = new System.Drawing.Size(104, 25);
            this.folderBrowserButtonPushUrl.TabIndex = 8;
            this.folderBrowserButtonPushUrl.Visible = false;
            // 
            // comboBoxPushUrl
            // 
            this.comboBoxPushUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPushUrl.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxPushUrl.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxPushUrl.FormattingEnabled = true;
            this.comboBoxPushUrl.Location = new System.Drawing.Point(109, 86);
            this.comboBoxPushUrl.Name = "comboBoxPushUrl";
            this.comboBoxPushUrl.Size = new System.Drawing.Size(248, 21);
            this.comboBoxPushUrl.TabIndex = 7;
            this.comboBoxPushUrl.Visible = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 31);
            this.label2.TabIndex = 2;
            this.label2.Text = "Url";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Url
            // 
            this.Url.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Url.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Url.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Url.FormattingEnabled = true;
            this.Url.Location = new System.Drawing.Point(109, 32);
            this.Url.Name = "Url";
            this.Url.Size = new System.Drawing.Size(248, 21);
            this.Url.TabIndex = 3;
            // 
            // labelPushUrl
            // 
            this.labelPushUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPushUrl.Location = new System.Drawing.Point(3, 83);
            this.labelPushUrl.Name = "labelPushUrl";
            this.labelPushUrl.Size = new System.Drawing.Size(100, 31);
            this.labelPushUrl.TabIndex = 6;
            this.labelPushUrl.Text = "Push Url";
            this.labelPushUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelPushUrl.Visible = false;
            // 
            // folderBrowserButtonUrl
            // 
            this.folderBrowserButtonUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderBrowserButtonUrl.Location = new System.Drawing.Point(363, 32);
            this.folderBrowserButtonUrl.Name = "folderBrowserButtonUrl";
            this.folderBrowserButtonUrl.PathShowingControl = this.Url;
            this.folderBrowserButtonUrl.Size = new System.Drawing.Size(104, 25);
            this.folderBrowserButtonUrl.TabIndex = 4;
            // 
            // checkBoxSepPushUrl
            // 
            this.checkBoxSepPushUrl.AutoSize = true;
            this.tblpnlMgtDetails.SetColumnSpan(this.checkBoxSepPushUrl, 2);
            this.checkBoxSepPushUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxSepPushUrl.Location = new System.Drawing.Point(3, 63);
            this.checkBoxSepPushUrl.Name = "checkBoxSepPushUrl";
            this.checkBoxSepPushUrl.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.checkBoxSepPushUrl.Size = new System.Drawing.Size(354, 17);
            this.checkBoxSepPushUrl.TabIndex = 5;
            this.checkBoxSepPushUrl.Text = "Separate Push Url";
            this.checkBoxSepPushUrl.UseVisualStyleBackColor = true;
            this.checkBoxSepPushUrl.CheckedChanged += new System.EventHandler(this.checkBoxSepPushUrl_CheckedChanged);
            // 
            // RemoteName
            // 
            this.RemoteName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemoteName.Location = new System.Drawing.Point(109, 3);
            this.RemoteName.Name = "RemoteName";
            this.RemoteName.Size = new System.Drawing.Size(248, 20);
            this.RemoteName.TabIndex = 1;
            this.RemoteName.TextChanged += new System.EventHandler(this.RemoteName_TextChanged);
            // 
            // pnlManagementContainer
            // 
            this.pnlManagementContainer.Controls.Add(this.gbMgtPanel);
            this.pnlManagementContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlManagementContainer.Location = new System.Drawing.Point(197, 3);
            this.pnlManagementContainer.Name = "pnlManagementContainer";
            this.pnlManagementContainer.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            this.pnlManagementContainer.Size = new System.Drawing.Size(536, 299);
            this.pnlManagementContainer.TabIndex = 0;
            // 
            // gbMgtPanel
            // 
            this.gbMgtPanel.AutoSize = true;
            this.gbMgtPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbMgtPanel.Controls.Add(this.flpnlRemoteManagement);
            this.gbMgtPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbMgtPanel.Location = new System.Drawing.Point(8, 4);
            this.gbMgtPanel.Name = "gbMgtPanel";
            this.gbMgtPanel.Size = new System.Drawing.Size(520, 293);
            this.gbMgtPanel.TabIndex = 0;
            this.gbMgtPanel.TabStop = false;
            this.gbMgtPanel.Text = "Create New Remote";
            // 
            // panelButtons
            // 
            this.panelButtons.AutoSize = true;
            this.panelButtons.Controls.Add(this.New);
            this.panelButtons.Controls.Add(this.Delete);
            this.panelButtons.Controls.Add(this.btnToggleState);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelButtons.Location = new System.Drawing.Point(150, 8);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(8);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.panelButtons.Size = new System.Drawing.Size(36, 283);
            this.panelButtons.TabIndex = 1;
            // 
            // New
            // 
            this.New.Image = global::GitUI.Properties.Images.RemoteAdd;
            this.New.Location = new System.Drawing.Point(7, 3);
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(26, 26);
            this.New.TabIndex = 0;
            this.toolTip1.SetToolTip(this.New, "Add new remote");
            this.New.UseVisualStyleBackColor = true;
            this.New.Click += new System.EventHandler(this.NewClick);
            // 
            // Delete
            // 
            this.Delete.Image = global::GitUI.Properties.Images.RemoteDelete;
            this.Delete.Location = new System.Drawing.Point(7, 35);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(26, 26);
            this.Delete.TabIndex = 1;
            this.toolTip1.SetToolTip(this.Delete, "Delete the selected remote");
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.DeleteClick);
            // 
            // btnToggleState
            // 
            this.btnToggleState.Image = global::GitUI.Properties.Images.EyeOpened;
            this.btnToggleState.Location = new System.Drawing.Point(7, 67);
            this.btnToggleState.Name = "btnToggleState";
            this.btnToggleState.Size = new System.Drawing.Size(26, 26);
            this.btnToggleState.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnToggleState, "Enable or disable the selected remote");
            this.btnToggleState.UseVisualStyleBackColor = true;
            this.btnToggleState.Click += new System.EventHandler(this.btnToggleState_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(744, 331);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pnlManagementContainer);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(736, 305);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Remote repositories";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Remotes);
            this.panel1.Controls.Add(this.panelButtons);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(8);
            this.panel1.Size = new System.Drawing.Size(194, 299);
            this.panel1.TabIndex = 0;
            // 
            // Remotes
            // 
            this.Remotes.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.Remotes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.Remotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotes.FullRowSelect = true;
            listViewGroup1.Header = "ListViewGroup";
            listViewGroup1.Name = "listViewGroup1";
            this.Remotes.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.Remotes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Remotes.HideSelection = false;
            this.Remotes.LabelWrap = false;
            this.Remotes.Location = new System.Drawing.Point(8, 8);
            this.Remotes.MultiSelect = false;
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(142, 283);
            this.Remotes.TabIndex = 1;
            this.Remotes.TileSize = new System.Drawing.Size(136, 18);
            this.Remotes.UseCompatibleStateImageBehavior = false;
            this.Remotes.View = System.Windows.Forms.View.Details;
            this.Remotes.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Remotes_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 120;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(736, 305);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Default pull behavior (fetch & merge)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.splitContainer3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(730, 299);
            this.tableLayoutPanel2.TabIndex = 12;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.RemoteBranches);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.SaveDefaultPushPull);
            this.splitContainer3.Panel2.Controls.Add(this.LocalBranchNameEdit);
            this.splitContainer3.Panel2.Controls.Add(this.RemoteRepositoryCombo);
            this.splitContainer3.Panel2.Controls.Add(this.DefaultMergeWithCombo);
            this.splitContainer3.Panel2.Controls.Add(this.label6);
            this.splitContainer3.Panel2.Controls.Add(this.label5);
            this.splitContainer3.Panel2.Controls.Add(this.label4);
            this.splitContainer3.Size = new System.Drawing.Size(724, 256);
            this.splitContainer3.SplitterDistance = 370;
            this.splitContainer3.TabIndex = 0;
            // 
            // RemoteBranches
            // 
            this.RemoteBranches.AllowUserToAddRows = false;
            this.RemoteBranches.AllowUserToDeleteRows = false;
            this.RemoteBranches.AllowUserToResizeRows = false;
            this.RemoteBranches.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.RemoteBranches.BackgroundColor = System.Drawing.SystemColors.Window;
            this.RemoteBranches.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RemoteBranches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.RemoteBranches.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BranchName,
            this.RemoteCombo,
            this.MergeWith});
            this.RemoteBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemoteBranches.GridColor = System.Drawing.SystemColors.ControlLight;
            this.RemoteBranches.Location = new System.Drawing.Point(0, 0);
            this.RemoteBranches.MultiSelect = false;
            this.RemoteBranches.Name = "RemoteBranches";
            this.RemoteBranches.ReadOnly = true;
            this.RemoteBranches.RowHeadersVisible = false;
            this.RemoteBranches.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.RemoteBranches.Size = new System.Drawing.Size(370, 256);
            this.RemoteBranches.TabIndex = 0;
            this.RemoteBranches.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.RemoteBranchesDataError);
            // 
            // BranchName
            // 
            this.BranchName.HeaderText = "Local branch name";
            this.BranchName.Name = "BranchName";
            this.BranchName.ReadOnly = true;
            // 
            // RemoteCombo
            // 
            this.RemoteCombo.HeaderText = "Remote repository";
            this.RemoteCombo.Name = "RemoteCombo";
            this.RemoteCombo.ReadOnly = true;
            // 
            // MergeWith
            // 
            this.MergeWith.HeaderText = "Default merge with";
            this.MergeWith.Name = "MergeWith";
            this.MergeWith.ReadOnly = true;
            // 
            // SaveDefaultPushPull
            // 
            this.SaveDefaultPushPull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveDefaultPushPull.Image = global::GitUI.Properties.Images.Save;
            this.SaveDefaultPushPull.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SaveDefaultPushPull.Location = new System.Drawing.Point(215, 88);
            this.SaveDefaultPushPull.Name = "SaveDefaultPushPull";
            this.SaveDefaultPushPull.Size = new System.Drawing.Size(130, 25);
            this.SaveDefaultPushPull.TabIndex = 3;
            this.SaveDefaultPushPull.Text = "Save changes";
            this.SaveDefaultPushPull.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.SaveDefaultPushPull.UseVisualStyleBackColor = true;
            this.SaveDefaultPushPull.Click += new System.EventHandler(this.SaveDefaultPushPullClick);
            // 
            // LocalBranchNameEdit
            // 
            this.LocalBranchNameEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LocalBranchNameEdit.Location = new System.Drawing.Point(144, 6);
            this.LocalBranchNameEdit.Name = "LocalBranchNameEdit";
            this.LocalBranchNameEdit.Size = new System.Drawing.Size(201, 20);
            this.LocalBranchNameEdit.TabIndex = 0;
            // 
            // RemoteRepositoryCombo
            // 
            this.RemoteRepositoryCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoteRepositoryCombo.FormattingEnabled = true;
            this.RemoteRepositoryCombo.Location = new System.Drawing.Point(144, 32);
            this.RemoteRepositoryCombo.Name = "RemoteRepositoryCombo";
            this.RemoteRepositoryCombo.Size = new System.Drawing.Size(201, 21);
            this.RemoteRepositoryCombo.TabIndex = 1;
            this.RemoteRepositoryCombo.Validated += new System.EventHandler(this.RemoteRepositoryComboValidated);
            // 
            // DefaultMergeWithCombo
            // 
            this.DefaultMergeWithCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DefaultMergeWithCombo.FormattingEnabled = true;
            this.DefaultMergeWithCombo.Location = new System.Drawing.Point(144, 59);
            this.DefaultMergeWithCombo.Name = "DefaultMergeWithCombo";
            this.DefaultMergeWithCombo.Size = new System.Drawing.Size(201, 21);
            this.DefaultMergeWithCombo.TabIndex = 2;
            this.DefaultMergeWithCombo.DropDown += new System.EventHandler(this.DefaultMergeWithComboDropDown);
            this.DefaultMergeWithCombo.Validated += new System.EventHandler(this.DefaultMergeWithComboValidated);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Default merge with";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Remote repository";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Local branch name";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.UpdateBranch);
            this.flowLayoutPanel1.Controls.Add(this.Prune);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 265);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(724, 31);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // UpdateBranch
            // 
            this.UpdateBranch.AutoSize = true;
            this.UpdateBranch.Location = new System.Drawing.Point(472, 3);
            this.UpdateBranch.Name = "UpdateBranch";
            this.UpdateBranch.Size = new System.Drawing.Size(249, 25);
            this.UpdateBranch.TabIndex = 1;
            this.UpdateBranch.Text = "Update all remote branch info";
            this.UpdateBranch.UseVisualStyleBackColor = true;
            this.UpdateBranch.Click += new System.EventHandler(this.UpdateBranchClick);
            // 
            // Prune
            // 
            this.Prune.AutoSize = true;
            this.Prune.Location = new System.Drawing.Point(217, 3);
            this.Prune.Name = "Prune";
            this.Prune.Size = new System.Drawing.Size(249, 25);
            this.Prune.TabIndex = 0;
            this.Prune.Text = "Prune remote branches";
            this.Prune.UseVisualStyleBackColor = true;
            this.Prune.Click += new System.EventHandler(this.PruneClick);
            // 
            // FormRemotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(744, 331);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 370);
            this.Name = "FormRemotes";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repositories";
            this.flpnlRemoteManagement.ResumeLayout(false);
            this.flpnlRemoteManagement.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.pnlMgtPuttySsh.ResumeLayout(false);
            this.pnlMgtPuttySsh.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.pnlMgtDetails.ResumeLayout(false);
            this.pnlMgtDetails.PerformLayout();
            this.tblpnlMgtDetails.ResumeLayout(false);
            this.tblpnlMgtDetails.PerformLayout();
            this.pnlManagementContainer.ResumeLayout(false);
            this.pnlManagementContainer.PerformLayout();
            this.gbMgtPanel.ResumeLayout(false);
            this.gbMgtPanel.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private UserControls.NativeListView Remotes;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button UpdateBranch;
        private System.Windows.Forms.Button Prune;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.TextBox PuttySshKey;
        private System.Windows.Forms.Button TestConnection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button SshBrowse;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Button New;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.TextBox RemoteName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Url;
        private System.Windows.Forms.CheckBox checkBoxSepPushUrl;
        private System.Windows.Forms.Label labelPushUrl;
        private System.Windows.Forms.ComboBox comboBoxPushUrl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView RemoteBranches;
        private System.Windows.Forms.Button SaveDefaultPushPull;
        private System.Windows.Forms.TextBox LocalBranchNameEdit;
        private System.Windows.Forms.ComboBox RemoteRepositoryCombo;
        private System.Windows.Forms.ComboBox DefaultMergeWithCombo;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private UserControls.FolderBrowserButton folderBrowserButtonUrl;
        private UserControls.FolderBrowserButton folderBrowserButtonPushUrl;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.GroupBox gbMgtPanel;
        private System.Windows.Forms.Panel pnlMgtPuttySsh;
        private System.Windows.Forms.Label lblMgtPuttyPanelHeader;
        private System.Windows.Forms.TableLayoutPanel flpnlRemoteManagement;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label lblHeaderLine2;
        private System.Windows.Forms.Panel pnlMgtDetails;
        private System.Windows.Forms.TableLayoutPanel tblpnlMgtDetails;
        private System.Windows.Forms.Panel pnlManagementContainer;
        private System.Windows.Forms.DataGridViewTextBoxColumn BranchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn RemoteCombo;
        private System.Windows.Forms.DataGridViewTextBoxColumn MergeWith;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnToggleState;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
