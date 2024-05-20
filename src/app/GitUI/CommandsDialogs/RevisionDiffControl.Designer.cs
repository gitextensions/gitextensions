using System.Windows.Forms;
using GitUI.CommandsDialogs.Menus;

namespace GitUI.CommandsDialogs
{
    partial class RevisionDiffControl
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
            DiffSplitContainer = new SplitContainer();
            DiffFiles = new GitUI.FileStatusList();
            DiffContextMenu = new ContextMenuStrip(components);
            diffUpdateSubmoduleMenuItem = new ToolStripMenuItem();
            diffResetSubmoduleChanges = new ToolStripMenuItem();
            diffStashSubmoduleChangesToolStripMenuItem = new ToolStripMenuItem();
            diffCommitSubmoduleChanges = new ToolStripMenuItem();
            submoduleStripSeparator = new ToolStripSeparator();
            stageFileToolStripMenuItem = new ToolStripMenuItem();
            unstageFileToolStripMenuItem = new ToolStripMenuItem();
            resetFileToToolStripMenuItem = new ToolStripMenuItem();
            resetFileToSelectedToolStripMenuItem = new ToolStripMenuItem();
            resetFileToParentToolStripMenuItem = new ToolStripMenuItem();
            cherryPickSelectedDiffFileToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator32 = new ToolStripSeparator();
            openWithDifftoolToolStripMenuItem = new ToolStripMenuItem();
            secondDiffCaptionMenuItem = new ToolStripMenuItem();
            firstDiffCaptionMenuItem = new ToolStripMenuItem();
            firstToSelectedToolStripMenuItem = new ToolStripMenuItem();
            selectedToLocalToolStripMenuItem = new ToolStripMenuItem();
            firstToLocalToolStripMenuItem = new ToolStripMenuItem();
            diffRememberStripSeparator = new ToolStripSeparator();
            diffTwoSelectedDifftoolToolStripMenuItem = new ToolStripMenuItem();
            diffWithRememberedDifftoolToolStripMenuItem = new ToolStripMenuItem();
            rememberSecondRevDiffToolStripMenuItem = new ToolStripMenuItem();
            rememberFirstRevDiffToolStripMenuItem = new ToolStripMenuItem();
            diffOpenWorkingDirectoryFileWithToolStripMenuItem = new ToolStripMenuItem();
            diffOpenRevisionFileToolStripMenuItem = new ToolStripMenuItem();
            diffOpenRevisionFileWithToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem1 = new ToolStripMenuItem();
            diffEditWorkingDirectoryFileToolStripMenuItem = new ToolStripMenuItem();
            diffDeleteFileToolStripMenuItem = new ToolStripMenuItem();
            diffToolStripSeparator13 = new ToolStripSeparator();
            copyPathsToolStripMenuItem = new CopyPathsToolStripMenuItem();
            openContainingFolderToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator33 = new ToolStripSeparator();
            diffShowInFileTreeToolStripMenuItem = new ToolStripMenuItem();
            diffFilterFileInGridToolStripMenuItem = new ToolStripMenuItem();
            fileHistoryDiffToolstripMenuItem = new ToolStripMenuItem();
            blameToolStripMenuItem = new ToolStripMenuItem();
            findInDiffToolStripMenuItem = new ToolStripMenuItem();
            showSearchCommitToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorScript = new ToolStripSeparator();
            runScriptToolStripMenuItem = new ToolStripMenuItem();
            DiffText = new GitUI.Editor.FileViewer();
            BlameControl = new Blame.BlameControl();
            saveToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(DiffSplitContainer)).BeginInit();
            DiffSplitContainer.Panel1.SuspendLayout();
            DiffSplitContainer.Panel2.SuspendLayout();
            DiffSplitContainer.SuspendLayout();
            DiffContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // DiffSplitContainer
            // 
            DiffSplitContainer.BackColor = SystemColors.Control;
            DiffSplitContainer.Dock = DockStyle.Fill;
            DiffSplitContainer.FixedPanel = FixedPanel.Panel1;
            DiffSplitContainer.Location = new Point(0, 0);
            DiffSplitContainer.Margin = new Padding(0);
            DiffSplitContainer.Name = "DiffSplitContainer";
            // 
            // DiffSplitContainer.Panel1
            // 
            DiffSplitContainer.Panel1.Controls.Add(DiffFiles);
            // 
            // DiffSplitContainer.Panel2
            // 
            DiffSplitContainer.Panel2.Controls.Add(DiffText);
            DiffSplitContainer.Panel2.Controls.Add(BlameControl);
            DiffSplitContainer.Size = new Size(850, 415);
            DiffSplitContainer.SplitterDistance = 300;
            DiffSplitContainer.SplitterWidth = 7;
            DiffSplitContainer.TabIndex = 0;
            // 
            // DiffFiles
            // 
            DiffFiles.ContextMenuStrip = DiffContextMenu;
            DiffFiles.Dock = DockStyle.Fill;
            DiffFiles.GroupByRevision = true;
            DiffFiles.Location = new Point(0, 0);
            DiffFiles.Margin = new Padding(0);
            DiffFiles.Name = "DiffFiles";
            DiffFiles.SelectFirstItemOnSetItems = false;
            DiffFiles.Size = new Size(300, 415);
            DiffFiles.TabIndex = 1;
            DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;
            DiffFiles.DataSourceChanged += DiffFiles_DataSourceChanged;
            DiffFiles.DoubleClick += DiffFiles_DoubleClick;
            // 
            // BlameControl
            // 
            BlameControl.Dock = DockStyle.Fill;
            BlameControl.Location = new Point(0, 0);
            BlameControl.Margin = new Padding(0);
            BlameControl.Name = "BlameControl";
            BlameControl.Size = new Size(300, 360);
            BlameControl.TabIndex = 1;
            // 
            // DiffContextMenu
            // 
            DiffContextMenu.Items.AddRange(new ToolStripItem[] {
            diffUpdateSubmoduleMenuItem,
            diffResetSubmoduleChanges,
            diffStashSubmoduleChangesToolStripMenuItem,
            diffCommitSubmoduleChanges,
            submoduleStripSeparator,
            stageFileToolStripMenuItem,
            unstageFileToolStripMenuItem,
            resetFileToToolStripMenuItem,
            cherryPickSelectedDiffFileToolStripMenuItem,
            toolStripSeparator32,
            openWithDifftoolToolStripMenuItem,
            diffOpenWorkingDirectoryFileWithToolStripMenuItem,
            diffOpenRevisionFileToolStripMenuItem,
            diffOpenRevisionFileWithToolStripMenuItem,
            saveAsToolStripMenuItem1,
            diffEditWorkingDirectoryFileToolStripMenuItem,
            diffDeleteFileToolStripMenuItem,
            diffToolStripSeparator13,
            copyPathsToolStripMenuItem,
            openContainingFolderToolStripMenuItem,
            toolStripSeparator33,
            diffShowInFileTreeToolStripMenuItem,
            diffFilterFileInGridToolStripMenuItem,
            fileHistoryDiffToolstripMenuItem,
            blameToolStripMenuItem,
            findInDiffToolStripMenuItem,
            showSearchCommitToolStripMenuItem,
            toolStripSeparatorScript,
            runScriptToolStripMenuItem});
            DiffContextMenu.Name = "DiffContextMenu";
            DiffContextMenu.Size = new Size(263, 534);
            DiffContextMenu.Opening += DiffContextMenu_Opening;
            // 
            // diffUpdateSubmoduleMenuItem
            // 
            diffUpdateSubmoduleMenuItem.Image = Properties.Images.SubmodulesUpdate;
            diffUpdateSubmoduleMenuItem.Name = "diffUpdateSubmoduleMenuItem";
            diffUpdateSubmoduleMenuItem.Size = new Size(262, 22);
            diffUpdateSubmoduleMenuItem.Tag = "1";
            diffUpdateSubmoduleMenuItem.Text = "&Update submodule";
            diffUpdateSubmoduleMenuItem.Click += diffUpdateSubmoduleMenuItem_Click;
            // 
            // diffResetSubmoduleChanges
            // 
            diffResetSubmoduleChanges.Image = Properties.Images.ResetWorkingDirChanges;
            diffResetSubmoduleChanges.Name = "diffResetSubmoduleChanges";
            diffResetSubmoduleChanges.Size = new Size(262, 22);
            diffResetSubmoduleChanges.Text = "R&eset submodule changes";
            diffResetSubmoduleChanges.Click += diffResetSubmoduleChanges_Click;
            // 
            // diffStashSubmoduleChangesToolStripMenuItem
            // 
            diffStashSubmoduleChangesToolStripMenuItem.Image = Properties.Images.Stash;
            diffStashSubmoduleChangesToolStripMenuItem.Name = "diffStashSubmoduleChangesToolStripMenuItem";
            diffStashSubmoduleChangesToolStripMenuItem.Size = new Size(262, 22);
            diffStashSubmoduleChangesToolStripMenuItem.Text = "S&tash submodule changes";
            diffStashSubmoduleChangesToolStripMenuItem.Click += diffStashSubmoduleChangesToolStripMenuItem_Click;
            // 
            // diffCommitSubmoduleChanges
            // 
            diffCommitSubmoduleChanges.Image = Properties.Images.RepoStateDirtySubmodules;
            diffCommitSubmoduleChanges.Name = "diffCommitSubmoduleChanges";
            diffCommitSubmoduleChanges.Size = new Size(262, 22);
            diffCommitSubmoduleChanges.Text = "&Commit submodule changes";
            diffCommitSubmoduleChanges.Click += diffCommitSubmoduleChanges_Click;
            // 
            // submoduleStripSeparator
            // 
            submoduleStripSeparator.Name = "submoduleStripSeparator";
            submoduleStripSeparator.Size = new Size(259, 6);
            submoduleStripSeparator.Tag = "1";
            // 
            // stageFileToolStripMenuItem
            // 
            stageFileToolStripMenuItem.Image = Properties.Images.Stage;
            stageFileToolStripMenuItem.Name = "stageFileToolStripMenuItem";
            stageFileToolStripMenuItem.Size = new Size(262, 22);
            stageFileToolStripMenuItem.Text = "&Stage file(s)";
            stageFileToolStripMenuItem.Click += StageFileToolStripMenuItemClick;
            // 
            // unstageFileToolStripMenuItem
            // 
            unstageFileToolStripMenuItem.Image = Properties.Images.Unstage;
            unstageFileToolStripMenuItem.Name = "unstageFileToolStripMenuItem";
            unstageFileToolStripMenuItem.Size = new Size(262, 22);
            unstageFileToolStripMenuItem.Text = "&Unstage file(s)";
            unstageFileToolStripMenuItem.Click += UnstageFileToolStripMenuItemClick;
            // 
            // resetFileToToolStripMenuItem
            // 
            resetFileToToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            resetFileToSelectedToolStripMenuItem,
            resetFileToParentToolStripMenuItem});
            resetFileToToolStripMenuItem.Image = Properties.Images.ResetWorkingDirChanges;
            resetFileToToolStripMenuItem.Name = "resetFileToToolStripMenuItem";
            resetFileToToolStripMenuItem.Size = new Size(262, 22);
            resetFileToToolStripMenuItem.Text = "&Reset file(s) to";
            resetFileToToolStripMenuItem.DropDownOpening += resetFileToToolStripMenuItem_DropDownOpening;
            // 
            // resetFileToSelectedToolStripMenuItem
            // 
            resetFileToSelectedToolStripMenuItem.Name = "resetFileToSelectedToolStripMenuItem";
            resetFileToSelectedToolStripMenuItem.Size = new Size(67, 22);
            resetFileToSelectedToolStripMenuItem.Click += resetFileToolStripMenuItem_Click;
            // 
            // resetFileToParentToolStripMenuItem
            // 
            resetFileToParentToolStripMenuItem.Name = "resetFileToParentToolStripMenuItem";
            resetFileToParentToolStripMenuItem.Size = new Size(67, 22);
            resetFileToParentToolStripMenuItem.Click += resetFileToolStripMenuItem_Click;
            // 
            // cherryPickSelectedDiffFileToolStripMenuItem
            // 
            cherryPickSelectedDiffFileToolStripMenuItem.Image = Properties.Images.CherryPick;
            cherryPickSelectedDiffFileToolStripMenuItem.Name = "cherryPickSelectedDiffFileToolStripMenuItem";
            cherryPickSelectedDiffFileToolStripMenuItem.Size = new Size(262, 22);
            cherryPickSelectedDiffFileToolStripMenuItem.Text = "Cherr&y pick changes";
            cherryPickSelectedDiffFileToolStripMenuItem.Click += cherryPickSelectedDiffFileToolStripMenuItem_Click;
            // 
            // toolStripSeparator32
            // 
            toolStripSeparator32.Name = "toolStripSeparator32";
            toolStripSeparator32.Size = new Size(259, 6);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            secondDiffCaptionMenuItem,
            firstDiffCaptionMenuItem,
            firstToSelectedToolStripMenuItem,
            selectedToLocalToolStripMenuItem,
            firstToLocalToolStripMenuItem,
            diffRememberStripSeparator,
            diffTwoSelectedDifftoolToolStripMenuItem,
            diffWithRememberedDifftoolToolStripMenuItem,
            rememberSecondRevDiffToolStripMenuItem,
            rememberFirstRevDiffToolStripMenuItem});
            openWithDifftoolToolStripMenuItem.Image = Properties.Images.Diff;
            openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            openWithDifftoolToolStripMenuItem.Size = new Size(262, 22);
            openWithDifftoolToolStripMenuItem.Text = "Open with &difftool";
            openWithDifftoolToolStripMenuItem.DropDownOpening += openWithDifftoolToolStripMenuItem_DropDownOpening;
            // 
            // secondDiffCaptionMenuItem
            // 
            secondDiffCaptionMenuItem.Enabled = false;
            secondDiffCaptionMenuItem.Name = "secondDiffCaptionMenuItem";
            secondDiffCaptionMenuItem.Size = new Size(227, 22);
            // 
            // firstDiffCaptionMenuItem
            // 
            firstDiffCaptionMenuItem.Enabled = false;
            firstDiffCaptionMenuItem.Name = "firstDiffCaptionMenuItem";
            firstDiffCaptionMenuItem.Size = new Size(227, 22);
            // 
            // firstToSelectedToolStripMenuItem
            // 
            firstToSelectedToolStripMenuItem.Name = "firstToSelectedToolStripMenuItem";
            firstToSelectedToolStripMenuItem.Size = new Size(227, 22);
            firstToSelectedToolStripMenuItem.Text = "&First -> Second";
            firstToSelectedToolStripMenuItem.Click += firstToSelectedToolStripMenuItem_Click;
            // 
            // selectedToLocalToolStripMenuItem
            // 
            selectedToLocalToolStripMenuItem.Name = "selectedToLocalToolStripMenuItem";
            selectedToLocalToolStripMenuItem.Size = new Size(227, 22);
            selectedToLocalToolStripMenuItem.Text = "&Second -> Working directory";
            selectedToLocalToolStripMenuItem.Click += selectedToLocalToolStripMenuItem_Click;
            // 
            // firstToLocalToolStripMenuItem
            // 
            firstToLocalToolStripMenuItem.Name = "firstToLocalToolStripMenuItem";
            firstToLocalToolStripMenuItem.Size = new Size(227, 22);
            firstToLocalToolStripMenuItem.Text = "First -> &Working directory";
            firstToLocalToolStripMenuItem.Click += firstToLocalToolStripMenuItem_Click;
            // 
            // diffRememberStripSeparator
            // 
            diffRememberStripSeparator.Name = "diffRememberStripSeparator";
            diffRememberStripSeparator.Size = new Size(224, 6);
            // 
            // diffTwoSelectedDifftoolToolStripMenuItem
            // 
            diffTwoSelectedDifftoolToolStripMenuItem.Name = "diffTwoSelectedDifftoolToolStripMenuItem";
            diffTwoSelectedDifftoolToolStripMenuItem.Size = new Size(227, 22);
            diffTwoSelectedDifftoolToolStripMenuItem.Text = "&Diff the selected files";
            diffTwoSelectedDifftoolToolStripMenuItem.Click += diffTwoSelectedDiffToolToolStripMenuItem_Click;
            // 
            // diffWithRememberedDifftoolToolStripMenuItem
            // 
            diffWithRememberedDifftoolToolStripMenuItem.Name = "diffWithRememberedDifftoolToolStripMenuItem";
            diffWithRememberedDifftoolToolStripMenuItem.Size = new Size(227, 22);
            diffWithRememberedDifftoolToolStripMenuItem.Click += diffWithRememberedDiffToolToolStripMenuItem_Click;
            // 
            // rememberSecondRevDiffToolStripMenuItem
            // 
            rememberSecondRevDiffToolStripMenuItem.Name = "rememberSecondRevDiffToolStripMenuItem";
            rememberSecondRevDiffToolStripMenuItem.Size = new Size(227, 22);
            rememberSecondRevDiffToolStripMenuItem.Text = "&Remember Second for diff";
            rememberSecondRevDiffToolStripMenuItem.Click += rememberSecondDiffToolToolStripMenuItem_Click;
            // 
            // rememberFirstRevDiffToolStripMenuItem
            // 
            rememberFirstRevDiffToolStripMenuItem.Name = "rememberFirstRevDiffToolStripMenuItem";
            rememberFirstRevDiffToolStripMenuItem.Size = new Size(227, 22);
            rememberFirstRevDiffToolStripMenuItem.Text = "R&emember First for diff";
            rememberFirstRevDiffToolStripMenuItem.Click += rememberFirstDiffToolToolStripMenuItem_Click;
            // 
            // diffOpenWorkingDirectoryFileWithToolStripMenuItem
            // 
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Image = Properties.Images.EditFile;
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Name = "diffOpenWorkingDirectoryFileWithToolStripMenuItem";
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Size = new Size(262, 22);
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Text = "&Open working directory file with...";
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Click += diffOpenWorkingDirectoryFileWithToolStripMenuItem_Click;
            // 
            // diffOpenRevisionFileToolStripMenuItem
            // 
            diffOpenRevisionFileToolStripMenuItem.Image = Properties.Images.ViewFile;
            diffOpenRevisionFileToolStripMenuItem.Name = "diffOpenRevisionFileToolStripMenuItem";
            diffOpenRevisionFileToolStripMenuItem.Size = new Size(262, 22);
            diffOpenRevisionFileToolStripMenuItem.Text = "Ope&n this revision (temp file)";
            diffOpenRevisionFileToolStripMenuItem.Click += diffOpenRevisionFileToolStripMenuItem_Click;
            // 
            // diffOpenRevisionFileWithToolStripMenuItem
            // 
            diffOpenRevisionFileWithToolStripMenuItem.Image = Properties.Images.ViewFile;
            diffOpenRevisionFileWithToolStripMenuItem.Name = "diffOpenRevisionFileWithToolStripMenuItem";
            diffOpenRevisionFileWithToolStripMenuItem.Size = new Size(262, 22);
            diffOpenRevisionFileWithToolStripMenuItem.Text = "Open this revision &with... (temp file)";
            diffOpenRevisionFileWithToolStripMenuItem.Click += diffOpenRevisionFileWithToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem1
            // 
            saveAsToolStripMenuItem1.Image = Properties.Images.SaveAs;
            saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            saveAsToolStripMenuItem1.ShortcutKeys = ((Keys)((Keys.Control | Keys.S)));
            saveAsToolStripMenuItem1.Size = new Size(262, 22);
            saveAsToolStripMenuItem1.Text = "S&ave selected as...";
            saveAsToolStripMenuItem1.Click += saveAsToolStripMenuItem1_Click;
            // 
            // diffEditWorkingDirectoryFileToolStripMenuItem
            // 
            diffEditWorkingDirectoryFileToolStripMenuItem.Image = Properties.Images.EditFile;
            diffEditWorkingDirectoryFileToolStripMenuItem.Name = "diffEditWorkingDirectoryFileToolStripMenuItem";
            diffEditWorkingDirectoryFileToolStripMenuItem.Size = new Size(262, 22);
            diffEditWorkingDirectoryFileToolStripMenuItem.Text = "&Edit working directory file";
            diffEditWorkingDirectoryFileToolStripMenuItem.Click += diffEditWorkingDirectoryFileToolStripMenuItem_Click;
            // 
            // diffDeleteFileToolStripMenuItem
            // 
            diffDeleteFileToolStripMenuItem.Image = Properties.Images.DeleteFile;
            diffDeleteFileToolStripMenuItem.Name = "diffDeleteFileToolStripMenuItem";
            diffDeleteFileToolStripMenuItem.Size = new Size(262, 22);
            diffDeleteFileToolStripMenuItem.Text = "De&lete file";
            diffDeleteFileToolStripMenuItem.Click += diffDeleteFileToolStripMenuItem_Click;
            // 
            // diffToolStripSeparator13
            // 
            diffToolStripSeparator13.Name = "diffToolStripSeparator13";
            diffToolStripSeparator13.Size = new Size(259, 6);
            diffToolStripSeparator13.Tag = "1";
            // 
            // copyPathsToolStripMenuItem
            // 
            copyPathsToolStripMenuItem.Name = "copyPathsToolStripMenuItem";
            // 
            // openContainingFolderToolStripMenuItem
            // 
            openContainingFolderToolStripMenuItem.Image = Properties.Images.BrowseFileExplorer;
            openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            openContainingFolderToolStripMenuItem.Size = new Size(262, 22);
            openContainingFolderToolStripMenuItem.Text = "Show &in folder";
            openContainingFolderToolStripMenuItem.Click += openContainingFolderToolStripMenuItem_Click;
            // 
            // toolStripSeparator33
            // 
            toolStripSeparator33.Name = "toolStripSeparator33";
            toolStripSeparator33.Size = new Size(259, 6);
            // 
            // diffShowInFileTreeToolStripMenuItem
            // 
            diffShowInFileTreeToolStripMenuItem.Image = Properties.Images.FileTree;
            diffShowInFileTreeToolStripMenuItem.Name = "diffShowInFileTreeToolStripMenuItem";
            diffShowInFileTreeToolStripMenuItem.Size = new Size(262, 22);
            diffShowInFileTreeToolStripMenuItem.Text = "Show in File &tree";
            diffShowInFileTreeToolStripMenuItem.Click += diffShowInFileTreeToolStripMenuItem_Click;
            // 
            // diffFilterFileInGridToolStripMenuItem
            // 
            diffFilterFileInGridToolStripMenuItem.Image = Properties.Images.FunnelPencil;
            diffFilterFileInGridToolStripMenuItem.Name = "diffFilterFileInGridToolStripMenuItem";
            diffFilterFileInGridToolStripMenuItem.Size = new Size(262, 22);
            diffFilterFileInGridToolStripMenuItem.Click += diffFilterFileInGridToolStripMenuItem_Click;
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            fileHistoryDiffToolstripMenuItem.Image = Properties.Images.FileHistory;
            fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            fileHistoryDiffToolstripMenuItem.Size = new Size(262, 22);
            fileHistoryDiffToolstripMenuItem.Text = "File &history";
            fileHistoryDiffToolstripMenuItem.Click += fileHistoryDiffToolstripMenuItem_Click;
            // 
            // blameToolStripMenuItem
            // 
            blameToolStripMenuItem.Image = Properties.Images.Blame;
            blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            blameToolStripMenuItem.Size = new Size(262, 22);
            blameToolStripMenuItem.Text = "&Blame";
            blameToolStripMenuItem.Click += blameToolStripMenuItem_Click;
            // 
            // findInDiffToolStripMenuItem
            // 
            findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            findInDiffToolStripMenuItem.Size = new Size(262, 22);
            findInDiffToolStripMenuItem.Text = "&Find file...";
            findInDiffToolStripMenuItem.Click += findInDiffToolStripMenuItem_Click;
            // 
            // showSearchCommitToolStripMenuItem
            // 
            showSearchCommitToolStripMenuItem.Image = Properties.Images.ViewFile;
            showSearchCommitToolStripMenuItem.Name = "showSearchCommitToolStripMenuItem";
            showSearchCommitToolStripMenuItem.Size = new Size(262, 22);
            showSearchCommitToolStripMenuItem.Text = "Sear&ch files in commit...";
            showSearchCommitToolStripMenuItem.Click += showSearchCommitToolStripMenuItem_Click;
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
            // DiffText
            // 
            DiffText.Dock = DockStyle.Fill;
            DiffText.EnableAutomaticContinuousScroll = false;
            DiffText.Location = new Point(0, 0);
            DiffText.Margin = new Padding(0);
            DiffText.Name = "DiffText";
            DiffText.Size = new Size(543, 415);
            DiffText.TabIndex = 0;
            DiffText.ExtraDiffArgumentsChanged += new System.EventHandler<System.EventArgs>(DiffText_ExtraDiffArgumentsChanged);
            DiffText.PatchApplied += new System.EventHandler<System.EventArgs>(DiffText_PatchApplied);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(98, 22);
            saveToolStripMenuItem.Text = "Save";
            // 
            // RevisionDiffControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(DiffSplitContainer);
            Margin = new Padding(0);
            Name = "RevisionDiffControl";
            Size = new Size(850, 415);
            DiffSplitContainer.Panel1.ResumeLayout(false);
            DiffSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(DiffSplitContainer)).EndInit();
            DiffSplitContainer.ResumeLayout(false);
            DiffContextMenu.ResumeLayout(false);
            ResumeLayout(false);

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
        private ToolStripMenuItem showSearchCommitToolStripMenuItem;
        private ToolStripMenuItem diffFilterFileInGridToolStripMenuItem;
        private ToolStripMenuItem blameToolStripMenuItem;
        private ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private ToolStripSeparator toolStripSeparator33;
        private ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private CopyPathsToolStripMenuItem copyPathsToolStripMenuItem;
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
        private Blame.BlameControl BlameControl;
        private ToolStripMenuItem diffEditWorkingDirectoryFileToolStripMenuItem;
        private ToolStripMenuItem diffOpenWorkingDirectoryFileWithToolStripMenuItem;
        private ToolStripMenuItem diffOpenRevisionFileToolStripMenuItem;
        private ToolStripMenuItem diffOpenRevisionFileWithToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorScript;
        private ToolStripMenuItem runScriptToolStripMenuItem;
    }
}
