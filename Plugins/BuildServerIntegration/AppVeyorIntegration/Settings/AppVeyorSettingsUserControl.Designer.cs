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
            lblApiToken.Location = new System.Drawing.Point(3, 34);
            lblApiToken.Name = "lblApiToken";
            lblApiToken.Size = new System.Drawing.Size(52, 13);
            lblApiToken.TabIndex = 0;
            lblApiToken.Text = "Api token";
            // 
            // lblAccountName
            // 
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new System.Drawing.Point(3, 10);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new System.Drawing.Size(75, 13);
            lblAccountName.TabIndex = 2;
            lblAccountName.Text = "Account name";
            // 
            // lblProjects
            // 
            lblProjects.AutoSize = true;
            lblProjects.Location = new System.Drawing.Point(3, 81);
            lblProjects.Name = "lblProjects";
            lblProjects.Size = new System.Drawing.Size(97, 13);
            lblProjects.TabIndex = 4;
            lblProjects.Text = "Project(s) Name(s)";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(114, 107);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(480, 13);
            label1.TabIndex = 4;
            label1.Text = "If you want to use the result of different projects, separate your different proj" +
    "ects names by a \'|\'.";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(114, 53);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(457, 13);
            label2.TabIndex = 4;
            label2.Text = "Token used to be able to query AppVeyor rest Api. You will find out in the user a" +
    "ccount menu.";
            // 
            // AppVeyorAccountToken
            // 
            this.AppVeyorAccountToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppVeyorAccountToken.Location = new System.Drawing.Point(117, 30);
            this.AppVeyorAccountToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppVeyorAccountToken.Name = "AppVeyorAccountToken";
            this.AppVeyorAccountToken.Size = new System.Drawing.Size(504, 21);
            this.AppVeyorAccountToken.TabIndex = 1;
            // 
            // AppVeyorProjectName
            // 
            this.AppVeyorProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppVeyorProjectName.Location = new System.Drawing.Point(117, 78);
            this.AppVeyorProjectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppVeyorProjectName.Name = "AppVeyorProjectName";
            this.AppVeyorProjectName.Size = new System.Drawing.Size(504, 21);
            this.AppVeyorProjectName.TabIndex = 3;
            // 
            // AppVeyorAccountName
            // 
            this.AppVeyorAccountName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppVeyorAccountName.Location = new System.Drawing.Point(117, 6);
            this.AppVeyorAccountName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppVeyorAccountName.Name = "AppVeyorAccountName";
            this.AppVeyorAccountName.Size = new System.Drawing.Size(504, 21);
            this.AppVeyorAccountName.TabIndex = 5;
            // 
            // cbLoadTestResults
            // 
            this.cbLoadTestResults.AutoSize = true;
            this.cbLoadTestResults.Location = new System.Drawing.Point(117, 141);
            this.cbLoadTestResults.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbLoadTestResults.Name = "cbLoadTestResults";
            this.cbLoadTestResults.Size = new System.Drawing.Size(434, 17);
            this.cbLoadTestResults.TabIndex = 6;
            this.cbLoadTestResults.Text = "display tests results in build status summary for each build result (network inte" +
    "nsive!)";
            this.cbLoadTestResults.UseVisualStyleBackColor = true;
            // 
            // cbGitHubPullRequest
            // 
            this.cbGitHubPullRequest.AutoSize = true;
            this.cbGitHubPullRequest.Location = new System.Drawing.Point(117, 171);
            this.cbGitHubPullRequest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbGitHubPullRequest.Name = "cbGitHubPullRequest";
            this.cbGitHubPullRequest.Size = new System.Drawing.Size(257, 17);
            this.cbGitHubPullRequest.TabIndex = 6;
            this.cbGitHubPullRequest.Text = "display github pull requests builds. GithubToken:";
            this.cbGitHubPullRequest.UseVisualStyleBackColor = true;
            this.cbGitHubPullRequest.CheckedChanged += new System.EventHandler(this.cbGitHubPullRequest_CheckedChanged);
            // 
            // txtGitHubToken
            // 
            this.txtGitHubToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGitHubToken.Location = new System.Drawing.Point(389, 171);
            this.txtGitHubToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtGitHubToken.Name = "txtGitHubToken";
            this.txtGitHubToken.Size = new System.Drawing.Size(230, 21);
            this.txtGitHubToken.TabIndex = 3;
            // 
            // AppVeyorSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
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
            this.Size = new System.Drawing.Size(631, 245);
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
