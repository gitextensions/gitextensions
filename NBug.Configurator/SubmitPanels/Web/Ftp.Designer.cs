namespace NBug.Configurator.SubmitPanels.Web
{
	partial class Ftp
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.urlLabel = new System.Windows.Forms.Label();
			this.urlTextBox = new System.Windows.Forms.TextBox();
			this.useSslCheckBox = new System.Windows.Forms.CheckBox();
			this.authenticationGroupBox = new System.Windows.Forms.GroupBox();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.usernameTextBox = new System.Windows.Forms.TextBox();
			this.passwordLabel = new System.Windows.Forms.Label();
			this.usernameLabel = new System.Windows.Forms.Label();
			this.usernamePasswordRadioButton = new System.Windows.Forms.RadioButton();
			this.RadioButton = new System.Windows.Forms.RadioButton();
			this.noteLabel = new System.Windows.Forms.Label();
			this.ftpServerGroupBox = new System.Windows.Forms.GroupBox();
			this.authenticationGroupBox.SuspendLayout();
			this.ftpServerGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// urlLabel
			// 
			this.urlLabel.AutoSize = true;
			this.urlLabel.Location = new System.Drawing.Point(6, 23);
			this.urlLabel.Name = "urlLabel";
			this.urlLabel.Size = new System.Drawing.Size(27, 13);
			this.urlLabel.TabIndex = 0;
			this.urlLabel.Text = "Url*:";
			// 
			// urlTextBox
			// 
			this.urlTextBox.Location = new System.Drawing.Point(39, 20);
			this.urlTextBox.Name = "urlTextBox";
			this.urlTextBox.Size = new System.Drawing.Size(262, 20);
			this.urlTextBox.TabIndex = 1;
			// 
			// useSslCheckBox
			// 
			this.useSslCheckBox.AutoSize = true;
			this.useSslCheckBox.Location = new System.Drawing.Point(39, 48);
			this.useSslCheckBox.Name = "useSslCheckBox";
			this.useSslCheckBox.Size = new System.Drawing.Size(68, 17);
			this.useSslCheckBox.TabIndex = 2;
			this.useSslCheckBox.Text = "Use SSL";
			this.useSslCheckBox.UseVisualStyleBackColor = true;
			// 
			// authenticationGroupBox
			// 
			this.authenticationGroupBox.Controls.Add(this.passwordTextBox);
			this.authenticationGroupBox.Controls.Add(this.usernameTextBox);
			this.authenticationGroupBox.Controls.Add(this.passwordLabel);
			this.authenticationGroupBox.Controls.Add(this.usernameLabel);
			this.authenticationGroupBox.Controls.Add(this.usernamePasswordRadioButton);
			this.authenticationGroupBox.Controls.Add(this.RadioButton);
			this.authenticationGroupBox.Location = new System.Drawing.Point(343, 3);
			this.authenticationGroupBox.Name = "authenticationGroupBox";
			this.authenticationGroupBox.Size = new System.Drawing.Size(235, 110);
			this.authenticationGroupBox.TabIndex = 3;
			this.authenticationGroupBox.TabStop = false;
			this.authenticationGroupBox.Text = "Authentication";
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Enabled = false;
			this.passwordTextBox.Location = new System.Drawing.Point(70, 72);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.Size = new System.Drawing.Size(146, 20);
			this.passwordTextBox.TabIndex = 5;
			// 
			// usernameTextBox
			// 
			this.usernameTextBox.Enabled = false;
			this.usernameTextBox.Location = new System.Drawing.Point(70, 46);
			this.usernameTextBox.Name = "usernameTextBox";
			this.usernameTextBox.Size = new System.Drawing.Size(146, 20);
			this.usernameTextBox.TabIndex = 4;
			// 
			// passwordLabel
			// 
			this.passwordLabel.AutoSize = true;
			this.passwordLabel.Location = new System.Drawing.Point(6, 75);
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Size = new System.Drawing.Size(56, 13);
			this.passwordLabel.TabIndex = 3;
			this.passwordLabel.Text = "Password:";
			// 
			// usernameLabel
			// 
			this.usernameLabel.AutoSize = true;
			this.usernameLabel.Location = new System.Drawing.Point(6, 49);
			this.usernameLabel.Name = "usernameLabel";
			this.usernameLabel.Size = new System.Drawing.Size(58, 13);
			this.usernameLabel.TabIndex = 2;
			this.usernameLabel.Text = "Username:";
			// 
			// usernamePasswordRadioButton
			// 
			this.usernamePasswordRadioButton.AutoSize = true;
			this.usernamePasswordRadioButton.Location = new System.Drawing.Point(92, 19);
			this.usernamePasswordRadioButton.Name = "usernamePasswordRadioButton";
			this.usernamePasswordRadioButton.Size = new System.Drawing.Size(124, 17);
			this.usernamePasswordRadioButton.TabIndex = 1;
			this.usernamePasswordRadioButton.Text = "Username/Password";
			this.usernamePasswordRadioButton.UseVisualStyleBackColor = true;
			this.usernamePasswordRadioButton.CheckedChanged += new System.EventHandler(this.UsernamePasswordRadioButton_CheckedChanged);
			// 
			// RadioButton
			// 
			this.RadioButton.AutoSize = true;
			this.RadioButton.Checked = true;
			this.RadioButton.Location = new System.Drawing.Point(6, 19);
			this.RadioButton.Name = "RadioButton";
			this.RadioButton.Size = new System.Drawing.Size(80, 17);
			this.RadioButton.TabIndex = 0;
			this.RadioButton.TabStop = true;
			this.RadioButton.Text = "Anonymous";
			this.RadioButton.UseVisualStyleBackColor = true;
			// 
			// noteLabel
			// 
			this.noteLabel.AutoSize = true;
			this.noteLabel.Location = new System.Drawing.Point(9, 100);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(162, 13);
			this.noteLabel.TabIndex = 42;
			this.noteLabel.Text = "Note: * Denotes mendatory fields";
			// 
			// ftpServerGroupBox
			// 
			this.ftpServerGroupBox.Controls.Add(this.urlLabel);
			this.ftpServerGroupBox.Controls.Add(this.urlTextBox);
			this.ftpServerGroupBox.Controls.Add(this.useSslCheckBox);
			this.ftpServerGroupBox.Location = new System.Drawing.Point(3, 3);
			this.ftpServerGroupBox.Name = "ftpServerGroupBox";
			this.ftpServerGroupBox.Size = new System.Drawing.Size(316, 72);
			this.ftpServerGroupBox.TabIndex = 43;
			this.ftpServerGroupBox.TabStop = false;
			this.ftpServerGroupBox.Text = "FTP Server";
			// 
			// Ftp
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ftpServerGroupBox);
			this.Controls.Add(this.noteLabel);
			this.Controls.Add(this.authenticationGroupBox);
			this.Name = "Ftp";
			this.Size = new System.Drawing.Size(660, 158);
			this.authenticationGroupBox.ResumeLayout(false);
			this.authenticationGroupBox.PerformLayout();
			this.ftpServerGroupBox.ResumeLayout(false);
			this.ftpServerGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label urlLabel;
		private System.Windows.Forms.TextBox urlTextBox;
		private System.Windows.Forms.CheckBox useSslCheckBox;
		private System.Windows.Forms.GroupBox authenticationGroupBox;
		private System.Windows.Forms.TextBox passwordTextBox;
		private System.Windows.Forms.TextBox usernameTextBox;
		private System.Windows.Forms.Label passwordLabel;
		private System.Windows.Forms.Label usernameLabel;
		private System.Windows.Forms.RadioButton usernamePasswordRadioButton;
		private System.Windows.Forms.RadioButton RadioButton;
		private System.Windows.Forms.Label noteLabel;
		private System.Windows.Forms.GroupBox ftpServerGroupBox;
	}
}
