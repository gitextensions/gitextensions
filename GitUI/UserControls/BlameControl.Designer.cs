namespace GitUI.Blame
{
    partial class BlameControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CommitInfo = new GitUI.CommitInfo.CommitInfo();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.BlameAuthor = new GitUI.Editor.FileViewer();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.blameRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blamePreviousRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitHashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allCommitInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BlameFile = new GitUI.Editor.FileViewer();
            this.blameTooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CommitInfo);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(858, 740);
            this.splitContainer1.SplitterDistance = 160;
            this.splitContainer1.TabIndex = 7;
            // 
            // CommitInfo
            // 
            this.CommitInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitInfo.Location = new System.Drawing.Point(0, 0);
            this.CommitInfo.Margin = new System.Windows.Forms.Padding(0);
            this.CommitInfo.Name = "CommitInfo";
            this.CommitInfo.Size = new System.Drawing.Size(858, 160);
            this.CommitInfo.TabIndex = 5;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.BlameAuthor);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.BlameFile);
            this.splitContainer2.Size = new System.Drawing.Size(858, 576);
            this.splitContainer2.SplitterDistance = 186;
            this.splitContainer2.TabIndex = 0;
            // 
            // BlameCommitter
            // 
            this.BlameAuthor.ContextMenuStrip = this.contextMenu;
            this.BlameAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameAuthor.IsReadOnly = false;
            this.BlameAuthor.Location = new System.Drawing.Point(0, 0);
            this.BlameAuthor.Margin = new System.Windows.Forms.Padding(0);
            this.BlameAuthor.Name = "BlameAuthor";
            this.BlameAuthor.Size = new System.Drawing.Size(186, 576);
            this.BlameAuthor.TabIndex = 5;
            this.BlameAuthor.TabStop = false;
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blameRevisionToolStripMenuItem,
            this.blamePreviousRevisionToolStripMenuItem,
            this.showChangesToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyToClipboardToolStripMenuItem});
            this.contextMenu.Name = "ContextMenu";
            this.contextMenu.Size = new System.Drawing.Size(239, 76);
            this.contextMenu.Opened += new System.EventHandler(this.contextMenu_Opened);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(235, 6);
            this.blameRevisionToolStripMenuItem.Image = global::GitUI.Properties.Resources.Blame;
            this.blameRevisionToolStripMenuItem.Name = "blameRevisionToolStripMenuItem";
            this.blameRevisionToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.blameRevisionToolStripMenuItem.Text = "Blame this revision";
            this.blameRevisionToolStripMenuItem.Click += new System.EventHandler(this.blameRevisionToolStripMenuItem_Click);
            // 
            // blamePreviousRevisionToolStripMenuItem
            // 
            this.blamePreviousRevisionToolStripMenuItem.Image = global::GitUI.Properties.Resources.RecentRepositories;
            this.blamePreviousRevisionToolStripMenuItem.Name = "blamePreviousRevisionToolStripMenuItem";
            this.blamePreviousRevisionToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.blamePreviousRevisionToolStripMenuItem.Text = "Blame previous revision";
            this.blamePreviousRevisionToolStripMenuItem.Click += new System.EventHandler(this.blamePreviousRevisionToolStripMenuItem_Click);
            // 
            // showChangesToolStripMenuItem
            // 
            this.showChangesToolStripMenuItem.Image = global::GitUI.Properties.Resources.information;
            this.showChangesToolStripMenuItem.Name = "showChangesToolStripMenuItem";
            this.showChangesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.showChangesToolStripMenuItem.Text = "Show changes";
            this.showChangesToolStripMenuItem.Click += new System.EventHandler(this.showChangesToolStripMenuItem_Click);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitHashToolStripMenuItem,
            this.commitMessageToolStripMenuItem,
            this.allCommitInfoToolStripMenuItem});
            this.copyToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.CopyToClipboard;
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            // 
            // commitHashToolStripMenuItem
            // 
            this.commitHashToolStripMenuItem.Image = global::GitUI.Properties.Resources.CommitId;
            this.commitHashToolStripMenuItem.Name = "commitHashToolStripMenuItem";
            this.commitHashToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.commitHashToolStripMenuItem.Text = "Commit hash";
            this.commitHashToolStripMenuItem.Click += new System.EventHandler(this.copyCommitHashToClipboardToolStripMenuItem_Click);
            // 
            // commitMessageToolStripMenuItem
            // 
            this.commitMessageToolStripMenuItem.Image = global::GitUI.Properties.Resources.Message;
            this.commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            this.commitMessageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.commitMessageToolStripMenuItem.Text = "Commit message";
            this.commitMessageToolStripMenuItem.Click += new System.EventHandler(this.copyLogMessageToolStripMenuItem_Click);
            // 
            // allCommitInfoToolStripMenuItem
            // 
            this.allCommitInfoToolStripMenuItem.Image = global::GitUI.Properties.Resources.CommitSummary;
            this.allCommitInfoToolStripMenuItem.Name = "allCommitInfoToolStripMenuItem";
            this.allCommitInfoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.allCommitInfoToolStripMenuItem.Text = "All commit info";
            this.allCommitInfoToolStripMenuItem.Click += new System.EventHandler(this.copyAllCommitInfoToClipboardToolStripMenuItem_Click);
            // 
            // BlameFile
            // 
            this.BlameFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameFile.IsReadOnly = false;
            this.BlameFile.Location = new System.Drawing.Point(0, 0);
            this.BlameFile.Margin = new System.Windows.Forms.Padding(0);
            this.BlameFile.Name = "BlameFile";
            this.BlameFile.Size = new System.Drawing.Size(666, 576);
            this.BlameFile.TabIndex = 0;
            // 
            // BlameControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BlameControl";
            this.Size = new System.Drawing.Size(858, 740);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private CommitInfo.CommitInfo CommitInfo;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private GitUI.Editor.FileViewer BlameAuthor;
        private GitUI.Editor.FileViewer BlameFile;
        private System.Windows.Forms.ToolTip blameTooltip;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem blamePreviousRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commitHashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commitMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allCommitInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameRevisionToolStripMenuItem;
    }
}
