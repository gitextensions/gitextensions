using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using JetBrains.Annotations;

namespace GitCommands
{
    public interface ISshPathLocator
    {
        string Find(string gitBinDirectory);
    }

    public sealed class SshPathLocator : ISshPathLocator
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironmentAbstraction _environment;

        public SshPathLocator(IFileSystem fileSystem, IEnvironmentAbstraction environment)
        {
            _fileSystem = fileSystem;
            _environment = environment;
        }

        public SshPathLocator()
            : this(new FileSystem(), new EnvironmentAbstraction())
        {
        }

        /// <summary>
        /// Gets the git SSH command;
        /// If the environment variable is not set, will try to find ssh.exe in git installation directory;
        /// If not found, will return "".
        /// </summary>
        public string Find(string gitBinDirectory)
        {
            var ssh = _environment.GetEnvironmentVariable("GIT_SSH", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(ssh))
            {
                ssh = GetSshFromGitDir(gitBinDirectory);
            }

            return ssh ?? "";
        }

        [CanBeNull]
        private string GetSshFromGitDir(string gitBinDirectory)
        {
            if (string.IsNullOrEmpty(gitBinDirectory))
            {
                return null;
            }

            try
            {
                // gitBinDirectory will normally end with a directory separator
                // (at least this is what AppSettings.GitBinDir ensures),
                // but then GetParent() returns the same directory, only without the trailing separator
                var gitDirInfo = _fileSystem.Directory.GetParent(gitBinDirectory.RemoveTrailingPathSeparator());
                if (gitDirInfo == null)
                {
                    return null;
                }

                return _fileSystem.Directory.EnumerateFiles(gitDirInfo.FullName, "ssh.exe", SearchOption.AllDirectories).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
