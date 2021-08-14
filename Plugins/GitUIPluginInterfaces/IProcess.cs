using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GitUIPluginInterfaces
{
    /// <summary>
    /// Defines a process instance.
    /// </summary>
    /// <remarks>
    /// This process will either be running or exited.
    /// </remarks>
    public interface IProcess : IDisposable
    {
        /// <summary>
        /// Gets an object that facilitates writing to the process's standard input stream.
        /// </summary>
        /// <remarks>
        /// To access the underlying <see cref="Stream"/>, dereference <see cref="StreamWriter.BaseStream"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">This process's input was not redirected
        /// when calling <see cref="IExecutable.Start"/>.</exception>
        StreamWriter StandardInput { get; }

        /// <summary>
        /// Gets an object that facilitates writing to the process's standard output stream.
        /// </summary>
        /// <remarks>
        /// To access the underlying <see cref="Stream"/>, dereference <see cref="StreamWriter.BaseStream"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">This process's output was not redirected
        /// when calling <see cref="IExecutable.Start"/>.</exception>
        StreamReader StandardOutput { get; }

        /// <summary>
        /// Gets an object that facilitates writing to the process's standard error stream.
        /// </summary>
        /// <remarks>
        /// To access the underlying <see cref="Stream"/>, dereference <see cref="StreamWriter.BaseStream"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">This process's output was not redirected
        /// when calling <see cref="IExecutable.Start"/>.</exception>
        StreamReader StandardError { get; }

        /// <summary>
        /// Blocks the calling thread until the process exits, or when this object is disposed.
        /// </summary>
        /// <returns>The process's exit code, or <c>null</c> if this object was disposed before the process exited.</returns>
        int WaitForExit();

        /// <summary>
        /// Returns a task that completes when the process exits, or when this object is disposed.
        /// </summary>
        /// <returns>A task that yields the process's exit code, or <c>null</c> if this object was disposed before the process exited.</returns>
        Task<int> WaitForExitAsync();

        /// <summary>
        /// Instructs the process component to wait for the associated process to exit, or for the cancellationToken to be cancelled (cancells the process).
        /// Note that the exit task must be awaited too.
        /// </summary>
        /// <param name="token">An optional token to cancel the asynchronous operation.</param>
        /// <returns>A task that will complete when the process has exited, cancellation has been requested, or an error occurs.</returns>
        Task WaitForProcessExitAsync(CancellationToken token);

        /// <summary>
        /// Waits for the process to reach an idle state.
        /// </summary>
        /// <see cref="Process.WaitForInputIdle()"/>
        void WaitForInputIdle();
    }
}
