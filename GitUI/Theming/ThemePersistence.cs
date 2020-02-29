using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitExtUtils.GitUI.Theming;
using ResourceManager;

namespace GitUI.Theming
{
    public class ThemePersistence
    {
        private static readonly Regex Pattern = new Regex(
            @"^\s*\.\s*(?<name>\w+)\s*\{\s*color\s*:\s*#(?<argb>[\da-f]{6})\s*\}\s*$",
            RegexOptions.IgnoreCase);

        private const string Format = ".{0} {{ color: #{1:x6} }}";

        private readonly TranslationString _failedToLoadThemeFrom =
            new TranslationString("Failed to read theme from {0}");
        private readonly TranslationString _fileNotFound =
            new TranslationString("File not found");
        private readonly TranslationString _fileTooLarge =
            new TranslationString("File too large");
        private readonly TranslationString _invalidFormatOfLine =
            new TranslationString("Invalid format of line {0}: {1}");
        private readonly TranslationString _invalidColorValueAtLine =
            new TranslationString("Invalid color value at line {0}: {1}");

        public Theme Load(string fileName, ThemeId id)
        {
            if (!TryReadFile(fileName, out string serialized))
            {
                return null;
            }

            if (!TryParse(fileName, serialized, out var appColors, out var sysColors))
            {
                return null;
            }

            return new Theme(appColors, sysColors, id);
        }

        public void Save(Theme theme, string fileName)
        {
            string serialized = string.Join(
                Environment.NewLine,
                theme.SysColorValues.Select(_ => string.Format(Format, _.Key, ToRbgInt(_.Value))).Concat(
                    theme.AppColorValues.Select(_ => string.Format(Format, _.Key, ToRbgInt(_.Value)))));

            File.WriteAllText(fileName, serialized);

            static int ToRbgInt(Color с) => с.ToArgb() & 0x00ffffff;
        }

        private bool TryParse(
            string fileName,
            string input,
            out IReadOnlyDictionary<AppColor, Color> applicationColors,
            out IReadOnlyDictionary<KnownColor, Color> systemColors)
        {
            var appColors = new Dictionary<AppColor, Color>();
            var sysColors = new Dictionary<KnownColor, Color>();
            applicationColors = null;
            systemColors = null;

            var lines = input.Split('\r', '\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var match = Pattern.Match(line);
                if (!match.Success)
                {
                    PrintTraceWarning(fileName, string.Format(_invalidFormatOfLine.Text, i + 1, line));
                    return false;
                }

                string nameStr = match.Groups["name"].Value;
                string rgbaStr = match.Groups["argb"].Value;

                if (!int.TryParse(rgbaStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out int rgb))
                {
                    PrintTraceWarning(fileName, string.Format(_invalidColorValueAtLine.Text, i + 1, rgbaStr));
                    return false;
                }

                if (Enum.TryParse(nameStr, out AppColor appColorName))
                {
                    appColors.Add(appColorName, ToColor(rgb));
                }
                else if (Enum.TryParse(nameStr, out KnownColor sysColorName))
                {
                    sysColors.Add(sysColorName, ToColor(rgb));
                }
            }

            applicationColors = appColors;
            systemColors = sysColors;
            return true;

            Color ToColor(int rbgInt) =>
                Color.FromArgb(rbgInt | -16777216); // 0xff000000, add alpha bits
        }

        private bool TryReadFile(string fileName, out string result)
        {
            result = null;
            var fileInfo = new FileInfo(fileName);

            if (!fileInfo.Exists)
            {
                PrintTraceWarning(fileName, _fileNotFound.Text);
                return false;
            }

            // > 1MB
            if (fileInfo.Length > (1 << 20))
            {
                PrintTraceWarning(fileName, _fileTooLarge.Text);
                return false;
            }

            try
            {
                result = File.ReadAllText(fileName);
                return true;
            }
            catch (Exception ex)
            {
                PrintTraceWarning(fileName, ex.Message);
                return false;
            }
        }

        [Conditional("DEBUG")]
        private void PrintTraceWarning(string fileName, string message) =>
            Trace.WriteLine(string.Format(_failedToLoadThemeFrom.Text, fileName) + Environment.NewLine + message);
    }
}
