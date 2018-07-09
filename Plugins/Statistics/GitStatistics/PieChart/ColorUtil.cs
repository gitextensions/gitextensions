using System.Diagnostics;
using System.Drawing;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Color utility structure.
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        ///   Small brightness change factor.
        /// </summary>
        public const float BrightnessEnhancementFactor1 = 0.3F;

        /// <summary>
        ///   Large brightness change factor.
        /// </summary>
        public const float BrightnessEnhancementFactor2 = 0.5F;

        /// <summary>
        ///   Creates color with corrected lightness.
        /// </summary>
        /// <param name = "color">
        ///   Color to correct.
        /// </param>
        /// <param name = "correctionFactor">
        ///   Correction factor, with a value between -1 and 1. Negative values
        ///   create darker color, positive values lighter color. Zero value
        ///   returns the current color.
        /// </param>
        /// <returns>
        ///   Corrected <c>Color</c> structure.
        /// </returns>
        public static Color CreateColorWithCorrectedLightness(Color color, float correctionFactor)
        {
            Debug.Assert(correctionFactor <= 1 && correctionFactor >= -1, "correctionFactor <= 1 && correctionFactor >= -1");
            if (correctionFactor == 0)
            {
                return color;
            }

            float red = color.R;
            float green = color.G;
            float blue = color.B;
            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = ((255 - red) * correctionFactor) + red;
                green = ((255 - green) * correctionFactor) + green;
                blue = ((255 - blue) * correctionFactor) + blue;
            }

            return Color.FromArgb((int)red, (int)green, (int)blue);
        }
    }
}