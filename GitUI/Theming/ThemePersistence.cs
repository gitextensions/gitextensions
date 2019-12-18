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

namespace GitUI.Theming
{
    public class ThemePersistence
    {
        private const string Format = "{0}: {1:x6}";
        private static readonly Regex Pattern = new Regex(
            @"^(?<name>\w+): (?<argb>[\da-f]{6})$",
            RegexOptions.IgnoreCase);

        public bool SaveToFile(
            IReadOnlyDictionary<AppColor, Color> appColors,
            IReadOnlyDictionary<KnownColor, Color> systemColors,
            string file,
            bool quiet = false)
        {
            string serialized = string.Join(
                Environment.NewLine,
                systemColors.Select(_ => string.Format(Format, _.Key, ToRbgInt(_.Value))).Concat(
                    appColors.Select(_ => string.Format(Format, _.Key, ToRbgInt(_.Value)))));
            try
            {
                File.WriteAllText(file, serialized);
                return true;
            }
            catch (Exception ex)
            {
                if (!quiet)
                {
                    MessageBox.Show($"Failed to write file: {file}{Environment.NewLine}{ex.Message}");
                }

                return false;
            }

            int ToRbgInt(Color с) => с.ToArgb() & 0x00ffffff;
        }

        public bool TryLoadFile(
            string fileName,
            out IReadOnlyDictionary<AppColor, Color> appColors,
            out IReadOnlyDictionary<KnownColor, Color> sysColors,
            bool quiet = false)
        {
            if (TryReadFile(fileName, out string serialized, quiet))
            {
                return TryParse(fileName, serialized, out appColors, out sysColors, quiet);
            }

            appColors = new Dictionary<AppColor, Color>();
            sysColors = new Dictionary<KnownColor, Color>();
            return false;
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
                    WarnOnInvalidContent(fileName, $"Invalid format of line {i + 1}: {line}", quiet);
                    return false;
                }

                string nameStr = match.Groups["name"].Value;
                string rgbaStr = match.Groups["argb"].Value;

                if (!int.TryParse(rgbaStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out int rgb))
                {
                    WarnOnInvalidContent(fileName, $"Invalid color value at line {i + 1}: {rgbaStr}", quiet);
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
                WarnOnInvalidContent(fileName, "File not found", quiet);
                return false;
            }

            // > 1MB
            if (fileInfo.Length > (1 << 20))
            {
                WarnOnInvalidContent(fileName, "File too large", quiet);
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
            var line = $"Failed to read theme from: {fileName}" +
                $"{Environment.NewLine}{message}";

            Trace.WriteLine(line);
            if (!quiet)
            {
                MessageBox.Show(
                    message, "Failed to load theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }
    }
}
