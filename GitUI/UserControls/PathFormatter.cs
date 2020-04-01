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

        public (string prefix, string text, string suffix, int width) FormatTextForDrawing(int maxWidth, string name, string oldName)
        {
            string prefix = null;
            string text = string.Empty;
            string suffix = null;
            int width = 0;

            switch (AppSettings.TruncatePathMethod)
            {
                case TruncatePathMethod.FileNameOnly:
                    (text, suffix) = FormatTextForFileNameOnly(name, oldName);
                    width = MeasureString(prefix, text, suffix).Width;
                    return (prefix, text, suffix, width);

                case TruncatePathMethod.None:
                case TruncatePathMethod.Compact when !EnvUtils.RunningOnWindows():
                    (prefix, text, suffix) = FormatString(name, oldName, step: 0, isNameTruncated: false);
                    width = MeasureString(prefix, text, suffix).Width;
                    return (prefix, text, suffix, width);

                default:
                    int maxStep = oldName == null
                        ? name.Length
                        : Math.Max(name.Length, oldName.Length) * 2;

                    BinarySearch.Find(min: 0, count: maxStep + 1, step =>
                    {
                        var (tmpPrefix, tmpText, tmpSuffix) = FormatString(name, oldName, step, isNameTruncated: step % 2 == 0);
                        int measuredWidth = MeasureString(tmpPrefix, tmpText, tmpSuffix).Width;
                        bool isShortEnough = measuredWidth <= maxWidth;

                        if (isShortEnough)
                        {
                            prefix = tmpPrefix;
                            text = tmpText;
                            suffix = tmpSuffix;
                            width = measuredWidth;
                        }

                        return isShortEnough;
                    });

                    return (prefix, text, suffix, width);
            }
        }

        [CanBeNull]
        public static (string text, string suffix) FormatTextForFileNameOnly(string name, string oldName)
        {
            name = name.TrimEnd(PathUtil.PosixDirectorySeparatorChar);
            var fileName = Path.GetFileName(name);
            var oldFileName = Path.GetFileName(oldName);
            string suffix = fileName == oldFileName ? null : FormatOldName(oldFileName);
            return (fileName, suffix);
        }

        public Size MeasureString(string prefix, string text, string suffix)
        {
            string str = prefix.Combine(string.Empty, text).Combine(string.Empty, suffix);
            return MeasureString(str, withPadding: true);
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

        private static (string prefix, string text, string suffix) FormatString(string name, string oldName, int step, bool isNameTruncated)
        {
            if (oldName != null)
            {
                int numberOfTruncatedChars = step / 2;
                int nameTruncatedChars = isNameTruncated ? step - numberOfTruncatedChars : numberOfTruncatedChars;
                int oldNameTruncatedChars = step - nameTruncatedChars;

                var (path, filename) = SplitPathName(TruncatePath(name, name.Length - oldNameTruncatedChars));
                string suffix = FormatOldName(TruncatePath(oldName, oldName.Length - oldNameTruncatedChars));
                return (path, filename, suffix);
            }

            var (prefix, text) = SplitPathName(TruncatePath(name, name.Length - step));
            return (prefix, text, null);

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

        private static string FormatOldName(string oldName)
        {
            return string.IsNullOrEmpty(oldName) ? null : " (" + oldName + ")";
        }

        private static (string path, string fileName) SplitPathName(string name)
        {
            if (name == null)
            {
                return (null, null);
            }

            int slashIndex = name.TrimEnd(PathUtil.PosixDirectorySeparatorChar).LastIndexOf(PathUtil.PosixDirectorySeparatorChar);
            if (slashIndex >= 0 && slashIndex < name.Length)
            {
                string path = name.Substring(0, slashIndex + 1);
                string fileName = name.Substring(slashIndex + 1);
                return (path, fileName);
            }

            return (null, name);
        }

        private const TextFormatFlags FilePathStringFormat =
            TextFormatFlags.NoClipping |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.TextBoxControl;

        internal readonly struct TestAccessor
        {
            internal static string FormatOldName(string oldName) => PathFormatter.FormatOldName(oldName);
            internal static (string path, string fileName) SplitPathName(string name) => PathFormatter.SplitPathName(name);
        }
    }
}