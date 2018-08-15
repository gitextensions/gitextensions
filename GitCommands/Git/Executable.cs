using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Logging;
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

        #region ProcessWrapper

        /// <summary>
        /// Manages the lifetime of a process. The <see cref="System.Diagnostics.Process"/> object has many members
        /// that throw at different times in the lifecycle of the process, such as after it is disposed. This class
        /// provides a simplified API that meets the need of this application via the <see cref="IProcess"/> interface.
        /// </summary>
        private sealed class ProcessWrapper : IProcess
        {
            // TODO should this use TaskCreationOptions.RunContinuationsAsynchronously
            private readonly TaskCompletionSource<int?> _exitTaskCompletionSource = new TaskCompletionSource<int?>();

            private readonly Process _process;
            private readonly ProcessOperation _logOperation;
            private readonly bool _redirectInput;
            private readonly bool _redirectOutput;

            private int _disposed;

            /// <inheritdoc />
            public int? ExitCode { get; private set; }

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

                _logOperation = CommandLog.LogProcessStart(fileName, arguments);

                _process.Exited += OnProcessExit;

                _process.Start();

                _logOperation.SetProcessId(_process.Id);
            }

            private void OnProcessExit(object sender, EventArgs eventArgs)
            {
                // The Exited event can be raised after the process is disposed, however
                // if the Process is disposed then reading ExitCode will throw.
                if (_disposed == 0)
                {
                    ExitCode = _process.ExitCode;
                }

                _logOperation.LogProcessEnd(ExitCode);
                _exitTaskCompletionSource.TrySetResult(ExitCode);
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
            public Task<int?> WaitForExitAsync() => _exitTaskCompletionSource.Task;

            /// <inheritdoc />
            public int? WaitForExit()
            {
                if (_disposed != 0)
                {
                    return ExitCode;
                }

                _process.WaitForExit();

                return GitUI.ThreadHelper.JoinableTaskFactory.Run(() => _exitTaskCompletionSource.Task);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                {
                    return;
                }

                _process.Exited -= OnProcessExit;

                _exitTaskCompletionSource.TrySetResult(null);

                _process.Dispose();

                _logOperation.NotifyDisposed();
            }
        }

        #endregion
    }
}