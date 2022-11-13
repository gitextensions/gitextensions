using System.Windows.Forms;
using GitUI.CommandsDialogs.Menus;

namespace GitUI.CommandsDialogs
{
    partial class FormDiff
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
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.settingsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnSwap = new System.Windows.Forms.Button();
            this.ckCompareToMergeBase = new System.Windows.Forms.CheckBox();
            this.btnCompareDirectoriesWithDiffTool = new System.Windows.Forms.Button();
            this.diffOptionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.firstCommitGroup = new System.Windows.Forms.GroupBox();
            this.firstCommitPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFirstCommit = new System.Windows.Forms.Label();
            this.btnAnotherFirstBranch = new System.Windows.Forms.Button();
            this.btnAnotherFirstCommit = new System.Windows.Forms.Button();
            this.secondCommitGroup = new System.Windows.Forms.GroupBox();
            this.secondCommitPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSecondCommit = new System.Windows.Forms.Label();
            this.btnAnotherSecondBranch = new System.Windows.Forms.Button();
            this.btnAnotherSecondCommit = new System.Windows.Forms.Button();
            this.diffShowInFileTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstToSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedToLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.copyPathsToolStripMenuItem = new CopyPathsToolStripMenuItem();
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
            this.firstCommitGroup.SuspendLayout();
            this.firstCommitPanel.SuspendLayout();
            this.secondCommitGroup.SuspendLayout();
            this.secondCommitPanel.SuspendLayout();
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
            this.settingsLayoutPanel.Controls.Add(this.firstCommitGroup, 0, 0);
            this.settingsLayoutPanel.Controls.Add(this.secondCommitGroup, 2, 0);
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
            // firstCommitGroup
            // 
            this.firstCommitGroup.AutoSize = true;
            this.firstCommitGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.firstCommitGroup.Controls.Add(this.firstCommitPanel);
            this.firstCommitGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.firstCommitGroup.Location = new System.Drawing.Point(0, 0);
            this.firstCommitGroup.Margin = new System.Windows.Forms.Padding(0);
            this.firstCommitGroup.Name = "firstCommitGroup";
            this.firstCommitGroup.Size = new System.Drawing.Size(497, 48);
            this.firstCommitGroup.TabIndex = 16;
            this.firstCommitGroup.TabStop = false;
            this.firstCommitGroup.Text = "BASE";
            // 
            // firstCommitPanel
            // 
            this.firstCommitPanel.AutoSize = true;
            this.firstCommitPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.firstCommitPanel.Controls.Add(this.lblFirstCommit);
            this.firstCommitPanel.Controls.Add(this.btnAnotherFirstBranch);
            this.firstCommitPanel.Controls.Add(this.btnAnotherFirstCommit);
            this.firstCommitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.firstCommitPanel.Location = new System.Drawing.Point(3, 17);
            this.firstCommitPanel.Margin = new System.Windows.Forms.Padding(0);
            this.firstCommitPanel.Name = "firstCommitPanel";
            this.firstCommitPanel.Size = new System.Drawing.Size(491, 28);
            this.firstCommitPanel.TabIndex = 14;
            // 
            // lblFirstCommit
            // 
            this.lblFirstCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFirstCommit.AutoSize = true;
            this.lblFirstCommit.Location = new System.Drawing.Point(3, 7);
            this.lblFirstCommit.MinimumSize = new System.Drawing.Size(200, 0);
            this.lblFirstCommit.Name = "lblFirstCommit";
            this.lblFirstCommit.Size = new System.Drawing.Size(200, 13);
            this.lblFirstCommit.TabIndex = 14;
            this.lblFirstCommit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAnotherFirstBranch
            // 
            this.btnAnotherFirstBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherFirstBranch.AutoSize = true;
            this.btnAnotherFirstBranch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherFirstBranch.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.btnAnotherFirstBranch.Location = new System.Drawing.Point(209, 3);
            this.btnAnotherFirstBranch.Name = "btnAnotherFirstBranch";
            this.btnAnotherFirstBranch.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherFirstBranch.TabIndex = 7;
            this.btnAnotherFirstBranch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherFirstBranch.UseVisualStyleBackColor = true;
            this.btnAnotherFirstBranch.Click += new System.EventHandler(this.btnPickAnotherFirstBranch_Click);
            // 
            // btnAnotherFirstCommit
            // 
            this.btnAnotherFirstCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherFirstCommit.AutoSize = true;
            this.btnAnotherFirstCommit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherFirstCommit.Image = global::GitUI.Properties.Images.SelectRevision;
            this.btnAnotherFirstCommit.Location = new System.Drawing.Point(237, 3);
            this.btnAnotherFirstCommit.Name = "btnAnotherFirstCommit";
            this.btnAnotherFirstCommit.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherFirstCommit.TabIndex = 10;
            this.btnAnotherFirstCommit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherFirstCommit.UseVisualStyleBackColor = true;
            this.btnAnotherFirstCommit.Click += new System.EventHandler(this.btnAnotherFirstCommit_Click);
            // 
            // secondCommitGroup
            // 
            this.secondCommitGroup.AutoSize = true;
            this.secondCommitGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.secondCommitGroup.Controls.Add(this.secondCommitPanel);
            this.secondCommitGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.secondCommitGroup.Location = new System.Drawing.Point(539, 0);
            this.secondCommitGroup.Margin = new System.Windows.Forms.Padding(0);
            this.secondCommitGroup.Name = "secondCommitGroup";
            this.secondCommitGroup.Size = new System.Drawing.Size(497, 48);
            this.secondCommitGroup.TabIndex = 17;
            this.secondCommitGroup.TabStop = false;
            this.secondCommitGroup.Text = "Compare";
            // 
            // secondCommitPanel
            // 
            this.secondCommitPanel.AutoSize = true;
            this.secondCommitPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.secondCommitPanel.Controls.Add(this.lblSecondCommit);
            this.secondCommitPanel.Controls.Add(this.btnAnotherSecondBranch);
            this.secondCommitPanel.Controls.Add(this.btnAnotherSecondCommit);
            this.secondCommitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.secondCommitPanel.Location = new System.Drawing.Point(3, 17);
            this.secondCommitPanel.Margin = new System.Windows.Forms.Padding(0);
            this.secondCommitPanel.Name = "secondCommitPanel";
            this.secondCommitPanel.Size = new System.Drawing.Size(491, 28);
            this.secondCommitPanel.TabIndex = 15;
            // 
            // lblSecondCommit
            // 
            this.lblSecondCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSecondCommit.AutoSize = true;
            this.lblSecondCommit.Location = new System.Drawing.Point(3, 7);
            this.lblSecondCommit.MinimumSize = new System.Drawing.Size(200, 0);
            this.lblSecondCommit.Name = "lblSecondCommit";
            this.lblSecondCommit.Size = new System.Drawing.Size(200, 13);
            this.lblSecondCommit.TabIndex = 1;
            this.lblSecondCommit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAnotherSecondBranch
            // 
            this.btnAnotherSecondBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherSecondBranch.AutoSize = true;
            this.btnAnotherSecondBranch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherSecondBranch.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.btnAnotherSecondBranch.Location = new System.Drawing.Point(209, 3);
            this.btnAnotherSecondBranch.Name = "btnAnotherSecondBranch";
            this.btnAnotherSecondBranch.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherSecondBranch.TabIndex = 16;
            this.btnAnotherSecondBranch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherSecondBranch.UseVisualStyleBackColor = true;
            this.btnAnotherSecondBranch.Click += new System.EventHandler(this.btnAnotherSecondBranch_Click);
            // 
            // btnAnotherSecondCommit
            // 
            this.btnAnotherSecondCommit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAnotherSecondCommit.AutoSize = true;
            this.btnAnotherSecondCommit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnotherSecondCommit.Image = global::GitUI.Properties.Images.SelectRevision;
            this.btnAnotherSecondCommit.Location = new System.Drawing.Point(237, 3);
            this.btnAnotherSecondCommit.Name = "btnAnotherSecondCommit";
            this.btnAnotherSecondCommit.Size = new System.Drawing.Size(22, 22);
            this.btnAnotherSecondCommit.TabIndex = 17;
            this.btnAnotherSecondCommit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAnotherSecondCommit.UseVisualStyleBackColor = true;
            this.btnAnotherSecondCommit.Click += new System.EventHandler(this.btnAnotherSecondCommit_Click);
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
            this.copyPathsToolStripMenuItem,
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
            this.selectedToLocalToolStripMenuItem});
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
            this.firstToSelectedToolStripMenuItem.Text = "First -> Compare";
            this.firstToSelectedToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // firstToLocalToolStripMenuItem
            // 
            this.firstToLocalToolStripMenuItem.Name = "firstToLocalToolStripMenuItem";
            this.firstToLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.firstToLocalToolStripMenuItem.Text = "First -> Working directory";
            this.firstToLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // selectedToLocalToolStripMenuItem
            // 
            this.selectedToLocalToolStripMenuItem.Name = "selectedToLocalToolStripMenuItem";
            this.selectedToLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.selectedToLocalToolStripMenuItem.Text = "Compare -> Working directory";
            this.selectedToLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(207, 6);
            // 
            // copyPathsToolStripMenuItem
            // 
            this.copyPathsToolStripMenuItem.Name = "copyPathsToolStripMenuItem";
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
            this.firstCommitGroup.ResumeLayout(false);
            this.firstCommitGroup.PerformLayout();
            this.firstCommitPanel.ResumeLayout(false);
            this.firstCommitPanel.PerformLayout();
            this.secondCommitGroup.ResumeLayout(false);
            this.secondCommitGroup.PerformLayout();
            this.secondCommitPanel.ResumeLayout(false);
            this.secondCommitPanel.PerformLayout();
            this.DiffContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
        private System.Windows.Forms.TableLayoutPanel settingsLayoutPanel;
        private System.Windows.Forms.Label lblSecondCommit;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.Button btnAnotherFirstBranch;
        private System.Windows.Forms.ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstToSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstToLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedToLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator32;
        private CopyPathsToolStripMenuItem copyPathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator33;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findInDiffToolStripMenuItem;
        private System.Windows.Forms.Button btnAnotherFirstCommit;
        private System.Windows.Forms.FlowLayoutPanel firstCommitPanel;
        private System.Windows.Forms.Label lblFirstCommit;
        private System.Windows.Forms.FlowLayoutPanel secondCommitPanel;
        private System.Windows.Forms.Button btnAnotherSecondBranch;
        private System.Windows.Forms.Button btnAnotherSecondCommit;
        private System.Windows.Forms.Button btnCompareDirectoriesWithDiffTool;
        private System.Windows.Forms.CheckBox ckCompareToMergeBase;
        private System.Windows.Forms.GroupBox firstCommitGroup;
        private System.Windows.Forms.GroupBox secondCommitGroup;
        private System.Windows.Forms.FlowLayoutPanel diffOptionsPanel;
    }
}
