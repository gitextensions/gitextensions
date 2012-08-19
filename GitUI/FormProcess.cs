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
        public string WorkingDir { get; set; }
        public Process Process { get; set; }
        public HandleOnExit HandleOnExitCallback { get; set; }

        private GitCommandsInstance gitCommand;

        protected FormProcess()
            : base(true)
        { }

        protected FormProcess(string process, string arguments, GitModule module, string input, bool useDialogSettings)
            : base(useDialogSettings)
        {
            ProcessCallback = processStart;
            AbortCallback = processAbort;
            ProcessString = process ?? Settings.GitCommand;
            WorkingDir = module == null ? Settings.WorkingDir : module.WorkingDir;
            ProcessArguments = arguments;
            Remote = "";
            ProcessInput = input;
        }

        public static bool ShowDialog(IWin32Window owner, GitModule module, string arguments)
        {
            return ShowDialog(owner, null, arguments, module, null, true);
        }

        public static bool ShowDialog(IWin32Window owner, string arguments)
        {
            return ShowDialog(owner, arguments, true);
        }

        public static bool ShowDialog(IWin32Window owner, string process, string arguments)
        {
            return ShowDialog(owner, process, arguments, null, null, true);
        }

        public static bool ShowDialog(IWin32Window owner, string arguments, bool useDialogSettings)
        {
            return ShowDialog(owner, null, arguments, null, null, useDialogSettings);
        }

        public static bool ShowDialog(IWin32Window owner, string process, string arguments, GitModule module, string input, bool useDialogSettings)
        {
            using (var formProcess = new FormProcess(process, arguments, module, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return !formProcess.ErrorOccurred();
            }
        }

        public static string ReadDialog(IWin32Window owner, string arguments)
        {
            return ReadDialog(owner, null, arguments, null, null, true);
        }

        public static string ReadDialog(IWin32Window owner, string process, string arguments, GitModule module, string input, bool useDialogSettings)
        {
            using (var formProcess = new FormProcess(process, arguments, module, input, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
                return formProcess.OutputString.ToString();
            }
        }

        protected virtual void BeforeProcessStart()
        {

        }

        private void processStart(FormStatus form)
        {
            BeforeProcessStart();
            AddMessageLine(ProcessString + " " + ProcessArguments);
            gitCommand = new GitCommandsInstance { CollectOutput = false };

            try
            {
                Process = gitCommand.CmdStartProcess(ProcessString, ProcessArguments, WorkingDir);

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
                AddMessageLine(e.Message);
                gitCommand.ExitCode = 1;
                gitCommand_Exited(null, null);
            }
        }

        private void processAbort(FormStatus form)
        {
            if (Process != null)
            {
                Process.Kill();
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

        public void AppendOutputLine(string line)
        {
            OutputString.AppendLine(line);

            AddMessageLine(line);
        }

        public static bool IsOperationAborted(string dialogResult)
        {
            return dialogResult.Trim('\r', '\n') == "Aborted";
        }

        private void InitializeComponent()
        {
            //TODO: this was automatically created by visual studio
            this.SuspendLayout();
            // 
            // FormProcess
            // 
            
            this.ClientSize = new System.Drawing.Size(565, 326);
            this.Name = "FormProcess";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
