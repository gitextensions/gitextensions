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
            if (disposing && (components is not null))
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
            label60 = new Label();
            Global_FilesEncoding = new ComboBox();
            btnCommitTemplateBrowse = new Button();
            txtCommitTemplatePath = new TextBox();
            btnDiffToolCommandSuggest = new Button();
            txtDiffToolCommand = new TextBox();
            btnDiffToolBrowse = new Button();
            txtDiffToolPath = new TextBox();
            _NO_TRANSLATE_cboDiffTool = new ComboBox();
            InvalidGitPathGlobal = new Panel();
            label9 = new Label();
            pictureBox1 = new PictureBox();
            btnMergeToolCommandSuggest = new Button();
            txtMergeToolCommand = new TextBox();
            btnMergeToolBrowse = new Button();
            _NO_TRANSLATE_cboMergeTool = new ComboBox();
            txtMergeToolPath = new TextBox();
            GlobalEditor = new ComboBox();
            label6 = new Label();
            GlobalUserEmail = new TextBox();
            GlobalUserName = new TextBox();
            label4 = new Label();
            label3 = new Label();
            groupBoxLineEndings = new GroupBox();
            flowLayoutPanelLineEndings = new FlowLayoutPanel();
            globalAutoCrlfTrue = new RadioButton();
            globalAutoCrlfInput = new RadioButton();
            globalAutoCrlfFalse = new RadioButton();
            globalAutoCrlfNotSet = new RadioButton();
            tableLayoutPanelGitConfig = new TableLayoutPanel();
            ConfigureEncoding = new Button();
            lblCommitTemplatePath = new Label();
            lblDiffToolCommand = new Label();
            lblDiffToolPath = new Label();
            lblDiffTool = new Label();
            lblMergeToolCommand = new Label();
            lblMergeToolPath = new Label();
            lblMergeTool = new Label();
            InvalidGitPathGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            groupBoxLineEndings.SuspendLayout();
            flowLayoutPanelLineEndings.SuspendLayout();
            tableLayoutPanelGitConfig.SuspendLayout();
            SuspendLayout();
            // 
            // label60
            // 
            label60.Anchor = AnchorStyles.Left;
            label60.AutoSize = true;
            label60.Location = new Point(3, 417);
            label60.Name = "label60";
            label60.Size = new Size(114, 13);
            label60.Text = "Files content encoding";
            // 
            // Global_FilesEncoding
            // 
            Global_FilesEncoding.Anchor = AnchorStyles.Left;
            Global_FilesEncoding.FormattingEnabled = true;
            Global_FilesEncoding.Location = new Point(129, 413);
            Global_FilesEncoding.Name = "Global_FilesEncoding";
            Global_FilesEncoding.Size = new Size(241, 21);
            // 
            // btnCommitTemplateBrowse
            // 
            btnCommitTemplateBrowse.AccessibleName = "Browse Path to commit template";
            btnCommitTemplateBrowse.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnCommitTemplateBrowse.Location = new Point(1037, 260);
            btnCommitTemplateBrowse.Name = "btnCommitTemplateBrowse";
            btnCommitTemplateBrowse.Size = new Size(123, 25);
            btnCommitTemplateBrowse.Text = "Browse";
            btnCommitTemplateBrowse.UseVisualStyleBackColor = true;
            btnCommitTemplateBrowse.Click += btnCommitTemplateBrowse_Click;
            // 
            // lblCommitTemplatePath
            // 
            lblCommitTemplatePath.Anchor = AnchorStyles.Left;
            lblCommitTemplatePath.AutoSize = true;
            lblCommitTemplatePath.Location = new Point(3, 266);
            lblCommitTemplatePath.Name = "lblCommitTemplatePath";
            lblCommitTemplatePath.Size = new Size(120, 13);
            lblCommitTemplatePath.Text = "Path to commit template";
            // 
            // txtCommitTemplatePath
            // 
            txtCommitTemplatePath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCommitTemplatePath.Location = new Point(129, 262);
            txtCommitTemplatePath.Name = "txtCommitTemplatePath";
            txtCommitTemplatePath.Size = new Size(902, 20);
            // 
            // btnDiffToolCommandSuggest
            // 
            btnDiffToolCommandSuggest.AccessibleName = "Suggest Difftool command";
            btnDiffToolCommandSuggest.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnDiffToolCommandSuggest.Location = new Point(1037, 229);
            btnDiffToolCommandSuggest.Name = "btnDiffToolCommandSuggest";
            btnDiffToolCommandSuggest.Size = new Size(123, 25);
            btnDiffToolCommandSuggest.Text = "Suggest";
            btnDiffToolCommandSuggest.UseVisualStyleBackColor = true;
            btnDiffToolCommandSuggest.Click += btnDiffToolCommandSuggest_Click;
            // 
            // txtDiffToolCommand
            // 
            txtDiffToolCommand.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtDiffToolCommand.Location = new Point(129, 231);
            txtDiffToolCommand.Name = "txtDiffToolCommand";
            txtDiffToolCommand.Size = new Size(902, 20);
            // 
            // lblDiffToolCommand
            // 
            lblDiffToolCommand.Anchor = AnchorStyles.Left;
            lblDiffToolCommand.AutoSize = true;
            lblDiffToolCommand.Location = new Point(3, 235);
            lblDiffToolCommand.Name = "lblDiffToolCommand";
            lblDiffToolCommand.Size = new Size(89, 13);
            lblDiffToolCommand.Text = "Difftool command";
            // 
            // btnDiffToolBrowse
            // 
            btnDiffToolBrowse.AccessibleName = "Browse Difftool command";
            btnDiffToolBrowse.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnDiffToolBrowse.Location = new Point(1037, 198);
            btnDiffToolBrowse.Name = "btnDiffToolBrowse";
            btnDiffToolBrowse.Size = new Size(123, 25);
            btnDiffToolBrowse.Text = "Browse";
            btnDiffToolBrowse.UseVisualStyleBackColor = true;
            btnDiffToolBrowse.Click += btnDiffToolBrowse_Click;
            // 
            // lblDiffToolPath
            // 
            lblDiffToolPath.Anchor = AnchorStyles.Left;
            lblDiffToolPath.AutoSize = true;
            lblDiffToolPath.Location = new Point(3, 204);
            lblDiffToolPath.Name = "lblDiffToolPath";
            lblDiffToolPath.Size = new Size(75, 13);
            lblDiffToolPath.Text = "Path to difftool";
            // 
            // txtDiffToolPath
            // 
            txtDiffToolPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtDiffToolPath.Location = new Point(129, 200);
            txtDiffToolPath.Name = "txtDiffToolPath";
            txtDiffToolPath.Size = new Size(902, 20);
            txtDiffToolPath.LostFocus += txtDiffMergeToolPath_LostFocus;
            txtDiffToolPath.TextChanged += txtDiffToolPath_TextChanged;
            // 
            // _NO_TRANSLATE_cboDiffTool
            // 
            _NO_TRANSLATE_cboDiffTool.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_cboDiffTool.FormattingEnabled = true;
            _NO_TRANSLATE_cboDiffTool.Location = new Point(129, 171);
            _NO_TRANSLATE_cboDiffTool.Name = "_NO_TRANSLATE_cboDiffTool";
            _NO_TRANSLATE_cboDiffTool.Size = new Size(241, 21);
            _NO_TRANSLATE_cboDiffTool.TextChanged += cboDiffTool_TextChanged;
            // 
            // lblDiffTool
            // 
            lblDiffTool.Anchor = AnchorStyles.Left;
            lblDiffTool.AutoSize = true;
            lblDiffTool.Location = new Point(3, 175);
            lblDiffTool.Name = "lblDiffTool";
            lblDiffTool.Size = new Size(40, 13);
            lblDiffTool.Text = "Difftool";
            // 
            // InvalidGitPathGlobal
            // 
            InvalidGitPathGlobal.Anchor = AnchorStyles.Right;
            InvalidGitPathGlobal.AutoSize = true;
            InvalidGitPathGlobal.BackColor = SystemColors.Info;
            InvalidGitPathGlobal.BorderStyle = BorderStyle.FixedSingle;
            tableLayoutPanelGitConfig.SetColumnSpan(InvalidGitPathGlobal, 2);
            InvalidGitPathGlobal.Controls.Add(pictureBox1);
            InvalidGitPathGlobal.Controls.Add(label9);
            InvalidGitPathGlobal.Location = new Point(1037, 29);
            InvalidGitPathGlobal.Name = "InvalidGitPathGlobal";
            InvalidGitPathGlobal.Padding = new Padding(3, 3, 3, 3);
            tableLayoutPanelGitConfig.SetRowSpan(InvalidGitPathGlobal, 4);
            InvalidGitPathGlobal.Size = new Size(237, 47);
            InvalidGitPathGlobal.Visible = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Dock = DockStyle.Fill;
            label9.Location = new Point(57, 3);
            label9.Name = "label9";
            label9.Size = new Size(175, 39);
            label9.Text = "You need to set the correct path to \r\ngit before you can change\r\nglobal settings." +
    "\r\n";
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Left;
            pictureBox1.Image = Properties.Images.StatusBadgeError;
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(54, 39);
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.TabStop = false;
            // 
            // btnMergeToolCommandSuggest
            // 
            btnMergeToolCommandSuggest.AccessibleName = "Suggest Mergetool command";
            btnMergeToolCommandSuggest.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnMergeToolCommandSuggest.Location = new Point(1037, 140);
            btnMergeToolCommandSuggest.Name = "btnMergeToolCommandSuggest";
            btnMergeToolCommandSuggest.Size = new Size(123, 25);
            btnMergeToolCommandSuggest.Text = "Suggest";
            btnMergeToolCommandSuggest.UseVisualStyleBackColor = true;
            btnMergeToolCommandSuggest.Click += btnMergeToolCommandSuggest_Click;
            // 
            // txtMergeToolCommand
            // 
            txtMergeToolCommand.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMergeToolCommand.Location = new Point(129, 142);
            txtMergeToolCommand.Name = "txtMergeToolCommand";
            txtMergeToolCommand.Size = new Size(902, 20);
            // 
            // lblMergeToolCommand
            // 
            lblMergeToolCommand.Anchor = AnchorStyles.Left;
            lblMergeToolCommand.AutoSize = true;
            lblMergeToolCommand.Location = new Point(3, 146);
            lblMergeToolCommand.Name = "lblMergeToolCommand";
            lblMergeToolCommand.Size = new Size(103, 13);
            lblMergeToolCommand.Text = "Mergetool command";
            // 
            // btnMergeToolBrowse
            // 
            btnMergeToolBrowse.AccessibleName = "Browse Path to mergetool";
            btnMergeToolBrowse.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnMergeToolBrowse.Location = new Point(1037, 109);
            btnMergeToolBrowse.Name = "btnMergeToolBrowse";
            btnMergeToolBrowse.Size = new Size(123, 25);
            btnMergeToolBrowse.Text = "Browse";
            btnMergeToolBrowse.UseVisualStyleBackColor = true;
            btnMergeToolBrowse.Click += btnMergeToolBrowse_Click;
            // 
            // _NO_TRANSLATE_cboMergeTool
            // 
            _NO_TRANSLATE_cboMergeTool.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_cboMergeTool.FormattingEnabled = true;
            _NO_TRANSLATE_cboMergeTool.Location = new Point(129, 82);
            _NO_TRANSLATE_cboMergeTool.Name = "_NO_TRANSLATE_cboMergeTool";
            _NO_TRANSLATE_cboMergeTool.Size = new Size(241, 21);
            _NO_TRANSLATE_cboMergeTool.TextChanged += cboMergeTool_TextChanged;
            // 
            // lblMergeToolPath
            // 
            lblMergeToolPath.Anchor = AnchorStyles.Left;
            lblMergeToolPath.AutoSize = true;
            lblMergeToolPath.Location = new Point(3, 115);
            lblMergeToolPath.Name = "lblMergeToolPath";
            lblMergeToolPath.Size = new Size(90, 13);
            lblMergeToolPath.Text = "Path to mergetool";
            // 
            // txtMergeToolPath
            // 
            txtMergeToolPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMergeToolPath.Location = new Point(129, 111);
            txtMergeToolPath.Name = "txtMergeToolPath";
            txtMergeToolPath.Size = new Size(902, 20);
            txtMergeToolPath.LostFocus += txtDiffMergeToolPath_LostFocus;
            txtMergeToolPath.TextChanged += txtMergeToolPath_TextChanged;
            // 
            // lblMergeTool
            // 
            lblMergeTool.Anchor = AnchorStyles.Left;
            lblMergeTool.AutoSize = true;
            lblMergeTool.Location = new Point(3, 86);
            lblMergeTool.Name = "lblMergeTool";
            lblMergeTool.Size = new Size(54, 13);
            lblMergeTool.Text = "Mergetool";
            // 
            // GlobalEditor
            // 
            GlobalEditor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            GlobalEditor.Location = new Point(129, 55);
            GlobalEditor.Name = "GlobalEditor";
            GlobalEditor.Size = new Size(902, 21);
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Left;
            label6.AutoSize = true;
            label6.Location = new Point(3, 59);
            label6.Name = "label6";
            label6.Size = new Size(34, 13);
            label6.Text = "Editor";
            // 
            // GlobalUserEmail
            // 
            GlobalUserEmail.Anchor = AnchorStyles.Left;
            GlobalUserEmail.Location = new Point(129, 29);
            GlobalUserEmail.Name = "GlobalUserEmail";
            GlobalUserEmail.Size = new Size(241, 20);
            // 
            // GlobalUserName
            // 
            GlobalUserName.Anchor = AnchorStyles.Left;
            GlobalUserName.Location = new Point(129, 3);
            GlobalUserName.Name = "GlobalUserName";
            GlobalUserName.Size = new Size(241, 20);
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(3, 32);
            label4.Name = "label4";
            label4.Size = new Size(56, 13);
            label4.Text = "User email";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 6);
            label3.Name = "label3";
            label3.Size = new Size(58, 13);
            label3.Text = "User name";
            // 
            // groupBoxLineEndings
            // 
            groupBoxLineEndings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxLineEndings.AutoSize = true;
            tableLayoutPanelGitConfig.SetColumnSpan(groupBoxLineEndings, 4);
            groupBoxLineEndings.Controls.Add(flowLayoutPanelLineEndings);
            groupBoxLineEndings.Location = new Point(3, 291);
            groupBoxLineEndings.Margin = new Padding(3, 3, 3, 6);
            groupBoxLineEndings.Name = "groupBoxLineEndings";
            groupBoxLineEndings.Size = new Size(1271, 111);
            groupBoxLineEndings.TabStop = false;
            groupBoxLineEndings.Text = "Line endings";
            // 
            // flowLayoutPanelLineEndings
            // 
            flowLayoutPanelLineEndings.AutoSize = true;
            flowLayoutPanelLineEndings.Controls.Add(globalAutoCrlfTrue);
            flowLayoutPanelLineEndings.Controls.Add(globalAutoCrlfInput);
            flowLayoutPanelLineEndings.Controls.Add(globalAutoCrlfFalse);
            flowLayoutPanelLineEndings.Controls.Add(globalAutoCrlfNotSet);
            flowLayoutPanelLineEndings.Dock = DockStyle.Fill;
            flowLayoutPanelLineEndings.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelLineEndings.Location = new Point(3, 16);
            flowLayoutPanelLineEndings.Name = "flowLayoutPanelLineEndings";
            flowLayoutPanelLineEndings.Size = new Size(1265, 92);
            // 
            // globalAutoCrlfTrue
            // 
            globalAutoCrlfTrue.AutoSize = true;
            globalAutoCrlfTrue.Location = new Point(3, 3);
            globalAutoCrlfTrue.Name = "globalAutoCrlfTrue";
            globalAutoCrlfTrue.Size = new Size(439, 17);
            globalAutoCrlfTrue.TabStop = true;
            globalAutoCrlfTrue.Text = "Checkout Windows-style, commit Unix-style line endings (\"core.autocrlf\"  is set t" +
    "o \"true\")";
            globalAutoCrlfTrue.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfInput
            // 
            globalAutoCrlfInput.AutoSize = true;
            globalAutoCrlfInput.Location = new Point(3, 26);
            globalAutoCrlfInput.Name = "globalAutoCrlfInput";
            globalAutoCrlfInput.Size = new Size(397, 17);
            globalAutoCrlfInput.TabStop = true;
            globalAutoCrlfInput.Text = "Checkout as-is, commit Unix-style line endings (\"core.autocrlf\"  is set to \"input" +
    "\")";
            globalAutoCrlfInput.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfFalse
            // 
            globalAutoCrlfFalse.AutoSize = true;
            globalAutoCrlfFalse.Location = new Point(3, 49);
            globalAutoCrlfFalse.Name = "globalAutoCrlfFalse";
            globalAutoCrlfFalse.Size = new Size(313, 17);
            globalAutoCrlfFalse.TabStop = true;
            globalAutoCrlfFalse.Text = "Checkout as-is, commit as-is (\"core.autocrlf\"  is set to \"false\")";
            globalAutoCrlfFalse.UseVisualStyleBackColor = true;
            // 
            // globalAutoCrlfNotSet
            // 
            globalAutoCrlfNotSet.AutoSize = true;
            globalAutoCrlfNotSet.Location = new Point(3, 72);
            globalAutoCrlfNotSet.Name = "globalAutoCrlfNotSet";
            globalAutoCrlfNotSet.Size = new Size(59, 17);
            globalAutoCrlfNotSet.TabStop = true;
            globalAutoCrlfNotSet.Text = "Not set";
            globalAutoCrlfNotSet.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelGitConfig
            // 
            tableLayoutPanelGitConfig.ColumnCount = 4;
            tableLayoutPanelGitConfig.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelGitConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelGitConfig.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelGitConfig.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelGitConfig.Controls.Add(label3, 0, 0);
            tableLayoutPanelGitConfig.Controls.Add(GlobalUserName, 1, 0);
            tableLayoutPanelGitConfig.Controls.Add(label4, 0, 1);
            tableLayoutPanelGitConfig.Controls.Add(GlobalUserEmail, 1, 1);
            tableLayoutPanelGitConfig.Controls.Add(label6, 0, 2);
            tableLayoutPanelGitConfig.Controls.Add(GlobalEditor, 1, 2);
            tableLayoutPanelGitConfig.Controls.Add(lblMergeTool, 0, 3);
            tableLayoutPanelGitConfig.Controls.Add(_NO_TRANSLATE_cboMergeTool, 1, 3);
            tableLayoutPanelGitConfig.Controls.Add(lblMergeToolPath, 0, 4);
            tableLayoutPanelGitConfig.Controls.Add(txtMergeToolPath, 1, 4);
            tableLayoutPanelGitConfig.Controls.Add(btnMergeToolBrowse, 2, 4);
            tableLayoutPanelGitConfig.Controls.Add(lblMergeToolCommand, 0, 5);
            tableLayoutPanelGitConfig.Controls.Add(txtMergeToolCommand, 1, 5);
            tableLayoutPanelGitConfig.Controls.Add(btnMergeToolCommandSuggest, 2, 5);
            tableLayoutPanelGitConfig.Controls.Add(lblDiffTool, 0, 6);
            tableLayoutPanelGitConfig.Controls.Add(_NO_TRANSLATE_cboDiffTool, 1, 6);
            tableLayoutPanelGitConfig.Controls.Add(lblDiffToolPath, 0, 7);
            tableLayoutPanelGitConfig.Controls.Add(txtDiffToolPath, 1, 7);
            tableLayoutPanelGitConfig.Controls.Add(btnDiffToolBrowse, 2, 7);
            tableLayoutPanelGitConfig.Controls.Add(lblDiffToolCommand, 0, 8);
            tableLayoutPanelGitConfig.Controls.Add(txtDiffToolCommand, 1, 8);
            tableLayoutPanelGitConfig.Controls.Add(btnDiffToolCommandSuggest, 2, 8);
            tableLayoutPanelGitConfig.Controls.Add(lblCommitTemplatePath, 0, 9);
            tableLayoutPanelGitConfig.Controls.Add(txtCommitTemplatePath, 1, 9);
            tableLayoutPanelGitConfig.Controls.Add(btnCommitTemplateBrowse, 2, 9);
            tableLayoutPanelGitConfig.Controls.Add(groupBoxLineEndings, 0, 10);
            tableLayoutPanelGitConfig.Controls.Add(label60, 0, 11);
            tableLayoutPanelGitConfig.Controls.Add(InvalidGitPathGlobal, 2, 0);
            tableLayoutPanelGitConfig.Controls.Add(Global_FilesEncoding, 1, 11);
            tableLayoutPanelGitConfig.Controls.Add(ConfigureEncoding, 2, 11);
            tableLayoutPanelGitConfig.Dock = DockStyle.Top;
            tableLayoutPanelGitConfig.Location = new Point(0, 0);
            tableLayoutPanelGitConfig.Name = "tableLayoutPanelGitConfig";
            tableLayoutPanelGitConfig.RowCount = 13;
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle());
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelGitConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelGitConfig.Size = new Size(1277, 778);
            // 
            // ConfigureEncoding
            // 
            ConfigureEncoding.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ConfigureEncoding.Location = new Point(1037, 411);
            ConfigureEncoding.Name = "ConfigureEncoding";
            ConfigureEncoding.Size = new Size(123, 25);
            ConfigureEncoding.Text = "Configure";
            ConfigureEncoding.UseVisualStyleBackColor = true;
            ConfigureEncoding.Click += ConfigureEncoding_Click;
            // 
            // GitConfigSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tableLayoutPanelGitConfig);
            Name = "GitConfigSettingsPage";
            Size = new Size(1277, 715);
            Text = "Config";
            InvalidGitPathGlobal.ResumeLayout(false);
            InvalidGitPathGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            groupBoxLineEndings.ResumeLayout(false);
            groupBoxLineEndings.PerformLayout();
            flowLayoutPanelLineEndings.ResumeLayout(false);
            flowLayoutPanelLineEndings.PerformLayout();
            tableLayoutPanelGitConfig.ResumeLayout(false);
            tableLayoutPanelGitConfig.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Label label60;
        private ComboBox Global_FilesEncoding;
        private Button btnCommitTemplateBrowse;
        private TextBox txtCommitTemplatePath;
        private Button btnDiffToolCommandSuggest;
        private TextBox txtDiffToolCommand;
        private Button btnDiffToolBrowse;
        private TextBox txtDiffToolPath;
        private ComboBox _NO_TRANSLATE_cboDiffTool;
        private Panel InvalidGitPathGlobal;
        private Label label9;
        private PictureBox pictureBox1;
        private Button btnMergeToolCommandSuggest;
        private TextBox txtMergeToolCommand;
        private Button btnMergeToolBrowse;
        private ComboBox _NO_TRANSLATE_cboMergeTool;
        private TextBox txtMergeToolPath;
        private ComboBox GlobalEditor;
        private Label label6;
        private TextBox GlobalUserEmail;
        private TextBox GlobalUserName;
        private Label label4;
        private Label label3;
        private GroupBox groupBoxLineEndings;
        private RadioButton globalAutoCrlfFalse;
        private RadioButton globalAutoCrlfInput;
        private RadioButton globalAutoCrlfTrue;
        private RadioButton globalAutoCrlfNotSet;
        private TableLayoutPanel tableLayoutPanelGitConfig;
        private FlowLayoutPanel flowLayoutPanelLineEndings;
        private Button ConfigureEncoding;
        private Label lblCommitTemplatePath;
        private Label lblDiffToolCommand;
        private Label lblDiffToolPath;
        private Label lblDiffTool;
        private Label lblMergeToolCommand;
        private Label lblMergeToolPath;
        private Label lblMergeTool;
    }
}
