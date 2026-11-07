using System.Globalization;

namespace GitUI.Compat;

/// <summary>
///  Preserves WinForms character matching for real and first-letter menu mnemonics.
/// </summary>
internal static class ToolStripMnemonic
{
    internal static bool IsMnemonic(char charCode, string text)
    {
        if (charCode == '&')
        {
            return false;
        }

        int position = -1;
        char upperCode = char.ToUpper(charCode, CultureInfo.CurrentCulture);
        while (position + 1 < text.Length)
        {
            position = text.IndexOf('&', position + 1) + 1;
            if (position <= 0 || position >= text.Length)
            {
                break;
            }

            char upperText = char.ToUpper(text[position], CultureInfo.CurrentCulture);
            if (upperText == upperCode
                || char.ToLower(upperText, CultureInfo.CurrentCulture) == char.ToLower(upperCode, CultureInfo.CurrentCulture))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsPseudoMnemonic(char charCode, string text)
    {
        // Preserve WinFormsUtils.ContainsMnemonic's actual legacy rule rather than
        // replacing it with a more conventional prefix parser.
        int firstAmpersand = text.IndexOf('&');
        bool containsMnemonic = firstAmpersand >= 0 && firstAmpersand <= text.Length - 2
            && text.IndexOf('&', firstAmpersand + 1) == -1;
        return !containsMnemonic
            && (char.ToUpper(text[0], CultureInfo.CurrentCulture) == char.ToUpper(charCode, CultureInfo.CurrentCulture)
                || char.ToLower(text[0], CultureInfo.CurrentCulture) == char.ToLower(charCode, CultureInfo.CurrentCulture));
    }
}
