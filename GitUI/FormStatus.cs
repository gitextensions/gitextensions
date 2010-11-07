using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public partial class FormStatus : GitExtensionsForm
    {
        public delegate void ProcessStart(FormStatus form);
        public delegate void ProcessAbort(FormStatus form);

        protected readonly SynchronizationContext syncContext;

        public FormStatus()
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent(); Translate();
            KeepDialogOpen.Checked = !GitCommands.Settings.CloseProcessDialog;
        }

        public FormStatus(ProcessStart process, ProcessAbort abort)
            : this()
        {
            ProcessCallback = process;
            AbortCallback = abort;
        }

        public StringBuilder OutputString = new StringBuilder();
        public ProcessStart ProcessCallback = null;
        public ProcessAbort AbortCallback = null;
        private bool errorOccurred = false;
        private bool showOnError = false;

        public bool ErrorOccurred()
        {
            return errorOccurred;
        }

        public void SetProgress(string text)
        {
            // This has to happen on the UI thread
            SendOrPostCallback method = new SendOrPostCallback(delegate(object o)
            {
                int index = text.IndexOf('%');
                int progressValue;
                if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out progressValue))
                {
                    if (ProgressBar.Style != ProgressBarStyle.Blocks)
                        ProgressBar.Style = ProgressBarStyle.Blocks;
                    ProgressBar.Value = Math.Min(100, progressValue);

                    if (TaskbarManager.IsPlatformSupported)
                    {
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                        TaskbarManager.Instance.SetProgressValue(progressValue, 100);
                    }
                }
                this.Text = text;
            });
            syncContext.Send(method, this);
        }

        public void AddOutput(string text)
        {
            Output.Text += text + Environment.NewLine;
            Output.Visible = true;
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

            if (TaskbarManager.IsPlatformSupported)
            {
                if (isSuccess)
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                else
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);

                TaskbarManager.Instance.SetProgressValue(100, 100);
            }

            SuccessImage.Visible = isSuccess;
            ErrorImage.Visible = !isSuccess;
            errorOccurred = !isSuccess;
            splitContainer5.Panel2Collapsed = false;

            if (showOnError && !isSuccess)
            {
                // For some reason setting the state to normal interferes with
                // proper parent centering...
                WindowState = FormWindowState.Normal;
                CenterToParent();
                Visible = true;
            }

            if (isSuccess && (showOnError || GitCommands.Settings.CloseProcessDialog))
            {
                Close();
            }
        }

        public void Reset()
        {
            Output.Text = "";
            Output.Visible = false;
            ProgressBar.Visible = true;
            Ok.Enabled = false;
        }

        public void ShowDialogOnError()
        {
            Visible = false;
            KeepDialogOpen.Visible = false;
            Abort.Visible = false;
            showOnError = true;
            // Just hiding it still seems to draw one frame of the control
            WindowState = FormWindowState.Minimized;
            ShowDialog();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormStatus_Load(object sender, EventArgs e)
        {
            if (ProcessCallback == null)
            {
                throw new InvalidOperationException("You can't load the form without a ProcessCallback");
            }
            
            if (AbortCallback == null)
            {
                Abort.Visible = false;
            }
            RestorePosition("process");
            Start();
        }

        private void FormStatus_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (TaskbarManager.IsPlatformSupported)
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }

        private void Start()
        {
            if (TaskbarManager.IsPlatformSupported)
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);

            Reset();
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
    }
}
