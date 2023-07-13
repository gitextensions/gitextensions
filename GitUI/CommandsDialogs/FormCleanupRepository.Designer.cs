namespace GitUI.CommandsDialogs
{
    partial class FormCleanupRepository
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Preview = new Button();
            AddPath = new Button();
            Cleanup = new Button();
            _NO_TRANSLATE_Close = new Button();
            groupBox1 = new GroupBox();
            RemoveIgnored = new RadioButton();
            RemoveNonIgnored = new RadioButton();
            RemoveAll = new RadioButton();
            RemoveDirectories = new CheckBox();
            PreviewOutput = new TextBox();
            label1 = new Label();
            textBoxPaths = new TextBox();
            checkBoxPathFilter = new CheckBox();
            labelPathHint = new Label();
            CleanSubmodules = new CheckBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // Preview
            // 
            Preview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Preview.Image = Properties.Images.Preview;
            Preview.ImageAlign = ContentAlignment.MiddleLeft;
            Preview.Location = new Point(50, 300);
            Preview.Name = "Preview";
            Preview.Size = new Size(120, 25);
            Preview.TabIndex = 0;
            Preview.Text = "Preview";
            Preview.UseVisualStyleBackColor = true;
            Preview.Click += Preview_Click;
            // 
            // AddPath
            // 
            AddPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddPath.ImageAlign = ContentAlignment.MiddleLeft;
            AddPath.Location = new Point(302, 164);
            AddPath.Name = "AddPath";
            AddPath.Size = new Size(120, 25);
            AddPath.TabIndex = 0;
            AddPath.Text = "Add a path...";
            AddPath.UseVisualStyleBackColor = true;
            AddPath.Click += AddPath_Click;
            // 
            // Cleanup
            // 
            Cleanup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Cleanup.Image = Properties.Images.CleanupRepo;
            Cleanup.ImageAlign = ContentAlignment.MiddleLeft;
            Cleanup.Location = new Point(176, 300);
            Cleanup.Name = "Cleanup";
            Cleanup.Size = new Size(120, 25);
            Cleanup.TabIndex = 1;
            Cleanup.Text = "Cleanup";
            Cleanup.UseVisualStyleBackColor = true;
            Cleanup.Click += Cleanup_Click;
            // 
            // _NO_TRANSLATE_Close
            // 
            _NO_TRANSLATE_Close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _NO_TRANSLATE_Close.DialogResult = DialogResult.OK;
            _NO_TRANSLATE_Close.Location = new Point(302, 300);
            _NO_TRANSLATE_Close.Name = "_NO_TRANSLATE_Close";
            _NO_TRANSLATE_Close.Size = new Size(120, 25);
            _NO_TRANSLATE_Close.TabIndex = 2;
            _NO_TRANSLATE_Close.Text = TranslatedStrings.Close;
            _NO_TRANSLATE_Close.UseVisualStyleBackColor = true;
            _NO_TRANSLATE_Close.Click += Close_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(RemoveIgnored);
            groupBox1.Controls.Add(RemoveNonIgnored);
            groupBox1.Controls.Add(RemoveAll);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(410, 100);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Remove untracked files from working directory";
            // 
            // RemoveIgnored
            // 
            RemoveIgnored.AutoSize = true;
            RemoveIgnored.Location = new Point(7, 67);
            RemoveIgnored.Name = "RemoveIgnored";
            RemoveIgnored.Size = new Size(197, 17);
            RemoveIgnored.TabIndex = 2;
            RemoveIgnored.Text = "Remove only ignored untracked files";
            RemoveIgnored.UseVisualStyleBackColor = true;
            // 
            // RemoveNonIgnored
            // 
            RemoveNonIgnored.AutoSize = true;
            RemoveNonIgnored.Location = new Point(7, 43);
            RemoveNonIgnored.Name = "RemoveNonIgnored";
            RemoveNonIgnored.Size = new Size(218, 17);
            RemoveNonIgnored.TabIndex = 1;
            RemoveNonIgnored.Text = "Remove only non-ignored untracked files";
            RemoveNonIgnored.UseVisualStyleBackColor = true;
            // 
            // RemoveAll
            // 
            RemoveAll.AutoSize = true;
            RemoveAll.Checked = true;
            RemoveAll.Location = new Point(7, 20);
            RemoveAll.Name = "RemoveAll";
            RemoveAll.Size = new Size(150, 17);
            RemoveAll.TabIndex = 0;
            RemoveAll.TabStop = true;
            RemoveAll.Text = "Remove all untracked files";
            RemoveAll.UseVisualStyleBackColor = true;
            // 
            // RemoveDirectories
            // 
            RemoveDirectories.AutoSize = true;
            RemoveDirectories.Checked = true;
            RemoveDirectories.CheckState = CheckState.Checked;
            RemoveDirectories.Location = new Point(19, 119);
            RemoveDirectories.Name = "RemoveDirectories";
            RemoveDirectories.Size = new Size(168, 17);
            RemoveDirectories.TabIndex = 4;
            RemoveDirectories.Text = "Remove untracked directories";
            RemoveDirectories.UseVisualStyleBackColor = true;
            // 
            // PreviewOutput
            // 
            PreviewOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PreviewOutput.Location = new Point(12, 353);
            PreviewOutput.Multiline = true;
            PreviewOutput.Name = "PreviewOutput";
            PreviewOutput.ScrollBars = ScrollBars.Both;
            PreviewOutput.Size = new Size(410, 87);
            PreviewOutput.TabIndex = 5;
            PreviewOutput.WordWrap = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 335);
            label1.Name = "label1";
            label1.Size = new Size(28, 13);
            label1.TabIndex = 6;
            label1.Text = "Log:";
            // 
            // textBoxPaths
            // 
            textBoxPaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxPaths.Location = new Point(48, 194);
            textBoxPaths.Multiline = true;
            textBoxPaths.Name = "textBoxPaths";
            textBoxPaths.ScrollBars = ScrollBars.Vertical;
            textBoxPaths.Size = new Size(374, 63);
            textBoxPaths.TabIndex = 1;
            // 
            // checkBoxPathFilter
            // 
            checkBoxPathFilter.AutoSize = true;
            checkBoxPathFilter.Location = new Point(19, 169);
            checkBoxPathFilter.Name = "checkBoxPathFilter";
            checkBoxPathFilter.Size = new Size(176, 17);
            checkBoxPathFilter.TabIndex = 0;
            checkBoxPathFilter.Text = "Affect the following path(s) only:";
            checkBoxPathFilter.UseVisualStyleBackColor = true;
            checkBoxPathFilter.CheckedChanged += checkBoxPathFilter_CheckedChanged;
            // 
            // labelPathHint
            // 
            labelPathHint.AutoSize = true;
            labelPathHint.Location = new Point(50, 261);
            labelPathHint.Name = "labelPathHint";
            labelPathHint.Size = new Size(92, 13);
            labelPathHint.TabIndex = 7;
            labelPathHint.Text = "(one path per line)";
            // 
            // CleanSubmodules
            // 
            CleanSubmodules.AutoSize = true;
            CleanSubmodules.Location = new Point(19, 144);
            CleanSubmodules.Name = "CleanSubmodules";
            CleanSubmodules.Size = new Size(183, 19);
            CleanSubmodules.TabIndex = 8;
            CleanSubmodules.Text = "Clean submodules recursively";
            CleanSubmodules.UseVisualStyleBackColor = true;
            // 
            // FormCleanupRepository
            // 
            AcceptButton = AddPath;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = _NO_TRANSLATE_Close;
            ClientSize = new Size(434, 462);
            Controls.Add(CleanSubmodules);
            Controls.Add(labelPathHint);
            Controls.Add(textBoxPaths);
            Controls.Add(checkBoxPathFilter);
            Controls.Add(label1);
            Controls.Add(PreviewOutput);
            Controls.Add(RemoveDirectories);
            Controls.Add(groupBox1);
            Controls.Add(_NO_TRANSLATE_Close);
            Controls.Add(Cleanup);
            Controls.Add(Preview);
            Controls.Add(AddPath);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(450, 500);
            Name = "FormCleanupRepository";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Clean working directory";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Preview;
        private Button AddPath;
        private Button Cleanup;
        private Button _NO_TRANSLATE_Close;
        private GroupBox groupBox1;
        private RadioButton RemoveIgnored;
        private RadioButton RemoveNonIgnored;
        private RadioButton RemoveAll;
        private CheckBox RemoveDirectories;
        private TextBox PreviewOutput;
        private Label label1;
        private TextBox textBoxPaths;
        private CheckBox checkBoxPathFilter;
        private Label labelPathHint;
        private CheckBox CleanSubmodules;
    }
}
