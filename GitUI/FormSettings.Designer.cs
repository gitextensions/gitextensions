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
            this.tpLocalSettings = new System.Windows.Forms.TabPage();
            this.Local_AppEncoding = new System.Windows.Forms.ComboBox();
            this.LogEncodingLabel = new System.Windows.Forms.Label();
            this.label61 = new System.Windows.Forms.Label();
            this.Local_FilesEncoding = new System.Windows.Forms.ComboBox();
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
            this.LocalMergeTool = new System.Windows.Forms.ComboBox();
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
            this.tpChecklist = new System.Windows.Forms.TabPage();
            this.translationConfig_Fix = new System.Windows.Forms.Button();
            this.SshConfig_Fix = new System.Windows.Forms.Button();
            this.GitExtensionsInstall_Fix = new System.Windows.Forms.Button();
            this.GitBinFound_Fix = new System.Windows.Forms.Button();
            this.ShellExtensionsRegistered_Fix = new System.Windows.Forms.Button();
            this.DiffTool_Fix = new System.Windows.Forms.Button();
            this.MergeTool_Fix = new System.Windows.Forms.Button();
            this.UserNameSet_Fix = new System.Windows.Forms.Button();
            this.GitFound_Fix = new System.Windows.Forms.Button();
            this.translationConfig = new System.Windows.Forms.Button();
            this.DiffTool = new System.Windows.Forms.Button();
            this.SshConfig = new System.Windows.Forms.Button();
            this.GitBinFound = new System.Windows.Forms.Button();
            this.Rescan = new System.Windows.Forms.Button();
            this.CheckAtStartup = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.GitFound = new System.Windows.Forms.Button();
            this.MergeTool = new System.Windows.Forms.Button();
            this.UserNameSet = new System.Windows.Forms.Button();
            this.ShellExtensionsRegistered = new System.Windows.Forms.Button();
            this.GitExtensionsInstall = new System.Windows.Forms.Button();
            this.tpGit = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.homeIsSetToLabel = new System.Windows.Forms.Label();
            this.ChangeHomeButton = new System.Windows.Forms.Button();
            this.label51 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.downloadMsysgit = new System.Windows.Forms.LinkLabel();
            this.label50 = new System.Windows.Forms.Label();
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.tpGitExtensions = new System.Windows.Forms.TabPage();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.label49 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.Dictionary = new System.Windows.Forms.ComboBox();
            this.downloadDictionary = new System.Windows.Forms.LinkLabel();
            this.Language = new System.Windows.Forms.ComboBox();
            this.helpTranslate = new System.Windows.Forms.LinkLabel();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.chkCloseProcessDialog = new System.Windows.Forms.CheckBox();
            this.chkShowGitCommandLine = new System.Windows.Forms.CheckBox();
            this.chkStartWithRecentWorkingDir = new System.Windows.Forms.CheckBox();
            this.RevisionGridQuickSearchTimeout = new System.Windows.Forms.NumericUpDown();
            this.chkStashUntrackedFiles = new System.Windows.Forms.CheckBox();
            this.label24 = new System.Windows.Forms.Label();
            this.chkWarnBeforeCheckout = new System.Windows.Forms.CheckBox();
            this.chkUsePatienceDiffAlgorithm = new System.Windows.Forms.CheckBox();
            this.chkShowErrorsWhenStagingFiles = new System.Windows.Forms.CheckBox();
            this.chkFollowRenamesInFileHistory = new System.Windows.Forms.CheckBox();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.chkShowGitStatusInToolbar = new System.Windows.Forms.CheckBox();
            this.chkShowCurrentChangesInRevisionGraph = new System.Windows.Forms.CheckBox();
            this.chkUseFastChecks = new System.Windows.Forms.CheckBox();
            this.chkShowStashCountInBrowseWindow = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.tpAppearance = new System.Windows.Forms.TabPage();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.chkShowRelativeDate = new System.Windows.Forms.CheckBox();
            this.chkShowCurrentBranchInVisualStudio = new System.Windows.Forms.CheckBox();
            this.truncatePathMethod = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_truncatePathMethod = new System.Windows.Forms.ComboBox();
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
            this.tpColors = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label55 = new System.Windows.Forms.Label();
            this.IconPreviewSmall = new System.Windows.Forms.PictureBox();
            this.IconPreview = new System.Windows.Forms.PictureBox();
            this.IconStyle = new System.Windows.Forms.ComboBox();
            this.label54 = new System.Windows.Forms.Label();
            this.LightblueIcon = new System.Windows.Forms.RadioButton();
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
            this.diffFontChangeButton = new System.Windows.Forms.Button();
            this.label43 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineDiffLabel = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorSectionLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLine = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineLabel = new System.Windows.Forms.Label();
            this.tpStart = new System.Windows.Forms.TabPage();
            this.dashboardEditor1 = new GitUI.DashboardEditor();
            this.tpGlobalSettings = new System.Windows.Forms.TabPage();
            this.Global_AppEncoding = new System.Windows.Forms.ComboBox();
            this.label59 = new System.Windows.Forms.Label();
            this.label60 = new System.Windows.Forms.Label();
            this.Global_FilesEncoding = new System.Windows.Forms.ComboBox();
            this.BrowseCommitTemplate = new System.Windows.Forms.Button();
            this.label57 = new System.Windows.Forms.Label();
            this.CommitTemplatePath = new System.Windows.Forms.TextBox();
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
            this.MergeToolCmdSuggest = new System.Windows.Forms.Button();
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
            this.tpSsh = new System.Windows.Forms.TabPage();
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
            this.tpScriptsTab = new System.Windows.Forms.TabPage();
            this.lbl_icon = new System.Windows.Forms.Label();
            this.sbtn_icon = new GitUI.Script.SplitButton();
            this.contextMenuStrip_SplitButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptNeedsConfirmation = new System.Windows.Forms.CheckBox();
            this.labelOnEvent = new System.Windows.Forms.Label();
            this.scriptEvent = new System.Windows.Forms.ComboBox();
            this.scriptEnabled = new System.Windows.Forms.CheckBox();
            this.ScriptList = new System.Windows.Forms.DataGridView();
            this.HotkeyCommandIdentifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EnabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OnEvent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AskConfirmation = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.scriptInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.helpLabel = new System.Windows.Forms.Label();
            this.inMenuCheckBox = new System.Windows.Forms.CheckBox();
            this.argumentsLabel = new System.Windows.Forms.Label();
            this.commandLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.browseScriptButton = new System.Windows.Forms.Button();
            this.argumentsTextBox = new System.Windows.Forms.RichTextBox();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.removeScriptButton = new System.Windows.Forms.Button();
            this.addScriptButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.tpHotkeys = new System.Windows.Forms.TabPage();
            this.controlHotkeys = new GitUI.Hotkey.ControlHotkeys();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Ok = new System.Windows.Forms.Button();
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.directorySearcher2 = new System.DirectoryServices.DirectorySearcher();
            this.label10 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.diffFontDialog = new System.Windows.Forms.FontDialog();
            this.tpLocalSettings.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.InvalidGitPathLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpChecklist.SuspendLayout();
            this.tpGit.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tpGitExtensions.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).BeginInit();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).BeginInit();
            this.tpAppearance.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_authorImageSize)).BeginInit();
            this.tpColors.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreviewSmall)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tpStart.SuspendLayout();
            this.tpGlobalSettings.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tpSsh.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tpScriptsTab.SuspendLayout();
            this.contextMenuStrip_SplitButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScriptList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).BeginInit();
            this.tpHotkeys.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // tpLocalSettings
            // 
            this.tpLocalSettings.Controls.Add(this.Local_AppEncoding);
            this.tpLocalSettings.Controls.Add(this.LogEncodingLabel);
            this.tpLocalSettings.Controls.Add(this.label61);
            this.tpLocalSettings.Controls.Add(this.Local_FilesEncoding);
            this.tpLocalSettings.Controls.Add(this.groupBox10);
            this.tpLocalSettings.Controls.Add(this.label30);
            this.tpLocalSettings.Controls.Add(this.InvalidGitPathLocal);
            this.tpLocalSettings.Controls.Add(this.NoGitRepo);
            this.tpLocalSettings.Controls.Add(this.label20);
            this.tpLocalSettings.Controls.Add(this.LocalMergeTool);
            this.tpLocalSettings.Controls.Add(this.KeepMergeBackup);
            this.tpLocalSettings.Controls.Add(this.label8);
            this.tpLocalSettings.Controls.Add(this.Editor);
            this.tpLocalSettings.Controls.Add(this.label5);
            this.tpLocalSettings.Controls.Add(this.UserEmail);
            this.tpLocalSettings.Controls.Add(this.UserName);
            this.tpLocalSettings.Controls.Add(this.label2);
            this.tpLocalSettings.Controls.Add(this.label1);
            this.tpLocalSettings.Location = new System.Drawing.Point(4, 24);
            this.tpLocalSettings.Name = "tpLocalSettings";
            this.tpLocalSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tpLocalSettings.Size = new System.Drawing.Size(805, 518);
            this.tpLocalSettings.TabIndex = 0;
            this.tpLocalSettings.Text = "Local settings";
            this.tpLocalSettings.UseVisualStyleBackColor = true;
            // 
            // Local_AppEncoding
            // 
            this.Local_AppEncoding.FormattingEnabled = true;
            this.Local_AppEncoding.Location = new System.Drawing.Point(150, 286);
            this.Local_AppEncoding.Name = "Local_AppEncoding";
            this.helpProvider1.SetShowHelp(this.Local_AppEncoding, true);
            this.Local_AppEncoding.Size = new System.Drawing.Size(250, 23);
            this.Local_AppEncoding.TabIndex = 47;
            // 
            // LogEncodingLabel
            // 
            this.LogEncodingLabel.AutoSize = true;
            this.LogEncodingLabel.Location = new System.Drawing.Point(15, 289);
            this.LogEncodingLabel.Name = "LogEncodingLabel";
            this.LogEncodingLabel.Size = new System.Drawing.Size(130, 15);
            this.LogEncodingLabel.TabIndex = 46;
            this.LogEncodingLabel.Text = "GitExtensions encoding";
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Location = new System.Drawing.Point(14, 261);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(83, 15);
            this.label61.TabIndex = 45;
            this.label61.Text = "Files encoding";
            // 
            // Local_FilesEncoding
            // 
            this.Local_FilesEncoding.FormattingEnabled = true;
            this.Local_FilesEncoding.Location = new System.Drawing.Point(150, 258);
            this.Local_FilesEncoding.Name = "Local_FilesEncoding";
            this.Local_FilesEncoding.Size = new System.Drawing.Size(250, 23);
            this.Local_FilesEncoding.TabIndex = 44;
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
            this.groupBox10.Size = new System.Drawing.Size(784, 105);
            this.groupBox10.TabIndex = 32;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Line endings";
            // 
            // localAutoCrlfFalse
            // 
            this.localAutoCrlfFalse.AutoSize = true;
            this.localAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.localAutoCrlfFalse.Name = "localAutoCrlfFalse";
            this.localAutoCrlfFalse.Size = new System.Drawing.Size(349, 19);
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
            this.localAutoCrlfInput.Size = new System.Drawing.Size(448, 19);
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
            this.localAutoCrlfTrue.Size = new System.Drawing.Size(495, 19);
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
            this.label30.Size = new System.Drawing.Size(174, 15);
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
            this.label21.Size = new System.Drawing.Size(193, 45);
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
            this.NoGitRepo.Size = new System.Drawing.Size(122, 15);
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
            this.label20.Size = new System.Drawing.Size(165, 62);
            this.label20.TabIndex = 11;
            this.label20.Text = "You only need local settings\r\nif you want to override the \r\nglobal settings for t" +
    "he current\r\nrepository.";
            // 
            // LocalMergeTool
            // 
            this.LocalMergeTool.FormattingEnabled = true;
            this.LocalMergeTool.Items.AddRange(new object[] {
            "kdiff3",
            "p4merge"});
            this.LocalMergeTool.Location = new System.Drawing.Point(150, 94);
            this.LocalMergeTool.Name = "LocalMergeTool";
            this.LocalMergeTool.Size = new System.Drawing.Size(159, 23);
            this.LocalMergeTool.TabIndex = 10;
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
            this.label8.Size = new System.Drawing.Size(62, 15);
            this.label8.TabIndex = 6;
            this.label8.Text = "Mergetool";
            // 
            // Editor
            // 
            this.Editor.Location = new System.Drawing.Point(150, 67);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(304, 23);
            this.Editor.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "Editor";
            // 
            // UserEmail
            // 
            this.UserEmail.Location = new System.Drawing.Point(150, 40);
            this.UserEmail.Name = "UserEmail";
            this.UserEmail.Size = new System.Drawing.Size(280, 23);
            this.UserEmail.TabIndex = 3;
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(150, 12);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(280, 23);
            this.UserName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "User email";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "User name";
            // 
            // SmtpServer
            // 
            this.SmtpServer.Location = new System.Drawing.Point(394, 229);
            this.SmtpServer.Name = "SmtpServer";
            this.SmtpServer.Size = new System.Drawing.Size(242, 23);
            this.SmtpServer.TabIndex = 17;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 232);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(224, 15);
            this.label23.TabIndex = 18;
            this.label23.Text = "Smtp server for sending patches by email";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpChecklist);
            this.tabControl1.Controls.Add(this.tpGit);
            this.tabControl1.Controls.Add(this.tpGitExtensions);
            this.tabControl1.Controls.Add(this.tpAppearance);
            this.tabControl1.Controls.Add(this.tpColors);
            this.tabControl1.Controls.Add(this.tpStart);
            this.tabControl1.Controls.Add(this.tpGlobalSettings);
            this.tabControl1.Controls.Add(this.tpLocalSettings);
            this.tabControl1.Controls.Add(this.tpSsh);
            this.tabControl1.Controls.Add(this.tpScriptsTab);
            this.tabControl1.Controls.Add(this.tpHotkeys);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(813, 546);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpChecklist
            // 
            this.tpChecklist.Controls.Add(this.translationConfig_Fix);
            this.tpChecklist.Controls.Add(this.SshConfig_Fix);
            this.tpChecklist.Controls.Add(this.GitExtensionsInstall_Fix);
            this.tpChecklist.Controls.Add(this.GitBinFound_Fix);
            this.tpChecklist.Controls.Add(this.ShellExtensionsRegistered_Fix);
            this.tpChecklist.Controls.Add(this.DiffTool_Fix);
            this.tpChecklist.Controls.Add(this.MergeTool_Fix);
            this.tpChecklist.Controls.Add(this.UserNameSet_Fix);
            this.tpChecklist.Controls.Add(this.GitFound_Fix);
            this.tpChecklist.Controls.Add(this.translationConfig);
            this.tpChecklist.Controls.Add(this.DiffTool);
            this.tpChecklist.Controls.Add(this.SshConfig);
            this.tpChecklist.Controls.Add(this.GitBinFound);
            this.tpChecklist.Controls.Add(this.Rescan);
            this.tpChecklist.Controls.Add(this.CheckAtStartup);
            this.tpChecklist.Controls.Add(this.label11);
            this.tpChecklist.Controls.Add(this.GitFound);
            this.tpChecklist.Controls.Add(this.MergeTool);
            this.tpChecklist.Controls.Add(this.UserNameSet);
            this.tpChecklist.Controls.Add(this.ShellExtensionsRegistered);
            this.tpChecklist.Controls.Add(this.GitExtensionsInstall);
            this.tpChecklist.Location = new System.Drawing.Point(4, 24);
            this.tpChecklist.Name = "tpChecklist";
            this.tpChecklist.Size = new System.Drawing.Size(805, 518);
            this.tpChecklist.TabIndex = 2;
            this.tpChecklist.Text = "Checklist";
            this.tpChecklist.UseVisualStyleBackColor = true;
            // 
            // translationConfig_Fix
            // 
            this.translationConfig_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.translationConfig_Fix.Location = new System.Drawing.Point(703, 303);
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
            this.SshConfig_Fix.Location = new System.Drawing.Point(703, 269);
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
            this.GitExtensionsInstall_Fix.Location = new System.Drawing.Point(703, 235);
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
            this.GitBinFound_Fix.Location = new System.Drawing.Point(703, 201);
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
            this.ShellExtensionsRegistered_Fix.Location = new System.Drawing.Point(703, 167);
            this.ShellExtensionsRegistered_Fix.Name = "ShellExtensionsRegistered_Fix";
            this.ShellExtensionsRegistered_Fix.Size = new System.Drawing.Size(91, 25);
            this.ShellExtensionsRegistered_Fix.TabIndex = 17;
            this.ShellExtensionsRegistered_Fix.Text = "Repair";
            this.ShellExtensionsRegistered_Fix.UseVisualStyleBackColor = true;
            this.ShellExtensionsRegistered_Fix.Visible = false;
            this.ShellExtensionsRegistered_Fix.Click += new System.EventHandler(this.ShellExtensionsRegistered_Click);
            // 
            // DiffTool_Fix
            // 
            this.DiffTool_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DiffTool_Fix.Location = new System.Drawing.Point(703, 133);
            this.DiffTool_Fix.Name = "DiffTool_Fix";
            this.DiffTool_Fix.Size = new System.Drawing.Size(91, 25);
            this.DiffTool_Fix.TabIndex = 16;
            this.DiffTool_Fix.Text = "Repair";
            this.DiffTool_Fix.UseVisualStyleBackColor = true;
            this.DiffTool_Fix.Visible = false;
            this.DiffTool_Fix.Click += new System.EventHandler(this.DiffToolFix_Click);
            // 
            // MergeTool_Fix
            // 
            this.MergeTool_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MergeTool_Fix.Location = new System.Drawing.Point(703, 99);
            this.MergeTool_Fix.Name = "MergeTool_Fix";
            this.MergeTool_Fix.Size = new System.Drawing.Size(91, 25);
            this.MergeTool_Fix.TabIndex = 15;
            this.MergeTool_Fix.Text = "Repair";
            this.MergeTool_Fix.UseVisualStyleBackColor = true;
            this.MergeTool_Fix.Visible = false;
            this.MergeTool_Fix.Click += new System.EventHandler(this.MergeToolFix_Click);
            // 
            // UserNameSet_Fix
            // 
            this.UserNameSet_Fix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UserNameSet_Fix.Location = new System.Drawing.Point(703, 65);
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
            this.GitFound_Fix.Location = new System.Drawing.Point(703, 31);
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
            this.translationConfig.Size = new System.Drawing.Size(788, 29);
            this.translationConfig.TabIndex = 12;
            this.translationConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.translationConfig.UseVisualStyleBackColor = false;
            this.translationConfig.Visible = false;
            this.translationConfig.Click += new System.EventHandler(this.translationConfig_Click);
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
            this.DiffTool.Location = new System.Drawing.Point(9, 131);
            this.DiffTool.Name = "DiffTool";
            this.DiffTool.Size = new System.Drawing.Size(788, 29);
            this.DiffTool.TabIndex = 11;
            this.DiffTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool.UseVisualStyleBackColor = false;
            this.DiffTool.Visible = false;
            this.DiffTool.Click += new System.EventHandler(this.DiffToolFix_Click);
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
            this.SshConfig.Size = new System.Drawing.Size(788, 29);
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
            this.GitBinFound.Size = new System.Drawing.Size(788, 29);
            this.GitBinFound.TabIndex = 9;
            this.GitBinFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitBinFound.UseVisualStyleBackColor = false;
            this.GitBinFound.Visible = false;
            this.GitBinFound.Click += new System.EventHandler(this.GitBinFound_Click);
            // 
            // Rescan
            // 
            this.Rescan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Rescan.Location = new System.Drawing.Point(601, 346);
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
            this.label11.Size = new System.Drawing.Size(480, 15);
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
            this.GitFound.Size = new System.Drawing.Size(788, 29);
            this.GitFound.TabIndex = 5;
            this.GitFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitFound.UseVisualStyleBackColor = false;
            this.GitFound.Visible = false;
            this.GitFound.Click += new System.EventHandler(this.GitFound_Click);
            // 
            // MergeTool
            // 
            this.MergeTool.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MergeTool.BackColor = System.Drawing.Color.Gray;
            this.MergeTool.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MergeTool.FlatAppearance.BorderSize = 0;
            this.MergeTool.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.MergeTool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MergeTool.Location = new System.Drawing.Point(9, 97);
            this.MergeTool.Name = "MergeTool";
            this.MergeTool.Size = new System.Drawing.Size(788, 29);
            this.MergeTool.TabIndex = 4;
            this.MergeTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MergeTool.UseVisualStyleBackColor = false;
            this.MergeTool.Visible = false;
            this.MergeTool.Click += new System.EventHandler(this.MergeToolFix_Click);
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
            this.UserNameSet.Size = new System.Drawing.Size(788, 29);
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
            this.ShellExtensionsRegistered.Size = new System.Drawing.Size(788, 29);
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
            this.GitExtensionsInstall.Size = new System.Drawing.Size(788, 29);
            this.GitExtensionsInstall.TabIndex = 1;
            this.GitExtensionsInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitExtensionsInstall.UseVisualStyleBackColor = false;
            this.GitExtensionsInstall.Visible = false;
            this.GitExtensionsInstall.Click += new System.EventHandler(this.GitExtensionsInstall_Click);
            // 
            // tpGit
            // 
            this.tpGit.Controls.Add(this.groupBox8);
            this.tpGit.Controls.Add(this.groupBox7);
            this.tpGit.Location = new System.Drawing.Point(4, 24);
            this.tpGit.Name = "tpGit";
            this.tpGit.Size = new System.Drawing.Size(805, 518);
            this.tpGit.TabIndex = 7;
            this.tpGit.Text = "Git";
            this.tpGit.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox8.Controls.Add(this.homeIsSetToLabel);
            this.groupBox8.Controls.Add(this.ChangeHomeButton);
            this.groupBox8.Controls.Add(this.label51);
            this.groupBox8.Location = new System.Drawing.Point(5, 134);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(794, 136);
            this.groupBox8.TabIndex = 10;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Environment";
            // 
            // homeIsSetToLabel
            // 
            this.homeIsSetToLabel.AutoSize = true;
            this.homeIsSetToLabel.Location = new System.Drawing.Point(11, 59);
            this.homeIsSetToLabel.Name = "homeIsSetToLabel";
            this.homeIsSetToLabel.Size = new System.Drawing.Size(105, 15);
            this.homeIsSetToLabel.TabIndex = 12;
            this.homeIsSetToLabel.Text = "HOME is set to: {0}";
            // 
            // ChangeHomeButton
            // 
            this.ChangeHomeButton.Location = new System.Drawing.Point(11, 92);
            this.ChangeHomeButton.Name = "ChangeHomeButton";
            this.ChangeHomeButton.Size = new System.Drawing.Size(132, 23);
            this.ChangeHomeButton.TabIndex = 11;
            this.ChangeHomeButton.Text = "Change HOME";
            this.ChangeHomeButton.UseVisualStyleBackColor = true;
            this.ChangeHomeButton.Click += new System.EventHandler(this.ChangeHomeButton_Click);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(8, 19);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(624, 30);
            this.label51.TabIndex = 0;
            this.label51.Text = resources.GetString("label51.Text");
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Controls.Add(this.downloadMsysgit);
            this.groupBox7.Controls.Add(this.label50);
            this.groupBox7.Controls.Add(this.BrowseGitBinPath);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.label14);
            this.groupBox7.Controls.Add(this.GitPath);
            this.groupBox7.Controls.Add(this.BrowseGitPath);
            this.groupBox7.Controls.Add(this.GitBinPath);
            this.groupBox7.Location = new System.Drawing.Point(5, 4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(794, 115);
            this.groupBox7.TabIndex = 9;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Git";
            // 
            // downloadMsysgit
            // 
            this.downloadMsysgit.AutoSize = true;
            this.downloadMsysgit.Location = new System.Drawing.Point(373, 91);
            this.downloadMsysgit.Name = "downloadMsysgit";
            this.downloadMsysgit.Size = new System.Drawing.Size(105, 15);
            this.downloadMsysgit.TabIndex = 10;
            this.downloadMsysgit.TabStop = true;
            this.downloadMsysgit.Text = "Download msysgit";
            this.downloadMsysgit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadMsysgit_LinkClicked);
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(8, 18);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(506, 15);
            this.label50.TabIndex = 9;
            this.label50.Text = "Git Extensions can use msysgit or cygwin to access git repositories. Set the corr" +
    "ect paths below.";
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitBinPath.Location = new System.Drawing.Point(713, 63);
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
            this.label13.Size = new System.Drawing.Size(247, 15);
            this.label13.TabIndex = 3;
            this.label13.Text = "Command used to run git (git.cmd or git.exe)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 70);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(314, 15);
            this.label14.TabIndex = 6;
            this.label14.Text = "Path to linux tools (sh). Leave empty when it is in the path.";
            // 
            // GitPath
            // 
            this.GitPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GitPath.Location = new System.Drawing.Point(373, 39);
            this.GitPath.Name = "GitPath";
            this.GitPath.Size = new System.Drawing.Size(337, 23);
            this.GitPath.TabIndex = 4;
            this.GitPath.TextChanged += new System.EventHandler(this.GitPath_TextChanged);
            // 
            // BrowseGitPath
            // 
            this.BrowseGitPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitPath.Location = new System.Drawing.Point(713, 37);
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
            this.GitBinPath.Size = new System.Drawing.Size(337, 23);
            this.GitBinPath.TabIndex = 7;
            // 
            // tpGitExtensions
            // 
            this.tpGitExtensions.Controls.Add(this.groupBox13);
            this.tpGitExtensions.Controls.Add(this.groupBox12);
            this.tpGitExtensions.Controls.Add(this.groupBox11);
            this.tpGitExtensions.Location = new System.Drawing.Point(4, 24);
            this.tpGitExtensions.Name = "tpGitExtensions";
            this.tpGitExtensions.Size = new System.Drawing.Size(805, 518);
            this.tpGitExtensions.TabIndex = 3;
            this.tpGitExtensions.Text = "Git extensions";
            this.tpGitExtensions.UseVisualStyleBackColor = true;
            this.tpGitExtensions.Click += new System.EventHandler(this.TabPageGitExtensions_Click);
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.label49);
            this.groupBox13.Controls.Add(this.label22);
            this.groupBox13.Controls.Add(this.Dictionary);
            this.groupBox13.Controls.Add(this.downloadDictionary);
            this.groupBox13.Controls.Add(this.Language);
            this.groupBox13.Controls.Add(this.helpTranslate);
            this.groupBox13.Location = new System.Drawing.Point(11, 3);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(772, 88);
            this.groupBox13.TabIndex = 55;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Language";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(6, 21);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(150, 15);
            this.label49.TabIndex = 28;
            this.label49.Text = "Language (restart required)";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(6, 50);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(170, 15);
            this.label22.TabIndex = 14;
            this.label22.Text = "Dictionary for spelling checker.";
            // 
            // Dictionary
            // 
            this.Dictionary.FormattingEnabled = true;
            this.Dictionary.Location = new System.Drawing.Point(394, 47);
            this.Dictionary.Name = "Dictionary";
            this.Dictionary.Size = new System.Drawing.Size(169, 23);
            this.Dictionary.TabIndex = 15;
            this.Dictionary.DropDown += new System.EventHandler(this.Dictionary_DropDown);
            // 
            // downloadDictionary
            // 
            this.downloadDictionary.AutoSize = true;
            this.downloadDictionary.Location = new System.Drawing.Point(569, 50);
            this.downloadDictionary.Name = "downloadDictionary";
            this.downloadDictionary.Size = new System.Drawing.Size(117, 15);
            this.downloadDictionary.TabIndex = 40;
            this.downloadDictionary.TabStop = true;
            this.downloadDictionary.Text = "Download dictionary";
            this.downloadDictionary.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadDictionary_LinkClicked);
            // 
            // Language
            // 
            this.Language.FormattingEnabled = true;
            this.Language.Items.AddRange(new object[] {
            "en-US",
            "ja-JP",
            "nl-NL"});
            this.Language.Location = new System.Drawing.Point(394, 18);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(169, 23);
            this.Language.TabIndex = 29;
            // 
            // helpTranslate
            // 
            this.helpTranslate.AutoSize = true;
            this.helpTranslate.Location = new System.Drawing.Point(569, 21);
            this.helpTranslate.Name = "helpTranslate";
            this.helpTranslate.Size = new System.Drawing.Size(80, 15);
            this.helpTranslate.TabIndex = 30;
            this.helpTranslate.TabStop = true;
            this.helpTranslate.Text = "Help translate";
            this.helpTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpTranslate_LinkClicked);
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.chkCloseProcessDialog);
            this.groupBox12.Controls.Add(this.chkShowGitCommandLine);
            this.groupBox12.Controls.Add(this.chkStartWithRecentWorkingDir);
            this.groupBox12.Controls.Add(this.label23);
            this.groupBox12.Controls.Add(this.SmtpServer);
            this.groupBox12.Controls.Add(this.RevisionGridQuickSearchTimeout);
            this.groupBox12.Controls.Add(this.chkStashUntrackedFiles);
            this.groupBox12.Controls.Add(this.label24);
            this.groupBox12.Controls.Add(this.chkWarnBeforeCheckout);
            this.groupBox12.Controls.Add(this.chkUsePatienceDiffAlgorithm);
            this.groupBox12.Controls.Add(this.chkShowErrorsWhenStagingFiles);
            this.groupBox12.Controls.Add(this.chkFollowRenamesInFileHistory);
            this.groupBox12.Location = new System.Drawing.Point(11, 257);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(772, 260);
            this.groupBox12.TabIndex = 54;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Behaviour";
            // 
            // chkCloseProcessDialog
            // 
            this.chkCloseProcessDialog.AutoSize = true;
            this.chkCloseProcessDialog.Location = new System.Drawing.Point(10, 20);
            this.chkCloseProcessDialog.Name = "chkCloseProcessDialog";
            this.chkCloseProcessDialog.Size = new System.Drawing.Size(354, 19);
            this.chkCloseProcessDialog.TabIndex = 9;
            this.chkCloseProcessDialog.Text = "Close process dialog automatically when process is succeeded";
            this.chkCloseProcessDialog.UseVisualStyleBackColor = true;
            // 
            // chkShowGitCommandLine
            // 
            this.chkShowGitCommandLine.AutoSize = true;
            this.chkShowGitCommandLine.Location = new System.Drawing.Point(10, 43);
            this.chkShowGitCommandLine.Name = "chkShowGitCommandLine";
            this.chkShowGitCommandLine.Size = new System.Drawing.Size(315, 19);
            this.chkShowGitCommandLine.TabIndex = 11;
            this.chkShowGitCommandLine.Text = "Show Git commandline dialog when executing process";
            this.chkShowGitCommandLine.UseVisualStyleBackColor = true;
            // 
            // chkStartWithRecentWorkingDir
            // 
            this.chkStartWithRecentWorkingDir.AutoSize = true;
            this.chkStartWithRecentWorkingDir.Location = new System.Drawing.Point(10, 178);
            this.chkStartWithRecentWorkingDir.Name = "chkStartWithRecentWorkingDir";
            this.chkStartWithRecentWorkingDir.Size = new System.Drawing.Size(196, 19);
            this.chkStartWithRecentWorkingDir.TabIndex = 52;
            this.chkStartWithRecentWorkingDir.Text = "Open last working dir on startup";
            this.chkStartWithRecentWorkingDir.UseVisualStyleBackColor = true;
            // 
            // RevisionGridQuickSearchTimeout
            // 
            this.RevisionGridQuickSearchTimeout.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RevisionGridQuickSearchTimeout.Location = new System.Drawing.Point(394, 203);
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
            this.RevisionGridQuickSearchTimeout.Size = new System.Drawing.Size(123, 23);
            this.RevisionGridQuickSearchTimeout.TabIndex = 33;
            this.RevisionGridQuickSearchTimeout.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // chkStashUntrackedFiles
            // 
            this.chkStashUntrackedFiles.AutoSize = true;
            this.chkStashUntrackedFiles.Location = new System.Drawing.Point(10, 110);
            this.chkStashUntrackedFiles.Name = "chkStashUntrackedFiles";
            this.chkStashUntrackedFiles.Size = new System.Drawing.Size(188, 19);
            this.chkStashUntrackedFiles.TabIndex = 51;
            this.chkStashUntrackedFiles.Text = "Include untracked files in stash";
            this.chkStashUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 205);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(216, 15);
            this.label24.TabIndex = 32;
            this.label24.Text = "Revision grid quick search timeout [ms]";
            // 
            // chkWarnBeforeCheckout
            // 
            this.chkWarnBeforeCheckout.AutoSize = true;
            this.chkWarnBeforeCheckout.Location = new System.Drawing.Point(10, 156);
            this.chkWarnBeforeCheckout.Name = "chkWarnBeforeCheckout";
            this.chkWarnBeforeCheckout.Size = new System.Drawing.Size(287, 19);
            this.chkWarnBeforeCheckout.TabIndex = 49;
            this.chkWarnBeforeCheckout.Text = "Warn of not committed changes before checkout";
            this.chkWarnBeforeCheckout.UseVisualStyleBackColor = true;
            // 
            // chkUsePatienceDiffAlgorithm
            // 
            this.chkUsePatienceDiffAlgorithm.AutoSize = true;
            this.chkUsePatienceDiffAlgorithm.Location = new System.Drawing.Point(10, 66);
            this.chkUsePatienceDiffAlgorithm.Name = "chkUsePatienceDiffAlgorithm";
            this.chkUsePatienceDiffAlgorithm.Size = new System.Drawing.Size(169, 19);
            this.chkUsePatienceDiffAlgorithm.TabIndex = 43;
            this.chkUsePatienceDiffAlgorithm.Text = "Use patience diff algorithm";
            this.chkUsePatienceDiffAlgorithm.UseVisualStyleBackColor = true;
            // 
            // chkShowErrorsWhenStagingFiles
            // 
            this.chkShowErrorsWhenStagingFiles.AutoSize = true;
            this.chkShowErrorsWhenStagingFiles.Location = new System.Drawing.Point(10, 87);
            this.chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
            this.chkShowErrorsWhenStagingFiles.Size = new System.Drawing.Size(186, 19);
            this.chkShowErrorsWhenStagingFiles.TabIndex = 34;
            this.chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
            this.chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
            // 
            // chkFollowRenamesInFileHistory
            // 
            this.chkFollowRenamesInFileHistory.AutoSize = true;
            this.chkFollowRenamesInFileHistory.Location = new System.Drawing.Point(10, 133);
            this.chkFollowRenamesInFileHistory.Name = "chkFollowRenamesInFileHistory";
            this.chkFollowRenamesInFileHistory.Size = new System.Drawing.Size(259, 19);
            this.chkFollowRenamesInFileHistory.TabIndex = 26;
            this.chkFollowRenamesInFileHistory.Text = "Follow renames in file history (experimental)";
            this.chkFollowRenamesInFileHistory.UseVisualStyleBackColor = true;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.chkShowGitStatusInToolbar);
            this.groupBox11.Controls.Add(this.chkShowCurrentChangesInRevisionGraph);
            this.groupBox11.Controls.Add(this.chkUseFastChecks);
            this.groupBox11.Controls.Add(this.chkShowStashCountInBrowseWindow);
            this.groupBox11.Controls.Add(this.label12);
            this.groupBox11.Controls.Add(this._NO_TRANSLATE_MaxCommits);
            this.groupBox11.Location = new System.Drawing.Point(11, 97);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(772, 154);
            this.groupBox11.TabIndex = 53;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Performance";
            // 
            // chkShowGitStatusInToolbar
            // 
            this.chkShowGitStatusInToolbar.AutoSize = true;
            this.chkShowGitStatusInToolbar.Location = new System.Drawing.Point(10, 23);
            this.chkShowGitStatusInToolbar.Name = "chkShowGitStatusInToolbar";
            this.chkShowGitStatusInToolbar.Size = new System.Drawing.Size(489, 19);
            this.chkShowGitStatusInToolbar.TabIndex = 31;
            this.chkShowGitStatusInToolbar.Text = "Show repository status in browse dialog (number of changes in toolbar, restart re" +
    "quired)";
            this.chkShowGitStatusInToolbar.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentChangesInRevisionGraph
            // 
            this.chkShowCurrentChangesInRevisionGraph.AutoSize = true;
            this.chkShowCurrentChangesInRevisionGraph.Location = new System.Drawing.Point(10, 43);
            this.chkShowCurrentChangesInRevisionGraph.Name = "chkShowCurrentChangesInRevisionGraph";
            this.chkShowCurrentChangesInRevisionGraph.Size = new System.Drawing.Size(335, 19);
            this.chkShowCurrentChangesInRevisionGraph.TabIndex = 36;
            this.chkShowCurrentChangesInRevisionGraph.Text = "Show current working dir changes in revision graph (slow!)";
            this.chkShowCurrentChangesInRevisionGraph.UseVisualStyleBackColor = true;
            // 
            // chkUseFastChecks
            // 
            this.chkUseFastChecks.AutoSize = true;
            this.chkUseFastChecks.Location = new System.Drawing.Point(10, 65);
            this.chkUseFastChecks.Name = "chkUseFastChecks";
            this.chkUseFastChecks.Size = new System.Drawing.Size(297, 19);
            this.chkUseFastChecks.TabIndex = 12;
            this.chkUseFastChecks.Text = "Use FileSystemWatcher to check if index is changed";
            this.chkUseFastChecks.UseVisualStyleBackColor = true;
            // 
            // chkShowStashCountInBrowseWindow
            // 
            this.chkShowStashCountInBrowseWindow.AutoSize = true;
            this.chkShowStashCountInBrowseWindow.Location = new System.Drawing.Point(10, 87);
            this.chkShowStashCountInBrowseWindow.Name = "chkShowStashCountInBrowseWindow";
            this.chkShowStashCountInBrowseWindow.Size = new System.Drawing.Size(289, 19);
            this.chkShowStashCountInBrowseWindow.TabIndex = 38;
            this.chkShowStashCountInBrowseWindow.Text = "Show stash count on status bar in browse window";
            this.chkShowStashCountInBrowseWindow.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 116);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(299, 15);
            this.label12.TabIndex = 0;
            this.label12.Text = "Limit number of commits that will be loaded at startup.";
            // 
            // _NO_TRANSLATE_MaxCommits
            // 
            this._NO_TRANSLATE_MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Location = new System.Drawing.Point(395, 115);
            this._NO_TRANSLATE_MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_MaxCommits.Name = "_NO_TRANSLATE_MaxCommits";
            this._NO_TRANSLATE_MaxCommits.Size = new System.Drawing.Size(123, 23);
            this._NO_TRANSLATE_MaxCommits.TabIndex = 2;
            this._NO_TRANSLATE_MaxCommits.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // tpAppearance
            // 
            this.tpAppearance.Controls.Add(this.groupBox14);
            this.tpAppearance.Controls.Add(this.groupBox6);
            this.tpAppearance.Location = new System.Drawing.Point(4, 24);
            this.tpAppearance.Name = "tpAppearance";
            this.tpAppearance.Padding = new System.Windows.Forms.Padding(3);
            this.tpAppearance.Size = new System.Drawing.Size(805, 518);
            this.tpAppearance.TabIndex = 10;
            this.tpAppearance.Text = "Appearance";
            this.tpAppearance.UseVisualStyleBackColor = true;
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.chkShowRelativeDate);
            this.groupBox14.Controls.Add(this.chkShowCurrentBranchInVisualStudio);
            this.groupBox14.Controls.Add(this.truncatePathMethod);
            this.groupBox14.Controls.Add(this._NO_TRANSLATE_truncatePathMethod);
            this.groupBox14.Location = new System.Drawing.Point(11, 5);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(772, 108);
            this.groupBox14.TabIndex = 52;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "General";
            // 
            // chkShowRelativeDate
            // 
            this.chkShowRelativeDate.AutoSize = true;
            this.chkShowRelativeDate.Location = new System.Drawing.Point(11, 20);
            this.chkShowRelativeDate.Name = "chkShowRelativeDate";
            this.chkShowRelativeDate.Size = new System.Drawing.Size(223, 19);
            this.chkShowRelativeDate.TabIndex = 47;
            this.chkShowRelativeDate.Text = "Show relative date instead of full date";
            this.chkShowRelativeDate.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentBranchInVisualStudio
            // 
            this.chkShowCurrentBranchInVisualStudio.AutoSize = true;
            this.chkShowCurrentBranchInVisualStudio.Location = new System.Drawing.Point(11, 43);
            this.chkShowCurrentBranchInVisualStudio.Name = "chkShowCurrentBranchInVisualStudio";
            this.chkShowCurrentBranchInVisualStudio.Size = new System.Drawing.Size(220, 19);
            this.chkShowCurrentBranchInVisualStudio.TabIndex = 48;
            this.chkShowCurrentBranchInVisualStudio.Text = "Show current branch in Visual Studio";
            this.chkShowCurrentBranchInVisualStudio.UseVisualStyleBackColor = true;
            // 
            // truncatePathMethod
            // 
            this.truncatePathMethod.AutoSize = true;
            this.truncatePathMethod.Location = new System.Drawing.Point(8, 68);
            this.truncatePathMethod.Name = "truncatePathMethod";
            this.truncatePathMethod.Size = new System.Drawing.Size(135, 15);
            this.truncatePathMethod.TabIndex = 50;
            this.truncatePathMethod.Text = "Truncate long filenames";
            // 
            // _NO_TRANSLATE_truncatePathMethod
            // 
            this._NO_TRANSLATE_truncatePathMethod.FormattingEnabled = true;
            this._NO_TRANSLATE_truncatePathMethod.Items.AddRange(new object[] {
            "none",
            "compact",
            "trimstart"});
            this._NO_TRANSLATE_truncatePathMethod.Location = new System.Drawing.Point(396, 65);
            this._NO_TRANSLATE_truncatePathMethod.Name = "_NO_TRANSLATE_truncatePathMethod";
            this._NO_TRANSLATE_truncatePathMethod.Size = new System.Drawing.Size(242, 23);
            this._NO_TRANSLATE_truncatePathMethod.TabIndex = 49;
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
            this.groupBox6.Location = new System.Drawing.Point(11, 121);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(772, 171);
            this.groupBox6.TabIndex = 51;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Author images";
            // 
            // noImageService
            // 
            this.noImageService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.noImageService.FormattingEnabled = true;
            this.noImageService.Location = new System.Drawing.Point(149, 101);
            this.noImageService.Name = "noImageService";
            this.noImageService.Size = new System.Drawing.Size(142, 23);
            this.noImageService.TabIndex = 9;
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(7, 104);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(98, 15);
            this.label53.TabIndex = 8;
            this.label53.Text = "No image service";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(241, 77);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(31, 15);
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
            this._NO_TRANSLATE_DaysToCacheImages.Size = new System.Drawing.Size(77, 23);
            this._NO_TRANSLATE_DaysToCacheImages.TabIndex = 6;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(7, 77);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(81, 15);
            this.label46.TabIndex = 5;
            this.label46.Text = "Cache images";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(7, 49);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(62, 15);
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
            this._NO_TRANSLATE_authorImageSize.Size = new System.Drawing.Size(77, 23);
            this._NO_TRANSLATE_authorImageSize.TabIndex = 3;
            // 
            // ClearImageCache
            // 
            this.ClearImageCache.Location = new System.Drawing.Point(6, 134);
            this.ClearImageCache.Name = "ClearImageCache";
            this.ClearImageCache.Size = new System.Drawing.Size(142, 25);
            this.ClearImageCache.TabIndex = 1;
            this.ClearImageCache.Text = "Clear image cache";
            this.ClearImageCache.UseVisualStyleBackColor = true;
            // 
            // ShowAuthorGravatar
            // 
            this.ShowAuthorGravatar.AutoSize = true;
            this.ShowAuthorGravatar.Location = new System.Drawing.Point(7, 20);
            this.ShowAuthorGravatar.Name = "ShowAuthorGravatar";
            this.ShowAuthorGravatar.Size = new System.Drawing.Size(220, 19);
            this.ShowAuthorGravatar.TabIndex = 0;
            this.ShowAuthorGravatar.Text = "Get author image from gravatar.com";
            this.ShowAuthorGravatar.UseVisualStyleBackColor = true;
            // 
            // tpColors
            // 
            this.tpColors.Controls.Add(this.groupBox5);
            this.tpColors.Controls.Add(this.groupBox4);
            this.tpColors.Controls.Add(this.groupBox3);
            this.tpColors.Location = new System.Drawing.Point(4, 24);
            this.tpColors.Name = "tpColors";
            this.tpColors.Size = new System.Drawing.Size(805, 518);
            this.tpColors.TabIndex = 5;
            this.tpColors.Text = "Colors";
            this.tpColors.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label55);
            this.groupBox5.Controls.Add(this.IconPreviewSmall);
            this.groupBox5.Controls.Add(this.IconPreview);
            this.groupBox5.Controls.Add(this.IconStyle);
            this.groupBox5.Controls.Add(this.label54);
            this.groupBox5.Controls.Add(this.LightblueIcon);
            this.groupBox5.Controls.Add(this.RandomIcon);
            this.groupBox5.Controls.Add(this.YellowIcon);
            this.groupBox5.Controls.Add(this.RedIcon);
            this.groupBox5.Controls.Add(this.GreenIcon);
            this.groupBox5.Controls.Add(this.PurpleIcon);
            this.groupBox5.Controls.Add(this.BlueIcon);
            this.groupBox5.Controls.Add(this.DefaultIcon);
            this.groupBox5.Location = new System.Drawing.Point(407, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(315, 279);
            this.groupBox5.TabIndex = 12;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Application Icon";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(13, 55);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(60, 15);
            this.label55.TabIndex = 14;
            this.label55.Text = "Icon color";
            // 
            // IconPreviewSmall
            // 
            this.IconPreviewSmall.Location = new System.Drawing.Point(227, 66);
            this.IconPreviewSmall.Name = "IconPreviewSmall";
            this.IconPreviewSmall.Size = new System.Drawing.Size(16, 16);
            this.IconPreviewSmall.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.IconPreviewSmall.TabIndex = 13;
            this.IconPreviewSmall.TabStop = false;
            // 
            // IconPreview
            // 
            this.IconPreview.Location = new System.Drawing.Point(265, 50);
            this.IconPreview.Name = "IconPreview";
            this.IconPreview.Size = new System.Drawing.Size(32, 32);
            this.IconPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.IconPreview.TabIndex = 12;
            this.IconPreview.TabStop = false;
            // 
            // IconStyle
            // 
            this.IconStyle.FormattingEnabled = true;
            this.IconStyle.Items.AddRange(new object[] {
            "Default",
            "Large",
            "Small",
            "Cow"});
            this.IconStyle.Location = new System.Drawing.Point(111, 23);
            this.IconStyle.Name = "IconStyle";
            this.IconStyle.Size = new System.Drawing.Size(121, 23);
            this.IconStyle.TabIndex = 11;
            this.IconStyle.SelectedIndexChanged += new System.EventHandler(this.IconStyle_SelectedIndexChanged);
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(13, 26);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(57, 15);
            this.label54.TabIndex = 10;
            this.label54.Text = "Icon style";
            // 
            // LightblueIcon
            // 
            this.LightblueIcon.AutoSize = true;
            this.LightblueIcon.Location = new System.Drawing.Point(111, 81);
            this.LightblueIcon.Name = "LightblueIcon";
            this.LightblueIcon.Size = new System.Drawing.Size(78, 19);
            this.LightblueIcon.TabIndex = 7;
            this.LightblueIcon.TabStop = true;
            this.LightblueIcon.Text = "Light blue";
            this.LightblueIcon.UseVisualStyleBackColor = true;
            this.LightblueIcon.CheckedChanged += new System.EventHandler(this.LightblueIcon_CheckedChanged);
            // 
            // RandomIcon
            // 
            this.RandomIcon.AutoSize = true;
            this.RandomIcon.Location = new System.Drawing.Point(111, 250);
            this.RandomIcon.Name = "RandomIcon";
            this.RandomIcon.Size = new System.Drawing.Size(70, 19);
            this.RandomIcon.TabIndex = 6;
            this.RandomIcon.TabStop = true;
            this.RandomIcon.Text = "Random";
            this.RandomIcon.UseVisualStyleBackColor = true;
            this.RandomIcon.CheckedChanged += new System.EventHandler(this.RandomIcon_CheckedChanged);
            // 
            // YellowIcon
            // 
            this.YellowIcon.AutoSize = true;
            this.YellowIcon.Location = new System.Drawing.Point(111, 222);
            this.YellowIcon.Name = "YellowIcon";
            this.YellowIcon.Size = new System.Drawing.Size(60, 19);
            this.YellowIcon.TabIndex = 5;
            this.YellowIcon.TabStop = true;
            this.YellowIcon.Text = "Yellow";
            this.YellowIcon.UseVisualStyleBackColor = true;
            this.YellowIcon.CheckedChanged += new System.EventHandler(this.YellowIcon_CheckedChanged);
            // 
            // RedIcon
            // 
            this.RedIcon.AutoSize = true;
            this.RedIcon.Location = new System.Drawing.Point(111, 194);
            this.RedIcon.Name = "RedIcon";
            this.RedIcon.Size = new System.Drawing.Size(45, 19);
            this.RedIcon.TabIndex = 4;
            this.RedIcon.TabStop = true;
            this.RedIcon.Text = "Red";
            this.RedIcon.UseVisualStyleBackColor = true;
            this.RedIcon.CheckedChanged += new System.EventHandler(this.RedIcon_CheckedChanged);
            // 
            // GreenIcon
            // 
            this.GreenIcon.AutoSize = true;
            this.GreenIcon.Location = new System.Drawing.Point(111, 165);
            this.GreenIcon.Name = "GreenIcon";
            this.GreenIcon.Size = new System.Drawing.Size(56, 19);
            this.GreenIcon.TabIndex = 3;
            this.GreenIcon.TabStop = true;
            this.GreenIcon.Text = "Green";
            this.GreenIcon.UseVisualStyleBackColor = true;
            this.GreenIcon.CheckedChanged += new System.EventHandler(this.GreenIcon_CheckedChanged);
            // 
            // PurpleIcon
            // 
            this.PurpleIcon.AutoSize = true;
            this.PurpleIcon.Location = new System.Drawing.Point(111, 137);
            this.PurpleIcon.Name = "PurpleIcon";
            this.PurpleIcon.Size = new System.Drawing.Size(59, 19);
            this.PurpleIcon.TabIndex = 2;
            this.PurpleIcon.TabStop = true;
            this.PurpleIcon.Text = "Purple";
            this.PurpleIcon.UseVisualStyleBackColor = true;
            this.PurpleIcon.CheckedChanged += new System.EventHandler(this.PurpleIcon_CheckedChanged);
            // 
            // BlueIcon
            // 
            this.BlueIcon.AutoSize = true;
            this.BlueIcon.Location = new System.Drawing.Point(111, 109);
            this.BlueIcon.Name = "BlueIcon";
            this.BlueIcon.Size = new System.Drawing.Size(48, 19);
            this.BlueIcon.TabIndex = 1;
            this.BlueIcon.TabStop = true;
            this.BlueIcon.Text = "Blue";
            this.BlueIcon.UseVisualStyleBackColor = true;
            this.BlueIcon.CheckedChanged += new System.EventHandler(this.BlueIcon_CheckedChanged);
            // 
            // DefaultIcon
            // 
            this.DefaultIcon.AutoSize = true;
            this.DefaultIcon.Location = new System.Drawing.Point(111, 53);
            this.DefaultIcon.Name = "DefaultIcon";
            this.DefaultIcon.Size = new System.Drawing.Size(63, 19);
            this.DefaultIcon.TabIndex = 0;
            this.DefaultIcon.TabStop = true;
            this.DefaultIcon.Text = "Default";
            this.DefaultIcon.UseVisualStyleBackColor = true;
            this.DefaultIcon.CheckedChanged += new System.EventHandler(this.DefaultIcon_CheckedChanged);
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
            this.groupBox4.Size = new System.Drawing.Size(387, 279);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Revision graph";
            // 
            // DrawNonRelativesTextGray
            // 
            this.DrawNonRelativesTextGray.AutoSize = true;
            this.DrawNonRelativesTextGray.Location = new System.Drawing.Point(9, 120);
            this.DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(171, 19);
            this.DrawNonRelativesTextGray.TabIndex = 17;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(9, 96);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(183, 19);
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
            this._NO_TRANSLATE_ColorGraphLabel.Location = new System.Drawing.Point(287, 21);
            this._NO_TRANSLATE_ColorGraphLabel.Name = "_NO_TRANSLATE_ColorGraphLabel";
            this._NO_TRANSLATE_ColorGraphLabel.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorGraphLabel.TabIndex = 15;
            this._NO_TRANSLATE_ColorGraphLabel.Text = "Red";
            this._NO_TRANSLATE_ColorGraphLabel.Click += new System.EventHandler(this._ColorGraphLabel_Click);
            // 
            // StripedBanchChange
            // 
            this.StripedBanchChange.AutoSize = true;
            this.StripedBanchChange.Location = new System.Drawing.Point(9, 45);
            this.StripedBanchChange.Name = "StripedBanchChange";
            this.StripedBanchChange.Size = new System.Drawing.Size(145, 19);
            this.StripedBanchChange.TabIndex = 14;
            this.StripedBanchChange.Text = "Striped branch change";
            this.StripedBanchChange.UseVisualStyleBackColor = true;
            // 
            // BranchBorders
            // 
            this.BranchBorders.AutoSize = true;
            this.BranchBorders.Location = new System.Drawing.Point(9, 71);
            this.BranchBorders.Name = "BranchBorders";
            this.BranchBorders.Size = new System.Drawing.Size(136, 19);
            this.BranchBorders.TabIndex = 13;
            this.BranchBorders.Text = "Draw branch borders";
            this.BranchBorders.UseVisualStyleBackColor = true;
            // 
            // MulticolorBranches
            // 
            this.MulticolorBranches.AutoSize = true;
            this.MulticolorBranches.Location = new System.Drawing.Point(9, 20);
            this.MulticolorBranches.Name = "MulticolorBranches";
            this.MulticolorBranches.Size = new System.Drawing.Size(132, 19);
            this.MulticolorBranches.TabIndex = 12;
            this.MulticolorBranches.Text = "Multicolor branches";
            this.MulticolorBranches.UseVisualStyleBackColor = true;
            this.MulticolorBranches.CheckedChanged += new System.EventHandler(this.MulticolorBranches_CheckedChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 204);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(117, 15);
            this.label33.TabIndex = 8;
            this.label33.Text = "Color remote branch";
            // 
            // _NO_TRANSLATE_ColorRemoteBranchLabel
            // 
            this._NO_TRANSLATE_ColorRemoteBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Location = new System.Drawing.Point(287, 204);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Name = "_NO_TRANSLATE_ColorRemoteBranchLabel";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Size = new System.Drawing.Size(29, 17);
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
            this._NO_TRANSLATE_ColorOtherLabel.Location = new System.Drawing.Point(287, 232);
            this._NO_TRANSLATE_ColorOtherLabel.Name = "_NO_TRANSLATE_ColorOtherLabel";
            this._NO_TRANSLATE_ColorOtherLabel.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorOtherLabel.TabIndex = 11;
            this._NO_TRANSLATE_ColorOtherLabel.Text = "Red";
            this._NO_TRANSLATE_ColorOtherLabel.Click += new System.EventHandler(this.ColorOtherLabel_Click);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 232);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(95, 15);
            this.label36.TabIndex = 10;
            this.label36.Text = "Color other label";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 147);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(56, 15);
            this.label25.TabIndex = 4;
            this.label25.Text = "Color tag";
            // 
            // _NO_TRANSLATE_ColorTagLabel
            // 
            this._NO_TRANSLATE_ColorTagLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorTagLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorTagLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorTagLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorTagLabel.Location = new System.Drawing.Point(287, 147);
            this._NO_TRANSLATE_ColorTagLabel.Name = "_NO_TRANSLATE_ColorTagLabel";
            this._NO_TRANSLATE_ColorTagLabel.Size = new System.Drawing.Size(29, 17);
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
            this._NO_TRANSLATE_ColorBranchLabel.Location = new System.Drawing.Point(287, 175);
            this._NO_TRANSLATE_ColorBranchLabel.Name = "_NO_TRANSLATE_ColorBranchLabel";
            this._NO_TRANSLATE_ColorBranchLabel.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorBranchLabel.TabIndex = 7;
            this._NO_TRANSLATE_ColorBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorBranchLabel.Click += new System.EventHandler(this.ColorBranchLabel_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 175);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(76, 15);
            this.label32.TabIndex = 6;
            this.label32.Text = "Color branch";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.diffFontChangeButton);
            this.groupBox3.Controls.Add(this.label43);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorRemovedLineDiffLabel);
            this.groupBox3.Controls.Add(this.label45);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorAddedLineDiffLabel);
            this.groupBox3.Controls.Add(this.label27);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorSectionLabel);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorRemovedLine);
            this.groupBox3.Controls.Add(this.label56);
            this.groupBox3.Controls.Add(this.label31);
            this.groupBox3.Controls.Add(this.label29);
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_ColorAddedLineLabel);
            this.groupBox3.Location = new System.Drawing.Point(8, 288);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(387, 194);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Difference view";
            // 
            // diffFontChangeButton
            // 
            this.diffFontChangeButton.AutoSize = true;
            this.diffFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.diffFontChangeButton.Location = new System.Drawing.Point(132, 164);
            this.diffFontChangeButton.Name = "diffFontChangeButton";
            this.diffFontChangeButton.Size = new System.Drawing.Size(72, 25);
            this.diffFontChangeButton.TabIndex = 14;
            this.diffFontChangeButton.Text = "font name";
            this.diffFontChangeButton.UseVisualStyleBackColor = true;
            this.diffFontChangeButton.Click += new System.EventHandler(this.diffFontChangeButton_Click);
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(6, 79);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(176, 15);
            this.label43.TabIndex = 10;
            this.label43.Text = "Color removed line highlighting";
            // 
            // _NO_TRANSLATE_ColorRemovedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Location = new System.Drawing.Point(284, 79);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Name = "_NO_TRANSLATE_ColorRemovedLineDiffLabel";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.TabIndex = 11;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Click += new System.EventHandler(this.ColorRemovedLineDiffLabel_Click);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(6, 109);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(162, 15);
            this.label45.TabIndex = 12;
            this.label45.Text = "Color added line highlighting";
            // 
            // _NO_TRANSLATE_ColorAddedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Location = new System.Drawing.Point(284, 109);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Name = "_NO_TRANSLATE_ColorAddedLineDiffLabel";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.TabIndex = 13;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Click += new System.EventHandler(this.ColorAddedLineDiffLabel_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 18);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(108, 15);
            this.label27.TabIndex = 4;
            this.label27.Text = "Color removed line";
            // 
            // _NO_TRANSLATE_ColorSectionLabel
            // 
            this._NO_TRANSLATE_ColorSectionLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorSectionLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorSectionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorSectionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorSectionLabel.Location = new System.Drawing.Point(284, 138);
            this._NO_TRANSLATE_ColorSectionLabel.Name = "_NO_TRANSLATE_ColorSectionLabel";
            this._NO_TRANSLATE_ColorSectionLabel.Size = new System.Drawing.Size(29, 17);
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
            this._NO_TRANSLATE_ColorRemovedLine.Location = new System.Drawing.Point(284, 18);
            this._NO_TRANSLATE_ColorRemovedLine.Name = "_NO_TRANSLATE_ColorRemovedLine";
            this._NO_TRANSLATE_ColorRemovedLine.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorRemovedLine.TabIndex = 5;
            this._NO_TRANSLATE_ColorRemovedLine.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLine.Click += new System.EventHandler(this.ColorRemovedLine_Click);
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(6, 169);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(31, 15);
            this.label56.TabIndex = 8;
            this.label56.Text = "Font";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(6, 139);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(77, 15);
            this.label31.TabIndex = 8;
            this.label31.Text = "Color section";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 48);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(94, 15);
            this.label29.TabIndex = 6;
            this.label29.Text = "Color added line";
            // 
            // _NO_TRANSLATE_ColorAddedLineLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAddedLineLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineLabel.Location = new System.Drawing.Point(284, 48);
            this._NO_TRANSLATE_ColorAddedLineLabel.Name = "_NO_TRANSLATE_ColorAddedLineLabel";
            this._NO_TRANSLATE_ColorAddedLineLabel.Size = new System.Drawing.Size(29, 17);
            this._NO_TRANSLATE_ColorAddedLineLabel.TabIndex = 7;
            this._NO_TRANSLATE_ColorAddedLineLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineLabel.Click += new System.EventHandler(this.label28_Click);
            // 
            // tpStart
            // 
            this.tpStart.Controls.Add(this.dashboardEditor1);
            this.tpStart.Location = new System.Drawing.Point(4, 24);
            this.tpStart.Name = "tpStart";
            this.tpStart.Padding = new System.Windows.Forms.Padding(3);
            this.tpStart.Size = new System.Drawing.Size(805, 518);
            this.tpStart.TabIndex = 6;
            this.tpStart.Text = "Start page";
            this.tpStart.UseVisualStyleBackColor = true;
            // 
            // dashboardEditor1
            // 
            this.dashboardEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardEditor1.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.dashboardEditor1.Location = new System.Drawing.Point(3, 3);
            this.dashboardEditor1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dashboardEditor1.Name = "dashboardEditor1";
            this.dashboardEditor1.Size = new System.Drawing.Size(799, 512);
            this.dashboardEditor1.TabIndex = 0;
            // 
            // tpGlobalSettings
            // 
            this.tpGlobalSettings.Controls.Add(this.Global_AppEncoding);
            this.tpGlobalSettings.Controls.Add(this.label59);
            this.tpGlobalSettings.Controls.Add(this.label60);
            this.tpGlobalSettings.Controls.Add(this.Global_FilesEncoding);
            this.tpGlobalSettings.Controls.Add(this.BrowseCommitTemplate);
            this.tpGlobalSettings.Controls.Add(this.label57);
            this.tpGlobalSettings.Controls.Add(this.CommitTemplatePath);
            this.tpGlobalSettings.Controls.Add(this.groupBox9);
            this.tpGlobalSettings.Controls.Add(this.DiffToolCmdSuggest);
            this.tpGlobalSettings.Controls.Add(this.DifftoolCmd);
            this.tpGlobalSettings.Controls.Add(this.label48);
            this.tpGlobalSettings.Controls.Add(this.BrowseDiffTool);
            this.tpGlobalSettings.Controls.Add(this.label42);
            this.tpGlobalSettings.Controls.Add(this.DifftoolPath);
            this.tpGlobalSettings.Controls.Add(this.GlobalDiffTool);
            this.tpGlobalSettings.Controls.Add(this.label41);
            this.tpGlobalSettings.Controls.Add(this.label28);
            this.tpGlobalSettings.Controls.Add(this.InvalidGitPathGlobal);
            this.tpGlobalSettings.Controls.Add(this.MergeToolCmdSuggest);
            this.tpGlobalSettings.Controls.Add(this.MergeToolCmd);
            this.tpGlobalSettings.Controls.Add(this.label19);
            this.tpGlobalSettings.Controls.Add(this.BrowseMergeTool);
            this.tpGlobalSettings.Controls.Add(this.GlobalMergeTool);
            this.tpGlobalSettings.Controls.Add(this.PathToKDiff3);
            this.tpGlobalSettings.Controls.Add(this.MergetoolPath);
            this.tpGlobalSettings.Controls.Add(this.GlobalKeepMergeBackup);
            this.tpGlobalSettings.Controls.Add(this.label7);
            this.tpGlobalSettings.Controls.Add(this.GlobalEditor);
            this.tpGlobalSettings.Controls.Add(this.label6);
            this.tpGlobalSettings.Controls.Add(this.GlobalUserEmail);
            this.tpGlobalSettings.Controls.Add(this.GlobalUserName);
            this.tpGlobalSettings.Controls.Add(this.label4);
            this.tpGlobalSettings.Controls.Add(this.label3);
            this.tpGlobalSettings.Location = new System.Drawing.Point(4, 24);
            this.tpGlobalSettings.Name = "tpGlobalSettings";
            this.tpGlobalSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tpGlobalSettings.Size = new System.Drawing.Size(805, 518);
            this.tpGlobalSettings.TabIndex = 1;
            this.tpGlobalSettings.Text = "Global settings";
            this.tpGlobalSettings.UseVisualStyleBackColor = true;
            // 
            // Global_AppEncoding
            // 
            this.Global_AppEncoding.FormattingEnabled = true;
            this.Global_AppEncoding.Location = new System.Drawing.Point(153, 462);
            this.Global_AppEncoding.Name = "Global_AppEncoding";
            this.helpProvider1.SetShowHelp(this.Global_AppEncoding, true);
            this.Global_AppEncoding.Size = new System.Drawing.Size(250, 23);
            this.Global_AppEncoding.TabIndex = 51;
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Location = new System.Drawing.Point(9, 464);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(130, 15);
            this.label59.TabIndex = 50;
            this.label59.Text = "GitExtensions encoding";
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Location = new System.Drawing.Point(8, 436);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(83, 15);
            this.label60.TabIndex = 49;
            this.label60.Text = "Files encoding";
            // 
            // Global_FilesEncoding
            // 
            this.Global_FilesEncoding.FormattingEnabled = true;
            this.Global_FilesEncoding.Location = new System.Drawing.Point(153, 434);
            this.Global_FilesEncoding.Name = "Global_FilesEncoding";
            this.Global_FilesEncoding.Size = new System.Drawing.Size(250, 23);
            this.Global_FilesEncoding.TabIndex = 48;
            // 
            // BrowseCommitTemplate
            // 
            this.BrowseCommitTemplate.Location = new System.Drawing.Point(506, 285);
            this.BrowseCommitTemplate.Name = "BrowseCommitTemplate";
            this.BrowseCommitTemplate.Size = new System.Drawing.Size(108, 25);
            this.BrowseCommitTemplate.TabIndex = 34;
            this.BrowseCommitTemplate.Text = "Browse";
            this.BrowseCommitTemplate.UseVisualStyleBackColor = true;
            this.BrowseCommitTemplate.Click += new System.EventHandler(this.BrowseCommitTemplate_Click);
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(9, 290);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(140, 15);
            this.label57.TabIndex = 33;
            this.label57.Text = "Path to commit template";
            // 
            // CommitTemplatePath
            // 
            this.CommitTemplatePath.Location = new System.Drawing.Point(153, 286);
            this.CommitTemplatePath.Name = "CommitTemplatePath";
            this.CommitTemplatePath.Size = new System.Drawing.Size(347, 23);
            this.CommitTemplatePath.TabIndex = 32;
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox9.Controls.Add(this.globalAutoCrlfFalse);
            this.groupBox9.Controls.Add(this.globalAutoCrlfInput);
            this.groupBox9.Controls.Add(this.globalAutoCrlfTrue);
            this.groupBox9.Location = new System.Drawing.Point(6, 320);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(780, 105);
            this.groupBox9.TabIndex = 31;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Line endings";
            // 
            // globalAutoCrlfFalse
            // 
            this.globalAutoCrlfFalse.AutoSize = true;
            this.globalAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.globalAutoCrlfFalse.Name = "globalAutoCrlfFalse";
            this.globalAutoCrlfFalse.Size = new System.Drawing.Size(349, 19);
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
            this.globalAutoCrlfInput.Size = new System.Drawing.Size(448, 19);
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
            this.globalAutoCrlfTrue.Size = new System.Drawing.Size(495, 19);
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
            this.DifftoolCmd.Location = new System.Drawing.Point(153, 259);
            this.DifftoolCmd.Name = "DifftoolCmd";
            this.DifftoolCmd.Size = new System.Drawing.Size(347, 23);
            this.DifftoolCmd.TabIndex = 29;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(9, 263);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(105, 15);
            this.label48.TabIndex = 28;
            this.label48.Text = "Difftool command";
            // 
            // BrowseDiffTool
            // 
            this.BrowseDiffTool.Location = new System.Drawing.Point(506, 229);
            this.BrowseDiffTool.Name = "BrowseDiffTool";
            this.BrowseDiffTool.Size = new System.Drawing.Size(108, 25);
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
            this.label42.Size = new System.Drawing.Size(87, 15);
            this.label42.TabIndex = 26;
            this.label42.Text = "Path to difftool";
            // 
            // DifftoolPath
            // 
            this.DifftoolPath.Location = new System.Drawing.Point(153, 232);
            this.DifftoolPath.Name = "DifftoolPath";
            this.DifftoolPath.Size = new System.Drawing.Size(347, 23);
            this.DifftoolPath.TabIndex = 25;
            // 
            // GlobalDiffTool
            // 
            this.GlobalDiffTool.FormattingEnabled = true;
            this.GlobalDiffTool.Items.AddRange(new object[] {
            "araxis",
            "beyondcompare3",
            "diffuse",
            "ecmerge",
            "emerge",
            "gvimdiff",
            "kdiff3",
            "kompare",
            "meld",
            "opendiff",
            "tkdiff",
            "tmerge",
            "vimdiff",
            "winmerge",
            "xxdiff"});
            this.GlobalDiffTool.Location = new System.Drawing.Point(153, 205);
            this.GlobalDiffTool.Name = "GlobalDiffTool";
            this.GlobalDiffTool.Size = new System.Drawing.Size(164, 23);
            this.GlobalDiffTool.TabIndex = 24;
            this.GlobalDiffTool.TextChanged += new System.EventHandler(this.GlobalDiffTool_TextChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(10, 208);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(47, 15);
            this.label41.TabIndex = 23;
            this.label41.Text = "Difftool";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(10, 179);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(174, 15);
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
            this.label9.Size = new System.Drawing.Size(193, 45);
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
            // MergeToolCmdSuggest
            // 
            this.MergeToolCmdSuggest.Location = new System.Drawing.Point(506, 145);
            this.MergeToolCmdSuggest.Name = "MergeToolCmdSuggest";
            this.MergeToolCmdSuggest.Size = new System.Drawing.Size(108, 25);
            this.MergeToolCmdSuggest.TabIndex = 16;
            this.MergeToolCmdSuggest.Text = "Suggest command";
            this.MergeToolCmdSuggest.UseVisualStyleBackColor = true;
            this.MergeToolCmdSuggest.Click += new System.EventHandler(this.MergeToolCmdSuggest_Click);
            // 
            // MergeToolCmd
            // 
            this.MergeToolCmd.FormattingEnabled = true;
            this.MergeToolCmd.Location = new System.Drawing.Point(153, 147);
            this.MergeToolCmd.Name = "MergeToolCmd";
            this.MergeToolCmd.Size = new System.Drawing.Size(347, 23);
            this.MergeToolCmd.TabIndex = 15;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(9, 151);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(120, 15);
            this.label19.TabIndex = 14;
            this.label19.Text = "Mergetool command";
            // 
            // BrowseMergeTool
            // 
            this.BrowseMergeTool.Location = new System.Drawing.Point(506, 117);
            this.BrowseMergeTool.Name = "BrowseMergeTool";
            this.BrowseMergeTool.Size = new System.Drawing.Size(108, 25);
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
            this.GlobalMergeTool.Size = new System.Drawing.Size(164, 23);
            this.GlobalMergeTool.TabIndex = 12;
            this.GlobalMergeTool.TextChanged += new System.EventHandler(this.GlobalMergeTool_TextChanged);
            // 
            // PathToKDiff3
            // 
            this.PathToKDiff3.AutoSize = true;
            this.PathToKDiff3.Location = new System.Drawing.Point(9, 124);
            this.PathToKDiff3.Name = "PathToKDiff3";
            this.PathToKDiff3.Size = new System.Drawing.Size(103, 15);
            this.PathToKDiff3.TabIndex = 11;
            this.PathToKDiff3.Text = "Path to mergetool";
            // 
            // MergetoolPath
            // 
            this.MergetoolPath.Location = new System.Drawing.Point(153, 120);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(347, 23);
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
            this.label7.Size = new System.Drawing.Size(62, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "Mergetool";
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalEditor.Location = new System.Drawing.Point(153, 65);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(629, 23);
            this.GlobalEditor.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 15);
            this.label6.TabIndex = 4;
            this.label6.Text = "Editor";
            // 
            // GlobalUserEmail
            // 
            this.GlobalUserEmail.Location = new System.Drawing.Point(153, 37);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(236, 23);
            this.GlobalUserEmail.TabIndex = 3;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(153, 8);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(236, 23);
            this.GlobalUserName.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "User email";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "User name";
            // 
            // tpSsh
            // 
            this.tpSsh.Controls.Add(this.groupBox2);
            this.tpSsh.Controls.Add(this.groupBox1);
            this.tpSsh.Location = new System.Drawing.Point(4, 24);
            this.tpSsh.Name = "tpSsh";
            this.tpSsh.Padding = new System.Windows.Forms.Padding(3);
            this.tpSsh.Size = new System.Drawing.Size(805, 518);
            this.tpSsh.TabIndex = 4;
            this.tpSsh.Text = "Ssh";
            this.tpSsh.UseVisualStyleBackColor = true;
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
            this.groupBox2.Size = new System.Drawing.Size(789, 154);
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
            this.PageantPath.Size = new System.Drawing.Size(556, 23);
            this.PageantPath.TabIndex = 9;
            // 
            // PageantBrowse
            // 
            this.PageantBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PageantBrowse.Location = new System.Drawing.Point(705, 74);
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
            this.label17.Size = new System.Drawing.Size(91, 15);
            this.label17.TabIndex = 8;
            this.label17.Text = "Path to pageant";
            // 
            // PuttygenPath
            // 
            this.PuttygenPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenPath.Location = new System.Drawing.Point(143, 46);
            this.PuttygenPath.Name = "PuttygenPath";
            this.PuttygenPath.Size = new System.Drawing.Size(556, 23);
            this.PuttygenPath.TabIndex = 6;
            // 
            // PuttygenBrowse
            // 
            this.PuttygenBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenBrowse.Location = new System.Drawing.Point(705, 44);
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
            this.label16.Size = new System.Drawing.Size(96, 15);
            this.label16.TabIndex = 5;
            this.label16.Text = "Path to puttygen";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 20);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(94, 15);
            this.label15.TabIndex = 4;
            this.label15.Text = "Path to plink.exe";
            // 
            // PlinkPath
            // 
            this.PlinkPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkPath.Location = new System.Drawing.Point(143, 17);
            this.PlinkPath.Name = "PlinkPath";
            this.PlinkPath.Size = new System.Drawing.Size(556, 23);
            this.PlinkPath.TabIndex = 2;
            // 
            // PlinkBrowse
            // 
            this.PlinkBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkBrowse.Location = new System.Drawing.Point(705, 14);
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
            this.groupBox1.Size = new System.Drawing.Size(789, 121);
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
            this.OtherSsh.Size = new System.Drawing.Size(556, 23);
            this.OtherSsh.TabIndex = 4;
            // 
            // OtherSshBrowse
            // 
            this.OtherSshBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherSshBrowse.Location = new System.Drawing.Point(705, 77);
            this.OtherSshBrowse.Name = "OtherSshBrowse";
            this.OtherSshBrowse.Size = new System.Drawing.Size(75, 25);
            this.OtherSshBrowse.TabIndex = 5;
            this.OtherSshBrowse.Text = "Browse";
            this.OtherSshBrowse.UseVisualStyleBackColor = true;
            this.OtherSshBrowse.Click += new System.EventHandler(this.OtherSshBrowse_Click);
            // 
            // Other
            // 
            this.Other.AutoSize = true;
            this.Other.Location = new System.Drawing.Point(9, 81);
            this.Other.Name = "Other";
            this.Other.Size = new System.Drawing.Size(107, 19);
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
            this.label18.Size = new System.Drawing.Size(467, 47);
            this.label18.TabIndex = 2;
            this.label18.Text = resources.GetString("label18.Text");
            // 
            // OpenSSH
            // 
            this.OpenSSH.AutoSize = true;
            this.OpenSSH.Location = new System.Drawing.Point(9, 50);
            this.OpenSSH.Name = "OpenSSH";
            this.OpenSSH.Size = new System.Drawing.Size(75, 19);
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
            this.Putty.Size = new System.Drawing.Size(60, 19);
            this.Putty.TabIndex = 0;
            this.Putty.TabStop = true;
            this.Putty.Text = "PuTTY";
            this.Putty.UseVisualStyleBackColor = true;
            this.Putty.CheckedChanged += new System.EventHandler(this.Putty_CheckedChanged);
            // 
            // tpScriptsTab
            // 
            this.tpScriptsTab.Controls.Add(this.lbl_icon);
            this.tpScriptsTab.Controls.Add(this.sbtn_icon);
            this.tpScriptsTab.Controls.Add(this.scriptNeedsConfirmation);
            this.tpScriptsTab.Controls.Add(this.labelOnEvent);
            this.tpScriptsTab.Controls.Add(this.scriptEvent);
            this.tpScriptsTab.Controls.Add(this.scriptEnabled);
            this.tpScriptsTab.Controls.Add(this.ScriptList);
            this.tpScriptsTab.Controls.Add(this.helpLabel);
            this.tpScriptsTab.Controls.Add(this.inMenuCheckBox);
            this.tpScriptsTab.Controls.Add(this.argumentsLabel);
            this.tpScriptsTab.Controls.Add(this.commandLabel);
            this.tpScriptsTab.Controls.Add(this.nameLabel);
            this.tpScriptsTab.Controls.Add(this.browseScriptButton);
            this.tpScriptsTab.Controls.Add(this.argumentsTextBox);
            this.tpScriptsTab.Controls.Add(this.commandTextBox);
            this.tpScriptsTab.Controls.Add(this.nameTextBox);
            this.tpScriptsTab.Controls.Add(this.moveDownButton);
            this.tpScriptsTab.Controls.Add(this.removeScriptButton);
            this.tpScriptsTab.Controls.Add(this.addScriptButton);
            this.tpScriptsTab.Controls.Add(this.moveUpButton);
            this.tpScriptsTab.Location = new System.Drawing.Point(4, 24);
            this.tpScriptsTab.Name = "tpScriptsTab";
            this.tpScriptsTab.Padding = new System.Windows.Forms.Padding(3);
            this.tpScriptsTab.Size = new System.Drawing.Size(805, 518);
            this.tpScriptsTab.TabIndex = 8;
            this.tpScriptsTab.Text = "Scripts";
            this.tpScriptsTab.UseVisualStyleBackColor = true;
            // 
            // lbl_icon
            // 
            this.lbl_icon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_icon.AutoSize = true;
            this.lbl_icon.Location = new System.Drawing.Point(324, 418);
            this.lbl_icon.Name = "lbl_icon";
            this.lbl_icon.Size = new System.Drawing.Size(33, 15);
            this.lbl_icon.TabIndex = 23;
            this.lbl_icon.Text = "Icon:";
            this.lbl_icon.Visible = false;
            // 
            // sbtn_icon
            // 
            this.sbtn_icon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.sbtn_icon.AutoSize = true;
            this.sbtn_icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.sbtn_icon.ContextMenuStrip = this.contextMenuStrip_SplitButton;
            this.sbtn_icon.Location = new System.Drawing.Point(370, 413);
            this.sbtn_icon.Name = "sbtn_icon";
            this.sbtn_icon.Size = new System.Drawing.Size(109, 30);
            this.sbtn_icon.SplitMenuStrip = this.contextMenuStrip_SplitButton;
            this.sbtn_icon.TabIndex = 22;
            this.sbtn_icon.Text = "Select icon";
            this.sbtn_icon.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sbtn_icon.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.sbtn_icon.UseVisualStyleBackColor = true;
            this.sbtn_icon.Visible = false;
            this.sbtn_icon.WholeButtonDropdown = true;
            // 
            // contextMenuStrip_SplitButton
            // 
            this.contextMenuStrip_SplitButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.contextMenuStrip_SplitButton.Name = "contextMenuStrip1";
            this.contextMenuStrip_SplitButton.Size = new System.Drawing.Size(68, 70);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(67, 22);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(67, 22);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(67, 22);
            // 
            // scriptNeedsConfirmation
            // 
            this.scriptNeedsConfirmation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.scriptNeedsConfirmation.AutoSize = true;
            this.scriptNeedsConfirmation.Location = new System.Drawing.Point(107, 446);
            this.scriptNeedsConfirmation.Name = "scriptNeedsConfirmation";
            this.scriptNeedsConfirmation.Size = new System.Drawing.Size(135, 19);
            this.scriptNeedsConfirmation.TabIndex = 21;
            this.scriptNeedsConfirmation.Text = "Ask for confirmation";
            this.scriptNeedsConfirmation.UseVisualStyleBackColor = true;
            this.scriptNeedsConfirmation.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // labelOnEvent
            // 
            this.labelOnEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelOnEvent.AutoSize = true;
            this.labelOnEvent.Location = new System.Drawing.Point(8, 420);
            this.labelOnEvent.Name = "labelOnEvent";
            this.labelOnEvent.Size = new System.Drawing.Size(58, 15);
            this.labelOnEvent.TabIndex = 20;
            this.labelOnEvent.Text = "On event:";
            // 
            // scriptEvent
            // 
            this.scriptEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.scriptEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scriptEvent.FormattingEnabled = true;
            this.scriptEvent.Location = new System.Drawing.Point(107, 414);
            this.scriptEvent.Name = "scriptEvent";
            this.scriptEvent.Size = new System.Drawing.Size(188, 23);
            this.scriptEvent.TabIndex = 19;
            this.scriptEvent.SelectedIndexChanged += new System.EventHandler(this.scriptEvent_SelectedIndexChanged);
            this.scriptEvent.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // scriptEnabled
            // 
            this.scriptEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptEnabled.AutoSize = true;
            this.scriptEnabled.Location = new System.Drawing.Point(437, 246);
            this.scriptEnabled.Name = "scriptEnabled";
            this.scriptEnabled.Size = new System.Drawing.Size(68, 19);
            this.scriptEnabled.TabIndex = 18;
            this.scriptEnabled.Text = "Enabled";
            this.scriptEnabled.UseVisualStyleBackColor = true;
            this.scriptEnabled.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // ScriptList
            // 
            this.ScriptList.AllowUserToAddRows = false;
            this.ScriptList.AllowUserToDeleteRows = false;
            this.ScriptList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ScriptList.AutoGenerateColumns = false;
            this.ScriptList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ScriptList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.HotkeyCommandIdentifier,
            this.EnabledColumn,
            this.nameDataGridViewTextBoxColumn,
            this.OnEvent,
            this.AskConfirmation,
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn});
            this.ScriptList.DataSource = this.scriptInfoBindingSource;
            this.ScriptList.GridColor = System.Drawing.SystemColors.ActiveBorder;
            this.ScriptList.Location = new System.Drawing.Point(3, 3);
            this.ScriptList.Name = "ScriptList";
            this.ScriptList.ReadOnly = true;
            this.ScriptList.RowHeadersVisible = false;
            this.ScriptList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ScriptList.Size = new System.Drawing.Size(705, 232);
            this.ScriptList.TabIndex = 17;
            this.ScriptList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ScriptList_CellClick);
            this.ScriptList.SelectionChanged += new System.EventHandler(this.ScriptList_SelectionChanged);
            // 
            // HotkeyCommandIdentifier
            // 
            this.HotkeyCommandIdentifier.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.HotkeyCommandIdentifier.DataPropertyName = "HotkeyCommandIdentifier";
            this.HotkeyCommandIdentifier.HeaderText = "#";
            this.HotkeyCommandIdentifier.Name = "HotkeyCommandIdentifier";
            this.HotkeyCommandIdentifier.ReadOnly = true;
            this.HotkeyCommandIdentifier.Width = 39;
            // 
            // EnabledColumn
            // 
            this.EnabledColumn.DataPropertyName = "Enabled";
            this.EnabledColumn.HeaderText = "Enabled";
            this.EnabledColumn.Name = "EnabledColumn";
            this.EnabledColumn.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // OnEvent
            // 
            this.OnEvent.DataPropertyName = "OnEvent";
            this.OnEvent.HeaderText = "OnEvent";
            this.OnEvent.Name = "OnEvent";
            this.OnEvent.ReadOnly = true;
            // 
            // AskConfirmation
            // 
            this.AskConfirmation.DataPropertyName = "AskConfirmation";
            this.AskConfirmation.HeaderText = "Confirmation";
            this.AskConfirmation.Name = "AskConfirmation";
            this.AskConfirmation.ReadOnly = true;
            // 
            // addToRevisionGridContextMenuDataGridViewCheckBoxColumn
            // 
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn.DataPropertyName = "AddToRevisionGridContextMenu";
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn.HeaderText = "Context menu";
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn.Name = "addToRevisionGridContextMenuDataGridViewCheckBoxColumn";
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // scriptInfoBindingSource
            // 
            this.scriptInfoBindingSource.DataSource = typeof(GitUI.Script.ScriptInfo);
            // 
            // helpLabel
            // 
            this.helpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.helpLabel.AutoSize = true;
            this.helpLabel.BackColor = System.Drawing.SystemColors.Info;
            this.helpLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.helpLabel.Location = new System.Drawing.Point(542, 417);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(177, 17);
            this.helpLabel.TabIndex = 16;
            this.helpLabel.Text = "Press F1 to see available options";
            this.helpLabel.Visible = false;
            // 
            // inMenuCheckBox
            // 
            this.inMenuCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.inMenuCheckBox.AutoSize = true;
            this.inMenuCheckBox.Location = new System.Drawing.Point(107, 469);
            this.inMenuCheckBox.Name = "inMenuCheckBox";
            this.inMenuCheckBox.Size = new System.Drawing.Size(206, 19);
            this.inMenuCheckBox.TabIndex = 15;
            this.inMenuCheckBox.Text = "Add to revision grid context menu";
            this.inMenuCheckBox.UseVisualStyleBackColor = true;
            this.inMenuCheckBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // argumentsLabel
            // 
            this.argumentsLabel.AutoSize = true;
            this.argumentsLabel.Location = new System.Drawing.Point(8, 302);
            this.argumentsLabel.Name = "argumentsLabel";
            this.argumentsLabel.Size = new System.Drawing.Size(69, 15);
            this.argumentsLabel.TabIndex = 14;
            this.argumentsLabel.Text = "Arguments:";
            // 
            // commandLabel
            // 
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(8, 274);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(67, 15);
            this.commandLabel.TabIndex = 13;
            this.commandLabel.Text = "Command:";
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(8, 246);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(42, 15);
            this.nameLabel.TabIndex = 12;
            this.nameLabel.Text = "Name:";
            // 
            // browseScriptButton
            // 
            this.browseScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseScriptButton.Location = new System.Drawing.Point(629, 269);
            this.browseScriptButton.Name = "browseScriptButton";
            this.browseScriptButton.Size = new System.Drawing.Size(75, 25);
            this.browseScriptButton.TabIndex = 11;
            this.browseScriptButton.Text = "Browse";
            this.browseScriptButton.UseVisualStyleBackColor = true;
            this.browseScriptButton.Click += new System.EventHandler(this.browseScriptButton_Click);
            // 
            // argumentsTextBox
            // 
            this.argumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProvider1.SetHelpString(this.argumentsTextBox, resources.GetString("argumentsTextBox.HelpString"));
            this.argumentsTextBox.Location = new System.Drawing.Point(107, 299);
            this.argumentsTextBox.Name = "argumentsTextBox";
            this.helpProvider1.SetShowHelp(this.argumentsTextBox, true);
            this.argumentsTextBox.Size = new System.Drawing.Size(601, 109);
            this.argumentsTextBox.TabIndex = 8;
            this.argumentsTextBox.Text = "";
            this.argumentsTextBox.Enter += new System.EventHandler(this.argumentsTextBox_Enter);
            this.argumentsTextBox.Leave += new System.EventHandler(this.argumentsTextBox_Leave);
            this.argumentsTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // commandTextBox
            // 
            this.commandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandTextBox.Location = new System.Drawing.Point(107, 271);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(510, 23);
            this.commandTextBox.TabIndex = 7;
            this.commandTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // nameTextBox
            // 
            this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameTextBox.Location = new System.Drawing.Point(107, 243);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(229, 23);
            this.nameTextBox.TabIndex = 6;
            this.nameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // moveDownButton
            // 
            this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Image = global::GitUI.Properties.Resources.ArrowDown;
            this.moveDownButton.Location = new System.Drawing.Point(739, 152);
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
            this.removeScriptButton.Location = new System.Drawing.Point(717, 121);
            this.removeScriptButton.Name = "removeScriptButton";
            this.removeScriptButton.Size = new System.Drawing.Size(75, 25);
            this.removeScriptButton.TabIndex = 4;
            this.removeScriptButton.Text = "Remove";
            this.removeScriptButton.UseVisualStyleBackColor = true;
            this.removeScriptButton.Click += new System.EventHandler(this.removeScriptButton_Click);
            // 
            // addScriptButton
            // 
            this.addScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addScriptButton.Location = new System.Drawing.Point(717, 90);
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
            this.moveUpButton.Image = global::GitUI.Properties.Resources.ArrowUp;
            this.moveUpButton.Location = new System.Drawing.Point(739, 61);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(26, 23);
            this.moveUpButton.TabIndex = 1;
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // tpHotkeys
            // 
            this.tpHotkeys.Controls.Add(this.controlHotkeys);
            this.tpHotkeys.Location = new System.Drawing.Point(4, 24);
            this.tpHotkeys.Name = "tpHotkeys";
            this.tpHotkeys.Padding = new System.Windows.Forms.Padding(3);
            this.tpHotkeys.Size = new System.Drawing.Size(805, 518);
            this.tpHotkeys.TabIndex = 9;
            this.tpHotkeys.Text = "Hotkeys";
            this.tpHotkeys.UseVisualStyleBackColor = true;
            // 
            // controlHotkeys
            // 
            this.controlHotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlHotkeys.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.controlHotkeys.Location = new System.Drawing.Point(3, 3);
            this.controlHotkeys.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.controlHotkeys.Name = "controlHotkeys";
            this.controlHotkeys.Size = new System.Drawing.Size(799, 512);
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
            this.splitContainer1.Size = new System.Drawing.Size(813, 579);
            this.splitContainer1.SplitterDistance = 546;
            this.splitContainer1.TabIndex = 1;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(721, 2);
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
            // diffFontDialog
            // 
            this.diffFontDialog.AllowVerticalFonts = false;
            this.diffFontDialog.FixedPitchOnly = true;
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 579);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 600);
            this.Name = "FormSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.Shown += new System.EventHandler(this.FormSettings_Shown);
            this.tpLocalSettings.ResumeLayout(false);
            this.tpLocalSettings.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.InvalidGitPathLocal.ResumeLayout(false);
            this.InvalidGitPathLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpChecklist.ResumeLayout(false);
            this.tpChecklist.PerformLayout();
            this.tpGit.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tpGitExtensions.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionGridQuickSearchTimeout)).EndInit();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_MaxCommits)).EndInit();
            this.tpAppearance.ResumeLayout(false);
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_authorImageSize)).EndInit();
            this.tpColors.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreviewSmall)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tpStart.ResumeLayout(false);
            this.tpGlobalSettings.ResumeLayout(false);
            this.tpGlobalSettings.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.InvalidGitPathGlobal.ResumeLayout(false);
            this.InvalidGitPathGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tpSsh.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tpScriptsTab.ResumeLayout(false);
            this.tpScriptsTab.PerformLayout();
            this.contextMenuStrip_SplitButton.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ScriptList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).EndInit();
            this.tpHotkeys.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tpLocalSettings;
        private System.Windows.Forms.TextBox UserEmail;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TabPage tpGlobalSettings;
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
        private System.Windows.Forms.TabPage tpChecklist;
        private System.Windows.Forms.Button GitExtensionsInstall;
        private System.Windows.Forms.Button ShellExtensionsRegistered;
        private System.Windows.Forms.Button UserNameSet;
        private System.Windows.Forms.Button MergeTool;
        private System.Windows.Forms.Button GitFound;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox CheckAtStartup;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.TabPage tpGitExtensions;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_MaxCommits;
        private System.Windows.Forms.Button BrowseGitPath;
        private System.Windows.Forms.TextBox GitPath;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button BrowseGitBinPath;
        private System.Windows.Forms.TextBox GitBinPath;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button GitBinFound;
        private System.Windows.Forms.TabPage tpSsh;
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
        private System.Windows.Forms.ComboBox LocalMergeTool;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button BrowseMergeTool;
        private System.Windows.Forms.ComboBox MergeToolCmd;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button MergeToolCmdSuggest;
        private System.Windows.Forms.CheckBox chkCloseProcessDialog;
        private System.Windows.Forms.CheckBox chkShowGitCommandLine;
        private System.Windows.Forms.CheckBox chkUseFastChecks;
        private System.Windows.Forms.Label NoGitRepo;
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
        private System.Windows.Forms.TabPage tpColors;
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
        private System.Windows.Forms.CheckBox chkFollowRenamesInFileHistory;
        private System.Windows.Forms.ComboBox GlobalDiffTool;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Button BrowseDiffTool;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox DifftoolPath;
        private System.Windows.Forms.Button DiffTool;
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
        private System.Windows.Forms.TabPage tpStart;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private DashboardEditor dashboardEditor1;
        private System.Windows.Forms.Button DiffToolCmdSuggest;
        private System.Windows.Forms.ComboBox DifftoolCmd;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.ComboBox Language;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.LinkLabel helpTranslate;
        private System.Windows.Forms.TabPage tpGit;
        private System.Windows.Forms.CheckBox MulticolorBranches;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.CheckBox BranchBorders;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorGraphLabel;
        private System.Windows.Forms.CheckBox StripedBanchChange;
        private System.Windows.Forms.CheckBox chkShowGitStatusInToolbar;
        private System.Windows.Forms.NumericUpDown RevisionGridQuickSearchTimeout;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.CheckBox chkShowErrorsWhenStagingFiles;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.RadioButton globalAutoCrlfFalse;
        private System.Windows.Forms.RadioButton globalAutoCrlfInput;
        private System.Windows.Forms.RadioButton globalAutoCrlfTrue;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton localAutoCrlfFalse;
        private System.Windows.Forms.RadioButton localAutoCrlfInput;
        private System.Windows.Forms.RadioButton localAutoCrlfTrue;
        private System.Windows.Forms.CheckBox DrawNonRelativesGray;
        private System.Windows.Forms.CheckBox chkShowCurrentChangesInRevisionGraph;
        private System.Windows.Forms.CheckBox chkShowStashCountInBrowseWindow;
        private System.Windows.Forms.TabPage tpScriptsTab;
        private System.Windows.Forms.CheckBox inMenuCheckBox;
        private System.Windows.Forms.Label argumentsLabel;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Button browseScriptButton;
        private System.Windows.Forms.RichTextBox argumentsTextBox;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button removeScriptButton;
        private System.Windows.Forms.Button addScriptButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.Button translationConfig;
        private System.Windows.Forms.CheckBox DrawNonRelativesTextGray;
        private System.Windows.Forms.Button translationConfig_Fix;
        private System.Windows.Forms.Button SshConfig_Fix;
        private System.Windows.Forms.Button GitExtensionsInstall_Fix;
        private System.Windows.Forms.Button GitBinFound_Fix;
        private System.Windows.Forms.Button ShellExtensionsRegistered_Fix;
        private System.Windows.Forms.Button DiffTool_Fix;
        private System.Windows.Forms.Button MergeTool_Fix;
        private System.Windows.Forms.Button UserNameSet_Fix;
        private System.Windows.Forms.Button GitFound_Fix;
        private System.Windows.Forms.TabPage tpHotkeys;
        private Hotkey.ControlHotkeys controlHotkeys;
        private DataGridView ScriptList;
        private BindingSource scriptInfoBindingSource;
        private CheckBox scriptEnabled;
        private ComboBox scriptEvent;
        private Label labelOnEvent;
        private CheckBox scriptNeedsConfirmation;
        private CheckBox chkUsePatienceDiffAlgorithm;
        private RadioButton LightblueIcon;
        private ComboBox IconStyle;
        private Label label54;
        private PictureBox IconPreview;
        private PictureBox IconPreviewSmall;
        private Label label55;
        private Button diffFontChangeButton;
        private Label label56;
        private FontDialog diffFontDialog;
        private Label lbl_icon;
        private GitUI.Script.SplitButton sbtn_icon;
        private ContextMenuStrip contextMenuStrip_SplitButton;
        private Button BrowseCommitTemplate;
        private Label label57;
        private TextBox CommitTemplatePath;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private LinkLabel downloadMsysgit;
        private Button ChangeHomeButton;
        private Label homeIsSetToLabel;
        private DataGridViewTextBoxColumn HotkeyCommandIdentifier;
        private DataGridViewCheckBoxColumn EnabledColumn;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn OnEvent;
        private DataGridViewCheckBoxColumn AskConfirmation;
        private DataGridViewCheckBoxColumn addToRevisionGridContextMenuDataGridViewCheckBoxColumn;
        private CheckBox chkWarnBeforeCheckout;
        private CheckBox chkStashUntrackedFiles;
        private ComboBox Global_AppEncoding;
        private Label label59;
        private Label label60;
        private ComboBox Global_FilesEncoding;
        private ComboBox Local_AppEncoding;
        private Label LogEncodingLabel;
        private Label label61;
        private ComboBox Local_FilesEncoding;
        private CheckBox chkStartWithRecentWorkingDir;
        private GroupBox groupBox12;
        private GroupBox groupBox11;
        private TabPage tpAppearance;
        private GroupBox groupBox13;
        private Label truncatePathMethod;
        private ComboBox _NO_TRANSLATE_truncatePathMethod;
        private CheckBox chkShowCurrentBranchInVisualStudio;
        private CheckBox chkShowRelativeDate;
        private LinkLabel downloadDictionary;
        private GroupBox groupBox14;
        private GroupBox groupBox6;
        private ComboBox noImageService;
        private Label label53;
        private Label label47;
        private NumericUpDown _NO_TRANSLATE_DaysToCacheImages;
        private Label label46;
        private Label label44;
        private NumericUpDown _NO_TRANSLATE_authorImageSize;
        private Button ClearImageCache;
        private CheckBox ShowAuthorGravatar;

    }
}
