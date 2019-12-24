using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitExtUtils.GitUI.Theming;
using ResourceManager;

namespace GitUI.Theming
{
    public class ThemePersistence
    {
        private const string Format = "{0}: {1:x6}";
        private static readonly Regex Pattern = new Regex(
            @"^(?<name>\w+): (?<argb>[\da-f]{6})$",
            RegexOptions.IgnoreCase);

        private readonly TranslationString _failedToLoadThemeFrom =
            new TranslationString("Failed to read theme from {0}");
        private readonly TranslationString _failedToLoadTheme =
            new TranslationString("Failed to read theme");
        private readonly TranslationString _failedToWriteFile =
            new TranslationString("Failed write file {0}");
        private readonly TranslationString _fileNotFound =
            new TranslationString("File not found");
        private readonly TranslationString _fileTooLarge =
            new TranslationString("File too large");
        private readonly TranslationString _invalidFormatOfLine =
            new TranslationString("Invalid format of line {0}: {1}");
        private readonly TranslationString _invalidColorValueAtLine =
            new TranslationString("Invalid color value at line {0}: {1}");

        public bool SaveToFile(
            StaticTheme theme,
            string file,
            bool quiet = false)
        {
            string serialized = string.Join(
                Environment.NewLine,
                theme.SysColorValues.Select(_ => string.Format(Format, _.Key, ToRbgInt(_.Value))).Concat(
                theme.AppColorValues.Select(_ => string.Format(Format, _.Key, ToRbgInt(_.Value)))));
            try
            {
                File.WriteAllText(file, serialized);
                return true;
            }
            catch (Exception ex)
            {
                string message = string.Format(_failedToWriteFile.Text, file) + Environment.NewLine + ex.Message;
                Trace.WriteLine(message);
                if (!quiet)
                {
                    MessageBox.Show(message);
                }

                return false;
            }

            int ToRbgInt(Color с) => с.ToArgb() & 0x00ffffff;
        }

        public StaticTheme LoadFile(string fileName, bool quiet = false)
        {
            if (!TryReadFile(fileName, out string serialized, quiet))
            {
                return null;
            }

            if (!TryParse(fileName, serialized, out var appColors, out var sysColors, quiet))
            {
                return null;
            }

            return new StaticTheme(appColors, sysColors, fileName);
        }

        private bool TryParse(
            string fileName,
            string input,
            out IReadOnlyDictionary<AppColor, Color> applicationColors,
            out IReadOnlyDictionary<KnownColor, Color> systemColors,
            bool quiet = false)
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
                    WarnOnInvalidContent(fileName, string.Format(_invalidFormatOfLine.Text, i + 1, line), quiet);
                    return false;
                }

                string nameStr = match.Groups["name"].Value;
                string rgbaStr = match.Groups["argb"].Value;

                if (!int.TryParse(rgbaStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out int rgb))
                {
                    WarnOnInvalidContent(fileName, string.Format(_invalidColorValueAtLine.Text, i + 1, rgbaStr), quiet);
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

        private bool TryReadFile(string fileName, out string result, bool quiet)
        {
            result = null;
            var fileInfo = new FileInfo(fileName);

            if (!fileInfo.Exists)
            {
                WarnOnInvalidContent(fileName, _fileNotFound.Text, quiet);
                return false;
            }

            // > 1MB
            if (fileInfo.Length > (1 << 20))
            {
                WarnOnInvalidContent(fileName, _fileTooLarge.Text, quiet);
                return false;
            }

            try
            {
                result = File.ReadAllText(fileName);
                return true;
            }
            catch (Exception ex)
            {
                WarnOnInvalidContent(fileName, ex.Message, quiet);
                return false;
            }
        }

        private void WarnOnInvalidContent(string fileName, string message, bool quiet)
        {
            var fullMessage = string.Format(_failedToLoadThemeFrom.Text, fileName) +
                Environment.NewLine + message;

            Trace.WriteLine(fullMessage);
            if (!quiet)
            {
                MessageBox.Show(
                    message, _failedToLoadTheme.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }
    }
}
