using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;

namespace GitCommands
{
    /// <summary>
    /// Provides the ability to extract icons associated with file types.
    /// </summary>
    public interface IFileAssociatedIconProvider
    {
        /// <summary>
        /// Retrieves the icon associated with the given file type.
        /// </summary>
        /// <param name="workingDirectory">The git repository working directory.</param>
        /// <param name="relativeFilePath">The relative path to the file.</param>
        /// <returns>The icon associated with the given file type or <see langword="null"/>.</returns>
        Icon Get(string workingDirectory, string relativeFilePath);
    }

    public sealed class FileAssociatedIconProvider : IFileAssociatedIconProvider
    {
        private readonly IFileSystem _fileSystem;
        private static readonly ConcurrentDictionary<string, Icon> LoadedFileIcons = new ConcurrentDictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);

        public FileAssociatedIconProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public FileAssociatedIconProvider()
            : this(new FileSystem())
        {
        }

        // unit tests only
        internal int CacheCount => LoadedFileIcons.Count;

        /// <summary>
        /// Retrieves the icon associated with the given file type.
        /// The retrieved icons are cached by extensions.
        /// </summary>
        /// <param name="workingDirectory">The git repository working directory.</param>
        /// <param name="relativeFilePath">The relative path to the file.</param>
        /// <returns>The icon associated with the given file type or <see langword="null"/>.</returns>
        /// <remarks>
        /// The method takes two parameters to performance reasons - the full path is established
        /// only if the file type has not been processed already and the extensions is not cached.
        /// </remarks>
        public Icon Get(string workingDirectory, string relativeFilePath)
        {
            var extension = Path.GetExtension(relativeFilePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }

            var icon = LoadedFileIcons.GetOrAdd(extension, ext =>
            {
                string tempFile = null;
                try
                {
                    // if the file doesn't exist - create a blank temp file with the required extension
                    // so we can call Icon.ExtractAssociatedIcon on it
                    // this may have a slight overhead, however an alternative would be extracting
                    // extensions from the registry and using p/invokes and WinAPI, which have
                    // significantly higher maintenance overhead.

                    var fullPath = Path.Combine(workingDirectory, relativeFilePath);
                    if (!_fileSystem.File.Exists(fullPath))
                    {
                        tempFile = CreateTempFile(Path.GetFileName(fullPath));
                        fullPath = tempFile;
                    }

                    return Icon.ExtractAssociatedIcon(fullPath);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(tempFile))
                    {
                        DeleteFile(tempFile);
                    }
                }
            });
            return icon;
        }

        private string CreateTempFile(string fileName)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), fileName);
            _fileSystem.File.WriteAllText(tempFile, string.Empty);
            return tempFile;
        }

        private void DeleteFile(string path)
        {
            try
            {
                _fileSystem.File.Delete(path);
            }
            catch
            {
                // do nothing
            }
        }

        // unit tests only
        internal void ResetCache()
        {
            LoadedFileIcons.Clear();
        }
    }
}