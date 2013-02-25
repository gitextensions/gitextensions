namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class HotkeysSettingsPage
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
            this.controlHotkeys = new ControlHotkeys();
            this.SuspendLayout();
            // 
            // controlHotkeys
            // 
            this.controlHotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlHotkeys.Location = new System.Drawing.Point(0, 0);
            this.controlHotkeys.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.controlHotkeys.Name = "controlHotkeys";
            this.controlHotkeys.Size = new System.Drawing.Size(670, 330);
            this.controlHotkeys.TabIndex = 1;
            // 
            // HotkeysSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.controlHotkeys);
            this.MinimumSize = new System.Drawing.Size(670, 330);
            this.Name = "HotkeysSettingsPage";
            this.Size = new System.Drawing.Size(670, 330);
            this.ResumeLayout(false);

        }

        #endregion

        private ControlHotkeys controlHotkeys;
    }
}
