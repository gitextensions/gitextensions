using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using Microsoft;
using DrawingColor = System.Drawing.Color;
using DrawingColorTranslator = System.Drawing.ColorTranslator;

namespace GitUI.Avatars;

/// <summary>
/// A provider that generates avatar images based on the initials of the user.
/// </summary>
public class InitialsAvatarProvider : IAvatarProvider
{
    private int _unkownCounter;
    private static readonly char[] _emailInitialSeparator = ['.', '-', '_'];
    private FontFamily? _fontFamily;

    public InitialsAvatarProvider()
    {
        _avatarColors = [.. AppSettings.AvatarAuthorInitialsPalette.Split(',').Select(GetAvatarDrawingMaterial)];
        UpdateFontsSettings();
    }

    /// <inheritdoc/>
    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        (string initials, int colorIndex) = GetInitialsAndColorIndex(email, name);
        (IBrush foregroundBrush, Avalonia.Media.Color backgroundColor) = _avatarColors[colorIndex];
        if (Dispatcher.UIThread.CheckAccess())
        {
            return DrawText(initials, foregroundBrush, backgroundColor, imageSize);
        }

        return await Dispatcher.UIThread.InvokeAsync(
            () => DrawText(initials, foregroundBrush, backgroundColor, imageSize));
    }

    public bool PerformsIo => false;

    /// <summary>
    /// Calculate the most simpler non-cryptographic deterministic hash (to get the same result every times it is calculated for the same string)
    /// We just need an inexpensive way to convert a string to an integer, and calculating a hash is a good way to do it.
    /// We are not using <c>GetHashCode()</c> because it returns a different value for each process.
    /// Borrowed from https://stackoverflow.com/a/5155015
    /// </summary>
    /// <param name="str">The string to calculate a hash for.</param>
    /// <returns>The calculated hash.</returns>
    private static int GetDeterministicHashCode(string str)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in str)
            {
                hash = (hash * 31) + c;
            }

            return Math.Abs(hash);
        }
    }

    protected internal (string initials, int hashCode) GetInitialsAndColorIndex(string email, string? name)
    {
        (string? selectedName, char[]? separator) = NameSelector(name, email);

        if (selectedName is null)
        {
            return ("?", _unkownCounter++ % _avatarColors.Length);
        }

        string[] nameParts = selectedName.Split(separator);
        string initials = GetInitialsFromNames(nameParts);
        return (initials, GetDeterministicHashCode(email) % _avatarColors.Length);
    }

    private static (string? name, char[]? separator) NameSelector(string? name, string? email)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            return (name.Trim(), null);
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            string withoutDomain = email.LazySplit('@').First().TrimStart();
            return (withoutDomain, _emailInitialSeparator);
        }

        return (null, null);
    }

    private static string GetInitialsFromNames(string[]? possibleNames)
    {
        possibleNames = possibleNames?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        // if no valid name-elements are found, return acceptable fallback
        if (possibleNames?.Length is not > 0)
        {
            return "?";
        }

        string[] names = [.. possibleNames.Where(s => char.IsLetter(s[0]) || char.IsDigit(s[0]))];

        if (names.Length == 0)
        {
            return possibleNames.Length > 1 ? $"{possibleNames[0][0]}{possibleNames[1][0]}" : $"{possibleNames[0][0]}";
        }

        string name = names[0];

        // If only a single valid name-element is found ...
        if (names.Length == 1)
        {
            // ... and that name-element is only a single character long ...
            if (name.Length == 1)
            {
                // ... return that character as uppercase
                return name.ToUpper();
            }

            if (char.IsUpper(name[1]))
            {
                return $"{char.ToUpper(name[0])}{name[1]}";
            }

            string[] splitNames = name.Split(_emailInitialSeparator);

            if (splitNames.Length > 1)
            {
                return GetInitialsFromNames(splitNames);
            }

            char[] upperChars = [.. name.Where(char.IsUpper)];
            if (upperChars.Length > 1)
            {
                return $"{upperChars[0]}{upperChars[^1]}";
            }

            // return first letter upper-case and second letter original/lower case.
            return $"{char.ToUpper(name[0])}{name[1]}";
        }

        // Return initials from first and last name-element as uppercase
        return $"{name[0]}{names[^1][0]}".ToUpper();
    }

    private readonly (IBrush foregroundBrush, Avalonia.Media.Color backgroundColor)[] _avatarColors;

    private static (IBrush foregroundBrush, Avalonia.Media.Color backgroundColor) GetAvatarDrawingMaterial(string colorCode)
    {
        DrawingColor drawingBackground;

        try
        {
            drawingBackground = DrawingColorTranslator.FromHtml(colorCode);
        }
        catch (Exception)
        {
            drawingBackground = DrawingColor.Black;
        }

        DrawingColor drawingForeground = GetContrastColor(drawingBackground);
        Avalonia.Media.Color background = Avalonia.Media.Color.FromArgb(
            drawingBackground.A,
            drawingBackground.R,
            drawingBackground.G,
            drawingBackground.B);
        IBrush foreground = new SolidColorBrush(Avalonia.Media.Color.FromArgb(
            drawingForeground.A,
            drawingForeground.R,
            drawingForeground.G,
            drawingForeground.B));
        return (foreground, background);

        static DrawingColor GetContrastColor(DrawingColor backgroundColor)
        {
            const double WcagEqualContrastMidpoint = 0.185;
            double threshold = AppSettings.AvatarAuthorInitialsLuminanceThreshold * (WcagEqualContrastMidpoint * 2.0);
            double luminance = (0.2126 * SrgbLinearize(backgroundColor.R))
                + (0.7152 * SrgbLinearize(backgroundColor.G))
                + (0.0722 * SrgbLinearize(backgroundColor.B));
            return luminance > threshold ? DrawingColor.Black : DrawingColor.White;
        }

        static double SrgbLinearize(byte channel)
        {
            double normalized = channel / 255.0;
            return normalized <= 0.04045
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }
    }

    private byte[] DrawText(string? text, IBrush foreColor, Avalonia.Media.Color backColor, int avatarSize)
    {
        Validates.NotNull(_fontFamily);
        text ??= "?";

        double initialFontSize = Math.Max(1, avatarSize * 0.7);
        FormattedText measuredText = CreateText(initialFontSize);

        // Adjust font size based on the estimated measure of input text
        double ratio = Math.Min(
            avatarSize / Math.Max(measuredText.Width, 1),
            avatarSize / Math.Max(measuredText.Height, 1));
        FormattedText displayedText = CreateText(initialFontSize * Math.Min(ratio, 1));

        // centering horizontally and vertically
        double xOffset = Math.Max((avatarSize - displayedText.Width) / 2, 0);
        double yOffset = Math.Max((avatarSize - displayedText.Height) / 2, 0);

        using RenderTargetBitmap bitmap = new(new PixelSize(avatarSize, avatarSize), new Vector(96, 96));
        using (DrawingContext context = bitmap.CreateDrawingContext())
        {
            context.FillRectangle(new SolidColorBrush(backColor), new Rect(0, 0, avatarSize, avatarSize));
            context.DrawText(displayedText, new Avalonia.Point(xOffset, yOffset));
        }

        return AvatarImage.Encode(bitmap);

        FormattedText CreateText(double fontSize)
            => new(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(_fontFamily),
                fontSize,
                foreColor);
    }

    public void UpdateFontsSettings()
    {
        string fontFamilyName = AppSettings.Font.FontFamily.Name;
        _fontFamily = new FontFamily(fontFamilyName);
    }
}
