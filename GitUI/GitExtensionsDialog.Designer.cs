using System.Windows.Forms;

namespace GitUI
{
    partial class GitExtensionsDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ControlsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.AutoSize = true;
            this.ControlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ControlsPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ControlsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ControlsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ControlsPanel.Location = new System.Drawing.Point(0, 305);
            this.ControlsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ControlsPanel.MinimumSize = new System.Drawing.Size(0, 32);
            this.ControlsPanel.Name = "ControlsPanel";
            this.ControlsPanel.Padding = new System.Windows.Forms.Padding(5);
            this.ControlsPanel.Size = new System.Drawing.Size(553, 32);
            this.ControlsPanel.TabIndex = 0;
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.SystemColors.Window;
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.MainPanel.Size = new System.Drawing.Size(553, 305);
            this.MainPanel.TabIndex = 1;
            // 
            // GitExtensionsDialog
            // 
            this.ClientSize = new System.Drawing.Size(553, 337);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.ControlsPanel);
            this.DoubleBuffered = true;
            this.Name = "GitExtensionsDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal Panel MainPanel;
        protected internal FlowLayoutPanel ControlsPanel;
    }
}
