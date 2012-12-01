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
            this.tpStart = new System.Windows.Forms.TabPage();
            this.dashboardEditor1 = new GitUI.DashboardEditor();
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.settingsTreeViewUserControl1 = new GitUI.SettingsDialog.SettingsTreeViewUserControl();
            this.labelSettingsPageTitle = new System.Windows.Forms.Label();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDiscard = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpStart.SuspendLayout();
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
            this.flowLayoutPanel4.SuspendLayout();
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
            this.argumentsTextBox.Size = new System.Drawing.Size(3, 1);
            this.argumentsTextBox.TabIndex = 8;
            this.argumentsTextBox.Text = "";
            this.argumentsTextBox.Enter += new System.EventHandler(this.argumentsTextBox_Enter);
            this.argumentsTextBox.Leave += new System.EventHandler(this.argumentsTextBox_Leave);
            this.argumentsTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpStart);
            this.tabControl1.Controls.Add(this.tpLocalSettings);
            this.tabControl1.Controls.Add(this.tpScriptsTab);
            this.tabControl1.Controls.Add(this.tpHotkeys);
            this.tabControl1.Controls.Add(this.tpShellExt);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(203, 38);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 542);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpStart
            // 
            this.tpStart.Controls.Add(this.dashboardEditor1);
            this.tpStart.Location = new System.Drawing.Point(4, 24);
            this.tpStart.Name = "tpStart";
            this.tpStart.Padding = new System.Windows.Forms.Padding(3);
            this.tpStart.Size = new System.Drawing.Size(792, 514);
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
            this.dashboardEditor1.Size = new System.Drawing.Size(786, 508);
            this.dashboardEditor1.TabIndex = 0;
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
            this.tpLocalSettings.Size = new System.Drawing.Size(1148, 521);
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
            this.tpScriptsTab.Size = new System.Drawing.Size(1148, 521);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(186, 68);
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
            this.ScriptList.Size = new System.Drawing.Size(84, 224);
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
            this.panel1.Location = new System.Drawing.Point(93, 3);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(84, 1);
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
            this.flowLayoutPanel3.Location = new System.Drawing.Point(78, -79);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(3, 77);
            this.flowLayoutPanel3.TabIndex = 26;
            // 
            // scriptEvent
            // 
            this.scriptEvent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.scriptEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scriptEvent.FormattingEnabled = true;
            this.scriptEvent.Location = new System.Drawing.Point(3, 3);
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
            this.lbl_icon.Location = new System.Drawing.Point(3, 29);
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
            this.sbtn_icon.Location = new System.Drawing.Point(3, 47);
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
            this.helpLabel.Location = new System.Drawing.Point(3, 75);
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
            this.flowLayoutPanel1.Size = new System.Drawing.Size(3, 34);
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
            this.scriptEnabled.Location = new System.Drawing.Point(3, 32);
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
            this.flowLayoutPanel2.Size = new System.Drawing.Size(3, 34);
            this.flowLayoutPanel2.TabIndex = 14;
            // 
            // commandTextBox
            // 
            this.commandTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.commandTextBox.Location = new System.Drawing.Point(3, 3);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(441, 23);
            this.commandTextBox.TabIndex = 7;
            this.commandTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // browseScriptButton
            // 
            this.browseScriptButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.browseScriptButton.Location = new System.Drawing.Point(3, 32);
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
            this.argumentsLabel.Location = new System.Drawing.Point(3, 80);
            this.argumentsLabel.Name = "argumentsLabel";
            this.argumentsLabel.Size = new System.Drawing.Size(69, 1);
            this.argumentsLabel.TabIndex = 14;
            this.argumentsLabel.Text = "Arguments:";
            // 
            // labelOnEvent
            // 
            this.labelOnEvent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelOnEvent.AutoSize = true;
            this.labelOnEvent.Location = new System.Drawing.Point(3, -48);
            this.labelOnEvent.Name = "labelOnEvent";
            this.labelOnEvent.Size = new System.Drawing.Size(58, 15);
            this.labelOnEvent.TabIndex = 20;
            this.labelOnEvent.Text = "On event:";
            // 
            // scriptNeedsConfirmation
            // 
            this.scriptNeedsConfirmation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.scriptNeedsConfirmation.AutoSize = true;
            this.scriptNeedsConfirmation.Location = new System.Drawing.Point(107, -11752);
            this.scriptNeedsConfirmation.Name = "scriptNeedsConfirmation";
            this.scriptNeedsConfirmation.Size = new System.Drawing.Size(119, 17);
            this.scriptNeedsConfirmation.TabIndex = 21;
            this.scriptNeedsConfirmation.Text = "Ask for confirmation";
            this.scriptNeedsConfirmation.UseVisualStyleBackColor = true;
            this.scriptNeedsConfirmation.Validating += new System.ComponentModel.CancelEventHandler(this.ScriptInfoEdit_Validating);
            // 
            // inMenuCheckBox
            // 
            this.inMenuCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.inMenuCheckBox.AutoSize = true;
            this.inMenuCheckBox.Location = new System.Drawing.Point(107, -11713);
            this.inMenuCheckBox.Name = "inMenuCheckBox";
            this.inMenuCheckBox.Size = new System.Drawing.Size(183, 17);
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
            this.tpHotkeys.Size = new System.Drawing.Size(1148, 521);
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
            this.controlHotkeys.Size = new System.Drawing.Size(186, 68);
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
            this.tpShellExt.Size = new System.Drawing.Size(1148, 521);
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
            this.chkCascadedContextMenu.Size = new System.Drawing.Size(141, 17);
            this.chkCascadedContextMenu.TabIndex = 0;
            this.chkCascadedContextMenu.Text = "Cascaded context menu";
            this.chkCascadedContextMenu.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(427, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(88, 25);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.Ok_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tabControl1, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.settingsTreeViewUserControl1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelSettingsPageTitle, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel4, 1, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1006, 620);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // settingsTreeViewUserControl1
            // 
            this.settingsTreeViewUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTreeViewUserControl1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.settingsTreeViewUserControl1.Location = new System.Drawing.Point(3, 3);
            this.settingsTreeViewUserControl1.MinimumSize = new System.Drawing.Size(100, 220);
            this.settingsTreeViewUserControl1.Name = "settingsTreeViewUserControl1";
            this.tableLayoutPanel3.SetRowSpan(this.settingsTreeViewUserControl1, 2);
            this.settingsTreeViewUserControl1.Size = new System.Drawing.Size(194, 577);
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
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.AutoSize = true;
            this.flowLayoutPanel4.Controls.Add(this.buttonApply);
            this.flowLayoutPanel4.Controls.Add(this.buttonDiscard);
            this.flowLayoutPanel4.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel4.Controls.Add(this.buttonOk);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel4.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(203, 586);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(800, 31);
            this.flowLayoutPanel4.TabIndex = 3;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(521, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 25);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonDiscard
            // 
            this.buttonDiscard.Location = new System.Drawing.Point(615, 3);
            this.buttonDiscard.Name = "buttonDiscard";
            this.buttonDiscard.Size = new System.Drawing.Size(88, 25);
            this.buttonDiscard.TabIndex = 2;
            this.buttonDiscard.Text = "Discard";
            this.buttonDiscard.UseVisualStyleBackColor = true;
            this.buttonDiscard.Click += new System.EventHandler(this.buttonDiscard_Click);
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(709, 3);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(88, 25);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1006, 620);
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
            this.tpStart.ResumeLayout(false);
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
            this.flowLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tpLocalSettings;
        private System.Windows.Forms.TextBox UserEmail;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox Editor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox KeepMergeBackup;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.DirectoryServices.DirectorySearcher directorySearcher2;
        private System.Windows.Forms.ComboBox LocalMergeTool;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label NoGitRepo;
        private System.Windows.Forms.Panel InvalidGitPathLocal;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TabPage tpStart;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private DashboardEditor dashboardEditor1;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton localAutoCrlfFalse;
        private System.Windows.Forms.RadioButton localAutoCrlfInput;
        private System.Windows.Forms.RadioButton localAutoCrlfTrue;
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
        private System.Windows.Forms.TabPage tpHotkeys;
        private Hotkey.ControlHotkeys controlHotkeys;
        private BindingSource scriptInfoBindingSource;
        private CheckBox scriptEnabled;
        private Label labelOnEvent;
        private CheckBox scriptNeedsConfirmation;
        private ContextMenuStrip contextMenuStrip_SplitButton;
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
        private FlowLayoutPanel flowLayoutPanel4;
        private Button buttonApply;
        private Button buttonDiscard;
        private Button buttonCancel;

    }
}
