using System.Collections.Concurrent;

namespace GitExtUtils.GitUI.Theming;

public static class ColorHelper
{
    private static readonly ConcurrentDictionary<(Color fore, Color back), Color> _foreColorForBackColors = new();

    private static readonly (KnownColor back, KnownColor fore)[] BackForeExamples =
    [
        (KnownColor.Window, KnownColor.WindowText),
        (KnownColor.Control, KnownColor.ControlText),
        (KnownColor.Info, KnownColor.InfoText),
        (KnownColor.Highlight, KnownColor.HighlightText),
        (KnownColor.MenuHighlight, KnownColor.HighlightText),
    ];

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

    public static void SetForeColorForBackColor(this Control control)
        => control.ForeColor = control.ForeColor.AdaptForeColor(control.BackColor);

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

    /// <summary>
    ///  Get a color to be used instead of SystemColors.GrayText
    ///  when background is SystemColors.Highlight or SystemColors.MenuHighlight.
    /// </summary>
    /// <remarks>
    ///  Consider a transformation of color range [SystemColors.ControlText, SystemColors.Control] to
    ///  [SystemColors.HighlightText, SystemColors.Highlight].
    ///  What result would such transformation produce given SystemColors.GrayText as input?
    ///  First we calculate transformed GrayText color relative to InvariantTheme.
    ///  Then we apply transformation from InvariantTheme to current theme by calling AdaptTextColor.
    ///  Only used in Light mode.
    /// </remarks>
    public static Color GetHighlightGrayTextColor(
        KnownColor backgroundColorName,
        KnownColor textColorName,
        KnownColor highlightColorName,
        float degreeOfGrayness = 1f)
    {
        HslColor grayTextHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(KnownColor.GrayText));
        HslColor textHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(textColorName));
        HslColor highlightTextHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(KnownColor.HighlightText));
        HslColor backgroundHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(backgroundColorName));
        HslColor highlightBackgroundHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(highlightColorName));

        double grayTextL = textHsl.L + (degreeOfGrayness * (grayTextHsl.L - textHsl.L));

        double highlightGrayTextL = Transform(
            grayTextL,
            textHsl.L, backgroundHsl.L,
            highlightTextHsl.L, highlightBackgroundHsl.L);

        HslColor highlightGrayTextHsl = grayTextHsl.WithLuminosity(highlightGrayTextL);
        return AdaptColor(highlightGrayTextHsl.ToColor(), isForeground: true);
    }

    /// <summary>
    ///  Get a color to be used instead of <see cref="SystemColors.GrayText"/> which is more or less gray than
    ///  the usual <see cref="SystemColors.GrayText"/>.
    ///  Only used in Light mode.
    /// </summary>
    /// <param name="textColorName">The name of the text color to base the grayness on.</param>
    /// <param name="degreeOfGrayness">How gray the result should be; 1.0 is full gray, 0.0 is same as text.</param>
    public static Color GetGrayTextColor(KnownColor textColorName, float degreeOfGrayness = 1f)
    {
        HslColor grayTextHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(KnownColor.GrayText));
        HslColor textHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(textColorName));

        double grayTextL = textHsl.L + (degreeOfGrayness * (grayTextHsl.L - textHsl.L));
        HslColor highlightGrayTextHsl = grayTextHsl.WithLuminosity(grayTextL);
        return AdaptColor(highlightGrayTextHsl.ToColor(), isForeground: true);
    }

    /// <summary>
    ///  Adapt invariant background colors to the current theme,
    ///  by comparing to Text/Background color pairs in the invariant theme.
    ///  Note that <see cref="SystemColors"/> and <see cref="AppColor"/> should not be adapted.
    /// </summary>
    /// <param name="original">The original <see cref="Color"/></param>
    /// <returns>The adapted color.</returns>
    public static Color AdaptBackColor(this Color original)
        => AdaptColor(original, isForeground: false);

    private static Color AdaptColor(Color original, bool isForeground)
    {
        if (IsDefaultTheme)
        {
            return original;
        }

        HslColor hsl1 = original.ToPerceptedHsl();

        int index = Enumerable.Range(0, BackForeExamples.Length)
            .Min(i => (
                 distance: DistanceTo(
                    isForeground ? BackForeExamples[i].fore : BackForeExamples[i].back),
                 index: i)).index;

        (KnownColor back, KnownColor fore) option = BackForeExamples[index];
        return isForeground
            ? AdaptColor(original, option.fore, option.back)
            : AdaptColor(original, option.back, option.fore);

        double DistanceTo(KnownColor c2)
        {
            HslColor hsl2 = ThemeSettings.InvariantTheme.GetNonEmptyColor(c2).ToPerceptedHsl();
            return Math.Abs(hsl1.L - hsl2.L) + (0.25 * Math.Abs(hsl1.S - hsl2.S));
        }
    }

    /// <remarks>0.05 is subtle. 0.3 is quite strong.</remarks>
    public static Color MakeDarkerBy(this Color color, double amount) =>
        color.TransformHsl(l: l => l - amount);

    public static void AdaptImageLightness(this ToolStripItem item) =>
        item.Image = ((Bitmap?)item.Image)?.AdaptLightness();

    public static void AdaptImageLightness(this ButtonBase button) =>
        button.Image = ((Bitmap?)button.Image)?.AdaptLightness();

    public static Bitmap AdaptLightness(this Bitmap original)
    {
        if (IsDefaultTheme)
        {
            return original;
        }

        Bitmap clone = (Bitmap)original.Clone();
        new LightnessCorrection(clone).Execute();
        return clone;
    }

    /// <summary>
    /// Transform the invariant color to be related to the known colors
    /// in the same way in the current theme as in the invariant theme.
    /// </summary>
    /// <param name="original">Original color.</param>
    /// <param name="exampleName">The name of foreground/background for the pair.</param>
    /// <param name="oppositeName">The name of background/foreground for the pair.</param>
    /// <returns>The transformed color.</returns>
    private static Color AdaptColor(Color original, KnownColor exampleName, KnownColor oppositeName)
    {
        Color exampleOriginal = ThemeSettings.InvariantTheme.GetNonEmptyColor(exampleName);
        Color oppositeOriginal = ThemeSettings.InvariantTheme.GetNonEmptyColor(oppositeName);
        Color example = ThemeSettings.Theme.GetNonEmptyColor(exampleName);
        Color opposite = ThemeSettings.Theme.GetNonEmptyColor(oppositeName);

        if (ThemeSettings.Variations.Contains(ThemeVariations.Colorblind))
        {
            original = original.AdaptToColorblindness();
        }

        (Color rgb, HslColor hsl) exampleOrigRgbHsl = RgbHsl(exampleOriginal);
        (Color rgb, HslColor hsl) oppositeOrigRgbHsl = RgbHsl(oppositeOriginal);
        (Color rgb, HslColor hsl) exampleRgbHsl = RgbHsl(example);
        (Color rgb, HslColor hsl) oppositeRgbHsl = RgbHsl(opposite);
        (Color rgb, HslColor hsl) originalRgbHsl = RgbHsl(original);

        double perceptedL = Transform(
            PerceptedL(originalRgbHsl),
            PerceptedL(exampleOrigRgbHsl),
            PerceptedL(oppositeOrigRgbHsl),
            PerceptedL(exampleRgbHsl),
            PerceptedL(oppositeRgbHsl));

        double actualL = ActualL(originalRgbHsl.rgb, perceptedL);

        // Keep the original alpha (not expected to have alpha!=255 in the SystemColors)
        Color result = originalRgbHsl.hsl.WithLuminosity(actualL).ToColor();
        result = Color.FromArgb(original.A, result);
        return result;

        double PerceptedL((Color rgb, HslColor hsl) c) =>
            ColorHelper.PerceptedL(c.rgb, c.hsl.L);

        (Color rgb, HslColor hsl) RgbHsl(Color c) =>
            (c, new HslColor(c));
    }

    /// <summary>
    /// Roughly speaking makes a linear transformation of input orig 0 &lt; orig &lt; 1
    /// The transformation is specified by how it affected a pair of values
    /// (exampleOrig, oppositeOrig) to (example, opposite).
    /// </summary>
    private static double Transform(double orig,
        double exampleOrig, double oppositeOrig,
        double example, double opposite)
    {
        // if the value we are transforming was outside segment [example, opposite]
        // then transform it with segment [0, example] or [opposite, 1] instead
        if (oppositeOrig < exampleOrig && exampleOrig < orig)
        {
            oppositeOrig = 1d;
            opposite = opposite < example ? 1d : 0d;
        }
        else if (orig < exampleOrig && exampleOrig < oppositeOrig)
        {
            oppositeOrig = 0d;
            opposite = example < opposite ? 0d : 1d;
        }

        double deltaExample = example - opposite;
        double deltaExampleOrig = exampleOrig - oppositeOrig;

        // prevent division by zero
        if (Math.Abs(deltaExampleOrig) < 0.01)
        {
            oppositeOrig = (oppositeOrig - 0.2).WithinRange(0, 1);
            exampleOrig = (exampleOrig + 0.2).WithinRange(0, 1);
            deltaExampleOrig = exampleOrig - oppositeOrig;
        }

        double deltaOrig = orig - oppositeOrig;

        // delta / deltaOrig = deltaExample / deltaExampleOrig
        double result = opposite + (deltaOrig / deltaExampleOrig * deltaExample);
        return result.WithinRange(0, 1);
    }

    private static Color TransformHsl(
        this Color c,
        Func<double, double>? h = null,
        Func<double, double>? s = null,
        Func<double, double>? l = null)
    {
        HslColor hsl = new(c);
        HslColor transformed = new(
            h?.Invoke(hsl.H) ?? hsl.H,
            s?.Invoke(hsl.S) ?? hsl.S,
            l?.Invoke(hsl.L) ?? hsl.L);
        return transformed.ToColor();
    }

    private static double PerceptedL(Color rgb, double l) =>
        l.GammaTransform(Gamma(rgb));

    private static double ActualL(Color rgb, double percepted) =>
        percepted.GammaTransform(1d / Gamma(rgb));

    private static double GammaTransform(this double l, double gamma)
    {
        double l0 = gamma / (gamma + 1);
        if (l < l0)
        {
            return l / gamma;
        }

        return 1 + (gamma * (l - 1));
    }

    private static double Gamma(Color c)
    {
        if (c.R == c.G && c.G == c.B)
        {
            return 1;
        }

        const double r = 0.8;
        const double g = 1.75;
        const double b = 0.45;

        return (c.R + c.G + c.B) / ((c.R * r) + (c.G * g) + (c.B * b));
    }

    public static HslColor ToPerceptedHsl(this Color rgb)
    {
        HslColor hsl = new(rgb);
        return hsl.WithLuminosity(PerceptedL(rgb, hsl.L));
    }

    public static HslColor ToActualHsl(this HslColor hsl, Color rgb) =>
        hsl.WithLuminosity(ActualL(rgb, hsl.L));

    private static Color AdaptToColorblindness(this Color color)
    {
        double excludeHTo = 15d; // orange

        HslColor hsl = new(color);
        double deltaH = ((hsl.H * 360d) - excludeHTo + 180).Modulo(360) - 180;

        const double deltaFrom = -140d;
        const double deltaTo = 0d;

        if (deltaH <= deltaFrom || deltaH >= deltaTo)
        {
            return color;
        }

        double correctedDelta = deltaFrom + ((deltaH - deltaFrom) / 2d);
        double correctedH = (excludeHTo + correctedDelta).Modulo(360);
        return new HslColor(correctedH / 360d, hsl.S, hsl.L).ToColor();
    }

    /// <summary>
    /// Find if the theme is the default.
    /// Note that the theme is parsed, so ThemeSettings.DefaultLight is another instance.
    /// </summary>
    /// <returns><see langword="true"/> if the theme is default; otherwise <see langword="false"/>.</returns>
    private static bool IsDefaultTheme => ThemeSettings.Theme.Id == ThemeId.DefaultLight;

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

    /// <summary>
    /// Get contrast color (Black or White) of a background color to use as a foreground color (not using the theme settings).
    /// </summary>
    /// <param name="backgroundColor">the background color</param>
    /// <param name="luminanceThreshold">
    ///  scaled to the WCAG relative luminance threshold; so 0.5 corresponds to 0.18,
    ///  which is approximately the WCAG equal-contrast midpoint between black and white.
    ///  Therefore "force all" to white is about 5.5.
    ///  Note that the rgb midpoint is closer to 77 than 7f, not linear.
    /// </param>
    /// <returns>the black or white color to use as foreground</returns>
    public static Color GetContrastColor(this Color backgroundColor, float luminanceThreshold)
    {
        // scale so 0.5 corresponds to the WCAG equal-contrast midpoint between black and white
        const float wcagEqualContrastMidpoint = 0.185f;
        luminanceThreshold = luminanceThreshold * (wcagEqualContrastMidpoint * 2.0f);
        double luminance = WcagRelativeLuminance(backgroundColor);
        return luminance > luminanceThreshold ? Color.Black : Color.White;
    }

    private static double WcagRelativeLuminance(Color c) =>
        (0.2126 * SrgbLinearize(c.R)) + (0.7152 * SrgbLinearize(c.G)) + (0.0722 * SrgbLinearize(c.B));

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

    internal static class TestAccessor
    {
        public static double Transform(double orig, double exampleOrig, double oppositeOrig, double example, double opposite) =>
            ColorHelper.Transform(orig, exampleOrig, oppositeOrig, example, opposite);

        public static Color GetContrastColor(Color backgroundColor, float luminanceThreshold) =>
            backgroundColor.GetContrastColor(luminanceThreshold);
    }
}
