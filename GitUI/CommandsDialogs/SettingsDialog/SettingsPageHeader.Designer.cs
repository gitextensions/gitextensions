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
            this.settingsPagePanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.linePanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.arrows1 = new System.Windows.Forms.Label();
            this.EffectiveRB = new System.Windows.Forms.RadioButton();
            this.GlobalRB = new System.Windows.Forms.RadioButton();
            this.LocalRB = new System.Windows.Forms.RadioButton();
            this.arrows2 = new System.Windows.Forms.Label();
            this.arrow3 = new System.Windows.Forms.Label();
            this.DistributedRB = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
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
            this.HeaderPanel.Location = new System.Drawing.Point(0, 3);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.HeaderPanel.Size = new System.Drawing.Size(797, 56);
            this.HeaderPanel.TabIndex = 0;
            // 
            // settingsPagePanel
            // 
            this.settingsPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPagePanel.Location = new System.Drawing.Point(0, 59);
            this.settingsPagePanel.Name = "settingsPagePanel";
            this.settingsPagePanel.Size = new System.Drawing.Size(797, 373);
            this.settingsPagePanel.TabIndex = 3;
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
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(797, 51);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // linePanel
            // 
            this.linePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linePanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.linePanel.Location = new System.Drawing.Point(3, 35);
            this.linePanel.Name = "linePanel";
            this.linePanel.Size = new System.Drawing.Size(791, 3);
            this.linePanel.TabIndex = 10;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(791, 29);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Settings source:";
            // 
            // arrows1
            // 
            this.arrows1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrows1.AutoSize = true;
            this.arrows1.Location = new System.Drawing.Point(77, 5);
            this.arrows1.Name = "arrows1";
            this.arrows1.Size = new System.Drawing.Size(23, 13);
            this.arrows1.TabIndex = 3;
            this.arrows1.Text = "<<";
            // 
            // EffectiveRB
            // 
            this.EffectiveRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.EffectiveRB.AutoSize = true;
            this.EffectiveRB.Checked = true;
            this.EffectiveRB.Location = new System.Drawing.Point(3, 3);
            this.EffectiveRB.Name = "EffectiveRB";
            this.EffectiveRB.Size = new System.Drawing.Size(68, 17);
            this.EffectiveRB.TabIndex = 1;
            this.EffectiveRB.TabStop = true;
            this.EffectiveRB.Text = "Effective";
            this.EffectiveRB.UseVisualStyleBackColor = true;
            // 
            // GlobalRB
            // 
            this.GlobalRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GlobalRB.AutoSize = true;
            this.GlobalRB.Location = new System.Drawing.Point(522, 3);
            this.GlobalRB.Name = "GlobalRB";
            this.GlobalRB.Size = new System.Drawing.Size(143, 17);
            this.GlobalRB.TabIndex = 8;
            this.GlobalRB.Text = "Global for all repositories";
            this.GlobalRB.UseVisualStyleBackColor = true;
            // 
            // LocalRB
            // 
            this.LocalRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LocalRB.AutoSize = true;
            this.LocalRB.Location = new System.Drawing.Point(106, 3);
            this.LocalRB.Name = "LocalRB";
            this.LocalRB.Size = new System.Drawing.Size(156, 17);
            this.LocalRB.TabIndex = 9;
            this.LocalRB.Text = "Local for current repository";
            this.LocalRB.UseVisualStyleBackColor = true;
            // 
            // arrows2
            // 
            this.arrows2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrows2.AutoSize = true;
            this.arrows2.Location = new System.Drawing.Point(268, 5);
            this.arrows2.Name = "arrows2";
            this.arrows2.Size = new System.Drawing.Size(23, 13);
            this.arrows2.TabIndex = 10;
            this.arrows2.Text = "<<";
            // 
            // arrow3
            // 
            this.arrow3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrow3.AutoSize = true;
            this.arrow3.Location = new System.Drawing.Point(493, 5);
            this.arrow3.Name = "arrow3";
            this.arrow3.Size = new System.Drawing.Size(23, 13);
            this.arrow3.TabIndex = 11;
            this.arrow3.Text = "<<";
            // 
            // DistributedRB
            // 
            this.DistributedRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DistributedRB.AutoSize = true;
            this.DistributedRB.Location = new System.Drawing.Point(297, 3);
            this.DistributedRB.Name = "DistributedRB";
            this.DistributedRB.Size = new System.Drawing.Size(190, 17);
            this.DistributedRB.TabIndex = 12;
            this.DistributedRB.Text = "Distributed with current repository";
            this.DistributedRB.UseVisualStyleBackColor = true;
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(107, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(668, 23);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // SettingsPageHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.settingsPagePanel);
            this.Controls.Add(this.HeaderPanel);
            this.Name = "SettingsPageHeader";
            this.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.Size = new System.Drawing.Size(797, 432);
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
