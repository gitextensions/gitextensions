using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using GitCommands;

namespace GitUI.Avatars
{
    public class InitialsAvatarGenerator : IAvatarGenerator
    {
        private int _unkownCounter = 0;

        public Image GetAvatarImage(string email, string name, int imageSize)
        {
            if (AppSettings.GravatarFallbackAvatarType != GravatarFallbackAvatarType.AuthorInitials)
            {
                return null;
            }

            return GenerateAvatarImage(email, name, imageSize);
        }

        public Image GenerateAvatarImage(string email, string name, int imageSize)
        {
            var (initials, hashCode) = GetInitialsAndHashCode(email, name);

            var avatarColor = _avatarColors[Math.Abs(hashCode) % _avatarColors.Length];

            return DrawText(initials, avatarColor, imageSize);
        }

        protected internal (string initials, int hashCode) GetInitialsAndHashCode(string email, string name)
        {
            string initials;
            int hashCode;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var trimmedName = name.Trim();
                var indexSpace = trimmedName.IndexOf(' ');
                initials = trimmedName[0].ToString().ToUpper();
                if (indexSpace != -1)
                {
                    initials += trimmedName[indexSpace + 1].ToString().ToUpper();
                }

                hashCode = string.IsNullOrWhiteSpace(email) ? name.GetHashCode() : email.GetHashCode();
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                initials = ("" + email.TrimStart().First()).ToUpper();
                hashCode = email.GetHashCode();
            }
            else
            {
                initials = "?";
                hashCode = _unkownCounter;
                _unkownCounter++;
            }

            return (initials, hashCode);
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

        private Image DrawText(string text, Color backColor, int size)
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

                using (var drawing = Graphics.FromImage(img))
                {
                    drawing.Clear(backColor);

                    drawing.SmoothingMode = SmoothingMode.AntiAlias;
                    drawing.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    var x = textSize.Width >= textSize.Height ? 0 : (textSize.Height - textSize.Width) / 2;
                    var y = textSize.Width >= textSize.Height ? (textSize.Width - textSize.Height) / 2 : 0;
                    drawing.DrawString(text, font, _textBrush, x, y);

                    drawing.Save();
                }

                return img;
            }
        }
    }
}