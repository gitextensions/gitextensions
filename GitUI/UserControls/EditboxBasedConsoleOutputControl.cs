using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Logging;
using JetBrains.Annotations;

namespace GitUI.UserControls
{
    /// <summary>
    /// Uses an edit box and process output streams redirection.
    /// </summary>
    public sealed class EditboxBasedConsoleOutputControl : ConsoleOutputControl
    {
        private readonly RichTextBox _editbox;

        private int _exitcode;

        private Process _process;

        private ProcessOutputTimer _timer;

        public EditboxBasedConsoleOutputControl()
        {
            _timer = new ProcessOutputTimer(AppendMessage);
            _editbox = new RichTextBox { BackColor = SystemColors.Window, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Name = "_editbox", ReadOnly = true };
            Controls.Add(_editbox);
            _timer.Start();
        }

        public override int ExitCode => _exitcode;

        public override bool IsDisplayingFullProcessOutput => false;

        public override void AppendMessageFreeThreaded(string text)
        {
            _timer?.Append(text);
        }

        public override void KillProcess()
        {
            if (InvokeRequired)
            {
                throw new InvalidOperationException("This operation is to be executed on the home thread.");
            }

            if (_process == null)
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
            _timer.Clear();
            _editbox.Text = "";
            _editbox.Visible = false;
        }

        public override void StartProcess(string command, string arguments, string workdir, Dictionary<string, string> envVariables)
        {
            try
            {
                EnvironmentConfiguration.SetEnvironmentVariables();

                bool ssh = GitCommandHelpers.UseSsh(arguments);

                KillProcess();

                // process used to execute external commands
                var process = new Process();
                ProcessStartInfo startInfo = GitCommandHelpers.CreateProcessStartInfo(command, arguments, workdir, GitModule.SystemEncoding);
                startInfo.CreateNoWindow = !ssh && !AppSettings.ShowGitCommandLine;

                foreach (var (name, value) in envVariables)
                {
                    startInfo.EnvironmentVariables.Add(name, value);
                }

                process.StartInfo = startInfo;

                process.EnableRaisingEvents = true;
                process.OutputDataReceived += (sender, args) => FireDataReceived(new TextEventArgs((args.Data ?? "") + '\n'));
                process.ErrorDataReceived += (sender, args) => FireDataReceived(new TextEventArgs((args.Data ?? "") + '\n'));
                process.Exited += delegate
                {
                    this.InvokeAsync(() =>
                    {
                        if (_process == null)
                        {
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
                        catch
                        {
                            // NOP
                        }

                        _exitcode = _process.ExitCode;
                        _process = null;
                        _timer.Stop(true);
                        FireProcessExited();
                    }).FileAndForget();
                };

                var operation = CommandLog.LogProcessStart(command, arguments);
                process.Exited += (s, e) => operation.LogProcessEnd();
                process.Start();
                operation.SetProcessId(process.Id);
                _process = process;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                ex.Data.Add("command", command);
                ex.Data.Add("arguments", arguments);
                throw;
            }
        }

        private void AppendMessage([NotNull] string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (InvokeRequired)
            {
                throw new InvalidOperationException("This operation must be called on the GUI thread.");
            }

            // if not disposed
            if (!IsDisposed)
            {
                _editbox.Visible = true;
                _editbox.Text += text;
                _editbox.SelectionStart = _editbox.Text.Length;
                _editbox.ScrollToCaret();
            }
        }

        protected override void Dispose(bool disposing)
        {
            KillProcess();
            if (disposing && _timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            base.Dispose(disposing);
        }
    }
}