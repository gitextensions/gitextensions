using System;
using System.IO;

namespace GitUI.Theming
{
    public interface IThemeFileReader
    {
        string ReadThemeFile(string themeFileName);
    }

    public class ThemeFileReader : IThemeFileReader
    {
        private const int MaxFileSize = 1024 * 1024;

        public string ReadThemeFile(string themeFileName)
        {
            FileInfo fileInfo = new(themeFileName);
            if (fileInfo.Exists && fileInfo.Length > MaxFileSize)
            {
                throw new ThemeException($"Theme file size exceeds {MaxFileSize:#,##0} bytes", themeFileName);
            }

            try
            {
                return File.ReadAllText(themeFileName);
            }
            catch (SystemException ex)
            {
                throw new ThemeException(ex.Message, themeFileName, ex);
            }
        }
    }
}
