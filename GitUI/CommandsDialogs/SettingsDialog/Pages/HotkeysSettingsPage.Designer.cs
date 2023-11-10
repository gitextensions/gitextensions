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
            controlHotkeys = new ControlHotkeys();
            SuspendLayout();
            // 
            // controlHotkeys
            // 
            controlHotkeys.Dock = DockStyle.Fill;
            controlHotkeys.Location = new Point(0, 0);
            controlHotkeys.Margin = new Padding(3, 4, 3, 4);
            controlHotkeys.Name = "controlHotkeys";
            controlHotkeys.Size = new Size(670, 330);
            controlHotkeys.TabIndex = 1;
            // 
            // HotkeysSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(controlHotkeys);
            MinimumSize = new Size(670, 330);
            Name = "HotkeysSettingsPage";
            Size = new Size(670, 330);
            Text = "Hotkeys";
            ResumeLayout(false);

        }

        #endregion

        private ControlHotkeys controlHotkeys;
    }
}
