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
            if (!File.Exists(fileName))
            {
                // The file doesn't exist yet, no need to make it writable
                writableAction(fileName);
                return;
            }

            FileInfo fileInfo = new(fileName);
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
