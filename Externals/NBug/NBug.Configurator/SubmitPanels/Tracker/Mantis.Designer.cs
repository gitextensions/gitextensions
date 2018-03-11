namespace NBug.Configurator.SubmitPanels.Tracker
{
	partial class Mantis
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
            this.summaryTextBox = new System.Windows.Forms.TextBox();
            this.noteLabel = new System.Windows.Forms.Label();
            this.projectTrackerGroupBox = new System.Windows.Forms.GroupBox();
            this.uploadFailCheckBox = new System.Windows.Forms.CheckBox();
            this.uploadCheckbox = new System.Windows.Forms.CheckBox();
            this.trackerURLTextBox = new System.Windows.Forms.TextBox();
            this.trackerURLLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.categoryTextBox = new System.Windows.Forms.TextBox();
            this.categoryLabel = new System.Windows.Forms.Label();
            this.projectIDTextBox = new System.Windows.Forms.TextBox();
            this.projectIDLabel = new System.Windows.Forms.Label();
            this.customSubjectLabel = new System.Windows.Forms.Label();
            this.versionAddCheckBox = new System.Windows.Forms.CheckBox();
            this.projectTrackerGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // summaryTextBox
            // 
            this.summaryTextBox.Location = new System.Drawing.Point(3, 176);
            this.summaryTextBox.Multiline = true;
            this.summaryTextBox.Name = "summaryTextBox";
            this.summaryTextBox.Size = new System.Drawing.Size(351, 37);
            this.summaryTextBox.TabIndex = 0;
            // 
            // noteLabel
            // 
            this.noteLabel.AutoSize = true;
            this.noteLabel.Location = new System.Drawing.Point(3, 223);
            this.noteLabel.Name = "noteLabel";
            this.noteLabel.Size = new System.Drawing.Size(162, 13);
            this.noteLabel.TabIndex = 41;
            this.noteLabel.Text = "Note: * Denotes mendatory fields";
            // 
            // projectTrackerGroupBox
            // 
            this.projectTrackerGroupBox.Controls.Add(this.versionAddCheckBox);
            this.projectTrackerGroupBox.Controls.Add(this.uploadFailCheckBox);
            this.projectTrackerGroupBox.Controls.Add(this.uploadCheckbox);
            this.projectTrackerGroupBox.Controls.Add(this.trackerURLTextBox);
            this.projectTrackerGroupBox.Controls.Add(this.trackerURLLabel);
            this.projectTrackerGroupBox.Controls.Add(this.passwordTextBox);
            this.projectTrackerGroupBox.Controls.Add(this.passwordLabel);
            this.projectTrackerGroupBox.Controls.Add(this.usernameTextBox);
            this.projectTrackerGroupBox.Controls.Add(this.usernameLabel);
            this.projectTrackerGroupBox.Controls.Add(this.categoryTextBox);
            this.projectTrackerGroupBox.Controls.Add(this.categoryLabel);
            this.projectTrackerGroupBox.Controls.Add(this.projectIDTextBox);
            this.projectTrackerGroupBox.Controls.Add(this.projectIDLabel);
            this.projectTrackerGroupBox.Location = new System.Drawing.Point(3, 3);
            this.projectTrackerGroupBox.Name = "projectTrackerGroupBox";
            this.projectTrackerGroupBox.Size = new System.Drawing.Size(490, 154);
            this.projectTrackerGroupBox.TabIndex = 43;
            this.projectTrackerGroupBox.TabStop = false;
            this.projectTrackerGroupBox.Text = "Project Tracker";
            // 
            // uploadFailCheckBox
            // 
            this.uploadFailCheckBox.AutoSize = true;
            this.uploadFailCheckBox.Location = new System.Drawing.Point(9, 132);
            this.uploadFailCheckBox.Name = "uploadFailCheckBox";
            this.uploadFailCheckBox.Size = new System.Drawing.Size(203, 17);
            this.uploadFailCheckBox.TabIndex = 7;
            this.uploadFailCheckBox.Text = "Treat action as success if upload fails";
            this.uploadFailCheckBox.UseVisualStyleBackColor = true;
            // 
            // uploadCheckbox
            // 
            this.uploadCheckbox.AutoSize = true;
            this.uploadCheckbox.Location = new System.Drawing.Point(9, 104);
            this.uploadCheckbox.Name = "uploadCheckbox";
            this.uploadCheckbox.Size = new System.Drawing.Size(116, 17);
            this.uploadCheckbox.TabIndex = 5;
            this.uploadCheckbox.Text = "Upload attachment";
            this.uploadCheckbox.UseVisualStyleBackColor = true;
            this.uploadCheckbox.CheckedChanged += new System.EventHandler(this.uploadCheckbox_CheckedChanged);
            // 
            // trackerURLTextBox
            // 
            this.trackerURLTextBox.Location = new System.Drawing.Point(88, 19);
            this.trackerURLTextBox.Name = "trackerURLTextBox";
            this.trackerURLTextBox.Size = new System.Drawing.Size(332, 20);
            this.trackerURLTextBox.TabIndex = 0;
            this.trackerURLTextBox.Text = "http://www.yourdomain.com/mantis/api/soap/mantisconnect.php";
            // 
            // trackerURLLabel
            // 
            this.trackerURLLabel.AutoSize = true;
            this.trackerURLLabel.Location = new System.Drawing.Point(6, 22);
            this.trackerURLLabel.Name = "trackerURLLabel";
            this.trackerURLLabel.Size = new System.Drawing.Size(76, 13);
            this.trackerURLLabel.TabIndex = 6;
            this.trackerURLLabel.Text = "Tracker URL*:";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(312, 48);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(108, 20);
            this.passwordTextBox.TabIndex = 2;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(230, 51);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(60, 13);
            this.passwordLabel.TabIndex = 4;
            this.passwordLabel.Text = "Password*:";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(88, 45);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(108, 20);
            this.usernameTextBox.TabIndex = 1;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(6, 48);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(62, 13);
            this.usernameLabel.TabIndex = 4;
            this.usernameLabel.Text = "Username*:";
            // 
            // categoryTextBox
            // 
            this.categoryTextBox.Location = new System.Drawing.Point(312, 74);
            this.categoryTextBox.Name = "categoryTextBox";
            this.categoryTextBox.Size = new System.Drawing.Size(108, 20);
            this.categoryTextBox.TabIndex = 4;
            this.categoryTextBox.Text = "General";
            // 
            // categoryLabel
            // 
            this.categoryLabel.AutoSize = true;
            this.categoryLabel.Location = new System.Drawing.Point(230, 77);
            this.categoryLabel.Name = "categoryLabel";
            this.categoryLabel.Size = new System.Drawing.Size(56, 13);
            this.categoryLabel.TabIndex = 4;
            this.categoryLabel.Text = "Category*:";
            // 
            // projectIDTextBox
            // 
            this.projectIDTextBox.Location = new System.Drawing.Point(88, 71);
            this.projectIDTextBox.Name = "projectIDTextBox";
            this.projectIDTextBox.Size = new System.Drawing.Size(48, 20);
            this.projectIDTextBox.TabIndex = 3;
            this.projectIDTextBox.Text = "0";
            this.projectIDTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // projectIDLabel
            // 
            this.projectIDLabel.AutoSize = true;
            this.projectIDLabel.Location = new System.Drawing.Point(6, 74);
            this.projectIDLabel.Name = "projectIDLabel";
            this.projectIDLabel.Size = new System.Drawing.Size(61, 13);
            this.projectIDLabel.TabIndex = 4;
            this.projectIDLabel.Text = "Project ID*:";
            // 
            // customSubjectLabel
            // 
            this.customSubjectLabel.AutoSize = true;
            this.customSubjectLabel.Location = new System.Drawing.Point(4, 161);
            this.customSubjectLabel.Name = "customSubjectLabel";
            this.customSubjectLabel.Size = new System.Drawing.Size(81, 13);
            this.customSubjectLabel.TabIndex = 44;
            this.customSubjectLabel.Text = "Custom Subject";
            // 
            // versionAddCheckBox
            // 
            this.versionAddCheckBox.AutoSize = true;
            this.versionAddCheckBox.Location = new System.Drawing.Point(233, 104);
            this.versionAddCheckBox.Name = "versionAddCheckBox";
            this.versionAddCheckBox.Size = new System.Drawing.Size(205, 17);
            this.versionAddCheckBox.TabIndex = 6;
            this.versionAddCheckBox.Text = "Add version to Mantis if it doesn\'t exist";
            this.versionAddCheckBox.UseVisualStyleBackColor = true;
            // 
            // Mantis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.customSubjectLabel);
            this.Controls.Add(this.projectTrackerGroupBox);
            this.Controls.Add(this.noteLabel);
            this.Controls.Add(this.summaryTextBox);
            this.Name = "Mantis";
            this.Size = new System.Drawing.Size(502, 241);
            this.projectTrackerGroupBox.ResumeLayout(false);
            this.projectTrackerGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.TextBox summaryTextBox;
        private System.Windows.Forms.Label noteLabel;
		private System.Windows.Forms.GroupBox projectTrackerGroupBox;
		private System.Windows.Forms.TextBox trackerURLTextBox;
		private System.Windows.Forms.Label trackerURLLabel;
		private System.Windows.Forms.TextBox projectIDTextBox;
		private System.Windows.Forms.Label projectIDLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.TextBox categoryTextBox;
        private System.Windows.Forms.Label categoryLabel;
        private System.Windows.Forms.CheckBox uploadFailCheckBox;
        private System.Windows.Forms.CheckBox uploadCheckbox;
        private System.Windows.Forms.Label customSubjectLabel;
        private System.Windows.Forms.CheckBox versionAddCheckBox;

	}
}
