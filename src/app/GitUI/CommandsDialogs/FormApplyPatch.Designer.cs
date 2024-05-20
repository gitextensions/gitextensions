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
            BrowsePatch = new Button();
            PatchFile = new TextBox();
            Apply = new Button();
            Mergetool = new Button();
            Skip = new Button();
            Abort = new Button();
            Resolved = new Button();
            AddFiles = new Button();
            PatchDir = new TextBox();
            BrowseDir = new Button();
            PatchDirMode = new RadioButton();
            PatchFileMode = new RadioButton();
            PatchGrid = new GitUI.PatchGrid();
            SolveMergeConflicts = new Button();
            IgnoreWhitespace = new CheckBox();
            ContinuePanel = new Panel();
            MergeToolPanel = new Panel();
            PanelBR = new FlowLayoutPanel();
            panel2 = new Panel();
            panel3 = new Panel();
            MainLayoutPanel = new TableLayoutPanel();
            PanelTR = new FlowLayoutPanel();
            SignOff = new CheckBox();
            PanelTL = new Panel();
            ContinuePanel.SuspendLayout();
            MergeToolPanel.SuspendLayout();
            PanelBR.SuspendLayout();
            MainLayoutPanel.SuspendLayout();
            PanelTR.SuspendLayout();
            PanelTL.SuspendLayout();
            SuspendLayout();
            // 
            // BrowsePatch
            // 
            BrowsePatch.Location = new Point(450, 7);
            BrowsePatch.Name = "BrowsePatch";
            BrowsePatch.Size = new Size(100, 25);
            BrowsePatch.TabIndex = 4;
            BrowsePatch.Text = "B&rowse";
            BrowsePatch.UseVisualStyleBackColor = true;
            BrowsePatch.Click += BrowsePatch_Click;
            // 
            // PatchFile
            // 
            PatchFile.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            PatchFile.AutoCompleteSource = AutoCompleteSource.FileSystem;
            PatchFile.Location = new Point(163, 9);
            PatchFile.Name = "PatchFile";
            PatchFile.Size = new Size(281, 23);
            PatchFile.TabIndex = 3;
            // 
            // Apply
            // 
            Apply.DialogResult = DialogResult.OK;
            Apply.Location = new Point(3, 3);
            Apply.Name = "Apply";
            Apply.Size = new Size(125, 25);
            Apply.TabIndex = 10;
            Apply.Text = "Apply patch";
            Apply.UseVisualStyleBackColor = true;
            Apply.Click += Apply_Click;
            // 
            // Mergetool
            // 
            Mergetool.BackColor = SystemColors.ControlDark;
            Mergetool.BackgroundImageLayout = ImageLayout.None;
            Mergetool.FlatAppearance.BorderSize = 0;
            Mergetool.ImageAlign = ContentAlignment.MiddleLeft;
            Mergetool.Location = new Point(3, 3);
            Mergetool.Name = "Mergetool";
            Mergetool.Size = new Size(119, 25);
            Mergetool.TabIndex = 15;
            Mergetool.Text = "&Solve conflicts";
            Mergetool.UseVisualStyleBackColor = true;
            Mergetool.Click += Mergetool_Click;
            // 
            // Skip
            // 
            Skip.Dock = DockStyle.Top;
            Skip.Location = new Point(3, 151);
            Skip.Name = "Skip";
            Skip.Size = new Size(125, 25);
            Skip.TabIndex = 21;
            Skip.Text = "S&kip patch";
            Skip.UseVisualStyleBackColor = true;
            Skip.Click += Skip_Click;
            // 
            // Abort
            // 
            Abort.Dock = DockStyle.Top;
            Abort.Location = new Point(3, 182);
            Abort.Name = "Abort";
            Abort.Size = new Size(125, 25);
            Abort.TabIndex = 22;
            Abort.Text = "A&bort patch";
            Abort.UseVisualStyleBackColor = true;
            Abort.Click += Abort_Click;
            // 
            // Resolved
            // 
            Resolved.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Resolved.Location = new Point(3, 3);
            Resolved.Name = "Resolved";
            Resolved.Size = new Size(119, 25);
            Resolved.TabIndex = 20;
            Resolved.Text = "Conflicts resolved";
            Resolved.UseVisualStyleBackColor = true;
            Resolved.Click += Resolved_Click;
            // 
            // AddFiles
            // 
            AddFiles.Dock = DockStyle.Top;
            AddFiles.Location = new Point(3, 61);
            AddFiles.Name = "AddFiles";
            AddFiles.Size = new Size(125, 25);
            AddFiles.TabIndex = 17;
            AddFiles.Text = "&Add files";
            AddFiles.UseVisualStyleBackColor = true;
            AddFiles.Click += AddFiles_Click;
            // 
            // PatchDir
            // 
            PatchDir.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            PatchDir.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            PatchDir.Enabled = false;
            PatchDir.Location = new Point(163, 35);
            PatchDir.Name = "PatchDir";
            PatchDir.Size = new Size(281, 23);
            PatchDir.TabIndex = 6;
            // 
            // BrowseDir
            // 
            BrowseDir.Enabled = false;
            BrowseDir.Location = new Point(450, 33);
            BrowseDir.Name = "BrowseDir";
            BrowseDir.Size = new Size(100, 25);
            BrowseDir.TabIndex = 7;
            BrowseDir.Text = "Bro&wse";
            BrowseDir.UseVisualStyleBackColor = true;
            BrowseDir.Click += BrowseDir_Click;
            // 
            // PatchDirMode
            // 
            PatchDirMode.AutoSize = true;
            PatchDirMode.Location = new Point(10, 35);
            PatchDirMode.Name = "PatchDirMode";
            PatchDirMode.Size = new Size(105, 19);
            PatchDirMode.TabIndex = 5;
            PatchDirMode.Text = "Patch &directory";
            PatchDirMode.UseVisualStyleBackColor = true;
            // 
            // PatchFileMode
            // 
            PatchFileMode.AutoSize = true;
            PatchFileMode.Checked = true;
            PatchFileMode.Location = new Point(10, 10);
            PatchFileMode.Name = "PatchFileMode";
            PatchFileMode.Size = new Size(74, 19);
            PatchFileMode.TabIndex = 2;
            PatchFileMode.TabStop = true;
            PatchFileMode.Text = "Patch &file";
            PatchFileMode.UseVisualStyleBackColor = true;
            PatchFileMode.CheckedChanged += PatchFileMode_CheckedChanged;
            // 
            // PatchGrid
            // 
            PatchGrid.Dock = DockStyle.Fill;
            PatchGrid.IsManagingRebase = false;
            PatchGrid.Location = new Point(3, 89);
            PatchGrid.Margin = new Padding(3, 2, 3, 2);
            PatchGrid.Name = "PatchGrid";
            PatchGrid.Size = new Size(568, 345);
            PatchGrid.TabIndex = 8;
            PatchGrid.TabStop = false;
            // 
            // SolveMergeConflicts
            // 
            SolveMergeConflicts.BackColor = Color.Salmon;
            SolveMergeConflicts.Dock = DockStyle.Top;
            SolveMergeConflicts.FlatStyle = FlatStyle.Flat;
            SolveMergeConflicts.Location = new Point(3, 213);
            SolveMergeConflicts.Name = "SolveMergeConflicts";
            SolveMergeConflicts.Size = new Size(125, 78);
            SolveMergeConflicts.TabIndex = 23;
            SolveMergeConflicts.Text = "There are unresolved merge conflicts\r\n";
            SolveMergeConflicts.UseVisualStyleBackColor = false;
            SolveMergeConflicts.Visible = false;
            SolveMergeConflicts.Click += SolveMergeConflicts_Click;
            // 
            // IgnoreWhitespace
            // 
            IgnoreWhitespace.AutoSize = true;
            IgnoreWhitespace.Location = new Point(3, 34);
            IgnoreWhitespace.Name = "IgnoreWhitespace";
            IgnoreWhitespace.Size = new Size(105, 19);
            IgnoreWhitespace.TabIndex = 11;
            IgnoreWhitespace.Text = "&Ignore Wh.spc.";
            IgnoreWhitespace.UseVisualStyleBackColor = true;
            IgnoreWhitespace.CheckedChanged += IgnoreWhitespace_CheckedChanged;
            // 
            // ContinuePanel
            // 
            ContinuePanel.BackColor = SystemColors.ActiveCaption;
            ContinuePanel.Controls.Add(Resolved);
            ContinuePanel.Dock = DockStyle.Top;
            ContinuePanel.Location = new Point(3, 113);
            ContinuePanel.Name = "ContinuePanel";
            ContinuePanel.Size = new Size(125, 32);
            ContinuePanel.TabIndex = 19;
            // 
            // MergeToolPanel
            // 
            MergeToolPanel.AutoSize = true;
            MergeToolPanel.BackColor = SystemColors.ActiveCaption;
            MergeToolPanel.Controls.Add(Mergetool);
            MergeToolPanel.Dock = DockStyle.Top;
            MergeToolPanel.Location = new Point(3, 3);
            MergeToolPanel.Name = "MergeToolPanel";
            MergeToolPanel.Size = new Size(125, 31);
            MergeToolPanel.TabIndex = 14;
            // 
            // PanelBR
            // 
            PanelBR.AutoSize = true;
            PanelBR.Controls.Add(MergeToolPanel);
            PanelBR.Controls.Add(panel2);
            PanelBR.Controls.Add(AddFiles);
            PanelBR.Controls.Add(panel3);
            PanelBR.Controls.Add(ContinuePanel);
            PanelBR.Controls.Add(Skip);
            PanelBR.Controls.Add(Abort);
            PanelBR.Controls.Add(SolveMergeConflicts);
            PanelBR.Dock = DockStyle.Fill;
            PanelBR.FlowDirection = FlowDirection.TopDown;
            PanelBR.Location = new Point(577, 90);
            PanelBR.Name = "PanelBR";
            PanelBR.Size = new Size(131, 343);
            PanelBR.TabIndex = 13;
            PanelBR.WrapContents = false;
            // 
            // panel2
            // 
            panel2.Location = new Point(3, 40);
            panel2.Name = "panel2";
            panel2.Size = new Size(10, 15);
            panel2.TabIndex = 16;
            // 
            // panel3
            // 
            panel3.Location = new Point(3, 92);
            panel3.Name = "panel3";
            panel3.Size = new Size(10, 15);
            panel3.TabIndex = 18;
            // 
            // MainLayoutPanel
            // 
            MainLayoutPanel.ColumnCount = 2;
            MainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            MainLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            MainLayoutPanel.Controls.Add(PatchGrid, 0, 1);
            MainLayoutPanel.Controls.Add(PanelBR, 1, 1);
            MainLayoutPanel.Controls.Add(PanelTR, 1, 0);
            MainLayoutPanel.Controls.Add(PanelTL, 0, 0);
            MainLayoutPanel.Dock = DockStyle.Fill;
            MainLayoutPanel.Location = new Point(0, 0);
            MainLayoutPanel.Name = "MainLayoutPanel";
            MainLayoutPanel.RowCount = 2;
            MainLayoutPanel.RowStyles.Add(new RowStyle());
            MainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            MainLayoutPanel.Size = new Size(711, 436);
            MainLayoutPanel.TabIndex = 0;
            // 
            // PanelTR
            // 
            PanelTR.AutoSize = true;
            PanelTR.Controls.Add(Apply);
            PanelTR.Controls.Add(IgnoreWhitespace);
            PanelTR.Controls.Add(SignOff);
            PanelTR.Dock = DockStyle.Fill;
            PanelTR.FlowDirection = FlowDirection.TopDown;
            PanelTR.Location = new Point(577, 3);
            PanelTR.Name = "PanelTR";
            PanelTR.Size = new Size(131, 81);
            PanelTR.TabIndex = 9;
            PanelTR.WrapContents = false;
            // 
            // SignOff
            // 
            SignOff.AutoSize = true;
            SignOff.Location = new Point(3, 59);
            SignOff.Name = "SignOff";
            SignOff.Size = new Size(71, 19);
            SignOff.TabIndex = 12;
            SignOff.Text = "Sign-&Off";
            SignOff.UseVisualStyleBackColor = true;
            SignOff.CheckedChanged += SignOff_CheckedChanged;
            // 
            // PanelTL
            // 
            PanelTL.Controls.Add(PatchDir);
            PanelTL.Controls.Add(PatchFileMode);
            PanelTL.Controls.Add(BrowseDir);
            PanelTL.Controls.Add(BrowsePatch);
            PanelTL.Controls.Add(PatchDirMode);
            PanelTL.Controls.Add(PatchFile);
            PanelTL.Dock = DockStyle.Fill;
            PanelTL.Location = new Point(3, 3);
            PanelTL.MinimumSize = new Size(560, 65);
            PanelTL.Name = "PanelTL";
            PanelTL.Size = new Size(568, 81);
            PanelTL.TabIndex = 1;
            // 
            // FormApplyPatch
            // 
            AcceptButton = Apply;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(711, 436);
            Controls.Add(MainLayoutPanel);
            MinimumSize = new Size(720, 410);
            Name = "FormApplyPatch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Apply patch";
            Load += MergePatch_Load;
            ContinuePanel.ResumeLayout(false);
            MergeToolPanel.ResumeLayout(false);
            PanelBR.ResumeLayout(false);
            PanelBR.PerformLayout();
            MainLayoutPanel.ResumeLayout(false);
            MainLayoutPanel.PerformLayout();
            PanelTR.ResumeLayout(false);
            PanelTR.PerformLayout();
            PanelTL.ResumeLayout(false);
            PanelTL.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Button BrowsePatch;
        private TextBox PatchFile;
        private Button Apply;
        private Button Mergetool;
        private Button Skip;
        private Button Abort;
        private Button Resolved;
        private Button AddFiles;
        private PatchGrid PatchGrid;
        private TextBox PatchDir;
        private Button BrowseDir;
        private RadioButton PatchDirMode;
        private RadioButton PatchFileMode;
        private Button SolveMergeConflicts;
        private CheckBox IgnoreWhitespace;
        private Panel ContinuePanel;
        private Panel MergeToolPanel;
        private FlowLayoutPanel PanelBR;
        private Panel panel2;
        private Panel panel3;
        private TableLayoutPanel MainLayoutPanel;
        private FlowLayoutPanel PanelTR;
        private Panel PanelTL;
        private CheckBox SignOff;
    }
}
