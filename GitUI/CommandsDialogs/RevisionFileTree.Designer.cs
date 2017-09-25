namespace GitUI.CommandsDialogs
{
    partial class RevisionFileTree
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
            this.FileTreeSplitContainer = new System.Windows.Forms.SplitContainer();
            this.tvGitTree = new System.Windows.Forms.TreeView();
            this.FileText = new GitUI.Editor.FileViewer();
            this.FileTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToThisRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator30 = new System.Windows.Forms.ToolStripSeparator();
            this.openSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFilenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeOpenContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeCleanWorkingTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator31 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.editCheckedOutFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.FileTreeSplitContainer)).BeginInit();
            this.FileTreeSplitContainer.Panel1.SuspendLayout();
            this.FileTreeSplitContainer.Panel2.SuspendLayout();
            this.FileTreeSplitContainer.SuspendLayout();
            this.FileTreeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // FileTreeSplitContainer
            // 
            this.FileTreeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileTreeSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.FileTreeSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.FileTreeSplitContainer.Name = "FileTreeSplitContainer";
            // 
            // FileTreeSplitContainer.Panel1
            // 
            this.FileTreeSplitContainer.Panel1.Controls.Add(this.tvGitTree);
            // 
            // FileTreeSplitContainer.Panel2
            // 
            this.FileTreeSplitContainer.Panel2.Controls.Add(this.FileText);
            this.FileTreeSplitContainer.Size = new System.Drawing.Size(909, 251);
            this.FileTreeSplitContainer.SplitterDistance = 300;
            this.FileTreeSplitContainer.TabIndex = 1;
            // 
            // GitTree
            // 
            this.tvGitTree.ContextMenuStrip = this.FileTreeContextMenu;
            this.tvGitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvGitTree.HideSelection = false;
            this.tvGitTree.Location = new System.Drawing.Point(0, 0);
            this.tvGitTree.Name = "tvGitTree";
            this.tvGitTree.Size = new System.Drawing.Size(300, 251);
            this.tvGitTree.TabIndex = 0;
            this.tvGitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTree_BeforeExpand);
            this.tvGitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.GitTree_AfterSelect);
            this.tvGitTree.DoubleClick += new System.EventHandler(this.GitTree_DoubleClick);
            this.tvGitTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GitTree_KeyDown);
            this.tvGitTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GitTree_MouseDown);
            this.tvGitTree.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GitTree_MouseMove);
            // 
            // FileTreeContextMenu
            // 
            this.FileTreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.saveAsToolStripMenuItem,
                this.resetToThisRevisionToolStripMenuItem,
                this.toolStripSeparator30,
                this.openSubmoduleMenuItem,
                this.copyFilenameToClipboardToolStripMenuItem,
                this.fileTreeOpenContainingFolderToolStripMenuItem,
                this.fileTreeArchiveToolStripMenuItem,
                this.fileTreeCleanWorkingTreeToolStripMenuItem,
                this.toolStripSeparator31,
                this.fileHistoryToolStripMenuItem,
                this.blameToolStripMenuItem1,
                this.findToolStripMenuItem,
                this.toolStripSeparator20,
                this.editCheckedOutFileToolStripMenuItem,
                this.openFileToolStripMenuItem,
                this.openFileWithToolStripMenuItem,
                this.openWithToolStripMenuItem,
                this.toolStripSeparator18,
                this.expandAllToolStripMenuItem,
                this.collapseAllToolStripMenuItem});
            this.FileTreeContextMenu.Name = "FileTreeContextMenu";
            this.FileTreeContextMenu.Size = new System.Drawing.Size(297, 380);
            this.FileTreeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.FileTreeContextMenu_Opening);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // resetToThisRevisionToolStripMenuItem
            // 
            this.resetToThisRevisionToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetFileTo;
            this.resetToThisRevisionToolStripMenuItem.Name = "resetToThisRevisionToolStripMenuItem";
            this.resetToThisRevisionToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.resetToThisRevisionToolStripMenuItem.Text = "Reset to selected revision";
            this.resetToThisRevisionToolStripMenuItem.Click += new System.EventHandler(this.resetToThisRevisionToolStripMenuItem_Click);
            // 
            // toolStripSeparator30
            // 
            this.toolStripSeparator30.Name = "toolStripSeparator30";
            this.toolStripSeparator30.Size = new System.Drawing.Size(293, 6);
            // 
            // openSubmoduleMenuItem
            // 
            this.openSubmoduleMenuItem.Image = global::GitUI.Properties.Resources.IconFolderSubmodule;
            this.openSubmoduleMenuItem.Name = "openSubmoduleMenuItem";
            this.openSubmoduleMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openSubmoduleMenuItem.Text = "Open with Git Extensions";
            this.openSubmoduleMenuItem.Click += new System.EventHandler(this.openSubmoduleMenuItem_Click);
            // 
            // copyFilenameToClipboardToolStripMenuItem
            // 
            this.copyFilenameToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem.Name = "copyFilenameToClipboardToolStripMenuItem";
            this.copyFilenameToClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.copyFilenameToClipboardToolStripMenuItem.Text = "Copy full path";
            this.copyFilenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem_Click);
            // 
            // fileTreeOpenContainingFolderToolStripMenuItem
            // 
            this.fileTreeOpenContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.fileTreeOpenContainingFolderToolStripMenuItem.Name = "fileTreeOpenContainingFolderToolStripMenuItem";
            this.fileTreeOpenContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileTreeOpenContainingFolderToolStripMenuItem.Text = "Open containing folder";
            this.fileTreeOpenContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.fileTreeOpenContainingFolderToolStripMenuItem_Click);
            // 
            // fileTreeArchiveToolStripMenuItem
            // 
            this.fileTreeArchiveToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconArchiveRevision;
            this.fileTreeArchiveToolStripMenuItem.Name = "fileTreeArchiveToolStripMenuItem";
            this.fileTreeArchiveToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileTreeArchiveToolStripMenuItem.Text = "Archive...";
            this.fileTreeArchiveToolStripMenuItem.Click += new System.EventHandler(this.fileTreeArchiveToolStripMenuItem_Click);
            // 
            // fileTreeCleanWorkingTreeToolStripMenuItem
            // 
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCleanupRepo;
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Name = "fileTreeCleanWorkingTreeToolStripMenuItem";
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Text = "Clean working directory...";
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Click += new System.EventHandler(this.fileTreeCleanWorkingTreeToolStripMenuItem_Click);
            // 
            // toolStripSeparator31
            // 
            this.toolStripSeparator31.Name = "toolStripSeparator31";
            this.toolStripSeparator31.Size = new System.Drawing.Size(293, 6);
            // 
            // fileHistoryToolStripMenuItem
            // 
            this.fileHistoryToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            this.fileHistoryToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileHistoryToolStripMenuItem.Text = "File history";
            this.fileHistoryToolStripMenuItem.Click += new System.EventHandler(this.fileHistoryItem_Click);
            // 
            // blameToolStripMenuItem1
            // 
            this.blameToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconBlame;
            this.blameToolStripMenuItem1.Name = "blameToolStripMenuItem1";
            this.blameToolStripMenuItem1.Size = new System.Drawing.Size(296, 22);
            this.blameToolStripMenuItem1.Text = "Blame";
            this.blameToolStripMenuItem1.Click += new System.EventHandler(this.blameMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFind;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(293, 6);
            // 
            // editCheckedOutFileToolStripMenuItem
            // 
            this.editCheckedOutFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconEditFile;
            this.editCheckedOutFileToolStripMenuItem.Name = "editCheckedOutFileToolStripMenuItem";
            this.editCheckedOutFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.editCheckedOutFileToolStripMenuItem.Text = "Edit working directory file";
            this.editCheckedOutFileToolStripMenuItem.Click += new System.EventHandler(this.editCheckedOutFileToolStripMenuItem_Click);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openFileToolStripMenuItem.Text = "Open this revision (temp file)";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openFileWithToolStripMenuItem
            // 
            this.openFileWithToolStripMenuItem.Name = "openFileWithToolStripMenuItem";
            this.openFileWithToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openFileWithToolStripMenuItem.Text = "Open this revision with... (temp file)";
            this.openFileWithToolStripMenuItem.Click += new System.EventHandler(this.openFileWithToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openWithToolStripMenuItem.Text = "Open working directory file with...";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(293, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.expandAllToolStripMenuItem.Text = "Expand all (takes a while on large trees)";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse all";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.Location = new System.Drawing.Point(0, 0);
            this.FileText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FileText.Name = "FileText";
            this.FileText.Size = new System.Drawing.Size(605, 251);
            this.FileText.TabIndex = 0;
            //             // 
            // FileTreeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FileTreeSplitContainer);
            this.Name = "FileTreeControl";
            this.Size = new System.Drawing.Size(793, 303);
            this.FileTreeSplitContainer.Panel1.ResumeLayout(false);
            this.FileTreeSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FileTreeSplitContainer)).EndInit();
            this.FileTreeSplitContainer.ResumeLayout(false);
            this.FileTreeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer FileTreeSplitContainer;
        private System.Windows.Forms.TreeView tvGitTree;
        private Editor.FileViewer FileText;
        private System.Windows.Forms.ContextMenuStrip FileTreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToThisRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator30;
        private System.Windows.Forms.ToolStripMenuItem openSubmoduleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeOpenContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeCleanWorkingTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator31;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripMenuItem editCheckedOutFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
    }
}
