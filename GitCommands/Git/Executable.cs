using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using GitCommands.Logging;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <inheritdoc />
    public sealed class Executable : IExecutable
    {
        private readonly string _workingDir;
        private readonly Func<string> _fileNameProvider;

        public Executable([NotNull] string fileName, [NotNull] string workingDir = "")
            : this(() => fileName, workingDir)
        {
        }

        public Executable([NotNull] Func<string> fileNameProvider, [NotNull] string workingDir = "")
        {
            _workingDir = workingDir;
            _fileNameProvider = fileNameProvider;
        }

        /// <inheritdoc />
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public IProcess Start(ArgumentString arguments = default, bool createWindow = false, bool redirectInput = false, bool redirectOutput = false, Encoding outputEncoding = null)
        {
            // TODO should we set these on the child process only?
            EnvironmentConfiguration.SetEnvironmentVariables();

            var args = (arguments.Arguments ?? "").Replace("$QUOTE$", "\\\"");

            var fileName = _fileNameProvider();

            return new ProcessWrapper(fileName, args, _workingDir, createWindow, redirectInput, redirectOutput, outputEncoding);
        }

        public string GetOutput(ArgumentString arguments)
        {
            return this.GetOutput(arguments, null);
        }

        #region ProcessWrapper

        /// <summary>
        /// Manages the lifetime of a process. The <see cref="System.Diagnostics.Process"/> object has many members
        /// that throw at different times in the lifecycle of the process, such as after it is disposed. This class
        /// provides a simplified API that meets the need of this application via the <see cref="IProcess"/> interface.
        /// </summary>
        private sealed class ProcessWrapper : IProcess
        {
            // TODO should this use TaskCreationOptions.RunContinuationsAsynchronously
            private readonly TaskCompletionSource<int> _exitTaskCompletionSource = new TaskCompletionSource<int>();

            private readonly object _syncRoot = new object();
            private readonly Process _process;
            private readonly ProcessOperation _logOperation;
            private readonly bool _redirectInput;
            private readonly bool _redirectOutput;

            private bool _disposed;

            [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
            public ProcessWrapper(string fileName, string arguments, string workDir, bool createWindow, bool redirectInput, bool redirectOutput, [CanBeNull] Encoding outputEncoding)
            {
                Debug.Assert(redirectOutput == (outputEncoding != null), "redirectOutput == (outputEncoding != null)");
                _redirectInput = redirectInput;
                _redirectOutput = redirectOutput;

                _process = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo =
                    {
                        UseShellExecute = false,
                        ErrorDialog = false,
                        CreateNoWindow = !createWindow,
                        RedirectStandardInput = redirectInput,
                        RedirectStandardOutput = redirectOutput,
                        RedirectStandardError = redirectOutput,
                        StandardOutputEncoding = outputEncoding,
                        StandardErrorEncoding = outputEncoding,
                        FileName = fileName,
                        Arguments = arguments,
                        WorkingDirectory = workDir
                    }
                };

                _logOperation = CommandLog.LogProcessStart(fileName, arguments, workDir);

                _process.Exited += OnProcessExit;

                _process.Start();

                _logOperation.SetProcessId(_process.Id);
            }

            private void OnProcessExit(object sender, EventArgs eventArgs)
            {
                lock (_syncRoot)
                {
                    // The Exited event can be raised after the process is disposed, however
                    // if the Process is disposed then reading ExitCode will throw.
                    if (!_disposed)
                    {
                        var exitCode = _process.ExitCode;
                        _logOperation.LogProcessEnd(exitCode);
                        _exitTaskCompletionSource.TrySetResult(exitCode);
                    }
                }
            }

            /// <inheritdoc />
            public StreamWriter StandardInput
            {
                get
                {
                    if (!_redirectInput)
                    {
                        throw new InvalidOperationException("Process was not created with redirected input.");
                    }

                    return _process.StandardInput;
                }
            }

            /// <inheritdoc />
            public StreamReader StandardOutput
            {
                get
                {
                    if (!_redirectOutput)
                    {
                        throw new InvalidOperationException("Process was not created with redirected output.");
                    }

                    return _process.StandardOutput;
                }
            }

            /// <inheritdoc />
            public StreamReader StandardError
            {
                get
                {
                    if (!_redirectOutput)
                    {
                        throw new InvalidOperationException("Process was not created with redirected output.");
                    }

                    return _process.StandardError;
                }
            }

            /// <inheritdoc />
            public void WaitForInputIdle() => _process.WaitForInputIdle();

            /// <inheritdoc />
            public Task<int> WaitForExitAsync() => _exitTaskCompletionSource.Task;

            /// <inheritdoc />
            public int WaitForExit()
            {
                return ThreadHelper.JoinableTaskFactory.Run(() => WaitForExitAsync());
            }

            /// <inheritdoc />
            public void Dispose()
            {
                lock (_syncRoot)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _disposed = true;
                }

                _process.Exited -= OnProcessExit;

                _exitTaskCompletionSource.TrySetCanceled();

                _process.Dispose();

                _logOperation.NotifyDisposed();
            }
        }

        #endregion
    }
}