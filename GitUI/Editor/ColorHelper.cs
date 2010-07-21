using System.Drawing;

namespace GitUI.Editor
{
    public static class ColorHelper
    {
        public static Color GetForeColorForBackColor(Color backColor)
        {
            if ((backColor.R > 130 ||
                 backColor.B > 130 ||
                 backColor.G > 130) &&
                backColor.R + backColor.B + backColor.G > 350)
                return Color.Black;
            return Color.White;
        }
    }
}