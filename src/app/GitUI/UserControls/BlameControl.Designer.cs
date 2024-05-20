namespace GitUI.Blame
{
    partial class BlameControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            CommitInfo = new GitUI.CommitInfo.CommitInfo();
            splitContainer2 = new SplitContainer();
            BlameAuthor = new GitUI.Editor.FileViewer();
            contextMenu = new ContextMenuStrip(components);
            blameRevisionToolStripMenuItem = new ToolStripMenuItem();
            blamePreviousRevisionToolStripMenuItem = new ToolStripMenuItem();
            showChangesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            copyToClipboardToolStripMenuItem = new ToolStripMenuItem();
            commitHashToolStripMenuItem = new ToolStripMenuItem();
            commitMessageToolStripMenuItem = new ToolStripMenuItem();
            allCommitInfoToolStripMenuItem = new ToolStripMenuItem();
            BlameFile = new GitUI.Editor.FileViewer();
            blameTooltip = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            contextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = SystemColors.Control;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(CommitInfo);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(858, 740);
            splitContainer1.SplitterDistance = 160;
            splitContainer1.TabIndex = 7;
            // 
            // CommitInfo
            // 
            CommitInfo.Dock = DockStyle.Fill;
            CommitInfo.Location = new Point(0, 0);
            CommitInfo.Margin = new Padding(0);
            CommitInfo.Name = "CommitInfo";
            CommitInfo.Size = new Size(858, 160);
            CommitInfo.TabIndex = 5;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(BlameAuthor);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(BlameFile);
            splitContainer2.Size = new Size(858, 576);
            splitContainer2.SplitterDistance = 186;
            splitContainer2.TabIndex = 0;
            // 
            // BlameCommitter
            // 
            BlameAuthor.ContextMenuStrip = contextMenu;
            BlameAuthor.Dock = DockStyle.Fill;
            BlameAuthor.IsReadOnly = false;
            BlameAuthor.Location = new Point(0, 0);
            BlameAuthor.Margin = new Padding(0);
            BlameAuthor.Name = "BlameAuthor";
            BlameAuthor.Size = new Size(186, 576);
            BlameAuthor.TabIndex = 5;
            BlameAuthor.TabStop = false;
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] {
            blameRevisionToolStripMenuItem,
            blamePreviousRevisionToolStripMenuItem,
            showChangesToolStripMenuItem,
            toolStripSeparator1,
            copyToClipboardToolStripMenuItem});
            contextMenu.Name = "ContextMenu";
            contextMenu.Size = new Size(239, 76);
            contextMenu.Opened += contextMenu_Opened;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(235, 6);
            blameRevisionToolStripMenuItem.Image = Properties.Resources.Blame;
            blameRevisionToolStripMenuItem.Name = "blameRevisionToolStripMenuItem";
            blameRevisionToolStripMenuItem.Size = new Size(199, 22);
            blameRevisionToolStripMenuItem.Text = "Blame this revision";
            blameRevisionToolStripMenuItem.Click += blameRevisionToolStripMenuItem_Click;
            // 
            // blamePreviousRevisionToolStripMenuItem
            // 
            blamePreviousRevisionToolStripMenuItem.Image = Properties.Resources.RecentRepositories;
            blamePreviousRevisionToolStripMenuItem.Name = "blamePreviousRevisionToolStripMenuItem";
            blamePreviousRevisionToolStripMenuItem.Size = new Size(238, 22);
            blamePreviousRevisionToolStripMenuItem.Text = "Blame previous revision";
            blamePreviousRevisionToolStripMenuItem.Click += blamePreviousRevisionToolStripMenuItem_Click;
            // 
            // showChangesToolStripMenuItem
            // 
            showChangesToolStripMenuItem.Image = Properties.Resources.information;
            showChangesToolStripMenuItem.Name = "showChangesToolStripMenuItem";
            showChangesToolStripMenuItem.Size = new Size(238, 22);
            showChangesToolStripMenuItem.Text = "Show changes";
            showChangesToolStripMenuItem.Click += showChangesToolStripMenuItem_Click;
            // 
            // copyToClipboardToolStripMenuItem
            // 
            copyToClipboardToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            commitHashToolStripMenuItem,
            commitMessageToolStripMenuItem,
            allCommitInfoToolStripMenuItem});
            copyToClipboardToolStripMenuItem.Image = Properties.Resources.CopyToClipboard;
            copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            copyToClipboardToolStripMenuItem.Size = new Size(199, 22);
            copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            // 
            // commitHashToolStripMenuItem
            // 
            commitHashToolStripMenuItem.Image = Properties.Resources.CommitId;
            commitHashToolStripMenuItem.Name = "commitHashToolStripMenuItem";
            commitHashToolStripMenuItem.Size = new Size(180, 22);
            commitHashToolStripMenuItem.Text = "Commit hash";
            commitHashToolStripMenuItem.Click += copyCommitHashToClipboardToolStripMenuItem_Click;
            // 
            // commitMessageToolStripMenuItem
            // 
            commitMessageToolStripMenuItem.Image = Properties.Resources.Message;
            commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            commitMessageToolStripMenuItem.Size = new Size(180, 22);
            commitMessageToolStripMenuItem.Text = "Commit message";
            commitMessageToolStripMenuItem.Click += copyLogMessageToolStripMenuItem_Click;
            // 
            // allCommitInfoToolStripMenuItem
            // 
            allCommitInfoToolStripMenuItem.Image = Properties.Resources.CommitSummary;
            allCommitInfoToolStripMenuItem.Name = "allCommitInfoToolStripMenuItem";
            allCommitInfoToolStripMenuItem.Size = new Size(180, 22);
            allCommitInfoToolStripMenuItem.Text = "All commit info";
            allCommitInfoToolStripMenuItem.Click += copyAllCommitInfoToClipboardToolStripMenuItem_Click;
            // 
            // BlameFile
            // 
            BlameFile.Dock = DockStyle.Fill;
            BlameFile.IsReadOnly = false;
            BlameFile.Location = new Point(0, 0);
            BlameFile.Margin = new Padding(0);
            BlameFile.Name = "BlameFile";
            BlameFile.Size = new Size(666, 576);
            BlameFile.TabIndex = 0;
            // 
            // BlameControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(splitContainer1);
            Margin = new Padding(4);
            Name = "BlameControl";
            Size = new Size(858, 740);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).EndInit();
            splitContainer2.ResumeLayout(false);
            contextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private CommitInfo.CommitInfo CommitInfo;
        private SplitContainer splitContainer2;
        private GitUI.Editor.FileViewer BlameAuthor;
        private GitUI.Editor.FileViewer BlameFile;
        private ToolTip blameTooltip;
        private ContextMenuStrip contextMenu;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem blamePreviousRevisionToolStripMenuItem;
        private ToolStripMenuItem showChangesToolStripMenuItem;
        private ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private ToolStripMenuItem commitHashToolStripMenuItem;
        private ToolStripMenuItem commitMessageToolStripMenuItem;
        private ToolStripMenuItem allCommitInfoToolStripMenuItem;
        private ToolStripMenuItem blameRevisionToolStripMenuItem;
    }
}
