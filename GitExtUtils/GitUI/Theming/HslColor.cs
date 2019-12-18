using System;
using System.Drawing;

namespace GitExtUtils.GitUI.Theming
{
    public readonly struct HslColor
    {
        /// <summary>
        /// Creates a new HslColor value.
        /// </summary>
        /// <param name="hue">Hue, as a value between 0 and 1.</param>
        /// <param name="saturation">Saturation, as a value between 0 and 1.</param>
        /// <param name="luminance">Luminance, as a value between 0 and 1.</param>
        public HslColor(double hue, double saturation, double luminance)
        {
            H = Preprocess(hue);
            S = Preprocess(saturation);
            L = Preprocess(luminance);

            double Preprocess(double value)
            {
                if (double.IsNaN(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Cannot have a NaN channel value.");
                }

                return value.WithinRange(0, 1);
            }
        }

        /// <summary>
        /// Creates an <see cref="HslColor"/> from an RGB <see cref="Color"/> object.
        /// </summary>
        /// <param name="c">A Color to convert</param>
        /// <returns>An HSL value</returns>
        public HslColor(in Color c)
        {
            double r = c.R / 255d;
            double g = c.G / 255d;
            double b = c.B / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double h;
            double s;
            double l = (max + min) / 2;

            if (max.Equals(min))
            {
                h = s = 0; // achromatic
            }
            else
            {
                double d = max - min;
                s = l > 0.5
                    ? d / (2 - max - min)
                    : d / (max + min);

                if (max.Equals(r))
                {
                    h = ((g - b) / d) + (g < b ? 6 : 0);
                }
                else if (max.Equals(g))
                {
                    h = ((b - r) / d) + 2;
                }
                else
                {
                    // if (max.Equals(b))
                    h = ((r - g) / d) + 4;
                }

                h /= 6;
            }

            H = h;
            S = s;
            L = l;
        }

        /// <summary>
        /// Hue as a value between 0 and 1.
        /// </summary>
        public double H { get; }

        /// <summary>
        /// Saturation as a value between 0 and 1.
        /// </summary>
        public double S { get; }

        /// <summary>
        /// Luminosity (brightness) as a value between 0 and 1.
        /// </summary>
        public double L { get; }

        public HslColor WithHue(double hue) => new HslColor(hue, S, L);
        public HslColor WithSaturation(double saturation) => new HslColor(H, saturation, L);
        public HslColor WithLuminosity(double luminosity) => new HslColor(H, S, luminosity);

        /// <summary>
        /// Converts this HSL color object to a <see cref="Color"/> object based on RGB values.
        /// </summary>
        public Color ToColor()
        {
            double r;
            double g;
            double b;

            if (S.Equals(0))
            {
                r = g = b = L; // achromatic
            }
            else
            {
                var q = L < 0.5 ? L * (1 + S) : L + S - (L * S);
                var p = (2 * L) - q;
                r = Hue2Rgb(p, q, H + (1 / 3d));
                g = Hue2Rgb(p, q, H);
                b = Hue2Rgb(p, q, H - (1 / 3d));
            }

            return Color.FromArgb(
                (int)Math.Floor(r * 256).AtMost(255),
                (int)Math.Floor(g * 256).AtMost(255),
                (int)Math.Floor(b * 256).AtMost(255));

            double Hue2Rgb(double p, double q, double t)
            {
                if (t < 0)
                {
                    t++;
                }

                if (t > 1)
                {
                    t--;
                }

                if (t < 1 / 6d)
                {
                    return p + ((q - p) * 6 * t);
                }

                if (t < 1 / 2d)
                {
                    return q;
                }

                if (t < 2 / 3d)
                {
                    return p + ((q - p) * ((2 / 3d) - t) * 6);
                }

                return p;
            }
        }

        public override string ToString() => $"H = {H:#.000}, S = {S:#.000}, L = {L:#.000}";
    }
}
