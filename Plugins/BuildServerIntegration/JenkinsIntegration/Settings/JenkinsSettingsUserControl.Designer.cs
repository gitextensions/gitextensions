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
            if (disposing && (components is not null))
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
            lblJenkinsServerUrl = new Label();
            lblProjectName = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            JenkinsServerUrl = new TextBox();
            JenkinsProjectName = new TextBox();
            lblIgnoreBuildBranch = new Label();
            IgnoreBuildBranch = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblJenkinsServerUrl
            // 
            lblJenkinsServerUrl.Anchor = AnchorStyles.Left;
            lblJenkinsServerUrl.AutoSize = true;
            lblJenkinsServerUrl.Location = new Point(3, 7);
            lblJenkinsServerUrl.Name = "lblJenkinsServerUrl";
            lblJenkinsServerUrl.Size = new Size(98, 13);
            lblJenkinsServerUrl.TabIndex = 0;
            lblJenkinsServerUrl.Text = "Jenkins server URL";
            // 
            // lblProjectName
            // 
            lblProjectName.Anchor = AnchorStyles.Left;
            lblProjectName.AutoSize = true;
            lblProjectName.Location = new Point(3, 34);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Size = new Size(70, 13);
            lblProjectName.TabIndex = 2;
            lblProjectName.Text = "Project name";
            // 
            // lblIgnoreBuildBranch
            // 
            lblIgnoreBuildBranch.Anchor = AnchorStyles.Left;
            lblIgnoreBuildBranch.AutoSize = true;
            lblIgnoreBuildBranch.Location = new Point(3, 54);
            lblIgnoreBuildBranch.Name = "lblIgnoreBuildBranch";
            lblIgnoreBuildBranch.Size = new Size(98, 13);
            lblIgnoreBuildBranch.TabIndex = 4;
            lblIgnoreBuildBranch.Text = "Ignore build for branch";
            // 
            // JenkinsServerUrl
            // 
            JenkinsServerUrl.Dock = DockStyle.Fill;
            JenkinsServerUrl.Location = new Point(107, 3);
            JenkinsServerUrl.Name = "JenkinsServerUrl";
            JenkinsServerUrl.Size = new Size(504, 21);
            JenkinsServerUrl.TabIndex = 1;
            // 
            // JenkinsProjectName
            // 
            JenkinsProjectName.Dock = DockStyle.Fill;
            JenkinsProjectName.Location = new Point(107, 30);
            JenkinsProjectName.Name = "JenkinsProjectName";
            JenkinsProjectName.Size = new Size(504, 21);
            JenkinsProjectName.TabIndex = 3;
            // 
            // IgnoreBuildBranch
            // 
            IgnoreBuildBranch.Dock = DockStyle.Fill;
            IgnoreBuildBranch.Location = new Point(107, 3);
            IgnoreBuildBranch.Name = "IgnoreBuildBranch";
            IgnoreBuildBranch.Size = new Size(504, 21);
            IgnoreBuildBranch.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(lblJenkinsServerUrl, 0, 0);
            tableLayoutPanel1.Controls.Add(lblProjectName, 0, 1);
            tableLayoutPanel1.Controls.Add(lblIgnoreBuildBranch, 0, 2);
            tableLayoutPanel1.Controls.Add(JenkinsServerUrl, 1, 0);
            tableLayoutPanel1.Controls.Add(JenkinsProjectName, 1, 1);
            tableLayoutPanel1.Controls.Add(IgnoreBuildBranch, 1, 2);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(614, 87);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // JenkinsSettingsUserControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "JenkinsSettingsUserControl";
            Size = new Size(617, 90);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox JenkinsServerUrl;
        private TextBox JenkinsProjectName;
        private TextBox IgnoreBuildBranch;
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblJenkinsServerUrl;
        private Label lblProjectName;
        private Label lblIgnoreBuildBranch;
    }
}
