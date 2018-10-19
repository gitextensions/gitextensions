namespace GitUI.CommandsDialogs
{
    partial class RevisionFileTreeControl
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
            this.tvGitTree = new GitUI.UserControls.NativeTreeView();
            this.FileTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToThisRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFileSystemActions = new System.Windows.Forms.ToolStripSeparator();
            this.openSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFilenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeOpenContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeCleanWorkingTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorGitHistoryActions = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorEditFileActions = new System.Windows.Forms.ToolStripSeparator();
            this.editCheckedOutFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorGitActions = new System.Windows.Forms.ToolStripSeparator();
            this.stopTrackingThisFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assumeUnchangedTheFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFileTreeActions = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandSubtreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileText = new GitUI.Editor.FileViewer();
            ((System.ComponentModel.ISupportInitialize)(this.FileTreeSplitContainer)).BeginInit();
            this.FileTreeSplitContainer.Panel1.SuspendLayout();
            this.FileTreeSplitContainer.Panel2.SuspendLayout();
            this.FileTreeSplitContainer.SuspendLayout();
            this.FileTreeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // FileTreeSplitContainer
            // 
            this.FileTreeSplitContainer.BackColor = System.Drawing.SystemColors.Control;
            this.FileTreeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileTreeSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.FileTreeSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.FileTreeSplitContainer.Name = "FileTreeSplitContainer";
            // 
            // FileTreeSplitContainer.Panel1
            // 
            this.FileTreeSplitContainer.Panel1.Controls.Add(this.tvGitTree);
            // 
            // FileTreeSplitContainer.Panel2
            // 
            this.FileTreeSplitContainer.Panel2.Controls.Add(this.FileText);
            this.FileTreeSplitContainer.Size = new System.Drawing.Size(793, 303);
            this.FileTreeSplitContainer.SplitterDistance = 300;
            this.FileTreeSplitContainer.SplitterWidth = 6;
            this.FileTreeSplitContainer.TabIndex = 1;
            // 
            // tvGitTree
            // 
            this.tvGitTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvGitTree.ContextMenuStrip = this.FileTreeContextMenu;
            this.tvGitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvGitTree.HideSelection = false;
            this.tvGitTree.Location = new System.Drawing.Point(0, 0);
            this.tvGitTree.Margin = new System.Windows.Forms.Padding(0);
            this.tvGitTree.Name = "tvGitTree";
            this.tvGitTree.Size = new System.Drawing.Size(300, 303);
            this.tvGitTree.TabIndex = 0;
            this.tvGitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvGitTree_BeforeExpand);
            this.tvGitTree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvGitTree_ItemDrag);
            this.tvGitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvGitTree_AfterSelect);
            this.tvGitTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvGitTree_NodeMouseClick);
            this.tvGitTree.DoubleClick += new System.EventHandler(this.tvGitTree_DoubleClick);
            this.tvGitTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvGitTree_KeyDown);
            // 
            // FileTreeContextMenu
            // 
            this.FileTreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.resetToThisRevisionToolStripMenuItem,
            this.toolStripSeparatorFileSystemActions,
            this.openSubmoduleMenuItem,
            this.copyFilenameToClipboardToolStripMenuItem,
            this.fileTreeOpenContainingFolderToolStripMenuItem,
            this.fileTreeArchiveToolStripMenuItem,
            this.fileTreeCleanWorkingTreeToolStripMenuItem,
            this.toolStripSeparatorGitHistoryActions,
            this.fileHistoryToolStripMenuItem,
            this.blameToolStripMenuItem1,
            this.toolStripSeparatorEditFileActions,
            this.editCheckedOutFileToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.openFileToolStripMenuItem,
            this.openFileWithToolStripMenuItem,
            this.toolStripSeparatorGitActions,
            this.stopTrackingThisFileToolStripMenuItem,
            this.assumeUnchangedTheFileToolStripMenuItem,
            this.toolStripSeparatorFileTreeActions,
            this.findToolStripMenuItem,
            this.expandSubtreeToolStripMenuItem,
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem});
            this.FileTreeContextMenu.Name = "FileTreeContextMenu";
            this.FileTreeContextMenu.Size = new System.Drawing.Size(326, 474);
            this.FileTreeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.FileTreeContextMenu_Opening);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Images.Diff;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::GitUI.Properties.Images.SaveAs;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // resetToThisRevisionToolStripMenuItem
            // 
            this.resetToThisRevisionToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetFileTo;
            this.resetToThisRevisionToolStripMenuItem.Name = "resetToThisRevisionToolStripMenuItem";
            this.resetToThisRevisionToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.resetToThisRevisionToolStripMenuItem.Text = "Reset to selected revision";
            this.resetToThisRevisionToolStripMenuItem.Click += new System.EventHandler(this.resetToThisRevisionToolStripMenuItem_Click);
            // 
            // toolStripSeparatorFileSystemActions
            // 
            this.toolStripSeparatorFileSystemActions.Name = "toolStripSeparatorFileSystemActions";
            this.toolStripSeparatorFileSystemActions.Size = new System.Drawing.Size(322, 6);
            // 
            // openSubmoduleMenuItem
            // 
            this.openSubmoduleMenuItem.Image = global::GitUI.Properties.Images.GitExtensionsLogo16;
            this.openSubmoduleMenuItem.Name = "openSubmoduleMenuItem";
            this.openSubmoduleMenuItem.Size = new System.Drawing.Size(325, 22);
            this.openSubmoduleMenuItem.Text = "Open with Git Extensions";
            this.openSubmoduleMenuItem.Click += new System.EventHandler(this.openSubmoduleMenuItem_Click);
            // 
            // copyFilenameToClipboardToolStripMenuItem
            // 
            this.copyFilenameToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem.Name = "copyFilenameToClipboardToolStripMenuItem";
            this.copyFilenameToClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.copyFilenameToClipboardToolStripMenuItem.Text = "Copy full path";
            this.copyFilenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem_Click);
            // 
            // fileTreeOpenContainingFolderToolStripMenuItem
            // 
            this.fileTreeOpenContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.fileTreeOpenContainingFolderToolStripMenuItem.Name = "fileTreeOpenContainingFolderToolStripMenuItem";
            this.fileTreeOpenContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.fileTreeOpenContainingFolderToolStripMenuItem.Text = "Show in folder";
            this.fileTreeOpenContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.fileTreeOpenContainingFolderToolStripMenuItem_Click);
            // 
            // fileTreeArchiveToolStripMenuItem
            // 
            this.fileTreeArchiveToolStripMenuItem.Image = global::GitUI.Properties.Images.ArchiveRevision;
            this.fileTreeArchiveToolStripMenuItem.Name = "fileTreeArchiveToolStripMenuItem";
            this.fileTreeArchiveToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.fileTreeArchiveToolStripMenuItem.Text = "Archive...";
            this.fileTreeArchiveToolStripMenuItem.Click += new System.EventHandler(this.fileTreeArchiveToolStripMenuItem_Click);
            // 
            // fileTreeCleanWorkingTreeToolStripMenuItem
            // 
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Image = global::GitUI.Properties.Images.CleanupRepo;
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Name = "fileTreeCleanWorkingTreeToolStripMenuItem";
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Text = "Clean this folder in the working directory...";
            this.fileTreeCleanWorkingTreeToolStripMenuItem.Click += new System.EventHandler(this.fileTreeCleanWorkingTreeToolStripMenuItem_Click);
            // 
            // toolStripSeparatorGitHistoryActions
            // 
            this.toolStripSeparatorGitHistoryActions.Name = "toolStripSeparatorGitHistoryActions";
            this.toolStripSeparatorGitHistoryActions.Size = new System.Drawing.Size(322, 6);
            // 
            // fileHistoryToolStripMenuItem
            // 
            this.fileHistoryToolStripMenuItem.Image = global::GitUI.Properties.Images.FileHistory;
            this.fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            this.fileHistoryToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.fileHistoryToolStripMenuItem.Text = "View history";
            this.fileHistoryToolStripMenuItem.Click += new System.EventHandler(this.fileHistoryItem_Click);
            // 
            // blameToolStripMenuItem1
            // 
            this.blameToolStripMenuItem1.Image = global::GitUI.Properties.Images.Blame;
            this.blameToolStripMenuItem1.Name = "blameToolStripMenuItem1";
            this.blameToolStripMenuItem1.Size = new System.Drawing.Size(325, 22);
            this.blameToolStripMenuItem1.Text = "Blame";
            this.blameToolStripMenuItem1.Click += new System.EventHandler(this.blameMenuItem_Click);
            // 
            // toolStripSeparatorEditFileActions
            // 
            this.toolStripSeparatorEditFileActions.Name = "toolStripSeparatorEditFileActions";
            this.toolStripSeparatorEditFileActions.Size = new System.Drawing.Size(322, 6);
            // 
            // editCheckedOutFileToolStripMenuItem
            // 
            this.editCheckedOutFileToolStripMenuItem.Image = global::GitUI.Properties.Images.EditFile;
            this.editCheckedOutFileToolStripMenuItem.Name = "editCheckedOutFileToolStripMenuItem";
            this.editCheckedOutFileToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.editCheckedOutFileToolStripMenuItem.Text = "Edit working directory file";
            this.editCheckedOutFileToolStripMenuItem.Click += new System.EventHandler(this.editCheckedOutFileToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Image = global::GitUI.Properties.Images.EditFile;
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.openWithToolStripMenuItem.Text = "Open working directory file with...";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Image = global::GitUI.Properties.Images.ViewFile;
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.openFileToolStripMenuItem.Text = "Open this revision (temp file)";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openFileWithToolStripMenuItem
            // 
            this.openFileWithToolStripMenuItem.Image = global::GitUI.Properties.Images.ViewFile;
            this.openFileWithToolStripMenuItem.Name = "openFileWithToolStripMenuItem";
            this.openFileWithToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.openFileWithToolStripMenuItem.Text = "Open this revision with... (temp file)";
            this.openFileWithToolStripMenuItem.Click += new System.EventHandler(this.openFileWithToolStripMenuItem_Click);
            // 
            // toolStripSeparatorGitActions
            // 
            this.toolStripSeparatorGitActions.Name = "toolStripSeparatorGitActions";
            this.toolStripSeparatorGitActions.Size = new System.Drawing.Size(322, 6);
            // 
            // stopTrackingThisFileToolStripMenuItem
            // 
            this.stopTrackingThisFileToolStripMenuItem.Image = global::GitUI.Properties.Images.StopTrackingFile;
            this.stopTrackingThisFileToolStripMenuItem.Name = "stopTrackingThisFileToolStripMenuItem";
            this.stopTrackingThisFileToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.stopTrackingThisFileToolStripMenuItem.Text = "Stop tracking this file";
            this.stopTrackingThisFileToolStripMenuItem.Click += new System.EventHandler(this.stopTrackingToolStripMenuItem_Click);
            // 
            // assumeUnchangedTheFileToolStripMenuItem
            // 
            this.assumeUnchangedTheFileToolStripMenuItem.Image = global::GitUI.Properties.Images.AddToGitIgnore;
            this.assumeUnchangedTheFileToolStripMenuItem.Name = "assumeUnchangedTheFileToolStripMenuItem";
            this.assumeUnchangedTheFileToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.assumeUnchangedTheFileToolStripMenuItem.Text = "Assume unchanged this file";
            this.assumeUnchangedTheFileToolStripMenuItem.Click += new System.EventHandler(this.assumeUnchangedToolStripMenuItem_Click);
            // 
            // toolStripSeparatorFileTreeActions
            // 
            this.toolStripSeparatorFileTreeActions.Name = "toolStripSeparatorFileTreeActions";
            this.toolStripSeparatorFileTreeActions.Size = new System.Drawing.Size(322, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Image = global::GitUI.Properties.Images.Preview;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.findToolStripMenuItem.Text = "Find in file tree...";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // expandSubtreeToolStripMenuItem
            // 
            this.expandSubtreeToolStripMenuItem.Image = global::GitUI.Properties.Images.TreeExpandSubtree;
            this.expandSubtreeToolStripMenuItem.Name = "expandSubtreeToolStripMenuItem";
            this.expandSubtreeToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.expandSubtreeToolStripMenuItem.Text = "Expand subtree (takes a while on large subtrees)";
            this.expandSubtreeToolStripMenuItem.Click += new System.EventHandler(this.expandSubtreeToolStripMenuItem_Click);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Image = global::GitUI.Properties.Images.TreeExpandAll;
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.expandAllToolStripMenuItem.Text = "Expand all (takes a while on large trees)";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Image = global::GitUI.Properties.Images.TreeCollapseAll;
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse all";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.Location = new System.Drawing.Point(0, 0);
            this.FileText.Margin = new System.Windows.Forms.Padding(0);
            this.FileText.Name = "FileText";
            this.FileText.Size = new System.Drawing.Size(487, 303);
            this.FileText.TabIndex = 0;
            // 
            // RevisionFileTreeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FileTreeSplitContainer);
            this.Name = "RevisionFileTreeControl";
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
        private UserControls.NativeTreeView tvGitTree;
        private Editor.FileViewer FileText;
        private System.Windows.Forms.ContextMenuStrip FileTreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToThisRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFileSystemActions;
        private System.Windows.Forms.ToolStripMenuItem openSubmoduleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeOpenContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeCleanWorkingTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorGitHistoryActions;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorEditFileActions;
        private System.Windows.Forms.ToolStripMenuItem editCheckedOutFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFileTreeActions;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assumeUnchangedTheFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorGitActions;
        private System.Windows.Forms.ToolStripMenuItem stopTrackingThisFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandSubtreeToolStripMenuItem;
    }
}
