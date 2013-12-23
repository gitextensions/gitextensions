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
            System.Windows.Forms.Label labelBuildFilter;
            System.Windows.Forms.Label labelProjectNameComment;
            System.Windows.Forms.Label labelBuildIdFilter;
            this.TeamCityServerUrl = new System.Windows.Forms.TextBox();
            this.TeamCityProjectName = new System.Windows.Forms.TextBox();
            this.TeamCityBuildIdFilter = new System.Windows.Forms.TextBox();
            label13 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            labelBuildFilter = new System.Windows.Forms.Label();
            labelProjectNameComment = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 11);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(116, 15);
            label13.TabIndex = 0;
            label13.Text = "TeamCity server URL";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 40);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(77, 15);
            label1.TabIndex = 2;
            label1.Text = "Project name";
            // 
            // labelBuildFilter
            // 
            labelBuildFilter.AutoSize = true;
            labelBuildFilter.Location = new System.Drawing.Point(3, 90);
            labelBuildFilter.Name = "labelBuildFilter";
            labelBuildFilter.Size = new System.Drawing.Size(76, 15);
            labelBuildFilter.TabIndex = 4;
            labelBuildFilter.Text = "Build Id Filter";
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
            // TeamCityBuildIdFilter
            // 
            this.TeamCityBuildIdFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityBuildIdFilter.Location = new System.Drawing.Point(136, 87);
            this.TeamCityBuildIdFilter.Name = "TeamCityBuildIdFilter";
            this.TeamCityBuildIdFilter.Size = new System.Drawing.Size(318, 23);
            this.TeamCityBuildIdFilter.TabIndex = 5;
            // 
            // labelProjectNameComment
            // 
            labelProjectNameComment.AutoSize = true;
            labelProjectNameComment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelProjectNameComment.Location = new System.Drawing.Point(133, 63);
            labelProjectNameComment.Name = "labelProjectNameComment";
            labelProjectNameComment.Size = new System.Drawing.Size(173, 15);
            labelProjectNameComment.TabIndex = 6;
            labelProjectNameComment.Text = "Several names splitted by | char";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelBuildIdFilter.Location = new System.Drawing.Point(133, 113);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(45, 15);
            labelBuildIdFilter.TabIndex = 7;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // TeamCitySettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(labelBuildIdFilter);
            this.Controls.Add(labelProjectNameComment);
            this.Controls.Add(labelBuildFilter);
            this.Controls.Add(this.TeamCityBuildIdFilter);
            this.Controls.Add(label1);
            this.Controls.Add(label13);
            this.Controls.Add(this.TeamCityProjectName);
            this.Controls.Add(this.TeamCityServerUrl);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TeamCitySettingsUserControl";
            this.Size = new System.Drawing.Size(467, 136);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TeamCityServerUrl;
        private System.Windows.Forms.TextBox TeamCityProjectName;
        private System.Windows.Forms.TextBox TeamCityBuildIdFilter;
    }
}
