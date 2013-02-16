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
            if (disposing && (components != null))
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
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Directory = new System.Windows.Forms.ComboBox();
            this.Load = new System.Windows.Forms.Button();
            this.folderBrowserButton1 = new GitUI.UserControls.FolderBrowserButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Directory";
            // 
            // _NO_TRANSLATE_Directory
            // 
            this._NO_TRANSLATE_Directory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Directory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Directory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_Directory.FormattingEnabled = true;
            this._NO_TRANSLATE_Directory.Location = new System.Drawing.Point(85, 10);
            this._NO_TRANSLATE_Directory.Name = "_NO_TRANSLATE_Directory";
            this._NO_TRANSLATE_Directory.Size = new System.Drawing.Size(229, 23);
            this._NO_TRANSLATE_Directory.TabIndex = 1;
            this._NO_TRANSLATE_Directory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DirectoryKeyPress);
            // 
            // Load
            // 
            this.Load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Load.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Load.Image = global::GitUI.Properties.Resources.IconRepoOpen;
            this.Load.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Load.Location = new System.Drawing.Point(296, 45);
            this.Load.Name = "Load";
            this.Load.Size = new System.Drawing.Size(134, 25);
            this.Load.TabIndex = 3;
            this.Load.Text = "Open";
            this.Load.UseVisualStyleBackColor = true;
            this.Load.Click += new System.EventHandler(this.LoadClick);
            // 
            // folderBrowserButton1
            // 
            this.folderBrowserButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserButton1.Location = new System.Drawing.Point(320, 8);
            this.folderBrowserButton1.Name = "folderBrowserButton1";
            this.folderBrowserButton1.PathShowingControl = this._NO_TRANSLATE_Directory;
            this.folderBrowserButton1.Size = new System.Drawing.Size(110, 25);
            this.folderBrowserButton1.TabIndex = 4;
            // 
            // Open
            // 
            this.AcceptButton = this.Load;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(434, 82);
            this.Controls.Add(this.folderBrowserButton1);
            this.Controls.Add(this.Load);
            this.Controls.Add(this._NO_TRANSLATE_Directory);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 120);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 120);
            this.Name = "FormOpenDirectory";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open local repository";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Open_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Directory;
        private new System.Windows.Forms.Button Load;
        private UserControls.FolderBrowserButton folderBrowserButton1;
    }
}