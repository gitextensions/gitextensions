using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace GitUI
{
    delegate void DataCallback(string text);
    public class FormProcess : FormStatus
    {
        public string Remote { get; set; }
        public bool Plink { get; set; }
        public string ProcessString { get; set; }
        public List<string> ProcessArguments { get; set; }
        public Process Process { get; set; }

        private bool restart = false;
        private GitCommands.GitCommands[] gitCommands;

        public FormProcess(string process, List<string> arguments)
        {
            ProcessCallback = new ProcessStart(processStart);
            AbortCallback = new ProcessAbort(processAbort);
            ProcessString = process ?? GitCommands.Settings.GitCommand;
            ProcessArguments = arguments;
            Remote = "";
        }

        public FormProcess(string arguments)
            : this(null, arguments)
        {
        }

        public FormProcess(string process, string arguments)
            : this(process, new List<string>() { arguments })
        {
        }

        public FormProcess(List<string> arguments)
            : this(null, arguments)
        {
        }

        private void processStart(FormStatus form)
        {
            restart = false;            

            Plink = GitCommands.GitCommands.Plink();
            gitCommands = new GitCommands.GitCommands[ProcessArguments.Count];
            int t=0;
            // execute each command in list ProcessArguments
            // - atm return values are async, this needs to be changed before release
            // - error handling below in gitCommand_Exited() is broken too with this
            foreach (var ProcessArgument in ProcessArguments)
            {
                AddOutput(ProcessString + " " + ProcessArguments[t]);
                gitCommands[t] = new GitCommands.GitCommands();
                gitCommands[t].CollectOutput = false;
                Process = gitCommands[t].CmdStartProcess(ProcessString, ProcessArgument);
                gitCommands[t].Exited += new EventHandler(gitCommand_Exited);
                gitCommands[t].DataReceived += new DataReceivedEventHandler(gitCommand_DataReceived);
                t++;
            }
            

        }

        private void processAbort(FormStatus form)
        {
            if (Process != null)
            {
                Process.Kill();
            }
        }

        void gitCommand_Exited(object sender, EventArgs e)
        {
            // This has to happen on the UI thread
            SendOrPostCallback method = new SendOrPostCallback(delegate(object o)
            {

            if (restart)
            {
                Reset();
                ProcessCallback(this);
                return;
            }

            bool isError;
            try
            {
                // An error occurred!
                //if (gitCommand != null && gitCommand.Process != null && gitCommand.Process.ExitCode != 0)
                //{
                //    isError = true;

                //    //// TODO: This Plink stuff here seems misplaced. Is there a better
                //    //// home for all of this stuff? For example, if I had a label called pull, 
                //    //// we could end up in this code incorrectly.
                //    //if (Plink)
                //    //{
                //    //    if (ProcessArguments.ToLower().Contains("pull") ||
                //    //        ProcessArguments.ToLower().Contains("push") ||
                //    //        ProcessArguments.ToLower().Contains("plink") ||
                //    //        ProcessArguments.ToLower().Contains("tortoiseplink") ||
                //    //        ProcessArguments.ToLower().Contains("remote") ||
                //    //        ProcessString.ToLower().Contains("clone") ||
                //    //        ProcessArguments.ToLower().Contains("clone"))
                //    //    {
                //    //        if (OutputString.ToString().Contains("successfully authenticated"))
                //    //        {
                //    //            isError = false;
                //    //        }

                //    //        if (OutputString.ToString().Contains("FATAL ERROR") && OutputString.ToString().Contains("authentication"))
                //    //        {
                //    //            FormPuttyError puttyError = new FormPuttyError();
                //    //            puttyError.ShowDialog();
                //    //            if (puttyError.RetryProcess)
                //    //            {
                //    //                Reset();
                //    //                ProcessCallback(this);
                //    //                return;
                //    //            }
                //    //        }
                //    //    }
                //    //}
                //}
                //else
                //{
                    isError = false;
                //}
            }
            catch
            {
                isError = true;
            }

            Done(!isError);

            });

            syncContext.Send(method, this);
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
                OutputString.Append(e.Data);
                OutputString.Append(Environment.NewLine);
            }


            if (Plink)
            {
                if (e.Data.StartsWith("If you trust this host, enter \"y\" to add the key to"))
                {
                    if (MessageBox.Show("The fingerprint of this host is not registered by PuTTY." + Environment.NewLine + "This causes this process to hang, and that why it is automatically stopped." + Environment.NewLine + Environment.NewLine + "When the connection is opened detached from Git and GitExtensions, the host's fingerprint can be registered." + Environment.NewLine + "You could also manually add the host's fingerprint or run Test Connection from the remotes dialog." + Environment.NewLine + Environment.NewLine + "Do you want to register the host's fingerprint and restart the process?", "Host Fingerprint not registered", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string remoteUrl = GitCommands.GitCommands.GetSetting("remote." + Remote + ".url");

                        if (string.IsNullOrEmpty(remoteUrl))
                            GitCommands.GitCommands.RunRealCmd("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" " + Remote + "\"");
                        else
                            GitCommands.GitCommands.RunRealCmd("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" " + remoteUrl + "\"");

                        restart = true;
                    }

                    try
                    {
                        //gitCommand.Process.Kill();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
