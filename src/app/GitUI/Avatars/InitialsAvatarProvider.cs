using System.Drawing.Drawing2D;
using System.Drawing.Text;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Avatars
{
    /// <summary>
    /// A provider that generates avatar images based on the initials of the user.
    /// </summary>
    public class InitialsAvatarProvider : IAvatarProvider
    {
        private const float _fontSizeEstimation = 20f;
        private int _unkownCounter = 0;
        private static readonly char[] _emailInitialSeparator = ['.', '-', '_'];
        private FontFamily _fontFamily;
        private Font _estimationFont;

        public InitialsAvatarProvider()
        {
            UpdateFontsSettings();
        }

        /// <inheritdoc/>
        public Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            (string initials, int colorIndex) = GetInitialsAndColorIndex(email, name);

            (Brush foregroundBrush, Color backgroundColor) = _avatarColors[colorIndex];
            Image avatar = DrawText(initials, foregroundBrush, backgroundColor, imageSize);

            return Task.FromResult<Image?>(avatar);
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
            (string selectedName, char[] separator) = NameSelector(name, email);

            if (selectedName is null)
            {
                return ("?", _unkownCounter++);
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

            if (possibleNames?.Length is not > 0)
            {
                return "?";
            }

            string[]? names = possibleNames?.Where(s => char.IsLetter(s[0]) || char.IsDigit(s[0])).ToArray();

            // if no valid name-elements are found, return acceptable fallback
            if (names?.Length is not > 0)
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

                char[] upperChars = name.Where(char.IsUpper).ToArray();
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

        private readonly (Brush foregroundBrush, Color backgroundColor)[] _avatarColors = AppSettings.AvatarAuthorInitialsPalette.Split(',').Select(GetAvatarDrawingMaterial).ToArray();

        private static (Brush foregroundBrush, Color backgroundColor) GetAvatarDrawingMaterial(string colorCode)
        {
            Color backgroundColor = ConvertToColor(colorCode);

            return (new SolidBrush(backgroundColor.GetContrastColor(AppSettings.AvatarAuthorInitialsLuminanceThreshold)), backgroundColor);

            Color ConvertToColor(string colorCode)
            {
                try
                {
                    return ColorTranslator.FromHtml(colorCode);
                }
                catch (Exception)
                {
                    return Color.Black;
                }
            }
        }

        private Image DrawText(string? text, Brush foreColor, Color backColor, int avatarSize)
        {
            text ??= "?";

            Bitmap bitmap = new(avatarSize, avatarSize);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(backColor);
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            SizeF estimatedSize = graphics.MeasureString(text, _estimationFont);

            // Adjust font size based on the estimated measure of input text
            int squareSize = Math.Max((int)estimatedSize.Width, (int)estimatedSize.Height);
            float ratio = (float)avatarSize / squareSize;
            using Font drawingFont = new(_fontFamily, _fontSizeEstimation * ratio);
            SizeF displayedSize = estimatedSize * ratio;

            // centering horizontally and vertically
            float xOffset = Math.Max((avatarSize - displayedSize.Width) / 2, 0);
            float yOffset = Math.Max((avatarSize - displayedSize.Height) / 2, 0);
            graphics.DrawString(text, drawingFont, foreColor, xOffset, yOffset);
            graphics.Save();

            return bitmap;
        }

        public void UpdateFontsSettings()
        {
            Font oldFont = _estimationFont;
            _fontFamily = AppSettings.Font.FontFamily;
            _estimationFont = new(_fontFamily, _fontSizeEstimation);
            oldFont?.Dispose();
        }
    }
}
