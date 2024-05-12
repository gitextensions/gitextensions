namespace GitExtensions.Extensibility.Settings;

/// <summary>
/// The exception that is thrown when a git setting value is converted in an incompatible format.
/// </summary>
public class GitConfigFormatException : Exception
{
    public GitConfigFormatException()
    {
    }

    public GitConfigFormatException(string? message) : base(message)
    {
    }

    public GitConfigFormatException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
