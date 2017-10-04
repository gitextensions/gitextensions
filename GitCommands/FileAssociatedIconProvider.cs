using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using GitCommands.Utils;

namespace GitCommands
{
    public interface IFileAssociatedIconProvider
    {
        Icon Get(string fullPath);
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


        public Icon Get(string fullPath)
        {
            if (EnvUtils.IsMonoRuntime())
            {
                // Mono does not support icon extraction
                // https://github.com/mono/mono/blob/master/mcs/class/System.Drawing/System.Drawing/Icon.cs#L314
                return null;
            }

            var extension = Path.GetExtension(fullPath);
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

                    if (!_fileSystem.File.Exists(fullPath))
                    {
                        tempFile = CreateTempFile(Path.GetFileName(fullPath));
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