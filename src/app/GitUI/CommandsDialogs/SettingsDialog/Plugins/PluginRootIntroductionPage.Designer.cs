
namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class PluginRootIntroductionPage
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
            label1 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new Point(12, 13);
            label1.Name = "label1";
            label1.Size = new Size(284, 132);
            label1.TabIndex = 0;
            label1.Text = "Select one of the subnodes to view or edit the settings of a Git Extensions Plugi" +
    "n.";
            // 
            // PluginRootIntroductionPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(label1);
            Name = "PluginRootIntroductionPage";
            Size = new Size(473, 229);
            Text = "Plugins Settings";
            ResumeLayout(false);

        }

        #endregion

        private Label label1;

    }
}
