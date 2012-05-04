namespace NBug.Configurator
{
	partial class PreviewForm
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
			this.closeButton = new System.Windows.Forms.Button();
			this.consoleOutputTextBox = new System.Windows.Forms.TextBox();
			this.consoleOutputLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.Location = new System.Drawing.Point(321, 153);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(58, 22);
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "&Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// consoleOutputTextBox
			// 
			this.consoleOutputTextBox.BackColor = System.Drawing.Color.Black;
			this.consoleOutputTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.consoleOutputTextBox.ForeColor = System.Drawing.Color.White;
			this.consoleOutputTextBox.Location = new System.Drawing.Point(12, 25);
			this.consoleOutputTextBox.Multiline = true;
			this.consoleOutputTextBox.Name = "consoleOutputTextBox";
			this.consoleOutputTextBox.ReadOnly = true;
			this.consoleOutputTextBox.Size = new System.Drawing.Size(367, 122);
			this.consoleOutputTextBox.TabIndex = 1;
			// 
			// consoleOutputLabel
			// 
			this.consoleOutputLabel.AutoSize = true;
			this.consoleOutputLabel.Location = new System.Drawing.Point(12, 9);
			this.consoleOutputLabel.Name = "consoleOutputLabel";
			this.consoleOutputLabel.Size = new System.Drawing.Size(83, 13);
			this.consoleOutputLabel.TabIndex = 2;
			this.consoleOutputLabel.Text = "Console Output:";
			// 
			// PreviewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(391, 181);
			this.Controls.Add(this.consoleOutputLabel);
			this.Controls.Add(this.consoleOutputTextBox);
			this.Controls.Add(this.closeButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PreviewForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Preview";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.TextBox consoleOutputTextBox;
		private System.Windows.Forms.Label consoleOutputLabel;
	}
}