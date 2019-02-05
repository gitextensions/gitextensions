using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class FileHelper
    {
        private static readonly IEnumerable<string> BinaryExtensions = new[]
        {
            ".avi", // movie
            ".bmp", // image
            ".dat", // data file
            ".bin", // binary file
            ".dll", // dynamic link library
            ".doc", // office word
            ".docx", // office word
            ".ppt", // office powerpoint
            ".pps", // office powerpoint
            ".pptx", // office powerpoint
            ".ppsx", // office powerpoint
            ".dwg", // autocad
            ".exe", // executable
            ".gif", // image
            ".ico", // icon
            ".jpg", // image
            ".jpeg", // image
            ".mpg", // movie
            ".mpeg", // movie
            ".msi", // installer
            ".pdf", // pdf document
            ".png", // image
            ".pdb", // debug file
            ".sc1", // screen file
            ".tif", // image
            ".tiff", // image
            ".vsd", // microsoft visio
            ".vsdx", // microsoft
            ".xls", // microsoft excel
            ".xlsx", // microsoft excel
            ".odt" // Open office
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

        public static bool IsBinaryFileName(GitModule module, string fileName)
        {
            return IsBinaryAccordingToGitAttributes(module, fileName)
                ?? HasMatchingExtension(BinaryExtensions, fileName);
        }

        /// <returns>null if no info in .gitattributes (or ambiguous). True if marked as binary, false if marked as text</returns>
        private static bool? IsBinaryAccordingToGitAttributes(GitModule module, string fileName)
        {
            string[] diffValues = { "set", "astextplain", "ada", "bibtext", "cpp", "csharp", "fortran", "html", "java", "matlab", "objc", "pascal", "perl", "php", "python", "ruby", "tex" };
            var cmd = new GitArgumentBuilder("check-attr")
            {
                "-z",
                "diff",
                "text",
                "crlf",
                "eol",
                "--",
                fileName.Quote()
            };
            string result = module.GitExecutable.GetOutput(cmd);
            var lines = result.Split('\n', '\0');
            var attributes = new Dictionary<string, string>();
            for (int i = 0; i < lines.Length - 2; i += 3)
            {
                attributes[lines[i + 1].Trim()] = lines[i + 2].Trim();
            }

            if (attributes.TryGetValue("diff", out var diff))
            {
                if (diff == "unset")
                {
                    return true;
                }

                if (diffValues.Contains(diff))
                {
                    return false;
                }
            }

            if (attributes.TryGetValue("text", out var text))
            {
                if (text != "unset" && text != "unspecified")
                {
                    return false;
                }
            }

            if (attributes.TryGetValue("crlf", out var crlf))
            {
                if (crlf != "unset" && crlf != "unspecified")
                {
                    return false;
                }
            }

            if (attributes.TryGetValue("eol", out var eol))
            {
                if (eol != "unset" && eol != "unspecified")
                {
                    return false;
                }
            }

            return null;
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

        public static bool IsBinaryFileAccordingToContent([CanBeNull] byte[] content)
        {
            // Check for binary file.
            if (content != null && content.Length > 0)
            {
                int nullCount = 0;
                foreach (char c in content)
                {
                    if (c == '\0')
                    {
                        nullCount++;
                    }

                    if (nullCount > 5)
                    {
                        break;
                    }
                }

                if (nullCount > 5)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsBinaryFileAccordingToContent(string content)
        {
            // Check for binary file.
            if (!string.IsNullOrEmpty(content))
            {
                int nullCount = 0;
                foreach (char c in content)
                {
                    if (c == '\0')
                    {
                        nullCount++;
                        if (nullCount > 5)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
