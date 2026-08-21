using Avalonia.Controls;

namespace GitUI.Compat.Components;

/// <summary>Preserves the WinForms component-shaped tooltip boundary over Avalonia's attached property.</summary>
/// <remarks>Consumed by directly ported forms whose original Designer owns a <c>ToolTip</c> component.</remarks>
internal sealed class ToolTip
{
    public void SetToolTip(Control control, object? value)
    {
        Avalonia.Controls.ToolTip.SetTip(control, value);
    }
}
