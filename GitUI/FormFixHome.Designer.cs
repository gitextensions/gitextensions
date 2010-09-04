namespace GitUI
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
            this.otherHomeBrowse = new System.Windows.Forms.Button();
            this.otherHomeDir = new System.Windows.Forms.TextBox();
            this.otherHome = new System.Windows.Forms.RadioButton();
            this.userprofileHome = new System.Windows.Forms.RadioButton();
            this.defaultHome = new System.Windows.Forms.RadioButton();
            this.label51 = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.groupBox8.SuspendLayout();
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
            this.groupBox8.Location = new System.Drawing.Point(12, 12);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(723, 145);
            this.groupBox8.TabIndex = 11;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Environment";
            // 
            // otherHomeBrowse
            // 
            this.otherHomeBrowse.Location = new System.Drawing.Point(618, 100);
            this.otherHomeBrowse.Name = "otherHomeBrowse";
            this.otherHomeBrowse.Size = new System.Drawing.Size(75, 23);
            this.otherHomeBrowse.TabIndex = 10;
            this.otherHomeBrowse.Text = "Browse";
            this.otherHomeBrowse.UseVisualStyleBackColor = true;
            // 
            // otherHomeDir
            // 
            this.otherHomeDir.Location = new System.Drawing.Point(88, 102);
            this.otherHomeDir.Name = "otherHomeDir";
            this.otherHomeDir.Size = new System.Drawing.Size(527, 20);
            this.otherHomeDir.TabIndex = 4;
            // 
            // otherHome
            // 
            this.otherHome.AutoSize = true;
            this.otherHome.Location = new System.Drawing.Point(11, 103);
            this.otherHome.Name = "otherHome";
            this.otherHome.Size = new System.Drawing.Size(51, 17);
            this.otherHome.TabIndex = 3;
            this.otherHome.TabStop = true;
            this.otherHome.Text = "Other";
            this.otherHome.UseVisualStyleBackColor = true;
            // 
            // userprofileHome
            // 
            this.userprofileHome.AutoSize = true;
            this.userprofileHome.Location = new System.Drawing.Point(11, 80);
            this.userprofileHome.Name = "userprofileHome";
            this.userprofileHome.Size = new System.Drawing.Size(166, 17);
            this.userprofileHome.TabIndex = 2;
            this.userprofileHome.TabStop = true;
            this.userprofileHome.Text = "Set HOME to USERPROFILE";
            this.userprofileHome.UseVisualStyleBackColor = true;
            // 
            // defaultHome
            // 
            this.defaultHome.AutoSize = true;
            this.defaultHome.Location = new System.Drawing.Point(11, 57);
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
            this.label51.Size = new System.Drawing.Size(552, 26);
            this.label51.TabIndex = 0;
            this.label51.Text = resources.GetString("label51.Text");
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(660, 177);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 12;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // FormFixHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 226);
            this.ControlBox = false;
            this.Controls.Add(this.ok);
            this.Controls.Add(this.groupBox8);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormFixHome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Home";
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button otherHomeBrowse;
        private System.Windows.Forms.TextBox otherHomeDir;
        private System.Windows.Forms.RadioButton otherHome;
        private System.Windows.Forms.RadioButton userprofileHome;
        private System.Windows.Forms.RadioButton defaultHome;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.Button ok;

    }
}