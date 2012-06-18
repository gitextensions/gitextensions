namespace Gerrit
{
    partial class FormGerritChangeSubmitted
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
            this.labelSubmitted = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_TargetLabel = new System.Windows.Forms.LinkLabel();
            this.Close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelSubmitted
            // 
            this.labelSubmitted.AutoSize = true;
            this.labelSubmitted.Location = new System.Drawing.Point(21, 18);
            this.labelSubmitted.Name = "labelSubmitted";
            this.labelSubmitted.Size = new System.Drawing.Size(371, 15);
            this.labelSubmitted.TabIndex = 0;
            this.labelSubmitted.Text = "Your change has been submitted for review at the following location:";
            // 
            // _NO_TRANSLATE_TargetLabel
            // 
            this._NO_TRANSLATE_TargetLabel.AutoSize = true;
            this._NO_TRANSLATE_TargetLabel.Location = new System.Drawing.Point(21, 46);
            this._NO_TRANSLATE_TargetLabel.Name = "_NO_TRANSLATE_TargetLabel";
            this._NO_TRANSLATE_TargetLabel.Size = new System.Drawing.Size(89, 15);
            this._NO_TRANSLATE_TargetLabel.TabIndex = 1;
            this._NO_TRANSLATE_TargetLabel.TabStop = true;
            this._NO_TRANSLATE_TargetLabel.Text = "Location of link";
            // 
            // Close
            // 
            this.Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close.Location = new System.Drawing.Point(364, 71);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(75, 23);
            this.Close.TabIndex = 2;
            this.Close.Text = "Close";
            this.Close.UseVisualStyleBackColor = true;
            // 
            // FormGerritChangeSubmitted
            // 
            this.AcceptButton = this.Close;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Close;
            this.ClientSize = new System.Drawing.Size(462, 109);
            this.Controls.Add(this.Close);
            this.Controls.Add(this._NO_TRANSLATE_TargetLabel);
            this.Controls.Add(this.labelSubmitted);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGerritChangeSubmitted";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change Submitted";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSubmitted;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_TargetLabel;
        private System.Windows.Forms.Button Close;
    }
}