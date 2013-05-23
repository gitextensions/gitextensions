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
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.arrows2 = new System.Windows.Forms.Label();
            this.LocalRB = new System.Windows.Forms.RadioButton();
            this.GlobalRB = new System.Windows.Forms.RadioButton();
            this.EffectiveRB = new System.Windows.Forms.RadioButton();
            this.arrows1 = new System.Windows.Forms.Label();
            this.HeaderPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.AutoSize = true;
            this.HeaderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HeaderPanel.Controls.Add(this.flowLayoutPanel1);
            this.HeaderPanel.Controls.Add(this.panel1);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 3);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.HeaderPanel.Size = new System.Drawing.Size(688, 38);
            this.HeaderPanel.TabIndex = 0;
            // 
            // settingsPagePanel
            // 
            this.settingsPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPagePanel.Location = new System.Drawing.Point(0, 41);
            this.settingsPagePanel.Name = "settingsPagePanel";
            this.settingsPagePanel.Size = new System.Drawing.Size(688, 391);
            this.settingsPagePanel.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(688, 3);
            this.panel1.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 1);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(553, 29);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Settings source:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.arrows2, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.LocalRB, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.GlobalRB, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.EffectiveRB, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.arrows1, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(107, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(443, 23);
            this.tableLayoutPanel1.TabIndex = 6;
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
            // GlobalRB
            // 
            this.GlobalRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GlobalRB.AutoSize = true;
            this.GlobalRB.Location = new System.Drawing.Point(297, 3);
            this.GlobalRB.Name = "GlobalRB";
            this.GlobalRB.Size = new System.Drawing.Size(143, 17);
            this.GlobalRB.TabIndex = 8;
            this.GlobalRB.Text = "Global for all repositories";
            this.GlobalRB.UseVisualStyleBackColor = true;
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
            // SettingsPageHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.settingsPagePanel);
            this.Controls.Add(this.HeaderPanel);
            this.Name = "SettingsPageHeader";
            this.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.Size = new System.Drawing.Size(688, 432);
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
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
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label arrows2;
        private System.Windows.Forms.RadioButton LocalRB;
        private System.Windows.Forms.RadioButton GlobalRB;
        private System.Windows.Forms.RadioButton EffectiveRB;
        private System.Windows.Forms.Label arrows1;
    }
}
