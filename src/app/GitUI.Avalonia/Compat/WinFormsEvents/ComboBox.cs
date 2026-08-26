using Avalonia;

namespace GitUI.Compat.WinFormsEvents;

/// <summary>
/// Exposes the WinForms <c>ComboBox.TextChanged</c> event over Avalonia's text property.
/// </summary>
public sealed class ComboBox : Avalonia.Controls.ComboBox
{
    private string? _lastText = string.Empty;

    public event EventHandler? TextChanged;

    // Framework constraint: Avalonia derived controls must opt into the base ComboBox theme template.
    protected override Type StyleKeyOverride => typeof(Avalonia.Controls.ComboBox);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != TextProperty || string.Equals(_lastText, Text, StringComparison.Ordinal))
        {
            return;
        }

        _lastText = Text;
        TextChanged?.Invoke(this, EventArgs.Empty);
    }
}
