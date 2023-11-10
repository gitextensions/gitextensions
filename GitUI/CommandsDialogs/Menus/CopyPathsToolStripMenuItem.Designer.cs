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
            copyRelativePathsPosixToolStripMenuItem = new ToolStripMenuItem();
            copyRelativePathsNativeToolStripMenuItem = new ToolStripMenuItem();
            copyFullPathsNativeToolStripMenuItem = new ToolStripMenuItem();
            copyFullPathsWslToolStripMenuItem = new ToolStripMenuItem();
            copyFullPathsCygwinToolStripMenuItem = new ToolStripMenuItem();
            // 
            // CopyPathsToolStripMenuItem
            // 
            DropDownItems.AddRange(new ToolStripItem[] {
            copyRelativePathsPosixToolStripMenuItem,
            copyRelativePathsNativeToolStripMenuItem,
            copyFullPathsNativeToolStripMenuItem,
            copyFullPathsWslToolStripMenuItem,
            copyFullPathsCygwinToolStripMenuItem});
            Image = Properties.Images.CopyToClipboard;
            Name = "CopyPathsToolStripMenuItem";
            Size = new Size(262, 22);
            Text = "Copy &path(s)";
            Click += CopyFullPathsNativeToolStripMenuItem_Click;
            // 
            // copyRelativePathsPosixToolStripMenuItem
            // 
            copyRelativePathsPosixToolStripMenuItem.Name = "copyRelativePathsPosixToolStripMenuItem";
            copyRelativePathsPosixToolStripMenuItem.Size = new Size(247, 22);
            copyRelativePathsPosixToolStripMenuItem.Text = "Copy relative path(s) - &POSIX";
            copyRelativePathsPosixToolStripMenuItem.Click += CopyRelativePathsPosixToolStripMenuItem_Click;
            // 
            // copyRelativePathsNativeToolStripMenuItem
            // 
            copyRelativePathsNativeToolStripMenuItem.Name = "copyRelativePathsNativeToolStripMenuItem";
            copyRelativePathsNativeToolStripMenuItem.Size = new Size(247, 22);
            copyRelativePathsNativeToolStripMenuItem.Text = "Copy relative path(s) - &native";
            copyRelativePathsNativeToolStripMenuItem.Click += CopyRelativePathsNativeToolStripMenuItem_Click;
            // 
            // copyFullPathsNativeToolStripMenuItem
            // 
            copyFullPathsNativeToolStripMenuItem.Name = "copyFullPathsNativeToolStripMenuItem";
            copyFullPathsNativeToolStripMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.C)));
            copyFullPathsNativeToolStripMenuItem.Size = new Size(247, 22);
            copyFullPathsNativeToolStripMenuItem.Text = "Copy &full path(s) - native";
            copyFullPathsNativeToolStripMenuItem.Click += CopyFullPathsNativeToolStripMenuItem_Click;
            // 
            // copyFullPathsWslToolStripMenuItem
            // 
            copyFullPathsWslToolStripMenuItem.Name = "copyFullPathsWslToolStripMenuItem";
            copyFullPathsWslToolStripMenuItem.Size = new Size(247, 22);
            copyFullPathsWslToolStripMenuItem.Text = "Copy full path(s) - &WSL";
            copyFullPathsWslToolStripMenuItem.Click += CopyFullPathsWslToolStripMenuItem_Click;
            // 
            // copyFullPathsCygwinToolStripMenuItem
            // 
            copyFullPathsCygwinToolStripMenuItem.Name = "copyFullPathsCygwinToolStripMenuItem";
            copyFullPathsCygwinToolStripMenuItem.Size = new Size(247, 22);
            copyFullPathsCygwinToolStripMenuItem.Text = "Copy full path(s) - &Cygwin";
            copyFullPathsCygwinToolStripMenuItem.Click += CopyFullPathsCygwinToolStripMenuItem_Click;
        }

        #endregion

        private ToolStripMenuItem copyRelativePathsPosixToolStripMenuItem;
        private ToolStripMenuItem copyRelativePathsNativeToolStripMenuItem;
        private ToolStripMenuItem copyFullPathsNativeToolStripMenuItem;
        private ToolStripMenuItem copyFullPathsWslToolStripMenuItem;
        private ToolStripMenuItem copyFullPathsCygwinToolStripMenuItem;
    }
}
