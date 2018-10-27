using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    partial class FormDiff
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
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.settingsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnSwap = new System.Windows.Forms.Button();
            this.ckCompareToMergeBase = new System.Windows.Forms.CheckBox();
            this.btnCompareDirectoriesWithDiffTool = new System.Windows.Forms.Button();
            this.diffOptionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.baseCommitGroup = new System.Windows.Forms.GroupBox();
            this.baseCommitPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblBaseCommit = new System.Windows.Forms.Label();
            this.btnAnotherBaseBranch = new System.Windows.Forms.Button();
            this.btnAnotherBaseCommit = new System.Windows.Forms.Button();
            this.headCommitGroup = new System.Windows.Forms.GroupBox();
            this.headCommitPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblHeadCommit = new System.Windows.Forms.Label();
            this.btnAnotherHeadBranch = new System.Windows.Forms.Button();
            this.btnAnotherHeadCommit = new System.Windows.Forms.Button();
            this.diffShowInFileTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstToSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstParentToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedParentToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFilenameToClipboardToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator33 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryDiffToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findInDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.settingsLayoutPanel.SuspendLayout();
            this.baseCommitGroup.SuspendLayout();
            this.baseCommitPanel.SuspendLayout();
            this.headCommitGroup.SuspendLayout();
            this.headCommitPanel.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 1;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.Controls.Add(this.splitContainer1, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.settingsLayoutPanel, 0, 0);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(1042, 685);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 80);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.DiffFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.DiffText);
            this.splitContainer1.Size = new System.Drawing.Size(1036, 602);
            this.splitContainer1.SplitterDistance = 345;
            this.splitContainer1.TabIndex = 1;
            // 
            // DiffFiles
            // 
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.FilterVisible = false;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(345, 602);
            this.DiffFiles.TabIndex = 0;
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DiffText.Name = "DiffText";
            this.DiffText.ShowLineNumbers = false;
            this.DiffText.Size = new System.Drawing.Size(687, 602);
            this.DiffText.TabIndex = 0;
            // 
            // settingsLayoutPanel
            // 
            this.settingsLayoutPanel.AutoSize = true;
            this.settingsLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.settingsLayoutPanel.ColumnCount = 3;
            this.settingsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.settingsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.settingsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.settingsLayoutPanel.Controls.Add(this.btnSwap, 1, 0);
            this.settingsLayoutPanel.Controls.Add(this.diffOptionsPanel, 0, 1);
            this.settingsLayoutPanel.Controls.Add(this.baseCommitGroup, 0, 0);
            this.settingsLayoutPanel.Controls.Add(this.headCommitGroup, 2, 0);
            this.settingsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.settingsLayoutPanel.Name = "settingsLayoutPanel";
            this.settingsLayoutPanel.RowCount = 2;
            this.settingsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.settingsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.settingsLayoutPanel.Size = new System.Drawing.Size(1036, 71);
            this.settingsLayoutPanel.TabIndex = 3;
            // 
            // btnSwap
            // 
            this.btnSwap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSwap.AutoSize = true;
            this.btnSwap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSwap.Image = global::GitUI.Properties.Images.Renamed;
            this.btnSwap.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSwap.Location = new System.Drawing.Point(507, 20);
            this.btnSwap.Margin = new System.Windows.Forms.Padding(10, 15, 10, 0);
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(22, 22);
            this.btnSwap.TabIndex = 6;
            this.btnSwap.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSwap.UseVisualStyleBackColor = true;
            this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
            // 
            // ckCompareToMergeBase
            // 
            this.ckCompareToMergeBase.Anchor = AnchorStyles.Left;
            this.ckCompareToMergeBase.AutoSize = true;
            this.ckCompareToMergeBase.Name = "ckCompareToMergeBase";
            this.ckCompareToMergeBase.TabIndex = 9;
            this.ckCompareToMergeBase.Text = "Compare to merge &base";
            this.ckCompareToMergeBase.UseMnemonic = true;
            this.ckCompareToMergeBase.UseVisualStyleBackColor = true;
            this.ckCompareToMergeBase.CheckedChanged += new System.EventHandler(this.ckCompareToMergeBase_CheckedChanged);
            // 
            // btnCompareDirectoriesWithDiffTool
            // 
            this.btnCompareDirectoriesWithDiffTool.Anchor = AnchorStyles.Left;
            this.btnCompareDirectoriesWithDiffTool.AutoSize = true;
            this.btnCompareDirectoriesWithDiffTool.Name = "btnCompareDirectoriesWithDiffTool";
            this.btnCompareDirectoriesWithDiffTool.Margin = new System.Windows.Forms.Padding(0);
            this.btnCompareDirectoriesWithDiffTool.Size = new System.Drawing.Size(141, 17);
            this.btnCompareDirectoriesWithDiffTool.TabIndex = 10;
            this.btnCompareDirectoriesWithDiffTool.Text = "Open diff using &directory diff tool";
            this.btnCompareDirectoriesWithDiffTool.UseMnemonic = true;
            this.btnCompareDirectoriesWithDiffTool.UseVisualStyleBackColor = true;
            this.btnCompareDirectoriesWithDiffTool.Click += new System.EventHandler(this.btnCompareDirectoriesWithDiffTool_Clicked);
            this.btnCompareDirectoriesWithDiffTool.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // 
            // diffOptionsPanel
            // 
            this.diffOptionsPanel.AutoSize = true;
            this.diffOptionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.diffOptionsPanel.Controls.Add(this.ckCompareToMergeBase);
            this.diffOptionsPanel.Controls.Add(this.btnCompareDirectoriesWithDiffTool);
            this.diffOptionsPanel.Margin = new System.Windows.Forms.Padding(3);
            this.diffOptionsPanel.Name = "diffOptionsPanel";
            this.diffOptionsPanel.Size = new System.Drawing.Size(491, 23);
            this.diffOptionsPanel.TabIndex = 14;
            // 
            // baseCommitGroup
            // 
            this.baseCommitGroup.AutoSize = true;
            this.baseCommitGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.baseCommitGroup.Controls.Add(this.baseCommitPanel);
            this.baseCommitGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseCommitGroup.Location = new System.Drawing.Point(0, 0);
            this.baseCommitGroup.Margin = new System.Windows.Forms.Padding(0);
            this.baseCommitGroup.Name = "baseCommitGroup";
            this.baseCommitGroup.Size = new System.Drawing.Size(497, 48);
            this.baseCommitGroup.TabIndex = 16;
            this.baseCommitGroup.TabStop = false;
            this.baseCommitGroup.Text = "BASE";
            // 
            // baseCommitPanel
            // 
            this.baseCommitPanel.AutoSize = true;
            this.baseCommitPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.baseCommitPanel.Controls.Add(this.lblBaseCommit);
            this.baseCommitPanel.Controls.Add(this.btnAnotherBaseBranch);
            this.baseCommitPanel.Controls.Add(this.btnAnotherBaseCommit);
            this.baseCommitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseCommitPanel.Location = new System.Drawing.Point(3, 17);
            this.baseCommitPanel.Margin = new System.Windows.Forms.Padding(0);
            this.baseCommitPanel.Name = "baseCommitPanel";
            this.baseCommitPanel.Size = new System.Drawing.Size(491, 28);
            this.baseCommitPanel.TabIndex = 14;
            // 
            // lblBaseCommit
            // 
            this.lblBaseCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBaseCommit.AutoSize = true;
            this.lblBaseCommit.Location = new System.Drawing.Point(3, 7);
            this.lblBaseCommit.MinimumSize = new System.Drawing.Size(200, 0);
            this.lblBaseCommit.Name = "lblBaseCommit";
            this.lblBaseCommit.Size = new System.Drawing.Size(200, 13);
            this.lblBaseCommit.TabIndex = 14;
            this.lblBaseCommit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAnotherBaseBranch
            // 
            this.btnAnotherBaseBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherBaseBranch.AutoSize = true;
            this.btnAnotherBaseBranch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherBaseBranch.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.btnAnotherBaseBranch.Location = new System.Drawing.Point(209, 3);
            this.btnAnotherBaseBranch.Name = "btnAnotherBaseBranch";
            this.btnAnotherBaseBranch.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherBaseBranch.TabIndex = 7;
            this.btnAnotherBaseBranch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherBaseBranch.UseVisualStyleBackColor = true;
            this.btnAnotherBaseBranch.Click += new System.EventHandler(this.btnPickAnotherBranch_Click);
            // 
            // btnAnotherBaseCommit
            // 
            this.btnAnotherBaseCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherBaseCommit.AutoSize = true;
            this.btnAnotherBaseCommit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherBaseCommit.Image = global::GitUI.Properties.Images.SelectRevision;
            this.btnAnotherBaseCommit.Location = new System.Drawing.Point(237, 3);
            this.btnAnotherBaseCommit.Name = "btnAnotherBaseCommit";
            this.btnAnotherBaseCommit.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherBaseCommit.TabIndex = 10;
            this.btnAnotherBaseCommit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherBaseCommit.UseVisualStyleBackColor = true;
            this.btnAnotherBaseCommit.Click += new System.EventHandler(this.btnAnotherCommit_Click);
            // 
            // headCommitGroup
            // 
            this.headCommitGroup.AutoSize = true;
            this.headCommitGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.headCommitGroup.Controls.Add(this.headCommitPanel);
            this.headCommitGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headCommitGroup.Location = new System.Drawing.Point(539, 0);
            this.headCommitGroup.Margin = new System.Windows.Forms.Padding(0);
            this.headCommitGroup.Name = "headCommitGroup";
            this.headCommitGroup.Size = new System.Drawing.Size(497, 48);
            this.headCommitGroup.TabIndex = 17;
            this.headCommitGroup.TabStop = false;
            this.headCommitGroup.Text = "Compare";
            // 
            // headCommitPanel
            // 
            this.headCommitPanel.AutoSize = true;
            this.headCommitPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.headCommitPanel.Controls.Add(this.lblHeadCommit);
            this.headCommitPanel.Controls.Add(this.btnAnotherHeadBranch);
            this.headCommitPanel.Controls.Add(this.btnAnotherHeadCommit);
            this.headCommitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headCommitPanel.Location = new System.Drawing.Point(3, 17);
            this.headCommitPanel.Margin = new System.Windows.Forms.Padding(0);
            this.headCommitPanel.Name = "headCommitPanel";
            this.headCommitPanel.Size = new System.Drawing.Size(491, 28);
            this.headCommitPanel.TabIndex = 15;
            // 
            // lblHeadCommit
            // 
            this.lblHeadCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblHeadCommit.AutoSize = true;
            this.lblHeadCommit.Location = new System.Drawing.Point(3, 7);
            this.lblHeadCommit.MinimumSize = new System.Drawing.Size(200, 0);
            this.lblHeadCommit.Name = "lblHeadCommit";
            this.lblHeadCommit.Size = new System.Drawing.Size(200, 13);
            this.lblHeadCommit.TabIndex = 1;
            this.lblHeadCommit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAnotherHeadBranch
            // 
            this.btnAnotherHeadBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherHeadBranch.AutoSize = true;
            this.btnAnotherHeadBranch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherHeadBranch.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.btnAnotherHeadBranch.Location = new System.Drawing.Point(209, 3);
            this.btnAnotherHeadBranch.Name = "btnAnotherHeadBranch";
            this.btnAnotherHeadBranch.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherHeadBranch.TabIndex = 16;
            this.btnAnotherHeadBranch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherHeadBranch.UseVisualStyleBackColor = true;
            this.btnAnotherHeadBranch.Click += new System.EventHandler(this.btnAnotherHeadBranch_Click);
            // 
            // btnAnotherHeadCommit
            // 
            this.btnAnotherHeadCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherHeadCommit.AutoSize = true;
            this.btnAnotherHeadCommit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherHeadCommit.Image = global::GitUI.Properties.Images.SelectRevision;
            this.btnAnotherHeadCommit.Location = new System.Drawing.Point(237, 3);
            this.btnAnotherHeadCommit.Name = "btnAnotherHeadCommit";
            this.btnAnotherHeadCommit.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherHeadCommit.TabIndex = 17;
            this.btnAnotherHeadCommit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherHeadCommit.UseVisualStyleBackColor = true;
            this.btnAnotherHeadCommit.Click += new System.EventHandler(this.btnAnotherHeadCommit_Click);
            // 
            // diffShowInFileTreeToolStripMenuItem
            // 
            this.diffShowInFileTreeToolStripMenuItem.Name = "diffShowInFileTreeToolStripMenuItem";
            this.diffShowInFileTreeToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.diffShowInFileTreeToolStripMenuItem.Text = "Show in File tree";
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.toolStripSeparator32,
            this.copyFilenameToClipboardToolStripMenuItem1,
            this.openContainingFolderToolStripMenuItem,
            this.toolStripSeparator33,
            this.fileHistoryDiffToolstripMenuItem,
            this.blameToolStripMenuItem,
            this.findInDiffToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(211, 148);
            this.DiffContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.diffContextToolStripMenuItem_Opening);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.firstToSelectedToolStripMenuItem,
            this.firstToLocalToolStripMenuItem,
            this.selectedToLocalToolStripMenuItem,
            this.firstParentToLocalToolStripMenuItem,
            this.selectedParentToLocalToolStripMenuItem});
            this.openWithDifftoolToolStripMenuItem.Image = global::GitUI.Properties.Images.Diff;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.DropDownOpening += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_DropDownOpening);
            // 
            // firstToSelectedToolStripMenuItem
            // 
            this.firstToSelectedToolStripMenuItem.Name = "firstToSelectedToolStripMenuItem";
            this.firstToSelectedToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.firstToSelectedToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.firstToSelectedToolStripMenuItem.Text = "BASE -> Compare";
            this.firstToSelectedToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // firstToLocalToolStripMenuItem
            // 
            this.firstToLocalToolStripMenuItem.Name = "firstToLocalToolStripMenuItem";
            this.firstToLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.firstToLocalToolStripMenuItem.Text = "BASE -> Working directory";
            this.firstToLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // selectedToLocalToolStripMenuItem
            // 
            this.selectedToLocalToolStripMenuItem.Name = "selectedToLocalToolStripMenuItem";
            this.selectedToLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.selectedToLocalToolStripMenuItem.Text = "Compare -> Working directory";
            this.selectedToLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // firstParentToLocalToolStripMenuItem
            // 
            this.firstParentToLocalToolStripMenuItem.Name = "firstParentToLocalToolStripMenuItem";
            this.firstParentToLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.firstParentToLocalToolStripMenuItem.Text = "Parent to BASE -> Working directory";
            this.firstParentToLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // selectedParentToLocalToolStripMenuItem
            // 
            this.selectedParentToLocalToolStripMenuItem.Name = "selectedParentToLocalToolStripMenuItem";
            this.selectedParentToLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.selectedParentToLocalToolStripMenuItem.Text = "Parent to Compare -> Working directory";
            this.selectedParentToLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(207, 6);
            // 
            // copyFilenameToClipboardToolStripMenuItem1
            // 
            this.copyFilenameToClipboardToolStripMenuItem1.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyFilenameToClipboardToolStripMenuItem1.Name = "copyFilenameToClipboardToolStripMenuItem1";
            this.copyFilenameToClipboardToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.copyFilenameToClipboardToolStripMenuItem1.Text = "Copy full path(s)";
            this.copyFilenameToClipboardToolStripMenuItem1.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem1_Click);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Show in folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(207, 6);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            this.fileHistoryDiffToolstripMenuItem.Image = global::GitUI.Properties.Images.FileHistory;
            this.fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            this.fileHistoryDiffToolstripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.fileHistoryDiffToolstripMenuItem.Text = "File history";
            this.fileHistoryDiffToolstripMenuItem.Click += new System.EventHandler(this.fileHistoryDiffToolStripMenuItem_Click);
            // 
            // blameToolStripMenuItem
            // 
            this.blameToolStripMenuItem.Image = global::GitUI.Properties.Images.Blame;
            this.blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            this.blameToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.blameToolStripMenuItem.Text = "Blame";
            this.blameToolStripMenuItem.Click += new System.EventHandler(this.blameToolStripMenuItem_Click);
            // 
            // findInDiffToolStripMenuItem
            // 
            this.findInDiffToolStripMenuItem.Image = global::GitUI.Properties.Images.Preview;
            this.findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            this.findInDiffToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findInDiffToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.findInDiffToolStripMenuItem.Text = "Find";
            this.findInDiffToolStripMenuItem.Click += new System.EventHandler(this.findInDiffToolStripMenuItem_Click);
            // 
            // FormDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1042, 685);
            this.Controls.Add(this.mainLayoutPanel);
            this.DoubleBuffered = true;
            this.Name = "FormDiff";
            this.Text = "Diff";
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.settingsLayoutPanel.ResumeLayout(false);
            this.settingsLayoutPanel.PerformLayout();
            this.diffOptionsPanel.ResumeLayout(false);
            this.diffOptionsPanel.PerformLayout();
            this.baseCommitGroup.ResumeLayout(false);
            this.baseCommitGroup.PerformLayout();
            this.baseCommitPanel.ResumeLayout(false);
            this.baseCommitPanel.PerformLayout();
            this.headCommitGroup.ResumeLayout(false);
            this.headCommitGroup.PerformLayout();
            this.headCommitPanel.ResumeLayout(false);
            this.headCommitPanel.PerformLayout();
            this.DiffContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
        private System.Windows.Forms.TableLayoutPanel settingsLayoutPanel;
        private System.Windows.Forms.Label lblHeadCommit;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.Button btnAnotherBaseBranch;
        private System.Windows.Forms.ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstToSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstToLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedToLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstParentToLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedParentToLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator32;
        private System.Windows.Forms.ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator33;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findInDiffToolStripMenuItem;
        private System.Windows.Forms.Button btnAnotherBaseCommit;
        private System.Windows.Forms.FlowLayoutPanel baseCommitPanel;
        private System.Windows.Forms.Label lblBaseCommit;
        private System.Windows.Forms.FlowLayoutPanel headCommitPanel;
        private System.Windows.Forms.Button btnAnotherHeadBranch;
        private System.Windows.Forms.Button btnAnotherHeadCommit;
        private System.Windows.Forms.Button btnCompareDirectoriesWithDiffTool;
        private System.Windows.Forms.CheckBox ckCompareToMergeBase;
        private System.Windows.Forms.GroupBox baseCommitGroup;
        private System.Windows.Forms.GroupBox headCommitGroup;
        private System.Windows.Forms.FlowLayoutPanel diffOptionsPanel;
    }
}
