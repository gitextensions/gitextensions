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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRemotes));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Remotes = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PuTTYSSH = new System.Windows.Forms.GroupBox();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.PuttySshKey = new System.Windows.Forms.TextBox();
            this.TestConnection = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SshBrowse = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.New = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.RemoteName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Url = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
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
            this.headTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.groupBox1.SuspendLayout();
            this.PuTTYSSH.SuspendLayout();
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
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Remotes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(642, 189);
            this.splitContainer1.SplitterDistance = 177;
            this.splitContainer1.TabIndex = 0;
            // 
            // Remotes
            // 
            this.Remotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(0, 0);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(177, 186);
            this.Remotes.TabIndex = 0;
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.Remotes_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PuTTYSSH);
            this.groupBox1.Controls.Add(this.Delete);
            this.groupBox1.Controls.Add(this.New);
            this.groupBox1.Controls.Add(this.Save);
            this.groupBox1.Controls.Add(this.RemoteName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Url);
            this.groupBox1.Controls.Add(this.Browse);
            this.groupBox1.Location = new System.Drawing.Point(2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(454, 180);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            // 
            // PuTTYSSH
            // 
            this.PuTTYSSH.Controls.Add(this.LoadSSHKey);
            this.PuTTYSSH.Controls.Add(this.PuttySshKey);
            this.PuTTYSSH.Controls.Add(this.TestConnection);
            this.PuTTYSSH.Controls.Add(this.label3);
            this.PuTTYSSH.Controls.Add(this.SshBrowse);
            this.PuTTYSSH.Location = new System.Drawing.Point(6, 72);
            this.PuTTYSSH.Name = "PuTTYSSH";
            this.PuTTYSSH.Size = new System.Drawing.Size(442, 70);
            this.PuTTYSSH.TabIndex = 10;
            this.PuTTYSSH.TabStop = false;
            this.PuTTYSSH.Text = "PuTTY SSH";
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(183, 39);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(123, 23);
            this.LoadSSHKey.TabIndex = 9;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // PuttySshKey
            // 
            this.PuttySshKey.Location = new System.Drawing.Point(107, 13);
            this.PuttySshKey.Name = "PuttySshKey";
            this.PuttySshKey.Size = new System.Drawing.Size(246, 20);
            this.PuttySshKey.TabIndex = 7;
            this.PuttySshKey.TextChanged += new System.EventHandler(this.PuttySshKey_TextChanged);
            // 
            // TestConnection
            // 
            this.TestConnection.Image = global::GitUI.Properties.Resources.putty;
            this.TestConnection.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TestConnection.Location = new System.Drawing.Point(312, 39);
            this.TestConnection.Name = "TestConnection";
            this.TestConnection.Size = new System.Drawing.Size(123, 23);
            this.TestConnection.TabIndex = 8;
            this.TestConnection.Text = "Test connection";
            this.TestConnection.UseVisualStyleBackColor = true;
            this.TestConnection.Click += new System.EventHandler(this.TestConnection_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Private key file";
            // 
            // SshBrowse
            // 
            this.SshBrowse.Location = new System.Drawing.Point(360, 11);
            this.SshBrowse.Name = "SshBrowse";
            this.SshBrowse.Size = new System.Drawing.Size(75, 23);
            this.SshBrowse.TabIndex = 6;
            this.SshBrowse.Text = "Browse";
            this.SshBrowse.UseVisualStyleBackColor = true;
            this.SshBrowse.Click += new System.EventHandler(this.SshBrowse_Click);
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(204, 148);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 7;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // New
            // 
            this.New.Location = new System.Drawing.Point(285, 148);
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(75, 23);
            this.New.TabIndex = 6;
            this.New.Text = "New";
            this.New.UseVisualStyleBackColor = true;
            this.New.Click += new System.EventHandler(this.New_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(366, 148);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 5;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // RemoteName
            // 
            this.RemoteName.Location = new System.Drawing.Point(113, 19);
            this.RemoteName.Name = "RemoteName";
            this.RemoteName.Size = new System.Drawing.Size(246, 20);
            this.RemoteName.TabIndex = 1;
            this.RemoteName.TextChanged += new System.EventHandler(this.RemoteName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Url";
            // 
            // Url
            // 
            this.Url.FormattingEnabled = true;
            this.Url.Location = new System.Drawing.Point(113, 48);
            this.Url.Name = "Url";
            this.Url.Size = new System.Drawing.Size(246, 21);
            this.Url.TabIndex = 3;
            this.Url.SelectedIndexChanged += new System.EventHandler(this.Url_SelectedIndexChanged);
            this.Url.DropDown += new System.EventHandler(this.Url_DropDown);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(366, 46);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 4;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(656, 221);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(648, 195);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Remote repositories";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(648, 195);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Default pull behaviour (fetch & merge)";
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
            this.splitContainer2.Size = new System.Drawing.Size(642, 189);
            this.splitContainer2.SplitterDistance = 155;
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
            this.splitContainer3.Size = new System.Drawing.Size(642, 155);
            this.splitContainer3.SplitterDistance = 376;
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
            this.headTypeDataGridViewTextBoxColumn,
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
            this.RemoteBranches.Size = new System.Drawing.Size(376, 155);
            this.RemoteBranches.TabIndex = 0;
            this.RemoteBranches.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.RemoteBranches_RowValidated);
            this.RemoteBranches.SelectionChanged += new System.EventHandler(this.RemoteBranches_SelectionChanged);
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
            this.RemoteCombo.DataPropertyName = "Remote";
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
            // headTypeDataGridViewTextBoxColumn
            // 
            this.headTypeDataGridViewTextBoxColumn.DataPropertyName = "HeadType";
            this.headTypeDataGridViewTextBoxColumn.HeaderText = "HeadType";
            this.headTypeDataGridViewTextBoxColumn.Name = "headTypeDataGridViewTextBoxColumn";
            this.headTypeDataGridViewTextBoxColumn.ReadOnly = true;
            this.headTypeDataGridViewTextBoxColumn.Visible = false;
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
            this.SaveDefaultPushPull.Location = new System.Drawing.Point(182, 129);
            this.SaveDefaultPushPull.Name = "SaveDefaultPushPull";
            this.SaveDefaultPushPull.Size = new System.Drawing.Size(75, 23);
            this.SaveDefaultPushPull.TabIndex = 6;
            this.SaveDefaultPushPull.Text = "Save";
            this.SaveDefaultPushPull.UseVisualStyleBackColor = true;
            this.SaveDefaultPushPull.Click += new System.EventHandler(this.SaveDefaultPushPull_Click);
            // 
            // LocalBranchNameEdit
            // 
            this.LocalBranchNameEdit.Location = new System.Drawing.Point(124, 6);
            this.LocalBranchNameEdit.Name = "LocalBranchNameEdit";
            this.LocalBranchNameEdit.Size = new System.Drawing.Size(133, 20);
            this.LocalBranchNameEdit.TabIndex = 5;
            this.LocalBranchNameEdit.TextChanged += new System.EventHandler(this.LocalBranchNameEdit_TextChanged);
            // 
            // RemoteRepositoryCombo
            // 
            this.RemoteRepositoryCombo.FormattingEnabled = true;
            this.RemoteRepositoryCombo.Location = new System.Drawing.Point(124, 32);
            this.RemoteRepositoryCombo.Name = "RemoteRepositoryCombo";
            this.RemoteRepositoryCombo.Size = new System.Drawing.Size(133, 21);
            this.RemoteRepositoryCombo.TabIndex = 4;
            this.RemoteRepositoryCombo.SelectedIndexChanged += new System.EventHandler(this.RemoteRepositoryCombo_SelectedIndexChanged);
            this.RemoteRepositoryCombo.Validated += new System.EventHandler(this.RemoteRepositoryCombo_Validated);
            // 
            // DefaultMergeWithCombo
            // 
            this.DefaultMergeWithCombo.FormattingEnabled = true;
            this.DefaultMergeWithCombo.Location = new System.Drawing.Point(124, 59);
            this.DefaultMergeWithCombo.Name = "DefaultMergeWithCombo";
            this.DefaultMergeWithCombo.Size = new System.Drawing.Size(133, 21);
            this.DefaultMergeWithCombo.TabIndex = 3;
            this.DefaultMergeWithCombo.SelectedIndexChanged += new System.EventHandler(this.DefaultMergeWithCombo_SelectedIndexChanged);
            this.DefaultMergeWithCombo.Validated += new System.EventHandler(this.DefaultMergeWithCombo_Validated);
            this.DefaultMergeWithCombo.DropDown += new System.EventHandler(this.DefaultMergeWithCombo_DropDown);
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
            // Prune
            // 
            this.Prune.Location = new System.Drawing.Point(327, 4);
            this.Prune.Name = "Prune";
            this.Prune.Size = new System.Drawing.Size(137, 23);
            this.Prune.TabIndex = 11;
            this.Prune.Text = "Prune remote branches";
            this.Prune.UseVisualStyleBackColor = true;
            this.Prune.Click += new System.EventHandler(this.Prune_Click);
            // 
            // UpdateBranch
            // 
            this.UpdateBranch.Location = new System.Drawing.Point(470, 4);
            this.UpdateBranch.Name = "UpdateBranch";
            this.UpdateBranch.Size = new System.Drawing.Size(167, 23);
            this.UpdateBranch.TabIndex = 10;
            this.UpdateBranch.Text = "Update all remote branch info";
            this.UpdateBranch.UseVisualStyleBackColor = true;
            this.UpdateBranch.Click += new System.EventHandler(this.UpdateBranch_Click);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 221);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRemotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repositories";
            this.Load += new System.EventHandler(this.FormRemotes_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.PuTTYSSH.ResumeLayout(false);
            this.PuTTYSSH.PerformLayout();
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.ComboBox Url;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox RemoteName;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button New;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.GroupBox groupBox1;
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
        private System.Windows.Forms.TextBox PuttySshKey;
        private System.Windows.Forms.Button SshBrowse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button TestConnection;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.GroupBox PuTTYSSH;
        private System.Windows.Forms.Button Prune;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TextBox LocalBranchNameEdit;
        private System.Windows.Forms.ComboBox RemoteRepositoryCombo;
        private System.Windows.Forms.ComboBox DefaultMergeWithCombo;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn BranchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn RemoteCombo;
        private System.Windows.Forms.DataGridViewTextBoxColumn MergeWith;
        private System.Windows.Forms.DataGridViewTextBoxColumn guidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn headTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn selectedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isHeadDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isTagDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isRemoteDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isOtherDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remoteDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn mergeWithDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button SaveDefaultPushPull;
    }
}