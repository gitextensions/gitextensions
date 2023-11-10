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
            if (disposing && (components is not null))
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
            lblMustAuthenticate = new Label();
            pictureBox1 = new PictureBox();
            LoadSSHKey = new Button();
            Cancel = new Button();
            lblPleaseLoadKey = new Label();
            Retry = new Button();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            SuspendLayout();
            // 
            // lblMustAuthenticate
            // 
            lblMustAuthenticate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblMustAuthenticate.Location = new Point(65, 47);
            lblMustAuthenticate.Name = "lblMustAuthenticate";
            lblMustAuthenticate.AutoSize = true;
            lblMustAuthenticate.TabIndex = 3;
            lblMustAuthenticate.Text = "You must authenticate to run this command.";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Images.Pageant;
            pictureBox1.Location = new Point(23, 26);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(32, 32);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // LoadSSHKey
            // 
            LoadSSHKey.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LoadSSHKey.Image = Properties.Images.Putty;
            LoadSSHKey.ImageAlign = ContentAlignment.MiddleLeft;
            LoadSSHKey.Location = new Point(23, 86);
            LoadSSHKey.Name = "LoadSSHKey";
            LoadSSHKey.Size = new Size(140, 25);
            LoadSSHKey.TabIndex = 4;
            LoadSSHKey.Text = "Load SSH key";
            LoadSSHKey.UseVisualStyleBackColor = true;
            LoadSSHKey.Click += LoadSSHKey_Click;
            // 
            // Cancel
            // 
            Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(252, 86);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(77, 25);
            Cancel.TabIndex = 1;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            // 
            // lblPleaseLoadKey
            // 
            lblPleaseLoadKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPleaseLoadKey.Font = new Font(Font,FontStyle.Bold);
            lblPleaseLoadKey.ForeColor = SystemColors.ControlDarkDark;
            lblPleaseLoadKey.Location = new Point(64, 18);
            lblPleaseLoadKey.Name = "lblPleaseLoadKey";
            lblPleaseLoadKey.AutoSize = true;
            lblPleaseLoadKey.TabIndex = 2;
            lblPleaseLoadKey.Text = "Please load your SSH private key";
            // 
            // Retry
            // 
            Retry.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Retry.DialogResult = DialogResult.Retry;
            Retry.Location = new Point(169, 86);
            Retry.Name = "Retry";
            Retry.Size = new Size(77, 25);
            Retry.TabIndex = 0;
            Retry.Text = "Retry";
            Retry.UseVisualStyleBackColor = true;
            // 
            // FormPuttyError
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(353, 125);
            Controls.Add(Retry);
            Controls.Add(lblPleaseLoadKey);
            Controls.Add(Cancel);
            Controls.Add(LoadSSHKey);
            Controls.Add(pictureBox1);
            Controls.Add(lblMustAuthenticate);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormPuttyError";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Authentication error";
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label lblMustAuthenticate;
        private PictureBox pictureBox1;
        private Button LoadSSHKey;
        private Button Cancel;
        private Label lblPleaseLoadKey;
        private Button Retry;
    }
}
