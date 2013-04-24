﻿
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
            this.gitHeadBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Remotes = new System.Windows.Forms.ListBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.New = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonClose = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.PuTTYSSH = new System.Windows.Forms.GroupBox();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.PuttySshKey = new System.Windows.Forms.TextBox();
            this.TestConnection = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SshBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.folderBrowserButtonPushUrl = new GitUI.UserControls.FolderBrowserButton();
            this.comboBoxPushUrl = new System.Windows.Forms.ComboBox();
            this.folderBrowserButtonUrl = new GitUI.UserControls.FolderBrowserButton();
            this.Url = new System.Windows.Forms.ComboBox();
            this.labelPushUrl = new System.Windows.Forms.Label();
            this.checkBoxSepPushUrl = new System.Windows.Forms.CheckBox();
            this.RemoteName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.RemoteBranches = new System.Windows.Forms.DataGridView();
            this.BranchName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RemoteCombo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MergeWith = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.guidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.selectedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.isHeadDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.isTagDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.isRemoteDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.isOtherDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.remoteDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mergeWithDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gitHeadBindingSource)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
#endif
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.PuTTYSSH.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
#endif
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gitHeadBindingSource
            // 
            this.gitHeadBindingSource.DataSource = typeof(GitCommands.GitRef);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(654, 297);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(646, 269);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Remote repositories";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(640, 263);
            this.splitContainer1.SplitterDistance = 162;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Remotes);
            this.panel1.Controls.Add(this.panelButtons);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(162, 263);
            this.panel1.TabIndex = 1;
            // 
            // Remotes
            // 
            this.Remotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotes.FormattingEnabled = true;
            this.Remotes.ItemHeight = 15;
            this.Remotes.Location = new System.Drawing.Point(0, 0);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(162, 163);
            this.Remotes.TabIndex = 0;
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.RemotesSelectedIndexChanged);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowLayoutPanel3);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 163);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(162, 100);
            this.panelButtons.TabIndex = 1;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.New);
            this.flowLayoutPanel3.Controls.Add(this.Delete);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(162, 100);
            this.flowLayoutPanel3.TabIndex = 0;
            // 
            // New
            // 
            this.New.Image = global::GitUI.Properties.Resources.New;
            this.New.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.New.Location = new System.Drawing.Point(3, 3);
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(92, 25);
            this.New.TabIndex = 0;
            this.New.Text = "New";
            this.New.UseVisualStyleBackColor = true;
            this.New.Click += new System.EventHandler(this.NewClick);
            // 
            // Delete
            // 
            this.Delete.Image = global::GitUI.Properties.Resources.Delete;
            this.Delete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Delete.Location = new System.Drawing.Point(3, 34);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(92, 25);
            this.Delete.TabIndex = 1;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.DeleteClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.PuTTYSSH, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(474, 263);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.Controls.Add(this.buttonClose);
            this.flowLayoutPanel2.Controls.Add(this.Save);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(200, 230);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(271, 30);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(193, 3);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 25);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Image = global::GitUI.Properties.Resources.IconSave;
            this.Save.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Save.Location = new System.Drawing.Point(57, 3);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(130, 25);
            this.Save.TabIndex = 0;
            this.Save.Text = "Save changes";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // PuTTYSSH
            // 
            this.PuTTYSSH.Controls.Add(this.LoadSSHKey);
            this.PuTTYSSH.Controls.Add(this.PuttySshKey);
            this.PuTTYSSH.Controls.Add(this.TestConnection);
            this.PuTTYSSH.Controls.Add(this.label3);
            this.PuTTYSSH.Controls.Add(this.SshBrowse);
            this.PuTTYSSH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PuTTYSSH.Location = new System.Drawing.Point(3, 150);
            this.PuTTYSSH.Name = "PuTTYSSH";
            this.PuTTYSSH.Size = new System.Drawing.Size(468, 70);
            this.PuTTYSSH.TabIndex = 1;
            this.PuTTYSSH.TabStop = false;
            this.PuTTYSSH.Text = "PuTTY SSH";
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(146, 40);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(153, 25);
            this.LoadSSHKey.TabIndex = 2;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // PuttySshKey
            // 
            this.PuttySshKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttySshKey.Location = new System.Drawing.Point(130, 13);
            this.PuttySshKey.Name = "PuttySshKey";
            this.PuttySshKey.Size = new System.Drawing.Size(225, 23);
            this.PuttySshKey.TabIndex = 0;
            // 
            // TestConnection
            // 
            this.TestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TestConnection.Image = global::GitUI.Properties.Resources.putty;
            this.TestConnection.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TestConnection.Location = new System.Drawing.Point(305, 40);
            this.TestConnection.Name = "TestConnection";
            this.TestConnection.Size = new System.Drawing.Size(156, 25);
            this.TestConnection.TabIndex = 3;
            this.TestConnection.Text = "Test connection";
            this.TestConnection.UseVisualStyleBackColor = true;
            this.TestConnection.Click += new System.EventHandler(this.TestConnectionClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Private key file";
            // 
            // SshBrowse
            // 
            this.SshBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SshBrowse.Location = new System.Drawing.Point(361, 11);
            this.SshBrowse.Name = "SshBrowse";
            this.SshBrowse.Size = new System.Drawing.Size(100, 25);
            this.SshBrowse.TabIndex = 1;
            this.SshBrowse.Text = "Browse";
            this.SshBrowse.UseVisualStyleBackColor = true;
            this.SshBrowse.Click += new System.EventHandler(this.SshBrowseClick);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.folderBrowserButtonPushUrl);
            this.groupBox1.Controls.Add(this.folderBrowserButtonUrl);
            this.groupBox1.Controls.Add(this.labelPushUrl);
            this.groupBox1.Controls.Add(this.comboBoxPushUrl);
            this.groupBox1.Controls.Add(this.checkBoxSepPushUrl);
            this.groupBox1.Controls.Add(this.RemoteName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Url);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(468, 141);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            // 
            // folderBrowserButtonPushUrl
            // 
            this.folderBrowserButtonPushUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButtonPushUrl.Location = new System.Drawing.Point(361, 94);
            this.folderBrowserButtonPushUrl.Name = "folderBrowserButtonPushUrl";
            this.folderBrowserButtonPushUrl.PathShowingControl = this.comboBoxPushUrl;
            this.folderBrowserButtonPushUrl.Size = new System.Drawing.Size(100, 25);
            this.folderBrowserButtonPushUrl.TabIndex = 5;
            this.folderBrowserButtonPushUrl.Visible = false;
            // 
            // comboBoxPushUrl
            // 
            this.comboBoxPushUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPushUrl.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxPushUrl.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxPushUrl.FormattingEnabled = true;
            this.comboBoxPushUrl.Location = new System.Drawing.Point(136, 96);
            this.comboBoxPushUrl.Name = "comboBoxPushUrl";
            this.comboBoxPushUrl.Size = new System.Drawing.Size(219, 23);
            this.comboBoxPushUrl.TabIndex = 4;
            this.comboBoxPushUrl.Visible = false;
            // 
            // folderBrowserButtonUrl
            // 
            this.folderBrowserButtonUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButtonUrl.Location = new System.Drawing.Point(361, 46);
            this.folderBrowserButtonUrl.Name = "folderBrowserButtonUrl";
            this.folderBrowserButtonUrl.PathShowingControl = this.Url;
            this.folderBrowserButtonUrl.Size = new System.Drawing.Size(100, 25);
            this.folderBrowserButtonUrl.TabIndex = 2;
            // 
            // Url
            // 
            this.Url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Url.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Url.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Url.FormattingEnabled = true;
            this.Url.Location = new System.Drawing.Point(136, 48);
            this.Url.Name = "Url";
            this.Url.Size = new System.Drawing.Size(219, 23);
            this.Url.TabIndex = 1;
            // 
            // labelPushUrl
            // 
            this.labelPushUrl.AutoSize = true;
            this.labelPushUrl.Location = new System.Drawing.Point(13, 99);
            this.labelPushUrl.Name = "labelPushUrl";
            this.labelPushUrl.Size = new System.Drawing.Size(51, 15);
            this.labelPushUrl.TabIndex = 6;
            this.labelPushUrl.Text = "Push Url";
            this.labelPushUrl.Visible = false;
            // 
            // checkBoxSepPushUrl
            // 
            this.checkBoxSepPushUrl.AutoSize = true;
            this.checkBoxSepPushUrl.Location = new System.Drawing.Point(16, 77);
            this.checkBoxSepPushUrl.Name = "checkBoxSepPushUrl";
            this.checkBoxSepPushUrl.Size = new System.Drawing.Size(118, 19);
            this.checkBoxSepPushUrl.TabIndex = 3;
            this.checkBoxSepPushUrl.Text = "Separate Push Url";
            this.checkBoxSepPushUrl.UseVisualStyleBackColor = true;
            this.checkBoxSepPushUrl.CheckedChanged += new System.EventHandler(this.checkBoxSepPushUrl_CheckedChanged);
            // 
            // RemoteName
            // 
            this.RemoteName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoteName.Location = new System.Drawing.Point(136, 19);
            this.RemoteName.Name = "RemoteName";
            this.RemoteName.Size = new System.Drawing.Size(219, 23);
            this.RemoteName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Url";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(646, 269);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(640, 263);
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
            this.splitContainer3.Size = new System.Drawing.Size(634, 220);
            this.splitContainer3.SplitterDistance = 328;
            this.splitContainer3.TabIndex = 0;
            // 
            // RemoteBranches
            // 
            this.RemoteBranches.AllowUserToAddRows = false;
            this.RemoteBranches.AllowUserToDeleteRows = false;
            this.RemoteBranches.AutoGenerateColumns = false;
            this.RemoteBranches.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.RemoteBranches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.RemoteBranches.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BranchName,
            this.RemoteCombo,
            this.MergeWith,
            this.guidDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn1,
            this.selectedDataGridViewCheckBoxColumn,
            this.isHeadDataGridViewCheckBoxColumn,
            this.isTagDataGridViewCheckBoxColumn,
            this.isRemoteDataGridViewCheckBoxColumn,
            this.isOtherDataGridViewCheckBoxColumn,
            this.remoteDataGridViewTextBoxColumn,
            this.mergeWithDataGridViewTextBoxColumn});
            this.RemoteBranches.DataSource = this.gitHeadBindingSource;
            this.RemoteBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemoteBranches.Location = new System.Drawing.Point(0, 0);
            this.RemoteBranches.MultiSelect = false;
            this.RemoteBranches.Name = "RemoteBranches";
            this.RemoteBranches.ReadOnly = true;
            this.RemoteBranches.RowHeadersVisible = false;
            this.RemoteBranches.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.RemoteBranches.Size = new System.Drawing.Size(328, 220);
            this.RemoteBranches.TabIndex = 0;
            this.RemoteBranches.SelectionChanged += new System.EventHandler(this.RemoteBranchesSelectionChanged);
            // 
            // BranchName
            // 
            this.BranchName.DataPropertyName = "Name";
            this.BranchName.HeaderText = "Local branch name";
            this.BranchName.Name = "BranchName";
            this.BranchName.ReadOnly = true;
            // 
            // RemoteCombo
            // 
            this.RemoteCombo.DataPropertyName = "TrackingRemote";
            this.RemoteCombo.HeaderText = "Remote repository";
            this.RemoteCombo.Name = "RemoteCombo";
            this.RemoteCombo.ReadOnly = true;
            // 
            // MergeWith
            // 
            this.MergeWith.DataPropertyName = "MergeWith";
            this.MergeWith.HeaderText = "Default merge with";
            this.MergeWith.Name = "MergeWith";
            this.MergeWith.ReadOnly = true;
            // 
            // guidDataGridViewTextBoxColumn
            // 
            this.guidDataGridViewTextBoxColumn.DataPropertyName = "Guid";
            this.guidDataGridViewTextBoxColumn.HeaderText = "Guid";
            this.guidDataGridViewTextBoxColumn.Name = "guidDataGridViewTextBoxColumn";
            this.guidDataGridViewTextBoxColumn.ReadOnly = true;
            this.guidDataGridViewTextBoxColumn.Visible = false;
            // 
            // nameDataGridViewTextBoxColumn1
            // 
            this.nameDataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn1.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn1.Name = "nameDataGridViewTextBoxColumn1";
            this.nameDataGridViewTextBoxColumn1.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn1.Visible = false;
            // 
            // selectedDataGridViewCheckBoxColumn
            // 
            this.selectedDataGridViewCheckBoxColumn.DataPropertyName = "Selected";
            this.selectedDataGridViewCheckBoxColumn.HeaderText = "Selected";
            this.selectedDataGridViewCheckBoxColumn.Name = "selectedDataGridViewCheckBoxColumn";
            this.selectedDataGridViewCheckBoxColumn.ReadOnly = true;
            this.selectedDataGridViewCheckBoxColumn.Visible = false;
            // 
            // isHeadDataGridViewCheckBoxColumn
            // 
            this.isHeadDataGridViewCheckBoxColumn.DataPropertyName = "IsHead";
            this.isHeadDataGridViewCheckBoxColumn.HeaderText = "IsHead";
            this.isHeadDataGridViewCheckBoxColumn.Name = "isHeadDataGridViewCheckBoxColumn";
            this.isHeadDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isHeadDataGridViewCheckBoxColumn.Visible = false;
            // 
            // isTagDataGridViewCheckBoxColumn
            // 
            this.isTagDataGridViewCheckBoxColumn.DataPropertyName = "IsTag";
            this.isTagDataGridViewCheckBoxColumn.HeaderText = "IsTag";
            this.isTagDataGridViewCheckBoxColumn.Name = "isTagDataGridViewCheckBoxColumn";
            this.isTagDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isTagDataGridViewCheckBoxColumn.Visible = false;
            // 
            // isRemoteDataGridViewCheckBoxColumn
            // 
            this.isRemoteDataGridViewCheckBoxColumn.DataPropertyName = "IsRemote";
            this.isRemoteDataGridViewCheckBoxColumn.HeaderText = "IsRemote";
            this.isRemoteDataGridViewCheckBoxColumn.Name = "isRemoteDataGridViewCheckBoxColumn";
            this.isRemoteDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isRemoteDataGridViewCheckBoxColumn.Visible = false;
            // 
            // isOtherDataGridViewCheckBoxColumn
            // 
            this.isOtherDataGridViewCheckBoxColumn.DataPropertyName = "IsOther";
            this.isOtherDataGridViewCheckBoxColumn.HeaderText = "IsOther";
            this.isOtherDataGridViewCheckBoxColumn.Name = "isOtherDataGridViewCheckBoxColumn";
            this.isOtherDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isOtherDataGridViewCheckBoxColumn.Visible = false;
            // 
            // remoteDataGridViewTextBoxColumn
            // 
            this.remoteDataGridViewTextBoxColumn.DataPropertyName = "Remote";
            this.remoteDataGridViewTextBoxColumn.HeaderText = "Remote";
            this.remoteDataGridViewTextBoxColumn.Name = "remoteDataGridViewTextBoxColumn";
            this.remoteDataGridViewTextBoxColumn.ReadOnly = true;
            this.remoteDataGridViewTextBoxColumn.Visible = false;
            // 
            // mergeWithDataGridViewTextBoxColumn
            // 
            this.mergeWithDataGridViewTextBoxColumn.DataPropertyName = "MergeWith";
            this.mergeWithDataGridViewTextBoxColumn.HeaderText = "MergeWith";
            this.mergeWithDataGridViewTextBoxColumn.Name = "mergeWithDataGridViewTextBoxColumn";
            this.mergeWithDataGridViewTextBoxColumn.ReadOnly = true;
            this.mergeWithDataGridViewTextBoxColumn.Visible = false;
            // 
            // SaveDefaultPushPull
            // 
            this.SaveDefaultPushPull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveDefaultPushPull.Location = new System.Drawing.Point(222, 192);
            this.SaveDefaultPushPull.Name = "SaveDefaultPushPull";
            this.SaveDefaultPushPull.Size = new System.Drawing.Size(75, 25);
            this.SaveDefaultPushPull.TabIndex = 3;
            this.SaveDefaultPushPull.Text = "Save";
            this.SaveDefaultPushPull.UseVisualStyleBackColor = true;
            this.SaveDefaultPushPull.Click += new System.EventHandler(this.SaveDefaultPushPullClick);
            // 
            // LocalBranchNameEdit
            // 
            this.LocalBranchNameEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LocalBranchNameEdit.Location = new System.Drawing.Point(144, 6);
            this.LocalBranchNameEdit.Name = "LocalBranchNameEdit";
            this.LocalBranchNameEdit.Size = new System.Drawing.Size(153, 23);
            this.LocalBranchNameEdit.TabIndex = 0;
            // 
            // RemoteRepositoryCombo
            // 
            this.RemoteRepositoryCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoteRepositoryCombo.FormattingEnabled = true;
            this.RemoteRepositoryCombo.Location = new System.Drawing.Point(144, 32);
            this.RemoteRepositoryCombo.Name = "RemoteRepositoryCombo";
            this.RemoteRepositoryCombo.Size = new System.Drawing.Size(153, 23);
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
            this.DefaultMergeWithCombo.Size = new System.Drawing.Size(153, 23);
            this.DefaultMergeWithCombo.TabIndex = 2;
            this.DefaultMergeWithCombo.DropDown += new System.EventHandler(this.DefaultMergeWithComboDropDown);
            this.DefaultMergeWithCombo.Validated += new System.EventHandler(this.DefaultMergeWithComboValidated);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 15);
            this.label6.TabIndex = 2;
            this.label6.Text = "Default merge with";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 15);
            this.label5.TabIndex = 1;
            this.label5.Text = "Remote repository";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 15);
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
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 229);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(634, 31);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // UpdateBranch
            // 
            this.UpdateBranch.AutoSize = true;
            this.UpdateBranch.Location = new System.Drawing.Point(382, 3);
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
            this.Prune.Location = new System.Drawing.Point(127, 3);
            this.Prune.Name = "Prune";
            this.Prune.Size = new System.Drawing.Size(249, 25);
            this.Prune.TabIndex = 0;
            this.Prune.Text = "Prune remote branches";
            this.Prune.UseVisualStyleBackColor = true;
            this.Prune.Click += new System.EventHandler(this.PruneClick);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Branch";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // BName
            // 
            this.BName.DataPropertyName = "Name";
            this.BName.HeaderText = "Name";
            this.BName.Name = "BName";
            this.BName.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // FormRemotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(654, 297);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 335);
            this.Name = "FormRemotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repositories";
            this.Load += new System.EventHandler(this.FormRemotesLoad);
            ((System.ComponentModel.ISupportInitialize)(this.gitHeadBindingSource)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
#endif
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.PuTTYSSH.ResumeLayout(false);
            this.PuTTYSSH.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
#endif
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox Remotes;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.BindingSource gitHeadBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Button UpdateBranch;
        private System.Windows.Forms.Button Prune;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox PuTTYSSH;
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
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView RemoteBranches;
        private System.Windows.Forms.DataGridViewTextBoxColumn BranchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn RemoteCombo;
        private System.Windows.Forms.DataGridViewTextBoxColumn MergeWith;
        private System.Windows.Forms.DataGridViewTextBoxColumn guidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn selectedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isHeadDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isTagDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isRemoteDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isOtherDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remoteDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn mergeWithDataGridViewTextBoxColumn;
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
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}