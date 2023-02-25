using GitCommands;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    internal static class FormBrowseUtil
    {
        public static bool FileOrParentDirectoryExists(string path)
        {
            return File.Exists(path) || (Directory.Exists(path) && new FileInfo(path).Directory.Exists);
        }

        public static bool IsFileOrDirectory(string? path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public static void ShowFileOrParentFolderInFileExplorer(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fileInfo = new(path);
                OsShellUtil.SelectPathInFileExplorer(fileInfo.FullName);
            }
            else if (Directory.Exists(path))
            {
                FileInfo fileInfo = new(path);
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
