using System.Diagnostics;
using System.Text;
using GitCommands;
using GitCommands.Git.Extensions;
using GitCommands.Logging;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using Microsoft;
using Timer = System.Windows.Forms.Timer;

namespace GitUI.UserControls;

/// <summary>
/// Uses an edit box and process output streams redirection.
/// </summary>
public sealed class EditboxBasedConsoleOutputControl : ConsoleOutputControl
{
    private readonly RichTextBox _editbox;

    private int _exitcode;

    private Process? _process;

    private Action? _logProcessKilled;

    private ProcessOutputThrottle? _outputThrottle;

    private StreamWriter? _input;

    public EditboxBasedConsoleOutputControl()
    {
        _editbox = new RichTextBox
        {
            BackColor = Application.IsDarkModeEnabled ? AppColor.EditorBackground.GetThemeColor() : SystemColors.Info,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Font = AppSettings.MonospaceFont,
            ReadOnly = true
        };
        _editbox.LinkClicked += editbox_LinkClicked;
        Controls.Add(_editbox);

        _outputThrottle = new ProcessOutputThrottle(AppendMessage);

        void AppendMessage(string text)
        {
            DebugHelpers.Assert(text is not null, "text is not null");
            if (IsDisposed)
            {
                return;
            }

            DebugHelpers.Assert(!InvokeRequired, "!InvokeRequired");

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

    public override void AppendInput(string text)
    {
        Validates.NotNull(_input);
        _input.Write(text);
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

        _logProcessKilled();

        try
        {
            _process.TerminateTree();
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }

        _process.Dispose();
        _process = null;
        _input?.Dispose();
        _input = null;
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

            KillProcess();

            _logProcessKilled = () => operation.LogProcessEnd(new Exception("Process killed"));

            // process used to execute external commands
            Encoding outputEncoding = GitModule.SystemEncoding;
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = !AppSettings.ShowGitCommandLine,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = outputEncoding,
                StandardErrorEncoding = outputEncoding,
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workDir
            };

            foreach ((string name, string value) in envVariables)
            {
                startInfo.EnvironmentVariables.Add(name, value);
            }

            _process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

            AsyncStreamReader? outputReader = null;
            AsyncStreamReader? errorReader = null;

            _process.Exited += delegate
            {
                ThreadHelper.FileAndForget(async () =>
                    {
                        if (_process is null)
                        {
                            await this.SwitchToMainThreadAsync();
                            operation.LogProcessEnd(new Exception("Process instance is null in Exited event"));
                            return;
                        }

                        // The process is exited already, but this command waits also until all output is received.
                        // Only WaitForExit when someone is connected to the exited event. For some reason a
                        // null reference is thrown sometimes when staging/unstaging in the commit dialog when
                        // we wait for exit, probably a timing issue...
                        try
                        {
                            // WaitForExit[Async] blocks here for unknown reason if the process has already exited
                            if (!_process.HasExited)
                            {
                                await _process.WaitForExitAsync();
                            }

                            _logProcessKilled = null;

                            if (_process is null)
                            {
                                // The process has been killed meanwhile.
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            await this.SwitchToMainThreadAsync();
                            operation.LogProcessEnd(ex);
                        }

                        _exitcode = _process.ExitCode;

                        using CancellationTokenSource eofTimeoutTokenSource = new(millisecondsDelay: 5000);

                        if (outputReader is not null)
                        {
                            await outputReader.WaitUntilEofAsync(eofTimeoutTokenSource.Token);
                            outputReader.Dispose();
                        }

                        if (errorReader is not null)
                        {
                            await errorReader.WaitUntilEofAsync(eofTimeoutTokenSource.Token);
                            errorReader.Dispose();
                        }

                        await this.SwitchToMainThreadAsync();
                        operation.LogProcessEnd(_exitcode);
                        _process.Dispose();
                        _process = null;
                        await _input.DisposeAsync();
                        _input = null;
                        _outputThrottle?.Stop(flush: true);
                        FireProcessExited();
                    });
            };

            _process.Start();
            operation.SetProcessId(_process.Id);
            _input = _process.StandardInput;
            outputReader = new AsyncStreamReader(_process.StandardOutput, ForwardOutput);
            errorReader = new AsyncStreamReader(_process.StandardError, ForwardOutput);
        }
        catch (Exception ex)
        {
            operation.LogProcessEnd(ex);
            ex.Data.Add("command", command);
            ex.Data.Add("arguments", arguments);
            throw;
        }

        return;

        void ForwardOutput(string output)
        {
            output = output.Replace("\r\n", "\n");

            for (int startIndex = 0; startIndex < output.Length;)
            {
                int nextLineEnd = output.IndexOfAny(Delimiters.LineFeedAndCarriageReturn, startIndex) + 1;
                if (nextLineEnd == 0)
                {
                    nextLineEnd = output.Length;
                }

                FireDataReceived(new TextEventArgs(output[startIndex..nextLineEnd]));
                startIndex = nextLineEnd;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        KillProcess();
        if (disposing)
        {
            _outputThrottle?.Dispose();
            _outputThrottle = null;
            _process?.Dispose();
            _process = null;
            _input?.Dispose();
            _input = null;
        }

        base.Dispose(disposing);
    }

    private void editbox_LinkClicked(object? sender, LinkClickedEventArgs e)
    {
        try
        {
            OsShellUtil.OpenUrlInDefaultBrowser(e.LinkText);
        }
        catch (Exception ex)
        {
            MessageBoxes.ShowError(this, ex.Message);
        }
    }

    #region ProcessOutputThrottle

    private sealed class ProcessOutputThrottle : IDisposable
    {
        private readonly Lock _textToAddLock = new();
        private readonly StringBuilder _textToAdd = new();
        private readonly Timer _timer;
        private readonly Action<string> _doOutput;

        /// <param name="doOutput">Will be called on the UI thread.</param>
        public ProcessOutputThrottle(Action<string> doOutput)
        {
            _doOutput = doOutput;

            _timer = new Timer { Interval = 1 };
            _timer.Tick += delegate { FlushOutput(); };
            _timer.Start();
        }

        public void Stop(bool flush)
        {
            if (flush)
            {
                FlushOutput();
            }

            _timer.Stop();
        }

        /// <remarks>Can be called on any thread.</remarks>
        public void Append(string text)
        {
            lock (_textToAddLock)
            {
                _textToAdd.Append(text);
            }
        }

        public void FlushOutput()
        {
            _timer.Stop();
            _timer.Interval = 100;
            _timer.Start();

            string textToAdd = "";
            lock (_textToAddLock)
            {
                if (_textToAdd.Length > 0)
                {
                    textToAdd = _textToAdd.ToString();
                    _textToAdd.Clear();
                }
            }

            if (textToAdd.Length > 0)
            {
                _doOutput?.Invoke(textToAdd);
            }
        }

        public void Clear()
        {
            lock (_textToAddLock)
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
