using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormFileHistory
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            RevisionGrid = new GitUI.RevisionGridControl();
            FileHistoryContextMenu = new ContextMenuStrip(components);
            copyToClipboardToolStripMenuItem = new GitUI.UserControls.RevisionGrid.CopyContextMenuItem();
            separatorAfterCopySubmenu = new ToolStripSeparator();
            openWithDifftoolToolStripMenuItem = new ToolStripMenuItem();
            diffToolRemoteLocalStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            manipulateCommitToolStripMenuItem = new ToolStripMenuItem();
            revertCommitToolStripMenuItem = new ToolStripMenuItem();
            cherryPickThisCommitToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            followFileHistoryToolStripMenuItem = new ToolStripMenuItem();
            followFileHistoryRenamesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            tabControl1 = new GitUI.CommandsDialogs.FullBleedTabControl();
            CommitInfoTabPage = new TabPage();
            CommitDiff = new GitUI.UserControls.CommitDiff();
            DiffTab = new TabPage();
            Diff = new GitUI.Editor.FileViewer();
            ViewTab = new TabPage();
            View = new GitUI.Editor.FileViewer();
            BlameTab = new TabPage();
            Blame = new GitUI.Blame.BlameControl();
            ToolStripFilters = new GitUI.UserControls.FilterToolBar();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripSplitLoad = new ToolStripSplitButton();
            loadHistoryOnShowToolStripMenuItem = new ToolStripMenuItem();
            loadBlameOnShowToolStripMenuItem = new ToolStripMenuItem();
            ShowFullHistory = new ToolStripDropDownButton();
            showFullHistoryToolStripMenuItem = new ToolStripMenuItem();
            simplifyMergesToolStripMenuItem = new ToolStripMenuItem();
            toolStripBlameOptions = new ToolStripDropDownButton();
            blameSettingsToolStripMenuItem = new ToolStripMenuItem();
            ignoreWhitespaceToolStripMenuItem = new ToolStripMenuItem();
            detectMoveAndCopyInThisFileToolStripMenuItem = new ToolStripMenuItem();
            detectMoveAndCopyInAllFilesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            displaySettingsToolStripMenuItem = new ToolStripMenuItem();
            displayAuthorFirstToolStripMenuItem = new ToolStripMenuItem();
            showAuthorToolStripMenuItem = new ToolStripMenuItem();
            showAuthorDateToolStripMenuItem = new ToolStripMenuItem();
            showAuthorTimeToolStripMenuItem = new ToolStripMenuItem();
            showLineNumbersToolStripMenuItem = new ToolStripMenuItem();
            showOriginalFilePathToolStripMenuItem = new ToolStripMenuItem();
            showAuthorAvatarToolStripMenuItem = new ToolStripMenuItem();
            gitcommandLogToolStripMenuItem = new ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            FileHistoryContextMenu.SuspendLayout();
            tabControl1.SuspendLayout();
            CommitInfoTabPage.SuspendLayout();
            DiffTab.SuspendLayout();
            ViewTab.SuspendLayout();
            BlameTab.SuspendLayout();
            ToolStripFilters.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 25);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(RevisionGrid);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Size = new Size(748, 419);
            splitContainer1.SplitterDistance = 101;
            splitContainer1.TabIndex = 0;
            // 
            // FileChanges
            // 
            RevisionGrid.ContextMenuStrip = FileHistoryContextMenu;
            RevisionGrid.Dock = DockStyle.Fill;
            RevisionGrid.Location = new Point(0, 0);
            RevisionGrid.Name = "FileChanges";
            RevisionGrid.Size = new Size(748, 101);
            RevisionGrid.TabIndex = 2;
            RevisionGrid.Padding = new Padding(0, 22, 0, 0);
            RevisionGrid.DoubleClick += FileChangesDoubleClick;
            // 
            // FileHistoryContextMenu
            // 
            FileHistoryContextMenu.Items.AddRange(new ToolStripItem[] {
            copyToClipboardToolStripMenuItem,
            separatorAfterCopySubmenu,
            openWithDifftoolToolStripMenuItem,
            diffToolRemoteLocalStripMenuItem,
            saveAsToolStripMenuItem,
            toolStripSeparator2,
            manipulateCommitToolStripMenuItem,
            toolStripSeparator1,
            followFileHistoryToolStripMenuItem,
            followFileHistoryRenamesToolStripMenuItem,
            toolStripSeparator4});
            FileHistoryContextMenu.Name = "DiffContextMenu";
            FileHistoryContextMenu.Size = new Size(340, 226);
            FileHistoryContextMenu.Opening += FileHistoryContextMenuOpening;
            // 
            // copyToClipboardToolStripMenuItem
            // 
            copyToClipboardToolStripMenuItem.Image = Properties.Images.CopyToClipboard;
            copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            copyToClipboardToolStripMenuItem.Size = new Size(339, 22);
            copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            // 
            // separatorAfterCopySubmenu
            // 
            separatorAfterCopySubmenu.Name = "separatorAfterCopySubmenu";
            separatorAfterCopySubmenu.Size = new Size(336, 6);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            openWithDifftoolToolStripMenuItem.Image = Properties.Images.Diff;
            openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            openWithDifftoolToolStripMenuItem.ShortcutKeys = Keys.F3;
            openWithDifftoolToolStripMenuItem.Size = new Size(339, 22);
            openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            openWithDifftoolToolStripMenuItem.Click += OpenWithDifftoolToolStripMenuItem_Click;
            // 
            // diffToolRemoteLocalStripMenuItem
            // 
            diffToolRemoteLocalStripMenuItem.Name = "diffToolRemoteLocalStripMenuItem";
            diffToolRemoteLocalStripMenuItem.Size = new Size(339, 22);
            diffToolRemoteLocalStripMenuItem.Text = "Difftool selected < - > local";
            diffToolRemoteLocalStripMenuItem.Click += diffToolRemoteLocalStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Image = Properties.Images.SaveAs;
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(339, 22);
            saveAsToolStripMenuItem.Text = "Save as";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(336, 6);
            // 
            // manipulateCommitToolStripMenuItem
            // 
            manipulateCommitToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            revertCommitToolStripMenuItem,
            cherryPickThisCommitToolStripMenuItem});
            manipulateCommitToolStripMenuItem.Name = "manipulateCommitToolStripMenuItem";
            manipulateCommitToolStripMenuItem.Size = new Size(339, 22);
            manipulateCommitToolStripMenuItem.Text = "Manipulate commit";
            // 
            // revertCommitToolStripMenuItem
            // 
            revertCommitToolStripMenuItem.Image = Properties.Images.RevertCommit;
            revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            revertCommitToolStripMenuItem.Size = new Size(179, 22);
            revertCommitToolStripMenuItem.Text = "Revert commit";
            revertCommitToolStripMenuItem.Click += revertCommitToolStripMenuItem_Click;
            // 
            // cherryPickThisCommitToolStripMenuItem
            // 
            cherryPickThisCommitToolStripMenuItem.Image = Properties.Images.CherryPick;
            cherryPickThisCommitToolStripMenuItem.Name = "cherryPickThisCommitToolStripMenuItem";
            cherryPickThisCommitToolStripMenuItem.Size = new Size(179, 22);
            cherryPickThisCommitToolStripMenuItem.Text = "Cherry pick commit";
            cherryPickThisCommitToolStripMenuItem.Click += cherryPickThisCommitToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(336, 6);
            // 
            // followFileHistoryToolStripMenuItem
            // 
            followFileHistoryToolStripMenuItem.Name = "followFileHistoryToolStripMenuItem";
            followFileHistoryToolStripMenuItem.Size = new Size(339, 22);
            followFileHistoryToolStripMenuItem.Text = "Detect and follow renames";
            followFileHistoryToolStripMenuItem.Click += followFileHistoryToolStripMenuItem_Click;
            // 
            // followFileHistoryRenamesToolStripMenuItem
            // 
            followFileHistoryRenamesToolStripMenuItem.Name = "followFileHistoryRenamesToolStripMenuItem";
            followFileHistoryRenamesToolStripMenuItem.Size = new Size(339, 22);
            followFileHistoryRenamesToolStripMenuItem.Text = "Detect and follow - exact renames and copies only";
            followFileHistoryRenamesToolStripMenuItem.Click += followFileHistoryRenamesToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(336, 6);
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(CommitInfoTabPage);
            tabControl1.Controls.Add(DiffTab);
            tabControl1.Controls.Add(ViewTab);
            tabControl1.Controls.Add(BlameTab);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(0);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(0, 0);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(748, 314);
            tabControl1.TabIndex = 0;
            tabControl1.SelectedIndexChanged += TabControl1SelectedIndexChanged;
            // 
            // CommitInfoTabPage
            // 
            CommitInfoTabPage.Controls.Add(CommitDiff);
            CommitInfoTabPage.Location = new Point(1, 21);
            CommitInfoTabPage.Margin = new Padding(0);
            CommitInfoTabPage.Name = "CommitInfoTabPage";
            CommitInfoTabPage.Size = new Size(744, 291);
            CommitInfoTabPage.TabIndex = 3;
            CommitInfoTabPage.Text = "Commit";
            // 
            // CommitDiff
            // 
            CommitDiff.Dock = DockStyle.Fill;
            CommitDiff.Location = new Point(0, 0);
            CommitDiff.Margin = new Padding(0);
            CommitDiff.MinimumSize = new Size(150, 148);
            CommitDiff.Name = "CommitDiff";
            CommitDiff.Size = new Size(744, 291);
            CommitDiff.TabIndex = 0;
            // 
            // DiffTab
            // 
            DiffTab.Controls.Add(Diff);
            DiffTab.Location = new Point(1, 21);
            DiffTab.Margin = new Padding(0);
            DiffTab.Name = "DiffTab";
            DiffTab.Size = new Size(744, 291);
            DiffTab.TabIndex = 1;
            DiffTab.Text = "Diff";
            DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            Diff.Dock = DockStyle.Fill;
            Diff.Location = new Point(0, 0);
            Diff.Margin = new Padding(0);
            Diff.Name = "Diff";
            Diff.Size = new Size(744, 291);
            Diff.TabIndex = 0;
            // 
            // ViewTab
            // 
            ViewTab.Controls.Add(View);
            ViewTab.Location = new Point(1, 21);
            ViewTab.Margin = new Padding(0);
            ViewTab.Name = "ViewTab";
            ViewTab.Size = new Size(744, 291);
            ViewTab.TabIndex = 0;
            ViewTab.Text = "View";
            ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            View.Dock = DockStyle.Fill;
            View.Location = new Point(0, 0);
            View.Margin = new Padding(0);
            View.Name = "View";
            View.Size = new Size(744, 291);
            View.TabIndex = 0;
            // 
            // BlameTab
            // 
            BlameTab.Controls.Add(Blame);
            BlameTab.Location = new Point(1, 21);
            BlameTab.Margin = new Padding(0);
            BlameTab.Name = "BlameTab";
            BlameTab.Size = new Size(744, 291);
            BlameTab.TabIndex = 2;
            BlameTab.Text = "Blame";
            BlameTab.UseVisualStyleBackColor = true;
            // 
            // Blame
            // 
            Blame.Dock = DockStyle.Fill;
            Blame.Location = new Point(0, 0);
            Blame.Margin = new Padding(0);
            Blame.Name = "Blame";
            Blame.Size = new Size(744, 291);
            Blame.TabIndex = 0;
            Blame.CommandClick += new System.EventHandler<ResourceManager.CommandEventArgs>(Blame_CommandClick);
            // ToolStripFilters
            // 
            ToolStripFilters.Dock = DockStyle.Top;
            ToolStripFilters.Items.AddRange(new ToolStripItem[] {
            toolStripSeparator3,
            toolStripSplitLoad,
            ShowFullHistory,
            toolStripBlameOptions,
            gitcommandLogToolStripMenuItem});
            ToolStripFilters.Location = new Point(0, 0);
            ToolStripFilters.Size = new Size(748, 25);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // toolStripSplitLoad
            // 
            toolStripSplitLoad.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripSplitLoad.DropDownItems.AddRange(new ToolStripItem[] {
            loadHistoryOnShowToolStripMenuItem,
            loadBlameOnShowToolStripMenuItem});
            toolStripSplitLoad.Image = Properties.Images.ReloadRevisions;
            toolStripSplitLoad.ImageTransparentColor = Color.Magenta;
            toolStripSplitLoad.Name = "toolStripSplitLoad";
            toolStripSplitLoad.Size = new Size(32, 22);
            toolStripSplitLoad.ToolTipText = "Load file history";
            toolStripSplitLoad.ButtonClick += toolStripSplitLoad_ButtonClick;
            // 
            // loadHistoryOnShowToolStripMenuItem
            // 
            loadHistoryOnShowToolStripMenuItem.Checked = true;
            loadHistoryOnShowToolStripMenuItem.CheckState = CheckState.Checked;
            loadHistoryOnShowToolStripMenuItem.Name = "loadHistoryOnShowToolStripMenuItem";
            loadHistoryOnShowToolStripMenuItem.Size = new Size(187, 22);
            loadHistoryOnShowToolStripMenuItem.Text = "Load history on show";
            loadHistoryOnShowToolStripMenuItem.Click += loadHistoryOnShowToolStripMenuItem_Click;
            // 
            // loadBlameOnShowToolStripMenuItem
            // 
            loadBlameOnShowToolStripMenuItem.Checked = true;
            loadBlameOnShowToolStripMenuItem.CheckState = CheckState.Checked;
            loadBlameOnShowToolStripMenuItem.Name = "loadBlameOnShowToolStripMenuItem";
            loadBlameOnShowToolStripMenuItem.Size = new Size(187, 22);
            loadBlameOnShowToolStripMenuItem.Text = "Load blame on show";
            loadBlameOnShowToolStripMenuItem.Click += loadBlameOnShowToolStripMenuItem_Click;
            // 
            // ShowFullHistory
            // 
            ShowFullHistory.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ShowFullHistory.DropDownItems.AddRange(new ToolStripItem[] {
            showFullHistoryToolStripMenuItem,
            simplifyMergesToolStripMenuItem});
            ShowFullHistory.Image = Properties.Images.FileHistory;
            ShowFullHistory.ImageTransparentColor = Color.Magenta;
            ShowFullHistory.Name = "ShowFullHistory";
            ShowFullHistory.Size = new Size(29, 22);
            ShowFullHistory.ToolTipText = "Show Full History";
            // 
            // showFullHistoryToolStripMenuItem
            // 
            showFullHistoryToolStripMenuItem.Name = "showFullHistoryToolStripMenuItem";
            showFullHistoryToolStripMenuItem.Size = new Size(166, 22);
            showFullHistoryToolStripMenuItem.Text = "Show full history";
            showFullHistoryToolStripMenuItem.Click += showFullHistoryToolStripMenuItem_Click;
            // 
            // simplifyMergesToolStripMenuItem
            // 
            simplifyMergesToolStripMenuItem.Name = "simplifyMergesToolStripMenuItem";
            simplifyMergesToolStripMenuItem.Size = new Size(166, 22);
            simplifyMergesToolStripMenuItem.Text = "Simplify merges";
            simplifyMergesToolStripMenuItem.Click += simplifyMergesToolStripMenuItem_Click;
            // 
            // toolStripBlameOptions
            // 
            toolStripBlameOptions.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripBlameOptions.DropDownItems.AddRange(new ToolStripItem[] {
            blameSettingsToolStripMenuItem,
            ignoreWhitespaceToolStripMenuItem,
            detectMoveAndCopyInThisFileToolStripMenuItem,
            detectMoveAndCopyInAllFilesToolStripMenuItem,
            toolStripSeparator5,
            displaySettingsToolStripMenuItem,
            displayAuthorFirstToolStripMenuItem,
            showAuthorAvatarToolStripMenuItem,
            showAuthorToolStripMenuItem,
            showAuthorDateToolStripMenuItem,
            showAuthorTimeToolStripMenuItem,
            showLineNumbersToolStripMenuItem,
            showOriginalFilePathToolStripMenuItem});
            toolStripBlameOptions.Image = Properties.Images.Blame;
            toolStripBlameOptions.ImageTransparentColor = Color.Magenta;
            toolStripBlameOptions.Name = "toolStripBlameOptions";
            toolStripBlameOptions.Size = new Size(29, 22);
            toolStripBlameOptions.Text = "Blame options";
            toolStripBlameOptions.ToolTipText = "Blame options";
            // 
            // blameSettingsToolStripMenuItem
            // 
            blameSettingsToolStripMenuItem.Enabled = false;
            blameSettingsToolStripMenuItem.Name = "blameSettingsToolStripMenuItem";
            blameSettingsToolStripMenuItem.Size = new Size(247, 22);
            blameSettingsToolStripMenuItem.Text = "Blame settings:";
            // 
            // ignoreWhitespaceToolStripMenuItem
            // 
            ignoreWhitespaceToolStripMenuItem.Name = "ignoreWhitespaceToolStripMenuItem";
            ignoreWhitespaceToolStripMenuItem.Size = new Size(247, 22);
            ignoreWhitespaceToolStripMenuItem.Text = "Ignore whitespace";
            ignoreWhitespaceToolStripMenuItem.Click += ignoreWhitespaceToolStripMenuItem_Click;
            // 
            // detectMoveAndCopyInThisFileToolStripMenuItem
            // 
            detectMoveAndCopyInThisFileToolStripMenuItem.Name = "detectMoveAndCopyInThisFileToolStripMenuItem";
            detectMoveAndCopyInThisFileToolStripMenuItem.Size = new Size(247, 22);
            detectMoveAndCopyInThisFileToolStripMenuItem.Text = "Detect move and copy in this file";
            detectMoveAndCopyInThisFileToolStripMenuItem.Click += detectMoveAndCopyInThisFileToolStripMenuItem_Click;
            // 
            // detectMoveAndCopyInAllFilesToolStripMenuItem
            // 
            detectMoveAndCopyInAllFilesToolStripMenuItem.Name = "detectMoveAndCopyInAllFilesToolStripMenuItem";
            detectMoveAndCopyInAllFilesToolStripMenuItem.Size = new Size(247, 22);
            detectMoveAndCopyInAllFilesToolStripMenuItem.Text = "Detect move and copy in all files";
            detectMoveAndCopyInAllFilesToolStripMenuItem.Click += detectMoveAndCopyInAllFilesToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(244, 6);
            // 
            // displaySettingsToolStripMenuItem
            // 
            displaySettingsToolStripMenuItem.Enabled = false;
            displaySettingsToolStripMenuItem.Name = "displaySettingsToolStripMenuItem";
            displaySettingsToolStripMenuItem.Size = new Size(247, 22);
            displaySettingsToolStripMenuItem.Text = "Display result settings:";
            // 
            // displayAuthorFirstToolStripMenuItem
            // 
            displayAuthorFirstToolStripMenuItem.Name = "displayAuthorFirstToolStripMenuItem";
            displayAuthorFirstToolStripMenuItem.Size = new Size(247, 22);
            displayAuthorFirstToolStripMenuItem.Text = "Display author first";
            displayAuthorFirstToolStripMenuItem.Click += displayAuthorFirstToolStripMenuItem_Click;
            // 
            // showAuthorToolStripMenuItem
            // 
            showAuthorToolStripMenuItem.Name = "showAuthorToolStripMenuItem";
            showAuthorToolStripMenuItem.Size = new Size(247, 22);
            showAuthorToolStripMenuItem.Text = "Show author";
            showAuthorToolStripMenuItem.Click += showAuthorToolStripMenuItem_Click;
            // 
            // showAuthorDateToolStripMenuItem
            // 
            showAuthorDateToolStripMenuItem.Name = "showAuthorDateToolStripMenuItem";
            showAuthorDateToolStripMenuItem.Size = new Size(247, 22);
            showAuthorDateToolStripMenuItem.Text = "Show author date";
            showAuthorDateToolStripMenuItem.Click += showAuthorDateToolStripMenuItem_Click;
            // 
            // showAuthorTimeToolStripMenuItem
            // 
            showAuthorTimeToolStripMenuItem.Name = "showAuthorTimeToolStripMenuItem";
            showAuthorTimeToolStripMenuItem.Size = new Size(247, 22);
            showAuthorTimeToolStripMenuItem.Text = "Show author time";
            showAuthorTimeToolStripMenuItem.Click += showAuthorTimeToolStripMenuItem_Click;
            // 
            // showLineNumbersToolStripMenuItem
            // 
            showLineNumbersToolStripMenuItem.Name = "showLineNumbersToolStripMenuItem";
            showLineNumbersToolStripMenuItem.Size = new Size(247, 22);
            showLineNumbersToolStripMenuItem.Text = "Show line numbers";
            showLineNumbersToolStripMenuItem.Click += showLineNumbersToolStripMenuItem_Click;
            // 
            // showOriginalFilePathToolStripMenuItem
            // 
            showOriginalFilePathToolStripMenuItem.Name = "showOriginalFilePathToolStripMenuItem";
            showOriginalFilePathToolStripMenuItem.Size = new Size(247, 22);
            showOriginalFilePathToolStripMenuItem.Text = "Show original file path";
            showOriginalFilePathToolStripMenuItem.Click += showOriginalFilePathToolStripMenuItem_Click;
            // 
            // showAuthorAvatarToolStripMenuItem
            // 
            showAuthorAvatarToolStripMenuItem.Name = "showAuthorAvatarToolStripMenuItem";
            showAuthorAvatarToolStripMenuItem.Size = new Size(247, 22);
            showAuthorAvatarToolStripMenuItem.Text = "Show author avatar";
            showAuthorAvatarToolStripMenuItem.Click += showAuthorAvatarToolStripMenuItem_Click;
            // 
            // gitcommandLogToolStripMenuItem
            // 
            gitcommandLogToolStripMenuItem.Image = Properties.Images.GitCommandLog;
            gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            gitcommandLogToolStripMenuItem.Size = new Size(217, 22);
            gitcommandLogToolStripMenuItem.ToolTipText = "Git command log";
            gitcommandLogToolStripMenuItem.Click += GitcommandLogToolStripMenuItemClick;
            // 
            // FormFileHistory
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(748, 444);
            Controls.Add(ToolStripFilters);
            Controls.Add(splitContainer1);
            MinimumSize = new Size(299, 198);
            Name = "FormFileHistory";
            StartPosition = FormStartPosition.CenterParent;
            Text = "File History";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            FileHistoryContextMenu.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            CommitInfoTabPage.ResumeLayout(false);
            DiffTab.ResumeLayout(false);
            ViewTab.ResumeLayout(false);
            BlameTab.ResumeLayout(false);
            ToolStripFilters.ResumeLayout(false);
            ToolStripFilters.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private SplitContainer splitContainer1;
        private GitUI.CommandsDialogs.FullBleedTabControl tabControl1;
        private TabPage CommitInfoTabPage;
        private UserControls.CommitDiff CommitDiff;
        private TabPage ViewTab;
        private TabPage DiffTab;
        private TabPage BlameTab;
        private FileViewer View;
        private FileViewer Diff;
        private RevisionGridControl RevisionGrid;
        private ContextMenuStrip FileHistoryContextMenu;
        private ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private Blame.BlameControl Blame;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem followFileHistoryToolStripMenuItem;
        private ToolStripMenuItem manipulateCommitToolStripMenuItem;
        private ToolStripMenuItem cherryPickThisCommitToolStripMenuItem;
        private ToolStripMenuItem revertCommitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private GitUI.UserControls.FilterToolBar ToolStripFilters;
        private ToolStripMenuItem diffToolRemoteLocalStripMenuItem;
        private ToolStripSplitButton toolStripSplitLoad;
        private ToolStripMenuItem loadHistoryOnShowToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem loadBlameOnShowToolStripMenuItem;
        private ToolStripMenuItem followFileHistoryRenamesToolStripMenuItem;
        private ToolStripDropDownButton toolStripBlameOptions;
        private ToolStripMenuItem ignoreWhitespaceToolStripMenuItem;
        private ToolStripMenuItem detectMoveAndCopyInThisFileToolStripMenuItem;
        private ToolStripMenuItem detectMoveAndCopyInAllFilesToolStripMenuItem;
        private GitUI.UserControls.RevisionGrid.CopyContextMenuItem copyToClipboardToolStripMenuItem;
        private ToolStripSeparator separatorAfterCopySubmenu;
        private ToolStripDropDownButton ShowFullHistory;
        private ToolStripMenuItem showFullHistoryToolStripMenuItem;
        private ToolStripMenuItem simplifyMergesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem displayAuthorFirstToolStripMenuItem;
        private ToolStripMenuItem showAuthorDateToolStripMenuItem;
        private ToolStripMenuItem showAuthorTimeToolStripMenuItem;
        private ToolStripMenuItem showLineNumbersToolStripMenuItem;
        private ToolStripMenuItem blameSettingsToolStripMenuItem;
        private ToolStripMenuItem displaySettingsToolStripMenuItem;
        private ToolStripMenuItem showAuthorToolStripMenuItem;
        private ToolStripMenuItem showOriginalFilePathToolStripMenuItem;
        private ToolStripMenuItem showAuthorAvatarToolStripMenuItem;
        private ToolStripButton gitcommandLogToolStripMenuItem;
    }
}
