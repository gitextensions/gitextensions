#pragma warning disable 0628
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
            this.FileStatusListBox.FormattingEnabled = true;
            this.FileStatusListBox.ItemHeight = 15;
            this.FileStatusListBox.Location = new System.Drawing.Point(0, 0);
            this.FileStatusListBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FileStatusListBox.Name = "FileStatusListBox";
            this.FileStatusListBox.Size = new System.Drawing.Size(682, 485);
            this.FileStatusListBox.TabIndex = 0;
            this.FileStatusListBox.SizeChanged += new System.EventHandler(this.NoFiles_SizeChanged);
            this.FileStatusListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileStatusListBox_KeyDown);
            // 
            // NoFiles
            // 
            this.NoFiles.BackColor = System.Drawing.SystemColors.Window;
            this.NoFiles.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.NoFiles.Location = new System.Drawing.Point(6, 6);
            this.NoFiles.Margin = new System.Windows.Forms.Padding(0);
            this.NoFiles.Name = "NoFiles";
            this.NoFiles.Size = new System.Drawing.Size(201, 56);
            this.NoFiles.TabIndex = 1;
            this.NoFiles.Text = "No changes";
            // 
            // FileStatusList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.NoFiles);
            this.Controls.Add(this.FileStatusListBox);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FileStatusList";
            this.Size = new System.Drawing.Size(682, 485);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox FileStatusListBox;
		//This property cannot be private because this will break compilation in monodevelop
        protected System.Windows.Forms.ToolTip DiffFilesTooltip;
        private System.Windows.Forms.Label NoFiles;
    }
}
