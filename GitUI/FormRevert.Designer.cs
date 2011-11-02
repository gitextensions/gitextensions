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
            System.Windows.Forms.Panel panel1;
            this.btnCancel = new System.Windows.Forms.Button();
            this.Revert = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_RevertLabel = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(this.btnCancel);
            panel1.Controls.Add(this.Revert);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 48);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(459, 41);
            panel1.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(272, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(178, 25);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // Revert
            // 
            this.Revert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Revert.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Revert.Location = new System.Drawing.Point(88, 4);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(178, 25);
            this.Revert.TabIndex = 3;
            this.Revert.Text = "Revert changes";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // _NO_TRANSLATE_RevertLabel
            // 
            this._NO_TRANSLATE_RevertLabel.AutoSize = true;
            this._NO_TRANSLATE_RevertLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_RevertLabel.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_RevertLabel.MaximumSize = new System.Drawing.Size(460, 0);
            this._NO_TRANSLATE_RevertLabel.Name = "_NO_TRANSLATE_RevertLabel";
            this._NO_TRANSLATE_RevertLabel.Padding = new System.Windows.Forms.Padding(15);
            this._NO_TRANSLATE_RevertLabel.Size = new System.Drawing.Size(80, 50);
            this._NO_TRANSLATE_RevertLabel.TabIndex = 5;
            this._NO_TRANSLATE_RevertLabel.Text = "label1";
            // 
            // FormRevert
            // 
            this.AcceptButton = this.Revert;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(459, 89);
            this.Controls.Add(this._NO_TRANSLATE_RevertLabel);
            this.Controls.Add(panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormRevert";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Revert file changes";
            this.Load += new System.EventHandler(this.FormRevert_Load);
            panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button Revert;
        private System.Windows.Forms.Label _NO_TRANSLATE_RevertLabel;
    }
}