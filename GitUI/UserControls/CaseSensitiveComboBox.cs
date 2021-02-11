using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    public class CaseSensitiveComboBox : ComboBox
    {
        private const AutoCompleteMode Mode = AutoCompleteMode.SuggestAppend;
        private string? _lastTextChangedValue;

        private bool SystemAutoCompleteEnabled
        {
            get => Mode != AutoCompleteMode.None && DropDownStyle != ComboBoxStyle.DropDownList;
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            if (SystemAutoCompleteEnabled)
            {
                NotifyAutoComplete();
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (SystemAutoCompleteEnabled)
            {
                string text = Text;
                if (text == _lastTextChangedValue)
                {
                    return;
                }

                _lastTextChangedValue = text;
            }

            base.OnTextChanged(e);
        }

        private int FindStringExactCase(string s)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (GetItemText(Items[i]).Equals(s, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private void NotifyAutoComplete()
        {
            string text = Text;
            bool textChanged = text != _lastTextChangedValue;
            bool selectedIndexSet = false;
            int index;
            if (textChanged && text.Equals(_lastTextChangedValue, StringComparison.OrdinalIgnoreCase))
            {
                index = -1;
            }
            else
            {
                index = FindStringExactCase(text);
            }

            if (index != -1 && index != SelectedIndex)
            {
                SelectedIndex = index;
                SelectionStart = 0;
                SelectionLength = text.Length;
                selectedIndexSet = true;
            }

            if (textChanged && !selectedIndexSet)
            {
                OnTextChanged(EventArgs.Empty);
            }

            _lastTextChangedValue = text;
        }
    }
}
