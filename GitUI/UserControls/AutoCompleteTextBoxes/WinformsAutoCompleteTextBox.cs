using System.Windows.Forms;

namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    public partial class WinformsAutoCompleteTextBox : AutoCompleteTextBox
    {
        public WinformsAutoCompleteTextBox()
        {
            InitializeComponent();
            _underlyingTextBox.KeyDown += RaiseKeyDownEvent;
            _underlyingTextBox.TextChanged += RaiseTextChangedEvent;
            _underlyingTextBox.PreviewKeyDown += RaisePreviewKeyDownEvent;
        }

        public override string Text
        {
            get { return _underlyingTextBox.Text; }
            set { _underlyingTextBox.Text = value; }
        }

        public override AutoCompleteMode AutoCompleteMode
        {
            get { return _underlyingTextBox.AutoCompleteMode; }
            set { _underlyingTextBox.AutoCompleteMode = value; }
        }

        public override AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get { return _underlyingTextBox.AutoCompleteCustomSource; }
            set { _underlyingTextBox.AutoCompleteCustomSource = value; }
        }

        public override AutoCompleteSource AutoCompleteSource
        {
            get { return _underlyingTextBox.AutoCompleteSource; }
            set { _underlyingTextBox.AutoCompleteSource = value; }
        }

        private void WinformsAutoCompleteTextBox_Resize(object sender, System.EventArgs e)
        {
            _underlyingTextBox.Top = Height/2 - _underlyingTextBox.Height/2;
            _underlyingTextBox.Width = this.Width;
        }
    }
}
