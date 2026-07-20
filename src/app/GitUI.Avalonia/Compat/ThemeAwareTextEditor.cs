using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>
///  An AvaloniaEdit control that keeps built-in syntax colors readable in the dark theme.
/// </summary>
public sealed class ThemeAwareTextEditor : TextEditor
{
    public ThemeAwareTextEditor()
    {
        UpdateLinkForeground();
        ActualThemeVariantChanged += (_, _) =>
        {
            UpdateLinkForeground();
            TextArea.TextView.Redraw();
        };
    }

    protected override Type StyleKeyOverride => typeof(TextEditor);

    protected override IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
        => new ThemeAwareHighlightingColorizer(highlightingDefinition, this);

    private void UpdateLinkForeground()
    {
        TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(
            ActualThemeVariant == ThemeVariant.Dark
                ? MediaColor.Parse("#4DA3FF")
                : MediaColor.Parse("#0563C1"));
    }

    private sealed class ThemeAwareHighlightingColorizer : HighlightingColorizer
    {
        private const double MinimumDarkForegroundLuminance = 0.68;
        private const double BlendStep = 0.05;

        private readonly ThemeAwareTextEditor _editor;

        public ThemeAwareHighlightingColorizer(
            IHighlightingDefinition highlightingDefinition,
            ThemeAwareTextEditor editor)
            : base(highlightingDefinition)
        {
            _editor = editor;
        }

        protected override void ApplyColorToElement(VisualLineElement element, HighlightingColor color)
        {
            base.ApplyColorToElement(element, color);
            if (_editor.ActualThemeVariant != ThemeVariant.Dark
                || color.Foreground is null
                || element.TextRunProperties.ForegroundBrush is not ISolidColorBrush foreground)
            {
                return;
            }

            MediaColor contrastSafeColor = EnsureMinimumLuminance(foreground.Color);
            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(contrastSafeColor));
        }

        private static MediaColor EnsureMinimumLuminance(MediaColor color)
        {
            if (GetRelativeLuminance(color) >= MinimumDarkForegroundLuminance)
            {
                return color;
            }

            for (double amount = BlendStep; amount <= 1; amount += BlendStep)
            {
                MediaColor candidate = MediaColor.FromArgb(
                    color.A,
                    BlendWithWhite(color.R, amount),
                    BlendWithWhite(color.G, amount),
                    BlendWithWhite(color.B, amount));
                if (GetRelativeLuminance(candidate) >= MinimumDarkForegroundLuminance)
                {
                    return candidate;
                }
            }

            return Colors.White;
        }

        private static byte BlendWithWhite(byte component, double amount)
            => (byte)Math.Round(component + ((byte.MaxValue - component) * amount));

        private static double GetRelativeLuminance(MediaColor color)
            => (0.2126 * ToLinear(color.R)) + (0.7152 * ToLinear(color.G)) + (0.0722 * ToLinear(color.B));

        private static double ToLinear(byte component)
        {
            double channel = component / (double)byte.MaxValue;
            return channel <= 0.04045
                ? channel / 12.92
                : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }
    }
}
