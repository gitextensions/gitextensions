using System.Text;
using GitCommands;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    /// <summary>
    /// Defines an executable that can be launched to create processes.
    /// </summary>
    public interface IExecutable
    {
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
        /// <returns>The started process.</returns>
        [NotNull]
        [MustUseReturnValue]
        IProcess Start(ArgumentString arguments = default, bool createWindow = false, bool redirectInput = false, bool redirectOutput = false, [CanBeNull] Encoding outputEncoding = null);

        /// <summary>
        /// Launches a process for the executable and returns its output.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the executable</param>
        /// <returns>The concatenation of standard output and standard error.</returns>
        [NotNull]
        [MustUseReturnValue]
        string GetOutput(ArgumentString arguments);
    }
}