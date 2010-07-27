using GitUI.Editor;

namespace GitUI
{
    partial class FormFileHistory
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.FileChanges = new GitUI.RevisionGrid();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ViewTab = new System.Windows.Forms.TabPage();
            this.View = new GitUI.Editor.FileViewer();
            this.DiffTab = new System.Windows.Forms.TabPage();
            this.Diff = new GitUI.Editor.FileViewer();
            this.Blame = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.commitInfo = new GitUI.CommitInfo();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.BlameCommitter = new ICSharpCode.TextEditor.TextEditorControl();
            this.BlameFile = new ICSharpCode.TextEditor.TextEditorControl();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.DiffTab.SuspendLayout();
            this.Blame.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(748, 444);
            this.splitContainer1.SplitterDistance = 111;
            this.splitContainer1.TabIndex = 0;
            // 
            // FileChanges
            // 
            this.FileChanges.BranchFilter = "";
            this.FileChanges.ContextMenuStrip = this.DiffContextMenu;
            this.FileChanges.CurrentCheckout = "\r\nDer Befehl \"git.exe\" ist entweder falsch geschrieben oder\r\nkonnte nicht gefunde" +
                "n werden.\r\n";
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Filter = "";
            this.FileChanges.LastRow = 0;
            this.FileChanges.Location = new System.Drawing.Point(0, 0);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileChanges.Size = new System.Drawing.Size(748, 111);
            this.FileChanges.TabIndex = 2;
            this.FileChanges.DoubleClick += new System.EventHandler(this.FileChangesDoubleClick);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(172, 48);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.ViewTab);
            this.tabControl1.Controls.Add(this.DiffTab);
            this.tabControl1.Controls.Add(this.Blame);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(748, 329);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // ViewTab
            // 
            this.ViewTab.Controls.Add(this.View);
            this.ViewTab.Location = new System.Drawing.Point(4, 22);
            this.ViewTab.Name = "ViewTab";
            this.ViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.ViewTab.Size = new System.Drawing.Size(740, 303);
            this.ViewTab.TabIndex = 0;
            this.ViewTab.Text = "View";
            this.ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.IgnoreWhitespaceChanges = false;
            this.View.IsReadOnly = true;
            this.View.Location = new System.Drawing.Point(3, 3);
            this.View.Name = "View";
            this.View.NumberOfVisibleLines = 3;
            this.View.ScrollPos = 0;
            this.View.ShowEntireFile = false;
            this.View.Size = new System.Drawing.Size(734, 297);
            this.View.TabIndex = 0;
            this.View.TreatAllFilesAsText = false;
            // 
            // DiffTab
            // 
            this.DiffTab.Controls.Add(this.Diff);
            this.DiffTab.Location = new System.Drawing.Point(4, 22);
            this.DiffTab.Name = "DiffTab";
            this.DiffTab.Padding = new System.Windows.Forms.Padding(3);
            this.DiffTab.Size = new System.Drawing.Size(740, 303);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.IgnoreWhitespaceChanges = false;
            this.Diff.IsReadOnly = true;
            this.Diff.Location = new System.Drawing.Point(3, 3);
            this.Diff.Name = "Diff";
            this.Diff.NumberOfVisibleLines = 3;
            this.Diff.ScrollPos = 0;
            this.Diff.ShowEntireFile = false;
            this.Diff.Size = new System.Drawing.Size(734, 297);
            this.Diff.TabIndex = 0;
            this.Diff.TreatAllFilesAsText = false;
            // 
            // Blame
            // 
            this.Blame.Controls.Add(this.splitContainer3);
            this.Blame.Location = new System.Drawing.Point(4, 22);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(740, 303);
            this.Blame.TabIndex = 2;
            this.Blame.Text = "Blame";
            this.Blame.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.commitInfo);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer3.Size = new System.Drawing.Size(740, 303);
            this.splitContainer3.SplitterDistance = 51;
            this.splitContainer3.TabIndex = 6;
            // 
            // commitInfo
            // 
            this.commitInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.commitInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitInfo.Location = new System.Drawing.Point(0, 0);
            this.commitInfo.Name = "commitInfo";
            this.commitInfo.Size = new System.Drawing.Size(740, 51);
            this.commitInfo.TabIndex = 5;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.BlameCommitter);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.BlameFile);
            this.splitContainer2.Size = new System.Drawing.Size(740, 248);
            this.splitContainer2.SplitterDistance = 246;
            this.splitContainer2.TabIndex = 0;
            // 
            // BlameCommitter
            // 
            this.BlameCommitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameCommitter.IsReadOnly = false;
            this.BlameCommitter.Location = new System.Drawing.Point(0, 0);
            this.BlameCommitter.Name = "BlameCommitter";
            this.BlameCommitter.Size = new System.Drawing.Size(244, 246);
            this.BlameCommitter.TabIndex = 5;
            // 
            // BlameFile
            // 
            this.BlameFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameFile.IsReadOnly = false;
            this.BlameFile.Location = new System.Drawing.Point(0, 0);
            this.BlameFile.Name = "BlameFile";
            this.BlameFile.Size = new System.Drawing.Size(488, 246);
            this.BlameFile.TabIndex = 4;
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // FormFileHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 444);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormFileHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File History";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormFileHistoryFormClosing);
            this.Load += new System.EventHandler(this.FormFileHistoryLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.DiffTab.ResumeLayout(false);
            this.Blame.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ViewTab;
        private System.Windows.Forms.TabPage DiffTab;
        private System.Windows.Forms.TabPage Blame;
        private System.Diagnostics.EventLog eventLog1;
        private FileViewer View;
        private FileViewer Diff;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ICSharpCode.TextEditor.TextEditorControl BlameFile;
        private ICSharpCode.TextEditor.TextEditorControl BlameCommitter;
        private RevisionGrid FileChanges;
        private CommitInfo commitInfo;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer3;
    }
}