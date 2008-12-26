namespace GitUI
{
    partial class FormSettigns
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettigns));
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.KeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.MergeTool = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.Editor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.UserEmail = new System.Windows.Forms.TextBox();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.Rescan = new System.Windows.Forms.Button();
            this.CheckAtStartup = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.GitFound = new System.Windows.Forms.Button();
            this.DiffTool = new System.Windows.Forms.Button();
            this.UserNameSet = new System.Windows.Forms.Button();
            this.ShellExtensionsRegistered = new System.Windows.Forms.Button();
            this.GitExtensionsInstall = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.MaxCommits = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.GlobalSettingsPage = new System.Windows.Forms.TabPage();
            this.GlobalKeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.GlobalMergeTool = new System.Windows.Forms.TextBox();
            this.GlobalEditor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.GlobalUserEmail = new System.Windows.Forms.TextBox();
            this.GlobalUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Ok = new System.Windows.Forms.Button();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxCommits)).BeginInit();
            this.GlobalSettingsPage.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.KeepMergeBackup);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.MergeTool);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.Editor);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.UserEmail);
            this.tabPage1.Controls.Add(this.UserName);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(655, 214);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Local settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // KeepMergeBackup
            // 
            this.KeepMergeBackup.AutoSize = true;
            this.KeepMergeBackup.Location = new System.Drawing.Point(187, 126);
            this.KeepMergeBackup.Name = "KeepMergeBackup";
            this.KeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.KeepMergeBackup.TabIndex = 9;
            this.KeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 126);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(156, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Keep backup (.orig) after merge";
            // 
            // MergeTool
            // 
            this.MergeTool.Location = new System.Drawing.Point(118, 94);
            this.MergeTool.Name = "MergeTool";
            this.MergeTool.Size = new System.Drawing.Size(159, 20);
            this.MergeTool.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 97);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Mergetool (kdiff3)";
            // 
            // Editor
            // 
            this.Editor.Location = new System.Drawing.Point(118, 67);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(159, 20);
            this.Editor.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Editor ( notepad.exe)";
            // 
            // UserEmail
            // 
            this.UserEmail.Location = new System.Drawing.Point(118, 40);
            this.UserEmail.Name = "UserEmail";
            this.UserEmail.Size = new System.Drawing.Size(159, 20);
            this.UserEmail.TabIndex = 3;
            this.UserEmail.TextChanged += new System.EventHandler(this.UserEmail_TextChanged);
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(118, 12);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(159, 20);
            this.UserName.TabIndex = 1;
            this.UserName.TextChanged += new System.EventHandler(this.UserName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "User email";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "User name";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.GlobalSettingsPage);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(663, 240);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.Rescan);
            this.tabPage3.Controls.Add(this.CheckAtStartup);
            this.tabPage3.Controls.Add(this.label11);
            this.tabPage3.Controls.Add(this.GitFound);
            this.tabPage3.Controls.Add(this.DiffTool);
            this.tabPage3.Controls.Add(this.UserNameSet);
            this.tabPage3.Controls.Add(this.ShellExtensionsRegistered);
            this.tabPage3.Controls.Add(this.GitExtensionsInstall);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(655, 214);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Checklist";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(576, 188);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(75, 23);
            this.Rescan.TabIndex = 8;
            this.Rescan.Text = "Rescan";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // CheckAtStartup
            // 
            this.CheckAtStartup.AutoSize = true;
            this.CheckAtStartup.Location = new System.Drawing.Point(384, 192);
            this.CheckAtStartup.Name = "CheckAtStartup";
            this.CheckAtStartup.Size = new System.Drawing.Size(182, 17);
            this.CheckAtStartup.TabIndex = 7;
            this.CheckAtStartup.Text = "Check settings at startup (slower)";
            this.CheckAtStartup.UseVisualStyleBackColor = true;
            this.CheckAtStartup.CheckedChanged += new System.EventHandler(this.CheckAtStartup_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(550, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "The checklist below validates the basic settings needed for GitExtensions to work" +
                " properly. (click on an item to fix it)";
            // 
            // GitFound
            // 
            this.GitFound.FlatAppearance.BorderSize = 0;
            this.GitFound.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitFound.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitFound.Location = new System.Drawing.Point(8, 145);
            this.GitFound.Name = "GitFound";
            this.GitFound.Size = new System.Drawing.Size(631, 23);
            this.GitFound.TabIndex = 5;
            this.GitFound.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitFound.UseVisualStyleBackColor = true;
            this.GitFound.Click += new System.EventHandler(this.GitFound_Click);
            // 
            // DiffTool
            // 
            this.DiffTool.FlatAppearance.BorderSize = 0;
            this.DiffTool.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.DiffTool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DiffTool.Location = new System.Drawing.Point(8, 116);
            this.DiffTool.Name = "DiffTool";
            this.DiffTool.Size = new System.Drawing.Size(631, 23);
            this.DiffTool.TabIndex = 4;
            this.DiffTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiffTool.UseVisualStyleBackColor = true;
            this.DiffTool.Click += new System.EventHandler(this.DiffTool_Click);
            // 
            // UserNameSet
            // 
            this.UserNameSet.FlatAppearance.BorderSize = 0;
            this.UserNameSet.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.UserNameSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserNameSet.Location = new System.Drawing.Point(8, 87);
            this.UserNameSet.Name = "UserNameSet";
            this.UserNameSet.Size = new System.Drawing.Size(631, 23);
            this.UserNameSet.TabIndex = 3;
            this.UserNameSet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UserNameSet.UseVisualStyleBackColor = true;
            this.UserNameSet.Click += new System.EventHandler(this.UserNameSet_Click);
            // 
            // ShellExtensionsRegistered
            // 
            this.ShellExtensionsRegistered.FlatAppearance.BorderSize = 0;
            this.ShellExtensionsRegistered.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ShellExtensionsRegistered.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShellExtensionsRegistered.Location = new System.Drawing.Point(8, 58);
            this.ShellExtensionsRegistered.Name = "ShellExtensionsRegistered";
            this.ShellExtensionsRegistered.Size = new System.Drawing.Size(631, 23);
            this.ShellExtensionsRegistered.TabIndex = 2;
            this.ShellExtensionsRegistered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ShellExtensionsRegistered.UseVisualStyleBackColor = true;
            this.ShellExtensionsRegistered.Click += new System.EventHandler(this.ShellExtensionsRegistered_Click);
            // 
            // GitExtensionsInstall
            // 
            this.GitExtensionsInstall.FlatAppearance.BorderSize = 0;
            this.GitExtensionsInstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.GitExtensionsInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitExtensionsInstall.Location = new System.Drawing.Point(8, 29);
            this.GitExtensionsInstall.Name = "GitExtensionsInstall";
            this.GitExtensionsInstall.Size = new System.Drawing.Size(631, 23);
            this.GitExtensionsInstall.TabIndex = 1;
            this.GitExtensionsInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GitExtensionsInstall.UseVisualStyleBackColor = true;
            this.GitExtensionsInstall.Click += new System.EventHandler(this.GitExtensionsInstall_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.MaxCommits);
            this.tabPage2.Controls.Add(this.label12);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(655, 214);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Git extensions";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // MaxCommits
            // 
            this.MaxCommits.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.MaxCommits.Location = new System.Drawing.Point(315, 12);
            this.MaxCommits.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.MaxCommits.Name = "MaxCommits";
            this.MaxCommits.Size = new System.Drawing.Size(114, 20);
            this.MaxCommits.TabIndex = 2;
            this.MaxCommits.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(9, 12);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(285, 39);
            this.label12.TabIndex = 0;
            this.label12.Text = "Limit number of commits that will be loaded in list at startup.\r\nOther commits wi" +
                "ll be loaded when needed. Lower number \r\nresult is shorter startup time, but slo" +
                "wer scrolling.";
            // 
            // GlobalSettingsPage
            // 
            this.GlobalSettingsPage.Controls.Add(this.GlobalKeepMergeBackup);
            this.GlobalSettingsPage.Controls.Add(this.label10);
            this.GlobalSettingsPage.Controls.Add(this.label7);
            this.GlobalSettingsPage.Controls.Add(this.GlobalMergeTool);
            this.GlobalSettingsPage.Controls.Add(this.GlobalEditor);
            this.GlobalSettingsPage.Controls.Add(this.label6);
            this.GlobalSettingsPage.Controls.Add(this.GlobalUserEmail);
            this.GlobalSettingsPage.Controls.Add(this.GlobalUserName);
            this.GlobalSettingsPage.Controls.Add(this.label4);
            this.GlobalSettingsPage.Controls.Add(this.label3);
            this.GlobalSettingsPage.Location = new System.Drawing.Point(4, 22);
            this.GlobalSettingsPage.Name = "GlobalSettingsPage";
            this.GlobalSettingsPage.Padding = new System.Windows.Forms.Padding(3);
            this.GlobalSettingsPage.Size = new System.Drawing.Size(655, 214);
            this.GlobalSettingsPage.TabIndex = 1;
            this.GlobalSettingsPage.Text = "Global settings";
            this.GlobalSettingsPage.UseVisualStyleBackColor = true;
            // 
            // GlobalKeepMergeBackup
            // 
            this.GlobalKeepMergeBackup.AutoSize = true;
            this.GlobalKeepMergeBackup.Location = new System.Drawing.Point(183, 125);
            this.GlobalKeepMergeBackup.Name = "GlobalKeepMergeBackup";
            this.GlobalKeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.GlobalKeepMergeBackup.TabIndex = 9;
            this.GlobalKeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 125);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(156, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Keep backup (.orig) after merge";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Mergetool (kdiff3)";
            // 
            // GlobalMergeTool
            // 
            this.GlobalMergeTool.Location = new System.Drawing.Point(113, 92);
            this.GlobalMergeTool.Name = "GlobalMergeTool";
            this.GlobalMergeTool.Size = new System.Drawing.Size(164, 20);
            this.GlobalMergeTool.TabIndex = 6;
            // 
            // GlobalEditor
            // 
            this.GlobalEditor.Location = new System.Drawing.Point(113, 65);
            this.GlobalEditor.Name = "GlobalEditor";
            this.GlobalEditor.Size = new System.Drawing.Size(164, 20);
            this.GlobalEditor.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Editor (notepad.exe)";
            // 
            // GlobalUserEmail
            // 
            this.GlobalUserEmail.Location = new System.Drawing.Point(113, 37);
            this.GlobalUserEmail.Name = "GlobalUserEmail";
            this.GlobalUserEmail.Size = new System.Drawing.Size(164, 20);
            this.GlobalUserEmail.TabIndex = 3;
            // 
            // GlobalUserName
            // 
            this.GlobalUserName.Location = new System.Drawing.Point(113, 8);
            this.GlobalUserName.Name = "GlobalUserName";
            this.GlobalUserName.Size = new System.Drawing.Size(164, 20);
            this.GlobalUserName.TabIndex = 2;
            this.GlobalUserName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "User email";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "User name";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Size = new System.Drawing.Size(663, 273);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 1;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(584, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // FormSettigns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 273);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSettigns";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.FormSettigns_Load);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxCommits)).EndInit();
            this.GlobalSettingsPage.ResumeLayout(false);
            this.GlobalSettingsPage.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox UserEmail;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TabPage GlobalSettingsPage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox GlobalUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox GlobalUserEmail;
        private System.Windows.Forms.TextBox Editor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox GlobalEditor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox GlobalMergeTool;
        private System.Windows.Forms.TextBox MergeTool;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox KeepMergeBackup;
        private System.Windows.Forms.CheckBox GlobalKeepMergeBackup;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button GitExtensionsInstall;
        private System.Windows.Forms.Button ShellExtensionsRegistered;
        private System.Windows.Forms.Button UserNameSet;
        private System.Windows.Forms.Button DiffTool;
        private System.Windows.Forms.Button GitFound;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox CheckAtStartup;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown MaxCommits;

    }
}