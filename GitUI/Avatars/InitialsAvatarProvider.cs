using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitExtUtils;

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
            var (initials, hashCode) = GetInitialsAndHashCode(email, name);

            var avatarColor = _avatarColors[Math.Abs(hashCode) % _avatarColors.Length];
            var avatar = DrawText(initials, avatarColor, imageSize);

            return Task.FromResult<Image?>(avatar);
        }

        protected internal (string? initials, int hashCode) GetInitialsAndHashCode(string email, string? name)
        {
            (var selectedName, var separator) = NameSelector(name, email);

            if (selectedName is null)
            {
                return ("?", _unkownCounter++);
            }

            var nameParts = selectedName.Split(separator);
            var initials = GetInitialsFromNames(nameParts);

            return (initials, selectedName.GetHashCode());
        }

        private static (string? name, char[]? separator) NameSelector(string? name, string? email)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                return (name.Trim(), null);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var withoutDomain = email.LazySplit('@').First().TrimStart();
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

            // If only a single valid name-element is found ...
            if (names.Length == 1)
            {
                // ... and that name-element is only a single character long ...
                if (names[0].Length == 1)
                {
                    // ... return that character as uppercase
                    return names[0][0].ToString().ToUpper();
                }

                // return first letter upper-case and second letter original/lower case.
                return $"{char.ToUpper(names[0][0])}{names[0][1]}";
            }

            // Return initials from first and last name-element as uppercase
            return $"{names[0][0]}{names[names.Length - 1][0]}".ToUpper();
        }

        private readonly Graphics _graphics = Graphics.FromImage(new Bitmap(1, 1));
        private readonly Brush _textBrush = new SolidBrush(Color.WhiteSmoke);

        private readonly Color[] _avatarColors =
        {
            Color.RoyalBlue,
            Color.DarkRed,
            Color.Purple,
            Color.ForestGreen,
            Color.DarkOrange
        };

        private Image DrawText(string? text, Color backColor, int size)
        {
            lock (_avatarColors)
            {
                var fontSizeEstimation = size / 4.0f;
                var font = new Font(AppSettings.CommitFont.FontFamily, fontSizeEstimation);

                SizeF textSize = _graphics.MeasureString(text, font);

                // Adjust font size with the measure
                var sizeSquare = Math.Max((int)textSize.Width, (int)textSize.Height);
                font = new Font(AppSettings.CommitFont.FontFamily, fontSizeEstimation * size / sizeSquare);
                textSize = _graphics.MeasureString(text, font);

                var img = new Bitmap(size, size);

                using var drawing = Graphics.FromImage(img);
                drawing.Clear(backColor);
                drawing.SmoothingMode = SmoothingMode.AntiAlias;
                drawing.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var x = textSize.Width >= textSize.Height ? 0 : (textSize.Height - textSize.Width) / 2;
                var y = textSize.Width >= textSize.Height ? (textSize.Width - textSize.Height) / 2 : 0;
                drawing.DrawString(text, font, _textBrush, x, y);
                drawing.Save();

                return img;
            }
        }
    }
}
