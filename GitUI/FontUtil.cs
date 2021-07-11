namespace GitUI
{
    using System;
    using System.Drawing;
    using Windows.Win32;
    using Windows.Win32.Foundation;
    using Windows.Win32.UI.Controls;

    public static class FontUtil
    {
#pragma warning disable SA1305 // Field names should not use Hungarian notation
        static FontUtil()
        {
            var hTheme = PInvoke.OpenThemeData((HWND)IntPtr.Zero, "TEXTSTYLE");
            if (hTheme != IntPtr.Zero)
            {
                PInvoke.GetThemeFont(hTheme, null, NativeMethods.TEXT_MAININSTRUCTION, 0, (int)THEME_PROPERTY_SYMBOL_ID.TMT_FONT, out var pFont);

                MainInstructionFont = Font.FromLogFont(pFont);

                PInvoke.GetThemeColor(hTheme, NativeMethods.TEXT_MAININSTRUCTION, 0, (int)THEME_PROPERTY_SYMBOL_ID.TMT_TEXTCOLOR, out var color);

                var pColor = new NativeMethods.COLORREF(color);

                MainInstructionColor = Color.FromArgb(pColor.R, pColor.G, pColor.B);

                PInvoke.CloseThemeData(hTheme);
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
