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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buildServerSettingsPanel
            // 
            this.buildServerSettingsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.buildServerSettingsPanel, 2);
            this.buildServerSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buildServerSettingsPanel.Location = new System.Drawing.Point(0, 120);
            this.buildServerSettingsPanel.Margin = new System.Windows.Forms.Padding(0, 10, 3, 2);
            this.buildServerSettingsPanel.MinimumSize = new System.Drawing.Size(343, 197);
            this.buildServerSettingsPanel.Name = "buildServerSettingsPanel";
            this.buildServerSettingsPanel.Size = new System.Drawing.Size(1294, 420);
            this.buildServerSettingsPanel.TabIndex = 5;
            // 
            // BuildServerType
            // 
            this.BuildServerType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BuildServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BuildServerType.Enabled = false;
            this.BuildServerType.FormattingEnabled = true;
            this.BuildServerType.Location = new System.Drawing.Point(97, 87);
            this.BuildServerType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BuildServerType.Name = "BuildServerType";
            this.BuildServerType.Size = new System.Drawing.Size(1197, 21);
            this.BuildServerType.TabIndex = 4;
            this.BuildServerType.SelectedIndexChanged += new System.EventHandler(this.BuildServerType_SelectedIndexChanged);
            // 
            // labelBuildServerType
            // 
            this.labelBuildServerType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelBuildServerType.AutoSize = true;
            this.labelBuildServerType.Location = new System.Drawing.Point(3, 91);
            this.labelBuildServerType.Name = "labelBuildServerType";
            this.labelBuildServerType.Size = new System.Drawing.Size(88, 13);
            this.labelBuildServerType.TabIndex = 3;
            this.labelBuildServerType.Text = "Build server type";
            // 
            // checkBoxEnableBuildServerIntegration
            // 
            this.checkBoxEnableBuildServerIntegration.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxEnableBuildServerIntegration, 2);
            this.checkBoxEnableBuildServerIntegration.Enabled = false;
            this.checkBoxEnableBuildServerIntegration.Location = new System.Drawing.Point(3, 45);
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
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxShowBuildSummary, 2);
            this.checkBoxShowBuildSummary.Enabled = false;
            this.checkBoxShowBuildSummary.Location = new System.Drawing.Point(3, 66);
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
            this.tableLayoutPanel1.SetColumnSpan(this.labelBuildServerSettingsInfo, 2);
            this.labelBuildServerSettingsInfo.Location = new System.Drawing.Point(3, 0);
            this.labelBuildServerSettingsInfo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 30);
            this.labelBuildServerSettingsInfo.Name = "labelBuildServerSettingsInfo";
            this.labelBuildServerSettingsInfo.Size = new System.Drawing.Size(507, 13);
            this.labelBuildServerSettingsInfo.TabIndex = 0;
            this.labelBuildServerSettingsInfo.Text = "Git Extensions can integrate with build servers to supply per-commit Continuous I" +
    "ntegration information.";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.labelBuildServerSettingsInfo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buildServerSettingsPanel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxShowBuildSummary, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.BuildServerType, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxEnableBuildServerIntegration, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelBuildServerType, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1297, 534);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // BuildServerIntegrationSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(454, 286);
            this.Name = "BuildServerIntegrationSettingsPage";
            this.Size = new System.Drawing.Size(1297, 534);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}