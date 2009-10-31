using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GitUI
{
    public static class ColorHelper
    {
        public static Color GetForeColorForBackColor(Color backColor)
        {
            if ((backColor.R > 130 || backColor.B > 130 || backColor.G > 130) && (backColor.R + backColor.B + backColor.G) > 350)
                return Color.Black;
            else
                return Color.White;
        }
    }
}
