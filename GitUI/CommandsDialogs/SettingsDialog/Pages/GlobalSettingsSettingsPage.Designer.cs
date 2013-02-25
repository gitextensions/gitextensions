namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GlobalSettingsSettingsPage
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
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.globalAutoCrlfFalse = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfInput = new System.Windows.Forms.RadioButton();
            this.globalAutoCrlfTrue = new System.Windows.Forms.RadioButton();
            this.InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox9.SuspendLayout();
            this.SuspendLayout();
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Location = new System.Drawing.Point(3, 434);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(127, 15);
            this.label60.TabIndex = 79;
            this.label60.Text = "Files content encoding";
            // 
            // Global_FilesEncoding
            // 
            this.Global_FilesEncoding.FormattingEnabled = true;
            this.Global_FilesEncoding.Location = new System.Drawing.Point(167, 432);
            this.Global_FilesEncoding.Name = "Global_FilesEncoding";
            this.Global_FilesEncoding.Size = new System.Drawing.Size(231, 23);
            this.Global_FilesEncoding.TabIndex = 78;
            // 
            // BrowseCommitTemplate
            // 
            this.BrowseCommitTemplate.Location = new System.Drawing.Point(501, 283);
            this.BrowseCommitTemplate.Name = "BrowseCommitTemplate";
            this.BrowseCommitTemplate.Size = new System.Drawing.Size(108, 25);
            this.BrowseCommitTemplate.TabIndex = 77;
            this.BrowseCommitTemplate.Text = "Browse";
            this.BrowseCommitTemplate.UseVisualStyleBackColor = true;
            this.BrowseCommitTemplate.Click += new System.EventHandler(this.BrowseCommitTemplate_Click);
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(4, 288);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(140, 15);
            this.label57.TabIndex = 76;
            this.label57.Text = "Path to commit template";
            // 
            // CommitTemplatePath
            // 
            this.CommitTemplatePath.Location = new System.Drawing.Point(198, 284);
            this.CommitTemplatePath.Name = "CommitTemplatePath";
            this.CommitTemplatePath.Size = new System.Drawing.Size(297, 23);
            this.CommitTemplatePath.TabIndex = 75;
            // 
            // DiffToolCmdSuggest
            // 
            this.DiffToolCmdSuggest.Location = new System.Drawing.Point(501, 255);
            this.DiffToolCmdSuggest.Name = "DiffToolCmdSuggest";
            this.DiffToolCmdSuggest.Size = new System.Drawing.Size(108, 25);
            this.DiffToolCmdSuggest.TabIndex = 74;
            this.DiffToolCmdSuggest.Text = "Suggest command";
            this.DiffToolCmdSuggest.UseVisualStyleBackColor = true;
            this.DiffToolCmdSuggest.Click += new System.EventHandler(this.DiffToolCmdSuggest_Click);
            // 
            // DifftoolCmd
            // 
            this.DifftoolCmd.FormattingEnabled = true;
            this.DifftoolCmd.Location = new System.Drawing.Point(157, 257);
            this.DifftoolCmd.Name = "DifftoolCmd";
            this.DifftoolCmd.Size = new System.Drawing.Size(338, 23);
            this.DifftoolCmd.TabIndex = 73;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(4, 261);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(105, 15);
            this.label48.TabIndex = 72;
            this.label48.Text = "Difftool command";
            // 
            // BrowseDiffTool
            // 
            this.BrowseDiffTool.Location = new System.Drawing.Point(501, 227);
            this.BrowseDiffTool.Name = "BrowseDiffTool";
            this.BrowseDiffTool.Size = new System.Drawing.Size(108, 25);
            this.BrowseDiffTool.TabIndex = 71;
            this.BrowseDiffTool.Text = "Browse";
            this.BrowseDiffTool.UseVisualStyleBackColor = true;
            this.BrowseDiffTool.Click += new System.EventHandler(this.BrowseDiffTool_Click);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(4, 234);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(87, 15);
            this.label42.TabIndex = 70;
            this.label42.Text = "Path to difftool";
            // 
            // DifftoolPath
            // 
            this.DifftoolPath.Location = new System.Drawing.Point(157, 230);
            this.DifftoolPath.Name = "DifftoolPath";
            this.DifftoolPath.Size = new System.Drawing.Size(338, 23);
            this.DifftoolPath.TabIndex = 69;
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
            this.GlobalDiffTool.Location = new System.Drawing.Point(157, 203);
            this.GlobalDiffTool.Name = "GlobalDiffTool";
            this.GlobalDiffTool.Size = new System.Drawing.Size(155, 23);
            this.GlobalDiffTool.TabIndex = 68;
            this.GlobalDiffTool.TextChanged += new System.EventHandler(this.GlobalDiffTool_TextChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(5, 206);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(47, 15);
            this.label41.TabIndex = 67;
            this.label41.Text = "Difftool";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(5, 177);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(174, 15);
            this.label28.TabIndex = 66;
            this.label28.Text = "Keep backup (.orig) after merge";
            // 
            // InvalidGitPathGlobal
            // 
            this.InvalidGitPathGlobal.BackColor = System.Drawing.SystemColors.Info;
            this.InvalidGitPathGlobal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InvalidGitPathGlobal.Controls.Add(this.label9);
            this.InvalidGitPathGlobal.Controls.Add(this.pictureBox1);
            this.InvalidGitPathGlobal.Location = new System.Drawing.Point(433, 6);
            this.InvalidGitPathGlobal.Name = "InvalidGitPathGlobal";
            this.InvalidGitPathGlobal.Size = new System.Drawing.Size(263, 59);
            this.InvalidGitPathGlobal.TabIndex = 65;
            this.InvalidGitPathGlobal.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(63, 6);
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
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // MergeToolCmdSuggest
            // 
            this.MergeToolCmdSuggest.Location = new System.Drawing.Point(501, 143);
            this.MergeToolCmdSuggest.Name = "MergeToolCmdSuggest";
            this.MergeToolCmdSuggest.Size = new System.Drawing.Size(108, 25);
            this.MergeToolCmdSuggest.TabIndex = 64;
            this.MergeToolCmdSuggest.Text = "Suggest command";
            this.MergeToolCmdSuggest.UseVisualStyleBackColor = true;
            this.MergeToolCmdSuggest.Click += new System.EventHandler(this.MergeToolCmdSuggest_Click);
            // 
            // MergeToolCmd
            // 
            this.MergeToolCmd.FormattingEnabled = true;
            this.MergeToolCmd.Location = new System.Drawing.Point(157, 145);
            this.MergeToolCmd.Name = "MergeToolCmd";
            this.MergeToolCmd.Size = new System.Drawing.Size(338, 23);
            this.MergeToolCmd.TabIndex = 63;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(4, 149);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(120, 15);
            this.label19.TabIndex = 62;
            this.label19.Text = "Mergetool command";
            // 
            // BrowseMergeTool
            // 
            this.BrowseMergeTool.Location = new System.Drawing.Point(501, 115);
            this.BrowseMergeTool.Name = "BrowseMergeTool";
            this.BrowseMergeTool.Size = new System.Drawing.Size(108, 25);
            this.BrowseMergeTool.TabIndex = 61;
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
            this.GlobalMergeTool.Location = new System.Drawing.Point(157, 90);
            this.GlobalMergeTool.Name = "GlobalMergeTool";
            this.GlobalMergeTool.Size = new System.Drawing.Size(155, 23);
            this.GlobalMergeTool.TabIndex = 60;
            this.GlobalMergeTool.TextChanged += new System.EventHandler(this.GlobalMergeTool_TextChanged);
            // 
            // PathToKDiff3
            // 
            this.PathToKDiff3.AutoSize = true;
            this.PathToKDiff3.Location = new System.Drawing.Point(4, 122);
            this.PathToKDiff3.Name = "PathToKDiff3";
            this.PathToKDiff3.Size = new System.Drawing.Size(103, 15);
            this.PathToKDiff3.TabIndex = 59;
            this.PathToKDiff3.Text = "Path to mergetool";
            // 
            // MergetoolPath
            // 
            this.MergetoolPath.Location = new System.Drawing.Point(157, 118);
            this.MergetoolPath.Name = "MergetoolPath";
            this.MergetoolPath.Size = new System.Drawing.Size(338, 23);
            this.MergetoolPath.TabIndex = 58;
            // 
            // GlobalKeepMergeBackup
            // 
            this.GlobalKeepMergeBackup.AutoSize = true;
            this.GlobalKeepMergeBackup.Checked = true;
            this.GlobalKeepMergeBackup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.GlobalKeepMergeBackup.Location = new System.Drawing.Point(501, 177);
            this.GlobalKeepMergeBackup.Name = "GlobalKeepMergeBackup";
            this.GlobalKeepMergeBackup.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.GlobalKeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.GlobalKeepMergeBackup.TabIndex = 57;
            this.GlobalKeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 93);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 15);
            this.label7.TabIndex = 56;
            this.label7.Text = "Mergetool";
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Location = new System.Drawing.Point(157, 63);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(260, 23);
            this.GlobalEditor.TabIndex = 55;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 15);
            this.label6.TabIndex = 54;
            this.label6.Text = "Editor";
            // 
            // GlobalUserEmail
            // 
            this.GlobalUserEmail.Location = new System.Drawing.Point(157, 35);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(241, 23);
            this.GlobalUserEmail.TabIndex = 53;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(157, 6);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(241, 23);
            this.GlobalUserName.TabIndex = 52;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 15);
            this.label4.TabIndex = 51;
            this.label4.Text = "User email";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 50;
            this.label3.Text = "User name";
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox9.Controls.Add(this.globalAutoCrlfFalse);
            this.groupBox9.Controls.Add(this.globalAutoCrlfInput);
            this.groupBox9.Controls.Add(this.globalAutoCrlfTrue);
            this.groupBox9.Location = new System.Drawing.Point(7, 316);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(679, 105);
            this.groupBox9.TabIndex = 80;
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
            // GlobalSettingsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox9);
            this.Controls.Add(this.label60);
            this.Controls.Add(this.Global_FilesEncoding);
            this.Controls.Add(this.BrowseCommitTemplate);
            this.Controls.Add(this.label57);
            this.Controls.Add(this.CommitTemplatePath);
            this.Controls.Add(this.DiffToolCmdSuggest);
            this.Controls.Add(this.DifftoolCmd);
            this.Controls.Add(this.label48);
            this.Controls.Add(this.BrowseDiffTool);
            this.Controls.Add(this.label42);
            this.Controls.Add(this.DifftoolPath);
            this.Controls.Add(this.GlobalDiffTool);
            this.Controls.Add(this.label41);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.InvalidGitPathGlobal);
            this.Controls.Add(this.MergeToolCmdSuggest);
            this.Controls.Add(this.MergeToolCmd);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.BrowseMergeTool);
            this.Controls.Add(this.GlobalMergeTool);
            this.Controls.Add(this.PathToKDiff3);
            this.Controls.Add(this.MergetoolPath);
            this.Controls.Add(this.GlobalKeepMergeBackup);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.GlobalEditor);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.GlobalUserEmail);
            this.Controls.Add(this.GlobalUserName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Name = "GlobalSettingsSettingsPage";
            this.Size = new System.Drawing.Size(704, 474);
            this.InvalidGitPathGlobal.ResumeLayout(false);
            this.InvalidGitPathGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label60;
        private System.Windows.Forms.ComboBox Global_FilesEncoding;
        private System.Windows.Forms.Button BrowseCommitTemplate;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.TextBox CommitTemplatePath;
        private System.Windows.Forms.Button DiffToolCmdSuggest;
        private System.Windows.Forms.ComboBox DifftoolCmd;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Button BrowseDiffTool;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox DifftoolPath;
        private System.Windows.Forms.ComboBox GlobalDiffTool;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Panel InvalidGitPathGlobal;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button MergeToolCmdSuggest;
        private System.Windows.Forms.ComboBox MergeToolCmd;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button BrowseMergeTool;
        private System.Windows.Forms.ComboBox GlobalMergeTool;
        private System.Windows.Forms.Label PathToKDiff3;
        private System.Windows.Forms.TextBox MergetoolPath;
        private System.Windows.Forms.CheckBox GlobalKeepMergeBackup;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox GlobalEditor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox GlobalUserEmail;
        private System.Windows.Forms.TextBox GlobalUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.RadioButton globalAutoCrlfFalse;
        private System.Windows.Forms.RadioButton globalAutoCrlfInput;
        private System.Windows.Forms.RadioButton globalAutoCrlfTrue;
    }
}
