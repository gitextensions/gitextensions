using System;

namespace GitCommands.Git
{
    public class FileDeleteException : Exception
    {
        private const string DefaultMessage = "Could not delete the file";

        public FileDeleteException(string fileName)
            : base(DefaultMessage)
        {
            FileName = fileName;
        }

        public FileDeleteException(string fileName, Exception inner)
            : base(inner.Message, inner)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Gets the name of the file which could not be deleted.
        /// </summary>
        public string FileName { get; }
    }
}