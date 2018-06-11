namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class AppearanceFontsSettingsPage
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
            this.gbFonts = new System.Windows.Forms.GroupBox();
            this.commitFontChangeButton = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.diffFontChangeButton = new System.Windows.Forms.Button();
            this.applicationFontChangeButton = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.diffFontDialog = new System.Windows.Forms.FontDialog();
            this.applicationDialog = new System.Windows.Forms.FontDialog();
            this.commitFontDialog = new System.Windows.Forms.FontDialog();
            this.gbFonts.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbFonts
            // 
            this.gbFonts.AutoSize = true;
            this.gbFonts.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbFonts.Controls.Add(this.commitFontChangeButton);
            this.gbFonts.Controls.Add(this.label34);
            this.gbFonts.Controls.Add(this.diffFontChangeButton);
            this.gbFonts.Controls.Add(this.applicationFontChangeButton);
            this.gbFonts.Controls.Add(this.label26);
            this.gbFonts.Controls.Add(this.label56);
            this.gbFonts.Location = new System.Drawing.Point(3, 288);
            this.gbFonts.Name = "gbFonts";
            this.gbFonts.Padding = new System.Windows.Forms.Padding(8);
            this.gbFonts.Size = new System.Drawing.Size(2073, 106);
            this.gbFonts.TabIndex = 2;
            this.gbFonts.TabStop = false;
            this.gbFonts.Text = "Fonts";
            // 
            // commitFontChangeButton
            // 
            this.commitFontChangeButton.AutoSize = true;
            this.commitFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitFontChangeButton.Location = new System.Drawing.Point(283, 75);
            this.commitFontChangeButton.Name = "commitFontChangeButton";
            this.commitFontChangeButton.Size = new System.Drawing.Size(118, 35);
            this.commitFontChangeButton.TabIndex = 5;
            this.commitFontChangeButton.Text = "font name";
            this.commitFontChangeButton.UseVisualStyleBackColor = true;
            this.commitFontChangeButton.Click += new System.EventHandler(this.commitFontChangeButton_Click);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(10, 80);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(128, 25);
            this.label34.TabIndex = 4;
            this.label34.Text = "Commit font";
            // 
            // diffFontChangeButton
            // 
            this.diffFontChangeButton.AutoSize = true;
            this.diffFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.diffFontChangeButton.Location = new System.Drawing.Point(283, 17);
            this.diffFontChangeButton.Name = "diffFontChangeButton";
            this.diffFontChangeButton.Size = new System.Drawing.Size(118, 35);
            this.diffFontChangeButton.TabIndex = 1;
            this.diffFontChangeButton.Text = "font name";
            this.diffFontChangeButton.UseVisualStyleBackColor = true;
            this.diffFontChangeButton.Click += new System.EventHandler(this.diffFontChangeButton_Click);
            // 
            // applicationFontChangeButton
            // 
            this.applicationFontChangeButton.AutoSize = true;
            this.applicationFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.applicationFontChangeButton.Location = new System.Drawing.Point(283, 46);
            this.applicationFontChangeButton.Name = "applicationFontChangeButton";
            this.applicationFontChangeButton.Size = new System.Drawing.Size(118, 35);
            this.applicationFontChangeButton.TabIndex = 3;
            this.applicationFontChangeButton.Text = "font name";
            this.applicationFontChangeButton.UseVisualStyleBackColor = true;
            this.applicationFontChangeButton.Click += new System.EventHandler(this.applicationFontChangeButton_Click);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(10, 51);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(159, 25);
            this.label26.TabIndex = 2;
            this.label26.Text = "Application font";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(10, 22);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(103, 25);
            this.label56.TabIndex = 0;
            this.label56.Text = "Code font";
            // 
            // diffFontDialog
            // 
            this.diffFontDialog.AllowVerticalFonts = false;
            this.diffFontDialog.Color = System.Drawing.SystemColors.ControlText;
            this.diffFontDialog.FixedPitchOnly = true;
            // 
            // applicationDialog
            // 
            this.applicationDialog.AllowVerticalFonts = false;
            this.applicationDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // commitFontDialog
            // 
            this.commitFontDialog.AllowVerticalFonts = false;
            this.commitFontDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // AppearanceFontsSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.gbFonts);
            this.MinimumSize = new System.Drawing.Size(515, 510);
            this.Name = "AppearanceFontsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.Size = new System.Drawing.Size(2082, 1141);
            this.gbFonts.ResumeLayout(false);
            this.gbFonts.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox gbFonts;
        private System.Windows.Forms.Button diffFontChangeButton;
        private System.Windows.Forms.Button applicationFontChangeButton;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.FontDialog diffFontDialog;
        private System.Windows.Forms.FontDialog applicationDialog;
        private System.Windows.Forms.Button commitFontChangeButton;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.FontDialog commitFontDialog;
    }
}
