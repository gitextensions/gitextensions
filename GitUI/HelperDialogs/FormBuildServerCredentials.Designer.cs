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
            System.Windows.Forms.Button buttonOK;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Button buttonCancel;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBuildServerCredentials));
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.labelHeader = new System.Windows.Forms.Label();
            this.radioButtonGuestAccess = new System.Windows.Forms.RadioButton();
            this.radioButtonAuthenticatedUser = new System.Windows.Forms.RadioButton();
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
            buttonOK.Location = new System.Drawing.Point(194, 150);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(75, 25);
            buttonOK.TabIndex = 7;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(30, 113);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 13);
            label3.TabIndex = 5;
            label3.Text = "Password:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(30, 87);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(58, 13);
            label2.TabIndex = 3;
            label2.Text = "Username:";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(275, 150);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 25);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(94, 110);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(256, 20);
            this.textBoxPassword.TabIndex = 6;
            this.textBoxPassword.UseSystemPasswordChar = true;
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUserName.Location = new System.Drawing.Point(94, 84);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(256, 20);
            this.textBoxUserName.TabIndex = 4;
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHeader.AutoEllipsis = true;
            this.labelHeader.Location = new System.Drawing.Point(12, 9);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(338, 29);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "Please enter the credentials for the build server at {0}.";
            // 
            // radioButtonGuestAccess
            // 
            this.radioButtonGuestAccess.AutoSize = true;
            this.radioButtonGuestAccess.Location = new System.Drawing.Point(15, 38);
            this.radioButtonGuestAccess.Name = "radioButtonGuestAccess";
            this.radioButtonGuestAccess.Size = new System.Drawing.Size(90, 17);
            this.radioButtonGuestAccess.TabIndex = 1;
            this.radioButtonGuestAccess.TabStop = true;
            this.radioButtonGuestAccess.Text = "Guest access";
            this.radioButtonGuestAccess.UseVisualStyleBackColor = true;
            this.radioButtonGuestAccess.CheckedChanged += new System.EventHandler(this.authenticationMethodChanged);
            // 
            // radioButtonAuthenticatedUser
            // 
            this.radioButtonAuthenticatedUser.AutoSize = true;
            this.radioButtonAuthenticatedUser.Location = new System.Drawing.Point(15, 61);
            this.radioButtonAuthenticatedUser.Name = "radioButtonAuthenticatedUser";
            this.radioButtonAuthenticatedUser.Size = new System.Drawing.Size(114, 17);
            this.radioButtonAuthenticatedUser.TabIndex = 2;
            this.radioButtonAuthenticatedUser.TabStop = true;
            this.radioButtonAuthenticatedUser.Text = "Authenticated user";
            this.radioButtonAuthenticatedUser.UseVisualStyleBackColor = true;
            this.radioButtonAuthenticatedUser.CheckedChanged += new System.EventHandler(this.authenticationMethodChanged);
            // 
            // FormBuildServerCredentials
            // 
            this.AcceptButton = buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = buttonCancel;
            this.ClientSize = new System.Drawing.Size(362, 187);
            this.Controls.Add(this.radioButtonAuthenticatedUser);
            this.Controls.Add(this.radioButtonGuestAccess);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(this.labelHeader);
            this.Controls.Add(buttonCancel);
            this.Controls.Add(buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(378, 226);
            this.Name = "FormBuildServerCredentials";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter credentials";
            this.Load += new System.EventHandler(this.FormBuildServerCredentials_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.RadioButton radioButtonGuestAccess;
        private System.Windows.Forms.RadioButton radioButtonAuthenticatedUser;

    }
}