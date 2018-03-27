using System;
using System.IO;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace CommonTestUtils
{
    public class GitModuleTestHelper : IDisposable
    {
        /// <summary>
        /// Creates a throw-away new repository in a temporary location.
        /// </summary>
        public GitModuleTestHelper(string repositoryName = "repo1")
        {
            TemporaryPath = GetTemporaryPath();

            string path = Path.Combine(TemporaryPath, repositoryName);
            if (Directory.Exists(path))
            {
                throw new ArgumentException($"Repository '{path}' already exists", nameof(repositoryName));
            }

            Directory.CreateDirectory(path);

            var module = new GitModule(path);
            module.Init(false, false);
            Module = module;
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public GitModule Module { get; }

        /// <summary>
        /// Gets the temporary path where test repositories will be created for integration tests.
        /// </summary>
        public string TemporaryPath { get; }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <returns>The path to the newly created file.</returns>
        public string CreateFile(string parentDir, string fileName, string fileContent)
        {
            EnsureCreatedInTempFolder(parentDir);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            string filePath = Path.Combine(parentDir, fileName);
            File.WriteAllText(filePath, fileContent ?? string.Empty);

            return filePath;
        }

        /// <summary>
        /// Creates a new file under the working folder to the specified directory,
        /// writes the specified string to the file and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <returns>The path to the newly created file.</returns>
        public string CreateRepoFile(string fileRelativePath, string fileName, string fileContent)
        {
            if (Path.IsPathRooted(fileRelativePath))
            {
                throw new ArgumentException("Relative path expected", nameof(fileRelativePath));
            }

            string parentDir = Path.Combine(Module.WorkingDir, fileRelativePath);
            EnsureCreatedInTempFolder(parentDir);

            return CreateFile(parentDir, fileName, fileContent);
        }

        /// <summary>
        /// Creates a new file to the root of the working folder,
        /// writes the specified string to the file and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <returns>The path to the newly created file.</returns>
        public string CreateRepoFile(string fileName, string fileContent)
        {
            return CreateRepoFile("", fileName, fileContent);
        }

        public void Dispose()
        {
            try
            {
                // if settings have been set, the corresponding local config file is in the directory
                // we want to delete, so we need to make sure the timers that will try to auto-save there
                // are stopped before actually deleting, else the timers will throw on a background thread.
                // Note that the intermittent failures mentioned below are likely related too.
                ((GitModule)Module).EffectiveConfigFile?.SettingsCache?.Dispose();
                ((GitModule)Module).EffectiveSettings?.SettingsCache?.Dispose();

                // Directory.Delete seems to intermittently fail, so delete the files first before deleting folders
                Directory.GetFiles(TemporaryPath, "*", SearchOption.AllDirectories).ForEach(File.Delete);
                Directory.Delete(TemporaryPath, true);
            }
            catch
            {
                // do nothing
            }
        }

        private void EnsureCreatedInTempFolder(string path)
        {
            if (!path.StartsWith(TemporaryPath))
            {
                throw new ArgumentException("The given module does not belong to this helper.");
            }
        }

        private static string GetTemporaryPath()
        {
            var tempPath = Path.GetTempPath();

            // workaround macOS symlinking its temp folder
            if (tempPath.StartsWith("/var"))
            {
                tempPath = "/private" + tempPath;
            }

            string tempDirectory = Path.Combine(tempPath, Path.GetRandomFileName());
            return tempDirectory;
        }
    }
}