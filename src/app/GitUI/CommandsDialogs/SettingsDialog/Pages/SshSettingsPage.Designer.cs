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
            TableLayoutPanel tableLayoutPanel2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SshSettingsPage));
            TableLayoutPanel tableLayoutPanel3;
            Putty = new RadioButton();
            OtherSsh = new TextBox();
            OpenSSH = new RadioButton();
            Other = new RadioButton();
            label18 = new Label();
            OtherSshBrowse = new Button();
            label15 = new Label();
            PageantBrowse = new Button();
            AutostartPageant = new CheckBox();
            PuttygenBrowse = new Button();
            label16 = new Label();
            PlinkBrowse = new Button();
            PageantPath = new TextBox();
            label17 = new Label();
            PlinkPath = new TextBox();
            PuttygenPath = new TextBox();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(Putty, 0, 0);
            tableLayoutPanel2.Controls.Add(OtherSsh, 1, 2);
            tableLayoutPanel2.Controls.Add(OpenSSH, 0, 1);
            tableLayoutPanel2.Controls.Add(Other, 0, 2);
            tableLayoutPanel2.Controls.Add(label18, 1, 0);
            tableLayoutPanel2.Controls.Add(OtherSshBrowse, 2, 2);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(8, 22);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(883, 78);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // Putty
            // 
            Putty.AutoSize = true;
            Putty.Checked = true;
            Putty.Dock = DockStyle.Fill;
            Putty.Location = new Point(3, 3);
            Putty.Name = "Putty";
            Putty.Size = new Size(134, 17);
            Putty.TabIndex = 0;
            Putty.TabStop = true;
            Putty.Text = "PuTTY";
            Putty.UseVisualStyleBackColor = true;
            Putty.CheckedChanged += Putty_CheckedChanged;
            // 
            // OtherSsh
            // 
            OtherSsh.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            OtherSsh.Location = new Point(143, 51);
            OtherSsh.MaxLength = 300;
            OtherSsh.MinimumSize = new Size(300, 4);
            OtherSsh.Name = "OtherSsh";
            OtherSsh.Size = new Size(656, 21);
            OtherSsh.TabIndex = 4;
            // 
            // OpenSSH
            // 
            OpenSSH.AutoSize = true;
            OpenSSH.Dock = DockStyle.Fill;
            OpenSSH.Location = new Point(3, 26);
            OpenSSH.Name = "OpenSSH";
            OpenSSH.Size = new Size(134, 17);
            OpenSSH.TabIndex = 2;
            OpenSSH.Text = "OpenSSH";
            OpenSSH.UseVisualStyleBackColor = true;
            OpenSSH.CheckedChanged += OpenSSH_CheckedChanged;
            // 
            // Other
            // 
            Other.AutoSize = true;
            Other.Dock = DockStyle.Fill;
            Other.Location = new Point(3, 49);
            Other.Name = "Other";
            Other.Size = new Size(134, 26);
            Other.TabIndex = 3;
            Other.Text = "Other ssh client";
            Other.UseVisualStyleBackColor = true;
            Other.CheckedChanged += Other_CheckedChanged;
            // 
            // label18
            // 
            label18.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label18.AutoSize = true;
            label18.BackColor = SystemColors.Info;
            tableLayoutPanel2.SetColumnSpan(label18, 2);
            label18.ForeColor = SystemColors.InfoText;
            label18.Location = new Point(143, 3);
            label18.Name = "label18";
            tableLayoutPanel2.SetRowSpan(label18, 2);
            label18.Size = new Size(737, 39);
            label18.TabIndex = 1;
            label18.Text = resources.GetString("label18.Text");
            label18.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // OtherSshBrowse
            // 
            OtherSshBrowse.AutoSize = true;
            OtherSshBrowse.Dock = DockStyle.Fill;
            OtherSshBrowse.Location = new Point(805, 49);
            OtherSshBrowse.Name = "OtherSshBrowse";
            OtherSshBrowse.Size = new Size(75, 26);
            OtherSshBrowse.TabIndex = 5;
            OtherSshBrowse.Text = "Browse";
            OtherSshBrowse.UseVisualStyleBackColor = true;
            OtherSshBrowse.Click += OtherSshBrowse_Click;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(label15, 0, 0);
            tableLayoutPanel3.Controls.Add(PageantBrowse, 2, 2);
            tableLayoutPanel3.Controls.Add(AutostartPageant, 1, 3);
            tableLayoutPanel3.Controls.Add(PuttygenBrowse, 2, 1);
            tableLayoutPanel3.Controls.Add(label16, 0, 1);
            tableLayoutPanel3.Controls.Add(PlinkBrowse, 2, 0);
            tableLayoutPanel3.Controls.Add(PageantPath, 1, 2);
            tableLayoutPanel3.Controls.Add(label17, 0, 2);
            tableLayoutPanel3.Controls.Add(PlinkPath, 1, 0);
            tableLayoutPanel3.Controls.Add(PuttygenPath, 1, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(8, 22);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 4;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(883, 116);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Dock = DockStyle.Fill;
            label15.Location = new Point(3, 0);
            label15.Name = "label15";
            label15.Size = new Size(134, 31);
            label15.TabIndex = 0;
            label15.Text = "Path to plink";
            label15.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PageantBrowse
            // 
            PageantBrowse.AutoSize = true;
            PageantBrowse.Dock = DockStyle.Fill;
            PageantBrowse.Location = new Point(805, 65);
            PageantBrowse.Name = "PageantBrowse";
            PageantBrowse.Size = new Size(75, 25);
            PageantBrowse.TabIndex = 8;
            PageantBrowse.Text = "Browse";
            PageantBrowse.UseVisualStyleBackColor = true;
            PageantBrowse.Click += PageantBrowse_Click;
            // 
            // AutostartPageant
            // 
            AutostartPageant.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            AutostartPageant.AutoSize = true;
            AutostartPageant.Checked = true;
            AutostartPageant.CheckState = CheckState.Checked;
            tableLayoutPanel3.SetColumnSpan(AutostartPageant, 2);
            AutostartPageant.Location = new Point(143, 96);
            AutostartPageant.Name = "AutostartPageant";
            AutostartPageant.Size = new Size(737, 17);
            AutostartPageant.TabIndex = 9;
            AutostartPageant.Text = "Automatically start authentication client when a private key is configured for a " +
    "remote";
            AutostartPageant.UseVisualStyleBackColor = true;
            // 
            // PuttygenBrowse
            // 
            PuttygenBrowse.AutoSize = true;
            PuttygenBrowse.Dock = DockStyle.Fill;
            PuttygenBrowse.Location = new Point(805, 34);
            PuttygenBrowse.Name = "PuttygenBrowse";
            PuttygenBrowse.Size = new Size(75, 25);
            PuttygenBrowse.TabIndex = 5;
            PuttygenBrowse.Text = "Browse";
            PuttygenBrowse.UseVisualStyleBackColor = true;
            PuttygenBrowse.Click += PuttygenBrowse_Click;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Dock = DockStyle.Fill;
            label16.Location = new Point(3, 31);
            label16.Name = "label16";
            label16.Size = new Size(134, 31);
            label16.TabIndex = 3;
            label16.Text = "Path to puttygen";
            label16.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PlinkBrowse
            // 
            PlinkBrowse.AutoSize = true;
            PlinkBrowse.Dock = DockStyle.Fill;
            PlinkBrowse.Location = new Point(805, 3);
            PlinkBrowse.Name = "PlinkBrowse";
            PlinkBrowse.Size = new Size(75, 25);
            PlinkBrowse.TabIndex = 2;
            PlinkBrowse.Text = "Browse";
            PlinkBrowse.UseVisualStyleBackColor = true;
            PlinkBrowse.Click += PuttyBrowse_Click;
            // 
            // PageantPath
            // 
            PageantPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            PageantPath.Location = new Point(143, 67);
            PageantPath.MaxLength = 300;
            PageantPath.MinimumSize = new Size(300, 4);
            PageantPath.Name = "PageantPath";
            PageantPath.Size = new Size(656, 21);
            PageantPath.TabIndex = 7;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Dock = DockStyle.Fill;
            label17.Location = new Point(3, 62);
            label17.Name = "label17";
            label17.Size = new Size(134, 31);
            label17.TabIndex = 6;
            label17.Text = "Path to pageant";
            label17.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PlinkPath
            // 
            PlinkPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            PlinkPath.Location = new Point(143, 5);
            PlinkPath.MaxLength = 300;
            PlinkPath.MinimumSize = new Size(300, 4);
            PlinkPath.Name = "PlinkPath";
            PlinkPath.Size = new Size(656, 21);
            PlinkPath.TabIndex = 1;
            // 
            // PuttygenPath
            // 
            PuttygenPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            PuttygenPath.Location = new Point(143, 36);
            PuttygenPath.MaxLength = 300;
            PuttygenPath.MinimumSize = new Size(300, 4);
            PuttygenPath.Name = "PuttygenPath";
            PuttygenPath.Size = new Size(656, 21);
            PuttygenPath.TabIndex = 4;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(tableLayoutPanel2);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(8);
            groupBox1.Size = new Size(899, 108);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Specify which ssh client to use";
            // 
            // groupBox2
            // 
            groupBox2.AutoSize = true;
            groupBox2.Controls.Add(tableLayoutPanel3);
            groupBox2.Dock = DockStyle.Top;
            groupBox2.Location = new Point(0, 108);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(8);
            groupBox2.Size = new Size(899, 146);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Configure PuTTY";
            // 
            // SshSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "SshSettingsPage";
            Size = new Size(899, 633);
            Text = "SSH";
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox groupBox2;
        private CheckBox AutostartPageant;
        private TextBox PageantPath;
        private Button PageantBrowse;
        private Label label17;
        private TextBox PuttygenPath;
        private Button PuttygenBrowse;
        private Label label16;
        private Label label15;
        private TextBox PlinkPath;
        private Button PlinkBrowse;
        private GroupBox groupBox1;
        private TextBox OtherSsh;
        private Button OtherSshBrowse;
        private RadioButton Other;
        private Label label18;
        private RadioButton OpenSSH;
        private RadioButton Putty;
    }
}
