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
            if (disposing && (components is not null))
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
            this.PatchGrid = new GitUI.PatchGrid();
            this.SolveMergeConflicts = new System.Windows.Forms.Button();
            this.IgnoreWhitespace = new System.Windows.Forms.CheckBox();
            this.ContinuePanel = new System.Windows.Forms.Panel();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.PanelBR = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.MainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.PanelTR = new System.Windows.Forms.FlowLayoutPanel();
            this.SignOff = new System.Windows.Forms.CheckBox();
            this.PanelTL = new System.Windows.Forms.Panel();
            this.ContinuePanel.SuspendLayout();
            this.MergeToolPanel.SuspendLayout();
            this.PanelBR.SuspendLayout();
            this.MainLayoutPanel.SuspendLayout();
            this.PanelTR.SuspendLayout();
            this.PanelTL.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Location = new System.Drawing.Point(450, 7);
            this.BrowsePatch.Name = "BrowsePatch";
            this.BrowsePatch.Size = new System.Drawing.Size(100, 25);
            this.BrowsePatch.TabIndex = 4;
            this.BrowsePatch.Text = "B&rowse";
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
            this.PatchFile.TabIndex = 3;
            // 
            // Apply
            // 
            this.Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Apply.Location = new System.Drawing.Point(3, 3);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(125, 25);
            this.Apply.TabIndex = 10;
            this.Apply.Text = "Apply patch";
            this.Apply.UseVisualStyleBackColor = true;
            this.Apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // Mergetool
            // 
            this.Mergetool.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Mergetool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Mergetool.FlatAppearance.BorderSize = 0;
            this.Mergetool.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Mergetool.Location = new System.Drawing.Point(3, 3);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(119, 25);
            this.Mergetool.TabIndex = 15;
            this.Mergetool.Text = "&Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Skip
            // 
            this.Skip.Dock = System.Windows.Forms.DockStyle.Top;
            this.Skip.Location = new System.Drawing.Point(3, 151);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(125, 25);
            this.Skip.TabIndex = 21;
            this.Skip.Text = "S&kip patch";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Abort
            // 
            this.Abort.Dock = System.Windows.Forms.DockStyle.Top;
            this.Abort.Location = new System.Drawing.Point(3, 182);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(125, 25);
            this.Abort.TabIndex = 22;
            this.Abort.Text = "A&bort patch";
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
            this.Resolved.TabIndex = 20;
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
            this.AddFiles.TabIndex = 17;
            this.AddFiles.Text = "&Add files";
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
            this.PatchDir.TabIndex = 6;
            // 
            // BrowseDir
            // 
            this.BrowseDir.Enabled = false;
            this.BrowseDir.Location = new System.Drawing.Point(450, 33);
            this.BrowseDir.Name = "BrowseDir";
            this.BrowseDir.Size = new System.Drawing.Size(100, 25);
            this.BrowseDir.TabIndex = 7;
            this.BrowseDir.Text = "Bro&wse";
            this.BrowseDir.UseVisualStyleBackColor = true;
            this.BrowseDir.Click += new System.EventHandler(this.BrowseDir_Click);
            // 
            // PatchDirMode
            // 
            this.PatchDirMode.AutoSize = true;
            this.PatchDirMode.Location = new System.Drawing.Point(10, 35);
            this.PatchDirMode.Name = "PatchDirMode";
            this.PatchDirMode.Size = new System.Drawing.Size(105, 19);
            this.PatchDirMode.TabIndex = 5;
            this.PatchDirMode.Text = "Patch &directory";
            this.PatchDirMode.UseVisualStyleBackColor = true;
            // 
            // PatchFileMode
            // 
            this.PatchFileMode.AutoSize = true;
            this.PatchFileMode.Checked = true;
            this.PatchFileMode.Location = new System.Drawing.Point(10, 10);
            this.PatchFileMode.Name = "PatchFileMode";
            this.PatchFileMode.Size = new System.Drawing.Size(74, 19);
            this.PatchFileMode.TabIndex = 2;
            this.PatchFileMode.TabStop = true;
            this.PatchFileMode.Text = "Patch &file";
            this.PatchFileMode.UseVisualStyleBackColor = true;
            this.PatchFileMode.CheckedChanged += new System.EventHandler(this.PatchFileMode_CheckedChanged);
            // 
            // PatchGrid
            // 
            this.PatchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PatchGrid.IsManagingRebase = false;
            this.PatchGrid.Location = new System.Drawing.Point(3, 89);
            this.PatchGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PatchGrid.Name = "PatchGrid";
            this.PatchGrid.Size = new System.Drawing.Size(568, 345);
            this.PatchGrid.TabIndex = 8;
            this.PatchGrid.TabStop = false;
            // 
            // SolveMergeConflicts
            // 
            this.SolveMergeConflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeConflicts.Dock = System.Windows.Forms.DockStyle.Top;
            this.SolveMergeConflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeConflicts.Location = new System.Drawing.Point(3, 213);
            this.SolveMergeConflicts.Name = "SolveMergeConflicts";
            this.SolveMergeConflicts.Size = new System.Drawing.Size(125, 78);
            this.SolveMergeConflicts.TabIndex = 23;
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
            this.IgnoreWhitespace.TabIndex = 11;
            this.IgnoreWhitespace.Text = "&Ignore Wh.spc.";
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
            this.ContinuePanel.TabIndex = 19;
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
            this.MergeToolPanel.TabIndex = 14;
            // 
            // PanelBR
            // 
            this.PanelBR.AutoSize = true;
            this.PanelBR.Controls.Add(this.MergeToolPanel);
            this.PanelBR.Controls.Add(this.panel2);
            this.PanelBR.Controls.Add(this.AddFiles);
            this.PanelBR.Controls.Add(this.panel3);
            this.PanelBR.Controls.Add(this.ContinuePanel);
            this.PanelBR.Controls.Add(this.Skip);
            this.PanelBR.Controls.Add(this.Abort);
            this.PanelBR.Controls.Add(this.SolveMergeConflicts);
            this.PanelBR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBR.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelBR.Location = new System.Drawing.Point(577, 90);
            this.PanelBR.Name = "PanelBR";
            this.PanelBR.Size = new System.Drawing.Size(131, 343);
            this.PanelBR.TabIndex = 13;
            this.PanelBR.WrapContents = false;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(3, 40);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 15);
            this.panel2.TabIndex = 16;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(3, 92);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 15);
            this.panel3.TabIndex = 18;
            // 
            // MainLayoutPanel
            // 
            this.MainLayoutPanel.ColumnCount = 2;
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayoutPanel.Controls.Add(this.PatchGrid, 0, 1);
            this.MainLayoutPanel.Controls.Add(this.PanelBR, 1, 1);
            this.MainLayoutPanel.Controls.Add(this.PanelTR, 1, 0);
            this.MainLayoutPanel.Controls.Add(this.PanelTL, 0, 0);
            this.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MainLayoutPanel.Name = "MainLayoutPanel";
            this.MainLayoutPanel.RowCount = 2;
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.Size = new System.Drawing.Size(711, 436);
            this.MainLayoutPanel.TabIndex = 0;
            // 
            // PanelTR
            // 
            this.PanelTR.AutoSize = true;
            this.PanelTR.Controls.Add(this.Apply);
            this.PanelTR.Controls.Add(this.IgnoreWhitespace);
            this.PanelTR.Controls.Add(this.SignOff);
            this.PanelTR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTR.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelTR.Location = new System.Drawing.Point(577, 3);
            this.PanelTR.Name = "PanelTR";
            this.PanelTR.Size = new System.Drawing.Size(131, 81);
            this.PanelTR.TabIndex = 9;
            this.PanelTR.WrapContents = false;
            // 
            // SignOff
            // 
            this.SignOff.AutoSize = true;
            this.SignOff.Location = new System.Drawing.Point(3, 59);
            this.SignOff.Name = "SignOff";
            this.SignOff.Size = new System.Drawing.Size(71, 19);
            this.SignOff.TabIndex = 12;
            this.SignOff.Text = "Sign-&Off";
            this.SignOff.UseVisualStyleBackColor = true;
            this.SignOff.CheckedChanged += new System.EventHandler(this.SignOff_CheckedChanged);
            // 
            // PanelTL
            // 
            this.PanelTL.Controls.Add(this.PatchDir);
            this.PanelTL.Controls.Add(this.PatchFileMode);
            this.PanelTL.Controls.Add(this.BrowseDir);
            this.PanelTL.Controls.Add(this.BrowsePatch);
            this.PanelTL.Controls.Add(this.PatchDirMode);
            this.PanelTL.Controls.Add(this.PatchFile);
            this.PanelTL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTL.Location = new System.Drawing.Point(3, 3);
            this.PanelTL.MinimumSize = new System.Drawing.Size(560, 65);
            this.PanelTL.Name = "PanelTL";
            this.PanelTL.Size = new System.Drawing.Size(568, 81);
            this.PanelTL.TabIndex = 1;
            // 
            // FormApplyPatch
            // 
            this.AcceptButton = this.Apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(711, 436);
            this.Controls.Add(this.MainLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(720, 410);
            this.Name = "FormApplyPatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply patch";
            this.Load += new System.EventHandler(this.MergePatch_Load);
            this.ContinuePanel.ResumeLayout(false);
            this.MergeToolPanel.ResumeLayout(false);
            this.PanelBR.ResumeLayout(false);
            this.PanelBR.PerformLayout();
            this.MainLayoutPanel.ResumeLayout(false);
            this.MainLayoutPanel.PerformLayout();
            this.PanelTR.ResumeLayout(false);
            this.PanelTR.PerformLayout();
            this.PanelTL.ResumeLayout(false);
            this.PanelTL.PerformLayout();
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
        private PatchGrid PatchGrid;
        private System.Windows.Forms.TextBox PatchDir;
        private System.Windows.Forms.Button BrowseDir;
        private System.Windows.Forms.RadioButton PatchDirMode;
        private System.Windows.Forms.RadioButton PatchFileMode;
        private System.Windows.Forms.Button SolveMergeConflicts;
        private System.Windows.Forms.CheckBox IgnoreWhitespace;
        private System.Windows.Forms.Panel ContinuePanel;
        private System.Windows.Forms.Panel MergeToolPanel;
        private System.Windows.Forms.FlowLayoutPanel PanelBR;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel MainLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel PanelTR;
        private System.Windows.Forms.Panel PanelTL;
        private System.Windows.Forms.CheckBox SignOff;
    }
}
