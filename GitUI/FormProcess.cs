using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    delegate void DataCallback(string text);
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
        public Process Process { get; set; }
        public HandleOnExit HandleOnExitCallback { get; set; }

        private GitCommandsInstance gitCommand;

        protected FormProcess()
            : base(true)
        { }

        protected FormProcess(string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
            : base(useDialogSettings)
        {
            ProcessCallback = processStart;
            AbortCallback = processAbort;
            ProcessString = process ?? AppSettings.GitCommand;
            ProcessArguments = arguments;
            Remote = "";
            ProcessInput = input;
            WorkingDirectory = aWorkingDirectory;
            Text = Text + " (" + WorkingDirectory + ")";
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
            return ShowDialog(owner, (string)null, arguments);
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

        public static FormProcess ShowModeless(IWin32Window owner, string process, string arguments, string aWorkingDirectory, string input, bool useDialogSettings)
        {
            FormProcess formProcess = new FormProcess(process, arguments, aWorkingDirectory, input, useDialogSettings);

            formProcess.ControlBox = true;
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
            gitCommand = new GitCommandsInstance(WorkingDirectory);

            try
            {
                Process = gitCommand.CmdStartProcess(ProcessString, ProcessArguments);

                gitCommand.Exited += gitCommand_Exited;
                gitCommand.DataReceived += gitCommand_DataReceived;
                if (!string.IsNullOrEmpty(ProcessInput))
                {
                    Thread.Sleep(500);
                    Process.StandardInput.Write(ProcessInput);
                    AddMessageLine(string.Format(":: Wrote [{0}] to process!\r\n", ProcessInput));
                }
            }
            catch (Exception e)
            {
                AddMessageLine("\n" + e.ToStringWithData());
                gitCommand.ExitCode = 1;
                gitCommand_Exited(null, null);
            }
        }

        private void processAbort(FormStatus form)
        {
            if (Process != null)
            {
                Process.TerminateTree();
            }
        }

        protected void KillGitCommand()
        {
            try
            {
                gitCommand.Kill();
            }
            catch
            {
            }
        }

        void gitCommand_Exited(object sender, EventArgs e)
        {
            // This has to happen on the UI thread
            var method = new SendOrPostCallback(OnExit);

            syncContext.Send(method, this);
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

        private void OnExit(object state)
        {
            bool isError;

            try
            {
                isError = gitCommand != null && gitCommand.ExitCode != 0;
                if (HandleOnExit(ref isError))
                    return;
            }
            catch
            {
                isError = true;
            }

            Done(!isError);
        }

        protected virtual void DataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        void gitCommand_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (e.Data.Contains("%") || e.Data.StartsWith("remote: Counting objects"))
            {
                SetProgress(e.Data);
            }
            else
            {
                AppendOutputLine(e.Data);
            }

            DataReceived(sender, e);
        }

        public void AppendOutputLine(string rawLine)
        {
            const string ansiSuffix = "\u001B[K";

            var line = rawLine.Replace(ansiSuffix, "");

            AppendToOutputString(line + Environment.NewLine);

            AddMessageLine(line);
        }

        public static bool IsOperationAborted(string dialogResult)
        {
            return dialogResult.Trim('\r', '\n') == "Aborted";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(565, 326);
            this.Name = "FormProcess";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
