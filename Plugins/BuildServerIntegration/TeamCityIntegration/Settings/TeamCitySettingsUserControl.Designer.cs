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
            label13.Location = new System.Drawing.Point(3, 12);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(116, 15);
            label13.TabIndex = 0;
            label13.Text = "TeamCity server URL";
            // 
            // TeamCityServerUrl
            // 
            this.TeamCityServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityServerUrl.Location = new System.Drawing.Point(136, 8);
            this.TeamCityServerUrl.Name = "TeamCityServerUrl";
            this.TeamCityServerUrl.Size = new System.Drawing.Size(318, 23);
            this.TeamCityServerUrl.TabIndex = 1;
            // 
            // TeamCityProjectName
            // 
            this.TeamCityProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityProjectName.Location = new System.Drawing.Point(136, 37);
            this.TeamCityProjectName.Name = "TeamCityProjectName";
            this.TeamCityProjectName.Size = new System.Drawing.Size(318, 23);
            this.TeamCityProjectName.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 41);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(77, 15);
            label1.TabIndex = 2;
            label1.Text = "Project name";
            // 
            // TeamCitySettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(label1);
            this.Controls.Add(label13);
            this.Controls.Add(this.TeamCityProjectName);
            this.Controls.Add(this.TeamCityServerUrl);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TeamCitySettingsUserControl";
            this.Size = new System.Drawing.Size(467, 82);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TeamCityServerUrl;
        private System.Windows.Forms.TextBox TeamCityProjectName;
    }
}
