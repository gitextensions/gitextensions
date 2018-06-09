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
            return new HslColor(color).L > 0.5;
        }

        public static Color MakeColorDarker(Color color, double amount = 0.1)
        {
            var hsl = new HslColor(color);
            return hsl.WithBrightness(hsl.L - amount).ToColor();
        }
    }
}