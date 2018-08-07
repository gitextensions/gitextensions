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
            if (disposing && (components != null))
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lvTranslations = new GitUI.UserControls.NativeListView();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose your language";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(338, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "You can change the language at any time in the settings dialog";
            // 
            // lvTranslations
            // 
            this.lvTranslations.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvTranslations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvTranslations.HotTracking = true;
            this.lvTranslations.HoverSelection = true;
            this.lvTranslations.Location = new System.Drawing.Point(12, 51);
            this.lvTranslations.MultiSelect = false;
            this.lvTranslations.Name = "lvTranslations";
            this.lvTranslations.ShowGroups = false;
            this.lvTranslations.Size = new System.Drawing.Size(776, 476);
            this.lvTranslations.TabIndex = 0;
            this.lvTranslations.UseCompatibleStateImageBehavior = false;
            this.lvTranslations.ItemActivate += new System.EventHandler(this.lvTranslations_ItemActivate);
            // 
            // FormChooseTranslation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(800, 539);
            this.Controls.Add(this.lvTranslations);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormChooseTranslation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Choose language";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormChooseTranslation_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private UserControls.NativeListView lvTranslations;
    }
}