using System.Windows.Forms;
using GitUI.Editor;
using GitUI.SpellChecker;

namespace GitUI
{
    partial class FormCommit
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
            if (disposing)
            {
                _gitGetUnstagedCommand.Dispose();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCommit));
            this.UnstagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ResetChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPartOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.ViewFileHistoryToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.editFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.addFileTogitignoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.filenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.gitItemStatusBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitLeft = new System.Windows.Forms.SplitContainer();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.Unstaged = new GitUI.FileStatusList();
            this.toolbarUnstaged = new System.Windows.Forms.ToolStrip();
            this.toolRefreshItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.workingToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.showIgnoredFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetAlltrackedChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editGitIgnoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.Staged = new GitUI.FileStatusList();
            this.Cancel = new System.Windows.Forms.Button();
            this.toolbarStaged = new System.Windows.Forms.ToolStrip();
            this.toolStageAllItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStageItem = new System.Windows.Forms.ToolStripButton();
            this.toolUnstageAllItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolUnstageItem = new System.Windows.Forms.ToolStripButton();
            this.Ok = new System.Windows.Forms.Button();
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.SelectedDiff = new GitUI.Editor.FileViewer();
            this.Message = new GitUI.SpellChecker.EditNetSpell();
            this.toolbarCommit = new System.Windows.Forms.ToolStrip();
            this.commitMessageToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.closeDialogAfterEachCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshDialogOnFormFocusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolAuthorLabelItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolAuthor = new System.Windows.Forms.ToolStripTextBox();
            this.flowCommitButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.Commit = new System.Windows.Forms.Button();
            this.CommitAndPush = new System.Windows.Forms.Button();
            this.Amend = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.UnstagedFileContext.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.toolbarUnstaged.SuspendLayout();
            this.toolbarStaged.SuspendLayout();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.toolbarCommit.SuspendLayout();
            this.flowCommitButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // UnstagedFileContext
            // 
            this.UnstagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ResetChanges,
            this.resetPartOfFileToolStripMenuItem,
            this.toolStripSeparator8,
            this.ViewFileHistoryToolStripItem,
            this.toolStripSeparator7,
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.openWithDifftoolToolStripMenuItem,
            this.toolStripSeparator4,
            this.editFileToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.toolStripSeparator9,
            this.addFileTogitignoreToolStripMenuItem,
            this.toolStripSeparator5,
            this.filenameToClipboardToolStripMenuItem});
            this.UnstagedFileContext.Name = "UnstagedFileContext";
            this.UnstagedFileContext.Size = new System.Drawing.Size(194, 254);
            // 
            // ResetChanges
            // 
            this.ResetChanges.Name = "ResetChanges";
            this.ResetChanges.Size = new System.Drawing.Size(193, 22);
            this.ResetChanges.Text = "Reset file changes";
            this.ResetChanges.Click += new System.EventHandler(this.ResetSoftClick);
            // 
            // resetPartOfFileToolStripMenuItem
            // 
            this.resetPartOfFileToolStripMenuItem.Name = "resetPartOfFileToolStripMenuItem";
            this.resetPartOfFileToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.resetPartOfFileToolStripMenuItem.Text = "Reset chunk of file";
            this.resetPartOfFileToolStripMenuItem.Click += new System.EventHandler(this.ResetPartOfFileToolStripMenuItemClick);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(190, 6);
            // 
            // ViewFileHistoryToolStripItem
            // 
            this.ViewFileHistoryToolStripItem.Name = "ViewFileHistoryToolStripItem";
            this.ViewFileHistoryToolStripItem.Size = new System.Drawing.Size(193, 22);
            this.ViewFileHistoryToolStripItem.Text = "View file history";
            this.ViewFileHistoryToolStripItem.Click += new System.EventHandler(this.ViewFileHistoryMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(190, 6);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.openWithToolStripMenuItem.Text = "Open With";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.OpenWithToolStripMenuItemClick);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open With Difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(190, 6);
            // 
            // editFileToolStripMenuItem
            // 
            this.editFileToolStripMenuItem.Name = "editFileToolStripMenuItem";
            this.editFileToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.editFileToolStripMenuItem.Text = "Edit file";
            this.editFileToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.deleteFileToolStripMenuItem.Text = "Delete file";
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.DeleteFileToolStripMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(190, 6);
            // 
            // addFileTogitignoreToolStripMenuItem
            // 
            this.addFileTogitignoreToolStripMenuItem.Name = "addFileTogitignoreToolStripMenuItem";
            this.addFileTogitignoreToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.addFileTogitignoreToolStripMenuItem.Text = "Add file to .gitignore";
            this.addFileTogitignoreToolStripMenuItem.Click += new System.EventHandler(this.AddFileTogitignoreToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(190, 6);
            // 
            // filenameToClipboardToolStripMenuItem
            // 
            this.filenameToClipboardToolStripMenuItem.Name = "filenameToClipboardToolStripMenuItem";
            this.filenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.filenameToClipboardToolStripMenuItem.Text = "Copy Filename";
            this.filenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.FilenameToClipboardToolStripMenuItemClick);
            // 
            // fileTooltip
            // 
            this.fileTooltip.AutomaticDelay = 0;
            this.fileTooltip.AutoPopDelay = 500;
            this.fileTooltip.InitialDelay = 0;
            this.fileTooltip.ReshowDelay = 0;
            // 
            // gitItemStatusBindingSource
            // 
            this.gitItemStatusBindingSource.DataSource = typeof(GitCommands.GitItemStatus);
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
            this.splitMain.Size = new System.Drawing.Size(918, 644);
            this.splitMain.SplitterDistance = 397;
            this.splitMain.TabIndex = 0;
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
            this.splitLeft.Panel1.Controls.Add(this.Loading);
            this.splitLeft.Panel1.Controls.Add(this.Unstaged);
            this.splitLeft.Panel1.Controls.Add(this.toolbarUnstaged);
            // 
            // splitLeft.Panel2
            // 
            this.splitLeft.Panel2.Controls.Add(this.Staged);
            this.splitLeft.Panel2.Controls.Add(this.Cancel);
            this.splitLeft.Panel2.Controls.Add(this.toolbarStaged);
            this.splitLeft.Size = new System.Drawing.Size(397, 644);
            this.splitLeft.SplitterDistance = 284;
            this.splitLeft.TabIndex = 3;
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
            this.Loading.Location = new System.Drawing.Point(0, 25);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(397, 259);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 11;
            this.Loading.TabStop = false;
            // 
            // Unstaged
            // 
            this.Unstaged.ContextMenuStrip = this.UnstagedFileContext;
            this.Unstaged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Unstaged.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.Unstaged.GitItemStatuses = null;
            this.Unstaged.Location = new System.Drawing.Point(0, 25);
            this.Unstaged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Unstaged.Name = "Unstaged";
            this.Unstaged.Revision = null;
            this.Unstaged.SelectedItem = null;
            this.Unstaged.Size = new System.Drawing.Size(397, 259);
            this.Unstaged.TabIndex = 10;
            // 
            // toolbarUnstaged
            // 
            this.toolbarUnstaged.BackColor = System.Drawing.SystemColors.Control;
            this.toolbarUnstaged.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbarUnstaged.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolRefreshItem,
            this.toolStripSeparator6,
            this.workingToolStripMenuItem,
            this.toolStripProgressBar1});
            this.toolbarUnstaged.Location = new System.Drawing.Point(0, 0);
            this.toolbarUnstaged.Name = "toolbarUnstaged";
            this.toolbarUnstaged.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.toolbarUnstaged.Size = new System.Drawing.Size(397, 25);
            this.toolbarUnstaged.Stretch = true;
            this.toolbarUnstaged.TabIndex = 12;
            this.toolbarUnstaged.Text = "toolStrip1";
            // 
            // toolRefreshItem
            // 
            this.toolRefreshItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRefreshItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.toolRefreshItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRefreshItem.Name = "toolRefreshItem";
            this.toolRefreshItem.Size = new System.Drawing.Size(23, 20);
            this.toolRefreshItem.Click += new System.EventHandler(this.RescanChangesToolStripMenuItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 23);
            // 
            // workingToolStripMenuItem
            // 
            this.workingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showIgnoredFilesToolStripMenuItem,
            this.showUntrackedFilesToolStripMenuItem,
            this.toolStripSeparator3,
            this.deleteSelectedFilesToolStripMenuItem,
            this.resetSelectedFilesToolStripMenuItem,
            this.resetAlltrackedChangesToolStripMenuItem,
            this.toolStripSeparator1,
            this.editGitIgnoreToolStripMenuItem,
            this.deleteAllUntrackedFilesToolStripMenuItem});
            this.workingToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.workingToolStripMenuItem.Name = "workingToolStripMenuItem";
            this.workingToolStripMenuItem.Size = new System.Drawing.Size(133, 20);
            this.workingToolStripMenuItem.Text = "Working dir changes";
            // 
            // showIgnoredFilesToolStripMenuItem
            // 
            this.showIgnoredFilesToolStripMenuItem.Name = "showIgnoredFilesToolStripMenuItem";
            this.showIgnoredFilesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.showIgnoredFilesToolStripMenuItem.Text = "Show ignored files";
            this.showIgnoredFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowIgnoredFilesToolStripMenuItemClick);
            // 
            // showUntrackedFilesToolStripMenuItem
            // 
            this.showUntrackedFilesToolStripMenuItem.Checked = true;
            this.showUntrackedFilesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showUntrackedFilesToolStripMenuItem.Name = "showUntrackedFilesToolStripMenuItem";
            this.showUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.showUntrackedFilesToolStripMenuItem.Text = "Show untracked files";
            this.showUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowUntrackedFilesToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(213, 6);
            // 
            // deleteSelectedFilesToolStripMenuItem
            // 
            this.deleteSelectedFilesToolStripMenuItem.Name = "deleteSelectedFilesToolStripMenuItem";
            this.deleteSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.deleteSelectedFilesToolStripMenuItem.Text = "Delete selected files";
            this.deleteSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.DeleteSelectedFilesToolStripMenuItemClick);
            // 
            // resetSelectedFilesToolStripMenuItem
            // 
            this.resetSelectedFilesToolStripMenuItem.Name = "resetSelectedFilesToolStripMenuItem";
            this.resetSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.resetSelectedFilesToolStripMenuItem.Text = "Reset selected files";
            this.resetSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.ResetSelectedFilesToolStripMenuItemClick);
            // 
            // resetAlltrackedChangesToolStripMenuItem
            // 
            this.resetAlltrackedChangesToolStripMenuItem.Name = "resetAlltrackedChangesToolStripMenuItem";
            this.resetAlltrackedChangesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.resetAlltrackedChangesToolStripMenuItem.Text = "Reset all (tracked) changes";
            this.resetAlltrackedChangesToolStripMenuItem.Click += new System.EventHandler(this.ResetAlltrackedChangesToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(213, 6);
            // 
            // editGitIgnoreToolStripMenuItem
            // 
            this.editGitIgnoreToolStripMenuItem.Name = "editGitIgnoreToolStripMenuItem";
            this.editGitIgnoreToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.editGitIgnoreToolStripMenuItem.Text = "Edit ignored files";
            this.editGitIgnoreToolStripMenuItem.Click += new System.EventHandler(this.EditGitIgnoreToolStripMenuItemClick);
            // 
            // deleteAllUntrackedFilesToolStripMenuItem
            // 
            this.deleteAllUntrackedFilesToolStripMenuItem.Name = "deleteAllUntrackedFilesToolStripMenuItem";
            this.deleteAllUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.deleteAllUntrackedFilesToolStripMenuItem.Text = "Delete all untracked files";
            this.deleteAllUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.DeleteAllUntrackedFilesToolStripMenuItemClick);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripProgressBar1.Size = new System.Drawing.Size(150, 23);
            this.toolStripProgressBar1.Visible = false;
            // 
            // Staged
            // 
            this.Staged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Staged.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.Staged.GitItemStatuses = null;
            this.Staged.Location = new System.Drawing.Point(0, 28);
            this.Staged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Staged.Name = "Staged";
            this.Staged.Revision = null;
            this.Staged.SelectedItem = null;
            this.Staged.Size = new System.Drawing.Size(397, 328);
            this.Staged.TabIndex = 16;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(134, 167);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(129, 23);
            this.Cancel.TabIndex = 15;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // toolbarStaged
            // 
            this.toolbarStaged.AutoSize = false;
            this.toolbarStaged.BackColor = System.Drawing.SystemColors.Control;
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
            this.toolbarStaged.Size = new System.Drawing.Size(397, 28);
            this.toolbarStaged.TabIndex = 13;
            // 
            // toolStageAllItem
            // 
            this.toolStageAllItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStageAllItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStageAllItem.Image = global::GitUI.Properties.Resources.double_arrow_down;
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
            this.toolStageItem.Image = global::GitUI.Properties.Resources._4;
            this.toolStageItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStageItem.Name = "toolStageItem";
            this.toolStageItem.Size = new System.Drawing.Size(55, 23);
            this.toolStageItem.Text = "&Stage";
            this.toolStageItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStageItem.Click += new System.EventHandler(this.StageClick);
            // 
            // toolUnstageAllItem
            // 
            this.toolUnstageAllItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolUnstageAllItem.Image = global::GitUI.Properties.Resources.double_arrow_up;
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
            this.toolUnstageItem.Image = global::GitUI.Properties.Resources._31;
            this.toolUnstageItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolUnstageItem.Name = "toolUnstageItem";
            this.toolUnstageItem.Size = new System.Drawing.Size(67, 23);
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
            this.splitRight.Panel1.Controls.Add(this.SolveMergeconflicts);
            this.splitRight.Panel1.Controls.Add(this.SelectedDiff);
            // 
            // splitRight.Panel2
            // 
            this.splitRight.Panel2.Controls.Add(this.Message);
            this.splitRight.Panel2.Controls.Add(this.toolbarCommit);
            this.splitRight.Panel2.Controls.Add(this.flowCommitButtons);
            this.splitRight.Size = new System.Drawing.Size(517, 644);
            this.splitRight.SplitterDistance = 502;
            this.splitRight.TabIndex = 0;
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.SeaShell;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Image = ((System.Drawing.Image)(resources.GetObject("SolveMergeconflicts.Image")));
            this.SolveMergeconflicts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(14, 12);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(188, 42);
            this.SolveMergeconflicts.TabIndex = 8;
            this.SolveMergeconflicts.Text = "There are unresolved mergeconflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeConflictsClick);
            // 
            // SelectedDiff
            // 
            this.SelectedDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedDiff.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.SelectedDiff.IgnoreWhitespaceChanges = false;
            this.SelectedDiff.IsReadOnly = true;
            this.SelectedDiff.Location = new System.Drawing.Point(0, 0);
            this.SelectedDiff.Margin = new System.Windows.Forms.Padding(2, 3, 3, 3);
            this.SelectedDiff.Name = "SelectedDiff";
            this.SelectedDiff.NumberOfVisibleLines = 3;
            this.SelectedDiff.ScrollPos = 0;
            this.SelectedDiff.ShowEntireFile = false;
            this.SelectedDiff.ShowLineNumbers = true;
            this.SelectedDiff.Size = new System.Drawing.Size(517, 502);
            this.SelectedDiff.TabIndex = 0;
            this.SelectedDiff.TreatAllFilesAsText = false;
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.Message.Location = new System.Drawing.Point(175, 28);
            this.Message.Margin = new System.Windows.Forms.Padding(0);
            this.Message.MistakeFont = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Underline);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(342, 110);
            this.Message.TabIndex = 4;
            this.Message.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Message_KeyUp);
            this.Message.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Message_KeyDown);
            // 
            // toolbarCommit
            // 
            this.toolbarCommit.AutoSize = false;
            this.toolbarCommit.BackColor = System.Drawing.SystemColors.Control;
            this.toolbarCommit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbarCommit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitMessageToolStripMenuItem,
            this.toolStripMenuItem3});
            this.toolbarCommit.Location = new System.Drawing.Point(175, 0);
            this.toolbarCommit.Name = "toolbarCommit";
            this.toolbarCommit.Padding = new System.Windows.Forms.Padding(1, 1, 2, 1);
            this.toolbarCommit.Size = new System.Drawing.Size(342, 28);
            this.toolbarCommit.Stretch = true;
            this.toolbarCommit.TabIndex = 5;
            // 
            // commitMessageToolStripMenuItem
            // 
            this.commitMessageToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            this.commitMessageToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.commitMessageToolStripMenuItem.Size = new System.Drawing.Size(116, 23);
            this.commitMessageToolStripMenuItem.Text = "Commit &message";
            this.commitMessageToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.CommitMessageToolStripMenuItemDropDownItemClicked);
            this.commitMessageToolStripMenuItem.DropDownOpening += new System.EventHandler(this.CommitMessageToolStripMenuItemDropDownOpening);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeDialogAfterEachCommitToolStripMenuItem,
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem,
            this.refreshDialogOnFormFocusToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolAuthorLabelItem,
            this.toolAuthor});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStripMenuItem3.Size = new System.Drawing.Size(57, 23);
            this.toolStripMenuItem3.Text = "Options";
            // 
            // closeDialogAfterEachCommitToolStripMenuItem
            // 
            this.closeDialogAfterEachCommitToolStripMenuItem.Name = "closeDialogAfterEachCommitToolStripMenuItem";
            this.closeDialogAfterEachCommitToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.closeDialogAfterEachCommitToolStripMenuItem.Text = "Close dialog after each commit";
            this.closeDialogAfterEachCommitToolStripMenuItem.Click += new System.EventHandler(this.closeDialogAfterEachCommitToolStripMenuItem_Click);
            // 
            // closeDialogAfterAllFilesCommittedToolStripMenuItem
            // 
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Name = "closeDialogAfterAllFilesCommittedToolStripMenuItem";
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Text = "Close dialog when all changes are committed";
            this.closeDialogAfterAllFilesCommittedToolStripMenuItem.Click += new System.EventHandler(this.closeDialogAfterAllFilesCommittedToolStripMenuItem_Click);
            // 
            // refreshDialogOnFormFocusToolStripMenuItem
            // 
            this.refreshDialogOnFormFocusToolStripMenuItem.Name = "refreshDialogOnFormFocusToolStripMenuItem";
            this.refreshDialogOnFormFocusToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.refreshDialogOnFormFocusToolStripMenuItem.Text = "Refresh dialog on form focus";
            this.refreshDialogOnFormFocusToolStripMenuItem.Click += new System.EventHandler(this.refreshDialogOnFormFocusToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(295, 6);
            // 
            // toolAuthorLabelItem
            // 
            this.toolAuthorLabelItem.Enabled = false;
            this.toolAuthorLabelItem.Name = "toolAuthorLabelItem";
            this.toolAuthorLabelItem.Size = new System.Drawing.Size(298, 22);
            this.toolAuthorLabelItem.Text = "Author: (Format: \"name <mail>\")";
            this.toolAuthorLabelItem.Click += new System.EventHandler(this.toolAuthorLabelItem_Click);
            // 
            // toolAuthor
            // 
            this.toolAuthor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolAuthor.Name = "toolAuthor";
            this.toolAuthor.Size = new System.Drawing.Size(230, 21);
            this.toolAuthor.TextChanged += new System.EventHandler(this.toolAuthor_TextChanged);
            // 
            // flowCommitButtons
            // 
            this.flowCommitButtons.AutoSize = true;
            this.flowCommitButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowCommitButtons.Controls.Add(this.Commit);
            this.flowCommitButtons.Controls.Add(this.CommitAndPush);
            this.flowCommitButtons.Controls.Add(this.Amend);
            this.flowCommitButtons.Controls.Add(this.Reset);
            this.flowCommitButtons.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowCommitButtons.Location = new System.Drawing.Point(0, 0);
            this.flowCommitButtons.Name = "flowCommitButtons";
            this.flowCommitButtons.Size = new System.Drawing.Size(175, 138);
            this.flowCommitButtons.TabIndex = 1;
            // 
            // Commit
            // 
            this.Commit.Image = global::GitUI.Properties.Resources._10;
            this.Commit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Commit.Location = new System.Drawing.Point(1, 3);
            this.Commit.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(171, 26);
            this.Commit.TabIndex = 3;
            this.Commit.Text = "&Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.CommitClick);
            // 
            // CommitAndPush
            // 
            this.CommitAndPush.Image = global::GitUI.Properties.Resources._31;
            this.CommitAndPush.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CommitAndPush.Location = new System.Drawing.Point(1, 35);
            this.CommitAndPush.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.CommitAndPush.Name = "CommitAndPush";
            this.CommitAndPush.Size = new System.Drawing.Size(171, 26);
            this.CommitAndPush.TabIndex = 9;
            this.CommitAndPush.Text = "C&ommit && push";
            this.CommitAndPush.UseVisualStyleBackColor = true;
            this.CommitAndPush.Click += new System.EventHandler(this.CommitAndPush_Click);
            // 
            // Amend
            // 
            this.Amend.Location = new System.Drawing.Point(1, 67);
            this.Amend.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.Amend.Name = "Amend";
            this.Amend.Size = new System.Drawing.Size(171, 26);
            this.Amend.TabIndex = 10;
            this.Amend.Text = "&Amend last commit";
            this.Amend.UseVisualStyleBackColor = true;
            this.Amend.Click += new System.EventHandler(this.AmendClick);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(1, 99);
            this.Reset.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(171, 26);
            this.Reset.TabIndex = 11;
            this.Reset.Text = "Reset changes";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.ResetClick);
            // 
            // FormCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = null;
            this.ClientSize = new System.Drawing.Size(918, 644);
            this.Controls.Add(this.splitMain);
            this.Name = "FormCommit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commit";
            this.Load += new System.EventHandler(this.FormCommitLoad);
            this.Shown += new System.EventHandler(this.FormCommitShown);
            this.Activated += new System.EventHandler(this.FormCommitActivated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCommitFormClosing);
            this.UnstagedFileContext.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).EndInit();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel1.PerformLayout();
            this.splitLeft.Panel2.ResumeLayout(false);
            this.splitLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.toolbarUnstaged.ResumeLayout(false);
            this.toolbarUnstaged.PerformLayout();
            this.toolbarStaged.ResumeLayout(false);
            this.toolbarStaged.PerformLayout();
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            this.splitRight.Panel2.PerformLayout();
            this.splitRight.ResumeLayout(false);
            this.toolbarCommit.ResumeLayout(false);
            this.toolbarCommit.PerformLayout();
            this.flowCommitButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitLeft;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.BindingSource gitItemStatusBindingSource;
        private System.Windows.Forms.ContextMenuStrip UnstagedFileContext;
        private System.Windows.Forms.ToolStripMenuItem ResetChanges;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private FileViewer SelectedDiff;
        private System.Windows.Forms.ToolStripMenuItem addFileTogitignoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filenameToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolTip fileTooltip;
        private System.Windows.Forms.ToolStripMenuItem resetPartOfFileToolStripMenuItem;
        private ToolStripMenuItem editFileToolStripMenuItem;
        private ToolStripMenuItem ViewFileHistoryToolStripItem;
        private EditNetSpell Message;
        private FlowLayoutPanel flowCommitButtons;
        private Button Commit;
        private Button CommitAndPush;
        private Button Amend;
        private Button Reset;
        private ToolStrip toolbarCommit;
        private ToolStripDropDownButton commitMessageToolStripMenuItem;
        private ToolStripDropDownButton toolStripMenuItem3;
        private ToolStripMenuItem closeDialogAfterEachCommitToolStripMenuItem;
        private ToolStripMenuItem closeDialogAfterAllFilesCommittedToolStripMenuItem;
        private ToolStripMenuItem refreshDialogOnFormFocusToolStripMenuItem;
        private PictureBox Loading;
        private FileStatusList Unstaged;
        private ToolStrip toolbarUnstaged;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripDropDownButton workingToolStripMenuItem;
        private ToolStripMenuItem showIgnoredFilesToolStripMenuItem;
        private ToolStripMenuItem showUntrackedFilesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem deleteSelectedFilesToolStripMenuItem;
        private ToolStripMenuItem resetSelectedFilesToolStripMenuItem;
        private ToolStripMenuItem resetAlltrackedChangesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem editGitIgnoreToolStripMenuItem;
        private ToolStripMenuItem deleteAllUntrackedFilesToolStripMenuItem;
        private ToolStrip toolbarStaged;
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
    }
}