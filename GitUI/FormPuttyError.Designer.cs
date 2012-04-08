namespace GitUI
{
    partial class FormPuttyError
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
            this.lblMustAuthenticate = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.lblPleaseLoadKey = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblMustAuthenticate
            // 
            this.lblMustAuthenticate.AutoSize = true;
            this.lblMustAuthenticate.Location = new System.Drawing.Point(65, 45);
            this.lblMustAuthenticate.Name = "lblMustAuthenticate";
            this.lblMustAuthenticate.Size = new System.Drawing.Size(245, 15);
            this.lblMustAuthenticate.TabIndex = 0;
            this.lblMustAuthenticate.Text = "You must authenticate to run this command.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Resources.pageant;
            this.pictureBox1.Location = new System.Drawing.Point(23, 26);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(23, 86);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(140, 25);
            this.LoadSSHKey.TabIndex = 26;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // Abort
            // 
            this.Abort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Abort.Location = new System.Drawing.Point(246, 86);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(77, 25);
            this.Abort.TabIndex = 28;
            this.Abort.Text = "Cancel";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // lblPleaseLoadKey
            // 
            this.lblPleaseLoadKey.AutoSize = true;
            this.lblPleaseLoadKey.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPleaseLoadKey.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblPleaseLoadKey.Location = new System.Drawing.Point(64, 22);
            this.lblPleaseLoadKey.Name = "lblPleaseLoadKey";
            this.lblPleaseLoadKey.Size = new System.Drawing.Size(259, 21);
            this.lblPleaseLoadKey.TabIndex = 29;
            this.lblPleaseLoadKey.Text = "Please load your SSH private key";
            // 
            // FormPuttyError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(353, 125);
            this.Controls.Add(this.lblPleaseLoadKey);
            this.Controls.Add(this.Abort);
            this.Controls.Add(this.LoadSSHKey);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblMustAuthenticate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPuttyError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Authentication error";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMustAuthenticate;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Label lblPleaseLoadKey;
    }
}