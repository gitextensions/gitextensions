using System.Windows.Controls;

namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    partial class WpfAutoCompleteTextBoxHost
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
            this._wpfElementHost = new System.Windows.Forms.Integration.ElementHost();
            this._underlyingTextBox = new GitUI.UserControls.AutoCompleteTextBoxes.WpfAutoCompleteTextBoxEx();
            this.SuspendLayout();
            // 
            // _wpfElementHost
            // 
            this._wpfElementHost.AutoSize = true;
            this._wpfElementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this._wpfElementHost.Location = new System.Drawing.Point(0, 2);
            this._wpfElementHost.Name = "_wpfElementHost";
            this._wpfElementHost.Size = new System.Drawing.Size(150, 19);
            this._wpfElementHost.TabIndex = 0;
            this._wpfElementHost.Text = "elementHost1";
            this._wpfElementHost.Child = this._underlyingTextBox;
            // 
            // WpfAutoCompleteTextBoxHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._wpfElementHost);
            this.Name = "WpfAutoCompleteTextBoxHost";
            this.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.Size = new System.Drawing.Size(150, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost _wpfElementHost;
        private WpfAutoCompleteTextBoxEx _underlyingTextBox;
    }
}
