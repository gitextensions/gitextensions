namespace GitUI
{
    partial class FormSplash
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this._NO_TRANSLATE_versionLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_programTitle = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_actionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Resources.Cow;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(128, 128);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this._NO_TRANSLATE_versionLabel);
            this.panel1.Controls.Add(this._NO_TRANSLATE_programTitle);
            this.panel1.Controls.Add(this._NO_TRANSLATE_actionLabel);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(256, 128);
            this.panel1.TabIndex = 1;
            // 
            // _NO_TRANSLATE_versionLabel
            // 
            this._NO_TRANSLATE_versionLabel.AutoSize = true;
            this._NO_TRANSLATE_versionLabel.Location = new System.Drawing.Point(134, 30);
            this._NO_TRANSLATE_versionLabel.Name = "_NO_TRANSLATE_versionLabel";
            this._NO_TRANSLATE_versionLabel.Size = new System.Drawing.Size(59, 13);
            this._NO_TRANSLATE_versionLabel.TabIndex = 3;
            this._NO_TRANSLATE_versionLabel.Text = "Version {0}";
            // 
            // _NO_TRANSLATE_programTitle
            // 
            this._NO_TRANSLATE_programTitle.AutoSize = true;
            this._NO_TRANSLATE_programTitle.Location = new System.Drawing.Point(134, 8);
            this._NO_TRANSLATE_programTitle.Name = "_NO_TRANSLATE_programTitle";
            this._NO_TRANSLATE_programTitle.Size = new System.Drawing.Size(74, 13);
            this._NO_TRANSLATE_programTitle.TabIndex = 2;
            this._NO_TRANSLATE_programTitle.Text = "Git Extensions";
            // 
            // _NO_TRANSLATE_actionLabel
            // 
            this._NO_TRANSLATE_actionLabel.AutoSize = true;
            this._NO_TRANSLATE_actionLabel.Location = new System.Drawing.Point(134, 104);
            this._NO_TRANSLATE_actionLabel.Name = "_NO_TRANSLATE_actionLabel";
            this._NO_TRANSLATE_actionLabel.Size = new System.Drawing.Size(54, 13);
            this._NO_TRANSLATE_actionLabel.TabIndex = 1;
            this._NO_TRANSLATE_actionLabel.Text = "Loading...";
            // 
            // FormSplash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(256, 128);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormSplash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _NO_TRANSLATE_programTitle;
        private System.Windows.Forms.Label _NO_TRANSLATE_actionLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_versionLabel;
    }
}