using System.ComponentModel;
using Avalonia.Input;

namespace GitUI.Compat.WinFormsEvents;

/// <summary>
/// Preserves the cancellable WinForms <c>TextBox.Validating</c> boundary on Avalonia.
/// </summary>
public sealed class TextBox : Avalonia.Controls.TextBox
{
    public TextBox()
    {
        LosingFocus += OnLosingFocus;
    }

    public event EventHandler<CancelEventArgs>? Validating;

    // Framework constraint: Avalonia derived controls must opt into the base TextBox theme template.
    protected override Type StyleKeyOverride => typeof(Avalonia.Controls.TextBox);

    private void OnLosingFocus(object? sender, FocusChangingEventArgs e)
    {
        CancelEventArgs validatingEvent = new();
        Validating?.Invoke(this, validatingEvent);
        if (validatingEvent.Cancel)
        {
            e.TryCancel();
        }
    }
}
