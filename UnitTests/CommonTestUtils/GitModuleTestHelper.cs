using System.Diagnostics;
using System.Text;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git.Commands;
using GitExtUtils;
using NUnit.Framework;

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

            GitModule module = new(path);
            module.Init(bare: false, shared: false);
            Module = module;

            // Don't assume global user/email
            SetRepoConfig(module);

            // Stage operations may fail due to different line endings, so want only warning and not a fatal error
            //
            //  fatal: LF would be replaced by CRLF in .gitmodules
            //         Failed to register submodule 'repo2'
            module.GitExecutable.GetOutput(@"config core.safecrlf false");

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

        public string DeleteRepoFile(string fileName)
        {
            string parentDir = Module.WorkingDir;
            string filePath = Path.Combine(parentDir, fileName);
            if (!Directory.Exists(parentDir))
            {
                return filePath;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return filePath;
        }

        /// <summary>
        /// Set dummy user and email locally for the module along with specific tests configs, no global setting in AppVeyor
        /// Must also be set on the submodule, local settings are not included when adding it
        /// </summary>
        private void SetRepoConfig(GitModule module)
        {
            module.LocalConfigFile.SetString(SettingKeyString.UserName, "author");
            module.LocalConfigFile.SetString(SettingKeyString.UserEmail, "author@mail.com");
            module.LocalConfigFile.FilesEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            module.LocalConfigFile.SetString(SettingKeyString.AllowFileProtocol, "always"); // git version 2.38.1 and later disabled file protocol by default
            module.LocalConfigFile.Save();
        }

        /// <summary>
        /// Adds 'subModuleHelper' as a submodule of the current subModuleHelper.
        /// </summary>
        /// <param name="subModuleHelper">GitModuleTestHelper to add as a submodule of this.</param>
        /// <param name="path">Relative submodule path.</param>
        public void AddSubmodule(GitModuleTestHelper subModuleHelper, string path)
        {
            // Submodules require at least one commit
            subModuleHelper.Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Initial empty commit""");

            // Ensure config is set to allow file submodules
            string fileEnabled = Module.GetEffectiveSetting(SettingKeyString.AllowFileProtocol)?.Trim('\n');
            Assert.That(fileEnabled == "always");

            // Even though above is set, adding a file protocol submodule fails unless -c... is used for protocol.file.allow config.
            IEnumerable<GitConfigItem> cfgs = GitCommandHelpers.GetAllowFileConfig();

            var result = Module.GitExecutable.Execute(GitCommandHelpers.AddSubmoduleCmd(subModuleHelper.Module.WorkingDir.ToPosixPath(), path, null, true, cfgs));
            Debug.WriteLine(result.AllOutput);

            Module.GitExecutable.GetOutput(@"commit -am ""Add submodule""");
        }

        /// <summary>
        /// Updates and inits submodules recursively, returning all submodule Modules
        /// </summary>
        /// <returns>All submodule Modules</returns>
        public IEnumerable<GitModule> GetSubmodulesRecursive()
        {
            IEnumerable<GitConfigItem> configs = GitCommandHelpers.GetAllowFileConfig();
            ArgumentString args = GitCommandHelpers.SubmoduleUpdateCmd(name: null, configs: configs);

            Module.GitExecutable.Execute(args);
            var paths = Module.GetSubmodulesLocalPaths(recursive: true);
            return paths.Select(path =>
            {
                GitModule module = new(Path.Combine(Module.WorkingDir, path).ToNativePath());
                SetRepoConfig(module);
                return module;
            });
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
