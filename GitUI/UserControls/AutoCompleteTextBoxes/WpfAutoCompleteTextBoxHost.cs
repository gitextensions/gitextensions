using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace GitUI.UserControls.AutoCompleteTextBoxes
{
    public partial class WpfAutoCompleteTextBoxHost : AutoCompleteTextBox
    {
        private AutoCompleteMode _autoCompleteMode;

        public WpfAutoCompleteTextBoxHost()
        {
            InitializeComponent();
            _underlyingTextBox.FilterMode = AutoCompleteFilterMode.Contains;
            _underlyingTextBox.TextChanged += RaiseTextChangedEvent;
            _underlyingTextBox.PreviewKeyDown += (s, e) =>
            {
                var args = new PreviewKeyDownEventArgs((Keys)KeyInterop.VirtualKeyFromKey(e.Key));
                RaisePreviewKeyDownEvent(s, args);
            };
            _underlyingTextBox.KeyDown += (s, e) =>
            {
                var args = new KeyEventArgs((Keys)KeyInterop.VirtualKeyFromKey(e.Key));
                RaiseKeyDownEvent(s, args);
            };
        }

        public override AutoCompleteMode AutoCompleteMode
        {
            get { return _autoCompleteMode; }
            set
            {
                _autoCompleteMode = value;
                _underlyingTextBox.IsTextCompletionEnabled = value == AutoCompleteMode.SuggestAppend
                    || value == AutoCompleteMode.Append;
            }
        }

        public override AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get { return _underlyingTextBox.ItemsSource as AutoCompleteStringCollection; }
            set { _underlyingTextBox.ItemsSource = value; }
        }

        public override string Text
        {
            get { return _underlyingTextBox.Text; }
            set { _underlyingTextBox.Text = value; }
        }
    }
}
