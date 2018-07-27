using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    /// <summary>
    /// Provides the ability to resolve the location of .git folder.
    /// </summary>
    public interface IGitDirectoryResolver
    {
        /// <summary>
        /// Resolves the .git folder for the given repository.
        /// </summary>
        /// <param name="repositoryPath">The repository working folder.</param>
        /// <returns>The resolved location of .git folder.</returns>
        [NotNull]
        string Resolve([NotNull] string repositoryPath);
    }

    /// <summary>
    /// Resolves the location of .git folder.
    /// </summary>
    public sealed class GitDirectoryResolver : IGitDirectoryResolver
    {
        private readonly IFileSystem _fileSystem;

        public GitDirectoryResolver([NotNull] IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public GitDirectoryResolver()
            : this(new FileSystem())
        {
        }

        /// <summary>
        /// Resolves the .git folder for the given repository.
        /// </summary>
        /// <param name="repositoryPath">The repository working folder.</param>
        /// <returns>
        /// The resolved location of .git folder.
        /// <list type="table">
        ///   <item>
        ///     <term>If <paramref name="repositoryPath"/> is an empty string</term>
        ///     <description>it resolves to <see cref="string.Empty"/></description>
        ///   </item>
        ///   <item>
        ///     <term>If <paramref name="repositoryPath"/> contains a .git file (i.e. the repository is a submodule)</term>
        ///     <description>it resolves to the location of the submodule's .git folder under the superproject's .git folder with the trailing slash</description>
        ///   </item>
        ///   <item>
        ///     <term>If <paramref name="repositoryPath"/> contains .git folder</term>
        ///     <description>it resolves to the .git folder with the trailing slash</description>
        ///   </item>
        ///   <item>
        ///     <term>else</term>
        ///     <description>it returns <paramref name="repositoryPath"/> unchanged.</description>
        ///   </item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="repositoryPath"/> is <see langword="null"/>.</exception>
        public string Resolve(string repositoryPath)
        {
            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }

            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                return string.Empty;
            }

            var gitPath = Path.Combine(repositoryPath, ".git");
            if (_fileSystem.File.Exists(gitPath))
            {
                const string gitdir = "gitdir:";
                var line = _fileSystem.File.ReadLines(gitPath).FirstOrDefault(l => l.StartsWith(gitdir));
                if (line != null)
                {
                    string path = line.Substring(gitdir.Length).Trim().ToNativePath();
                    if (Path.IsPathRooted(path))
                    {
                        return path.EnsureTrailingPathSeparator();
                    }

                    return Path.GetFullPath(Path.Combine(repositoryPath, path)).EnsureTrailingPathSeparator();
                }
            }

            gitPath = gitPath.EnsureTrailingPathSeparator();
            return !_fileSystem.Directory.Exists(gitPath) ? repositoryPath : gitPath;
        }
    }
}