using GitCommands.Repository;
using System.Windows.Forms;

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
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.localAutoCrlfFalse = new System.Windows.Forms.RadioButton();
            this.localAutoCrlfInput = new System.Windows.Forms.RadioButton();
            this.localAutoCrlfTrue = new System.Windows.Forms.RadioButton();
            this.label30 = new System.Windows.Forms.Label();
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
            this.translationConfig_Fix = new System.Windows.Forms.Button();
            this.SshConfig_Fix = new System.Windows.Forms.Button();
            this.GitExtensionsInstall_Fix = new System.Windows.Forms.Button();
            this.GitBinFound_Fix = new System.Windows.Forms.Button();
            this.ShellExtensionsRegistered_Fix = new System.Windows.Forms.Button();
            this.DiffTool2_Fix = new System.Windows.Forms.Button();
            this.DiffTool_Fix = new System.Windows.Forms.Button();
            this.UserNameSet_Fix = new System.Windows.Forms.Button();
            this.GitFound_Fix = new System.Windows.Forms.Button();
            this.translationConfig = new System.Windows.Forms.Button();
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
            this.TabPageGit = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.otherHomeBrowse = new System.Windows.Forms.Button();
            this.otherHomeDir = new System.Windows.Forms.TextBox();
            this.otherHome = new System.Windows.Forms.RadioButton();
            this.userprofileHome = new System.Windows.Forms.RadioButton();
            this.defaultHome = new System.Windows.Forms.RadioButton();
            this.label51 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label50 = new System.Windows.Forms.Label();
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.TabPageGitExtensions = new System.Windows.Forms.TabPage();
            this.downloadDictionary = new System.Windows.Forms.LinkLabel();
            this.label52 = new System.Windows.Forms.Label();
            this.ShowStashCountInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.label26 = new System.Windows.Forms.Label();
            this.ShowCurrentChangesInRevisionGraph = new System.Windows.Forms.CheckBox();
            this.showErrorsWhenStagingFilesLabel = new System.Windows.Forms.Label();
            this.showErrorsWhenStagingFiles = new System.Windows.Forms.CheckBox();
            this.showGitStatusInToolbarLabel = new System.Windows.Forms.Label();
            this.ShowGitStatusInToolbar = new System.Windows.Forms.CheckBox();
            this.RevisionGridQuickSearchTimeout = new System.Windows.Forms.NumericUpDown();
            this.label24 = new System.Windows.Forms.Label();
            this.helpTranslate = new System.Windows.Forms.LinkLabel();
            this.Language = new System.Windows.Forms.ComboBox();
            this.label49 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.FollowRenamesInFileHistory = new System.Windows.Forms.CheckBox();
            this.label39 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.EncodingLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Encoding = new System.Windows.Forms.ComboBox();
            this.Dictionary = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.ShowRelativeDate = new System.Windows.Forms.CheckBox();
            this.UseFastChecks = new System.Windows.Forms.CheckBox();
            this.ShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.CloseProcessDialog = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.StartPage = new System.Windows.Forms.TabPage();
            this.dashboardEditor1 = new GitUI.DashboardEditor();
            this.AppearancePage = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.noImageService = new System.Windows.Forms.ComboBox();
            this.label53 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_DaysToCacheImages = new System.Windows.Forms.NumericUpDown();
            this.label46 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_authorImageSize = new System.Windows.Forms.NumericUpDown();
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
            this.DrawNonRelativesTextGray = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesGray = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_ColorGraphLabel = new System.Windows.Forms.Label();
            this.StripedBanchChange = new System.Windows.Forms.CheckBox();
            this.BranchBorders = new System.Windows.Forms.CheckBox();
            this.MulticolorBranches = new System.Windows.Forms.CheckBox();
            this.label33 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemoteBranchLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorOtherLabel = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorTagLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorBranchLabel = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label43 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineDiffLabel = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorSectionLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLine = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineLabel = new System.Windows.Forms.Label();
            this.GlobalSettingsPage = new System.Windows.Forms.TabPage();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.globalAutoCrlfFalse = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfInput = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfTrue = new System.Windows.Forms.RadioButton();
            this.DiffToolCmdSuggest = new System.Windows.Forms.Button();
            this.DifftoolCmd = new System.Windows.Forms.ComboBox();
            this.label48 = new System.Windows.Forms.Label();
            this.BrowseDiffTool = new System.Windows.Forms.Button();
            this.label42 = new System.Windows.Forms.Label();
            this.DifftoolPath = new System.Windows.Forms.TextBox();
            this.GlobalDiffTool = new System.Windows.Forms.ComboBox();
            this.label41 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
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
            this.GlobalEditor = new System.Windows.Forms.ComboBox();
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
            this.scriptsTab = new System.Windows.Forms.TabPage();
            this.helpLabel = new System.Windows.Forms.Label();
            this.inMenuCheckBox = new System.Windows.Forms.CheckBox();
            this.argumentsLabel = new System.Windows.Forms.Label();
            this.commandLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.browseScriptButton = new System.Windows.Forms.Button();
            this.cancelScriptButton = new System.Windows.Forms.Button();
            this.saveScriptButton = new System.Windows.Forms.Button();
            this.argumentsTextBox = new System.Windows.Forms.RichTextBox();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.removeScriptButton = new System.Windows.Forms.Button();
            this.editScriptButton = new System.Windows.Forms.Button();
            this.addScriptButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.ScriptList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.tabPageHotkeys = new System.Windows.Forms.TabPage();
            this.controlHotkeys = new GitUI.Hotkey.ControlHotkeys();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Ok = new System.Windows.Forms.Button();
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.directorySearcher2 = new System.DirectoryServices.DirectorySearcher();
            this.label10 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.LocalSettings.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.InvalidGitPathLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.TabPageGit.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.TabPageGitExtensions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.StartPage.SuspendLayout();
            this.AppearancePage.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_authorImageSize)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.GlobalSettingsPage.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.Ssh.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.scriptsTab.SuspendLayout();
            this.tabPageHotkeys.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // LocalSettings
            // 
            this.LocalSettings.Controls.Add(this.groupBox10);
            this.LocalSettings.Controls.Add(this.label30);
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
            this.LocalSettings.Size = new System.Drawing.Size(824, 431);
            this.LocalSettings.TabIndex = 0;
            this.LocalSettings.Text = "Local settings";
            this.LocalSettings.UseVisualStyleBackColor = true;
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox10.Controls.Add(this.localAutoCrlfFalse);
            this.groupBox10.Controls.Add(this.localAutoCrlfInput);
            this.groupBox10.Controls.Add(this.localAutoCrlfTrue);
            this.groupBox10.Location = new System.Drawing.Point(13, 147);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(803, 105);
            this.groupBox10.TabIndex = 32;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Line endings";
            // 
            // localAutoCrlfFalse
            // 
            this.localAutoCrlfFalse.AutoSize = true;
            this.localAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.localAutoCrlfFalse.Name = "localAutoCrlfFalse";
            this.localAutoCrlfFalse.Size = new System.Drawing.Size(319, 17);
            this.localAutoCrlfFalse.TabIndex = 2;
            this.localAutoCrlfFalse.TabStop = true;
            this.localAutoCrlfFalse.Text = "Checkout as-is, commit as-is (\"core.autocrlf\"  is set to \"false\")";
            this.localAutoCrlfFalse.UseVisualStyleBackColor = true;
            // 
            // localAutoCrlfInput
            // 
            this.localAutoCrlfInput.AutoSize = true;
            this.localAutoCrlfInput.Location = new System.Drawing.Point(5, 48);
            this.localAutoCrlfInput.Name = "localAutoCrlfInput";
            this.localAutoCrlfInput.Size = new System.Drawing.Size(405, 17);
            this.localAutoCrlfInput.TabIndex = 1;
            this.localAutoCrlfInput.TabStop = true;
            this.localAutoCrlfInput.Text = "Checkout as-is, commit Unix-style line endings (\"core.autocrlf\"  is set to \"input" +
                "\")";
            this.localAutoCrlfInput.UseVisualStyleBackColor = true;
            // 
            // localAutoCrlfTrue
            // 
            this.localAutoCrlfTrue.AutoSize = true;
            this.localAutoCrlfTrue.Location = new System.Drawing.Point(5, 22);
            this.localAutoCrlfTrue.Name = "localAutoCrlfTrue";
            this.localAutoCrlfTrue.Size = new System.Drawing.Size(449, 17);
            this.localAutoCrlfTrue.TabIndex = 0;
            this.localAutoCrlfTrue.TabStop = true;
            this.localAutoCrlfTrue.Text = "Checkout Windows-style, commit Unix-style line endings (\"core.autocrlf\"  is set t" +
                "o \"true\")";
            this.localAutoCrlfTrue.UseVisualStyleBackColor = true;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(8, 127);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(161, 13);
            this.label30.TabIndex = 23;
            this.label30.Text = "Keep backup (.orig) after merge";
            // 
            // InvalidGitPathLocal
            // 
            this.InvalidGitPathLocal.BackColor = System.Drawing.SystemColors.Info;
            this.InvalidGitPathLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InvalidGitPathLocal.Controls.Add(this.label21);
            this.InvalidGitPathLocal.Controls.Add(this.pictureBox3);
            this.InvalidGitPathLocal.Location = new System.Drawing.Point(423, 8);
            this.InvalidGitPathLocal.Name = "InvalidGitPathLocal";
            this.InvalidGitPathLocal.Size = new System.Drawing.Size(279, 65);
            this.InvalidGitPathLocal.TabIndex = 20;
            this.InvalidGitPathLocal.Visible = false;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(74, 6);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(180, 39);
            this.label21.TabIndex = 19;
            this.label21.Text = "You need to set the correct path to \r\ngit before you can change\r\nlocal settings.\r" +
                "\n";
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
            this.NoGitRepo.ForeColor = System.Drawing.Color.Red;
            this.NoGitRepo.Location = new System.Drawing.Point(460, 76);
            this.NoGitRepo.Name = "NoGitRepo";
            this.NoGitRepo.Size = new System.Drawing.Size(111, 13);
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
            this.label20.Size = new System.Drawing.Size(152, 54);
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
            this.MergeTool.Location = new System.Drawing.Point(150, 94);
            this.MergeTool.Name = "MergeTool";
            this.MergeTool.Size = new System.Drawing.Size(159, 21);
            this.MergeTool.TabIndex = 10;
            // 
            // KeepMergeBackup
            // 
            this.KeepMergeBackup.AutoSize = true;
            this.KeepMergeBackup.Checked = true;
            this.KeepMergeBackup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.KeepMergeBackup.Location = new System.Drawing.Point(472, 127);
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
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Mergetool";
            // 
            // Editor
            // 
            this.Editor.Location = new System.Drawing.Point(150, 67);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(304, 21);
            this.Editor.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Editor";
            // 
            // UserEmail
            // 
            this.UserEmail.Location = new System.Drawing.Point(150, 40);
            this.UserEmail.Name = "UserEmail";
            this.UserEmail.Size = new System.Drawing.Size(280, 21);
            this.UserEmail.TabIndex = 3;
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(150, 12);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(280, 21);
            this.UserName.TabIndex = 1;
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
            this.SmtpServer.Location = new System.Drawing.Point(396, 109);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(242, 21);
            this.SmtpServer.TabIndex = 17;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(8, 112);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(205, 13);
            this.label23.TabIndex = 18;
            this.label23.Text = "Smtp server for sending patches by email";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.TabPageGit);
            this.tabControl1.Controls.Add(this.TabPageGitExtensions);
            this.tabControl1.Controls.Add(this.StartPage);
            this.tabControl1.Controls.Add(this.AppearancePage);
            this.tabControl1.Controls.Add(this.GlobalSettingsPage);
            this.tabControl1.Controls.Add(this.LocalSettings);
            this.tabControl1.Controls.Add(this.Ssh);
            this.tabControl1.Controls.Add(this.scriptsTab);
            this.tabControl1.Controls.Add(this.tabPageHotkeys);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(832, 457);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.translationConfig_Fix);
            this.tabPage3.Controls.Add(this.SshConfig_Fix);
            this.tabPage3.Controls.Add(this.GitExtensionsInstall_Fix);
            this.tabPage3.Controls.Add(this.GitBinFound_Fix);
            this.tabPage3.Controls.Add(this.ShellExtensionsRegistered_Fix);
            this.tabPage3.Controls.Add(this.DiffTool2_Fix);
            this.tabPage3.Controls.Add(this.DiffTool_Fix);
            this.tabPage3.Controls.Add(this.UserNameSet_Fix);
            this.tabPage3.Controls.Add(this.GitFound_Fix);
            this.tabPage3.Controls.Add(this.translationConfig);
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
            this.tabPage3.Size = new System.Drawing.Size(824, 431);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Checklist";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // translationConfig_Fix
            // 
            this.translationConfig_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.translationConfig_Fix.Location = new System.Drawing.Point(722, 303);
            this.translationConfig_Fix.Name = "translationConfig_Fix";
            this.translationConfig_Fix.Size = new System.Drawing.Size(91, 25);
            this.translationConfig_Fix.TabIndex = 21;
            this.translationConfig_Fix.Text = "Repair";
            this.translationConfig_Fix.UseVisualStyleBackColor = true;
            this.translationConfig_Fix.Visible = false;
            this.translationConfig_Fix.Click += new System.EventHandler(this.translationConfig_Click);
            // 
            // SshConfig_Fix
            // 
            this.SshConfig_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SshConfig_Fix.Location = new System.Drawing.Point(722, 269);
            this.SshConfig_Fix.Name = "SshConfig_Fix";
            this.SshConfig_Fix.Size = new System.Drawing.Size(91, 25);
            this.SshConfig_Fix.TabIndex = 20;
            this.SshConfig_Fix.Text = "Repair";
            this.SshConfig_Fix.UseVisualStyleBackColor = true;
            this.SshConfig_Fix.Visible = false;
            this.SshConfig_Fix.Click += new System.EventHandler(this.SshConfig_Click);
            // 
            // GitExtensionsInstall_Fix
            // 
            this.GitExtensionsInstall_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GitExtensionsInstall_Fix.Location = new System.Drawing.Point(722, 235);
            this.GitExtensionsInstall_Fix.Name = "GitExtensionsInstall_Fix";
            this.GitExtensionsInstall_Fix.Size = new System.Drawing.Size(91, 25);
            this.GitExtensionsInstall_Fix.TabIndex = 19;
            this.GitExtensionsInstall_Fix.Text = "Repair";
            this.GitExtensionsInstall_Fix.UseVisualStyleBackColor = true;
            this.GitExtensionsInstall_Fix.Visible = false;
            this.GitExtensionsInstall_Fix.Click += new System.EventHandler(this.GitExtensionsInstall_Click);
            // 
            // GitBinFound_Fix
            // 
            this.GitBinFound_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GitBinFound_Fix.Location = new System.Drawing.Point(722, 201);
            this.GitBinFound_Fix.Name = "GitBinFound_Fix";
            this.GitBinFound_Fix.Size = new System.Drawing.Size(91, 25);
            this.GitBinFound_Fix.TabIndex = 18;
            this.GitBinFound_Fix.Text = "Repair";
            this.GitBinFound_Fix.UseVisualStyleBackColor = true;
            this.GitBinFound_Fix.Visible = false;
            this.GitBinFound_Fix.Click += new System.EventHandler(this.GitBinFound_Click);
            // 
            // ShellExtensionsRegistered_Fix
            // 
            this.ShellExtensionsRegistered_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShellExtensionsRegistered_Fix.Location = new System.Drawing.Point(722, 167);
            this.ShellExtensionsRegistered_Fix.Name = "ShellExtensionsRegistered_Fix";
            this.ShellExtensionsRegistered_Fix.Size = new System.Drawing.Size(91, 25);
            this.ShellExtensionsRegistered_Fix.TabIndex = 17;
            this.ShellExtensionsRegistered_Fix.Text = "Repair";
            this.ShellExtensionsRegistered_Fix.UseVisualStyleBackColor = true;
            this.ShellExtensionsRegistered_Fix.Visible = false;
            this.ShellExtensionsRegistered_Fix.Click += new System.EventHandler(this.ShellExtensionsRegistered_Click);
            // 
            // DiffTool2_Fix
            // 
            this.DiffTool2_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DiffTool2_Fix.Location = new System.Drawing.Point(722, 133);
            this.DiffTool2_Fix.Name = "DiffTool2_Fix";
            this.DiffTool2_Fix.Size = new System.Drawing.Size(91, 25);
            this.DiffTool2_Fix.TabIndex = 16;
            this.DiffTool2_Fix.Text = "Repair";
            this.DiffTool2_Fix.UseVisualStyleBackColor = true;
            this.DiffTool2_Fix.Visible = false;
            this.DiffTool2_Fix.Click += new System.EventHandler(this.DiffTool2_Click);
            // 
            // DiffTool_Fix
            // 
            this.DiffTool_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DiffTool_Fix.Location = new System.Drawing.Point(722, 99);
            this.DiffTool_Fix.Name = "DiffTool_Fix";
            this.DiffTool_Fix.Size = new System.Drawing.Size(91, 25);
            this.DiffTool_Fix.TabIndex = 15;
            this.DiffTool_Fix.Text = "Repair";
            this.DiffTool_Fix.UseVisualStyleBackColor = true;
            this.DiffTool_Fix.Visible = false;
            this.DiffTool_Fix.Click += new System.EventHandler(this.DiffTool_Click);
            // 
            // UserNameSet_Fix
            // 
            this.UserNameSet_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UserNameSet_Fix.Location = new System.Drawing.Point(722, 65);
            this.UserNameSet_Fix.Name = "UserNameSet_Fix";
            this.UserNameSet_Fix.Size = new System.Drawing.Size(91, 25);
            this.UserNameSet_Fix.TabIndex = 14;
            this.UserNameSet_Fix.Text = "Repair";
            this.UserNameSet_Fix.UseVisualStyleBackColor = true;
            this.UserNameSet_Fix.Visible = false;
            this.UserNameSet_Fix.Click += new System.EventHandler(this.UserNameSet_Click);
            // 
            // GitFound_Fix
            // 
            this.GitFound_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GitFound_Fix.Location = new System.Drawing.Point(722, 31);
            this.GitFound_Fix.Name = "GitFound_Fix";
            this.GitFound_Fix.Size = new System.Drawing.Size(91, 25);
            this.GitFound_Fix.TabIndex = 13;
            this.GitFound_Fix.Text = "Repair";
            this.GitFound_Fix.UseVisualStyleBackColor = true;
            this.GitFound_Fix.Visible = false;
            this.GitFound_Fix.Click += new System.EventHandler(this.GitFound_Click);
            // 
            // translationConfig
            // 
            this.translationConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.translationConfig.BackColor = System.Drawing.Color.Gray;
            this.translationConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.translationConfig.FlatAppearance.BorderSize = 0;
            this.translationConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.translationConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.translationConfig.Location = new System.Drawing.Point(9, 301);
            this.translationConfig.Name = "translationConfig";
            this.translationConfig.Size = new System.Drawing.Size(807, 29);
            this.translationConfig.TabIndex = 12;
            this.translationConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.translationConfig.UseVisualStyleBackColor = false;
            this.translationConfig.Visible = false;
            this.translationConfig.Click += new System.EventHandler(this.translationConfig_Click);
            // 
            // DiffTool2
            // 
            this.DiffTool2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DiffTool2.BackColor = System.Drawing.Color.Gray;
            this.DiffTool2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DiffTool2.FlatAppearance.BorderSize = 0;
            this.DiffTool2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.DiffTool2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiffTool2.Location = new System.Drawing.Point(9, 131);
            this.DiffTool2.Name = "DiffTool2";
            this.DiffTool2.Size = new System.Drawing.Size(807, 29);
            this.DiffTool2.TabIndex = 11;
            this.DiffTool2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool2.UseVisualStyleBackColor = false;
            this.DiffTool2.Visible = false;
            this.DiffTool2.Click += new System.EventHandler(this.DiffTool2_Click);
            // 
            // SshConfig
            // 
            this.SshConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SshConfig.BackColor = System.Drawing.Color.Gray;
            this.SshConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SshConfig.FlatAppearance.BorderSize = 0;
            this.SshConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.SshConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SshConfig.Location = new System.Drawing.Point(9, 267);
            this.SshConfig.Name = "SshConfig";
            this.SshConfig.Size = new System.Drawing.Size(807, 29);
            this.SshConfig.TabIndex = 10;
            this.SshConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SshConfig.UseVisualStyleBackColor = false;
            this.SshConfig.Visible = false;
            this.SshConfig.Click += new System.EventHandler(this.SshConfig_Click);
            // 
            // GitBinFound
            // 
            this.GitBinFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GitBinFound.BackColor = System.Drawing.Color.Gray;
            this.GitBinFound.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitBinFound.FlatAppearance.BorderSize = 0;
            this.GitBinFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitBinFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitBinFound.Location = new System.Drawing.Point(9, 199);
            this.GitBinFound.Name = "GitBinFound";
            this.GitBinFound.Size = new System.Drawing.Size(807, 29);
            this.GitBinFound.TabIndex = 9;
            this.GitBinFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitBinFound.UseVisualStyleBackColor = false;
            this.GitBinFound.Visible = false;
            this.GitBinFound.Click += new System.EventHandler(this.GitBinFound_Click);
            // 
            // Rescan
            // 
            this.Rescan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Rescan.Location = new System.Drawing.Point(620, 346);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(195, 25);
            this.Rescan.TabIndex = 8;
            this.Rescan.Text = "Save and rescan";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // CheckAtStartup
            // 
            this.CheckAtStartup.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.CheckAtStartup.Location = new System.Drawing.Point(12, 351);
            this.CheckAtStartup.Name = "CheckAtStartup";
            this.CheckAtStartup.Size = new System.Drawing.Size(489, 57);
            this.CheckAtStartup.TabIndex = 7;
            this.CheckAtStartup.Text = "Check settings at startup (disables automatically if all settings are correct)";
            this.CheckAtStartup.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.CheckAtStartup.UseVisualStyleBackColor = true;
            this.CheckAtStartup.CheckedChanged += new System.EventHandler(this.CheckAtStartup_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(442, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "The checklist below validates the basic settings needed for GitExtensions to work" +
                " properly.";
            // 
            // GitFound
            // 
            this.GitFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GitFound.BackColor = System.Drawing.Color.Gray;
            this.GitFound.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitFound.FlatAppearance.BorderSize = 0;
            this.GitFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitFound.Location = new System.Drawing.Point(9, 29);
            this.GitFound.Name = "GitFound";
            this.GitFound.Size = new System.Drawing.Size(807, 29);
            this.GitFound.TabIndex = 5;
            this.GitFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitFound.UseVisualStyleBackColor = false;
            this.GitFound.Visible = false;
            this.GitFound.Click += new System.EventHandler(this.GitFound_Click);
            // 
            // DiffTool
            // 
            this.DiffTool.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DiffTool.BackColor = System.Drawing.Color.Gray;
            this.DiffTool.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DiffTool.FlatAppearance.BorderSize = 0;
            this.DiffTool.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.DiffTool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiffTool.Location = new System.Drawing.Point(9, 97);
            this.DiffTool.Name = "DiffTool";
            this.DiffTool.Size = new System.Drawing.Size(807, 29);
            this.DiffTool.TabIndex = 4;
            this.DiffTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool.UseVisualStyleBackColor = false;
            this.DiffTool.Visible = false;
            this.DiffTool.Click += new System.EventHandler(this.DiffTool_Click);
            // 
            // UserNameSet
            // 
            this.UserNameSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.UserNameSet.BackColor = System.Drawing.Color.Gray;
            this.UserNameSet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.UserNameSet.FlatAppearance.BorderSize = 0;
            this.UserNameSet.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.UserNameSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserNameSet.Location = new System.Drawing.Point(9, 63);
            this.UserNameSet.Name = "UserNameSet";
            this.UserNameSet.Size = new System.Drawing.Size(807, 29);
            this.UserNameSet.TabIndex = 3;
            this.UserNameSet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UserNameSet.UseVisualStyleBackColor = false;
            this.UserNameSet.Visible = false;
            this.UserNameSet.Click += new System.EventHandler(this.UserNameSet_Click);
            // 
            // ShellExtensionsRegistered
            // 
            this.ShellExtensionsRegistered.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ShellExtensionsRegistered.BackColor = System.Drawing.Color.Gray;
            this.ShellExtensionsRegistered.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ShellExtensionsRegistered.FlatAppearance.BorderSize = 0;
            this.ShellExtensionsRegistered.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ShellExtensionsRegistered.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShellExtensionsRegistered.Location = new System.Drawing.Point(9, 165);
            this.ShellExtensionsRegistered.Name = "ShellExtensionsRegistered";
            this.ShellExtensionsRegistered.Size = new System.Drawing.Size(807, 29);
            this.ShellExtensionsRegistered.TabIndex = 2;
            this.ShellExtensionsRegistered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ShellExtensionsRegistered.UseVisualStyleBackColor = false;
            this.ShellExtensionsRegistered.Visible = false;
            this.ShellExtensionsRegistered.Click += new System.EventHandler(this.ShellExtensionsRegistered_Click);
            // 
            // GitExtensionsInstall
            // 
            this.GitExtensionsInstall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GitExtensionsInstall.BackColor = System.Drawing.Color.Gray;
            this.GitExtensionsInstall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitExtensionsInstall.FlatAppearance.BorderSize = 0;
            this.GitExtensionsInstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitExtensionsInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitExtensionsInstall.Location = new System.Drawing.Point(9, 233);
            this.GitExtensionsInstall.Name = "GitExtensionsInstall";
            this.GitExtensionsInstall.Size = new System.Drawing.Size(807, 29);
            this.GitExtensionsInstall.TabIndex = 1;
            this.GitExtensionsInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitExtensionsInstall.UseVisualStyleBackColor = false;
            this.GitExtensionsInstall.Visible = false;
            this.GitExtensionsInstall.Click += new System.EventHandler(this.GitExtensionsInstall_Click);
            // 
            // TabPageGit
            // 
            this.TabPageGit.Controls.Add(this.groupBox8);
            this.TabPageGit.Controls.Add(this.groupBox7);
            this.TabPageGit.Location = new System.Drawing.Point(4, 22);
            this.TabPageGit.Name = "TabPageGit";
            this.TabPageGit.Size = new System.Drawing.Size(824, 431);
            this.TabPageGit.TabIndex = 7;
            this.TabPageGit.Text = "Git";
            this.TabPageGit.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox8.Controls.Add(this.otherHomeBrowse);
            this.groupBox8.Controls.Add(this.otherHomeDir);
            this.groupBox8.Controls.Add(this.otherHome);
            this.groupBox8.Controls.Add(this.userprofileHome);
            this.groupBox8.Controls.Add(this.defaultHome);
            this.groupBox8.Controls.Add(this.label51);
            this.groupBox8.Location = new System.Drawing.Point(5, 108);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(813, 145);
            this.groupBox8.TabIndex = 10;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Environment";
            // 
            // otherHomeBrowse
            // 
            this.otherHomeBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.otherHomeBrowse.Location = new System.Drawing.Point(732, 100);
            this.otherHomeBrowse.Name = "otherHomeBrowse";
            this.otherHomeBrowse.Size = new System.Drawing.Size(75, 25);
            this.otherHomeBrowse.TabIndex = 10;
            this.otherHomeBrowse.Text = "Browse";
            this.otherHomeBrowse.UseVisualStyleBackColor = true;
            this.otherHomeBrowse.Click += new System.EventHandler(this.otherHomeBrowse_Click);
            // 
            // otherHomeDir
            // 
            this.otherHomeDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.otherHomeDir.Location = new System.Drawing.Point(88, 102);
            this.otherHomeDir.Name = "otherHomeDir";
            this.otherHomeDir.Size = new System.Drawing.Size(641, 21);
            this.otherHomeDir.TabIndex = 4;
            // 
            // otherHome
            // 
            this.otherHome.AutoSize = true;
            this.otherHome.Location = new System.Drawing.Point(11, 103);
            this.otherHome.Name = "otherHome";
            this.otherHome.Size = new System.Drawing.Size(53, 17);
            this.otherHome.TabIndex = 3;
            this.otherHome.TabStop = true;
            this.otherHome.Text = "Other";
            this.otherHome.UseVisualStyleBackColor = true;
            this.otherHome.CheckedChanged += new System.EventHandler(this.otherHome_CheckedChanged);
            // 
            // userprofileHome
            // 
            this.userprofileHome.AutoSize = true;
            this.userprofileHome.Location = new System.Drawing.Point(11, 80);
            this.userprofileHome.Name = "userprofileHome";
            this.userprofileHome.Size = new System.Drawing.Size(157, 17);
            this.userprofileHome.TabIndex = 2;
            this.userprofileHome.TabStop = true;
            this.userprofileHome.Text = "Set HOME to USERPROFILE";
            this.userprofileHome.UseVisualStyleBackColor = true;
            // 
            // defaultHome
            // 
            this.defaultHome.AutoSize = true;
            this.defaultHome.Location = new System.Drawing.Point(11, 57);
            this.defaultHome.Name = "defaultHome";
            this.defaultHome.Size = new System.Drawing.Size(129, 17);
            this.defaultHome.TabIndex = 1;
            this.defaultHome.TabStop = true;
            this.defaultHome.Text = "Use default for HOME";
            this.defaultHome.UseVisualStyleBackColor = true;
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(8, 19);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(568, 26);
            this.label51.TabIndex = 0;
            this.label51.Text = resources.GetString("label51.Text");
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Controls.Add(this.label50);
            this.groupBox7.Controls.Add(this.BrowseGitBinPath);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.label14);
            this.groupBox7.Controls.Add(this.GitPath);
            this.groupBox7.Controls.Add(this.BrowseGitPath);
            this.groupBox7.Controls.Add(this.GitBinPath);
            this.groupBox7.Location = new System.Drawing.Point(5, 4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(813, 98);
            this.groupBox7.TabIndex = 9;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Git";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(8, 18);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(469, 13);
            this.label50.TabIndex = 9;
            this.label50.Text = "Git Extensions can use msysgit or cygwin to access git repositories. Set the corr" +
                "ect paths below.";
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitBinPath.Location = new System.Drawing.Point(732, 63);
            this.BrowseGitBinPath.Name = "BrowseGitBinPath";
            this.BrowseGitBinPath.Size = new System.Drawing.Size(75, 25);
            this.BrowseGitBinPath.TabIndex = 8;
            this.BrowseGitBinPath.Text = "Browse";
            this.BrowseGitBinPath.UseVisualStyleBackColor = true;
            this.BrowseGitBinPath.Click += new System.EventHandler(this.BrowseGitBinPath_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 44);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(223, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Command used to run git (git.cmd or git.exe)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 70);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(291, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "Path to linux tools (sh). Leave empty when it is in the path.";
            // 
            // GitPath
            // 
            this.GitPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GitPath.Location = new System.Drawing.Point(373, 39);
            this.GitPath.Name = "GitPath";
            this.GitPath.Size = new System.Drawing.Size(356, 21);
            this.GitPath.TabIndex = 4;
            this.GitPath.TextChanged += new System.EventHandler(this.GitPath_TextChanged);
            // 
            // BrowseGitPath
            // 
            this.BrowseGitPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitPath.Location = new System.Drawing.Point(732, 37);
            this.BrowseGitPath.Name = "BrowseGitPath";
            this.BrowseGitPath.Size = new System.Drawing.Size(75, 25);
            this.BrowseGitPath.TabIndex = 5;
            this.BrowseGitPath.Text = "Browse";
            this.BrowseGitPath.UseVisualStyleBackColor = true;
            this.BrowseGitPath.Click += new System.EventHandler(this.BrowseGitPath_Click);
            // 
            // GitBinPath
            // 
            this.GitBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GitBinPath.Location = new System.Drawing.Point(373, 65);
            this.GitBinPath.Name = "GitBinPath";
            this.GitBinPath.Size = new System.Drawing.Size(356, 21);
            this.GitBinPath.TabIndex = 7;
            // 
            // TabPageGitExtensions
            // 
            this.TabPageGitExtensions.Controls.Add(this.downloadDictionary);
            this.TabPageGitExtensions.Controls.Add(this.label52);
            this.TabPageGitExtensions.Controls.Add(this.ShowStashCountInBrowseWindow);
            this.TabPageGitExtensions.Controls.Add(this.label26);
            this.TabPageGitExtensions.Controls.Add(this.ShowCurrentChangesInRevisionGraph);
            this.TabPageGitExtensions.Controls.Add(this.showErrorsWhenStagingFilesLabel);
            this.TabPageGitExtensions.Controls.Add(this.showErrorsWhenStagingFiles);
            this.TabPageGitExtensions.Controls.Add(this.showGitStatusInToolbarLabel);
            this.TabPageGitExtensions.Controls.Add(this.ShowGitStatusInToolbar);
            this.TabPageGitExtensions.Controls.Add(this.RevisionGridQuickSearchTimeout);
            this.TabPageGitExtensions.Controls.Add(this.label24);
            this.TabPageGitExtensions.Controls.Add(this.helpTranslate);
            this.TabPageGitExtensions.Controls.Add(this.Language);
            this.TabPageGitExtensions.Controls.Add(this.label49);
            this.TabPageGitExtensions.Controls.Add(this.label40);
            this.TabPageGitExtensions.Controls.Add(this.FollowRenamesInFileHistory);
            this.TabPageGitExtensions.Controls.Add(this.label39);
            this.TabPageGitExtensions.Controls.Add(this.label38);
            this.TabPageGitExtensions.Controls.Add(this.label35);
            this.TabPageGitExtensions.Controls.Add(this.label34);
            this.TabPageGitExtensions.Controls.Add(this.EncodingLabel);
            this.TabPageGitExtensions.Controls.Add(this._NO_TRANSLATE_Encoding);
            this.TabPageGitExtensions.Controls.Add(this.label23);
            this.TabPageGitExtensions.Controls.Add(this.SmtpServer);
            this.TabPageGitExtensions.Controls.Add(this.Dictionary);
            this.TabPageGitExtensions.Controls.Add(this.label22);
            this.TabPageGitExtensions.Controls.Add(this.ShowRelativeDate);
            this.TabPageGitExtensions.Controls.Add(this.UseFastChecks);
            this.TabPageGitExtensions.Controls.Add(this.ShowGitCommandLine);
            this.TabPageGitExtensions.Controls.Add(this.CloseProcessDialog);
            this.TabPageGitExtensions.Controls.Add(this._NO_TRANSLATE_MaxCommits);
            this.TabPageGitExtensions.Controls.Add(this.label12);
            this.TabPageGitExtensions.Location = new System.Drawing.Point(4, 22);
            this.TabPageGitExtensions.Name = "TabPageGitExtensions";
            this.TabPageGitExtensions.Size = new System.Drawing.Size(824, 431);
            this.TabPageGitExtensions.TabIndex = 3;
            this.TabPageGitExtensions.Text = "Git extensions";
            this.TabPageGitExtensions.UseVisualStyleBackColor = true;
            this.TabPageGitExtensions.Click += new System.EventHandler(this.TabPageGitExtensions_Click);
            // 
            // downloadDictionary
            // 
            this.downloadDictionary.AutoSize = true;
            this.downloadDictionary.Location = new System.Drawing.Point(571, 81);
            this.downloadDictionary.Name = "downloadDictionary";
            this.downloadDictionary.Size = new System.Drawing.Size(104, 13);
            this.downloadDictionary.TabIndex = 40;
            this.downloadDictionary.TabStop = true;
            this.downloadDictionary.Text = "Download dictionary";
            this.downloadDictionary.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadDictionary_LinkClicked);
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Location = new System.Drawing.Point(33, 169);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(247, 13);
            this.label52.TabIndex = 39;
            this.label52.Text = "Show stash count on status bar in browse window";
            // 
            // ShowStashCountInBrowseWindow
            // 
            this.ShowStashCountInBrowseWindow.AutoSize = true;
            this.ShowStashCountInBrowseWindow.Location = new System.Drawing.Point(12, 170);
            this.ShowStashCountInBrowseWindow.Name = "ShowStashCountInBrowseWindow";
            this.ShowStashCountInBrowseWindow.Size = new System.Drawing.Size(15, 14);
            this.ShowStashCountInBrowseWindow.TabIndex = 38;
            this.ShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(32, 147);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(287, 13);
            this.label26.TabIndex = 37;
            this.label26.Text = "Show current working dir changes in revision graph (slow!)";
            // 
            // ShowCurrentChangesInRevisionGraph
            // 
            this.ShowCurrentChangesInRevisionGraph.AutoSize = true;
            this.ShowCurrentChangesInRevisionGraph.Location = new System.Drawing.Point(11, 148);
            this.ShowCurrentChangesInRevisionGraph.Name = "ShowCurrentChangesInRevisionGraph";
            this.ShowCurrentChangesInRevisionGraph.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowCurrentChangesInRevisionGraph.Size = new System.Drawing.Size(15, 14);
            this.ShowCurrentChangesInRevisionGraph.TabIndex = 36;
            this.ShowCurrentChangesInRevisionGraph.UseVisualStyleBackColor = true;
            // 
            // showErrorsWhenStagingFilesLabel
            // 
            this.showErrorsWhenStagingFilesLabel.AutoSize = true;
            this.showErrorsWhenStagingFilesLabel.Location = new System.Drawing.Point(32, 325);
            this.showErrorsWhenStagingFilesLabel.Name = "showErrorsWhenStagingFilesLabel";
            this.showErrorsWhenStagingFilesLabel.Size = new System.Drawing.Size(154, 13);
            this.showErrorsWhenStagingFilesLabel.TabIndex = 35;
            this.showErrorsWhenStagingFilesLabel.Text = "Show errors when staging files";
            // 
            // showErrorsWhenStagingFiles
            // 
            this.showErrorsWhenStagingFiles.AutoSize = true;
            this.showErrorsWhenStagingFiles.Location = new System.Drawing.Point(11, 325);
            this.showErrorsWhenStagingFiles.Name = "showErrorsWhenStagingFiles";
            this.showErrorsWhenStagingFiles.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.showErrorsWhenStagingFiles.Size = new System.Drawing.Size(15, 14);
            this.showErrorsWhenStagingFiles.TabIndex = 34;
            this.showErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // showGitStatusInToolbarLabel
            // 
            this.showGitStatusInToolbarLabel.AutoSize = true;
            this.showGitStatusInToolbarLabel.Location = new System.Drawing.Point(32, 302);
            this.showGitStatusInToolbarLabel.Name = "showGitStatusInToolbarLabel";
            this.showGitStatusInToolbarLabel.Size = new System.Drawing.Size(432, 13);
            this.showGitStatusInToolbarLabel.TabIndex = 32;
            this.showGitStatusInToolbarLabel.Text = "Show repository status in browse dialog (number of changes in toolbar, restart re" +
                "quired)";
            // 
            // ShowGitStatusInToolbar
            // 
            this.ShowGitStatusInToolbar.AutoSize = true;
            this.ShowGitStatusInToolbar.Location = new System.Drawing.Point(11, 302);
            this.ShowGitStatusInToolbar.Name = "ShowGitStatusInToolbar";
            this.ShowGitStatusInToolbar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowGitStatusInToolbar.Size = new System.Drawing.Size(15, 14);
            this.ShowGitStatusInToolbar.TabIndex = 31;
            this.ShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            this.RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(396, 352);
            this.RevisionGridQuickSearchTimeout.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Name = "RevisionGridQuickSearchTimeout";
            this.RevisionGridQuickSearchTimeout.Size = new System.Drawing.Size(123, 21);
            this.RevisionGridQuickSearchTimeout.TabIndex = 33;
            this.RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(8, 354);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(193, 13);
            this.label24.TabIndex = 32;
            this.label24.Text = "Revision grid quick search timeout [ms]";
            // 
            // helpTranslate
            // 
            this.helpTranslate.AutoSize = true;
            this.helpTranslate.Location = new System.Drawing.Point(571, 50);
            this.helpTranslate.Name = "helpTranslate";
            this.helpTranslate.Size = new System.Drawing.Size(74, 13);
            this.helpTranslate.TabIndex = 30;
            this.helpTranslate.TabStop = true;
            this.helpTranslate.Text = "Help translate";
            this.helpTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpTranslate_LinkClicked);
            // 
            // Language
            // 
            this.Language.FormattingEnabled = true;
            this.Language.Items.AddRange(new object[] {
            "en-US",
            "ja-JP",
            "nl-NL"});
            this.Language.Location = new System.Drawing.Point(396, 47);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(169, 21);
            this.Language.TabIndex = 29;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(8, 50);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(141, 13);
            this.label49.TabIndex = 28;
            this.label49.Text = "Language (restart required)";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(32, 280);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(218, 13);
            this.label40.TabIndex = 27;
            this.label40.Text = "Follow renames in file history (experimental)";
            // 
            // FollowRenamesInFileHistory
            // 
            this.FollowRenamesInFileHistory.AutoSize = true;
            this.FollowRenamesInFileHistory.Location = new System.Drawing.Point(11, 280);
            this.FollowRenamesInFileHistory.Name = "FollowRenamesInFileHistory";
            this.FollowRenamesInFileHistory.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.FollowRenamesInFileHistory.Size = new System.Drawing.Size(15, 14);
            this.FollowRenamesInFileHistory.TabIndex = 26;
            this.FollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(32, 258);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(190, 13);
            this.label39.TabIndex = 25;
            this.label39.Text = "Show relative date instead of full date";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(32, 235);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(255, 13);
            this.label38.TabIndex = 24;
            this.label38.Text = "Use FileSystemWatcher to check if index is changed";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(32, 214);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(263, 13);
            this.label35.TabIndex = 22;
            this.label35.Text = "Show Git commandline dialog when executing process";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(32, 191);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(303, 13);
            this.label34.TabIndex = 21;
            this.label34.Text = "Close process dialog automatically when process is succeeded";
            // 
            // EncodingLabel
            // 
            this.EncodingLabel.AutoSize = true;
            this.EncodingLabel.Location = new System.Drawing.Point(8, 384);
            this.EncodingLabel.Name = "EncodingLabel";
            this.EncodingLabel.Size = new System.Drawing.Size(50, 13);
            this.EncodingLabel.TabIndex = 20;
            this.EncodingLabel.Text = "Encoding";
            // 
            // _NO_TRANSLATE_Encoding
            // 
            this._NO_TRANSLATE_Encoding.FormattingEnabled = true;
            this._NO_TRANSLATE_Encoding.Location = new System.Drawing.Point(396, 381);
            this._NO_TRANSLATE_Encoding.Name = "_NO_TRANSLATE_Encoding";
            this._NO_TRANSLATE_Encoding.Size = new System.Drawing.Size(242, 21);
            this._NO_TRANSLATE_Encoding.TabIndex = 19;
            // 
            // Dictionary
            // 
            this.Dictionary.FormattingEnabled = true;
            this.Dictionary.Location = new System.Drawing.Point(396, 78);
            this.Dictionary.Name = "Dictionary";
            this.Dictionary.Size = new System.Drawing.Size(169, 21);
            this.Dictionary.TabIndex = 15;
            this.Dictionary.DropDown += new System.EventHandler(this.Dictionary_DropDown);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(8, 81);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(154, 13);
            this.label22.TabIndex = 14;
            this.label22.Text = "Dictionary for spelling checker.";
            // 
            // ShowRelativeDate
            // 
            this.ShowRelativeDate.AutoSize = true;
            this.ShowRelativeDate.Location = new System.Drawing.Point(11, 258);
            this.ShowRelativeDate.Name = "ShowRelativeDate";
            this.ShowRelativeDate.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowRelativeDate.Size = new System.Drawing.Size(15, 14);
            this.ShowRelativeDate.TabIndex = 13;
            this.ShowRelativeDate.UseVisualStyleBackColor = true;
            // 
            // UseFastChecks
            // 
            this.UseFastChecks.AutoSize = true;
            this.UseFastChecks.Location = new System.Drawing.Point(11, 236);
            this.UseFastChecks.Name = "UseFastChecks";
            this.UseFastChecks.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.UseFastChecks.Size = new System.Drawing.Size(15, 14);
            this.UseFastChecks.TabIndex = 12;
            this.UseFastChecks.UseVisualStyleBackColor = true;
            // 
            // ShowGitCommandLine
            // 
            this.ShowGitCommandLine.AutoSize = true;
            this.ShowGitCommandLine.Location = new System.Drawing.Point(11, 214);
            this.ShowGitCommandLine.Name = "ShowGitCommandLine";
            this.ShowGitCommandLine.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowGitCommandLine.Size = new System.Drawing.Size(15, 14);
            this.ShowGitCommandLine.TabIndex = 11;
            this.ShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // CloseProcessDialog
            // 
            this.CloseProcessDialog.AutoSize = true;
            this.CloseProcessDialog.Location = new System.Drawing.Point(11, 192);
            this.CloseProcessDialog.Name = "CloseProcessDialog";
            this.CloseProcessDialog.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.CloseProcessDialog.Size = new System.Drawing.Size(15, 14);
            this.CloseProcessDialog.TabIndex = 9;
            this.CloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(396, 15);
            this._NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            this._NO_TRANSLATE_MaxCommits.Size = new System.Drawing.Size(123, 21);
            this._NO_TRANSLATE_MaxCommits.TabIndex = 2;
            this._NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 16);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(266, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Limit number of commits that will be loaded at startup.";
            // 
            // StartPage
            // 
            this.StartPage.Controls.Add(this.dashboardEditor1);
            this.StartPage.Location = new System.Drawing.Point(4, 22);
            this.StartPage.Name = "StartPage";
            this.StartPage.Padding = new System.Windows.Forms.Padding(3);
            this.StartPage.Size = new System.Drawing.Size(824, 431);
            this.StartPage.TabIndex = 6;
            this.StartPage.Text = "Start page";
            this.StartPage.UseVisualStyleBackColor = true;
            // 
            // dashboardEditor1
            // 
            this.dashboardEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardEditor1.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.dashboardEditor1.Location = new System.Drawing.Point(3, 3);
            this.dashboardEditor1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dashboardEditor1.Name = "dashboardEditor1";
            this.dashboardEditor1.Size = new System.Drawing.Size(818, 425);
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
            this.AppearancePage.Size = new System.Drawing.Size(824, 431);
            this.AppearancePage.TabIndex = 5;
            this.AppearancePage.Text = "Appearance";
            this.AppearancePage.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.noImageService);
            this.groupBox6.Controls.Add(this.label53);
            this.groupBox6.Controls.Add(this.label47);
            this.groupBox6.Controls.Add(this._NO_TRANSLATE_DaysToCacheImages);
            this.groupBox6.Controls.Add(this.label46);
            this.groupBox6.Controls.Add(this.label44);
            this.groupBox6.Controls.Add(this._NO_TRANSLATE_authorImageSize);
            this.groupBox6.Controls.Add(this.ClearImageCache);
            this.groupBox6.Controls.Add(this.ShowAuthorGravatar);
            this.groupBox6.Location = new System.Drawing.Point(328, 236);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(311, 158);
            this.groupBox6.TabIndex = 13;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Author images";
            // 
            // noImageService
            // 
            this.noImageService.FormattingEnabled = true;
            this.noImageService.Items.AddRange(new object[] {
            "Identicon",
            "Wavatar",
            "MonsterId"});
            this.noImageService.Location = new System.Drawing.Point(149, 101);
            this.noImageService.Name = "noImageService";
            this.noImageService.Size = new System.Drawing.Size(142, 21);
            this.noImageService.TabIndex = 9;
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(7, 104);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(88, 13);
            this.label53.TabIndex = 8;
            this.label53.Text = "No image service";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(241, 77);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(30, 13);
            this.label47.TabIndex = 7;
            this.label47.Text = "days";
            // 
            // _NO_TRANSLATE_DaysToCacheImages
            // 
            this._NO_TRANSLATE_DaysToCacheImages.Location = new System.Drawing.Point(149, 73);
            this._NO_TRANSLATE_DaysToCacheImages.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._NO_TRANSLATE_DaysToCacheImages.Name = "_NO_TRANSLATE_DaysToCacheImages";
            this._NO_TRANSLATE_DaysToCacheImages.Size = new System.Drawing.Size(77, 21);
            this._NO_TRANSLATE_DaysToCacheImages.TabIndex = 6;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(7, 77);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(73, 13);
            this.label46.TabIndex = 5;
            this.label46.Text = "Cache images";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(7, 49);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(58, 13);
            this.label44.TabIndex = 4;
            this.label44.Text = "Image size";
            // 
            // _NO_TRANSLATE_authorImageSize
            // 
            this._NO_TRANSLATE_authorImageSize.Location = new System.Drawing.Point(149, 46);
            this._NO_TRANSLATE_authorImageSize.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._NO_TRANSLATE_authorImageSize.Name = "_NO_TRANSLATE_authorImageSize";
            this._NO_TRANSLATE_authorImageSize.Size = new System.Drawing.Size(77, 21);
            this._NO_TRANSLATE_authorImageSize.TabIndex = 3;
            // 
            // ClearImageCache
            // 
            this.ClearImageCache.Location = new System.Drawing.Point(5, 130);
            this.ClearImageCache.Name = "ClearImageCache";
            this.ClearImageCache.Size = new System.Drawing.Size(142, 25);
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
            this.ShowAuthorGravatar.Size = new System.Drawing.Size(202, 17);
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
            this.groupBox5.Size = new System.Drawing.Size(312, 227);
            this.groupBox5.TabIndex = 12;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Application Icon";
            // 
            // RandomIcon
            // 
            this.RandomIcon.AutoSize = true;
            this.RandomIcon.Location = new System.Drawing.Point(6, 187);
            this.RandomIcon.Name = "RandomIcon";
            this.RandomIcon.Size = new System.Drawing.Size(64, 17);
            this.RandomIcon.TabIndex = 6;
            this.RandomIcon.TabStop = true;
            this.RandomIcon.Text = "Random";
            this.RandomIcon.UseVisualStyleBackColor = true;
            // 
            // YellowIcon
            // 
            this.YellowIcon.AutoSize = true;
            this.YellowIcon.Location = new System.Drawing.Point(6, 159);
            this.YellowIcon.Name = "YellowIcon";
            this.YellowIcon.Size = new System.Drawing.Size(55, 17);
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
            this.RedIcon.Size = new System.Drawing.Size(44, 17);
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
            this.BlueIcon.Size = new System.Drawing.Size(45, 17);
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
            this.DefaultIcon.Size = new System.Drawing.Size(60, 17);
            this.DefaultIcon.TabIndex = 0;
            this.DefaultIcon.TabStop = true;
            this.DefaultIcon.Text = "Default";
            this.DefaultIcon.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.DrawNonRelativesTextGray);
            this.groupBox4.Controls.Add(this.DrawNonRelativesGray);
            this.groupBox4.Controls.Add(this._NO_TRANSLATE_ColorGraphLabel);
            this.groupBox4.Controls.Add(this.StripedBanchChange);
            this.groupBox4.Controls.Add(this.BranchBorders);
            this.groupBox4.Controls.Add(this.MulticolorBranches);
            this.groupBox4.Controls.Add(this.label33);
            this.groupBox4.Controls.Add(this._NO_TRANSLATE_ColorRemoteBranchLabel);
            this.groupBox4.Controls.Add(this._NO_TRANSLATE_ColorOtherLabel);
            this.groupBox4.Controls.Add(this.label36);
            this.groupBox4.Controls.Add(this.label25);
            this.groupBox4.Controls.Add(this._NO_TRANSLATE_ColorTagLabel);
            this.groupBox4.Controls.Add(this._NO_TRANSLATE_ColorBranchLabel);
            this.groupBox4.Controls.Add(this.label32);
            this.groupBox4.Location = new System.Drawing.Point(8, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(313, 250);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Revision graph";
            // 
            // DrawNonRelativesTextGray
            // 
            this.DrawNonRelativesTextGray.AutoSize = true;
            this.DrawNonRelativesTextGray.Location = new System.Drawing.Point(9, 120);
            this.DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(164, 17);
            this.DrawNonRelativesTextGray.TabIndex = 17;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(9, 96);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(172, 17);
            this.DrawNonRelativesGray.TabIndex = 16;
            this.DrawNonRelativesGray.Text = "Draw non relatives graph gray";
            this.DrawNonRelativesGray.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_ColorGraphLabel
            // 
            this._NO_TRANSLATE_ColorGraphLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorGraphLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorGraphLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorGraphLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorGraphLabel.Location = new System.Drawing.Point(211, 21);
            this._NO_TRANSLATE_ColorGraphLabel.Name = "_NO_TRANSLATE_ColorGraphLabel";
            this._NO_TRANSLATE_ColorGraphLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorGraphLabel.TabIndex = 15;
            this._NO_TRANSLATE_ColorGraphLabel.Text = "Red";
            this._NO_TRANSLATE_ColorGraphLabel.Click += new System.EventHandler(this._ColorGraphLabel_Click);
            // 
            // StripedBanchChange
            // 
            this.StripedBanchChange.AutoSize = true;
            this.StripedBanchChange.Location = new System.Drawing.Point(9, 45);
            this.StripedBanchChange.Name = "StripedBanchChange";
            this.StripedBanchChange.Size = new System.Drawing.Size(134, 17);
            this.StripedBanchChange.TabIndex = 14;
            this.StripedBanchChange.Text = "Striped branch change";
            this.StripedBanchChange.UseVisualStyleBackColor = true;
            // 
            // BranchBorders
            // 
            this.BranchBorders.AutoSize = true;
            this.BranchBorders.Location = new System.Drawing.Point(9, 71);
            this.BranchBorders.Name = "BranchBorders";
            this.BranchBorders.Size = new System.Drawing.Size(127, 17);
            this.BranchBorders.TabIndex = 13;
            this.BranchBorders.Text = "Draw branch borders";
            this.BranchBorders.UseVisualStyleBackColor = true;
            // 
            // MulticolorBranches
            // 
            this.MulticolorBranches.AutoSize = true;
            this.MulticolorBranches.Location = new System.Drawing.Point(9, 20);
            this.MulticolorBranches.Name = "MulticolorBranches";
            this.MulticolorBranches.Size = new System.Drawing.Size(118, 17);
            this.MulticolorBranches.TabIndex = 12;
            this.MulticolorBranches.Text = "Multicolor branches";
            this.MulticolorBranches.UseVisualStyleBackColor = true;
            this.MulticolorBranches.CheckedChanged += new System.EventHandler(this.MulticolorBranches_CheckedChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 200);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(105, 13);
            this.label33.TabIndex = 8;
            this.label33.Text = "Color remote branch";
            // 
            // _NO_TRANSLATE_ColorRemoteBranchLabel
            // 
            this._NO_TRANSLATE_ColorRemoteBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Location = new System.Drawing.Point(211, 200);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Name = "_NO_TRANSLATE_ColorRemoteBranchLabel";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.TabIndex = 9;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Click += new System.EventHandler(this.ColorRemoteBranchLabel_Click);
            // 
            // _NO_TRANSLATE_ColorOtherLabel
            // 
            this._NO_TRANSLATE_ColorOtherLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorOtherLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorOtherLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorOtherLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorOtherLabel.Location = new System.Drawing.Point(211, 228);
            this._NO_TRANSLATE_ColorOtherLabel.Name = "_NO_TRANSLATE_ColorOtherLabel";
            this._NO_TRANSLATE_ColorOtherLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorOtherLabel.TabIndex = 11;
            this._NO_TRANSLATE_ColorOtherLabel.Text = "Red";
            this._NO_TRANSLATE_ColorOtherLabel.Click += new System.EventHandler(this.ColorOtherLabel_Click);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 228);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(86, 13);
            this.label36.TabIndex = 10;
            this.label36.Text = "Color other label";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 143);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(51, 13);
            this.label25.TabIndex = 4;
            this.label25.Text = "Color tag";
            // 
            // _NO_TRANSLATE_ColorTagLabel
            // 
            this._NO_TRANSLATE_ColorTagLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorTagLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorTagLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorTagLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorTagLabel.Location = new System.Drawing.Point(211, 143);
            this._NO_TRANSLATE_ColorTagLabel.Name = "_NO_TRANSLATE_ColorTagLabel";
            this._NO_TRANSLATE_ColorTagLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorTagLabel.TabIndex = 5;
            this._NO_TRANSLATE_ColorTagLabel.Text = "Red";
            this._NO_TRANSLATE_ColorTagLabel.Click += new System.EventHandler(this.ColorTagLabel_Click);
            // 
            // _NO_TRANSLATE_ColorBranchLabel
            // 
            this._NO_TRANSLATE_ColorBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorBranchLabel.Location = new System.Drawing.Point(211, 171);
            this._NO_TRANSLATE_ColorBranchLabel.Name = "_NO_TRANSLATE_ColorBranchLabel";
            this._NO_TRANSLATE_ColorBranchLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorBranchLabel.TabIndex = 7;
            this._NO_TRANSLATE_ColorBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorBranchLabel.Click += new System.EventHandler(this.ColorBranchLabel_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 171);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(68, 13);
            this.label32.TabIndex = 6;
            this.label32.Text = "Color branch";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label43);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorRemovedLineDiffLabel);
            this.groupBox3.Controls.Add(this.label45);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorAddedLineDiffLabel);
            this.groupBox3.Controls.Add(this.label27);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorSectionLabel);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorRemovedLine);
            this.groupBox3.Controls.Add(this.label31);
            this.groupBox3.Controls.Add(this.label29);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorAddedLineLabel);
            this.groupBox3.Location = new System.Drawing.Point(8, 259);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(313, 158);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Difference view";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(6, 79);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(153, 13);
            this.label43.TabIndex = 10;
            this.label43.Text = "Color removed line highlighting";
            // 
            // _NO_TRANSLATE_ColorRemovedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Location = new System.Drawing.Point(208, 79);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Name = "_NO_TRANSLATE_ColorRemovedLineDiffLabel";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.TabIndex = 11;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Click += new System.EventHandler(this.ColorRemovedLineDiffLabel_Click);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(6, 109);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(141, 13);
            this.label45.TabIndex = 12;
            this.label45.Text = "Color added line highlighting";
            // 
            // _NO_TRANSLATE_ColorAddedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Location = new System.Drawing.Point(208, 109);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Name = "_NO_TRANSLATE_ColorAddedLineDiffLabel";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.TabIndex = 13;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Click += new System.EventHandler(this.ColorAddedLineDiffLabel_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 18);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(96, 13);
            this.label27.TabIndex = 4;
            this.label27.Text = "Color removed line";
            // 
            // _NO_TRANSLATE_ColorSectionLabel
            // 
            this._NO_TRANSLATE_ColorSectionLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorSectionLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorSectionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorSectionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorSectionLabel.Location = new System.Drawing.Point(208, 138);
            this._NO_TRANSLATE_ColorSectionLabel.Name = "_NO_TRANSLATE_ColorSectionLabel";
            this._NO_TRANSLATE_ColorSectionLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorSectionLabel.TabIndex = 9;
            this._NO_TRANSLATE_ColorSectionLabel.Text = "Red";
            this._NO_TRANSLATE_ColorSectionLabel.Click += new System.EventHandler(this.ColorSectionLabel_Click);
            // 
            // _NO_TRANSLATE_ColorRemovedLine
            // 
            this._NO_TRANSLATE_ColorRemovedLine.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLine.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemovedLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLine.Location = new System.Drawing.Point(208, 18);
            this._NO_TRANSLATE_ColorRemovedLine.Name = "_NO_TRANSLATE_ColorRemovedLine";
            this._NO_TRANSLATE_ColorRemovedLine.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorRemovedLine.TabIndex = 5;
            this._NO_TRANSLATE_ColorRemovedLine.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLine.Click += new System.EventHandler(this.ColorRemovedLine_Click);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(6, 139);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(69, 13);
            this.label31.TabIndex = 8;
            this.label31.Text = "Color section";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 48);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(84, 13);
            this.label29.TabIndex = 6;
            this.label29.Text = "Color added line";
            // 
            // _NO_TRANSLATE_ColorAddedLineLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAddedLineLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineLabel.Location = new System.Drawing.Point(208, 48);
            this._NO_TRANSLATE_ColorAddedLineLabel.Name = "_NO_TRANSLATE_ColorAddedLineLabel";
            this._NO_TRANSLATE_ColorAddedLineLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorAddedLineLabel.TabIndex = 7;
            this._NO_TRANSLATE_ColorAddedLineLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineLabel.Click += new System.EventHandler(this.label28_Click);
            // 
            // GlobalSettingsPage
            // 
            this.GlobalSettingsPage.Controls.Add(this.groupBox9);
            this.GlobalSettingsPage.Controls.Add(this.DiffToolCmdSuggest);
            this.GlobalSettingsPage.Controls.Add(this.DifftoolCmd);
            this.GlobalSettingsPage.Controls.Add(this.label48);
            this.GlobalSettingsPage.Controls.Add(this.BrowseDiffTool);
            this.GlobalSettingsPage.Controls.Add(this.label42);
            this.GlobalSettingsPage.Controls.Add(this.DifftoolPath);
            this.GlobalSettingsPage.Controls.Add(this.GlobalDiffTool);
            this.GlobalSettingsPage.Controls.Add(this.label41);
            this.GlobalSettingsPage.Controls.Add(this.label28);
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
            this.GlobalSettingsPage.Size = new System.Drawing.Size(824, 431);
            this.GlobalSettingsPage.TabIndex = 1;
            this.GlobalSettingsPage.Text = "Global settings";
            this.GlobalSettingsPage.UseVisualStyleBackColor = true;
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox9.Controls.Add(this.globalAutoCrlfFalse);
            this.groupBox9.Controls.Add(this.globalAutoCrlfInput);
            this.groupBox9.Controls.Add(this.globalAutoCrlfTrue);
            this.groupBox9.Location = new System.Drawing.Point(6, 290);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(799, 105);
            this.groupBox9.TabIndex = 31;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Line endings";
            // 
            // globalAutoCrlfFalse
            // 
            this.globalAutoCrlfFalse.AutoSize = true;
            this.globalAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.globalAutoCrlfFalse.Name = "globalAutoCrlfFalse";
            this.globalAutoCrlfFalse.Size = new System.Drawing.Size(319, 17);
            this.globalAutoCrlfFalse.TabIndex = 2;
            this.globalAutoCrlfFalse.TabStop = true;
            this.globalAutoCrlfFalse.Text = "Checkout as-is, commit as-is (\"core.autocrlf\"  is set to \"false\")";
            this.globalAutoCrlfFalse.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfInput
            // 
            this.globalAutoCrlfInput.AutoSize = true;
            this.globalAutoCrlfInput.Location = new System.Drawing.Point(5, 48);
            this.globalAutoCrlfInput.Name = "globalAutoCrlfInput";
            this.globalAutoCrlfInput.Size = new System.Drawing.Size(405, 17);
            this.globalAutoCrlfInput.TabIndex = 1;
            this.globalAutoCrlfInput.TabStop = true;
            this.globalAutoCrlfInput.Text = "Checkout as-is, commit Unix-style line endings (\"core.autocrlf\"  is set to \"input" +
                "\")";
            this.globalAutoCrlfInput.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfTrue
            // 
            this.globalAutoCrlfTrue.AutoSize = true;
            this.globalAutoCrlfTrue.Location = new System.Drawing.Point(5, 22);
            this.globalAutoCrlfTrue.Name = "globalAutoCrlfTrue";
            this.globalAutoCrlfTrue.Size = new System.Drawing.Size(449, 17);
            this.globalAutoCrlfTrue.TabIndex = 0;
            this.globalAutoCrlfTrue.TabStop = true;
            this.globalAutoCrlfTrue.Text = "Checkout Windows-style, commit Unix-style line endings (\"core.autocrlf\"  is set t" +
                "o \"true\")";
            this.globalAutoCrlfTrue.UseVisualStyleBackColor = true;
            // 
            // DiffToolCmdSuggest
            // 
            this.DiffToolCmdSuggest.Location = new System.Drawing.Point(506, 257);
            this.DiffToolCmdSuggest.Name = "DiffToolCmdSuggest";
            this.DiffToolCmdSuggest.Size = new System.Drawing.Size(108, 25);
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
                "BASE\" \"$REMOTE\"",
            "\"C:/Program Files/Beyond Compare 3/bcomp.exe\" \"$LOCAL\" \"$REMOTE\""});
            this.DifftoolCmd.Location = new System.Drawing.Point(153, 259);
            this.DifftoolCmd.Name = "DifftoolCmd";
            this.DifftoolCmd.Size = new System.Drawing.Size(347, 21);
            this.DifftoolCmd.TabIndex = 29;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(9, 263);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(90, 13);
            this.label48.TabIndex = 28;
            this.label48.Text = "Difftool command";
            // 
            // BrowseDiffTool
            // 
            this.BrowseDiffTool.Location = new System.Drawing.Point(506, 229);
            this.BrowseDiffTool.Name = "BrowseDiffTool";
            this.BrowseDiffTool.Size = new System.Drawing.Size(75, 25);
            this.BrowseDiffTool.TabIndex = 27;
            this.BrowseDiffTool.Text = "Browse";
            this.BrowseDiffTool.UseVisualStyleBackColor = true;
            this.BrowseDiffTool.Click += new System.EventHandler(this.BrowseDiffTool_Click);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(9, 236);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(79, 13);
            this.label42.TabIndex = 26;
            this.label42.Text = "Path to difftool";
            // 
            // DifftoolPath
            // 
            this.DifftoolPath.Location = new System.Drawing.Point(153, 232);
            this.DifftoolPath.Name = "DifftoolPath";
            this.DifftoolPath.Size = new System.Drawing.Size(347, 21);
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
            "beyondcompare3",
            "winmerge"});
            this.GlobalDiffTool.Location = new System.Drawing.Point(153, 205);
            this.GlobalDiffTool.Name = "GlobalDiffTool";
            this.GlobalDiffTool.Size = new System.Drawing.Size(164, 21);
            this.GlobalDiffTool.TabIndex = 24;
            this.GlobalDiffTool.TextChanged += new System.EventHandler(this.ExternalDiffTool_TextChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(10, 208);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(42, 13);
            this.label41.TabIndex = 23;
            this.label41.Text = "Difftool";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(10, 179);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(161, 13);
            this.label28.TabIndex = 22;
            this.label28.Text = "Keep backup (.orig) after merge";
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
            this.label9.Size = new System.Drawing.Size(180, 39);
            this.label9.TabIndex = 19;
            this.label9.Text = "You need to set the correct path to \r\ngit before you can change\r\nglobal settings." +
                "\r\n";
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
            this.button1.Size = new System.Drawing.Size(108, 25);
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
                "BASE\" \"$REMOTE\"",
            "\"C:/Program Files/Beyond Compare 3/bcomp.exe\" \"$BASE\" \"$LOCAL\" \"$REMOTE\""});
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
            this.BrowseMergeTool.Size = new System.Drawing.Size(75, 25);
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
            "BeyondCompare3",
            "DiffMerge",
            "kdiff3",
            "p4merge",
            "TortoiseMerge"});
            this.GlobalMergeTool.Location = new System.Drawing.Point(153, 92);
            this.GlobalMergeTool.Name = "GlobalMergeTool";
            this.GlobalMergeTool.Size = new System.Drawing.Size(164, 21);
            this.GlobalMergeTool.TabIndex = 12;
            this.GlobalMergeTool.TextChanged += new System.EventHandler(this.GlobalMergeTool_TextChanged);
            // 
            // PathToKDiff3
            // 
            this.PathToKDiff3.AutoSize = true;
            this.PathToKDiff3.Location = new System.Drawing.Point(9, 124);
            this.PathToKDiff3.Name = "PathToKDiff3";
            this.PathToKDiff3.Size = new System.Drawing.Size(93, 13);
            this.PathToKDiff3.TabIndex = 11;
            this.PathToKDiff3.Text = "Path to mergetool";
            // 
            // MergetoolPath
            // 
            this.MergetoolPath.Location = new System.Drawing.Point(153, 120);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(347, 21);
            this.MergetoolPath.TabIndex = 10;
            // 
            // GlobalKeepMergeBackup
            // 
            this.GlobalKeepMergeBackup.AutoSize = true;
            this.GlobalKeepMergeBackup.Checked = true;
            this.GlobalKeepMergeBackup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.GlobalKeepMergeBackup.Location = new System.Drawing.Point(506, 179);
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
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Mergetool";
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalEditor.Location = new System.Drawing.Point(153, 65);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(648, 21);
            this.GlobalEditor.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Editor";
            // 
            // GlobalUserEmail
            // 
            this.GlobalUserEmail.Location = new System.Drawing.Point(153, 37);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(236, 21);
            this.GlobalUserEmail.TabIndex = 3;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(153, 8);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(236, 21);
            this.GlobalUserName.TabIndex = 2;
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
            // 
            // Ssh
            // 
            this.Ssh.Controls.Add(this.groupBox2);
            this.Ssh.Controls.Add(this.groupBox1);
            this.Ssh.Location = new System.Drawing.Point(4, 22);
            this.Ssh.Name = "Ssh";
            this.Ssh.Padding = new System.Windows.Forms.Padding(3);
            this.Ssh.Size = new System.Drawing.Size(824, 431);
            this.Ssh.TabIndex = 4;
            this.Ssh.Text = "Ssh";
            this.Ssh.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBox2.Size = new System.Drawing.Size(808, 154);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Configure PuTTY";
            // 
            // AutostartPageant
            // 
            this.AutostartPageant.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutostartPageant.Checked = true;
            this.AutostartPageant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutostartPageant.Location = new System.Drawing.Point(143, 103);
            this.AutostartPageant.Name = "AutostartPageant";
            this.AutostartPageant.Size = new System.Drawing.Size(542, 51);
            this.AutostartPageant.TabIndex = 11;
            this.AutostartPageant.Text = "Automatically start authentication client when a private key is configured for a " +
                "remote";
            this.AutostartPageant.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutostartPageant.UseVisualStyleBackColor = true;
            // 
            // PageantPath
            // 
            this.PageantPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PageantPath.Location = new System.Drawing.Point(143, 76);
            this.PageantPath.Name = "PageantPath";
            this.PageantPath.Size = new System.Drawing.Size(575, 21);
            this.PageantPath.TabIndex = 9;
            // 
            // PageantBrowse
            // 
            this.PageantBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PageantBrowse.Location = new System.Drawing.Point(724, 74);
            this.PageantBrowse.Name = "PageantBrowse";
            this.PageantBrowse.Size = new System.Drawing.Size(75, 25);
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
            this.label17.Size = new System.Drawing.Size(85, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "Path to pageant";
            // 
            // PuttygenPath
            // 
            this.PuttygenPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenPath.Location = new System.Drawing.Point(143, 46);
            this.PuttygenPath.Name = "PuttygenPath";
            this.PuttygenPath.Size = new System.Drawing.Size(575, 21);
            this.PuttygenPath.TabIndex = 6;
            // 
            // PuttygenBrowse
            // 
            this.PuttygenBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenBrowse.Location = new System.Drawing.Point(724, 44);
            this.PuttygenBrowse.Name = "PuttygenBrowse";
            this.PuttygenBrowse.Size = new System.Drawing.Size(75, 25);
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
            this.label16.Size = new System.Drawing.Size(89, 13);
            this.label16.TabIndex = 5;
            this.label16.Text = "Path to puttygen";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 20);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(88, 13);
            this.label15.TabIndex = 4;
            this.label15.Text = "Path to plink.exe";
            // 
            // PlinkPath
            // 
            this.PlinkPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkPath.Location = new System.Drawing.Point(143, 17);
            this.PlinkPath.Name = "PlinkPath";
            this.PlinkPath.Size = new System.Drawing.Size(575, 21);
            this.PlinkPath.TabIndex = 2;
            // 
            // PlinkBrowse
            // 
            this.PlinkBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkBrowse.Location = new System.Drawing.Point(724, 14);
            this.PlinkBrowse.Name = "PlinkBrowse";
            this.PlinkBrowse.Size = new System.Drawing.Size(75, 25);
            this.PlinkBrowse.TabIndex = 3;
            this.PlinkBrowse.Text = "Browse";
            this.PlinkBrowse.UseVisualStyleBackColor = true;
            this.PlinkBrowse.Click += new System.EventHandler(this.PuttyBrowse_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.OtherSsh);
            this.groupBox1.Controls.Add(this.OtherSshBrowse);
            this.groupBox1.Controls.Add(this.Other);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.OpenSSH);
            this.groupBox1.Controls.Add(this.Putty);
            this.groupBox1.Location = new System.Drawing.Point(8, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(808, 121);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Specify which ssh client to use";
            // 
            // OtherSsh
            // 
            this.OtherSsh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherSsh.Location = new System.Drawing.Point(143, 80);
            this.OtherSsh.Name = "OtherSsh";
            this.OtherSsh.Size = new System.Drawing.Size(575, 21);
            this.OtherSsh.TabIndex = 4;
            // 
            // OtherSshBrowse
            // 
            this.OtherSshBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherSshBrowse.Location = new System.Drawing.Point(724, 77);
            this.OtherSshBrowse.Name = "OtherSshBrowse";
            this.OtherSshBrowse.Size = new System.Drawing.Size(75, 25);
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
            this.Other.Size = new System.Drawing.Size(100, 17);
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
            this.label18.Location = new System.Drawing.Point(141, 17);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(422, 41);
            this.label18.TabIndex = 2;
            this.label18.Text = resources.GetString("label18.Text");
            // 
            // OpenSSH
            // 
            this.OpenSSH.AutoSize = true;
            this.OpenSSH.Location = new System.Drawing.Point(9, 50);
            this.OpenSSH.Name = "OpenSSH";
            this.OpenSSH.Size = new System.Drawing.Size(70, 17);
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
            this.Putty.Size = new System.Drawing.Size(55, 17);
            this.Putty.TabIndex = 0;
            this.Putty.TabStop = true;
            this.Putty.Text = "PuTTY";
            this.Putty.UseVisualStyleBackColor = true;
            this.Putty.CheckedChanged += new System.EventHandler(this.Putty_CheckedChanged);
            // 
            // scriptsTab
            // 
            this.scriptsTab.Controls.Add(this.helpLabel);
            this.scriptsTab.Controls.Add(this.inMenuCheckBox);
            this.scriptsTab.Controls.Add(this.argumentsLabel);
            this.scriptsTab.Controls.Add(this.commandLabel);
            this.scriptsTab.Controls.Add(this.nameLabel);
            this.scriptsTab.Controls.Add(this.browseScriptButton);
            this.scriptsTab.Controls.Add(this.cancelScriptButton);
            this.scriptsTab.Controls.Add(this.saveScriptButton);
            this.scriptsTab.Controls.Add(this.argumentsTextBox);
            this.scriptsTab.Controls.Add(this.commandTextBox);
            this.scriptsTab.Controls.Add(this.nameTextBox);
            this.scriptsTab.Controls.Add(this.moveDownButton);
            this.scriptsTab.Controls.Add(this.removeScriptButton);
            this.scriptsTab.Controls.Add(this.editScriptButton);
            this.scriptsTab.Controls.Add(this.addScriptButton);
            this.scriptsTab.Controls.Add(this.moveUpButton);
            this.scriptsTab.Controls.Add(this.ScriptList);
            this.scriptsTab.Location = new System.Drawing.Point(4, 22);
            this.scriptsTab.Name = "scriptsTab";
            this.scriptsTab.Padding = new System.Windows.Forms.Padding(3);
            this.scriptsTab.Size = new System.Drawing.Size(824, 431);
            this.scriptsTab.TabIndex = 8;
            this.scriptsTab.Text = "Scripts";
            this.scriptsTab.UseVisualStyleBackColor = true;
            // 
            // helpLabel
            // 
            this.helpLabel.AutoSize = true;
            this.helpLabel.BackColor = System.Drawing.SystemColors.Info;
            this.helpLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.helpLabel.Location = new System.Drawing.Point(107, 413);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(166, 15);
            this.helpLabel.TabIndex = 16;
            this.helpLabel.Text = "Press F1 to see available options";
            this.helpLabel.Visible = false;
            // 
            // inMenuCheckBox
            // 
            this.inMenuCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.inMenuCheckBox.AutoSize = true;
            this.inMenuCheckBox.Enabled = false;
            this.inMenuCheckBox.Location = new System.Drawing.Point(458, 245);
            this.inMenuCheckBox.Name = "inMenuCheckBox";
            this.inMenuCheckBox.Size = new System.Drawing.Size(188, 17);
            this.inMenuCheckBox.TabIndex = 15;
            this.inMenuCheckBox.Text = "Add to revision grid context menu";
            this.inMenuCheckBox.UseVisualStyleBackColor = true;
            // 
            // argumentsLabel
            // 
            this.argumentsLabel.AutoSize = true;
            this.argumentsLabel.Location = new System.Drawing.Point(8, 302);
            this.argumentsLabel.Name = "argumentsLabel";
            this.argumentsLabel.Size = new System.Drawing.Size(63, 13);
            this.argumentsLabel.TabIndex = 14;
            this.argumentsLabel.Text = "Arguments:";
            // 
            // commandLabel
            // 
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(8, 274);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(58, 13);
            this.commandLabel.TabIndex = 13;
            this.commandLabel.Text = "Command:";
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(8, 246);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(38, 13);
            this.nameLabel.TabIndex = 12;
            this.nameLabel.Text = "Name:";
            // 
            // browseScriptButton
            // 
            this.browseScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseScriptButton.Enabled = false;
            this.browseScriptButton.Location = new System.Drawing.Point(739, 269);
            this.browseScriptButton.Name = "browseScriptButton";
            this.browseScriptButton.Size = new System.Drawing.Size(75, 25);
            this.browseScriptButton.TabIndex = 11;
            this.browseScriptButton.Text = "Browse";
            this.browseScriptButton.UseVisualStyleBackColor = true;
            this.browseScriptButton.Click += new System.EventHandler(this.browseScriptButton_Click);
            // 
            // cancelScriptButton
            // 
            this.cancelScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelScriptButton.Enabled = false;
            this.cancelScriptButton.Location = new System.Drawing.Point(739, 241);
            this.cancelScriptButton.Name = "cancelScriptButton";
            this.cancelScriptButton.Size = new System.Drawing.Size(75, 25);
            this.cancelScriptButton.TabIndex = 10;
            this.cancelScriptButton.Text = "Cancel";
            this.cancelScriptButton.UseVisualStyleBackColor = true;
            this.cancelScriptButton.Click += new System.EventHandler(this.cancelScriptButton_Click);
            // 
            // saveScriptButton
            // 
            this.saveScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.saveScriptButton.Enabled = false;
            this.saveScriptButton.Location = new System.Drawing.Point(652, 241);
            this.saveScriptButton.Name = "saveScriptButton";
            this.saveScriptButton.Size = new System.Drawing.Size(75, 25);
            this.saveScriptButton.TabIndex = 9;
            this.saveScriptButton.Text = "Save";
            this.saveScriptButton.UseVisualStyleBackColor = true;
            this.saveScriptButton.Click += new System.EventHandler(this.saveScriptButton_Click);
            // 
            // argumentsTextBox
            // 
            this.argumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.argumentsTextBox.Enabled = false;
            this.helpProvider1.SetHelpString(this.argumentsTextBox, resources.GetString("argumentsTextBox.HelpString"));
            this.argumentsTextBox.Location = new System.Drawing.Point(107, 299);
            this.argumentsTextBox.Name = "argumentsTextBox";
            this.helpProvider1.SetShowHelp(this.argumentsTextBox, true);
            this.argumentsTextBox.Size = new System.Drawing.Size(707, 111);
            this.argumentsTextBox.TabIndex = 8;
            this.argumentsTextBox.Text = "";
            this.argumentsTextBox.Enter += new System.EventHandler(this.argumentsTextBox_Enter);
            this.argumentsTextBox.Leave += new System.EventHandler(this.argumentsTextBox_Leave);
            // 
            // commandTextBox
            // 
            this.commandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.commandTextBox.Enabled = false;
            this.commandTextBox.Location = new System.Drawing.Point(107, 271);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(620, 21);
            this.commandTextBox.TabIndex = 7;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.nameTextBox.Enabled = false;
            this.nameTextBox.Location = new System.Drawing.Point(107, 243);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(248, 21);
            this.nameTextBox.TabIndex = 6;
            // 
            // moveDownButton
            // 
            this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Image = global::GitUI.Properties.Resources._4;
            this.moveDownButton.Location = new System.Drawing.Point(761, 153);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(26, 23);
            this.moveDownButton.TabIndex = 5;
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // removeScriptButton
            // 
            this.removeScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeScriptButton.Enabled = false;
            this.removeScriptButton.Location = new System.Drawing.Point(739, 124);
            this.removeScriptButton.Name = "removeScriptButton";
            this.removeScriptButton.Size = new System.Drawing.Size(75, 25);
            this.removeScriptButton.TabIndex = 4;
            this.removeScriptButton.Text = "Remove";
            this.removeScriptButton.UseVisualStyleBackColor = true;
            this.removeScriptButton.Click += new System.EventHandler(this.removeScriptButton_Click);
            // 
            // editScriptButton
            // 
            this.editScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.editScriptButton.Enabled = false;
            this.editScriptButton.Location = new System.Drawing.Point(739, 94);
            this.editScriptButton.Name = "editScriptButton";
            this.editScriptButton.Size = new System.Drawing.Size(75, 25);
            this.editScriptButton.TabIndex = 3;
            this.editScriptButton.Text = "Edit";
            this.editScriptButton.UseVisualStyleBackColor = true;
            this.editScriptButton.Click += new System.EventHandler(this.editScriptButton_Click);
            // 
            // addScriptButton
            // 
            this.addScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addScriptButton.Location = new System.Drawing.Point(739, 64);
            this.addScriptButton.Name = "addScriptButton";
            this.addScriptButton.Size = new System.Drawing.Size(75, 25);
            this.addScriptButton.TabIndex = 2;
            this.addScriptButton.Text = "Add";
            this.addScriptButton.UseVisualStyleBackColor = true;
            this.addScriptButton.Click += new System.EventHandler(this.addScriptButton_Click);
            // 
            // moveUpButton
            // 
            this.moveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveUpButton.Enabled = false;
            this.moveUpButton.Image = global::GitUI.Properties.Resources._3;
            this.moveUpButton.Location = new System.Drawing.Point(761, 35);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(26, 23);
            this.moveUpButton.TabIndex = 1;
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // ScriptList
            // 
            this.ScriptList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ScriptList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.ScriptList.FullRowSelect = true;
            this.ScriptList.Location = new System.Drawing.Point(9, 7);
            this.ScriptList.Name = "ScriptList";
            this.ScriptList.Size = new System.Drawing.Size(718, 210);
            this.ScriptList.TabIndex = 0;
            this.ScriptList.UseCompatibleStateImageBehavior = false;
            this.ScriptList.View = System.Windows.Forms.View.Details;
            this.ScriptList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ScriptList_ItemSelectionChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No.";
            this.columnHeader1.Width = 30;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Command";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Arguments";
            this.columnHeader4.Width = 200;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Context menu";
            this.columnHeader5.Width = 80;
            // 
            // tabPageHotkeys
            // 
            this.tabPageHotkeys.Controls.Add(this.controlHotkeys);
            this.tabPageHotkeys.Location = new System.Drawing.Point(4, 22);
            this.tabPageHotkeys.Name = "tabPageHotkeys";
            this.tabPageHotkeys.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHotkeys.Size = new System.Drawing.Size(824, 431);
            this.tabPageHotkeys.TabIndex = 9;
            this.tabPageHotkeys.Text = "Hotkeys";
            this.tabPageHotkeys.UseVisualStyleBackColor = true;
            // 
            // controlHotkeys
            // 
            this.controlHotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlHotkeys.Location = new System.Drawing.Point(3, 3);
            this.controlHotkeys.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.controlHotkeys.Name = "controlHotkeys";
            this.controlHotkeys.Size = new System.Drawing.Size(818, 425);
            this.controlHotkeys.TabIndex = 0;
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
            this.splitContainer1.Size = new System.Drawing.Size(832, 490);
            this.splitContainer1.SplitterDistance = 457;
            this.splitContainer1.TabIndex = 1;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(740, 2);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(88, 25);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "OK";
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
            this.repositoryBindingSource.DataSource = typeof(GitCommands.Repository.Repository);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 490);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(726, 524);
            this.Name = "FormSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.Shown += new System.EventHandler(this.FormSettings_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.LocalSettings.ResumeLayout(false);
            this.LocalSettings.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.InvalidGitPathLocal.ResumeLayout(false);
            this.InvalidGitPathLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.TabPageGit.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.TabPageGitExtensions.ResumeLayout(false);
            this.TabPageGitExtensions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.StartPage.ResumeLayout(false);
            this.AppearancePage.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_authorImageSize)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.GlobalSettingsPage.ResumeLayout(false);
            this.GlobalSettingsPage.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.InvalidGitPathGlobal.ResumeLayout(false);
            this.InvalidGitPathGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.Ssh.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.scriptsTab.ResumeLayout(false);
            this.scriptsTab.PerformLayout();
            this.tabPageHotkeys.ResumeLayout(false);
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
        private System.Windows.Forms.ComboBox GlobalEditor;
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
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_MaxCommits;
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
        private System.Windows.Forms.Label EncodingLabel;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Encoding;
        private System.Windows.Forms.TabPage AppearancePage;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorSectionLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemovedLine;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAddedLineLabel;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemoteBranchLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorOtherLabel;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorTagLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorBranchLabel;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label28;
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
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemovedLineDiffLabel;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAddedLineDiffLabel;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox ShowAuthorGravatar;
        private System.Windows.Forms.Button ClearImageCache;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_authorImageSize;
        private System.Windows.Forms.TabPage StartPage;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private DashboardEditor dashboardEditor1;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_DaysToCacheImages;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Button DiffToolCmdSuggest;
        private System.Windows.Forms.ComboBox DifftoolCmd;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.ComboBox Language;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.LinkLabel helpTranslate;
        private System.Windows.Forms.TabPage TabPageGit;
        private System.Windows.Forms.CheckBox MulticolorBranches;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button otherHomeBrowse;
        private System.Windows.Forms.TextBox otherHomeDir;
        private System.Windows.Forms.RadioButton otherHome;
        private System.Windows.Forms.RadioButton userprofileHome;
        private System.Windows.Forms.RadioButton defaultHome;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.CheckBox BranchBorders;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorGraphLabel;
        private System.Windows.Forms.CheckBox StripedBanchChange;
        private System.Windows.Forms.Label showGitStatusInToolbarLabel;
        private System.Windows.Forms.CheckBox ShowGitStatusInToolbar;
        private System.Windows.Forms.NumericUpDown RevisionGridQuickSearchTimeout;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label showErrorsWhenStagingFilesLabel;
        private System.Windows.Forms.CheckBox showErrorsWhenStagingFiles;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.RadioButton globalAutoCrlfFalse;
        private System.Windows.Forms.RadioButton globalAutoCrlfInput;
        private System.Windows.Forms.RadioButton globalAutoCrlfTrue;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton localAutoCrlfFalse;
        private System.Windows.Forms.RadioButton localAutoCrlfInput;
        private System.Windows.Forms.RadioButton localAutoCrlfTrue;
        private System.Windows.Forms.CheckBox DrawNonRelativesGray;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.CheckBox ShowCurrentChangesInRevisionGraph;
        private System.Windows.Forms.CheckBox ShowStashCountInBrowseWindow;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.TabPage scriptsTab;
        private System.Windows.Forms.CheckBox inMenuCheckBox;
        private System.Windows.Forms.ComboBox noImageService;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.Label argumentsLabel;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Button browseScriptButton;
        private System.Windows.Forms.Button cancelScriptButton;
        private System.Windows.Forms.Button saveScriptButton;
        private System.Windows.Forms.RichTextBox argumentsTextBox;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button removeScriptButton;
        private System.Windows.Forms.Button editScriptButton;
        private System.Windows.Forms.Button addScriptButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.ListView ScriptList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.Button translationConfig;
        private System.Windows.Forms.LinkLabel downloadDictionary;
        private System.Windows.Forms.CheckBox DrawNonRelativesTextGray;
        private System.Windows.Forms.Button translationConfig_Fix;
        private System.Windows.Forms.Button SshConfig_Fix;
        private System.Windows.Forms.Button GitExtensionsInstall_Fix;
        private System.Windows.Forms.Button GitBinFound_Fix;
        private System.Windows.Forms.Button ShellExtensionsRegistered_Fix;
        private System.Windows.Forms.Button DiffTool2_Fix;
        private System.Windows.Forms.Button DiffTool_Fix;
        private System.Windows.Forms.Button UserNameSet_Fix;
        private System.Windows.Forms.Button GitFound_Fix;
        private System.Windows.Forms.TabPage tabPageHotkeys;
        private Hotkey.ControlHotkeys controlHotkeys;

    }
}
