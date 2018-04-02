namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class FormFixHome
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFixHome));
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.otherHomeBrowse = new GitUI.UserControls.FolderBrowserButton();
            this.otherHomeDir = new System.Windows.Forms.TextBox();
            this.otherHome = new System.Windows.Forms.RadioButton();
            this.userprofileHome = new System.Windows.Forms.RadioButton();
            this.defaultHome = new System.Windows.Forms.RadioButton();
            this.label51 = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox8.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.otherHomeBrowse);
            this.groupBox8.Controls.Add(this.otherHomeDir);
            this.groupBox8.Controls.Add(this.otherHome);
            this.groupBox8.Controls.Add(this.userprofileHome);
            this.groupBox8.Controls.Add(this.defaultHome);
            this.groupBox8.Controls.Add(this.label51);
            this.groupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox8.Location = new System.Drawing.Point(8, 8);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(588, 154);
            this.groupBox8.TabIndex = 0;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Environment";
            // 
            // otherHomeBrowse
            // 
            this.otherHomeBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.otherHomeBrowse.Location = new System.Drawing.Point(444, 114);
            this.otherHomeBrowse.Name = "otherHomeBrowse";
            this.otherHomeBrowse.PathShowingControl = this.otherHomeDir;
            this.otherHomeBrowse.Size = new System.Drawing.Size(130, 25);
            this.otherHomeBrowse.TabIndex = 5;
            // 
            // otherHomeDir
            // 
            this.otherHomeDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.otherHomeDir.Location = new System.Drawing.Point(141, 115);
            this.otherHomeDir.MaxLength = 250;
            this.otherHomeDir.Name = "otherHomeDir";
            this.otherHomeDir.Size = new System.Drawing.Size(292, 21);
            this.otherHomeDir.TabIndex = 4;
            // 
            // otherHome
            // 
            this.otherHome.AutoSize = true;
            this.otherHome.Location = new System.Drawing.Point(11, 118);
            this.otherHome.Name = "otherHome";
            this.otherHome.Size = new System.Drawing.Size(53, 17);
            this.otherHome.TabIndex = 3;
            this.otherHome.TabStop = true;
            this.otherHome.Text = "Other";
            this.otherHome.UseVisualStyleBackColor = true;
            // 
            // userprofileHome
            // 
            this.userprofileHome.AutoSize = true;
            this.userprofileHome.Location = new System.Drawing.Point(11, 95);
            this.userprofileHome.Name = "userprofileHome";
            this.userprofileHome.Size = new System.Drawing.Size(157, 17);
            this.userprofileHome.TabIndex = 2;
            this.userprofileHome.TabStop = true;
            this.userprofileHome.Text = "Set HOME to USERPROFILE";
            this.userprofileHome.UseVisualStyleBackColor = true;
            // 
            // defaultHome
            // 
            this.defaultHome.AutoSize = true;
            this.defaultHome.Location = new System.Drawing.Point(11, 72);
            this.defaultHome.Name = "defaultHome";
            this.defaultHome.Size = new System.Drawing.Size(129, 17);
            this.defaultHome.TabIndex = 1;
            this.defaultHome.TabStop = true;
            this.defaultHome.Text = "Use default for HOME";
            this.defaultHome.UseVisualStyleBackColor = true;
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(8, 19);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(412, 39);
            this.label51.TabIndex = 0;
            this.label51.Text = resources.GetString("label51.Text");
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(455, 3);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(130, 25);
            this.ok.TabIndex = 0;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.ok);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(8, 162);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(588, 31);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // FormFixHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(604, 201);
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 240);
            this.Name = "FormFixHome";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Home";
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox8;
        private UserControls.FolderBrowserButton otherHomeBrowse;
        private System.Windows.Forms.TextBox otherHomeDir;
        private System.Windows.Forms.RadioButton otherHome;
        private System.Windows.Forms.RadioButton userprofileHome;
        private System.Windows.Forms.RadioButton defaultHome;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}