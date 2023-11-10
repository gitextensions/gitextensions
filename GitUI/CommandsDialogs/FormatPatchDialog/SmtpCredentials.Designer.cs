namespace GitUI.CommandsDialogs.FormatPatchDialog
{
    partial class SmtpCredentials
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
            label1 = new Label();
            label2 = new Label();
            login = new TextBox();
            password = new TextBox();
            Ok = new Button();
            Cancel = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 13);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(33, 13);
            label1.TabIndex = 0;
            label1.Text = "Login";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 36);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(53, 13);
            label2.TabIndex = 1;
            label2.Text = "Password";
            // 
            // login
            // 
            login.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            login.Location = new Point(78, 11);
            login.Margin = new Padding(2, 2, 2, 2);
            login.Name = "login";
            login.Size = new Size(155, 20);
            login.TabIndex = 2;
            // 
            // password
            // 
            password.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            password.Location = new Point(78, 33);
            password.Margin = new Padding(2, 2, 2, 2);
            password.Name = "password";
            password.PasswordChar = '*';
            password.Size = new Size(155, 20);
            password.TabIndex = 3;
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(85, 56);
            Ok.Margin = new Padding(2, 2, 2, 2);
            Ok.Name = "Ok";
            Ok.Size = new Size(69, 21);
            Ok.TabIndex = 4;
            Ok.Text = "Ok";
            Ok.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(158, 56);
            Cancel.Margin = new Padding(2, 2, 2, 2);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(74, 21);
            Cancel.TabIndex = 5;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            // 
            // SmtpCredentials
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = Cancel;
            ClientSize = new Size(241, 84);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(password);
            Controls.Add(login);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(2, 2, 2, 2);
            Name = "SmtpCredentials";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Credentials";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        public TextBox login;
        public TextBox password;
        private Button Ok;
        private Button Cancel;
    }
}