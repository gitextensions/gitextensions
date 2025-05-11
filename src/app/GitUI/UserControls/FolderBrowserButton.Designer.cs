namespace GitUI.UserControls
{
    partial class FolderBrowserButton
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
            _NO_TRANSLATE_buttonBrowse = new Button();
            SuspendLayout();
            // 
            // _NO_TRANSLATE_buttonBrowse
            // 
            _NO_TRANSLATE_buttonBrowse.AutoSize = true;
            _NO_TRANSLATE_buttonBrowse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _NO_TRANSLATE_buttonBrowse.Dock = DockStyle.Fill;
            _NO_TRANSLATE_buttonBrowse.Image = Properties.Images.BrowseFileExplorer;
            _NO_TRANSLATE_buttonBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            _NO_TRANSLATE_buttonBrowse.Location = new Point(0, 0);
            _NO_TRANSLATE_buttonBrowse.MinimumSize = new Size(100, 25);
            _NO_TRANSLATE_buttonBrowse.Name = "_NO_TRANSLATE_buttonBrowse";
            _NO_TRANSLATE_buttonBrowse.Size = new Size(100, 25);
            _NO_TRANSLATE_buttonBrowse.TabIndex = 5;
            _NO_TRANSLATE_buttonBrowse.Text = "&Browse...";
            _NO_TRANSLATE_buttonBrowse.UseVisualStyleBackColor = true;
            _NO_TRANSLATE_buttonBrowse.Click += buttonBrowse_Click;
            // 
            // FolderBrowserButton
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(_NO_TRANSLATE_buttonBrowse);
            Name = "FolderBrowserButton";
            Size = new Size(100, 25);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button _NO_TRANSLATE_buttonBrowse;
    }
}
