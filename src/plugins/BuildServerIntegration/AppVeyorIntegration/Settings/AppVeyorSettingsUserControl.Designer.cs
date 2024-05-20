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
            Label lblApiToken;
            Label lblAccountName;
            Label lblProjects;
            Label label1;
            Label label2;
            AppVeyorAccountToken = new TextBox();
            AppVeyorProjectName = new TextBox();
            AppVeyorAccountName = new TextBox();
            cbLoadTestResults = new CheckBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            lblApiToken = new Label();
            lblAccountName = new Label();
            lblProjects = new Label();
            label1 = new Label();
            label2 = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblApiToken
            // 
            lblApiToken.Anchor = AnchorStyles.Left;
            lblApiToken.AutoSize = true;
            lblApiToken.Location = new Point(3, 33);
            lblApiToken.Name = "lblApiToken";
            lblApiToken.Size = new Size(58, 15);
            lblApiToken.TabIndex = 0;
            lblApiToken.Text = "Api token";
            // 
            // lblAccountName
            // 
            lblAccountName.Anchor = AnchorStyles.Left;
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new Point(3, 6);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new Size(85, 15);
            lblAccountName.TabIndex = 0;
            lblAccountName.Text = "Account name";
            // 
            // lblProjects
            // 
            lblProjects.Anchor = AnchorStyles.Left;
            lblProjects.AutoSize = true;
            lblProjects.Location = new Point(3, 85);
            lblProjects.Name = "lblProjects";
            lblProjects.Size = new Size(105, 15);
            lblProjects.TabIndex = 0;
            lblProjects.Text = "Project(s) Name(s)";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(114, 106);
            label1.Margin = new Padding(3, 0, 3, 10);
            label1.Name = "label1";
            label1.Size = new Size(508, 15);
            label1.TabIndex = 0;
            label1.Text = "If you want to use the result of different projects, separate your different proj" +
    "ects names by a \'|\'.";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(114, 54);
            label2.Margin = new Padding(3, 0, 3, 10);
            label2.Name = "label2";
            label2.Size = new Size(499, 15);
            label2.TabIndex = 0;
            label2.Text = "Token used to be able to query AppVeyor rest Api. You will find out in the user a" +
    "ccount menu.";
            // 
            // AppVeyorAccountToken
            // 
            AppVeyorAccountToken.Dock = DockStyle.Fill;
            AppVeyorAccountToken.Location = new Point(114, 29);
            AppVeyorAccountToken.Margin = new Padding(3, 2, 3, 2);
            AppVeyorAccountToken.Name = "AppVeyorAccountToken";
            AppVeyorAccountToken.Size = new Size(508, 23);
            AppVeyorAccountToken.TabIndex = 2;
            // 
            // AppVeyorProjectName
            // 
            AppVeyorProjectName.Dock = DockStyle.Fill;
            AppVeyorProjectName.Location = new Point(114, 81);
            AppVeyorProjectName.Margin = new Padding(3, 2, 3, 2);
            AppVeyorProjectName.Name = "AppVeyorProjectName";
            AppVeyorProjectName.Size = new Size(508, 23);
            AppVeyorProjectName.TabIndex = 3;
            // 
            // AppVeyorAccountName
            // 
            AppVeyorAccountName.Dock = DockStyle.Fill;
            AppVeyorAccountName.Location = new Point(114, 2);
            AppVeyorAccountName.Margin = new Padding(3, 2, 3, 2);
            AppVeyorAccountName.Name = "AppVeyorAccountName";
            AppVeyorAccountName.Size = new Size(508, 23);
            AppVeyorAccountName.TabIndex = 1;
            // 
            // cbLoadTestResults
            // 
            cbLoadTestResults.Anchor = AnchorStyles.Left;
            cbLoadTestResults.AutoSize = true;
            cbLoadTestResults.Location = new Point(114, 133);
            cbLoadTestResults.Margin = new Padding(3, 2, 3, 2);
            cbLoadTestResults.Name = "cbLoadTestResults";
            cbLoadTestResults.Size = new Size(472, 19);
            cbLoadTestResults.TabIndex = 4;
            cbLoadTestResults.Text = "Display test results in build status summary for each build result (network inten" +
    "sive!)";
            cbLoadTestResults.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 6);
            tableLayoutPanel1.Controls.Add(lblAccountName, 0, 0);
            tableLayoutPanel1.Controls.Add(AppVeyorAccountName, 1, 0);
            tableLayoutPanel1.Controls.Add(cbLoadTestResults, 1, 5);
            tableLayoutPanel1.Controls.Add(lblApiToken, 0, 1);
            tableLayoutPanel1.Controls.Add(label1, 1, 4);
            tableLayoutPanel1.Controls.Add(label2, 1, 2);
            tableLayoutPanel1.Controls.Add(lblProjects, 0, 3);
            tableLayoutPanel1.Controls.Add(AppVeyorProjectName, 1, 3);
            tableLayoutPanel1.Controls.Add(AppVeyorAccountToken, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(625, 154);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Location = new Point(111, 154);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(0, 0);
            flowLayoutPanel1.TabIndex = 8;
            // 
            // AppVeyorSettingsUserControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "AppVeyorSettingsUserControl";
            Size = new Size(625, 154);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox AppVeyorAccountToken;
        private TextBox AppVeyorProjectName;
        private TextBox AppVeyorAccountName;
        private CheckBox cbLoadTestResults;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
