namespace GitCommands.Git
{
    public class FileDeleteException : Exception
    {
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
