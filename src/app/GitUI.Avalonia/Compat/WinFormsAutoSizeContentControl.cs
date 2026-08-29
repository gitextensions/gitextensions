using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace GitUI.Compat;

/// <summary>
/// Applies the WinForms TextRenderer preferred-size boundary to ported AutoSize content controls.
/// </summary>
internal static class WinFormsAutoSizeContentControl
{
    public static void Attach(TemplatedControl control, double nonTextWidth, double minimumHeight)
    {
        UpdateSize(control, nonTextWidth, minimumHeight);
        control.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name is nameof(ContentControl.Content)
                or nameof(TemplatedControl.FontFamily)
                or nameof(TemplatedControl.FontSize)
                or nameof(TemplatedControl.FontStyle)
                or nameof(TemplatedControl.FontWeight))
            {
                UpdateSize(control, nonTextWidth, minimumHeight);
            }
        };
    }

    private static void UpdateSize(TemplatedControl control, double nonTextWidth, double minimumHeight)
    {
        string text = control is ContentControl contentControl
            ? contentControl.Content switch
            {
                string value => value,
                TextBlock textBlock => textBlock.Text ?? string.Empty,
                _ => string.Empty,
            }
            : string.Empty;
        text = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(text);
        Avalonia.Size textSize = WinFormsTextMeasurer.MeasureSize(control, text);
        control.Width = Math.Ceiling(textSize.Width + nonTextWidth);
        control.Height = Math.Max(minimumHeight, Math.Ceiling(textSize.Height));
    }
}
