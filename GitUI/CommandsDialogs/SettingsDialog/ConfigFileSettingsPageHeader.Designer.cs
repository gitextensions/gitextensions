namespace GitUI.CommandsDialogs.SettingsDialog
{
    partial class ConfigFileSettingsPageHeader
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.arrows2 = new System.Windows.Forms.Label();
            this.LocalRB = new System.Windows.Forms.RadioButton();
            this.GlobalRB = new System.Windows.Forms.RadioButton();
            this.EffectiveRB = new System.Windows.Forms.RadioButton();
            this.arrows1 = new System.Windows.Forms.Label();
            this.settingsPagePanel = new System.Windows.Forms.Panel();
            this.HeaderPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.AutoSize = true;
            this.HeaderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HeaderPanel.Controls.Add(this.groupBox1);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 4);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Size = new System.Drawing.Size(803, 77);
            this.HeaderPanel.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(3, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox1.Size = new System.Drawing.Size(524, 69);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings source";
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(518, 29);
            this.tableLayoutPanel1.TabIndex = 1;
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
            this.LocalRB.CheckedChanged += new System.EventHandler(this.LocalRB_CheckedChanged);
            // 
            // GlobalRB
            // 
            this.GlobalRB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GlobalRB.AutoSize = true;
            this.GlobalRB.Location = new System.Drawing.Point(339, 4);
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
            this.EffectiveRB.CheckedChanged += new System.EventHandler(this.EffectiveRB_CheckedChanged);
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
            this.settingsPagePanel.Location = new System.Drawing.Point(0, 81);
            this.settingsPagePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.settingsPagePanel.Name = "settingsPagePanel";
            this.settingsPagePanel.Size = new System.Drawing.Size(803, 484);
            this.settingsPagePanel.TabIndex = 1;
            // 
            // ConfigFileSettingsPageHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.settingsPagePanel);
            this.Controls.Add(this.HeaderPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ConfigFileSettingsPageHeader";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.Size = new System.Drawing.Size(803, 565);
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel settingsPagePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton EffectiveRB;
        private System.Windows.Forms.Label arrows1;
        private System.Windows.Forms.RadioButton LocalRB;
        private System.Windows.Forms.RadioButton GlobalRB;
        private System.Windows.Forms.Label arrows2;
    }
}
