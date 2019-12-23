using System.Drawing;

namespace GitUI.Theming
{
    internal static class Win32ColorTranslator
    {
        public static KnownColor GetKnownColor(int systemColorIndex)
        {
            if ((systemColorIndex & 0xffffff00) == 0)
            {
                systemColorIndex |= -0x80000000;
                return ColorTranslator.FromOle(systemColorIndex).ToKnownColor();
            }

            return ColorTranslator.FromWin32(systemColorIndex).ToKnownColor();
        }

        public static int GetSystemColorIndex(KnownColor name)
        {
            var ole = ColorTranslator.ToOle(Color.FromKnownColor(name));
            var result = ole & 0xffffff;
            return result;
        }
    }
}
