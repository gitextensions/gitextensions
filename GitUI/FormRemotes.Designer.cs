using System;

namespace GitUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Remotes = new System.Windows.Forms.ListBox();
            this.PuTTYSSH = new System.Windows.Forms.GroupBox();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.PuttySshKey = new System.Windows.Forms.TextBox();
            this.TestConnection = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SshBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelPushUrl = new System.Windows.Forms.Label();
            this.comboBoxPushUrl = new System.Windows.Forms.ComboBox();
            this.buttonBrowsePushUrl = new System.Windows.Forms.Button();
            this.checkBoxSepPushUrl = new System.Windows.Forms.CheckBox();
            this.RemoteName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Url = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.New = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
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
            this.gitHeadBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.SaveDefaultPushPull = new System.Windows.Forms.Button();
            this.LocalBranchNameEdit = new System.Windows.Forms.TextBox();
            this.RemoteRepositoryCombo = new System.Windows.Forms.ComboBox();
            this.DefaultMergeWithCombo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Prune = new System.Windows.Forms.Button();
            this.UpdateBranch = new System.Windows.Forms.Button();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.PuTTYSSH.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitHeadBindingSource)).BeginInit();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.Remotes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.PuTTYSSH);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.Delete);
            this.splitContainer1.Panel2.Controls.Add(this.New);
            this.splitContainer1.Panel2.Controls.Add(this.Save);
            this.splitContainer1.Size = new System.Drawing.Size(640, 253);
            this.splitContainer1.SplitterDistance = 162;
            this.splitContainer1.TabIndex = 0;
            // 
            // Remotes
            // 
            this.Remotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotes.FormattingEnabled = true;
            this.Remotes.ItemHeight = 15;
            this.Remotes.Location = new System.Drawing.Point(0, 0);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(162, 253);
            this.Remotes.TabIndex = 0;
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.RemotesSelectedIndexChanged);
            // 
            // PuTTYSSH
            // 
            this.PuTTYSSH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PuTTYSSH.Controls.Add(this.LoadSSHKey);
            this.PuTTYSSH.Controls.Add(this.PuttySshKey);
            this.PuTTYSSH.Controls.Add(this.TestConnection);
            this.PuTTYSSH.Controls.Add(this.label3);
            this.PuTTYSSH.Controls.Add(this.SshBrowse);
            this.PuTTYSSH.Location = new System.Drawing.Point(2, 140);
            this.PuTTYSSH.Name = "PuTTYSSH";
            this.PuTTYSSH.Size = new System.Drawing.Size(467, 70);
            this.PuTTYSSH.TabIndex = 10;
            this.PuTTYSSH.TabStop = false;
            this.PuTTYSSH.Text = "PuTTY SSH";
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(145, 40);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(153, 25);
            this.LoadSSHKey.TabIndex = 9;
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
            this.PuttySshKey.Size = new System.Drawing.Size(224, 23);
            this.PuttySshKey.TabIndex = 7;
            // 
            // TestConnection
            // 
            this.TestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TestConnection.Image = global::GitUI.Properties.Resources.putty;
            this.TestConnection.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TestConnection.Location = new System.Drawing.Point(304, 40);
            this.TestConnection.Name = "TestConnection";
            this.TestConnection.Size = new System.Drawing.Size(156, 25);
            this.TestConnection.TabIndex = 8;
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
            this.SshBrowse.Location = new System.Drawing.Point(360, 11);
            this.SshBrowse.Name = "SshBrowse";
            this.SshBrowse.Size = new System.Drawing.Size(100, 25);
            this.SshBrowse.TabIndex = 6;
            this.SshBrowse.Text = "Browse";
            this.SshBrowse.UseVisualStyleBackColor = true;
            this.SshBrowse.Click += new System.EventHandler(this.SshBrowseClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelPushUrl);
            this.groupBox1.Controls.Add(this.comboBoxPushUrl);
            this.groupBox1.Controls.Add(this.buttonBrowsePushUrl);
            this.groupBox1.Controls.Add(this.checkBoxSepPushUrl);
            this.groupBox1.Controls.Add(this.RemoteName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Url);
            this.groupBox1.Controls.Add(this.Browse);
            this.groupBox1.Location = new System.Drawing.Point(2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 131);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
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
            // comboBoxPushUrl
            // 
            this.comboBoxPushUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPushUrl.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxPushUrl.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxPushUrl.FormattingEnabled = true;
            this.comboBoxPushUrl.Location = new System.Drawing.Point(136, 96);
            this.comboBoxPushUrl.Name = "comboBoxPushUrl";
            this.comboBoxPushUrl.Size = new System.Drawing.Size(218, 23);
            this.comboBoxPushUrl.TabIndex = 7;
            this.comboBoxPushUrl.Visible = false;
            // 
            // buttonBrowsePushUrl
            // 
            this.buttonBrowsePushUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowsePushUrl.Location = new System.Drawing.Point(360, 93);
            this.buttonBrowsePushUrl.Name = "buttonBrowsePushUrl";
            this.buttonBrowsePushUrl.Size = new System.Drawing.Size(100, 25);
            this.buttonBrowsePushUrl.TabIndex = 8;
            this.buttonBrowsePushUrl.Text = "Browse";
            this.buttonBrowsePushUrl.UseVisualStyleBackColor = true;
            this.buttonBrowsePushUrl.Visible = false;
            this.buttonBrowsePushUrl.Click += new System.EventHandler(this.buttonBrowsePushUrl_Click);
            // 
            // checkBoxSepPushUrl
            // 
            this.checkBoxSepPushUrl.AutoSize = true;
            this.checkBoxSepPushUrl.Location = new System.Drawing.Point(16, 77);
            this.checkBoxSepPushUrl.Name = "checkBoxSepPushUrl";
            this.checkBoxSepPushUrl.Size = new System.Drawing.Size(118, 19);
            this.checkBoxSepPushUrl.TabIndex = 5;
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
            this.RemoteName.Size = new System.Drawing.Size(218, 23);
            this.RemoteName.TabIndex = 1;
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
            // Url
            // 
            this.Url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Url.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Url.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Url.FormattingEnabled = true;
            this.Url.Location = new System.Drawing.Point(136, 48);
            this.Url.Name = "Url";
            this.Url.Size = new System.Drawing.Size(218, 23);
            this.Url.TabIndex = 3;
            // 
            // Browse
            // 
            this.Browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Browse.Location = new System.Drawing.Point(360, 45);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(100, 25);
            this.Browse.TabIndex = 4;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.BrowseClick);
            // 
            // Delete
            // 
            this.Delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Delete.Location = new System.Drawing.Point(225, 225);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 25);
            this.Delete.TabIndex = 7;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.DeleteClick);
            // 
            // New
            // 
            this.New.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.New.Location = new System.Drawing.Point(306, 225);
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(75, 25);
            this.New.TabIndex = 6;
            this.New.Text = "New";
            this.New.UseVisualStyleBackColor = true;
            this.New.Click += new System.EventHandler(this.NewClick);
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Location = new System.Drawing.Point(387, 225);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 25);
            this.Save.TabIndex = 5;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(654, 287);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(646, 259);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Remote repositories";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer2);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(644, 252);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Default pull behavior (fetch & merge)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.Prune);
            this.splitContainer2.Panel2.Controls.Add(this.UpdateBranch);
            this.splitContainer2.Size = new System.Drawing.Size(638, 241);
            this.splitContainer2.SplitterDistance = 207;
            this.splitContainer2.TabIndex = 11;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer3.Size = new System.Drawing.Size(638, 207);
            this.splitContainer3.SplitterDistance = 332;
            this.splitContainer3.TabIndex = 1;
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
            this.RemoteBranches.Size = new System.Drawing.Size(332, 207);
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
            // gitHeadBindingSource
            // 
            this.gitHeadBindingSource.DataSource = typeof(GitCommands.GitHead);
            // 
            // SaveDefaultPushPull
            // 
            this.SaveDefaultPushPull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveDefaultPushPull.Location = new System.Drawing.Point(222, 181);
            this.SaveDefaultPushPull.Name = "SaveDefaultPushPull";
            this.SaveDefaultPushPull.Size = new System.Drawing.Size(75, 25);
            this.SaveDefaultPushPull.TabIndex = 6;
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
            this.LocalBranchNameEdit.TabIndex = 5;
            // 
            // RemoteRepositoryCombo
            // 
            this.RemoteRepositoryCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoteRepositoryCombo.FormattingEnabled = true;
            this.RemoteRepositoryCombo.Location = new System.Drawing.Point(144, 32);
            this.RemoteRepositoryCombo.Name = "RemoteRepositoryCombo";
            this.RemoteRepositoryCombo.Size = new System.Drawing.Size(153, 23);
            this.RemoteRepositoryCombo.TabIndex = 4;
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
            this.DefaultMergeWithCombo.TabIndex = 3;
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
            // Prune
            // 
            this.Prune.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Prune.Location = new System.Drawing.Point(129, 4);
            this.Prune.Name = "Prune";
            this.Prune.Size = new System.Drawing.Size(249, 25);
            this.Prune.TabIndex = 11;
            this.Prune.Text = "Prune remote branches";
            this.Prune.UseVisualStyleBackColor = true;
            this.Prune.Click += new System.EventHandler(this.PruneClick);
            // 
            // UpdateBranch
            // 
            this.UpdateBranch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateBranch.Location = new System.Drawing.Point(384, 4);
            this.UpdateBranch.Name = "UpdateBranch";
            this.UpdateBranch.Size = new System.Drawing.Size(249, 25);
            this.UpdateBranch.TabIndex = 10;
            this.UpdateBranch.Text = "Update all remote branch info";
            this.UpdateBranch.UseVisualStyleBackColor = true;
            this.UpdateBranch.Click += new System.EventHandler(this.UpdateBranchClick);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 287);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 325);
            this.Name = "FormRemotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repositories";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRemotes_FormClosing);
            this.Load += new System.EventHandler(this.FormRemotesLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.PuTTYSSH.ResumeLayout(false);
            this.PuTTYSSH.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitHeadBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox Remotes;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView RemoteBranches;
        private System.Windows.Forms.BindingSource gitHeadBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button UpdateBranch;
        private System.Windows.Forms.Button Prune;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TextBox LocalBranchNameEdit;
        private System.Windows.Forms.ComboBox RemoteRepositoryCombo;
        private System.Windows.Forms.ComboBox DefaultMergeWithCombo;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button SaveDefaultPushPull;
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
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.CheckBox checkBoxSepPushUrl;
        private System.Windows.Forms.Label labelPushUrl;
        private System.Windows.Forms.ComboBox comboBoxPushUrl;
        private System.Windows.Forms.Button buttonBrowsePushUrl;
    }
}