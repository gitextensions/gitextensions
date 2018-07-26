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
            this.txtGitHubToken = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cbGitHubPullRequest = new System.Windows.Forms.CheckBox();
            lblApiToken = new System.Windows.Forms.Label();
            lblAccountName = new System.Windows.Forms.Label();
            lblProjects = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblApiToken
            // 
            lblApiToken.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblApiToken.AutoSize = true;
            lblApiToken.Location = new System.Drawing.Point(3, 31);
            lblApiToken.Name = "lblApiToken";
            lblApiToken.Size = new System.Drawing.Size(52, 13);
            lblApiToken.TabIndex = 0;
            lblApiToken.Text = "Api token";
            // 
            // lblAccountName
            // 
            lblAccountName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new System.Drawing.Point(3, 6);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new System.Drawing.Size(75, 13);
            lblAccountName.TabIndex = 2;
            lblAccountName.Text = "Account name";
            // 
            // lblProjects
            // 
            lblProjects.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProjects.AutoSize = true;
            lblProjects.Location = new System.Drawing.Point(3, 79);
            lblProjects.Name = "lblProjects";
            lblProjects.Size = new System.Drawing.Size(97, 13);
            lblProjects.TabIndex = 4;
            lblProjects.Text = "Project(s) Name(s)";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(106, 98);
            label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(480, 13);
            label1.TabIndex = 4;
            label1.Text = "If you want to use the result of different projects, separate your different proj" +
    "ects names by a \'|\'.";
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(106, 50);
            label2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(457, 13);
            label2.TabIndex = 4;
            label2.Text = "Token used to be able to query AppVeyor rest Api. You will find out in the user a" +
    "ccount menu.";
            // 
            // AppVeyorAccountToken
            // 
            this.AppVeyorAccountToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AppVeyorAccountToken.Location = new System.Drawing.Point(106, 27);
            this.AppVeyorAccountToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppVeyorAccountToken.Name = "AppVeyorAccountToken";
            this.AppVeyorAccountToken.Size = new System.Drawing.Size(504, 21);
            this.AppVeyorAccountToken.TabIndex = 1;
            // 
            // AppVeyorProjectName
            // 
            this.AppVeyorProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AppVeyorProjectName.Location = new System.Drawing.Point(106, 75);
            this.AppVeyorProjectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppVeyorProjectName.Name = "AppVeyorProjectName";
            this.AppVeyorProjectName.Size = new System.Drawing.Size(504, 21);
            this.AppVeyorProjectName.TabIndex = 3;
            // 
            // AppVeyorAccountName
            // 
            this.AppVeyorAccountName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AppVeyorAccountName.Location = new System.Drawing.Point(106, 2);
            this.AppVeyorAccountName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppVeyorAccountName.Name = "AppVeyorAccountName";
            this.AppVeyorAccountName.Size = new System.Drawing.Size(504, 21);
            this.AppVeyorAccountName.TabIndex = 5;
            // 
            // cbLoadTestResults
            // 
            this.cbLoadTestResults.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbLoadTestResults.AutoSize = true;
            this.cbLoadTestResults.Location = new System.Drawing.Point(106, 123);
            this.cbLoadTestResults.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbLoadTestResults.Name = "cbLoadTestResults";
            this.cbLoadTestResults.Size = new System.Drawing.Size(434, 17);
            this.cbLoadTestResults.TabIndex = 6;
            this.cbLoadTestResults.Text = "display tests results in build status summary for each build result (network inte" +
    "nsive!)";
            this.cbLoadTestResults.UseVisualStyleBackColor = true;
            // 
            // txtGitHubToken
            // 
            this.txtGitHubToken.Location = new System.Drawing.Point(266, 2);
            this.txtGitHubToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtGitHubToken.Name = "txtGitHubToken";
            this.txtGitHubToken.Size = new System.Drawing.Size(241, 21);
            this.txtGitHubToken.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 6);
            this.tableLayoutPanel1.Controls.Add(lblAccountName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.AppVeyorAccountName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbLoadTestResults, 1, 5);
            this.tableLayoutPanel1.Controls.Add(lblApiToken, 0, 1);
            this.tableLayoutPanel1.Controls.Add(label1, 1, 4);
            this.tableLayoutPanel1.Controls.Add(label2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(lblProjects, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.AppVeyorProjectName, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.AppVeyorAccountToken, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(613, 167);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.cbGitHubPullRequest);
            this.flowLayoutPanel1.Controls.Add(this.txtGitHubToken);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(103, 142);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(510, 25);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // cbGitHubPullRequest
            // 
            this.cbGitHubPullRequest.AutoSize = true;
            this.cbGitHubPullRequest.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbGitHubPullRequest.Location = new System.Drawing.Point(3, 2);
            this.cbGitHubPullRequest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbGitHubPullRequest.Name = "cbGitHubPullRequest";
            this.cbGitHubPullRequest.Size = new System.Drawing.Size(257, 21);
            this.cbGitHubPullRequest.TabIndex = 6;
            this.cbGitHubPullRequest.Text = "display GitHub pull requests builds. GitHubToken:";
            this.cbGitHubPullRequest.UseVisualStyleBackColor = true;
            this.cbGitHubPullRequest.CheckedChanged += new System.EventHandler(this.cbGitHubPullRequest_CheckedChanged);
            // 
            // AppVeyorSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AppVeyorSettingsUserControl";
            this.Size = new System.Drawing.Size(613, 167);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox AppVeyorAccountToken;
        private System.Windows.Forms.TextBox AppVeyorProjectName;
        private System.Windows.Forms.TextBox AppVeyorAccountName;
        private System.Windows.Forms.CheckBox cbLoadTestResults;
        private System.Windows.Forms.TextBox txtGitHubToken;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox cbGitHubPullRequest;
    }
}
