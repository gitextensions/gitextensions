using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GitExtUtils.GitUI.Theming
{
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

        public static void SetForeColorForBackColor(this Control control) =>
            control.ForeColor = GetForeColorForBackColor(control.BackColor);

        public static void SetForeColorForBackColor(this ToolStripItem control) =>
            control.ForeColor = GetForeColorForBackColor(control.BackColor);

        public static Color GetForeColorForBackColor(Color backColor)
        {
            var hsl1 = backColor.ToPerceptedHsl();

            var closestBack = Enumerable.Range(0, BackForeExamples.Length)
                .Select(i => (Distance: DistanceTo(BackForeExamples[i].back), Index: i))
                .Min();

            var closestFore = Enumerable.Range(0, BackForeExamples.Length)
                .Select(i => (Distance: DistanceTo(BackForeExamples[i].fore), Index: i))
                .Min();

            return ThemeSettings.Theme.GetNonEmptyColor(closestBack.Distance <= closestFore.Distance * 1.25 // prefer back-fore combination
                ? BackForeExamples[closestBack.Index].fore
                : BackForeExamples[closestFore.Index].back);

            double DistanceTo(KnownColor c2)
            {
                var hsl2 = ThemeSettings.Theme.GetNonEmptyColor(c2).ToPerceptedHsl();
                return
                    Math.Abs(hsl1.L - hsl2.L) +
                    (0.25 * Math.Abs(hsl1.S - hsl2.S)) +
                    (0.25 * (hsl1.H - hsl2.H).Modulo(1));
            }
        }

        public static Color AdaptTextColor(this Color original) =>
            AdaptColor(original, isForeground: true);

        public static Color AdaptBackColor(this Color original) =>
            AdaptColor(original, isForeground: false);

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

            var highlightGrayTextHsl = grayTextHsl.WithLuminosity(highlightGrayTextL);
            return AdaptTextColor(highlightGrayTextHsl.ToColor());
        }

        /// <summary>
        /// Get a color to be used instead of SystemColors.GrayText which is more ore less gray than
        /// the usual SystemColors.GrayText.
        /// </summary>
        public static Color GetGrayTextColor(KnownColor textColorName, float degreeOfGrayness = 1f)
        {
            HslColor grayTextHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(KnownColor.GrayText));
            HslColor textHsl = new(ThemeSettings.InvariantTheme.GetNonEmptyColor(textColorName));

            double grayTextL = textHsl.L + (degreeOfGrayness * (grayTextHsl.L - textHsl.L));
            var highlightGrayTextHsl = grayTextHsl.WithLuminosity(grayTextL);
            return AdaptTextColor(highlightGrayTextHsl.ToColor());
        }

        public static Color AdaptColor(Color original, bool isForeground)
        {
            if (ThemeSettings == ThemeSettings.Default)
            {
                return original;
            }

            var hsl1 = original.ToPerceptedHsl();

            var index = Enumerable.Range(0, BackForeExamples.Length)
                .Select(i => (
                    distance: DistanceTo(
                        isForeground ? BackForeExamples[i].fore : BackForeExamples[i].back),
                    index: i))
                .Min().index;

            var option = BackForeExamples[index];
            return isForeground
                ? AdaptColor(original, option.fore, option.back)
                : AdaptColor(original, option.back, option.fore);

            double DistanceTo(KnownColor c2)
            {
                var hsl2 = ThemeSettings.InvariantTheme.GetNonEmptyColor(c2).ToPerceptedHsl();
                return Math.Abs(hsl1.L - hsl2.L) + (0.25 * Math.Abs(hsl1.S - hsl2.S));
            }
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

        /// <remarks>0.05 is subtle. 0.3 is quite strong.</remarks>
        public static Color MakeBackgroundDarkerBy(this KnownColor name, double amount) =>
            ThemeSettings.InvariantTheme.GetNonEmptyColor(name)
                .TransformHsl(l: l => l - amount)
                .AdaptBackColor();

        public static bool IsLightTheme()
        {
            return IsLightColor(SystemColors.Window);
        }

        public static Color GetSplitterColor() =>
            KnownColor.Window.MakeBackgroundDarkerBy(0);

        public static void AdaptImageLightness(this ToolStripItem item) =>
            item.Image = ((Bitmap)item.Image)?.AdaptLightness();

        public static void AdaptImageLightness(this ButtonBase button) =>
            button.Image = ((Bitmap)button.Image)?.AdaptLightness();

        public static Bitmap AdaptLightness(this Bitmap original)
        {
            if (ThemeSettings == ThemeSettings.Default)
            {
                return original;
            }

            var clone = (Bitmap)original.Clone();
            new LightnessCorrection(clone).Execute();
            return clone;
        }

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

            var exampleOrigRgbHsl = RgbHsl(exampleOriginal);
            var oppositeOrigRgbHsl = RgbHsl(oppositeOriginal);
            var exampleRgbHsl = RgbHsl(example);
            var oppositeRgbHsl = RgbHsl(opposite);
            var originalRgbHsl = RgbHsl(original);

            double perceptedL = Transform(
                PerceptedL(originalRgbHsl),
                PerceptedL(exampleOrigRgbHsl),
                PerceptedL(oppositeOrigRgbHsl),
                PerceptedL(exampleRgbHsl),
                PerceptedL(oppositeRgbHsl));

            double actualL = ActualL(originalRgbHsl.rgb, perceptedL);

            var result = originalRgbHsl.hsl.WithLuminosity(actualL).ToColor();
            return result;

            double PerceptedL((Color rgb, HslColor hsl) c) =>
                ColorHelper.PerceptedL(c.rgb, c.hsl.L);

            (Color rgb, HslColor hsl) RgbHsl(Color c) =>
                (c, new HslColor(c));
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
            var l0 = gamma / (gamma + 1);
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

        private static bool IsLightColor(this Color color)
        {
            return new HslColor(color).L > 0.5;
        }

        private static Color AdaptToColorblindness(this Color color)
        {
            double excludeHTo = 15d; // orange

            HslColor hsl = new(color);
            var deltaH = ((hsl.H * 360d) - excludeHTo + 180).Modulo(360) - 180;

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
                var difference = end - start;
                var adjusted = difference * amount;
                return start + adjusted;
            }
        }

        internal static class TestAccessor
        {
            public static double Transform(double orig, double exampleOrig, double oppositeOrig, double example, double opposite) =>
                ColorHelper.Transform(orig, exampleOrig, oppositeOrig, example, opposite);
        }
    }
}
