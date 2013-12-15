namespace TeamCityIntegration.Settings
{
    partial class TeamCitySettingsUserControl
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
			this.TeamCityServerUrl = new System.Windows.Forms.TextBox();
			this.TeamCityProjectName = new System.Windows.Forms.TextBox();
			label13 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label13
			// 
			label13.AutoSize = true;
			label13.Location = new System.Drawing.Point(3, 10);
			label13.Name = "label13";
			label13.Size = new System.Drawing.Size(108, 13);
			label13.TabIndex = 0;
			label13.Text = "TeamCity server URL";
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
			// TeamCityServerUrl
			// 
			this.TeamCityServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TeamCityServerUrl.Location = new System.Drawing.Point(117, 7);
			this.TeamCityServerUrl.Name = "TeamCityServerUrl";
			this.TeamCityServerUrl.Size = new System.Drawing.Size(504, 21);
			this.TeamCityServerUrl.TabIndex = 1;
			// 
			// TeamCityProjectName
			// 
			this.TeamCityProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TeamCityProjectName.Location = new System.Drawing.Point(117, 32);
			this.TeamCityProjectName.Name = "TeamCityProjectName";
			this.TeamCityProjectName.Size = new System.Drawing.Size(504, 21);
			this.TeamCityProjectName.TabIndex = 3;
			// 
			// TeamCitySettingsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(label1);
			this.Controls.Add(label13);
			this.Controls.Add(this.TeamCityProjectName);
			this.Controls.Add(this.TeamCityServerUrl);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "TeamCitySettingsUserControl";
			this.Size = new System.Drawing.Size(631, 71);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TeamCityServerUrl;
        private System.Windows.Forms.TextBox TeamCityProjectName;
    }
}
