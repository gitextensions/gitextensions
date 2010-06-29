using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace GitUI
{
    delegate void DataCallback(string text);
    public partial class FormProcess : GitExtensionsForm
    {
        public delegate void ProcessStart(FormProcess form);
        public delegate void ProcessAbort(FormProcess form);

        public FormProcess(ProcessStart process, ProcessAbort abort)
        {
            InitializeComponent(); Translate();
            KeepDialogOpen.Checked = !GitCommands.Settings.CloseProcessDialog;
            
            ProcessCallback = process;
            AbortCallback = abort;
            if (abort == null)
            {
                Abort.Visible = false;
            }
        }

        public FormProcess(string process, string arguments)
        {
            InitializeComponent(); Translate();
            KeepDialogOpen.Checked = !GitCommands.Settings.CloseProcessDialog;
            
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

        public StringBuilder OutputString = new StringBuilder();
        public ProcessStart ProcessCallback;
        public ProcessAbort AbortCallback;
        private bool errorOccured = false;
        private bool showOnError = false;

        public bool ErrorOccured()
        {
            return errorOccured;
        }

        public void SetProgress(string text)
        {
            int index = text.IndexOf('%');
            int progressValue;
            if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out progressValue))
            {
                if (ProgressBar.Style != ProgressBarStyle.Blocks)
                    ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressBar.Value = Math.Min(100, progressValue);
            }
            this.Text = text;
        }

        public void AddOutput(string text)
        {
            Output.Text += text + Environment.NewLine;
        }

        public void Done(bool isSuccess)
        {
            AddOutput(OutputString.ToString());
            AddOutput("Done");
            ProgressBar.Visible = false;
            Ok.Enabled = true;
            Ok.Focus();
            AcceptButton = Ok;
            Abort.Enabled = false;

            SuccessImage.Visible = isSuccess;
            ErrorImage.Visible = !isSuccess;
            errorOccured = !isSuccess;

            if (showOnError)
            {
                if (isSuccess)
                {
                    Close();
                }
                else
                {
                    // For some reason setting the state to normal interferes with
                    // proper parent centering...
                    WindowState = FormWindowState.Normal;
                    CenterToParent();
                    Visible = true;
                }
            }
        }

        public void ShowDialogOnError()
        {
            showOnError = true;
            Visible = false;
            // Just hiding it still seems to draw one frame of the control
            WindowState = FormWindowState.Minimized;
            ShowDialog();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormProcess_Load(object sender, EventArgs e)
        {
            RestorePosition("process");
            Start();
        }

        private void Start()
        {
            Output.Text = "";
            ProgressBar.Visible = true;
            Ok.Enabled = false;

            ProcessCallback(this);
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            try
            {
                AbortCallback(this);
                OutputString.Append(Environment.NewLine + "Aborted");
                Done(false);
            }
            catch { }
        }

        private void KeepDialogOpen_CheckedChanged(object sender, EventArgs e)
        {
            GitCommands.Settings.CloseProcessDialog = !KeepDialogOpen.Checked;
        }

        private void FormProcess_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("process");
        }

        // TODO: Move this into its own class (likely called FormProcess, with the current
        // FormProcess getting renamed to FormStatusDisplay or something.
        #region Built in process handling

        public string Remote { get; set; }
        public bool Plink { get; set; }
        public string ProcessString { get; set; }
        public string ProcessArguments { get; set; }
        public Process Process { get; set; }

        private bool restart = false;
        private GitCommands.GitCommands gitCommand;

        private void processStart(FormProcess form)
        {
            restart = false;
            AddOutput(ProcessString + " " + ProcessArguments);

            Plink = GitCommands.GitCommands.Plink();

            gitCommand = new GitCommands.GitCommands();
            gitCommand.CollectOutput = false;
            Process = gitCommand.CmdStartProcess(ProcessString, ProcessArguments);

            gitCommand.Exited += new EventHandler(gitCommand_Exited);
            gitCommand.DataReceived += new DataReceivedEventHandler(gitCommand_DataReceived);
        }

        private void processAbort(FormProcess form)
        {
            if (Process != null)
            {
                Process.Kill();
            }
        }

        void gitCommand_Exited(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate() { gitCommand_Exited(sender, e); }));
                return;
            }

            if (restart)
            {
                Start();
                return;
            }

            bool isError;
            try
            {
                // An error occurred!
                if (gitCommand != null && gitCommand.Process != null && gitCommand.Process.ExitCode != 0)
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
                            if (Output.Text.Contains("successfully authenticated"))
                            {
                                isError = false;
                            }

                            if (Output.Text.Contains("FATAL ERROR") && Output.Text.Contains("authentication"))
                            {
                                FormPuttyError puttyError = new FormPuttyError();
                                puttyError.ShowDialog();
                                if (puttyError.RetryProcess)
                                {
                                    Start();
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    isError = false;

                    if (GitCommands.Settings.CloseProcessDialog)
                        Close();
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
                if (ProgressBar.InvokeRequired)
                {
                    // It's on a different thread, so use Invoke.
                    DataCallback d = new DataCallback(SetProgress);
                    this.Invoke(d, new object[] { e.Data });
                }
                else
                {
                    SetProgress(e.Data);
                }
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
                        gitCommand.Process.Kill();
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion

    }
}
