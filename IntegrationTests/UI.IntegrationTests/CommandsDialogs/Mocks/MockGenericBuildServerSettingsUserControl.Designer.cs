using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace GitExtensions.UITests.CommandsDialogs
{
    partial class MockGenericBuildServerSettingsUserControl
    {
        private void InitializeComponent()
        {
            System.Windows.Forms.Label lblAccountName;
            System.Windows.Forms.Label lblProjects;
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.txtAccountName = new System.Windows.Forms.TextBox();
            this.cbLoadTestResults = new System.Windows.Forms.CheckBox();
            lblAccountName = new System.Windows.Forms.Label();
            lblProjects = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblAccountName
            // 
            lblAccountName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblAccountName.AutoSize = true;
            lblAccountName.Location = new System.Drawing.Point(14, 17);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new System.Drawing.Size(85, 15);
            lblAccountName.Text = "Account name";
            // 
            // lblProjects
            // 
            lblProjects.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProjects.AutoSize = true;
            lblProjects.Location = new System.Drawing.Point(14, 51);
            lblProjects.Name = "lblProjects";
            lblProjects.Size = new System.Drawing.Size(105, 15);
            lblProjects.Text = "Project(s) Name(s)";
            // 
            // AppVeyorProjectName
            // 
            this.txtProjectName.Location = new System.Drawing.Point(125, 48);
            this.txtProjectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtProjectName.Name = "AppVeyorProjectName";
            this.txtProjectName.Size = new System.Drawing.Size(232, 23);
            // 
            // AppVeyorAccountName
            // 
            this.txtAccountName.Location = new System.Drawing.Point(125, 14);
            this.txtAccountName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtAccountName.Name = "AppVeyorAccountName";
            this.txtAccountName.Size = new System.Drawing.Size(232, 23);
            // 
            // cbLoadTestResults
            // 
            this.cbLoadTestResults.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbLoadTestResults.AutoSize = true;
            this.cbLoadTestResults.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbLoadTestResults.Location = new System.Drawing.Point(14, 86);
            this.cbLoadTestResults.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbLoadTestResults.Name = "cbLoadTestResults";
            this.cbLoadTestResults.Size = new System.Drawing.Size(123, 19);
            this.cbLoadTestResults.Text = "Display test results";
            this.cbLoadTestResults.UseVisualStyleBackColor = true;
            // 
            // AppVeyorSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.cbLoadTestResults);
            this.Controls.Add(this.txtAccountName);
            this.Controls.Add(lblAccountName);
            this.Controls.Add(lblProjects);
            this.Controls.Add(this.txtProjectName);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AppVeyorSettingsUserControl";
            this.Size = new System.Drawing.Size(744, 411);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.TextBox txtAccountName;
        private System.Windows.Forms.CheckBox cbLoadTestResults;
    }
}
