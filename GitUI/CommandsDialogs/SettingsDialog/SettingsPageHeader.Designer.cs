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
            if (disposing && (components is not null))
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
            HeaderPanel = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            linePanel = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            DistributedRB = new RadioButton();
            arrowGlobal = new Label();
            arrowDistributed = new Label();
            LocalRB = new RadioButton();
            GlobalRB = new RadioButton();
            EffectiveRB = new RadioButton();
            arrowLocal = new Label();
            settingsPagePanel = new Panel();
            HeaderPanel.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // HeaderPanel
            // 
            HeaderPanel.AutoSize = true;
            HeaderPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            HeaderPanel.Controls.Add(tableLayoutPanel2);
            HeaderPanel.Dock = DockStyle.Top;
            HeaderPanel.Location = new Point(0, 4);
            HeaderPanel.Margin = new Padding(3, 4, 3, 4);
            HeaderPanel.Name = "HeaderPanel";
            HeaderPanel.Padding = new Padding(0, 0, 0, 7);
            HeaderPanel.Size = new Size(930, 71);
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(linePanel, 0, 1);
            tableLayoutPanel2.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 13F));
            tableLayoutPanel2.Size = new Size(930, 64);
            // 
            // linePanel
            // 
            linePanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            linePanel.BackColor = SystemColors.ControlDarkDark;
            linePanel.Location = new Point(3, 43);
            linePanel.Margin = new Padding(3, 4, 3, 4);
            linePanel.Name = "linePanel";
            linePanel.Size = new Size(924, 4);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(tableLayoutPanel1);
            flowLayoutPanel1.Dock = DockStyle.Top;
            flowLayoutPanel1.Location = new Point(3, 4);
            flowLayoutPanel1.Margin = new Padding(3, 4, 3, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(924, 35);
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.None;
            label1.AutoSize = true;
            label1.Location = new Point(3, 10);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.Text = "Settings source:";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.None;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 7;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(EffectiveRB, 0, 0);
            tableLayoutPanel1.Controls.Add(arrowLocal, 1, 0);
            tableLayoutPanel1.Controls.Add(LocalRB, 2, 0);
            tableLayoutPanel1.Controls.Add(arrowDistributed, 3, 0);
            tableLayoutPanel1.Controls.Add(DistributedRB, 4, 0);
            tableLayoutPanel1.Controls.Add(arrowGlobal, 5, 0);
            tableLayoutPanel1.Controls.Add(GlobalRB, 6, 0);
            tableLayoutPanel1.Location = new Point(99, 4);
            tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new Size(711, 27);
            // 
            // DistributedRB
            // 
            DistributedRB.Anchor = AnchorStyles.Left;
            DistributedRB.AutoSize = true;
            DistributedRB.Location = new Point(311, 4);
            DistributedRB.Margin = new Padding(3, 4, 3, 4);
            DistributedRB.Name = "DistributedRB";
            DistributedRB.Size = new Size(206, 19);
            DistributedRB.Text = "Distributed with current repository";
            DistributedRB.UseVisualStyleBackColor = true;
            // 
            // arrowGlobal
            // 
            arrowGlobal.Anchor = AnchorStyles.Left;
            arrowGlobal.AutoSize = true;
            arrowGlobal.Location = new Point(523, 6);
            arrowGlobal.Name = "arrowGlobal";
            arrowGlobal.Size = new Size(23, 15);
            arrowGlobal.Text = "<<";
            // 
            // arrowDistributed
            // 
            arrowDistributed.Anchor = AnchorStyles.Left;
            arrowDistributed.AutoSize = true;
            arrowDistributed.Location = new Point(282, 6);
            arrowDistributed.Name = "arrowDistributed";
            arrowDistributed.Size = new Size(23, 15);
            arrowDistributed.Text = "<<";
            // 
            // LocalRB
            // 
            LocalRB.Anchor = AnchorStyles.Left;
            LocalRB.AutoSize = true;
            LocalRB.Location = new Point(108, 4);
            LocalRB.Margin = new Padding(3, 4, 3, 4);
            LocalRB.Name = "LocalRB";
            LocalRB.Size = new Size(168, 19);
            LocalRB.Text = "Local for current repository";
            LocalRB.UseVisualStyleBackColor = true;
            // 
            // GlobalRB
            // 
            GlobalRB.Anchor = AnchorStyles.Left;
            GlobalRB.AutoSize = true;
            GlobalRB.Location = new Point(552, 4);
            GlobalRB.Margin = new Padding(3, 4, 3, 4);
            GlobalRB.Name = "GlobalRB";
            GlobalRB.Size = new Size(156, 19);
            GlobalRB.Text = "Global for all repositories";
            GlobalRB.UseVisualStyleBackColor = true;
            GlobalRB.CheckedChanged += GlobalRB_CheckedChanged;
            // 
            // EffectiveRB
            // 
            EffectiveRB.Anchor = AnchorStyles.Left;
            EffectiveRB.AutoSize = true;
            EffectiveRB.Checked = true;
            EffectiveRB.Location = new Point(3, 4);
            EffectiveRB.Margin = new Padding(3, 4, 3, 4);
            EffectiveRB.Name = "EffectiveRB";
            EffectiveRB.Size = new Size(70, 19);
            EffectiveRB.TabStop = true;
            EffectiveRB.Text = "Effective";
            EffectiveRB.UseVisualStyleBackColor = true;
            // 
            // arrowLocal
            // 
            arrowLocal.Anchor = AnchorStyles.Left;
            arrowLocal.AutoSize = true;
            arrowLocal.Location = new Point(79, 6);
            arrowLocal.Name = "arrowLocal";
            arrowLocal.Size = new Size(23, 15);
            arrowLocal.Text = "<<";
            // 
            // settingsPagePanel
            // 
            settingsPagePanel.Dock = DockStyle.Fill;
            settingsPagePanel.Location = new Point(0, 75);
            settingsPagePanel.Margin = new Padding(3, 4, 3, 4);
            settingsPagePanel.Name = "settingsPagePanel";
            settingsPagePanel.Size = new Size(930, 490);
            // 
            // SettingsPageHeader
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(settingsPagePanel);
            Controls.Add(HeaderPanel);
            Margin = new Padding(3, 4, 3, 4);
            Name = "SettingsPageHeader";
            Padding = new Padding(0, 4, 0, 0);
            Size = new Size(930, 565);
            HeaderPanel.ResumeLayout(false);
            HeaderPanel.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Label arrowGlobal;
        private System.Windows.Forms.Label arrowDistributed;
        private System.Windows.Forms.RadioButton LocalRB;
        private System.Windows.Forms.RadioButton GlobalRB;
        private System.Windows.Forms.RadioButton EffectiveRB;
        private System.Windows.Forms.Label arrowLocal;
    }
}
