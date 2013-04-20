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
        private ProcessOutputTimer outpuTimer;

        public FormStatus(): this(true)
        { }

        public FormStatus(bool useDialogSettings)
            : base(true)
        {
            outpuTimer = new ProcessOutputTimer(AppendMessageCrossThread);
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

        private readonly StringBuilder _outputString = new StringBuilder();
        public ProcessStart ProcessCallback;
        public ProcessAbort AbortCallback;
        private bool errorOccurred;
        private bool showOnError;

        protected override CreateParams CreateParams
        {

            get
            {

                CreateParams mdiCp = base.CreateParams;

                mdiCp.ClassStyle = mdiCp.ClassStyle | NativeConstants.CP_NOCLOSE_BUTTON;

                return mdiCp;

            }

        }

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
                        if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
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

        public void AppendMessageCrossThread(string text)
        {
            if (syncContext == SynchronizationContext.Current)
                AppendMessage(text); 
            else
                syncContext.Post(o => AppendMessage(text), this);
        }

        public void AddMessage(string text)
        {
            AddMessageToTimer(text);
        }

        public void AddMessageLine(string text)
        {
            AddMessage(text + Environment.NewLine);
        }

        private void AddMessageToTimer(string text)
        {
            if (outpuTimer != null)
                outpuTimer.Append(text);
        }
        
        private void AppendMessage(string text)
        {
            //if not disposed
            if (outpuTimer != null)
            {
                MessageTextBox.Text += text;
                MessageTextBox.SelectionStart = MessageTextBox.Text.Length;
                MessageTextBox.ScrollToCaret();
                MessageTextBox.Visible = true;
            }
        }


        public void Done(bool isSuccess)
        {
            if (outpuTimer != null)
                outpuTimer.Stop(true);
            AppendMessage("Done");
            ProgressBar.Visible = false;
            Ok.Enabled = true;
            Ok.Focus();
            AcceptButton = Ok;
            Abort.Enabled = false;
#if !__MonoCS__
            if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
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
            outpuTimer.Clear();
            MessageTextBox.Text = "";
            MessageTextBox.Visible = false;
            lock (_outputString)
            {
                _outputString.Clear();
            }
            ProgressBar.Visible = true;
            Ok.Enabled = false;
            ActiveControl = null;
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

            Start();
        }

        private void FormStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
#if !__MonoCS__
            if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
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
            if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
                }
                catch (InvalidOperationException) { }
            }
#endif
            outpuTimer.Start();
            Reset();
            ProcessCallback(this);
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            try
            {
                AbortCallback(this);
                AppendToOutputString(Environment.NewLine + "Aborted");
                Done(false);
                DialogResult = DialogResult.Abort;
            }
            catch { }
        }

        public void AppendToOutputString(string text)
        {
            lock (_outputString)
            {
                _outputString.Append(text);
            }
        }

        public string GetOutputString()
        {
            lock (_outputString)
            {
                return _outputString.ToString();
            }
        }

        private void KeepDialogOpen_CheckedChanged(object sender, EventArgs e)
        {
            GitCommands.Settings.CloseProcessDialog = !KeepDialogOpen.Checked;
        }
    }
}
