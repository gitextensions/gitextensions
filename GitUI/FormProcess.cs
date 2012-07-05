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
            : this(true)
        { }

        //constructor for VS designer
        protected FormProcess(bool useDialogSettings)
            : base(useDialogSettings)
        { }

        public FormProcess(string process, string arguments)
            : this(process, arguments, null, null, true)
        { }

        public FormProcess(string process, string arguments, GitModule module, bool useDialogSettings)
            : this(process, arguments, module, null, true)
        { }

        public FormProcess(string process, string arguments, GitModule module, string input, bool useDialogSettings)
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

        public FormProcess(string arguments)
            : this(arguments, true)
        { }

        public FormProcess(string arguments, bool useDialogSettings)
            : this(null, arguments, null, null, useDialogSettings)
        { }

        public FormProcess(GitModule module, string arguments)
            : this(module, arguments, true)
        { }

        public FormProcess(GitModule module, string arguments, bool useDialogSettings)
            : this(null, arguments, module, useDialogSettings)
        { }

        //Input does not work for password inputs. I don't know why, but it turned out not to be really necessary.
        //For other inputs, it is not tested.
        public FormProcess(string process, string arguments, string input)
            : this(process, arguments, input, true)
        { }

        public FormProcess(string process, string arguments, string input, bool useDialogSettings)
            : this(process, arguments, null, input, useDialogSettings)
        { }

        public static void ShowDialog(IWin32Window owner, string arguments)
        {
            using (var formProcess = new FormProcess(arguments))
            {
                formProcess.ShowDialog(owner);
            }
        }

        public static void ShowDialog(IWin32Window owner, string arguments, bool useDialogSettings)
        {
            using (var formProcess = new FormProcess(arguments, useDialogSettings))
            {
                formProcess.ShowDialog(owner);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.ClientSize = new System.Drawing.Size(565, 326);
            this.Name = "FormProcess";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
