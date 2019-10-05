using System.IO;
using System.Linq;

namespace GitUI.Theming
{
    internal class ThemeDeployment
    {
        private readonly FormThemeEditorController _editorController;

        public ThemeDeployment(FormThemeEditorController editorController)
        {
            _editorController = editorController;
        }

        public void DeployThemesToUserDirectory()
        {
            string deployedSaveDirectory = _editorController.AppDirectory;
            if (!Directory.Exists(deployedSaveDirectory))
            {
                return;
            }

            string saveDirectory = _editorController.UserDirectory;
            Directory.CreateDirectory(saveDirectory);

            var sourceFiles = Directory.EnumerateFiles(deployedSaveDirectory,
                    "*" + FormThemeEditorController.Extension,
                    SearchOption.TopDirectoryOnly)
                .Where(file => !_editorController.IsCurrentThemeFile(file))
                .ToArray();

            // to prevent permanent loss of user customizations
            BackupExistingFiles(sourceFiles, saveDirectory);
            foreach (string file in sourceFiles)
            {
                string targetFile = GetPathInDirectory(file, saveDirectory);
                File.Copy(file, targetFile, overwrite: true);
            }
        }

        private void BackupExistingFiles(string[] sourceFiles, string saveDirectory)
        {
            var targetFiles = sourceFiles
                .Select(file => GetPathInDirectory(file, saveDirectory))
                .ToArray();

            if (!targetFiles.Any(File.Exists))
            {
                return;
            }

            var modified = Enumerable.Range(0, sourceFiles.Length)
                .Where(i =>
                    File.Exists(targetFiles[i]) &&

                    // prevent accidentally reading > 1MB files
                    new FileInfo(targetFiles[i]).Length < (1 << 20) &&
                    File.ReadAllText(targetFiles[i]) != File.ReadAllText(sourceFiles[i]))
                .Select(i => targetFiles[i])
                .ToArray();

            if (modified.Length != 0)
            {
                var backupDirectory = CreateBackupDirectory(saveDirectory);
                foreach (string file in modified)
                {
                    File.Move(file, GetPathInDirectory(file, backupDirectory));
                }
            }
        }

        private string CreateBackupDirectory(string saveDirectory)
        {
            var backupDirectory = Path.Combine(saveDirectory, "backup");
            Directory.CreateDirectory(backupDirectory);
            var oldBackups = Directory.GetDirectories(backupDirectory);
            var number = 1 + oldBackups
                .Select(Path.GetFileName)
                .Where(_ => _.All(char.IsDigit))
                .Select(int.Parse)
                .DefaultIfEmpty(0)
                .Max();
            var path = Path.Combine(backupDirectory, number.ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        private static string GetPathInDirectory(string file, string targetDirectory) =>
            Path.Combine(targetDirectory, Path.GetFileName(file));
    }
}
