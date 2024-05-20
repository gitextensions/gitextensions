namespace GitUI.Editor
{
    partial class FormGoToLine
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGoToLine));
            lineLabel = new Label();
            _NO_TRANSLATE_LineNumberUpDown = new NumericUpDown();
            cancelBtn = new Button();
            okBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_LineNumberUpDown)).BeginInit();
            SuspendLayout();
            // 
            // lineLabel
            // 
            lineLabel.AutoEllipsis = true;
            lineLabel.AutoSize = true;
            lineLabel.Location = new Point(12, 9);
            lineLabel.Name = "lineLabel";
            lineLabel.Size = new Size(74, 15);
            lineLabel.TabIndex = 5;
            lineLabel.Text = "Line number";
            // 
            // _NO_TRANSLATE_LineNumberUpDown
            // 
            _NO_TRANSLATE_LineNumberUpDown.ImeMode = ImeMode.Off;
            _NO_TRANSLATE_LineNumberUpDown.Location = new Point(15, 25);
            _NO_TRANSLATE_LineNumberUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            _NO_TRANSLATE_LineNumberUpDown.Name = "_NO_TRANSLATE_LineNumberUpDown";
            _NO_TRANSLATE_LineNumberUpDown.Size = new Size(210, 23);
            _NO_TRANSLATE_LineNumberUpDown.TabIndex = 0;
            _NO_TRANSLATE_LineNumberUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cancelBtn
            // 
            cancelBtn.DialogResult = DialogResult.Cancel;
            cancelBtn.Location = new Point(121, 56);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(75, 23);
            cancelBtn.TabIndex = 6;
            cancelBtn.Text = "Cancel";
            cancelBtn.UseVisualStyleBackColor = true;
            // 
            // okBtn
            // 
            okBtn.DialogResult = DialogResult.OK;
            okBtn.Location = new Point(40, 56);
            okBtn.Name = "okBtn";
            okBtn.Size = new Size(75, 23);
            okBtn.TabIndex = 7;
            okBtn.Text = "OK";
            okBtn.UseVisualStyleBackColor = true;
            // 
            // FormGoToLine
            // 
            AcceptButton = okBtn;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(237, 91);
            Controls.Add(okBtn);
            Controls.Add(cancelBtn);
            Controls.Add(_NO_TRANSLATE_LineNumberUpDown);
            Controls.Add(lineLabel);
            Name = "FormGoToLine";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Go to line";
            Load += FormGoToLine_Load;
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_LineNumberUpDown)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label lineLabel;
        private NumericUpDown _NO_TRANSLATE_LineNumberUpDown;
        private Button cancelBtn;
        private Button okBtn;
    }
}