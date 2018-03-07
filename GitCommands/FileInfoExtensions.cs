using System;
using System.IO;

namespace GitCommands
{
    public static class FileInfoExtensions
    {
        /// <summary>
        ///   Remove all attributes that could cause the file to be read-only
        ///   and restores them later
        /// </summary>
        public static void MakeFileTemporaryWritable(string fileName, Action<string> writableAction)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                // The file doesn't exist yet, no need to make it writable
                writableAction(fileName);
                return;
            }

            var oldAttributes = fileInfo.Attributes;
            fileInfo.Attributes = FileAttributes.Normal;
            writableAction(fileName);

            fileInfo.Refresh();
            if (fileInfo.Exists)
            {
                fileInfo.Attributes = oldAttributes;
            }
        }
    }
}