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
            buttonBrowse = new Button();
            SuspendLayout();
            // 
            // buttonBrowse
            // 
            buttonBrowse.AutoSize = true;
            buttonBrowse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonBrowse.Dock = DockStyle.Fill;
            buttonBrowse.Image = Properties.Images.BrowseFileExplorer;
            buttonBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            buttonBrowse.Location = new Point(0, 0);
            buttonBrowse.MinimumSize = new Size(100, 25);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new Size(100, 25);
            buttonBrowse.TabIndex = 5;
            buttonBrowse.Text = "Browse...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += buttonBrowse_Click;
            // 
            // FolderBrowserButton
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(buttonBrowse);
            Name = "FolderBrowserButton";
            Size = new Size(100, 25);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button buttonBrowse;
    }
}
