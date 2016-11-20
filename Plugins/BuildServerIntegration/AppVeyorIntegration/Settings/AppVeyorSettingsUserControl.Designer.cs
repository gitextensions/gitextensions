namespace AppVeyorIntegration.Settings
{
    partial class AppVeyorSettingsUserControl
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
            System.Windows.Forms.Label lblApiToken;
            System.Windows.Forms.Label lblAccountName;
            System.Windows.Forms.Label lblProjects;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            this.AppVeyorAccountToken = new System.Windows.Forms.TextBox();
            this.AppVeyorProjectName = new System.Windows.Forms.TextBox();
            this.AppVeyorAccountName = new System.Windows.Forms.TextBox();
            this.cbLoadTestResults = new System.Windows.Forms.CheckBox();
            this.cbGitHubPullRequest = new System.Windows.Forms.CheckBox();
            this.txtGitHubToken = new System.Windows.Forms.TextBox();
            lblApiToken = new System.Windows.Forms.Label();
            lblAccountName = new System.Windows.Forms.Label();
            lblProjects = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblApiToken
            // 
            lblApiToken.AutoSize = true;
            lblApiToken.Location = new System.Drawing.Point(3, 42);
            lblApiToken.Name = "lblApiToken";
            lblApiToken.Size = new System.Drawing.Size(65, 17);
            lblApiToken.TabIndex = 0;
            lblApiToken.Text = "Api token";
            // 
            // lblAccountName
            // 
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new System.Drawing.Point(3, 12);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new System.Drawing.Size(97, 17);
            lblAccountName.TabIndex = 2;
            lblAccountName.Text = "Account name";
            // 
            // lblProjects
            // 
            lblProjects.AutoSize = true;
            lblProjects.Location = new System.Drawing.Point(3, 100);
            lblProjects.Name = "lblProjects";
            lblProjects.Size = new System.Drawing.Size(123, 17);
            lblProjects.TabIndex = 4;
            lblProjects.Text = "Project(s) Name(s)";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(133, 132);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(600, 17);
            label1.TabIndex = 4;
            label1.Text = "If you want to use the result of different projects, separate your different proj" +
    "ects names by a \'|\'.";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(133, 65);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(581, 17);
            label2.TabIndex = 4;
            label2.Text = "Token used to be able to query AppVeyor rest Api. You will find out in the user a" +
    "ccount menu.";
            // 
            // AppVeyorAccountToken
            // 
            this.AppVeyorAccountToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppVeyorAccountToken.Location = new System.Drawing.Point(136, 37);
            this.AppVeyorAccountToken.Name = "AppVeyorAccountToken";
            this.AppVeyorAccountToken.Size = new System.Drawing.Size(587, 23);
            this.AppVeyorAccountToken.TabIndex = 1;
            // 
            // AppVeyorProjectName
            // 
            this.AppVeyorProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppVeyorProjectName.Location = new System.Drawing.Point(136, 96);
            this.AppVeyorProjectName.Name = "AppVeyorProjectName";
            this.AppVeyorProjectName.Size = new System.Drawing.Size(587, 23);
            this.AppVeyorProjectName.TabIndex = 3;
            // 
            // AppVeyorAccountName
            // 
            this.AppVeyorAccountName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppVeyorAccountName.Location = new System.Drawing.Point(136, 7);
            this.AppVeyorAccountName.Name = "AppVeyorAccountName";
            this.AppVeyorAccountName.Size = new System.Drawing.Size(587, 23);
            this.AppVeyorAccountName.TabIndex = 5;
            // 
            // cbLoadTestResults
            // 
            this.cbLoadTestResults.AutoSize = true;
            this.cbLoadTestResults.Location = new System.Drawing.Point(136, 173);
            this.cbLoadTestResults.Name = "cbLoadTestResults";
            this.cbLoadTestResults.Size = new System.Drawing.Size(544, 21);
            this.cbLoadTestResults.TabIndex = 6;
            this.cbLoadTestResults.Text = "display tests results in build status summary for each build result (network inte" +
    "nsive!)";
            this.cbLoadTestResults.UseVisualStyleBackColor = true;
            // 
            // cbGitHubPullRequest
            // 
            this.cbGitHubPullRequest.AutoSize = true;
            this.cbGitHubPullRequest.Location = new System.Drawing.Point(136, 211);
            this.cbGitHubPullRequest.Name = "cbGitHubPullRequest";
            this.cbGitHubPullRequest.Size = new System.Drawing.Size(323, 21);
            this.cbGitHubPullRequest.TabIndex = 6;
            this.cbGitHubPullRequest.Text = "display github pull requests builds. GithubToken:";
            this.cbGitHubPullRequest.UseVisualStyleBackColor = true;
            this.cbGitHubPullRequest.CheckedChanged += new System.EventHandler(this.cbGitHubPullRequest_CheckedChanged);
            // 
            // txtGitHubToken
            // 
            this.txtGitHubToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGitHubToken.Location = new System.Drawing.Point(454, 210);
            this.txtGitHubToken.Name = "txtGitHubToken";
            this.txtGitHubToken.Size = new System.Drawing.Size(268, 23);
            this.txtGitHubToken.TabIndex = 3;
            // 
            // AppVeyorSettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbGitHubPullRequest);
            this.Controls.Add(this.cbLoadTestResults);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(lblProjects);
            this.Controls.Add(this.AppVeyorAccountName);
            this.Controls.Add(lblAccountName);
            this.Controls.Add(lblApiToken);
            this.Controls.Add(this.txtGitHubToken);
            this.Controls.Add(this.AppVeyorProjectName);
            this.Controls.Add(this.AppVeyorAccountToken);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AppVeyorSettingsUserControl";
            this.Size = new System.Drawing.Size(736, 302);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox AppVeyorAccountToken;
        private System.Windows.Forms.TextBox AppVeyorProjectName;
        private System.Windows.Forms.TextBox AppVeyorAccountName;
        private System.Windows.Forms.CheckBox cbLoadTestResults;
        private System.Windows.Forms.CheckBox cbGitHubPullRequest;
        private System.Windows.Forms.TextBox txtGitHubToken;
    }
}
