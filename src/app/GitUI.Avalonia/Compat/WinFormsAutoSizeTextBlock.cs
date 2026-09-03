using Avalonia.Controls;

namespace GitUI.Compat;

/// <summary>
///  Applies the WinForms Label preferred-width boundary to ported AutoSize labels.
/// </summary>
internal static class WinFormsAutoSizeTextBlock
{
    private const double TextRendererOverhang = 7;

    public static void Attach(TextBlock textBlock, bool includePadding = false)
    {
        UpdateSize(textBlock, includePadding);
        textBlock.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name is nameof(TextBlock.Text)
                or nameof(TextBlock.FontFamily)
                or nameof(TextBlock.FontSize)
                or nameof(TextBlock.FontStyle)
                or nameof(TextBlock.FontWeight))
            {
                UpdateSize(textBlock, includePadding);
            }
        };
    }

    private static void UpdateSize(TextBlock textBlock, bool includePadding)
    {
        // Framework constraint: WinForms AutoSize Label uses TextRenderer's GDI overhang.
        Avalonia.Size textSize = WinFormsTextMeasurer.MeasureSize(textBlock, textBlock.Text ?? string.Empty);
        textBlock.Width = Math.Ceiling(textSize.Width
            + TextRendererOverhang
            + (includePadding ? textBlock.Padding.Left + textBlock.Padding.Right : 0));
        if (includePadding)
        {
            textBlock.Height = Math.Ceiling(textSize.Height + textBlock.Padding.Top + textBlock.Padding.Bottom);
        }
    }
}
