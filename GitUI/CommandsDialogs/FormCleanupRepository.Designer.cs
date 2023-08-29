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
            AddInclusivePath = new Button();
            Cleanup = new Button();
            _NO_TRANSLATE_Close = new Button();
            groupBox1 = new GroupBox();
            RemoveIgnored = new RadioButton();
            RemoveNonIgnored = new RadioButton();
            RemoveAll = new RadioButton();
            RemoveDirectories = new CheckBox();
            PreviewOutput = new TextBox();
            label1 = new Label();
            textBoxIncludePaths = new TextBox();
            checkBoxIncludePathFilter = new CheckBox();
            textBoxExcludePaths = new TextBox();
            checkBoxExcludePathFilter = new CheckBox();
            AddExclusivePath = new Button();
            labelPathHintExclude = new Label();
            labelPathHintInclude = new Label();
            CleanSubmodules = new CheckBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // Preview
            // 
            Preview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Preview.Image = Properties.Images.Preview;
            Preview.ImageAlign = ContentAlignment.MiddleLeft;
            Preview.Location = new Point(60, 529);
            Preview.Margin = new Padding(4);
            Preview.Name = "Preview";
            Preview.Size = new Size(150, 31);
            Preview.TabIndex = 10;
            Preview.Text = "Preview";
            Preview.UseVisualStyleBackColor = true;
            Preview.Click += Preview_Click;
            // 
            // AddInclusivePath
            // 
            AddInclusivePath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddInclusivePath.ImageAlign = ContentAlignment.MiddleLeft;
            AddInclusivePath.Location = new Point(378, 203);
            AddInclusivePath.Margin = new Padding(4);
            AddInclusivePath.Name = "AddInclusivePath";
            AddInclusivePath.Size = new Size(150, 31);
            AddInclusivePath.TabIndex = 3;
            AddInclusivePath.Text = "Add a path...";
            AddInclusivePath.UseVisualStyleBackColor = true;
            AddInclusivePath.Click += AddIncludePath_Click;
            // 
            // Cleanup
            // 
            Cleanup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Cleanup.Image = Properties.Images.CleanupRepo;
            Cleanup.ImageAlign = ContentAlignment.MiddleLeft;
            Cleanup.Location = new Point(218, 529);
            Cleanup.Margin = new Padding(4);
            Cleanup.Name = "Cleanup";
            Cleanup.Size = new Size(150, 31);
            Cleanup.TabIndex = 11;
            Cleanup.Text = "Cleanup";
            Cleanup.UseVisualStyleBackColor = true;
            Cleanup.Click += Cleanup_Click;
            // 
            // _NO_TRANSLATE_Close
            // 
            _NO_TRANSLATE_Close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _NO_TRANSLATE_Close.DialogResult = DialogResult.OK;
            _NO_TRANSLATE_Close.Location = new Point(376, 529);
            _NO_TRANSLATE_Close.Margin = new Padding(4);
            _NO_TRANSLATE_Close.Name = "_NO_TRANSLATE_Close";
            _NO_TRANSLATE_Close.Size = new Size(150, 31);
            _NO_TRANSLATE_Close.TabIndex = 12;
            _NO_TRANSLATE_Close.Text = "Close";
            _NO_TRANSLATE_Close.UseVisualStyleBackColor = true;
            _NO_TRANSLATE_Close.Click += Close_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(RemoveIgnored);
            groupBox1.Controls.Add(RemoveNonIgnored);
            groupBox1.Controls.Add(RemoveAll);
            groupBox1.Location = new Point(15, 15);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new Size(512, 125);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Remove untracked files from working directory";
            // 
            // RemoveIgnored
            // 
            RemoveIgnored.AutoSize = true;
            RemoveIgnored.Location = new Point(9, 84);
            RemoveIgnored.Margin = new Padding(4);
            RemoveIgnored.Name = "RemoveIgnored";
            RemoveIgnored.Size = new Size(272, 24);
            RemoveIgnored.TabIndex = 2;
            RemoveIgnored.Text = "Remove only ignored untracked files";
            RemoveIgnored.UseVisualStyleBackColor = true;
            // 
            // RemoveNonIgnored
            // 
            RemoveNonIgnored.AutoSize = true;
            RemoveNonIgnored.Location = new Point(9, 54);
            RemoveNonIgnored.Margin = new Padding(4);
            RemoveNonIgnored.Name = "RemoveNonIgnored";
            RemoveNonIgnored.Size = new Size(303, 24);
            RemoveNonIgnored.TabIndex = 1;
            RemoveNonIgnored.Text = "Remove only non-ignored untracked files";
            RemoveNonIgnored.UseVisualStyleBackColor = true;
            // 
            // RemoveAll
            // 
            RemoveAll.AutoSize = true;
            RemoveAll.Checked = true;
            RemoveAll.Location = new Point(9, 25);
            RemoveAll.Margin = new Padding(4);
            RemoveAll.Name = "RemoveAll";
            RemoveAll.Size = new Size(204, 24);
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
            RemoveDirectories.Location = new Point(24, 148);
            RemoveDirectories.Margin = new Padding(4);
            RemoveDirectories.Name = "RemoveDirectories";
            RemoveDirectories.Size = new Size(228, 24);
            RemoveDirectories.TabIndex = 1;
            RemoveDirectories.Text = "Remove untracked directories";
            RemoveDirectories.UseVisualStyleBackColor = true;
            // 
            // PreviewOutput
            // 
            PreviewOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PreviewOutput.Location = new Point(17, 597);
            PreviewOutput.Margin = new Padding(4);
            PreviewOutput.Multiline = true;
            PreviewOutput.Name = "PreviewOutput";
            PreviewOutput.ScrollBars = ScrollBars.Both;
            PreviewOutput.Size = new Size(512, 117);
            PreviewOutput.TabIndex = 14;
            PreviewOutput.WordWrap = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 573);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(37, 20);
            label1.TabIndex = 13;
            label1.Text = "Log:";
            // 
            // textBoxIncludePaths
            // 
            textBoxIncludePaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxIncludePaths.Location = new Point(60, 240);
            textBoxIncludePaths.Margin = new Padding(4);
            textBoxIncludePaths.Multiline = true;
            textBoxIncludePaths.Name = "textBoxIncludePaths";
            textBoxIncludePaths.ScrollBars = ScrollBars.Vertical;
            textBoxIncludePaths.Size = new Size(466, 78);
            textBoxIncludePaths.TabIndex = 4;
            // 
            // checkBoxIncludePathFilter
            // 
            checkBoxIncludePathFilter.AutoSize = true;
            checkBoxIncludePathFilter.Location = new Point(24, 209);
            checkBoxIncludePathFilter.Margin = new Padding(4);
            checkBoxIncludePathFilter.Name = "checkBoxIncludePathFilter";
            checkBoxIncludePathFilter.Size = new Size(311, 24);
            checkBoxIncludePathFilter.TabIndex = 2;
            checkBoxIncludePathFilter.Text = "Affect the following directory path(s) only:";
            checkBoxIncludePathFilter.UseVisualStyleBackColor = true;
            checkBoxIncludePathFilter.CheckedChanged += checkBoxPathFilter_CheckedChanged;
            // 
            // textBoxExcludePaths
            // 
            textBoxExcludePaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxExcludePaths.Location = new Point(60, 391);
            textBoxExcludePaths.Margin = new Padding(4);
            textBoxExcludePaths.Multiline = true;
            textBoxExcludePaths.Name = "textBoxExcludePaths";
            textBoxExcludePaths.ScrollBars = ScrollBars.Vertical;
            textBoxExcludePaths.Size = new Size(466, 78);
            textBoxExcludePaths.TabIndex = 8;
            // 
            // checkBoxExcludePathFilter
            // 
            checkBoxExcludePathFilter.AutoSize = true;
            checkBoxExcludePathFilter.Location = new Point(24, 360);
            checkBoxExcludePathFilter.Margin = new Padding(4);
            checkBoxExcludePathFilter.Name = "checkBoxExcludePathFilter";
            checkBoxExcludePathFilter.Size = new Size(252, 24);
            checkBoxExcludePathFilter.TabIndex = 6;
            checkBoxExcludePathFilter.Text = "Exclude the following file path(s):";
            checkBoxExcludePathFilter.UseVisualStyleBackColor = true;
            checkBoxExcludePathFilter.CheckedChanged += checkBoxExcludePathFilter_CheckedChanged;
            // 
            // AddExclusivePath
            // 
            AddExclusivePath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddExclusivePath.ImageAlign = ContentAlignment.MiddleLeft;
            AddExclusivePath.Location = new Point(378, 354);
            AddExclusivePath.Margin = new Padding(4);
            AddExclusivePath.Name = "AddExclusivePath";
            AddExclusivePath.Size = new Size(150, 31);
            AddExclusivePath.TabIndex = 7;
            AddExclusivePath.Text = "Add a path...";
            AddExclusivePath.UseVisualStyleBackColor = true;
            AddExclusivePath.Click += AddExcludePath_Click;
            // 
            // labelPathHintExclude
            // 
            labelPathHintExclude.AutoSize = true;
            labelPathHintExclude.Location = new Point(62, 475);
            labelPathHintExclude.Margin = new Padding(4, 0, 4, 0);
            labelPathHintExclude.Name = "labelPathHintExclude";
            labelPathHintExclude.Size = new Size(132, 20);
            labelPathHintExclude.TabIndex = 9;
            labelPathHintExclude.Text = "(one path per line)";
            // 
            // labelPathHintInclude
            // 
            labelPathHintInclude.AutoSize = true;
            labelPathHintInclude.Location = new Point(62, 323);
            labelPathHintInclude.Margin = new Padding(4, 0, 4, 0);
            labelPathHintInclude.Name = "labelPathHintInclude";
            labelPathHintInclude.Size = new Size(132, 20);
            labelPathHintInclude.TabIndex = 5;
            labelPathHintInclude.Text = "(one path per line)";
            // 
            // CleanSubmodules
            // 
            CleanSubmodules.AutoSize = true;
            CleanSubmodules.Location = new Point(24, 178);
            CleanSubmodules.Name = "CleanSubmodules";
            CleanSubmodules.Size = new Size(225, 24);
            CleanSubmodules.TabIndex = 8;
            CleanSubmodules.Text = "Clean submodules recursively";
            CleanSubmodules.UseVisualStyleBackColor = true;
            // 
            // FormCleanupRepository
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = _NO_TRANSLATE_Close;
            ClientSize = new Size(542, 727);
            Controls.Add(labelPathHintInclude);
            Controls.Add(labelPathHintExclude);
            Controls.Add(textBoxExcludePaths);
            Controls.Add(checkBoxExcludePathFilter);
            Controls.Add(AddExclusivePath);
            Controls.Add(textBoxIncludePaths);
            Controls.Add(checkBoxIncludePathFilter);
            Controls.Add(label1);
            Controls.Add(PreviewOutput);
            Controls.Add(RemoveDirectories);
            Controls.Add(groupBox1);
            Controls.Add(_NO_TRANSLATE_Close);
            Controls.Add(Cleanup);
            Controls.Add(Preview);
            Controls.Add(AddInclusivePath);
            Controls.Add(CleanSubmodules);
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(558, 738);
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
        private Button AddInclusivePath;
        private Button Cleanup;
        private Button _NO_TRANSLATE_Close;
        private GroupBox groupBox1;
        private RadioButton RemoveIgnored;
        private RadioButton RemoveNonIgnored;
        private RadioButton RemoveAll;
        private CheckBox RemoveDirectories;
        private TextBox PreviewOutput;
        private Label label1;
        private TextBox textBoxIncludePaths;
        private CheckBox checkBoxIncludePathFilter;
        private TextBox textBoxExcludePaths;
        private CheckBox checkBoxExcludePathFilter;
        private Button AddExclusivePath;
        private Label labelPathHintExclude;
        private Label labelPathHintInclude;
        private CheckBox CleanSubmodules;
    }
}
