using System.Text;

namespace GitExtensions.Extensibility.Git;

public interface IGitCommandRunner
{
    /// <summary>
    /// Starts git with the given arguments in the background cancellably.
    /// The process exit is awaited on dispose of the IProcess instance.
    /// </summary>
    IProcess RunDetached(
        CancellationToken cancellationToken,
        ArgumentString arguments = default,
        bool createWindow = false,
        bool redirectInput = false,
        bool redirectOutput = false,
        Encoding? outputEncoding = null);

    /// <summary>
    /// Starts git with the given arguments in the background.
    /// The process exit or exceptions are awaited in the background.
    /// </summary>
    void RunDetached(
        ArgumentString arguments = default,
        bool createWindow = false,
        bool redirectInput = false,
        bool redirectOutput = false,
        Encoding? outputEncoding = null);
}
