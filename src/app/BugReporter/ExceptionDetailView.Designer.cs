namespace BugReporter
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
            if (disposing && (components is not null))
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
            propertyLabel = new Label();
            propertyTextBox = new TextBox();
            propertyInformationTextBox = new TextBox();
            topPanel = new Panel();
            statusStrip = new StatusStrip();
            SuspendLayout();
            // 
            // propertyLabel
            // 
            propertyLabel.AutoSize = true;
            propertyLabel.Location = new Point(2, 8);
            propertyLabel.Name = "propertyLabel";
            propertyLabel.Size = new Size(49, 13);
            propertyLabel.TabIndex = 0;
            propertyLabel.Text = "Property:";
            // 
            // propertyTextBox
            // 
            propertyTextBox.Location = new Point(57, 5);
            propertyTextBox.Name = "propertyTextBox";
            propertyTextBox.ReadOnly = true;
            propertyTextBox.Size = new Size(507, 20);
            propertyTextBox.TabIndex = 2;
            // 
            // propertyInformationTextBox
            // 
            propertyInformationTextBox.Dock = DockStyle.Fill;
            propertyInformationTextBox.Location = new Point(0, 32);
            propertyInformationTextBox.Multiline = true;
            propertyInformationTextBox.Name = "propertyInformationTextBox";
            propertyInformationTextBox.ReadOnly = true;
            propertyInformationTextBox.ScrollBars = ScrollBars.Both;
            propertyInformationTextBox.Size = new Size(567, 169);
            propertyInformationTextBox.TabIndex = 3;
            propertyInformationTextBox.WordWrap = false;
            // 
            // topPanel
            // 
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(567, 32);
            topPanel.TabIndex = 4;
            // 
            // statusStrip
            // 
            statusStrip.AutoSize = false;
            statusStrip.Location = new Point(0, 201);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(567, 17);
            statusStrip.TabIndex = 5;
            // 
            // ExceptionDetailView
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(567, 218);
            Controls.Add(propertyInformationTextBox);
            Controls.Add(statusStrip);
            Controls.Add(propertyTextBox);
            Controls.Add(propertyLabel);
            Controls.Add(topPanel);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "ExceptionDetailView";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Property Details";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label propertyLabel;
        private TextBox propertyTextBox;
        private TextBox propertyInformationTextBox;
        private Panel topPanel;
        private StatusStrip statusStrip;
    }
}