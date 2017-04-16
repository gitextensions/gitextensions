namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class BuildServerIntegrationSettingsPage
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buildServerSettingsPanel = new System.Windows.Forms.Panel();
            this.BuildServerType = new System.Windows.Forms.ComboBox();
            this.labelBuildServerType = new System.Windows.Forms.Label();
            this.checkBoxEnableBuildServerIntegration = new System.Windows.Forms.CheckBox();
            this.checkBoxShowBuildSummary = new System.Windows.Forms.CheckBox();
            this.labelBuildServerSettingsInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buildServerSettingsPanel
            // 
            this.buildServerSettingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buildServerSettingsPanel.Location = new System.Drawing.Point(14, 125);
            this.buildServerSettingsPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buildServerSettingsPanel.MinimumSize = new System.Drawing.Size(343, 197);
            this.buildServerSettingsPanel.Name = "buildServerSettingsPanel";
            this.buildServerSettingsPanel.Size = new System.Drawing.Size(1365, 197);
            this.buildServerSettingsPanel.TabIndex = 5;
            // 
            // BuildServerType
            // 
            this.BuildServerType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BuildServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BuildServerType.Enabled = false;
            this.BuildServerType.FormattingEnabled = true;
            this.BuildServerType.Location = new System.Drawing.Point(105, 100);
            this.BuildServerType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BuildServerType.Name = "BuildServerType";
            this.BuildServerType.Size = new System.Drawing.Size(1274, 21);
            this.BuildServerType.TabIndex = 4;
            this.BuildServerType.SelectedIndexChanged += new System.EventHandler(this.BuildServerType_SelectedIndexChanged);
            // 
            // labelBuildServerType
            // 
            this.labelBuildServerType.AutoSize = true;
            this.labelBuildServerType.Location = new System.Drawing.Point(11, 102);
            this.labelBuildServerType.Name = "labelBuildServerType";
            this.labelBuildServerType.Size = new System.Drawing.Size(88, 13);
            this.labelBuildServerType.TabIndex = 3;
            this.labelBuildServerType.Text = "Build server type";
            // 
            // checkBoxEnableBuildServerIntegration
            // 
            this.checkBoxEnableBuildServerIntegration.AutoSize = true;
            this.checkBoxEnableBuildServerIntegration.Enabled = false;
            this.checkBoxEnableBuildServerIntegration.Location = new System.Drawing.Point(11, 56);
            this.checkBoxEnableBuildServerIntegration.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxEnableBuildServerIntegration.Name = "checkBoxEnableBuildServerIntegration";
            this.checkBoxEnableBuildServerIntegration.Size = new System.Drawing.Size(172, 17);
            this.checkBoxEnableBuildServerIntegration.TabIndex = 1;
            this.checkBoxEnableBuildServerIntegration.Text = "Enable build server integration";
            this.checkBoxEnableBuildServerIntegration.ThreeState = true;
            this.checkBoxEnableBuildServerIntegration.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowBuildSummary
            // 
            this.checkBoxShowBuildSummary.AutoSize = true;
            this.checkBoxShowBuildSummary.Enabled = false;
            this.checkBoxShowBuildSummary.Location = new System.Drawing.Point(11, 78);
            this.checkBoxShowBuildSummary.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxShowBuildSummary.Name = "checkBoxShowBuildSummary";
            this.checkBoxShowBuildSummary.Size = new System.Drawing.Size(224, 17);
            this.checkBoxShowBuildSummary.TabIndex = 2;
            this.checkBoxShowBuildSummary.Text = "Show build status summary in revision log";
            this.checkBoxShowBuildSummary.ThreeState = true;
            this.checkBoxShowBuildSummary.UseVisualStyleBackColor = true;
            // 
            // labelBuildServerSettingsInfo
            // 
            this.labelBuildServerSettingsInfo.AutoSize = true;
            this.labelBuildServerSettingsInfo.Location = new System.Drawing.Point(9, 15);
            this.labelBuildServerSettingsInfo.Name = "labelBuildServerSettingsInfo";
            this.labelBuildServerSettingsInfo.Size = new System.Drawing.Size(507, 13);
            this.labelBuildServerSettingsInfo.TabIndex = 0;
            this.labelBuildServerSettingsInfo.Text = "Git Extensions can integrate with build servers to supply per-commit Continuous I" +
    "ntegration information.";
            // 
            // BuildServerIntegrationSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.labelBuildServerSettingsInfo);
            this.Controls.Add(this.checkBoxShowBuildSummary);
            this.Controls.Add(this.checkBoxEnableBuildServerIntegration);
            this.Controls.Add(this.buildServerSettingsPanel);
            this.Controls.Add(this.BuildServerType);
            this.Controls.Add(this.labelBuildServerType);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(454, 286);
            this.Name = "BuildServerIntegrationSettingsPage";
            this.Size = new System.Drawing.Size(1109, 461);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel buildServerSettingsPanel;
        private System.Windows.Forms.ComboBox BuildServerType;
        private System.Windows.Forms.Label labelBuildServerType;
        private System.Windows.Forms.CheckBox checkBoxEnableBuildServerIntegration;
        private System.Windows.Forms.CheckBox checkBoxShowBuildSummary;
        private System.Windows.Forms.Label labelBuildServerSettingsInfo;

    }
}