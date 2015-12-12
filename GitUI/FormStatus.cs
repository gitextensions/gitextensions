using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using GitUI.UserControls;

using ResourceManager;
#if !__MonoCS__
using Microsoft.WindowsAPICodePack.Taskbar;
#endif

namespace GitUI
{
    public partial class FormStatus : GitExtensionsForm
    {
        public delegate void ProcessStart(FormStatus form);
        public delegate void ProcessAbort(FormStatus form);

        private bool UseDialogSettings = true;

        public FormStatus(): this(true)
        { }

        public FormStatus(bool useDialogSettings)
            : base(true)
        {
            UseDialogSettings = useDialogSettings;
			ConsoleOutput = ConsoleOutputControl.CreateInstance();
	        ConsoleOutput.Dock = DockStyle.Fill;

	        InitializeComponent();
            Translate();
            if (UseDialogSettings)
                KeepDialogOpen.Checked = !GitCommands.AppSettings.CloseProcessDialog;
            else
                KeepDialogOpen.Hide();
        }

        public FormStatus(ProcessStart process, ProcessAbort abort)
            : this()
        {
            ProcessCallback = process;
            AbortCallback = abort;
        }

	    protected readonly ConsoleOutputControl ConsoleOutput;	// Naming: protected stuff must be CLS-compliant here
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
                    if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out progressValue) && progressValue >= 0)
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
            BeginInvoke(method, this);
        }


        public void AddMessage(string text)
        {
            ConsoleOutput.AppendMessageFreeThreaded(text);
        }

        public void AddMessageLine(string text)
        {
            AddMessage(text + Environment.NewLine);
        }

        public void Done(bool isSuccess)
        {
			ConsoleOutput.Done();
            AppendMessageCrossThread("Done");
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

            if (isSuccess && (showOnError || (UseDialogSettings && GitCommands.AppSettings.CloseProcessDialog)))
            {
                Close();
            }
        }

	    public void AppendMessageCrossThread(string text)
	    {
		    ConsoleOutput.AppendMessageFreeThreaded(text);
	    }

	    public void Reset()
        {
			ConsoleOutput.Reset();
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
            ConsoleOutput.Start();
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
            GitCommands.AppSettings.CloseProcessDialog = !KeepDialogOpen.Checked;
        }
    }
}
