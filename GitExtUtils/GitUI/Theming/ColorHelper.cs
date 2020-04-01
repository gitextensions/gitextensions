using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

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

        [NotNull]
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

        public static Color AdaptColor(Color originalRgb, bool isForeground)
        {
            if (ThemeSettings == ThemeSettings.Default)
            {
                return originalRgb;
            }

            var hsl1 = originalRgb.ToPerceptedHsl();

            var index = Enumerable.Range(0, BackForeExamples.Length)
                .Select(i => (
                    distance: DistanceTo(
                        isForeground ? BackForeExamples[i].fore : BackForeExamples[i].back),
                    index: i))
                .Min().index;

            var option = BackForeExamples[index];
            return isForeground
                ? AdaptColor(originalRgb, option.fore, option.back)
                : AdaptColor(originalRgb, option.back, option.fore);

            double DistanceTo(KnownColor c2)
            {
                var hsl2 = ThemeSettings.InvariantTheme.GetNonEmptyColor(c2).ToPerceptedHsl();
                return Math.Abs(hsl1.L - hsl2.L) + (0.25 * Math.Abs(hsl1.S - hsl2.S));
            }
        }

        /// <summary>
        /// Roughly speaking makes a linear transformation of input orig 0 &lt; orig &lt; 1
        /// The transformation is specified by how it affected a pair of values
        /// (exampleOrig, oppositeOrig) to (example, opposite)
        /// </summary>
        public static double Transform(double orig,
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
            KnownColor.ControlLight.MakeBackgroundDarkerBy(0.035);

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

        private static Color AdaptColor(Color originalRgb, KnownColor exampleName, KnownColor oppositeName)
        {
            var exampleOrig = RgbHsl(ThemeSettings.InvariantTheme.GetNonEmptyColor(exampleName));
            var oppositeOrig = RgbHsl(ThemeSettings.InvariantTheme.GetNonEmptyColor(oppositeName));
            var example = RgbHsl(ThemeSettings.Theme.GetNonEmptyColor(exampleName));
            var opposite = RgbHsl(ThemeSettings.Theme.GetNonEmptyColor(oppositeName));
            var original = RgbHsl(originalRgb);

            double perceptedL = Transform(
                PerceptedL(original),
                PerceptedL(exampleOrig),
                PerceptedL(oppositeOrig),
                PerceptedL(example),
                PerceptedL(opposite));

            double actualL = ActualL(original.rgb, perceptedL);

            var result = original.hsl.WithLuminosity(actualL).ToColor();
            return result;

            double PerceptedL((Color rgb, HslColor hsl) c) =>
                ColorHelper.PerceptedL(c.rgb, c.hsl.L);

            (Color rgb, HslColor hsl) RgbHsl(Color c) =>
                (c, new HslColor(c));
        }

        private static Color TransformHsl(
            this Color c,
            Func<double, double> h = null,
            Func<double, double> s = null,
            Func<double, double> l = null)
        {
            var hsl = new HslColor(c);
            var transformed = new HslColor(
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
            var hsl = new HslColor(rgb);
            return hsl.WithLuminosity(PerceptedL(rgb, hsl.L));
        }

        public static HslColor ToActualHsl(this HslColor hsl, Color rgb) =>
            hsl.WithLuminosity(ActualL(rgb, hsl.L));

        private static bool IsLightColor(this Color color)
        {
            return new HslColor(color).L > 0.5;
        }
    }
}
