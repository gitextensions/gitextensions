namespace JenkinsIntegration.Settings
{
    partial class JenkinsSettingsUserControl
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
			System.Windows.Forms.Label label13;
			System.Windows.Forms.Label label1;
			this.JenkinsServerUrl = new System.Windows.Forms.TextBox();
			this.JenkinsProjectName = new System.Windows.Forms.TextBox();
			label13 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label13
			// 
			label13.AutoSize = true;
			label13.Location = new System.Drawing.Point(3, 10);
			label13.Name = "label13";
			label13.Size = new System.Drawing.Size(98, 13);
			label13.TabIndex = 0;
			label13.Text = "Jenkins server URL";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(3, 36);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(70, 13);
			label1.TabIndex = 2;
			label1.Text = "Project name";
			// 
			// JenkinsServerUrl
			// 
			this.JenkinsServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JenkinsServerUrl.Location = new System.Drawing.Point(117, 7);
			this.JenkinsServerUrl.Name = "JenkinsServerUrl";
			this.JenkinsServerUrl.Size = new System.Drawing.Size(504, 21);
			this.JenkinsServerUrl.TabIndex = 1;
			// 
			// JenkinsProjectName
			// 
			this.JenkinsProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JenkinsProjectName.Location = new System.Drawing.Point(117, 32);
			this.JenkinsProjectName.Name = "JenkinsProjectName";
			this.JenkinsProjectName.Size = new System.Drawing.Size(504, 21);
			this.JenkinsProjectName.TabIndex = 3;
			// 
			// JenkinsSettingsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(label1);
			this.Controls.Add(label13);
			this.Controls.Add(this.JenkinsProjectName);
			this.Controls.Add(this.JenkinsServerUrl);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "JenkinsSettingsUserControl";
			this.Size = new System.Drawing.Size(631, 71);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox JenkinsServerUrl;
        private System.Windows.Forms.TextBox JenkinsProjectName;
    }
}
