using System.Drawing.Drawing2D;
using System.Drawing.Text;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Avatars
{
    /// <summary>
    /// A provider that generates avatar images based on the initials of the user.
    /// </summary>
    public class InitialsAvatarProvider : IAvatarProvider
    {
        private int _unkownCounter = 0;
        private static readonly char[] _emailInitialSeparator = new[] { '.', '-', '_' };

        /// <inheritdoc/>
        public Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            (string initials, int hashCode) = GetInitialsAndHashCode(email, name);

            (Brush foregroundBrush, Color backgroundColor) = _avatarColors[hashCode];
            Image avatar = DrawText(initials, foregroundBrush, backgroundColor, imageSize);

            return Task.FromResult<Image?>(avatar);
        }

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

        protected internal (string? initials, int hashCode) GetInitialsAndHashCode(string email, string? name)
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

        private static string? GetInitialsFromNames(string[]? names)
        {
            names = names?.Where(s => !string.IsNullOrWhiteSpace(s) && char.IsLetter(s[0])).ToArray();

            // if no valid name-elements are found, return null
            if (names is null || names.Length == 0)
            {
                return null;
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
                    return $"{upperChars[0]}{upperChars[upperChars.Length - 1]}";
                }

                // return first letter upper-case and second letter original/lower case.
                return $"{char.ToUpper(name[0])}{name[1]}";
            }

            // Return initials from first and last name-element as uppercase
            return $"{name[0]}{names[names.Length - 1][0]}".ToUpper();
        }

        private readonly Graphics _graphics = Graphics.FromImage(new Bitmap(1, 1));

        private readonly (Brush foregroundBrush, Color backgroundColor)[] _avatarColors = AppSettings.AvatarAuthorInitialsPalette.Split(",").Select(GetAvatarDrawingMaterial).ToArray();

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

        private Image DrawText(string? text, Brush foreColor, Color backColor, int size)
        {
            lock (_avatarColors)
            {
                var fontSizeEstimation = size / 4.0f;
                Font font = new(AppSettings.CommitFont.FontFamily, fontSizeEstimation);

                SizeF textSize = _graphics.MeasureString(text, font);

                // Adjust font size with the measure
                var sizeSquare = Math.Max((int)textSize.Width, (int)textSize.Height);
                font = new Font(AppSettings.CommitFont.FontFamily, fontSizeEstimation * size / sizeSquare);
                textSize = _graphics.MeasureString(text, font);

                Bitmap img = new(size, size);

                using var drawing = Graphics.FromImage(img);
                drawing.Clear(backColor);
                drawing.SmoothingMode = SmoothingMode.AntiAlias;
                drawing.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var x = textSize.Width >= textSize.Height ? 0 : (textSize.Height - textSize.Width) / 2;
                var y = textSize.Width >= textSize.Height ? (textSize.Width - textSize.Height) / 2 : 0;
                drawing.DrawString(text, font, foreColor, x, y);
                drawing.Save();

                return img;
            }
        }
    }
}
