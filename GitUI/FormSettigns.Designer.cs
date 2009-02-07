namespace GitUI
{
    partial class FormSettigns
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettigns));
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label20 = new System.Windows.Forms.Label();
            this.MergeTool = new System.Windows.Forms.ComboBox();
            this.KeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.Editor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.UserEmail = new System.Windows.Forms.TextBox();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
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
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.GlobalSettingsPage = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.MergeToolCmd = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.BrowseMergeTool = new System.Windows.Forms.Button();
            this.GlobalMergeTool = new System.Windows.Forms.ComboBox();
            this.PathToKDiff3 = new System.Windows.Forms.Label();
            this.MergetoolPath = new System.Windows.Forms.TextBox();
            this.GlobalKeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
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
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.TabPageGitExtensions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxCommits)).BeginInit();
            this.GlobalSettingsPage.SuspendLayout();
            this.Ssh.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label20);
            this.tabPage1.Controls.Add(this.MergeTool);
            this.tabPage1.Controls.Add(this.KeepMergeBackup);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.Editor);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.UserEmail);
            this.tabPage1.Controls.Add(this.UserName);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(655, 299);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Local settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.BackColor = System.Drawing.SystemColors.Info;
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label20.Location = new System.Drawing.Point(335, 8);
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
            this.KeepMergeBackup.Location = new System.Drawing.Point(187, 126);
            this.KeepMergeBackup.Name = "KeepMergeBackup";
            this.KeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.KeepMergeBackup.TabIndex = 9;
            this.KeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 126);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(156, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Keep backup (.orig) after merge";
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
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.TabPageGitExtensions);
            this.tabControl1.Controls.Add(this.GlobalSettingsPage);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.Ssh);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(663, 325);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
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
            this.tabPage3.Size = new System.Drawing.Size(655, 299);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Checklist";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // SshConfig
            // 
            this.SshConfig.BackColor = System.Drawing.Color.Gray;
            this.SshConfig.FlatAppearance.BorderSize = 0;
            this.SshConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.SshConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SshConfig.Location = new System.Drawing.Point(9, 203);
            this.SshConfig.Name = "SshConfig";
            this.SshConfig.Size = new System.Drawing.Size(631, 23);
            this.SshConfig.TabIndex = 10;
            this.SshConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SshConfig.UseVisualStyleBackColor = false;
            this.SshConfig.Click += new System.EventHandler(this.SshConfig_Click);
            // 
            // GitBinFound
            // 
            this.GitBinFound.BackColor = System.Drawing.Color.Gray;
            this.GitBinFound.FlatAppearance.BorderSize = 0;
            this.GitBinFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitBinFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitBinFound.Location = new System.Drawing.Point(9, 58);
            this.GitBinFound.Name = "GitBinFound";
            this.GitBinFound.Size = new System.Drawing.Size(631, 23);
            this.GitBinFound.TabIndex = 9;
            this.GitBinFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitBinFound.UseVisualStyleBackColor = false;
            this.GitBinFound.Click += new System.EventHandler(this.GitBinFound_Click);
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(540, 273);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(100, 23);
            this.Rescan.TabIndex = 8;
            this.Rescan.Text = "Save and rescan";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // CheckAtStartup
            // 
            this.CheckAtStartup.AutoSize = true;
            this.CheckAtStartup.Location = new System.Drawing.Point(174, 277);
            this.CheckAtStartup.Name = "CheckAtStartup";
            this.CheckAtStartup.Size = new System.Drawing.Size(360, 17);
            this.CheckAtStartup.TabIndex = 7;
            this.CheckAtStartup.Text = "Check settings at startup (disables automaticly if all settings are correct)";
            this.CheckAtStartup.UseVisualStyleBackColor = true;
            this.CheckAtStartup.CheckedChanged += new System.EventHandler(this.CheckAtStartup_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(550, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "The checklist below validates the basic settings needed for GitExtensions to work" +
                " properly. (click on an item to fix it)";
            // 
            // GitFound
            // 
            this.GitFound.BackColor = System.Drawing.Color.Gray;
            this.GitFound.FlatAppearance.BorderSize = 0;
            this.GitFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitFound.Location = new System.Drawing.Point(9, 29);
            this.GitFound.Name = "GitFound";
            this.GitFound.Size = new System.Drawing.Size(631, 23);
            this.GitFound.TabIndex = 5;
            this.GitFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitFound.UseVisualStyleBackColor = false;
            this.GitFound.Click += new System.EventHandler(this.GitFound_Click);
            // 
            // DiffTool
            // 
            this.DiffTool.BackColor = System.Drawing.Color.Gray;
            this.DiffTool.FlatAppearance.BorderSize = 0;
            this.DiffTool.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.DiffTool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiffTool.Location = new System.Drawing.Point(9, 145);
            this.DiffTool.Name = "DiffTool";
            this.DiffTool.Size = new System.Drawing.Size(631, 23);
            this.DiffTool.TabIndex = 4;
            this.DiffTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool.UseVisualStyleBackColor = false;
            this.DiffTool.Click += new System.EventHandler(this.DiffTool_Click);
            // 
            // UserNameSet
            // 
            this.UserNameSet.BackColor = System.Drawing.Color.Gray;
            this.UserNameSet.FlatAppearance.BorderSize = 0;
            this.UserNameSet.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.UserNameSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserNameSet.Location = new System.Drawing.Point(9, 116);
            this.UserNameSet.Name = "UserNameSet";
            this.UserNameSet.Size = new System.Drawing.Size(631, 23);
            this.UserNameSet.TabIndex = 3;
            this.UserNameSet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UserNameSet.UseVisualStyleBackColor = false;
            this.UserNameSet.Click += new System.EventHandler(this.UserNameSet_Click);
            // 
            // ShellExtensionsRegistered
            // 
            this.ShellExtensionsRegistered.BackColor = System.Drawing.Color.Gray;
            this.ShellExtensionsRegistered.FlatAppearance.BorderSize = 0;
            this.ShellExtensionsRegistered.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ShellExtensionsRegistered.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShellExtensionsRegistered.Location = new System.Drawing.Point(9, 87);
            this.ShellExtensionsRegistered.Name = "ShellExtensionsRegistered";
            this.ShellExtensionsRegistered.Size = new System.Drawing.Size(631, 23);
            this.ShellExtensionsRegistered.TabIndex = 2;
            this.ShellExtensionsRegistered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ShellExtensionsRegistered.UseVisualStyleBackColor = false;
            this.ShellExtensionsRegistered.Click += new System.EventHandler(this.ShellExtensionsRegistered_Click);
            // 
            // GitExtensionsInstall
            // 
            this.GitExtensionsInstall.BackColor = System.Drawing.Color.Gray;
            this.GitExtensionsInstall.FlatAppearance.BorderSize = 0;
            this.GitExtensionsInstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitExtensionsInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitExtensionsInstall.Location = new System.Drawing.Point(9, 174);
            this.GitExtensionsInstall.Name = "GitExtensionsInstall";
            this.GitExtensionsInstall.Size = new System.Drawing.Size(631, 23);
            this.GitExtensionsInstall.TabIndex = 1;
            this.GitExtensionsInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitExtensionsInstall.UseVisualStyleBackColor = false;
            this.GitExtensionsInstall.Click += new System.EventHandler(this.GitExtensionsInstall_Click);
            // 
            // TabPageGitExtensions
            // 
            this.TabPageGitExtensions.Controls.Add(this.BrowseGitBinPath);
            this.TabPageGitExtensions.Controls.Add(this.GitBinPath);
            this.TabPageGitExtensions.Controls.Add(this.label14);
            this.TabPageGitExtensions.Controls.Add(this.BrowseGitPath);
            this.TabPageGitExtensions.Controls.Add(this.GitPath);
            this.TabPageGitExtensions.Controls.Add(this.label13);
            this.TabPageGitExtensions.Controls.Add(this.MaxCommits);
            this.TabPageGitExtensions.Controls.Add(this.label12);
            this.TabPageGitExtensions.Location = new System.Drawing.Point(4, 22);
            this.TabPageGitExtensions.Name = "TabPageGitExtensions";
            this.TabPageGitExtensions.Size = new System.Drawing.Size(655, 299);
            this.TabPageGitExtensions.TabIndex = 3;
            this.TabPageGitExtensions.Text = "Git extensions";
            this.TabPageGitExtensions.UseVisualStyleBackColor = true;
            this.TabPageGitExtensions.Click += new System.EventHandler(this.TabPageGitExtensions_Click);
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.Location = new System.Drawing.Point(571, 32);
            this.BrowseGitBinPath.Name = "BrowseGitBinPath";
            this.BrowseGitBinPath.Size = new System.Drawing.Size(75, 23);
            this.BrowseGitBinPath.TabIndex = 8;
            this.BrowseGitBinPath.Text = "Browse";
            this.BrowseGitBinPath.UseVisualStyleBackColor = true;
            this.BrowseGitBinPath.Click += new System.EventHandler(this.BrowseGitBinPath_Click);
            // 
            // GitBinPath
            // 
            this.GitBinPath.Location = new System.Drawing.Point(305, 36);
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
            this.BrowseGitPath.Location = new System.Drawing.Point(571, 6);
            this.BrowseGitPath.Name = "BrowseGitPath";
            this.BrowseGitPath.Size = new System.Drawing.Size(75, 23);
            this.BrowseGitPath.TabIndex = 5;
            this.BrowseGitPath.Text = "Browse";
            this.BrowseGitPath.UseVisualStyleBackColor = true;
            this.BrowseGitPath.Click += new System.EventHandler(this.BrowseGitPath_Click);
            // 
            // GitPath
            // 
            this.GitPath.Location = new System.Drawing.Point(305, 10);
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
            this.label13.Size = new System.Drawing.Size(273, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Path to git.cmd (leave empty when git.cmd is in the path)";
            // 
            // MaxCommits
            // 
            this.MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.MaxCommits.Location = new System.Drawing.Point(305, 64);
            this.MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.MaxCommits.Name = "MaxCommits";
            this.MaxCommits.Size = new System.Drawing.Size(123, 20);
            this.MaxCommits.TabIndex = 2;
            this.MaxCommits.Value = new decimal(new int[] {
            2000,
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
            this.label12.Text = "Limit number of commits that will be loaded in list at startup.\r\nOther commits wi" +
                "ll be loaded when needed. Lower number \r\nresult is shorter startup time, but slo" +
                "wer scrolling.";
            // 
            // GlobalSettingsPage
            // 
            this.GlobalSettingsPage.Controls.Add(this.button1);
            this.GlobalSettingsPage.Controls.Add(this.MergeToolCmd);
            this.GlobalSettingsPage.Controls.Add(this.label19);
            this.GlobalSettingsPage.Controls.Add(this.BrowseMergeTool);
            this.GlobalSettingsPage.Controls.Add(this.GlobalMergeTool);
            this.GlobalSettingsPage.Controls.Add(this.PathToKDiff3);
            this.GlobalSettingsPage.Controls.Add(this.MergetoolPath);
            this.GlobalSettingsPage.Controls.Add(this.GlobalKeepMergeBackup);
            this.GlobalSettingsPage.Controls.Add(this.label10);
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
            this.GlobalSettingsPage.Size = new System.Drawing.Size(655, 299);
            this.GlobalSettingsPage.TabIndex = 1;
            this.GlobalSettingsPage.Text = "Global settings";
            this.GlobalSettingsPage.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(466, 145);
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
                " /theirs:\"$REMOTE\" /merged:\"$MERGED\""});
            this.MergeToolCmd.Location = new System.Drawing.Point(113, 147);
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
            this.BrowseMergeTool.Location = new System.Drawing.Point(466, 117);
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
            "kdiff3",
            "p4merge",
            "TortoiseMerge"});
            this.GlobalMergeTool.Location = new System.Drawing.Point(113, 92);
            this.GlobalMergeTool.Name = "GlobalMergeTool";
            this.GlobalMergeTool.Size = new System.Drawing.Size(164, 21);
            this.GlobalMergeTool.TabIndex = 12;
            this.GlobalMergeTool.SelectedIndexChanged += new System.EventHandler(this.GlobalMergeTool_SelectedIndexChanged);
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
            this.MergetoolPath.Location = new System.Drawing.Point(113, 120);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(347, 20);
            this.MergetoolPath.TabIndex = 10;
            // 
            // GlobalKeepMergeBackup
            // 
            this.GlobalKeepMergeBackup.AutoSize = true;
            this.GlobalKeepMergeBackup.Location = new System.Drawing.Point(183, 179);
            this.GlobalKeepMergeBackup.Name = "GlobalKeepMergeBackup";
            this.GlobalKeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.GlobalKeepMergeBackup.TabIndex = 9;
            this.GlobalKeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 179);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(156, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Keep backup (.orig) after merge";
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
            this.GlobalEditor.Location = new System.Drawing.Point(113, 65);
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
            this.GlobalUserEmail.Location = new System.Drawing.Point(113, 37);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(164, 20);
            this.GlobalUserEmail.TabIndex = 3;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(113, 8);
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
            this.Ssh.Size = new System.Drawing.Size(655, 299);
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
            this.groupBox2.Location = new System.Drawing.Point(8, 123);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(639, 126);
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
            this.AutostartPageant.Size = new System.Drawing.Size(416, 17);
            this.AutostartPageant.TabIndex = 11;
            this.AutostartPageant.Text = "Automaticly start authentication client when a private key is configured for a re" +
                "mote";
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
            this.groupBox1.Size = new System.Drawing.Size(639, 111);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Specify which ssh client to use";
            // 
            // OtherSsh
            // 
            this.OtherSsh.Location = new System.Drawing.Point(118, 80);
            this.OtherSsh.Name = "OtherSsh";
            this.OtherSsh.Size = new System.Drawing.Size(323, 20);
            this.OtherSsh.TabIndex = 4;
            // 
            // OtherSshBrowse
            // 
            this.OtherSshBrowse.Location = new System.Drawing.Point(463, 79);
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
            this.label18.Location = new System.Drawing.Point(118, 19);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(518, 54);
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
            this.splitContainer1.Size = new System.Drawing.Size(663, 358);
            this.splitContainer1.SplitterDistance = 325;
            this.splitContainer1.TabIndex = 1;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(584, 3);
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
            // FormSettigns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 358);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettigns";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.FormSettigns_Load);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.TabPageGitExtensions.ResumeLayout(false);
            this.TabPageGitExtensions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxCommits)).EndInit();
            this.GlobalSettingsPage.ResumeLayout(false);
            this.GlobalSettingsPage.PerformLayout();
            this.Ssh.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage1;
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
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox KeepMergeBackup;
        private System.Windows.Forms.CheckBox GlobalKeepMergeBackup;
        private System.Windows.Forms.Label label10;
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
        private System.Windows.Forms.NumericUpDown MaxCommits;
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

    }
}