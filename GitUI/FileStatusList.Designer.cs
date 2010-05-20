namespace GitUI
{
    partial class FileStatusList
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileStatusList));
            this.FileStatusListBox = new System.Windows.Forms.ListBox();
            this.DiffFilesTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.NoFiles = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FileStatusListBox
            // 
            this.FileStatusListBox.AccessibleDescription = null;
            this.FileStatusListBox.AccessibleName = null;
            resources.ApplyResources(this.FileStatusListBox, "FileStatusListBox");
            this.FileStatusListBox.BackgroundImage = null;
            this.FileStatusListBox.FormattingEnabled = true;
            this.FileStatusListBox.Name = "FileStatusListBox";
            this.DiffFilesTooltip.SetToolTip(this.FileStatusListBox, resources.GetString("FileStatusListBox.ToolTip"));
            this.FileStatusListBox.SizeChanged += new System.EventHandler(this.NoFiles_SizeChanged);
            // 
            // NoFiles
            // 
            this.NoFiles.AccessibleDescription = null;
            this.NoFiles.AccessibleName = null;
            resources.ApplyResources(this.NoFiles, "NoFiles");
            this.NoFiles.BackColor = System.Drawing.SystemColors.Window;
            this.NoFiles.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.NoFiles.Name = "NoFiles";
            this.DiffFilesTooltip.SetToolTip(this.NoFiles, resources.GetString("NoFiles.ToolTip"));
            // 
            // FileStatusList
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.NoFiles);
            this.Controls.Add(this.FileStatusListBox);
            this.Font = null;
            this.Name = "FileStatusList";
            this.DiffFilesTooltip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox FileStatusListBox;
        private System.Windows.Forms.ToolTip DiffFilesTooltip;
        private System.Windows.Forms.Label NoFiles;
    }
}
