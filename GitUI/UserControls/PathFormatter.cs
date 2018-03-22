using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GitCommands;
using GitCommands.Utils;

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
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (font == null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            _graphics = graphics;
            _font = font;
        }

        private static string TruncatePath(string path, int length)
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
            string truncatePathMethod = AppSettings.TruncatePathMethod;
            if (truncatePathMethod.Equals("compact", StringComparison.OrdinalIgnoreCase) &&
                EnvUtils.RunningOnWindows())
            {
                var result = new StringBuilder(length);
                NativeMethods.PathCompactPathEx(result, path, length, 0);
                return result.ToString();
            }

            if (truncatePathMethod.Equals("trimStart", StringComparison.OrdinalIgnoreCase))
            {
                return "..." + path.Substring(path.Length - length);
            }

            return path; ////.Substring(0, length+1);
        }

        public string FormatTextForDrawing(int width, string name, string oldName)
        {
            string truncatePathMethod = AppSettings.TruncatePathMethod;

            if (truncatePathMethod == "fileNameOnly")
            {
                return FormatTextForFileNameOnly(name, oldName);
            }

            if ((!truncatePathMethod.Equals("compact", StringComparison.OrdinalIgnoreCase) || !EnvUtils.RunningOnWindows()) &&
                !truncatePathMethod.Equals("trimStart", StringComparison.OrdinalIgnoreCase))
            {
                return FormatString(name, oldName, 0, false);
            }

            int step = 0;
            bool isNameBeingTruncated = true;
            int maxStep = oldName == null ? name.Length : Math.Max(name.Length, oldName.Length) * 2;
            string result = string.Empty;

            while (step <= maxStep)
            {
                result = FormatString(name, oldName, step, isNameBeingTruncated);

                if (_graphics.MeasureString(result, _font).Width <= width)
                {
                    break;
                }

                step++;
                isNameBeingTruncated = !isNameBeingTruncated;
            }

            return result;
        }

        public static string FormatTextForFileNameOnly(string name, string oldName)
        {
            name = name.TrimEnd(AppSettings.PosixPathSeparator);
            var fileName = Path.GetFileName(name);
            var oldFileName = Path.GetFileName(oldName);

            if (fileName == oldFileName)
            {
                oldFileName = null;
            }

            return fileName.Combine(" ", oldFileName.AddParenthesesNE());
        }

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
        }
    }
}