using System.Runtime.Serialization;

namespace GitCommands.Git
{
    [Serializable]
    public class FileDeleteException : Exception
    {
        public FileDeleteException(string fileName, Exception inner)
            : base(inner.Message, inner)
        {
            FileName = fileName;
        }

        protected FileDeleteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the name of the file which could not be deleted.
        /// </summary>
        public string FileName { get; }
    }
}
