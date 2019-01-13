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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCommitMessage = new System.Windows.Forms.Panel();
            this.rtbxCommitMessage = new System.Windows.Forms.RichTextBox();
            this.commitInfoContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyLinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyCommitInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showContainedInBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showContainedInBranchesRemoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showContainedInTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMessagesOfAnnotatedTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTagThisCommitDerivesFromMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitInfoHeader = new GitUI.CommitInfo.CommitInfoHeader();
            this.RevisionInfo = new System.Windows.Forms.RichTextBox();
            this.tableLayout.SuspendLayout();
            this.pnlCommitMessage.SuspendLayout();
            this.commitInfoContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayout
            // 
            this.tableLayout.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayout.ColumnCount = 1;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayout.Controls.Add(this.commitInfoHeader, 0, 0);
            this.tableLayout.Controls.Add(this.pnlCommitMessage, 0, 1);
            this.tableLayout.Controls.Add(this.RevisionInfo, 0, 2);
            this.tableLayout.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayout.Location = new System.Drawing.Point(0, 0);
            this.tableLayout.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.RowCount = 3;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayout.Size = new System.Drawing.Size(472, 262);
            this.tableLayout.TabIndex = 0;
            this.tableLayout.Visible = false;
            // 
            // pnlCommitMessage
            // 
            this.pnlCommitMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCommitMessage.BackColor = System.Drawing.SystemColors.Control;
            this.pnlCommitMessage.Controls.Add(this.rtbxCommitMessage);
            this.pnlCommitMessage.Location = new System.Drawing.Point(8, 112);
            this.pnlCommitMessage.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.pnlCommitMessage.Name = "pnlCommitMessage";
            this.pnlCommitMessage.Size = new System.Drawing.Size(456, 36);
            this.pnlCommitMessage.TabIndex = 0;
            // 
            // rtbxCommitMessage
            // 
            this.rtbxCommitMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbxCommitMessage.BackColor = System.Drawing.SystemColors.Control;
            this.rtbxCommitMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbxCommitMessage.ContextMenuStrip = this.commitInfoContextMenuStrip;
            this.rtbxCommitMessage.Location = new System.Drawing.Point(8, 8);
            this.rtbxCommitMessage.Margin = new System.Windows.Forms.Padding(8);
            this.rtbxCommitMessage.Name = "rtbxCommitMessage";
            this.rtbxCommitMessage.ReadOnly = true;
            this.rtbxCommitMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbxCommitMessage.Size = new System.Drawing.Size(440, 20);
            this.rtbxCommitMessage.TabIndex = 1;
            this.rtbxCommitMessage.Text = "";
            this.rtbxCommitMessage.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RevisionInfoLinkClicked);
            this.rtbxCommitMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RichTextBox_KeyDown);
            this.rtbxCommitMessage.MouseDown += new System.Windows.Forms.MouseEventHandler(this._RevisionHeader_MouseDown);
            // 
            // commitInfoContextMenuStrip
            // 
            this.commitInfoContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyLinkToolStripMenuItem,
            this.copyCommitInfoToolStripMenuItem,
            this.toolStripSeparator1,
            this.showContainedInBranchesToolStripMenuItem,
            this.showContainedInBranchesRemoteToolStripMenuItem,
            this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem,
            this.showContainedInTagsToolStripMenuItem,
            this.showMessagesOfAnnotatedTagsToolStripMenuItem,
            this.showTagThisCommitDerivesFromMenuItem,
            this.toolStripSeparator2,
            this.addNoteToolStripMenuItem});
            this.commitInfoContextMenuStrip.Name = "commitInfoContextMenuStrip";
            this.commitInfoContextMenuStrip.Size = new System.Drawing.Size(454, 192);
            this.commitInfoContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.commitInfoContextMenuStrip_Opening);
            // 
            // copyLinkStripMenuItem
            // 
            this.copyLinkToolStripMenuItem.Name = "copyLinkStripMenuItem";
            this.copyLinkToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.copyLinkToolStripMenuItem.Text = "Copy link";
            this.copyLinkToolStripMenuItem.Click += new System.EventHandler(this.copyLinkToolStripMenuItem_Click);
            // 
            // copyCommitInfoToolStripMenuItem
            // 
            this.copyCommitInfoToolStripMenuItem.Name = "copyCommitInfoToolStripMenuItem";
            this.copyCommitInfoToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.copyCommitInfoToolStripMenuItem.Text = "&Copy commit info";
            this.copyCommitInfoToolStripMenuItem.Click += new System.EventHandler(this.copyCommitInfoToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(450, 6);
            // 
            // showContainedInBranchesToolStripMenuItem
            // 
            this.showContainedInBranchesToolStripMenuItem.Name = "showContainedInBranchesToolStripMenuItem";
            this.showContainedInBranchesToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.showContainedInBranchesToolStripMenuItem.Text = "Show local branches containing this commit";
            this.showContainedInBranchesToolStripMenuItem.Click += new System.EventHandler(this.showContainedInBranchesToolStripMenuItem_Click);
            // 
            // showContainedInBranchesRemoteToolStripMenuItem
            // 
            this.showContainedInBranchesRemoteToolStripMenuItem.Name = "showContainedInBranchesRemoteToolStripMenuItem";
            this.showContainedInBranchesRemoteToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.showContainedInBranchesRemoteToolStripMenuItem.Text = "Show remote branches containing this commit";
            this.showContainedInBranchesRemoteToolStripMenuItem.Click += new System.EventHandler(this.showContainedInBranchesRemoteToolStripMenuItem_Click);
            // 
            // showContainedInBranchesRemoteIfNoLocalToolStripMenuItem
            // 
            this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Name = "showContainedInBranchesRemoteIfNoLocalToolStripMenuItem";
            this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Text = "Show remote branches only when no local branch contains this commit";
            this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem.Click += new System.EventHandler(this.showContainedInBranchesRemoteIfNoLocalToolStripMenuItem_Click);
            // 
            // showContainedInTagsToolStripMenuItem
            // 
            this.showContainedInTagsToolStripMenuItem.Name = "showContainedInTagsToolStripMenuItem";
            this.showContainedInTagsToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.showContainedInTagsToolStripMenuItem.Text = "Show tags containing this commit";
            this.showContainedInTagsToolStripMenuItem.Click += new System.EventHandler(this.showContainedInTagsToolStripMenuItem_Click);
            // 
            // showMessagesOfAnnotatedTagsToolStripMenuItem
            // 
            this.showMessagesOfAnnotatedTagsToolStripMenuItem.Name = "showMessagesOfAnnotatedTagsToolStripMenuItem";
            this.showMessagesOfAnnotatedTagsToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.showMessagesOfAnnotatedTagsToolStripMenuItem.Text = "Show messages of annotated tags";
            this.showMessagesOfAnnotatedTagsToolStripMenuItem.Click += new System.EventHandler(this.showMessagesOfAnnotatedTagsToolStripMenuItem_Click);
            // 
            // showTagThisCommitDerivesFromMenuItem
            // 
            this.showTagThisCommitDerivesFromMenuItem.Name = "showTagThisCommitDerivesFromMenuItem";
            this.showTagThisCommitDerivesFromMenuItem.Size = new System.Drawing.Size(453, 22);
            this.showTagThisCommitDerivesFromMenuItem.Text = "Show the most recent tag this commit derives from";
            this.showTagThisCommitDerivesFromMenuItem.Click += new System.EventHandler(this.showTagThisCommitDerivesFromMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(450, 6);
            // 
            // addNoteToolStripMenuItem
            // 
            this.addNoteToolStripMenuItem.Name = "addNoteToolStripMenuItem";
            this.addNoteToolStripMenuItem.Size = new System.Drawing.Size(453, 22);
            this.addNoteToolStripMenuItem.Text = "Add &notes";
            this.addNoteToolStripMenuItem.Click += new System.EventHandler(this.addNoteToolStripMenuItem_Click);
            // 
            // commitInfoHeader
            // 
            this.commitInfoHeader.AutoSize = true;
            this.commitInfoHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitInfoHeader.BackColor = System.Drawing.SystemColors.Window;
            this.commitInfoHeader.Location = new System.Drawing.Point(8, 8);
            this.commitInfoHeader.Margin = new System.Windows.Forms.Padding(8, 8, 16, 8);
            this.commitInfoHeader.Name = "commitInfoHeader";
            this.commitInfoHeader.Size = new System.Drawing.Size(260, 96);
            this.commitInfoHeader.TabIndex = 0;
            // 
            // RevisionInfo
            // 
            this.RevisionInfo.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.RevisionInfo.BackColor = System.Drawing.SystemColors.Window;
            this.RevisionInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RevisionInfo.ContextMenuStrip = this.commitInfoContextMenuStrip;
            this.RevisionInfo.Location = new System.Drawing.Point(8, 156);
            this.RevisionInfo.Margin = new System.Windows.Forms.Padding(8, 8, 16, 8);
            this.RevisionInfo.Name = "RevisionInfo";
            this.RevisionInfo.ReadOnly = true;
            this.RevisionInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.RevisionInfo.Size = new System.Drawing.Size(448, 98);
            this.RevisionInfo.TabIndex = 2;
            this.RevisionInfo.Text = "";
            this.RevisionInfo.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RevisionInfoLinkClicked);
            this.RevisionInfo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RichTextBox_KeyDown);
            this.RevisionInfo.MouseDown += new System.Windows.Forms.MouseEventHandler(this._RevisionHeader_MouseDown);
            // 
            // CommitInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayout);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CommitInfo";
            this.Size = new System.Drawing.Size(472, 262);
            this.tableLayout.ResumeLayout(false);
            this.tableLayout.PerformLayout();
            this.pnlCommitMessage.ResumeLayout(false);
            this.pnlCommitMessage.PerformLayout();
            this.commitInfoContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox RevisionInfo;
        private System.Windows.Forms.ContextMenuStrip commitInfoContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyLinkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showContainedInBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showContainedInTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCommitInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showContainedInBranchesRemoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showContainedInBranchesRemoteIfNoLocalToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem addNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMessagesOfAnnotatedTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTagThisCommitDerivesFromMenuItem;
        private CommitInfoHeader commitInfoHeader;
        private System.Windows.Forms.Panel pnlCommitMessage;
        private System.Windows.Forms.RichTextBox rtbxCommitMessage;
        private System.Windows.Forms.TableLayoutPanel tableLayout;
    }
}
