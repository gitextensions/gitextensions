using System;

namespace GitUI.Editor
{
    public partial class FormGoToLine : GitExtensionsForm
    {
        public FormGoToLine()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public int GetLineNumber()
        {
            return (int)_NO_TRANSLATE_LineNumberUpDown.Value;
        }

        public void SetMaxLineNumber(int maxLineNumber)
        {
            _NO_TRANSLATE_LineNumberUpDown.Maximum = maxLineNumber;
            lineLabel.Text = lineLabel.Text + " (1 - " + maxLineNumber + "):";
        }

        private void FormGoToLine_Load(object sender, EventArgs e)
        {
            _NO_TRANSLATE_LineNumberUpDown.Select(0, _NO_TRANSLATE_LineNumberUpDown.ToString().Length);
        }
    }
}
