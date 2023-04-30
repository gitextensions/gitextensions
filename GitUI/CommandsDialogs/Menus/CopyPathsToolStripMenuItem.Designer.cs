namespace GitUI.CommandsDialogs.Menus
{
    partial class CopyPathsToolStripMenuItem
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
            components = new System.ComponentModel.Container();
            this.copyRelativePathsPosixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRelativePathsNativeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFullPathsNativeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFullPathsWslToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFullPathsCygwinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // CopyPathsToolStripMenuItem
            // 
            this.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyRelativePathsPosixToolStripMenuItem,
            this.copyRelativePathsNativeToolStripMenuItem,
            this.copyFullPathsNativeToolStripMenuItem,
            this.copyFullPathsWslToolStripMenuItem,
            this.copyFullPathsCygwinToolStripMenuItem});
            this.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.Name = "CopyPathsToolStripMenuItem";
            this.Size = new System.Drawing.Size(262, 22);
            this.Text = "Copy &path(s)";
            this.Click += new System.EventHandler(this.CopyFullPathsNativeToolStripMenuItem_Click);
            // 
            // copyRelativePathsPosixToolStripMenuItem
            // 
            this.copyRelativePathsPosixToolStripMenuItem.Name = "copyRelativePathsPosixToolStripMenuItem";
            this.copyRelativePathsPosixToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.copyRelativePathsPosixToolStripMenuItem.Text = "Copy relative path(s) - &POSIX";
            this.copyRelativePathsPosixToolStripMenuItem.Click += new System.EventHandler(this.CopyRelativePathsPosixToolStripMenuItem_Click);
            // 
            // copyRelativePathsNativeToolStripMenuItem
            // 
            this.copyRelativePathsNativeToolStripMenuItem.Name = "copyRelativePathsNativeToolStripMenuItem";
            this.copyRelativePathsNativeToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.copyRelativePathsNativeToolStripMenuItem.Text = "Copy relative path(s) - &native";
            this.copyRelativePathsNativeToolStripMenuItem.Click += new System.EventHandler(this.CopyRelativePathsNativeToolStripMenuItem_Click);
            // 
            // copyFullPathsNativeToolStripMenuItem
            // 
            this.copyFullPathsNativeToolStripMenuItem.Name = "copyFullPathsNativeToolStripMenuItem";
            this.copyFullPathsNativeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFullPathsNativeToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.copyFullPathsNativeToolStripMenuItem.Text = "Copy &full path(s) - native";
            this.copyFullPathsNativeToolStripMenuItem.Click += new System.EventHandler(this.CopyFullPathsNativeToolStripMenuItem_Click);
            // 
            // copyFullPathsWslToolStripMenuItem
            // 
            this.copyFullPathsWslToolStripMenuItem.Name = "copyFullPathsWslToolStripMenuItem";
            this.copyFullPathsWslToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.copyFullPathsWslToolStripMenuItem.Text = "Copy full path(s) - &WSL";
            this.copyFullPathsWslToolStripMenuItem.Click += new System.EventHandler(this.CopyFullPathsWslToolStripMenuItem_Click);
            // 
            // copyFullPathsCygwinToolStripMenuItem
            // 
            this.copyFullPathsCygwinToolStripMenuItem.Name = "copyFullPathsCygwinToolStripMenuItem";
            this.copyFullPathsCygwinToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.copyFullPathsCygwinToolStripMenuItem.Text = "Copy full path(s) - &Cygwin";
            this.copyFullPathsCygwinToolStripMenuItem.Click += new System.EventHandler(this.CopyFullPathsCygwinToolStripMenuItem_Click);
        }

        #endregion

        private ToolStripMenuItem copyRelativePathsPosixToolStripMenuItem;
        private ToolStripMenuItem copyRelativePathsNativeToolStripMenuItem;
        private ToolStripMenuItem copyFullPathsNativeToolStripMenuItem;
        private ToolStripMenuItem copyFullPathsWslToolStripMenuItem;
        private ToolStripMenuItem copyFullPathsCygwinToolStripMenuItem;
    }
}
