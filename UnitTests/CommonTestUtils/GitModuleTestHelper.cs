using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands;

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
            module.Init(bare: false, shared: false);
            Module = module;

            // Don't assume global user/email
            Module.GitExecutable.GetOutput(@"config user.name ""author""");
            Module.GitExecutable.GetOutput(@"config user.email ""author@mail.com""");

            return;

            string GetTemporaryPath()
            {
                var tempPath = Path.GetTempPath();

                // workaround macOS symlinking its temp folder
                if (tempPath.StartsWith("/var"))
                {
                    tempPath = "/private" + tempPath;
                }

                return Path.Combine(tempPath, Path.GetRandomFileName());
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public GitModule Module { get; private set; }

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

        /// <summary>
        /// Adds 'helper' as a submodule of the current helper.
        /// </summary>
        /// <param name="helper">GitModuleTestHelper to add as a submodule of this.</param>
        /// <param name="path">Relative submodule path.</param>
        /// <returns>Module of added submodule</returns>
        public GitModule AddSubmodule(GitModuleTestHelper helper, string path)
        {
            // Submodules require at least one commit
            helper.Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Empty commit""");

            Module.GitExecutable.GetOutput(GitCommandHelpers.AddSubmoduleCmd(helper.Module.WorkingDir.ToPosixPath(), path, null, true));
            Module.GitExecutable.GetOutput(@"commit -am ""Add submodule""");

            return new GitModule(Path.Combine(Module.WorkingDir, path).ToPosixPath(), Module.GitExecutable);
        }

        /// <summary>
        /// Updates and inits submodules recursively, returning all submodule Modules
        /// </summary>
        /// <returns>All submodule Modules</returns>
        public IEnumerable<GitModule> GetSubmodulesRecursive()
        {
            Module.GitExecutable.GetOutput(@"submodule update --init --recursive");
            var paths = Module.GetSubmodulesLocalPaths(recursive: true);
            return paths.Select(path => new GitModule(Path.Combine(Module.WorkingDir, path).ToNativePath(), Module.GitExecutable));
        }

        public void Dispose()
        {
            try
            {
                // if settings have been set, the corresponding local config file is in the directory
                // we want to delete, so we need to make sure the timers that will try to auto-save there
                // are stopped before actually deleting, else the timers will throw on a background thread.
                // Note that the intermittent failures mentioned below are likely related too.
                Module.EffectiveConfigFile.SettingsCache.Dispose();
                Module.EffectiveSettings.SettingsCache.Dispose();

                // Directory.Delete seems to intermittently fail, so delete the files first before deleting folders
                foreach (var file in Directory.GetFiles(TemporaryPath, "*", SearchOption.AllDirectories))
                {
                    if (File.GetAttributes(file).HasFlag(FileAttributes.ReparsePoint))
                    {
                        continue;
                    }

                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                // Delete tends to fail on the first try, so give it a few tries as a best effort.
                // By this point, all files have been deleted anyway, so this is mainly about removing
                // empty directories.
                for (int tries = 0; tries < 10; ++tries)
                {
                    try
                    {
                        Directory.Delete(TemporaryPath, true);
                        break;
                    }
                    catch
                    {
                    }
                }
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
    }
}