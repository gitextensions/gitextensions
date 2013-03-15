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
            System.Windows.Forms.Label label50;
            this.buildServerSettingsPanel = new System.Windows.Forms.Panel();
            this.BuildServerType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            label50 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label50
            // 
            label50.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label50.Location = new System.Drawing.Point(10, 12);
            label50.Name = "label50";
            label50.Size = new System.Drawing.Size(493, 47);
            label50.TabIndex = 0;
            label50.Text = "Git Extensions can integrate with build servers to supply per-commit Continuous I" +
    "ntegration information.\r\nSet the build server information below:";
            // 
            // buildServerSettingsPanel
            // 
            this.buildServerSettingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buildServerSettingsPanel.Location = new System.Drawing.Point(13, 91);
            this.buildServerSettingsPanel.MinimumSize = new System.Drawing.Size(400, 200);
            this.buildServerSettingsPanel.Name = "buildServerSettingsPanel";
            this.buildServerSettingsPanel.Size = new System.Drawing.Size(490, 200);
            this.buildServerSettingsPanel.TabIndex = 3;
            // 
            // BuildServerType
            // 
            this.BuildServerType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BuildServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BuildServerType.FormattingEnabled = true;
            this.BuildServerType.Location = new System.Drawing.Point(112, 62);
            this.BuildServerType.Name = "BuildServerType";
            this.BuildServerType.Size = new System.Drawing.Size(230, 23);
            this.BuildServerType.TabIndex = 2;
            this.BuildServerType.SelectedIndexChanged += new System.EventHandler(this.BuildServerType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Build server type";
            // 
            // BuildServerIntegrationSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buildServerSettingsPanel);
            this.Controls.Add(this.BuildServerType);
            this.Controls.Add(this.label1);
            this.Controls.Add(label50);
            this.MinimumSize = new System.Drawing.Size(530, 330);
            this.Name = "BuildServerIntegrationSettingsPage";
            this.Size = new System.Drawing.Size(530, 330);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel buildServerSettingsPanel;
        private System.Windows.Forms.ComboBox BuildServerType;
        private System.Windows.Forms.Label label1;

    }
}