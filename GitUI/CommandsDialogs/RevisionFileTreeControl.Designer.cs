using GitUI.CommandsDialogs.Menus;

namespace GitUI.CommandsDialogs
{
    partial class RevisionFileTreeControl
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
            this.FileTreeSplitContainer = new System.Windows.Forms.SplitContainer();
            this.tvGitTree = new GitUI.UserControls.NativeTreeView();
            this.FileTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffWithRememberedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rememberFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToThisRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFileSystemActions = new System.Windows.Forms.ToolStripSeparator();
            this.openSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPathsToolStripMenuItem = new CopyPathsToolStripMenuItem();
            this.fileTreeOpenContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTreeCleanWorkingTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFileNameActions = new System.Windows.Forms.ToolStripSeparator();
            this.filterFileInGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorTopActions = new System.Windows.Forms.ToolStripSeparator();
            this.editCheckedOutFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorGitActions = new System.Windows.Forms.ToolStripSeparator();
            this.stopTrackingThisFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assumeUnchangedTheFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorGitTrackingActions = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileText = new GitUI.Editor.FileViewer();
            this.BlameControl = new Blame.BlameControl();
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
            this.FileTreeSplitContainer.Panel2.Controls.Add(this.BlameControl);
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
            this.openSubmoduleMenuItem,
            this.resetToThisRevisionToolStripMenuItem,
            this.toolStripSeparatorTopActions,
            this.openWithDifftoolToolStripMenuItem,
            this.diffWithRememberedFileToolStripMenuItem,
            this.rememberFileStripMenuItem,
            this.openWithToolStripMenuItem,
            this.openFileToolStripMenuItem,
            this.openFileWithToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.editCheckedOutFileToolStripMenuItem,
            this.toolStripSeparatorFileSystemActions,
            this.copyPathsToolStripMenuItem,
            this.fileTreeOpenContainingFolderToolStripMenuItem,
            this.toolStripSeparatorFileNameActions,
            this.filterFileInGridToolStripMenuItem,
            this.fileHistoryToolStripMenuItem,
            this.blameToolStripMenuItem1,
            this.fileTreeArchiveToolStripMenuItem,
            this.fileTreeCleanWorkingTreeToolStripMenuItem,
            this.toolStripSeparatorGitActions,
            this.stopTrackingThisFileToolStripMenuItem,
            this.assumeUnchangedTheFileToolStripMenuItem,
            this.toolStripSeparatorGitTrackingActions,
            this.findToolStripMenuItem,
            this.expandToolStripMenuItem,
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
            // diffWithRememberedFileToolStripMenuItem
            // 
            this.diffWithRememberedFileToolStripMenuItem.Name = "diffWithRememberedFileToolStripMenuItem";
            this.diffWithRememberedFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffWithRememberedFileToolStripMenuItem.Click += new System.EventHandler(this.diffWithRememberedFileToolStripMenuItem_Click);
            // 
            // rememberFileStripMenuItem
            // 
            this.rememberFileStripMenuItem.Name = "rememberFileStripMenuItem";
            this.rememberFileStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.rememberFileStripMenuItem.Text = "Remember file for diff";
            this.rememberFileStripMenuItem.Click += new System.EventHandler(this.rememberFileToolStripMenuItem_Click);
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
            this.openSubmoduleMenuItem.Text = TranslatedStrings.OpenWithGitExtensions;
            this.openSubmoduleMenuItem.Click += new System.EventHandler(this.openSubmoduleMenuItem_Click);
            // 
            // copyPathsToolStripMenuItem
            // 
            this.copyPathsToolStripMenuItem.Name = "copyPathsToolStripMenuItem";
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
            // toolStripSeparatorFileNameActions
            // 
            this.toolStripSeparatorFileNameActions.Name = "toolStripSeparatorFileNameActions";
            this.toolStripSeparatorFileNameActions.Size = new System.Drawing.Size(322, 6);
            // 
            // filterFileInGridToolStripMenuItem
            // 
            this.filterFileInGridToolStripMenuItem.Image = global::GitUI.Properties.Images.FunnelPencil;
            this.filterFileInGridToolStripMenuItem.Name = "filterFileInGridToolStripMenuItem";
            this.filterFileInGridToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.filterFileInGridToolStripMenuItem.Click += new System.EventHandler(this.filterFileInGridToolStripMenuItem_Click);
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
            // toolStripSeparatorTopActions
            // 
            this.toolStripSeparatorTopActions.Name = "toolStripSeparatorTopActions";
            this.toolStripSeparatorTopActions.Size = new System.Drawing.Size(322, 6);
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
            // toolStripSeparatorGitTrackingActions
            // 
            this.toolStripSeparatorGitTrackingActions.Name = "toolStripSeparatorGitTrackingActions";
            this.toolStripSeparatorGitTrackingActions.Size = new System.Drawing.Size(322, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Image = global::GitUI.Properties.Images.Preview;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.findToolStripMenuItem.Text = "Find in file tree...";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // expandSubtreeToolStripMenuItem
            // 
            this.expandToolStripMenuItem.Image = global::GitUI.Properties.Images.TreeExpandSubtree;
            this.expandToolStripMenuItem.Name = "expandToolStripMenuItem";
            this.expandToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.expandToolStripMenuItem.Text = "Expand";
            this.expandToolStripMenuItem.Click += new System.EventHandler(this.expandToolStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Image = global::GitUI.Properties.Images.TreeCollapseAll;
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(325, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
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
            // BlameControl
            // 
            this.BlameControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameControl.Location = new System.Drawing.Point(0, 0);
            this.BlameControl.Margin = new System.Windows.Forms.Padding(0);
            this.BlameControl.Name = "blameControl";
            this.BlameControl.Size = new System.Drawing.Size(487, 303);
            this.BlameControl.TabIndex = 0;
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
        private Blame.BlameControl BlameControl;
        private System.Windows.Forms.ContextMenuStrip FileTreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diffWithRememberedFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rememberFileStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToThisRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFileSystemActions;
        private System.Windows.Forms.ToolStripMenuItem openSubmoduleMenuItem;
        private CopyPathsToolStripMenuItem copyPathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeOpenContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileTreeCleanWorkingTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFileNameActions;
        private System.Windows.Forms.ToolStripMenuItem filterFileInGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorTopActions;
        private System.Windows.Forms.ToolStripMenuItem editCheckedOutFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorGitTrackingActions;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assumeUnchangedTheFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorGitActions;
        private System.Windows.Forms.ToolStripMenuItem stopTrackingThisFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandToolStripMenuItem;
    }
}
