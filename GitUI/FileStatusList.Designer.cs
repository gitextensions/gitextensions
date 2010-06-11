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
            this.FileStatusListBox = new System.Windows.Forms.ListBox();
            this.DiffFilesTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.NoFiles = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FileStatusListBox
            // 
            this.FileStatusListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileStatusListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FileStatusListBox.FormattingEnabled = true;
            this.FileStatusListBox.Location = new System.Drawing.Point(0, 0);
            this.FileStatusListBox.Name = "FileStatusListBox";
            this.FileStatusListBox.Size = new System.Drawing.Size(585, 420);
            this.FileStatusListBox.TabIndex = 0;
            this.FileStatusListBox.SizeChanged += new System.EventHandler(this.NoFiles_SizeChanged);
            this.FileStatusListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileStatusListBox_KeyDown);
            // 
            // NoFiles
            // 
            this.NoFiles.BackColor = System.Drawing.SystemColors.Window;
            this.NoFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
            this.NoFiles.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.NoFiles.Location = new System.Drawing.Point(5, 5);
            this.NoFiles.Margin = new System.Windows.Forms.Padding(0);
            this.NoFiles.Name = "NoFiles";
            this.NoFiles.Size = new System.Drawing.Size(172, 49);
            this.NoFiles.TabIndex = 1;
            this.NoFiles.Text = "No changes";
            // 
            // FileStatusList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.NoFiles);
            this.Controls.Add(this.FileStatusListBox);
            this.Name = "FileStatusList";
            this.Size = new System.Drawing.Size(585, 420);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox FileStatusListBox;
        private System.Windows.Forms.ToolTip DiffFilesTooltip;
        private System.Windows.Forms.Label NoFiles;
    }
}
