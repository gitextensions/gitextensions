namespace GitUI
{
    using System;
    using System.Drawing;

    public static class FontUtil
    {
#pragma warning disable SA1305 // Field names should not use Hungarian notation
        static FontUtil()
        {
            var hTheme = NativeMethods.OpenThemeData(IntPtr.Zero, "TEXTSTYLE");
            if (hTheme != IntPtr.Zero)
            {
                NativeMethods.GetThemeFont(hTheme, IntPtr.Zero, NativeMethods.TEXT_MAININSTRUCTION, 0, NativeMethods.TMT_FONT, out var pFont);

                MainInstructionFont = Font.FromLogFont(pFont);

                NativeMethods.COLORREF pColor;
                NativeMethods.GetThemeColor(hTheme, NativeMethods.TEXT_MAININSTRUCTION, 0, NativeMethods.TMT_TEXTCOLOR, out pColor);

                MainInstructionColor = Color.FromArgb(pColor.R, pColor.G, pColor.B);

                NativeMethods.CloseThemeData(hTheme);
            }
            else
            {
                MainInstructionFont = SystemFonts.CaptionFont;
                MainInstructionColor = SystemColors.WindowText;
            }
        }
#pragma warning restore SA1305 // Field names should not use Hungarian notation

        public static Font MainInstructionFont { get; }

        public static Color MainInstructionColor { get; }
    }
}
