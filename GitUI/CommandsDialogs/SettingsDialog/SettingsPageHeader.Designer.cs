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
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.linePanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DistributedRB = new System.Windows.Forms.RadioButton();
            this.arrow3 = new System.Windows.Forms.Label();
            this.arrows2 = new System.Windows.Forms.Label();
            this.LocalRB = new System.Windows.Forms.RadioButton();
            this.GlobalRB = new System.Windows.Forms.RadioButton();
            this.EffectiveRB = new System.Windows.Forms.RadioButton();
            this.arrows1 = new System.Windows.Forms.Label();
            this.settingsPagePanel = new System.Windows.Forms.Panel();
            this.HeaderPanel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.AutoSize = true;
            this.HeaderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HeaderPanel.Controls.Add(this.tableLayoutPanel2);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 4);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.HeaderPanel.Size = new System.Drawing.Size(930, 73);
            this.HeaderPanel.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.linePanel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(930, 66);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // linePanel
            // 
            this.linePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linePanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.linePanel.Location = new System.Drawing.Point(3, 45);
            this.linePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.linePanel.Name = "linePanel";
            this.linePanel.Size = new System.Drawing.Size(924, 4);
            this.linePanel.TabIndex = 10;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 4);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(924, 37);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Settings source:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.DistributedRB, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.arrow3, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.arrows2, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.LocalRB, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.GlobalRB, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.EffectiveRB, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.arrows1, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(107, 4);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(782, 29);
            this.tableLayoutPanel1.TabIndex = 8;
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
            // arrows2
            // 
            this.arrows2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrows2.AutoSize = true;
            this.arrows2.Location = new System.Drawing.Point(307, 6);
            this.arrows2.Name = "arrows2";
            this.arrows2.Size = new System.Drawing.Size(26, 17);
            this.arrows2.TabIndex = 10;
            this.arrows2.Text = "<<";
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
            // arrows1
            // 
            this.arrows1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrows1.AutoSize = true;
            this.arrows1.Location = new System.Drawing.Point(83, 6);
            this.arrows1.Name = "arrows1";
            this.arrows1.Size = new System.Drawing.Size(26, 17);
            this.arrows1.TabIndex = 3;
            this.arrows1.Text = "<<";
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
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Panel settingsPagePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel linePanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton DistributedRB;
        private System.Windows.Forms.Label arrow3;
        private System.Windows.Forms.Label arrows2;
        private System.Windows.Forms.RadioButton LocalRB;
        private System.Windows.Forms.RadioButton GlobalRB;
        private System.Windows.Forms.RadioButton EffectiveRB;
        private System.Windows.Forms.Label arrows1;
    }
}
