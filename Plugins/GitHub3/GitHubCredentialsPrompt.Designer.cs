namespace GitHub3
{
    partial class GitHubCredentialsPrompt
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
            this.btn_OK = new System.Windows.Forms.Button();
            this.txtGitHubLogin = new System.Windows.Forms.TextBox();
            this.labelLogin = new System.Windows.Forms.Label();
            this.txtGitHubPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.txtSecondFactor = new System.Windows.Forms.TextBox();
            this.lbl2ndFactor = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.lblError = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_OK.Enabled = false;
            this.btn_OK.Location = new System.Drawing.Point(173, 171);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 5;
            this.btn_OK.Text = "&OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // txtGitHubLogin
            // 
            this.txtGitHubLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGitHubLogin.Location = new System.Drawing.Point(69, 5);
            this.txtGitHubLogin.Margin = new System.Windows.Forms.Padding(5);
            this.txtGitHubLogin.Name = "txtGitHubLogin";
            this.txtGitHubLogin.Size = new System.Drawing.Size(335, 20);
            this.txtGitHubLogin.TabIndex = 0;
            this.txtGitHubLogin.TextChanged += new System.EventHandler(this.TxtGitHubLogin_TextChanged);
            // 
            // labelLogin
            // 
            this.labelLogin.AutoSize = true;
            this.labelLogin.Location = new System.Drawing.Point(3, 7);
            this.labelLogin.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(58, 13);
            this.labelLogin.TabIndex = 2;
            this.labelLogin.Text = "Username:";
            // 
            // txtGitHubPassword
            // 
            this.txtGitHubPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGitHubPassword.Location = new System.Drawing.Point(69, 35);
            this.txtGitHubPassword.Margin = new System.Windows.Forms.Padding(5);
            this.txtGitHubPassword.Name = "txtGitHubPassword";
            this.txtGitHubPassword.PasswordChar = '*';
            this.txtGitHubPassword.Size = new System.Drawing.Size(335, 20);
            this.txtGitHubPassword.TabIndex = 1;
            this.txtGitHubPassword.TextChanged += new System.EventHandler(this.TxtGitHubLogin_TextChanged);
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(3, 37);
            this.labelPassword.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(56, 13);
            this.labelPassword.TabIndex = 2;
            this.labelPassword.Text = "Password:";
            // 
            // txtSecondFactor
            // 
            this.txtSecondFactor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSecondFactor.Location = new System.Drawing.Point(69, 88);
            this.txtSecondFactor.Margin = new System.Windows.Forms.Padding(5);
            this.txtSecondFactor.Name = "txtSecondFactor";
            this.txtSecondFactor.Size = new System.Drawing.Size(335, 20);
            this.txtSecondFactor.TabIndex = 3;
            this.txtSecondFactor.Visible = false;
            this.txtSecondFactor.TextChanged += new System.EventHandler(this.TxtGitHubLogin_TextChanged);
            // 
            // lbl2ndFactor
            // 
            this.lbl2ndFactor.AutoSize = true;
            this.lbl2ndFactor.Location = new System.Drawing.Point(3, 90);
            this.lbl2ndFactor.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.lbl2ndFactor.Name = "lbl2ndFactor";
            this.lbl2ndFactor.Size = new System.Drawing.Size(56, 13);
            this.lbl2ndFactor.TabIndex = 2;
            this.lbl2ndFactor.Text = "2FA code:";
            this.lbl2ndFactor.Visible = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.linkLabel1, 2);
            this.linkLabel1.Location = new System.Drawing.Point(5, 65);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(5);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(243, 13);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Enter a 2nd factor code from an authenticator app";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblError, 2);
            this.lblError.ForeColor = System.Drawing.Color.Red;
            this.lblError.Location = new System.Drawing.Point(3, 116);
            this.lblError.Margin = new System.Windows.Forms.Padding(3);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(29, 13);
            this.lblError.TabIndex = 4;
            this.lblError.Text = "Error";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.labelLogin, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelPassword, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblError, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lbl2ndFactor, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtGitHubLogin, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtGitHubPassword, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtSecondFactor, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(405, 154);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // GitHubCredentialsPrompt
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(409, 206);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(297, 177);
            this.Name = "GitHubCredentialsPrompt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GitHub credentials";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GitHubCredentialsPrompt_FormClosing);
            this.Shown += new System.EventHandler(this.GitHubCredentialsPrompt_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.TextBox txtGitHubLogin;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.TextBox txtGitHubPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox txtSecondFactor;
        private System.Windows.Forms.Label lbl2ndFactor;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}