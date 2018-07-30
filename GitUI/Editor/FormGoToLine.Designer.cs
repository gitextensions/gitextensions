namespace GitUI.Editor
{
    partial class FormGoToLine
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGoToLine));
            this.lineLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_LineNumberUpDown = new System.Windows.Forms.NumericUpDown();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_LineNumberUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // lineLabel
            // 
            this.lineLabel.AutoEllipsis = true;
            this.lineLabel.AutoSize = true;
            this.lineLabel.Location = new System.Drawing.Point(12, 9);
            this.lineLabel.Name = "lineLabel";
            this.lineLabel.Size = new System.Drawing.Size(74, 15);
            this.lineLabel.TabIndex = 5;
            this.lineLabel.Text = "Line number";
            // 
            // _NO_TRANSLATE_LineNumberUpDown
            // 
            this._NO_TRANSLATE_LineNumberUpDown.ImeMode = System.Windows.Forms.ImeMode.Off;
            this._NO_TRANSLATE_LineNumberUpDown.Location = new System.Drawing.Point(15, 25);
            this._NO_TRANSLATE_LineNumberUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._NO_TRANSLATE_LineNumberUpDown.Name = "_NO_TRANSLATE_LineNumberUpDown";
            this._NO_TRANSLATE_LineNumberUpDown.Size = new System.Drawing.Size(210, 23);
            this._NO_TRANSLATE_LineNumberUpDown.TabIndex = 0;
            this._NO_TRANSLATE_LineNumberUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(121, 56);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 6;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // okBtn
            // 
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Location = new System.Drawing.Point(40, 56);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 7;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            // 
            // FormGoToLine
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(237, 91);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this._NO_TRANSLATE_LineNumberUpDown);
            this.Controls.Add(this.lineLabel);
            this.Name = "FormGoToLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Go to line";
            this.Load += new System.EventHandler(this.FormGoToLine_Load);
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_LineNumberUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lineLabel;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_LineNumberUpDown;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
    }
}