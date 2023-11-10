namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ChecklistSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            GroupBox groupBox1;
            tableLayoutPanel1 = new TableLayoutPanel();
            GcmDetectedFix = new Button();
            GcmDetected = new Button();
            label11 = new Label();
            translationConfig_Fix = new Button();
            CheckAtStartup = new CheckBox();
            GitFound = new Button();
            translationConfig = new Button();
            SshConfig_Fix = new Button();
            GitFound_Fix = new Button();
            GitExtensionsInstall_Fix = new Button();
            SshConfig = new Button();
            UserNameSet = new Button();
            GitBinFound_Fix = new Button();
            UserNameSet_Fix = new Button();
            ShellExtensionsRegistered_Fix = new Button();
            MergeTool = new Button();
            GitExtensionsInstall = new Button();
            GitBinFound = new Button();
            DiffTool_Fix = new Button();
            MergeTool_Fix = new Button();
            DiffTool = new Button();
            ShellExtensionsRegistered = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            Rescan = new Button();
            groupBox1 = new GroupBox();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(tableLayoutPanel1);
            groupBox1.Controls.Add(flowLayoutPanel1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(12);
            groupBox1.Size = new Size(1015, 478);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(GcmDetectedFix, 1, 10);
            tableLayoutPanel1.Controls.Add(GcmDetected, 0, 10);
            tableLayoutPanel1.Controls.Add(label11, 0, 0);
            tableLayoutPanel1.Controls.Add(translationConfig_Fix, 1, 9);
            tableLayoutPanel1.Controls.Add(CheckAtStartup, 0, 11);
            tableLayoutPanel1.Controls.Add(GitFound, 0, 1);
            tableLayoutPanel1.Controls.Add(translationConfig, 0, 9);
            tableLayoutPanel1.Controls.Add(SshConfig_Fix, 1, 8);
            tableLayoutPanel1.Controls.Add(GitFound_Fix, 1, 1);
            tableLayoutPanel1.Controls.Add(GitExtensionsInstall_Fix, 1, 7);
            tableLayoutPanel1.Controls.Add(SshConfig, 0, 8);
            tableLayoutPanel1.Controls.Add(UserNameSet, 0, 2);
            tableLayoutPanel1.Controls.Add(GitBinFound_Fix, 1, 6);
            tableLayoutPanel1.Controls.Add(UserNameSet_Fix, 1, 2);
            tableLayoutPanel1.Controls.Add(ShellExtensionsRegistered_Fix, 1, 5);
            tableLayoutPanel1.Controls.Add(MergeTool, 0, 3);
            tableLayoutPanel1.Controls.Add(GitExtensionsInstall, 0, 7);
            tableLayoutPanel1.Controls.Add(GitBinFound, 0, 6);
            tableLayoutPanel1.Controls.Add(DiffTool_Fix, 1, 4);
            tableLayoutPanel1.Controls.Add(MergeTool_Fix, 1, 3);
            tableLayoutPanel1.Controls.Add(DiffTool, 0, 4);
            tableLayoutPanel1.Controls.Add(ShellExtensionsRegistered, 0, 5);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(12, 26);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 14;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(991, 404);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // GcmDetectedFix
            // 
            GcmDetectedFix.AutoSize = true;
            GcmDetectedFix.Dock = DockStyle.Fill;
            GcmDetectedFix.Location = new Point(903, 348);
            GcmDetectedFix.Name = "GcmDetectedFix";
            GcmDetectedFix.Size = new Size(85, 30);
            GcmDetectedFix.TabIndex = 20;
            GcmDetectedFix.Text = "Repair";
            GcmDetectedFix.UseVisualStyleBackColor = true;
            GcmDetectedFix.Visible = false;
            GcmDetectedFix.Click += GcmDetectedFix_Click;
            // 
            // GcmDetected
            // 
            GcmDetected.AutoSize = true;
            GcmDetected.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GcmDetected.BackColor = SystemColors.ControlDark;
            GcmDetected.Cursor = Cursors.Hand;
            GcmDetected.Dock = DockStyle.Fill;
            GcmDetected.FlatAppearance.BorderSize = 0;
            GcmDetected.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            GcmDetected.FlatStyle = FlatStyle.Flat;
            GcmDetected.ForeColor = SystemColors.ControlText;
            GcmDetected.Location = new Point(3, 348);
            GcmDetected.Name = "GcmDetected";
            GcmDetected.Size = new Size(894, 30);
            GcmDetected.TabIndex = 19;
            GcmDetected.TextAlign = ContentAlignment.MiddleLeft;
            GcmDetected.UseVisualStyleBackColor = false;
            GcmDetected.Visible = false;
            // 
            // label11
            // 
            label11.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label11, 2);
            label11.Dock = DockStyle.Fill;
            label11.Location = new Point(3, 0);
            label11.Name = "label11";
            label11.Padding = new Padding(0, 0, 0, 8);
            label11.Size = new Size(985, 21);
            label11.TabIndex = 0;
            label11.Text = "The checklist below validates the basic settings needed for Git Extensions to work" +
    " properly.";
            // 
            // translationConfig_Fix
            // 
            translationConfig_Fix.AutoSize = true;
            translationConfig_Fix.Dock = DockStyle.Fill;
            translationConfig_Fix.Location = new Point(903, 312);
            translationConfig_Fix.Name = "translationConfig_Fix";
            translationConfig_Fix.Size = new Size(85, 30);
            translationConfig_Fix.TabIndex = 18;
            translationConfig_Fix.Text = "Repair";
            translationConfig_Fix.UseVisualStyleBackColor = true;
            translationConfig_Fix.Visible = false;
            translationConfig_Fix.Click += translationConfig_Click;
            // 
            // CheckAtStartup
            // 
            CheckAtStartup.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            CheckAtStartup.AutoSize = true;
            CheckAtStartup.Location = new Point(3, 384);
            CheckAtStartup.Name = "CheckAtStartup";
            CheckAtStartup.Size = new Size(894, 17);
            CheckAtStartup.TabIndex = 21;
            CheckAtStartup.Text = "Check settings at startup (disables automatically if all settings are correct)";
            CheckAtStartup.UseVisualStyleBackColor = true;
            CheckAtStartup.Click += CheckAtStartup_CheckedChanged;
            // 
            // GitFound
            // 
            GitFound.AutoSize = true;
            GitFound.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GitFound.BackColor = SystemColors.ControlDark;
            GitFound.Cursor = Cursors.Hand;
            GitFound.Dock = DockStyle.Fill;
            GitFound.FlatAppearance.BorderSize = 0;
            GitFound.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            GitFound.FlatStyle = FlatStyle.Flat;
            GitFound.ForeColor = SystemColors.ControlText;
            GitFound.Location = new Point(3, 24);
            GitFound.Name = "GitFound";
            GitFound.Size = new Size(894, 30);
            GitFound.TabIndex = 1;
            GitFound.TextAlign = ContentAlignment.MiddleLeft;
            GitFound.UseVisualStyleBackColor = false;
            GitFound.Visible = false;
            GitFound.Click += GitFound_Click;
            // 
            // translationConfig
            // 
            translationConfig.AutoSize = true;
            translationConfig.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            translationConfig.BackColor = SystemColors.ControlDark;
            translationConfig.Cursor = Cursors.Hand;
            translationConfig.Dock = DockStyle.Fill;
            translationConfig.FlatAppearance.BorderSize = 0;
            translationConfig.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            translationConfig.FlatStyle = FlatStyle.Flat;
            translationConfig.ForeColor = SystemColors.ControlText;
            translationConfig.Location = new Point(3, 312);
            translationConfig.Name = "translationConfig";
            translationConfig.Size = new Size(894, 30);
            translationConfig.TabIndex = 17;
            translationConfig.TextAlign = ContentAlignment.MiddleLeft;
            translationConfig.UseVisualStyleBackColor = false;
            translationConfig.Visible = false;
            translationConfig.Click += translationConfig_Click;
            // 
            // SshConfig_Fix
            // 
            SshConfig_Fix.AutoSize = true;
            SshConfig_Fix.Dock = DockStyle.Fill;
            SshConfig_Fix.Location = new Point(903, 276);
            SshConfig_Fix.Name = "SshConfig_Fix";
            SshConfig_Fix.Size = new Size(85, 30);
            SshConfig_Fix.TabIndex = 16;
            SshConfig_Fix.Text = "Repair";
            SshConfig_Fix.UseVisualStyleBackColor = true;
            SshConfig_Fix.Visible = false;
            SshConfig_Fix.Click += SshConfig_Click;
            // 
            // GitFound_Fix
            // 
            GitFound_Fix.AutoSize = true;
            GitFound_Fix.Dock = DockStyle.Fill;
            GitFound_Fix.Location = new Point(903, 24);
            GitFound_Fix.Name = "GitFound_Fix";
            GitFound_Fix.Size = new Size(85, 30);
            GitFound_Fix.TabIndex = 2;
            GitFound_Fix.Text = "Repair";
            GitFound_Fix.UseVisualStyleBackColor = true;
            GitFound_Fix.Visible = false;
            GitFound_Fix.Click += GitFound_Click;
            // 
            // GitExtensionsInstall_Fix
            // 
            GitExtensionsInstall_Fix.AutoSize = true;
            GitExtensionsInstall_Fix.Dock = DockStyle.Fill;
            GitExtensionsInstall_Fix.Location = new Point(903, 240);
            GitExtensionsInstall_Fix.Name = "GitExtensionsInstall_Fix";
            GitExtensionsInstall_Fix.Size = new Size(85, 30);
            GitExtensionsInstall_Fix.TabIndex = 14;
            GitExtensionsInstall_Fix.Text = "Repair";
            GitExtensionsInstall_Fix.UseVisualStyleBackColor = true;
            GitExtensionsInstall_Fix.Visible = false;
            GitExtensionsInstall_Fix.Click += GitExtensionsInstall_Click;
            // 
            // SshConfig
            // 
            SshConfig.AutoSize = true;
            SshConfig.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SshConfig.BackColor = SystemColors.ControlDark;
            SshConfig.Cursor = Cursors.Hand;
            SshConfig.Dock = DockStyle.Fill;
            SshConfig.FlatAppearance.BorderSize = 0;
            SshConfig.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            SshConfig.FlatStyle = FlatStyle.Flat;
            SshConfig.ForeColor = SystemColors.ControlText;
            SshConfig.Location = new Point(3, 276);
            SshConfig.Name = "SshConfig";
            SshConfig.Size = new Size(894, 30);
            SshConfig.TabIndex = 15;
            SshConfig.TextAlign = ContentAlignment.MiddleLeft;
            SshConfig.UseVisualStyleBackColor = false;
            SshConfig.Visible = false;
            SshConfig.Click += SshConfig_Click;
            // 
            // UserNameSet
            // 
            UserNameSet.AutoSize = true;
            UserNameSet.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            UserNameSet.BackColor = SystemColors.ControlDark;
            UserNameSet.Cursor = Cursors.Hand;
            UserNameSet.Dock = DockStyle.Fill;
            UserNameSet.FlatAppearance.BorderSize = 0;
            UserNameSet.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            UserNameSet.FlatStyle = FlatStyle.Flat;
            UserNameSet.ForeColor = SystemColors.ControlText;
            UserNameSet.Location = new Point(3, 60);
            UserNameSet.Name = "UserNameSet";
            UserNameSet.Size = new Size(894, 30);
            UserNameSet.TabIndex = 3;
            UserNameSet.TextAlign = ContentAlignment.MiddleLeft;
            UserNameSet.UseVisualStyleBackColor = false;
            UserNameSet.Visible = false;
            UserNameSet.Click += UserNameSet_Click;
            // 
            // GitBinFound_Fix
            // 
            GitBinFound_Fix.AutoSize = true;
            GitBinFound_Fix.Dock = DockStyle.Fill;
            GitBinFound_Fix.Location = new Point(903, 204);
            GitBinFound_Fix.Name = "GitBinFound_Fix";
            GitBinFound_Fix.Size = new Size(85, 30);
            GitBinFound_Fix.TabIndex = 12;
            GitBinFound_Fix.Text = "Repair";
            GitBinFound_Fix.UseVisualStyleBackColor = true;
            GitBinFound_Fix.Visible = false;
            GitBinFound_Fix.Click += GitBinFound_Click;
            // 
            // UserNameSet_Fix
            // 
            UserNameSet_Fix.AutoSize = true;
            UserNameSet_Fix.Dock = DockStyle.Fill;
            UserNameSet_Fix.Location = new Point(903, 60);
            UserNameSet_Fix.Name = "UserNameSet_Fix";
            UserNameSet_Fix.Size = new Size(85, 30);
            UserNameSet_Fix.TabIndex = 4;
            UserNameSet_Fix.Text = "Repair";
            UserNameSet_Fix.UseVisualStyleBackColor = true;
            UserNameSet_Fix.Visible = false;
            UserNameSet_Fix.Click += UserNameSet_Click;
            // 
            // ShellExtensionsRegistered_Fix
            // 
            ShellExtensionsRegistered_Fix.AutoSize = true;
            ShellExtensionsRegistered_Fix.Dock = DockStyle.Fill;
            ShellExtensionsRegistered_Fix.Location = new Point(903, 168);
            ShellExtensionsRegistered_Fix.Name = "ShellExtensionsRegistered_Fix";
            ShellExtensionsRegistered_Fix.Size = new Size(85, 30);
            ShellExtensionsRegistered_Fix.TabIndex = 10;
            ShellExtensionsRegistered_Fix.Text = "Repair";
            ShellExtensionsRegistered_Fix.UseVisualStyleBackColor = true;
            ShellExtensionsRegistered_Fix.Visible = false;
            ShellExtensionsRegistered_Fix.Click += ShellExtensionsRegistered_Click;
            // 
            // MergeTool
            // 
            MergeTool.AutoSize = true;
            MergeTool.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MergeTool.BackColor = SystemColors.ControlDark;
            MergeTool.Cursor = Cursors.Hand;
            MergeTool.Dock = DockStyle.Fill;
            MergeTool.FlatAppearance.BorderSize = 0;
            MergeTool.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            MergeTool.FlatStyle = FlatStyle.Flat;
            MergeTool.ForeColor = SystemColors.ControlText;
            MergeTool.Location = new Point(3, 96);
            MergeTool.Name = "MergeTool";
            MergeTool.Size = new Size(894, 30);
            MergeTool.TabIndex = 5;
            MergeTool.TextAlign = ContentAlignment.MiddleLeft;
            MergeTool.UseVisualStyleBackColor = false;
            MergeTool.Visible = false;
            MergeTool.Click += MergeToolFix_Click;
            // 
            // GitExtensionsInstall
            // 
            GitExtensionsInstall.AutoSize = true;
            GitExtensionsInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GitExtensionsInstall.BackColor = SystemColors.ControlDark;
            GitExtensionsInstall.Cursor = Cursors.Hand;
            GitExtensionsInstall.Dock = DockStyle.Fill;
            GitExtensionsInstall.FlatAppearance.BorderSize = 0;
            GitExtensionsInstall.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            GitExtensionsInstall.FlatStyle = FlatStyle.Flat;
            GitExtensionsInstall.ForeColor = SystemColors.ControlText;
            GitExtensionsInstall.Location = new Point(3, 240);
            GitExtensionsInstall.Name = "GitExtensionsInstall";
            GitExtensionsInstall.Size = new Size(894, 30);
            GitExtensionsInstall.TabIndex = 13;
            GitExtensionsInstall.TextAlign = ContentAlignment.MiddleLeft;
            GitExtensionsInstall.UseVisualStyleBackColor = false;
            GitExtensionsInstall.Visible = false;
            GitExtensionsInstall.Click += GitExtensionsInstall_Click;
            // 
            // GitBinFound
            // 
            GitBinFound.AutoSize = true;
            GitBinFound.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GitBinFound.BackColor = SystemColors.ControlDark;
            GitBinFound.Cursor = Cursors.Hand;
            GitBinFound.Dock = DockStyle.Fill;
            GitBinFound.FlatAppearance.BorderSize = 0;
            GitBinFound.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            GitBinFound.FlatStyle = FlatStyle.Flat;
            GitBinFound.ForeColor = SystemColors.ControlText;
            GitBinFound.Location = new Point(3, 204);
            GitBinFound.Name = "GitBinFound";
            GitBinFound.Size = new Size(894, 30);
            GitBinFound.TabIndex = 11;
            GitBinFound.TextAlign = ContentAlignment.MiddleLeft;
            GitBinFound.UseVisualStyleBackColor = false;
            GitBinFound.Visible = false;
            GitBinFound.Click += GitBinFound_Click;
            // 
            // DiffTool_Fix
            // 
            DiffTool_Fix.AutoSize = true;
            DiffTool_Fix.Dock = DockStyle.Fill;
            DiffTool_Fix.Location = new Point(903, 132);
            DiffTool_Fix.Name = "DiffTool_Fix";
            DiffTool_Fix.Size = new Size(85, 30);
            DiffTool_Fix.TabIndex = 8;
            DiffTool_Fix.Text = "Repair";
            DiffTool_Fix.UseVisualStyleBackColor = true;
            DiffTool_Fix.Visible = false;
            DiffTool_Fix.Click += DiffToolFix_Click;
            // 
            // MergeTool_Fix
            // 
            MergeTool_Fix.AutoSize = true;
            MergeTool_Fix.Dock = DockStyle.Fill;
            MergeTool_Fix.Location = new Point(903, 96);
            MergeTool_Fix.Name = "MergeTool_Fix";
            MergeTool_Fix.Size = new Size(85, 30);
            MergeTool_Fix.TabIndex = 6;
            MergeTool_Fix.Text = "Repair";
            MergeTool_Fix.UseVisualStyleBackColor = true;
            MergeTool_Fix.Visible = false;
            MergeTool_Fix.Click += MergeToolFix_Click;
            // 
            // DiffTool
            // 
            DiffTool.AutoSize = true;
            DiffTool.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            DiffTool.BackColor = SystemColors.ControlDark;
            DiffTool.Cursor = Cursors.Hand;
            DiffTool.Dock = DockStyle.Fill;
            DiffTool.FlatAppearance.BorderSize = 0;
            DiffTool.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            DiffTool.FlatStyle = FlatStyle.Flat;
            DiffTool.ForeColor = SystemColors.ControlText;
            DiffTool.Location = new Point(3, 132);
            DiffTool.Name = "DiffTool";
            DiffTool.Size = new Size(894, 30);
            DiffTool.TabIndex = 7;
            DiffTool.TextAlign = ContentAlignment.MiddleLeft;
            DiffTool.UseVisualStyleBackColor = false;
            DiffTool.Visible = false;
            DiffTool.Click += DiffToolFix_Click;
            // 
            // ShellExtensionsRegistered
            // 
            ShellExtensionsRegistered.AutoSize = true;
            ShellExtensionsRegistered.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ShellExtensionsRegistered.BackColor = SystemColors.ControlDark;
            ShellExtensionsRegistered.Cursor = Cursors.Hand;
            ShellExtensionsRegistered.Dock = DockStyle.Fill;
            ShellExtensionsRegistered.FlatAppearance.BorderSize = 0;
            ShellExtensionsRegistered.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            ShellExtensionsRegistered.FlatStyle = FlatStyle.Flat;
            ShellExtensionsRegistered.ForeColor = SystemColors.ControlText;
            ShellExtensionsRegistered.Location = new Point(3, 168);
            ShellExtensionsRegistered.Name = "ShellExtensionsRegistered";
            ShellExtensionsRegistered.Size = new Size(894, 30);
            ShellExtensionsRegistered.TabIndex = 9;
            ShellExtensionsRegistered.TextAlign = ContentAlignment.MiddleLeft;
            ShellExtensionsRegistered.UseVisualStyleBackColor = false;
            ShellExtensionsRegistered.Visible = false;
            ShellExtensionsRegistered.Click += ShellExtensionsRegistered_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(Rescan);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(12, 430);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(991, 36);
            flowLayoutPanel1.TabIndex = 1;
            flowLayoutPanel1.WrapContents = false;
            // 
            // Rescan
            // 
            Rescan.AutoSize = true;
            Rescan.Location = new Point(838, 3);
            Rescan.Name = "Rescan";
            Rescan.Size = new Size(150, 30);
            Rescan.TabIndex = 0;
            Rescan.Text = "Save and rescan";
            Rescan.UseVisualStyleBackColor = true;
            Rescan.Click += SaveAndRescan_Click;
            // 
            // ChecklistSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(groupBox1);
            MinimumSize = new Size(680, 460);
            Name = "ChecklistSettingsPage";
            Size = new Size(1015, 608);
            Text = "Checklist";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button translationConfig_Fix;
        private Button SshConfig_Fix;
        private Button GitExtensionsInstall_Fix;
        private Button GitBinFound_Fix;
        private Button ShellExtensionsRegistered_Fix;
        private Button DiffTool_Fix;
        private Button MergeTool_Fix;
        private Button UserNameSet_Fix;
        private Button GitFound_Fix;
        private Button translationConfig;
        private Button DiffTool;
        private Button SshConfig;
        private Button GitBinFound;
        private Button Rescan;
        private CheckBox CheckAtStartup;
        private Label label11;
        private Button GitFound;
        private Button MergeTool;
        private Button UserNameSet;
        private Button ShellExtensionsRegistered;
        private Button GitExtensionsInstall;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button GcmDetectedFix;
        private Button GcmDetected;
    }
}
