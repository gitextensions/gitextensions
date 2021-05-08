using System;
using System.IO;
using GitExtUtils;
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
        string? Resolve(string? path);
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
        /// <remarks>
        /// Behaves similar to the .NET Core 2.1 version that do not throw on paths with illegal
        /// Windows characters (that could be OK in Git paths or for cross platform) but returns
        /// null instead of an possible path.</remarks>
        /// <param name="path">Folder or file path to resolve.</param>
        /// <returns>
        /// <paramref name="path" /> if <paramref name="path" /> is rooted; otherwise resolved path from working directory of the current repository.
        /// </returns>
        /// <exception cref="PathTooLongException">The resolved path is too long (greater than 248 characters).</exception>
        public string? Resolve(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                if (Path.IsPathRooted(path))
                {
                    return path;
                }
            }
            catch (ArgumentException)
            {
                // Illegal characters in path.
                // This is used for Git paths that may not be possible in the host file system
                return null;
            }

            var workingDir = _getWorkingDir();
            if (string.IsNullOrWhiteSpace(workingDir))
            {
                workingDir = Environment.CurrentDirectory;
            }

            var basePath = Path.GetFullPath(workingDir);
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString())
                && !basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }

            return PathUtil.Resolve(basePath, path);
        }
    }
}
