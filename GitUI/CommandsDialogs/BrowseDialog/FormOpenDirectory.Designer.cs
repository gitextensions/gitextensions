namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormOpenDirectory
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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            _NO_TRANSLATE_Directory = new ComboBox();
            Load = new Button();
            folderBrowserButton = new Button();
            folderGoUpButton = new Button();
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 13);
            label1.Name = "label1";
            label1.Size = new Size(52, 13);
            label1.TabIndex = 0;
            label1.Text = "&Directory:";
            // 
            // _NO_TRANSLATE_Directory
            // 
            _NO_TRANSLATE_Directory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_Directory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_Directory.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            _NO_TRANSLATE_Directory.FormattingEnabled = true;
            _NO_TRANSLATE_Directory.Location = new Point(85, 9);
            _NO_TRANSLATE_Directory.Name = "_NO_TRANSLATE_Directory";
            _NO_TRANSLATE_Directory.Size = new Size(360, 21);
            _NO_TRANSLATE_Directory.TabIndex = 1;
            _NO_TRANSLATE_Directory.TextChanged += _NO_TRANSLATE_Directory_TextChanged;
            _NO_TRANSLATE_Directory.KeyPress += DirectoryKeyPress;
            // 
            // Load
            // 
            Load.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Load.Image = Properties.Images.RepoOpen;
            Load.ImageAlign = ContentAlignment.MiddleLeft;
            Load.Location = new Point(448, 39);
            Load.Name = "Load";
            Load.Size = new Size(164, 25);
            Load.TabIndex = 3;
            Load.Text = "Open";
            Load.UseVisualStyleBackColor = true;
            Load.Click += LoadClick;
            // 
            // folderBrowserButton
            // 
            folderBrowserButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            folderBrowserButton.AutoSize = true;
            folderBrowserButton.Image = Properties.Images.BrowseFileExplorer;
            folderBrowserButton.ImageAlign = ContentAlignment.MiddleLeft;
            folderBrowserButton.Location = new Point(477, 7);
            folderBrowserButton.MinimumSize = new Size(135, 25);
            folderBrowserButton.Name = "folderBrowserButton";
            folderBrowserButton.Size = new Size(135, 25);
            folderBrowserButton.TabIndex = 4;
            folderBrowserButton.Text = "&Browse...";
            folderBrowserButton.UseVisualStyleBackColor = true;
            folderBrowserButton.Click += folderBrowserButton_Click;
            // 
            // folderGoUpButton
            // 
            folderGoUpButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            folderGoUpButton.Image = Properties.Images.NavigateUp;
            folderGoUpButton.Location = new Point(448, 7);
            folderGoUpButton.Name = "folderGoUpButton";
            folderGoUpButton.Size = new Size(26, 25);
            folderGoUpButton.TabIndex = 5;
            toolTip1.SetToolTip(folderGoUpButton, "Go to parent directory...");
            folderGoUpButton.UseVisualStyleBackColor = true;
            folderGoUpButton.Click += folderGoUpButton_Click;
            // 
            // toolTip1
            // 
            toolTip1.AutomaticDelay = 100;
            toolTip1.ShowAlways = true;
            toolTip1.ToolTipTitle = "Help";
            // 
            // FormOpenDirectory
            // 
            AcceptButton = Load;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(615, 81);
            Controls.Add(folderGoUpButton);
            Controls.Add(folderBrowserButton);
            Controls.Add(Load);
            Controls.Add(_NO_TRANSLATE_Directory);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormOpenDirectory";
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Open local repository";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private ComboBox _NO_TRANSLATE_Directory;
        private new Button Load;
        private Button folderBrowserButton;
        private Button folderGoUpButton;
        private ToolTip toolTip1;
    }
}
