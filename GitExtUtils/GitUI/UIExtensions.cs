using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI
{
    public static class UIExtensions
    {
        public static bool? GetNullableChecked(this CheckBox chx)
        {
            if (chx.CheckState == CheckState.Indeterminate)
            {
                return null;
            }
            else
            {
                return chx.Checked;
            }
        }

        public static void SetNullableChecked(this CheckBox chx, bool? @checked)
        {
            if (@checked.HasValue)
            {
                chx.CheckState = @checked.Value ? CheckState.Checked : CheckState.Unchecked;
            }
            else
            {
                chx.CheckState = CheckState.Indeterminate;
            }
        }

        public static bool IsFixedWidth(this Font ft, Graphics g)
        {
            char[] charSizes = { 'i', 'a', 'Z', '%', '#', 'a', 'B', 'l', 'm', ',', '.' };
            float charWidth = g.MeasureString("I", ft).Width;

            bool fixedWidth = true;

            foreach (char c in charSizes)
            {
                if (Math.Abs(g.MeasureString(c.ToString(), ft).Width - charWidth) > float.Epsilon)
                {
                    fixedWidth = false;
                }
            }

            return fixedWidth;
        }
    }
}
