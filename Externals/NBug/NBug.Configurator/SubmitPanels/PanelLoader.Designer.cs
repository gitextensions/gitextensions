namespace NBug.Configurator.SubmitPanels
{
	partial class PanelLoader
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
            this.selectorPanel = new System.Windows.Forms.Panel();
            this.btnRemove = new System.Windows.Forms.Button();
            this.submitComboBox = new System.Windows.Forms.ComboBox();
            this.submitLabel = new System.Windows.Forms.Label();
            this.selectorPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectorPanel
            // 
            this.selectorPanel.Controls.Add(this.btnRemove);
            this.selectorPanel.Controls.Add(this.submitComboBox);
            this.selectorPanel.Controls.Add(this.submitLabel);
            this.selectorPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.selectorPanel.Location = new System.Drawing.Point(10, 0);
            this.selectorPanel.Name = "selectorPanel";
            this.selectorPanel.Size = new System.Drawing.Size(650, 45);
            this.selectorPanel.TabIndex = 2;
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(244, 7);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // submitComboBox
            // 
            this.submitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.submitComboBox.FormattingEnabled = true;
            this.submitComboBox.Items.AddRange(new object[] {
            "None",
            "E-Mail",
            "Redmine Issue Tracker",
            "FTP",
            "HTTP",
            "Custom",
            "Mantis Bug Tracker"});
            this.submitComboBox.Location = new System.Drawing.Point(72, 9);
            this.submitComboBox.Name = "submitComboBox";
            this.submitComboBox.Size = new System.Drawing.Size(166, 21);
            this.submitComboBox.TabIndex = 3;
            this.submitComboBox.SelectedIndexChanged += new System.EventHandler(this.SubmitComboBox_SelectedIndexChanged);
            // 
            // submitLabel
            // 
            this.submitLabel.AutoSize = true;
            this.submitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.submitLabel.Location = new System.Drawing.Point(-2, 12);
            this.submitLabel.Name = "submitLabel";
            this.submitLabel.Size = new System.Drawing.Size(68, 13);
            this.submitLabel.TabIndex = 2;
            this.submitLabel.Text = "Submit To:";
            // 
            // PanelLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.Controls.Add(this.selectorPanel);
            this.Name = "PanelLoader";
            this.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Size = new System.Drawing.Size(660, 294);
            this.selectorPanel.ResumeLayout(false);
            this.selectorPanel.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel selectorPanel;
		private System.Windows.Forms.ComboBox submitComboBox;
		private System.Windows.Forms.Label submitLabel;
        private System.Windows.Forms.Button btnRemove;



	}
}
