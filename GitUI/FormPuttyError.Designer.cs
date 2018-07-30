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
            this.Cancel = new System.Windows.Forms.Button();
            this.lblPleaseLoadKey = new System.Windows.Forms.Label();
            this.Retry = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblMustAuthenticate
            // 
            this.lblMustAuthenticate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMustAuthenticate.Location = new System.Drawing.Point(65, 47);
            this.lblMustAuthenticate.Name = "lblMustAuthenticate";
            this.lblMustAuthenticate.Size = new System.Drawing.Size(258, 22);
            this.lblMustAuthenticate.TabIndex = 3;
            this.lblMustAuthenticate.Text = "You must authenticate to run this command.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Images.Pageant;
            this.pictureBox1.Location = new System.Drawing.Point(23, 26);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSSHKey.Image = global::GitUI.Properties.Images.Putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(23, 86);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(140, 25);
            this.LoadSSHKey.TabIndex = 4;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(252, 86);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(77, 25);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // lblPleaseLoadKey
            // 
            this.lblPleaseLoadKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPleaseLoadKey.Font = new System.Drawing.Font(this.Font,System.Drawing.FontStyle.Bold);
            this.lblPleaseLoadKey.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblPleaseLoadKey.Location = new System.Drawing.Point(64, 18);
            this.lblPleaseLoadKey.Name = "lblPleaseLoadKey";
            this.lblPleaseLoadKey.Size = new System.Drawing.Size(259, 34);
            this.lblPleaseLoadKey.TabIndex = 2;
            this.lblPleaseLoadKey.Text = "Please load your SSH private key";
            // 
            // Retry
            // 
            this.Retry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Retry.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.Retry.Location = new System.Drawing.Point(169, 86);
            this.Retry.Name = "Retry";
            this.Retry.Size = new System.Drawing.Size(77, 25);
            this.Retry.TabIndex = 0;
            this.Retry.Text = "Retry";
            this.Retry.UseVisualStyleBackColor = true;
            // 
            // FormPuttyError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(353, 125);
            this.Controls.Add(this.Retry);
            this.Controls.Add(this.lblPleaseLoadKey);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.LoadSSHKey);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblMustAuthenticate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPuttyError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Authentication error";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMustAuthenticate;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label lblPleaseLoadKey;
        private System.Windows.Forms.Button Retry;
    }
}