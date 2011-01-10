using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    delegate void DataCallback(string text);
    public class FormProcess : FormStatus
    {
        public string Remote { get; set; }
        public bool Plink { get; set; }
        public string ProcessString { get; set; }
        public string ProcessArguments { get; set; }
        public Process Process { get; set; }

        private bool restart = false;
        private GitCommands.GitCommandsInstance gitCommand;

        public FormProcess(string process, string arguments) : base()
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

        private void processStart(FormStatus form)
        {
            restart = false;
            AddOutput(ProcessString + " " + ProcessArguments);

            Plink = GitCommandHelpers.Plink();

            gitCommand = new GitCommands.GitCommandsInstance();
            gitCommand.CollectOutput = false;
            try
            {
                Process = gitCommand.CmdStartProcess(ProcessString, ProcessArguments);

                gitCommand.Exited += new EventHandler(gitCommand_Exited);
                gitCommand.DataReceived += new DataReceivedEventHandler(gitCommand_DataReceived);
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
                if (gitCommand != null && gitCommand.ExitCode != 0)
                {
                    isError = true;

                    // TODO: This Plink stuff here seems misplaced. Is there a better
                    // home for all of this stuff? For example, if I had a label called pull, 
                    // we could end up in this code incorrectly.
                    if (Plink)
                    {
                        if (ProcessArguments.ToLower().Contains("pull") ||
                            ProcessArguments.ToLower().Contains("push") ||
                            ProcessArguments.ToLower().Contains("plink") ||
                            ProcessArguments.ToLower().Contains("tortoiseplink") ||
                            ProcessArguments.ToLower().Contains("remote") ||
                            ProcessString.ToLower().Contains("clone") ||
                            ProcessArguments.ToLower().Contains("clone"))
                        {
                            if (OutputString.ToString().Contains("successfully authenticated"))
                            {
                                isError = false;
                            }

                            if (OutputString.ToString().Contains("FATAL ERROR") && OutputString.ToString().Contains("authentication"))
                            {
                                FormPuttyError puttyError = new FormPuttyError();
                                puttyError.ShowDialog();
                                if (puttyError.RetryProcess)
                                {
                                    Reset();
                                    ProcessCallback(this);
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    isError = false;
                }
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
                OutputString.AppendLine(e.Data);

                AddToTimer(e.Data);
                AddToTimer(Environment.NewLine);
            }


            if (Plink)
            {
                if (e.Data.StartsWith("If you trust this host, enter \"y\" to add the key to"))
                {
                    if (MessageBox.Show("The fingerprint of this host is not registered by PuTTY." + Environment.NewLine + "This causes this process to hang, and that why it is automatically stopped." + Environment.NewLine + Environment.NewLine + "When the connection is opened detached from Git and GitExtensions, the host's fingerprint can be registered." + Environment.NewLine + "You could also manually add the host's fingerprint or run Test Connection from the remotes dialog." + Environment.NewLine + Environment.NewLine + "Do you want to register the host's fingerprint and restart the process?", "Host Fingerprint not registered", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string remoteUrl = GitCommandHelpers.GetSetting("remote." + Remote + ".url");

                        if (string.IsNullOrEmpty(remoteUrl))
                            GitCommandHelpers.RunRealCmd("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" " + Remote + "\"");
                        else
                            GitCommandHelpers.RunRealCmd("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" " + remoteUrl + "\"");

                        restart = true;
                    }

                    try
                    {
                        gitCommand.Kill();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
