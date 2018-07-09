namespace GitUI.CommandsDialogs
{
    partial class FormAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.components = new System.ComponentModel.Container();
            this.thanksTimer = new System.Windows.Forms.Timer(this.components);
            this.okButton = new System.Windows.Forms.Button();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_labelCopyright = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_labelVersionInfo = new System.Windows.Forms.Label();
            this.pictureDonate = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_labelProductName = new System.Windows.Forms.Label();
            this.thanksTo = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabelIcons = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDonate)).BeginInit();
            this.SuspendLayout();
            // 
            // thanksTimer
            // 
            this.thanksTimer.Interval = 1000;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(468, 588);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(176, 48);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Image = global::GitUI.Properties.Images.GitExtensionsLogo256;
            this.logoPictureBox.Location = new System.Drawing.Point(100, 98);
            this.logoPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.Size = new System.Drawing.Size(256, 256);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoPictureBox.TabIndex = 12;
            this.logoPictureBox.TabStop = false;
            // 
            // _NO_TRANSLATE_labelCopyright
            // 
            this._NO_TRANSLATE_labelCopyright.AutoSize = true;
            this._NO_TRANSLATE_labelCopyright.Location = new System.Drawing.Point(462, 188);
            this._NO_TRANSLATE_labelCopyright.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelCopyright.Name = "_NO_TRANSLATE_labelCopyright";
            this._NO_TRANSLATE_labelCopyright.Size = new System.Drawing.Size(443, 25);
            this._NO_TRANSLATE_labelCopyright.TabIndex = 21;
            this._NO_TRANSLATE_labelCopyright.Text = "Henk Westhuis (henk_westhuis@hotmail.com)";
            this._NO_TRANSLATE_labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_labelVersionInfo
            // 
            this._NO_TRANSLATE_labelVersionInfo.AutoSize = true;
            this._NO_TRANSLATE_labelVersionInfo.Location = new System.Drawing.Point(462, 142);
            this._NO_TRANSLATE_labelVersionInfo.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelVersionInfo.Name = "_NO_TRANSLATE_labelVersionInfo";
            this._NO_TRANSLATE_labelVersionInfo.Size = new System.Drawing.Size(88, 25);
            this._NO_TRANSLATE_labelVersionInfo.TabIndex = 0;
            this._NO_TRANSLATE_labelVersionInfo.Text = "Version ";
            this._NO_TRANSLATE_labelVersionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureDonate
            // 
            this.pictureDonate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureDonate.Image = global::GitUI.Properties.Images.DonateBadge;
            this.pictureDonate.Location = new System.Drawing.Point(468, 496);
            this.pictureDonate.Margin = new System.Windows.Forms.Padding(6);
            this.pictureDonate.Name = "pictureDonate";
            this.pictureDonate.Size = new System.Drawing.Size(176, 64);
            this.pictureDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureDonate.TabIndex = 25;
            this.pictureDonate.TabStop = false;
            // 
            // _NO_TRANSLATE_labelProductName
            // 
            this._NO_TRANSLATE_labelProductName.AutoSize = true;
            this._NO_TRANSLATE_labelProductName.Location = new System.Drawing.Point(462, 96);
            this._NO_TRANSLATE_labelProductName.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelProductName.Name = "_NO_TRANSLATE_labelProductName";
            this._NO_TRANSLATE_labelProductName.Size = new System.Drawing.Size(635, 25);
            this._NO_TRANSLATE_labelProductName.TabIndex = 19;
            this._NO_TRANSLATE_labelProductName.Text = "Git Extensions - Visual Studio and Shell Explorer Extensions for Git";
            this._NO_TRANSLATE_labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // thanksTo
            // 
            this.thanksTo.AutoSize = true;
            this.thanksTo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.thanksTo.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.thanksTo.Location = new System.Drawing.Point(462, 234);
            this.thanksTo.Margin = new System.Windows.Forms.Padding(0);
            this.thanksTo.Name = "thanksTo";
            this.thanksTo.Size = new System.Drawing.Size(370, 27);
            this.thanksTo.TabIndex = 27;
            this.thanksTo.TabStop = true;
            this.thanksTo.Text = "Thanks to over {0} contributors: {1}";
            this.thanksTo.UseMnemonic = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(462, 396);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(634, 90);
            this.label1.TabIndex = 27;
            this.label1.Text = "This program is distributed in the hope that it will be useful, but WITHOUT ANY W" +
    "ARRANTY; without even the implied warranty of MERCHANTABILITY of FITNESS FOR A P" +
    "ARTICULAR PURPOSE.";
            // 
            // linkLabelIcons
            // 
            this.linkLabelIcons.AutoSize = true;
            this.linkLabelIcons.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelIcons.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.linkLabelIcons.Location = new System.Drawing.Point(463, 338);
            this.linkLabelIcons.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelIcons.Name = "linkLabelIcons";
            this.linkLabelIcons.Size = new System.Drawing.Size(435, 27);
            this.linkLabelIcons.TabIndex = 28;
            this.linkLabelIcons.TabStop = true;
            this.linkLabelIcons.Text = "Some icons by Yusuke Kamiyamane (CCA3)";
            this.linkLabelIcons.UseMnemonic = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(462, 285);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(420, 25);
            this.label2.TabIndex = 29;
            this.label2.Text = "Git Extensions is open source. Get involved!";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormAbout
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1182, 696);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.linkLabelIcons);
            this.Controls.Add(this.logoPictureBox);
            this.Controls.Add(this._NO_TRANSLATE_labelCopyright);
            this.Controls.Add(this._NO_TRANSLATE_labelVersionInfo);
            this.Controls.Add(this.pictureDonate);
            this.Controls.Add(this._NO_TRANSLATE_labelProductName);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.thanksTo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDonate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label _NO_TRANSLATE_labelProductName;
        private System.Windows.Forms.Label _NO_TRANSLATE_labelVersionInfo;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label _NO_TRANSLATE_labelCopyright;
        private System.Windows.Forms.PictureBox pictureDonate;
        private System.Windows.Forms.Timer thanksTimer;
        private System.Windows.Forms.LinkLabel thanksTo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabelIcons;
        private System.Windows.Forms.Label label2;
    }
}
