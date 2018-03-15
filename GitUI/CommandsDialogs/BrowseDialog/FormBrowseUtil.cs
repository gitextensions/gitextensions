using System.IO;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    internal static class FormBrowseUtil
    {
        public static bool FileOrParentDirectoryExists(string path)
        {
            var fileInfo = new FileInfo(path);
            return fileInfo.Exists || fileInfo.Directory.Exists;
        }

        public static bool IsFileOrDirectory(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public static void ShowFileOrParentFolderInFileExplorer(string path)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                OsShellUtil.SelectPathInFileExplorer(fileInfo.FullName);
            }
            else if (fileInfo.Directory.Exists)
            {
                OsShellUtil.OpenWithFileExplorer(fileInfo.Directory.FullName);
            }
        }

        public static void ShowFileOrFolderInFileExplorer(string path)
        {
            if (File.Exists(path))
            {
                OsShellUtil.SelectPathInFileExplorer(path);
            }
            else if (Directory.Exists(path))
            {
                OsShellUtil.OpenWithFileExplorer(path);
            }
        }
    }
}
