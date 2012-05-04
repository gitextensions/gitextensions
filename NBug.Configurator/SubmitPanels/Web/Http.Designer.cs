namespace NBug.Configurator.SubmitPanels.Web
{
	partial class Http
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Http));
			this.urlTextBox = new System.Windows.Forms.TextBox();
			this.urlLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.noteLabel = new System.Windows.Forms.Label();
			this.webServerGroupBox = new System.Windows.Forms.GroupBox();
			this.webServerGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// urlTextBox
			// 
			this.urlTextBox.Location = new System.Drawing.Point(39, 18);
			this.urlTextBox.Name = "urlTextBox";
			this.urlTextBox.Size = new System.Drawing.Size(262, 20);
			this.urlTextBox.TabIndex = 3;
			// 
			// urlLabel
			// 
			this.urlLabel.AutoSize = true;
			this.urlLabel.Location = new System.Drawing.Point(6, 21);
			this.urlLabel.Name = "urlLabel";
			this.urlLabel.Size = new System.Drawing.Size(27, 13);
			this.urlLabel.TabIndex = 2;
			this.urlLabel.Text = "Url*:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.SystemColors.AppWorkspace;
			this.label1.Location = new System.Drawing.Point(39, 109);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(429, 273);
			this.label1.TabIndex = 4;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// noteLabel
			// 
			this.noteLabel.AutoSize = true;
			this.noteLabel.Location = new System.Drawing.Point(9, 69);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(162, 13);
			this.noteLabel.TabIndex = 42;
			this.noteLabel.Text = "Note: * Denotes mendatory fields";
			// 
			// webServerGroupBox
			// 
			this.webServerGroupBox.Controls.Add(this.urlLabel);
			this.webServerGroupBox.Controls.Add(this.urlTextBox);
			this.webServerGroupBox.Location = new System.Drawing.Point(3, 3);
			this.webServerGroupBox.Name = "webServerGroupBox";
			this.webServerGroupBox.Size = new System.Drawing.Size(319, 50);
			this.webServerGroupBox.TabIndex = 43;
			this.webServerGroupBox.TabStop = false;
			this.webServerGroupBox.Text = "Web Server";
			// 
			// Http
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.webServerGroupBox);
			this.Controls.Add(this.noteLabel);
			this.Controls.Add(this.label1);
			this.Name = "Http";
			this.Size = new System.Drawing.Size(660, 423);
			this.webServerGroupBox.ResumeLayout(false);
			this.webServerGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox urlTextBox;
		private System.Windows.Forms.Label urlLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label noteLabel;
		private System.Windows.Forms.GroupBox webServerGroupBox;
	}
}
