namespace GitUI
{
    partial class FormSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            this.LocalSettings = new System.Windows.Forms.TabPage();
            this.label30 = new System.Windows.Forms.Label();
            this.lblLocalAutoCRLF = new System.Windows.Forms.Label();
            this.LocalAutoCRLF = new System.Windows.Forms.ComboBox();
            this.InvalidGitPathLocal = new System.Windows.Forms.Panel();
            this.label21 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.NoGitRepo = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.MergeTool = new System.Windows.Forms.ComboBox();
            this.KeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.Editor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.UserEmail = new System.Windows.Forms.TextBox();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SmtpServer = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.DiffTool2 = new System.Windows.Forms.Button();
            this.SshConfig = new System.Windows.Forms.Button();
            this.GitBinFound = new System.Windows.Forms.Button();
            this.Rescan = new System.Windows.Forms.Button();
            this.CheckAtStartup = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.GitFound = new System.Windows.Forms.Button();
            this.DiffTool = new System.Windows.Forms.Button();
            this.UserNameSet = new System.Windows.Forms.Button();
            this.ShellExtensionsRegistered = new System.Windows.Forms.Button();
            this.GitExtensionsInstall = new System.Windows.Forms.Button();
            this.TabPageGitExtensions = new System.Windows.Forms.TabPage();
            this.Language = new System.Windows.Forms.ComboBox();
            this.label49 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.FollowRenamesInFileHistory = new System.Windows.Forms.CheckBox();
            this.label39 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.EncodingLabel = new System.Windows.Forms.Label();
            this._Encoding = new System.Windows.Forms.ComboBox();
            this.Dictionary = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.ShowRelativeDate = new System.Windows.Forms.CheckBox();
            this.UseFastChecks = new System.Windows.Forms.CheckBox();
            this.ShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.ShowRevisionGraph = new System.Windows.Forms.CheckBox();
            this.CloseProcessDialog = new System.Windows.Forms.CheckBox();
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this._MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.StartPage = new System.Windows.Forms.TabPage();
            this.dashboardEditor1 = new GitUI.DashboardEditor();
            this.AppearancePage = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label47 = new System.Windows.Forms.Label();
            this._DaysToCacheImages = new System.Windows.Forms.NumericUpDown();
            this.label46 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this._authorImageSize = new System.Windows.Forms.NumericUpDown();
            this.ClearImageCache = new System.Windows.Forms.Button();
            this.ShowAuthorGravatar = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.RandomIcon = new System.Windows.Forms.RadioButton();
            this.YellowIcon = new System.Windows.Forms.RadioButton();
            this.RedIcon = new System.Windows.Forms.RadioButton();
            this.GreenIcon = new System.Windows.Forms.RadioButton();
            this.PurpleIcon = new System.Windows.Forms.RadioButton();
            this.BlueIcon = new System.Windows.Forms.RadioButton();
            this.DefaultIcon = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label33 = new System.Windows.Forms.Label();
            this._ColorRemoteBranchLabel = new System.Windows.Forms.Label();
            this._ColorOtherLabel = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this._ColorTagLabel = new System.Windows.Forms.Label();
            this._ColorBranchLabel = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this._RevisionGraphColorLabel = new System.Windows.Forms.Label();
            this._RevisionGraphColorSelected = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label43 = new System.Windows.Forms.Label();
            this._ColorRemovedLineDiffLabel = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this._ColorAddedLineDiffLabel = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this._ColorSectionLabel = new System.Windows.Forms.Label();
            this._ColorRemovedLine = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this._ColorAddedLineLabel = new System.Windows.Forms.Label();
            this.GlobalSettingsPage = new System.Windows.Forms.TabPage();
            this.DiffToolCmdSuggest = new System.Windows.Forms.Button();
            this.DifftoolCmd = new System.Windows.Forms.ComboBox();
            this.label48 = new System.Windows.Forms.Label();
            this.BrowseDiffTool = new System.Windows.Forms.Button();
            this.label42 = new System.Windows.Forms.Label();
            this.DifftoolPath = new System.Windows.Forms.TextBox();
            this.GlobalDiffTool = new System.Windows.Forms.ComboBox();
            this.label41 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.lblGlobalAutoCRLF = new System.Windows.Forms.Label();
            this.GlobalAutoCRLF = new System.Windows.Forms.ComboBox();
            this.InvalidGitPathGlobal = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.MergeToolCmd = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.BrowseMergeTool = new System.Windows.Forms.Button();
            this.GlobalMergeTool = new System.Windows.Forms.ComboBox();
            this.PathToKDiff3 = new System.Windows.Forms.Label();
            this.MergetoolPath = new System.Windows.Forms.TextBox();
            this.GlobalKeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.GlobalEditor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.GlobalUserEmail = new System.Windows.Forms.TextBox();
            this.GlobalUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Ssh = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AutostartPageant = new System.Windows.Forms.CheckBox();
            this.PageantPath = new System.Windows.Forms.TextBox();
            this.PageantBrowse = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.PuttygenPath = new System.Windows.Forms.TextBox();
            this.PuttygenBrowse = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.PlinkPath = new System.Windows.Forms.TextBox();
            this.PlinkBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OtherSsh = new System.Windows.Forms.TextBox();
            this.OtherSshBrowse = new System.Windows.Forms.Button();
            this.Other = new System.Windows.Forms.RadioButton();
            this.label18 = new System.Windows.Forms.Label();
            this.OpenSSH = new System.Windows.Forms.RadioButton();
            this.Putty = new System.Windows.Forms.RadioButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Ok = new System.Windows.Forms.Button();
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.directorySearcher2 = new System.DirectoryServices.DirectorySearcher();
            this.label10 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.LocalSettings.SuspendLayout();
            this.InvalidGitPathLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.TabPageGitExtensions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._MaxCommits)).BeginInit();
            this.StartPage.SuspendLayout();
            this.AppearancePage.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._DaysToCacheImages)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._authorImageSize)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.GlobalSettingsPage.SuspendLayout();
            this.InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.Ssh.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // LocalSettings
            // 
            this.LocalSettings.Controls.Add(this.label30);
            this.LocalSettings.Controls.Add(this.lblLocalAutoCRLF);
            this.LocalSettings.Controls.Add(this.LocalAutoCRLF);
            this.LocalSettings.Controls.Add(this.InvalidGitPathLocal);
            this.LocalSettings.Controls.Add(this.NoGitRepo);
            this.LocalSettings.Controls.Add(this.label20);
            this.LocalSettings.Controls.Add(this.MergeTool);
            this.LocalSettings.Controls.Add(this.KeepMergeBackup);
            this.LocalSettings.Controls.Add(this.label8);
            this.LocalSettings.Controls.Add(this.Editor);
            this.LocalSettings.Controls.Add(this.label5);
            this.LocalSettings.Controls.Add(this.UserEmail);
            this.LocalSettings.Controls.Add(this.UserName);
            this.LocalSettings.Controls.Add(this.label2);
            this.LocalSettings.Controls.Add(this.label1);
            this.LocalSettings.Location = new System.Drawing.Point(4, 22);
            this.LocalSettings.Name = "LocalSettings";
            this.LocalSettings.Padding = new System.Windows.Forms.Padding(3);
            this.LocalSettings.Size = new System.Drawing.Size(710, 383);
            this.LocalSettings.TabIndex = 0;
            this.LocalSettings.Text = "Local settings";
            this.LocalSettings.UseVisualStyleBackColor = true;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(8, 127);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(156, 13);
            this.label30.TabIndex = 23;
            this.label30.Text = "Keep backup (.orig) after merge";
            // 
            // lblLocalAutoCRLF
            // 
            this.lblLocalAutoCRLF.AutoSize = true;
            this.lblLocalAutoCRLF.Location = new System.Drawing.Point(8, 156);
            this.lblLocalAutoCRLF.Name = "lblLocalAutoCRLF";
            this.lblLocalAutoCRLF.Size = new System.Drawing.Size(295, 13);
            this.lblLocalAutoCRLF.TabIndex = 22;
            this.lblLocalAutoCRLF.Text = "Convert CRLF at the end of lines in text files to LF, AutoCRLF";
            // 
            // LocalAutoCRLF
            // 
            this.LocalAutoCRLF.FormattingEnabled = true;
            this.LocalAutoCRLF.Items.AddRange(new object[] {
            "true",
            "false",
            "input"});
            this.LocalAutoCRLF.Location = new System.Drawing.Point(405, 149);
            this.LocalAutoCRLF.Name = "LocalAutoCRLF";
            this.LocalAutoCRLF.Size = new System.Drawing.Size(121, 21);
            this.LocalAutoCRLF.TabIndex = 21;
            // 
            // InvalidGitPathLocal
            // 
            this.InvalidGitPathLocal.BackColor = System.Drawing.SystemColors.Info;
            this.InvalidGitPathLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InvalidGitPathLocal.Controls.Add(this.label21);
            this.InvalidGitPathLocal.Controls.Add(this.pictureBox3);
            this.InvalidGitPathLocal.Location = new System.Drawing.Point(405, 8);
            this.InvalidGitPathLocal.Name = "InvalidGitPathLocal";
            this.InvalidGitPathLocal.Size = new System.Drawing.Size(297, 65);
            this.InvalidGitPathLocal.TabIndex = 20;
            this.InvalidGitPathLocal.Visible = false;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(63, 9);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(175, 39);
            this.label21.TabIndex = 19;
            this.label21.Text = "You need to set the correct path to \r\ngit.cmd before you can change\r\nlocal settin" +
                "gs.\r\n";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::GitUI.Properties.Resources.error;
            this.pictureBox3.Location = new System.Drawing.Point(3, 4);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(54, 50);
            this.pictureBox3.TabIndex = 18;
            this.pictureBox3.TabStop = false;
            // 
            // NoGitRepo
            // 
            this.NoGitRepo.AutoSize = true;
            this.NoGitRepo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoGitRepo.ForeColor = System.Drawing.Color.Red;
            this.NoGitRepo.Location = new System.Drawing.Point(460, 76);
            this.NoGitRepo.Name = "NoGitRepo";
            this.NoGitRepo.Size = new System.Drawing.Size(106, 13);
            this.NoGitRepo.TabIndex = 12;
            this.NoGitRepo.Text = "Not in a git repository";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.BackColor = System.Drawing.SystemColors.Info;
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label20.Location = new System.Drawing.Point(440, 6);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(145, 54);
            this.label20.TabIndex = 11;
            this.label20.Text = "You only need local settings\r\nif you want to override the \r\nglobal settings for t" +
                "he current\r\nrepository.";
            // 
            // MergeTool
            // 
            this.MergeTool.FormattingEnabled = true;
            this.MergeTool.Items.AddRange(new object[] {
            "kdiff3",
            "p4merge"});
            this.MergeTool.Location = new System.Drawing.Point(118, 94);
            this.MergeTool.Name = "MergeTool";
            this.MergeTool.Size = new System.Drawing.Size(159, 21);
            this.MergeTool.TabIndex = 10;
            // 
            // KeepMergeBackup
            // 
            this.KeepMergeBackup.AutoSize = true;
            this.KeepMergeBackup.Checked = true;
            this.KeepMergeBackup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.KeepMergeBackup.Location = new System.Drawing.Point(405, 127);
            this.KeepMergeBackup.Name = "KeepMergeBackup";
            this.KeepMergeBackup.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.KeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.KeepMergeBackup.TabIndex = 9;
            this.KeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 97);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Mergetool";
            // 
            // Editor
            // 
            this.Editor.Location = new System.Drawing.Point(118, 67);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(159, 20);
            this.Editor.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Editor";
            // 
            // UserEmail
            // 
            this.UserEmail.Location = new System.Drawing.Point(118, 40);
            this.UserEmail.Name = "UserEmail";
            this.UserEmail.Size = new System.Drawing.Size(159, 20);
            this.UserEmail.TabIndex = 3;
            this.UserEmail.TextChanged += new System.EventHandler(this.UserEmail_TextChanged);
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(118, 12);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(159, 20);
            this.UserName.TabIndex = 1;
            this.UserName.TextChanged += new System.EventHandler(this.UserName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "User email";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "User name";
            // 
            // SmtpServer
            // 
            this.SmtpServer.Location = new System.Drawing.Point(382, 171);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(242, 20);
            this.SmtpServer.TabIndex = 17;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(8, 175);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(200, 13);
            this.label23.TabIndex = 18;
            this.label23.Text = "Smtp server for sending patches by email";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.TabPageGitExtensions);
            this.tabControl1.Controls.Add(this.StartPage);
            this.tabControl1.Controls.Add(this.AppearancePage);
            this.tabControl1.Controls.Add(this.GlobalSettingsPage);
            this.tabControl1.Controls.Add(this.LocalSettings);
            this.tabControl1.Controls.Add(this.Ssh);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(718, 409);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.DiffTool2);
            this.tabPage3.Controls.Add(this.SshConfig);
            this.tabPage3.Controls.Add(this.GitBinFound);
            this.tabPage3.Controls.Add(this.Rescan);
            this.tabPage3.Controls.Add(this.CheckAtStartup);
            this.tabPage3.Controls.Add(this.label11);
            this.tabPage3.Controls.Add(this.GitFound);
            this.tabPage3.Controls.Add(this.DiffTool);
            this.tabPage3.Controls.Add(this.UserNameSet);
            this.tabPage3.Controls.Add(this.ShellExtensionsRegistered);
            this.tabPage3.Controls.Add(this.GitExtensionsInstall);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(710, 383);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Checklist";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // DiffTool2
            // 
            this.DiffTool2.BackColor = System.Drawing.Color.Gray;
            this.DiffTool2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DiffTool2.FlatAppearance.BorderSize = 0;
            this.DiffTool2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.DiffTool2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiffTool2.Location = new System.Drawing.Point(9, 174);
            this.DiffTool2.Name = "DiffTool2";
            this.DiffTool2.Size = new System.Drawing.Size(693, 23);
            this.DiffTool2.TabIndex = 11;
            this.DiffTool2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool2.UseVisualStyleBackColor = false;
            this.DiffTool2.Click += new System.EventHandler(this.DiffTool2_Click);
            // 
            // SshConfig
            // 
            this.SshConfig.BackColor = System.Drawing.Color.Gray;
            this.SshConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SshConfig.FlatAppearance.BorderSize = 0;
            this.SshConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.SshConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SshConfig.Location = new System.Drawing.Point(9, 232);
            this.SshConfig.Name = "SshConfig";
            this.SshConfig.Size = new System.Drawing.Size(693, 23);
            this.SshConfig.TabIndex = 10;
            this.SshConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SshConfig.UseVisualStyleBackColor = false;
            this.SshConfig.Click += new System.EventHandler(this.SshConfig_Click);
            // 
            // GitBinFound
            // 
            this.GitBinFound.BackColor = System.Drawing.Color.Gray;
            this.GitBinFound.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitBinFound.FlatAppearance.BorderSize = 0;
            this.GitBinFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitBinFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitBinFound.Location = new System.Drawing.Point(9, 58);
            this.GitBinFound.Name = "GitBinFound";
            this.GitBinFound.Size = new System.Drawing.Size(693, 23);
            this.GitBinFound.TabIndex = 9;
            this.GitBinFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitBinFound.UseVisualStyleBackColor = false;
            this.GitBinFound.Click += new System.EventHandler(this.GitBinFound_Click);
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(574, 286);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(128, 24);
            this.Rescan.TabIndex = 8;
            this.Rescan.Text = "Save and rescan";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // CheckAtStartup
            // 
            this.CheckAtStartup.AutoSize = true;
            this.CheckAtStartup.Location = new System.Drawing.Point(12, 286);
            this.CheckAtStartup.Name = "CheckAtStartup";
            this.CheckAtStartup.Size = new System.Drawing.Size(368, 17);
            this.CheckAtStartup.TabIndex = 7;
            this.CheckAtStartup.Text = "Check settings at startup (disables automatically if all settings are correct)";
            this.CheckAtStartup.UseVisualStyleBackColor = true;
            this.CheckAtStartup.CheckedChanged += new System.EventHandler(this.CheckAtStartup_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(434, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "The checklist below validates the basic settings needed for GitExtensions to work" +
                " properly.";
            // 
            // GitFound
            // 
            this.GitFound.BackColor = System.Drawing.Color.Gray;
            this.GitFound.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitFound.FlatAppearance.BorderSize = 0;
            this.GitFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitFound.Location = new System.Drawing.Point(9, 29);
            this.GitFound.Name = "GitFound";
            this.GitFound.Size = new System.Drawing.Size(693, 23);
            this.GitFound.TabIndex = 5;
            this.GitFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitFound.UseVisualStyleBackColor = false;
            this.GitFound.Click += new System.EventHandler(this.GitFound_Click);
            // 
            // DiffTool
            // 
            this.DiffTool.BackColor = System.Drawing.Color.Gray;
            this.DiffTool.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DiffTool.FlatAppearance.BorderSize = 0;
            this.DiffTool.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.DiffTool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiffTool.Location = new System.Drawing.Point(9, 145);
            this.DiffTool.Name = "DiffTool";
            this.DiffTool.Size = new System.Drawing.Size(693, 23);
            this.DiffTool.TabIndex = 4;
            this.DiffTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool.UseVisualStyleBackColor = false;
            this.DiffTool.Click += new System.EventHandler(this.DiffTool_Click);
            // 
            // UserNameSet
            // 
            this.UserNameSet.BackColor = System.Drawing.Color.Gray;
            this.UserNameSet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.UserNameSet.FlatAppearance.BorderSize = 0;
            this.UserNameSet.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.UserNameSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserNameSet.Location = new System.Drawing.Point(9, 116);
            this.UserNameSet.Name = "UserNameSet";
            this.UserNameSet.Size = new System.Drawing.Size(693, 23);
            this.UserNameSet.TabIndex = 3;
            this.UserNameSet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UserNameSet.UseVisualStyleBackColor = false;
            this.UserNameSet.Click += new System.EventHandler(this.UserNameSet_Click);
            // 
            // ShellExtensionsRegistered
            // 
            this.ShellExtensionsRegistered.BackColor = System.Drawing.Color.Gray;
            this.ShellExtensionsRegistered.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ShellExtensionsRegistered.FlatAppearance.BorderSize = 0;
            this.ShellExtensionsRegistered.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ShellExtensionsRegistered.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShellExtensionsRegistered.Location = new System.Drawing.Point(9, 87);
            this.ShellExtensionsRegistered.Name = "ShellExtensionsRegistered";
            this.ShellExtensionsRegistered.Size = new System.Drawing.Size(693, 23);
            this.ShellExtensionsRegistered.TabIndex = 2;
            this.ShellExtensionsRegistered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ShellExtensionsRegistered.UseVisualStyleBackColor = false;
            this.ShellExtensionsRegistered.Click += new System.EventHandler(this.ShellExtensionsRegistered_Click);
            // 
            // GitExtensionsInstall
            // 
            this.GitExtensionsInstall.BackColor = System.Drawing.Color.Gray;
            this.GitExtensionsInstall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitExtensionsInstall.FlatAppearance.BorderSize = 0;
            this.GitExtensionsInstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitExtensionsInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitExtensionsInstall.Location = new System.Drawing.Point(9, 203);
            this.GitExtensionsInstall.Name = "GitExtensionsInstall";
            this.GitExtensionsInstall.Size = new System.Drawing.Size(693, 23);
            this.GitExtensionsInstall.TabIndex = 1;
            this.GitExtensionsInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitExtensionsInstall.UseVisualStyleBackColor = false;
            this.GitExtensionsInstall.Click += new System.EventHandler(this.GitExtensionsInstall_Click);
            // 
            // TabPageGitExtensions
            // 
            this.TabPageGitExtensions.Controls.Add(this.Language);
            this.TabPageGitExtensions.Controls.Add(this.label49);
            this.TabPageGitExtensions.Controls.Add(this.label40);
            this.TabPageGitExtensions.Controls.Add(this.FollowRenamesInFileHistory);
            this.TabPageGitExtensions.Controls.Add(this.label39);
            this.TabPageGitExtensions.Controls.Add(this.label38);
            this.TabPageGitExtensions.Controls.Add(this.label37);
            this.TabPageGitExtensions.Controls.Add(this.label35);
            this.TabPageGitExtensions.Controls.Add(this.label34);
            this.TabPageGitExtensions.Controls.Add(this.EncodingLabel);
            this.TabPageGitExtensions.Controls.Add(this._Encoding);
            this.TabPageGitExtensions.Controls.Add(this.label23);
            this.TabPageGitExtensions.Controls.Add(this.SmtpServer);
            this.TabPageGitExtensions.Controls.Add(this.Dictionary);
            this.TabPageGitExtensions.Controls.Add(this.label22);
            this.TabPageGitExtensions.Controls.Add(this.ShowRelativeDate);
            this.TabPageGitExtensions.Controls.Add(this.UseFastChecks);
            this.TabPageGitExtensions.Controls.Add(this.ShowGitCommandLine);
            this.TabPageGitExtensions.Controls.Add(this.ShowRevisionGraph);
            this.TabPageGitExtensions.Controls.Add(this.CloseProcessDialog);
            this.TabPageGitExtensions.Controls.Add(this.BrowseGitBinPath);
            this.TabPageGitExtensions.Controls.Add(this.GitBinPath);
            this.TabPageGitExtensions.Controls.Add(this.label14);
            this.TabPageGitExtensions.Controls.Add(this.BrowseGitPath);
            this.TabPageGitExtensions.Controls.Add(this.GitPath);
            this.TabPageGitExtensions.Controls.Add(this.label13);
            this.TabPageGitExtensions.Controls.Add(this._MaxCommits);
            this.TabPageGitExtensions.Controls.Add(this.label12);
            this.TabPageGitExtensions.Location = new System.Drawing.Point(4, 22);
            this.TabPageGitExtensions.Name = "TabPageGitExtensions";
            this.TabPageGitExtensions.Size = new System.Drawing.Size(710, 383);
            this.TabPageGitExtensions.TabIndex = 3;
            this.TabPageGitExtensions.Text = "Git extensions";
            this.TabPageGitExtensions.UseVisualStyleBackColor = true;
            this.TabPageGitExtensions.Click += new System.EventHandler(this.TabPageGitExtensions_Click);
            // 
            // Language
            // 
            this.Language.FormattingEnabled = true;
            this.Language.Items.AddRange(new object[] {
            "en-US",
            "ja-JP",
            "nl-NL"});
            this.Language.Location = new System.Drawing.Point(382, 109);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(242, 21);
            this.Language.TabIndex = 29;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(8, 118);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(134, 13);
            this.label49.TabIndex = 28;
            this.label49.Text = "Language (restart required)";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(8, 316);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(208, 13);
            this.label40.TabIndex = 27;
            this.label40.Text = "Follow renames in file history (experimental)";
            // 
            // FollowRenamesInFileHistory
            // 
            this.FollowRenamesInFileHistory.AutoSize = true;
            this.FollowRenamesInFileHistory.Location = new System.Drawing.Point(382, 315);
            this.FollowRenamesInFileHistory.Name = "FollowRenamesInFileHistory";
            this.FollowRenamesInFileHistory.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.FollowRenamesInFileHistory.Size = new System.Drawing.Size(15, 14);
            this.FollowRenamesInFileHistory.TabIndex = 26;
            this.FollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(8, 294);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(184, 13);
            this.label39.TabIndex = 25;
            this.label39.Text = "Show relative date instead of full date";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(8, 271);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(256, 13);
            this.label38.TabIndex = 24;
            this.label38.Text = "Use FileSystemWatcher to check if index is changed";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(8, 249);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(167, 13);
            this.label37.TabIndex = 23;
            this.label37.Text = "Show revision graph in commit log";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(8, 228);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(264, 13);
            this.label35.TabIndex = 22;
            this.label35.Text = "Show Git commandline dialog when executing process";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(8, 206);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(295, 13);
            this.label34.TabIndex = 21;
            this.label34.Text = "Close process dialog automaticly when process is succeeded";
            // 
            // EncodingLabel
            // 
            this.EncodingLabel.AutoSize = true;
            this.EncodingLabel.Location = new System.Drawing.Point(324, 349);
            this.EncodingLabel.Name = "EncodingLabel";
            this.EncodingLabel.Size = new System.Drawing.Size(52, 13);
            this.EncodingLabel.TabIndex = 20;
            this.EncodingLabel.Text = "Encoding";
            // 
            // _Encoding
            // 
            this._Encoding.FormattingEnabled = true;
            this._Encoding.Items.AddRange(new object[] {
            "Default",
            "ASCII",
            "Unicode",
            "UTF7",
            "UTF8",
            "UTF32"});
            this._Encoding.Location = new System.Drawing.Point(382, 346);
            this._Encoding.Name = "_Encoding";
            this._Encoding.Size = new System.Drawing.Size(242, 21);
            this._Encoding.TabIndex = 19;
            // 
            // Dictionary
            // 
            this.Dictionary.FormattingEnabled = true;
            this.Dictionary.Location = new System.Drawing.Point(382, 140);
            this.Dictionary.Name = "Dictionary";
            this.Dictionary.Size = new System.Drawing.Size(242, 21);
            this.Dictionary.TabIndex = 15;
            this.Dictionary.SelectedIndexChanged += new System.EventHandler(this.Dictionary_SelectedIndexChanged);
            this.Dictionary.DropDown += new System.EventHandler(this.Dictionary_DropDown);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(8, 148);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(152, 13);
            this.label22.TabIndex = 14;
            this.label22.Text = "Dictionary for spelling checker.";
            // 
            // ShowRelativeDate
            // 
            this.ShowRelativeDate.AutoSize = true;
            this.ShowRelativeDate.Location = new System.Drawing.Point(382, 293);
            this.ShowRelativeDate.Name = "ShowRelativeDate";
            this.ShowRelativeDate.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowRelativeDate.Size = new System.Drawing.Size(15, 14);
            this.ShowRelativeDate.TabIndex = 13;
            this.ShowRelativeDate.UseVisualStyleBackColor = true;
            // 
            // UseFastChecks
            // 
            this.UseFastChecks.AutoSize = true;
            this.UseFastChecks.Location = new System.Drawing.Point(382, 271);
            this.UseFastChecks.Name = "UseFastChecks";
            this.UseFastChecks.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.UseFastChecks.Size = new System.Drawing.Size(15, 14);
            this.UseFastChecks.TabIndex = 12;
            this.UseFastChecks.UseVisualStyleBackColor = true;
            // 
            // ShowGitCommandLine
            // 
            this.ShowGitCommandLine.AutoSize = true;
            this.ShowGitCommandLine.Location = new System.Drawing.Point(382, 227);
            this.ShowGitCommandLine.Name = "ShowGitCommandLine";
            this.ShowGitCommandLine.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowGitCommandLine.Size = new System.Drawing.Size(15, 14);
            this.ShowGitCommandLine.TabIndex = 11;
            this.ShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // ShowRevisionGraph
            // 
            this.ShowRevisionGraph.AutoSize = true;
            this.ShowRevisionGraph.Location = new System.Drawing.Point(382, 249);
            this.ShowRevisionGraph.Name = "ShowRevisionGraph";
            this.ShowRevisionGraph.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowRevisionGraph.Size = new System.Drawing.Size(15, 14);
            this.ShowRevisionGraph.TabIndex = 10;
            this.ShowRevisionGraph.UseVisualStyleBackColor = true;
            this.ShowRevisionGraph.CheckedChanged += new System.EventHandler(this.ShowRevisionGraph_CheckedChanged);
            // 
            // CloseProcessDialog
            // 
            this.CloseProcessDialog.AutoSize = true;
            this.CloseProcessDialog.Location = new System.Drawing.Point(382, 205);
            this.CloseProcessDialog.Name = "CloseProcessDialog";
            this.CloseProcessDialog.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.CloseProcessDialog.Size = new System.Drawing.Size(15, 14);
            this.CloseProcessDialog.TabIndex = 9;
            this.CloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.Location = new System.Drawing.Point(627, 34);
            this.BrowseGitBinPath.Name = "BrowseGitBinPath";
            this.BrowseGitBinPath.Size = new System.Drawing.Size(75, 23);
            this.BrowseGitBinPath.TabIndex = 8;
            this.BrowseGitBinPath.Text = "Browse";
            this.BrowseGitBinPath.UseVisualStyleBackColor = true;
            this.BrowseGitBinPath.Click += new System.EventHandler(this.BrowseGitBinPath_Click);
            // 
            // GitBinPath
            // 
            this.GitBinPath.Location = new System.Drawing.Point(382, 35);
            this.GitBinPath.Name = "GitBinPath";
            this.GitBinPath.Size = new System.Drawing.Size(242, 20);
            this.GitBinPath.TabIndex = 7;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 40);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(267, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "Path to git.exe (leave empty when git.exe is in the path)";
            // 
            // BrowseGitPath
            // 
            this.BrowseGitPath.Location = new System.Drawing.Point(627, 8);
            this.BrowseGitPath.Name = "BrowseGitPath";
            this.BrowseGitPath.Size = new System.Drawing.Size(75, 23);
            this.BrowseGitPath.TabIndex = 5;
            this.BrowseGitPath.Text = "Browse";
            this.BrowseGitPath.UseVisualStyleBackColor = true;
            this.BrowseGitPath.Click += new System.EventHandler(this.BrowseGitPath_Click);
            // 
            // GitPath
            // 
            this.GitPath.Location = new System.Drawing.Point(382, 9);
            this.GitPath.Name = "GitPath";
            this.GitPath.Size = new System.Drawing.Size(242, 20);
            this.GitPath.TabIndex = 4;
            this.GitPath.TextChanged += new System.EventHandler(this.GitPath_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 14);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(213, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Command used to run git (git.cmd or git.exe)";
            // 
            // _MaxCommits
            // 
            this._MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._MaxCommits.Location = new System.Drawing.Point(382, 63);
            this._MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._MaxCommits.Name = "_MaxCommits";
            this._MaxCommits.Size = new System.Drawing.Size(123, 20);
            this._MaxCommits.TabIndex = 2;
            this._MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 64);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(285, 39);
            this.label12.TabIndex = 0;
            this.label12.Text = "Limit number of commits that will be loaded at startup.\r\nOther commits will be lo" +
                "aded when needed. Lower number \r\nresult is shorter startup time, but slower scro" +
                "lling.";
            // 
            // StartPage
            // 
            this.StartPage.Controls.Add(this.dashboardEditor1);
            this.StartPage.Location = new System.Drawing.Point(4, 22);
            this.StartPage.Name = "StartPage";
            this.StartPage.Padding = new System.Windows.Forms.Padding(3);
            this.StartPage.Size = new System.Drawing.Size(710, 383);
            this.StartPage.TabIndex = 6;
            this.StartPage.Text = "Start page";
            this.StartPage.UseVisualStyleBackColor = true;
            // 
            // dashboardEditor1
            // 
            this.dashboardEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardEditor1.Location = new System.Drawing.Point(3, 3);
            this.dashboardEditor1.Name = "dashboardEditor1";
            this.dashboardEditor1.Size = new System.Drawing.Size(704, 377);
            this.dashboardEditor1.TabIndex = 0;
            // 
            // AppearancePage
            // 
            this.AppearancePage.Controls.Add(this.groupBox6);
            this.AppearancePage.Controls.Add(this.groupBox5);
            this.AppearancePage.Controls.Add(this.groupBox4);
            this.AppearancePage.Controls.Add(this.groupBox3);
            this.AppearancePage.Location = new System.Drawing.Point(4, 22);
            this.AppearancePage.Name = "AppearancePage";
            this.AppearancePage.Size = new System.Drawing.Size(710, 383);
            this.AppearancePage.TabIndex = 5;
            this.AppearancePage.Text = "Appearance";
            this.AppearancePage.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label47);
            this.groupBox6.Controls.Add(this._DaysToCacheImages);
            this.groupBox6.Controls.Add(this.label46);
            this.groupBox6.Controls.Add(this.label44);
            this.groupBox6.Controls.Add(this._authorImageSize);
            this.groupBox6.Controls.Add(this.ClearImageCache);
            this.groupBox6.Controls.Add(this.ShowAuthorGravatar);
            this.groupBox6.Location = new System.Drawing.Point(328, 229);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(311, 129);
            this.groupBox6.TabIndex = 13;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Author images";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(217, 77);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(29, 13);
            this.label47.TabIndex = 7;
            this.label47.Text = "days";
            // 
            // _DaysToCacheImages
            // 
            this._DaysToCacheImages.Location = new System.Drawing.Point(125, 73);
            this._DaysToCacheImages.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._DaysToCacheImages.Name = "_DaysToCacheImages";
            this._DaysToCacheImages.Size = new System.Drawing.Size(77, 20);
            this._DaysToCacheImages.TabIndex = 6;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(7, 77);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(74, 13);
            this.label46.TabIndex = 5;
            this.label46.Text = "Cache images";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(7, 49);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(57, 13);
            this.label44.TabIndex = 4;
            this.label44.Text = "Image size";
            // 
            // _authorImageSize
            // 
            this._authorImageSize.Location = new System.Drawing.Point(125, 46);
            this._authorImageSize.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._authorImageSize.Name = "_authorImageSize";
            this._authorImageSize.Size = new System.Drawing.Size(77, 20);
            this._authorImageSize.TabIndex = 3;
            // 
            // ClearImageCache
            // 
            this.ClearImageCache.Location = new System.Drawing.Point(5, 98);
            this.ClearImageCache.Name = "ClearImageCache";
            this.ClearImageCache.Size = new System.Drawing.Size(142, 23);
            this.ClearImageCache.TabIndex = 1;
            this.ClearImageCache.Text = "Clear image cache";
            this.ClearImageCache.UseVisualStyleBackColor = true;
            this.ClearImageCache.Click += new System.EventHandler(this.ClearImageCache_Click);
            // 
            // ShowAuthorGravatar
            // 
            this.ShowAuthorGravatar.AutoSize = true;
            this.ShowAuthorGravatar.Location = new System.Drawing.Point(7, 20);
            this.ShowAuthorGravatar.Name = "ShowAuthorGravatar";
            this.ShowAuthorGravatar.Size = new System.Drawing.Size(195, 17);
            this.ShowAuthorGravatar.TabIndex = 0;
            this.ShowAuthorGravatar.Text = "Get author image from gravatar.com";
            this.ShowAuthorGravatar.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.RandomIcon);
            this.groupBox5.Controls.Add(this.YellowIcon);
            this.groupBox5.Controls.Add(this.RedIcon);
            this.groupBox5.Controls.Add(this.GreenIcon);
            this.groupBox5.Controls.Add(this.PurpleIcon);
            this.groupBox5.Controls.Add(this.BlueIcon);
            this.groupBox5.Controls.Add(this.DefaultIcon);
            this.groupBox5.Location = new System.Drawing.Point(327, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(312, 219);
            this.groupBox5.TabIndex = 12;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Application Icon";
            this.groupBox5.Enter += new System.EventHandler(this.groupBox5_Enter);
            // 
            // RandomIcon
            // 
            this.RandomIcon.AutoSize = true;
            this.RandomIcon.Location = new System.Drawing.Point(6, 187);
            this.RandomIcon.Name = "RandomIcon";
            this.RandomIcon.Size = new System.Drawing.Size(65, 17);
            this.RandomIcon.TabIndex = 6;
            this.RandomIcon.TabStop = true;
            this.RandomIcon.Text = "Random";
            this.RandomIcon.UseVisualStyleBackColor = true;
            this.RandomIcon.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // YellowIcon
            // 
            this.YellowIcon.AutoSize = true;
            this.YellowIcon.Location = new System.Drawing.Point(6, 159);
            this.YellowIcon.Name = "YellowIcon";
            this.YellowIcon.Size = new System.Drawing.Size(56, 17);
            this.YellowIcon.TabIndex = 5;
            this.YellowIcon.TabStop = true;
            this.YellowIcon.Text = "Yellow";
            this.YellowIcon.UseVisualStyleBackColor = true;
            // 
            // RedIcon
            // 
            this.RedIcon.AutoSize = true;
            this.RedIcon.Location = new System.Drawing.Point(6, 131);
            this.RedIcon.Name = "RedIcon";
            this.RedIcon.Size = new System.Drawing.Size(45, 17);
            this.RedIcon.TabIndex = 4;
            this.RedIcon.TabStop = true;
            this.RedIcon.Text = "Red";
            this.RedIcon.UseVisualStyleBackColor = true;
            // 
            // GreenIcon
            // 
            this.GreenIcon.AutoSize = true;
            this.GreenIcon.Location = new System.Drawing.Point(6, 102);
            this.GreenIcon.Name = "GreenIcon";
            this.GreenIcon.Size = new System.Drawing.Size(54, 17);
            this.GreenIcon.TabIndex = 3;
            this.GreenIcon.TabStop = true;
            this.GreenIcon.Text = "Green";
            this.GreenIcon.UseVisualStyleBackColor = true;
            // 
            // PurpleIcon
            // 
            this.PurpleIcon.AutoSize = true;
            this.PurpleIcon.Location = new System.Drawing.Point(6, 74);
            this.PurpleIcon.Name = "PurpleIcon";
            this.PurpleIcon.Size = new System.Drawing.Size(55, 17);
            this.PurpleIcon.TabIndex = 2;
            this.PurpleIcon.TabStop = true;
            this.PurpleIcon.Text = "Purple";
            this.PurpleIcon.UseVisualStyleBackColor = true;
            // 
            // BlueIcon
            // 
            this.BlueIcon.AutoSize = true;
            this.BlueIcon.Location = new System.Drawing.Point(6, 45);
            this.BlueIcon.Name = "BlueIcon";
            this.BlueIcon.Size = new System.Drawing.Size(46, 17);
            this.BlueIcon.TabIndex = 1;
            this.BlueIcon.TabStop = true;
            this.BlueIcon.Text = "Blue";
            this.BlueIcon.UseVisualStyleBackColor = true;
            // 
            // DefaultIcon
            // 
            this.DefaultIcon.AutoSize = true;
            this.DefaultIcon.Location = new System.Drawing.Point(6, 19);
            this.DefaultIcon.Name = "DefaultIcon";
            this.DefaultIcon.Size = new System.Drawing.Size(59, 17);
            this.DefaultIcon.TabIndex = 0;
            this.DefaultIcon.TabStop = true;
            this.DefaultIcon.Text = "Default";
            this.DefaultIcon.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label33);
            this.groupBox4.Controls.Add(this._ColorRemoteBranchLabel);
            this.groupBox4.Controls.Add(this._ColorOtherLabel);
            this.groupBox4.Controls.Add(this.label36);
            this.groupBox4.Controls.Add(this.label25);
            this.groupBox4.Controls.Add(this._ColorTagLabel);
            this.groupBox4.Controls.Add(this._ColorBranchLabel);
            this.groupBox4.Controls.Add(this.label32);
            this.groupBox4.Controls.Add(this.label24);
            this.groupBox4.Controls.Add(this._RevisionGraphColorLabel);
            this.groupBox4.Controls.Add(this._RevisionGraphColorSelected);
            this.groupBox4.Controls.Add(this.label26);
            this.groupBox4.Location = new System.Drawing.Point(8, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(313, 190);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Revision graph";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 133);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(102, 13);
            this.label33.TabIndex = 8;
            this.label33.Text = "Color remote branch";
            // 
            // _ColorRemoteBranchLabel
            // 
            this._ColorRemoteBranchLabel.AutoSize = true;
            this._ColorRemoteBranchLabel.BackColor = System.Drawing.Color.Red;
            this._ColorRemoteBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorRemoteBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorRemoteBranchLabel.Location = new System.Drawing.Point(211, 133);
            this._ColorRemoteBranchLabel.Name = "_ColorRemoteBranchLabel";
            this._ColorRemoteBranchLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorRemoteBranchLabel.TabIndex = 9;
            this._ColorRemoteBranchLabel.Text = "Red";
            this._ColorRemoteBranchLabel.Click += new System.EventHandler(this.ColorRemoteBranchLabel_Click);
            // 
            // _ColorOtherLabel
            // 
            this._ColorOtherLabel.AutoSize = true;
            this._ColorOtherLabel.BackColor = System.Drawing.Color.Red;
            this._ColorOtherLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorOtherLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorOtherLabel.Location = new System.Drawing.Point(211, 161);
            this._ColorOtherLabel.Name = "_ColorOtherLabel";
            this._ColorOtherLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorOtherLabel.TabIndex = 11;
            this._ColorOtherLabel.Text = "Red";
            this._ColorOtherLabel.Click += new System.EventHandler(this.ColorOtherLabel_Click);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 161);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(83, 13);
            this.label36.TabIndex = 10;
            this.label36.Text = "Color other label";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 76);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(49, 13);
            this.label25.TabIndex = 4;
            this.label25.Text = "Color tag";
            // 
            // _ColorTagLabel
            // 
            this._ColorTagLabel.AutoSize = true;
            this._ColorTagLabel.BackColor = System.Drawing.Color.Red;
            this._ColorTagLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorTagLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorTagLabel.Location = new System.Drawing.Point(211, 76);
            this._ColorTagLabel.Name = "_ColorTagLabel";
            this._ColorTagLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorTagLabel.TabIndex = 5;
            this._ColorTagLabel.Text = "Red";
            this._ColorTagLabel.Click += new System.EventHandler(this.ColorTagLabel_Click);
            // 
            // _ColorBranchLabel
            // 
            this._ColorBranchLabel.AutoSize = true;
            this._ColorBranchLabel.BackColor = System.Drawing.Color.Red;
            this._ColorBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorBranchLabel.Location = new System.Drawing.Point(211, 104);
            this._ColorBranchLabel.Name = "_ColorBranchLabel";
            this._ColorBranchLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorBranchLabel.TabIndex = 7;
            this._ColorBranchLabel.Text = "Red";
            this._ColorBranchLabel.Click += new System.EventHandler(this.ColorBranchLabel_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 104);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(67, 13);
            this.label32.TabIndex = 6;
            this.label32.Text = "Color branch";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 19);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(100, 13);
            this.label24.TabIndex = 0;
            this.label24.Text = "Color revision graph";
            // 
            // _RevisionGraphColorLabel
            // 
            this._RevisionGraphColorLabel.AutoSize = true;
            this._RevisionGraphColorLabel.BackColor = System.Drawing.Color.Red;
            this._RevisionGraphColorLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._RevisionGraphColorLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._RevisionGraphColorLabel.Location = new System.Drawing.Point(211, 19);
            this._RevisionGraphColorLabel.Name = "_RevisionGraphColorLabel";
            this._RevisionGraphColorLabel.Size = new System.Drawing.Size(29, 15);
            this._RevisionGraphColorLabel.TabIndex = 1;
            this._RevisionGraphColorLabel.Text = "Red";
            this._RevisionGraphColorLabel.Click += new System.EventHandler(this.label25_Click);
            // 
            // _RevisionGraphColorSelected
            // 
            this._RevisionGraphColorSelected.AutoSize = true;
            this._RevisionGraphColorSelected.BackColor = System.Drawing.Color.Red;
            this._RevisionGraphColorSelected.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._RevisionGraphColorSelected.Cursor = System.Windows.Forms.Cursors.Hand;
            this._RevisionGraphColorSelected.Location = new System.Drawing.Point(211, 47);
            this._RevisionGraphColorSelected.Name = "_RevisionGraphColorSelected";
            this._RevisionGraphColorSelected.Size = new System.Drawing.Size(29, 15);
            this._RevisionGraphColorSelected.TabIndex = 3;
            this._RevisionGraphColorSelected.Text = "Red";
            this._RevisionGraphColorSelected.Click += new System.EventHandler(this.label25_Click_1);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(6, 47);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(143, 13);
            this.label26.TabIndex = 2;
            this.label26.Text = "Color revision graph selected";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label43);
            this.groupBox3.Controls.Add(this._ColorRemovedLineDiffLabel);
            this.groupBox3.Controls.Add(this.label45);
            this.groupBox3.Controls.Add(this._ColorAddedLineDiffLabel);
            this.groupBox3.Controls.Add(this.label27);
            this.groupBox3.Controls.Add(this._ColorSectionLabel);
            this.groupBox3.Controls.Add(this._ColorRemovedLine);
            this.groupBox3.Controls.Add(this.label31);
            this.groupBox3.Controls.Add(this.label29);
            this.groupBox3.Controls.Add(this._ColorAddedLineLabel);
            this.groupBox3.Location = new System.Drawing.Point(8, 198);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(313, 160);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Difference view";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(6, 79);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(150, 13);
            this.label43.TabIndex = 10;
            this.label43.Text = "Color removed line highlighting";
            // 
            // _ColorRemovedLineDiffLabel
            // 
            this._ColorRemovedLineDiffLabel.AutoSize = true;
            this._ColorRemovedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._ColorRemovedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorRemovedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorRemovedLineDiffLabel.Location = new System.Drawing.Point(208, 79);
            this._ColorRemovedLineDiffLabel.Name = "_ColorRemovedLineDiffLabel";
            this._ColorRemovedLineDiffLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorRemovedLineDiffLabel.TabIndex = 11;
            this._ColorRemovedLineDiffLabel.Text = "Red";
            this._ColorRemovedLineDiffLabel.Click += new System.EventHandler(this.ColorRemovedLineDiffLabel_Click);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(6, 109);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(139, 13);
            this.label45.TabIndex = 12;
            this.label45.Text = "Color added line highlighting";
            // 
            // _ColorAddedLineDiffLabel
            // 
            this._ColorAddedLineDiffLabel.AutoSize = true;
            this._ColorAddedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._ColorAddedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorAddedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorAddedLineDiffLabel.Location = new System.Drawing.Point(208, 109);
            this._ColorAddedLineDiffLabel.Name = "_ColorAddedLineDiffLabel";
            this._ColorAddedLineDiffLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorAddedLineDiffLabel.TabIndex = 13;
            this._ColorAddedLineDiffLabel.Text = "Red";
            this._ColorAddedLineDiffLabel.Click += new System.EventHandler(this.ColorAddedLineDiffLabel_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 18);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(94, 13);
            this.label27.TabIndex = 4;
            this.label27.Text = "Color removed line";
            // 
            // _ColorSectionLabel
            // 
            this._ColorSectionLabel.AutoSize = true;
            this._ColorSectionLabel.BackColor = System.Drawing.Color.Red;
            this._ColorSectionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorSectionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorSectionLabel.Location = new System.Drawing.Point(208, 138);
            this._ColorSectionLabel.Name = "_ColorSectionLabel";
            this._ColorSectionLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorSectionLabel.TabIndex = 9;
            this._ColorSectionLabel.Text = "Red";
            this._ColorSectionLabel.Click += new System.EventHandler(this.ColorSectionLabel_Click);
            // 
            // _ColorRemovedLine
            // 
            this._ColorRemovedLine.AutoSize = true;
            this._ColorRemovedLine.BackColor = System.Drawing.Color.Red;
            this._ColorRemovedLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorRemovedLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorRemovedLine.Location = new System.Drawing.Point(208, 18);
            this._ColorRemovedLine.Name = "_ColorRemovedLine";
            this._ColorRemovedLine.Size = new System.Drawing.Size(29, 15);
            this._ColorRemovedLine.TabIndex = 5;
            this._ColorRemovedLine.Text = "Red";
            this._ColorRemovedLine.Click += new System.EventHandler(this.ColorRemovedLine_Click);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(6, 139);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(68, 13);
            this.label31.TabIndex = 8;
            this.label31.Text = "Color section";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 48);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(83, 13);
            this.label29.TabIndex = 6;
            this.label29.Text = "Color added line";
            // 
            // _ColorAddedLineLabel
            // 
            this._ColorAddedLineLabel.AutoSize = true;
            this._ColorAddedLineLabel.BackColor = System.Drawing.Color.Red;
            this._ColorAddedLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ColorAddedLineLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._ColorAddedLineLabel.Location = new System.Drawing.Point(208, 48);
            this._ColorAddedLineLabel.Name = "_ColorAddedLineLabel";
            this._ColorAddedLineLabel.Size = new System.Drawing.Size(29, 15);
            this._ColorAddedLineLabel.TabIndex = 7;
            this._ColorAddedLineLabel.Text = "Red";
            this._ColorAddedLineLabel.Click += new System.EventHandler(this.label28_Click);
            // 
            // GlobalSettingsPage
            // 
            this.GlobalSettingsPage.Controls.Add(this.DiffToolCmdSuggest);
            this.GlobalSettingsPage.Controls.Add(this.DifftoolCmd);
            this.GlobalSettingsPage.Controls.Add(this.label48);
            this.GlobalSettingsPage.Controls.Add(this.BrowseDiffTool);
            this.GlobalSettingsPage.Controls.Add(this.label42);
            this.GlobalSettingsPage.Controls.Add(this.DifftoolPath);
            this.GlobalSettingsPage.Controls.Add(this.GlobalDiffTool);
            this.GlobalSettingsPage.Controls.Add(this.label41);
            this.GlobalSettingsPage.Controls.Add(this.label28);
            this.GlobalSettingsPage.Controls.Add(this.lblGlobalAutoCRLF);
            this.GlobalSettingsPage.Controls.Add(this.GlobalAutoCRLF);
            this.GlobalSettingsPage.Controls.Add(this.InvalidGitPathGlobal);
            this.GlobalSettingsPage.Controls.Add(this.button1);
            this.GlobalSettingsPage.Controls.Add(this.MergeToolCmd);
            this.GlobalSettingsPage.Controls.Add(this.label19);
            this.GlobalSettingsPage.Controls.Add(this.BrowseMergeTool);
            this.GlobalSettingsPage.Controls.Add(this.GlobalMergeTool);
            this.GlobalSettingsPage.Controls.Add(this.PathToKDiff3);
            this.GlobalSettingsPage.Controls.Add(this.MergetoolPath);
            this.GlobalSettingsPage.Controls.Add(this.GlobalKeepMergeBackup);
            this.GlobalSettingsPage.Controls.Add(this.label7);
            this.GlobalSettingsPage.Controls.Add(this.GlobalEditor);
            this.GlobalSettingsPage.Controls.Add(this.label6);
            this.GlobalSettingsPage.Controls.Add(this.GlobalUserEmail);
            this.GlobalSettingsPage.Controls.Add(this.GlobalUserName);
            this.GlobalSettingsPage.Controls.Add(this.label4);
            this.GlobalSettingsPage.Controls.Add(this.label3);
            this.GlobalSettingsPage.Location = new System.Drawing.Point(4, 22);
            this.GlobalSettingsPage.Name = "GlobalSettingsPage";
            this.GlobalSettingsPage.Padding = new System.Windows.Forms.Padding(3);
            this.GlobalSettingsPage.Size = new System.Drawing.Size(710, 383);
            this.GlobalSettingsPage.TabIndex = 1;
            this.GlobalSettingsPage.Text = "Global settings";
            this.GlobalSettingsPage.UseVisualStyleBackColor = true;
            // 
            // DiffToolCmdSuggest
            // 
            this.DiffToolCmdSuggest.Location = new System.Drawing.Point(506, 285);
            this.DiffToolCmdSuggest.Name = "DiffToolCmdSuggest";
            this.DiffToolCmdSuggest.Size = new System.Drawing.Size(108, 23);
            this.DiffToolCmdSuggest.TabIndex = 30;
            this.DiffToolCmdSuggest.Text = "Suggest command";
            this.DiffToolCmdSuggest.UseVisualStyleBackColor = true;
            this.DiffToolCmdSuggest.Click += new System.EventHandler(this.DiffToolCmdSuggest_Click);
            // 
            // DifftoolCmd
            // 
            this.DifftoolCmd.FormattingEnabled = true;
            this.DifftoolCmd.Items.AddRange(new object[] {
            "\"c:/Program Files/Perforce/p4merge.exe\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"",
            "\"c:/Program Files/TortoiseSVN/bin/TortoiseMerge.exe\" /base:\"$BASE\" /mine:\"$LOCAL\"" +
                " /theirs:\"$REMOTE\" /merged:\"$MERGED\"",
            "\"c:/Program Files/Araxis/Araxis Merge/Compare.exe\" -wait -merge -3 -a1 \"$BASE\" \"$" +
                "LOCAL\" \"$REMOTE\" \"$MERGED\"",
            "\"c:/Program Files/SourceGear/DiffMerge/DiffMerge.exe\" /m /r=\"$MERGED\" \"$LOCAL\" \"$" +
                "BASE\" \"$REMOTE\""});
            this.DifftoolCmd.Location = new System.Drawing.Point(153, 287);
            this.DifftoolCmd.Name = "DifftoolCmd";
            this.DifftoolCmd.Size = new System.Drawing.Size(347, 21);
            this.DifftoolCmd.TabIndex = 29;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(9, 291);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(89, 13);
            this.label48.TabIndex = 28;
            this.label48.Text = "Difftool command";
            // 
            // BrowseDiffTool
            // 
            this.BrowseDiffTool.Location = new System.Drawing.Point(506, 256);
            this.BrowseDiffTool.Name = "BrowseDiffTool";
            this.BrowseDiffTool.Size = new System.Drawing.Size(75, 23);
            this.BrowseDiffTool.TabIndex = 27;
            this.BrowseDiffTool.Text = "Browse";
            this.BrowseDiffTool.UseVisualStyleBackColor = true;
            this.BrowseDiffTool.Click += new System.EventHandler(this.BrowseDiffTool_Click);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(9, 263);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(75, 13);
            this.label42.TabIndex = 26;
            this.label42.Text = "Path to difftool";
            // 
            // DifftoolPath
            // 
            this.DifftoolPath.Location = new System.Drawing.Point(153, 259);
            this.DifftoolPath.Name = "DifftoolPath";
            this.DifftoolPath.Size = new System.Drawing.Size(347, 20);
            this.DifftoolPath.TabIndex = 25;
            // 
            // GlobalDiffTool
            // 
            this.GlobalDiffTool.FormattingEnabled = true;
            this.GlobalDiffTool.Items.AddRange(new object[] {
            "kdiff3",
            "kompare",
            "tkdiff",
            "meld",
            "xxdiff",
            "emerge",
            "vimdiff",
            "gvimdiff",
            "ecmerge",
            "diffuse",
            "opendiff",
            "araxis",
            "winmerge"});
            this.GlobalDiffTool.Location = new System.Drawing.Point(153, 232);
            this.GlobalDiffTool.Name = "GlobalDiffTool";
            this.GlobalDiffTool.Size = new System.Drawing.Size(164, 21);
            this.GlobalDiffTool.TabIndex = 24;
            this.GlobalDiffTool.SelectedIndexChanged += new System.EventHandler(this.GlobalDiffTool_SelectedIndexChanged);
            this.GlobalDiffTool.TextChanged += new System.EventHandler(this.ExternalDiffTool_TextChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(10, 235);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(40, 13);
            this.label41.TabIndex = 23;
            this.label41.Text = "Difftool";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(10, 179);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(156, 13);
            this.label28.TabIndex = 22;
            this.label28.Text = "Keep backup (.orig) after merge";
            // 
            // lblGlobalAutoCRLF
            // 
            this.lblGlobalAutoCRLF.AutoSize = true;
            this.lblGlobalAutoCRLF.Location = new System.Drawing.Point(10, 206);
            this.lblGlobalAutoCRLF.Name = "lblGlobalAutoCRLF";
            this.lblGlobalAutoCRLF.Size = new System.Drawing.Size(295, 13);
            this.lblGlobalAutoCRLF.TabIndex = 21;
            this.lblGlobalAutoCRLF.Text = "Convert CRLF at the end of lines in text files to LF, AutoCRLF";
            // 
            // GlobalAutoCRLF
            // 
            this.GlobalAutoCRLF.FormattingEnabled = true;
            this.GlobalAutoCRLF.Items.AddRange(new object[] {
            "true",
            "false",
            "input"});
            this.GlobalAutoCRLF.Location = new System.Drawing.Point(411, 201);
            this.GlobalAutoCRLF.Name = "GlobalAutoCRLF";
            this.GlobalAutoCRLF.Size = new System.Drawing.Size(121, 21);
            this.GlobalAutoCRLF.TabIndex = 20;
            // 
            // InvalidGitPathGlobal
            // 
            this.InvalidGitPathGlobal.BackColor = System.Drawing.SystemColors.Info;
            this.InvalidGitPathGlobal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InvalidGitPathGlobal.Controls.Add(this.label9);
            this.InvalidGitPathGlobal.Controls.Add(this.pictureBox1);
            this.InvalidGitPathGlobal.Location = new System.Drawing.Point(395, 8);
            this.InvalidGitPathGlobal.Name = "InvalidGitPathGlobal";
            this.InvalidGitPathGlobal.Size = new System.Drawing.Size(296, 68);
            this.InvalidGitPathGlobal.TabIndex = 19;
            this.InvalidGitPathGlobal.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(63, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(175, 39);
            this.label9.TabIndex = 19;
            this.label9.Text = "You need to set the correct path to \r\ngit.cmd before you can change\r\nglobal setti" +
                "ngs.\r\n";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Resources.error;
            this.pictureBox1.Location = new System.Drawing.Point(3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(54, 50);
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(506, 145);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "Suggest command";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // MergeToolCmd
            // 
            this.MergeToolCmd.FormattingEnabled = true;
            this.MergeToolCmd.Items.AddRange(new object[] {
            "\"c:/Program Files/Perforce/p4merge.exe\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"",
            "\"c:/Program Files/TortoiseSVN/bin/TortoiseMerge.exe\" /base:\"$BASE\" /mine:\"$LOCAL\"" +
                " /theirs:\"$REMOTE\" /merged:\"$MERGED\"",
            "\"c:/Program Files/Araxis/Araxis Merge/Compare.exe\" -wait -merge -3 -a1 \"$BASE\" \"$" +
                "LOCAL\" \"$REMOTE\" \"$MERGED\"",
            "\"c:/Program Files/SourceGear/DiffMerge/DiffMerge.exe\" /m /r=\"$MERGED\" \"$LOCAL\" \"$" +
                "BASE\" \"$REMOTE\""});
            this.MergeToolCmd.Location = new System.Drawing.Point(153, 147);
            this.MergeToolCmd.Name = "MergeToolCmd";
            this.MergeToolCmd.Size = new System.Drawing.Size(347, 21);
            this.MergeToolCmd.TabIndex = 15;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(9, 151);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(103, 13);
            this.label19.TabIndex = 14;
            this.label19.Text = "Mergetool command";
            // 
            // BrowseMergeTool
            // 
            this.BrowseMergeTool.Location = new System.Drawing.Point(506, 117);
            this.BrowseMergeTool.Name = "BrowseMergeTool";
            this.BrowseMergeTool.Size = new System.Drawing.Size(75, 23);
            this.BrowseMergeTool.TabIndex = 13;
            this.BrowseMergeTool.Text = "Browse";
            this.BrowseMergeTool.UseVisualStyleBackColor = true;
            this.BrowseMergeTool.Click += new System.EventHandler(this.BrowseMergeTool_Click);
            // 
            // GlobalMergeTool
            // 
            this.GlobalMergeTool.FormattingEnabled = true;
            this.GlobalMergeTool.Items.AddRange(new object[] {
            "Araxis",
            "DiffMerge",
            "kdiff3",
            "p4merge",
            "TortoiseMerge"});
            this.GlobalMergeTool.Location = new System.Drawing.Point(153, 92);
            this.GlobalMergeTool.Name = "GlobalMergeTool";
            this.GlobalMergeTool.Size = new System.Drawing.Size(164, 21);
            this.GlobalMergeTool.TabIndex = 12;
            this.GlobalMergeTool.SelectedIndexChanged += new System.EventHandler(this.GlobalMergeTool_SelectedIndexChanged);
            this.GlobalMergeTool.TextChanged += new System.EventHandler(this.GlobalMergeTool_TextChanged);
            // 
            // PathToKDiff3
            // 
            this.PathToKDiff3.AutoSize = true;
            this.PathToKDiff3.Location = new System.Drawing.Point(9, 124);
            this.PathToKDiff3.Name = "PathToKDiff3";
            this.PathToKDiff3.Size = new System.Drawing.Size(90, 13);
            this.PathToKDiff3.TabIndex = 11;
            this.PathToKDiff3.Text = "Path to mergetool";
            // 
            // MergetoolPath
            // 
            this.MergetoolPath.Location = new System.Drawing.Point(153, 120);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(347, 20);
            this.MergetoolPath.TabIndex = 10;
            // 
            // GlobalKeepMergeBackup
            // 
            this.GlobalKeepMergeBackup.AutoSize = true;
            this.GlobalKeepMergeBackup.Checked = true;
            this.GlobalKeepMergeBackup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.GlobalKeepMergeBackup.Location = new System.Drawing.Point(411, 179);
            this.GlobalKeepMergeBackup.Name = "GlobalKeepMergeBackup";
            this.GlobalKeepMergeBackup.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.GlobalKeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.GlobalKeepMergeBackup.TabIndex = 9;
            this.GlobalKeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Mergetool";
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Location = new System.Drawing.Point(153, 65);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(164, 20);
            this.GlobalEditor.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Editor";
            // 
            // GlobalUserEmail
            // 
            this.GlobalUserEmail.Location = new System.Drawing.Point(153, 37);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(164, 20);
            this.GlobalUserEmail.TabIndex = 3;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(153, 8);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(164, 20);
            this.GlobalUserName.TabIndex = 2;
            this.GlobalUserName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "User email";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "User name";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // Ssh
            // 
            this.Ssh.Controls.Add(this.groupBox2);
            this.Ssh.Controls.Add(this.groupBox1);
            this.Ssh.Location = new System.Drawing.Point(4, 22);
            this.Ssh.Name = "Ssh";
            this.Ssh.Padding = new System.Windows.Forms.Padding(3);
            this.Ssh.Size = new System.Drawing.Size(710, 383);
            this.Ssh.TabIndex = 4;
            this.Ssh.Text = "Ssh";
            this.Ssh.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.AutostartPageant);
            this.groupBox2.Controls.Add(this.PageantPath);
            this.groupBox2.Controls.Add(this.PageantBrowse);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.PuttygenPath);
            this.groupBox2.Controls.Add(this.PuttygenBrowse);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.PlinkPath);
            this.groupBox2.Controls.Add(this.PlinkBrowse);
            this.groupBox2.Location = new System.Drawing.Point(8, 133);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(691, 126);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Configure PuTTY";
            // 
            // AutostartPageant
            // 
            this.AutostartPageant.AutoSize = true;
            this.AutostartPageant.Checked = true;
            this.AutostartPageant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutostartPageant.Location = new System.Drawing.Point(118, 103);
            this.AutostartPageant.Name = "AutostartPageant";
            this.AutostartPageant.Size = new System.Drawing.Size(424, 17);
            this.AutostartPageant.TabIndex = 11;
            this.AutostartPageant.Text = "Automatically start authentication client when a private key is configured for a " +
                "remote";
            this.AutostartPageant.UseVisualStyleBackColor = true;
            // 
            // PageantPath
            // 
            this.PageantPath.Location = new System.Drawing.Point(118, 76);
            this.PageantPath.Name = "PageantPath";
            this.PageantPath.Size = new System.Drawing.Size(323, 20);
            this.PageantPath.TabIndex = 9;
            // 
            // PageantBrowse
            // 
            this.PageantBrowse.Location = new System.Drawing.Point(463, 75);
            this.PageantBrowse.Name = "PageantBrowse";
            this.PageantBrowse.Size = new System.Drawing.Size(75, 23);
            this.PageantBrowse.TabIndex = 10;
            this.PageantBrowse.Text = "Browse";
            this.PageantBrowse.UseVisualStyleBackColor = true;
            this.PageantBrowse.Click += new System.EventHandler(this.PageantBrowse_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(8, 79);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(83, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "Path to pageant";
            // 
            // PuttygenPath
            // 
            this.PuttygenPath.Location = new System.Drawing.Point(118, 46);
            this.PuttygenPath.Name = "PuttygenPath";
            this.PuttygenPath.Size = new System.Drawing.Size(323, 20);
            this.PuttygenPath.TabIndex = 6;
            // 
            // PuttygenBrowse
            // 
            this.PuttygenBrowse.Location = new System.Drawing.Point(463, 45);
            this.PuttygenBrowse.Name = "PuttygenBrowse";
            this.PuttygenBrowse.Size = new System.Drawing.Size(75, 23);
            this.PuttygenBrowse.TabIndex = 7;
            this.PuttygenBrowse.Text = "Browse";
            this.PuttygenBrowse.UseVisualStyleBackColor = true;
            this.PuttygenBrowse.Click += new System.EventHandler(this.PuttygenBrowse_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 49);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(85, 13);
            this.label16.TabIndex = 5;
            this.label16.Text = "Path to puttygen";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 20);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(86, 13);
            this.label15.TabIndex = 4;
            this.label15.Text = "Path to plink.exe";
            // 
            // PlinkPath
            // 
            this.PlinkPath.Location = new System.Drawing.Point(118, 17);
            this.PlinkPath.Name = "PlinkPath";
            this.PlinkPath.Size = new System.Drawing.Size(323, 20);
            this.PlinkPath.TabIndex = 2;
            // 
            // PlinkBrowse
            // 
            this.PlinkBrowse.Location = new System.Drawing.Point(463, 16);
            this.PlinkBrowse.Name = "PlinkBrowse";
            this.PlinkBrowse.Size = new System.Drawing.Size(75, 23);
            this.PlinkBrowse.TabIndex = 3;
            this.PlinkBrowse.Text = "Browse";
            this.PlinkBrowse.UseVisualStyleBackColor = true;
            this.PlinkBrowse.Click += new System.EventHandler(this.PuttyBrowse_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.OtherSsh);
            this.groupBox1.Controls.Add(this.OtherSshBrowse);
            this.groupBox1.Controls.Add(this.Other);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.OpenSSH);
            this.groupBox1.Controls.Add(this.Putty);
            this.groupBox1.Location = new System.Drawing.Point(8, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(691, 121);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Specify which ssh client to use";
            // 
            // OtherSsh
            // 
            this.OtherSsh.Location = new System.Drawing.Point(143, 80);
            this.OtherSsh.Name = "OtherSsh";
            this.OtherSsh.Size = new System.Drawing.Size(323, 20);
            this.OtherSsh.TabIndex = 4;
            // 
            // OtherSshBrowse
            // 
            this.OtherSshBrowse.Location = new System.Drawing.Point(488, 79);
            this.OtherSshBrowse.Name = "OtherSshBrowse";
            this.OtherSshBrowse.Size = new System.Drawing.Size(75, 23);
            this.OtherSshBrowse.TabIndex = 5;
            this.OtherSshBrowse.Text = "Browse";
            this.OtherSshBrowse.UseVisualStyleBackColor = true;
            this.OtherSshBrowse.Click += new System.EventHandler(this.button1_Click);
            // 
            // Other
            // 
            this.Other.AutoSize = true;
            this.Other.Location = new System.Drawing.Point(9, 81);
            this.Other.Name = "Other";
            this.Other.Size = new System.Drawing.Size(98, 17);
            this.Other.TabIndex = 3;
            this.Other.Text = "Other ssh client";
            this.Other.UseVisualStyleBackColor = true;
            this.Other.CheckedChanged += new System.EventHandler(this.Other_CheckedChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.BackColor = System.Drawing.SystemColors.Info;
            this.label18.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label18.Location = new System.Drawing.Point(121, 18);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(418, 41);
            this.label18.TabIndex = 2;
            this.label18.Text = resources.GetString("label18.Text");
            // 
            // OpenSSH
            // 
            this.OpenSSH.AutoSize = true;
            this.OpenSSH.Location = new System.Drawing.Point(9, 50);
            this.OpenSSH.Name = "OpenSSH";
            this.OpenSSH.Size = new System.Drawing.Size(73, 17);
            this.OpenSSH.TabIndex = 1;
            this.OpenSSH.Text = "OpenSSH";
            this.OpenSSH.UseVisualStyleBackColor = true;
            this.OpenSSH.CheckedChanged += new System.EventHandler(this.OpenSSH_CheckedChanged);
            // 
            // Putty
            // 
            this.Putty.AutoSize = true;
            this.Putty.Checked = true;
            this.Putty.Location = new System.Drawing.Point(9, 20);
            this.Putty.Name = "Putty";
            this.Putty.Size = new System.Drawing.Size(59, 17);
            this.Putty.TabIndex = 0;
            this.Putty.TabStop = true;
            this.Putty.Text = "PuTTY";
            this.Putty.UseVisualStyleBackColor = true;
            this.Putty.CheckedChanged += new System.EventHandler(this.Putty_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Size = new System.Drawing.Size(718, 442);
            this.splitContainer1.SplitterDistance = 409;
            this.splitContainer1.TabIndex = 1;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(639, 2);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // directorySearcher1
            // 
            this.directorySearcher1.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            // 
            // directorySearcher2
            // 
            this.directorySearcher2.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher2.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher2.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(63, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(175, 39);
            this.label10.TabIndex = 19;
            this.label10.Text = "You need to set the correct path to \r\ngit.cmd before you can change\r\nany global s" +
                "etting.\r\n";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::GitUI.Properties.Resources.error;
            this.pictureBox2.Location = new System.Drawing.Point(3, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(54, 50);
            this.pictureBox2.TabIndex = 18;
            this.pictureBox2.TabStop = false;
            // 
            // repositoryBindingSource
            // 
            this.repositoryBindingSource.DataSource = typeof(GitCommands.Repository);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(718, 442);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.FormSettigns_Load);
            this.Shown += new System.EventHandler(this.FormSettigns_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettigns_FormClosing);
            this.LocalSettings.ResumeLayout(false);
            this.LocalSettings.PerformLayout();
            this.InvalidGitPathLocal.ResumeLayout(false);
            this.InvalidGitPathLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.TabPageGitExtensions.ResumeLayout(false);
            this.TabPageGitExtensions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._MaxCommits)).EndInit();
            this.StartPage.ResumeLayout(false);
            this.AppearancePage.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._DaysToCacheImages)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._authorImageSize)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.GlobalSettingsPage.ResumeLayout(false);
            this.GlobalSettingsPage.PerformLayout();
            this.InvalidGitPathGlobal.ResumeLayout(false);
            this.InvalidGitPathGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.Ssh.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage LocalSettings;
        private System.Windows.Forms.TextBox UserEmail;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TabPage GlobalSettingsPage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox GlobalUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox GlobalUserEmail;
        private System.Windows.Forms.TextBox Editor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox GlobalEditor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox KeepMergeBackup;
        private System.Windows.Forms.CheckBox GlobalKeepMergeBackup;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button GitExtensionsInstall;
        private System.Windows.Forms.Button ShellExtensionsRegistered;
        private System.Windows.Forms.Button UserNameSet;
        private System.Windows.Forms.Button DiffTool;
        private System.Windows.Forms.Button GitFound;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox CheckAtStartup;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.TabPage TabPageGitExtensions;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown _MaxCommits;
        private System.Windows.Forms.Button BrowseGitPath;
        private System.Windows.Forms.TextBox GitPath;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button BrowseGitBinPath;
        private System.Windows.Forms.TextBox GitBinPath;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button GitBinFound;
        private System.Windows.Forms.TabPage Ssh;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button PlinkBrowse;
        private System.Windows.Forms.TextBox PlinkPath;
        private System.Windows.Forms.RadioButton OpenSSH;
        private System.Windows.Forms.RadioButton Putty;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox PageantPath;
        private System.Windows.Forms.Button PageantBrowse;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox PuttygenPath;
        private System.Windows.Forms.Button PuttygenBrowse;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox OtherSsh;
        private System.Windows.Forms.Button OtherSshBrowse;
        private System.Windows.Forms.RadioButton Other;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.DirectoryServices.DirectorySearcher directorySearcher2;
        private System.Windows.Forms.Button SshConfig;
        private System.Windows.Forms.CheckBox AutostartPageant;
        private System.Windows.Forms.Label PathToKDiff3;
        private System.Windows.Forms.TextBox MergetoolPath;
        private System.Windows.Forms.ComboBox GlobalMergeTool;
        private System.Windows.Forms.ComboBox MergeTool;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button BrowseMergeTool;
        private System.Windows.Forms.ComboBox MergeToolCmd;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox CloseProcessDialog;
        private System.Windows.Forms.CheckBox ShowRevisionGraph;
        private System.Windows.Forms.CheckBox ShowGitCommandLine;
        private System.Windows.Forms.CheckBox UseFastChecks;
        private System.Windows.Forms.Label NoGitRepo;
		private System.Windows.Forms.CheckBox ShowRelativeDate;
        private System.Windows.Forms.Panel InvalidGitPathGlobal;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel InvalidGitPathLocal;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ComboBox Dictionary;
		private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox SmtpServer;
		private System.Windows.Forms.Label lblGlobalAutoCRLF;
		private System.Windows.Forms.ComboBox GlobalAutoCRLF;
		private System.Windows.Forms.Label lblLocalAutoCRLF;
		private System.Windows.Forms.ComboBox LocalAutoCRLF;
        private System.Windows.Forms.Label EncodingLabel;
        private System.Windows.Forms.ComboBox _Encoding;
        private System.Windows.Forms.TabPage AppearancePage;
        private System.Windows.Forms.Label _RevisionGraphColorLabel;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label _RevisionGraphColorSelected;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label _ColorSectionLabel;
        private System.Windows.Forms.Label _ColorRemovedLine;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label _ColorAddedLineLabel;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label _ColorRemoteBranchLabel;
        private System.Windows.Forms.Label _ColorOtherLabel;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label _ColorTagLabel;
        private System.Windows.Forms.Label _ColorBranchLabel;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.CheckBox FollowRenamesInFileHistory;
        private System.Windows.Forms.ComboBox GlobalDiffTool;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Button BrowseDiffTool;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox DifftoolPath;
        private System.Windows.Forms.Button DiffTool2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton GreenIcon;
        private System.Windows.Forms.RadioButton PurpleIcon;
        private System.Windows.Forms.RadioButton BlueIcon;
        private System.Windows.Forms.RadioButton DefaultIcon;
        private System.Windows.Forms.RadioButton YellowIcon;
        private System.Windows.Forms.RadioButton RedIcon;
        private System.Windows.Forms.RadioButton RandomIcon;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label _ColorRemovedLineDiffLabel;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label _ColorAddedLineDiffLabel;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox ShowAuthorGravatar;
        private System.Windows.Forms.Button ClearImageCache;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.NumericUpDown _authorImageSize;
        private System.Windows.Forms.TabPage StartPage;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private DashboardEditor dashboardEditor1;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.NumericUpDown _DaysToCacheImages;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Button DiffToolCmdSuggest;
        private System.Windows.Forms.ComboBox DifftoolCmd;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.ComboBox Language;
        private System.Windows.Forms.Label label49;

    }
}