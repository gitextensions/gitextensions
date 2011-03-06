using System;
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
        public string ProcessInput { get; set; }
        public Process Process { get; set; }

        private bool restart;
        private GitCommandsInstance gitCommand;

        //Input does not work for password inputs. I don't know why, but it turned out not to be really necessary.
        //For other inputs, it is not tested.
        public FormProcess(string process, string arguments, string input)
            : this(process, arguments)
        {
            ProcessInput = input;
        }

        public FormProcess(string process, string arguments)
        {
            ProcessCallback = new ProcessStart(processStart);
            AbortCallback = new ProcessAbort(processAbort);
            ProcessString = process ?? Settings.GitCommand;
            ProcessArguments = arguments;
            Remote = "";
            ProcessInput = null;
        }

        public FormProcess(string arguments)
            : this(null, arguments)
        {
        }

        private string UrlTryingToConnect = string.Empty;
        /// <summary>
        /// When cloning a remote using putty, sometimes an error occurs that the fingerprint is not known.
        /// This is fixed by trying to connect from the command line, and choose yes when asked for storing
        /// the fingerpring. Just a dirty fix...
        /// </summary>
        public void SetUrlTryingToConnect(string url)
        {
            UrlTryingToConnect = url;
        }

        private void processStart(FormStatus form)
        {
            restart = false;
            AddOutput(ProcessString + " " + ProcessArguments);

            Plink = GitCommandHelpers.Plink();

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

        void gitCommand_Exited(object sender, EventArgs e)
        {
            // This has to happen on the UI thread
            var method = new SendOrPostCallback(OnExit);

            syncContext.Send(method, this);
        }

        private void OnExit(object state)
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
                        if (ProcessArguments.ToLower().Contains("pull") || ProcessArguments.ToLower().Contains("push") || ProcessArguments.ToLower().Contains("plink") || ProcessArguments.ToLower().Contains("tortoiseplink") || ProcessArguments.ToLower().Contains("remote") || ProcessString.ToLower().Contains("clone") || ProcessArguments.ToLower().Contains("clone"))
                        {
                            if (OutputString.ToString().Contains("successfully authenticated"))
                            {
                                isError = false;
                            }

                            if (OutputString.ToString().Contains("FATAL ERROR") && OutputString.ToString().Contains("authentication"))
                            {
                                var puttyError = new FormPuttyError();
                                puttyError.ShowDialog();
                                if (puttyError.RetryProcess)
                                {
                                    Reset();
                                    ProcessCallback(this);
                                    return;
                                }
                            }
                            if (OutputString.ToString().ToLower().Contains("the server's host key is not cached in the registry") && !string.IsNullOrEmpty(UrlTryingToConnect))
                            {
                                if (MessageBox.Show("The server's host key is not cached in the registry.\n\nDo you want to trust this host key and then try again?", "SSH", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    GitCommandHelpers.RunRealCmdDetached(
                                        "cmd.exe",
                                        string.Format("/k \"\"{0}\" -T \"{1}\"\"", Settings.Plink, UrlTryingToConnect));

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
                            GitCommandHelpers.RunRealCmd("cmd.exe", "/k \"\"" + Settings.Plink + "\" " + Remote + "\"");
                        else
                            GitCommandHelpers.RunRealCmd("cmd.exe", "/k \"\"" + Settings.Plink + "\" " + remoteUrl + "\"");

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
