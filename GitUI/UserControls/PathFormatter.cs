using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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

        public string FormatTextForDrawing(int width, string name, string oldName)
        {
            var truncatePathMethod = AppSettings.TruncatePathMethod;

            if (truncatePathMethod == TruncatePathMethod.FileNameOnly)
            {
                return FormatTextForFileNameOnly(name, oldName);
            }

            if ((truncatePathMethod != TruncatePathMethod.Compact || !EnvUtils.RunningOnWindows()) &&
                truncatePathMethod != TruncatePathMethod.TrimStart)
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
    }
}