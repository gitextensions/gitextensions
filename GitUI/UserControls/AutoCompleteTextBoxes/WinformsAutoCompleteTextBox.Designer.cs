namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    partial class WinformsAutoCompleteTextBox
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
            this._underlyingTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _underlyingTextBox
            // 
            this._underlyingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._underlyingTextBox.BackColor = System.Drawing.SystemColors.Window;
            this._underlyingTextBox.Location = new System.Drawing.Point(0, 0);
            this._underlyingTextBox.Margin = new System.Windows.Forms.Padding(0);
            this._underlyingTextBox.Name = "_underlyingTextBox";
            this._underlyingTextBox.Size = new System.Drawing.Size(150, 20);
            this._underlyingTextBox.TabIndex = 0;
            // 
            // WinformsAutoCompleteTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._underlyingTextBox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "WinformsAutoCompleteTextBox";
            this.Size = new System.Drawing.Size(150, 20);
            this.Resize += new System.EventHandler(this.WinformsAutoCompleteTextBox_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _underlyingTextBox;
    }
}
