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
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SshSettingsPage));
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Putty = new System.Windows.Forms.RadioButton();
            this.OtherSshBrowse = new System.Windows.Forms.Button();
            this.OtherSsh = new System.Windows.Forms.TextBox();
            this.OpenSSH = new System.Windows.Forms.RadioButton();
            this.Other = new System.Windows.Forms.RadioButton();
            this.label18 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.PageantBrowse = new System.Windows.Forms.Button();
            this.AutostartPageant = new System.Windows.Forms.CheckBox();
            this.PuttygenBrowse = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.PlinkBrowse = new System.Windows.Forms.Button();
            this.PageantPath = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.PlinkPath = new System.Windows.Forms.TextBox();
            this.PuttygenPath = new System.Windows.Forms.TextBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(674, 975);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8);
            this.groupBox1.Size = new System.Drawing.Size(668, 107);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Specify which ssh client to use";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel2.Controls.Add(this.Putty, 0, 0);
            tableLayoutPanel2.Controls.Add(this.OtherSshBrowse, 2, 2);
            tableLayoutPanel2.Controls.Add(this.OtherSsh, 1, 2);
            tableLayoutPanel2.Controls.Add(this.OpenSSH, 0, 1);
            tableLayoutPanel2.Controls.Add(this.Other, 0, 2);
            tableLayoutPanel2.Controls.Add(this.label18, 1, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(8, 22);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.Size = new System.Drawing.Size(652, 77);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // Putty
            // 
            this.Putty.AutoSize = true;
            this.Putty.Checked = true;
            this.Putty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Putty.Location = new System.Drawing.Point(3, 3);
            this.Putty.Name = "Putty";
            this.Putty.Size = new System.Drawing.Size(134, 17);
            this.Putty.TabIndex = 0;
            this.Putty.TabStop = true;
            this.Putty.Text = "PuTTY";
            this.Putty.UseVisualStyleBackColor = true;
            this.Putty.CheckedChanged += new System.EventHandler(this.Putty_CheckedChanged);
            // 
            // OtherSshBrowse
            // 
            this.OtherSshBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherSshBrowse.Location = new System.Drawing.Point(574, 49);
            this.OtherSshBrowse.Name = "OtherSshBrowse";
            this.OtherSshBrowse.Size = new System.Drawing.Size(75, 25);
            this.OtherSshBrowse.TabIndex = 5;
            this.OtherSshBrowse.Text = "Browse";
            this.OtherSshBrowse.UseVisualStyleBackColor = true;
            this.OtherSshBrowse.Click += new System.EventHandler(this.OtherSshBrowse_Click);
            // 
            // OtherSsh
            // 
            this.OtherSsh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OtherSsh.Location = new System.Drawing.Point(143, 49);
            this.OtherSsh.MinimumSize = new System.Drawing.Size(300, 0);
            this.OtherSsh.Name = "OtherSsh";
            this.OtherSsh.Size = new System.Drawing.Size(425, 21);
            this.OtherSsh.TabIndex = 4;
            // 
            // OpenSSH
            // 
            this.OpenSSH.AutoSize = true;
            this.OpenSSH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OpenSSH.Location = new System.Drawing.Point(3, 26);
            this.OpenSSH.Name = "OpenSSH";
            this.OpenSSH.Size = new System.Drawing.Size(134, 17);
            this.OpenSSH.TabIndex = 2;
            this.OpenSSH.Text = "OpenSSH";
            this.OpenSSH.UseVisualStyleBackColor = true;
            this.OpenSSH.CheckedChanged += new System.EventHandler(this.OpenSSH_CheckedChanged);
            // 
            // Other
            // 
            this.Other.AutoSize = true;
            this.Other.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Other.Location = new System.Drawing.Point(3, 49);
            this.Other.Name = "Other";
            this.Other.Size = new System.Drawing.Size(134, 25);
            this.Other.TabIndex = 3;
            this.Other.Text = "Other ssh client";
            this.Other.UseVisualStyleBackColor = true;
            this.Other.CheckedChanged += new System.EventHandler(this.Other_CheckedChanged);
            // 
            // label18
            // 
            this.label18.BackColor = System.Drawing.SystemColors.Info;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(143, 0);
            this.label18.Name = "label18";
            tableLayoutPanel2.SetRowSpan(this.label18, 2);
            this.label18.Size = new System.Drawing.Size(425, 46);
            this.label18.TabIndex = 1;
            this.label18.Text = resources.GetString("label18.Text");
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(tableLayoutPanel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(3, 116);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(8);
            this.groupBox2.Size = new System.Drawing.Size(668, 146);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Configure PuTTY";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel3.Controls.Add(this.label15, 0, 0);
            tableLayoutPanel3.Controls.Add(this.PageantBrowse, 2, 2);
            tableLayoutPanel3.Controls.Add(this.AutostartPageant, 1, 3);
            tableLayoutPanel3.Controls.Add(this.PuttygenBrowse, 2, 1);
            tableLayoutPanel3.Controls.Add(this.label16, 0, 1);
            tableLayoutPanel3.Controls.Add(this.PlinkBrowse, 2, 0);
            tableLayoutPanel3.Controls.Add(this.PageantPath, 1, 2);
            tableLayoutPanel3.Controls.Add(this.label17, 0, 2);
            tableLayoutPanel3.Controls.Add(this.PlinkPath, 1, 0);
            tableLayoutPanel3.Controls.Add(this.PuttygenPath, 1, 1);
            tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel3.Location = new System.Drawing.Point(8, 22);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 4;
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel3.Size = new System.Drawing.Size(652, 116);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(3, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(134, 31);
            this.label15.TabIndex = 0;
            this.label15.Text = "Path to plink";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PageantBrowse
            // 
            this.PageantBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PageantBrowse.Location = new System.Drawing.Point(574, 65);
            this.PageantBrowse.Name = "PageantBrowse";
            this.PageantBrowse.Size = new System.Drawing.Size(75, 25);
            this.PageantBrowse.TabIndex = 8;
            this.PageantBrowse.Text = "Browse";
            this.PageantBrowse.UseVisualStyleBackColor = true;
            this.PageantBrowse.Click += new System.EventHandler(this.PageantBrowse_Click);
            // 
            // AutostartPageant
            // 
            this.AutostartPageant.AutoEllipsis = true;
            this.AutostartPageant.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutostartPageant.Checked = true;
            this.AutostartPageant.CheckState = System.Windows.Forms.CheckState.Checked;
            tableLayoutPanel3.SetColumnSpan(this.AutostartPageant, 2);
            this.AutostartPageant.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AutostartPageant.Location = new System.Drawing.Point(143, 96);
            this.AutostartPageant.Name = "AutostartPageant";
            this.AutostartPageant.Size = new System.Drawing.Size(506, 17);
            this.AutostartPageant.TabIndex = 9;
            this.AutostartPageant.Text = "Automatically start authentication client when a private key is configured for a " +
    "remote";
            this.AutostartPageant.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutostartPageant.UseVisualStyleBackColor = true;
            // 
            // PuttygenBrowse
            // 
            this.PuttygenBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PuttygenBrowse.Location = new System.Drawing.Point(574, 34);
            this.PuttygenBrowse.Name = "PuttygenBrowse";
            this.PuttygenBrowse.Size = new System.Drawing.Size(75, 25);
            this.PuttygenBrowse.TabIndex = 5;
            this.PuttygenBrowse.Text = "Browse";
            this.PuttygenBrowse.UseVisualStyleBackColor = true;
            this.PuttygenBrowse.Click += new System.EventHandler(this.PuttygenBrowse_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(3, 31);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(134, 31);
            this.label16.TabIndex = 3;
            this.label16.Text = "Path to puttygen";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PlinkBrowse
            // 
            this.PlinkBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlinkBrowse.Location = new System.Drawing.Point(574, 3);
            this.PlinkBrowse.Name = "PlinkBrowse";
            this.PlinkBrowse.Size = new System.Drawing.Size(75, 25);
            this.PlinkBrowse.TabIndex = 2;
            this.PlinkBrowse.Text = "Browse";
            this.PlinkBrowse.UseVisualStyleBackColor = true;
            this.PlinkBrowse.Click += new System.EventHandler(this.PuttyBrowse_Click);
            // 
            // PageantPath
            // 
            this.PageantPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PageantPath.Location = new System.Drawing.Point(143, 65);
            this.PageantPath.MinimumSize = new System.Drawing.Size(300, 0);
            this.PageantPath.Name = "PageantPath";
            this.PageantPath.Size = new System.Drawing.Size(425, 21);
            this.PageantPath.TabIndex = 7;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Location = new System.Drawing.Point(3, 62);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(134, 31);
            this.label17.TabIndex = 6;
            this.label17.Text = "Path to pageant";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PlinkPath
            // 
            this.PlinkPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlinkPath.Location = new System.Drawing.Point(143, 3);
            this.PlinkPath.MinimumSize = new System.Drawing.Size(300, 0);
            this.PlinkPath.Name = "PlinkPath";
            this.PlinkPath.Size = new System.Drawing.Size(425, 21);
            this.PlinkPath.TabIndex = 1;
            // 
            // PuttygenPath
            // 
            this.PuttygenPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PuttygenPath.Location = new System.Drawing.Point(143, 34);
            this.PuttygenPath.MinimumSize = new System.Drawing.Size(300, 0);
            this.PuttygenPath.Name = "PuttygenPath";
            this.PuttygenPath.Size = new System.Drawing.Size(425, 21);
            this.PuttygenPath.TabIndex = 4;
            // 
            // SshSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tableLayoutPanel1);
            this.Name = "SshSettingsPage";
            this.Size = new System.Drawing.Size(674, 975);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
