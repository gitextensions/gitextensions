using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace GitCommands
{
    public interface ISshPathLocator
    {
        public string? GetSshFromGitDir(string gitBinDirectory);
    }

    public sealed class SshPathLocator : ISshPathLocator
    {
        private readonly IFileSystem _fileSystem;

        public SshPathLocator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public SshPathLocator()
            : this(new FileSystem())
        {
        }

        /// <summary>
        /// Get ssh path from Git installation.
        /// (Also used by plugins to get the OpenSSH path).
        /// </summary>
        /// <param name="gitBinDirectory">Git installation directory.</param>
        /// <returns>Path to ssh.exe or null.</returns>
        public string? GetSshFromGitDir(string gitBinDirectory)
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
                IDirectoryInfo gitDirInfo = _fileSystem.Directory.GetParent(gitBinDirectory.RemoveTrailingPathSeparator());
                if (gitDirInfo is null)
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
