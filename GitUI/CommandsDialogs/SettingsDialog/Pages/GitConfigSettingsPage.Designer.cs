namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GitConfigSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label lblCommitTemplatePath;
            System.Windows.Forms.Label lblMergeTool;
            System.Windows.Forms.Label lblDiffToolCommand;
            System.Windows.Forms.Label lblMergeToolPath;
            System.Windows.Forms.Label lblDiffToolPath;
            System.Windows.Forms.Label lblMergeToolCommand;
            System.Windows.Forms.Label lblDiffTool;
            this.label60 = new System.Windows.Forms.Label();
            this.Global_FilesEncoding = new System.Windows.Forms.ComboBox();
            this.btnCommitTemplateBrowse = new System.Windows.Forms.Button();
            this.txtCommitTemplatePath = new System.Windows.Forms.TextBox();
            this.btnDiffToolCommandSuggest = new System.Windows.Forms.Button();
            this.txtDiffToolCommand = new System.Windows.Forms.TextBox();
            this.btnDiffToolBrowse = new System.Windows.Forms.Button();
            this.txtDiffToolPath = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_cboDiffTool = new System.Windows.Forms.ComboBox();
            this.InvalidGitPathGlobal = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnMergeToolCommandSuggest = new System.Windows.Forms.Button();
            this.txtMergeToolCommand = new System.Windows.Forms.TextBox();
            this.btnMergeToolBrowse = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_cboMergeTool = new System.Windows.Forms.ComboBox();
            this.txtMergeToolPath = new System.Windows.Forms.TextBox();
            this.GlobalEditor = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.GlobalUserEmail = new System.Windows.Forms.TextBox();
            this.GlobalUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxLineEndings = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelLineEndings = new System.Windows.Forms.FlowLayoutPanel();
            this.globalAutoCrlfTrue = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfInput = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfFalse = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfNotSet = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanelGitConfig = new System.Windows.Forms.TableLayoutPanel();
            this.ConfigureEncoding = new System.Windows.Forms.Button();
            this.lblGPGProgram = new System.Windows.Forms.Label();
            this.lblSigningKey = new System.Windows.Forms.Label();
            this.lblCommitSigning = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.chkCommitSigning = new System.Windows.Forms.CheckBox();
            this.txtGPGProgram = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.gpgSecretKeysComboboxUserKeys = new GitUI.UserControls.GPGSecretKeysComboboxControl();
            lblCommitTemplatePath = new System.Windows.Forms.Label();
            lblMergeTool = new System.Windows.Forms.Label();
            lblDiffToolCommand = new System.Windows.Forms.Label();
            lblMergeToolPath = new System.Windows.Forms.Label();
            lblDiffToolPath = new System.Windows.Forms.Label();
            lblMergeToolCommand = new System.Windows.Forms.Label();
            lblDiffTool = new System.Windows.Forms.Label();
            this.InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxLineEndings.SuspendLayout();
            this.flowLayoutPanelLineEndings.SuspendLayout();
            this.tableLayoutPanelGitConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCommitTemplatePath
            // 
            lblCommitTemplatePath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCommitTemplatePath.AutoSize = true;
            lblCommitTemplatePath.Location = new System.Drawing.Point(3, 266);
            lblCommitTemplatePath.Name = "lblCommitTemplatePath";
            lblCommitTemplatePath.Size = new System.Drawing.Size(120, 13);
            lblCommitTemplatePath.TabIndex = 76;
            lblCommitTemplatePath.Text = "Path to commit template";
            // 
            // lblMergeTool
            // 
            lblMergeTool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMergeTool.AutoSize = true;
            lblMergeTool.Location = new System.Drawing.Point(3, 86);
            lblMergeTool.Name = "lblMergeTool";
            lblMergeTool.Size = new System.Drawing.Size(54, 13);
            lblMergeTool.TabIndex = 56;
            lblMergeTool.Text = "Mergetool";
            // 
            // lblDiffToolCommand
            // 
            lblDiffToolCommand.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDiffToolCommand.AutoSize = true;
            lblDiffToolCommand.Location = new System.Drawing.Point(3, 235);
            lblDiffToolCommand.Name = "lblDiffToolCommand";
            lblDiffToolCommand.Size = new System.Drawing.Size(89, 13);
            lblDiffToolCommand.TabIndex = 72;
            lblDiffToolCommand.Text = "Difftool command";
            // 
            // lblMergeToolPath
            // 
            lblMergeToolPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMergeToolPath.AutoSize = true;
            lblMergeToolPath.Location = new System.Drawing.Point(3, 115);
            lblMergeToolPath.Name = "lblMergeToolPath";
            lblMergeToolPath.Size = new System.Drawing.Size(90, 13);
            lblMergeToolPath.TabIndex = 59;
            lblMergeToolPath.Text = "Path to mergetool";
            // 
            // lblDiffToolPath
            // 
            lblDiffToolPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDiffToolPath.AutoSize = true;
            lblDiffToolPath.Location = new System.Drawing.Point(3, 204);
            lblDiffToolPath.Name = "lblDiffToolPath";
            lblDiffToolPath.Size = new System.Drawing.Size(75, 13);
            lblDiffToolPath.TabIndex = 70;
            lblDiffToolPath.Text = "Path to difftool";
            // 
            // lblMergeToolCommand
            // 
            lblMergeToolCommand.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMergeToolCommand.AutoSize = true;
            lblMergeToolCommand.Location = new System.Drawing.Point(3, 146);
            lblMergeToolCommand.Name = "lblMergeToolCommand";
            lblMergeToolCommand.Size = new System.Drawing.Size(103, 13);
            lblMergeToolCommand.TabIndex = 62;
            lblMergeToolCommand.Text = "Mergetool command";
            // 
            // lblDiffTool
            // 
            lblDiffTool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDiffTool.AutoSize = true;
            lblDiffTool.Location = new System.Drawing.Point(3, 175);
            lblDiffTool.Name = "lblDiffTool";
            lblDiffTool.Size = new System.Drawing.Size(40, 13);
            lblDiffTool.TabIndex = 67;
            lblDiffTool.Text = "Difftool";
            // 
            // label60
            // 
            this.label60.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label60.AutoSize = true;
            this.label60.Location = new System.Drawing.Point(3, 417);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(114, 13);
            this.label60.TabIndex = 79;
            this.label60.Text = "Files content encoding";
            // 
            // Global_FilesEncoding
            // 
            this.Global_FilesEncoding.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Global_FilesEncoding.FormattingEnabled = true;
            this.Global_FilesEncoding.Location = new System.Drawing.Point(129, 413);
            this.Global_FilesEncoding.Name = "Global_FilesEncoding";
            this.Global_FilesEncoding.Size = new System.Drawing.Size(241, 21);
            this.Global_FilesEncoding.TabIndex = 17;
            // 
            // btnCommitTemplateBrowse
            // 
            this.btnCommitTemplateBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCommitTemplateBrowse.Location = new System.Drawing.Point(1902, 260);
            this.btnCommitTemplateBrowse.Name = "btnCommitTemplateBrowse";
            this.btnCommitTemplateBrowse.Size = new System.Drawing.Size(123, 25);
            this.btnCommitTemplateBrowse.TabIndex = 15;
            this.btnCommitTemplateBrowse.Text = "Browse";
            this.btnCommitTemplateBrowse.UseVisualStyleBackColor = true;
            this.btnCommitTemplateBrowse.Click += new System.EventHandler(this.btnCommitTemplateBrowse_Click);
            // 
            // txtCommitTemplatePath
            // 
            this.txtCommitTemplatePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommitTemplatePath.Location = new System.Drawing.Point(129, 262);
            this.txtCommitTemplatePath.Name = "txtCommitTemplatePath";
            this.txtCommitTemplatePath.Size = new System.Drawing.Size(1767, 20);
            this.txtCommitTemplatePath.TabIndex = 14;
            // 
            // btnDiffToolCommandSuggest
            // 
            this.btnDiffToolCommandSuggest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDiffToolCommandSuggest.Location = new System.Drawing.Point(1902, 229);
            this.btnDiffToolCommandSuggest.Name = "btnDiffToolCommandSuggest";
            this.btnDiffToolCommandSuggest.Size = new System.Drawing.Size(123, 25);
            this.btnDiffToolCommandSuggest.TabIndex = 13;
            this.btnDiffToolCommandSuggest.Text = "Suggest";
            this.btnDiffToolCommandSuggest.UseVisualStyleBackColor = true;
            this.btnDiffToolCommandSuggest.Click += new System.EventHandler(this.btnDiffToolCommandSuggest_Click);
            // 
            // txtDiffToolCommand
            // 
            this.txtDiffToolCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDiffToolCommand.Location = new System.Drawing.Point(129, 231);
            this.txtDiffToolCommand.Name = "txtDiffToolCommand";
            this.txtDiffToolCommand.Size = new System.Drawing.Size(1767, 20);
            this.txtDiffToolCommand.TabIndex = 12;
            // 
            // btnDiffToolBrowse
            // 
            this.btnDiffToolBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDiffToolBrowse.Location = new System.Drawing.Point(1902, 198);
            this.btnDiffToolBrowse.Name = "btnDiffToolBrowse";
            this.btnDiffToolBrowse.Size = new System.Drawing.Size(123, 25);
            this.btnDiffToolBrowse.TabIndex = 11;
            this.btnDiffToolBrowse.Text = "Browse";
            this.btnDiffToolBrowse.UseVisualStyleBackColor = true;
            this.btnDiffToolBrowse.Click += new System.EventHandler(this.btnDiffToolBrowse_Click);
            // 
            // txtDiffToolPath
            // 
            this.txtDiffToolPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDiffToolPath.Location = new System.Drawing.Point(129, 200);
            this.txtDiffToolPath.Name = "txtDiffToolPath";
            this.txtDiffToolPath.Size = new System.Drawing.Size(1767, 20);
            this.txtDiffToolPath.TabIndex = 10;
            this.txtDiffToolPath.LostFocus += new System.EventHandler(this.txtDiffMergeToolPath_LostFocus);
            // 
            // _NO_TRANSLATE_cboDiffTool
            // 
            this._NO_TRANSLATE_cboDiffTool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_cboDiffTool.FormattingEnabled = true;
            this._NO_TRANSLATE_cboDiffTool.Location = new System.Drawing.Point(129, 171);
            this._NO_TRANSLATE_cboDiffTool.Name = "_NO_TRANSLATE_cboDiffTool";
            this._NO_TRANSLATE_cboDiffTool.Size = new System.Drawing.Size(241, 21);
            this._NO_TRANSLATE_cboDiffTool.TabIndex = 9;
            this._NO_TRANSLATE_cboDiffTool.TextChanged += new System.EventHandler(this.cboDiffTool_TextChanged);
            // 
            // InvalidGitPathGlobal
            // 
            this.InvalidGitPathGlobal.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.InvalidGitPathGlobal.AutoSize = true;
            this.InvalidGitPathGlobal.BackColor = System.Drawing.SystemColors.Info;
            this.InvalidGitPathGlobal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanelGitConfig.SetColumnSpan(this.InvalidGitPathGlobal, 2);
            this.InvalidGitPathGlobal.Controls.Add(this.label9);
            this.InvalidGitPathGlobal.Controls.Add(this.pictureBox1);
            this.InvalidGitPathGlobal.Location = new System.Drawing.Point(1902, 29);
            this.InvalidGitPathGlobal.Name = "InvalidGitPathGlobal";
            this.InvalidGitPathGlobal.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanelGitConfig.SetRowSpan(this.InvalidGitPathGlobal, 4);
            this.InvalidGitPathGlobal.Size = new System.Drawing.Size(237, 47);
            this.InvalidGitPathGlobal.TabIndex = 65;
            this.InvalidGitPathGlobal.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(57, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(175, 39);
            this.label9.TabIndex = 19;
            this.label9.Text = "You need to set the correct path to \r\ngit before you can change\r\nglobal settings." +
    "\r\n";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = global::GitUI.Properties.Images.StatusBadgeError;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(54, 39);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // btnMergeToolCommandSuggest
            // 
            this.btnMergeToolCommandSuggest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMergeToolCommandSuggest.Location = new System.Drawing.Point(1902, 140);
            this.btnMergeToolCommandSuggest.Name = "btnMergeToolCommandSuggest";
            this.btnMergeToolCommandSuggest.Size = new System.Drawing.Size(123, 25);
            this.btnMergeToolCommandSuggest.TabIndex = 7;
            this.btnMergeToolCommandSuggest.Text = "Suggest";
            this.btnMergeToolCommandSuggest.UseVisualStyleBackColor = true;
            this.btnMergeToolCommandSuggest.Click += new System.EventHandler(this.btnMergeToolCommandSuggest_Click);
            // 
            // txtMergeToolCommand
            // 
            this.txtMergeToolCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMergeToolCommand.Location = new System.Drawing.Point(129, 142);
            this.txtMergeToolCommand.Name = "txtMergeToolCommand";
            this.txtMergeToolCommand.Size = new System.Drawing.Size(1767, 20);
            this.txtMergeToolCommand.TabIndex = 6;
            // 
            // btnMergeToolBrowse
            // 
            this.btnMergeToolBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMergeToolBrowse.Location = new System.Drawing.Point(1902, 109);
            this.btnMergeToolBrowse.Name = "btnMergeToolBrowse";
            this.btnMergeToolBrowse.Size = new System.Drawing.Size(123, 25);
            this.btnMergeToolBrowse.TabIndex = 5;
            this.btnMergeToolBrowse.Text = "Browse";
            this.btnMergeToolBrowse.UseVisualStyleBackColor = true;
            this.btnMergeToolBrowse.Click += new System.EventHandler(this.btnMergeToolBrowse_Click);
            // 
            // _NO_TRANSLATE_cboMergeTool
            // 
            this._NO_TRANSLATE_cboMergeTool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_cboMergeTool.FormattingEnabled = true;
            this._NO_TRANSLATE_cboMergeTool.Location = new System.Drawing.Point(129, 82);
            this._NO_TRANSLATE_cboMergeTool.Name = "_NO_TRANSLATE_cboMergeTool";
            this._NO_TRANSLATE_cboMergeTool.Size = new System.Drawing.Size(241, 21);
            this._NO_TRANSLATE_cboMergeTool.TabIndex = 3;
            this._NO_TRANSLATE_cboMergeTool.TextChanged += new System.EventHandler(this.cboMergeTool_TextChanged);
            // 
            // txtMergeToolPath
            // 
            this.txtMergeToolPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMergeToolPath.Location = new System.Drawing.Point(129, 111);
            this.txtMergeToolPath.Name = "txtMergeToolPath";
            this.txtMergeToolPath.Size = new System.Drawing.Size(1767, 20);
            this.txtMergeToolPath.TabIndex = 4;
            this.txtMergeToolPath.LostFocus += new System.EventHandler(this.txtDiffMergeToolPath_LostFocus);
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalEditor.Location = new System.Drawing.Point(129, 55);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(1767, 21);
            this.GlobalEditor.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 59);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 54;
            this.label6.Text = "Editor";
            // 
            // GlobalUserEmail
            // 
            this.GlobalUserEmail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GlobalUserEmail.Location = new System.Drawing.Point(129, 29);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(241, 20);
            this.GlobalUserEmail.TabIndex = 1;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GlobalUserName.Location = new System.Drawing.Point(129, 3);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(241, 20);
            this.GlobalUserName.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 51;
            this.label4.Text = "User email";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 50;
            this.label3.Text = "User name";
            // 
            // groupBoxLineEndings
            // 
            this.groupBoxLineEndings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLineEndings.AutoSize = true;
            this.tableLayoutPanelGitConfig.SetColumnSpan(this.groupBoxLineEndings, 4);
            this.groupBoxLineEndings.Controls.Add(this.flowLayoutPanelLineEndings);
            this.groupBoxLineEndings.Location = new System.Drawing.Point(3, 291);
            this.groupBoxLineEndings.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.groupBoxLineEndings.Name = "groupBoxLineEndings";
            this.groupBoxLineEndings.Size = new System.Drawing.Size(2136, 111);
            this.groupBoxLineEndings.TabIndex = 16;
            this.groupBoxLineEndings.TabStop = false;
            this.groupBoxLineEndings.Text = "Line endings";
            // 
            // flowLayoutPanelLineEndings
            // 
            this.flowLayoutPanelLineEndings.AutoSize = true;
            this.flowLayoutPanelLineEndings.Controls.Add(this.globalAutoCrlfTrue);
            this.flowLayoutPanelLineEndings.Controls.Add(this.globalAutoCrlfInput);
            this.flowLayoutPanelLineEndings.Controls.Add(this.globalAutoCrlfFalse);
            this.flowLayoutPanelLineEndings.Controls.Add(this.globalAutoCrlfNotSet);
            this.flowLayoutPanelLineEndings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelLineEndings.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelLineEndings.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanelLineEndings.Name = "flowLayoutPanelLineEndings";
            this.flowLayoutPanelLineEndings.Size = new System.Drawing.Size(2130, 92);
            this.flowLayoutPanelLineEndings.TabIndex = 5;
            // 
            // globalAutoCrlfTrue
            // 
            this.globalAutoCrlfTrue.AutoSize = true;
            this.globalAutoCrlfTrue.Location = new System.Drawing.Point(3, 3);
            this.globalAutoCrlfTrue.Name = "globalAutoCrlfTrue";
            this.globalAutoCrlfTrue.Size = new System.Drawing.Size(439, 17);
            this.globalAutoCrlfTrue.TabIndex = 0;
            this.globalAutoCrlfTrue.TabStop = true;
            this.globalAutoCrlfTrue.Text = "Checkout Windows-style, commit Unix-style line endings (\"core.autocrlf\"  is set t" +
    "o \"true\")";
            this.globalAutoCrlfTrue.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfInput
            // 
            this.globalAutoCrlfInput.AutoSize = true;
            this.globalAutoCrlfInput.Location = new System.Drawing.Point(3, 26);
            this.globalAutoCrlfInput.Name = "globalAutoCrlfInput";
            this.globalAutoCrlfInput.Size = new System.Drawing.Size(397, 17);
            this.globalAutoCrlfInput.TabIndex = 1;
            this.globalAutoCrlfInput.TabStop = true;
            this.globalAutoCrlfInput.Text = "Checkout as-is, commit Unix-style line endings (\"core.autocrlf\"  is set to \"input" +
    "\")";
            this.globalAutoCrlfInput.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfFalse
            // 
            this.globalAutoCrlfFalse.AutoSize = true;
            this.globalAutoCrlfFalse.Location = new System.Drawing.Point(3, 49);
            this.globalAutoCrlfFalse.Name = "globalAutoCrlfFalse";
            this.globalAutoCrlfFalse.Size = new System.Drawing.Size(313, 17);
            this.globalAutoCrlfFalse.TabIndex = 2;
            this.globalAutoCrlfFalse.TabStop = true;
            this.globalAutoCrlfFalse.Text = "Checkout as-is, commit as-is (\"core.autocrlf\"  is set to \"false\")";
            this.globalAutoCrlfFalse.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfNotSet
            // 
            this.globalAutoCrlfNotSet.AutoSize = true;
            this.globalAutoCrlfNotSet.Location = new System.Drawing.Point(3, 72);
            this.globalAutoCrlfNotSet.Name = "globalAutoCrlfNotSet";
            this.globalAutoCrlfNotSet.Size = new System.Drawing.Size(59, 17);
            this.globalAutoCrlfNotSet.TabIndex = 4;
            this.globalAutoCrlfNotSet.TabStop = true;
            this.globalAutoCrlfNotSet.Text = "Not set";
            this.globalAutoCrlfNotSet.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelGitConfig
            // 
            this.tableLayoutPanelGitConfig.ColumnCount = 4;
            this.tableLayoutPanelGitConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelGitConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelGitConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelGitConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelGitConfig.Controls.Add(this.ConfigureEncoding, 2, 11);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label60, 0, 11);
            this.tableLayoutPanelGitConfig.Controls.Add(this.groupBoxLineEndings, 0, 10);
            this.tableLayoutPanelGitConfig.Controls.Add(this.GlobalUserName, 1, 0);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanelGitConfig.Controls.Add(this.GlobalUserEmail, 1, 1);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanelGitConfig.Controls.Add(this.txtCommitTemplatePath, 1, 9);
            this.tableLayoutPanelGitConfig.Controls.Add(lblCommitTemplatePath, 0, 9);
            this.tableLayoutPanelGitConfig.Controls.Add(this.GlobalEditor, 1, 2);
            this.tableLayoutPanelGitConfig.Controls.Add(this.InvalidGitPathGlobal, 2, 0);
            this.tableLayoutPanelGitConfig.Controls.Add(lblMergeTool, 0, 3);
            this.tableLayoutPanelGitConfig.Controls.Add(this.txtDiffToolCommand, 1, 8);
            this.tableLayoutPanelGitConfig.Controls.Add(this._NO_TRANSLATE_cboMergeTool, 1, 3);
            this.tableLayoutPanelGitConfig.Controls.Add(lblDiffToolCommand, 0, 8);
            this.tableLayoutPanelGitConfig.Controls.Add(lblMergeToolPath, 0, 4);
            this.tableLayoutPanelGitConfig.Controls.Add(this.txtMergeToolPath, 1, 4);
            this.tableLayoutPanelGitConfig.Controls.Add(this.txtDiffToolPath, 1, 7);
            this.tableLayoutPanelGitConfig.Controls.Add(lblDiffToolPath, 0, 7);
            this.tableLayoutPanelGitConfig.Controls.Add(lblMergeToolCommand, 0, 5);
            this.tableLayoutPanelGitConfig.Controls.Add(this._NO_TRANSLATE_cboDiffTool, 1, 6);
            this.tableLayoutPanelGitConfig.Controls.Add(this.txtMergeToolCommand, 1, 5);
            this.tableLayoutPanelGitConfig.Controls.Add(lblDiffTool, 0, 6);
            this.tableLayoutPanelGitConfig.Controls.Add(this.Global_FilesEncoding, 1, 11);
            this.tableLayoutPanelGitConfig.Controls.Add(this.btnMergeToolBrowse, 2, 4);
            this.tableLayoutPanelGitConfig.Controls.Add(this.btnMergeToolCommandSuggest, 2, 5);
            this.tableLayoutPanelGitConfig.Controls.Add(this.btnDiffToolBrowse, 2, 7);
            this.tableLayoutPanelGitConfig.Controls.Add(this.btnDiffToolCommandSuggest, 2, 8);
            this.tableLayoutPanelGitConfig.Controls.Add(this.btnCommitTemplateBrowse, 2, 9);
            this.tableLayoutPanelGitConfig.Controls.Add(this.lblGPGProgram, 0, 12);
            this.tableLayoutPanelGitConfig.Controls.Add(this.lblSigningKey, 0, 13);
            this.tableLayoutPanelGitConfig.Controls.Add(this.lblCommitSigning, 0, 14);
            this.tableLayoutPanelGitConfig.Controls.Add(this.button1, 2, 12);
            this.tableLayoutPanelGitConfig.Controls.Add(this.chkCommitSigning, 1, 14);
            this.tableLayoutPanelGitConfig.Controls.Add(this.txtGPGProgram, 1, 12);
            this.tableLayoutPanelGitConfig.Controls.Add(this.button2, 2, 13);
            this.tableLayoutPanelGitConfig.Controls.Add(this.gpgSecretKeysComboboxUserKeys, 1, 13);
            this.tableLayoutPanelGitConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelGitConfig.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelGitConfig.Name = "tableLayoutPanelGitConfig";
            this.tableLayoutPanelGitConfig.RowCount = 16;
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelGitConfig.Size = new System.Drawing.Size(2142, 778);
            this.tableLayoutPanelGitConfig.TabIndex = 81;
            // 
            // ConfigureEncoding
            // 
            this.ConfigureEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfigureEncoding.Location = new System.Drawing.Point(1902, 411);
            this.ConfigureEncoding.Name = "ConfigureEncoding";
            this.ConfigureEncoding.Size = new System.Drawing.Size(123, 25);
            this.ConfigureEncoding.TabIndex = 81;
            this.ConfigureEncoding.Text = "Configure";
            this.ConfigureEncoding.UseVisualStyleBackColor = true;
            this.ConfigureEncoding.Click += new System.EventHandler(this.ConfigureEncoding_Click);
            // 
            // lblGPGProgram
            // 
            this.lblGPGProgram.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblGPGProgram.AutoSize = true;
            this.lblGPGProgram.Location = new System.Drawing.Point(3, 448);
            this.lblGPGProgram.Name = "lblGPGProgram";
            this.lblGPGProgram.Size = new System.Drawing.Size(97, 13);
            this.lblGPGProgram.TabIndex = 79;
            this.lblGPGProgram.Text = "GPG Program Path";
            // 
            // lblSigningKey
            // 
            this.lblSigningKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSigningKey.AutoSize = true;
            this.lblSigningKey.Location = new System.Drawing.Point(3, 488);
            this.lblSigningKey.Name = "lblSigningKey";
            this.lblSigningKey.Size = new System.Drawing.Size(88, 13);
            this.lblSigningKey.TabIndex = 79;
            this.lblSigningKey.Text = "User Signing Key";
            // 
            // lblCommitSigning
            // 
            this.lblCommitSigning.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCommitSigning.AutoSize = true;
            this.lblCommitSigning.Location = new System.Drawing.Point(3, 525);
            this.lblCommitSigning.Name = "lblCommitSigning";
            this.lblCommitSigning.Size = new System.Drawing.Size(79, 13);
            this.lblCommitSigning.TabIndex = 79;
            this.lblCommitSigning.Text = "Commit Signing";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(1902, 442);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 25);
            this.button1.TabIndex = 5;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnGPGProgramBrowse_Click);
            // 
            // chkCommitSigning
            // 
            this.chkCommitSigning.AutoSize = true;
            this.chkCommitSigning.Location = new System.Drawing.Point(129, 523);
            this.chkCommitSigning.Name = "chkCommitSigning";
            this.chkCommitSigning.Size = new System.Drawing.Size(158, 17);
            this.chkCommitSigning.TabIndex = 82;
            this.chkCommitSigning.Text = "Sign commits automatically?";
            this.chkCommitSigning.UseVisualStyleBackColor = true;
            // 
            // txtGPGProgram
            // 
            this.txtGPGProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGPGProgram.Location = new System.Drawing.Point(129, 444);
            this.txtGPGProgram.Name = "txtGPGProgram";
            this.txtGPGProgram.Size = new System.Drawing.Size(1767, 20);
            this.txtGPGProgram.TabIndex = 4;
            this.txtGPGProgram.LostFocus += new System.EventHandler(this.txtDiffMergeToolPath_LostFocus);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1902, 473);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 83;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // gpgSecretKeysComboboxUserKeys
            // 
            this.gpgSecretKeysComboboxUserKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpgSecretKeysComboboxUserKeys.Location = new System.Drawing.Point(129, 473);
            this.gpgSecretKeysComboboxUserKeys.MinimumSize = new System.Drawing.Size(20, 20);
            this.gpgSecretKeysComboboxUserKeys.Name = "gpgSecretKeysComboboxUserKeys";
            this.gpgSecretKeysComboboxUserKeys.Size = new System.Drawing.Size(1767, 44);
            this.gpgSecretKeysComboboxUserKeys.TabIndex = 84;
            // 
            // GitConfigSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanelGitConfig);
            this.Name = "GitConfigSettingsPage";
            this.Size = new System.Drawing.Size(2142, 868);
            this.InvalidGitPathGlobal.ResumeLayout(false);
            this.InvalidGitPathGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxLineEndings.ResumeLayout(false);
            this.groupBoxLineEndings.PerformLayout();
            this.flowLayoutPanelLineEndings.ResumeLayout(false);
            this.flowLayoutPanelLineEndings.PerformLayout();
            this.tableLayoutPanelGitConfig.ResumeLayout(false);
            this.tableLayoutPanelGitConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label60;
        private System.Windows.Forms.ComboBox Global_FilesEncoding;
        private System.Windows.Forms.Button btnCommitTemplateBrowse;
        private System.Windows.Forms.TextBox txtCommitTemplatePath;
        private System.Windows.Forms.Button btnDiffToolCommandSuggest;
        private System.Windows.Forms.TextBox txtDiffToolCommand;
        private System.Windows.Forms.Button btnDiffToolBrowse;
        private System.Windows.Forms.TextBox txtDiffToolPath;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cboDiffTool;
        private System.Windows.Forms.Panel InvalidGitPathGlobal;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnMergeToolCommandSuggest;
        private System.Windows.Forms.TextBox txtMergeToolCommand;
        private System.Windows.Forms.Button btnMergeToolBrowse;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cboMergeTool;
        private System.Windows.Forms.TextBox txtMergeToolPath;
        private System.Windows.Forms.ComboBox GlobalEditor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox GlobalUserEmail;
        private System.Windows.Forms.TextBox GlobalUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBoxLineEndings;
        private System.Windows.Forms.RadioButton globalAutoCrlfFalse;
        private System.Windows.Forms.RadioButton globalAutoCrlfInput;
        private System.Windows.Forms.RadioButton globalAutoCrlfTrue;
        private System.Windows.Forms.RadioButton globalAutoCrlfNotSet;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelGitConfig;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLineEndings;
        private System.Windows.Forms.Button ConfigureEncoding;
        private System.Windows.Forms.Label lblGPGProgram;
        private System.Windows.Forms.Label lblSigningKey;
        private System.Windows.Forms.Label lblCommitSigning;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chkCommitSigning;
        private System.Windows.Forms.TextBox txtGPGProgram;
        private System.Windows.Forms.Button button2;
        private UserControls.GPGSecretKeysComboboxControl gpgSecretKeysComboboxUserKeys;
    }
}
