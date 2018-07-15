namespace GitUI
{
    using System;
    using System.Drawing;

    public static class FontUtil
    {
        static FontUtil()
        {
            var themeHandle = NativeMethods.OpenThemeData(IntPtr.Zero, "TEXTSTYLE");
            if (themeHandle != IntPtr.Zero)
            {
                // TODO: use C# 7.0 out var parameters
                NativeMethods.GetThemeFont(themeHandle, IntPtr.Zero, NativeMethods.TEXT_MAININSTRUCTION, 0, NativeMethods.TMT_FONT, out var logFont);

                MainInstructionFont = Font.FromLogFont(logFont);

                NativeMethods.GetThemeColor(themeHandle, NativeMethods.TEXT_MAININSTRUCTION, 0, NativeMethods.TMT_TEXTCOLOR, out var colorRef);

                MainInstructionColor = Color.FromArgb(colorRef.R, colorRef.G, colorRef.B);

                NativeMethods.CloseThemeData(themeHandle);
            }
            else
            {
                MainInstructionFont = SystemFonts.CaptionFont;
                MainInstructionColor = SystemColors.WindowText;
            }
        }

        public static Font MainInstructionFont { get; }

        public static Color MainInstructionColor { get; }
    }
}
