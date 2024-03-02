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
            components = new System.ComponentModel.Container();
            mainLayoutPanel = new TableLayoutPanel();
            splitContainer1 = new SplitContainer();
            DiffFiles = new GitUI.FileStatusList();
            DiffText = new GitUI.Editor.FileViewer();
            settingsLayoutPanel = new TableLayoutPanel();
            btnSwap = new Button();
            ckCompareToMergeBase = new CheckBox();
            btnCompareDirectoriesWithDiffTool = new Button();
            diffOptionsPanel = new FlowLayoutPanel();
            firstCommitGroup = new GroupBox();
            firstCommitPanel = new FlowLayoutPanel();
            lblFirstCommit = new Label();
            btnAnotherFirstBranch = new Button();
            btnAnotherFirstCommit = new Button();
            secondCommitGroup = new GroupBox();
            secondCommitPanel = new FlowLayoutPanel();
            lblSecondCommit = new Label();
            btnAnotherSecondBranch = new Button();
            btnAnotherSecondCommit = new Button();
            diffShowInFileTreeToolStripMenuItem = new ToolStripMenuItem();
            DiffContextMenu = new ContextMenuStrip(components);
            openWithDifftoolToolStripMenuItem = new ToolStripMenuItem();
            firstToSelectedToolStripMenuItem = new ToolStripMenuItem();
            firstToLocalToolStripMenuItem = new ToolStripMenuItem();
            selectedToLocalToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator32 = new ToolStripSeparator();
            copyPathsToolStripMenuItem = new CopyPathsToolStripMenuItem();
            openContainingFolderToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator33 = new ToolStripSeparator();
            fileHistoryDiffToolstripMenuItem = new ToolStripMenuItem();
            blameToolStripMenuItem = new ToolStripMenuItem();
            findInDiffToolStripMenuItem = new ToolStripMenuItem();
            mainLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            settingsLayoutPanel.SuspendLayout();
            firstCommitGroup.SuspendLayout();
            firstCommitPanel.SuspendLayout();
            secondCommitGroup.SuspendLayout();
            secondCommitPanel.SuspendLayout();
            DiffContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            mainLayoutPanel.ColumnCount = 1;
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            mainLayoutPanel.Controls.Add(splitContainer1, 0, 1);
            mainLayoutPanel.Controls.Add(settingsLayoutPanel, 0, 0);
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.Location = new Point(0, 0);
            mainLayoutPanel.Name = "mainLayoutPanel";
            mainLayoutPanel.RowCount = 2;
            mainLayoutPanel.RowStyles.Add(new RowStyle());
            mainLayoutPanel.RowStyles.Add(new RowStyle());
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            mainLayoutPanel.Size = new Size(1042, 685);
            mainLayoutPanel.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 80);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(DiffFiles);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(DiffText);
            splitContainer1.Size = new Size(1036, 602);
            splitContainer1.SplitterDistance = 345;
            splitContainer1.TabIndex = 1;
            // 
            // DiffFiles
            // 
            DiffFiles.Dock = DockStyle.Fill;
            DiffFiles.Location = new Point(0, 0);
            DiffFiles.Margin = new Padding(3, 4, 3, 4);
            DiffFiles.Name = "DiffFiles";
            DiffFiles.Size = new Size(345, 602);
            DiffFiles.TabIndex = 0;
            // 
            // DiffText
            // 
            DiffText.Dock = DockStyle.Fill;
            DiffText.Location = new Point(0, 0);
            DiffText.Margin = new Padding(3, 2, 3, 2);
            DiffText.Name = "DiffText";
            DiffText.ShowLineNumbers = false;
            DiffText.Size = new Size(687, 602);
            DiffText.TabIndex = 0;
            // 
            // settingsLayoutPanel
            // 
            settingsLayoutPanel.AutoSize = true;
            settingsLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            settingsLayoutPanel.ColumnCount = 3;
            settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            settingsLayoutPanel.Controls.Add(btnSwap, 1, 0);
            settingsLayoutPanel.Controls.Add(diffOptionsPanel, 0, 1);
            settingsLayoutPanel.Controls.Add(firstCommitGroup, 0, 0);
            settingsLayoutPanel.Controls.Add(secondCommitGroup, 2, 0);
            settingsLayoutPanel.Dock = DockStyle.Fill;
            settingsLayoutPanel.Location = new Point(3, 3);
            settingsLayoutPanel.Name = "settingsLayoutPanel";
            settingsLayoutPanel.RowCount = 2;
            settingsLayoutPanel.RowStyles.Add(new RowStyle());
            settingsLayoutPanel.RowStyles.Add(new RowStyle());
            settingsLayoutPanel.Size = new Size(1036, 71);
            settingsLayoutPanel.TabIndex = 3;
            // 
            // btnSwap
            // 
            btnSwap.Anchor = AnchorStyles.Left;
            btnSwap.AutoSize = true;
            btnSwap.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSwap.Image = Properties.Images.Renamed;
            btnSwap.ImageAlign = ContentAlignment.MiddleLeft;
            btnSwap.Location = new Point(507, 20);
            btnSwap.Margin = new Padding(10, 15, 10, 0);
            btnSwap.Name = "btnSwap";
            btnSwap.Size = new Size(22, 22);
            btnSwap.TabIndex = 6;
            btnSwap.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSwap.UseVisualStyleBackColor = true;
            btnSwap.Click += btnSwap_Click;
            // 
            // ckCompareToMergeBase
            // 
            ckCompareToMergeBase.Anchor = AnchorStyles.Left;
            ckCompareToMergeBase.AutoSize = true;
            ckCompareToMergeBase.Name = "ckCompareToMergeBase";
            ckCompareToMergeBase.TabIndex = 9;
            ckCompareToMergeBase.Text = "Compare to merge &base";
            ckCompareToMergeBase.UseMnemonic = true;
            ckCompareToMergeBase.UseVisualStyleBackColor = true;
            ckCompareToMergeBase.CheckedChanged += ckCompareToMergeBase_CheckedChanged;
            // 
            // btnCompareDirectoriesWithDiffTool
            // 
            btnCompareDirectoriesWithDiffTool.Anchor = AnchorStyles.Left;
            btnCompareDirectoriesWithDiffTool.AutoSize = true;
            btnCompareDirectoriesWithDiffTool.Name = "btnCompareDirectoriesWithDiffTool";
            btnCompareDirectoriesWithDiffTool.Margin = new Padding(0);
            btnCompareDirectoriesWithDiffTool.Size = new Size(141, 17);
            btnCompareDirectoriesWithDiffTool.TabIndex = 10;
            btnCompareDirectoriesWithDiffTool.Text = "Open diff using &directory diff tool";
            btnCompareDirectoriesWithDiffTool.UseMnemonic = true;
            btnCompareDirectoriesWithDiffTool.UseVisualStyleBackColor = true;
            btnCompareDirectoriesWithDiffTool.Click += btnCompareDirectoriesWithDiffTool_Clicked;
            btnCompareDirectoriesWithDiffTool.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // 
            // diffOptionsPanel
            // 
            diffOptionsPanel.AutoSize = true;
            diffOptionsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            diffOptionsPanel.Controls.Add(ckCompareToMergeBase);
            diffOptionsPanel.Controls.Add(btnCompareDirectoriesWithDiffTool);
            diffOptionsPanel.Margin = new Padding(3);
            diffOptionsPanel.Name = "diffOptionsPanel";
            diffOptionsPanel.Size = new Size(491, 23);
            diffOptionsPanel.TabIndex = 14;
            // 
            // firstCommitGroup
            // 
            firstCommitGroup.AutoSize = true;
            firstCommitGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            firstCommitGroup.Controls.Add(firstCommitPanel);
            firstCommitGroup.Dock = DockStyle.Fill;
            firstCommitGroup.Location = new Point(0, 0);
            firstCommitGroup.Margin = new Padding(0);
            firstCommitGroup.Name = "firstCommitGroup";
            firstCommitGroup.Size = new Size(497, 48);
            firstCommitGroup.TabIndex = 16;
            firstCommitGroup.TabStop = false;
            firstCommitGroup.Text = "BASE";
            // 
            // firstCommitPanel
            // 
            firstCommitPanel.AutoSize = true;
            firstCommitPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            firstCommitPanel.Controls.Add(lblFirstCommit);
            firstCommitPanel.Controls.Add(btnAnotherFirstBranch);
            firstCommitPanel.Controls.Add(btnAnotherFirstCommit);
            firstCommitPanel.Dock = DockStyle.Fill;
            firstCommitPanel.Location = new Point(3, 17);
            firstCommitPanel.Margin = new Padding(0);
            firstCommitPanel.Name = "firstCommitPanel";
            firstCommitPanel.Size = new Size(491, 28);
            firstCommitPanel.TabIndex = 14;
            // 
            // lblFirstCommit
            // 
            lblFirstCommit.Anchor = AnchorStyles.Left;
            lblFirstCommit.AutoSize = true;
            lblFirstCommit.Location = new Point(3, 7);
            lblFirstCommit.MinimumSize = new Size(200, 0);
            lblFirstCommit.Name = "lblFirstCommit";
            lblFirstCommit.Size = new Size(200, 13);
            lblFirstCommit.TabIndex = 14;
            lblFirstCommit.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnAnotherFirstBranch
            // 
            btnAnotherFirstBranch.Anchor = AnchorStyles.Left;
            btnAnotherFirstBranch.AutoSize = true;
            btnAnotherFirstBranch.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAnotherFirstBranch.Image = Properties.Images.BranchCheckout;
            btnAnotherFirstBranch.Location = new Point(209, 3);
            btnAnotherFirstBranch.Name = "btnAnotherFirstBranch";
            btnAnotherFirstBranch.Size = new Size(22, 22);
            btnAnotherFirstBranch.TabIndex = 7;
            btnAnotherFirstBranch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAnotherFirstBranch.UseVisualStyleBackColor = true;
            btnAnotherFirstBranch.Click += btnPickAnotherFirstBranch_Click;
            // 
            // btnAnotherFirstCommit
            // 
            btnAnotherFirstCommit.Anchor = AnchorStyles.Left;
            btnAnotherFirstCommit.AutoSize = true;
            btnAnotherFirstCommit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAnotherFirstCommit.Image = Properties.Images.SelectRevision;
            btnAnotherFirstCommit.Location = new Point(237, 3);
            btnAnotherFirstCommit.Name = "btnAnotherFirstCommit";
            btnAnotherFirstCommit.Size = new Size(22, 22);
            btnAnotherFirstCommit.TabIndex = 10;
            btnAnotherFirstCommit.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAnotherFirstCommit.UseVisualStyleBackColor = true;
            btnAnotherFirstCommit.Click += btnAnotherFirstCommit_Click;
            // 
            // secondCommitGroup
            // 
            secondCommitGroup.AutoSize = true;
            secondCommitGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            secondCommitGroup.Controls.Add(secondCommitPanel);
            secondCommitGroup.Dock = DockStyle.Fill;
            secondCommitGroup.Location = new Point(539, 0);
            secondCommitGroup.Margin = new Padding(0);
            secondCommitGroup.Name = "secondCommitGroup";
            secondCommitGroup.Size = new Size(497, 48);
            secondCommitGroup.TabIndex = 17;
            secondCommitGroup.TabStop = false;
            secondCommitGroup.Text = "Compare";
            // 
            // secondCommitPanel
            // 
            secondCommitPanel.AutoSize = true;
            secondCommitPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            secondCommitPanel.Controls.Add(lblSecondCommit);
            secondCommitPanel.Controls.Add(btnAnotherSecondBranch);
            secondCommitPanel.Controls.Add(btnAnotherSecondCommit);
            secondCommitPanel.Dock = DockStyle.Fill;
            secondCommitPanel.Location = new Point(3, 17);
            secondCommitPanel.Margin = new Padding(0);
            secondCommitPanel.Name = "secondCommitPanel";
            secondCommitPanel.Size = new Size(491, 28);
            secondCommitPanel.TabIndex = 15;
            // 
            // lblSecondCommit
            // 
            lblSecondCommit.Anchor = AnchorStyles.Left;
            lblSecondCommit.AutoSize = true;
            lblSecondCommit.Location = new Point(3, 7);
            lblSecondCommit.MinimumSize = new Size(200, 0);
            lblSecondCommit.Name = "lblSecondCommit";
            lblSecondCommit.Size = new Size(200, 13);
            lblSecondCommit.TabIndex = 1;
            lblSecondCommit.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnAnotherSecondBranch
            // 
            btnAnotherSecondBranch.Anchor = AnchorStyles.Left;
            btnAnotherSecondBranch.AutoSize = true;
            btnAnotherSecondBranch.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAnotherSecondBranch.Image = Properties.Images.BranchCheckout;
            btnAnotherSecondBranch.Location = new Point(209, 3);
            btnAnotherSecondBranch.Name = "btnAnotherSecondBranch";
            btnAnotherSecondBranch.Size = new Size(22, 22);
            btnAnotherSecondBranch.TabIndex = 16;
            btnAnotherSecondBranch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAnotherSecondBranch.UseVisualStyleBackColor = true;
            btnAnotherSecondBranch.Click += btnAnotherSecondBranch_Click;
            // 
            // btnAnotherSecondCommit
            // 
            btnAnotherSecondCommit.Anchor = AnchorStyles.Left;
            btnAnotherSecondCommit.AutoSize = true;
            btnAnotherSecondCommit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAnotherSecondCommit.Image = Properties.Images.SelectRevision;
            btnAnotherSecondCommit.Location = new Point(237, 3);
            btnAnotherSecondCommit.Name = "btnAnotherSecondCommit";
            btnAnotherSecondCommit.Size = new Size(22, 22);
            btnAnotherSecondCommit.TabIndex = 17;
            btnAnotherSecondCommit.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAnotherSecondCommit.UseVisualStyleBackColor = true;
            btnAnotherSecondCommit.Click += btnAnotherSecondCommit_Click;
            // 
            // diffShowInFileTreeToolStripMenuItem
            // 
            diffShowInFileTreeToolStripMenuItem.Name = "diffShowInFileTreeToolStripMenuItem";
            diffShowInFileTreeToolStripMenuItem.Size = new Size(210, 22);
            diffShowInFileTreeToolStripMenuItem.Text = "Show in File tree";
            // 
            // DiffContextMenu
            // 
            DiffContextMenu.Items.AddRange(new ToolStripItem[] {
            openWithDifftoolToolStripMenuItem,
            toolStripSeparator32,
            copyPathsToolStripMenuItem,
            openContainingFolderToolStripMenuItem,
            toolStripSeparator33,
            fileHistoryDiffToolstripMenuItem,
            blameToolStripMenuItem,
            findInDiffToolStripMenuItem});
            DiffContextMenu.Name = "DiffContextMenu";
            DiffContextMenu.Size = new Size(211, 148);
            DiffContextMenu.Opening += diffContextToolStripMenuItem_Opening;
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            firstToSelectedToolStripMenuItem,
            firstToLocalToolStripMenuItem,
            selectedToLocalToolStripMenuItem});
            openWithDifftoolToolStripMenuItem.Image = Properties.Images.Diff;
            openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            openWithDifftoolToolStripMenuItem.Size = new Size(210, 22);
            openWithDifftoolToolStripMenuItem.Text = "Open with &difftool";
            openWithDifftoolToolStripMenuItem.DropDownOpening += openWithDifftoolToolStripMenuItem_DropDownOpening;
            // 
            // firstToSelectedToolStripMenuItem
            // 
            firstToSelectedToolStripMenuItem.Name = "firstToSelectedToolStripMenuItem";
            firstToSelectedToolStripMenuItem.ShortcutKeys = Keys.F3;
            firstToSelectedToolStripMenuItem.Size = new Size(254, 22);
            firstToSelectedToolStripMenuItem.Text = "First -> Compare";
            firstToSelectedToolStripMenuItem.Click += openWithDifftoolToolStripMenuItem_Click;
            // 
            // firstToLocalToolStripMenuItem
            // 
            firstToLocalToolStripMenuItem.Name = "firstToLocalToolStripMenuItem";
            firstToLocalToolStripMenuItem.Size = new Size(254, 22);
            firstToLocalToolStripMenuItem.Text = "First -> Working directory";
            firstToLocalToolStripMenuItem.Click += openWithDifftoolToolStripMenuItem_Click;
            // 
            // selectedToLocalToolStripMenuItem
            // 
            selectedToLocalToolStripMenuItem.Name = "selectedToLocalToolStripMenuItem";
            selectedToLocalToolStripMenuItem.Size = new Size(254, 22);
            selectedToLocalToolStripMenuItem.Text = "Compare -> Working directory";
            selectedToLocalToolStripMenuItem.Click += openWithDifftoolToolStripMenuItem_Click;
            // 
            // toolStripSeparator32
            // 
            toolStripSeparator32.Name = "toolStripSeparator32";
            toolStripSeparator32.Size = new Size(207, 6);
            // 
            // copyPathsToolStripMenuItem
            // 
            copyPathsToolStripMenuItem.Name = "copyPathsToolStripMenuItem";
            // 
            // openContainingFolderToolStripMenuItem
            // 
            openContainingFolderToolStripMenuItem.Image = Properties.Images.BrowseFileExplorer;
            openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            openContainingFolderToolStripMenuItem.Size = new Size(210, 22);
            openContainingFolderToolStripMenuItem.Text = "Show &in folder";
            openContainingFolderToolStripMenuItem.Click += openContainingFolderToolStripMenuItem_Click;
            // 
            // toolStripSeparator33
            // 
            toolStripSeparator33.Name = "toolStripSeparator33";
            toolStripSeparator33.Size = new Size(207, 6);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            fileHistoryDiffToolstripMenuItem.Image = Properties.Images.FileHistory;
            fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            fileHistoryDiffToolstripMenuItem.Size = new Size(210, 22);
            fileHistoryDiffToolstripMenuItem.Text = "File &history";
            fileHistoryDiffToolstripMenuItem.Click += fileHistoryDiffToolStripMenuItem_Click;
            // 
            // blameToolStripMenuItem
            // 
            blameToolStripMenuItem.Image = Properties.Images.Blame;
            blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            blameToolStripMenuItem.Size = new Size(210, 22);
            blameToolStripMenuItem.Text = "&Blame";
            blameToolStripMenuItem.Click += blameToolStripMenuItem_Click;
            // 
            // findInDiffToolStripMenuItem
            // 
            findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            findInDiffToolStripMenuItem.Size = new Size(210, 22);
            findInDiffToolStripMenuItem.Text = "&Find file...";
            findInDiffToolStripMenuItem.Click += findInDiffToolStripMenuItem_Click;
            // 
            // FormDiff
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1042, 685);
            Controls.Add(mainLayoutPanel);
            DoubleBuffered = true;
            Name = "FormDiff";
            Text = "Diff";
            mainLayoutPanel.ResumeLayout(false);
            mainLayoutPanel.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            settingsLayoutPanel.ResumeLayout(false);
            settingsLayoutPanel.PerformLayout();
            diffOptionsPanel.ResumeLayout(false);
            diffOptionsPanel.PerformLayout();
            firstCommitGroup.ResumeLayout(false);
            firstCommitGroup.PerformLayout();
            firstCommitPanel.ResumeLayout(false);
            firstCommitPanel.PerformLayout();
            secondCommitGroup.ResumeLayout(false);
            secondCommitGroup.PerformLayout();
            secondCommitPanel.ResumeLayout(false);
            secondCommitPanel.PerformLayout();
            DiffContextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel mainLayoutPanel;
        private SplitContainer splitContainer1;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
        private TableLayoutPanel settingsLayoutPanel;
        private Label lblSecondCommit;
        private Button btnSwap;
        private Button btnAnotherFirstBranch;
        private ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private ContextMenuStrip DiffContextMenu;
        private ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private ToolStripMenuItem firstToSelectedToolStripMenuItem;
        private ToolStripMenuItem firstToLocalToolStripMenuItem;
        private ToolStripMenuItem selectedToLocalToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator32;
        private CopyPathsToolStripMenuItem copyPathsToolStripMenuItem;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator33;
        private ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private ToolStripMenuItem blameToolStripMenuItem;
        private ToolStripMenuItem findInDiffToolStripMenuItem;
        private Button btnAnotherFirstCommit;
        private FlowLayoutPanel firstCommitPanel;
        private Label lblFirstCommit;
        private FlowLayoutPanel secondCommitPanel;
        private Button btnAnotherSecondBranch;
        private Button btnAnotherSecondCommit;
        private Button btnCompareDirectoriesWithDiffTool;
        private CheckBox ckCompareToMergeBase;
        private GroupBox firstCommitGroup;
        private GroupBox secondCommitGroup;
        private FlowLayoutPanel diffOptionsPanel;
    }
}
