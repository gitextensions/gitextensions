using System;
using System.Collections.Generic;
using System.Text;

namespace GitUI
{
    public static class FileHelper
    {
        public static bool IsBinaryFile(string fileName)
        {
            return (fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".tiff", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".mpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".mpeg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".doc", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".vsd", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".docx", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".xls", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".vsdx", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".pdb", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".avi", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
