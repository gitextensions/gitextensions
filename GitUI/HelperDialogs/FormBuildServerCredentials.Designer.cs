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
            Button buttonOK;
            Label label3;
            Label label2;
            Button buttonCancel;
            label4 = new Label();
            textBoxBearerToken = new TextBox();
            textBoxPassword = new TextBox();
            textBoxUserName = new TextBox();
            labelHeader = new Label();
            radioButtonGuestAccess = new RadioButton();
            radioButtonAuthenticatedUser = new RadioButton();
            radioButtonBearerToken = new RadioButton();
            buttonOK = new Button();
            label3 = new Label();
            label2 = new Label();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(412, 213);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 25);
            buttonOK.TabIndex = 12;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 180);
            label4.Name = "label4";
            label4.Size = new Size(76, 15);
            label4.TabIndex = 9;
            label4.Text = "Bearer token:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(28, 151);
            label3.Name = "label3";
            label3.Size = new Size(60, 15);
            label3.TabIndex = 7;
            label3.Text = "Password:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 125);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 5;
            label2.Text = "Username:";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(493, 213);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 25);
            buttonCancel.TabIndex = 13;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonOK_Click;
            // 
            // textBoxBearerToken
            // 
            textBoxBearerToken.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxBearerToken.Location = new Point(94, 177);
            textBoxBearerToken.Name = "textBoxBearerToken";
            textBoxBearerToken.Size = new Size(474, 23);
            textBoxBearerToken.TabIndex = 10;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxPassword.Location = new Point(94, 148);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(474, 23);
            textBoxPassword.TabIndex = 8;
            textBoxPassword.UseSystemPasswordChar = true;
            // 
            // textBoxUserName
            // 
            textBoxUserName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxUserName.Location = new Point(94, 122);
            textBoxUserName.Name = "textBoxUserName";
            textBoxUserName.Size = new Size(474, 23);
            textBoxUserName.TabIndex = 6;
            // 
            // labelHeader
            // 
            labelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelHeader.AutoEllipsis = true;
            labelHeader.Location = new Point(12, 9);
            labelHeader.Name = "labelHeader";
            labelHeader.Size = new Size(556, 29);
            labelHeader.TabIndex = 0;
            labelHeader.Text = "Please enter the credentials for the build server at {0}.";
            // 
            // radioButtonGuestAccess
            // 
            radioButtonGuestAccess.AutoSize = true;
            radioButtonGuestAccess.Location = new Point(15, 38);
            radioButtonGuestAccess.Name = "radioButtonGuestAccess";
            radioButtonGuestAccess.Size = new Size(92, 19);
            radioButtonGuestAccess.TabIndex = 1;
            radioButtonGuestAccess.TabStop = true;
            radioButtonGuestAccess.Text = "Guest access";
            radioButtonGuestAccess.UseVisualStyleBackColor = true;
            radioButtonGuestAccess.CheckedChanged += authenticationMethodChanged;
            // 
            // radioButtonAuthenticatedUser
            // 
            radioButtonAuthenticatedUser.AutoSize = true;
            radioButtonAuthenticatedUser.Location = new Point(15, 63);
            radioButtonAuthenticatedUser.Name = "radioButtonAuthenticatedUser";
            radioButtonAuthenticatedUser.Size = new Size(125, 19);
            radioButtonAuthenticatedUser.TabIndex = 2;
            radioButtonAuthenticatedUser.TabStop = true;
            radioButtonAuthenticatedUser.Text = "Authenticated user";
            radioButtonAuthenticatedUser.UseVisualStyleBackColor = true;
            radioButtonAuthenticatedUser.CheckedChanged += authenticationMethodChanged;
            // 
            // radioButtonBearerToken
            // 
            radioButtonBearerToken.AutoSize = true;
            radioButtonBearerToken.Location = new Point(15, 88);
            radioButtonBearerToken.Name = "radioButtonBearerToken";
            radioButtonBearerToken.Size = new Size(104, 19);
            radioButtonBearerToken.TabIndex = 3;
            radioButtonBearerToken.TabStop = true;
            radioButtonBearerToken.Text = "Bearer token";
            radioButtonBearerToken.UseVisualStyleBackColor = true;
            radioButtonBearerToken.CheckedChanged += authenticationMethodChanged;
            // 
            // FormBuildServerCredentials
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = buttonCancel;
            ClientSize = new Size(580, 251);
            Controls.Add(radioButtonBearerToken);
            Controls.Add(radioButtonAuthenticatedUser);
            Controls.Add(radioButtonGuestAccess);
            Controls.Add(textBoxBearerToken);
            Controls.Add(textBoxPassword);
            Controls.Add(textBoxUserName);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(labelHeader);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(373, 210);
            Name = "FormBuildServerCredentials";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Enter credentials";
            Load += FormBuildServerCredentials_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox textBoxBearerToken;
        private TextBox textBoxPassword;
        private TextBox textBoxUserName;
        private Label labelHeader;
        private RadioButton radioButtonGuestAccess;
        private RadioButton radioButtonAuthenticatedUser;
		private RadioButton radioButtonBearerToken;
        private Label label4;
    }
}
