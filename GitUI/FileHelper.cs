using System;
using System.Collections.Generic;

namespace GitUI
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
            ".pdf",
            ".png",
            ".pdb",
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
            return HasMatchingExtension(BinaryExtensions, fileName);
        }

        public static bool IsImage(string fileName)
        {
            return HasMatchingExtension(ImageExtensions, fileName);
        }

        private static bool HasMatchingExtension(IEnumerable<string> extensions, string fileName)
        {
            foreach (string extension in extensions)
            {
                if (fileName.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}