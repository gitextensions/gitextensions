
namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GitRootIntroductionPage
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
            label1.Text = "Select one of the subnodes to view or edit the Git settings";
            // 
            // GitRootIntroductionPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(label1);
            Name = "GitRootIntroductionPage";
            Size = new Size(473, 229);
            Text = "Git Settings";
            ResumeLayout(false);

        }

        #endregion

        private Label label1;

    }
}
