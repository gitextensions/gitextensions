namespace GitUI.UserControls.RevisionGridClasses
{
    partial class CopyContextMenuItem
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
            this.messageCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.branchNameCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagNameCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            // 
            // branchNameCopyToolStripMenuItem
            // 
            this.branchNameCopyToolStripMenuItem.Enabled = false;
            this.branchNameCopyToolStripMenuItem.Name = "branchNameCopyToolStripMenuItem";
            this.branchNameCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.branchNameCopyToolStripMenuItem.Text = "Branch name:";
            // 
            // tagNameCopyToolStripMenuItem
            // 
            this.tagNameCopyToolStripMenuItem.Enabled = false;
            this.tagNameCopyToolStripMenuItem.Name = "tagNameCopyToolStripMenuItem";
            this.tagNameCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.tagNameCopyToolStripMenuItem.Text = "Tag name:";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(185, 6);
            // 
            // hashCopyToolStripMenuItem
            // 
            this.hashCopyToolStripMenuItem.Name = "hashCopyToolStripMenuItem";
            this.hashCopyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.hashCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.hashCopyToolStripMenuItem.Text = "Commit hash";
            this.hashCopyToolStripMenuItem.Click += new System.EventHandler(this.HashToolStripMenuItemClick);
            // 
            // messageCopyToolStripMenuItem
            // 
            this.messageCopyToolStripMenuItem.Name = "messageCopyToolStripMenuItem";
            this.messageCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.messageCopyToolStripMenuItem.Text = "Message";
            this.messageCopyToolStripMenuItem.Click += new System.EventHandler(this.MessageToolStripMenuItemClick);
            // 
            // authorCopyToolStripMenuItem
            // 
            this.authorCopyToolStripMenuItem.Name = "authorCopyToolStripMenuItem";
            this.authorCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.authorCopyToolStripMenuItem.Text = "Author";
            this.authorCopyToolStripMenuItem.Click += new System.EventHandler(this.AuthorToolStripMenuItemClick);
            // 
            // dateCopyToolStripMenuItem
            // 
            this.dateCopyToolStripMenuItem.Name = "dateCopyToolStripMenuItem";
            this.dateCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.dateCopyToolStripMenuItem.Text = "Date";
            this.dateCopyToolStripMenuItem.Click += new System.EventHandler(this.DateToolStripMenuItemClick);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.branchNameCopyToolStripMenuItem,
                this.tagNameCopyToolStripMenuItem,
                this.toolStripSeparator6,
                this.hashCopyToolStripMenuItem,
                this.messageCopyToolStripMenuItem,
                this.authorCopyToolStripMenuItem,
                this.dateCopyToolStripMenuItem});
            this.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.Name = "copyToClipboardToolStripMenuItem";
            this.Size = new System.Drawing.Size(264, 24);
            this.Text = "Copy to clipboard";
            this.DropDownOpened += new System.EventHandler(this.copyToClipboardToolStripMenuItem_DropDownOpened);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem branchNameCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagNameCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem hashCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem messageCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem authorCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dateCopyToolStripMenuItem;
    }
}
