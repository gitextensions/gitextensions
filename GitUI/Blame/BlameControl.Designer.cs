namespace GitUI.Blame
{
    partial class BlameControl
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.commitInfo = new GitUI.CommitInfo();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.BlameCommitter = new GitUI.Editor.FileViewer();
            this.BlameFile = new GitUI.Editor.FileViewer();
            this.blameTooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.commitInfo);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(980, 986);
            this.splitContainer1.SplitterDistance = 241;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 7;
            // 
            // commitInfo
            // 
            this.commitInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.commitInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitInfo.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.commitInfo.Location = new System.Drawing.Point(0, 0);
            this.commitInfo.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.commitInfo.Name = "commitInfo";
            this.commitInfo.Size = new System.Drawing.Size(980, 241);
            this.commitInfo.TabIndex = 5;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.BlameCommitter);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.BlameFile);
            this.splitContainer2.Size = new System.Drawing.Size(980, 739);
            this.splitContainer2.SplitterDistance = 213;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // BlameCommitter
            // 
            this.BlameCommitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameCommitter.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.BlameCommitter.IgnoreWhitespaceChanges = false;
            this.BlameCommitter.IsReadOnly = false;
            this.BlameCommitter.Location = new System.Drawing.Point(0, 0);
            this.BlameCommitter.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.BlameCommitter.Name = "BlameCommitter";
            this.BlameCommitter.NumberOfVisibleLines = 3;
            this.BlameCommitter.ScrollPos = 0;
            this.BlameCommitter.ShowEntireFile = false;
            this.BlameCommitter.ShowLineNumbers = true;
            this.BlameCommitter.Size = new System.Drawing.Size(211, 737);
            this.BlameCommitter.TabIndex = 5;
            this.BlameCommitter.TabStop = false;
            this.BlameCommitter.TreatAllFilesAsText = false;
            // 
            // BlameFile
            // 
            this.BlameFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlameFile.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.BlameFile.IgnoreWhitespaceChanges = false;
            this.BlameFile.IsReadOnly = false;
            this.BlameFile.Location = new System.Drawing.Point(0, 0);
            this.BlameFile.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.BlameFile.Name = "BlameFile";
            this.BlameFile.NumberOfVisibleLines = 3;
            this.BlameFile.ScrollPos = 0;
            this.BlameFile.ShowEntireFile = false;
            this.BlameFile.ShowLineNumbers = true;
            this.BlameFile.Size = new System.Drawing.Size(760, 737);
            this.BlameFile.TabIndex = 0;
            this.BlameFile.TreatAllFilesAsText = false;
            // 
            // BlameControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "BlameControl";
            this.Size = new System.Drawing.Size(980, 986);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private CommitInfo commitInfo;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private GitUI.Editor.FileViewer BlameCommitter;
        private GitUI.Editor.FileViewer BlameFile;
        private System.Windows.Forms.ToolTip blameTooltip;
    }
}
