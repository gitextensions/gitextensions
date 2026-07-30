using System.Collections.Concurrent;

namespace GitExtUtils.GitUI.Theming;

/// <summary>
/// Portable color-only subset of <c>ColorHelper</c> for the Avalonia build.
/// </summary>
/// <remarks>
/// The Windows source also contains GDI bitmap and WinForms control helpers. Keeping the
/// color computations here byte-for-byte equivalent lets portable UI consumers use the
/// original theme math without bringing those rendering dependencies into <c>net10.0</c>.
/// </remarks>
public static class ColorHelper
{
    private static readonly ConcurrentDictionary<(Color fore, Color back), Color> _foreColorForBackColors = new();

    public static ThemeSettings ThemeSettings { private get; set; } = ThemeSettings.Default;

    /// <summary>
    ///  Blends the color with the current editor background color at 50% in linear light (sRGB gamma-corrected) space,
    ///  producing a perceptually correct midpoint. The original alpha is preserved.
    /// </summary>
    public static Color DimColor(this Color color)
    {
        Color background = ThemeSettings.Theme.GetColor(AppColor.EditorBackground);
        byte r = SrgbDelinearize((SrgbLinearize(color.R) + SrgbLinearize(background.R)) * 0.5);
        byte g = SrgbDelinearize((SrgbLinearize(color.G) + SrgbLinearize(background.G)) * 0.5);
        byte b = SrgbDelinearize((SrgbLinearize(color.B) + SrgbLinearize(background.B)) * 0.5);
        return Color.FromArgb(color.A, r, g, b);
    }

    public static Color GetTextColor(this Color backColor)
        => ThemeSettings.Theme.GetNonEmptyColor(KnownColor.WindowText).AdaptForeColor(backColor);

    public static Color AdaptForeColor(
        this Color original, Color backColor)
    {
        if (backColor == Color.Empty)
        {
            backColor = ThemeSettings.Theme.GetColor(AppColor.PanelBackground);
        }

        (Color fore, Color back) key = (original, backColor);

        if (_foreColorForBackColors.TryGetValue(key, out Color cachedColor))
        {
            return cachedColor;
        }

        Color foreColor = EnsureContrast(backColor, original);
        _foreColorForBackColors[key] = foreColor;
        return foreColor;
    }

    public static Color Lerp(Color colour, Color to, float amount)
    {
        // start colours as lerp-able floats
        float sr = colour.R, sg = colour.G, sb = colour.B;

        // end colours as lerp-able floats
        float er = to.R, eg = to.G, eb = to.B;

        // lerp the colours to get the difference
        byte r = (byte)Lerp(sr, er),
            g = (byte)Lerp(sg, eg),
            b = (byte)Lerp(sb, eb);

        // return the new colour
        return Color.FromArgb(r, g, b);

        float Lerp(float start, float end)
        {
            float difference = end - start;
            float adjusted = difference * amount;
            return start + adjusted;
        }
    }

    private static double SrgbLinearize(byte channel)
    {
        double normalized = channel / 255.0;
        return normalized <= 0.04045 ? normalized / 12.92 : Math.Pow((normalized + 0.055) / 1.055, 2.4);
    }

    private static byte SrgbDelinearize(double linear)
    {
        double normalized = linear <= 0.0031308 ? 12.92 * linear : (1.055 * Math.Pow(linear, 1.0 / 2.4)) - 0.055;
        return (byte)Math.Round(Math.Clamp(normalized * 255.0, 0.0, 255.0));
    }

    private static double WcagRelativeLuminance(Color c) =>
        (0.2126 * SrgbLinearize(c.R)) + (0.7152 * SrgbLinearize(c.G)) + (0.0722 * SrgbLinearize(c.B));

    private static double WcagContrastRatio(Color c1, Color c2)
    {
        double l1 = WcagRelativeLuminance(c1);
        double l2 = WcagRelativeLuminance(c2);
        return l1 > l2 ? (l1 + 0.05) / (l2 + 0.05) : (l2 + 0.05) / (l1 + 0.05);
    }

    /// <summary>
    ///  Adjusts <paramref name="foreground"/> luminance until <paramref name="ratio"/> contrast is met against
    ///  <paramref name="background"/>, mirroring the xterm.js algorithm used by VS Code.
    ///  If neither direction reaches the target, the result with the higher contrast ratio is returned.
    /// </summary>
    private static Color EnsureContrast(Color background, Color foreground, double ratio = 4.5)
    {
        if (WcagContrastRatio(background, foreground) >= ratio)
        {
            return foreground;
        }

        double foregroundLuminance = WcagRelativeLuminance(foreground);
        double backgroundLuminance = WcagRelativeLuminance(background);

        Color resultA = foregroundLuminance < backgroundLuminance
            ? ReduceLuminance(background, foreground, ratio)
            : IncreaseLuminance(background, foreground, ratio);

        if (WcagContrastRatio(background, resultA) >= ratio)
        {
            return resultA;
        }

        Color resultB = foregroundLuminance < backgroundLuminance
            ? IncreaseLuminance(background, foreground, ratio)
            : ReduceLuminance(background, foreground, ratio);

        return WcagContrastRatio(background, resultA) >= WcagContrastRatio(background, resultB)
            ? resultA
            : resultB;
    }

    private static Color ReduceLuminance(Color background, Color foreground, double ratio)
    {
        int r = foreground.R, g = foreground.G, b = foreground.B;
        Color current = foreground;
        while (WcagContrastRatio(background, current) < ratio && (r > 0 || g > 0 || b > 0))
        {
            r -= (int)Math.Ceiling(r * 0.1);
            g -= (int)Math.Ceiling(g * 0.1);
            b -= (int)Math.Ceiling(b * 0.1);
            current = Color.FromArgb(foreground.A, r, g, b);
        }

        return current;
    }

    private static Color IncreaseLuminance(Color background, Color foreground, double ratio)
    {
        int r = foreground.R, g = foreground.G, b = foreground.B;
        Color current = foreground;
        while (WcagContrastRatio(background, current) < ratio && (r < 255 || g < 255 || b < 255))
        {
            r = Math.Min(255, r + (int)Math.Ceiling((255 - r) * 0.1));
            g = Math.Min(255, g + (int)Math.Ceiling((255 - g) * 0.1));
            b = Math.Min(255, b + (int)Math.Ceiling((255 - b) * 0.1));
            current = Color.FromArgb(foreground.A, r, g, b);
        }

        return current;
    }
}
