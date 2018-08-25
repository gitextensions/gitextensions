using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    partial class SettingsPageHeader : GitExtensionsControl
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
            this.HeaderPanel = new System.Windows.Forms.TableLayoutPanel();
            this.linePanel = new System.Windows.Forms.Panel();
            this.labelSettingsSource = new System.Windows.Forms.Label();
            this.flowLayoutPanelSettingsSource = new System.Windows.Forms.FlowLayoutPanel();
            this.DistributedRB = new System.Windows.Forms.RadioButton();
            this.arrow3 = new System.Windows.Forms.Label();
            this.arrow2 = new System.Windows.Forms.Label();
            this.LocalRB = new System.Windows.Forms.RadioButton();
            this.GlobalRB = new System.Windows.Forms.RadioButton();
            this.EffectiveRB = new System.Windows.Forms.RadioButton();
            this.arrow1 = new System.Windows.Forms.Label();
            this.settingsPagePanel = new System.Windows.Forms.Panel();
            this.HeaderPanel.SuspendLayout();
            this.flowLayoutPanelSettingsSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.AutoSize = true;
            this.HeaderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HeaderPanel.ColumnCount = 1;
            this.HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.HeaderPanel.Controls.Add(this.linePanel, 0, 1);
            this.HeaderPanel.Controls.Add(this.flowLayoutPanelSettingsSource, 0, 0);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 4);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.RowCount = 2;
            this.HeaderPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.HeaderPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.HeaderPanel.Size = new System.Drawing.Size(930, 73);
            this.HeaderPanel.TabIndex = 0;
            // 
            // labelSettingsSource
            // 
            this.linePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linePanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.linePanel.Location = new System.Drawing.Point(3, 45);
            this.linePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 17);
            this.linePanel.Name = "linePanel";
            this.linePanel.Size = new System.Drawing.Size(924, 4);
            this.linePanel.TabIndex = 10;
            // 
            // label1
            // 
            this.labelSettingsSource.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelSettingsSource.AutoSize = true;
            this.labelSettingsSource.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelSettingsSource.Location = new System.Drawing.Point(3, 12);
            this.labelSettingsSource.Name = "labelSettingsSource";
            this.labelSettingsSource.Size = new System.Drawing.Size(98, 13);
            this.labelSettingsSource.TabIndex = 7;
            this.labelSettingsSource.Text = "Settings source:";
            // 
            // flowLayoutPanelSettingsSource
            // 
            this.flowLayoutPanelSettingsSource.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flowLayoutPanelSettingsSource.AutoSize = true;
            this.flowLayoutPanelSettingsSource.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelSettingsSource.Controls.Add(this.labelSettingsSource);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.EffectiveRB);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.arrow1);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.LocalRB);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.arrow2);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.DistributedRB);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.arrow3);
            this.flowLayoutPanelSettingsSource.Controls.Add(this.GlobalRB);
            this.flowLayoutPanelSettingsSource.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanelSettingsSource.Location = new System.Drawing.Point(3, 4);
            this.flowLayoutPanelSettingsSource.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPanelSettingsSource.Name = "flowLayoutPanelSettingsSource";
            this.flowLayoutPanelSettingsSource.Size = new System.Drawing.Size(924, 37);
            this.flowLayoutPanelSettingsSource.TabIndex = 5;
            // 
            // DistributedRB
            // 
            this.DistributedRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DistributedRB.AutoSize = true;
            this.DistributedRB.Location = new System.Drawing.Point(339, 4);
            this.DistributedRB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DistributedRB.Name = "DistributedRB";
            this.DistributedRB.Size = new System.Drawing.Size(226, 21);
            this.DistributedRB.TabIndex = 12;
            this.DistributedRB.Text = "Distributed with current repository";
            this.DistributedRB.UseVisualStyleBackColor = true;
            // 
            // arrow3
            // 
            this.arrow3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrow3.AutoSize = true;
            this.arrow3.Location = new System.Drawing.Point(571, 6);
            this.arrow3.Name = "arrow3";
            this.arrow3.Size = new System.Drawing.Size(26, 17);
            this.arrow3.TabIndex = 11;
            this.arrow3.Text = "<<";
            // 
            // arrow2
            // 
            this.arrow2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrow2.AutoSize = true;
            this.arrow2.Location = new System.Drawing.Point(307, 6);
            this.arrow2.Name = "arrow2";
            this.arrow2.Size = new System.Drawing.Size(26, 17);
            this.arrow2.TabIndex = 10;
            this.arrow2.Text = "<<";
            // 
            // LocalRB
            // 
            this.LocalRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LocalRB.AutoSize = true;
            this.LocalRB.Location = new System.Drawing.Point(115, 4);
            this.LocalRB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.LocalRB.Name = "LocalRB";
            this.LocalRB.Size = new System.Drawing.Size(186, 21);
            this.LocalRB.TabIndex = 9;
            this.LocalRB.Text = "Local for current repository";
            this.LocalRB.UseVisualStyleBackColor = true;
            // 
            // GlobalRB
            // 
            this.GlobalRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GlobalRB.AutoSize = true;
            this.GlobalRB.Location = new System.Drawing.Point(603, 4);
            this.GlobalRB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.GlobalRB.Name = "GlobalRB";
            this.GlobalRB.Size = new System.Drawing.Size(176, 21);
            this.GlobalRB.TabIndex = 8;
            this.GlobalRB.Text = "Global for all repositories";
            this.GlobalRB.UseVisualStyleBackColor = true;
            this.GlobalRB.CheckedChanged += new System.EventHandler(this.GlobalRB_CheckedChanged);
            // 
            // EffectiveRB
            // 
            this.EffectiveRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.EffectiveRB.AutoSize = true;
            this.EffectiveRB.Checked = true;
            this.EffectiveRB.Location = new System.Drawing.Point(3, 4);
            this.EffectiveRB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.EffectiveRB.Name = "EffectiveRB";
            this.EffectiveRB.Size = new System.Drawing.Size(74, 21);
            this.EffectiveRB.TabIndex = 1;
            this.EffectiveRB.TabStop = true;
            this.EffectiveRB.Text = "Effective";
            this.EffectiveRB.UseVisualStyleBackColor = true;
            // 
            // arrow1
            // 
            this.arrow1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrow1.AutoSize = true;
            this.arrow1.Location = new System.Drawing.Point(83, 6);
            this.arrow1.Name = "arrow1";
            this.arrow1.Size = new System.Drawing.Size(26, 17);
            this.arrow1.TabIndex = 3;
            this.arrow1.Text = "<<";
            // 
            // settingsPagePanel
            // 
            this.settingsPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPagePanel.Location = new System.Drawing.Point(0, 77);
            this.settingsPagePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.settingsPagePanel.Name = "settingsPagePanel";
            this.settingsPagePanel.Size = new System.Drawing.Size(930, 488);
            this.settingsPagePanel.TabIndex = 3;
            // 
            // SettingsPageHeader
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.settingsPagePanel);
            this.Controls.Add(this.HeaderPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "SettingsPageHeader";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.Size = new System.Drawing.Size(930, 565);
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
            this.flowLayoutPanelSettingsSource.ResumeLayout(false);
            this.flowLayoutPanelSettingsSource.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel settingsPagePanel;
        private System.Windows.Forms.TableLayoutPanel HeaderPanel;
        private System.Windows.Forms.Panel linePanel;
        private System.Windows.Forms.Label labelSettingsSource;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSettingsSource;
        private System.Windows.Forms.RadioButton DistributedRB;
        private System.Windows.Forms.Label arrow3;
        private System.Windows.Forms.Label arrow2;
        private System.Windows.Forms.RadioButton LocalRB;
        private System.Windows.Forms.RadioButton GlobalRB;
        private System.Windows.Forms.RadioButton EffectiveRB;
        private System.Windows.Forms.Label arrow1;
    }
}
