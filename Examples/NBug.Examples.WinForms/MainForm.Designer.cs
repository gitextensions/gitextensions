namespace NBug.Examples.WinForms
{
	partial class MainForm
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
			System.Windows.Forms.Label label1;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.crashButton = new System.Windows.Forms.Button();
			this.crashTypeComboBox = new System.Windows.Forms.ComboBox();
			this.closeButton = new System.Windows.Forms.Button();
			this.StackTextBox = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 56);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(307, 13);
			label1.TabIndex = 1;
			label1.Text = "Stack (use numbers for functions, comma for nested exception):";
			// 
			// crashButton
			// 
			this.crashButton.Image = ((System.Drawing.Image)(resources.GetObject("crashButton.Image")));
			this.crashButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.crashButton.Location = new System.Drawing.Point(53, 98);
			this.crashButton.Name = "crashButton";
			this.crashButton.Size = new System.Drawing.Size(127, 23);
			this.crashButton.TabIndex = 3;
			this.crashButton.Text = "Generate Exception";
			this.crashButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.crashButton.UseVisualStyleBackColor = true;
			this.crashButton.Click += new System.EventHandler(this.CrashButton_Click);
			// 
			// crashTypeComboBox
			// 
			this.crashTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.crashTypeComboBox.FormattingEnabled = true;
			this.crashTypeComboBox.Items.AddRange(new object[] {
			"UI Thread: System.Exception",
			"UI Thread: System.ArgumentException",
			"Background Thread (Task): System.Exception",
			"Process Corrupted State Exception: Access Violation"});
			this.crashTypeComboBox.Location = new System.Drawing.Point(12, 27);
			this.crashTypeComboBox.Name = "crashTypeComboBox";
			this.crashTypeComboBox.Size = new System.Drawing.Size(317, 21);
			this.crashTypeComboBox.TabIndex = 0;
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(199, 98);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 4;
			this.closeButton.Text = "Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// StackTextBox
			// 
			this.StackTextBox.Location = new System.Drawing.Point(12, 72);
			this.StackTextBox.Name = "StackTextBox";
			this.StackTextBox.Size = new System.Drawing.Size(317, 20);
			this.StackTextBox.TabIndex = 2;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(341, 132);
			this.Controls.Add(this.StackTextBox);
			this.Controls.Add(label1);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.crashTypeComboBox);
			this.Controls.Add(this.crashButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "NBug - Test Application";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button crashButton;
		private System.Windows.Forms.ComboBox crashTypeComboBox;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.TextBox StackTextBox;
	}
}

