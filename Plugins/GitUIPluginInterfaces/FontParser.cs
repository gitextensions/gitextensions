using System.Drawing;
using System.Globalization;

namespace GitUIPluginInterfaces
{
    public static class FontParser
    {
        private const string InvariantCultureId = "_IC_";

        public static string AsString(this Font value)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0};{1};{2};{3};{4}", value.FontFamily.Name, value.Size, InvariantCultureId, value.Bold ? 1 : 0, value.Italic ? 1 : 0);
        }

        public static Font Parse(this string value, Font defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            string[] parts = value.Split(';');
            if (parts.Length < 2)
            {
                return defaultValue;
            }

            try
            {
                string fontSize;
                if (parts.Length == 3 && parts[2] == InvariantCultureId)
                {
                    fontSize = parts[1];
                }
                else
                {
                    fontSize = parts[1].Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    fontSize = fontSize.Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                }

                var fontStyle = parts.Length > 3 && parts[3] == "1" ? FontStyle.Bold : FontStyle.Regular;
                fontStyle |= parts.Length > 4 && parts[4] == "1" ? FontStyle.Italic : FontStyle.Regular;

                return new Font(parts[0], float.Parse(fontSize, CultureInfo.InvariantCulture), fontStyle);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}