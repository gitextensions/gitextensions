using System;
using System.Drawing;
using GitUI.Properties;

namespace GitUI
{
    static class Lemmings
    {
        public static Bitmap GetPictureBoxImage(DateTime currentDate)
        {
            // Lemmings
            // Also, we removed repeated calls to DateTime.Now and made this method testable
            if (currentDate.Month == 12 && currentDate.Day > 18 && currentDate.Day < 27) //X-Mass
            {
                return Resources.Cow_xmass;
            }
            if (currentDate.Month == 6 && currentDate.Day > 17 && currentDate.Day < 24) //summer
            {
                return Resources.Cow_sunglass;
            }
            return null;
        }
    }
}
