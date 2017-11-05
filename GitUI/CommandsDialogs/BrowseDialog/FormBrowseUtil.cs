using GitCommands;
using System.IO;
using GitCommands.Git;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    internal class FormBrowseUtil
    {
        public static string GetFullPathFromGitItem(GitModule gitModule, GitItem gitItem)
        {
            return GetFullPathFromFilename(gitModule, gitItem.FileName);
        }

        public static string GetFullPathFromGitItemStatus(GitModule gitModule, GitItemStatus gitItemStatus)
        {
            return GetFullPathFromFilename(gitModule, gitItemStatus.Name);
        }

        public static string GetFullPathFromFilename(GitModule gitModule, string filename)
        {
            var filePath = Path.Combine(gitModule.WorkingDir, filename.ToNativePath());

            return filePath;
        }

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
