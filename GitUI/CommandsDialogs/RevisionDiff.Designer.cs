using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    partial class RevisionDiff
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
            this.DiffSplitContainer = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parentOfALocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parentOfBLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToFirstToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToSecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToParentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unstageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickSelectedDiffFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffEditFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffDeleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffToolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.diffCommitSubmoduleChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.diffResetSubmoduleChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.diffStashSubmoduleChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffUpdateSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffSubmoduleSummaryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFilenameToClipboardToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffShowInFileTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator33 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryDiffToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findInDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.DiffSplitContainer)).BeginInit();
            this.DiffSplitContainer.Panel1.SuspendLayout();
            this.DiffSplitContainer.Panel2.SuspendLayout();
            this.DiffSplitContainer.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // DiffSplitContainer
            // 
            this.DiffSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.DiffSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.DiffSplitContainer.Name = "DiffSplitContainer";
            // 
            // DiffSplitContainer.Panel1
            // 
            this.DiffSplitContainer.Panel1.Controls.Add(this.DiffFiles);
            // 
            // DiffSplitContainer.Panel2
            // 
            this.DiffSplitContainer.Panel2.Controls.Add(this.DiffText);
            this.DiffSplitContainer.Size = new System.Drawing.Size(729, 360);
            this.DiffSplitContainer.SplitterDistance = 300;
            this.DiffSplitContainer.TabIndex = 0;
            // 
            // DiffFiles
            // 
            this.DiffFiles.ContextMenuStrip = this.DiffContextMenu;
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.FilterVisible = false;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(300, 360);
            this.DiffFiles.TabIndex = 1;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFiles_SelectedIndexChanged);
            this.DiffFiles.DataSourceChanged += new System.EventHandler(this.DiffFiles_DataSourceChanged);
            this.DiffFiles.DoubleClick += new System.EventHandler(this.DiffFiles_DoubleClick);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.saveAsToolStripMenuItem1,
            this.resetFileToToolStripMenuItem,
            this.stageFileToolStripMenuItem,
            this.unstageFileToolStripMenuItem,
            this.cherryPickSelectedDiffFileToolStripMenuItem,
            this.toolStripSeparator32,
            this.diffEditFileToolStripMenuItem,
            this.diffDeleteFileToolStripMenuItem,
            this.diffCommitSubmoduleChanges,
            this.diffResetSubmoduleChanges,
            this.diffStashSubmoduleChangesToolStripMenuItem,
            this.diffUpdateSubmoduleMenuItem,
            this.diffSubmoduleSummaryMenuItem,
            this.diffToolStripSeparator13,
            this.copyFilenameToClipboardToolStripMenuItem1,
            this.openContainingFolderToolStripMenuItem,
            this.diffShowInFileTreeToolStripMenuItem,
            this.toolStripSeparator33,
            this.fileHistoryDiffToolstripMenuItem,
            this.blameToolStripMenuItem,
            this.findInDiffToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(211, 236);
            this.DiffContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.DiffContextMenu_Opening);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aBToolStripMenuItem,
            this.aLocalToolStripMenuItem,
            this.bLocalToolStripMenuItem,
            this.parentOfALocalToolStripMenuItem,
            this.parentOfBLocalToolStripMenuItem});
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconDiffTool;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.DropDownOpening += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_DropDownOpening);
            // 
            // aBToolStripMenuItem
            // 
            this.aBToolStripMenuItem.Name = "aBToolStripMenuItem";
            this.aBToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.aBToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.aBToolStripMenuItem.Text = "A <--> B";
            this.aBToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // aLocalToolStripMenuItem
            // 
            this.aLocalToolStripMenuItem.Name = "aLocalToolStripMenuItem";
            this.aLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.aLocalToolStripMenuItem.Text = "A <--> Working directory";
            this.aLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // bLocalToolStripMenuItem
            // 
            this.bLocalToolStripMenuItem.Name = "bLocalToolStripMenuItem";
            this.bLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.bLocalToolStripMenuItem.Text = "B <--> Working directory";
            this.bLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // parentOfALocalToolStripMenuItem
            // 
            this.parentOfALocalToolStripMenuItem.Name = "parentOfALocalToolStripMenuItem";
            this.parentOfALocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.parentOfALocalToolStripMenuItem.Text = "A\'s parent <--> Working directory";
            this.parentOfALocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // parentOfBLocalToolStripMenuItem
            // 
            this.parentOfBLocalToolStripMenuItem.Name = "parentOfBLocalToolStripMenuItem";
            this.parentOfBLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.parentOfBLocalToolStripMenuItem.Text = "B\'s parent <--> Working directory";
            this.parentOfBLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem1
            // 
            this.saveAsToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            this.saveAsToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.saveAsToolStripMenuItem1.Text = "Save (B) as...";
            this.saveAsToolStripMenuItem1.Click += new System.EventHandler(this.saveAsToolStripMenuItem1_Click);
            // 
            // resetFileToToolStripMenuItem
            // 
            this.resetFileToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetFileToFirstToolStripMenuItem,
            this.resetFileToSecondToolStripMenuItem,
            this.resetFileToSelectedToolStripMenuItem,
            this.resetFileToParentToolStripMenuItem});
            this.resetFileToToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetFileTo;
            this.resetFileToToolStripMenuItem.Name = "resetFileToToolStripMenuItem";
            this.resetFileToToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.resetFileToToolStripMenuItem.Text = "Reset file(s) to";
            this.resetFileToToolStripMenuItem.DropDownOpening += new System.EventHandler(this.resetFileToToolStripMenuItem_DropDownOpening);
            // 
            // resetFileToFirstToolStripMenuItem
            // 
            this.resetFileToFirstToolStripMenuItem.Name = "resetFileToFirstToolStripMenuItem";
            this.resetFileToFirstToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToFirstToolStripMenuItem.Text = "First";
            this.resetFileToFirstToolStripMenuItem.Click += new System.EventHandler(this.resetFileToFirstToolStripMenuItem_Click);
            // 
            // resetFileToSecondToolStripMenuItem
            // 
            this.resetFileToSecondToolStripMenuItem.Name = "resetFileToSecondToolStripMenuItem";
            this.resetFileToSecondToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToSecondToolStripMenuItem.Text = "Second";
            this.resetFileToSecondToolStripMenuItem.Click += new System.EventHandler(this.resetFileToSecondToolStripMenuItem_Click);
            // 
            // resetFileToSelectedToolStripMenuItem
            // 
            this.resetFileToSelectedToolStripMenuItem.Name = "resetFileToSelectedToolStripMenuItem";
            this.resetFileToSelectedToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToSelectedToolStripMenuItem.Text = "Selected";
            this.resetFileToSelectedToolStripMenuItem.Click += new System.EventHandler(this.resetFileToSelectedToolStripMenuItem_Click);
            // 
            // resetFileToParentToolStripMenuItem
            // 
            this.resetFileToParentToolStripMenuItem.Name = "resetFileToParentToolStripMenuItem";
            this.resetFileToParentToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.resetFileToParentToolStripMenuItem.Text = "Parent";
            this.resetFileToParentToolStripMenuItem.Click += new System.EventHandler(this.resetFileToParentToolStripMenuItem_Click);
            // 
            // stageFileToolStripMenuItem
            // 
            this.stageFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconStage;
            this.stageFileToolStripMenuItem.Name = "stageFileToolStripMenuItem";
            this.stageFileToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.stageFileToolStripMenuItem.Text = "Stage file(s)";
            this.stageFileToolStripMenuItem.Click += new System.EventHandler(this.StageFileToolStripMenuItemClick);
            // 
            // unstageFileToolStripMenuItem
            // 
            this.unstageFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconUnstage;
            this.unstageFileToolStripMenuItem.Name = "unstageFileToolStripMenuItem";
            this.unstageFileToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.unstageFileToolStripMenuItem.Text = "Unstage file(s)";
            this.unstageFileToolStripMenuItem.Click += new System.EventHandler(this.UnstageFileToolStripMenuItemClick);
            // 
            // cherryPickSelectedDiffFileToolStripMenuItem
            // 
            this.cherryPickSelectedDiffFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCherryPick;
            this.cherryPickSelectedDiffFileToolStripMenuItem.Name = "cherryPickSelectedDiffFileToolStripMenuItem";
            this.cherryPickSelectedDiffFileToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.cherryPickSelectedDiffFileToolStripMenuItem.Text = "Cherry pick file\'s changes";
            this.cherryPickSelectedDiffFileToolStripMenuItem.Click += new System.EventHandler(this.cherryPickSelectedDiffFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(207, 6);
            //
            // diffEditFileToolStripMenuItem
            // 
            this.diffEditFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconEditFile;
            this.diffEditFileToolStripMenuItem.Name = "diffEditFileToolStripMenuItem";
            this.diffEditFileToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.diffEditFileToolStripMenuItem.Text = "Edit file";
            this.diffEditFileToolStripMenuItem.Click += new System.EventHandler(this.diffEditFileToolStripMenuItem_Click);
            // 
            // diffDeleteFileToolStripMenuItem
            // 
            this.diffDeleteFileToolStripMenuItem.Name = "diffDeleteFileToolStripMenuItem";
            this.diffDeleteFileToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.diffDeleteFileToolStripMenuItem.Text = "Delete file";
            this.diffDeleteFileToolStripMenuItem.Click += new System.EventHandler(this.diffDeleteFileToolStripMenuItem_Click);
            // 
            // diffCommitSubmoduleChanges
            // 
            this.diffCommitSubmoduleChanges.Image = global::GitUI.Properties.Resources.IconDirtySubmodules;
            this.diffCommitSubmoduleChanges.Name = "diffCommitSubmoduleChanges";
            this.diffCommitSubmoduleChanges.Size = new System.Drawing.Size(228, 22);
            this.diffCommitSubmoduleChanges.Text = "Commit submodule changes";
            this.diffCommitSubmoduleChanges.Click += new System.EventHandler(this.diffCommitSubmoduleChanges_Click);
            // 
            // diffResetSubmoduleChanges
            // 
            this.diffResetSubmoduleChanges.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.diffResetSubmoduleChanges.Name = "diffResetSubmoduleChanges";
            this.diffResetSubmoduleChanges.Size = new System.Drawing.Size(228, 22);
            this.diffResetSubmoduleChanges.Text = "Reset submodule changes";
            this.diffResetSubmoduleChanges.Click += new System.EventHandler(this.diffResetSubmoduleChanges_Click);
            // 
            // diffStashSubmoduleChangesToolStripMenuItem
            // 
            this.diffStashSubmoduleChangesToolStripMenuItem.Image = global::GitUI.Properties.Resources.stash;
            this.diffStashSubmoduleChangesToolStripMenuItem.Name = "diffStashSubmoduleChangesToolStripMenuItem";
            this.diffStashSubmoduleChangesToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.diffStashSubmoduleChangesToolStripMenuItem.Text = "Stash submodule changes";
            this.diffStashSubmoduleChangesToolStripMenuItem.Click += new System.EventHandler(this.diffStashSubmoduleChangesToolStripMenuItem_Click);
            // 
            // diffUpdateSubmoduleMenuItem
            // 
            this.diffUpdateSubmoduleMenuItem.Image = global::GitUI.Properties.Resources.IconSubmodulesUpdate;
            this.diffUpdateSubmoduleMenuItem.Name = "diffUpdateSubmoduleMenuItem";
            this.diffUpdateSubmoduleMenuItem.Size = new System.Drawing.Size(228, 22);
            this.diffUpdateSubmoduleMenuItem.Tag = "1";
            this.diffUpdateSubmoduleMenuItem.Text = "Update submodule";
            this.diffUpdateSubmoduleMenuItem.Click += new System.EventHandler(this.diffUpdateSubmoduleMenuItem_Click);
            // 
            // diffSubmoduleSummaryMenuItem
            // 
            this.diffSubmoduleSummaryMenuItem.Name = "diffSubmoduleSummaryMenuItem";
            this.diffSubmoduleSummaryMenuItem.Size = new System.Drawing.Size(228, 22);
            this.diffSubmoduleSummaryMenuItem.Text = "View summary";
            this.diffSubmoduleSummaryMenuItem.Click += new System.EventHandler(this.diffSubmoduleSummaryMenuItem_Click);
            // 
            // diffToolStripSeparator13
            // 
            this.diffToolStripSeparator13.Name = "diffToolStripSeparator13";
            this.diffToolStripSeparator13.Size = new System.Drawing.Size(225, 6);
            this.diffToolStripSeparator13.Tag = "1";
            // 
            // copyFilenameToClipboardToolStripMenuItem1
            // 
            this.copyFilenameToClipboardToolStripMenuItem1.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem1.Name = "copyFilenameToClipboardToolStripMenuItem1";
            this.copyFilenameToClipboardToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.copyFilenameToClipboardToolStripMenuItem1.Text = "Copy full path(s)";
            this.copyFilenameToClipboardToolStripMenuItem1.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem1_Click);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open containing folder(s)";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // diffShowInFileTreeToolStripMenuItem
            // 
            this.diffShowInFileTreeToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFileTree;
            this.diffShowInFileTreeToolStripMenuItem.Name = "diffShowInFileTreeToolStripMenuItem";
            this.diffShowInFileTreeToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.diffShowInFileTreeToolStripMenuItem.Text = "Show in File tree";
            this.diffShowInFileTreeToolStripMenuItem.Click += new System.EventHandler(this.diffShowInFileTreeToolStripMenuItem_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(207, 6);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            this.fileHistoryDiffToolstripMenuItem.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            this.fileHistoryDiffToolstripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.fileHistoryDiffToolstripMenuItem.Text = "File history";
            this.fileHistoryDiffToolstripMenuItem.Click += new System.EventHandler(this.fileHistoryDiffToolstripMenuItem_Click);
            // 
            // blameToolStripMenuItem
            // 
            this.blameToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBlame;
            this.blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            this.blameToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.blameToolStripMenuItem.Text = "Blame";
            this.blameToolStripMenuItem.Click += new System.EventHandler(this.blameToolStripMenuItem_Click);
            // 
            // findInDiffToolStripMenuItem
            // 
            this.findInDiffToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFind;
            this.findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            this.findInDiffToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findInDiffToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.findInDiffToolStripMenuItem.Text = "Find";
            this.findInDiffToolStripMenuItem.Click += new System.EventHandler(this.findInDiffToolStripMenuItem_Click);
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffText.Name = "DiffText";
            this.DiffText.Size = new System.Drawing.Size(425, 360);
            this.DiffText.TabIndex = 0;
            this.DiffText.ExtraDiffArgumentsChanged += new System.EventHandler<System.EventArgs>(this.DiffText_ExtraDiffArgumentsChanged);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // RevisionDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DiffSplitContainer);
            this.Name = "RevisionDiff";
            this.Size = new System.Drawing.Size(729, 360);
            this.DiffSplitContainer.Panel1.ResumeLayout(false);
            this.DiffSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DiffSplitContainer)).EndInit();
            this.DiffSplitContainer.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ToolStripMenuItem resetFileToParentToolStripMenuItem;
        private ToolStripMenuItem resetFileToSelectedToolStripMenuItem;
        private ToolStripMenuItem resetFileToSecondToolStripMenuItem;
        private ToolStripMenuItem resetFileToFirstToolStripMenuItem;
        private ToolStripMenuItem parentOfALocalToolStripMenuItem;
        private ToolStripMenuItem parentOfBLocalToolStripMenuItem;
        private ToolStripMenuItem bLocalToolStripMenuItem;
        private ToolStripMenuItem aLocalToolStripMenuItem;
        private ToolStripMenuItem aBToolStripMenuItem;
        private ToolStripMenuItem findInDiffToolStripMenuItem;
        private ToolStripMenuItem blameToolStripMenuItem;
        private ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private ToolStripSeparator toolStripSeparator33;
        private ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem1;
        private ToolStripMenuItem stageFileToolStripMenuItem;
        private ToolStripMenuItem unstageFileToolStripMenuItem;
        private ToolStripMenuItem cherryPickSelectedDiffFileToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator32;
        private ToolStripMenuItem diffEditFileToolStripMenuItem;
        private ToolStripMenuItem diffDeleteFileToolStripMenuItem;
        private ToolStripMenuItem diffUpdateSubmoduleMenuItem;
        private ToolStripMenuItem diffSubmoduleSummaryMenuItem;
        private ToolStripMenuItem diffResetSubmoduleChanges;
        private ToolStripMenuItem diffCommitSubmoduleChanges;
        private ToolStripMenuItem diffStashSubmoduleChangesToolStripMenuItem;
        private ToolStripSeparator diffToolStripSeparator13;
        private ToolStripMenuItem resetFileToToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.SplitContainer DiffSplitContainer;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
    }
}
