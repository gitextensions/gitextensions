namespace Gerrit
{
    partial class FormPluginInformation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPluginInformation));
            this._NO_TRANSLATE_TargetLabel = new System.Windows.Forms.LinkLabel();
            this.Close = new System.Windows.Forms.Button();
            this.informationLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_TargetLabel
            // 
            this._NO_TRANSLATE_TargetLabel.AutoSize = true;
            this._NO_TRANSLATE_TargetLabel.Location = new System.Drawing.Point(18, 123);
            this._NO_TRANSLATE_TargetLabel.Name = "_NO_TRANSLATE_TargetLabel";
            this._NO_TRANSLATE_TargetLabel.Size = new System.Drawing.Size(148, 15);
            this._NO_TRANSLATE_TargetLabel.TabIndex = 1;
            this._NO_TRANSLATE_TargetLabel.TabStop = true;
            this._NO_TRANSLATE_TargetLabel.Text = "GitHub page for git-review";
            this._NO_TRANSLATE_TargetLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._NO_TRANSLATE_TargetLabel_LinkClicked);
            // 
            // Close
            // 
            this.Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close.Location = new System.Drawing.Point(427, 149);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(75, 23);
            this.Close.TabIndex = 2;
            this.Close.Text = "Close";
            this.Close.UseVisualStyleBackColor = true;
            // 
            // informationLabel
            // 
            this.informationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.informationLabel.Location = new System.Drawing.Point(18, 16);
            this.informationLabel.Name = "informationLabel";
            this.informationLabel.Size = new System.Drawing.Size(484, 107);
            this.informationLabel.TabIndex = 3;
            this.informationLabel.Text = resources.GetString("informationLabel.Text");
            // 
            // FormPluginInformation
            // 
            this.AcceptButton = this.Close;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Close;
            this.ClientSize = new System.Drawing.Size(525, 187);
            this.Controls.Add(this.informationLabel);
            this.Controls.Add(this.Close);
            this.Controls.Add(this._NO_TRANSLATE_TargetLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPluginInformation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gerrit Plugin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_TargetLabel;
        private System.Windows.Forms.Button Close;
        private System.Windows.Forms.Label informationLabel;
    }
}