using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GitUI.UserControls;

public class CaseSensitiveComboBox : ComboBox
{
    private string? _lastTextChangedValue;
    private bool _notifyingAutoComplete;

    private bool SystemAutoCompleteEnabled => IsEditable;

    public CaseSensitiveComboBox()
    {
        IsEditable = true;
        LostFocus += OnValidating;
    }

    // Framework constraint: Avalonia derived controls must opt into the base ComboBox theme template.
    protected override Type StyleKeyOverride => typeof(ComboBox);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty && SystemAutoCompleteEnabled && !_notifyingAutoComplete)
        {
            string text = Text ?? string.Empty;
            if (text == _lastTextChangedValue)
            {
                return;
            }

            _lastTextChangedValue = text;
        }
    }

    private void OnValidating(object? sender, RoutedEventArgs e)
    {
        if (SystemAutoCompleteEnabled)
        {
            NotifyAutoComplete();
        }
    }

    private int FindStringExactCase(string s)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if ((Items[i]?.ToString() ?? string.Empty).Equals(s, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private void NotifyAutoComplete()
    {
        string text = Text ?? string.Empty;
        bool textChanged = text != _lastTextChangedValue;
        int index;
        if (textChanged && text.Equals(_lastTextChangedValue, StringComparison.OrdinalIgnoreCase))
        {
            index = -1;
        }
        else
        {
            index = FindStringExactCase(text);
        }

        try
        {
            _notifyingAutoComplete = true;
            if (index != -1 && index != SelectedIndex)
            {
                SelectedIndex = index;
                Text = text;
            }
        }
        finally
        {
            _notifyingAutoComplete = false;
            _lastTextChangedValue = text;
        }
    }

    // parity-scaffolding: Invokes the validating boundary without moving real desktop focus.
    internal void NotifyAutoCompleteForTest() => NotifyAutoComplete();
}
