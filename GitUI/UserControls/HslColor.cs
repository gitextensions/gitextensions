using System;
using System.Drawing;

namespace GitUI
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

                // limit to range [0-1] inclusive
                return value > 1
                    ? 1
                    : value < 0
                        ? 0
                        : value;
            }
        }

        /// <summary>
        /// Creates an <see cref="HslColor"/> from an RGB <see cref="Color"/> object.
        /// </summary>
        /// <remarks>Takes advantage of whats already built in to .NET by using the Color.GetHue, Color.GetSaturation and Color.GetBrightness methods</remarks>
        /// <param name="c">A Color to convert</param>
        /// <returns>An HSL value</returns>
        public HslColor(in Color c)
        {
            // TODO avoid method calls and consequent defensive copies
            // we store hue as 0-1 as opposed to 0-360
            H = c.GetHue() / 360.0;
            S = c.GetSaturation();
            L = c.GetBrightness();
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
        public HslColor WithBrightness(double luminosity) => new HslColor(H, S, luminosity);

        /// <summary>
        /// Converts this HSL color object to a <see cref="Color"/> object based on RGB values.
        /// </summary>
        public Color ToColor()
        {
            const double tolerance = 0.00001;

            if (Math.Abs(L) < tolerance)
            {
                return Color.Black;
            }

            if (Math.Abs(S) < tolerance)
            {
                var l = (int)(255 * L);
                return Color.FromArgb(l, l, l);
            }

            var v1 = L <= 0.5
                ? L * (1 + S)
                : L + S - (L * S);

            var v2 = (2 * L) - v1;

            const double oneOnThree = 1d / 3;

            return Color.FromArgb(
                (int)(255 * CalculateChannel(H + oneOnThree)),
                (int)(255 * CalculateChannel(H)),
                (int)(255 * CalculateChannel(H - oneOnThree)));

            double CalculateChannel(double v3)
            {
                if (v3 < 0)
                {
                    v3 += 1;
                }

                if (v3 > 1)
                {
                    v3 -= 1;
                }

                if (6 * v3 < 1)
                {
                    return v2 + ((v1 - v2) * v3 * 6);
                }
                else if (2 * v3 < 1)
                {
                    return v1;
                }
                else if (3 * v3 < 2)
                {
                    return v2 + ((v1 - v2) * ((2d / 3) - v3) * 6);
                }
                else
                {
                    return v2;
                }
            }
        }

        public override string ToString() => $"H = {H:#.000}, S = {S:#.000}, L = {L:#.000}";
    }
}