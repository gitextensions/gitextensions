namespace GitUI
{
    partial class FormRebase
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
            this.label1 = new System.Windows.Forms.Label();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AddFiles = new System.Windows.Forms.Button();
            this.Resolved = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chkInteractive = new System.Windows.Forms.CheckBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.patchGrid1 = new GitUI.PatchGrid();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.ContinuePanel = new System.Windows.Forms.Panel();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(241, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rebase current branch on top of another branch";
            // 
            // Currentbranch
            // 
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(4, 26);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(44, 13);
            this.Currentbranch.TabIndex = 2;
            this.Currentbranch.Text = "Current";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(3, 47);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(116, 23);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "Rebase";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // Branches
            // 
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(88, 47);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(218, 21);
            this.Branches.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Rebase on";
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(3, 152);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(116, 23);
            this.AddFiles.TabIndex = 14;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFilesClick);
            // 
            // Resolved
            // 
            this.Resolved.Location = new System.Drawing.Point(3, 199);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(116, 23);
            this.Resolved.TabIndex = 13;
            this.Resolved.Text = "Continue rebase";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.ResolvedClick);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(3, 257);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(116, 23);
            this.Abort.TabIndex = 12;
            this.Abort.Text = "Abort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.AbortClick);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(3, 228);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(116, 23);
            this.Skip.TabIndex = 11;
            this.Skip.Text = "Skip this commit";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.SkipClick);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(3, 98);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(116, 23);
            this.Mergetool.TabIndex = 10;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SolveMergeconflicts);
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Panel2.Controls.Add(this.Mergetool);
            this.splitContainer1.Panel2.Controls.Add(this.AddFiles);
            this.splitContainer1.Panel2.Controls.Add(this.Abort);
            this.splitContainer1.Panel2.Controls.Add(this.Resolved);
            this.splitContainer1.Panel2.Controls.Add(this.Skip);
            this.splitContainer1.Panel2.Controls.Add(this.ContinuePanel);
            this.splitContainer1.Panel2.Controls.Add(this.MergeToolPanel);
            this.splitContainer1.Size = new System.Drawing.Size(675, 368);
            this.splitContainer1.SplitterDistance = 549;
            this.splitContainer1.TabIndex = 17;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.chkInteractive);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.Currentbranch);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.Branches);
            this.splitContainer2.Panel1MinSize = 0;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Panel2MinSize = 0;
            this.splitContainer2.Size = new System.Drawing.Size(549, 368);
            this.splitContainer2.SplitterDistance = 74;
            this.splitContainer2.TabIndex = 0;
            // 
            // chkInteractive
            // 
            this.chkInteractive.AutoSize = true;
            this.chkInteractive.Location = new System.Drawing.Point(312, 49);
            this.chkInteractive.Name = "chkInteractive";
            this.chkInteractive.Size = new System.Drawing.Size(118, 17);
            this.chkInteractive.TabIndex = 7;
            this.chkInteractive.Text = "Interactive Rebase";
            this.chkInteractive.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.label3);
            this.splitContainer3.Panel1MinSize = 10;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.patchGrid1);
            this.splitContainer3.Size = new System.Drawing.Size(549, 290);
            this.splitContainer3.SplitterDistance = 16;
            this.splitContainer3.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Commits to re-apply:";
            // 
            // patchGrid1
            // 
            this.patchGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patchGrid1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.patchGrid1.Location = new System.Drawing.Point(0, 0);
            this.patchGrid1.Name = "patchGrid1";
            this.patchGrid1.Size = new System.Drawing.Size(549, 270);
            this.patchGrid1.TabIndex = 16;
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(2, 307);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(117, 49);
            this.SolveMergeconflicts.TabIndex = 19;
            this.SolveMergeconflicts.Text = "There are unresolved mergeconflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeconflictsClick);
            // 
            // ContinuePanel
            // 
            this.ContinuePanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ContinuePanel.Location = new System.Drawing.Point(0, 197);
            this.ContinuePanel.Name = "ContinuePanel";
            this.ContinuePanel.Size = new System.Drawing.Size(122, 27);
            this.ContinuePanel.TabIndex = 7;
            // 
            // MergeToolPanel
            // 
            this.MergeToolPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.MergeToolPanel.Location = new System.Drawing.Point(-1, 96);
            this.MergeToolPanel.Name = "MergeToolPanel";
            this.MergeToolPanel.Size = new System.Drawing.Size(122, 27);
            this.MergeToolPanel.TabIndex = 8;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer4.Size = new System.Drawing.Size(759, 368);
            this.splitContainer4.SplitterDistance = 80;
            this.splitContainer4.TabIndex = 18;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BackgroundImage = global::GitUI.Properties.Resources.Rebase;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(80, 368);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // FormRebase
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(759, 368);
            this.Controls.Add(this.splitContainer4);
            this.Name = "FormRebase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rebase";
            this.Load += new System.EventHandler(this.FormRebaseLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRebaseFormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button Resolved;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.ComboBox Branches;
        private PatchGrid patchGrid1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.Panel ContinuePanel;
        private System.Windows.Forms.Panel MergeToolPanel;
        private System.Windows.Forms.CheckBox chkInteractive;
    }
}