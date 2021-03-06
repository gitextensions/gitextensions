using System;
using System.IO.Abstractions;

namespace GitExtUtils
{
    public static class FileSystemWrapper
    {
        private static readonly Lazy<FileSystem> _defaultFileSystem = new();

        public static void DeleteFile(string fileName, IFileSystem? fileSystem = null)
            => WrapFileException(fileName, fileSystem, "Delete", fs => fs.File.Delete(fileName));

        private static void WrapFileException(string fileName, IFileSystem? fileSystem, string operation, Action<IFileSystem> fileAction)
        {
            try
            {
                fileSystem ??= _defaultFileSystem.Value;
                fileAction(fileSystem);
            }
            catch (Exception fileOperationException)
            {
                string? directory = null;
                try
                {
                    if (fileSystem?.Path.IsPathRooted(fileName) ?? false)
                    {
                        directory = fileSystem.Path.GetDirectoryName(fileName);
                        fileName = fileSystem.Path.GetFileName(fileName);
                    }
                    else
                    {
                        directory = fileSystem?.Directory.GetCurrentDirectory();
                    }
                }
                catch (Exception descriptionException)
                {
                    try
                    {
                        fileOperationException.Data.Add(nameof(descriptionException), descriptionException?.Message);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                throw new ExternalOperationException(operation, obj: fileName, arguments: null, directory, fileOperationException);
            }
        }
    }
}
