namespace GitUI.NBugReports
{
    partial class ExceptionDetailView
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
            this.propertyLabel = new System.Windows.Forms.Label();
            this.propertyTextBox = new System.Windows.Forms.TextBox();
            this.propertyInformationTextBox = new System.Windows.Forms.TextBox();
            this.topPanel = new System.Windows.Forms.Panel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.SuspendLayout();
            // 
            // propertyLabel
            // 
            this.propertyLabel.AutoSize = true;
            this.propertyLabel.Location = new System.Drawing.Point(2, 8);
            this.propertyLabel.Name = "propertyLabel";
            this.propertyLabel.Size = new System.Drawing.Size(49, 13);
            this.propertyLabel.TabIndex = 0;
            this.propertyLabel.Text = "Property:";
            // 
            // propertyTextBox
            // 
            this.propertyTextBox.Location = new System.Drawing.Point(57, 5);
            this.propertyTextBox.Name = "propertyTextBox";
            this.propertyTextBox.ReadOnly = true;
            this.propertyTextBox.Size = new System.Drawing.Size(507, 20);
            this.propertyTextBox.TabIndex = 2;
            // 
            // propertyInformationTextBox
            // 
            this.propertyInformationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyInformationTextBox.Location = new System.Drawing.Point(0, 32);
            this.propertyInformationTextBox.Multiline = true;
            this.propertyInformationTextBox.Name = "propertyInformationTextBox";
            this.propertyInformationTextBox.ReadOnly = true;
            this.propertyInformationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.propertyInformationTextBox.Size = new System.Drawing.Size(567, 169);
            this.propertyInformationTextBox.TabIndex = 3;
            this.propertyInformationTextBox.WordWrap = false;
            // 
            // topPanel
            // 
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(567, 32);
            this.topPanel.TabIndex = 4;
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.Location = new System.Drawing.Point(0, 201);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(567, 17);
            this.statusStrip.TabIndex = 5;
            // 
            // ExceptionDetailView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 218);
            this.Controls.Add(this.propertyInformationTextBox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.propertyTextBox);
            this.Controls.Add(this.propertyLabel);
            this.Controls.Add(this.topPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ExceptionDetailView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Property Details";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label propertyLabel;
        private System.Windows.Forms.TextBox propertyTextBox;
        private System.Windows.Forms.TextBox propertyInformationTextBox;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
    }
}