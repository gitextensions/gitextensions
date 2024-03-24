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
            components = new System.ComponentModel.Container();
            FileTreeSplitContainer = new SplitContainer();
            tvGitTree = new GitUI.UserControls.NativeTreeView();
            FileTreeContextMenu = new ContextMenuStrip(components);
            openWithDifftoolToolStripMenuItem = new ToolStripMenuItem();
            diffWithRememberedFileToolStripMenuItem = new ToolStripMenuItem();
            rememberFileStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            resetToThisRevisionToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorFileSystemActions = new ToolStripSeparator();
            openSubmoduleMenuItem = new ToolStripMenuItem();
            copyPathsToolStripMenuItem = new CopyPathsToolStripMenuItem();
            fileTreeOpenContainingFolderToolStripMenuItem = new ToolStripMenuItem();
            fileTreeArchiveToolStripMenuItem = new ToolStripMenuItem();
            fileTreeCleanWorkingTreeToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorFileNameActions = new ToolStripSeparator();
            filterFileInGridToolStripMenuItem = new ToolStripMenuItem();
            fileHistoryToolStripMenuItem = new ToolStripMenuItem();
            blameToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparatorTopActions = new ToolStripSeparator();
            editCheckedOutFileToolStripMenuItem = new ToolStripMenuItem();
            openWithToolStripMenuItem = new ToolStripMenuItem();
            openFileToolStripMenuItem = new ToolStripMenuItem();
            openFileWithToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorGitActions = new ToolStripSeparator();
            stopTrackingThisFileToolStripMenuItem = new ToolStripMenuItem();
            assumeUnchangedTheFileToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorGitTrackingActions = new ToolStripSeparator();
            findToolStripMenuItem = new ToolStripMenuItem();
            expandToolStripMenuItem = new ToolStripMenuItem();
            collapseAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorScript = new ToolStripSeparator();
            runScriptToolStripMenuItem = new ToolStripMenuItem();
            FileText = new GitUI.Editor.FileViewer();
            BlameControl = new Blame.BlameControl();
            ((System.ComponentModel.ISupportInitialize)(FileTreeSplitContainer)).BeginInit();
            FileTreeSplitContainer.Panel1.SuspendLayout();
            FileTreeSplitContainer.Panel2.SuspendLayout();
            FileTreeSplitContainer.SuspendLayout();
            FileTreeContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // FileTreeSplitContainer
            // 
            FileTreeSplitContainer.BackColor = SystemColors.Control;
            FileTreeSplitContainer.Dock = DockStyle.Fill;
            FileTreeSplitContainer.FixedPanel = FixedPanel.Panel1;
            FileTreeSplitContainer.Location = new Point(0, 0);
            FileTreeSplitContainer.Name = "FileTreeSplitContainer";
            // 
            // FileTreeSplitContainer.Panel1
            // 
            FileTreeSplitContainer.Panel1.Controls.Add(tvGitTree);
            // 
            // FileTreeSplitContainer.Panel2
            // 
            FileTreeSplitContainer.Panel2.Controls.Add(FileText);
            FileTreeSplitContainer.Panel2.Controls.Add(BlameControl);
            FileTreeSplitContainer.Size = new Size(793, 303);
            FileTreeSplitContainer.SplitterDistance = 300;
            FileTreeSplitContainer.SplitterWidth = 6;
            FileTreeSplitContainer.TabIndex = 1;
            // 
            // tvGitTree
            // 
            tvGitTree.BorderStyle = BorderStyle.None;
            tvGitTree.ContextMenuStrip = FileTreeContextMenu;
            tvGitTree.Dock = DockStyle.Fill;
            tvGitTree.HideSelection = false;
            tvGitTree.Location = new Point(0, 0);
            tvGitTree.Margin = new Padding(0);
            tvGitTree.Name = "tvGitTree";
            tvGitTree.Size = new Size(300, 303);
            tvGitTree.TabIndex = 0;
            tvGitTree.BeforeExpand += tvGitTree_BeforeExpand;
            tvGitTree.ItemDrag += tvGitTree_ItemDrag;
            tvGitTree.AfterSelect += tvGitTree_AfterSelect;
            tvGitTree.NodeMouseClick += tvGitTree_NodeMouseClick;
            tvGitTree.DoubleClick += tvGitTree_DoubleClick;
            tvGitTree.KeyDown += tvGitTree_KeyDown;
            // 
            // FileTreeContextMenu
            // 
            FileTreeContextMenu.Items.AddRange(new ToolStripItem[] {
            openSubmoduleMenuItem,
            resetToThisRevisionToolStripMenuItem,
            toolStripSeparatorTopActions,
            openWithDifftoolToolStripMenuItem,
            diffWithRememberedFileToolStripMenuItem,
            rememberFileStripMenuItem,
            openWithToolStripMenuItem,
            openFileToolStripMenuItem,
            openFileWithToolStripMenuItem,
            saveAsToolStripMenuItem,
            editCheckedOutFileToolStripMenuItem,
            toolStripSeparatorFileSystemActions,
            copyPathsToolStripMenuItem,
            fileTreeOpenContainingFolderToolStripMenuItem,
            toolStripSeparatorFileNameActions,
            filterFileInGridToolStripMenuItem,
            fileHistoryToolStripMenuItem,
            blameToolStripMenuItem1,
            fileTreeArchiveToolStripMenuItem,
            fileTreeCleanWorkingTreeToolStripMenuItem,
            toolStripSeparatorGitActions,
            stopTrackingThisFileToolStripMenuItem,
            assumeUnchangedTheFileToolStripMenuItem,
            toolStripSeparatorGitTrackingActions,
            findToolStripMenuItem,
            expandToolStripMenuItem,
            collapseAllToolStripMenuItem,
            toolStripSeparatorScript,
            runScriptToolStripMenuItem});
            FileTreeContextMenu.Name = "FileTreeContextMenu";
            FileTreeContextMenu.Size = new Size(326, 474);
            FileTreeContextMenu.Opening += FileTreeContextMenu_Opening;
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            openWithDifftoolToolStripMenuItem.Image = Properties.Images.Diff;
            openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            openWithDifftoolToolStripMenuItem.Size = new Size(325, 22);
            openWithDifftoolToolStripMenuItem.Text = "Open with &difftool";
            openWithDifftoolToolStripMenuItem.Click += openWithDifftoolToolStripMenuItem_Click;
            // 
            // diffWithRememberedFileToolStripMenuItem
            // 
            diffWithRememberedFileToolStripMenuItem.Name = "diffWithRememberedFileToolStripMenuItem";
            diffWithRememberedFileToolStripMenuItem.Size = new Size(296, 22);
            diffWithRememberedFileToolStripMenuItem.Click += diffWithRememberedFileToolStripMenuItem_Click;
            // 
            // rememberFileStripMenuItem
            // 
            rememberFileStripMenuItem.Name = "rememberFileStripMenuItem";
            rememberFileStripMenuItem.Size = new Size(296, 22);
            rememberFileStripMenuItem.Text = "Re&member file for diff";
            rememberFileStripMenuItem.Click += rememberFileToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Image = Properties.Images.SaveAs;
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(325, 22);
            saveAsToolStripMenuItem.Text = "S&ave as...";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // resetToThisRevisionToolStripMenuItem
            // 
            resetToThisRevisionToolStripMenuItem.Image = Properties.Images.ResetFileTo;
            resetToThisRevisionToolStripMenuItem.Name = "resetToThisRevisionToolStripMenuItem";
            resetToThisRevisionToolStripMenuItem.Size = new Size(325, 22);
            resetToThisRevisionToolStripMenuItem.Text = "&Reset to selected revision";
            resetToThisRevisionToolStripMenuItem.Click += resetToThisRevisionToolStripMenuItem_Click;
            // 
            // toolStripSeparatorFileSystemActions
            // 
            toolStripSeparatorFileSystemActions.Name = "toolStripSeparatorFileSystemActions";
            toolStripSeparatorFileSystemActions.Size = new Size(322, 6);
            // 
            // openSubmoduleMenuItem
            // 
            openSubmoduleMenuItem.Image = Properties.Images.GitExtensionsLogo16;
            openSubmoduleMenuItem.Name = "openSubmoduleMenuItem";
            openSubmoduleMenuItem.Size = new Size(325, 22);
            openSubmoduleMenuItem.Text = TranslatedStrings.OpenWithGitExtensions;
            openSubmoduleMenuItem.Click += openSubmoduleMenuItem_Click;
            // 
            // copyPathsToolStripMenuItem
            // 
            copyPathsToolStripMenuItem.Name = "copyPathsToolStripMenuItem";
            // 
            // fileTreeOpenContainingFolderToolStripMenuItem
            // 
            fileTreeOpenContainingFolderToolStripMenuItem.Image = Properties.Images.BrowseFileExplorer;
            fileTreeOpenContainingFolderToolStripMenuItem.Name = "fileTreeOpenContainingFolderToolStripMenuItem";
            fileTreeOpenContainingFolderToolStripMenuItem.Size = new Size(325, 22);
            fileTreeOpenContainingFolderToolStripMenuItem.Text = "Show &in folder";
            fileTreeOpenContainingFolderToolStripMenuItem.Click += fileTreeOpenContainingFolderToolStripMenuItem_Click;
            // 
            // fileTreeArchiveToolStripMenuItem
            // 
            fileTreeArchiveToolStripMenuItem.Image = Properties.Images.ArchiveRevision;
            fileTreeArchiveToolStripMenuItem.Name = "fileTreeArchiveToolStripMenuItem";
            fileTreeArchiveToolStripMenuItem.Size = new Size(325, 22);
            fileTreeArchiveToolStripMenuItem.Text = "Archi&ve...";
            fileTreeArchiveToolStripMenuItem.Click += fileTreeArchiveToolStripMenuItem_Click;
            // 
            // fileTreeCleanWorkingTreeToolStripMenuItem
            // 
            fileTreeCleanWorkingTreeToolStripMenuItem.Image = Properties.Images.CleanupRepo;
            fileTreeCleanWorkingTreeToolStripMenuItem.Name = "fileTreeCleanWorkingTreeToolStripMenuItem";
            fileTreeCleanWorkingTreeToolStripMenuItem.Size = new Size(325, 22);
            fileTreeCleanWorkingTreeToolStripMenuItem.Text = "&Clean this folder in the working directory...";
            fileTreeCleanWorkingTreeToolStripMenuItem.Click += fileTreeCleanWorkingTreeToolStripMenuItem_Click;
            // 
            // toolStripSeparatorFileNameActions
            // 
            toolStripSeparatorFileNameActions.Name = "toolStripSeparatorFileNameActions";
            toolStripSeparatorFileNameActions.Size = new Size(322, 6);
            // 
            // filterFileInGridToolStripMenuItem
            // 
            filterFileInGridToolStripMenuItem.Image = Properties.Images.FunnelPencil;
            filterFileInGridToolStripMenuItem.Name = "filterFileInGridToolStripMenuItem";
            filterFileInGridToolStripMenuItem.Size = new Size(296, 22);
            filterFileInGridToolStripMenuItem.Click += filterFileInGridToolStripMenuItem_Click;
            // 
            // fileHistoryToolStripMenuItem
            // 
            fileHistoryToolStripMenuItem.Image = Properties.Images.FileHistory;
            fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            fileHistoryToolStripMenuItem.Size = new Size(325, 22);
            fileHistoryToolStripMenuItem.Text = "View &history";
            fileHistoryToolStripMenuItem.Click += fileHistoryItem_Click;
            // 
            // blameToolStripMenuItem1
            // 
            blameToolStripMenuItem1.Image = Properties.Images.Blame;
            blameToolStripMenuItem1.Name = "blameToolStripMenuItem1";
            blameToolStripMenuItem1.Size = new Size(325, 22);
            blameToolStripMenuItem1.Text = "&Blame";
            blameToolStripMenuItem1.Click += blameMenuItem_Click;
            // 
            // toolStripSeparatorTopActions
            // 
            toolStripSeparatorTopActions.Name = "toolStripSeparatorTopActions";
            toolStripSeparatorTopActions.Size = new Size(322, 6);
            // 
            // editCheckedOutFileToolStripMenuItem
            // 
            editCheckedOutFileToolStripMenuItem.Image = Properties.Images.EditFile;
            editCheckedOutFileToolStripMenuItem.Name = "editCheckedOutFileToolStripMenuItem";
            editCheckedOutFileToolStripMenuItem.Size = new Size(325, 22);
            editCheckedOutFileToolStripMenuItem.Text = "&Edit working directory file";
            editCheckedOutFileToolStripMenuItem.Click += editCheckedOutFileToolStripMenuItem_Click;
            // 
            // openWithToolStripMenuItem
            // 
            openWithToolStripMenuItem.Image = Properties.Images.EditFile;
            openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            openWithToolStripMenuItem.Size = new Size(325, 22);
            openWithToolStripMenuItem.Text = "&Open working directory file with...";
            openWithToolStripMenuItem.Click += openWithToolStripMenuItem_Click;
            // 
            // openFileToolStripMenuItem
            // 
            openFileToolStripMenuItem.Image = Properties.Images.ViewFile;
            openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            openFileToolStripMenuItem.Size = new Size(325, 22);
            openFileToolStripMenuItem.Text = "Ope&n this revision (temp file)";
            openFileToolStripMenuItem.Click += openFileToolStripMenuItem_Click;
            // 
            // openFileWithToolStripMenuItem
            // 
            openFileWithToolStripMenuItem.Image = Properties.Images.ViewFile;
            openFileWithToolStripMenuItem.Name = "openFileWithToolStripMenuItem";
            openFileWithToolStripMenuItem.Size = new Size(325, 22);
            openFileWithToolStripMenuItem.Text = "Open this revision &with... (temp file)";
            openFileWithToolStripMenuItem.Click += openFileWithToolStripMenuItem_Click;
            // 
            // toolStripSeparatorGitActions
            // 
            toolStripSeparatorGitActions.Name = "toolStripSeparatorGitActions";
            toolStripSeparatorGitActions.Size = new Size(322, 6);
            // 
            // stopTrackingThisFileToolStripMenuItem
            // 
            stopTrackingThisFileToolStripMenuItem.Image = Properties.Images.StopTrackingFile;
            stopTrackingThisFileToolStripMenuItem.Name = "stopTrackingThisFileToolStripMenuItem";
            stopTrackingThisFileToolStripMenuItem.Size = new Size(325, 22);
            stopTrackingThisFileToolStripMenuItem.Text = "Stop &tracking this file";
            stopTrackingThisFileToolStripMenuItem.Click += stopTrackingToolStripMenuItem_Click;
            // 
            // assumeUnchangedTheFileToolStripMenuItem
            // 
            assumeUnchangedTheFileToolStripMenuItem.Image = Properties.Images.AddToGitIgnore;
            assumeUnchangedTheFileToolStripMenuItem.Name = "assumeUnchangedTheFileToolStripMenuItem";
            assumeUnchangedTheFileToolStripMenuItem.Size = new Size(325, 22);
            assumeUnchangedTheFileToolStripMenuItem.Text = "Assume &unchanged this file";
            assumeUnchangedTheFileToolStripMenuItem.Click += assumeUnchangedToolStripMenuItem_Click;
            // 
            // toolStripSeparatorGitTrackingActions
            // 
            toolStripSeparatorGitTrackingActions.Name = "toolStripSeparatorGitTrackingActions";
            toolStripSeparatorGitTrackingActions.Size = new Size(322, 6);
            // 
            // findToolStripMenuItem
            // 
            findToolStripMenuItem.Image = Properties.Images.FileTree;
            findToolStripMenuItem.Name = "findToolStripMenuItem";
            findToolStripMenuItem.Size = new Size(325, 22);
            findToolStripMenuItem.Text = "&Find file...";
            findToolStripMenuItem.Click += findToolStripMenuItem_Click;
            // 
            // expandSubtreeToolStripMenuItem
            // 
            expandToolStripMenuItem.Image = Properties.Images.TreeExpandSubtree;
            expandToolStripMenuItem.Name = "expandToolStripMenuItem";
            expandToolStripMenuItem.Size = new Size(325, 22);
            expandToolStripMenuItem.Text = "&Expand";
            expandToolStripMenuItem.Click += expandToolStripMenuItem_Click;
            // 
            // collapseAllToolStripMenuItem
            // 
            collapseAllToolStripMenuItem.Image = Properties.Images.TreeCollapseAll;
            collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            collapseAllToolStripMenuItem.Size = new Size(325, 22);
            collapseAllToolStripMenuItem.Text = "Co&llapse all";
            collapseAllToolStripMenuItem.Click += collapseAllToolStripMenuItem_Click;
            // 
            // toolStripSeparatorScript
            // 
            toolStripSeparatorScript.Name = "toolStripSeparatorScript";
            toolStripSeparatorScript.Size = new Size(259, 6);
            // 
            // runScriptToolStripMenuItem
            // 
            runScriptToolStripMenuItem.Image = Properties.Images.Console;
            runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            runScriptToolStripMenuItem.Size = new Size(262, 22);
            runScriptToolStripMenuItem.Text = "Run script";
            // 
            // FileText
            // 
            FileText.Dock = DockStyle.Fill;
            FileText.Location = new Point(0, 0);
            FileText.Margin = new Padding(0);
            FileText.Name = "FileText";
            FileText.Size = new Size(487, 303);
            FileText.TabIndex = 0;
            // 
            // BlameControl
            // 
            BlameControl.Dock = DockStyle.Fill;
            BlameControl.Location = new Point(0, 0);
            BlameControl.Margin = new Padding(0);
            BlameControl.Name = "blameControl";
            BlameControl.Size = new Size(487, 303);
            BlameControl.TabIndex = 0;
            // 
            // RevisionFileTreeControl
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(FileTreeSplitContainer);
            Name = "RevisionFileTreeControl";
            Size = new Size(793, 303);
            FileTreeSplitContainer.Panel1.ResumeLayout(false);
            FileTreeSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(FileTreeSplitContainer)).EndInit();
            FileTreeSplitContainer.ResumeLayout(false);
            FileTreeContextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer FileTreeSplitContainer;
        private UserControls.NativeTreeView tvGitTree;
        private Editor.FileViewer FileText;
        private Blame.BlameControl BlameControl;
        private ContextMenuStrip FileTreeContextMenu;
        private ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private ToolStripMenuItem diffWithRememberedFileToolStripMenuItem;
        private ToolStripMenuItem rememberFileStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem resetToThisRevisionToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorFileSystemActions;
        private ToolStripMenuItem openSubmoduleMenuItem;
        private CopyPathsToolStripMenuItem copyPathsToolStripMenuItem;
        private ToolStripMenuItem fileTreeOpenContainingFolderToolStripMenuItem;
        private ToolStripMenuItem fileTreeArchiveToolStripMenuItem;
        private ToolStripMenuItem fileTreeCleanWorkingTreeToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorFileNameActions;
        private ToolStripMenuItem filterFileInGridToolStripMenuItem;
        private ToolStripMenuItem fileHistoryToolStripMenuItem;
        private ToolStripMenuItem blameToolStripMenuItem1;
        private ToolStripMenuItem findToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorTopActions;
        private ToolStripMenuItem editCheckedOutFileToolStripMenuItem;
        private ToolStripMenuItem openWithToolStripMenuItem;
        private ToolStripMenuItem openFileToolStripMenuItem;
        private ToolStripMenuItem openFileWithToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorGitTrackingActions;
        private ToolStripMenuItem collapseAllToolStripMenuItem;
        private ToolStripMenuItem assumeUnchangedTheFileToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorGitActions;
        private ToolStripMenuItem stopTrackingThisFileToolStripMenuItem;
        private ToolStripMenuItem expandToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorScript;
        private ToolStripMenuItem runScriptToolStripMenuItem;
    }
}
