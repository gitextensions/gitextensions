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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GitSettingsPage));
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ChangeHomeButton = new System.Windows.Forms.Button();
            this.label51 = new System.Windows.Forms.Label();
            this.homeIsSetToLabel = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label50 = new System.Windows.Forms.Label();
            this.downloadGitForWindows = new System.Windows.Forms.LinkLabel();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.groupBox8.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox8
            // 
            this.groupBox8.AutoSize = true;
            this.groupBox8.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox8.Controls.Add(this.tableLayoutPanel2);
            this.groupBox8.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox8.Location = new System.Drawing.Point(0, 126);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Padding = new System.Windows.Forms.Padding(12);
            this.groupBox8.Size = new System.Drawing.Size(1297, 140);
            this.groupBox8.TabIndex = 1;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Environment";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.ChangeHomeButton, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label51, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.homeIsSetToLabel, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(12, 26);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1273, 102);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // ChangeHomeButton
            // 
            this.ChangeHomeButton.AutoSize = true;
            this.ChangeHomeButton.Location = new System.Drawing.Point(3, 74);
            this.ChangeHomeButton.Name = "ChangeHomeButton";
            this.ChangeHomeButton.Size = new System.Drawing.Size(90, 25);
            this.ChangeHomeButton.TabIndex = 2;
            this.ChangeHomeButton.Text = "Change HOME";
            this.ChangeHomeButton.UseVisualStyleBackColor = true;
            this.ChangeHomeButton.Click += new System.EventHandler(this.ChangeHomeButton_Click);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(3, 0);
            this.label51.Margin = new System.Windows.Forms.Padding(3, 0, 3, 24);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(568, 26);
            this.label51.TabIndex = 0;
            this.label51.Text = resources.GetString("label51.Text");
            // 
            // homeIsSetToLabel
            // 
            this.homeIsSetToLabel.AutoSize = true;
            this.homeIsSetToLabel.Location = new System.Drawing.Point(3, 50);
            this.homeIsSetToLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
            this.homeIsSetToLabel.Name = "homeIsSetToLabel";
            this.homeIsSetToLabel.Size = new System.Drawing.Size(100, 13);
            this.homeIsSetToLabel.TabIndex = 1;
            this.homeIsSetToLabel.Text = "HOME is set to: {0}";
            // 
            // groupBox7
            // 
            this.groupBox7.AutoSize = true;
            this.groupBox7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox7.Controls.Add(this.tableLayoutPanel1);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox7.Location = new System.Drawing.Point(0, 0);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(12);
            this.groupBox7.Size = new System.Drawing.Size(1297, 126);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Git";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label50, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.downloadGitForWindows, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label13, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label14, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.BrowseGitBinPath, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.GitBinPath, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.GitPath, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.BrowseGitPath, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 26);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1273, 88);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label50, 3);
            this.label50.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label50.Location = new System.Drawing.Point(3, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(1267, 13);
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
            this.downloadGitForWindows.Location = new System.Drawing.Point(3, 75);
            this.downloadGitForWindows.Name = "downloadGitForWindows";
            this.downloadGitForWindows.Size = new System.Drawing.Size(291, 13);
            this.downloadGitForWindows.TabIndex = 7;
            this.downloadGitForWindows.TabStop = true;
            this.downloadGitForWindows.Text = "Download Git";
            this.downloadGitForWindows.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.downloadGitForWindows.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadGitForWindows_LinkClicked);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(3, 13);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(291, 31);
            this.label13.TabIndex = 1;
            this.label13.Text = "Command used to run git (git.cmd or git.exe)";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(3, 44);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(291, 31);
            this.label14.TabIndex = 4;
            this.label14.Text = "Path to linux tools (sh). Leave empty when it is in the path.";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.AutoSize = true;
            this.BrowseGitBinPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BrowseGitBinPath.Location = new System.Drawing.Point(1195, 47);
            this.BrowseGitBinPath.Name = "BrowseGitBinPath";
            this.BrowseGitBinPath.Size = new System.Drawing.Size(75, 25);
            this.BrowseGitBinPath.TabIndex = 6;
            this.BrowseGitBinPath.Text = "Browse";
            this.BrowseGitBinPath.UseVisualStyleBackColor = true;
            this.BrowseGitBinPath.Click += new System.EventHandler(this.BrowseGitBinPath_Click);
            // 
            // GitBinPath
            // 
            this.GitBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.GitBinPath.Location = new System.Drawing.Point(300, 49);
            this.GitBinPath.MaxLength = 300;
            this.GitBinPath.Name = "GitBinPath";
            this.GitBinPath.Size = new System.Drawing.Size(889, 21);
            this.GitBinPath.TabIndex = 5;
            this.GitBinPath.TextChanged += new System.EventHandler(this.GitBinPath_TextChanged);
            // 
            // GitPath
            // 
            this.GitPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.GitPath.Location = new System.Drawing.Point(300, 18);
            this.GitPath.MaxLength = 300;
            this.GitPath.Name = "GitPath";
            this.GitPath.Size = new System.Drawing.Size(889, 21);
            this.GitPath.TabIndex = 2;
            this.GitPath.TextChanged += new System.EventHandler(this.GitPath_TextChanged);
            // 
            // BrowseGitPath
            // 
            this.BrowseGitPath.AutoSize = true;
            this.BrowseGitPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BrowseGitPath.Location = new System.Drawing.Point(1195, 16);
            this.BrowseGitPath.Name = "BrowseGitPath";
            this.BrowseGitPath.Size = new System.Drawing.Size(75, 25);
            this.BrowseGitPath.TabIndex = 3;
            this.BrowseGitPath.Text = "Browse";
            this.BrowseGitPath.UseVisualStyleBackColor = true;
            this.BrowseGitPath.Click += new System.EventHandler(this.BrowseGitPath_Click);
            // 
            // GitSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.groupBox7);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GitSettingsPage";
            this.Size = new System.Drawing.Size(1297, 592);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label homeIsSetToLabel;
        private System.Windows.Forms.Button ChangeHomeButton;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.LinkLabel downloadGitForWindows;
        private System.Windows.Forms.Button BrowseGitBinPath;
        private System.Windows.Forms.TextBox GitPath;
        private System.Windows.Forms.Button BrowseGitPath;
        private System.Windows.Forms.TextBox GitBinPath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label50;
    }
}
