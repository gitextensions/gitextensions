namespace Github
{
    partial class ConfigureGithub
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureGithub));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._usernameTB = new System.Windows.Forms.TextBox();
            this._apitokenTB = new System.Windows.Forms.TextBox();
            this._saveBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._passwordTB = new System.Windows.Forms.TextBox();
            this._getApiTokenBtn = new System.Windows.Forms.Button();
            this._useSshRB = new System.Windows.Forms.RadioButton();
            this._useHttpsRB = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this._notNeededWithSshLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "User:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "APIToken:";
            // 
            // _usernameTB
            // 
            this._usernameTB.Location = new System.Drawing.Point(76, 9);
            this._usernameTB.Name = "_usernameTB";
            this._usernameTB.Size = new System.Drawing.Size(100, 20);
            this._usernameTB.TabIndex = 0;
            this._usernameTB.TextChanged += new System.EventHandler(this._usernameTB_TextChanged);
            // 
            // _apitokenTB
            // 
            this._apitokenTB.Location = new System.Drawing.Point(76, 59);
            this._apitokenTB.Name = "_apitokenTB";
            this._apitokenTB.Size = new System.Drawing.Size(237, 20);
            this._apitokenTB.TabIndex = 2;
            // 
            // _saveBtn
            // 
            this._saveBtn.Location = new System.Drawing.Point(218, 163);
            this._saveBtn.Name = "_saveBtn";
            this._saveBtn.Size = new System.Drawing.Size(95, 23);
            this._saveBtn.TabIndex = 4;
            this._saveBtn.Text = "Save";
            this._saveBtn.UseVisualStyleBackColor = true;
            this._saveBtn.Click += new System.EventHandler(this._saveBtn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password:";
            // 
            // _passwordTB
            // 
            this._passwordTB.Location = new System.Drawing.Point(76, 33);
            this._passwordTB.Name = "_passwordTB";
            this._passwordTB.Size = new System.Drawing.Size(100, 20);
            this._passwordTB.TabIndex = 1;
            this._passwordTB.UseSystemPasswordChar = true;
            this._passwordTB.TextChanged += new System.EventHandler(this._passwordTB_TextChanged);
            // 
            // _getApiTokenBtn
            // 
            this._getApiTokenBtn.Location = new System.Drawing.Point(221, 85);
            this._getApiTokenBtn.Name = "_getApiTokenBtn";
            this._getApiTokenBtn.Size = new System.Drawing.Size(92, 23);
            this._getApiTokenBtn.TabIndex = 3;
            this._getApiTokenBtn.Text = "Get API Token";
            this._getApiTokenBtn.UseVisualStyleBackColor = true;
            this._getApiTokenBtn.Click += new System.EventHandler(this._getApiTokenBtn_Click);
            // 
            // _useSshRB
            // 
            this._useSshRB.AutoSize = true;
            this._useSshRB.Location = new System.Drawing.Point(23, 127);
            this._useSshRB.Name = "_useSshRB";
            this._useSshRB.Size = new System.Drawing.Size(123, 17);
            this._useSshRB.TabIndex = 5;
            this._useSshRB.TabStop = true;
            this._useSshRB.Text = "SSH (recommended)";
            this._useSshRB.UseVisualStyleBackColor = true;
            // 
            // _useHttpsRB
            // 
            this._useHttpsRB.AutoSize = true;
            this._useHttpsRB.Location = new System.Drawing.Point(23, 145);
            this._useHttpsRB.Name = "_useHttpsRB";
            this._useHttpsRB.Size = new System.Drawing.Size(48, 17);
            this._useHttpsRB.TabIndex = 6;
            this._useHttpsRB.TabStop = true;
            this._useHttpsRB.Text = "https";
            this._useHttpsRB.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Preferred access method:";
            // 
            // _notNeededWithSshLbl
            // 
            this._notNeededWithSshLbl.AutoSize = true;
            this._notNeededWithSshLbl.Location = new System.Drawing.Point(182, 36);
            this._notNeededWithSshLbl.Name = "_notNeededWithSshLbl";
            this._notNeededWithSshLbl.Size = new System.Drawing.Size(116, 13);
            this._notNeededWithSshLbl.TabIndex = 8;
            this._notNeededWithSshLbl.Text = "(Not needed with SSH)";
            // 
            // ConfigureGithub
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 196);
            this.Controls.Add(this._notNeededWithSshLbl);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._useHttpsRB);
            this.Controls.Add(this._useSshRB);
            this.Controls.Add(this._getApiTokenBtn);
            this.Controls.Add(this._passwordTB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._saveBtn);
            this.Controls.Add(this._apitokenTB);
            this.Controls.Add(this._usernameTB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigureGithub";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Github configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _usernameTB;
        private System.Windows.Forms.TextBox _apitokenTB;
        private System.Windows.Forms.Button _saveBtn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _passwordTB;
        private System.Windows.Forms.Button _getApiTokenBtn;
        private System.Windows.Forms.RadioButton _useSshRB;
        private System.Windows.Forms.RadioButton _useHttpsRB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label _notNeededWithSshLbl;
    }
}