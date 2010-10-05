using System;
using System.Windows.Forms;
using GitCommands;
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this._NO_TRANSLATE_menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.workingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIgnoredFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetAlltrackedChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editGitIgnoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rescanChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.Unstaged = new GitUI.FileStatusList();
            this.UnstagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ResetChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPartOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileTogitignoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.filenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.AddFiles = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.filesListedToCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stageAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unstageAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.stageChunkOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UnstageFiles = new System.Windows.Forms.Button();
            this.Staged = new GitUI.FileStatusList();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.SelectedDiff = new GitUI.Editor.FileViewer();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Scan = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.Amend = new System.Windows.Forms.Button();
            this.Commit = new System.Windows.Forms.Button();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.Message = new GitUI.SpellChecker.EditNetSpell();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_menuStrip3 = new System.Windows.Forms.MenuStrip();
            this.commitMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CommitAndPush = new System.Windows.Forms.Button();
            this.CloseDialogAfterCommit = new System.Windows.Forms.CheckBox();
            this.CloseCommitDialogTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.fileTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.gitItemStatusBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.editFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this._NO_TRANSLATE_menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.UnstagedFileContext.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this._NO_TRANSLATE_menuStrip2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this._NO_TRANSLATE_menuStrip3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.Controls.Add(this.Ok);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(891, 644);
            this.splitContainer1.SplitterDistance = 397;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer2.Size = new System.Drawing.Size(397, 644);
            this.splitContainer2.SplitterDistance = 284;
            this.splitContainer2.TabIndex = 3;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.tableLayoutPanel5);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.Loading);
            this.splitContainer4.Panel2.Controls.Add(this.Unstaged);
            this.splitContainer4.Size = new System.Drawing.Size(397, 284);
            this.splitContainer4.SplitterDistance = 25;
            this.splitContainer4.SplitterWidth = 1;
            this.splitContainer4.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.progressBar, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this._NO_TRANSLATE_menuStrip1, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(397, 25);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(201, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(193, 19);
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;
            // 
            // _NO_TRANSLATE_menuStrip1
            // 
            this._NO_TRANSLATE_menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this._NO_TRANSLATE_menuStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.workingToolStripMenuItem});
            this._NO_TRANSLATE_menuStrip1.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_menuStrip1.Name = "_NO_TRANSLATE_menuStrip1";
            this._NO_TRANSLATE_menuStrip1.Size = new System.Drawing.Size(198, 25);
            this._NO_TRANSLATE_menuStrip1.TabIndex = 2;
            this._NO_TRANSLATE_menuStrip1.Text = "menuStrip1";
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
            this.deleteAllUntrackedFilesToolStripMenuItem,
            this.rescanChangesToolStripMenuItem});
            this.workingToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.workingToolStripMenuItem.Name = "workingToolStripMenuItem";
            this.workingToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.workingToolStripMenuItem.Size = new System.Drawing.Size(153, 21);
            this.workingToolStripMenuItem.Text = "Working dir changes";
            // 
            // showIgnoredFilesToolStripMenuItem
            // 
            this.showIgnoredFilesToolStripMenuItem.Name = "showIgnoredFilesToolStripMenuItem";
            this.showIgnoredFilesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.showIgnoredFilesToolStripMenuItem.Text = "Show ignored files";
            this.showIgnoredFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowIgnoredFilesToolStripMenuItemClick);
            // 
            // showUntrackedFilesToolStripMenuItem
            // 
            this.showUntrackedFilesToolStripMenuItem.Checked = true;
            this.showUntrackedFilesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showUntrackedFilesToolStripMenuItem.Name = "showUntrackedFilesToolStripMenuItem";
            this.showUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.showUntrackedFilesToolStripMenuItem.Text = "Show untracked files";
            this.showUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowUntrackedFilesToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(242, 6);
            // 
            // deleteSelectedFilesToolStripMenuItem
            // 
            this.deleteSelectedFilesToolStripMenuItem.Name = "deleteSelectedFilesToolStripMenuItem";
            this.deleteSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.deleteSelectedFilesToolStripMenuItem.Text = "Delete selected files";
            this.deleteSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.DeleteSelectedFilesToolStripMenuItemClick);
            // 
            // resetSelectedFilesToolStripMenuItem
            // 
            this.resetSelectedFilesToolStripMenuItem.Name = "resetSelectedFilesToolStripMenuItem";
            this.resetSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.resetSelectedFilesToolStripMenuItem.Text = "Reset selected files";
            this.resetSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.ResetSelectedFilesToolStripMenuItemClick);
            // 
            // resetAlltrackedChangesToolStripMenuItem
            // 
            this.resetAlltrackedChangesToolStripMenuItem.Name = "resetAlltrackedChangesToolStripMenuItem";
            this.resetAlltrackedChangesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.resetAlltrackedChangesToolStripMenuItem.Text = "Reset all (tracked) changes";
            this.resetAlltrackedChangesToolStripMenuItem.Click += new System.EventHandler(this.ResetAlltrackedChangesToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(242, 6);
            // 
            // editGitIgnoreToolStripMenuItem
            // 
            this.editGitIgnoreToolStripMenuItem.Name = "editGitIgnoreToolStripMenuItem";
            this.editGitIgnoreToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.editGitIgnoreToolStripMenuItem.Text = "Edit ignored files";
            this.editGitIgnoreToolStripMenuItem.Click += new System.EventHandler(this.EditGitIgnoreToolStripMenuItemClick);
            // 
            // deleteAllUntrackedFilesToolStripMenuItem
            // 
            this.deleteAllUntrackedFilesToolStripMenuItem.Name = "deleteAllUntrackedFilesToolStripMenuItem";
            this.deleteAllUntrackedFilesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.deleteAllUntrackedFilesToolStripMenuItem.Text = "Delete all untracked files";
            this.deleteAllUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.DeleteAllUntrackedFilesToolStripMenuItemClick);
            // 
            // rescanChangesToolStripMenuItem
            // 
            this.rescanChangesToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.rescanChangesToolStripMenuItem.Name = "rescanChangesToolStripMenuItem";
            this.rescanChangesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.rescanChangesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.rescanChangesToolStripMenuItem.Text = "Rescan changes";
            this.rescanChangesToolStripMenuItem.Click += new System.EventHandler(this.RescanChangesToolStripMenuItemClick);
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(397, 258);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 2;
            this.Loading.TabStop = false;
            // 
            // Unstaged
            // 
            this.Unstaged.ContextMenuStrip = this.UnstagedFileContext;
            this.Unstaged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Unstaged.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.Unstaged.GitItemStatusses = null;
            this.Unstaged.Location = new System.Drawing.Point(0, 0);
            this.Unstaged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Unstaged.Name = "Unstaged";
            this.Unstaged.Revision = null;
            this.Unstaged.SelectedItem = null;
            this.Unstaged.Size = new System.Drawing.Size(397, 258);
            this.Unstaged.TabIndex = 0;
            // 
            // UnstagedFileContext
            // 
            this.UnstagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ResetChanges,
            this.resetPartOfFileToolStripMenuItem,
            this.editFileToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.addFileTogitignoreToolStripMenuItem,
            this.toolStripSeparator4,
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.openWithDifftoolToolStripMenuItem,
            this.toolStripSeparator5,
            this.filenameToClipboardToolStripMenuItem});
            this.UnstagedFileContext.Name = "UnstagedFileContext";
            this.UnstagedFileContext.Size = new System.Drawing.Size(213, 236);
            // 
            // ResetChanges
            // 
            this.ResetChanges.Name = "ResetChanges";
            this.ResetChanges.Size = new System.Drawing.Size(212, 22);
            this.ResetChanges.Text = "Reset file changes";
            this.ResetChanges.Click += new System.EventHandler(this.ResetSoftClick);
            // 
            // resetPartOfFileToolStripMenuItem
            // 
            this.resetPartOfFileToolStripMenuItem.Name = "resetPartOfFileToolStripMenuItem";
            this.resetPartOfFileToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.resetPartOfFileToolStripMenuItem.Text = "Reset chunk of file";
            this.resetPartOfFileToolStripMenuItem.Click += new System.EventHandler(this.ResetPartOfFileToolStripMenuItemClick);
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.deleteFileToolStripMenuItem.Text = "Delete file";
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.DeleteFileToolStripMenuItemClick);
            // 
            // addFileTogitignoreToolStripMenuItem
            // 
            this.addFileTogitignoreToolStripMenuItem.Name = "addFileTogitignoreToolStripMenuItem";
            this.addFileTogitignoreToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.addFileTogitignoreToolStripMenuItem.Text = "Add file to .gitignore";
            this.addFileTogitignoreToolStripMenuItem.Click += new System.EventHandler(this.AddFileTogitignoreToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(209, 6);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.openWithToolStripMenuItem.Text = "Open With";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.OpenWithToolStripMenuItemClick);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open With Difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(209, 6);
            // 
            // filenameToClipboardToolStripMenuItem
            // 
            this.filenameToClipboardToolStripMenuItem.Name = "filenameToClipboardToolStripMenuItem";
            this.filenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.filenameToClipboardToolStripMenuItem.Text = "Filename to clipboard";
            this.filenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.FilenameToClipboardToolStripMenuItemClick);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.tableLayoutPanel4);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.Staged);
            this.splitContainer5.Panel2.Controls.Add(this.Cancel);
            this.splitContainer5.Size = new System.Drawing.Size(397, 356);
            this.splitContainer5.SplitterDistance = 30;
            this.splitContainer5.SplitterWidth = 1;
            this.splitContainer5.TabIndex = 1;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel4.Controls.Add(this.AddFiles, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this._NO_TRANSLATE_menuStrip2, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.UnstageFiles, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(397, 30);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // AddFiles
            // 
            this.AddFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddFiles.Image = global::GitUI.Properties.Resources._4;
            this.AddFiles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddFiles.Location = new System.Drawing.Point(268, 3);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(126, 24);
            this.AddFiles.TabIndex = 4;
            this.AddFiles.Text = "Stage";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.StageClick);
            // 
            // _NO_TRANSLATE_menuStrip2
            // 
            this._NO_TRANSLATE_menuStrip2.BackColor = System.Drawing.SystemColors.Control;
            this._NO_TRANSLATE_menuStrip2.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filesListedToCommitToolStripMenuItem});
            this._NO_TRANSLATE_menuStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this._NO_TRANSLATE_menuStrip2.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_menuStrip2.Name = "_NO_TRANSLATE_menuStrip2";
            this._NO_TRANSLATE_menuStrip2.Size = new System.Drawing.Size(134, 30);
            this._NO_TRANSLATE_menuStrip2.TabIndex = 5;
            this._NO_TRANSLATE_menuStrip2.Text = "menuStrip2";
            // 
            // filesListedToCommitToolStripMenuItem
            // 
            this.filesListedToCommitToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.filesListedToCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stageAllToolStripMenuItem,
            this.unstageAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.stageChunkOfFileToolStripMenuItem});
            this.filesListedToCommitToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.filesListedToCommitToolStripMenuItem.Name = "filesListedToCommitToolStripMenuItem";
            this.filesListedToCommitToolStripMenuItem.Size = new System.Drawing.Size(103, 20);
            this.filesListedToCommitToolStripMenuItem.Text = "Staged files";
            // 
            // stageAllToolStripMenuItem
            // 
            this.stageAllToolStripMenuItem.Name = "stageAllToolStripMenuItem";
            this.stageAllToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.stageAllToolStripMenuItem.Text = "Stage all";
            this.stageAllToolStripMenuItem.Click += new System.EventHandler(this.StageAllToolStripMenuItemClick);
            // 
            // unstageAllToolStripMenuItem
            // 
            this.unstageAllToolStripMenuItem.Name = "unstageAllToolStripMenuItem";
            this.unstageAllToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.unstageAllToolStripMenuItem.Text = "Unstage all";
            this.unstageAllToolStripMenuItem.Click += new System.EventHandler(this.UnstageAllToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(192, 6);
            // 
            // stageChunkOfFileToolStripMenuItem
            // 
            this.stageChunkOfFileToolStripMenuItem.Name = "stageChunkOfFileToolStripMenuItem";
            this.stageChunkOfFileToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.stageChunkOfFileToolStripMenuItem.Text = "Stage chunk of file";
            this.stageChunkOfFileToolStripMenuItem.Click += new System.EventHandler(this.StageChunkOfFileToolStripMenuItemClick);
            // 
            // UnstageFiles
            // 
            this.UnstageFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UnstageFiles.Image = global::GitUI.Properties.Resources._3;
            this.UnstageFiles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UnstageFiles.Location = new System.Drawing.Point(137, 3);
            this.UnstageFiles.Name = "UnstageFiles";
            this.UnstageFiles.Size = new System.Drawing.Size(125, 24);
            this.UnstageFiles.TabIndex = 1;
            this.UnstageFiles.Text = "Unstage";
            this.UnstageFiles.UseVisualStyleBackColor = true;
            this.UnstageFiles.Click += new System.EventHandler(this.UnstageFilesClick);
            // 
            // Staged
            // 
            this.Staged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Staged.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.Staged.GitItemStatusses = null;
            this.Staged.Location = new System.Drawing.Point(0, 0);
            this.Staged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Staged.Name = "Staged";
            this.Staged.Revision = null;
            this.Staged.SelectedItem = null;
            this.Staged.Size = new System.Drawing.Size(397, 325);
            this.Staged.TabIndex = 0;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(139, 143);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(129, 23);
            this.Cancel.TabIndex = 9;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.CancelClick);
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
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.SolveMergeconflicts);
            this.splitContainer3.Panel1.Controls.Add(this.SelectedDiff);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tableLayoutPanel6);
            this.splitContainer3.Panel2MinSize = 134;
            this.splitContainer3.Size = new System.Drawing.Size(490, 644);
            this.splitContainer3.SplitterDistance = 469;
            this.splitContainer3.TabIndex = 0;
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
            this.SelectedDiff.Margin = new System.Windows.Forms.Padding(4);
            this.SelectedDiff.Name = "SelectedDiff";
            this.SelectedDiff.NumberOfVisibleLines = 3;
            this.SelectedDiff.ScrollPos = 0;
            this.SelectedDiff.ShowEntireFile = false;
            this.SelectedDiff.Size = new System.Drawing.Size(490, 506);
            this.SelectedDiff.TabIndex = 0;
            this.SelectedDiff.TreatAllFilesAsText = false;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(490, 171);
            this.tableLayoutPanel6.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.Scan, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.Reset, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.Amend, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.Commit, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.CommitAndPush, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(130, 171);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // Scan
            // 
            this.Scan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Scan.Location = new System.Drawing.Point(3, 131);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(124, 26);
            this.Scan.TabIndex = 3;
            this.Scan.Text = "&Rescan changes";
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.ScanClick);
            // 
            // Reset
            // 
            this.Reset.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Reset.Location = new System.Drawing.Point(3, 99);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(124, 26);
            this.Reset.TabIndex = 5;
            this.Reset.Text = "Reset changes";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.ResetClick);
            // 
            // Amend
            // 
            this.Amend.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Amend.Location = new System.Drawing.Point(3, 67);
            this.Amend.Name = "Amend";
            this.Amend.Size = new System.Drawing.Size(124, 26);
            this.Amend.TabIndex = 7;
            this.Amend.Text = "&Amend last commit";
            this.Amend.UseVisualStyleBackColor = true;
            this.Amend.Click += new System.EventHandler(this.AmendClick);
            // 
            // Commit
            // 
            this.Commit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Commit.Image = global::GitUI.Properties.Resources._10;
            this.Commit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Commit.Location = new System.Drawing.Point(3, 3);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(124, 26);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "&Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.CommitClick);
            // 
            // CommitAndPush
            // 
            this.CommitAndPush.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CommitAndPush.Image = global::GitUI.Properties.Resources._31;
            this.CommitAndPush.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CommitAndPush.Location = new System.Drawing.Point(3, 35);
            this.CommitAndPush.Name = "CommitAndPush";
            this.CommitAndPush.Size = new System.Drawing.Size(124, 26);
            this.CommitAndPush.TabIndex = 8;
            this.CommitAndPush.Text = "C&ommit && push";
            this.CommitAndPush.UseVisualStyleBackColor = true;
            this.CommitAndPush.Click += new System.EventHandler(this.CommitAndPush_Click);
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.Message, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(130, 0);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(364, 171);
            this.tableLayoutPanel7.TabIndex = 1;
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.Message.Location = new System.Drawing.Point(4, 34);
            this.Message.Margin = new System.Windows.Forms.Padding(4);
            this.Message.MistakeFont = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Underline);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(356, 133);
            this.Message.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this._NO_TRANSLATE_menuStrip3, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.CloseDialogAfterCommit, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(364, 30);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // _NO_TRANSLATE_menuStrip3
            // 
            this._NO_TRANSLATE_menuStrip3.BackColor = System.Drawing.SystemColors.Control;
            this._NO_TRANSLATE_menuStrip3.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_menuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitMessageToolStripMenuItem});
            this._NO_TRANSLATE_menuStrip3.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_menuStrip3.Name = "_NO_TRANSLATE_menuStrip3";
            this._NO_TRANSLATE_menuStrip3.Size = new System.Drawing.Size(182, 30);
            this._NO_TRANSLATE_menuStrip3.TabIndex = 0;
            this._NO_TRANSLATE_menuStrip3.Text = "menuStrip3";
            // 
            // commitMessageToolStripMenuItem
            // 
            this.commitMessageToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            this.commitMessageToolStripMenuItem.Size = new System.Drawing.Size(135, 26);
            this.commitMessageToolStripMenuItem.Text = "Commit &message";
            this.commitMessageToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.CommitMessageToolStripMenuItemDropDownItemClicked);
            this.commitMessageToolStripMenuItem.DropDownOpening += new System.EventHandler(this.CommitMessageToolStripMenuItemDropDownOpening);
            // 
            // CloseDialogAfterCommit
            // 
            this.CloseDialogAfterCommit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.CloseDialogAfterCommit.AutoSize = true;
            this.CloseDialogAfterCommit.Location = new System.Drawing.Point(185, 3);
            this.CloseDialogAfterCommit.Name = "CloseDialogAfterCommit";
            this.CloseDialogAfterCommit.Size = new System.Drawing.Size(173, 20);
            this.CloseDialogAfterCommit.TabIndex = 0;
            this.CloseDialogAfterCommit.Text = "Close dialog after commit";
            this.CloseDialogAfterCommit.UseVisualStyleBackColor = true;
            this.CloseDialogAfterCommit.CheckedChanged += new System.EventHandler(this.CloseDialogAfterCommitCheckedChanged);
            // 
            // CloseCommitDialogTooltip
            // 
            this.CloseCommitDialogTooltip.AutomaticDelay = 1;
            this.CloseCommitDialogTooltip.AutoPopDelay = 5000;
            this.CloseCommitDialogTooltip.InitialDelay = 1;
            this.CloseCommitDialogTooltip.ReshowDelay = 1;
            this.CloseCommitDialogTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
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
            // editFileToolStripMenuItem
            // 
            this.editFileToolStripMenuItem.Name = "editFileToolStripMenuItem";
            this.editFileToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.editFileToolStripMenuItem.Text = "Edit file";
            this.editFileToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // FormCommit
            // 
            this.AcceptButton = this.Commit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(891, 644);
            this.Controls.Add(this.splitContainer1);
            this.MainMenuStrip = this._NO_TRANSLATE_menuStrip1;
            this.Name = "FormCommit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commit";
            this.Load += new System.EventHandler(this.FormCommitLoad);
            this.Shown += new System.EventHandler(this.FormCommitShown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCommitFormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this._NO_TRANSLATE_menuStrip1.ResumeLayout(false);
            this._NO_TRANSLATE_menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.UnstagedFileContext.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this._NO_TRANSLATE_menuStrip2.ResumeLayout(false);
            this._NO_TRANSLATE_menuStrip2.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this._NO_TRANSLATE_menuStrip3.ResumeLayout(false);
            this._NO_TRANSLATE_menuStrip3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private FileStatusList Unstaged;
        private FileStatusList Staged;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.BindingSource gitItemStatusBindingSource;
        private System.Windows.Forms.Button UnstageFiles;
        private System.Windows.Forms.PictureBox Loading;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ContextMenuStrip UnstagedFileContext;
        private System.Windows.Forms.ToolStripMenuItem ResetChanges;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.MenuStrip _NO_TRANSLATE_menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem workingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetAlltrackedChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editGitIgnoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.MenuStrip _NO_TRANSLATE_menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem filesListedToCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stageAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unstageAllToolStripMenuItem;
        private FileViewer SelectedDiff;
        private EditNetSpell Message;
        private System.Windows.Forms.ToolStripMenuItem deleteAllUntrackedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem stageChunkOfFileToolStripMenuItem;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.CheckBox CloseDialogAfterCommit;
        private System.Windows.Forms.ToolStripMenuItem showIgnoredFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.MenuStrip _NO_TRANSLATE_menuStrip3;
        private System.Windows.Forms.ToolStripMenuItem commitMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFileTogitignoreToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ToolStripMenuItem showUntrackedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rescanChangesToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button Scan;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.Button Amend;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolTip CloseCommitDialogTooltip;
        private System.Windows.Forms.ToolStripMenuItem filenameToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolTip fileTooltip;
        private System.Windows.Forms.ToolStripMenuItem resetPartOfFileToolStripMenuItem;
        private ToolStripMenuItem editFileToolStripMenuItem;
        private Button CommitAndPush;
    }
}