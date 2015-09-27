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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.labelComparing = new System.Windows.Forms.Label();
            this.lblLeftCommit = new System.Windows.Forms.Label();
            this.labelAnd = new System.Windows.Forms.Label();
            this.lblRightCommit = new System.Windows.Forms.Label();
            this.btnSwap = new System.Windows.Forms.Button();
            this.btnPickAnotherBranch = new System.Windows.Forms.Button();
            this.ckCompareToMergeBase = new System.Windows.Forms.CheckBox();
            this.diffShowInFileTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parentOfALocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parentOfBLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFilenameToClipboardToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator33 = new System.Windows.Forms.ToolStripSeparator();
            this.fileHistoryDiffToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findInDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1042, 523);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 40);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.DiffFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.DiffText);
            this.splitContainer1.Size = new System.Drawing.Size(1036, 496);
            this.splitContainer1.SplitterDistance = 345;
            this.splitContainer1.TabIndex = 1;
            // 
            // DiffFiles
            // 
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(345, 496);
            this.DiffFiles.TabIndex = 0;
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DiffText.Name = "DiffText";
            this.DiffText.ShowLineNumbers = false;
            this.DiffText.Size = new System.Drawing.Size(687, 496);
            this.DiffText.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSwap, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnPickAnotherBranch, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.ckCompareToMergeBase, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1036, 31);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.labelComparing);
            this.flowLayoutPanel2.Controls.Add(this.lblLeftCommit);
            this.flowLayoutPanel2.Controls.Add(this.labelAnd);
            this.flowLayoutPanel2.Controls.Add(this.lblRightCommit);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 8);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(158, 15);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // labelComparing
            // 
            this.labelComparing.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelComparing.AutoSize = true;
            this.labelComparing.Location = new System.Drawing.Point(3, 0);
            this.labelComparing.Name = "labelComparing";
            this.labelComparing.Size = new System.Drawing.Size(67, 15);
            this.labelComparing.TabIndex = 0;
            this.labelComparing.Text = "Comparing";
            this.labelComparing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLeftCommit
            // 
            this.lblLeftCommit.AutoSize = true;
            this.lblLeftCommit.Location = new System.Drawing.Point(76, 0);
            this.lblLeftCommit.MinimumSize = new System.Drawing.Size(20, 0);
            this.lblLeftCommit.Name = "lblLeftCommit";
            this.lblLeftCommit.Size = new System.Drawing.Size(20, 15);
            this.lblLeftCommit.TabIndex = 1;
            this.lblLeftCommit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAnd
            // 
            this.labelAnd.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelAnd.AutoSize = true;
            this.labelAnd.Location = new System.Drawing.Point(102, 0);
            this.labelAnd.Name = "labelAnd";
            this.labelAnd.Size = new System.Drawing.Size(27, 15);
            this.labelAnd.TabIndex = 0;
            this.labelAnd.Text = "and";
            this.labelAnd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRightCommit
            // 
            this.lblRightCommit.AutoSize = true;
            this.lblRightCommit.Location = new System.Drawing.Point(135, 0);
            this.lblRightCommit.MinimumSize = new System.Drawing.Size(20, 0);
            this.lblRightCommit.Name = "lblRightCommit";
            this.lblRightCommit.Size = new System.Drawing.Size(20, 15);
            this.lblRightCommit.TabIndex = 1;
            this.lblRightCommit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSwap
            // 
            this.btnSwap.AutoSize = true;
            this.btnSwap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSwap.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSwap.Image = global::GitUI.Properties.Resources.Renamed;
            this.btnSwap.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSwap.Location = new System.Drawing.Point(399, 3);
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(61, 25);
            this.btnSwap.TabIndex = 6;
            this.btnSwap.Text = "Swap";
            this.btnSwap.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSwap.UseVisualStyleBackColor = true;
            this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
            // 
            // btnPickAnotherBranch
            // 
            this.btnPickAnotherBranch.AutoSize = true;
            this.btnPickAnotherBranch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPickAnotherBranch.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPickAnotherBranch.Image = global::GitUI.Properties.Resources.IconFind;
            this.btnPickAnotherBranch.Location = new System.Drawing.Point(466, 3);
            this.btnPickAnotherBranch.Name = "btnPickAnotherBranch";
            this.btnPickAnotherBranch.Size = new System.Drawing.Size(116, 25);
            this.btnPickAnotherBranch.TabIndex = 7;
            this.btnPickAnotherBranch.Text = "Another Branch";
            this.btnPickAnotherBranch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPickAnotherBranch.UseVisualStyleBackColor = true;
            this.btnPickAnotherBranch.Click += new System.EventHandler(this.btnPickAnotherBranch_Click);
            // 
            // ckCompareToMergeBase
            // 
            this.ckCompareToMergeBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ckCompareToMergeBase.AutoSize = true;
            this.ckCompareToMergeBase.Location = new System.Drawing.Point(167, 3);
            this.ckCompareToMergeBase.Name = "ckCompareToMergeBase";
            this.ckCompareToMergeBase.Size = new System.Drawing.Size(226, 25);
            this.ckCompareToMergeBase.TabIndex = 8;
            this.ckCompareToMergeBase.Text = "Compare right commit to merge base";
            this.ckCompareToMergeBase.UseVisualStyleBackColor = true;
            this.ckCompareToMergeBase.CheckedChanged += new System.EventHandler(this.ckCompareToMergeBase_CheckedChanged);
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
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aBToolStripMenuItem,
            this.aLocalToolStripMenuItem,
            this.bLocalToolStripMenuItem,
            this.parentOfALocalToolStripMenuItem,
            this.parentOfBLocalToolStripMenuItem});
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            // 
            // aBToolStripMenuItem
            // 
            this.aBToolStripMenuItem.Name = "aBToolStripMenuItem";
            this.aBToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.aBToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.aBToolStripMenuItem.Text = "A <--> B";
            this.aBToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // aLocalToolStripMenuItem
            // 
            this.aLocalToolStripMenuItem.Name = "aLocalToolStripMenuItem";
            this.aLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.aLocalToolStripMenuItem.Text = "A <--> Working directory";
            this.aLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // bLocalToolStripMenuItem
            // 
            this.bLocalToolStripMenuItem.Name = "bLocalToolStripMenuItem";
            this.bLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.bLocalToolStripMenuItem.Text = "B <--> Working directory";
            this.bLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // parentOfALocalToolStripMenuItem
            // 
            this.parentOfALocalToolStripMenuItem.Name = "parentOfALocalToolStripMenuItem";
            this.parentOfALocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.parentOfALocalToolStripMenuItem.Text = "A\'s parent <--> Working directory";
            this.parentOfALocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // parentOfBLocalToolStripMenuItem
            // 
            this.parentOfBLocalToolStripMenuItem.Name = "parentOfBLocalToolStripMenuItem";
            this.parentOfBLocalToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.parentOfBLocalToolStripMenuItem.Text = "B\'s parent <--> Working directory";
            this.parentOfBLocalToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(207, 6);
            // 
            // copyFilenameToClipboardToolStripMenuItem1
            // 
            this.copyFilenameToClipboardToolStripMenuItem1.Name = "copyFilenameToClipboardToolStripMenuItem1";
            this.copyFilenameToClipboardToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.copyFilenameToClipboardToolStripMenuItem1.Text = "Copy full path(s)";
            this.copyFilenameToClipboardToolStripMenuItem1.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem1_Click);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open containing folder(s)";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(207, 6);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            this.fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            this.fileHistoryDiffToolstripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.fileHistoryDiffToolstripMenuItem.Text = "File history";
            this.fileHistoryDiffToolstripMenuItem.Click += new System.EventHandler(this.fileHistoryDiffToolstripMenuItem_Click);
            // 
            // blameToolStripMenuItem
            // 
            this.blameToolStripMenuItem.Name = "blameToolStripMenuItem";
            this.blameToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.blameToolStripMenuItem.Text = "Blame";
            this.blameToolStripMenuItem.Click += new System.EventHandler(this.blameToolStripMenuItem_Click);
            // 
            // findInDiffToolStripMenuItem
            // 
            this.findInDiffToolStripMenuItem.Name = "findInDiffToolStripMenuItem";
            this.findInDiffToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findInDiffToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.findInDiffToolStripMenuItem.Text = "Find";
            this.findInDiffToolStripMenuItem.Click += new System.EventHandler(this.findInDiffToolStripMenuItem_Click);
            // 
            // FormDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1042, 523);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormDiff";
            this.Text = "Diff";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.DiffContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label labelComparing;
        private System.Windows.Forms.Label lblLeftCommit;
        private System.Windows.Forms.Label labelAnd;
        private System.Windows.Forms.Label lblRightCommit;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.Button btnPickAnotherBranch;
        private System.Windows.Forms.ToolStripMenuItem diffShowInFileTreeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parentOfALocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parentOfBLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator32;
        private System.Windows.Forms.ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator33;
        private System.Windows.Forms.ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findInDiffToolStripMenuItem;
        private System.Windows.Forms.CheckBox ckCompareToMergeBase;
    }
}