using System.Drawing;
using System.Windows.Forms;
using GitExtensions.Extensibility.Git;
using GitUI.Editor;
using GitUI.SpellChecker;
using GitUI.UserControls.RevisionGrid;

namespace GitUI.CommandsDialogs
{
    partial class FormCommit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            UnstagedFileContext = new ContextMenuStrip(components);
            tsmiResetUnstagedChanges = new ToolStripMenuItem();
            resetPartOfFileToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator12 = new ToolStripSeparator();
            openToolStripMenuItem = new ToolStripMenuItem();
            openWithToolStripMenuItem = new ToolStripMenuItem();
            stageToolStripMenuItem = new ToolStripMenuItem();
            openWithDifftoolToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            filenameToClipboardToolStripMenuItem = new ToolStripMenuItem();
            openContainingFolderToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            viewFileHistoryToolStripItem = new ToolStripMenuItem();
            editFileToolStripMenuItem = new ToolStripMenuItem();
            deleteFileToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            addFileToGitIgnoreToolStripMenuItem = new ToolStripMenuItem();
            addFileToGitInfoExcludeLocallyToolStripMenuItem = new ToolStripMenuItem();
            skipWorktreeToolStripMenuItem = new ToolStripMenuItem();
            doNotSkipWorktreeToolStripMenuItem = new ToolStripMenuItem();
            assumeUnchangedToolStripMenuItem = new ToolStripMenuItem();
            doNotAssumeUnchangedToolStripMenuItem = new ToolStripMenuItem();
            interactiveAddToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparatorScript = new ToolStripSeparator();
            runScriptToolStripMenuItem = new ToolStripMenuItem();
            fileTooltip = new ToolTip(components);
            StageInSuperproject = new CheckBox();
            StagedFileContext = new ContextMenuStrip(components);
            stagedResetChanges = new ToolStripMenuItem();
            stagedFileHistoryToolStripMenuItem6 = new ToolStripMenuItem();
            stagedFileHistoryToolStripSeparator = new ToolStripSeparator();
            stagedToolStripSeparator14 = new ToolStripSeparator();
            stagedOpenToolStripMenuItem7 = new ToolStripMenuItem();
            stagedOpenWithToolStripMenuItem8 = new ToolStripMenuItem();
            stagedUnstageToolStripMenuItem = new ToolStripMenuItem();
            stagedOpenDifftoolToolStripMenuItem9 = new ToolStripMenuItem();
            stagedToolStripSeparator18 = new ToolStripSeparator();
            stagedCopyPathToolStripMenuItem14 = new ToolStripMenuItem();
            stagedOpenFolderToolStripMenuItem10 = new ToolStripMenuItem();
            stagedEditFileToolStripMenuItem11 = new ToolStripMenuItem();
            stagedToolStripSeparatorScript = new ToolStripSeparator();
            stagedRunScriptToolStripMenuItem = new ToolStripMenuItem();
            UnstagedSubmoduleContext = new ContextMenuStrip(components);
            commitSubmoduleChanges = new ToolStripMenuItem();
            resetSubmoduleChanges = new ToolStripMenuItem();
            stashSubmoduleChangesToolStripMenuItem = new ToolStripMenuItem();
            updateSubmoduleMenuItem = new ToolStripMenuItem();
            stageSubmoduleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            viewHistoryMenuItem = new ToolStripMenuItem();
            toolStripSeparator15 = new ToolStripSeparator();
            unstagedSubmoduleStageToolStripSeparator = new ToolStripSeparator();
            openFolderMenuItem = new ToolStripMenuItem();
            copyFolderNameMenuItem = new ToolStripMenuItem();
            gitItemStatusBindingSource = new BindingSource(components);
            Cancel = new Button();
            splitMain = new SplitContainer();
            splitLeft = new SplitContainer();
            toolStripContainer1 = new ToolStripContainer();
            Loading = new LoadingControl();
            Unstaged = new GitUI.FileStatusList();
            toolbarUnstaged = new GitUI.ToolStripEx();
            toolRefreshItem = new ToolStripButton();
            toolStripSeparator6 = new ToolStripSeparator();
            workingToolStripMenuItem = new ToolStripDropDownButton();
            showIgnoredFilesToolStripMenuItem = new ToolStripMenuItem();
            showSkipWorktreeFilesToolStripMenuItem = new ToolStripMenuItem();
            showAssumeUnchangedFilesToolStripMenuItem = new ToolStripMenuItem();
            showUntrackedFilesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            editGitIgnoreToolStripMenuItem = new ToolStripMenuItem();
            editLocallyIgnoredFilesToolStripMenuItem = new ToolStripMenuItem();
            deleteAllUntrackedFilesToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            selectionFilterToolStripMenuItem = new ToolStripMenuItem();
            toolStripProgressBar1 = new ToolStripProgressBar();
            toolbarSelectionFilter = new GitUI.ToolStripEx();
            toolStripLabel1 = new ToolStripLabel();
            selectionFilter = new ToolStripComboBox();
            LoadingStaged = new PictureBox();
            Staged = new GitUI.FileStatusList();
            toolbarStaged = new GitUI.ToolStripEx();
            toolStageAllItem = new ToolStripButton();
            toolStripSeparator10 = new ToolStripSeparator();
            toolStageItem = new ToolStripButton();
            toolUnstageAllItem = new ToolStripButton();
            toolStripSeparator11 = new ToolStripSeparator();
            toolUnstageItem = new ToolStripButton();
            Ok = new Button();
            splitRight = new SplitContainer();
            SolveMergeconflicts = new Button();
            SelectedDiff = new GitUI.Editor.FileViewer();
            tableLayoutPanel1 = new TableLayoutPanel();
            Message = new GitUI.SpellChecker.EditNetSpell();
            modifyCommitMessageButton = new Button();
            flowCommitButtons = new FlowLayoutPanel();
            AmendPanel = new FlowLayoutPanel();
            Commit = new Button();
            CommitAndPush = new Button();
            Amend = new CheckBox();
            ResetAuthor = new CheckBox();
            ResetSoft = new Button();
            StashStaged = new Button();
            btnResetAllChanges = new Button();
            btnResetUnstagedChanges = new Button();
            toolbarCommit = new GitUI.ToolStripEx();
            commitMessageToolStripMenuItem = new ToolStripDropDownButton();
            ShowOnlyMyMessagesToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            generateListOfChangesInSubmodulesChangesToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripDropDownButton();
            closeDialogAfterEachCommitToolStripMenuItem = new ToolStripMenuItem();
            closeDialogAfterAllFilesCommittedToolStripMenuItem = new ToolStripMenuItem();
            refreshDialogOnFormFocusToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            signOffToolStripMenuItem = new ToolStripMenuItem();
            toolAuthorLabelItem = new ToolStripMenuItem();
            toolAuthor = new ToolStripTextBox();
            noVerifyToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            gpgSignCommitToolStripComboBox = new ToolStripComboBox();
            toolStripGpgKeyTextBox = new ToolStripTextBox();
            commitTemplatesToolStripMenuItem = new ToolStripDropDownButton();
            createBranchToolStripButton = new ToolStripButton();
            commitStatusStrip = new StatusStrip();
            commitAuthorStatus = new ToolStripStatusLabel();
            toolStripStatusBranchIcon = new ToolStripStatusLabel();
            branchNameLabel = new ToolStripStatusLabel();
            remoteNameLabel = new ToolStripStatusLabel();
            commitStagedCountLabel = new ToolStripStatusLabel();
            commitStagedCount = new ToolStripStatusLabel();
            commitCursorLineLabel = new ToolStripStatusLabel();
            commitCursorLine = new ToolStripStatusLabel();
            commitCursorColumnLabel = new ToolStripStatusLabel();
            commitCursorColumn = new ToolStripStatusLabel();
            commitEndPadding = new ToolStripStatusLabel();
            stopTrackingThisFileToolStripMenuItem = new ToolStripMenuItem();
            UnstagedFileContext.SuspendLayout();
            StagedFileContext.SuspendLayout();
            UnstagedSubmoduleContext.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(gitItemStatusBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(splitMain)).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitLeft)).BeginInit();
            splitLeft.Panel1.SuspendLayout();
            splitLeft.Panel2.SuspendLayout();
            splitLeft.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            toolbarUnstaged.SuspendLayout();
            toolbarSelectionFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(LoadingStaged)).BeginInit();
            toolbarStaged.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitRight)).BeginInit();
            splitRight.Panel1.SuspendLayout();
            splitRight.Panel2.SuspendLayout();
            splitRight.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowCommitButtons.SuspendLayout();
            AmendPanel.SuspendLayout();
            toolbarCommit.SuspendLayout();
            commitStatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // UnstagedFileContext
            // 
            UnstagedFileContext.Items.AddRange(new ToolStripItem[] {
            stageToolStripMenuItem,
            tsmiResetUnstagedChanges,
            resetPartOfFileToolStripMenuItem,
            interactiveAddToolStripMenuItem,
            toolStripSeparator12,
            openWithDifftoolToolStripMenuItem,
            openToolStripMenuItem,
            openWithToolStripMenuItem,
            editFileToolStripMenuItem,
            deleteFileToolStripMenuItem,
            toolStripSeparator9,
            filenameToClipboardToolStripMenuItem,
            openContainingFolderToolStripMenuItem,
            toolStripSeparator8,
            viewFileHistoryToolStripItem,
            toolStripSeparator5,
            addFileToGitIgnoreToolStripMenuItem,
            addFileToGitInfoExcludeLocallyToolStripMenuItem,
            stopTrackingThisFileToolStripMenuItem,
            skipWorktreeToolStripMenuItem,
            doNotSkipWorktreeToolStripMenuItem,
            assumeUnchangedToolStripMenuItem,
            doNotAssumeUnchangedToolStripMenuItem,
            toolStripSeparatorScript,
            runScriptToolStripMenuItem});
            UnstagedFileContext.Name = "UnstagedFileContext";
            UnstagedFileContext.Size = new Size(233, 414);
            UnstagedFileContext.Opening += UnstagedFileContext_Opening;
            // 
            // tsmiResetUnstagedChanges
            // 
            tsmiResetUnstagedChanges.Image = Properties.Images.ResetWorkingDirChanges;
            tsmiResetUnstagedChanges.Name = "tsmiResetUnstagedChanges";
            tsmiResetUnstagedChanges.Size = new Size(232, 22);
            tsmiResetUnstagedChanges.Text = "Reset file or directory changes";
            tsmiResetUnstagedChanges.Click += ResetFilesClick;
            // 
            // resetPartOfFileToolStripMenuItem
            // 
            resetPartOfFileToolStripMenuItem.Name = "resetPartOfFileToolStripMenuItem";
            resetPartOfFileToolStripMenuItem.Size = new Size(232, 22);
            resetPartOfFileToolStripMenuItem.Text = "Reset chunk of file";
            resetPartOfFileToolStripMenuItem.Click += ResetPartOfFileToolStripMenuItemClick;
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(229, 6);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(232, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += OpenToolStripMenuItemClick;
            // 
            // openWithToolStripMenuItem
            // 
            openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            openWithToolStripMenuItem.Size = new Size(232, 22);
            openWithToolStripMenuItem.Text = "Open with...";
            openWithToolStripMenuItem.Click += OpenWithToolStripMenuItemClick;
            // 
            // stageToolStripMenuItem
            // 
            stageToolStripMenuItem.Image = Properties.Images.Stage;
            stageToolStripMenuItem.Name = "stageToolStripMenuItem";
            stageToolStripMenuItem.Size = new Size(232, 22);
            stageToolStripMenuItem.Font = new Font(stageToolStripMenuItem.Font, FontStyle.Bold);
            stageToolStripMenuItem.Click += StageClick;
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            openWithDifftoolToolStripMenuItem.Image = Properties.Images.Diff;
            openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            openWithDifftoolToolStripMenuItem.Size = new Size(232, 22);
            openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            openWithDifftoolToolStripMenuItem.Click += openWithDifftoolToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(229, 6);
            // 
            // filenameToClipboardToolStripMenuItem
            // 
            filenameToClipboardToolStripMenuItem.Image = Properties.Images.CopyToClipboard;
            filenameToClipboardToolStripMenuItem.Name = "filenameToClipboardToolStripMenuItem";
            filenameToClipboardToolStripMenuItem.Size = new Size(232, 22);
            filenameToClipboardToolStripMenuItem.Text = "Copy full path";
            filenameToClipboardToolStripMenuItem.Click += FilenameToClipboardToolStripMenuItemClick;
            // 
            // openContainingFolderToolStripMenuItem
            // 
            openContainingFolderToolStripMenuItem.Image = Properties.Images.BrowseFileExplorer;
            openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            openContainingFolderToolStripMenuItem.Size = new Size(232, 22);
            openContainingFolderToolStripMenuItem.Text = "Show in folder";
            openContainingFolderToolStripMenuItem.Click += openContainingFolderToolStripMenuItem_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(229, 6);
            // 
            // viewFileHistoryToolStripItem
            // 
            viewFileHistoryToolStripItem.Image = Properties.Images.FileHistory;
            viewFileHistoryToolStripItem.Name = "viewFileHistoryToolStripItem";
            viewFileHistoryToolStripItem.Size = new Size(232, 22);
            viewFileHistoryToolStripItem.Text = "View file history";
            viewFileHistoryToolStripItem.Click += ViewFileHistoryMenuItem_Click;
            // 
            // editFileToolStripMenuItem
            //
            editFileToolStripMenuItem.Image = Properties.Images.EditFile;
            editFileToolStripMenuItem.Name = "editFileToolStripMenuItem";
            editFileToolStripMenuItem.Size = new Size(232, 22);
            editFileToolStripMenuItem.Text = "Edit file";
            editFileToolStripMenuItem.Click += editFileToolStripMenuItem_Click;
            //
            // deleteFileToolStripMenuItem
            //
            deleteFileToolStripMenuItem.Image = Properties.Images.DeleteFile;
            deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            deleteFileToolStripMenuItem.Size = new Size(232, 22);
            deleteFileToolStripMenuItem.Text = "Delete file";
            deleteFileToolStripMenuItem.Click += DeleteFileToolStripMenuItemClick;
            //
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(229, 6);
            // 
            // addFileToGitIgnoreToolStripMenuItem
            // 
            addFileToGitIgnoreToolStripMenuItem.Image = Properties.Images.AddToGitIgnore;
            addFileToGitIgnoreToolStripMenuItem.Name = "addFileToGitIgnoreToolStripMenuItem";
            addFileToGitIgnoreToolStripMenuItem.Size = new Size(232, 22);
            addFileToGitIgnoreToolStripMenuItem.Text = "Add file to .gitignore";
            addFileToGitIgnoreToolStripMenuItem.Click += AddFileToGitIgnoreToolStripMenuItemClick;
            // 
            // addFileToGitInfoExcludeLocallyToolStripMenuItem
            // 
            addFileToGitInfoExcludeLocallyToolStripMenuItem.Image = Properties.Images.AddToGitIgnore;
            addFileToGitInfoExcludeLocallyToolStripMenuItem.Name = "addFileToGitInfoExcludeLocallyToolStripMenuItem";
            addFileToGitInfoExcludeLocallyToolStripMenuItem.Size = new Size(232, 22);
            addFileToGitInfoExcludeLocallyToolStripMenuItem.Text = "Add file to .git/info/exclude";
            addFileToGitInfoExcludeLocallyToolStripMenuItem.Click += AddFileToGitInfoExcludeToolStripMenuItemClick;
            // 
            // skipWorktreeToolStripMenuItem
            // 
            skipWorktreeToolStripMenuItem.Name = "skipWorktreeToolStripMenuItem";
            skipWorktreeToolStripMenuItem.Size = new Size(232, 22);
            skipWorktreeToolStripMenuItem.Text = "Skip worktree";
            skipWorktreeToolStripMenuItem.Click += SkipWorktreeToolStripMenuItemClick;
            // 
            // doNotSkipWorktreeToolStripMenuItem
            // 
            doNotSkipWorktreeToolStripMenuItem.Name = "doNotSkipWorktreeToolStripMenuItem";
            doNotSkipWorktreeToolStripMenuItem.Size = new Size(232, 22);
            doNotSkipWorktreeToolStripMenuItem.Text = "Do not skip worktree";
            doNotSkipWorktreeToolStripMenuItem.Visible = false;
            doNotSkipWorktreeToolStripMenuItem.Click += DoNotSkipWorktreeToolStripMenuItemClick;
            // 
            // assumeUnchangedToolStripMenuItem
            // 
            assumeUnchangedToolStripMenuItem.Name = "assumeUnchangedToolStripMenuItem";
            assumeUnchangedToolStripMenuItem.Size = new Size(232, 22);
            assumeUnchangedToolStripMenuItem.Text = "Assume unchanged";
            assumeUnchangedToolStripMenuItem.Click += AssumeUnchangedToolStripMenuItemClick;
            // 
            // doNotAssumeUnchangedToolStripMenuItem
            // 
            doNotAssumeUnchangedToolStripMenuItem.Name = "doNotAssumeUnchangedToolStripMenuItem";
            doNotAssumeUnchangedToolStripMenuItem.Size = new Size(232, 22);
            doNotAssumeUnchangedToolStripMenuItem.Text = "Do not assume unchanged";
            doNotAssumeUnchangedToolStripMenuItem.Visible = false;
            doNotAssumeUnchangedToolStripMenuItem.Click += DoNotAssumeUnchangedToolStripMenuItemClick;
            // 
            // interactiveAddToolStripMenuItem
            //
            interactiveAddToolStripMenuItem.Name = "interactiveAddToolStripMenuItem";
            interactiveAddToolStripMenuItem.Size = new Size(232, 22);
            interactiveAddToolStripMenuItem.Text = "Interactive Add";
            interactiveAddToolStripMenuItem.Click += interactiveAddToolStripMenuItem_Click;
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
            // StageInSuperproject
            // 
            StageInSuperproject.AutoSize = true;
            StageInSuperproject.Location = new Point(0, 70);
            StageInSuperproject.Margin = new Padding(0, 9, 0, 3);
            StageInSuperproject.Name = "StageInSuperproject";
            StageInSuperproject.Size = new Size(130, 17);
            StageInSuperproject.TabIndex = 103;
            StageInSuperproject.Text = "Stage &in Superproject";
            fileTooltip.SetToolTip(StageInSuperproject, "Stage current submodule in superproject after commit");
            StageInSuperproject.UseVisualStyleBackColor = true;
            StageInSuperproject.CheckedChanged += StageInSuperproject_CheckedChanged;
            // 
            // StagedFileContext
            // 
            StagedFileContext.Items.AddRange(new ToolStripItem[] {
            stagedUnstageToolStripMenuItem,
            stagedResetChanges,
            stagedToolStripSeparator14,
            stagedOpenDifftoolToolStripMenuItem9,
            stagedOpenToolStripMenuItem7,
            stagedOpenWithToolStripMenuItem8,
            stagedEditFileToolStripMenuItem11,
            stagedToolStripSeparator18,
            stagedCopyPathToolStripMenuItem14,
            stagedOpenFolderToolStripMenuItem10,
            stagedFileHistoryToolStripSeparator,
            stagedFileHistoryToolStripMenuItem6,
            stagedToolStripSeparatorScript,
            stagedRunScriptToolStripMenuItem});
            StagedFileContext.Name = "StagedFileContext";
            StagedFileContext.Size = new Size(233, 198);
            StagedFileContext.Opening += StagedFileContext_Opening;
            // 
            // stagedResetChanges
            // 
            stagedResetChanges.Image = Properties.Images.ResetWorkingDirChanges;
            stagedResetChanges.Name = "stagedResetChanges";
            stagedResetChanges.Size = new Size(232, 22);
            stagedResetChanges.Text = "Reset file or directory changes";
            stagedResetChanges.Click += ResetFilesClick;
            // 
            // stagedFileHistoryToolStripMenuItem6
            //
            stagedFileHistoryToolStripMenuItem6.Image = Properties.Images.FileHistory;
            stagedFileHistoryToolStripMenuItem6.Name = "stagedFileHistoryToolStripMenuItem6";
            stagedFileHistoryToolStripMenuItem6.Size = new Size(232, 22);
            stagedFileHistoryToolStripMenuItem6.Text = "View file history";
            stagedFileHistoryToolStripMenuItem6.Click += ViewFileHistoryMenuItem_Click;
            //
            // stagedFileHistoryToolStripSeparator
            //
            stagedFileHistoryToolStripSeparator.Name = "stagedFileHistoryToolStripSeparator";
            stagedFileHistoryToolStripSeparator.Size = new Size(229, 6);
            //
            // stagedToolStripSeparator14
            // 
            stagedToolStripSeparator14.Name = "stagedToolStripSeparator14";
            stagedToolStripSeparator14.Size = new Size(229, 6);
            // 
            // stagedOpenToolStripMenuItem7
            // 
            stagedOpenToolStripMenuItem7.Name = "stagedOpenToolStripMenuItem7";
            stagedOpenToolStripMenuItem7.Size = new Size(232, 22);
            stagedOpenToolStripMenuItem7.Text = "Open";
            stagedOpenToolStripMenuItem7.Click += OpenToolStripMenuItemClick;
            // 
            // stagedOpenWithToolStripMenuItem8
            // 
            stagedOpenWithToolStripMenuItem8.Name = "stagedOpenWithToolStripMenuItem8";
            stagedOpenWithToolStripMenuItem8.Size = new Size(232, 22);
            stagedOpenWithToolStripMenuItem8.Text = "Open with...";
            stagedOpenWithToolStripMenuItem8.Click += OpenWithToolStripMenuItemClick;
            // 
            // stagedUnstageToolStripMenuItem
            //
            stagedUnstageToolStripMenuItem.Image = Properties.Images.Unstage;
            stagedUnstageToolStripMenuItem.Name = "stagedUnstageToolStripMenuItem";
            stagedUnstageToolStripMenuItem.Size = new Size(232, 22);
            stagedUnstageToolStripMenuItem.Font = new Font(stagedUnstageToolStripMenuItem.Font, FontStyle.Bold);
            stagedUnstageToolStripMenuItem.Click += UnstageFilesClick;
            //
            // stagedOpenDifftoolToolStripMenuItem9
            // 
            stagedOpenDifftoolToolStripMenuItem9.Image = Properties.Images.Diff;
            stagedOpenDifftoolToolStripMenuItem9.Name = "stagedOpenDifftoolToolStripMenuItem9";
            stagedOpenDifftoolToolStripMenuItem9.Size = new Size(232, 22);
            stagedOpenDifftoolToolStripMenuItem9.Text = "Open with difftool";
            stagedOpenDifftoolToolStripMenuItem9.Click += stagedOpenDifftoolToolStripMenuItem9_Click;
            // 
            // stagedToolStripSeparator18
            // 
            stagedToolStripSeparator18.Name = "stagedToolStripSeparator18";
            stagedToolStripSeparator18.Size = new Size(229, 6);
            // 
            // stagedCopyPathToolStripMenuItem14
            // 
            stagedCopyPathToolStripMenuItem14.Image = Properties.Images.CopyToClipboard;
            stagedCopyPathToolStripMenuItem14.Name = "stagedCopyPathToolStripMenuItem14";
            stagedCopyPathToolStripMenuItem14.Size = new Size(232, 22);
            stagedCopyPathToolStripMenuItem14.Text = "Copy full path";
            stagedCopyPathToolStripMenuItem14.Click += FilenameToClipboardToolStripMenuItemClick;
            // 
            // stagedOpenFolderToolStripMenuItem10
            // 
            stagedOpenFolderToolStripMenuItem10.Image = Properties.Images.BrowseFileExplorer;
            stagedOpenFolderToolStripMenuItem10.Name = "stagedOpenFolderToolStripMenuItem10";
            stagedOpenFolderToolStripMenuItem10.Size = new Size(232, 22);
            stagedOpenFolderToolStripMenuItem10.Text = "Show in folder";
            stagedOpenFolderToolStripMenuItem10.Click += openFolderToolStripMenuItem10_Click;
            // 
            // stagedEditFileToolStripMenuItem11
            // 
            stagedEditFileToolStripMenuItem11.Image = Properties.Images.EditFile;
            stagedEditFileToolStripMenuItem11.Name = "stagedEditFileToolStripMenuItem11";
            stagedEditFileToolStripMenuItem11.Size = new Size(232, 22);
            stagedEditFileToolStripMenuItem11.Text = "Edit file";
            stagedEditFileToolStripMenuItem11.Click += editFileToolStripMenuItem_Click;
            // 
            // stagedToolStripSeparatorScript
            // 
            stagedToolStripSeparatorScript.Name = "stagedToolStripSeparatorScript";
            stagedToolStripSeparatorScript.Size = new Size(259, 6);
            // 
            // stagedRunScriptToolStripMenuItem
            // 
            stagedRunScriptToolStripMenuItem.Image = Properties.Images.Console;
            stagedRunScriptToolStripMenuItem.Name = "stagedRunScriptToolStripMenuItem";
            stagedRunScriptToolStripMenuItem.Size = new Size(262, 22);
            stagedRunScriptToolStripMenuItem.Text = "Run script";
            // 
            // UnstagedSubmoduleContext
            // 
            UnstagedSubmoduleContext.Items.AddRange(new ToolStripItem[] {
            updateSubmoduleMenuItem,
            resetSubmoduleChanges,
            stashSubmoduleChangesToolStripMenuItem,
            commitSubmoduleChanges,
            toolStripSeparator15,
            stageSubmoduleToolStripMenuItem,
            unstagedSubmoduleStageToolStripSeparator,
            copyFolderNameMenuItem,
            openFolderMenuItem,
            toolStripSeparator13,
            viewHistoryMenuItem});
            UnstagedSubmoduleContext.Name = "UnstagedSubmoduleContext";
            UnstagedSubmoduleContext.Size = new Size(229, 242);
            UnstagedSubmoduleContext.Opening += UnstagedSubmoduleContext_Opening;
            // 
            // commitSubmoduleChanges
            // 
            commitSubmoduleChanges.Image = Properties.Images.RepoStateDirtySubmodules;
            commitSubmoduleChanges.Name = "commitSubmoduleChanges";
            commitSubmoduleChanges.Size = new Size(228, 22);
            commitSubmoduleChanges.Text = "Commit submodule changes";
            commitSubmoduleChanges.Click += commitSubmoduleChanges_Click;
            // 
            // resetSubmoduleChanges
            // 
            resetSubmoduleChanges.Image = Properties.Images.ResetWorkingDirChanges;
            resetSubmoduleChanges.Name = "resetSubmoduleChanges";
            resetSubmoduleChanges.Size = new Size(228, 22);
            resetSubmoduleChanges.Text = "Reset submodule changes";
            resetSubmoduleChanges.Click += resetSubmoduleChanges_Click;
            // 
            // stashSubmoduleChangesToolStripMenuItem
            // 
            stashSubmoduleChangesToolStripMenuItem.Image = Properties.Images.Stash;
            stashSubmoduleChangesToolStripMenuItem.Name = "stashSubmoduleChangesToolStripMenuItem";
            stashSubmoduleChangesToolStripMenuItem.Size = new Size(228, 22);
            stashSubmoduleChangesToolStripMenuItem.Text = "Stash submodule changes";
            stashSubmoduleChangesToolStripMenuItem.Click += stashSubmoduleChangesToolStripMenuItem_Click;
            // 
            // updateSubmoduleMenuItem
            // 
            updateSubmoduleMenuItem.Image = Properties.Images.SubmodulesUpdate;
            updateSubmoduleMenuItem.Name = "updateSubmoduleMenuItem";
            updateSubmoduleMenuItem.Size = new Size(228, 22);
            updateSubmoduleMenuItem.Tag = "1";
            updateSubmoduleMenuItem.Text = "Update submodule";
            updateSubmoduleMenuItem.Click += updateSubmoduleMenuItem_Click;
            // 
            // stageSubmoduleToolStripMenuItem
            // 
            stageSubmoduleToolStripMenuItem.Image = Properties.Images.Stage;
            stageSubmoduleToolStripMenuItem.Name = "stageSubmoduleToolStripMenuItem";
            stageSubmoduleToolStripMenuItem.Size = new Size(232, 22);
            stageSubmoduleToolStripMenuItem.Font = new Font(stageToolStripMenuItem.Font, FontStyle.Bold);
            stageSubmoduleToolStripMenuItem.Click += StageClick;
            // 
            // toolStripSeparator13
            //
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(225, 6);
            toolStripSeparator13.Tag = "1";
            //
            // viewHistoryMenuItem
            //
            viewHistoryMenuItem.Image = Properties.Images.FileHistory;
            viewHistoryMenuItem.Name = "viewHistoryMenuItem";
            viewHistoryMenuItem.Size = new Size(228, 22);
            viewHistoryMenuItem.Text = "View history";
            viewHistoryMenuItem.Click += ViewFileHistoryMenuItem_Click;
            //
            // toolStripSeparator15
            //
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(225, 6);
            //
            // unstagedSubmoduleStageToolStripSeparator
            // 
            unstagedSubmoduleStageToolStripSeparator.Name = "unstagedSubmoduleStageToolStripSeparator";
            unstagedSubmoduleStageToolStripSeparator.Size = new Size(225, 6);
            // 
            // openFolderMenuItem
            // 
            openFolderMenuItem.Image = Properties.Images.BrowseFileExplorer;
            openFolderMenuItem.Name = "openFolderMenuItem";
            openFolderMenuItem.Size = new Size(228, 22);
            openFolderMenuItem.Text = "Show in folder";
            openFolderMenuItem.Click += OpenToolStripMenuItemClick;
            // 
            // copyFolderNameMenuItem
            // 
            copyFolderNameMenuItem.Image = Properties.Images.CopyToClipboard;
            copyFolderNameMenuItem.Name = "copyFolderNameMenuItem";
            copyFolderNameMenuItem.Size = new Size(228, 22);
            copyFolderNameMenuItem.Text = "Copy folder name";
            copyFolderNameMenuItem.Click += FilenameToClipboardToolStripMenuItemClick;
            // 
            // gitItemStatusBindingSource
            // 
            gitItemStatusBindingSource.DataSource = typeof(GitItemStatus);
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(134, 167);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(129, 23);
            Cancel.TabIndex = 15;
            Cancel.TabStop = false;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            // 
            // splitMain
            // 
            splitMain.BackColor = SystemColors.Control;
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(splitLeft);
            splitMain.Panel1.Controls.Add(Ok);
            splitMain.Panel1.Padding = new Padding(6, 6, 0, 6);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(splitRight);
            splitMain.Size = new Size(918, 622);
            splitMain.SplitterDistance = 397;
            splitMain.TabIndex = 0;
            splitMain.TabStop = false;
            splitMain.Panel2.Padding = new Padding(0, 6, 6, 6);
            // 
            // splitLeft
            // 
            splitLeft.Dock = DockStyle.Fill;
            splitLeft.Location = new Point(6, 6);
            splitLeft.Name = "splitLeft";
            splitLeft.Orientation = Orientation.Horizontal;
            // 
            // splitLeft.Panel1
            // 
            splitLeft.Panel1.Controls.Add(toolStripContainer1);
            splitLeft.Panel1.Padding = new Padding(1);
            // 
            // splitLeft.Panel2
            // 
            splitLeft.Panel2.Controls.Add(LoadingStaged);
            splitLeft.Panel2.Controls.Add(Staged);
            splitLeft.Panel2.Controls.Add(Cancel);
            splitLeft.Panel2.Controls.Add(toolbarStaged);
            splitLeft.Panel2.Padding = new Padding(1);
            splitLeft.Size = new Size(397, 622);
            splitLeft.SplitterDistance = 274;
            splitLeft.SplitterWidth = 6;
            splitLeft.TabIndex = 3;
            splitLeft.TabStop = false;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(Loading);
            toolStripContainer1.ContentPanel.Controls.Add(Unstaged);
            toolStripContainer1.ContentPanel.Size = new Size(397, 249);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(397, 274);
            toolStripContainer1.TabIndex = 13;
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolbarUnstaged);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolbarSelectionFilter);
            // 
            // Loading
            // 
            Loading.BackColor = SystemColors.Control;
            Loading.BackgroundImageLayout = ImageLayout.None;
            Loading.Dock = DockStyle.Fill;
            Loading.Location = new Point(0, 0);
            Loading.Name = "Loading";
            Loading.Size = new Size(397, 249);
            Loading.TabIndex = 11;
            Loading.TabStop = false;
            // 
            // Unstaged
            // 
            Unstaged.Dock = DockStyle.Fill;
            Unstaged.Location = new Point(0, 0);
            Unstaged.Margin = new Padding(3, 4, 3, 4);
            Unstaged.Name = "Unstaged";
            Unstaged.SelectFirstItemOnSetItems = false;
            Unstaged.Size = new Size(397, 249);
            Unstaged.TabIndex = 0;
            Unstaged.SelectedIndexChanged += UnstagedSelectionChanged;
            Unstaged.DataSourceChanged += Staged_DataSourceChanged;
            Unstaged.DoubleClick += Unstaged_DoubleClick;
            Unstaged.Enter += Unstaged_Enter;
            // 
            // toolbarUnstaged
            // 
            toolbarUnstaged.AutoSize = false;
            toolbarUnstaged.BackColor = SystemColors.Control;
            toolbarUnstaged.ClickThrough = true;
            toolbarUnstaged.Dock = DockStyle.None;
            toolbarUnstaged.DrawBorder = false;
            toolbarUnstaged.GripStyle = ToolStripGripStyle.Hidden;
            toolbarUnstaged.Items.AddRange(new ToolStripItem[] {
            toolRefreshItem,
            toolStripSeparator6,
            workingToolStripMenuItem,
            toolStripProgressBar1});
            toolbarUnstaged.Location = new Point(0, 0);
            toolbarUnstaged.Name = "toolbarUnstaged";
            toolbarUnstaged.Padding = new Padding(2, 1, 2, 1);
            toolbarUnstaged.RenderMode = ToolStripRenderMode.System;
            toolbarUnstaged.Size = new Size(397, 25);
            toolbarUnstaged.Stretch = true;
            toolbarUnstaged.TabIndex = 12;
            // 
            // toolRefreshItem
            // 
            toolRefreshItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolRefreshItem.Image = Properties.Images.ReloadRevisions;
            toolRefreshItem.ImageTransparentColor = Color.Magenta;
            toolRefreshItem.Name = "toolRefreshItem";
            toolRefreshItem.Size = new Size(23, 20);
            toolRefreshItem.Text = "Refresh";
            toolRefreshItem.Click += RescanChangesToolStripMenuItemClick;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(6, 23);
            // 
            // workingToolStripMenuItem
            // 
            workingToolStripMenuItem.AutoToolTip = false;
            workingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            showIgnoredFilesToolStripMenuItem,
            showSkipWorktreeFilesToolStripMenuItem,
            showAssumeUnchangedFilesToolStripMenuItem,
            showUntrackedFilesToolStripMenuItem,
            toolStripSeparator1,
            editGitIgnoreToolStripMenuItem,
            editLocallyIgnoredFilesToolStripMenuItem,
            deleteAllUntrackedFilesToolStripMenuItem,
            toolStripMenuItem2,
            selectionFilterToolStripMenuItem});
            workingToolStripMenuItem.Image = Properties.Images.WorkingDirChanges;
            workingToolStripMenuItem.Name = "workingToolStripMenuItem";
            workingToolStripMenuItem.Size = new Size(178, 20);
            workingToolStripMenuItem.Text = "&Working directory changes";
            // 
            // showIgnoredFilesToolStripMenuItem
            // 
            showIgnoredFilesToolStripMenuItem.Name = "showIgnoredFilesToolStripMenuItem";
            showIgnoredFilesToolStripMenuItem.Size = new Size(242, 22);
            showIgnoredFilesToolStripMenuItem.Text = "Show ignored files";
            showIgnoredFilesToolStripMenuItem.Click += ShowIgnoredFilesToolStripMenuItemClick;
            // 
            // showSkipWorktreeFilesToolStripMenuItem
            // 
            showSkipWorktreeFilesToolStripMenuItem.Name = "showSkipWorktreeFilesToolStripMenuItem";
            showSkipWorktreeFilesToolStripMenuItem.Size = new Size(242, 22);
            showSkipWorktreeFilesToolStripMenuItem.Text = "Show skip-worktree files";
            showSkipWorktreeFilesToolStripMenuItem.Click += ShowSkipWorktreeFilesToolStripMenuItemClick;
            // 
            // showAssumeUnchangedFilesToolStripMenuItem
            // 
            showAssumeUnchangedFilesToolStripMenuItem.Name = "showAssumeUnchangedFilesToolStripMenuItem";
            showAssumeUnchangedFilesToolStripMenuItem.Size = new Size(242, 22);
            showAssumeUnchangedFilesToolStripMenuItem.Text = "Show assumed-unchanged files";
            showAssumeUnchangedFilesToolStripMenuItem.Click += ShowAssumeUnchangedFilesToolStripMenuItemClick;
            // 
            // showUntrackedFilesToolStripMenuItem
            // 
            showUntrackedFilesToolStripMenuItem.Checked = true;
            showUntrackedFilesToolStripMenuItem.CheckState = CheckState.Checked;
            showUntrackedFilesToolStripMenuItem.Name = "showUntrackedFilesToolStripMenuItem";
            showUntrackedFilesToolStripMenuItem.Size = new Size(242, 22);
            showUntrackedFilesToolStripMenuItem.Text = "Show untracked files";
            showUntrackedFilesToolStripMenuItem.Click += ShowUntrackedFilesToolStripMenuItemClick;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(239, 6);
            // 
            // editGitIgnoreToolStripMenuItem
            // 
            editGitIgnoreToolStripMenuItem.Image = Properties.Images.EditGitIgnore;
            editGitIgnoreToolStripMenuItem.Name = "editGitIgnoreToolStripMenuItem";
            editGitIgnoreToolStripMenuItem.Size = new Size(242, 22);
            editGitIgnoreToolStripMenuItem.Text = "Edit ignored files";
            editGitIgnoreToolStripMenuItem.Click += EditGitIgnoreToolStripMenuItemClick;
            // 
            // editLocallyIgnoredFilesToolStripMenuItem
            // 
            editLocallyIgnoredFilesToolStripMenuItem.Image = Properties.Images.EditGitIgnore;
            editLocallyIgnoredFilesToolStripMenuItem.Name = "editLocallyIgnoredFilesToolStripMenuItem";
            editLocallyIgnoredFilesToolStripMenuItem.Size = new Size(242, 22);
            editLocallyIgnoredFilesToolStripMenuItem.Text = "Edit locally ignored files";
            editLocallyIgnoredFilesToolStripMenuItem.Click += EditGitInfoExcludeToolStripMenuItemClick;
            // 
            // deleteAllUntrackedFilesToolStripMenuItem
            // 
            deleteAllUntrackedFilesToolStripMenuItem.Name = "deleteAllUntrackedFilesToolStripMenuItem";
            deleteAllUntrackedFilesToolStripMenuItem.Size = new Size(242, 22);
            deleteAllUntrackedFilesToolStripMenuItem.Text = "Delete all untracked files";
            deleteAllUntrackedFilesToolStripMenuItem.Click += DeleteAllUntrackedFilesToolStripMenuItemClick;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(239, 6);
            // 
            // selectionFilterToolStripMenuItem
            // 
            selectionFilterToolStripMenuItem.CheckOnClick = true;
            selectionFilterToolStripMenuItem.Name = "selectionFilterToolStripMenuItem";
            selectionFilterToolStripMenuItem.Size = new Size(242, 22);
            selectionFilterToolStripMenuItem.Text = "Selection filter";
            selectionFilterToolStripMenuItem.CheckedChanged += ToggleShowSelectionFilter;
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Alignment = ToolStripItemAlignment.Right;
            toolStripProgressBar1.Margin = new Padding(0);
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Overflow = ToolStripItemOverflow.Never;
            toolStripProgressBar1.Size = new Size(150, 27);
            toolStripProgressBar1.Visible = false;
            // 
            // toolbarSelectionFilter
            // 
            toolbarSelectionFilter.ClickThrough = true;
            toolbarSelectionFilter.Dock = DockStyle.None;
            toolbarSelectionFilter.DrawBorder = false;
            toolbarSelectionFilter.Items.AddRange(new ToolStripItem[] {
            toolStripLabel1,
            selectionFilter});
            toolbarSelectionFilter.Location = new Point(3, 0);
            toolbarSelectionFilter.Name = "toolbarSelectionFilter";
            toolbarSelectionFilter.Size = new Size(219, 25);
            toolbarSelectionFilter.TabIndex = 13;
            toolbarSelectionFilter.Visible = false;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(84, 22);
            toolStripLabel1.Text = "Selection Filter";
            // 
            // selectionFilter
            // 
            selectionFilter.Name = "selectionFilter";
            selectionFilter.Size = new Size(121, 25);
            selectionFilter.SelectedIndexChanged += OnSelectionFilterIndexChanged;
            selectionFilter.TextChanged += OnSelectionFilterTextChanged;
            // 
            // LoadingStaged
            // 
            LoadingStaged.BackColor = SystemColors.AppWorkspace;
            LoadingStaged.BackgroundImageLayout = ImageLayout.None;
            LoadingStaged.Dock = DockStyle.Fill;
            LoadingStaged.Location = new Point(0, 28);
            LoadingStaged.Name = "LoadingStaged";
            LoadingStaged.Size = new Size(397, 314);
            LoadingStaged.SizeMode = PictureBoxSizeMode.CenterImage;
            LoadingStaged.TabIndex = 17;
            LoadingStaged.TabStop = false;
            // 
            // Staged
            // 
            Staged.ContextMenuStrip = StagedFileContext;
            Staged.Dock = DockStyle.Fill;
            Staged.Location = new Point(0, 28);
            Staged.Margin = new Padding(3, 4, 3, 4);
            Staged.Name = "Staged";
            Staged.SelectFirstItemOnSetItems = false;
            Staged.Size = new Size(397, 314);
            Staged.TabIndex = 0;
            Staged.SelectedIndexChanged += StagedSelectionChanged;
            Staged.DataSourceChanged += Staged_DataSourceChanged;
            Staged.DoubleClick += Staged_DoubleClick;
            Staged.Enter += Staged_Enter;
            // 
            // toolbarStaged
            // 
            toolbarStaged.AutoSize = false;
            toolbarStaged.BackColor = SystemColors.Control;
            toolbarStaged.ClickThrough = true;
            toolbarStaged.DrawBorder = false;
            toolbarStaged.GripStyle = ToolStripGripStyle.Hidden;
            toolbarStaged.Items.AddRange(new ToolStripItem[] {
            toolStageAllItem,
            toolStripSeparator10,
            toolStageItem,
            toolUnstageAllItem,
            toolStripSeparator11,
            toolUnstageItem});
            toolbarStaged.Location = new Point(0, 0);
            toolbarStaged.Name = "toolbarStaged";
            toolbarStaged.Padding = new Padding(2, 1, 2, 1);
            toolbarStaged.RenderMode = ToolStripRenderMode.System;
            toolbarStaged.Size = new Size(397, 28);
            toolbarStaged.TabIndex = 13;
            // 
            // toolStageAllItem
            // 
            toolStageAllItem.Alignment = ToolStripItemAlignment.Right;
            toolStageAllItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStageAllItem.Image = Properties.Images.StageAll;
            toolStageAllItem.ImageTransparentColor = Color.Magenta;
            toolStageAllItem.Name = "toolStageAllItem";
            toolStageAllItem.Size = new Size(23, 23);
            toolStageAllItem.Click += toolStageAllItem_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(6, 26);
            // 
            // toolStageItem
            // 
            toolStageItem.Alignment = ToolStripItemAlignment.Right;
            toolStageItem.AutoToolTip = false;
            toolStageItem.Image = Properties.Images.Stage;
            toolStageItem.ImageTransparentColor = Color.Magenta;
            toolStageItem.Name = "toolStageItem";
            toolStageItem.Size = new Size(56, 23);
            toolStageItem.Text = "&Stage";
            toolStageItem.TextAlign = ContentAlignment.MiddleRight;
            toolStageItem.Click += StageClick;
            // 
            // toolUnstageAllItem
            // 
            toolUnstageAllItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolUnstageAllItem.Image = Properties.Images.UnstageAll;
            toolUnstageAllItem.ImageTransparentColor = Color.Magenta;
            toolUnstageAllItem.Name = "toolUnstageAllItem";
            toolUnstageAllItem.Size = new Size(23, 23);
            toolUnstageAllItem.Click += toolUnstageAllItem_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(6, 26);
            // 
            // toolUnstageItem
            // 
            toolUnstageItem.AutoToolTip = false;
            toolUnstageItem.Image = Properties.Images.Unstage;
            toolUnstageItem.ImageTransparentColor = Color.Magenta;
            toolUnstageItem.Name = "toolUnstageItem";
            toolUnstageItem.Size = new Size(70, 23);
            toolUnstageItem.Text = "&Unstage";
            toolUnstageItem.TextAlign = ContentAlignment.MiddleRight;
            toolUnstageItem.Click += UnstageFilesClick;
            // 
            // Ok
            // 
            Ok.Location = new Point(334, 10);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 2;
            Ok.Text = "Commit";
            Ok.UseVisualStyleBackColor = true;
            // 
            // splitRight
            // 
            splitRight.Dock = DockStyle.Fill;
            splitRight.FixedPanel = FixedPanel.Panel2;
            splitRight.Location = new Point(0, 6);
            splitRight.Name = "splitRight";
            splitRight.Orientation = Orientation.Horizontal;
            // 
            // splitRight.Panel1
            // 
            splitRight.Panel1.Controls.Add(SolveMergeconflicts);
            splitRight.Panel1.Controls.Add(SelectedDiff);
            splitRight.Panel1.Padding = new Padding(1);
            // 
            // splitRight.Panel2
            // 
            splitRight.Panel2.Controls.Add(modifyCommitMessageButton);
            splitRight.Panel2.Controls.Add(tableLayoutPanel1);
            splitRight.Panel2.Padding = new Padding(1);
            splitRight.Size = new Size(517, 622);
            splitRight.SplitterDistance = 426;
            splitRight.TabIndex = 0;
            splitRight.TabStop = false;
            // 
            // modifyCommitMessageButton
            //
            modifyCommitMessageButton.AutoSize = true;
            modifyCommitMessageButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            modifyCommitMessageButton.BackColor = SystemColors.ButtonFace;
            modifyCommitMessageButton.FlatStyle = FlatStyle.Flat;
            modifyCommitMessageButton.ForeColor = SystemColors.HotTrack;
            modifyCommitMessageButton.Location = new Point(185, 55);
            modifyCommitMessageButton.MinimumSize = new Size(149, 25);
            modifyCommitMessageButton.Name = "modifyCommitMessageButton";
            modifyCommitMessageButton.Size = new Size(149, 25);
            modifyCommitMessageButton.TabIndex = 9;
            modifyCommitMessageButton.Text = "Modify the commit m&essage";
            modifyCommitMessageButton.UseVisualStyleBackColor = false;
            modifyCommitMessageButton.Click += modifyCommitMessageButton_Click;
            //
            // SolveMergeconflicts
            // 
            SolveMergeconflicts.BackColor = Color.SeaShell;
            SolveMergeconflicts.FlatStyle = FlatStyle.Flat;
            SolveMergeconflicts.Image = Properties.Images.SolveMerge;
            SolveMergeconflicts.ImageAlign = ContentAlignment.MiddleLeft;
            SolveMergeconflicts.Location = new Point(14, 12);
            SolveMergeconflicts.Name = "SolveMergeconflicts";
            SolveMergeconflicts.Size = new Size(188, 42);
            SolveMergeconflicts.TabIndex = 0;
            SolveMergeconflicts.Text = "There are unresolved merge conflicts\r\n";
            SolveMergeconflicts.UseVisualStyleBackColor = false;
            SolveMergeconflicts.Visible = false;
            SolveMergeconflicts.Click += SolveMergeConflictsClick;
            // 
            // SelectedDiff
            // 
            SelectedDiff.Dock = DockStyle.Fill;
            SelectedDiff.Location = new Point(0, 0);
            SelectedDiff.Margin = new Padding(2, 3, 3, 3);
            SelectedDiff.Name = "SelectedDiff";
            SelectedDiff.Size = new Size(517, 426);
            SelectedDiff.TabIndex = 0;
            SelectedDiff.TabStop = false;
            SelectedDiff.ExtraDiffArgumentsChanged += SelectedDiffExtraDiffArgumentsChanged;
            SelectedDiff.PatchApplied += SelectedDiff_PatchApplied;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = SystemColors.Control;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(Message, 1, 1);
            tableLayoutPanel1.Controls.Add(flowCommitButtons, 0, 0);
            tableLayoutPanel1.Controls.Add(toolbarCommit, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(517, 192);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // Message
            // 
            Message.Dock = DockStyle.Fill;
            Message.Location = new Point(177, 28);
            Message.Margin = new Padding(0);
            Message.Name = "Message";
            Message.Size = new Size(340, 164);
            Message.TabIndex = 7;
            Message.SelectionChanged += Message_SelectionChanged;
            Message.Enter += Message_Enter;
            Message.KeyDown += Message_KeyDown;
            // 
            // AmendPanel
            // 
            AmendPanel.AutoSize = true;
            AmendPanel.Controls.Add(ResetAuthor);
            AmendPanel.Controls.Add(ResetSoft);
            AmendPanel.Dock = DockStyle.Fill;
            AmendPanel.FlowDirection = FlowDirection.TopDown;
            AmendPanel.Location = new Point(12, 117);
            AmendPanel.Margin = new Padding(12, 0, 0, 0);
            AmendPanel.Name = "AmendPanel";
            AmendPanel.Size = new Size(159, 57);
            AmendPanel.TabIndex = 105;
            AmendPanel.Visible = false;
            AmendPanel.WrapContents = false;
            // 
            // flowCommitButtons
            // 
            flowCommitButtons.AutoSize = true;
            flowCommitButtons.Controls.Add(Commit);
            flowCommitButtons.Controls.Add(CommitAndPush);
            flowCommitButtons.Controls.Add(StageInSuperproject);
            flowCommitButtons.Controls.Add(Amend);
            flowCommitButtons.Controls.Add(AmendPanel);
            flowCommitButtons.Controls.Add(StashStaged);
            flowCommitButtons.Controls.Add(btnResetAllChanges);
            flowCommitButtons.Controls.Add(btnResetUnstagedChanges);
            flowCommitButtons.Dock = DockStyle.Fill;
            flowCommitButtons.FlowDirection = FlowDirection.TopDown;
            flowCommitButtons.Location = new Point(0, 0);
            flowCommitButtons.Margin = new Padding(6, 6, 6, 0);
            flowCommitButtons.Name = "flowCommitButtons";
            tableLayoutPanel1.SetRowSpan(flowCommitButtons, 2);
            flowCommitButtons.Size = new Size(171, 192);
            flowCommitButtons.TabIndex = 100;
            flowCommitButtons.WrapContents = false;
            // 
            // Commit
            // 
            Commit.Image = Properties.Images.RepoStateClean;
            Commit.ImageAlign = ContentAlignment.MiddleLeft;
            Commit.Location = new Point(0, 0);
            Commit.Margin = new Padding(0, 0, 0, 3);
            Commit.Name = "Commit";
            Commit.Size = new Size(171, 26);
            Commit.TabIndex = 101;
            Commit.Text = "&Commit";
            Commit.UseVisualStyleBackColor = true;
            Commit.Click += CommitClick;
            // 
            // CommitAndPush
            // 
            CommitAndPush.Image = Properties.Images.ArrowUp;
            CommitAndPush.ImageAlign = ContentAlignment.MiddleLeft;
            CommitAndPush.Location = new Point(0, 32);
            CommitAndPush.Margin = new Padding(0, 3, 0, 3);
            CommitAndPush.Name = "CommitAndPush";
            CommitAndPush.Size = new Size(171, 26);
            CommitAndPush.TabIndex = 102;
            CommitAndPush.UseVisualStyleBackColor = true;
            CommitAndPush.Click += CommitAndPush_Click;
            // 
            // Amend
            // 
            Amend.AutoSize = true;
            Amend.Location = new Point(0, 64);
            Amend.Margin = new Padding(0, 3, 0, 3);
            Amend.Name = "Amend";
            Amend.Size = new Size(97, 17);
            Amend.TabIndex = 104;
            Amend.Text = "&Amend commit";
            Amend.UseVisualStyleBackColor = true;
            Amend.CheckedChanged += Amend_CheckedChanged;
            // 
            // ResetAuthor
            // 
            ResetAuthor.AutoSize = true;
            ResetAuthor.Location = new Point(0, 0);
            ResetAuthor.Margin = new Padding(0, 3, 0, 3);
            ResetAuthor.Name = "ResetAuthor";
            ResetAuthor.Size = new Size(97, 17);
            ResetAuthor.TabIndex = 105;
            ResetAuthor.Text = "R&eset author";
            ResetAuthor.UseVisualStyleBackColor = true;
            // 
            // ResetSoft
            // 
            ResetSoft.Image = Properties.Images.ResetCurrentBranchToHere;
            ResetSoft.ImageAlign = ContentAlignment.MiddleLeft;
            ResetSoft.Margin = new Padding(0, 3, 0, 3);
            ResetSoft.Name = "ResetSoft";
            ResetSoft.Size = new Size(159, 26);
            ResetSoft.TabIndex = 106;
            ResetSoft.Text = "Reset so&ft";
            fileTooltip.SetToolTip(ResetSoft, "Perform a soft reset to the previous commit; leaves working directory and index u" +
        "ntouched");
            ResetSoft.UseVisualStyleBackColor = true;
            ResetSoft.Click += ResetSoftClick;
            // 
            // StashStaged
            // 
            StashStaged.Image = Properties.Images.Stash;
            StashStaged.ImageAlign = ContentAlignment.MiddleLeft;
            StashStaged.Location = new Point(0, 110);
            StashStaged.Margin = new Padding(0, 3, 0, 3);
            StashStaged.Name = "StashStaged";
            StashStaged.Size = new Size(171, 26);
            StashStaged.TabIndex = 107;
            StashStaged.Text = "Stas&h staged changes";
            StashStaged.UseVisualStyleBackColor = true;
            StashStaged.Click += StashStagedClick;
            // 
            // btnResetAllChanges
            // 
            btnResetAllChanges.Image = Properties.Images.ResetWorkingDirChanges;
            btnResetAllChanges.ImageAlign = ContentAlignment.MiddleLeft;
            btnResetAllChanges.Location = new Point(0, 142);
            btnResetAllChanges.Margin = new Padding(0, 3, 0, 3);
            btnResetAllChanges.Name = "btnResetAllChanges";
            btnResetAllChanges.Size = new Size(171, 26);
            btnResetAllChanges.TabIndex = 108;
            btnResetAllChanges.Text = "&Reset all changes";
            btnResetAllChanges.UseVisualStyleBackColor = true;
            btnResetAllChanges.Click += btnResetAllChanges_Click;
            // 
            // btnResetUnstagedChanges
            // 
            btnResetUnstagedChanges.Image = Properties.Images.ResetWorkingDirChanges;
            btnResetUnstagedChanges.ImageAlign = ContentAlignment.MiddleLeft;
            btnResetUnstagedChanges.Location = new Point(0, 174);
            btnResetUnstagedChanges.Margin = new Padding(0, 3, 0, 3);
            btnResetUnstagedChanges.Name = "btnResetUnstagedChanges";
            btnResetUnstagedChanges.Size = new Size(171, 26);
            btnResetUnstagedChanges.TabIndex = 109;
            btnResetUnstagedChanges.Text = "Reset u&nstaged changes";
            btnResetUnstagedChanges.UseVisualStyleBackColor = true;
            btnResetUnstagedChanges.Click += btnResetUnstagedChanges_Click;
            // 
            // toolbarCommit
            // 
            toolbarCommit.AutoSize = false;
            toolbarCommit.BackColor = SystemColors.Control;
            toolbarCommit.ClickThrough = true;
            toolbarCommit.DrawBorder = false;
            toolbarCommit.GripStyle = ToolStripGripStyle.Hidden;
            toolbarCommit.Items.AddRange(new ToolStripItem[] {
            commitMessageToolStripMenuItem,
            toolStripMenuItem3,
            commitTemplatesToolStripMenuItem,
            createBranchToolStripButton});
            toolbarCommit.Location = new Point(177, 0);
            toolbarCommit.Name = "toolbarCommit";
            toolbarCommit.Padding = new Padding(1, 1, 2, 1);
            toolbarCommit.RenderMode = ToolStripRenderMode.System;
            toolbarCommit.Size = new Size(340, 28);
            toolbarCommit.Stretch = true;
            toolbarCommit.TabIndex = 110;
            // 
            // commitMessageToolStripMenuItem
            // 
            commitMessageToolStripMenuItem.AutoToolTip = false;
            commitMessageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            toolStripMenuItem1,
            generateListOfChangesInSubmodulesChangesToolStripMenuItem,
            ShowOnlyMyMessagesToolStripMenuItem});
            commitMessageToolStripMenuItem.Image = Properties.Images.WorkingDirChanges;
            commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            commitMessageToolStripMenuItem.RightToLeft = RightToLeft.No;
            commitMessageToolStripMenuItem.Size = new Size(129, 23);
            commitMessageToolStripMenuItem.Text = "Commit &message";
            commitMessageToolStripMenuItem.DropDownOpening += CommitMessageToolStripMenuItemDropDownOpening;
            commitMessageToolStripMenuItem.DropDownItemClicked += CommitMessageToolStripMenuItemDropDownItemClicked;
            // 
            // ShowOnlyMyMessagesToolStripMenuItem
            //
            ShowOnlyMyMessagesToolStripMenuItem.Checked = true;
            ShowOnlyMyMessagesToolStripMenuItem.CheckOnClick = true;
            ShowOnlyMyMessagesToolStripMenuItem.CheckState = CheckState.Checked;
            ShowOnlyMyMessagesToolStripMenuItem.Name = "ShowOnlyMyMessagesToolStripMenuItem";
            ShowOnlyMyMessagesToolStripMenuItem.Size = new Size(290, 22);
            ShowOnlyMyMessagesToolStripMenuItem.Text = "Show only my messages";
            ShowOnlyMyMessagesToolStripMenuItem.CheckedChanged += ShowOnlyMyMessagesToolStripMenuItem_CheckedChanged;
            //
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(287, 6);
            // 
            // generateListOfChangesInSubmodulesChangesToolStripMenuItem
            // 
            generateListOfChangesInSubmodulesChangesToolStripMenuItem.Name = "generateListOfChangesInSubmodulesChangesToolStripMenuItem";
            generateListOfChangesInSubmodulesChangesToolStripMenuItem.Size = new Size(290, 22);
            generateListOfChangesInSubmodulesChangesToolStripMenuItem.Text = "Generate a list of changes in submodules";
            generateListOfChangesInSubmodulesChangesToolStripMenuItem.Click += generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Alignment = ToolStripItemAlignment.Right;
            toolStripMenuItem3.AutoToolTip = false;
            toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[] {
            closeDialogAfterEachCommitToolStripMenuItem,
            closeDialogAfterAllFilesCommittedToolStripMenuItem,
            refreshDialogOnFormFocusToolStripMenuItem,
            toolStripSeparator2,
            signOffToolStripMenuItem,
            toolAuthorLabelItem,
            toolAuthor,
            noVerifyToolStripMenuItem,
            toolStripSeparator14,
            gpgSignCommitToolStripComboBox,
            toolStripGpgKeyTextBox});
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.RightToLeft = RightToLeft.No;
            toolStripMenuItem3.Size = new Size(62, 23);
            toolStripMenuItem3.Text = "&Options";
            // 
            // closeDialogAfterEachCommitToolStripMenuItem
            // 
            closeDialogAfterEachCommitToolStripMenuItem.Name = "closeDialogAfterEachCommitToolStripMenuItem";
            closeDialogAfterEachCommitToolStripMenuItem.Size = new Size(314, 22);
            closeDialogAfterEachCommitToolStripMenuItem.Text = "Close dialog after each commit";
            closeDialogAfterEachCommitToolStripMenuItem.Click += closeDialogAfterEachCommitToolStripMenuItem_Click;
            // 
            // closeDialogAfterAllFilesCommittedToolStripMenuItem
            // 
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Name = "closeDialogAfterAllFilesCommittedToolStripMenuItem";
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Size = new Size(314, 22);
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Text = "Close dialog when all changes are committed";
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Click += closeDialogAfterAllFilesCommittedToolStripMenuItem_Click;
            // 
            // refreshDialogOnFormFocusToolStripMenuItem
            // 
            refreshDialogOnFormFocusToolStripMenuItem.Name = "refreshDialogOnFormFocusToolStripMenuItem";
            refreshDialogOnFormFocusToolStripMenuItem.Size = new Size(314, 22);
            refreshDialogOnFormFocusToolStripMenuItem.Text = "Refresh dialog on form focus";
            refreshDialogOnFormFocusToolStripMenuItem.Click += refreshDialogOnFormFocusToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(311, 6);
            // 
            // signOffToolStripMenuItem
            // 
            signOffToolStripMenuItem.Name = "signOffToolStripMenuItem";
            signOffToolStripMenuItem.Size = new Size(314, 22);
            signOffToolStripMenuItem.Text = "Sign-off commit";
            signOffToolStripMenuItem.Click += signOffToolStripMenuItem_Click;
            // 
            // toolAuthorLabelItem
            // 
            toolAuthorLabelItem.Enabled = false;
            toolAuthorLabelItem.Name = "toolAuthorLabelItem";
            toolAuthorLabelItem.Size = new Size(314, 22);
            toolAuthorLabelItem.Text = "Author: (Format: \"name <mail>\")";
            toolAuthorLabelItem.Click += toolAuthorLabelItem_Click;
            // 
            // toolAuthor
            // 
            toolAuthor.BorderStyle = BorderStyle.FixedSingle;
            toolAuthor.Name = "toolAuthor";
            toolAuthor.Size = new Size(230, 23);
            toolAuthor.Leave += toolAuthor_Leave;
            toolAuthor.TextChanged += toolAuthor_TextChanged;
            // 
            // noVerifyToolStripMenuItem
            // 
            noVerifyToolStripMenuItem.CheckOnClick = true;
            noVerifyToolStripMenuItem.Name = "noVerifyToolStripMenuItem";
            noVerifyToolStripMenuItem.Size = new Size(314, 22);
            noVerifyToolStripMenuItem.Text = "No verify";
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(311, 6);
            // 
            // gpgSignCommitToolStripComboBox
            // 
            gpgSignCommitToolStripComboBox.BackColor = SystemColors.Control;
            gpgSignCommitToolStripComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            gpgSignCommitToolStripComboBox.Items.AddRange(new object[] {
            "Do not sign commit",
            "Sign with default GPG",
            "Sign with specific GPG"});
            gpgSignCommitToolStripComboBox.MaxDropDownItems = 3;
            gpgSignCommitToolStripComboBox.Name = "gpgSignCommitToolStripComboBox";
            gpgSignCommitToolStripComboBox.Size = new Size(230, 23);
            gpgSignCommitToolStripComboBox.SelectedIndexChanged += gpgSignCommitChanged;
            // 
            // toolStripGpgKeyTextBox
            // 
            toolStripGpgKeyTextBox.BorderStyle = BorderStyle.FixedSingle;
            toolStripGpgKeyTextBox.MaxLength = 16;
            toolStripGpgKeyTextBox.Name = "toolStripGpgKeyTextBox";
            toolStripGpgKeyTextBox.Size = new Size(230, 23);
            toolStripGpgKeyTextBox.Visible = false;
            // 
            // commitTemplatesToolStripMenuItem
            // 
            commitTemplatesToolStripMenuItem.Image = Properties.Images.CommitTemplates;
            commitTemplatesToolStripMenuItem.Name = "commitTemplatesToolStripMenuItem";
            commitTemplatesToolStripMenuItem.RightToLeft = RightToLeft.No;
            commitTemplatesToolStripMenuItem.Size = new Size(135, 20);
            commitTemplatesToolStripMenuItem.Text = "Commit &templates";
            commitTemplatesToolStripMenuItem.DropDownOpening += commitTemplatesToolStripMenuItem_DropDownOpening;
            // 
            // createBranchToolStripButton
            // 
            createBranchToolStripButton.Image = Properties.Images.BranchCreate;
            createBranchToolStripButton.ImageTransparentColor = Color.Magenta;
            createBranchToolStripButton.Name = "createBranchToolStripButton";
            createBranchToolStripButton.Size = new Size(101, 20);
            createBranchToolStripButton.Text = "Create &branch";
            createBranchToolStripButton.Click += createBranchToolStripButton_Click;
            // 
            // commitStatusStrip
            // 
            commitStatusStrip.BackColor = SystemColors.Control;
            commitStatusStrip.Items.AddRange(new ToolStripItem[] {
            commitAuthorStatus,
            toolStripStatusBranchIcon,
            branchNameLabel,
            remoteNameLabel,
            commitStagedCountLabel,
            commitStagedCount,
            commitCursorLineLabel,
            commitCursorLine,
            commitCursorColumnLabel,
            commitCursorColumn,
            commitEndPadding});
            commitStatusStrip.Location = new Point(0, 622);
            commitStatusStrip.Name = "commitStatusStrip";
            commitStatusStrip.Padding = new Padding(1, 0, 11, 0);
            commitStatusStrip.ShowItemToolTips = true;
            commitStatusStrip.Size = new Size(918, 22);
            commitStatusStrip.TabIndex = 13;
            // 
            // commitAuthorStatus
            // 
            commitAuthorStatus.DisplayStyle = ToolStripItemDisplayStyle.Text;
            commitAuthorStatus.IsLink = true;
            commitAuthorStatus.LinkBehavior = LinkBehavior.HoverUnderline;
            commitAuthorStatus.Name = "commitAuthorStatus";
            commitAuthorStatus.Size = new Size(570, 17);
            commitAuthorStatus.Spring = true;
            commitAuthorStatus.TextAlign = ContentAlignment.MiddleLeft;
            commitAuthorStatus.ToolTipText = "Click to change author information.";
            commitAuthorStatus.Click += commitCommitter_Click;
            // 
            // toolStripStatusBranchIcon
            // 
            toolStripStatusBranchIcon.AutoSize = false;
            toolStripStatusBranchIcon.Image = Properties.Images.Branch;
            toolStripStatusBranchIcon.ImageAlign = ContentAlignment.MiddleRight;
            toolStripStatusBranchIcon.Name = "toolStripStatusBranchIcon";
            toolStripStatusBranchIcon.Size = new Size(17, 17);
            toolStripStatusBranchIcon.TextAlign = ContentAlignment.MiddleRight;
            // 
            // branchNameLabel
            // 
            branchNameLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            branchNameLabel.Margin = new Padding(0, 3, 0, 2);
            branchNameLabel.Name = "branchNameLabel";
            branchNameLabel.Size = new Size(85, 17);
            branchNameLabel.Text = "(Branch name)";
            branchNameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // remoteNameLabel
            // 
            remoteNameLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            remoteNameLabel.IsLink = true;
            remoteNameLabel.LinkBehavior = LinkBehavior.HoverUnderline;
            remoteNameLabel.Margin = new Padding(0, 3, 25, 2);
            remoteNameLabel.Name = "remoteNameLabel";
            remoteNameLabel.Size = new Size(85, 17);
            remoteNameLabel.Text = "(Remote name)";
            remoteNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // commitStagedCountLabel
            // 
            commitStagedCountLabel.Name = "commitStagedCountLabel";
            commitStagedCountLabel.Size = new Size(43, 17);
            commitStagedCountLabel.Text = "Staged";
            // 
            // commitStagedCount
            // 
            commitStagedCount.AutoSize = false;
            commitStagedCount.Name = "commitStagedCount";
            commitStagedCount.Size = new Size(40, 17);
            commitStagedCount.Text = "0";
            commitStagedCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // commitCursorLineLabel
            // 
            commitCursorLineLabel.Name = "commitCursorLineLabel";
            commitCursorLineLabel.Size = new Size(20, 17);
            commitCursorLineLabel.Text = "Ln";
            // 
            // commitCursorLine
            // 
            commitCursorLine.AutoSize = false;
            commitCursorLine.Name = "commitCursorLine";
            commitCursorLine.Size = new Size(40, 17);
            commitCursorLine.Text = "0";
            commitCursorLine.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // commitCursorColumnLabel
            // 
            commitCursorColumnLabel.Name = "commitCursorColumnLabel";
            commitCursorColumnLabel.Size = new Size(25, 17);
            commitCursorColumnLabel.Text = "Col";
            // 
            // commitCursorColumn
            // 
            commitCursorColumn.AutoSize = false;
            commitCursorColumn.Name = "commitCursorColumn";
            commitCursorColumn.Size = new Size(40, 17);
            commitCursorColumn.Text = "0";
            commitCursorColumn.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // commitEndPadding
            // 
            commitEndPadding.AutoSize = false;
            commitEndPadding.Name = "commitEndPadding";
            commitEndPadding.Size = new Size(1, 17);
            // 
            // stopTrackingThisFileToolStripMenuItem
            //
            stopTrackingThisFileToolStripMenuItem.Image = Properties.Images.StopTrackingFile;
            stopTrackingThisFileToolStripMenuItem.Name = "stopTrackingThisFileToolStripMenuItem";
            stopTrackingThisFileToolStripMenuItem.Size = new Size(428, 38);
            stopTrackingThisFileToolStripMenuItem.Text = "Stop tracking this file";
            stopTrackingThisFileToolStripMenuItem.Click += stopTrackingThisFileToolStripMenuItem_Click;
            //
            // FormCommit
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = Cancel;
            ClientSize = new Size(918, 644);
            Controls.Add(splitMain);
            Controls.Add(commitStatusStrip);
            KeyPreview = true;
            Margin = new Padding(2);
            MinimumSize = new Size(600, 297);
            Name = "FormCommit";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Commit";
            UnstagedFileContext.ResumeLayout(false);
            StagedFileContext.ResumeLayout(false);
            UnstagedSubmoduleContext.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(gitItemStatusBindingSource)).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitMain)).EndInit();
            splitMain.ResumeLayout(false);
            splitLeft.Panel1.ResumeLayout(false);
            splitLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitLeft)).EndInit();
            splitLeft.ResumeLayout(false);
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            toolbarUnstaged.ResumeLayout(false);
            toolbarUnstaged.PerformLayout();
            toolbarSelectionFilter.ResumeLayout(false);
            toolbarSelectionFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(LoadingStaged)).EndInit();
            toolbarStaged.ResumeLayout(false);
            toolbarStaged.PerformLayout();
            splitRight.Panel1.ResumeLayout(false);
            splitRight.Panel1.PerformLayout();
            splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitRight)).EndInit();
            splitRight.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            AmendPanel.ResumeLayout();
            AmendPanel.PerformLayout();
            flowCommitButtons.ResumeLayout(false);
            flowCommitButtons.PerformLayout();
            toolbarCommit.ResumeLayout(false);
            toolbarCommit.PerformLayout();
            commitStatusStrip.ResumeLayout(false);
            commitStatusStrip.PerformLayout();
            ResumeLayout(false);
            // Restoring splitter distance is deliberatly done here, early in the process.
            // This prevents additional re-drawing of the form as it's done before the initial layout.
            RestoreSplitters();
            PerformLayout();
        }

        #endregion

        private SplitContainer splitMain;
        private Button Ok;
        private SplitContainer splitLeft;
        private SplitContainer splitRight;
        private BindingSource gitItemStatusBindingSource;
        private ContextMenuStrip UnstagedFileContext;
        private ToolStripMenuItem tsmiResetUnstagedChanges;
        private ToolStripMenuItem deleteFileToolStripMenuItem;
        private Button SolveMergeconflicts;
        private FileViewer SelectedDiff;
        private ToolStripMenuItem addFileToGitIgnoreToolStripMenuItem;
        private ToolStripMenuItem addFileToGitInfoExcludeLocallyToolStripMenuItem;
        private ToolStripMenuItem assumeUnchangedToolStripMenuItem;
        private ToolStripMenuItem doNotAssumeUnchangedToolStripMenuItem;
        private ToolStripMenuItem skipWorktreeToolStripMenuItem;
        private ToolStripMenuItem doNotSkipWorktreeToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem openWithToolStripMenuItem;
        private ToolStripMenuItem filenameToClipboardToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem stageToolStripMenuItem;
        private ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private ToolTip fileTooltip;
        private ToolStripMenuItem resetPartOfFileToolStripMenuItem;
        private ToolStripMenuItem editFileToolStripMenuItem;
        private ToolStripMenuItem viewFileHistoryToolStripItem;
        private ToolStripEx toolbarCommit;
        private ToolStripDropDownButton commitMessageToolStripMenuItem;
        private ToolStripDropDownButton toolStripMenuItem3;
        private ToolStripMenuItem closeDialogAfterEachCommitToolStripMenuItem;
        private ToolStripMenuItem closeDialogAfterAllFilesCommittedToolStripMenuItem;
        private ToolStripMenuItem refreshDialogOnFormFocusToolStripMenuItem;
        private GitUI.UserControls.RevisionGrid.LoadingControl Loading;
        private FileStatusList Unstaged;
        private ToolStripEx toolbarUnstaged;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripDropDownButton workingToolStripMenuItem;
        private ToolStripMenuItem showIgnoredFilesToolStripMenuItem;
        private ToolStripMenuItem showAssumeUnchangedFilesToolStripMenuItem;
        private ToolStripMenuItem showSkipWorktreeFilesToolStripMenuItem;
        private ToolStripMenuItem showUntrackedFilesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem editGitIgnoreToolStripMenuItem;
        private ToolStripMenuItem editLocallyIgnoredFilesToolStripMenuItem;
        private ToolStripMenuItem deleteAllUntrackedFilesToolStripMenuItem;
        private ToolStripEx toolbarStaged;
        private FileStatusList Staged;
        private Button Cancel;
        private ToolStripButton toolStageItem;
        private ToolStripButton toolUnstageItem;
        private ToolStripButton toolRefreshItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripButton toolStageAllItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripButton toolUnstageAllItem;
        private ToolStripMenuItem toolAuthorLabelItem;
        private ToolStripTextBox toolAuthor;
        private ToolStripSeparator toolStripSeparator2;
        private PictureBox LoadingStaged;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripContainer toolStripContainer1;
        private ToolStripEx toolbarSelectionFilter;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox selectionFilter;
        private ToolStripMenuItem selectionFilterToolStripMenuItem;
        private ContextMenuStrip UnstagedSubmoduleContext;
        private ToolStripMenuItem updateSubmoduleMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem viewHistoryMenuItem;
        private ToolStripSeparator unstagedSubmoduleStageToolStripSeparator;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem openFolderMenuItem;
        private ToolStripMenuItem copyFolderNameMenuItem;
        private ToolStripMenuItem resetSubmoduleChanges;
        private ToolStripMenuItem commitSubmoduleChanges;
        private ToolStripMenuItem stashSubmoduleChangesToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem stageSubmoduleToolStripMenuItem;
        private ToolStripMenuItem generateListOfChangesInSubmodulesChangesToolStripMenuItem;
        private ToolStripDropDownButton commitTemplatesToolStripMenuItem;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private ToolStripMenuItem signOffToolStripMenuItem;
        private ContextMenuStrip StagedFileContext;
        private ToolStripMenuItem stagedResetChanges;
        private ToolStripMenuItem stagedFileHistoryToolStripMenuItem6;
        private ToolStripSeparator stagedFileHistoryToolStripSeparator;
        private ToolStripSeparator stagedToolStripSeparator14;
        private ToolStripMenuItem stagedOpenToolStripMenuItem7;
        private ToolStripMenuItem stagedOpenWithToolStripMenuItem8;
        private ToolStripMenuItem stagedUnstageToolStripMenuItem;
        private ToolStripMenuItem stagedOpenDifftoolToolStripMenuItem9;
        private ToolStripMenuItem stagedOpenFolderToolStripMenuItem10;
        private ToolStripMenuItem stagedEditFileToolStripMenuItem11;
        private ToolStripSeparator stagedToolStripSeparator18;
        private ToolStripMenuItem stagedCopyPathToolStripMenuItem14;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem interactiveAddToolStripMenuItem;
        private ToolStripSeparator toolStripSeparatorScript;
        private ToolStripMenuItem runScriptToolStripMenuItem;
        private ToolStripSeparator stagedToolStripSeparatorScript;
        private ToolStripMenuItem stagedRunScriptToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private StatusStrip commitStatusStrip;
        private ToolStripStatusLabel commitAuthorStatus;
        private ToolStripStatusLabel commitStagedCountLabel;
        private ToolStripStatusLabel commitStagedCount;
        private ToolStripStatusLabel commitCursorLineLabel;
        private ToolStripStatusLabel commitCursorLine;
        private ToolStripStatusLabel commitCursorColumnLabel;
        private ToolStripStatusLabel commitCursorColumn;
        private ToolStripStatusLabel commitEndPadding;
        private EditNetSpell Message;
        private FlowLayoutPanel AmendPanel;
        private FlowLayoutPanel flowCommitButtons;
        private Button Commit;
        private Button CommitAndPush;
        private Button ResetSoft;
        private Button StashStaged;
        private Button btnResetAllChanges;
        private CheckBox Amend;
        private CheckBox ResetAuthor;
        private CheckBox StageInSuperproject;
        private Button btnResetUnstagedChanges;
        private ToolStripMenuItem noVerifyToolStripMenuItem;
        private ToolStripButton createBranchToolStripButton;
        private ToolStripStatusLabel toolStripStatusBranchIcon;
        private ToolStripStatusLabel branchNameLabel;
        private ToolStripStatusLabel remoteNameLabel;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripTextBox toolStripGpgKeyTextBox;
        private ToolStripComboBox gpgSignCommitToolStripComboBox;
        private ToolStripMenuItem stopTrackingThisFileToolStripMenuItem;
        private Button modifyCommitMessageButton;
        private ToolStripMenuItem ShowOnlyMyMessagesToolStripMenuItem;
    }
}
