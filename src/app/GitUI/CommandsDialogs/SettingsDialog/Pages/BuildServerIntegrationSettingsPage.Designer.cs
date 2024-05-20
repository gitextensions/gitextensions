namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class BuildServerIntegrationSettingsPage
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components is not null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            buildServerSettingsPanel = new Panel();
            BuildServerType = new ComboBox();
            labelBuildServerType = new Label();
            checkBoxEnableBuildServerIntegration = new CheckBox();
            labelBuildServerSettingsInfo = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            checkBoxShowBuildResultPage = new CheckBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // buildServerSettingsPanel
            // 
            buildServerSettingsPanel.AutoSize = true;
            buildServerSettingsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(buildServerSettingsPanel, 2);
            buildServerSettingsPanel.Dock = DockStyle.Fill;
            buildServerSettingsPanel.Location = new Point(0, 120);
            buildServerSettingsPanel.Margin = new Padding(0, 10, 3, 2);
            buildServerSettingsPanel.MinimumSize = new Size(343, 197);
            buildServerSettingsPanel.Name = "buildServerSettingsPanel";
            buildServerSettingsPanel.Size = new Size(1549, 197);
            buildServerSettingsPanel.TabIndex = 5;
            // 
            // BuildServerType
            // 
            BuildServerType.Dock = DockStyle.Fill;
            BuildServerType.DropDownStyle = ComboBoxStyle.DropDownList;
            BuildServerType.Enabled = false;
            BuildServerType.FormattingEnabled = true;
            BuildServerType.Location = new Point(94, 87);
            BuildServerType.Margin = new Padding(3, 2, 3, 2);
            BuildServerType.Name = "BuildServerType";
            BuildServerType.Size = new Size(1455, 21);
            BuildServerType.TabIndex = 3;
            BuildServerType.SelectedIndexChanged += BuildServerType_SelectedIndexChanged;
            // 
            // labelBuildServerType
            // 
            labelBuildServerType.Anchor = AnchorStyles.Left;
            labelBuildServerType.AutoSize = true;
            labelBuildServerType.Location = new Point(3, 91);
            labelBuildServerType.Name = "labelBuildServerType";
            labelBuildServerType.Size = new Size(85, 13);
            labelBuildServerType.TabIndex = 3;
            labelBuildServerType.Text = "Build server type";
            // 
            // checkBoxEnableBuildServerIntegration
            // 
            checkBoxEnableBuildServerIntegration.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(checkBoxEnableBuildServerIntegration, 2);
            checkBoxEnableBuildServerIntegration.Enabled = false;
            checkBoxEnableBuildServerIntegration.Location = new Point(3, 45);
            checkBoxEnableBuildServerIntegration.Margin = new Padding(3, 2, 3, 2);
            checkBoxEnableBuildServerIntegration.Name = "checkBoxEnableBuildServerIntegration";
            checkBoxEnableBuildServerIntegration.Size = new Size(168, 17);
            checkBoxEnableBuildServerIntegration.TabIndex = 1;
            checkBoxEnableBuildServerIntegration.Text = "Enable build server integration";
            checkBoxEnableBuildServerIntegration.ThreeState = true;
            checkBoxEnableBuildServerIntegration.UseVisualStyleBackColor = true;
            // 
            // labelBuildServerSettingsInfo
            // 
            labelBuildServerSettingsInfo.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(labelBuildServerSettingsInfo, 2);
            labelBuildServerSettingsInfo.Location = new Point(3, 0);
            labelBuildServerSettingsInfo.Margin = new Padding(3, 0, 3, 30);
            labelBuildServerSettingsInfo.Name = "labelBuildServerSettingsInfo";
            labelBuildServerSettingsInfo.Size = new Size(488, 13);
            labelBuildServerSettingsInfo.TabIndex = 0;
            labelBuildServerSettingsInfo.Text = "Git Extensions can integrate with build servers to supply per-commit Continuous I" +
    "ntegration information.";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(labelBuildServerSettingsInfo, 0, 0);
            tableLayoutPanel1.Controls.Add(checkBoxEnableBuildServerIntegration, 0, 1);
            tableLayoutPanel1.Controls.Add(checkBoxShowBuildResultPage, 1, 1);
            tableLayoutPanel1.Controls.Add(labelBuildServerType, 0, 2);
            tableLayoutPanel1.Controls.Add(buildServerSettingsPanel, 0, 3);
            tableLayoutPanel1.Controls.Add(BuildServerType, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1552, 894);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // checkBoxShowBuildResultPage
            // 
            checkBoxShowBuildResultPage.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(checkBoxShowBuildResultPage, 2);
            checkBoxShowBuildResultPage.Enabled = false;
            checkBoxShowBuildResultPage.Location = new Point(3, 66);
            checkBoxShowBuildResultPage.Margin = new Padding(3, 2, 3, 2);
            checkBoxShowBuildResultPage.Name = "checkBoxShowBuildResultPage";
            checkBoxShowBuildResultPage.Size = new Size(133, 17);
            checkBoxShowBuildResultPage.TabIndex = 2;
            checkBoxShowBuildResultPage.Text = "Show build result page";
            checkBoxShowBuildResultPage.ThreeState = true;
            checkBoxShowBuildResultPage.UseVisualStyleBackColor = true;
            // 
            // BuildServerIntegrationSettingsPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(454, 286);
            Name = "BuildServerIntegrationSettingsPage";
            Size = new Size(1552, 894);
            Text = "Build server integration";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Panel buildServerSettingsPanel;
        private ComboBox BuildServerType;
        private Label labelBuildServerType;
        private CheckBox checkBoxEnableBuildServerIntegration;
        private Label labelBuildServerSettingsInfo;
        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox checkBoxShowBuildResultPage;
    }
}
