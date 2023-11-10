using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI.CommitInfo
{
    partial class CommitInfo
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
            tableLayout = new TableLayoutPanel();
            pnlCommitMessage = new Panel();
            rtbxCommitMessage = new RichTextBox();
            commitInfoContextMenuStrip = new ContextMenuStrip(components);
            copyLinkToolStripMenuItem = new ToolStripMenuItem();
            copyCommitInfoToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            showContainedInBranchesToolStripMenuItem = new ToolStripMenuItem();
            showContainedInBranchesRemoteToolStripMenuItem = new ToolStripMenuItem();
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem = new ToolStripMenuItem();
            showContainedInTagsToolStripMenuItem = new ToolStripMenuItem();
            showMessagesOfAnnotatedTagsToolStripMenuItem = new ToolStripMenuItem();
            showTagThisCommitDerivesFromMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            addNoteToolStripMenuItem = new ToolStripMenuItem();
            commitInfoHeader = new GitUI.CommitInfo.CommitInfoHeader();
            RevisionInfo = new RichTextBox();
            tableLayout.SuspendLayout();
            pnlCommitMessage.SuspendLayout();
            commitInfoContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayout
            // 
            tableLayout.BackColor = SystemColors.Window;
            tableLayout.ColumnCount = 1;
            tableLayout.ColumnStyles.Add(new ColumnStyle());
            tableLayout.Controls.Add(commitInfoHeader, 0, 0);
            tableLayout.Controls.Add(pnlCommitMessage, 0, 1);
            tableLayout.Controls.Add(RevisionInfo, 0, 2);
            tableLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayout.Location = new Point(0, 0);
            tableLayout.Margin = new Padding(0);
            tableLayout.Name = "tableLayout";
            tableLayout.RowCount = 3;
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.Size = new Size(472, 262);
            tableLayout.TabIndex = 0;
            tableLayout.Visible = false;
            // 
            // pnlCommitMessage
            // 
            pnlCommitMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlCommitMessage.BackColor = SystemColors.Control;
            pnlCommitMessage.Controls.Add(rtbxCommitMessage);
            pnlCommitMessage.Location = new Point(0, 112);
            pnlCommitMessage.Margin = new Padding(0);
            pnlCommitMessage.Name = "pnlCommitMessage";
            pnlCommitMessage.Size = new Size(456, 36);
            pnlCommitMessage.TabIndex = 0;
            // 
            // rtbxCommitMessage
            // 
            rtbxCommitMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbxCommitMessage.BackColor = SystemColors.Control;
            rtbxCommitMessage.BorderStyle = BorderStyle.None;
            rtbxCommitMessage.ContextMenuStrip = commitInfoContextMenuStrip;
            rtbxCommitMessage.Location = new Point(8, 8);
            rtbxCommitMessage.Margin = new Padding(8);
            rtbxCommitMessage.Name = "rtbxCommitMessage";
            rtbxCommitMessage.ReadOnly = true;
            rtbxCommitMessage.ScrollBars = RichTextBoxScrollBars.None;
            rtbxCommitMessage.Size = new Size(440, 20);
            rtbxCommitMessage.TabIndex = 1;
            rtbxCommitMessage.Text = "";
            rtbxCommitMessage.LinkClicked += LinkClicked;
            rtbxCommitMessage.KeyDown += RichTextBox_KeyDown;
            rtbxCommitMessage.MouseDown += _RevisionHeader_MouseDown;
            // 
            // commitInfoContextMenuStrip
            // 
            commitInfoContextMenuStrip.Items.AddRange(new ToolStripItem[] {
            copyLinkToolStripMenuItem,
            copyCommitInfoToolStripMenuItem,
            toolStripSeparator1,
            showContainedInBranchesToolStripMenuItem,
            showContainedInBranchesRemoteToolStripMenuItem,
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem,
            showContainedInTagsToolStripMenuItem,
            showMessagesOfAnnotatedTagsToolStripMenuItem,
            showTagThisCommitDerivesFromMenuItem,
            toolStripSeparator2,
            addNoteToolStripMenuItem});
            commitInfoContextMenuStrip.Name = "commitInfoContextMenuStrip";
            commitInfoContextMenuStrip.Size = new Size(454, 192);
            commitInfoContextMenuStrip.Opening += commitInfoContextMenuStrip_Opening;
            // 
            // copyLinkStripMenuItem
            // 
            copyLinkToolStripMenuItem.Name = "copyLinkStripMenuItem";
            copyLinkToolStripMenuItem.Size = new Size(453, 22);
            copyLinkToolStripMenuItem.Text = "Copy link";
            copyLinkToolStripMenuItem.Click += copyLinkToolStripMenuItem_Click;
            // 
            // copyCommitInfoToolStripMenuItem
            // 
            copyCommitInfoToolStripMenuItem.Name = "copyCommitInfoToolStripMenuItem";
            copyCommitInfoToolStripMenuItem.Size = new Size(453, 22);
            copyCommitInfoToolStripMenuItem.Text = "&Copy commit info";
            copyCommitInfoToolStripMenuItem.Click += copyCommitInfoToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(450, 6);
            // 
            // showContainedInBranchesToolStripMenuItem
            // 
            showContainedInBranchesToolStripMenuItem.Name = "showContainedInBranchesToolStripMenuItem";
            showContainedInBranchesToolStripMenuItem.Size = new Size(453, 22);
            showContainedInBranchesToolStripMenuItem.Text = "Show local branches containing this commit";
            showContainedInBranchesToolStripMenuItem.Click += showContainedInBranchesToolStripMenuItem_Click;
            // 
            // showContainedInBranchesRemoteToolStripMenuItem
            // 
            showContainedInBranchesRemoteToolStripMenuItem.Name = "showContainedInBranchesRemoteToolStripMenuItem";
            showContainedInBranchesRemoteToolStripMenuItem.Size = new Size(453, 22);
            showContainedInBranchesRemoteToolStripMenuItem.Text = "Show remote branches containing this commit";
            showContainedInBranchesRemoteToolStripMenuItem.Click += showContainedInBranchesRemoteToolStripMenuItem_Click;
            // 
            // showContainedInBranchesRemoteIfNoLocalToolStripMenuItem
            // 
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Name = "showContainedInBranchesRemoteIfNoLocalToolStripMenuItem";
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Size = new Size(453, 22);
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Text = "Show remote branches only when no local branch contains this commit";
            showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Click += showContainedInBranchesRemoteIfNoLocalToolStripMenuItem_Click;
            // 
            // showContainedInTagsToolStripMenuItem
            // 
            showContainedInTagsToolStripMenuItem.Name = "showContainedInTagsToolStripMenuItem";
            showContainedInTagsToolStripMenuItem.Size = new Size(453, 22);
            showContainedInTagsToolStripMenuItem.Text = "Show tags containing this commit";
            showContainedInTagsToolStripMenuItem.Click += showContainedInTagsToolStripMenuItem_Click;
            // 
            // showMessagesOfAnnotatedTagsToolStripMenuItem
            // 
            showMessagesOfAnnotatedTagsToolStripMenuItem.Name = "showMessagesOfAnnotatedTagsToolStripMenuItem";
            showMessagesOfAnnotatedTagsToolStripMenuItem.Size = new Size(453, 22);
            showMessagesOfAnnotatedTagsToolStripMenuItem.Text = "Show messages of annotated tags";
            showMessagesOfAnnotatedTagsToolStripMenuItem.Click += showMessagesOfAnnotatedTagsToolStripMenuItem_Click;
            // 
            // showTagThisCommitDerivesFromMenuItem
            // 
            showTagThisCommitDerivesFromMenuItem.Name = "showTagThisCommitDerivesFromMenuItem";
            showTagThisCommitDerivesFromMenuItem.Size = new Size(453, 22);
            showTagThisCommitDerivesFromMenuItem.Text = "Show the most recent tag this commit derives from";
            showTagThisCommitDerivesFromMenuItem.Click += showTagThisCommitDerivesFromMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(450, 6);
            // 
            // addNoteToolStripMenuItem
            // 
            addNoteToolStripMenuItem.Name = "addNoteToolStripMenuItem";
            addNoteToolStripMenuItem.Size = new Size(453, 22);
            addNoteToolStripMenuItem.Text = "Add &notes";
            addNoteToolStripMenuItem.Click += addNoteToolStripMenuItem_Click;
            // 
            // commitInfoHeader
            // 
            commitInfoHeader.AutoSize = true;
            commitInfoHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitInfoHeader.BackColor = SystemColors.Window;
            commitInfoHeader.Location = new Point(8, 8);
            commitInfoHeader.Margin = new Padding(8, 8, 16, 8);
            commitInfoHeader.Name = "commitInfoHeader";
            commitInfoHeader.Size = new Size(260, 96);
            commitInfoHeader.TabIndex = 0;
            // 
            // RevisionInfo
            // 
            RevisionInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RevisionInfo.BackColor = SystemColors.Window;
            RevisionInfo.BorderStyle = BorderStyle.None;
            RevisionInfo.ContextMenuStrip = commitInfoContextMenuStrip;
            RevisionInfo.Location = new Point(8, 156);
            RevisionInfo.Margin = new Padding(8, 8, 16, 8);
            RevisionInfo.Name = "RevisionInfo";
            RevisionInfo.ReadOnly = true;
            RevisionInfo.ScrollBars = RichTextBoxScrollBars.None;
            RevisionInfo.Size = new Size(448, 98);
            RevisionInfo.TabIndex = 2;
            RevisionInfo.Text = "";
            RevisionInfo.LinkClicked += LinkClicked;
            RevisionInfo.KeyDown += RichTextBox_KeyDown;
            RevisionInfo.MouseDown += _RevisionHeader_MouseDown;
            // 
            // CommitInfo
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            BackColor = SystemColors.Window;
            ContextMenuStrip = commitInfoContextMenuStrip;
            Controls.Add(tableLayout);
            Margin = new Padding(0);
            Name = "CommitInfo";
            Size = new Size(472, 262);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            pnlCommitMessage.ResumeLayout(false);
            pnlCommitMessage.PerformLayout();
            commitInfoContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private RichTextBox RevisionInfo;
        private ContextMenuStrip commitInfoContextMenuStrip;
        private ToolStripMenuItem copyLinkToolStripMenuItem;
        private ToolStripMenuItem showContainedInBranchesToolStripMenuItem;
        private ToolStripMenuItem showContainedInTagsToolStripMenuItem;
        private ToolStripMenuItem copyCommitInfoToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem showContainedInBranchesRemoteToolStripMenuItem;
        private ToolStripMenuItem showContainedInBranchesRemoteIfNoLocalToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem addNoteToolStripMenuItem;
        private ToolStripMenuItem showMessagesOfAnnotatedTagsToolStripMenuItem;
        private ToolStripMenuItem showTagThisCommitDerivesFromMenuItem;
        private CommitInfoHeader commitInfoHeader;
        private Panel pnlCommitMessage;
        private RichTextBox rtbxCommitMessage;
        private TableLayoutPanel tableLayout;
    }
}
