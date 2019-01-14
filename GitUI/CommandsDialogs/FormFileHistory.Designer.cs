using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormFileHistory
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.FileChanges = new GitUI.RevisionGridControl();
            this.FileHistoryContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToClipboardToolStripMenuItem = new GitUI.UserControls.RevisionGrid.CopyContextMenuItem();
            this.separatorAfterCopySubmenu = new System.Windows.Forms.ToolStripSeparator();
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffToolRemoteLocalStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.manipulateCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickThisCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.followFileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.followFileHistoryRenamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simplifyMergesContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tabControl1 = new GitUI.CommandsDialogs.FullBleedTabControl();
            this.CommitInfoTabPage = new System.Windows.Forms.TabPage();
            this.CommitDiff = new GitUI.UserControls.CommitDiff();
            this.DiffTab = new System.Windows.Forms.TabPage();
            this.Diff = new GitUI.Editor.FileViewer();
            this.ViewTab = new System.Windows.Forms.TabPage();
            this.View = new GitUI.Editor.FileViewer();
            this.BlameTab = new System.Windows.Forms.TabPage();
            this.Blame = new GitUI.Blame.BlameControl();
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
            this.ShowFullHistory = new System.Windows.Forms.ToolStripDropDownButton();
            this.showFullHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simplifyMergesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.DiffTab.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.BlameTab.SuspendLayout();
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
            this.FileChanges.Size = new System.Drawing.Size(748, 101);
            this.FileChanges.TabIndex = 2;
            this.FileChanges.DoubleClick += new System.EventHandler(this.FileChangesDoubleClick);
            // 
            // FileHistoryContextMenu
            // 
            this.FileHistoryContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToClipboardToolStripMenuItem,
            this.separatorAfterCopySubmenu,
            this.openWithDifftoolToolStripMenuItem,
            this.diffToolRemoteLocalStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.manipulateCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.followFileHistoryToolStripMenuItem,
            this.followFileHistoryRenamesToolStripMenuItem,
            this.fullHistoryToolStripMenuItem,
            this.simplifyMergesContextMenuItem,
            this.toolStripSeparator4});
            this.FileHistoryContextMenu.Name = "DiffContextMenu";
            this.FileHistoryContextMenu.Size = new System.Drawing.Size(340, 248);
            this.FileHistoryContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.FileHistoryContextMenuOpening);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            // 
            // separatorAfterCopySubmenu
            // 
            this.separatorAfterCopySubmenu.Name = "separatorAfterCopySubmenu";
            this.separatorAfterCopySubmenu.Size = new System.Drawing.Size(336, 6);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Images.Diff;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // diffToolRemoteLocalStripMenuItem
            // 
            this.diffToolRemoteLocalStripMenuItem.Name = "diffToolRemoteLocalStripMenuItem";
            this.diffToolRemoteLocalStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.diffToolRemoteLocalStripMenuItem.Text = "Difftool selected < - > local";
            this.diffToolRemoteLocalStripMenuItem.Click += new System.EventHandler(this.diffToolRemoteLocalStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::GitUI.Properties.Images.SaveAs;
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
            this.revertCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.RevertCommit;
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.revertCommitToolStripMenuItem_Click);
            // 
            // cherryPickThisCommitToolStripMenuItem
            // 
            this.cherryPickThisCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.CherryPick;
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
            // simplifyMergesContextMenuItem
            // 
            this.simplifyMergesContextMenuItem.Name = "simplifyMergesContextMenuItem";
            this.simplifyMergesContextMenuItem.Size = new System.Drawing.Size(339, 22);
            this.simplifyMergesContextMenuItem.Text = "Simplify Merges";
            this.simplifyMergesContextMenuItem.Click += new System.EventHandler(this.simplifyMergesContextMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(336, 6);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.CommitInfoTabPage);
            this.tabControl1.Controls.Add(this.DiffTab);
            this.tabControl1.Controls.Add(this.ViewTab);
            this.tabControl1.Controls.Add(this.BlameTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(748, 314);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // CommitInfoTabPage
            // 
            this.CommitInfoTabPage.Controls.Add(this.CommitDiff);
            this.CommitInfoTabPage.Location = new System.Drawing.Point(1, 21);
            this.CommitInfoTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.CommitInfoTabPage.Name = "CommitInfoTabPage";
            this.CommitInfoTabPage.Size = new System.Drawing.Size(744, 291);
            this.CommitInfoTabPage.TabIndex = 3;
            this.CommitInfoTabPage.Text = "Commit";
            // 
            // CommitDiff
            // 
            this.CommitDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitDiff.Location = new System.Drawing.Point(0, 0);
            this.CommitDiff.Margin = new System.Windows.Forms.Padding(0);
            this.CommitDiff.MinimumSize = new System.Drawing.Size(150, 148);
            this.CommitDiff.Name = "CommitDiff";
            this.CommitDiff.Size = new System.Drawing.Size(744, 291);
            this.CommitDiff.TabIndex = 0;
            // 
            // DiffTab
            // 
            this.DiffTab.Controls.Add(this.Diff);
            this.DiffTab.Location = new System.Drawing.Point(1, 21);
            this.DiffTab.Margin = new System.Windows.Forms.Padding(0);
            this.DiffTab.Name = "DiffTab";
            this.DiffTab.Size = new System.Drawing.Size(744, 291);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.Location = new System.Drawing.Point(0, 0);
            this.Diff.Margin = new System.Windows.Forms.Padding(0);
            this.Diff.Name = "Diff";
            this.Diff.Size = new System.Drawing.Size(744, 291);
            this.Diff.TabIndex = 0;
            // 
            // ViewTab
            // 
            this.ViewTab.Controls.Add(this.View);
            this.ViewTab.Location = new System.Drawing.Point(1, 21);
            this.ViewTab.Margin = new System.Windows.Forms.Padding(0);
            this.ViewTab.Name = "ViewTab";
            this.ViewTab.Size = new System.Drawing.Size(744, 291);
            this.ViewTab.TabIndex = 0;
            this.ViewTab.Text = "View";
            this.ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Location = new System.Drawing.Point(0, 0);
            this.View.Margin = new System.Windows.Forms.Padding(0);
            this.View.Name = "View";
            this.View.Size = new System.Drawing.Size(744, 291);
            this.View.TabIndex = 0;
            // 
            // BlameTab
            // 
            this.BlameTab.Controls.Add(this.Blame);
            this.BlameTab.Location = new System.Drawing.Point(1, 21);
            this.BlameTab.Margin = new System.Windows.Forms.Padding(0);
            this.BlameTab.Name = "BlameTab";
            this.BlameTab.Size = new System.Drawing.Size(744, 291);
            this.BlameTab.TabIndex = 2;
            this.BlameTab.Text = "Blame";
            this.BlameTab.UseVisualStyleBackColor = true;
            // 
            // Blame
            // 
            this.Blame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Blame.Location = new System.Drawing.Point(0, 0);
            this.Blame.Margin = new System.Windows.Forms.Padding(0);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(744, 291);
            this.Blame.TabIndex = 0;
            this.Blame.CommandClick += new System.EventHandler<GitUI.CommitInfo.CommandEventArgs>(this.Blame_CommandClick);
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
            this.toolStripBranchFilterDropDownButton.Image = global::GitUI.Properties.Images.EditFilter;
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
            this.toolStripRevisionFilterDropDownButton.Image = global::GitUI.Properties.Images.EditFilter;
            this.toolStripRevisionFilterDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRevisionFilterDropDownButton.Name = "toolStripRevisionFilterDropDownButton";
            this.toolStripRevisionFilterDropDownButton.Size = new System.Drawing.Size(29, 22);
            // 
            // ShowFirstParent
            // 
            this.ShowFirstParent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowFirstParent.Image = global::GitUI.Properties.Images.ShowFirstParent;
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
            this.toolStripSplitLoad.Image = global::GitUI.Properties.Images.ReloadRevisions;
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
            this.ShowFullHistory.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFullHistoryToolStripMenuItem,
            this.simplifyMergesToolStripMenuItem});
            this.ShowFullHistory.Image = global::GitUI.Properties.Images.FileHistory;
            this.ShowFullHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowFullHistory.Name = "ShowFullHistory";
            this.ShowFullHistory.Size = new System.Drawing.Size(29, 22);
            this.ShowFullHistory.ToolTipText = "Show Full History";
            // 
            // showFullHistoryToolStripMenuItem
            // 
            this.showFullHistoryToolStripMenuItem.Name = "showFullHistoryToolStripMenuItem";
            this.showFullHistoryToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.showFullHistoryToolStripMenuItem.Text = "Show Full History";
            this.showFullHistoryToolStripMenuItem.Click += new System.EventHandler(this.showFullHistoryToolStripMenuItem_Click);
            // 
            // simplifyMergesToolStripMenuItem
            // 
            this.simplifyMergesToolStripMenuItem.Name = "simplifyMergesToolStripMenuItem";
            this.simplifyMergesToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.simplifyMergesToolStripMenuItem.Text = "Simplify Merges";
            this.simplifyMergesToolStripMenuItem.Click += new System.EventHandler(this.simplifyMergesToolStripMenuItem_Click);
            // 
            // toolStripBlameOptions
            // 
            this.toolStripBlameOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBlameOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ignoreWhitespaceToolStripMenuItem,
            this.detectMoveAndCopyInThisFileToolStripMenuItem,
            this.detectMoveAndCopyInAllFilesToolStripMenuItem});
            this.toolStripBlameOptions.Image = global::GitUI.Properties.Images.Blame;
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
            this.DiffTab.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.BlameTab.ResumeLayout(false);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private GitUI.CommandsDialogs.FullBleedTabControl tabControl1;
        private System.Windows.Forms.TabPage CommitInfoTabPage;
        private UserControls.CommitDiff CommitDiff;
        private System.Windows.Forms.TabPage ViewTab;
        private System.Windows.Forms.TabPage DiffTab;
        private System.Windows.Forms.TabPage BlameTab;
        private FileViewer View;
        private FileViewer Diff;
        private RevisionGridControl FileChanges;
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
        private System.Windows.Forms.ToolStripMenuItem diffToolRemoteLocalStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitLoad;
        private System.Windows.Forms.ToolStripMenuItem loadHistoryOnShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem loadBlameOnShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem followFileHistoryRenamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton ShowFirstParent;
        private System.Windows.Forms.ToolStripDropDownButton toolStripBlameOptions;
        private System.Windows.Forms.ToolStripMenuItem ignoreWhitespaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectMoveAndCopyInThisFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectMoveAndCopyInAllFilesToolStripMenuItem;
        private GitUI.UserControls.RevisionGrid.CopyContextMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator separatorAfterCopySubmenu;
        private System.Windows.Forms.ToolStripDropDownButton ShowFullHistory;
        private System.Windows.Forms.ToolStripMenuItem showFullHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simplifyMergesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simplifyMergesContextMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}