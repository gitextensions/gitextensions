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
            System.Windows.Forms.TableLayoutPanel tlpnlEnvironment;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GitSettingsPage));
            System.Windows.Forms.TableLayoutPanel tlpnlGitPaths;
            System.Windows.Forms.TableLayoutPanel tlpnlMain;
            this.ChangeHomeButton = new System.Windows.Forms.Button();
            this.lblGlobalConfigPath = new System.Windows.Forms.Label();
            this.homeIsSetToLabel = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.downloadGitForWindows = new System.Windows.Forms.LinkLabel();
            this.lblGitCommand = new System.Windows.Forms.Label();
            this.lblShPath = new System.Windows.Forms.Label();
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.gbEnvironment = new System.Windows.Forms.GroupBox();
            this.gbPaths = new System.Windows.Forms.GroupBox();
            tlpnlEnvironment = new System.Windows.Forms.TableLayoutPanel();
            tlpnlGitPaths = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlEnvironment.SuspendLayout();
            tlpnlGitPaths.SuspendLayout();
            tlpnlMain.SuspendLayout();
            this.gbEnvironment.SuspendLayout();
            this.gbPaths.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpnlEnvironment
            // 
            tlpnlEnvironment.AutoSize = true;
            tlpnlEnvironment.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlpnlEnvironment.ColumnCount = 1;
            tlpnlEnvironment.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlEnvironment.Controls.Add(this.ChangeHomeButton, 0, 3);
            tlpnlEnvironment.Controls.Add(this.lblGlobalConfigPath, 0, 0);
            tlpnlEnvironment.Controls.Add(this.homeIsSetToLabel, 0, 2);
            tlpnlEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlEnvironment.Location = new System.Drawing.Point(8, 22);
            tlpnlEnvironment.Name = "tlpnlEnvironment";
            tlpnlEnvironment.RowCount = 4;
            tlpnlEnvironment.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlEnvironment.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tlpnlEnvironment.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlEnvironment.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlEnvironment.Size = new System.Drawing.Size(609, 98);
            tlpnlEnvironment.TabIndex = 0;
            // 
            // ChangeHomeButton
            // 
            this.ChangeHomeButton.AutoSize = true;
            this.ChangeHomeButton.Location = new System.Drawing.Point(3, 70);
            this.ChangeHomeButton.Name = "ChangeHomeButton";
            this.ChangeHomeButton.Size = new System.Drawing.Size(90, 25);
            this.ChangeHomeButton.TabIndex = 2;
            this.ChangeHomeButton.Text = "Change HOME";
            this.ChangeHomeButton.UseVisualStyleBackColor = true;
            this.ChangeHomeButton.Click += new System.EventHandler(this.ChangeHomeButton_Click);
            // 
            // lblGlobalConfigPath
            // 
            this.lblGlobalConfigPath.AutoSize = true;
            this.lblGlobalConfigPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGlobalConfigPath.Location = new System.Drawing.Point(3, 0);
            this.lblGlobalConfigPath.Name = "lblGlobalConfigPath";
            this.lblGlobalConfigPath.Size = new System.Drawing.Size(603, 26);
            this.lblGlobalConfigPath.TabIndex = 0;
            this.lblGlobalConfigPath.Text = resources.GetString("lblGlobalConfigPath.Text");
            // 
            // homeIsSetToLabel
            // 
            this.homeIsSetToLabel.AutoSize = true;
            this.homeIsSetToLabel.Location = new System.Drawing.Point(3, 46);
            this.homeIsSetToLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
            this.homeIsSetToLabel.Name = "homeIsSetToLabel";
            this.homeIsSetToLabel.Size = new System.Drawing.Size(100, 13);
            this.homeIsSetToLabel.TabIndex = 1;
            this.homeIsSetToLabel.Text = "HOME is set to: {0}";
            // 
            // tlpnlGitPaths
            // 
            tlpnlGitPaths.AutoSize = true;
            tlpnlGitPaths.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlpnlGitPaths.ColumnCount = 3;
            tlpnlGitPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlGitPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlGitPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlGitPaths.Controls.Add(this.label50, 0, 0);
            tlpnlGitPaths.Controls.Add(this.downloadGitForWindows, 0, 3);
            tlpnlGitPaths.Controls.Add(this.lblGitCommand, 0, 1);
            tlpnlGitPaths.Controls.Add(this.lblShPath, 0, 2);
            tlpnlGitPaths.Controls.Add(this.BrowseGitBinPath, 2, 2);
            tlpnlGitPaths.Controls.Add(this.GitBinPath, 1, 2);
            tlpnlGitPaths.Controls.Add(this.GitPath, 1, 1);
            tlpnlGitPaths.Controls.Add(this.BrowseGitPath, 2, 1);
            tlpnlGitPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlGitPaths.Location = new System.Drawing.Point(8, 22);
            tlpnlGitPaths.Name = "tlpnlGitPaths";
            tlpnlGitPaths.RowCount = 4;
            tlpnlGitPaths.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlGitPaths.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlGitPaths.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlGitPaths.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlGitPaths.Size = new System.Drawing.Size(609, 84);
            tlpnlGitPaths.TabIndex = 0;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            tlpnlGitPaths.SetColumnSpan(this.label50, 3);
            this.label50.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label50.Location = new System.Drawing.Point(3, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(603, 13);
            this.label50.TabIndex = 0;
            this.label50.Text = "Git Extensions can use Git for Windows or cygwin to access git repositories. Set " +
    "the correct paths below.";
            this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // downloadGitForWindows
            // 
            this.downloadGitForWindows.AutoSize = true;
            this.downloadGitForWindows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadGitForWindows.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.downloadGitForWindows.Location = new System.Drawing.Point(3, 71);
            this.downloadGitForWindows.Name = "downloadGitForWindows";
            this.downloadGitForWindows.Size = new System.Drawing.Size(291, 13);
            this.downloadGitForWindows.TabIndex = 7;
            this.downloadGitForWindows.TabStop = true;
            this.downloadGitForWindows.Text = "Download Git";
            this.downloadGitForWindows.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.downloadGitForWindows.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadGitForWindows_LinkClicked);
            // 
            // lblGitCommand
            // 
            this.lblGitCommand.AutoSize = true;
            this.lblGitCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGitCommand.Location = new System.Drawing.Point(3, 13);
            this.lblGitCommand.Name = "lblGitCommand";
            this.lblGitCommand.Size = new System.Drawing.Size(291, 29);
            this.lblGitCommand.TabIndex = 1;
            this.lblGitCommand.Text = "Command used to run git (git.cmd or git.exe)";
            this.lblGitCommand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblShPath
            // 
            this.lblShPath.AutoSize = true;
            this.lblShPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblShPath.Location = new System.Drawing.Point(3, 42);
            this.lblShPath.Name = "lblShPath";
            this.lblShPath.Size = new System.Drawing.Size(291, 29);
            this.lblShPath.TabIndex = 4;
            this.lblShPath.Text = "Path to linux tools (sh). Leave empty when it is in the path.";
            this.lblShPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.AutoSize = true;
            this.BrowseGitBinPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BrowseGitBinPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BrowseGitBinPath.Location = new System.Drawing.Point(554, 45);
            this.BrowseGitBinPath.Name = "BrowseGitBinPath";
            this.BrowseGitBinPath.Size = new System.Drawing.Size(52, 23);
            this.BrowseGitBinPath.TabIndex = 6;
            this.BrowseGitBinPath.Text = "Browse";
            this.BrowseGitBinPath.UseVisualStyleBackColor = true;
            this.BrowseGitBinPath.Click += new System.EventHandler(this.BrowseGitBinPath_Click);
            // 
            // GitBinPath
            // 
            this.GitBinPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitBinPath.Location = new System.Drawing.Point(300, 45);
            this.GitBinPath.MaxLength = 300;
            this.GitBinPath.Name = "GitBinPath";
            this.GitBinPath.Size = new System.Drawing.Size(248, 21);
            this.GitBinPath.TabIndex = 5;
            this.GitBinPath.TextChanged += new System.EventHandler(this.GitBinPath_TextChanged);
            // 
            // GitPath
            // 
            this.GitPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitPath.Location = new System.Drawing.Point(300, 16);
            this.GitPath.MaxLength = 300;
            this.GitPath.Name = "GitPath";
            this.GitPath.Size = new System.Drawing.Size(248, 21);
            this.GitPath.TabIndex = 2;
            this.GitPath.TextChanged += new System.EventHandler(this.GitPath_TextChanged);
            // 
            // BrowseGitPath
            // 
            this.BrowseGitPath.AutoSize = true;
            this.BrowseGitPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BrowseGitPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BrowseGitPath.Location = new System.Drawing.Point(554, 16);
            this.BrowseGitPath.Name = "BrowseGitPath";
            this.BrowseGitPath.Size = new System.Drawing.Size(52, 23);
            this.BrowseGitPath.TabIndex = 3;
            this.BrowseGitPath.Text = "Browse";
            this.BrowseGitPath.UseVisualStyleBackColor = true;
            this.BrowseGitPath.Click += new System.EventHandler(this.BrowseGitPath_Click);
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlMain.Controls.Add(this.gbEnvironment, 0, 1);
            tlpnlMain.Controls.Add(this.gbPaths, 0, 0);
            tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlMain.Location = new System.Drawing.Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 3;
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlMain.Size = new System.Drawing.Size(631, 598);
            tlpnlMain.TabIndex = 0;
            // 
            // gbEnvironment
            // 
            this.gbEnvironment.AutoSize = true;
            this.gbEnvironment.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbEnvironment.Controls.Add(tlpnlEnvironment);
            this.gbEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbEnvironment.Location = new System.Drawing.Point(3, 123);
            this.gbEnvironment.Name = "gbEnvironment";
            this.gbEnvironment.Padding = new System.Windows.Forms.Padding(8);
            this.gbEnvironment.Size = new System.Drawing.Size(625, 128);
            this.gbEnvironment.TabIndex = 1;
            this.gbEnvironment.TabStop = false;
            this.gbEnvironment.Text = "Environment";
            // 
            // gbPaths
            // 
            this.gbPaths.AutoSize = true;
            this.gbPaths.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbPaths.Controls.Add(tlpnlGitPaths);
            this.gbPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPaths.Location = new System.Drawing.Point(3, 3);
            this.gbPaths.Name = "gbPaths";
            this.gbPaths.Padding = new System.Windows.Forms.Padding(8);
            this.gbPaths.Size = new System.Drawing.Size(625, 114);
            this.gbPaths.TabIndex = 0;
            this.gbPaths.TabStop = false;
            this.gbPaths.Text = "Paths";
            // 
            // GitSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tlpnlMain);
            this.Name = "GitSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(647, 614);
            tlpnlEnvironment.ResumeLayout(false);
            tlpnlEnvironment.PerformLayout();
            tlpnlGitPaths.ResumeLayout(false);
            tlpnlGitPaths.PerformLayout();
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.gbEnvironment.ResumeLayout(false);
            this.gbEnvironment.PerformLayout();
            this.gbPaths.ResumeLayout(false);
            this.gbPaths.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbEnvironment;
        private System.Windows.Forms.Label homeIsSetToLabel;
        private System.Windows.Forms.Button ChangeHomeButton;
        private System.Windows.Forms.GroupBox gbPaths;
        private System.Windows.Forms.LinkLabel downloadGitForWindows;
        private System.Windows.Forms.Button BrowseGitBinPath;
        private System.Windows.Forms.TextBox GitPath;
        private System.Windows.Forms.Button BrowseGitPath;
        private System.Windows.Forms.TextBox GitBinPath;
        private System.Windows.Forms.Label lblGlobalConfigPath;
        private System.Windows.Forms.Label lblGitCommand;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Label lblShPath;
    }
}
