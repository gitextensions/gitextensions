namespace UITests.CommandsDialogs.SettingsDialog.Pages
{
    partial class MockGenericBuildServerSettingsUserControl
    {
        private void InitializeComponent()
        {
            Label lblAccountName;
            Label lblProjects;
            txtProjectName = new TextBox();
            txtAccountName = new TextBox();
            cbLoadTestResults = new CheckBox();
            lblAccountName = new Label();
            lblProjects = new Label();
            SuspendLayout();
            // 
            // lblAccountName
            // 
            lblAccountName.Anchor = AnchorStyles.Left;
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new Point(14, 17);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new Size(85, 15);
            lblAccountName.Text = "Account name";
            // 
            // lblProjects
            // 
            lblProjects.Anchor = AnchorStyles.Left;
            lblProjects.AutoSize = true;
            lblProjects.Location = new Point(14, 51);
            lblProjects.Name = "lblProjects";
            lblProjects.Size = new Size(105, 15);
            lblProjects.Text = "Project(s) Name(s)";
            // 
            // AppVeyorProjectName
            // 
            txtProjectName.Location = new Point(125, 48);
            txtProjectName.Margin = new Padding(3, 2, 3, 2);
            txtProjectName.Name = "AppVeyorProjectName";
            txtProjectName.Size = new Size(232, 23);
            // 
            // AppVeyorAccountName
            // 
            txtAccountName.Location = new Point(125, 14);
            txtAccountName.Margin = new Padding(3, 2, 3, 2);
            txtAccountName.Name = "AppVeyorAccountName";
            txtAccountName.Size = new Size(232, 23);
            // 
            // cbLoadTestResults
            // 
            cbLoadTestResults.Anchor = AnchorStyles.Left;
            cbLoadTestResults.AutoSize = true;
            cbLoadTestResults.CheckAlign = ContentAlignment.MiddleRight;
            cbLoadTestResults.Location = new Point(14, 86);
            cbLoadTestResults.Margin = new Padding(3, 2, 3, 2);
            cbLoadTestResults.Name = "cbLoadTestResults";
            cbLoadTestResults.Size = new Size(123, 19);
            cbLoadTestResults.Text = "Display test results";
            cbLoadTestResults.UseVisualStyleBackColor = true;
            // 
            // AppVeyorSettingsUserControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(cbLoadTestResults);
            Controls.Add(txtAccountName);
            Controls.Add(lblAccountName);
            Controls.Add(lblProjects);
            Controls.Add(txtProjectName);
            Margin = new Padding(0);
            Name = "AppVeyorSettingsUserControl";
            Size = new Size(744, 411);
            ResumeLayout(false);
            PerformLayout();
        }

        private TextBox txtProjectName;
        private TextBox txtAccountName;
        private CheckBox cbLoadTestResults;
    }
}
