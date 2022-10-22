namespace GitUI.HelperDialogs
{
    partial class FormBuildServerCredentials
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
            System.Windows.Forms.Button buttonOK;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Button buttonCancel;
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxBearerToken = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.labelHeader = new System.Windows.Forms.Label();
            this.radioButtonGuestAccess = new System.Windows.Forms.RadioButton();
            this.radioButtonAuthenticatedUser = new System.Windows.Forms.RadioButton();
            this.radioButtonBearerToken = new System.Windows.Forms.RadioButton();
            buttonOK = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(412, 213);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(75, 25);
            buttonOK.TabIndex = 12;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 180);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Bearer token:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(28, 151);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(60, 15);
            label3.TabIndex = 7;
            label3.Text = "Password:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(25, 125);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(63, 15);
            label2.TabIndex = 5;
            label2.Text = "Username:";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(493, 213);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 25);
            buttonCancel.TabIndex = 13;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBoxBearerToken
            // 
            this.textBoxBearerToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBearerToken.Location = new System.Drawing.Point(94, 177);
            this.textBoxBearerToken.Name = "textBoxBearerToken";
            this.textBoxBearerToken.Size = new System.Drawing.Size(474, 23);
            this.textBoxBearerToken.TabIndex = 10;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(94, 148);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(474, 23);
            this.textBoxPassword.TabIndex = 8;
            this.textBoxPassword.UseSystemPasswordChar = true;
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUserName.Location = new System.Drawing.Point(94, 122);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(474, 23);
            this.textBoxUserName.TabIndex = 6;
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHeader.AutoEllipsis = true;
            this.labelHeader.Location = new System.Drawing.Point(12, 9);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(556, 29);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "Please enter the credentials for the build server at {0}.";
            // 
            // radioButtonGuestAccess
            // 
            this.radioButtonGuestAccess.AutoSize = true;
            this.radioButtonGuestAccess.Location = new System.Drawing.Point(15, 38);
            this.radioButtonGuestAccess.Name = "radioButtonGuestAccess";
            this.radioButtonGuestAccess.Size = new System.Drawing.Size(92, 19);
            this.radioButtonGuestAccess.TabIndex = 1;
            this.radioButtonGuestAccess.TabStop = true;
            this.radioButtonGuestAccess.Text = "Guest access";
            this.radioButtonGuestAccess.UseVisualStyleBackColor = true;
            this.radioButtonGuestAccess.CheckedChanged += new System.EventHandler(this.authenticationMethodChanged);
            // 
            // radioButtonAuthenticatedUser
            // 
            this.radioButtonAuthenticatedUser.AutoSize = true;
            this.radioButtonAuthenticatedUser.Location = new System.Drawing.Point(15, 63);
            this.radioButtonAuthenticatedUser.Name = "radioButtonAuthenticatedUser";
            this.radioButtonAuthenticatedUser.Size = new System.Drawing.Size(125, 19);
            this.radioButtonAuthenticatedUser.TabIndex = 2;
            this.radioButtonAuthenticatedUser.TabStop = true;
            this.radioButtonAuthenticatedUser.Text = "Authenticated user";
            this.radioButtonAuthenticatedUser.UseVisualStyleBackColor = true;
            this.radioButtonAuthenticatedUser.CheckedChanged += new System.EventHandler(this.authenticationMethodChanged);
            // 
            // radioButtonBearerToken
            // 
            this.radioButtonBearerToken.AutoSize = true;
            this.radioButtonBearerToken.Location = new System.Drawing.Point(15, 88);
            this.radioButtonBearerToken.Name = "radioButtonBearerToken";
            this.radioButtonBearerToken.Size = new System.Drawing.Size(104, 19);
            this.radioButtonBearerToken.TabIndex = 3;
            this.radioButtonBearerToken.TabStop = true;
            this.radioButtonBearerToken.Text = "Bearer token";
            this.radioButtonBearerToken.UseVisualStyleBackColor = true;
            this.radioButtonBearerToken.CheckedChanged += new System.EventHandler(this.authenticationMethodChanged);
            // 
            // FormBuildServerCredentials
            // 
            this.AcceptButton = buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = buttonCancel;
            this.ClientSize = new System.Drawing.Size(580, 251);
            this.Controls.Add(this.radioButtonBearerToken);
            this.Controls.Add(this.radioButtonAuthenticatedUser);
            this.Controls.Add(this.radioButtonGuestAccess);
            this.Controls.Add(this.textBoxBearerToken);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.label4);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(this.labelHeader);
            this.Controls.Add(buttonCancel);
            this.Controls.Add(buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(373, 210);
            this.Name = "FormBuildServerCredentials";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter credentials";
            this.Load += new System.EventHandler(this.FormBuildServerCredentials_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxBearerToken;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.RadioButton radioButtonGuestAccess;
        private System.Windows.Forms.RadioButton radioButtonAuthenticatedUser;
		private System.Windows.Forms.RadioButton radioButtonBearerToken;
        private System.Windows.Forms.Label label4;
    }
}
