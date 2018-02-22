using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormFileHistory
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.FileChanges = new GitUI.RevisionGrid();
            this.FileHistoryContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffToolremotelocalStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.manipulateCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickThisCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.followFileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.followFileHistoryRenamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.CommitInfoTabPage = new System.Windows.Forms.TabPage();
            this.CommitDiff = new GitUI.UserControls.CommitDiff();
            this.ViewTab = new System.Windows.Forms.TabPage();
            this.View = new GitUI.Editor.FileViewer();
            this.DiffTab = new System.Windows.Forms.TabPage();
            this.Diff = new GitUI.Editor.FileViewer();
            this.BlameTab = new System.Windows.Forms.TabPage();
            this.Blame = new GitUI.Blame.BlameControl();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripBranchFilterComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripBranchFilterDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripRevisionFilterLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripRevisionFilterTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripRevisionFilterDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.ShowFirstParent = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitLoad = new System.Windows.Forms.ToolStripSplitButton();
            this.loadHistoryOnShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadBlameOnShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowFullHistory = new System.Windows.Forms.ToolStripButton();
            this.toolStripBlameOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.ignoreWhitespaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detectMoveAndCopyInThisFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detectMoveAndCopyInAllFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.FileHistoryContextMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.CommitInfoTabPage.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.DiffTab.SuspendLayout();
            this.BlameTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.FileChanges);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(748, 419);
            this.splitContainer1.SplitterDistance = 101;
            this.splitContainer1.TabIndex = 0;
            // 
            // FileChanges
            // 
            this.FileChanges.ContextMenuStrip = this.FileHistoryContextMenu;
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Location = new System.Drawing.Point(0, 0);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.RevisionGraphDrawStyle = GitUI.RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
            this.FileChanges.ShowUncommitedChangesIfPossible = true;
            this.FileChanges.Size = new System.Drawing.Size(748, 101);
            this.FileChanges.TabIndex = 2;
            this.FileChanges.DoubleClick += new System.EventHandler(this.FileChangesDoubleClick);
            // 
            // FileHistoryContextMenu
            // 
            this.FileHistoryContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.diffToolremotelocalStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.manipulateCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.followFileHistoryToolStripMenuItem,
            this.followFileHistoryRenamesToolStripMenuItem,
            this.fullHistoryToolStripMenuItem,
            this.toolStripSeparator4});
            this.FileHistoryContextMenu.Name = "DiffContextMenu";
            this.FileHistoryContextMenu.Size = new System.Drawing.Size(340, 198);
            this.FileHistoryContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.FileHistoryContextMenuOpening);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconDiffTool;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // diffToolremotelocalStripMenuItem
            // 
            this.diffToolremotelocalStripMenuItem.Name = "diffToolremotelocalStripMenuItem";
            this.diffToolremotelocalStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.diffToolremotelocalStripMenuItem.Text = "Difftool selected < - > local";
            this.diffToolremotelocalStripMenuItem.Click += new System.EventHandler(this.diffToolremotelocalStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(336, 6);
            // 
            // manipulateCommitToolStripMenuItem
            // 
            this.manipulateCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.revertCommitToolStripMenuItem,
            this.cherryPickThisCommitToolStripMenuItem});
            this.manipulateCommitToolStripMenuItem.Name = "manipulateCommitToolStripMenuItem";
            this.manipulateCommitToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.manipulateCommitToolStripMenuItem.Text = "Manipulate commit";
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRevertCommit;
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.revertCommitToolStripMenuItem_Click);
            // 
            // cherryPickThisCommitToolStripMenuItem
            // 
            this.cherryPickThisCommitToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCherryPick;
            this.cherryPickThisCommitToolStripMenuItem.Name = "cherryPickThisCommitToolStripMenuItem";
            this.cherryPickThisCommitToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.cherryPickThisCommitToolStripMenuItem.Text = "Cherry pick commit";
            this.cherryPickThisCommitToolStripMenuItem.Click += new System.EventHandler(this.cherryPickThisCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(336, 6);
            // 
            // followFileHistoryToolStripMenuItem
            // 
            this.followFileHistoryToolStripMenuItem.Name = "followFileHistoryToolStripMenuItem";
            this.followFileHistoryToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.followFileHistoryToolStripMenuItem.Text = "Detect and follow renames";
            this.followFileHistoryToolStripMenuItem.Click += new System.EventHandler(this.followFileHistoryToolStripMenuItem_Click);
            // 
            // followFileHistoryRenamesToolStripMenuItem
            // 
            this.followFileHistoryRenamesToolStripMenuItem.Name = "followFileHistoryRenamesToolStripMenuItem";
            this.followFileHistoryRenamesToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.followFileHistoryRenamesToolStripMenuItem.Text = "Detect and follow - exact renames and copies only";
            this.followFileHistoryRenamesToolStripMenuItem.Click += new System.EventHandler(this.followFileHistoryRenamesToolStripMenuItem_Click);
            // 
            // fullHistoryToolStripMenuItem
            // 
            this.fullHistoryToolStripMenuItem.Name = "fullHistoryToolStripMenuItem";
            this.fullHistoryToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.fullHistoryToolStripMenuItem.Text = "Full history";
            this.fullHistoryToolStripMenuItem.Click += new System.EventHandler(this.fullHistoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(336, 6);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.CommitInfoTabPage);
            this.tabControl1.Controls.Add(this.ViewTab);
            this.tabControl1.Controls.Add(this.DiffTab);
            this.tabControl1.Controls.Add(this.BlameTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(748, 314);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // CommitInfoTabPage
            // 
            this.CommitInfoTabPage.Controls.Add(this.CommitDiff);
            this.CommitInfoTabPage.Location = new System.Drawing.Point(4, 22);
            this.CommitInfoTabPage.Name = "CommitInfoTabPage";
            this.CommitInfoTabPage.Size = new System.Drawing.Size(740, 288);
            this.CommitInfoTabPage.TabIndex = 3;
            this.CommitInfoTabPage.Text = "Commit";
            // 
            // CommitDiff
            // 
            this.CommitDiff.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommitDiff.Location = new System.Drawing.Point(0, 0);
            this.CommitDiff.MinimumSize = new System.Drawing.Size(150, 148);
            this.CommitDiff.Name = "CommitDiff";
            this.CommitDiff.Size = new System.Drawing.Size(740, 288);
            this.CommitDiff.TabIndex = 0;
            // 
            // ViewTab
            // 
            this.ViewTab.Controls.Add(this.View);
            this.ViewTab.Location = new System.Drawing.Point(4, 22);
            this.ViewTab.Name = "ViewTab";
            this.ViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.ViewTab.Size = new System.Drawing.Size(740, 288);
            this.ViewTab.TabIndex = 0;
            this.ViewTab.Text = "View";
            this.ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Location = new System.Drawing.Point(3, 3);
            this.View.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.View.Name = "View";
            this.View.Size = new System.Drawing.Size(734, 282);
            this.View.TabIndex = 0;
            // 
            // DiffTab
            // 
            this.DiffTab.Controls.Add(this.Diff);
            this.DiffTab.Location = new System.Drawing.Point(4, 22);
            this.DiffTab.Name = "DiffTab";
            this.DiffTab.Padding = new System.Windows.Forms.Padding(3);
            this.DiffTab.Size = new System.Drawing.Size(740, 288);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.Location = new System.Drawing.Point(3, 3);
            this.Diff.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Diff.Name = "Diff";
            this.Diff.Size = new System.Drawing.Size(734, 282);
            this.Diff.TabIndex = 0;
            // 
            // BlameTab
            // 
            this.BlameTab.Controls.Add(this.Blame);
            this.BlameTab.Location = new System.Drawing.Point(4, 22);
            this.BlameTab.Name = "BlameTab";
            this.BlameTab.Size = new System.Drawing.Size(740, 288);
            this.BlameTab.TabIndex = 2;
            this.BlameTab.Text = "Blame";
            this.BlameTab.UseVisualStyleBackColor = true;
            // 
            // Blame
            // 
            this.Blame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Blame.Location = new System.Drawing.Point(0, 0);
            this.Blame.Margin = new System.Windows.Forms.Padding(4);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(740, 288);
            this.Blame.TabIndex = 0;
            this.Blame.CommandClick += new System.EventHandler<GitUI.CommitInfo.CommandEventArgs>(this.Blame_CommandClick);
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // ToolStrip
            // 
            this.ToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripBranchFilterComboBox,
            this.toolStripBranchFilterDropDownButton,
            this.toolStripSeparator19,
            this.toolStripRevisionFilterLabel,
            this.toolStripRevisionFilterTextBox,
            this.toolStripRevisionFilterDropDownButton,
            this.ShowFirstParent,
            this.toolStripSeparator3,
            this.toolStripSplitLoad,
            this.ShowFullHistory,
            this.toolStripBlameOptions});
            this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStrip.Size = new System.Drawing.Size(748, 25);
            this.ToolStrip.TabIndex = 5;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel1.Text = "Branches:";
            // 
            // toolStripBranchFilterComboBox
            // 
            this.toolStripBranchFilterComboBox.Name = "toolStripBranchFilterComboBox";
            this.toolStripBranchFilterComboBox.Size = new System.Drawing.Size(150, 25);
            this.toolStripBranchFilterComboBox.Click += new System.EventHandler(this.toolStripBranchFilterComboBox_Click);
            // 
            // toolStripBranchFilterDropDownButton
            // 
            this.toolStripBranchFilterDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBranchFilterDropDownButton.Image = global::GitUI.Properties.Resources.Settings;
            this.toolStripBranchFilterDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripBranchFilterDropDownButton.Name = "toolStripBranchFilterDropDownButton";
            this.toolStripBranchFilterDropDownButton.Size = new System.Drawing.Size(29, 22);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripRevisionFilterLabel
            // 
            this.toolStripRevisionFilterLabel.Name = "toolStripRevisionFilterLabel";
            this.toolStripRevisionFilterLabel.Size = new System.Drawing.Size(36, 22);
            this.toolStripRevisionFilterLabel.Text = "Filter:";
            // 
            // toolStripRevisionFilterTextBox
            // 
            this.toolStripRevisionFilterTextBox.ForeColor = System.Drawing.Color.Black;
            this.toolStripRevisionFilterTextBox.Name = "toolStripRevisionFilterTextBox";
            this.toolStripRevisionFilterTextBox.Size = new System.Drawing.Size(120, 25);
            // 
            // toolStripRevisionFilterDropDownButton
            // 
            this.toolStripRevisionFilterDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRevisionFilterDropDownButton.Image = global::GitUI.Properties.Resources.Settings;
            this.toolStripRevisionFilterDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRevisionFilterDropDownButton.Name = "toolStripRevisionFilterDropDownButton";
            this.toolStripRevisionFilterDropDownButton.Size = new System.Drawing.Size(29, 22);
            // 
            // ShowFirstParent
            // 
            this.ShowFirstParent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowFirstParent.Image = global::GitUI.Properties.Resources.IconShowFirstParent;
            this.ShowFirstParent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowFirstParent.Name = "ShowFirstParent";
            this.ShowFirstParent.Size = new System.Drawing.Size(23, 22);
            this.ShowFirstParent.ToolTipText = "Show first parents";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSplitLoad
            // 
            this.toolStripSplitLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitLoad.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadHistoryOnShowToolStripMenuItem,
            this.loadBlameOnShowToolStripMenuItem});
            this.toolStripSplitLoad.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.toolStripSplitLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitLoad.Name = "toolStripSplitLoad";
            this.toolStripSplitLoad.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitLoad.ToolTipText = "Load file history";
            this.toolStripSplitLoad.ButtonClick += new System.EventHandler(this.toolStripSplitLoad_ButtonClick);
            // 
            // loadHistoryOnShowToolStripMenuItem
            // 
            this.loadHistoryOnShowToolStripMenuItem.Checked = true;
            this.loadHistoryOnShowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loadHistoryOnShowToolStripMenuItem.Name = "loadHistoryOnShowToolStripMenuItem";
            this.loadHistoryOnShowToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.loadHistoryOnShowToolStripMenuItem.Text = "Load history on show";
            this.loadHistoryOnShowToolStripMenuItem.Click += new System.EventHandler(this.loadHistoryOnShowToolStripMenuItem_Click);
            // 
            // loadBlameOnShowToolStripMenuItem
            // 
            this.loadBlameOnShowToolStripMenuItem.Checked = true;
            this.loadBlameOnShowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loadBlameOnShowToolStripMenuItem.Name = "loadBlameOnShowToolStripMenuItem";
            this.loadBlameOnShowToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.loadBlameOnShowToolStripMenuItem.Text = "Load blame on show";
            this.loadBlameOnShowToolStripMenuItem.Click += new System.EventHandler(this.loadBlameOnShowToolStripMenuItem_Click);
            // 
            // ShowFullHistory
            // 
            this.ShowFullHistory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowFullHistory.Image = global::GitUI.Properties.Resources.IconFileHistory;
            this.ShowFullHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowFullHistory.Name = "ShowFullHistory";
            this.ShowFullHistory.Size = new System.Drawing.Size(23, 22);
            this.ShowFullHistory.ToolTipText = "Show Full History";
            this.ShowFullHistory.Click += new System.EventHandler(this.ShowFullHistory_Click);
            // 
            // toolStripBlameOptions
            // 
            this.toolStripBlameOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBlameOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ignoreWhitespaceToolStripMenuItem,
            this.detectMoveAndCopyInThisFileToolStripMenuItem,
            this.detectMoveAndCopyInAllFilesToolStripMenuItem});
            this.toolStripBlameOptions.Image = global::GitUI.Properties.Resources.IconBlame;
            this.toolStripBlameOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripBlameOptions.Name = "toolStripBlameOptions";
            this.toolStripBlameOptions.Size = new System.Drawing.Size(29, 22);
            this.toolStripBlameOptions.Text = "Blame options";
            this.toolStripBlameOptions.ToolTipText = "Blame options";
            // 
            // ignoreWhitespaceToolStripMenuItem
            // 
            this.ignoreWhitespaceToolStripMenuItem.Name = "ignoreWhitespaceToolStripMenuItem";
            this.ignoreWhitespaceToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.ignoreWhitespaceToolStripMenuItem.Text = "Ignore whitespace";
            this.ignoreWhitespaceToolStripMenuItem.Click += new System.EventHandler(this.ignoreWhitespaceToolStripMenuItem_Click);
            // 
            // detectMoveAndCopyInThisFileToolStripMenuItem
            // 
            this.detectMoveAndCopyInThisFileToolStripMenuItem.Name = "detectMoveAndCopyInThisFileToolStripMenuItem";
            this.detectMoveAndCopyInThisFileToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.detectMoveAndCopyInThisFileToolStripMenuItem.Text = "Detect move and copy in this file";
            this.detectMoveAndCopyInThisFileToolStripMenuItem.Click += new System.EventHandler(this.detectMoveAndCopyInThisFileToolStripMenuItem_Click);
            // 
            // detectMoveAndCopyInAllFilesToolStripMenuItem
            // 
            this.detectMoveAndCopyInAllFilesToolStripMenuItem.Name = "detectMoveAndCopyInAllFilesToolStripMenuItem";
            this.detectMoveAndCopyInAllFilesToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.detectMoveAndCopyInAllFilesToolStripMenuItem.Text = "Detect move and copy in all files";
            this.detectMoveAndCopyInAllFilesToolStripMenuItem.Click += new System.EventHandler(this.detectMoveAndCopyInAllFilesToolStripMenuItem_Click);
            // 
            // FormFileHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(748, 444);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ToolStrip);
            this.MinimumSize = new System.Drawing.Size(299, 198);
            this.Name = "FormFileHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File History";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.FileHistoryContextMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.CommitInfoTabPage.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.DiffTab.ResumeLayout(false);
            this.BlameTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage CommitInfoTabPage;
        private UserControls.CommitDiff CommitDiff;
        private System.Windows.Forms.TabPage ViewTab;
        private System.Windows.Forms.TabPage DiffTab;
        private System.Windows.Forms.TabPage BlameTab;
        private System.Diagnostics.EventLog eventLog1;
        private FileViewer View;
        private FileViewer Diff;
        private RevisionGrid FileChanges;
        private System.Windows.Forms.ContextMenuStrip FileHistoryContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private Blame.BlameControl Blame;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem followFileHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manipulateCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickThisCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripBranchFilterComboBox;
        private System.Windows.Forms.ToolStripDropDownButton toolStripBranchFilterDropDownButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ToolStripLabel toolStripRevisionFilterLabel;
        private System.Windows.Forms.ToolStripTextBox toolStripRevisionFilterTextBox;
        private System.Windows.Forms.ToolStripDropDownButton toolStripRevisionFilterDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem diffToolremotelocalStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitLoad;
        private System.Windows.Forms.ToolStripMenuItem loadHistoryOnShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem loadBlameOnShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem followFileHistoryRenamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton ShowFirstParent;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton ShowFullHistory;
        private System.Windows.Forms.ToolStripDropDownButton toolStripBlameOptions;
        private System.Windows.Forms.ToolStripMenuItem ignoreWhitespaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectMoveAndCopyInThisFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectMoveAndCopyInAllFilesToolStripMenuItem;
    }
}