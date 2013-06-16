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
            this.btnClose = new System.Windows.Forms.Button();
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
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(427, 149);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
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
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(525, 187);
            this.Controls.Add(this.informationLabel);
            this.Controls.Add(this.btnClose);
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
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label informationLabel;
    }
}