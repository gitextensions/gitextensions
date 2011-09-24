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
        public Process Process { get; set; }
        public HandleOnExit HandleOnExitCallback { get; set; }

        private GitCommandsInstance gitCommand;

        //constructor for VS designer
        protected FormProcess()
            : base()
        {
        }


        //Input does not work for password inputs. I don't know why, but it turned out not to be really necessary.
        //For other inputs, it is not tested.
        public FormProcess(string process, string arguments, string input, string ASettingsName)
            : this(process, arguments, ASettingsName)
        {
            ProcessInput = input;
        }

        public FormProcess(string process, string arguments, string ASettingsName)
            : base(ASettingsName)
        {
            ProcessCallback = new ProcessStart(processStart);
            AbortCallback = new ProcessAbort(processAbort);
            ProcessString = process ?? Settings.GitCommand;
            ProcessArguments = arguments;
            Remote = "";
            ProcessInput = null;
        }

        public FormProcess(string arguments, string ASettingsName)
            : this(null, arguments, ASettingsName)
        {
        }

        protected virtual void BeforeProcessStart()
        {
            
        }

        private void processStart(FormStatus form)
        {
            BeforeProcessStart();
            AddOutput(ProcessString + " " + ProcessArguments);
            gitCommand = new GitCommandsInstance { CollectOutput = false };

            try
            {
                Process = gitCommand.CmdStartProcess(ProcessString, ProcessArguments);

                gitCommand.Exited += gitCommand_Exited;
                gitCommand.DataReceived += gitCommand_DataReceived;
                if (!string.IsNullOrEmpty(ProcessInput))
                {
                    Thread.Sleep(500);
                    Process.StandardInput.Write(ProcessInput);
                    AddOutput(string.Format(":: Wrote [{0}] to process!\r\n", ProcessInput));
                }
            }
            catch (Exception e)
            {
                AddOutput(e.Message);
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
            if (HandleOnExitCallback != null)
                return HandleOnExitCallback(ref isError, this);
            else
                return false;
        
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
                //if (Output.InvokeRequired)
                //{
                //    // It's on a different thread, so use Invoke.
                //    DataCallback d = new DataCallback(AddOutput);
                //    this.Invoke(d, new object[] { e.Data });
                //} else
                //{
                //    AddOutput(e.Data);
                //}
                OutputString.AppendLine(e.Data);

                AddToTimer(e.Data);
                AddToTimer(Environment.NewLine);
            }

            DataReceived(sender, e);
        }

        public static bool IsOperationAborted(string dialogResult)
        {
            return dialogResult.Trim('\r', '\n') == "Aborted";
        }
    }
}
