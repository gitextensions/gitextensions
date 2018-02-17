﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls;

namespace GitUI
{
    internal delegate void DataCallback(string text);
    /// <summary>
    ///
    /// </summary>
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

        protected FormProcess()
            : base(true)
        { }

        public FormProcess(ConsoleOutputControl outputControl, string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
            : base(outputControl, useDialogSettings)
        {
            ProcessCallback = processStart;
            AbortCallback = processAbort;
            ProcessString = process ?? AppSettings.GitCommand;
            ProcessArguments = arguments;
            Remote = "";
            ProcessInput = input;
            WorkingDirectory = aWorkingDirectory;
            Text = Text + " (" + WorkingDirectory + ")";

            ConsoleOutput.ProcessExited += delegate { OnExit(ConsoleOutput.ExitCode); };
            ConsoleOutput.DataReceived += DataReceivedCore;
        }

        public FormProcess(string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
            : this(null, process, arguments, aWorkingDirectory, input, useDialogSettings)
        {
        }

        public static bool ShowDialog(IWin32Window owner, GitModule module, string arguments)
        {
            return ShowDialog(owner, null, arguments, module.WorkingDir, null, true);
        }

        public static bool ShowDialog(IWin32Window owner, GitModule module, string process, string arguments)
        {
            return ShowDialog(owner, process, arguments, module.WorkingDir, null, true);
        }

        public static bool ShowDialog(GitModuleForm owner, string arguments)
        {
            return ShowDialog(owner, null, arguments);
        }

        public static bool ShowDialog(GitModuleForm owner, string process, string arguments)
        {
            return ShowDialog(owner, process, arguments, owner.Module.WorkingDir, null, true);
        }

        public static bool ShowDialog(GitModuleForm owner, string arguments, bool useDialogSettings)
        {
            return ShowDialog(owner, owner.Module, arguments, useDialogSettings);
        }

        public static bool ShowDialog(IWin32Window owner, GitModule module, string arguments, bool useDialogSettings)
        {
            return ShowDialog(owner, null, arguments, module.WorkingDir, null, useDialogSettings);
        }


        public static bool ShowDialog(IWin32Window owner, string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
        {
            using (var formProcess = new FormProcess(process, arguments, aWorkingDirectory, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return !formProcess.ErrorOccurred();
            }
        }

        public static bool ShowStandardProcessDialog(IWin32Window owner, string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
        {
            var outputCtrl = new EditboxBasedConsoleOutputControl();
            using (var formProcess = new FormProcess(outputCtrl, process, arguments, aWorkingDirectory, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return !formProcess.ErrorOccurred();
            }
        }

        public static FormProcess ShowModeless(IWin32Window owner, string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
        {
            var formProcess = new FormProcess(process, arguments, aWorkingDirectory, input, useDialogSettings) {ControlBox = true};

            formProcess.Show(owner);

            return formProcess;
        }

        public static FormProcess ShowModeless(GitModuleForm owner, string arguments)
        {
            return ShowModeless(owner, null, arguments, owner.Module.WorkingDir, null, true);
        }

        public static string ReadDialog(GitModuleForm owner, string arguments)
        {
            return ReadDialog(owner, null, arguments, owner.Module, null, true);
        }

        public static string ReadDialog(IWin32Window owner, string process, string arguments, GitModule module, string input, bool useDialogSettings)
        {
            using (var formProcess = new FormProcess(process, arguments, module.WorkingDir, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return formProcess.GetOutputString();
            }
        }

        protected virtual void BeforeProcessStart()
        {

        }

        private void processStart(FormStatus form)
        {
            BeforeProcessStart();
            string QuotedProcessString = ProcessString;
            if (QuotedProcessString.IndexOf(' ') != -1)
                QuotedProcessString = QuotedProcessString.Quote();
            AddMessageLine(QuotedProcessString + " " + ProcessArguments);

            try
            {
                ConsoleOutput.StartProcess(ProcessString, ProcessArguments, WorkingDirectory, ProcessEnvVariables);

                if (!string.IsNullOrEmpty(ProcessInput))
                {
                    throw new NotSupportedException("No non-NULL usages of ProcessInput are currently expected.");    // Not implemented with all terminal variations, so let's postpone until there's at least one non-null case
/*
                    Thread.Sleep(500);
                    Process.StandardInput.Write(ProcessInput);
                    AddMessageLine(string.Format(":: Wrote [{0}] to process!\r\n", ProcessInput));
*/
                }
            }
            catch (Exception e)
            {
                AddMessageLine("\n" + e.ToStringWithData());
                OnExit(1);
            }
        }

        private void processAbort(FormStatus form)
        {
            KillProcess();
        }

        protected void KillProcess()
        {
            try
            {
                ConsoleOutput.KillProcess();

                var module = new GitModule(WorkingDirectory);
                module.UnlockIndex(true);
            }
            catch
            {
                // no-op
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="isError">if command finished with error</param>
        /// <returns>if handled</returns>
        protected virtual bool HandleOnExit(ref bool isError)
        {
            return HandleOnExitCallback != null && HandleOnExitCallback(ref isError, this);
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
                        return;
                }
                catch
                {
                    isError = true;
                }

                Done(!isError);
            });
        }

        protected virtual void DataReceived(object sender, TextEventArgs e)
        {

        }

        private void DataReceivedCore(object sender, TextEventArgs e)
        {
            if(e.Text.Contains("%") || e.Text.Contains("remote: Counting objects"))
                SetProgress(e.Text);
            else
            {
                const string ansiSuffix = "\u001B[K";
                string line = e.Text.Replace(ansiSuffix, "");

                if(ConsoleOutput.IsDisplayingFullProcessOutput)
                    OutputLog.Append(line); // To the log only, display control displays it by itself
                else
                    AppendOutput(line); // Both to log and display control
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
            AddMessage(line);
        }

        public static bool IsOperationAborted(string dialogResult)
        {
            return dialogResult.Trim('\r', '\n') == "Aborted";
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // FormProcess
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(565, 326);
            Name = "FormProcess";
            ResumeLayout(false);
            PerformLayout();

        }
    }
}
