using System;
using System.Collections.Generic;
using System.IO;
using GitUIPluginInterfaces;

namespace GitCommands
{
    /// <summary>
    /// Provides the ability to resolve full path.
    /// </summary>
    public interface IFullPathResolver
    {
        /// <summary>
        /// Resolves the provided path (folder or file) against the current working directory.
        /// </summary>
        /// <param name="path">Folder or file path to resolve.</param>
        /// <returns>
        /// <paramref name="path"/> if <paramref name="path"/> is rooted; otherwise resolved path from <see cref="IGitModule.WorkingDir"/>.
        /// </returns>
        string Resolve(string path);

        /// <summary>
        /// Finds if any of the provided strings to resolve exists as a file.
        /// </summary>
        /// <param name="names">List of names to resolve and check</param>
        /// <returns>
        /// True if at least one file exists.
        /// </returns>
        bool AnyLocalExists(IEnumerable<string> names);
    }

    public sealed class FullPathResolver : IFullPathResolver
    {
        private readonly Func<string> _getWorkingDir;

        public FullPathResolver(Func<string> getWorkingDir)
        {
            _getWorkingDir = getWorkingDir;
        }

        /// <inheritdoc />
        /// <summary>
        /// Resolves the provided path (folder or file) against the current working directory.
        /// </summary>
        /// <param name="path">Folder or file path to resolve.</param>
        /// <returns>
        /// <paramref name="path" /> if <paramref name="path" /> is rooted; otherwise resolved path from working directory of the current repository.
        /// </returns>
        /// <exception cref="PathTooLongException">The resolved path is too long (greater than 248 characters).</exception>
        public string Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            var fullPath = Path.GetFullPath(Path.Combine(_getWorkingDir() ?? "", path));
            var uri = new Uri(fullPath);
            return uri.LocalPath;
        }

        /// <inheritdoc />
        /// <summary>
        /// Finds if any of the provided strings to resolve exists as a file.
        /// </summary>
        /// <param name="names">List of names to resolve and check</param>
        /// <returns>
        /// True if at least one file exists.
        /// </returns>
        public bool AnyLocalExists(IEnumerable<string> names)
        {
            bool localExists = false;
            foreach (var name in names)
            {
                string filePath = Resolve(name);
                if (File.Exists(filePath))
                {
                    localExists = true;
                    break;
                }
            }

            return localExists;
        }
    }
}