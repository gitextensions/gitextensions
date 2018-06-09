using System;
using System.Drawing;

namespace GitUI
{
    public static class ColorHelper
    {
        public static Color GetForeColorForBackColor(Color backColor)
        {
            return IsLightColor(backColor) ? Color.Black : Color.White;
        }

        public static bool IsLightColor(this Color color)
        {
            if ((color.R > 130 ||
                 color.B > 130 ||
                 color.G > 130) &&
                color.R + color.B + color.G > 350)
            {
                return true;
            }

            return false;
        }

        public static Color AdjustColor(Color color, int amount = 10)
        {
            return Color.FromArgb(
                AdjustColorComponent(color.R),
                AdjustColorComponent(color.G),
                AdjustColorComponent(color.B));

            int AdjustColorComponent(int v)
            {
                var newValue = v + amount;

                return newValue < 0
                    ? 0
                    : newValue > 255
                        ? 255
                        : newValue;
            }
        }

        public static Color MakeColorLighter(Color color, int amount = 10)
        {
            return AdjustColor(color, amount);
        }

        public static Color MakeColorDarker(Color color, int amount = 10)
        {
            return AdjustColor(color, -amount);
        }

        public static int GetColorBrightnessIndex(Color c)
        {
            // From: http://www.had2know.com/technology/color-contrast-calculator-web-design.html
            return ((299 * c.R) + (587 * c.G) + (114 * c.B)) / 1000;
        }

        public static int GetColorBrightnessDifference(Color a, Color b)
        {
            return Math.Abs(GetColorBrightnessIndex(a) - GetColorBrightnessIndex(b));
        }
    }
}