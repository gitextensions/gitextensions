using System.Diagnostics;
using System.Text;
using GitCommands;
using GitCommands.Git.Extensions;
using GitCommands.Logging;
using GitUI.Infrastructure;
using Timer = System.Windows.Forms.Timer;

namespace GitUI.UserControls
{
    /// <summary>
    /// Uses an edit box and process output streams redirection.
    /// </summary>
    public sealed class EditboxBasedConsoleOutputControl : ConsoleOutputControl
    {
        private readonly RichTextBox _editbox;

        private int _exitcode;

        private Process? _process;

        private ProcessOutputThrottle? _outputThrottle;

        public EditboxBasedConsoleOutputControl()
        {
            _editbox = new RichTextBox
            {
                BackColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            Controls.Add(_editbox);

            _outputThrottle = new ProcessOutputThrottle(AppendMessage);

            void AppendMessage(string text)
            {
                Debug.Assert(text is not null, "text is not null");
                if (IsDisposed)
                {
                    return;
                }

                Debug.Assert(!InvokeRequired, "!InvokeRequired");

                _editbox.Visible = true;
                _editbox.Text += text;
                _editbox.SelectionStart = _editbox.Text.Length;
                _editbox.ScrollToCaret();
            }
        }

        public override int ExitCode => _exitcode;

        public override bool IsDisplayingFullProcessOutput => false;

        public override void AppendMessageFreeThreaded(string text)
        {
            _outputThrottle?.Append(text);
        }

        public override void KillProcess()
        {
            if (InvokeRequired)
            {
                throw new InvalidOperationException("This operation is to be executed on the home thread.");
            }

            if (_process is null)
            {
                return;
            }

            try
            {
                _process.TerminateTree();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            _process = null;
            FireProcessExited();
        }

        public override void Reset()
        {
            _outputThrottle?.Clear();
            _editbox.Text = "";
            _editbox.Visible = false;
        }

        public override void StartProcess(string command, string arguments, string workDir, Dictionary<string, string> envVariables)
        {
            ProcessOperation operation = CommandLog.LogProcessStart(command, arguments, workDir);

            try
            {
                EnvironmentConfiguration.SetEnvironmentVariables();

                bool ssh = UseSsh(arguments);

                KillProcess();

                // process used to execute external commands
                var outputEncoding = GitModule.SystemEncoding;
                ProcessStartInfo startInfo = new()
                {
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = !ssh && !AppSettings.ShowGitCommandLine,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = outputEncoding,
                    StandardErrorEncoding = outputEncoding,
                    FileName = command,
                    Arguments = arguments,
                    WorkingDirectory = workDir
                };

                foreach (var (name, value) in envVariables)
                {
                    startInfo.EnvironmentVariables.Add(name, value);
                }

                Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (sender, args) => FireDataReceived(new TextEventArgs((args.Data ?? "") + '\n'));
                process.ErrorDataReceived += (sender, args) => FireDataReceived(new TextEventArgs((args.Data ?? "") + '\n'));
                process.Exited += delegate
                {
                    this.InvokeAsync(
                        () =>
                        {
                            if (_process is null)
                            {
                                operation.LogProcessEnd(new Exception("Process instance is null in Exited event"));
                                return;
                            }

                            // The process is exited already, but this command waits also until all output is received.
                            // Only WaitForExit when someone is connected to the exited event. For some reason a
                            // null reference is thrown sometimes when staging/unstaging in the commit dialog when
                            // we wait for exit, probably a timing issue...
                            try
                            {
                                _process.WaitForExit();
                            }
                            catch (Exception ex)
                            {
                                operation.LogProcessEnd(ex);
                            }

                            _exitcode = _process.ExitCode;
                            operation.LogProcessEnd(_exitcode);
                            _process = null;
                            _outputThrottle?.FlushOutput();
                            FireProcessExited();
                            _outputThrottle?.Stop(flush: true);
                        }).FileAndForget();
                };

                process.Start();
                operation.SetProcessId(process.Id);
                _process = process;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                operation.LogProcessEnd(ex);
                ex.Data.Add("command", command);
                ex.Data.Add("arguments", arguments);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            KillProcess();
            if (disposing && _outputThrottle is not null)
            {
                _outputThrottle.Dispose();
                _outputThrottle = null;
            }

            base.Dispose(disposing);
        }

        private static bool UseSsh(string arguments)
        {
            return arguments.Contains("plink")
                || (!GitSshHelpers.IsPlink && DoArgumentsRequireSsh());

            bool DoArgumentsRequireSsh()
            {
                return (arguments.Contains('@') && arguments.Contains("://")) ||
                       (arguments.Contains('@') && arguments.Contains(':')) ||
                       arguments.Contains("ssh://") ||
                       arguments.Contains("http://") ||
                       arguments.Contains("git://") ||
                       arguments.Contains("push") ||
                       arguments.Contains("remote") ||
                       arguments.Contains("fetch") ||
                       arguments.Contains("pull");
            }
        }

        #region ProcessOutputThrottle

        private sealed class ProcessOutputThrottle : IDisposable
        {
            private readonly StringBuilder _textToAdd = new();
            private readonly Timer _timer;
            private readonly Action<string> _doOutput;

            /// <param name="doOutput">Will be called on the UI thread.</param>
            public ProcessOutputThrottle(Action<string> doOutput)
            {
                _doOutput = doOutput;

                _timer = new Timer { Interval = 600, Enabled = true };
                _timer.Tick += delegate { FlushOutput(); };
            }

            public void Stop(bool flush)
            {
                _timer.Stop();

                if (flush)
                {
                    FlushOutput();
                }
            }

            /// <remarks>Can be called on any thread.</remarks>
            public void Append(string text)
            {
                lock (_textToAdd)
                {
                    _textToAdd.Append(text);
                }
            }

            public void FlushOutput()
            {
                lock (_textToAdd)
                {
                    if (_textToAdd.Length > 0)
                    {
                        _doOutput?.Invoke(_textToAdd.ToString());
                    }

                    _textToAdd.Clear();
                }
            }

            public void Clear()
            {
                lock (_textToAdd)
                {
                    _textToAdd.Clear();
                }
            }

            public void Dispose()
            {
                Stop(flush: false);

                // clear will lock, to prevent outputting to disposed object
                Clear();
                _timer.Dispose();
            }
        }

        #endregion
    }
}
