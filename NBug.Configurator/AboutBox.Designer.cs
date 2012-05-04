namespace NBug.Configurator
{
	partial class AboutBox
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
			this.okButton = new System.Windows.Forms.Button();
			this.copyrightLabel = new System.Windows.Forms.Label();
			this.productNameLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.panelDevelopers = new System.Windows.Forms.Panel();
			this.productHomePageLinkLabel = new System.Windows.Forms.LinkLabel();
			this.projectHomeLabel = new System.Windows.Forms.Label();
			this.leadDeveloperLinkLabel = new System.Windows.Forms.LinkLabel();
			this.developmentTeamLabel = new System.Windows.Forms.Label();
			this.panelDescription = new System.Windows.Forms.Panel();
			this.descriptionLabel = new System.Windows.Forms.Label();
			this.descriptionTitleLabel = new System.Windows.Forms.Label();
			this.copyrightRichTextBox = new System.Windows.Forms.RichTextBox();
			this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
			this.imageListMain = new System.Windows.Forms.ImageList(this.components);
			this.panelDevelopers.SuspendLayout();
			this.panelDescription.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.Location = new System.Drawing.Point(363, 255);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "&OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// copyrightLabel
			// 
			this.copyrightLabel.AutoSize = true;
			this.copyrightLabel.BackColor = System.Drawing.Color.Transparent;
			this.copyrightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.copyrightLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.copyrightLabel.Location = new System.Drawing.Point(144, 111);
			this.copyrightLabel.Name = "copyrightLabel";
			this.copyrightLabel.Size = new System.Drawing.Size(167, 13);
			this.copyrightLabel.TabIndex = 2;
			this.copyrightLabel.Text = "Copyright © 2011 Teoman Soygul";
			// 
			// productNameLabel
			// 
			this.productNameLabel.AutoSize = true;
			this.productNameLabel.BackColor = System.Drawing.Color.Transparent;
			this.productNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.productNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.productNameLabel.Location = new System.Drawing.Point(145, 93);
			this.productNameLabel.Name = "productNameLabel";
			this.productNameLabel.Size = new System.Drawing.Size(38, 13);
			this.productNameLabel.TabIndex = 5;
			this.productNameLabel.Text = "NBug";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.BackColor = System.Drawing.Color.Transparent;
			this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.versionLabel.ForeColor = System.Drawing.Color.White;
			this.versionLabel.Location = new System.Drawing.Point(189, 93);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(109, 13);
			this.versionLabel.TabIndex = 6;
			this.versionLabel.Text = "-   Version 1.0.0.0";
			// 
			// panelDevelopers
			// 
			this.panelDevelopers.BackColor = System.Drawing.Color.Transparent;
			this.panelDevelopers.Controls.Add(this.productHomePageLinkLabel);
			this.panelDevelopers.Controls.Add(this.projectHomeLabel);
			this.panelDevelopers.Controls.Add(this.leadDeveloperLinkLabel);
			this.panelDevelopers.Controls.Add(this.developmentTeamLabel);
			this.panelDevelopers.Location = new System.Drawing.Point(12, 146);
			this.panelDevelopers.Name = "panelDevelopers";
			this.panelDevelopers.Size = new System.Drawing.Size(196, 73);
			this.panelDevelopers.TabIndex = 10;
			// 
			// productHomePageLinkLabel
			// 
			this.productHomePageLinkLabel.AutoSize = true;
			this.productHomePageLinkLabel.BackColor = System.Drawing.Color.Transparent;
			this.productHomePageLinkLabel.Location = new System.Drawing.Point(5, 52);
			this.productHomePageLinkLabel.Name = "productHomePageLinkLabel";
			this.productHomePageLinkLabel.Size = new System.Drawing.Size(187, 13);
			this.productHomePageLinkLabel.TabIndex = 5;
			this.productHomePageLinkLabel.TabStop = true;
			this.productHomePageLinkLabel.Text = "http://www.nbusy.com/projects/nbug";
			// 
			// projectHomeLabel
			// 
			this.projectHomeLabel.AutoSize = true;
			this.projectHomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.projectHomeLabel.Location = new System.Drawing.Point(4, 37);
			this.projectHomeLabel.Name = "projectHomeLabel";
			this.projectHomeLabel.Size = new System.Drawing.Size(83, 13);
			this.projectHomeLabel.TabIndex = 5;
			this.projectHomeLabel.Text = "Project Home";
			// 
			// leadDeveloperLinkLabel
			// 
			this.leadDeveloperLinkLabel.AutoSize = true;
			this.leadDeveloperLinkLabel.Location = new System.Drawing.Point(5, 19);
			this.leadDeveloperLinkLabel.Name = "leadDeveloperLinkLabel";
			this.leadDeveloperLinkLabel.Size = new System.Drawing.Size(81, 13);
			this.leadDeveloperLinkLabel.TabIndex = 4;
			this.leadDeveloperLinkLabel.TabStop = true;
			this.leadDeveloperLinkLabel.Text = "Teoman Soygul";
			this.leadDeveloperLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LeadDeveloperLinkLabel_LinkClicked);
			// 
			// developmentTeamLabel
			// 
			this.developmentTeamLabel.AutoSize = true;
			this.developmentTeamLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.developmentTeamLabel.Location = new System.Drawing.Point(4, 4);
			this.developmentTeamLabel.Name = "developmentTeamLabel";
			this.developmentTeamLabel.Size = new System.Drawing.Size(65, 13);
			this.developmentTeamLabel.TabIndex = 0;
			this.developmentTeamLabel.Text = "Developer";
			// 
			// panelDescription
			// 
			this.panelDescription.BackColor = System.Drawing.Color.Transparent;
			this.panelDescription.Controls.Add(this.descriptionLabel);
			this.panelDescription.Controls.Add(this.descriptionTitleLabel);
			this.panelDescription.Location = new System.Drawing.Point(214, 146);
			this.panelDescription.Name = "panelDescription";
			this.panelDescription.Size = new System.Drawing.Size(224, 73);
			this.panelDescription.TabIndex = 11;
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.AutoEllipsis = true;
			this.descriptionLabel.AutoSize = true;
			this.descriptionLabel.Location = new System.Drawing.Point(4, 19);
			this.descriptionLabel.MaximumSize = new System.Drawing.Size(215, 0);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = new System.Drawing.Size(188, 39);
			this.descriptionLabel.TabIndex = 1;
			this.descriptionLabel.Text = "NBug is an open-source .NET library created to automate the bug reporting process" +
    ".";
			// 
			// descriptionTitleLabel
			// 
			this.descriptionTitleLabel.AutoSize = true;
			this.descriptionTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.descriptionTitleLabel.Location = new System.Drawing.Point(4, 4);
			this.descriptionTitleLabel.Name = "descriptionTitleLabel";
			this.descriptionTitleLabel.Size = new System.Drawing.Size(71, 13);
			this.descriptionTitleLabel.TabIndex = 0;
			this.descriptionTitleLabel.Text = "Description";
			// 
			// copyrightRichTextBox
			// 
			this.copyrightRichTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
			this.copyrightRichTextBox.Location = new System.Drawing.Point(19, 233);
			this.copyrightRichTextBox.Name = "copyrightRichTextBox";
			this.copyrightRichTextBox.Size = new System.Drawing.Size(315, 45);
			this.copyrightRichTextBox.TabIndex = 9;
			this.copyrightRichTextBox.Text = resources.GetString("copyrightRichTextBox.Text");
			// 
			// pictureBoxIcon
			// 
			this.pictureBoxIcon.BackColor = System.Drawing.Color.Transparent;
			this.pictureBoxIcon.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxIcon.Image")));
			this.pictureBoxIcon.Location = new System.Drawing.Point(102, 91);
			this.pictureBoxIcon.Name = "pictureBoxIcon";
			this.pictureBoxIcon.Size = new System.Drawing.Size(38, 38);
			this.pictureBoxIcon.TabIndex = 7;
			this.pictureBoxIcon.TabStop = false;
			// 
			// imageListMain
			// 
			this.imageListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListMain.ImageStream")));
			this.imageListMain.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListMain.Images.SetKeyName(0, "Connected.png");
			// 
			// AboutBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(450, 290);
			this.Controls.Add(this.panelDescription);
			this.Controls.Add(this.panelDevelopers);
			this.Controls.Add(this.copyrightRichTextBox);
			this.Controls.Add(this.pictureBoxIcon);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.productNameLabel);
			this.Controls.Add(this.copyrightLabel);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutBox";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About NBug";
			this.TopMost = true;
			this.panelDevelopers.ResumeLayout(false);
			this.panelDevelopers.PerformLayout();
			this.panelDescription.ResumeLayout(false);
			this.panelDescription.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label copyrightLabel;
		private System.Windows.Forms.Label productNameLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Panel panelDevelopers;
		private System.Windows.Forms.Label developmentTeamLabel;
		private System.Windows.Forms.LinkLabel leadDeveloperLinkLabel;
		private System.Windows.Forms.Panel panelDescription;
		private System.Windows.Forms.Label descriptionTitleLabel;
		private System.Windows.Forms.Label descriptionLabel;
		private System.Windows.Forms.RichTextBox copyrightRichTextBox;
		private System.Windows.Forms.PictureBox pictureBoxIcon;
		private System.Windows.Forms.ImageList imageListMain;
		private System.Windows.Forms.LinkLabel productHomePageLinkLabel;
		private System.Windows.Forms.Label projectHomeLabel;

	}
}
