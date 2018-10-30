using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using JetBrains.Annotations;

namespace GitUI
{
    internal sealed class PathFormatter
    {
        private static class NativeMethods
        {
            [DllImport("shlwapi.dll")]
            public static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax,
                                                         int dwFlags);
        }

        private readonly Graphics _graphics;
        private readonly Font _font;

        public PathFormatter(Graphics graphics, Font font)
        {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _font = font ?? throw new ArgumentNullException(nameof(font));
        }

        public (string Text, int Width) FormatTextForDrawing(int maxWidth, string name, string oldName)
        {
            string result = string.Empty;
            int width = 0;

            switch (AppSettings.TruncatePathMethod)
            {
                case TruncatePathMethod.FileNameOnly:
                    result = FormatTextForFileNameOnly(name, oldName);
                    width = MeasureString(result, withPadding: true).Width;
                    return (result, width);

                case TruncatePathMethod.None:
                case TruncatePathMethod.Compact when !EnvUtils.RunningOnWindows():
                    result = FormatString(name, oldName, step: 0, isNameTruncated: false);
                    width = MeasureString(result, withPadding: true).Width;
                    return (result, width);

                default:
                    int maxStep = oldName == null
                        ? name.Length
                        : Math.Max(name.Length, oldName.Length) * 2;

                    BinarySearch.Find(min: 0, count: maxStep + 1, step =>
                    {
                        var formatted = FormatString(name, oldName, step, isNameTruncated: step % 2 == 0);
                        int measuredWidth = MeasureString(formatted, withPadding: true).Width;
                        bool isShortEnough = measuredWidth <= maxWidth;

                        if (isShortEnough)
                        {
                            result = formatted;
                            width = measuredWidth;
                        }

                        return isShortEnough;
                    });

                    return (result, width);
            }
        }

        [CanBeNull]
        public static string FormatTextForFileNameOnly(string name, string oldName)
        {
            name = name.TrimEnd(PathUtil.PosixDirectorySeparatorChar);
            var fileName = Path.GetFileName(name);
            var oldFileName = Path.GetFileName(oldName);

            if (fileName == oldFileName)
            {
                oldFileName = null;
            }

            return fileName.Combine(" ", oldFileName.AddParenthesesNE());
        }

        public Size MeasureString(string str, bool withPadding = false)
        {
            var formatFlags = FilePathStringFormat;
            if (!withPadding)
            {
                formatFlags |= TextFormatFlags.NoPadding;
            }

            return TextRenderer.MeasureText(
                _graphics,
                str,
                _font,
                new Size(int.MaxValue, int.MaxValue),
                formatFlags);
        }

        public void DrawString(string str, Rectangle rect, Color color) =>
            TextRenderer.DrawText(_graphics, str, _font, rect, color, FilePathStringFormat);

        private static string FormatString(string name, string oldName, int step, bool isNameTruncated)
        {
            if (oldName != null)
            {
                int numberOfTruncatedChars = step / 2;
                int nameTruncatedChars = isNameTruncated ? step - numberOfTruncatedChars : numberOfTruncatedChars;
                int oldNameTruncatedChars = step - nameTruncatedChars;

                return string.Concat(TruncatePath(name, name.Length - oldNameTruncatedChars), " (",
                                     TruncatePath(oldName, oldName.Length - oldNameTruncatedChars), ")");
            }

            return TruncatePath(name, name.Length - step);

            string TruncatePath(string path, int length)
            {
                if (path.Length == length)
                {
                    return path;
                }

                if (length <= 0)
                {
                    return string.Empty;
                }

                // The win32 method PathCompactPathEx is only supported on Windows
                var truncatePathMethod = AppSettings.TruncatePathMethod;

                if (truncatePathMethod == TruncatePathMethod.Compact && EnvUtils.RunningOnWindows())
                {
                    var result = new StringBuilder(length);
                    NativeMethods.PathCompactPathEx(result, path, length, 0);
                    return result.ToString();
                }

                if (truncatePathMethod == TruncatePathMethod.TrimStart)
                {
                    return "..." + path.Substring(path.Length - length);
                }

                return path;
            }
        }

        private const TextFormatFlags FilePathStringFormat =
            TextFormatFlags.NoClipping |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.TextBoxControl;
    }
}