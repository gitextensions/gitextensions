namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class SshSettingsPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SshSettingsPage));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AutostartPageant = new System.Windows.Forms.CheckBox();
            this.PageantPath = new System.Windows.Forms.TextBox();
            this.PageantBrowse = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.PuttygenPath = new System.Windows.Forms.TextBox();
            this.PuttygenBrowse = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.PlinkPath = new System.Windows.Forms.TextBox();
            this.PlinkBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OtherSsh = new System.Windows.Forms.TextBox();
            this.OtherSshBrowse = new System.Windows.Forms.Button();
            this.Other = new System.Windows.Forms.RadioButton();
            this.label18 = new System.Windows.Forms.Label();
            this.OpenSSH = new System.Windows.Forms.RadioButton();
            this.Putty = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.SuggestGitCred = new System.Windows.Forms.Button();
            this.label48 = new System.Windows.Forms.Label();
            this.BrowseGitCred = new System.Windows.Forms.Button();
            this.GitCredPath = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBox2
            //
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.AutostartPageant);
            this.groupBox2.Controls.Add(this.PageantPath);
            this.groupBox2.Controls.Add(this.PageantBrowse);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.PuttygenPath);
            this.groupBox2.Controls.Add(this.PuttygenBrowse);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.PlinkPath);
            this.groupBox2.Controls.Add(this.PlinkBrowse);
            this.groupBox2.Location = new System.Drawing.Point(4, 162);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(971, 192);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Configure PuTTY";
            //
            // AutostartPageant
            //
            this.AutostartPageant.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutostartPageant.Checked = true;
            this.AutostartPageant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutostartPageant.Location = new System.Drawing.Point(179, 129);
            this.AutostartPageant.Margin = new System.Windows.Forms.Padding(4);
            this.AutostartPageant.Name = "AutostartPageant";
            this.AutostartPageant.Size = new System.Drawing.Size(541, 56);
            this.AutostartPageant.TabIndex = 11;
            this.AutostartPageant.Text = "Automatically start authentication client when a private key is configured for a " +
    "remote";
            this.AutostartPageant.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutostartPageant.UseVisualStyleBackColor = true;
            //
            // PageantPath
            //
            this.PageantPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PageantPath.Location = new System.Drawing.Point(179, 95);
            this.PageantPath.Margin = new System.Windows.Forms.Padding(4);
            this.PageantPath.Name = "PageantPath";
            this.PageantPath.Size = new System.Drawing.Size(679, 27);
            this.PageantPath.TabIndex = 9;
            //
            // PageantBrowse
            //
            this.PageantBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PageantBrowse.Location = new System.Drawing.Point(866, 92);
            this.PageantBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.PageantBrowse.Name = "PageantBrowse";
            this.PageantBrowse.Size = new System.Drawing.Size(94, 31);
            this.PageantBrowse.TabIndex = 10;
            this.PageantBrowse.Text = "Browse";
            this.PageantBrowse.UseVisualStyleBackColor = true;
            this.PageantBrowse.Click += new System.EventHandler(this.PageantBrowse_Click);
            //
            // label17
            //
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(10, 99);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(115, 20);
            this.label17.TabIndex = 8;
            this.label17.Text = "Path to pageant";
            //
            // PuttygenPath
            //
            this.PuttygenPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenPath.Location = new System.Drawing.Point(179, 58);
            this.PuttygenPath.Margin = new System.Windows.Forms.Padding(4);
            this.PuttygenPath.Name = "PuttygenPath";
            this.PuttygenPath.Size = new System.Drawing.Size(679, 27);
            this.PuttygenPath.TabIndex = 6;
            //
            // PuttygenBrowse
            //
            this.PuttygenBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenBrowse.Location = new System.Drawing.Point(866, 55);
            this.PuttygenBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.PuttygenBrowse.Name = "PuttygenBrowse";
            this.PuttygenBrowse.Size = new System.Drawing.Size(94, 31);
            this.PuttygenBrowse.TabIndex = 7;
            this.PuttygenBrowse.Text = "Browse";
            this.PuttygenBrowse.UseVisualStyleBackColor = true;
            this.PuttygenBrowse.Click += new System.EventHandler(this.PuttygenBrowse_Click);
            //
            // label16
            //
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(10, 61);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(119, 20);
            this.label16.TabIndex = 5;
            this.label16.Text = "Path to puttygen";
            //
            // label15
            //
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(10, 25);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(92, 20);
            this.label15.TabIndex = 4;
            this.label15.Text = "Path to plink";
            //
            // PlinkPath
            //
            this.PlinkPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkPath.Location = new System.Drawing.Point(179, 21);
            this.PlinkPath.Margin = new System.Windows.Forms.Padding(4);
            this.PlinkPath.Name = "PlinkPath";
            this.PlinkPath.Size = new System.Drawing.Size(679, 27);
            this.PlinkPath.TabIndex = 2;
            //
            // PlinkBrowse
            //
            this.PlinkBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkBrowse.Location = new System.Drawing.Point(866, 18);
            this.PlinkBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.PlinkBrowse.Name = "PlinkBrowse";
            this.PlinkBrowse.Size = new System.Drawing.Size(94, 31);
            this.PlinkBrowse.TabIndex = 3;
            this.PlinkBrowse.Text = "Browse";
            this.PlinkBrowse.UseVisualStyleBackColor = true;
            this.PlinkBrowse.Click += new System.EventHandler(this.PuttyBrowse_Click);
            //
            // groupBox1
            //
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.OtherSsh);
            this.groupBox1.Controls.Add(this.OtherSshBrowse);
            this.groupBox1.Controls.Add(this.Other);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.OpenSSH);
            this.groupBox1.Controls.Add(this.Putty);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(971, 151);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Specify which ssh client to use";
            //
            // OtherSsh
            //
            this.OtherSsh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherSsh.Location = new System.Drawing.Point(179, 100);
            this.OtherSsh.Margin = new System.Windows.Forms.Padding(4);
            this.OtherSsh.Name = "OtherSsh";
            this.OtherSsh.Size = new System.Drawing.Size(679, 27);
            this.OtherSsh.TabIndex = 4;
            //
            // OtherSshBrowse
            //
            this.OtherSshBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherSshBrowse.Location = new System.Drawing.Point(866, 96);
            this.OtherSshBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.OtherSshBrowse.Name = "OtherSshBrowse";
            this.OtherSshBrowse.Size = new System.Drawing.Size(94, 31);
            this.OtherSshBrowse.TabIndex = 5;
            this.OtherSshBrowse.Text = "Browse";
            this.OtherSshBrowse.UseVisualStyleBackColor = true;
            this.OtherSshBrowse.Click += new System.EventHandler(this.OtherSshBrowse_Click);
            //
            // Other
            //
            this.Other.AutoSize = true;
            this.Other.Location = new System.Drawing.Point(11, 101);
            this.Other.Margin = new System.Windows.Forms.Padding(4);
            this.Other.Name = "Other";
            this.Other.Size = new System.Drawing.Size(131, 24);
            this.Other.TabIndex = 3;
            this.Other.Text = "Other ssh client";
            this.Other.UseVisualStyleBackColor = true;
            this.Other.CheckedChanged += new System.EventHandler(this.Other_CheckedChanged);
            //
            // label18
            //
            this.label18.AutoSize = true;
            this.label18.BackColor = System.Drawing.SystemColors.Info;
            this.label18.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label18.Location = new System.Drawing.Point(176, 21);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(580, 62);
            this.label18.TabIndex = 2;
            this.label18.Text = resources.GetString("label18.Text");
            //
            // OpenSSH
            //
            this.OpenSSH.AutoSize = true;
            this.OpenSSH.Location = new System.Drawing.Point(11, 62);
            this.OpenSSH.Margin = new System.Windows.Forms.Padding(4);
            this.OpenSSH.Name = "OpenSSH";
            this.OpenSSH.Size = new System.Drawing.Size(93, 24);
            this.OpenSSH.TabIndex = 1;
            this.OpenSSH.Text = "OpenSSH";
            this.OpenSSH.UseVisualStyleBackColor = true;
            this.OpenSSH.CheckedChanged += new System.EventHandler(this.OpenSSH_CheckedChanged);
            //
            // Putty
            //
            this.Putty.AutoSize = true;
            this.Putty.Checked = true;
            this.Putty.Location = new System.Drawing.Point(11, 25);
            this.Putty.Margin = new System.Windows.Forms.Padding(4);
            this.Putty.Name = "Putty";
            this.Putty.Size = new System.Drawing.Size(70, 24);
            this.Putty.TabIndex = 0;
            this.Putty.TabStop = true;
            this.Putty.Text = "PuTTY";
            this.Putty.UseVisualStyleBackColor = true;
            this.Putty.CheckedChanged += new System.EventHandler(this.Putty_CheckedChanged);
            //
            // groupBox3
            //
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.SuggestGitCred);
            this.groupBox3.Controls.Add(this.label48);
            this.groupBox3.Controls.Add(this.BrowseGitCred);
            this.groupBox3.Controls.Add(this.GitCredPath);
            this.groupBox3.Location = new System.Drawing.Point(4, 362);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(971, 74);
            this.groupBox3.TabIndex = 82;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Configure Git credential helper";
            //
            // SuggestGitCred
            //
            this.SuggestGitCred.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SuggestGitCred.Location = new System.Drawing.Point(837, 27);
            this.SuggestGitCred.Margin = new System.Windows.Forms.Padding(4);
            this.SuggestGitCred.Name = "SuggestGitCred";
            this.SuggestGitCred.Size = new System.Drawing.Size(123, 31);
            this.SuggestGitCred.TabIndex = 86;
            this.SuggestGitCred.Text = "Suggest";
            this.SuggestGitCred.UseVisualStyleBackColor = true;
            this.SuggestGitCred.Click += new System.EventHandler(this.SuggestGitCred_Click);
            //
            // label48
            //
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(10, 32);
            this.label48.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(78, 20);
            this.label48.TabIndex = 84;
            this.label48.Text = "Command";
            //
            // BrowseGitCred
            //
            this.BrowseGitCred.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseGitCred.Location = new System.Drawing.Point(735, 27);
            this.BrowseGitCred.Margin = new System.Windows.Forms.Padding(4);
            this.BrowseGitCred.Name = "BrowseGitCred";
            this.BrowseGitCred.Size = new System.Drawing.Size(94, 31);
            this.BrowseGitCred.TabIndex = 83;
            this.BrowseGitCred.Text = "Browse";
            this.BrowseGitCred.UseVisualStyleBackColor = true;
            this.BrowseGitCred.Click += new System.EventHandler(this.BrowseGitCred_Click);
            //
            // GitCredPath
            //
            this.GitCredPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GitCredPath.Location = new System.Drawing.Point(135, 29);
            this.GitCredPath.Margin = new System.Windows.Forms.Padding(4);
            this.GitCredPath.Name = "GitCredPath";
            this.GitCredPath.Size = new System.Drawing.Size(592, 27);
            this.GitCredPath.TabIndex = 81;
            //
            // SshSettingsPage
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SshSettingsPage";
            this.Size = new System.Drawing.Size(971, 699);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox AutostartPageant;
        private System.Windows.Forms.TextBox PageantPath;
        private System.Windows.Forms.Button PageantBrowse;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox PuttygenPath;
        private System.Windows.Forms.Button PuttygenBrowse;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox PlinkPath;
        private System.Windows.Forms.Button PlinkBrowse;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox OtherSsh;
        private System.Windows.Forms.Button OtherSshBrowse;
        private System.Windows.Forms.RadioButton Other;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.RadioButton OpenSSH;
        private System.Windows.Forms.RadioButton Putty;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button SuggestGitCred;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Button BrowseGitCred;
        private System.Windows.Forms.TextBox GitCredPath;
    }
}
