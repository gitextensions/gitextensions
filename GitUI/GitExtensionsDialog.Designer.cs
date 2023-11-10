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
            ControlsPanel = new FlowLayoutPanel();
            MainPanel = new Panel();
            SuspendLayout();
            // 
            // ControlsPanel
            // 
            ControlsPanel.AutoSize = true;
            ControlsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ControlsPanel.BackColor = SystemColors.ControlLight;
            ControlsPanel.Dock = DockStyle.Bottom;
            ControlsPanel.FlowDirection = FlowDirection.RightToLeft;
            ControlsPanel.Location = new Point(0, 305);
            ControlsPanel.Margin = new Padding(0);
            ControlsPanel.MinimumSize = new Size(0, 32);
            ControlsPanel.Name = "ControlsPanel";
            ControlsPanel.Padding = new Padding(5);
            ControlsPanel.Size = new Size(553, 32);
            ControlsPanel.TabIndex = 0;
            // 
            // MainPanel
            // 
            MainPanel.BackColor = SystemColors.Window;
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.Location = new Point(0, 0);
            MainPanel.Margin = new Padding(0);
            MainPanel.Name = "MainPanel";
            MainPanel.Padding = new Padding(12);
            MainPanel.Size = new Size(553, 305);
            MainPanel.TabIndex = 1;
            // 
            // GitExtensionsDialog
            // 
            ClientSize = new Size(553, 337);
            Controls.Add(MainPanel);
            Controls.Add(ControlsPanel);
            DoubleBuffered = true;
            Name = "GitExtensionsDialog";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        protected internal Panel MainPanel;
        protected internal FlowLayoutPanel ControlsPanel;
    }
}
