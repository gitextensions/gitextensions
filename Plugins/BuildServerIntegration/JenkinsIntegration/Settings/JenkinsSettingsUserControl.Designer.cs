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
            this.lblJenkinsServerUrl = new System.Windows.Forms.Label();
            this.lblProjectName = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.JenkinsServerUrl = new System.Windows.Forms.TextBox();
            this.JenkinsProjectName = new System.Windows.Forms.TextBox();
            this.lblIgnoreBuildBranch = new System.Windows.Forms.Label();
            this.IgnoreBuildBranch = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblJenkinsServerUrl
            // 
            this.lblJenkinsServerUrl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblJenkinsServerUrl.AutoSize = true;
            this.lblJenkinsServerUrl.Location = new System.Drawing.Point(3, 7);
            this.lblJenkinsServerUrl.Name = "lblJenkinsServerUrl";
            this.lblJenkinsServerUrl.Size = new System.Drawing.Size(98, 13);
            this.lblJenkinsServerUrl.TabIndex = 0;
            this.lblJenkinsServerUrl.Text = "Jenkins server URL";
            // 
            // lblProjectName
            // 
            this.lblProjectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProjectName.AutoSize = true;
            this.lblProjectName.Location = new System.Drawing.Point(3, 34);
            this.lblProjectName.Name = "lblProjectName";
            this.lblProjectName.Size = new System.Drawing.Size(70, 13);
            this.lblProjectName.TabIndex = 2;
            this.lblProjectName.Text = "Project name";
            // 
            // lblIgnoreBuildBranch
            // 
            this.lblIgnoreBuildBranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblIgnoreBuildBranch.AutoSize = true;
            this.lblIgnoreBuildBranch.Location = new System.Drawing.Point(3, 54);
            this.lblIgnoreBuildBranch.Name = "lblIgnoreBuildBranch";
            this.lblIgnoreBuildBranch.Size = new System.Drawing.Size(98, 13);
            this.lblIgnoreBuildBranch.TabIndex = 4;
            this.lblIgnoreBuildBranch.Text = "Ignore build for branch";
            // 
            // JenkinsServerUrl
            // 
            this.JenkinsServerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.JenkinsServerUrl.Location = new System.Drawing.Point(107, 3);
            this.JenkinsServerUrl.Name = "JenkinsServerUrl";
            this.JenkinsServerUrl.Size = new System.Drawing.Size(504, 21);
            this.JenkinsServerUrl.TabIndex = 1;
            // 
            // JenkinsProjectName
            // 
            this.JenkinsProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.JenkinsProjectName.Location = new System.Drawing.Point(107, 30);
            this.JenkinsProjectName.Name = "JenkinsProjectName";
            this.JenkinsProjectName.Size = new System.Drawing.Size(504, 21);
            this.JenkinsProjectName.TabIndex = 3;
            // 
            // IgnoreBuildBranch
            // 
            this.IgnoreBuildBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IgnoreBuildBranch.Location = new System.Drawing.Point(107, 3);
            this.IgnoreBuildBranch.Name = "IgnoreBuildBranch";
            this.IgnoreBuildBranch.Size = new System.Drawing.Size(504, 21);
            this.IgnoreBuildBranch.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblJenkinsServerUrl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblProjectName, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblIgnoreBuildBranch, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.JenkinsServerUrl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.JenkinsProjectName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.IgnoreBuildBranch, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(614, 87);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // JenkinsSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "JenkinsSettingsUserControl";
            this.Size = new System.Drawing.Size(617, 90);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox JenkinsServerUrl;
        private System.Windows.Forms.TextBox JenkinsProjectName;
        private System.Windows.Forms.TextBox IgnoreBuildBranch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblJenkinsServerUrl;
        private System.Windows.Forms.Label lblProjectName;
        private System.Windows.Forms.Label lblIgnoreBuildBranch;
    }
}
