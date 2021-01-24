using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls;
using JetBrains.Annotations;

namespace GitUI.HelperDialogs
{
    /// <param name="isError">if command finished with error</param>
    /// <param name="form">this form</param>
    /// <returns>if handled</returns>
    public delegate bool HandleOnExit(ref bool isError, FormProcess form);

    public class FormProcess : FormStatus
    {
        public string Remote { get; set; }
        public string ProcessString { get; set; }
        public string ProcessArguments { get; set; }
        public string ProcessInput { get; set; }
        public readonly string WorkingDirectory;
        public HandleOnExit HandleOnExitCallback { get; set; }
        public readonly Dictionary<string, string> ProcessEnvVariables = new Dictionary<string, string>();

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private protected FormProcess()
            : base()
        {
        }

        private FormProcess(GitUICommands commands, ConsoleOutputControl outputControl, [CanBeNull] string process, ArgumentString arguments, string workingDirectory, string input, bool useDialogSettings)
            : base(commands, outputControl, useDialogSettings)
        {
            ProcessCallback = ProcessStart;
            AbortCallback = ProcessAbort;
            ProcessString = process ?? AppSettings.GitCommand;
            ProcessArguments = arguments;
            Remote = "";
            ProcessInput = input;
            WorkingDirectory = workingDirectory;

            var displayPath = PathUtil.GetDisplayPath(WorkingDirectory);
            if (!string.IsNullOrWhiteSpace(displayPath))
            {
                Text += $" ({displayPath})";
            }

            ConsoleOutput.ProcessExited += delegate { OnExit(ConsoleOutput.ExitCode); };
            ConsoleOutput.DataReceived += DataReceivedCore;
        }

        public FormProcess(GitUICommands commands, string process, ArgumentString arguments, string workingDirectory, string input, bool useDialogSettings)
            : this(commands, outputControl: null, process, arguments, workingDirectory, input, useDialogSettings)
        {
        }

        public static bool ShowDialog([CanBeNull] IWin32Window owner, string process, ArgumentString arguments, string workingDirectory, string input, bool useDialogSettings)
        {
            Debug.Assert(owner is not null, "Progress window must be owned by another window! This is a bug, please correct and send a pull request with a fix.");

            using (var formProcess = new FormProcess(commands: null, process, arguments, workingDirectory, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return !formProcess.ErrorOccurred();
            }
        }

        public static string ReadDialog([CanBeNull] IWin32Window owner, string process, ArgumentString arguments, string workingDirectory, string input, bool useDialogSettings)
        {
            Debug.Assert(owner is not null, "Progress window must be owned by another window! This is a bug, please correct and send a pull request with a fix.");

            using (var formProcess = new FormProcess(commands: null, process, arguments, workingDirectory, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return formProcess.GetOutputString();
            }
        }

        protected virtual void BeforeProcessStart()
        {
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    {
                        Close();
                        return true;
                    }

                default:
                    {
                        return base.ProcessCmdKey(ref msg, keyData);
                    }
            }
        }

        private void ProcessStart(FormStatus form)
        {
            BeforeProcessStart();
            string quotedProcessString = ProcessString;
            if (quotedProcessString.IndexOf(' ') != -1)
            {
                quotedProcessString = quotedProcessString.Quote();
            }

            AppendMessage($"{quotedProcessString} {ProcessArguments}{Environment.NewLine}");

            try
            {
                ConsoleOutput.StartProcess(ProcessString, ProcessArguments, WorkingDirectory, ProcessEnvVariables);

                if (!string.IsNullOrEmpty(ProcessInput))
                {
                    throw new NotSupportedException("No non-NULL usages of ProcessInput are currently expected.");  // Not implemented with all terminal variations, so let's postpone until there's at least one non-null case
/*
                    Thread.Sleep(500);
                    Process.StandardInput.Write(ProcessInput);
                    AddMessageLine(string.Format(":: Wrote [{0}] to process!\r\n", ProcessInput));
*/
                }
            }
            catch (Exception e)
            {
                AppendMessage($"{Environment.NewLine}{e.ToStringWithData()}{Environment.NewLine}");
                OnExit(1);
            }
        }

        private void ProcessAbort(FormStatus form)
        {
            KillProcess();
        }

        protected void KillProcess()
        {
            try
            {
                ConsoleOutput.KillProcess();

                var module = new GitModule(WorkingDirectory);
                module.UnlockIndex(includeSubmodules: true);
            }
            catch
            {
                // no-op
            }
        }

        /// <param name="isError">if command finished with error</param>
        /// <returns>if handled</returns>
        protected virtual bool HandleOnExit(ref bool isError)
        {
            return HandleOnExitCallback is not null && HandleOnExitCallback(ref isError, this);
        }

        private void OnExit(int exitcode)
        {
            this.InvokeAsync(() =>
            {
                bool isError;
                try
                {
                    isError = exitcode != 0;

                    if (HandleOnExit(ref isError))
                    {
                        return;
                    }
                }
                catch
                {
                    isError = true;
                }

                Done(!isError);
            }).FileAndForget();
        }

        protected virtual void DataReceived(object sender, TextEventArgs e)
        {
        }

        private void DataReceivedCore(object sender, TextEventArgs e)
        {
            if (e.Text.Contains("%") || e.Text.Contains("remote: Counting objects"))
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => SetProgressAsync(e.Text)).FileAndForget();
            }
            else
            {
                const string ansiSuffix = "\u001B[K";
                string line = e.Text.Replace(ansiSuffix, "");

                if (ConsoleOutput.IsDisplayingFullProcessOutput)
                {
                    OutputLog.Append(line); // To the log only, display control displays it by itself
                }
                else
                {
                    AppendOutput(line); // Both to log and display control
                }
            }

            DataReceived(sender, e);
        }

        /// <summary>
        /// Appends a line of text (CRLF added automatically) both to the logged output (<see cref="FormStatus.GetOutputString"/>) and to the display console control.
        /// </summary>
        public void AppendOutput(string line)
        {
            // To the internal log (which can be then retrieved as full text from this form)
            OutputLog.Append(line);

            // To the display control
            AppendMessage(line);
        }

        public static bool IsOperationAborted(string dialogResult)
        {
            return dialogResult.Trim('\r', '\n') == "Aborted";
        }
    }
}
