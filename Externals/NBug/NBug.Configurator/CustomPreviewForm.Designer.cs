namespace NBug.Configurator
{
	partial class CustomPreviewForm
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
            this.warningLabel = new System.Windows.Forms.Label();
            this.exceptionMessageLabel = new System.Windows.Forms.Label();
            this.quitButton = new System.Windows.Forms.Button();
            this.continueButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // warningLabel
            // 
            this.warningLabel.Location = new System.Drawing.Point(77, 12);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(347, 47);
            this.warningLabel.TabIndex = 4;
            this.warningLabel.Text = "Unhandled exception has occurred in your application. If you click Continue, the " +
    "application will ignore this error and attempt to continue. If you click quit, t" +
    "he application will close immediately.";
            // 
            // exceptionMessageLabel
            // 
            this.exceptionMessageLabel.Location = new System.Drawing.Point(77, 64);
            this.exceptionMessageLabel.Name = "exceptionMessageLabel";
            this.exceptionMessageLabel.Size = new System.Drawing.Size(347, 47);
            this.exceptionMessageLabel.TabIndex = 3;
            // 
            // quitButton
            // 
            this.quitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.quitButton.Location = new System.Drawing.Point(329, 119);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(100, 23);
            this.quitButton.TabIndex = 1;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(223, 119);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(100, 23);
            this.continueButton.TabIndex = 2;
            this.continueButton.Text = "Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // Normal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.quitButton;
            this.ClientSize = new System.Drawing.Size(436, 148);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.exceptionMessageLabel);
            this.Controls.Add(this.warningLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Normal";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomUI Error";
            this.TopMost = true;
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Label warningLabel;
		private System.Windows.Forms.Label exceptionMessageLabel;
		private System.Windows.Forms.Button quitButton;
		private System.Windows.Forms.Button continueButton;
	}
}