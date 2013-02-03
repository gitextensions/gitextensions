using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitCommands
{
    public static class FileHelper
    {
        private static readonly IEnumerable<string> BinaryExtensions = new[]
        {
            ".avi",//movie
            ".bmp",//image
            ".dat",//data file
            ".bin", //binary file
            ".dll",//dynamic link library
            ".doc", //office word
            ".docx",//office word
            ".ppt",//office powerpoint
            ".pps",//office powerpoint
            ".pptx",//office powerpoint
            ".ppsx",//office powerpoint
            ".dwg",//autocad
            ".exe",//executable
            ".gif",//image
            ".ico",//icon
            ".jpg",//image
            ".jpeg",//image
            ".mpg",//movie
            ".mpeg",//movie
            ".msi",//instaler
            ".pdf",//pdf document
            ".png",//image
            ".pdb",//debug file
            ".sc1",//screen file
            ".tif",//image
            ".tiff",//image
            ".vsd",//microsoft visio
            ".vsdx",//microsoft
            ".xls",//microsoft excel
            ".xlsx",//microsoft excel
            ".odt" //Open office
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

        public static bool IsBinaryFile(GitModule aModule, string fileName)
        {
            var t = IsBinaryAccordingToGitAttributes(aModule, fileName);
            if (t.HasValue)
                return t.Value;
            return HasMatchingExtension(BinaryExtensions, fileName);
        }

        /// <returns>null if no info in .gitattributes (or ambiguous). True if marked as binary, false if marked as text</returns>
        private static bool? IsBinaryAccordingToGitAttributes(GitModule aModule, string fileName)
        {
            string gitAttributesPath = Path.Combine(aModule.WorkingDir, ".gitattributes");
            if (File.Exists(gitAttributesPath))
            {
                var lines = File.ReadLines(gitAttributesPath);
                bool? diff = null;
                bool? text = null;
                foreach (var parts in lines.Select(line => line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries)))
                {
                    if (parts.Length < 2 || parts[0][0] == '#')
                        continue;
                    try
                    {
                        if (!Regex.IsMatch(fileName, CreateRegexFromFilePattern(parts[0])))
                            continue;
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    if (parts.Contains("binary"))
                    {
                        diff = false;
                        text = false;
                    }
                    else
                    {
                        if (parts.Contains("diff"))
                            diff = true;
                        else if (parts.Contains("-diff"))
                            diff = false;
                        else if (parts.Contains("!diff"))
                            diff = null;
                        if (parts.Contains("text") || parts.Contains("crlf"))
                            text = true;
                        else if (parts.Contains("-text") || parts.Contains("-crlf"))
                            text = false;
                        else if (parts.Contains("!text") || parts.Contains("!crlf"))
                            text = null;
                    }
                }
                //detect explicit binary attributes (no textual diffs allowed)
                if (diff == false)
                    return true;
                //detect explicit text-based file (diff attr set) or implicit text-based file (text attr set)
                if (diff == true && text == true)
                    return false;
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

        #region binary file check
        public static bool IsBinaryFileAccordingToContent(byte[] content)
        {
            //Check for binary file.
            if (content != null && content.Length > 0)
            {
                int nullCount = 0;
                foreach (char c in content)
                {
                    if (c == '\0')
                        nullCount++;
                    if (nullCount > 5) break;
                }

                if (nullCount > 5)
                    return true;
            }

            return false;
        }

        public static bool IsBinaryFileAccordingToContent(string content)
        {
            //Check for binary file.
            if (!string.IsNullOrEmpty(content))
            {
                int nullCount = 0;
                foreach (char c in content)
                {
                    if (c == '\0')
                        nullCount++;
                    if (nullCount > 5) break;
                }

                if (nullCount > 5)
                    return true;
            }

            return false;
        }
        #endregion
    }
}