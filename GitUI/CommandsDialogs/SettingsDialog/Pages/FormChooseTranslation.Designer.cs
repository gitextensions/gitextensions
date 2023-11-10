namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class FormChooseTranslation
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
            label1 = new Label();
            label2 = new Label();
            lvTranslations = new GitUI.UserControls.NativeListView();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(126, 15);
            label1.TabIndex = 0;
            label1.Text = "Choose your language";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 33);
            label2.Name = "label2";
            label2.Size = new Size(338, 15);
            label2.TabIndex = 7;
            label2.Text = "You can change the language at any time in the settings dialog";
            // 
            // lvTranslations
            // 
            lvTranslations.Activation = ItemActivation.OneClick;
            lvTranslations.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvTranslations.HotTracking = true;
            lvTranslations.HoverSelection = true;
            lvTranslations.Location = new Point(12, 51);
            lvTranslations.MultiSelect = false;
            lvTranslations.Name = "lvTranslations";
            lvTranslations.ShowGroups = false;
            lvTranslations.Size = new Size(776, 476);
            lvTranslations.TabIndex = 0;
            lvTranslations.UseCompatibleStateImageBehavior = false;
            lvTranslations.ItemActivate += lvTranslations_ItemActivate;
            // 
            // FormChooseTranslation
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(800, 539);
            Controls.Add(lvTranslations);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "FormChooseTranslation";
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += FormChooseTranslation_FormClosing;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private UserControls.NativeListView lvTranslations;
    }
}
