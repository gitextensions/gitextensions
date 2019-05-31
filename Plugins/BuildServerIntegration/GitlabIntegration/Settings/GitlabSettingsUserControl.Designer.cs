namespace GitlabIntegration.Settings
{
    partial class GitlabSettingsUserControl
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
            this.lblGitlabServerUrl = new System.Windows.Forms.Label();
            this.lblProjectName = new System.Windows.Forms.Label();
            this.lblRestApiTokenName = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.GitlabServerUrl = new System.Windows.Forms.TextBox();
            this.GitlabProjectName = new System.Windows.Forms.TextBox();
            this.GitlabApiToken = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblGitlabServerUrl
            // 
            this.lblGitlabServerUrl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblGitlabServerUrl.AutoSize = true;
            this.lblGitlabServerUrl.Location = new System.Drawing.Point(3, 6);
            this.lblGitlabServerUrl.Name = "lblGitlabServerUrl";
            this.lblGitlabServerUrl.Size = new System.Drawing.Size(91, 13);
            this.lblGitlabServerUrl.TabIndex = 0;
            this.lblGitlabServerUrl.Text = "Gitlab server URL";
            // 
            // lblProjectName
            // 
            this.lblProjectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProjectName.AutoSize = true;
            this.lblProjectName.Location = new System.Drawing.Point(3, 32);
            this.lblProjectName.Name = "lblProjectName";
            this.lblProjectName.Size = new System.Drawing.Size(155, 13);
            this.lblProjectName.TabIndex = 2;
            this.lblProjectName.Text = "Namespaces and Project name";
            // 
            // lblRestApiTokenName
            // 
            this.lblRestApiTokenName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRestApiTokenName.AutoSize = true;
            this.lblRestApiTokenName.Location = new System.Drawing.Point(3, 58);
            this.lblRestApiTokenName.Name = "lblRestApiTokenName";
            this.lblRestApiTokenName.Size = new System.Drawing.Size(90, 13);
            this.lblRestApiTokenName.TabIndex = 4;
            this.lblRestApiTokenName.Text = "REST API Token";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblGitlabServerUrl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblProjectName, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblRestApiTokenName, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.GitlabServerUrl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.GitlabProjectName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.GitlabApiToken, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(688, 103);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // GitlabServerUrl
            // 
            this.GitlabServerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitlabServerUrl.Location = new System.Drawing.Point(164, 3);
            this.GitlabServerUrl.Name = "GitlabServerUrl";
            this.GitlabServerUrl.Size = new System.Drawing.Size(521, 20);
            this.GitlabServerUrl.TabIndex = 1;
            // 
            // GitlabProjectName
            // 
            this.GitlabProjectName.AccessibleDescription = "Fill with comma separated projects name with full namespaces path";
            this.GitlabProjectName.AccessibleName = "";
            this.GitlabProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitlabProjectName.Location = new System.Drawing.Point(164, 29);
            this.GitlabProjectName.Name = "GitlabProjectName";
            this.GitlabProjectName.Size = new System.Drawing.Size(521, 20);
            this.GitlabProjectName.TabIndex = 3;
            // 
            // GitlabApiToken
            // 
            this.GitlabApiToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitlabApiToken.Location = new System.Drawing.Point(164, 55);
            this.GitlabApiToken.Name = "GitlabApiToken";
            this.GitlabApiToken.PasswordChar = '*';
            this.GitlabApiToken.Size = new System.Drawing.Size(521, 20);
            this.GitlabApiToken.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(164, 82);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(432, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "This is your private token, created into your Gitlab \"User Settings -> Access Tok" +
    "ens\" page";
            // 
            // GitlabSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "GitlabSettingsUserControl";
            this.Size = new System.Drawing.Size(691, 103);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox GitlabServerUrl;
        private System.Windows.Forms.TextBox GitlabProjectName;
        private System.Windows.Forms.TextBox GitlabApiToken;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblGitlabServerUrl;
        private System.Windows.Forms.Label lblProjectName;
        private System.Windows.Forms.Label lblRestApiTokenName;
        private System.Windows.Forms.Label label1;
    }
}
