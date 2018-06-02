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
            this.separatorAfterRefNames = new System.Windows.Forms.ToolStripSeparator();
            this.branchNameCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagNameCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            // 
            // branchNameCopyToolStripMenuItem
            // 
            this.branchNameCopyToolStripMenuItem.Enabled = false;
            this.branchNameCopyToolStripMenuItem.Name = "branchNameCopyToolStripMenuItem";
            this.branchNameCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.branchNameCopyToolStripMenuItem.Text = "Branch name:";
            this.branchNameCopyToolStripMenuItem.Tag = "caption";
            // 
            // tagNameCopyToolStripMenuItem
            // 
            this.tagNameCopyToolStripMenuItem.Enabled = false;
            this.tagNameCopyToolStripMenuItem.Name = "tagNameCopyToolStripMenuItem";
            this.tagNameCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.tagNameCopyToolStripMenuItem.Text = "Tag name:";
            this.tagNameCopyToolStripMenuItem.Tag = "caption";
            // 
            // separatorAfterRefNames
            // 
            this.separatorAfterRefNames.Name = "separatorAfterRefNames";
            this.separatorAfterRefNames.Size = new System.Drawing.Size(185, 6);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.branchNameCopyToolStripMenuItem,
                this.tagNameCopyToolStripMenuItem,
                this.separatorAfterRefNames});
            this.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.Name = "copyToClipboardToolStripMenuItem";
            this.Size = new System.Drawing.Size(264, 24);
            this.Text = "Copy to clipboard";
            this.DropDownOpened += new System.EventHandler(this.copyToClipboardToolStripMenuItem_DropDownOpened);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem branchNameCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagNameCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator separatorAfterRefNames;
    }
}
