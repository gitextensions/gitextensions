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
            this.contextMenuStrip_SplitButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.directorySearcher2 = new System.DirectoryServices.DirectorySearcher();
            this.label10 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.argumentsTextBox = new System.Windows.Forms.RichTextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
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
            this.tpStart = new System.Windows.Forms.TabPage();
            this.dashboardEditor1 = new GitUI.DashboardEditor();
            this.tpGlobalSettings = new System.Windows.Forms.TabPage();
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
            this.tpLocalSettings = new System.Windows.Forms.TabPage();
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
            this.tpScriptsTab = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ScriptList = new System.Windows.Forms.DataGridView();
            this.HotkeyCommandIdentifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EnabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OnEvent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AskConfirmation = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.addToRevisionGridContextMenuDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.scriptInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.addScriptButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.removeScriptButton = new System.Windows.Forms.Button();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.scriptEvent = new System.Windows.Forms.ComboBox();
            this.lbl_icon = new System.Windows.Forms.Label();
            this.sbtn_icon = new GitUI.Script.SplitButton();
            this.helpLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.scriptEnabled = new System.Windows.Forms.CheckBox();
            this.commandLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.browseScriptButton = new System.Windows.Forms.Button();
            this.argumentsLabel = new System.Windows.Forms.Label();
            this.labelOnEvent = new System.Windows.Forms.Label();
            this.scriptNeedsConfirmation = new System.Windows.Forms.CheckBox();
            this.inMenuCheckBox = new System.Windows.Forms.CheckBox();
            this.tpHotkeys = new System.Windows.Forms.TabPage();
            this.controlHotkeys = new GitUI.Hotkey.ControlHotkeys();
            this.tpShellExt = new System.Windows.Forms.TabPage();
            this.lblMenuEntries = new System.Windows.Forms.Label();
            this.chlMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.chkCascadedContextMenu = new System.Windows.Forms.CheckBox();
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.settingsTreeViewUserControl1 = new GitUI.SettingsDialog.SettingsTreeViewUserControl();
            this.labelSettingsPageTitle = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.tabControl1.SuspendLayout();
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
            this.tpLocalSettings.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.InvalidGitPathLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.tpScriptsTab.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScriptList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).BeginInit();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tpHotkeys.SuspendLayout();
            this.tpShellExt.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip_SplitButton
            // 
            this.contextMenuStrip_SplitButton.Name = "contextMenuStrip1";
            this.contextMenuStrip_SplitButton.Size = new System.Drawing.Size(61, 4);
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
            // argumentsTextBox
            // 
            this.argumentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpProvider1.SetHelpString(this.argumentsTextBox, resources.GetString("argumentsTextBox.HelpString"));
            this.argumentsTextBox.Location = new System.Drawing.Point(78, 83);
            this.argumentsTextBox.Name = "argumentsTextBox";
            this.helpProvider1.SetShowHelp(this.argumentsTextBox, true);
            this.argumentsTextBox.Size = new System.Drawing.Size(627, 156);
            this.argumentsTextBox.TabIndex = 8;
            this.argumentsTextBox.Text = "";
            this.argumentsTextBox.Enter += new System.EventHandler(this.argumentsTextBox_Enter);
            this.argumentsTextBox.Leave += new System.EventHandler(this.argumentsTextBox_Leave);
            this.argumentsTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpColors);
            this.tabControl1.Controls.Add(this.tpStart);
            this.tabControl1.Controls.Add(this.tpGlobalSettings);
            this.tabControl1.Controls.Add(this.tpLocalSettings);
            this.tabControl1.Controls.Add(this.tpScriptsTab);
            this.tabControl1.Controls.Add(this.tpHotkeys);
            this.tabControl1.Controls.Add(this.tpShellExt);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(203, 38);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1478, 549);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpColors
            // 
            this.tpColors.Controls.Add(this.groupBox5);
            this.tpColors.Controls.Add(this.groupBox4);
            this.tpColors.Controls.Add(this.groupBox3);
            this.tpColors.Location = new System.Drawing.Point(4, 24);
            this.tpColors.Name = "tpColors";
            this.tpColors.Size = new System.Drawing.Size(1470, 521);
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
            this.groupBox5.Size = new System.Drawing.Size(321, 279);
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
            this.LightblueIcon.Size = new System.Drawing.Size(71, 17);
            this.LightblueIcon.TabIndex = 7;
            this.LightblueIcon.TabStop = true;
            this.LightblueIcon.Text = "Light blue";
            this.LightblueIcon.UseVisualStyleBackColor = true;
            this.LightblueIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // RandomIcon
            // 
            this.RandomIcon.AutoSize = true;
            this.RandomIcon.Location = new System.Drawing.Point(111, 250);
            this.RandomIcon.Name = "RandomIcon";
            this.RandomIcon.Size = new System.Drawing.Size(65, 17);
            this.RandomIcon.TabIndex = 6;
            this.RandomIcon.TabStop = true;
            this.RandomIcon.Text = "Random";
            this.RandomIcon.UseVisualStyleBackColor = true;
            this.RandomIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // YellowIcon
            // 
            this.YellowIcon.AutoSize = true;
            this.YellowIcon.Location = new System.Drawing.Point(111, 222);
            this.YellowIcon.Name = "YellowIcon";
            this.YellowIcon.Size = new System.Drawing.Size(56, 17);
            this.YellowIcon.TabIndex = 5;
            this.YellowIcon.TabStop = true;
            this.YellowIcon.Text = "Yellow";
            this.YellowIcon.UseVisualStyleBackColor = true;
            this.YellowIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // RedIcon
            // 
            this.RedIcon.AutoSize = true;
            this.RedIcon.Location = new System.Drawing.Point(111, 194);
            this.RedIcon.Name = "RedIcon";
            this.RedIcon.Size = new System.Drawing.Size(45, 17);
            this.RedIcon.TabIndex = 4;
            this.RedIcon.TabStop = true;
            this.RedIcon.Text = "Red";
            this.RedIcon.UseVisualStyleBackColor = true;
            this.RedIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // GreenIcon
            // 
            this.GreenIcon.AutoSize = true;
            this.GreenIcon.Location = new System.Drawing.Point(111, 165);
            this.GreenIcon.Name = "GreenIcon";
            this.GreenIcon.Size = new System.Drawing.Size(54, 17);
            this.GreenIcon.TabIndex = 3;
            this.GreenIcon.TabStop = true;
            this.GreenIcon.Text = "Green";
            this.GreenIcon.UseVisualStyleBackColor = true;
            this.GreenIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // PurpleIcon
            // 
            this.PurpleIcon.AutoSize = true;
            this.PurpleIcon.Location = new System.Drawing.Point(111, 137);
            this.PurpleIcon.Name = "PurpleIcon";
            this.PurpleIcon.Size = new System.Drawing.Size(55, 17);
            this.PurpleIcon.TabIndex = 2;
            this.PurpleIcon.TabStop = true;
            this.PurpleIcon.Text = "Purple";
            this.PurpleIcon.UseVisualStyleBackColor = true;
            this.PurpleIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // BlueIcon
            // 
            this.BlueIcon.AutoSize = true;
            this.BlueIcon.Location = new System.Drawing.Point(111, 109);
            this.BlueIcon.Name = "BlueIcon";
            this.BlueIcon.Size = new System.Drawing.Size(46, 17);
            this.BlueIcon.TabIndex = 1;
            this.BlueIcon.TabStop = true;
            this.BlueIcon.Text = "Blue";
            this.BlueIcon.UseVisualStyleBackColor = true;
            this.BlueIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // DefaultIcon
            // 
            this.DefaultIcon.AutoSize = true;
            this.DefaultIcon.Location = new System.Drawing.Point(111, 53);
            this.DefaultIcon.Name = "DefaultIcon";
            this.DefaultIcon.Size = new System.Drawing.Size(59, 17);
            this.DefaultIcon.TabIndex = 0;
            this.DefaultIcon.TabStop = true;
            this.DefaultIcon.Text = "Default";
            this.DefaultIcon.UseVisualStyleBackColor = true;
            this.DefaultIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
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
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(157, 17);
            this.DrawNonRelativesTextGray.TabIndex = 17;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(9, 96);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(167, 17);
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
            this.BranchBorders.Size = new System.Drawing.Size(125, 17);
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
            this.groupBox3.Location = new System.Drawing.Point(8, 288);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(387, 194);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Difference view";
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
            this.tpStart.Size = new System.Drawing.Size(816, 521);
            this.tpStart.TabIndex = 6;
            this.tpStart.Text = "Start page";
            this.tpStart.UseVisualStyleBackColor = true;
            // 
            // dashboardEditor1
            // 
            this.dashboardEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardEditor1.Location = new System.Drawing.Point(3, 3);
            this.dashboardEditor1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dashboardEditor1.Name = "dashboardEditor1";
            this.dashboardEditor1.Size = new System.Drawing.Size(810, 515);
            this.dashboardEditor1.TabIndex = 0;
            // 
            // tpGlobalSettings
            // 
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
            this.tpGlobalSettings.Size = new System.Drawing.Size(816, 521);
            this.tpGlobalSettings.TabIndex = 1;
            this.tpGlobalSettings.Text = "Global settings";
            this.tpGlobalSettings.UseVisualStyleBackColor = true;
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Location = new System.Drawing.Point(8, 436);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(127, 15);
            this.label60.TabIndex = 49;
            this.label60.Text = "Files content encoding";
            // 
            // Global_FilesEncoding
            // 
            this.Global_FilesEncoding.FormattingEnabled = true;
            this.Global_FilesEncoding.Location = new System.Drawing.Point(172, 434);
            this.Global_FilesEncoding.Name = "Global_FilesEncoding";
            this.Global_FilesEncoding.Size = new System.Drawing.Size(231, 23);
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
            this.CommitTemplatePath.Location = new System.Drawing.Point(203, 286);
            this.CommitTemplatePath.Name = "CommitTemplatePath";
            this.CommitTemplatePath.Size = new System.Drawing.Size(297, 23);
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
            this.groupBox9.Size = new System.Drawing.Size(0, 105);
            this.groupBox9.TabIndex = 31;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Line endings";
            // 
            // globalAutoCrlfFalse
            // 
            this.globalAutoCrlfFalse.AutoSize = true;
            this.globalAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.globalAutoCrlfFalse.Name = "globalAutoCrlfFalse";
            this.globalAutoCrlfFalse.Size = new System.Drawing.Size(313, 17);
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
            this.globalAutoCrlfInput.Size = new System.Drawing.Size(397, 17);
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
            this.globalAutoCrlfTrue.Size = new System.Drawing.Size(439, 17);
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
            this.DifftoolCmd.Location = new System.Drawing.Point(162, 259);
            this.DifftoolCmd.Name = "DifftoolCmd";
            this.DifftoolCmd.Size = new System.Drawing.Size(338, 23);
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
            this.DifftoolPath.Location = new System.Drawing.Point(162, 232);
            this.DifftoolPath.Name = "DifftoolPath";
            this.DifftoolPath.Size = new System.Drawing.Size(338, 23);
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
            this.GlobalDiffTool.Location = new System.Drawing.Point(162, 205);
            this.GlobalDiffTool.Name = "GlobalDiffTool";
            this.GlobalDiffTool.Size = new System.Drawing.Size(155, 23);
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
            this.MergeToolCmd.Location = new System.Drawing.Point(162, 147);
            this.MergeToolCmd.Name = "MergeToolCmd";
            this.MergeToolCmd.Size = new System.Drawing.Size(338, 23);
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
            this.GlobalMergeTool.Location = new System.Drawing.Point(162, 92);
            this.GlobalMergeTool.Name = "GlobalMergeTool";
            this.GlobalMergeTool.Size = new System.Drawing.Size(155, 23);
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
            this.MergetoolPath.Location = new System.Drawing.Point(162, 120);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(338, 23);
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
            this.GlobalEditor.Location = new System.Drawing.Point(162, 65);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(338, 23);
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
            this.GlobalUserEmail.Location = new System.Drawing.Point(162, 37);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(227, 23);
            this.GlobalUserEmail.TabIndex = 3;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(162, 8);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(227, 23);
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
            // tpLocalSettings
            // 
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
            this.tpLocalSettings.Size = new System.Drawing.Size(816, 521);
            this.tpLocalSettings.TabIndex = 0;
            this.tpLocalSettings.Text = "Local settings";
            this.tpLocalSettings.UseVisualStyleBackColor = true;
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Location = new System.Drawing.Point(14, 261);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(127, 15);
            this.label61.TabIndex = 45;
            this.label61.Text = "Files content encoding";
            // 
            // Local_FilesEncoding
            // 
            this.Local_FilesEncoding.FormattingEnabled = true;
            this.Local_FilesEncoding.Location = new System.Drawing.Point(178, 258);
            this.Local_FilesEncoding.Name = "Local_FilesEncoding";
            this.Local_FilesEncoding.Size = new System.Drawing.Size(262, 23);
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
            this.groupBox10.Size = new System.Drawing.Size(0, 105);
            this.groupBox10.TabIndex = 32;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Line endings";
            // 
            // localAutoCrlfFalse
            // 
            this.localAutoCrlfFalse.AutoSize = true;
            this.localAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.localAutoCrlfFalse.Name = "localAutoCrlfFalse";
            this.localAutoCrlfFalse.Size = new System.Drawing.Size(313, 17);
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
            this.localAutoCrlfInput.Size = new System.Drawing.Size(397, 17);
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
            this.localAutoCrlfTrue.Size = new System.Drawing.Size(439, 17);
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
            "Araxis",
            "BeyondCompare3",
            "DiffMerge",
            "kdiff3",
            "p4merge",
            "TortoiseMerge"});
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
            // tpScriptsTab
            // 
            this.tpScriptsTab.Controls.Add(this.tableLayoutPanel1);
            this.tpScriptsTab.Controls.Add(this.scriptNeedsConfirmation);
            this.tpScriptsTab.Controls.Add(this.inMenuCheckBox);
            this.tpScriptsTab.Location = new System.Drawing.Point(4, 24);
            this.tpScriptsTab.Name = "tpScriptsTab";
            this.tpScriptsTab.Padding = new System.Windows.Forms.Padding(3);
            this.tpScriptsTab.Size = new System.Drawing.Size(816, 521);
            this.tpScriptsTab.TabIndex = 8;
            this.tpScriptsTab.Text = "Scripts";
            this.tpScriptsTab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.ScriptList, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(810, 515);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // ScriptList
            // 
            this.ScriptList.AllowUserToAddRows = false;
            this.ScriptList.AllowUserToDeleteRows = false;
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
            this.ScriptList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScriptList.GridColor = System.Drawing.SystemColors.ActiveBorder;
            this.ScriptList.Location = new System.Drawing.Point(3, 3);
            this.ScriptList.Name = "ScriptList";
            this.ScriptList.ReadOnly = true;
            this.ScriptList.RowHeadersVisible = false;
            this.ScriptList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ScriptList.ShowCellErrors = false;
            this.ScriptList.Size = new System.Drawing.Size(708, 224);
            this.ScriptList.TabIndex = 18;
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
            // panel1
            // 
            this.panel1.Controls.Add(this.addScriptButton);
            this.panel1.Controls.Add(this.moveUpButton);
            this.panel1.Controls.Add(this.removeScriptButton);
            this.panel1.Controls.Add(this.moveDownButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(717, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(90, 224);
            this.panel1.TabIndex = 0;
            // 
            // addScriptButton
            // 
            this.addScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addScriptButton.Location = new System.Drawing.Point(7, 86);
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
            this.moveUpButton.Location = new System.Drawing.Point(32, 57);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(26, 23);
            this.moveUpButton.TabIndex = 1;
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // removeScriptButton
            // 
            this.removeScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeScriptButton.Enabled = false;
            this.removeScriptButton.Location = new System.Drawing.Point(7, 117);
            this.removeScriptButton.Name = "removeScriptButton";
            this.removeScriptButton.Size = new System.Drawing.Size(75, 25);
            this.removeScriptButton.TabIndex = 4;
            this.removeScriptButton.Text = "Remove";
            this.removeScriptButton.UseVisualStyleBackColor = true;
            this.removeScriptButton.Click += new System.EventHandler(this.removeScriptButton_Click);
            // 
            // moveDownButton
            // 
            this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Image = global::GitUI.Properties.Resources.ArrowDown;
            this.moveDownButton.Location = new System.Drawing.Point(32, 148);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(26, 23);
            this.moveDownButton.TabIndex = 5;
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel3, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.nameLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.commandLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel2, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.argumentsLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.argumentsTextBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelOnEvent, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 233);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(708, 279);
            this.tableLayoutPanel2.TabIndex = 19;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.Controls.Add(this.scriptEvent);
            this.flowLayoutPanel3.Controls.Add(this.lbl_icon);
            this.flowLayoutPanel3.Controls.Add(this.sbtn_icon);
            this.flowLayoutPanel3.Controls.Add(this.helpLabel);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(78, 245);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(627, 31);
            this.flowLayoutPanel3.TabIndex = 26;
            // 
            // scriptEvent
            // 
            this.scriptEvent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.scriptEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scriptEvent.FormattingEnabled = true;
            this.scriptEvent.Location = new System.Drawing.Point(3, 4);
            this.scriptEvent.Name = "scriptEvent";
            this.scriptEvent.Size = new System.Drawing.Size(208, 23);
            this.scriptEvent.TabIndex = 19;
            this.scriptEvent.SelectedIndexChanged += new System.EventHandler(this.scriptEvent_SelectedIndexChanged);
            this.scriptEvent.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // lbl_icon
            // 
            this.lbl_icon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_icon.AutoSize = true;
            this.lbl_icon.Location = new System.Drawing.Point(217, 8);
            this.lbl_icon.Name = "lbl_icon";
            this.lbl_icon.Size = new System.Drawing.Size(33, 15);
            this.lbl_icon.TabIndex = 23;
            this.lbl_icon.Text = "Icon:";
            this.lbl_icon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_icon.Visible = false;
            // 
            // sbtn_icon
            // 
            this.sbtn_icon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.sbtn_icon.AutoSize = true;
            this.sbtn_icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.sbtn_icon.ContextMenuStrip = this.contextMenuStrip_SplitButton;
            this.sbtn_icon.Location = new System.Drawing.Point(256, 3);
            this.sbtn_icon.Name = "sbtn_icon";
            this.sbtn_icon.Size = new System.Drawing.Size(92, 25);
            this.sbtn_icon.SplitMenuStrip = this.contextMenuStrip_SplitButton;
            this.sbtn_icon.TabIndex = 22;
            this.sbtn_icon.Text = "Select icon";
            this.sbtn_icon.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sbtn_icon.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.sbtn_icon.UseVisualStyleBackColor = true;
            this.sbtn_icon.Visible = false;
            this.sbtn_icon.WholeButtonDropdown = true;
            // 
            // helpLabel
            // 
            this.helpLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.helpLabel.AutoSize = true;
            this.helpLabel.BackColor = System.Drawing.SystemColors.Info;
            this.helpLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.helpLabel.Location = new System.Drawing.Point(354, 7);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(177, 17);
            this.helpLabel.TabIndex = 16;
            this.helpLabel.Text = "Press F1 to see available options";
            this.helpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.helpLabel.Visible = false;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 12);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(42, 15);
            this.nameLabel.TabIndex = 12;
            this.nameLabel.Text = "Name:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.nameTextBox);
            this.flowLayoutPanel1.Controls.Add(this.scriptEnabled);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(78, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(627, 34);
            this.flowLayoutPanel1.TabIndex = 13;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameTextBox.Location = new System.Drawing.Point(3, 3);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(160, 23);
            this.nameTextBox.TabIndex = 6;
            this.nameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // scriptEnabled
            // 
            this.scriptEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.scriptEnabled.AutoSize = true;
            this.scriptEnabled.Location = new System.Drawing.Point(169, 5);
            this.scriptEnabled.Name = "scriptEnabled";
            this.scriptEnabled.Size = new System.Drawing.Size(68, 19);
            this.scriptEnabled.TabIndex = 18;
            this.scriptEnabled.Text = "Enabled";
            this.scriptEnabled.UseVisualStyleBackColor = true;
            this.scriptEnabled.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // commandLabel
            // 
            this.commandLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(3, 52);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(67, 15);
            this.commandLabel.TabIndex = 13;
            this.commandLabel.Text = "Command:";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.commandTextBox);
            this.flowLayoutPanel2.Controls.Add(this.browseScriptButton);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(78, 43);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(627, 34);
            this.flowLayoutPanel2.TabIndex = 14;
            // 
            // commandTextBox
            // 
            this.commandTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.commandTextBox.Location = new System.Drawing.Point(3, 4);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(441, 23);
            this.commandTextBox.TabIndex = 7;
            this.commandTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // browseScriptButton
            // 
            this.browseScriptButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.browseScriptButton.Location = new System.Drawing.Point(450, 3);
            this.browseScriptButton.Name = "browseScriptButton";
            this.browseScriptButton.Size = new System.Drawing.Size(75, 25);
            this.browseScriptButton.TabIndex = 11;
            this.browseScriptButton.Text = "Browse";
            this.browseScriptButton.UseVisualStyleBackColor = true;
            this.browseScriptButton.Click += new System.EventHandler(this.browseScriptButton_Click);
            // 
            // argumentsLabel
            // 
            this.argumentsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.argumentsLabel.AutoSize = true;
            this.argumentsLabel.Location = new System.Drawing.Point(3, 153);
            this.argumentsLabel.Name = "argumentsLabel";
            this.argumentsLabel.Size = new System.Drawing.Size(69, 15);
            this.argumentsLabel.TabIndex = 14;
            this.argumentsLabel.Text = "Arguments:";
            // 
            // labelOnEvent
            // 
            this.labelOnEvent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelOnEvent.AutoSize = true;
            this.labelOnEvent.Location = new System.Drawing.Point(3, 253);
            this.labelOnEvent.Name = "labelOnEvent";
            this.labelOnEvent.Size = new System.Drawing.Size(58, 15);
            this.labelOnEvent.TabIndex = 20;
            this.labelOnEvent.Text = "On event:";
            // 
            // scriptNeedsConfirmation
            // 
            this.scriptNeedsConfirmation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.scriptNeedsConfirmation.AutoSize = true;
            this.scriptNeedsConfirmation.Location = new System.Drawing.Point(107, -9966);
            this.scriptNeedsConfirmation.Name = "scriptNeedsConfirmation";
            this.scriptNeedsConfirmation.Size = new System.Drawing.Size(135, 19);
            this.scriptNeedsConfirmation.TabIndex = 21;
            this.scriptNeedsConfirmation.Text = "Ask for confirmation";
            this.scriptNeedsConfirmation.UseVisualStyleBackColor = true;
            this.scriptNeedsConfirmation.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // inMenuCheckBox
            // 
            this.inMenuCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.inMenuCheckBox.AutoSize = true;
            this.inMenuCheckBox.Location = new System.Drawing.Point(107, -9927);
            this.inMenuCheckBox.Name = "inMenuCheckBox";
            this.inMenuCheckBox.Size = new System.Drawing.Size(206, 19);
            this.inMenuCheckBox.TabIndex = 15;
            this.inMenuCheckBox.Text = "Add to revision grid context menu";
            this.inMenuCheckBox.UseVisualStyleBackColor = true;
            this.inMenuCheckBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // tpHotkeys
            // 
            this.tpHotkeys.Controls.Add(this.controlHotkeys);
            this.tpHotkeys.Location = new System.Drawing.Point(4, 24);
            this.tpHotkeys.Name = "tpHotkeys";
            this.tpHotkeys.Padding = new System.Windows.Forms.Padding(3);
            this.tpHotkeys.Size = new System.Drawing.Size(816, 521);
            this.tpHotkeys.TabIndex = 9;
            this.tpHotkeys.Text = "Hotkeys";
            this.tpHotkeys.UseVisualStyleBackColor = true;
            // 
            // controlHotkeys
            // 
            this.controlHotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlHotkeys.Location = new System.Drawing.Point(3, 3);
            this.controlHotkeys.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.controlHotkeys.Name = "controlHotkeys";
            this.controlHotkeys.Size = new System.Drawing.Size(810, 515);
            this.controlHotkeys.TabIndex = 0;
            // 
            // tpShellExt
            // 
            this.tpShellExt.Controls.Add(this.lblMenuEntries);
            this.tpShellExt.Controls.Add(this.chlMenuEntries);
            this.tpShellExt.Controls.Add(this.chkCascadedContextMenu);
            this.tpShellExt.Location = new System.Drawing.Point(4, 24);
            this.tpShellExt.Name = "tpShellExt";
            this.tpShellExt.Padding = new System.Windows.Forms.Padding(3);
            this.tpShellExt.Size = new System.Drawing.Size(816, 521);
            this.tpShellExt.TabIndex = 11;
            this.tpShellExt.Text = "Shell extension";
            this.tpShellExt.UseVisualStyleBackColor = true;
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.lblMenuEntries.Location = new System.Drawing.Point(8, 50);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(158, 15);
            this.lblMenuEntries.TabIndex = 2;
            this.lblMenuEntries.Text = "Visible context menu entries:";
            // 
            // chlMenuEntries
            // 
            this.chlMenuEntries.CheckOnClick = true;
            this.chlMenuEntries.FormattingEnabled = true;
            this.chlMenuEntries.Items.AddRange(new object[] {
            "Add files",
            "Apply patch",
            "Browse",
            "Create branch",
            "Checkout branch",
            "Checkout revision",
            "Clone",
            "Commit",
            "File history",
            "Reset file changes",
            "Pull",
            "Push",
            "Settings",
            "View diff"});
            this.chlMenuEntries.Location = new System.Drawing.Point(10, 68);
            this.chlMenuEntries.Name = "chlMenuEntries";
            this.chlMenuEntries.Size = new System.Drawing.Size(240, 256);
            this.chlMenuEntries.TabIndex = 1;
            // 
            // chkCascadedContextMenu
            // 
            this.chkCascadedContextMenu.AutoSize = true;
            this.chkCascadedContextMenu.Location = new System.Drawing.Point(8, 15);
            this.chkCascadedContextMenu.Name = "chkCascadedContextMenu";
            this.chkCascadedContextMenu.Size = new System.Drawing.Size(153, 19);
            this.chkCascadedContextMenu.TabIndex = 0;
            this.chkCascadedContextMenu.Text = "Cascaded context menu";
            this.chkCascadedContextMenu.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(1593, 593);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(88, 24);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.Ok, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.tabControl1, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.settingsTreeViewUserControl1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelSettingsPageTitle, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label11, 0, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1684, 620);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // settingsTreeViewUserControl1
            // 
            this.settingsTreeViewUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTreeViewUserControl1.Location = new System.Drawing.Point(3, 3);
            this.settingsTreeViewUserControl1.MinimumSize = new System.Drawing.Size(100, 220);
            this.settingsTreeViewUserControl1.Name = "settingsTreeViewUserControl1";
            this.tableLayoutPanel3.SetRowSpan(this.settingsTreeViewUserControl1, 2);
            this.settingsTreeViewUserControl1.Size = new System.Drawing.Size(194, 584);
            this.settingsTreeViewUserControl1.TabIndex = 1;
            this.settingsTreeViewUserControl1.SettingsPageSelected += new System.EventHandler<GitUI.SettingsDialog.SettingsPageSelectedEventArgs>(this.settingsTreeViewUserControl1_SettingsPageSelected);
            // 
            // labelSettingsPageTitle
            // 
            this.labelSettingsPageTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSettingsPageTitle.AutoSize = true;
            this.labelSettingsPageTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelSettingsPageTitle.Location = new System.Drawing.Point(210, 0);
            this.labelSettingsPageTitle.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.labelSettingsPageTitle.Name = "labelSettingsPageTitle";
            this.labelSettingsPageTitle.Size = new System.Drawing.Size(47, 35);
            this.labelSettingsPageTitle.TabIndex = 2;
            this.labelSettingsPageTitle.Text = "label11";
            this.labelSettingsPageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 590);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(190, 30);
            this.label11.TabIndex = 3;
            this.label11.Text = "Settings will be saved when dialog closes.";
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1684, 620);
            this.Controls.Add(this.tableLayoutPanel3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 605);
            this.Name = "FormSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.Shown += new System.EventHandler(this.FormSettings_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            this.tabControl1.ResumeLayout(false);
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
            this.tpLocalSettings.ResumeLayout(false);
            this.tpLocalSettings.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.InvalidGitPathLocal.ResumeLayout(false);
            this.InvalidGitPathLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.tpScriptsTab.ResumeLayout(false);
            this.tpScriptsTab.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ScriptList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).EndInit();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.tpHotkeys.ResumeLayout(false);
            this.tpShellExt.ResumeLayout(false);
            this.tpShellExt.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tpLocalSettings;
        private System.Windows.Forms.TextBox UserEmail;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
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
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.DirectoryServices.DirectorySearcher directorySearcher2;
        private System.Windows.Forms.Label PathToKDiff3;
        private System.Windows.Forms.TextBox MergetoolPath;
        private System.Windows.Forms.ComboBox GlobalMergeTool;
        private System.Windows.Forms.ComboBox LocalMergeTool;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button BrowseMergeTool;
        private System.Windows.Forms.ComboBox MergeToolCmd;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button MergeToolCmdSuggest;
        private System.Windows.Forms.Label NoGitRepo;
        private System.Windows.Forms.Panel InvalidGitPathGlobal;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel InvalidGitPathLocal;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox2;
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
        private System.Windows.Forms.ComboBox GlobalDiffTool;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Button BrowseDiffTool;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox DifftoolPath;
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
        private System.Windows.Forms.CheckBox MulticolorBranches;
        private System.Windows.Forms.CheckBox BranchBorders;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorGraphLabel;
        private System.Windows.Forms.CheckBox StripedBanchChange;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.RadioButton globalAutoCrlfFalse;
        private System.Windows.Forms.RadioButton globalAutoCrlfInput;
        private System.Windows.Forms.RadioButton globalAutoCrlfTrue;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton localAutoCrlfFalse;
        private System.Windows.Forms.RadioButton localAutoCrlfInput;
        private System.Windows.Forms.RadioButton localAutoCrlfTrue;
        private System.Windows.Forms.CheckBox DrawNonRelativesGray;
        private System.Windows.Forms.TabPage tpScriptsTab;
        private System.Windows.Forms.CheckBox inMenuCheckBox;
        private System.Windows.Forms.Label argumentsLabel;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.Button browseScriptButton;
        private System.Windows.Forms.RichTextBox argumentsTextBox;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button removeScriptButton;
        private System.Windows.Forms.Button addScriptButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.CheckBox DrawNonRelativesTextGray;
        private System.Windows.Forms.TabPage tpHotkeys;
        private Hotkey.ControlHotkeys controlHotkeys;
        private BindingSource scriptInfoBindingSource;
        private CheckBox scriptEnabled;
        private Label labelOnEvent;
        private CheckBox scriptNeedsConfirmation;
        private RadioButton LightblueIcon;
        private ComboBox IconStyle;
        private Label label54;
        private PictureBox IconPreview;
        private PictureBox IconPreviewSmall;
        private Label label55;
        private ContextMenuStrip contextMenuStrip_SplitButton;
        private Button BrowseCommitTemplate;
        private Label label57;
        private TextBox CommitTemplatePath;
        private Label label60;
        private ComboBox Global_FilesEncoding;
        private Label label61;
        private ComboBox Local_FilesEncoding;
        private TabPage tpShellExt;
        private Label lblMenuEntries;
        private CheckedListBox chlMenuEntries;
        private CheckBox chkCascadedContextMenu;
        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView ScriptList;
        private DataGridViewTextBoxColumn HotkeyCommandIdentifier;
        private DataGridViewCheckBoxColumn EnabledColumn;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn OnEvent;
        private DataGridViewCheckBoxColumn AskConfirmation;
        private DataGridViewCheckBoxColumn addToRevisionGridContextMenuDataGridViewCheckBoxColumn;
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel2;
        private FlowLayoutPanel flowLayoutPanel3;
        private ComboBox scriptEvent;
        private Label lbl_icon;
        private Script.SplitButton sbtn_icon;
        private Label helpLabel;
        private Label nameLabel;
        private FlowLayoutPanel flowLayoutPanel1;
        private TextBox nameTextBox;
        private FlowLayoutPanel flowLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private SettingsDialog.SettingsTreeViewUserControl settingsTreeViewUserControl1;
        private Label labelSettingsPageTitle;
        private Label label11;

    }
}
