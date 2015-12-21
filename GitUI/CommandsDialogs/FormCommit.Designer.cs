using System.Windows.Forms;
using GitUI.Editor;
using GitUI.SpellChecker;

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
            this.components = new System.ComponentModel.Container();
            this.UnstagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPartOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.filenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.viewFileHistoryToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.editFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.addFileTogitignoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assumeUnchangedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doNotAssumeUnchangedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.interactiveAddtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.StageInSuperproject = new System.Windows.Forms.CheckBox();
            this.StagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.stagedResetChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.UnstagedSubmoduleContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.commitSubmoduleChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSubmoduleChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.stashSubmoduleChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.submoduleSummaryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.openSubmoduleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDiffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFolderNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitItemStatusBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.Cancel = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitLeft = new System.Windows.Forms.SplitContainer();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.Unstaged = new GitUI.FileStatusList();
            this.toolbarUnstaged = new GitUI.ToolStripEx();
            this.toolRefreshItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.workingToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.showIgnoredFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAssumeUnchangedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetUnstagedChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetAlltrackedChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editGitIgnoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.selectionFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolbarSelectionFilter = new GitUI.ToolStripEx();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.selectionFilter = new System.Windows.Forms.ToolStripComboBox();
            this.LoadingStaged = new System.Windows.Forms.PictureBox();
            this.Staged = new GitUI.FileStatusList();
            this.toolbarStaged = new GitUI.ToolStripEx();
            this.toolStageAllItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStageItem = new System.Windows.Forms.ToolStripButton();
            this.toolUnstageAllItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolUnstageItem = new System.Windows.Forms.ToolStripButton();
            this.Ok = new System.Windows.Forms.Button();
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.llShowPreview = new System.Windows.Forms.LinkLabel();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.SelectedDiff = new GitUI.Editor.FileViewer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Message = new GitUI.SpellChecker.EditNetSpell();
            this.flowCommitButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.Commit = new System.Windows.Forms.Button();
            this.CommitAndPush = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.ResetUnStaged = new System.Windows.Forms.Button();
            this.Amend = new System.Windows.Forms.CheckBox();
            this.toolbarCommit = new GitUI.ToolStripEx();
            this.commitMessageToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.generateListOfChangesInSubmodulesChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.closeDialogAfterEachCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshDialogOnFormFocusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.signOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolAuthorLabelItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolAuthor = new System.Windows.Forms.ToolStripTextBox();
            this.commitTemplatesToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.commitStatusStrip = new System.Windows.Forms.StatusStrip();
            this.commitAuthorStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.commitCursorLineLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.commitCursorLine = new System.Windows.Forms.ToolStripStatusLabel();
            this.commitCursorColumnLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.commitCursorColumn = new System.Windows.Forms.ToolStripStatusLabel();
            this.commitEndPadding = new System.Windows.Forms.ToolStripStatusLabel();
            this.UnstagedFileContext.SuspendLayout();
            this.StagedFileContext.SuspendLayout();
            this.UnstagedSubmoduleContext.SuspendLayout();
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
#endif
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
#endif
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.toolbarUnstaged.SuspendLayout();
            this.toolbarSelectionFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingStaged)).BeginInit();
            this.toolbarStaged.SuspendLayout();
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
#endif
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowCommitButtons.SuspendLayout();
            this.toolbarCommit.SuspendLayout();
            this.commitStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // UnstagedFileContext
            // 
            this.UnstagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetChanges,
            this.resetPartOfFileToolStripMenuItem,
            this.toolStripSeparator12,
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.openWithDifftoolToolStripMenuItem,
            this.toolStripSeparator9,
            this.filenameToClipboardToolStripMenuItem,
            this.openContainingFolderToolStripMenuItem,
            this.toolStripSeparator8,
            this.viewFileHistoryToolStripItem,
            this.toolStripSeparator7,
            this.editFileToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.toolStripSeparator5,
            this.addFileTogitignoreToolStripMenuItem,
            this.assumeUnchangedToolStripMenuItem,
            this.doNotAssumeUnchangedToolStripMenuItem,
            this.toolStripSeparator4,
            this.interactiveAddtoolStripMenuItem});
            this.UnstagedFileContext.Name = "UnstagedFileContext";
            this.UnstagedFileContext.Size = new System.Drawing.Size(233, 304);
            // 
            // resetChanges
            // 
            this.resetChanges.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.resetChanges.Name = "resetChanges";
            this.resetChanges.Size = new System.Drawing.Size(232, 22);
            this.resetChanges.Text = "Reset file or directory changes";
            this.resetChanges.Click += new System.EventHandler(this.ResetSoftClick);
            // 
            // resetPartOfFileToolStripMenuItem
            // 
            this.resetPartOfFileToolStripMenuItem.Name = "resetPartOfFileToolStripMenuItem";
            this.resetPartOfFileToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.resetPartOfFileToolStripMenuItem.Text = "Reset chunk of file";
            this.resetPartOfFileToolStripMenuItem.Click += new System.EventHandler(this.ResetPartOfFileToolStripMenuItemClick);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(229, 6);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.openWithToolStripMenuItem.Text = "Open with";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.OpenWithToolStripMenuItemClick);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconDiffTool;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(229, 6);
            // 
            // filenameToClipboardToolStripMenuItem
            // 
            this.filenameToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.filenameToClipboardToolStripMenuItem.Name = "filenameToClipboardToolStripMenuItem";
            this.filenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.filenameToClipboardToolStripMenuItem.Text = "Copy full path";
            this.filenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.FilenameToClipboardToolStripMenuItemClick);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open containing folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(229, 6);
            // 
            // viewFileHistoryToolStripItem
            // 
            this.viewFileHistoryToolStripItem.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.viewFileHistoryToolStripItem.Name = "viewFileHistoryToolStripItem";
            this.viewFileHistoryToolStripItem.Size = new System.Drawing.Size(232, 22);
            this.viewFileHistoryToolStripItem.Text = "View file history";
            this.viewFileHistoryToolStripItem.Click += new System.EventHandler(this.ViewFileHistoryMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(229, 6);
            // 
            // editFileToolStripMenuItem
            // 
            this.editFileToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconEditFile;
            this.editFileToolStripMenuItem.Name = "editFileToolStripMenuItem";
            this.editFileToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.editFileToolStripMenuItem.Text = "Edit file";
            this.editFileToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.deleteFileToolStripMenuItem.Text = "Delete file";
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.DeleteFileToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(229, 6);
            // 
            // addFileTogitignoreToolStripMenuItem
            // 
            this.addFileTogitignoreToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconAddToGitIgnore;
            this.addFileTogitignoreToolStripMenuItem.Name = "addFileTogitignoreToolStripMenuItem";
            this.addFileTogitignoreToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.addFileTogitignoreToolStripMenuItem.Text = "Add file to .gitignore";
            this.addFileTogitignoreToolStripMenuItem.Click += new System.EventHandler(this.AddFileTogitignoreToolStripMenuItemClick);
            // 
            // assumeUnchangedToolStripMenuItem
            // 
            this.assumeUnchangedToolStripMenuItem.Name = "assumeUnchangedToolStripMenuItem";
            this.assumeUnchangedToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.assumeUnchangedToolStripMenuItem.Text = "Assume unchanged";
            this.assumeUnchangedToolStripMenuItem.Click += new System.EventHandler(this.AssumeUnchangedToolStripMenuItemClick);
            // 
            // doNotAssumeUnchangedToolStripMenuItem
            // 
            this.doNotAssumeUnchangedToolStripMenuItem.Name = "doNotAssumeUnchangedToolStripMenuItem";
            this.doNotAssumeUnchangedToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.doNotAssumeUnchangedToolStripMenuItem.Text = "Do not assume unchanged";
            this.doNotAssumeUnchangedToolStripMenuItem.Visible = false;
            this.doNotAssumeUnchangedToolStripMenuItem.Click += new System.EventHandler(this.DoNotAssumeUnchangedToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(229, 6);
            // 
            // interactiveAddtoolStripMenuItem
            // 
            this.interactiveAddtoolStripMenuItem.Name = "interactiveAddtoolStripMenuItem";
            this.interactiveAddtoolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.interactiveAddtoolStripMenuItem.Text = "Interactive Add";
            this.interactiveAddtoolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // StageInSuperproject
            // 
            this.StageInSuperproject.AutoSize = true;
            this.StageInSuperproject.Location = new System.Drawing.Point(2, 130);
            this.StageInSuperproject.Margin = new System.Windows.Forms.Padding(2);
            this.StageInSuperproject.Name = "StageInSuperproject";
            this.StageInSuperproject.Size = new System.Drawing.Size(130, 17);
            this.StageInSuperproject.TabIndex = 13;
            this.StageInSuperproject.Text = "Stage in Superproject";
            this.fileTooltip.SetToolTip(this.StageInSuperproject, "Stage current submodule in superproject after commit");
            this.StageInSuperproject.UseVisualStyleBackColor = true;
            this.StageInSuperproject.CheckedChanged += new System.EventHandler(this.StageInSuperproject_CheckedChanged);
            // 
            // StagedFileContext
            // 
            this.StagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stagedResetChanges,
            this.toolStripMenuItem6,
            this.toolStripSeparator14,
            this.toolStripMenuItem7,
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripSeparator18,
            this.toolStripMenuItem14,
            this.toolStripMenuItem10,
            this.toolStripSeparator17,
            this.toolStripMenuItem11});
            this.StagedFileContext.Name = "UnstagedFileContext";
            this.StagedFileContext.Size = new System.Drawing.Size(198, 176);
            // 
            // resetChanges
            // 
            this.stagedResetChanges.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.stagedResetChanges.Name = "resetChanges";
            this.stagedResetChanges.Size = new System.Drawing.Size(232, 22);
            this.stagedResetChanges.Text = "Reset file or directory changes";
            this.stagedResetChanges.Click += new System.EventHandler(this.ResetSoftClick);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem6.Text = "View file history";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.ViewFileHistoryMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(194, 6);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem7.Text = "Open";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem8.Text = "Open with";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.OpenWithToolStripMenuItemClick);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Image = global::GitUI.Properties.Resources.IconDiffTool;
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.toolStripMenuItem9.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem9.Text = "Open with difftool";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.toolStripMenuItem9_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(194, 6);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem14.Text = "Copy full path";
            this.toolStripMenuItem14.Click += new System.EventHandler(this.FilenameToClipboardToolStripMenuItemClick);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem10.Text = "Open containing folder";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.toolStripMenuItem10_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(194, 6);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Image = global::GitUI.Properties.Resources.IconEditFile;
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(197, 22);
            this.toolStripMenuItem11.Text = "Edit file";
            this.toolStripMenuItem11.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // UnstagedSubmoduleContext
            // 
            this.UnstagedSubmoduleContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitSubmoduleChanges,
            this.resetSubmoduleChanges,
            this.stashSubmoduleChangesToolStripMenuItem,
            this.updateSubmoduleMenuItem,
            this.toolStripSeparator13,
            this.submoduleSummaryMenuItem,
            this.viewHistoryMenuItem,
            this.toolStripSeparator15,
            this.openSubmoduleMenuItem,
            this.openFolderMenuItem,
            this.openDiffMenuItem,
            this.toolStripSeparator16,
            this.copyFolderNameMenuItem});
            this.UnstagedSubmoduleContext.Name = "UnstagedFileContext";
            this.UnstagedSubmoduleContext.Size = new System.Drawing.Size(229, 242);
            // 
            // commitSubmoduleChanges
            // 
            this.commitSubmoduleChanges.Name = "commitSubmoduleChanges";
            this.commitSubmoduleChanges.Size = new System.Drawing.Size(228, 22);
            this.commitSubmoduleChanges.Text = "Commit submodule changes";
            this.commitSubmoduleChanges.Click += new System.EventHandler(this.commitSubmoduleChanges_Click);
            // 
            // resetSubmoduleChanges
            // 
            this.resetSubmoduleChanges.Name = "resetSubmoduleChanges";
            this.resetSubmoduleChanges.Size = new System.Drawing.Size(228, 22);
            this.resetSubmoduleChanges.Text = "Reset submodule changes";
            this.resetSubmoduleChanges.Click += new System.EventHandler(this.resetSubmoduleChanges_Click);
            // 
            // stashSubmoduleChangesToolStripMenuItem
            // 
            this.stashSubmoduleChangesToolStripMenuItem.Name = "stashSubmoduleChangesToolStripMenuItem";
            this.stashSubmoduleChangesToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.stashSubmoduleChangesToolStripMenuItem.Text = "Stash submodule changes";
            this.stashSubmoduleChangesToolStripMenuItem.Click += new System.EventHandler(this.stashSubmoduleChangesToolStripMenuItem_Click);
            // 
            // updateSubmoduleMenuItem
            // 
            this.updateSubmoduleMenuItem.Name = "updateSubmoduleMenuItem";
            this.updateSubmoduleMenuItem.Size = new System.Drawing.Size(228, 22);
            this.updateSubmoduleMenuItem.Tag = "1";
            this.updateSubmoduleMenuItem.Text = "Update submodule";
            this.updateSubmoduleMenuItem.Click += new System.EventHandler(this.updateSubmoduleMenuItem_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(225, 6);
            this.toolStripSeparator13.Tag = "1";
            // 
            // submoduleSummaryMenuItem
            // 
            this.submoduleSummaryMenuItem.Name = "submoduleSummaryMenuItem";
            this.submoduleSummaryMenuItem.Size = new System.Drawing.Size(228, 22);
            this.submoduleSummaryMenuItem.Text = "View summary";
            this.submoduleSummaryMenuItem.Click += new System.EventHandler(this.submoduleSummaryMenuItem_Click);
            // 
            // viewHistoryMenuItem
            // 
            this.viewHistoryMenuItem.Name = "viewHistoryMenuItem";
            this.viewHistoryMenuItem.Size = new System.Drawing.Size(228, 22);
            this.viewHistoryMenuItem.Text = "View history";
            this.viewHistoryMenuItem.Click += new System.EventHandler(this.viewHistoryMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(225, 6);
            // 
            // openSubmoduleMenuItem
            // 
            this.openSubmoduleMenuItem.Name = "openSubmoduleMenuItem";
            this.openSubmoduleMenuItem.Size = new System.Drawing.Size(228, 22);
            this.openSubmoduleMenuItem.Tag = "1";
            this.openSubmoduleMenuItem.Text = "Open with Git Extensions";
            this.openSubmoduleMenuItem.Click += new System.EventHandler(this.openSubmoduleMenuItem_Click);
            // 
            // openFolderMenuItem
            // 
            this.openFolderMenuItem.Name = "openFolderMenuItem";
            this.openFolderMenuItem.Size = new System.Drawing.Size(228, 22);
            this.openFolderMenuItem.Text = "Open folder";
            this.openFolderMenuItem.Click += new System.EventHandler(this.openFolderMenuItem_Click);
            // 
            // openDiffMenuItem
            // 
            this.openDiffMenuItem.Name = "openDiffMenuItem";
            this.openDiffMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openDiffMenuItem.Size = new System.Drawing.Size(228, 22);
            this.openDiffMenuItem.Text = "Open with Difftool";
            this.openDiffMenuItem.Click += new System.EventHandler(this.openDiffMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(225, 6);
            // 
            // copyFolderNameMenuItem
            // 
            this.copyFolderNameMenuItem.Name = "copyFolderNameMenuItem";
            this.copyFolderNameMenuItem.Size = new System.Drawing.Size(228, 22);
            this.copyFolderNameMenuItem.Text = "Copy folder name";
            this.copyFolderNameMenuItem.Click += new System.EventHandler(this.copyFolderNameMenuItem_Click);
            // 
            // gitItemStatusBindingSource
            // 
            this.gitItemStatusBindingSource.DataSource = typeof(GitCommands.GitItemStatus);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(134, 167);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(129, 23);
            this.Cancel.TabIndex = 15;
            this.Cancel.TabStop = false;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.SystemColors.Control;
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitLeft);
            this.splitMain.Panel1.Controls.Add(this.Ok);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitRight);
            this.splitMain.Size = new System.Drawing.Size(918, 622);
            this.splitMain.SplitterDistance = 397;
            this.splitMain.TabIndex = 0;
            this.splitMain.TabStop = false;
            // 
            // splitLeft
            // 
            this.splitLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLeft.Location = new System.Drawing.Point(0, 0);
            this.splitLeft.Name = "splitLeft";
            this.splitLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLeft.Panel1
            // 
            this.splitLeft.Panel1.Controls.Add(this.toolStripContainer1);
            // 
            // splitLeft.Panel2
            // 
            this.splitLeft.Panel2.Controls.Add(this.LoadingStaged);
            this.splitLeft.Panel2.Controls.Add(this.Staged);
            this.splitLeft.Panel2.Controls.Add(this.Cancel);
            this.splitLeft.Panel2.Controls.Add(this.toolbarStaged);
            this.splitLeft.Size = new System.Drawing.Size(397, 622);
            this.splitLeft.SplitterDistance = 274;
            this.splitLeft.TabIndex = 3;
            this.splitLeft.TabStop = false;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.Loading);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.Unstaged);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(397, 249);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(397, 274);
            this.toolStripContainer1.TabIndex = 13;
            this.toolStripContainer1.TabStop = false;
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolbarUnstaged);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolbarSelectionFilter);
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(397, 249);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 11;
            this.Loading.TabStop = false;
            // 
            // Unstaged
            // 
            this.Unstaged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Unstaged.Location = new System.Drawing.Point(0, 0);
            this.Unstaged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Unstaged.Name = "Unstaged";
            this.Unstaged.SelectFirstItemOnSetItems = false;
            this.Unstaged.Size = new System.Drawing.Size(397, 249);
            this.Unstaged.TabIndex = 1;
            this.Unstaged.SelectedIndexChanged += new System.EventHandler(this.UnstagedSelectionChanged);
            this.Unstaged.DoubleClick += new System.EventHandler(this.Unstaged_DoubleClick);
            this.Unstaged.Enter += new System.EventHandler(this.Unstaged_Enter);
            // 
            // toolbarUnstaged
            // 
            this.toolbarUnstaged.AutoSize = false;
            this.toolbarUnstaged.BackColor = System.Drawing.SystemColors.Control;
            this.toolbarUnstaged.ClickThrough = true;
            this.toolbarUnstaged.Dock = System.Windows.Forms.DockStyle.None;
            this.toolbarUnstaged.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbarUnstaged.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolRefreshItem,
            this.toolStripSeparator6,
            this.workingToolStripMenuItem,
            this.toolStripProgressBar1});
            this.toolbarUnstaged.Location = new System.Drawing.Point(0, 0);
            this.toolbarUnstaged.Name = "toolbarUnstaged";
            this.toolbarUnstaged.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.toolbarUnstaged.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolbarUnstaged.Size = new System.Drawing.Size(397, 25);
            this.toolbarUnstaged.Stretch = true;
            this.toolbarUnstaged.TabIndex = 12;
            // 
            // toolRefreshItem
            // 
            this.toolRefreshItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRefreshItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.toolRefreshItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRefreshItem.Name = "toolRefreshItem";
            this.toolRefreshItem.Size = new System.Drawing.Size(23, 20);
            this.toolRefreshItem.Text = "Refresh";
            this.toolRefreshItem.Click += new System.EventHandler(this.RescanChangesToolStripMenuItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 23);
            // 
            // workingToolStripMenuItem
            // 
            this.workingToolStripMenuItem.AutoToolTip = false;
            this.workingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showIgnoredFilesToolStripMenuItem,
            this.showAssumeUnchangedFilesToolStripMenuItem,
            this.showUntrackedFilesToolStripMenuItem,
            this.toolStripSeparator3,
            this.deleteSelectedFilesToolStripMenuItem,
            this.resetSelectedFilesToolStripMenuItem,
            this.resetUnstagedChangesToolStripMenuItem,
            this.resetAlltrackedChangesToolStripMenuItem,
            this.toolStripSeparator1,
            this.editGitIgnoreToolStripMenuItem,
            this.deleteAllUntrackedFilesToolStripMenuItem,
            this.toolStripMenuItem2,
            this.selectionFilterToolStripMenuItem});
            this.workingToolStripMenuItem.Image = global::GitUI.Properties.Resources.WorkingDirChanges;
            this.workingToolStripMenuItem.Name = "workingToolStripMenuItem";
            this.workingToolStripMenuItem.Size = new System.Drawing.Size(145, 20);
            this.workingToolStripMenuItem.Text = "Working directory changes";
            // 
            // showIgnoredFilesToolStripMenuItem
            // 
            this.showIgnoredFilesToolStripMenuItem.Name = "showIgnoredFilesToolStripMenuItem";
            this.showIgnoredFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.showIgnoredFilesToolStripMenuItem.Text = "Show ignored files";
            this.showIgnoredFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowIgnoredFilesToolStripMenuItemClick);
            // 
            // showAssumeUnchangedFilesToolStripMenuItem
            // 
            this.showAssumeUnchangedFilesToolStripMenuItem.Name = "showAssumeUnchangedFilesToolStripMenuItem";
            this.showAssumeUnchangedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.showAssumeUnchangedFilesToolStripMenuItem.Text = "Show assume unchanged files";
            this.showAssumeUnchangedFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowAssumeUnchangedFilesToolStripMenuItemClick);
            // 
            // showUntrackedFilesToolStripMenuItem
            // 
            this.showUntrackedFilesToolStripMenuItem.Checked = true;
            this.showUntrackedFilesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showUntrackedFilesToolStripMenuItem.Name = "showUntrackedFilesToolStripMenuItem";
            this.showUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.showUntrackedFilesToolStripMenuItem.Text = "Show untracked files";
            this.showUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowUntrackedFilesToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(211, 6);
            // 
            // deleteSelectedFilesToolStripMenuItem
            // 
            this.deleteSelectedFilesToolStripMenuItem.Name = "deleteSelectedFilesToolStripMenuItem";
            this.deleteSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.deleteSelectedFilesToolStripMenuItem.Text = "Delete selected files";
            this.deleteSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.DeleteSelectedFilesToolStripMenuItemClick);
            // 
            // resetSelectedFilesToolStripMenuItem
            // 
            this.resetSelectedFilesToolStripMenuItem.Name = "resetSelectedFilesToolStripMenuItem";
            this.resetSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.resetSelectedFilesToolStripMenuItem.Text = "Reset selected files";
            this.resetSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.ResetSelectedFilesToolStripMenuItemClick);
            // 
            // resetUnstagedChangesToolStripMenuItem
            // 
            this.resetUnstagedChangesToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.resetUnstagedChangesToolStripMenuItem.Name = "resetUnstagedChangesToolStripMenuItem";
            this.resetUnstagedChangesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.resetUnstagedChangesToolStripMenuItem.Text = "Reset unstaged changes";
            this.resetUnstagedChangesToolStripMenuItem.Click += new System.EventHandler(this.resetUnstagedChangesToolStripMenuItem_Click);
            // 
            // resetAlltrackedChangesToolStripMenuItem
            // 
            this.resetAlltrackedChangesToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.resetAlltrackedChangesToolStripMenuItem.Name = "resetAlltrackedChangesToolStripMenuItem";
            this.resetAlltrackedChangesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.resetAlltrackedChangesToolStripMenuItem.Text = "Reset all (tracked) changes";
            this.resetAlltrackedChangesToolStripMenuItem.Click += new System.EventHandler(this.ResetAlltrackedChangesToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(211, 6);
            // 
            // editGitIgnoreToolStripMenuItem
            // 
            this.editGitIgnoreToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconEditGitIgnore;
            this.editGitIgnoreToolStripMenuItem.Name = "editGitIgnoreToolStripMenuItem";
            this.editGitIgnoreToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editGitIgnoreToolStripMenuItem.Text = "Edit ignored files";
            this.editGitIgnoreToolStripMenuItem.Click += new System.EventHandler(this.EditGitIgnoreToolStripMenuItemClick);
            // 
            // deleteAllUntrackedFilesToolStripMenuItem
            // 
            this.deleteAllUntrackedFilesToolStripMenuItem.Name = "deleteAllUntrackedFilesToolStripMenuItem";
            this.deleteAllUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.deleteAllUntrackedFilesToolStripMenuItem.Text = "Delete all untracked files";
            this.deleteAllUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.DeleteAllUntrackedFilesToolStripMenuItemClick);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(211, 6);
            // 
            // selectionFilterToolStripMenuItem
            // 
            this.selectionFilterToolStripMenuItem.CheckOnClick = true;
            this.selectionFilterToolStripMenuItem.Name = "selectionFilterToolStripMenuItem";
            this.selectionFilterToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.selectionFilterToolStripMenuItem.Text = "Selection filter";
            this.selectionFilterToolStripMenuItem.CheckedChanged += new System.EventHandler(this.ToogleShowSelectionFilter);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripProgressBar1.Size = new System.Drawing.Size(150, 27);
            this.toolStripProgressBar1.Visible = false;
            // 
            // toolbarSelectionFilter
            // 
            this.toolbarSelectionFilter.ClickThrough = true;
            this.toolbarSelectionFilter.Dock = System.Windows.Forms.DockStyle.None;
            this.toolbarSelectionFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.selectionFilter});
            this.toolbarSelectionFilter.Location = new System.Drawing.Point(3, 0);
            this.toolbarSelectionFilter.Name = "toolbarSelectionFilter";
            this.toolbarSelectionFilter.Size = new System.Drawing.Size(219, 25);
            this.toolbarSelectionFilter.TabIndex = 13;
            this.toolbarSelectionFilter.Visible = false;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(84, 22);
            this.toolStripLabel1.Text = "Selection Filter";
            // 
            // selectionFilter
            // 
            this.selectionFilter.Name = "selectionFilter";
            this.selectionFilter.Size = new System.Drawing.Size(121, 25);
            this.selectionFilter.SelectedIndexChanged += new System.EventHandler(this.FilterIndexChanged);
            this.selectionFilter.TextChanged += new System.EventHandler(this.FilterChanged);
            // 
            // LoadingStaged
            // 
            this.LoadingStaged.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.LoadingStaged.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.LoadingStaged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LoadingStaged.Location = new System.Drawing.Point(0, 28);
            this.LoadingStaged.Name = "LoadingStaged";
            this.LoadingStaged.Size = new System.Drawing.Size(397, 316);
            this.LoadingStaged.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.LoadingStaged.TabIndex = 17;
            this.LoadingStaged.TabStop = false;
            // 
            // Staged
            // 
            this.Staged.ContextMenuStrip = this.StagedFileContext;
            this.Staged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Staged.Location = new System.Drawing.Point(0, 28);
            this.Staged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Staged.Name = "Staged";
            this.Staged.SelectFirstItemOnSetItems = false;
            this.Staged.Size = new System.Drawing.Size(397, 316);
            this.Staged.TabIndex = 16;
            this.Staged.TabStop = false;
            this.Staged.SelectedIndexChanged += new System.EventHandler(this.StagedSelectionChanged);
            this.Staged.DoubleClick += new System.EventHandler(this.Staged_DoubleClick);
            this.Staged.Enter += new System.EventHandler(this.Staged_Enter);
            // 
            // toolbarStaged
            // 
            this.toolbarStaged.AutoSize = false;
            this.toolbarStaged.BackColor = System.Drawing.SystemColors.Control;
            this.toolbarStaged.ClickThrough = true;
            this.toolbarStaged.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbarStaged.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStageAllItem,
            this.toolStripSeparator10,
            this.toolStageItem,
            this.toolUnstageAllItem,
            this.toolStripSeparator11,
            this.toolUnstageItem});
            this.toolbarStaged.Location = new System.Drawing.Point(0, 0);
            this.toolbarStaged.Name = "toolbarStaged";
            this.toolbarStaged.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.toolbarStaged.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolbarStaged.Size = new System.Drawing.Size(397, 28);
            this.toolbarStaged.TabIndex = 13;
            // 
            // toolStageAllItem
            // 
            this.toolStageAllItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStageAllItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStageAllItem.Image = global::GitUI.Properties.Resources.IconStageAll;
            this.toolStageAllItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStageAllItem.Name = "toolStageAllItem";
            this.toolStageAllItem.Size = new System.Drawing.Size(23, 23);
            this.toolStageAllItem.Text = "Stage All";
            this.toolStageAllItem.Click += new System.EventHandler(this.StageAllToolStripMenuItemClick);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 26);
            // 
            // toolStageItem
            // 
            this.toolStageItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStageItem.AutoToolTip = false;
            this.toolStageItem.Image = global::GitUI.Properties.Resources.IconStage;
            this.toolStageItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStageItem.Name = "toolStageItem";
            this.toolStageItem.Size = new System.Drawing.Size(56, 23);
            this.toolStageItem.Text = "&Stage";
            this.toolStageItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStageItem.Click += new System.EventHandler(this.StageClick);
            // 
            // toolUnstageAllItem
            // 
            this.toolUnstageAllItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolUnstageAllItem.Image = global::GitUI.Properties.Resources.IconUnstageAll;
            this.toolUnstageAllItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolUnstageAllItem.Name = "toolUnstageAllItem";
            this.toolUnstageAllItem.Size = new System.Drawing.Size(23, 23);
            this.toolUnstageAllItem.Text = "Unstage All";
            this.toolUnstageAllItem.Click += new System.EventHandler(this.UnstageAllToolStripMenuItemClick);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 26);
            // 
            // toolUnstageItem
            // 
            this.toolUnstageItem.AutoToolTip = false;
            this.toolUnstageItem.Image = global::GitUI.Properties.Resources.IconUnstage;
            this.toolUnstageItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolUnstageItem.Name = "toolUnstageItem";
            this.toolUnstageItem.Size = new System.Drawing.Size(70, 23);
            this.toolUnstageItem.Text = "&Unstage";
            this.toolUnstageItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolUnstageItem.Click += new System.EventHandler(this.UnstageFilesClick);
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(334, 10);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "Commit";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // splitRight
            // 
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitRight.Location = new System.Drawing.Point(0, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitRight.Panel1
            // 
            this.splitRight.Panel1.Controls.Add(this.llShowPreview);
            this.splitRight.Panel1.Controls.Add(this.SolveMergeconflicts);
            this.splitRight.Panel1.Controls.Add(this.SelectedDiff);
            // 
            // splitRight.Panel2
            // 
            this.splitRight.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitRight.Panel2MinSize = 145;
            this.splitRight.Size = new System.Drawing.Size(517, 622);
            this.splitRight.SplitterDistance = 426;
            this.splitRight.TabIndex = 0;
            this.splitRight.TabStop = false;
            // 
            // llShowPreview
            // 
            this.llShowPreview.AutoSize = true;
            this.llShowPreview.Location = new System.Drawing.Point(43, 23);
            this.llShowPreview.Name = "llShowPreview";
            this.llShowPreview.Size = new System.Drawing.Size(236, 15);
            this.llShowPreview.TabIndex = 9;
            this.llShowPreview.TabStop = true;
            this.llShowPreview.Text = "This file is over 5 MB. Click to show preview";
            this.llShowPreview.Visible = false;
            this.llShowPreview.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llShowPreview_LinkClicked);
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.SeaShell;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Image = global::GitUI.Properties.Resources.Icon_50;
            this.SolveMergeconflicts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(14, 12);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(188, 42);
            this.SolveMergeconflicts.TabIndex = 0;
            this.SolveMergeconflicts.Text = "There are unresolved mergeconflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeConflictsClick);
            // 
            // SelectedDiff
            // 
            this.SelectedDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedDiff.Location = new System.Drawing.Point(0, 0);
            this.SelectedDiff.Margin = new System.Windows.Forms.Padding(2, 3, 3, 3);
            this.SelectedDiff.Name = "SelectedDiff";
            this.SelectedDiff.Size = new System.Drawing.Size(517, 426);
            this.SelectedDiff.TabIndex = 0;
            this.SelectedDiff.TabStop = false;
            this.SelectedDiff.ContextMenuOpening += new System.ComponentModel.CancelEventHandler(this.SelectedDiff_ContextMenuOpening);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowCommitButtons, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.toolbarCommit, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(517, 192);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Message);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(178, 31);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(336, 158);
            this.panel1.TabIndex = 7;
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Location = new System.Drawing.Point(0, 0);
            this.Message.Margin = new System.Windows.Forms.Padding(0);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(336, 158);
            this.Message.TabIndex = 13;
            this.Message.TextBoxFont = new System.Drawing.Font("Segoe UI", 9F);
            this.Message.SelectionChanged += new System.EventHandler(this.Message_SelectionChanged);
            this.Message.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Message_KeyDown);
            this.Message.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Message_KeyUp);
            // 
            // flowCommitButtons
            // 
            this.flowCommitButtons.AutoSize = true;
            this.flowCommitButtons.Controls.Add(this.Commit);
            this.flowCommitButtons.Controls.Add(this.CommitAndPush);
            this.flowCommitButtons.Controls.Add(this.Reset);
            this.flowCommitButtons.Controls.Add(this.ResetUnStaged);
            this.flowCommitButtons.Controls.Add(this.StageInSuperproject);
            this.flowCommitButtons.Controls.Add(this.Amend);
            this.flowCommitButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowCommitButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowCommitButtons.Location = new System.Drawing.Point(0, 0);
            this.flowCommitButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flowCommitButtons.Name = "flowCommitButtons";
            this.tableLayoutPanel1.SetRowSpan(this.flowCommitButtons, 2);
            this.flowCommitButtons.Size = new System.Drawing.Size(175, 192);
            this.flowCommitButtons.TabIndex = 1;
            this.flowCommitButtons.WrapContents = false;
            // 
            // Commit
            // 
            this.Commit.Image = global::GitUI.Properties.Resources.IconClean;
            this.Commit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Commit.Location = new System.Drawing.Point(1, 3);
            this.Commit.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(171, 26);
            this.Commit.TabIndex = 3;
            this.Commit.TabStop = false;
            this.Commit.Text = "&Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.CommitClick);
            // 
            // CommitAndPush
            // 
            this.CommitAndPush.Image = global::GitUI.Properties.Resources.ArrowUp;
            this.CommitAndPush.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CommitAndPush.Location = new System.Drawing.Point(1, 35);
            this.CommitAndPush.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.CommitAndPush.Name = "CommitAndPush";
            this.CommitAndPush.Size = new System.Drawing.Size(171, 26);
            this.CommitAndPush.TabIndex = 9;
            this.CommitAndPush.TabStop = false;
            this.CommitAndPush.Text = "C&ommit && push";
            this.CommitAndPush.UseVisualStyleBackColor = true;
            this.CommitAndPush.Click += new System.EventHandler(this.CommitAndPush_Click);
            // 
            // Reset
            // 
            this.Reset.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.Reset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Reset.Location = new System.Drawing.Point(1, 67);
            this.Reset.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(171, 26);
            this.Reset.TabIndex = 11;
            this.Reset.TabStop = false;
            this.Reset.Text = "Reset all changes";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.ResetClick);
            // 
            // ResetUnStaged
            // 
            this.ResetUnStaged.Image = global::GitUI.Properties.Resources.IconResetWorkingDirChanges;
            this.ResetUnStaged.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ResetUnStaged.Location = new System.Drawing.Point(1, 99);
            this.ResetUnStaged.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.ResetUnStaged.Name = "ResetUnStaged";
            this.ResetUnStaged.Size = new System.Drawing.Size(171, 26);
            this.ResetUnStaged.TabIndex = 14;
            this.ResetUnStaged.Text = "Reset unstaged changes";
            this.ResetUnStaged.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ResetUnStaged.UseVisualStyleBackColor = true;
            this.ResetUnStaged.Click += new System.EventHandler(this.ResetUnStagedClick);
            // 
            // Amend
            // 
            this.Amend.AutoSize = true;
            this.Amend.Location = new System.Drawing.Point(3, 152);
            this.Amend.Name = "Amend";
            this.Amend.Size = new System.Drawing.Size(97, 17);
            this.Amend.TabIndex = 12;
            this.Amend.Text = "&Amend Commit";
            this.Amend.UseVisualStyleBackColor = true;
            this.Amend.CheckedChanged += new System.EventHandler(this.Amend_CheckedChanged);
            // 
            // toolbarCommit
            // 
            this.toolbarCommit.AutoSize = false;
            this.toolbarCommit.BackColor = System.Drawing.SystemColors.Control;
            this.toolbarCommit.ClickThrough = true;
            this.toolbarCommit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbarCommit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitMessageToolStripMenuItem,
            this.toolStripMenuItem3,
            this.commitTemplatesToolStripMenuItem});
            this.toolbarCommit.Location = new System.Drawing.Point(175, 0);
            this.toolbarCommit.Name = "toolbarCommit";
            this.toolbarCommit.Padding = new System.Windows.Forms.Padding(1, 1, 2, 1);
            this.toolbarCommit.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolbarCommit.Size = new System.Drawing.Size(342, 28);
            this.toolbarCommit.Stretch = true;
            this.toolbarCommit.TabIndex = 5;
            // 
            // commitMessageToolStripMenuItem
            // 
            this.commitMessageToolStripMenuItem.AutoToolTip = false;
            this.commitMessageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.generateListOfChangesInSubmodulesChangesToolStripMenuItem});
            this.commitMessageToolStripMenuItem.Image = global::GitUI.Properties.Resources.WorkingDirChanges;
            this.commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            this.commitMessageToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.commitMessageToolStripMenuItem.Size = new System.Drawing.Size(129, 23);
            this.commitMessageToolStripMenuItem.Text = "Commit &message";
            this.commitMessageToolStripMenuItem.DropDownOpening += new System.EventHandler(this.CommitMessageToolStripMenuItemDropDownOpening);
            this.commitMessageToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.CommitMessageToolStripMenuItemDropDownItemClicked);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(287, 6);
            // 
            // generateListOfChangesInSubmodulesChangesToolStripMenuItem
            // 
            this.generateListOfChangesInSubmodulesChangesToolStripMenuItem.Name = "generateListOfChangesInSubmodulesChangesToolStripMenuItem";
            this.generateListOfChangesInSubmodulesChangesToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
            this.generateListOfChangesInSubmodulesChangesToolStripMenuItem.Text = "Generate a list of changes in submodules";
            this.generateListOfChangesInSubmodulesChangesToolStripMenuItem.Click += new System.EventHandler(this.generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem3.AutoToolTip = false;
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeDialogAfterEachCommitToolStripMenuItem,
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem,
            this.refreshDialogOnFormFocusToolStripMenuItem,
            this.toolStripSeparator2,
            this.signOffToolStripMenuItem,
            this.toolAuthorLabelItem,
            this.toolAuthor});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStripMenuItem3.Size = new System.Drawing.Size(62, 23);
            this.toolStripMenuItem3.Text = "Options";
            // 
            // closeDialogAfterEachCommitToolStripMenuItem
            // 
            this.closeDialogAfterEachCommitToolStripMenuItem.Name = "closeDialogAfterEachCommitToolStripMenuItem";
            this.closeDialogAfterEachCommitToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.closeDialogAfterEachCommitToolStripMenuItem.Text = "Close dialog after each commit";
            this.closeDialogAfterEachCommitToolStripMenuItem.Click += new System.EventHandler(this.closeDialogAfterEachCommitToolStripMenuItem_Click);
            // 
            // closeDialogAfterAllFilesCommittedToolStripMenuItem
            // 
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Name = "closeDialogAfterAllFilesCommittedToolStripMenuItem";
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Text = "Close dialog when all changes are committed";
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Click += new System.EventHandler(this.closeDialogAfterAllFilesCommittedToolStripMenuItem_Click);
            // 
            // refreshDialogOnFormFocusToolStripMenuItem
            // 
            this.refreshDialogOnFormFocusToolStripMenuItem.Name = "refreshDialogOnFormFocusToolStripMenuItem";
            this.refreshDialogOnFormFocusToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.refreshDialogOnFormFocusToolStripMenuItem.Text = "Refresh dialog on form focus";
            this.refreshDialogOnFormFocusToolStripMenuItem.Click += new System.EventHandler(this.refreshDialogOnFormFocusToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(311, 6);
            // 
            // signOffToolStripMenuItem
            // 
            this.signOffToolStripMenuItem.Name = "signOffToolStripMenuItem";
            this.signOffToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.signOffToolStripMenuItem.Text = "Sign-off commit";
            this.signOffToolStripMenuItem.Click += new System.EventHandler(this.signOffToolStripMenuItem_Click);
            // 
            // toolAuthorLabelItem
            // 
            this.toolAuthorLabelItem.Enabled = false;
            this.toolAuthorLabelItem.Name = "toolAuthorLabelItem";
            this.toolAuthorLabelItem.Size = new System.Drawing.Size(314, 22);
            this.toolAuthorLabelItem.Text = "Author: (Format: \"name <mail>\")";
            this.toolAuthorLabelItem.Click += new System.EventHandler(this.toolAuthorLabelItem_Click);
            // 
            // toolAuthor
            // 
            this.toolAuthor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolAuthor.Name = "toolAuthor";
            this.toolAuthor.Size = new System.Drawing.Size(230, 23);
            this.toolAuthor.Leave += new System.EventHandler(this.toolAuthor_Leave);
            this.toolAuthor.TextChanged += new System.EventHandler(this.toolAuthor_TextChanged);
            // 
            // commitTemplatesToolStripMenuItem
            // 
            this.commitTemplatesToolStripMenuItem.Image = global::GitUI.Properties.Resources.CommitTemplates;
            this.commitTemplatesToolStripMenuItem.Name = "commitTemplatesToolStripMenuItem";
            this.commitTemplatesToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.commitTemplatesToolStripMenuItem.Size = new System.Drawing.Size(135, 23);
            this.commitTemplatesToolStripMenuItem.Text = "Commit &templates";
            this.commitTemplatesToolStripMenuItem.DropDownOpening += new System.EventHandler(this.commitTemplatesToolStripMenuItem_DropDownOpening);
            // 
            // commitStatusStrip
            // 
            this.commitStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitAuthorStatus,
            this.commitCursorLineLabel,
            this.commitCursorLine,
            this.commitCursorColumnLabel,
            this.commitCursorColumn,
            this.commitEndPadding});
            this.commitStatusStrip.Location = new System.Drawing.Point(0, 622);
            this.commitStatusStrip.Name = "commitStatusStrip";
            this.commitStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 11, 0);
            this.commitStatusStrip.ShowItemToolTips = true;
            this.commitStatusStrip.Size = new System.Drawing.Size(918, 22);
            this.commitStatusStrip.TabIndex = 13;
            // 
            // commitAuthorStatus
            // 
            this.commitAuthorStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.commitAuthorStatus.IsLink = true;
            this.commitAuthorStatus.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.commitAuthorStatus.Name = "commitAuthorStatus";
            this.commitAuthorStatus.Size = new System.Drawing.Size(749, 17);
            this.commitAuthorStatus.Spring = true;
            this.commitAuthorStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.commitAuthorStatus.ToolTipText = "Click to change author information.";
            this.commitAuthorStatus.Click += new System.EventHandler(this.commitCommitter_Click);
            // 
            // commitCursorLineLabel
            // 
            this.commitCursorLineLabel.Name = "commitCursorLineLabel";
            this.commitCursorLineLabel.Size = new System.Drawing.Size(20, 17);
            this.commitCursorLineLabel.Text = "Ln";
            // 
            // commitCursorLine
            // 
            this.commitCursorLine.AutoSize = false;
            this.commitCursorLine.Name = "commitCursorLine";
            this.commitCursorLine.Size = new System.Drawing.Size(40, 17);
            this.commitCursorLine.Text = "0";
            this.commitCursorLine.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commitCursorColumnLabel
            // 
            this.commitCursorColumnLabel.Name = "commitCursorColumnLabel";
            this.commitCursorColumnLabel.Size = new System.Drawing.Size(25, 17);
            this.commitCursorColumnLabel.Text = "Col";
            // 
            // commitCursorColumn
            // 
            this.commitCursorColumn.AutoSize = false;
            this.commitCursorColumn.Name = "commitCursorColumn";
            this.commitCursorColumn.Size = new System.Drawing.Size(40, 17);
            this.commitCursorColumn.Text = "0";
            this.commitCursorColumn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commitEndPadding
            // 
            this.commitEndPadding.AutoSize = false;
            this.commitEndPadding.Name = "commitEndPadding";
            this.commitEndPadding.Size = new System.Drawing.Size(1, 17);
            // 
            // FormCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(918, 644);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.commitStatusStrip);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(600, 297);
            this.Name = "FormCommit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commit";
            this.Activated += new System.EventHandler(this.FormCommitActivated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCommitFormClosing);
            this.Load += new System.EventHandler(this.FormCommit_Load);
            this.Shown += new System.EventHandler(this.FormCommitShown);
            this.UnstagedFileContext.ResumeLayout(false);
            this.StagedFileContext.ResumeLayout(false);
            this.UnstagedSubmoduleContext.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
#endif
            this.splitMain.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
#endif
            this.splitLeft.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.toolbarUnstaged.ResumeLayout(false);
            this.toolbarUnstaged.PerformLayout();
            this.toolbarSelectionFilter.ResumeLayout(false);
            this.toolbarSelectionFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingStaged)).EndInit();
            this.toolbarStaged.ResumeLayout(false);
            this.toolbarStaged.PerformLayout();
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel1.PerformLayout();
            this.splitRight.Panel2.ResumeLayout(false);
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
#endif
            this.splitRight.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.flowCommitButtons.ResumeLayout(false);
            this.flowCommitButtons.PerformLayout();
            this.toolbarCommit.ResumeLayout(false);
            this.toolbarCommit.PerformLayout();
            this.commitStatusStrip.ResumeLayout(false);
            this.commitStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitLeft;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.BindingSource gitItemStatusBindingSource;
        private System.Windows.Forms.ContextMenuStrip UnstagedFileContext;
        private System.Windows.Forms.ToolStripMenuItem resetChanges;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private FileViewer SelectedDiff;
        private System.Windows.Forms.ToolStripMenuItem addFileTogitignoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assumeUnchangedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem doNotAssumeUnchangedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filenameToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolTip fileTooltip;
        private System.Windows.Forms.ToolStripMenuItem resetPartOfFileToolStripMenuItem;
        private ToolStripMenuItem editFileToolStripMenuItem;
        private ToolStripMenuItem viewFileHistoryToolStripItem;
        private ToolStripEx toolbarCommit;
        private ToolStripDropDownButton commitMessageToolStripMenuItem;
        private ToolStripDropDownButton toolStripMenuItem3;
        private ToolStripMenuItem closeDialogAfterEachCommitToolStripMenuItem;
        private ToolStripMenuItem closeDialogAfterAllFilesCommittedToolStripMenuItem;
        private ToolStripMenuItem refreshDialogOnFormFocusToolStripMenuItem;
        private PictureBox Loading;
        private FileStatusList Unstaged;
        private ToolStripEx toolbarUnstaged;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripDropDownButton workingToolStripMenuItem;
        private ToolStripMenuItem showIgnoredFilesToolStripMenuItem;
        private ToolStripMenuItem showAssumeUnchangedFilesToolStripMenuItem;
        private ToolStripMenuItem showUntrackedFilesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem deleteSelectedFilesToolStripMenuItem;
        private ToolStripMenuItem resetSelectedFilesToolStripMenuItem;
        private ToolStripMenuItem resetAlltrackedChangesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem editGitIgnoreToolStripMenuItem;
        private ToolStripMenuItem deleteAllUntrackedFilesToolStripMenuItem;
        private ToolStripEx toolbarStaged;
        private FileStatusList Staged;
        private Button Cancel;
        private ToolStripButton toolStageItem;
        private ToolStripButton toolUnstageItem;
        private ToolStripButton toolRefreshItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator7;
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
        private LinkLabel llShowPreview;
        private ContextMenuStrip UnstagedSubmoduleContext;
        private ToolStripMenuItem openSubmoduleMenuItem;
        private ToolStripMenuItem updateSubmoduleMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem viewHistoryMenuItem;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem openFolderMenuItem;
        private ToolStripMenuItem openDiffMenuItem;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem copyFolderNameMenuItem;
        private ToolStripMenuItem submoduleSummaryMenuItem;
        private ToolStripMenuItem resetSubmoduleChanges;
        private ToolStripMenuItem commitSubmoduleChanges;
        private ToolStripMenuItem stashSubmoduleChangesToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem generateListOfChangesInSubmodulesChangesToolStripMenuItem;
        private ToolStripDropDownButton commitTemplatesToolStripMenuItem;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private ToolStripMenuItem signOffToolStripMenuItem;
        private ContextMenuStrip StagedFileContext;
        private ToolStripMenuItem stagedResetChanges;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripMenuItem toolStripMenuItem7;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem toolStripMenuItem9;
        private ToolStripMenuItem toolStripMenuItem10;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripMenuItem toolStripMenuItem11;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem toolStripMenuItem14;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem interactiveAddtoolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private StatusStrip commitStatusStrip;
        private ToolStripStatusLabel commitAuthorStatus;
        private ToolStripStatusLabel commitCursorLineLabel;
        private ToolStripStatusLabel commitCursorLine;
        private ToolStripStatusLabel commitCursorColumnLabel;
        private ToolStripStatusLabel commitCursorColumn;
        private ToolStripStatusLabel commitEndPadding;
        private Panel panel1;
        private EditNetSpell Message;
        private FlowLayoutPanel flowCommitButtons;
        private Button Commit;
        private Button CommitAndPush;
        private Button Reset;
        private CheckBox Amend;
        private CheckBox StageInSuperproject;
        private Button ResetUnStaged;
        private ToolStripMenuItem resetUnstagedChangesToolStripMenuItem;
    }
}
