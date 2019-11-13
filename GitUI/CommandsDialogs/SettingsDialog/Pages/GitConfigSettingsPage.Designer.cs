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
            this.label60 = new System.Windows.Forms.Label();
            this.Global_FilesEncoding = new System.Windows.Forms.ComboBox();
            this.BrowseCommitTemplate = new System.Windows.Forms.Button();
            this.label57 = new System.Windows.Forms.Label();
            this.CommitTemplatePath = new System.Windows.Forms.TextBox();
            this.DiffToolCmdSuggest = new System.Windows.Forms.Button();
            this.DifftoolCmd = new System.Windows.Forms.TextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.BrowseDiffTool = new System.Windows.Forms.Button();
            this.label42 = new System.Windows.Forms.Label();
            this.DifftoolPath = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_GlobalDiffTool = new System.Windows.Forms.ComboBox();
            this.label41 = new System.Windows.Forms.Label();
            this.InvalidGitPathGlobal = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.MergeToolCmdSuggest = new System.Windows.Forms.Button();
            this.MergeToolCmd = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.BrowseMergeTool = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_GlobalMergeTool = new System.Windows.Forms.ComboBox();
            this.MergeToolPathLabel = new System.Windows.Forms.Label();
            this.MergetoolPath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
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
            this.InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxLineEndings.SuspendLayout();
            this.flowLayoutPanelLineEndings.SuspendLayout();
            this.tableLayoutPanelGitConfig.SuspendLayout();
            this.SuspendLayout();
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
            // BrowseCommitTemplate
            // 
            this.BrowseCommitTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseCommitTemplate.Location = new System.Drawing.Point(1037, 260);
            this.BrowseCommitTemplate.Name = "BrowseCommitTemplate";
            this.BrowseCommitTemplate.Size = new System.Drawing.Size(123, 25);
            this.BrowseCommitTemplate.TabIndex = 15;
            this.BrowseCommitTemplate.Text = "Browse";
            this.BrowseCommitTemplate.UseVisualStyleBackColor = true;
            this.BrowseCommitTemplate.Click += new System.EventHandler(this.BrowseCommitTemplate_Click);
            // 
            // label57
            // 
            this.label57.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(3, 266);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(120, 13);
            this.label57.TabIndex = 76;
            this.label57.Text = "Path to commit template";
            // 
            // CommitTemplatePath
            // 
            this.CommitTemplatePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.CommitTemplatePath.Location = new System.Drawing.Point(129, 262);
            this.CommitTemplatePath.Name = "CommitTemplatePath";
            this.CommitTemplatePath.Size = new System.Drawing.Size(902, 20);
            this.CommitTemplatePath.TabIndex = 14;
            // 
            // DiffToolCmdSuggest
            // 
            this.DiffToolCmdSuggest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.DiffToolCmdSuggest.Location = new System.Drawing.Point(1037, 229);
            this.DiffToolCmdSuggest.Name = "DiffToolCmdSuggest";
            this.DiffToolCmdSuggest.Size = new System.Drawing.Size(123, 25);
            this.DiffToolCmdSuggest.TabIndex = 13;
            this.DiffToolCmdSuggest.Text = "Suggest";
            this.DiffToolCmdSuggest.UseVisualStyleBackColor = true;
            this.DiffToolCmdSuggest.Click += new System.EventHandler(this.DiffToolCmdSuggest_Click);
            // 
            // DifftoolCmd
            // 
            this.DifftoolCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.DifftoolCmd.Location = new System.Drawing.Point(129, 231);
            this.DifftoolCmd.Name = "DifftoolCmd";
            this.DifftoolCmd.Size = new System.Drawing.Size(902, 20);
            this.DifftoolCmd.TabIndex = 12;
            // 
            // label48
            // 
            this.label48.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(3, 235);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(89, 13);
            this.label48.TabIndex = 72;
            this.label48.Text = "Difftool command";
            // 
            // BrowseDiffTool
            // 
            this.BrowseDiffTool.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseDiffTool.Location = new System.Drawing.Point(1037, 198);
            this.BrowseDiffTool.Name = "BrowseDiffTool";
            this.BrowseDiffTool.Size = new System.Drawing.Size(123, 25);
            this.BrowseDiffTool.TabIndex = 11;
            this.BrowseDiffTool.Text = "Browse";
            this.BrowseDiffTool.UseVisualStyleBackColor = true;
            this.BrowseDiffTool.Click += new System.EventHandler(this.BrowseDiffTool_Click);
            // 
            // label42
            // 
            this.label42.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(3, 204);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(75, 13);
            this.label42.TabIndex = 70;
            this.label42.Text = "Path to difftool";
            // 
            // DifftoolPath
            // 
            this.DifftoolPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.DifftoolPath.Location = new System.Drawing.Point(129, 200);
            this.DifftoolPath.Name = "DifftoolPath";
            this.DifftoolPath.Size = new System.Drawing.Size(902, 20);
            this.DifftoolPath.TabIndex = 10;
            this.DifftoolPath.LostFocus += new System.EventHandler(this.DiffMergeToolPath_LostFocus);
            // 
            // _NO_TRANSLATE_GlobalDiffTool
            // 
            this._NO_TRANSLATE_GlobalDiffTool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_GlobalDiffTool.FormattingEnabled = true;
            this._NO_TRANSLATE_GlobalDiffTool.Location = new System.Drawing.Point(129, 171);
            this._NO_TRANSLATE_GlobalDiffTool.Name = "_NO_TRANSLATE_GlobalDiffTool";
            this._NO_TRANSLATE_GlobalDiffTool.Size = new System.Drawing.Size(241, 21);
            this._NO_TRANSLATE_GlobalDiffTool.TabIndex = 9;
            this._NO_TRANSLATE_GlobalDiffTool.TextChanged += new System.EventHandler(this.GlobalDiffTool_TextChanged);
            // 
            // label41
            // 
            this.label41.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(3, 175);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(40, 13);
            this.label41.TabIndex = 67;
            this.label41.Text = "Difftool";
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
            this.InvalidGitPathGlobal.Location = new System.Drawing.Point(1037, 29);
            this.InvalidGitPathGlobal.Name = "InvalidGitPathGlobal";
            this.InvalidGitPathGlobal.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
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
            // MergeToolCmdSuggest
            // 
            this.MergeToolCmdSuggest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.MergeToolCmdSuggest.Location = new System.Drawing.Point(1037, 140);
            this.MergeToolCmdSuggest.Name = "MergeToolCmdSuggest";
            this.MergeToolCmdSuggest.Size = new System.Drawing.Size(123, 25);
            this.MergeToolCmdSuggest.TabIndex = 7;
            this.MergeToolCmdSuggest.Text = "Suggest";
            this.MergeToolCmdSuggest.UseVisualStyleBackColor = true;
            this.MergeToolCmdSuggest.Click += new System.EventHandler(this.MergeToolCmdSuggest_Click);
            // 
            // MergeToolCmd
            // 
            this.MergeToolCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.MergeToolCmd.Location = new System.Drawing.Point(129, 142);
            this.MergeToolCmd.Name = "MergeToolCmd";
            this.MergeToolCmd.Size = new System.Drawing.Size(902, 20);
            this.MergeToolCmd.TabIndex = 6;
            // 
            // label19
            // 
            this.label19.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(3, 146);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(103, 13);
            this.label19.TabIndex = 62;
            this.label19.Text = "Mergetool command";
            // 
            // BrowseMergeTool
            // 
            this.BrowseMergeTool.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseMergeTool.Location = new System.Drawing.Point(1037, 109);
            this.BrowseMergeTool.Name = "BrowseMergeTool";
            this.BrowseMergeTool.Size = new System.Drawing.Size(123, 25);
            this.BrowseMergeTool.TabIndex = 5;
            this.BrowseMergeTool.Text = "Browse";
            this.BrowseMergeTool.UseVisualStyleBackColor = true;
            this.BrowseMergeTool.Click += new System.EventHandler(this.BrowseMergeTool_Click);
            // 
            // _NO_TRANSLATE_GlobalMergeTool
            // 
            this._NO_TRANSLATE_GlobalMergeTool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_GlobalMergeTool.FormattingEnabled = true;
            this._NO_TRANSLATE_GlobalMergeTool.Location = new System.Drawing.Point(129, 82);
            this._NO_TRANSLATE_GlobalMergeTool.Name = "_NO_TRANSLATE_GlobalMergeTool";
            this._NO_TRANSLATE_GlobalMergeTool.Size = new System.Drawing.Size(241, 21);
            this._NO_TRANSLATE_GlobalMergeTool.TabIndex = 3;
            this._NO_TRANSLATE_GlobalMergeTool.TextChanged += new System.EventHandler(this.GlobalMergeTool_TextChanged);
            // 
            // MergeToolPathLabel
            // 
            this.MergeToolPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.MergeToolPathLabel.AutoSize = true;
            this.MergeToolPathLabel.Location = new System.Drawing.Point(3, 115);
            this.MergeToolPathLabel.Name = "MergeToolPathLabel";
            this.MergeToolPathLabel.Size = new System.Drawing.Size(90, 13);
            this.MergeToolPathLabel.TabIndex = 59;
            this.MergeToolPathLabel.Text = "Path to mergetool";
            // 
            // MergetoolPath
            // 
            this.MergetoolPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.MergetoolPath.Location = new System.Drawing.Point(129, 111);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(902, 20);
            this.MergetoolPath.TabIndex = 4;
            this.MergetoolPath.LostFocus += new System.EventHandler(this.DiffMergeToolPath_LostFocus);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 86);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 13);
            this.label7.TabIndex = 56;
            this.label7.Text = "Mergetool";
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalEditor.Location = new System.Drawing.Point(129, 55);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(902, 21);
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
            this.groupBoxLineEndings.Size = new System.Drawing.Size(1271, 111);
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
            this.flowLayoutPanelLineEndings.Size = new System.Drawing.Size(1265, 92);
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
            this.tableLayoutPanelGitConfig.Controls.Add(this.CommitTemplatePath, 1, 9);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label57, 0, 9);
            this.tableLayoutPanelGitConfig.Controls.Add(this.GlobalEditor, 1, 2);
            this.tableLayoutPanelGitConfig.Controls.Add(this.InvalidGitPathGlobal, 2, 0);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label7, 0, 3);
            this.tableLayoutPanelGitConfig.Controls.Add(this.DifftoolCmd, 1, 8);
            this.tableLayoutPanelGitConfig.Controls.Add(this._NO_TRANSLATE_GlobalMergeTool, 1, 3);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label48, 0, 8);
            this.tableLayoutPanelGitConfig.Controls.Add(this.MergeToolPathLabel, 0, 4);
            this.tableLayoutPanelGitConfig.Controls.Add(this.MergetoolPath, 1, 4);
            this.tableLayoutPanelGitConfig.Controls.Add(this.DifftoolPath, 1, 7);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label42, 0, 7);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label19, 0, 5);
            this.tableLayoutPanelGitConfig.Controls.Add(this._NO_TRANSLATE_GlobalDiffTool, 1, 6);
            this.tableLayoutPanelGitConfig.Controls.Add(this.MergeToolCmd, 1, 5);
            this.tableLayoutPanelGitConfig.Controls.Add(this.label41, 0, 6);
            this.tableLayoutPanelGitConfig.Controls.Add(this.Global_FilesEncoding, 1, 11);
            this.tableLayoutPanelGitConfig.Controls.Add(this.BrowseMergeTool, 2, 4);
            this.tableLayoutPanelGitConfig.Controls.Add(this.MergeToolCmdSuggest, 2, 5);
            this.tableLayoutPanelGitConfig.Controls.Add(this.BrowseDiffTool, 2, 7);
            this.tableLayoutPanelGitConfig.Controls.Add(this.DiffToolCmdSuggest, 2, 8);
            this.tableLayoutPanelGitConfig.Controls.Add(this.BrowseCommitTemplate, 2, 9);
            this.tableLayoutPanelGitConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelGitConfig.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelGitConfig.Name = "tableLayoutPanelGitConfig";
            this.tableLayoutPanelGitConfig.RowCount = 13;
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
            this.tableLayoutPanelGitConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelGitConfig.Size = new System.Drawing.Size(1277, 778);
            this.tableLayoutPanelGitConfig.TabIndex = 81;
            // 
            // ConfigureEncoding
            // 
            this.ConfigureEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfigureEncoding.Location = new System.Drawing.Point(1037, 411);
            this.ConfigureEncoding.Name = "ConfigureEncoding";
            this.ConfigureEncoding.Size = new System.Drawing.Size(123, 25);
            this.ConfigureEncoding.TabIndex = 81;
            this.ConfigureEncoding.Text = "Configure";
            this.ConfigureEncoding.UseVisualStyleBackColor = true;
            this.ConfigureEncoding.Click += new System.EventHandler(this.ConfigureEncoding_Click);
            // 
            // GitConfigSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanelGitConfig);
            this.Name = "GitConfigSettingsPage";
            this.Size = new System.Drawing.Size(1277, 715);
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
        private System.Windows.Forms.Button BrowseCommitTemplate;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.TextBox CommitTemplatePath;
        private System.Windows.Forms.Button DiffToolCmdSuggest;
        private System.Windows.Forms.TextBox DifftoolCmd;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Button BrowseDiffTool;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox DifftoolPath;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_GlobalDiffTool;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Panel InvalidGitPathGlobal;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button MergeToolCmdSuggest;
        private System.Windows.Forms.TextBox MergeToolCmd;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button BrowseMergeTool;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_GlobalMergeTool;
        private System.Windows.Forms.Label MergeToolPathLabel;
        private System.Windows.Forms.TextBox MergetoolPath;
        private System.Windows.Forms.Label label7;
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
    }
}
