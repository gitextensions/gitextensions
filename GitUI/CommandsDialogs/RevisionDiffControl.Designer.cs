using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    partial class RevisionDiffControl
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
            if (disposing && (components is not null))
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
            this.diffRememberStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.diffTwoSelectedDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffWithRememberedDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rememberSecondRevDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rememberFirstRevDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.secondDiffCaptionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstDiffCaptionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstToSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFileToParentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unstageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickSelectedDiffFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.diffEditWorkingDirectoryFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffOpenRevisionFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffOpenRevisionFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffDeleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffCommitSubmoduleChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.diffResetSubmoduleChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.diffStashSubmoduleChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffUpdateSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submoduleStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.diffToolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
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
            this.DiffSplitContainer.BackColor = System.Drawing.SystemColors.Control;
            this.DiffSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.DiffSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.DiffSplitContainer.Margin = new System.Windows.Forms.Padding(0);
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
            this.DiffSplitContainer.SplitterWidth = 6;
            this.DiffSplitContainer.TabIndex = 0;
            // 
            // DiffFiles
            // 
            this.DiffFiles.ContextMenuStrip = this.DiffContextMenu;
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(0);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.SelectFirstItemOnSetItems = false;
            this.DiffFiles.Size = new System.Drawing.Size(300, 360);
            this.DiffFiles.TabIndex = 1;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFiles_SelectedIndexChanged);
            this.DiffFiles.DataSourceChanged += new System.EventHandler(this.DiffFiles_DataSourceChanged);
            this.DiffFiles.DoubleClick += new System.EventHandler(this.DiffFiles_DoubleClick);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diffUpdateSubmoduleMenuItem,
            this.diffResetSubmoduleChanges,
            this.diffStashSubmoduleChangesToolStripMenuItem,
            this.diffCommitSubmoduleChanges,
            this.submoduleStripSeparator,
            this.stageFileToolStripMenuItem,
            this.unstageFileToolStripMenuItem,
            this.resetFileToToolStripMenuItem,
            this.cherryPickSelectedDiffFileToolStripMenuItem,
            this.toolStripSeparator32,
            this.openWithDifftoolToolStripMenuItem,
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem,
            this.diffOpenRevisionFileToolStripMenuItem,
            this.diffOpenRevisionFileWithToolStripMenuItem,
            this.saveAsToolStripMenuItem1,
            this.diffEditWorkingDirectoryFileToolStripMenuItem,
            this.diffDeleteFileToolStripMenuItem,
            this.diffToolStripSeparator13,
            this.copyFilenameToClipboardToolStripMenuItem1,
            this.openContainingFolderToolStripMenuItem,
            this.diffShowInFileTreeToolStripMenuItem,
            this.toolStripSeparator33,
            this.fileHistoryDiffToolstripMenuItem,
            this.blameToolStripMenuItem,
            this.findInDiffToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(297, 506);
            this.DiffContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.DiffContextMenu_Opening);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.secondDiffCaptionMenuItem,
                this.firstDiffCaptionMenuItem,
                this.firstToSelectedToolStripMenuItem,
                this.selectedToLocalToolStripMenuItem,
                this.firstToLocalToolStripMenuItem,
                this.diffRememberStripSeparator,
                this.diffTwoSelectedDifftoolToolStripMenuItem,
                this.diffWithRememberedDifftoolToolStripMenuItem,
                this.rememberSecondRevDiffToolStripMenuItem,
                this.rememberFirstRevDiffToolStripMenuItem
            });
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Images.Diff;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.DropDownOpening += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_DropDownOpening);
            // 
            // diffRememberStripSeparator
            // 
            this.diffRememberStripSeparator.Name = "diffRememberStripSeparator";
            this.diffRememberStripSeparator.Size = new System.Drawing.Size(293, 6);
            // 
            // diffTwoSelectedDifftoolToolStripMenuItem
            // 
            this.diffTwoSelectedDifftoolToolStripMenuItem.Name = "diffTwoSelectedDifftoolToolStripMenuItem";
            this.diffTwoSelectedDifftoolToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffTwoSelectedDifftoolToolStripMenuItem.Text = "Diff the selected files";
            this.diffTwoSelectedDifftoolToolStripMenuItem.Click += new System.EventHandler(this.diffTwoSelectedDiffToolToolStripMenuItem_Click);
            // 
            // diffWithRememberedDifftoolToolStripMenuItem
            // 
            this.diffWithRememberedDifftoolToolStripMenuItem.Name = "diffWithRememberedDifftoolToolStripMenuItem";
            this.diffWithRememberedDifftoolToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffWithRememberedDifftoolToolStripMenuItem.Click += new System.EventHandler(this.diffWithRememberedDiffToolToolStripMenuItem_Click);
            // 
            // rememberSecondRevDiffToolStripMenuItem
            // 
            this.rememberSecondRevDiffToolStripMenuItem.Name = "rememberSecondRevDiffToolStripMenuItem";
            this.rememberSecondRevDiffToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.rememberSecondRevDiffToolStripMenuItem.Text = "Remember Second for diff";
            this.rememberSecondRevDiffToolStripMenuItem.Click += new System.EventHandler(this.rememberSecondDiffToolToolStripMenuItem_Click);
            // 
            // rememberFirstRevDiffToolStripMenuItem
            // 
            this.rememberFirstRevDiffToolStripMenuItem.Name = "rememberFirstRevDiffToolStripMenuItem";
            this.rememberFirstRevDiffToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.rememberFirstRevDiffToolStripMenuItem.Text = "Remember First for diff";
            this.rememberFirstRevDiffToolStripMenuItem.Click += new System.EventHandler(this.rememberFirstDiffToolToolStripMenuItem_Click);
            // 
            // firstDiffCaptionMenuItem
            // 
            this.firstDiffCaptionMenuItem.Enabled = false;
            this.firstDiffCaptionMenuItem.Name = "firstDiffCaptionMenuItem";
            this.firstDiffCaptionMenuItem.Size = new System.Drawing.Size(282, 22);
            // 
            // secondDiffCaptionMenuItem
            // 
            this.secondDiffCaptionMenuItem.Enabled = false;
            this.secondDiffCaptionMenuItem.Name = "secondDiffCaptionMenuItem";
            this.secondDiffCaptionMenuItem.Size = new System.Drawing.Size(282, 22);
            // 
            // firstToSelectedToolStripMenuItem
            // 
            this.firstToSelectedToolStripMenuItem.Name = "firstToSelectedToolStripMenuItem";
            this.firstToSelectedToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.firstToSelectedToolStripMenuItem.Text = "First -> Second";
            this.firstToSelectedToolStripMenuItem.Click += new System.EventHandler(this.firstToSelectedToolStripMenuItem_Click);
            // 
            // firstToLocalToolStripMenuItem
            // 
            this.firstToLocalToolStripMenuItem.Name = "firstToLocalToolStripMenuItem";
            this.firstToLocalToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.firstToLocalToolStripMenuItem.Text = "First -> Working directory";
            this.firstToLocalToolStripMenuItem.Click += new System.EventHandler(this.firstToLocalToolStripMenuItem_Click);
            // 
            // selectedToLocalToolStripMenuItem
            // 
            this.selectedToLocalToolStripMenuItem.Name = "selectedToLocalToolStripMenuItem";
            this.selectedToLocalToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.selectedToLocalToolStripMenuItem.Text = "Second -> Working directory";
            this.selectedToLocalToolStripMenuItem.Click += new System.EventHandler(this.selectedToLocalToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem1
            // 
            this.saveAsToolStripMenuItem1.Image = global::GitUI.Properties.Images.SaveAs;
            this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            this.saveAsToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(296, 22);
            this.saveAsToolStripMenuItem1.Text = "Save selected as...";
            this.saveAsToolStripMenuItem1.Click += new System.EventHandler(this.saveAsToolStripMenuItem1_Click);
            // 
            // resetFileToToolStripMenuItem
            // 
            this.resetFileToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetFileToSelectedToolStripMenuItem,
            this.resetFileToParentToolStripMenuItem});
            this.resetFileToToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetWorkingDirChanges;
            this.resetFileToToolStripMenuItem.Name = "resetFileToToolStripMenuItem";
            this.resetFileToToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.resetFileToToolStripMenuItem.Text = "Reset file(s) to";
            this.resetFileToToolStripMenuItem.DropDownOpening += new System.EventHandler(this.resetFileToToolStripMenuItem_DropDownOpening);
            // 
            // resetFileToSelectedToolStripMenuItem
            // 
            this.resetFileToSelectedToolStripMenuItem.Name = "resetFileToSelectedToolStripMenuItem";
            this.resetFileToSelectedToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.resetFileToSelectedToolStripMenuItem.Click += new System.EventHandler(this.resetFileToolStripMenuItem_Click);
            // 
            // resetFileToParentToolStripMenuItem
            // 
            this.resetFileToParentToolStripMenuItem.Name = "resetFileToParentToolStripMenuItem";
            this.resetFileToParentToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.resetFileToParentToolStripMenuItem.Click += new System.EventHandler(this.resetFileToolStripMenuItem_Click);
            // 
            // stageFileToolStripMenuItem
            // 
            this.stageFileToolStripMenuItem.Image = global::GitUI.Properties.Images.Stage;
            this.stageFileToolStripMenuItem.Name = "stageFileToolStripMenuItem";
            this.stageFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.stageFileToolStripMenuItem.Text = "Stage file(s)";
            this.stageFileToolStripMenuItem.Click += new System.EventHandler(this.StageFileToolStripMenuItemClick);
            // 
            // unstageFileToolStripMenuItem
            // 
            this.unstageFileToolStripMenuItem.Image = global::GitUI.Properties.Images.Unstage;
            this.unstageFileToolStripMenuItem.Name = "unstageFileToolStripMenuItem";
            this.unstageFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.unstageFileToolStripMenuItem.Text = "Unstage file(s)";
            this.unstageFileToolStripMenuItem.Click += new System.EventHandler(this.UnstageFileToolStripMenuItemClick);
            // 
            // cherryPickSelectedDiffFileToolStripMenuItem
            // 
            this.cherryPickSelectedDiffFileToolStripMenuItem.Image = global::GitUI.Properties.Images.CherryPick;
            this.cherryPickSelectedDiffFileToolStripMenuItem.Name = "cherryPickSelectedDiffFileToolStripMenuItem";
            this.cherryPickSelectedDiffFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.cherryPickSelectedDiffFileToolStripMenuItem.Text = "Cherry pick changes";
            this.cherryPickSelectedDiffFileToolStripMenuItem.Click += new System.EventHandler(this.cherryPickSelectedDiffFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(293, 6);
            // 
            // diffEditWorkingDirectoryFileToolStripMenuItem
            // 
            this.diffEditWorkingDirectoryFileToolStripMenuItem.Image = global::GitUI.Properties.Images.EditFile;
            this.diffEditWorkingDirectoryFileToolStripMenuItem.Name = "diffEditWorkingDirectoryFileToolStripMenuItem";
            this.diffEditWorkingDirectoryFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffEditWorkingDirectoryFileToolStripMenuItem.Text = "Edit working directory file";
            this.diffEditWorkingDirectoryFileToolStripMenuItem.Click += new System.EventHandler(this.diffEditWorkingDirectoryFileToolStripMenuItem_Click);
            // 
            // diffOpenWorkingDirectoryFileWithToolStripMenuItem
            // 
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem.Image = global::GitUI.Properties.Images.EditFile;
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem.Name = "diffOpenWorkingDirectoryFileWithToolStripMenuItem";
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem.Text = "Open working directory file with...";
            this.diffOpenWorkingDirectoryFileWithToolStripMenuItem.Click += new System.EventHandler(this.diffOpenWorkingDirectoryFileWithToolStripMenuItem_Click);
            // 
            // diffOpenRevisionFileToolStripMenuItem
            // 
            this.diffOpenRevisionFileToolStripMenuItem.Image = global::GitUI.Properties.Images.ViewFile;
            this.diffOpenRevisionFileToolStripMenuItem.Name = "diffOpenRevisionFileToolStripMenuItem";
            this.diffOpenRevisionFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffOpenRevisionFileToolStripMenuItem.Text = "Open this revision (temp file)";
            this.diffOpenRevisionFileToolStripMenuItem.Click += new System.EventHandler(this.diffOpenRevisionFileToolStripMenuItem_Click);
            // 
            // diffOpenRevisionFileWithToolStripMenuItem
            // 
            this.diffOpenRevisionFileWithToolStripMenuItem.Image = global::GitUI.Properties.Images.ViewFile;
            this.diffOpenRevisionFileWithToolStripMenuItem.Name = "diffOpenRevisionFileWithToolStripMenuItem";
            this.diffOpenRevisionFileWithToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffOpenRevisionFileWithToolStripMenuItem.Text = "Open this revision with... (temp file)";
            this.diffOpenRevisionFileWithToolStripMenuItem.Click += new System.EventHandler(this.diffOpenRevisionFileWithToolStripMenuItem_Click);
            // 
            // diffDeleteFileToolStripMenuItem
            // 
            this.diffDeleteFileToolStripMenuItem.Image = global::GitUI.Properties.Images.DeleteFile;
            this.diffDeleteFileToolStripMenuItem.Name = "diffDeleteFileToolStripMenuItem";
            this.diffDeleteFileToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffDeleteFileToolStripMenuItem.Text = "Delete file";
            this.diffDeleteFileToolStripMenuItem.Click += new System.EventHandler(this.diffDeleteFileToolStripMenuItem_Click);
            // 
            // diffCommitSubmoduleChanges
            // 
            this.diffCommitSubmoduleChanges.Image = global::GitUI.Properties.Images.RepoStateDirtySubmodules;
            this.diffCommitSubmoduleChanges.Name = "diffCommitSubmoduleChanges";
            this.diffCommitSubmoduleChanges.Size = new System.Drawing.Size(296, 22);
            this.diffCommitSubmoduleChanges.Text = "Commit submodule changes";
            this.diffCommitSubmoduleChanges.Click += new System.EventHandler(this.diffCommitSubmoduleChanges_Click);
            // 
            // diffResetSubmoduleChanges
            // 
            this.diffResetSubmoduleChanges.Image = global::GitUI.Properties.Images.ResetWorkingDirChanges;
            this.diffResetSubmoduleChanges.Name = "diffResetSubmoduleChanges";
            this.diffResetSubmoduleChanges.Size = new System.Drawing.Size(296, 22);
            this.diffResetSubmoduleChanges.Text = "Reset submodule changes";
            this.diffResetSubmoduleChanges.Click += new System.EventHandler(this.diffResetSubmoduleChanges_Click);
            // 
            // diffStashSubmoduleChangesToolStripMenuItem
            // 
            this.diffStashSubmoduleChangesToolStripMenuItem.Image = global::GitUI.Properties.Images.Stash;
            this.diffStashSubmoduleChangesToolStripMenuItem.Name = "diffStashSubmoduleChangesToolStripMenuItem";
            this.diffStashSubmoduleChangesToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffStashSubmoduleChangesToolStripMenuItem.Text = "Stash submodule changes";
            this.diffStashSubmoduleChangesToolStripMenuItem.Click += new System.EventHandler(this.diffStashSubmoduleChangesToolStripMenuItem_Click);
            // 
            // diffUpdateSubmoduleMenuItem
            // 
            this.diffUpdateSubmoduleMenuItem.Image = global::GitUI.Properties.Images.SubmodulesUpdate;
            this.diffUpdateSubmoduleMenuItem.Name = "diffUpdateSubmoduleMenuItem";
            this.diffUpdateSubmoduleMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffUpdateSubmoduleMenuItem.Tag = "1";
            this.diffUpdateSubmoduleMenuItem.Text = "Update submodule";
            this.diffUpdateSubmoduleMenuItem.Click += new System.EventHandler(this.diffUpdateSubmoduleMenuItem_Click);
            // 
            // submoduleStripSeparator
            // 
            this.submoduleStripSeparator.Name = "submoduleStripSeparator";
            this.submoduleStripSeparator.Size = new System.Drawing.Size(293, 6);
            this.submoduleStripSeparator.Tag = "1";
            // 
            // diffToolStripSeparator13
            // 
            this.diffToolStripSeparator13.Name = "diffToolStripSeparator13";
            this.diffToolStripSeparator13.Size = new System.Drawing.Size(293, 6);
            this.diffToolStripSeparator13.Tag = "1";
            // 
            // copyFilenameToClipboardToolStripMenuItem1
            // 
            this.copyFilenameToClipboardToolStripMenuItem1.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem1.Name = "copyFilenameToClipboardToolStripMenuItem1";
            this.copyFilenameToClipboardToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem1.Size = new System.Drawing.Size(296, 22);
            this.copyFilenameToClipboardToolStripMenuItem1.Text = "Copy full path(s)";
            this.copyFilenameToClipboardToolStripMenuItem1.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem1_Click);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Show in folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // diffShowInFileTreeToolStripMenuItem
            // 
            this.diffShowInFileTreeToolStripMenuItem.Image = global::GitUI.Properties.Images.FileTree;
            this.diffShowInFileTreeToolStripMenuItem.Name = "diffShowInFileTreeToolStripMenuItem";
            this.diffShowInFileTreeToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.diffShowInFileTreeToolStripMenuItem.Text = "Show in File tree";
            this.diffShowInFileTreeToolStripMenuItem.Click += new System.EventHandler(this.diffShowInFileTreeToolStripMenuItem_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(293, 6);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            this.fileHistoryDiffToolstripMenuItem.Image = global::GitUI.Properties.Images.FileHistory;
            this.fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            this.fileHistoryDiffToolstripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.fileHistoryDiffToolstripMenuItem.Text = "File history";
            this.fileHistoryDiffToolstripMenuItem.Click += new System.EventHandler(this.fileHistoryDiffToolstripMenuItem_Click);
            // 
            // blameToolStripMenuItem
            // 
            this.blameToolStripMenuItem.Image = global::GitUI.Properties.Images.Blame;
            this.blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            this.blameToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.blameToolStripMenuItem.Text = "Blame";
            this.blameToolStripMenuItem.Click += new System.EventHandler(this.blameToolStripMenuItem_Click);
            // 
            // findInDiffToolStripMenuItem
            // 
            this.findInDiffToolStripMenuItem.Image = global::GitUI.Properties.Images.Preview;
            this.findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            this.findInDiffToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findInDiffToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.findInDiffToolStripMenuItem.Text = "Find";
            this.findInDiffToolStripMenuItem.Click += new System.EventHandler(this.findInDiffToolStripMenuItem_Click);
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(0);
            this.DiffText.Name = "DiffText";
            this.DiffText.Size = new System.Drawing.Size(423, 360);
            this.DiffText.TabIndex = 0;
            this.DiffText.ExtraDiffArgumentsChanged += new System.EventHandler<System.EventArgs>(this.DiffText_ExtraDiffArgumentsChanged);
            this.DiffText.PatchApplied += new System.EventHandler<System.EventArgs>(this.DiffText_PatchApplied);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // RevisionDiffControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DiffSplitContainer);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "RevisionDiffControl";
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
        private ToolStripMenuItem secondDiffCaptionMenuItem;
        private ToolStripMenuItem firstDiffCaptionMenuItem;
        private ToolStripMenuItem selectedToLocalToolStripMenuItem;
        private ToolStripMenuItem firstToLocalToolStripMenuItem;
        private ToolStripMenuItem firstToSelectedToolStripMenuItem;
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
        private ToolStripMenuItem diffDeleteFileToolStripMenuItem;
        private ToolStripMenuItem diffUpdateSubmoduleMenuItem;
        private ToolStripMenuItem diffResetSubmoduleChanges;
        private ToolStripMenuItem diffCommitSubmoduleChanges;
        private ToolStripMenuItem diffStashSubmoduleChangesToolStripMenuItem;
        private ToolStripSeparator submoduleStripSeparator;
        private ToolStripSeparator diffToolStripSeparator13;
        private ToolStripMenuItem resetFileToToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem1;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private ToolStripSeparator diffRememberStripSeparator;
        private ToolStripMenuItem diffTwoSelectedDifftoolToolStripMenuItem;
        private ToolStripMenuItem diffWithRememberedDifftoolToolStripMenuItem;
        private ToolStripMenuItem rememberSecondRevDiffToolStripMenuItem;
        private ToolStripMenuItem rememberFirstRevDiffToolStripMenuItem;
        private SplitContainer DiffSplitContainer;
        private ContextMenuStrip DiffContextMenu;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
        private ToolStripMenuItem diffEditWorkingDirectoryFileToolStripMenuItem;
        private ToolStripMenuItem diffOpenWorkingDirectoryFileWithToolStripMenuItem;
        private ToolStripMenuItem diffOpenRevisionFileToolStripMenuItem;
        private ToolStripMenuItem diffOpenRevisionFileWithToolStripMenuItem;
    }
}
