namespace GitUI
{
    partial class FormRevert
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
            this._NO_TRANSLATE_RevertLabel = new System.Windows.Forms.Label();
            this.Revert = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_RevertLabel
            // 
            this._NO_TRANSLATE_RevertLabel.AutoSize = true;
            this._NO_TRANSLATE_RevertLabel.Location = new System.Drawing.Point(13, 13);
            this._NO_TRANSLATE_RevertLabel.Name = "_NO_TRANSLATE_RevertLabel";
            this._NO_TRANSLATE_RevertLabel.Size = new System.Drawing.Size(38, 15);
            this._NO_TRANSLATE_RevertLabel.TabIndex = 0;
            this._NO_TRANSLATE_RevertLabel.Text = "label1";
            // 
            // Revert
            // 
            this.Revert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Revert.Location = new System.Drawing.Point(178, 36);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(138, 25);
            this.Revert.TabIndex = 1;
            this.Revert.Text = "Revert changes";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(322, 36);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(125, 25);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormRevert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(459, 73);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.Revert);
            this.Controls.Add(this._NO_TRANSLATE_RevertLabel);
            this.Name = "FormRevert";
            this.Text = "Revert file changes";
            this.Load += new System.EventHandler(this.FormRevert_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _NO_TRANSLATE_RevertLabel;
        private System.Windows.Forms.Button Revert;
        private System.Windows.Forms.Button btnCancel;
    }
}