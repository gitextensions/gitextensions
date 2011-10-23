namespace GitUI
{
    partial class FormApplyPatch
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
            this.BrowsePatch = new System.Windows.Forms.Button();
            this.PatchFile = new System.Windows.Forms.TextBox();
            this.Apply = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Resolved = new System.Windows.Forms.Button();
            this.AddFiles = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.PatchDir = new System.Windows.Forms.TextBox();
            this.BrowseDir = new System.Windows.Forms.Button();
            this.PatchDirMode = new System.Windows.Forms.RadioButton();
            this.PatchFileMode = new System.Windows.Forms.RadioButton();
            this.patchGrid1 = new GitUI.PatchGrid();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.IgnoreWhitespace = new System.Windows.Forms.CheckBox();
            this.ContinuePanel = new System.Windows.Forms.Panel();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Location = new System.Drawing.Point(453, 8);
            this.BrowsePatch.Name = "BrowsePatch";
            this.BrowsePatch.Size = new System.Drawing.Size(100, 25);
            this.BrowsePatch.TabIndex = 2;
            this.BrowsePatch.Text = "Browse";
            this.BrowsePatch.UseVisualStyleBackColor = true;
            this.BrowsePatch.Click += new System.EventHandler(this.BrowsePatch_Click);
            // 
            // PatchFile
            // 
            this.PatchFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.PatchFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.PatchFile.Location = new System.Drawing.Point(166, 10);
            this.PatchFile.Name = "PatchFile";
            this.PatchFile.Size = new System.Drawing.Size(281, 23);
            this.PatchFile.TabIndex = 1;
            // 
            // Apply
            // 
            this.Apply.Location = new System.Drawing.Point(3, 3);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(125, 25);
            this.Apply.TabIndex = 6;
            this.Apply.Text = "Apply patch";
            this.Apply.UseVisualStyleBackColor = true;
            this.Apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // Mergetool
            // 
            this.Mergetool.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Mergetool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Mergetool.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Mergetool.FlatAppearance.BorderSize = 0;
            this.Mergetool.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Mergetool.Location = new System.Drawing.Point(3, 76);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(125, 25);
            this.Mergetool.TabIndex = 7;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(2, 209);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(125, 25);
            this.Skip.TabIndex = 10;
            this.Skip.Text = "Skip patch";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(2, 238);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(125, 25);
            this.Abort.TabIndex = 11;
            this.Abort.Text = "Abort patch";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // Resolved
            // 
            this.Resolved.Location = new System.Drawing.Point(3, 180);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(125, 25);
            this.Resolved.TabIndex = 9;
            this.Resolved.Text = "Conflicts resolved";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.Resolved_Click);
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(3, 131);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(125, 25);
            this.AddFiles.TabIndex = 8;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFiles_Click);
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
            this.splitContainer1.Panel2.Controls.Add(this.Apply);
            this.splitContainer1.Panel2.Controls.Add(this.Mergetool);
            this.splitContainer1.Panel2.Controls.Add(this.AddFiles);
            this.splitContainer1.Panel2.Controls.Add(this.Skip);
            this.splitContainer1.Panel2.Controls.Add(this.Resolved);
            this.splitContainer1.Panel2.Controls.Add(this.Abort);
            this.splitContainer1.Panel2.Controls.Add(this.IgnoreWhitespace);
            this.splitContainer1.Panel2.Controls.Add(this.ContinuePanel);
            this.splitContainer1.Panel2.Controls.Add(this.MergeToolPanel);
            this.splitContainer1.Size = new System.Drawing.Size(764, 391);
            this.splitContainer1.SplitterDistance = 629;
            this.splitContainer1.TabIndex = 11;
            this.splitContainer1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.PatchDir);
            this.splitContainer2.Panel1.Controls.Add(this.BrowseDir);
            this.splitContainer2.Panel1.Controls.Add(this.PatchDirMode);
            this.splitContainer2.Panel1.Controls.Add(this.PatchFileMode);
            this.splitContainer2.Panel1.Controls.Add(this.PatchFile);
            this.splitContainer2.Panel1.Controls.Add(this.BrowsePatch);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.patchGrid1);
            this.splitContainer2.Size = new System.Drawing.Size(629, 391);
            this.splitContainer2.SplitterDistance = 72;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.TabStop = false;
            // 
            // PatchDir
            // 
            this.PatchDir.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.PatchDir.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.PatchDir.Enabled = false;
            this.PatchDir.Location = new System.Drawing.Point(166, 36);
            this.PatchDir.Name = "PatchDir";
            this.PatchDir.Size = new System.Drawing.Size(281, 23);
            this.PatchDir.TabIndex = 1;
            // 
            // BrowseDir
            // 
            this.BrowseDir.Enabled = false;
            this.BrowseDir.Location = new System.Drawing.Point(453, 34);
            this.BrowseDir.Name = "BrowseDir";
            this.BrowseDir.Size = new System.Drawing.Size(100, 25);
            this.BrowseDir.TabIndex = 2;
            this.BrowseDir.Text = "Browse";
            this.BrowseDir.UseVisualStyleBackColor = true;
            this.BrowseDir.Click += new System.EventHandler(this.BrowseDir_Click);
            // 
            // PatchDirMode
            // 
            this.PatchDirMode.AutoSize = true;
            this.PatchDirMode.Location = new System.Drawing.Point(13, 36);
            this.PatchDirMode.Name = "PatchDirMode";
            this.PatchDirMode.Size = new System.Drawing.Size(72, 19);
            this.PatchDirMode.TabIndex = 1;
            this.PatchDirMode.Text = "Patch dir";
            this.PatchDirMode.UseVisualStyleBackColor = true;
            this.PatchDirMode.CheckedChanged += new System.EventHandler(this.PatchDirMode_CheckedChanged);
            // 
            // PatchFileMode
            // 
            this.PatchFileMode.AutoSize = true;
            this.PatchFileMode.Checked = true;
            this.PatchFileMode.Location = new System.Drawing.Point(13, 11);
            this.PatchFileMode.Name = "PatchFileMode";
            this.PatchFileMode.Size = new System.Drawing.Size(74, 19);
            this.PatchFileMode.TabIndex = 0;
            this.PatchFileMode.TabStop = true;
            this.PatchFileMode.Text = "Patch file";
            this.PatchFileMode.UseVisualStyleBackColor = true;
            this.PatchFileMode.CheckedChanged += new System.EventHandler(this.PatchFileMode_CheckedChanged);
            // 
            // patchGrid1
            // 
            this.patchGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patchGrid1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.patchGrid1.Location = new System.Drawing.Point(0, 0);
            this.patchGrid1.Name = "patchGrid1";
            this.patchGrid1.Size = new System.Drawing.Size(629, 315);
            this.patchGrid1.TabIndex = 10;
            this.patchGrid1.TabStop = false;
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(3, 267);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(120, 78);
            this.SolveMergeconflicts.TabIndex = 12;
            this.SolveMergeconflicts.Text = "There are unresolved mergeconflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeconflicts_Click);
            // 
            // IgnoreWhitespace
            // 
            this.IgnoreWhitespace.AutoSize = true;
            this.IgnoreWhitespace.Location = new System.Drawing.Point(8, 34);
            this.IgnoreWhitespace.Name = "IgnoreWhitespace";
            this.IgnoreWhitespace.Size = new System.Drawing.Size(105, 19);
            this.IgnoreWhitespace.TabIndex = 5;
            this.IgnoreWhitespace.Text = "Ignore Wh.spc.";
            this.IgnoreWhitespace.UseVisualStyleBackColor = true;
            this.IgnoreWhitespace.CheckedChanged += new System.EventHandler(this.IgnoreWhitespace_CheckedChanged);
            // 
            // ContinuePanel
            // 
            this.ContinuePanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ContinuePanel.Location = new System.Drawing.Point(6, 175);
            this.ContinuePanel.Name = "ContinuePanel";
            this.ContinuePanel.Size = new System.Drawing.Size(122, 34);
            this.ContinuePanel.TabIndex = 12;
            // 
            // MergeToolPanel
            // 
            this.MergeToolPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.MergeToolPanel.Location = new System.Drawing.Point(5, 72);
            this.MergeToolPanel.Name = "MergeToolPanel";
            this.MergeToolPanel.Size = new System.Drawing.Size(122, 34);
            this.MergeToolPanel.TabIndex = 13;
            // 
            // FormApplyPatch
            // 
            this.AcceptButton = this.Apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 391);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormApplyPatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply patch";
            this.Load += new System.EventHandler(this.MergePatch_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MergePatch_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BrowsePatch;
        private System.Windows.Forms.TextBox PatchFile;
        private System.Windows.Forms.Button Apply;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Resolved;
        private System.Windows.Forms.Button AddFiles;
        private PatchGrid patchGrid1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox PatchDir;
        private System.Windows.Forms.Button BrowseDir;
        private System.Windows.Forms.RadioButton PatchDirMode;
        private System.Windows.Forms.RadioButton PatchFileMode;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.CheckBox IgnoreWhitespace;
        private System.Windows.Forms.Panel ContinuePanel;
        private System.Windows.Forms.Panel MergeToolPanel;
    }
}
