﻿namespace GitUI.CommandsDialogs.SettingsDialog.Pages
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
            this.homeIsSetToLabel = new System.Windows.Forms.Label();
            this.ChangeHomeButton = new System.Windows.Forms.Button();
            this.label51 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.downloadGitForWindows = new System.Windows.Forms.LinkLabel();
            this.label50 = new System.Windows.Forms.Label();
            this.BrowseGitBinPath = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.GitPath = new System.Windows.Forms.TextBox();
            this.BrowseGitPath = new System.Windows.Forms.Button();
            this.GitBinPath = new System.Windows.Forms.TextBox();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox8
            // 
            this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox8.Controls.Add(this.homeIsSetToLabel);
            this.groupBox8.Controls.Add(this.ChangeHomeButton);
            this.groupBox8.Controls.Add(this.label51);
            this.groupBox8.Location = new System.Drawing.Point(3, 133);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(970, 144);
            this.groupBox8.TabIndex = 12;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Environment";
            // 
            // homeIsSetToLabel
            // 
            this.homeIsSetToLabel.AutoSize = true;
            this.homeIsSetToLabel.Location = new System.Drawing.Point(8, 88);
            this.homeIsSetToLabel.Name = "homeIsSetToLabel";
            this.homeIsSetToLabel.Size = new System.Drawing.Size(119, 16);
            this.homeIsSetToLabel.TabIndex = 12;
            this.homeIsSetToLabel.Text = "HOME is set to: {0}";
            // 
            // ChangeHomeButton
            // 
            this.ChangeHomeButton.Location = new System.Drawing.Point(11, 107);
            this.ChangeHomeButton.Name = "ChangeHomeButton";
            this.ChangeHomeButton.Size = new System.Drawing.Size(132, 25);
            this.ChangeHomeButton.TabIndex = 11;
            this.ChangeHomeButton.Text = "Change HOME";
            this.ChangeHomeButton.UseVisualStyleBackColor = true;
            this.ChangeHomeButton.Click += new System.EventHandler(this.ChangeHomeButton_Click);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(8, 19);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(674, 32);
            this.label51.TabIndex = 0;
            this.label51.Text = resources.GetString("label51.Text");
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Controls.Add(this.downloadGitForWindows);
            this.groupBox7.Controls.Add(this.label50);
            this.groupBox7.Controls.Add(this.BrowseGitBinPath);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.label14);
            this.groupBox7.Controls.Add(this.GitPath);
            this.groupBox7.Controls.Add(this.BrowseGitPath);
            this.groupBox7.Controls.Add(this.GitBinPath);
            this.groupBox7.Location = new System.Drawing.Point(3, 3);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(970, 115);
            this.groupBox7.TabIndex = 11;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Git";
            // 
            // downloadGitForWindows
            // 
            this.downloadGitForWindows.AutoSize = true;
            this.downloadGitForWindows.Location = new System.Drawing.Point(369, 91);
            this.downloadGitForWindows.Name = "downloadMsysgit";
            this.downloadGitForWindows.Size = new System.Drawing.Size(111, 16);
            this.downloadGitForWindows.TabIndex = 10;
            this.downloadGitForWindows.TabStop = true;
            this.downloadGitForWindows.Text = "Download msysgit";
            this.downloadGitForWindows.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadGitForWindows_LinkClicked);
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(8, 18);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(557, 16);
            this.label50.TabIndex = 9;
            this.label50.Text = "Git Extensions can use Git for Windows or cygwin to access git repositories. Set the correct paths below.";
            // 
            // BrowseGitBinPath
            // 
            this.BrowseGitBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitBinPath.Location = new System.Drawing.Point(864, 62);
            this.BrowseGitBinPath.Name = "BrowseGitBinPath";
            this.BrowseGitBinPath.Size = new System.Drawing.Size(97, 25);
            this.BrowseGitBinPath.TabIndex = 8;
            this.BrowseGitBinPath.Text = "Browse";
            this.BrowseGitBinPath.UseVisualStyleBackColor = true;
            this.BrowseGitBinPath.Click += new System.EventHandler(this.BrowseGitBinPath_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 44);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(267, 16);
            this.label13.TabIndex = 3;
            this.label13.Text = "Command used to run git (git.cmd or git.exe)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 70);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(344, 16);
            this.label14.TabIndex = 6;
            this.label14.Text = "Path to linux tools (sh). Leave empty when it is in the path.";
            // 
            // GitPath
            // 
            this.GitPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GitPath.Location = new System.Drawing.Point(372, 38);
            this.GitPath.Name = "GitPath";
            this.GitPath.Size = new System.Drawing.Size(486, 23);
            this.GitPath.TabIndex = 4;
            this.GitPath.TextChanged += new System.EventHandler(this.GitPath_TextChanged);
            // 
            // BrowseGitPath
            // 
            this.BrowseGitPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitPath.Location = new System.Drawing.Point(864, 36);
            this.BrowseGitPath.Name = "BrowseGitPath";
            this.BrowseGitPath.Size = new System.Drawing.Size(97, 25);
            this.BrowseGitPath.TabIndex = 5;
            this.BrowseGitPath.Text = "Browse";
            this.BrowseGitPath.UseVisualStyleBackColor = true;
            this.BrowseGitPath.Click += new System.EventHandler(this.BrowseGitPath_Click);
            // 
            // GitBinPath
            // 
            this.GitBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GitBinPath.Location = new System.Drawing.Point(372, 65);
            this.GitBinPath.Name = "GitBinPath";
            this.GitBinPath.Size = new System.Drawing.Size(486, 23);
            this.GitBinPath.TabIndex = 7;
            this.GitBinPath.TextChanged += new System.EventHandler(this.GitBinPath_TextChanged);
            // 
            // GitSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.groupBox7);
            this.Name = "GitSettingsPage";
            this.Size = new System.Drawing.Size(976, 538);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label homeIsSetToLabel;
        private System.Windows.Forms.Button ChangeHomeButton;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.LinkLabel downloadGitForWindows;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Button BrowseGitBinPath;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox GitPath;
        private System.Windows.Forms.Button BrowseGitPath;
        private System.Windows.Forms.TextBox GitBinPath;
    }
}
