using System;
using System.Collections.Generic;
using System.Linq;

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
            string[] diffvals = { "set", "astextplain", "ada", "bibtext", "cpp", "csharp", "fortran", "html", "java", "matlab", "objc", "pascal", "perl", "php", "python", "ruby", "tex" };
            string cmd = "check-attr -z diff text crlf eol -- " + fileName;
            string result = aModule.RunGitCmd(cmd);
            var lines = result.Split(new[] { '\n', '\0' }, StringSplitOptions.RemoveEmptyEntries);
            var attributes = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var values = line.Split(':');
                if (values.Length == 3)
                    attributes[values[1].Trim()] = values[2].Trim();
            }

            string val;
            if (attributes.TryGetValue("diff", out val))
            {
                if (val == "unset")
                    return true;
                if (diffvals.Contains(val))
                    return false;
            }
            if (attributes.TryGetValue("text", out val))
            {
                if (val != "unset" && val != "unspecified")
                    return false;
            }
            if (attributes.TryGetValue("crlf", out val))
            {
                if (val != "unset" && val != "unspecified")
                    return false;
            }
            if (attributes.TryGetValue("eol", out val))
            {
                if (val != "unset" && val != "unspecified")
                    return false;
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
