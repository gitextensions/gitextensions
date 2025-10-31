namespace GitExtUtils.GitUI.Theming;

public static class ColorHelper
{
    private static readonly (KnownColor back, KnownColor fore)[] BackForeExamples =
    {
        (KnownColor.Window, KnownColor.WindowText),
        (KnownColor.Control, KnownColor.ControlText),
        (KnownColor.Info, KnownColor.InfoText),
        (KnownColor.Highlight, KnownColor.HighlightText),
        (KnownColor.MenuHighlight, KnownColor.HighlightText),
    };

    public static ThemeSettings ThemeSettings { private get; set; } = ThemeSettings.Default;

    /// <summary>
    ///  Blends the color with the default Editor background color, halves each value first.
    ///  Keep the original alpha, SystemColors.Window.A==255 and we do not want rounding errors.
    /// </summary>
    public static Color DimColor(Color color)
    {
        const uint maskWithoutLeastSignificantBits = 0xFE_FE_FE_FE;
        uint defaultBackground = (uint)ThemeSettings.Theme.GetColor(AppColor.EditorBackground).ToArgb();
        int dimCode = (int)((((uint)color.ToArgb() & maskWithoutLeastSignificantBits) >> 1) + ((defaultBackground & maskWithoutLeastSignificantBits) >> 1));
        return Color.FromArgb(color.A, (dimCode >> 16) & 0xff, (dimCode >> 8) & 0xff, dimCode & 0xff);
    }

    public static void SetForeColorForBackColor(this Control control) =>
        control.ForeColor = GetForeColorForBackColor(control.BackColor);

    public static Color GetForeColorForBackColor(Color backColor)
    {
        HslColor hsl1 = backColor.ToPerceptedHsl();

        (double distance, int index) closestBack = Enumerable.Range(0, BackForeExamples.Length)
            .Select(i => (Distance: DistanceTo(BackForeExamples[i].back), Index: i))
            .Min();

        (double distance, int index) closestFore = Enumerable.Range(0, BackForeExamples.Length)
            .Select(i => (Distance: DistanceTo(BackForeExamples[i].fore), Index: i))
            .Min();

        return ThemeSettings.Theme.GetNonEmptyColor(closestBack.distance <= closestFore.distance * 1.25 // prefer back-fore combination
            ? BackForeExamples[closestBack.index].fore
            : BackForeExamples[closestFore.index].back);

        double DistanceTo(KnownColor c2)
        {
            HslColor hsl2 = ThemeSettings.Theme.GetNonEmptyColor(c2).ToPerceptedHsl();
            return
                Math.Abs(hsl1.L - hsl2.L) +
                (0.25 * Math.Abs(hsl1.S - hsl2.S)) +
                (0.25 * (hsl1.H - hsl2.H).Modulo(1));
        }
    }

    /// <summary>
    /// Get a color to be used instead of SystemColors.GrayText
    /// when background is SystemColors.Highlight or SystemColors.MenuHighlight.
    /// </summary>
    /// <remarks>
    /// Consider a transformation of color range [SystemColors.ControlText, SystemColors.Control] to
    /// [SystemColors.HighlightText, SystemColors.Highlight].
    /// What result would such transformation produce given SystemColors.GrayText as input?
    /// First we calculate transformed GrayText color relative to InvariantTheme.
    /// Then we apply transformation from InvariantTheme to current theme by calling AdaptTextColor.
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
        return AdaptTextColor(highlightGrayTextHsl.ToColor());
    }

    /// <summary>
    /// Get a color to be used instead of SystemColors.GrayText which is more or less gray than
    /// the usual SystemColors.GrayText.
    /// </summary>
    public static Color GetGrayTextColor(KnownColor textColorName, float degreeOfGrayness = 1f)
    {
        HslColor grayTextHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(KnownColor.GrayText));
        HslColor textHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(textColorName));

        double grayTextL = textHsl.L + (degreeOfGrayness * (grayTextHsl.L - textHsl.L));
        HslColor highlightGrayTextHsl = grayTextHsl.WithLuminosity(grayTextL);
        return AdaptTextColor(highlightGrayTextHsl.ToColor());
    }

    public static Color AdaptTextColor(this Color original) =>
        AdaptColor(original, isForeground: true);

    public static Color AdaptBackColor(this Color original) =>
        AdaptColor(original, isForeground: false);

    /// <summary>
    /// Adapt invariant colors to the current theme, by comparing to Text/Background color pairs in the invariant theme.
    /// Note that <see cref="SystemColors"/> and <see cref="AppColor"/> should not be adapted.
    /// </summary>
    /// <param name="original">The original <see cref="Color"/></param>
    /// <param name="isForeground"><see ref="true"/> if foreground/text, <see ref="false"/> if background.</param>
    /// <returns>The adapted color.</returns>
    public static Color AdaptColor(Color original, bool isForeground)
    {
        if (IsDefaultTheme)
        {
            return original;
        }

        HslColor hsl1 = original.ToPerceptedHsl();

        int index = Enumerable.Range(0, BackForeExamples.Length)
            .Select(i => (
                distance: DistanceTo(
                    isForeground ? BackForeExamples[i].fore : BackForeExamples[i].back),
                index: i))
            .Min().index;

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
    public static Color MakeBackgroundDarkerBy(this Color color, double amount) =>
        color.TransformHsl(l: l => l - amount);

    public static void AdaptImageLightness(this ToolStripItem item) =>
        item.Image = ((Bitmap)item.Image)?.AdaptLightness();

    public static void AdaptImageLightness(this ButtonBase button) =>
        button.Image = ((Bitmap)button.Image)?.AdaptLightness();

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
    /// Note that the theme is parsed, so ThemeSettings.Default is another instance.
    /// </summary>
    /// <returns><see langword="true"/> if the theme is default; otherwise <see langword="false"/>.</returns>
    private static bool IsDefaultTheme => string.IsNullOrWhiteSpace(ThemeSettings.Theme.Id.Name);

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
    /// <param name="luminanceThreshold">a custom luminance threshold to let the caller determine the boundary.</param>
    /// <returns>the black or white color to use as foreground</returns>
    public static Color GetContrastColor(this Color backgroundColor, float luminanceThreshold = 0.5f)
    {
        // (Loosely) Based on https://stackoverflow.com/a/3943023
        // Calculate the luminance of the background color
        double luminance = ((0.2126 * backgroundColor.R) + (0.7152 * backgroundColor.G) + (0.0722 * backgroundColor.B)) / 255;

        // Use the threshold value to determine whether to use black or white as the foreground color
        return (luminance > luminanceThreshold) ? Color.Black : Color.White;
    }

    internal static class TestAccessor
    {
        public static double Transform(double orig, double exampleOrig, double oppositeOrig, double example, double opposite) =>
            ColorHelper.Transform(orig, exampleOrig, oppositeOrig, example, opposite);

        public static Color GetContrastColor(Color backgroundColor, float luminanceThreshold = 0.5f) =>
            ColorHelper.GetContrastColor(backgroundColor, luminanceThreshold);
    }
}
