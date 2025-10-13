namespace GitCommands
{
    public sealed class CannotAccessFileException: Exception
    {
        public CannotAccessFileException(string message)
            : base(message)
        {
        }

        public CannotAccessFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
