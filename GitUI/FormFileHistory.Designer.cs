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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ViewTab = new System.Windows.Forms.TabPage();
            this.View = new GitUI.FileViewer();
            this.DiffTab = new System.Windows.Forms.TabPage();
            this.Diff = new GitUI.FileViewer();
            this.Blame = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.BlameCommitter = new ICSharpCode.TextEditor.TextEditorControl();
            this.commitInfo = new GitUI.CommitInfo();
            this.BlameFile = new ICSharpCode.TextEditor.TextEditorControl();
            this.gitItemBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.gitBlameBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.subItemsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.DiffTab.SuspendLayout();
            this.Blame.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitBlameBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subItemsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
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
            this.FileChanges.currentCheckout = null;
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Filter = "";
            this.FileChanges.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.FileChanges.LastRow = 0;
            this.FileChanges.Location = new System.Drawing.Point(0, 0);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileChanges.Size = new System.Drawing.Size(748, 111);
            this.FileChanges.TabIndex = 2;
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
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
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
            this.DiffTab.Size = new System.Drawing.Size(742, 304);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.IgnoreWhitespaceChanges = false;
            this.Diff.Location = new System.Drawing.Point(3, 3);
            this.Diff.Name = "Diff";
            this.Diff.NumberOfVisibleLines = 3;
            this.Diff.ScrollPos = 0;
            this.Diff.ShowEntireFile = false;
            this.Diff.Size = new System.Drawing.Size(736, 298);
            this.Diff.TabIndex = 0;
            this.Diff.TreatAllFilesAsText = false;
            // 
            // Blame
            // 
            this.Blame.Controls.Add(this.splitContainer2);
            this.Blame.Location = new System.Drawing.Point(4, 22);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(742, 304);
            this.Blame.TabIndex = 2;
            this.Blame.Text = "Blame";
            this.Blame.UseVisualStyleBackColor = true;
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
            this.splitContainer2.Panel2.Controls.Add(this.commitInfo);
            this.splitContainer2.Panel2.Controls.Add(this.BlameFile);
            this.splitContainer2.Size = new System.Drawing.Size(742, 304);
            this.splitContainer2.SplitterDistance = 247;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // BlameCommitter
            // 
            this.BlameCommitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameCommitter.IsReadOnly = false;
            this.BlameCommitter.Location = new System.Drawing.Point(0, 0);
            this.BlameCommitter.Name = "BlameCommitter";
            this.BlameCommitter.Size = new System.Drawing.Size(245, 302);
            this.BlameCommitter.TabIndex = 5;
            // 
            // commitInfo
            // 
            this.commitInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.commitInfo.Location = new System.Drawing.Point(250, 17);
            this.commitInfo.Name = "commitInfo";
            this.commitInfo.Size = new System.Drawing.Size(193, 85);
            this.commitInfo.TabIndex = 5;
            this.commitInfo.Visible = false;
            // 
            // BlameFile
            // 
            this.BlameFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameFile.IsReadOnly = false;
            this.BlameFile.Location = new System.Drawing.Point(0, 0);
            this.BlameFile.Name = "BlameFile";
            this.BlameFile.Size = new System.Drawing.Size(489, 302);
            this.BlameFile.TabIndex = 4;
            this.BlameFile.Resize += new System.EventHandler(this.BlameFile_Resize);
            // 
            // gitItemBindingSource1
            // 
            this.gitItemBindingSource1.DataSource = typeof(GitCommands.GitItem);
            // 
            // gitBlameBindingSource
            // 
            this.gitBlameBindingSource.DataSource = typeof(GitCommands.GitBlame);
            // 
            // subItemsBindingSource
            // 
            this.subItemsBindingSource.DataMember = "SubItems";
            this.subItemsBindingSource.DataSource = this.gitItemBindingSource1;
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormFileHistory_FormClosing);
            this.Load += new System.EventHandler(this.FormFileHistory_Load);
            this.Shown += new System.EventHandler(this.FormFileHistory_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.DiffTab.ResumeLayout(false);
            this.Blame.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitBlameBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subItemsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ViewTab;
        private System.Windows.Forms.TabPage DiffTab;
        private System.Windows.Forms.TabPage Blame;
        private System.Windows.Forms.BindingSource gitItemBindingSource1;
        private System.Windows.Forms.BindingSource subItemsBindingSource;
        private System.Windows.Forms.BindingSource gitBlameBindingSource;
        private System.Diagnostics.EventLog eventLog1;
        private FileViewer View;
        private FileViewer Diff;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ICSharpCode.TextEditor.TextEditorControl BlameFile;
        private ICSharpCode.TextEditor.TextEditorControl BlameCommitter;
        private RevisionGrid FileChanges;
        private CommitInfo commitInfo;
    }
}