namespace GitUI.CommandsDialogs
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
            this.PatchDir = new System.Windows.Forms.TextBox();
            this.BrowseDir = new System.Windows.Forms.Button();
            this.PatchDirMode = new System.Windows.Forms.RadioButton();
            this.PatchFileMode = new System.Windows.Forms.RadioButton();
            this.patchGrid1 = new GitUI.PatchGrid();
            this.SolveMergeConflicts = new System.Windows.Forms.Button();
            this.IgnoreWhitespace = new System.Windows.Forms.CheckBox();
            this.ContinuePanel = new System.Windows.Forms.Panel();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.ContinuePanel.SuspendLayout();
            this.MergeToolPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Location = new System.Drawing.Point(450, 7);
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
            this.PatchFile.Location = new System.Drawing.Point(163, 9);
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
            this.Mergetool.Location = new System.Drawing.Point(3, 3);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(119, 25);
            this.Mergetool.TabIndex = 7;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Skip
            // 
            this.Skip.Dock = System.Windows.Forms.DockStyle.Top;
            this.Skip.Location = new System.Drawing.Point(3, 151);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(125, 25);
            this.Skip.TabIndex = 10;
            this.Skip.Text = "Skip patch";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Abort
            // 
            this.Abort.Dock = System.Windows.Forms.DockStyle.Top;
            this.Abort.Location = new System.Drawing.Point(3, 182);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(125, 25);
            this.Abort.TabIndex = 11;
            this.Abort.Text = "Abort patch";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // Resolved
            // 
            this.Resolved.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Resolved.Location = new System.Drawing.Point(3, 3);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(119, 25);
            this.Resolved.TabIndex = 9;
            this.Resolved.Text = "Conflicts resolved";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.Resolved_Click);
            // 
            // AddFiles
            // 
            this.AddFiles.Dock = System.Windows.Forms.DockStyle.Top;
            this.AddFiles.Location = new System.Drawing.Point(3, 61);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(125, 25);
            this.AddFiles.TabIndex = 8;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFiles_Click);
            // 
            // PatchDir
            // 
            this.PatchDir.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.PatchDir.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.PatchDir.Enabled = false;
            this.PatchDir.Location = new System.Drawing.Point(163, 35);
            this.PatchDir.Name = "PatchDir";
            this.PatchDir.Size = new System.Drawing.Size(281, 23);
            this.PatchDir.TabIndex = 1;
            // 
            // BrowseDir
            // 
            this.BrowseDir.Enabled = false;
            this.BrowseDir.Location = new System.Drawing.Point(450, 33);
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
            this.PatchDirMode.Location = new System.Drawing.Point(10, 35);
            this.PatchDirMode.Name = "PatchDirMode";
            this.PatchDirMode.Size = new System.Drawing.Size(72, 19);
            this.PatchDirMode.TabIndex = 1;
            this.PatchDirMode.Text = "Patch directory";
            this.PatchDirMode.UseVisualStyleBackColor = true;
            // 
            // PatchFileMode
            // 
            this.PatchFileMode.AutoSize = true;
            this.PatchFileMode.Checked = true;
            this.PatchFileMode.Location = new System.Drawing.Point(10, 10);
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
            this.patchGrid1.Location = new System.Drawing.Point(3, 73);
            this.patchGrid1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.patchGrid1.Name = "patchGrid1";
            this.patchGrid1.Size = new System.Drawing.Size(568, 361);
            this.patchGrid1.TabIndex = 10;
            this.patchGrid1.TabStop = false;
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeConflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeConflicts.Dock = System.Windows.Forms.DockStyle.Top;
            this.SolveMergeConflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeConflicts.Location = new System.Drawing.Point(3, 213);
            this.SolveMergeConflicts.Name = "SolveMergeConflicts";
            this.SolveMergeConflicts.Size = new System.Drawing.Size(125, 78);
            this.SolveMergeConflicts.TabIndex = 12;
            this.SolveMergeConflicts.Text = "There are unresolved merge conflicts\r\n";
            this.SolveMergeConflicts.UseVisualStyleBackColor = false;
            this.SolveMergeConflicts.Visible = false;
            this.SolveMergeConflicts.Click += new System.EventHandler(this.SolveMergeConflicts_Click);
            // 
            // IgnoreWhitespace
            // 
            this.IgnoreWhitespace.AutoSize = true;
            this.IgnoreWhitespace.Location = new System.Drawing.Point(3, 34);
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
            this.ContinuePanel.Controls.Add(this.Resolved);
            this.ContinuePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ContinuePanel.Location = new System.Drawing.Point(3, 113);
            this.ContinuePanel.Name = "ContinuePanel";
            this.ContinuePanel.Size = new System.Drawing.Size(125, 32);
            this.ContinuePanel.TabIndex = 12;
            // 
            // MergeToolPanel
            // 
            this.MergeToolPanel.AutoSize = true;
            this.MergeToolPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.MergeToolPanel.Controls.Add(this.Mergetool);
            this.MergeToolPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.MergeToolPanel.Location = new System.Drawing.Point(3, 3);
            this.MergeToolPanel.Name = "MergeToolPanel";
            this.MergeToolPanel.Size = new System.Drawing.Size(125, 31);
            this.MergeToolPanel.TabIndex = 13;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.MergeToolPanel);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Controls.Add(this.AddFiles);
            this.flowLayoutPanel1.Controls.Add(this.panel3);
            this.flowLayoutPanel1.Controls.Add(this.ContinuePanel);
            this.flowLayoutPanel1.Controls.Add(this.Skip);
            this.flowLayoutPanel1.Controls.Add(this.Abort);
            this.flowLayoutPanel1.Controls.Add(this.SolveMergeConflicts);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(577, 74);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(131, 359);
            this.flowLayoutPanel1.TabIndex = 11;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(3, 40);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 15);
            this.panel2.TabIndex = 15;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(3, 92);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 15);
            this.panel3.TabIndex = 16;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.patchGrid1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(711, 436);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.PatchDir);
            this.panel4.Controls.Add(this.PatchFileMode);
            this.panel4.Controls.Add(this.BrowseDir);
            this.panel4.Controls.Add(this.BrowsePatch);
            this.panel4.Controls.Add(this.PatchDirMode);
            this.panel4.Controls.Add(this.PatchFile);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.MinimumSize = new System.Drawing.Size(560, 65);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(568, 65);
            this.panel4.TabIndex = 0;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.Apply);
            this.flowLayoutPanel2.Controls.Add(this.IgnoreWhitespace);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(577, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(131, 65);
            this.flowLayoutPanel2.TabIndex = 1;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // FormApplyPatch
            // 
            this.AcceptButton = this.Apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(711, 436);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(720, 410);
            this.Name = "FormApplyPatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply patch";
            this.Load += new System.EventHandler(this.MergePatch_Load);
            this.ContinuePanel.ResumeLayout(false);
            this.MergeToolPanel.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
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
        private System.Windows.Forms.TextBox PatchDir;
        private System.Windows.Forms.Button BrowseDir;
        private System.Windows.Forms.RadioButton PatchDirMode;
        private System.Windows.Forms.RadioButton PatchFileMode;
        private System.Windows.Forms.Button SolveMergeConflicts;
        private System.Windows.Forms.CheckBox IgnoreWhitespace;
        private System.Windows.Forms.Panel ContinuePanel;
        private System.Windows.Forms.Panel MergeToolPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Panel panel4;
    }
}
