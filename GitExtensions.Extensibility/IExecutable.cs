using System.Diagnostics.Contracts;
using System.Text;

namespace GitExtensions.Extensibility;

/// <summary>
/// Defines an executable that can be launched to create processes.
/// </summary>
public interface IExecutable
{
    public string WorkingDir { get; }
    public string Command { get; }

    /// <summary>
    /// Starts a process of this executable.
    /// </summary>
    /// <remarks>
    /// This is a low level means of starting a process. Most code will want to use one of the extension methods
    /// provided by <c>ExecutableExtensions</c>.
    /// </remarks>
    /// <param name="arguments">Any command line arguments to be passed to the executable when it is started.</param>
    /// <param name="createWindow">Whether to create a window for the process or not.</param>
    /// <param name="redirectInput">Whether the standard input stream of the process will be written to.</param>
    /// <param name="redirectOutput">Whether the standard output stream of the process will be read from.</param>
    /// <param name="outputEncoding">The <see cref="Encoding"/> to use when interpreting standard output and standard
    /// error, or <c>null</c> if <paramref name="redirectOutput"/> is <c>false</c>.</param>
    /// <param name="useShellExecute">The value for the flag <c>ProcessStartInfo.UseShellExecute</c>.</param>
    /// <param name="throwOnErrorExit">A flag configuring whether to throw an exception if the exit code is not 0.</param>
    /// <returns>The started process.</returns>
    [Pure]
    IProcess Start(ArgumentString arguments = default,
                   bool createWindow = false,
                   bool redirectInput = false,
                   bool redirectOutput = false,
                   Encoding? outputEncoding = null,
                   bool useShellExecute = false,
                   bool throwOnErrorExit = true,
                   CancellationToken cancellationToken = default);
}
