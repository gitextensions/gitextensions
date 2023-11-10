namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GitSettingsPage
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
            TableLayoutPanel tlpnlEnvironment;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GitSettingsPage));
            TableLayoutPanel tlpnlGitPaths;
            TableLayoutPanel tlpnlMain;
            ChangeHomeButton = new Button();
            lblGlobalConfigPath = new Label();
            homeIsSetToLabel = new Label();
            label50 = new Label();
            downloadGitForWindows = new LinkLabel();
            lblGitCommand = new Label();
            lblShPath = new Label();
            BrowseLinuxToolsDir = new Button();
            LinuxToolsDir = new TextBox();
            GitPath = new TextBox();
            BrowseGitPath = new Button();
            gbEnvironment = new GroupBox();
            gbPaths = new GroupBox();
            tlpnlEnvironment = new TableLayoutPanel();
            tlpnlGitPaths = new TableLayoutPanel();
            tlpnlMain = new TableLayoutPanel();
            tlpnlEnvironment.SuspendLayout();
            tlpnlGitPaths.SuspendLayout();
            tlpnlMain.SuspendLayout();
            gbEnvironment.SuspendLayout();
            gbPaths.SuspendLayout();
            SuspendLayout();
            // 
            // tlpnlEnvironment
            // 
            tlpnlEnvironment.AutoSize = true;
            tlpnlEnvironment.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlEnvironment.ColumnCount = 1;
            tlpnlEnvironment.ColumnStyles.Add(new ColumnStyle());
            tlpnlEnvironment.Controls.Add(ChangeHomeButton, 0, 3);
            tlpnlEnvironment.Controls.Add(lblGlobalConfigPath, 0, 0);
            tlpnlEnvironment.Controls.Add(homeIsSetToLabel, 0, 2);
            tlpnlEnvironment.Dock = DockStyle.Fill;
            tlpnlEnvironment.Location = new Point(8, 22);
            tlpnlEnvironment.Name = "tlpnlEnvironment";
            tlpnlEnvironment.RowCount = 4;
            tlpnlEnvironment.RowStyles.Add(new RowStyle());
            tlpnlEnvironment.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpnlEnvironment.RowStyles.Add(new RowStyle());
            tlpnlEnvironment.RowStyles.Add(new RowStyle());
            tlpnlEnvironment.Size = new Size(609, 98);
            tlpnlEnvironment.TabIndex = 8;
            // 
            // ChangeHomeButton
            // 
            ChangeHomeButton.AutoSize = true;
            ChangeHomeButton.Location = new Point(3, 70);
            ChangeHomeButton.Name = "ChangeHomeButton";
            ChangeHomeButton.Size = new Size(90, 25);
            ChangeHomeButton.TabIndex = 9;
            ChangeHomeButton.Text = "Change HOME";
            ChangeHomeButton.UseVisualStyleBackColor = true;
            ChangeHomeButton.Click += ChangeHomeButton_Click;
            // 
            // lblGlobalConfigPath
            // 
            lblGlobalConfigPath.AutoSize = true;
            lblGlobalConfigPath.Dock = DockStyle.Fill;
            lblGlobalConfigPath.Location = new Point(3, 0);
            lblGlobalConfigPath.Name = "lblGlobalConfigPath";
            lblGlobalConfigPath.Size = new Size(603, 26);
            lblGlobalConfigPath.TabIndex = 0;
            lblGlobalConfigPath.Text = resources.GetString("lblGlobalConfigPath.Text");
            // 
            // homeIsSetToLabel
            // 
            homeIsSetToLabel.AutoSize = true;
            homeIsSetToLabel.Location = new Point(3, 46);
            homeIsSetToLabel.Margin = new Padding(3, 0, 3, 8);
            homeIsSetToLabel.Name = "homeIsSetToLabel";
            homeIsSetToLabel.Size = new Size(100, 13);
            homeIsSetToLabel.TabIndex = 1;
            homeIsSetToLabel.Text = "HOME is set to: {0}";
            // 
            // tlpnlGitPaths
            // 
            tlpnlGitPaths.AutoSize = true;
            tlpnlGitPaths.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlGitPaths.ColumnCount = 3;
            tlpnlGitPaths.ColumnStyles.Add(new ColumnStyle());
            tlpnlGitPaths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlGitPaths.ColumnStyles.Add(new ColumnStyle());
            tlpnlGitPaths.Controls.Add(label50, 0, 0);
            tlpnlGitPaths.Controls.Add(downloadGitForWindows, 0, 3);
            tlpnlGitPaths.Controls.Add(lblGitCommand, 0, 1);
            tlpnlGitPaths.Controls.Add(lblShPath, 0, 2);
            tlpnlGitPaths.Controls.Add(BrowseLinuxToolsDir, 2, 2);
            tlpnlGitPaths.Controls.Add(LinuxToolsDir, 1, 2);
            tlpnlGitPaths.Controls.Add(GitPath, 1, 1);
            tlpnlGitPaths.Controls.Add(BrowseGitPath, 2, 1);
            tlpnlGitPaths.Dock = DockStyle.Fill;
            tlpnlGitPaths.Location = new Point(8, 22);
            tlpnlGitPaths.Name = "tlpnlGitPaths";
            tlpnlGitPaths.RowCount = 4;
            tlpnlGitPaths.RowStyles.Add(new RowStyle());
            tlpnlGitPaths.RowStyles.Add(new RowStyle());
            tlpnlGitPaths.RowStyles.Add(new RowStyle());
            tlpnlGitPaths.RowStyles.Add(new RowStyle());
            tlpnlGitPaths.Size = new Size(609, 84);
            tlpnlGitPaths.TabIndex = 0;
            // 
            // label50
            // 
            label50.AutoSize = true;
            tlpnlGitPaths.SetColumnSpan(label50, 3);
            label50.Dock = DockStyle.Fill;
            label50.Location = new Point(3, 0);
            label50.Name = "label50";
            label50.Size = new Size(603, 13);
            label50.TabIndex = 0;
            label50.Text = "Set the correct paths to Git for Windows. (WSL Git will be used for WSL repositories).";
            label50.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // downloadGitForWindows
            // 
            downloadGitForWindows.AutoSize = true;
            downloadGitForWindows.Dock = DockStyle.Fill;
            downloadGitForWindows.Location = new Point(3, 71);
            downloadGitForWindows.Name = "downloadGitForWindows";
            downloadGitForWindows.Size = new Size(291, 13);
            downloadGitForWindows.TabIndex = 7;
            downloadGitForWindows.TabStop = true;
            downloadGitForWindows.Text = "Download Git";
            downloadGitForWindows.TextAlign = ContentAlignment.MiddleRight;
            downloadGitForWindows.LinkClicked += downloadGitForWindows_LinkClicked;
            // 
            // lblGitCommand
            // 
            lblGitCommand.AutoSize = true;
            lblGitCommand.Dock = DockStyle.Fill;
            lblGitCommand.Location = new Point(3, 13);
            lblGitCommand.Name = "lblGitCommand";
            lblGitCommand.Size = new Size(291, 29);
            lblGitCommand.TabIndex = 1;
            lblGitCommand.Text = "Command used to run git (git.cmd or git.exe)";
            lblGitCommand.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblShPath
            // 
            lblShPath.AutoSize = true;
            lblShPath.Dock = DockStyle.Fill;
            lblShPath.Location = new Point(3, 42);
            lblShPath.Name = "lblShPath";
            lblShPath.Size = new Size(291, 29);
            lblShPath.TabIndex = 4;
            lblShPath.Text = "Path to linux tools (sh). Leave empty when it is in the path.";
            lblShPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BrowseLinuxToolsDir
            // 
            BrowseLinuxToolsDir.AutoSize = true;
            BrowseLinuxToolsDir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BrowseLinuxToolsDir.Dock = DockStyle.Fill;
            BrowseLinuxToolsDir.Location = new Point(554, 45);
            BrowseLinuxToolsDir.Name = "BrowseLinuxToolsDir";
            BrowseLinuxToolsDir.Size = new Size(52, 23);
            BrowseLinuxToolsDir.TabIndex = 6;
            BrowseLinuxToolsDir.Text = "Browse";
            BrowseLinuxToolsDir.UseVisualStyleBackColor = true;
            BrowseLinuxToolsDir.Click += BrowseLinuxToolsDir_Click;
            // 
            // LinuxToolsDir
            // 
            LinuxToolsDir.Dock = DockStyle.Fill;
            LinuxToolsDir.Location = new Point(300, 45);
            LinuxToolsDir.MaxLength = 300;
            LinuxToolsDir.Name = "LinuxToolsDir";
            LinuxToolsDir.Size = new Size(248, 21);
            LinuxToolsDir.TabIndex = 5;
            LinuxToolsDir.TextChanged += LinuxToolsDir_TextChanged;
            // 
            // GitPath
            // 
            GitPath.Dock = DockStyle.Fill;
            GitPath.Location = new Point(300, 16);
            GitPath.MaxLength = 300;
            GitPath.Name = "GitPath";
            GitPath.Size = new Size(248, 21);
            GitPath.TabIndex = 2;
            GitPath.TextChanged += GitPath_TextChanged;
            // 
            // BrowseGitPath
            // 
            BrowseGitPath.AutoSize = true;
            BrowseGitPath.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BrowseGitPath.Dock = DockStyle.Fill;
            BrowseGitPath.Location = new Point(554, 16);
            BrowseGitPath.Name = "BrowseGitPath";
            BrowseGitPath.Size = new Size(52, 23);
            BrowseGitPath.TabIndex = 3;
            BrowseGitPath.Text = "Browse";
            BrowseGitPath.UseVisualStyleBackColor = true;
            BrowseGitPath.Click += BrowseGitPath_Click;
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(gbEnvironment, 0, 1);
            tlpnlMain.Controls.Add(gbPaths, 0, 0);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 3;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.Size = new Size(631, 598);
            tlpnlMain.TabIndex = 0;
            // 
            // gbEnvironment
            // 
            gbEnvironment.AutoSize = true;
            gbEnvironment.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbEnvironment.Controls.Add(tlpnlEnvironment);
            gbEnvironment.Dock = DockStyle.Fill;
            gbEnvironment.Location = new Point(3, 123);
            gbEnvironment.Name = "gbEnvironment";
            gbEnvironment.Padding = new Padding(8);
            gbEnvironment.Size = new Size(625, 128);
            gbEnvironment.TabIndex = 1;
            gbEnvironment.TabStop = false;
            gbEnvironment.Text = "Environment";
            // 
            // gbPaths
            // 
            gbPaths.AutoSize = true;
            gbPaths.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbPaths.Controls.Add(tlpnlGitPaths);
            gbPaths.Dock = DockStyle.Fill;
            gbPaths.Location = new Point(3, 3);
            gbPaths.Name = "gbPaths";
            gbPaths.Padding = new Padding(8);
            gbPaths.Size = new Size(625, 114);
            gbPaths.TabIndex = 0;
            gbPaths.TabStop = false;
            gbPaths.Text = "Paths";
            // 
            // GitSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tlpnlMain);
            Name = "GitSettingsPage";
            Padding = new Padding(8);
            Size = new Size(647, 614);
            Text = "Paths";
            tlpnlEnvironment.ResumeLayout(false);
            tlpnlEnvironment.PerformLayout();
            tlpnlGitPaths.ResumeLayout(false);
            tlpnlGitPaths.PerformLayout();
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbEnvironment.ResumeLayout(false);
            gbEnvironment.PerformLayout();
            gbPaths.ResumeLayout(false);
            gbPaths.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private GroupBox gbEnvironment;
        private Label homeIsSetToLabel;
        private Button ChangeHomeButton;
        private GroupBox gbPaths;
        private LinkLabel downloadGitForWindows;
        private Button BrowseLinuxToolsDir;
        private TextBox GitPath;
        private Button BrowseGitPath;
        private TextBox LinuxToolsDir;
        private Label lblGlobalConfigPath;
        private Label lblGitCommand;
        private Label label50;
        private Label lblShPath;
    }
}
