using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GitUI.Editor
{
    public static class FileHelper
    {
        private static readonly IEnumerable<string> BinaryExtensions = new[]
        {
            ".avi",
            ".bmp",
            ".dat",
            ".dll",
            ".doc",
            ".docx",
            ".dwg",
            ".exe",
            ".gif",
            ".ico",
            ".jpg",
            ".jpeg",
            ".mpg",
            ".mpeg",
            ".msi",
            ".pdf",
            ".png",
            ".pdb",
            ".sc1",
            ".tif",
            ".tiff",
            ".vsd",
            ".vsdx",
            ".xls",
            ".xlsx",
        };

        private static readonly IEnumerable<string> ImageExtensions = new[]
        {
            ".bmp",
            ".gif",
            ".ico",
            ".jpg",
            ".jpeg",
            ".png",
            ".tif",
            ".tiff",
        };

        public static bool IsBinaryFile(string fileName)
        {
            var t = IsBinaryAccordingToGitAttributes(fileName);
            if (t.HasValue)
                return t.Value;
            return HasMatchingExtension(BinaryExtensions, fileName);
        }

        /// <returns>null if no info in .gitattributes. True if marked as binary, false if marked as text</returns>
        private static bool? IsBinaryAccordingToGitAttributes(string fileName)
        {
            string gitAttributesPath = Path.Combine(GitCommands.Settings.WorkingDir, ".gitattributes");
            if (File.Exists(gitAttributesPath))
            {
                string[] lines = File.ReadAllLines(gitAttributesPath);
                bool? lastMatchResult = null;
                foreach (var parts in lines.Select(line => line.Trim().Split(' ')))
                {
                    if (parts.Length < 2 || parts[0][0] == '#')
                        continue;
                    if (parts.Contains("binary") || parts.Contains("-text"))
                        if (Regex.IsMatch(fileName, CreateRegexFromFilePattern(parts[0])))
                            lastMatchResult = true;
                    if (parts.Contains("text"))
                        if (Regex.IsMatch(fileName, CreateRegexFromFilePattern(parts[0])))
                            lastMatchResult = false;
                }
                return lastMatchResult;
            }

            return null;
        }

        private static string CreateRegexFromFilePattern(string pattern)
        {
            return pattern.Replace(".", "\\.").Replace("*", ".*").Replace("?", ".");
        }

        public static bool IsImage(string fileName)
        {
            return HasMatchingExtension(ImageExtensions, fileName);
        }

        private static bool HasMatchingExtension(IEnumerable<string> extensions, string fileName)
        {
            return extensions.Any(extension => fileName.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}