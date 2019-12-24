using System;

namespace GitUI.Theming
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeChangedEventArgs(string themeName, bool colorsChanged)
        {
            ColorsChanged = colorsChanged;
            ThemeName = themeName;
        }

        public bool ColorsChanged { get; }

        public string ThemeName { get; }
    }
}
