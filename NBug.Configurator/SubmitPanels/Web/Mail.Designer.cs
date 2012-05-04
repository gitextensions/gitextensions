namespace NBug.Configurator.SubmitPanels.Web
{
	partial class Mail
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
			this.fromLabel = new System.Windows.Forms.Label();
			this.fromNameTextBox = new System.Windows.Forms.TextBox();
			this.fromNameLabel = new System.Windows.Forms.Label();
			this.toLabel = new System.Windows.Forms.Label();
			this.toListBox = new System.Windows.Forms.ListBox();
			this.toAddButton = new System.Windows.Forms.Button();
			this.toRemoveButton = new System.Windows.Forms.Button();
			this.fromTextBox = new System.Windows.Forms.TextBox();
			this.toTextBox = new System.Windows.Forms.TextBox();
			this.smtpServerLabel = new System.Windows.Forms.Label();
			this.smtpServerTextBox = new System.Windows.Forms.TextBox();
			this.useSslCheckBox = new System.Windows.Forms.CheckBox();
			this.portLabel = new System.Windows.Forms.Label();
			this.portNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.priorityLabel = new System.Windows.Forms.Label();
			this.priorityComboBox = new System.Windows.Forms.ComboBox();
			this.useAuthenticationCheckBox = new System.Windows.Forms.CheckBox();
			this.usernameTextBox = new System.Windows.Forms.TextBox();
			this.usernameLabel = new System.Windows.Forms.Label();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.passwordLabel = new System.Windows.Forms.Label();
			this.ccTextBox = new System.Windows.Forms.TextBox();
			this.ccRemoveButton = new System.Windows.Forms.Button();
			this.ccAddButton = new System.Windows.Forms.Button();
			this.ccListBox = new System.Windows.Forms.ListBox();
			this.ccLabel = new System.Windows.Forms.Label();
			this.bccTextBox = new System.Windows.Forms.TextBox();
			this.bccRemoveButton = new System.Windows.Forms.Button();
			this.bccAddButton = new System.Windows.Forms.Button();
			this.bccListBox = new System.Windows.Forms.ListBox();
			this.bccLabel = new System.Windows.Forms.Label();
			this.customSubjectTextBox = new System.Windows.Forms.TextBox();
			this.customSubjectLabel = new System.Windows.Forms.Label();
			this.customBodyTextBox = new System.Windows.Forms.TextBox();
			this.customBodyLabel = new System.Windows.Forms.Label();
			this.replyToTextBox = new System.Windows.Forms.TextBox();
			this.replyToLabel = new System.Windows.Forms.Label();
			this.useAttachmentCheckBox = new System.Windows.Forms.CheckBox();
			this.noteLabel = new System.Windows.Forms.Label();
			this.fromGroupBox = new System.Windows.Forms.GroupBox();
			this.smtpServerGroupBox = new System.Windows.Forms.GroupBox();
			this.mailGroupBox = new System.Windows.Forms.GroupBox();
			this.authenticationGroupBox = new System.Windows.Forms.GroupBox();
			this.recepientsGroupBox = new System.Windows.Forms.GroupBox();
			this.attachmentsGroupBox = new System.Windows.Forms.GroupBox();
			this.defaultPortCheckBox = new System.Windows.Forms.CheckBox();
			this.securityLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).BeginInit();
			this.fromGroupBox.SuspendLayout();
			this.smtpServerGroupBox.SuspendLayout();
			this.mailGroupBox.SuspendLayout();
			this.authenticationGroupBox.SuspendLayout();
			this.recepientsGroupBox.SuspendLayout();
			this.attachmentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// fromLabel
			// 
			this.fromLabel.AutoSize = true;
			this.fromLabel.Location = new System.Drawing.Point(6, 21);
			this.fromLabel.Name = "fromLabel";
			this.fromLabel.Size = new System.Drawing.Size(33, 13);
			this.fromLabel.TabIndex = 0;
			this.fromLabel.Text = "From:";
			// 
			// fromNameTextBox
			// 
			this.fromNameTextBox.Location = new System.Drawing.Point(76, 44);
			this.fromNameTextBox.Name = "fromNameTextBox";
			this.fromNameTextBox.Size = new System.Drawing.Size(159, 20);
			this.fromNameTextBox.TabIndex = 3;
			this.fromNameTextBox.Text = "NBug Error Reporter";
			// 
			// fromNameLabel
			// 
			this.fromNameLabel.AutoSize = true;
			this.fromNameLabel.Location = new System.Drawing.Point(6, 47);
			this.fromNameLabel.Name = "fromNameLabel";
			this.fromNameLabel.Size = new System.Drawing.Size(64, 13);
			this.fromNameLabel.TabIndex = 2;
			this.fromNameLabel.Text = "From Name:";
			// 
			// toLabel
			// 
			this.toLabel.AutoSize = true;
			this.toLabel.Location = new System.Drawing.Point(6, 24);
			this.toLabel.Name = "toLabel";
			this.toLabel.Size = new System.Drawing.Size(27, 13);
			this.toLabel.TabIndex = 4;
			this.toLabel.Text = "To*:";
			// 
			// toListBox
			// 
			this.toListBox.FormattingEnabled = true;
			this.toListBox.Location = new System.Drawing.Point(39, 47);
			this.toListBox.Name = "toListBox";
			this.toListBox.Size = new System.Drawing.Size(171, 95);
			this.toListBox.TabIndex = 5;
			// 
			// toAddButton
			// 
			this.toAddButton.Location = new System.Drawing.Point(216, 21);
			this.toAddButton.Name = "toAddButton";
			this.toAddButton.Size = new System.Drawing.Size(61, 20);
			this.toAddButton.TabIndex = 7;
			this.toAddButton.Text = "Add";
			this.toAddButton.UseVisualStyleBackColor = true;
			this.toAddButton.Click += new System.EventHandler(this.ToAddButton_Click);
			// 
			// toRemoveButton
			// 
			this.toRemoveButton.Location = new System.Drawing.Point(216, 84);
			this.toRemoveButton.Name = "toRemoveButton";
			this.toRemoveButton.Size = new System.Drawing.Size(61, 20);
			this.toRemoveButton.TabIndex = 8;
			this.toRemoveButton.Text = "Remove";
			this.toRemoveButton.UseVisualStyleBackColor = true;
			this.toRemoveButton.Click += new System.EventHandler(this.ToRemoveButton_Click);
			// 
			// fromTextBox
			// 
			this.fromTextBox.Location = new System.Drawing.Point(76, 18);
			this.fromTextBox.Name = "fromTextBox";
			this.fromTextBox.Size = new System.Drawing.Size(159, 20);
			this.fromTextBox.TabIndex = 1;
			this.fromTextBox.Text = "me@domain.com";
			// 
			// toTextBox
			// 
			this.toTextBox.Location = new System.Drawing.Point(39, 21);
			this.toTextBox.Name = "toTextBox";
			this.toTextBox.Size = new System.Drawing.Size(171, 20);
			this.toTextBox.TabIndex = 9;
			// 
			// smtpServerLabel
			// 
			this.smtpServerLabel.AutoSize = true;
			this.smtpServerLabel.Location = new System.Drawing.Point(6, 21);
			this.smtpServerLabel.Name = "smtpServerLabel";
			this.smtpServerLabel.Size = new System.Drawing.Size(74, 13);
			this.smtpServerLabel.TabIndex = 10;
			this.smtpServerLabel.Text = "SMTP Server:";
			// 
			// smtpServerTextBox
			// 
			this.smtpServerTextBox.Location = new System.Drawing.Point(86, 18);
			this.smtpServerTextBox.Name = "smtpServerTextBox";
			this.smtpServerTextBox.Size = new System.Drawing.Size(149, 20);
			this.smtpServerTextBox.TabIndex = 11;
			// 
			// useSslCheckBox
			// 
			this.useSslCheckBox.AutoSize = true;
			this.useSslCheckBox.Location = new System.Drawing.Point(86, 99);
			this.useSslCheckBox.Name = "useSslCheckBox";
			this.useSslCheckBox.Size = new System.Drawing.Size(68, 17);
			this.useSslCheckBox.TabIndex = 13;
			this.useSslCheckBox.Text = "Use SSL";
			this.useSslCheckBox.UseVisualStyleBackColor = true;
			this.useSslCheckBox.CheckedChanged += new System.EventHandler(this.UseSslCheckBox_CheckedChanged);
			// 
			// portLabel
			// 
			this.portLabel.AutoSize = true;
			this.portLabel.Location = new System.Drawing.Point(6, 75);
			this.portLabel.Name = "portLabel";
			this.portLabel.Size = new System.Drawing.Size(29, 13);
			this.portLabel.TabIndex = 14;
			this.portLabel.Text = "Port:";
			// 
			// portNumericUpDown
			// 
			this.portNumericUpDown.Enabled = false;
			this.portNumericUpDown.Location = new System.Drawing.Point(86, 73);
			this.portNumericUpDown.Name = "portNumericUpDown";
			this.portNumericUpDown.Size = new System.Drawing.Size(44, 20);
			this.portNumericUpDown.TabIndex = 15;
			this.portNumericUpDown.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
			// 
			// priorityLabel
			// 
			this.priorityLabel.AutoSize = true;
			this.priorityLabel.Location = new System.Drawing.Point(6, 47);
			this.priorityLabel.Name = "priorityLabel";
			this.priorityLabel.Size = new System.Drawing.Size(41, 13);
			this.priorityLabel.TabIndex = 16;
			this.priorityLabel.Text = "Priority:";
			// 
			// priorityComboBox
			// 
			this.priorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.priorityComboBox.FormattingEnabled = true;
			this.priorityComboBox.Items.AddRange(new object[] {
            "High",
            "Normal",
            "Low"});
			this.priorityComboBox.Location = new System.Drawing.Point(86, 44);
			this.priorityComboBox.Name = "priorityComboBox";
			this.priorityComboBox.Size = new System.Drawing.Size(98, 21);
			this.priorityComboBox.TabIndex = 17;
			// 
			// useAuthenticationCheckBox
			// 
			this.useAuthenticationCheckBox.AutoSize = true;
			this.useAuthenticationCheckBox.Location = new System.Drawing.Point(86, 15);
			this.useAuthenticationCheckBox.Name = "useAuthenticationCheckBox";
			this.useAuthenticationCheckBox.Size = new System.Drawing.Size(116, 17);
			this.useAuthenticationCheckBox.TabIndex = 18;
			this.useAuthenticationCheckBox.Text = "Use Authentication";
			this.useAuthenticationCheckBox.UseVisualStyleBackColor = true;
			this.useAuthenticationCheckBox.CheckedChanged += new System.EventHandler(this.UseAuthenticationCheckBox_CheckedChanged);
			// 
			// usernameTextBox
			// 
			this.usernameTextBox.Enabled = false;
			this.usernameTextBox.Location = new System.Drawing.Point(86, 38);
			this.usernameTextBox.Name = "usernameTextBox";
			this.usernameTextBox.Size = new System.Drawing.Size(149, 20);
			this.usernameTextBox.TabIndex = 20;
			// 
			// usernameLabel
			// 
			this.usernameLabel.AutoSize = true;
			this.usernameLabel.Location = new System.Drawing.Point(6, 41);
			this.usernameLabel.Name = "usernameLabel";
			this.usernameLabel.Size = new System.Drawing.Size(58, 13);
			this.usernameLabel.TabIndex = 19;
			this.usernameLabel.Text = "Username:";
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Enabled = false;
			this.passwordTextBox.Location = new System.Drawing.Point(86, 64);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.Size = new System.Drawing.Size(149, 20);
			this.passwordTextBox.TabIndex = 22;
			// 
			// passwordLabel
			// 
			this.passwordLabel.AutoSize = true;
			this.passwordLabel.Location = new System.Drawing.Point(6, 67);
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Size = new System.Drawing.Size(56, 13);
			this.passwordLabel.TabIndex = 21;
			this.passwordLabel.Text = "Password:";
			// 
			// ccTextBox
			// 
			this.ccTextBox.Location = new System.Drawing.Point(39, 159);
			this.ccTextBox.Name = "ccTextBox";
			this.ccTextBox.Size = new System.Drawing.Size(171, 20);
			this.ccTextBox.TabIndex = 27;
			// 
			// ccRemoveButton
			// 
			this.ccRemoveButton.Location = new System.Drawing.Point(216, 204);
			this.ccRemoveButton.Name = "ccRemoveButton";
			this.ccRemoveButton.Size = new System.Drawing.Size(61, 20);
			this.ccRemoveButton.TabIndex = 26;
			this.ccRemoveButton.Text = "Remove";
			this.ccRemoveButton.UseVisualStyleBackColor = true;
			this.ccRemoveButton.Click += new System.EventHandler(this.CcRemoveButton_Click);
			// 
			// ccAddButton
			// 
			this.ccAddButton.Location = new System.Drawing.Point(216, 159);
			this.ccAddButton.Name = "ccAddButton";
			this.ccAddButton.Size = new System.Drawing.Size(61, 20);
			this.ccAddButton.TabIndex = 25;
			this.ccAddButton.Text = "Add";
			this.ccAddButton.UseVisualStyleBackColor = true;
			this.ccAddButton.Click += new System.EventHandler(this.CcAddButton_Click);
			// 
			// ccListBox
			// 
			this.ccListBox.FormattingEnabled = true;
			this.ccListBox.Location = new System.Drawing.Point(39, 185);
			this.ccListBox.Name = "ccListBox";
			this.ccListBox.Size = new System.Drawing.Size(171, 56);
			this.ccListBox.TabIndex = 24;
			// 
			// ccLabel
			// 
			this.ccLabel.AutoSize = true;
			this.ccLabel.Location = new System.Drawing.Point(10, 162);
			this.ccLabel.Name = "ccLabel";
			this.ccLabel.Size = new System.Drawing.Size(23, 13);
			this.ccLabel.TabIndex = 23;
			this.ccLabel.Text = "Cc:";
			// 
			// bccTextBox
			// 
			this.bccTextBox.Location = new System.Drawing.Point(39, 259);
			this.bccTextBox.Name = "bccTextBox";
			this.bccTextBox.Size = new System.Drawing.Size(171, 20);
			this.bccTextBox.TabIndex = 32;
			// 
			// bccRemoveButton
			// 
			this.bccRemoveButton.Location = new System.Drawing.Point(216, 304);
			this.bccRemoveButton.Name = "bccRemoveButton";
			this.bccRemoveButton.Size = new System.Drawing.Size(61, 20);
			this.bccRemoveButton.TabIndex = 31;
			this.bccRemoveButton.Text = "Remove";
			this.bccRemoveButton.UseVisualStyleBackColor = true;
			this.bccRemoveButton.Click += new System.EventHandler(this.BccRemoveButton_Click);
			// 
			// bccAddButton
			// 
			this.bccAddButton.Location = new System.Drawing.Point(216, 259);
			this.bccAddButton.Name = "bccAddButton";
			this.bccAddButton.Size = new System.Drawing.Size(61, 20);
			this.bccAddButton.TabIndex = 30;
			this.bccAddButton.Text = "Add";
			this.bccAddButton.UseVisualStyleBackColor = true;
			this.bccAddButton.Click += new System.EventHandler(this.BccAddButton_Click);
			// 
			// bccListBox
			// 
			this.bccListBox.FormattingEnabled = true;
			this.bccListBox.Location = new System.Drawing.Point(39, 285);
			this.bccListBox.Name = "bccListBox";
			this.bccListBox.Size = new System.Drawing.Size(171, 56);
			this.bccListBox.TabIndex = 29;
			// 
			// bccLabel
			// 
			this.bccLabel.AutoSize = true;
			this.bccLabel.Location = new System.Drawing.Point(6, 262);
			this.bccLabel.Name = "bccLabel";
			this.bccLabel.Size = new System.Drawing.Size(29, 13);
			this.bccLabel.TabIndex = 28;
			this.bccLabel.Text = "Bcc:";
			// 
			// customSubjectTextBox
			// 
			this.customSubjectTextBox.Location = new System.Drawing.Point(6, 36);
			this.customSubjectTextBox.Name = "customSubjectTextBox";
			this.customSubjectTextBox.Size = new System.Drawing.Size(229, 20);
			this.customSubjectTextBox.TabIndex = 34;
			// 
			// customSubjectLabel
			// 
			this.customSubjectLabel.AutoSize = true;
			this.customSubjectLabel.Location = new System.Drawing.Point(6, 19);
			this.customSubjectLabel.Name = "customSubjectLabel";
			this.customSubjectLabel.Size = new System.Drawing.Size(84, 13);
			this.customSubjectLabel.TabIndex = 33;
			this.customSubjectLabel.Text = "Custom Subject:";
			// 
			// customBodyTextBox
			// 
			this.customBodyTextBox.Location = new System.Drawing.Point(6, 76);
			this.customBodyTextBox.Multiline = true;
			this.customBodyTextBox.Name = "customBodyTextBox";
			this.customBodyTextBox.Size = new System.Drawing.Size(229, 60);
			this.customBodyTextBox.TabIndex = 36;
			// 
			// customBodyLabel
			// 
			this.customBodyLabel.AutoSize = true;
			this.customBodyLabel.Location = new System.Drawing.Point(6, 59);
			this.customBodyLabel.Name = "customBodyLabel";
			this.customBodyLabel.Size = new System.Drawing.Size(72, 13);
			this.customBodyLabel.TabIndex = 35;
			this.customBodyLabel.Text = "Custom Body:";
			// 
			// replyToTextBox
			// 
			this.replyToTextBox.Location = new System.Drawing.Point(65, 357);
			this.replyToTextBox.Name = "replyToTextBox";
			this.replyToTextBox.Size = new System.Drawing.Size(145, 20);
			this.replyToTextBox.TabIndex = 38;
			// 
			// replyToLabel
			// 
			this.replyToLabel.AutoSize = true;
			this.replyToLabel.Location = new System.Drawing.Point(6, 360);
			this.replyToLabel.Name = "replyToLabel";
			this.replyToLabel.Size = new System.Drawing.Size(53, 13);
			this.replyToLabel.TabIndex = 37;
			this.replyToLabel.Text = "Reply To:";
			// 
			// useAttachmentCheckBox
			// 
			this.useAttachmentCheckBox.AutoSize = true;
			this.useAttachmentCheckBox.Location = new System.Drawing.Point(10, 24);
			this.useAttachmentCheckBox.Name = "useAttachmentCheckBox";
			this.useAttachmentCheckBox.Size = new System.Drawing.Size(299, 17);
			this.useAttachmentCheckBox.TabIndex = 39;
			this.useAttachmentCheckBox.Text = "Send compressed report file (Exception.zip) as attachment";
			this.useAttachmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// noteLabel
			// 
			this.noteLabel.AutoSize = true;
			this.noteLabel.Location = new System.Drawing.Point(9, 476);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(162, 13);
			this.noteLabel.TabIndex = 40;
			this.noteLabel.Text = "Note: * Denotes mendatory fields";
			// 
			// fromGroupBox
			// 
			this.fromGroupBox.Controls.Add(this.fromLabel);
			this.fromGroupBox.Controls.Add(this.fromTextBox);
			this.fromGroupBox.Controls.Add(this.fromNameLabel);
			this.fromGroupBox.Controls.Add(this.fromNameTextBox);
			this.fromGroupBox.Location = new System.Drawing.Point(3, 3);
			this.fromGroupBox.Name = "fromGroupBox";
			this.fromGroupBox.Padding = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.fromGroupBox.Size = new System.Drawing.Size(249, 75);
			this.fromGroupBox.TabIndex = 41;
			this.fromGroupBox.TabStop = false;
			this.fromGroupBox.Text = "From";
			// 
			// smtpServerGroupBox
			// 
			this.smtpServerGroupBox.Controls.Add(this.securityLabel);
			this.smtpServerGroupBox.Controls.Add(this.defaultPortCheckBox);
			this.smtpServerGroupBox.Controls.Add(this.smtpServerLabel);
			this.smtpServerGroupBox.Controls.Add(this.smtpServerTextBox);
			this.smtpServerGroupBox.Controls.Add(this.priorityLabel);
			this.smtpServerGroupBox.Controls.Add(this.priorityComboBox);
			this.smtpServerGroupBox.Controls.Add(this.portNumericUpDown);
			this.smtpServerGroupBox.Controls.Add(this.portLabel);
			this.smtpServerGroupBox.Controls.Add(this.useSslCheckBox);
			this.smtpServerGroupBox.Location = new System.Drawing.Point(3, 84);
			this.smtpServerGroupBox.Name = "smtpServerGroupBox";
			this.smtpServerGroupBox.Padding = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.smtpServerGroupBox.Size = new System.Drawing.Size(249, 125);
			this.smtpServerGroupBox.TabIndex = 42;
			this.smtpServerGroupBox.TabStop = false;
			this.smtpServerGroupBox.Text = "SMTP Server";
			// 
			// mailGroupBox
			// 
			this.mailGroupBox.Controls.Add(this.customSubjectLabel);
			this.mailGroupBox.Controls.Add(this.customSubjectTextBox);
			this.mailGroupBox.Controls.Add(this.customBodyLabel);
			this.mailGroupBox.Controls.Add(this.customBodyTextBox);
			this.mailGroupBox.Location = new System.Drawing.Point(3, 315);
			this.mailGroupBox.Name = "mailGroupBox";
			this.mailGroupBox.Padding = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.mailGroupBox.Size = new System.Drawing.Size(249, 146);
			this.mailGroupBox.TabIndex = 42;
			this.mailGroupBox.TabStop = false;
			this.mailGroupBox.Text = "Mail";
			// 
			// authenticationGroupBox
			// 
			this.authenticationGroupBox.Controls.Add(this.useAuthenticationCheckBox);
			this.authenticationGroupBox.Controls.Add(this.usernameLabel);
			this.authenticationGroupBox.Controls.Add(this.usernameTextBox);
			this.authenticationGroupBox.Controls.Add(this.passwordLabel);
			this.authenticationGroupBox.Controls.Add(this.passwordTextBox);
			this.authenticationGroupBox.Location = new System.Drawing.Point(3, 215);
			this.authenticationGroupBox.Name = "authenticationGroupBox";
			this.authenticationGroupBox.Size = new System.Drawing.Size(249, 94);
			this.authenticationGroupBox.TabIndex = 42;
			this.authenticationGroupBox.TabStop = false;
			this.authenticationGroupBox.Text = "Authentication";
			// 
			// recepientsGroupBox
			// 
			this.recepientsGroupBox.Controls.Add(this.replyToLabel);
			this.recepientsGroupBox.Controls.Add(this.replyToTextBox);
			this.recepientsGroupBox.Controls.Add(this.toLabel);
			this.recepientsGroupBox.Controls.Add(this.toListBox);
			this.recepientsGroupBox.Controls.Add(this.toAddButton);
			this.recepientsGroupBox.Controls.Add(this.toRemoveButton);
			this.recepientsGroupBox.Controls.Add(this.toTextBox);
			this.recepientsGroupBox.Controls.Add(this.ccLabel);
			this.recepientsGroupBox.Controls.Add(this.ccListBox);
			this.recepientsGroupBox.Controls.Add(this.ccAddButton);
			this.recepientsGroupBox.Controls.Add(this.ccRemoveButton);
			this.recepientsGroupBox.Controls.Add(this.bccTextBox);
			this.recepientsGroupBox.Controls.Add(this.ccTextBox);
			this.recepientsGroupBox.Controls.Add(this.bccRemoveButton);
			this.recepientsGroupBox.Controls.Add(this.bccLabel);
			this.recepientsGroupBox.Controls.Add(this.bccAddButton);
			this.recepientsGroupBox.Controls.Add(this.bccListBox);
			this.recepientsGroupBox.Location = new System.Drawing.Point(278, 3);
			this.recepientsGroupBox.Name = "recepientsGroupBox";
			this.recepientsGroupBox.Size = new System.Drawing.Size(311, 391);
			this.recepientsGroupBox.TabIndex = 42;
			this.recepientsGroupBox.TabStop = false;
			this.recepientsGroupBox.Text = "Recepients";
			// 
			// attachmentsGroupBox
			// 
			this.attachmentsGroupBox.Controls.Add(this.useAttachmentCheckBox);
			this.attachmentsGroupBox.Location = new System.Drawing.Point(278, 400);
			this.attachmentsGroupBox.Name = "attachmentsGroupBox";
			this.attachmentsGroupBox.Size = new System.Drawing.Size(311, 56);
			this.attachmentsGroupBox.TabIndex = 43;
			this.attachmentsGroupBox.TabStop = false;
			this.attachmentsGroupBox.Text = "Attachments";
			// 
			// defaultPortCheckBox
			// 
			this.defaultPortCheckBox.AutoSize = true;
			this.defaultPortCheckBox.Checked = true;
			this.defaultPortCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.defaultPortCheckBox.Location = new System.Drawing.Point(140, 75);
			this.defaultPortCheckBox.Name = "defaultPortCheckBox";
			this.defaultPortCheckBox.Size = new System.Drawing.Size(60, 17);
			this.defaultPortCheckBox.TabIndex = 18;
			this.defaultPortCheckBox.Text = "Default";
			this.defaultPortCheckBox.UseVisualStyleBackColor = true;
			this.defaultPortCheckBox.CheckedChanged += new System.EventHandler(this.DefaultPortCheckBox_CheckedChanged);
			// 
			// securityLabel
			// 
			this.securityLabel.AutoSize = true;
			this.securityLabel.Location = new System.Drawing.Point(6, 100);
			this.securityLabel.Name = "securityLabel";
			this.securityLabel.Size = new System.Drawing.Size(48, 13);
			this.securityLabel.TabIndex = 19;
			this.securityLabel.Text = "Security:";
			// 
			// Mail
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.attachmentsGroupBox);
			this.Controls.Add(this.mailGroupBox);
			this.Controls.Add(this.recepientsGroupBox);
			this.Controls.Add(this.authenticationGroupBox);
			this.Controls.Add(this.smtpServerGroupBox);
			this.Controls.Add(this.fromGroupBox);
			this.Controls.Add(this.noteLabel);
			this.Name = "Mail";
			this.Size = new System.Drawing.Size(660, 552);
			((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).EndInit();
			this.fromGroupBox.ResumeLayout(false);
			this.fromGroupBox.PerformLayout();
			this.smtpServerGroupBox.ResumeLayout(false);
			this.smtpServerGroupBox.PerformLayout();
			this.mailGroupBox.ResumeLayout(false);
			this.mailGroupBox.PerformLayout();
			this.authenticationGroupBox.ResumeLayout(false);
			this.authenticationGroupBox.PerformLayout();
			this.recepientsGroupBox.ResumeLayout(false);
			this.recepientsGroupBox.PerformLayout();
			this.attachmentsGroupBox.ResumeLayout(false);
			this.attachmentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label fromLabel;
		private System.Windows.Forms.TextBox fromNameTextBox;
		private System.Windows.Forms.Label fromNameLabel;
		private System.Windows.Forms.Label toLabel;
		private System.Windows.Forms.ListBox toListBox;
		private System.Windows.Forms.Button toAddButton;
		private System.Windows.Forms.Button toRemoveButton;
		private System.Windows.Forms.TextBox fromTextBox;
		private System.Windows.Forms.TextBox toTextBox;
		private System.Windows.Forms.Label smtpServerLabel;
		private System.Windows.Forms.TextBox smtpServerTextBox;
		private System.Windows.Forms.CheckBox useSslCheckBox;
		private System.Windows.Forms.Label portLabel;
		private System.Windows.Forms.NumericUpDown portNumericUpDown;
		private System.Windows.Forms.Label priorityLabel;
		private System.Windows.Forms.ComboBox priorityComboBox;
		private System.Windows.Forms.CheckBox useAuthenticationCheckBox;
		private System.Windows.Forms.TextBox usernameTextBox;
		private System.Windows.Forms.Label usernameLabel;
		private System.Windows.Forms.TextBox passwordTextBox;
		private System.Windows.Forms.Label passwordLabel;
		private System.Windows.Forms.TextBox ccTextBox;
		private System.Windows.Forms.Button ccRemoveButton;
		private System.Windows.Forms.Button ccAddButton;
		private System.Windows.Forms.ListBox ccListBox;
		private System.Windows.Forms.Label ccLabel;
		private System.Windows.Forms.TextBox bccTextBox;
		private System.Windows.Forms.Button bccRemoveButton;
		private System.Windows.Forms.Button bccAddButton;
		private System.Windows.Forms.ListBox bccListBox;
		private System.Windows.Forms.Label bccLabel;
		private System.Windows.Forms.TextBox customSubjectTextBox;
		private System.Windows.Forms.Label customSubjectLabel;
		private System.Windows.Forms.TextBox customBodyTextBox;
		private System.Windows.Forms.Label customBodyLabel;
		private System.Windows.Forms.TextBox replyToTextBox;
		private System.Windows.Forms.Label replyToLabel;
		private System.Windows.Forms.CheckBox useAttachmentCheckBox;
		private System.Windows.Forms.Label noteLabel;
		private System.Windows.Forms.GroupBox fromGroupBox;
		private System.Windows.Forms.GroupBox smtpServerGroupBox;
		private System.Windows.Forms.GroupBox mailGroupBox;
		private System.Windows.Forms.GroupBox authenticationGroupBox;
		private System.Windows.Forms.GroupBox recepientsGroupBox;
		private System.Windows.Forms.GroupBox attachmentsGroupBox;
		private System.Windows.Forms.Label securityLabel;
		private System.Windows.Forms.CheckBox defaultPortCheckBox;
	}
}
