using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
#if !__MonoCS__
using Microsoft.WindowsAPICodePack.Taskbar;
#endif

namespace GitUI
{
    public partial class FormStatus : GitExtensionsForm
    {
        public delegate void ProcessStart(FormStatus form);
        public delegate void ProcessAbort(FormStatus form);

        protected readonly SynchronizationContext syncContext;
        private bool UseDialogSettings = true;

        public FormStatus(): this(true)
        { }

        public FormStatus(bool useDialogSettings)
        {
            syncContext = SynchronizationContext.Current;
            UseDialogSettings = useDialogSettings;

            InitializeComponent();
            Translate();
            if (UseDialogSettings)
                KeepDialogOpen.Checked = !GitCommands.Settings.CloseProcessDialog;
            else
                KeepDialogOpen.Hide();
        }

        public FormStatus(ProcessStart process, ProcessAbort abort)
            : this()
        {
            ProcessCallback = process;
            AbortCallback = abort;
        }

        public StringBuilder OutputString = new StringBuilder();
        public ProcessStart ProcessCallback;
        public ProcessAbort AbortCallback;
        private bool errorOccurred;
        private bool showOnError;

        public bool ErrorOccurred()
        {
            return errorOccurred;
        }

        public void SetProgress(string text)
        {
            // This has to happen on the UI thread
            SendOrPostCallback method = o =>
                {
                    int index = text.IndexOf('%');
                    int progressValue;
                    if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out progressValue))
                    {
                        if (ProgressBar.Style != ProgressBarStyle.Blocks)
                            ProgressBar.Style = ProgressBarStyle.Blocks;
                        ProgressBar.Value = Math.Min(100, progressValue);

#if !__MonoCS__
                        if (GitCommands.Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
                            {
                                try
                                {
                                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                                    TaskbarManager.Instance.SetProgressValue(progressValue, 100);
                                }
                                catch (InvalidOperationException)
                                {
                                }
                            }
#endif
                    }
                    Text = text;
                };
            syncContext.Send(method, this);
        }

        public void AddToTimer(string text)
        {
            lock (ProcessOutputTimer.linesToAdd)
            {
                ProcessOutputTimer.addLine(text);
            }
        }

        public void AddOutputCrossThread(string text)
        {
            SendOrPostCallback method = o =>
                {
                    Output.Text += text;
                    Output.SelectionStart = Output.Text.Length;
                    Output.ScrollToCaret();
                    Output.Visible = true;
                };
            syncContext.Post(method, this);
        }

        public void AddOutput(string text)
        {
            Output.Text += text + Environment.NewLine;
            Output.Visible = true;
            Output.SelectionStart = Output.Text.Length;
            Output.ScrollToCaret();
        }

        public void Done(bool isSuccess)
        {
            ProcessOutputTimer.Stop();
            AddOutput("Done");
            ProgressBar.Visible = false;
            Ok.Enabled = true;
            Ok.Focus();
            AcceptButton = Ok;
            Abort.Enabled = false;
#if !__MonoCS__
            if (GitCommands.Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(isSuccess
                                                                 ? TaskbarProgressBarState.Normal
                                                                 : TaskbarProgressBarState.Error);

                    TaskbarManager.Instance.SetProgressValue(100, 100);
                }
                catch (InvalidOperationException) { }
            }
#endif

            if (isSuccess)
                picBoxSuccessFail.Image = GitUI.Properties.Resources.success;
            else
                picBoxSuccessFail.Image = GitUI.Properties.Resources.error;
            splitContainer1.Panel2Collapsed = false;

            errorOccurred = !isSuccess;

            if (showOnError && !isSuccess)
            {
                // For some reason setting the state to normal interferes with
                // proper parent centering...
                WindowState = FormWindowState.Normal;
                CenterToParent();
                Visible = true;
            }

            if (isSuccess && (showOnError || (UseDialogSettings && GitCommands.Settings.CloseProcessDialog)))
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

        public void Retry()
        {
            Reset();
            ProcessCallback(this);
        }

        public void ShowDialogOnError()
        {
            ShowDialogOnError(null);
        }

        public void ShowDialogOnError(IWin32Window owner)
        {
            Visible = false;
            KeepDialogOpen.Visible = false;
            Abort.Visible = false;
            showOnError = true;
            // Just hiding it still seems to draw one frame of the control
            WindowState = FormWindowState.Minimized;
            ShowDialog(owner);
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.OK;
        }

        private void FormStatus_Load(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = true;
            if (DesignMode)
                return;

            if (ProcessCallback == null)
            {
                throw new InvalidOperationException("You can't load the form without a ProcessCallback");
            }

            if (AbortCallback == null)
            {
                Abort.Visible = false;
            }
            StartPosition = FormStartPosition.CenterParent;
            RestorePosition("process");
            Start();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SavePosition("process");
            base.OnClosing(e);
        }

        private void FormStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
#if !__MonoCS__
            if (GitCommands.Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                }
                catch (InvalidOperationException) { }
            }
#endif
        }

        private void Start()
        {
#if !__MonoCS__
            if (GitCommands.Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
                }
                catch (InvalidOperationException) { }
            }
#endif
            ProcessOutputTimer.Start(this);
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
                DialogResult = DialogResult.Abort;
            }
            catch { }
        }

        private void KeepDialogOpen_CheckedChanged(object sender, EventArgs e)
        {
            GitCommands.Settings.CloseProcessDialog = !KeepDialogOpen.Checked;
        }
    }
}
