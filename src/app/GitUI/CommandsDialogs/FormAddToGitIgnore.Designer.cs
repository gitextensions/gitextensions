namespace GitUI.CommandsDialogs
{
    partial class FormAddToGitIgnore
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
            if (disposing)
            {
                _ignoredFilesLoader.Dispose();
                if (components is not null)
                {
                    components.Dispose();
                }
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
            groupBox1 = new GroupBox();
            panel3 = new Panel();
            panel2 = new Panel();
            _NO_TRANSLATE_filesWillBeIgnored = new Label();
            noMatchPanel = new Panel();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            _NO_TRANSLATE_Preview = new ListBox();
            AddToIgnore = new Button();
            FilePattern = new TextBox();
            panel1 = new Panel();
            btnCancel = new Button();
            groupFilePattern = new GroupBox();
            groupBox1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            noMatchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            panel1.SuspendLayout();
            groupFilePattern.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel3);
            groupBox1.Controls.Add(panel2);
            groupBox1.Controls.Add(noMatchPanel);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 93);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(12, 8, 12, 4);
            groupBox1.Size = new Size(599, 204);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Preview";
            // 
            // panel3
            // 
            panel3.Controls.Add(_NO_TRANSLATE_Preview);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(12, 22);
            panel3.Name = "panel3";
            panel3.Size = new Size(575, 159);
            panel3.TabIndex = 13;
            // 
            // panel2
            // 
            panel2.Controls.Add(_NO_TRANSLATE_filesWillBeIgnored);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(12, 181);
            panel2.Name = "panel2";
            panel2.Size = new Size(575, 19);
            panel2.TabIndex = 12;
            // 
            // _NO_TRANSLATE_filesWillBeIgnored
            // 
            _NO_TRANSLATE_filesWillBeIgnored.AutoSize = true;
            _NO_TRANSLATE_filesWillBeIgnored.Dock = DockStyle.Right;
            _NO_TRANSLATE_filesWillBeIgnored.Location = new Point(455, 0);
            _NO_TRANSLATE_filesWillBeIgnored.Name = "_NO_TRANSLATE_filesWillBeIgnored";
            _NO_TRANSLATE_filesWillBeIgnored.Size = new Size(120, 15);
            _NO_TRANSLATE_filesWillBeIgnored.TabIndex = 11;
            _NO_TRANSLATE_filesWillBeIgnored.Text = "(matched files count)";
            // 
            // noMatchPanel
            // 
            noMatchPanel.Anchor = AnchorStyles.None;
            noMatchPanel.BackColor = SystemColors.Control;
            noMatchPanel.Controls.Add(label2);
            noMatchPanel.Controls.Add(pictureBox1);
            noMatchPanel.Location = new Point(161, 89);
            noMatchPanel.Name = "noMatchPanel";
            noMatchPanel.Size = new Size(233, 26);
            noMatchPanel.TabIndex = 2;
            noMatchPanel.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = SystemColors.ControlDarkDark;
            label2.Location = new Point(25, 5);
            label2.Name = "label2";
            label2.Size = new Size(195, 15);
            label2.TabIndex = 1;
            label2.Text = "No existing files match that pattern.";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Images.Unmerged;
            pictureBox1.Location = new Point(10, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(16, 16);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // _NO_TRANSLATE_Preview
            // 
            _NO_TRANSLATE_Preview.Dock = DockStyle.Fill;
            _NO_TRANSLATE_Preview.FormattingEnabled = true;
            _NO_TRANSLATE_Preview.ItemHeight = 15;
            _NO_TRANSLATE_Preview.Location = new Point(12, 24);
            _NO_TRANSLATE_Preview.Name = "_NO_TRANSLATE_Preview";
            _NO_TRANSLATE_Preview.Size = new Size(575, 180);
            _NO_TRANSLATE_Preview.TabIndex = 2;
            // 
            // AddToIgnore
            // 
            AddToIgnore.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AddToIgnore.Location = new Point(335, 7);
            AddToIgnore.Name = "AddToIgnore";
            AddToIgnore.Size = new Size(110, 25);
            AddToIgnore.TabIndex = 7;
            AddToIgnore.Text = "Ignore";
            AddToIgnore.UseVisualStyleBackColor = true;
            AddToIgnore.Click += AddToIgnoreClick;
            // 
            // FilePattern
            // 
            FilePattern.AcceptsReturn = true;
            FilePattern.Dock = DockStyle.Fill;
            FilePattern.Location = new Point(12, 19);
            FilePattern.Multiline = true;
            FilePattern.Name = "FilePattern";
            FilePattern.ScrollBars = ScrollBars.Vertical;
            FilePattern.Size = new Size(575, 71);
            FilePattern.TabIndex = 6;
            FilePattern.WordWrap = false;
            FilePattern.TextChanged += FilePattern_TextChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnCancel);
            panel1.Controls.Add(AddToIgnore);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 297);
            panel1.Name = "panel1";
            panel1.Size = new Size(599, 44);
            panel1.TabIndex = 10;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(467, 7);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 25);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupFilePattern
            // 
            groupFilePattern.Controls.Add(FilePattern);
            groupFilePattern.Dock = DockStyle.Top;
            groupFilePattern.Location = new Point(0, 0);
            groupFilePattern.Name = "groupFilePattern";
            groupFilePattern.Padding = new Padding(12, 3, 12, 3);
            groupFilePattern.Size = new Size(599, 93);
            groupFilePattern.TabIndex = 11;
            groupFilePattern.TabStop = false;
            groupFilePattern.Text = "Enter a file pattern to ignore:";
            // 
            // FormAddToGitIgnore
            // 
            AcceptButton = AddToIgnore;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnCancel;
            ClientSize = new Size(599, 341);
            Controls.Add(groupBox1);
            Controls.Add(groupFilePattern);
            Controls.Add(panel1);
            MinimizeBox = false;
            Name = "FormAddToGitIgnore";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add file(s) to .gitignore";
            groupBox1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            noMatchPanel.ResumeLayout(false);
            noMatchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            panel1.ResumeLayout(false);
            groupFilePattern.ResumeLayout(false);
            groupFilePattern.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBox1;
        private ListBox _NO_TRANSLATE_Preview;
        private Button AddToIgnore;
        private TextBox FilePattern;
        private Panel noMatchPanel;
        private Label label2;
        private PictureBox pictureBox1;
        private Label _NO_TRANSLATE_filesWillBeIgnored;
        private Panel panel1;
        private Button btnCancel;
        private GroupBox groupFilePattern;
        private Panel panel2;
        private Panel panel3;

    }
}
