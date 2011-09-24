using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public partial class FormStatus : GitExtensionsForm
    {
        public delegate void ProcessStart(FormStatus form);
        public delegate void ProcessAbort(FormStatus form);

        private string _SettingsName;
        public string SettingsName
        {
            get { return _SettingsName; }
            set
            {
                _SettingsName = value;
                RefreshKeepDialogOpen();                
            }
        }

        protected readonly SynchronizationContext syncContext;

        //constructor for VS designer
        protected FormStatus()
        {
            syncContext = SynchronizationContext.Current;
            InitializeComponent();            
        }

        public FormStatus(string ASettingsName)
            : this()
        {
            Translate();
            SettingsName = ASettingsName;            
        }

        public FormStatus(ProcessStart process, ProcessAbort abort, string ASettingsName)
            : this(ASettingsName)
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

                        if (TaskbarManager.IsPlatformSupported)
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
            if (TaskbarManager.IsPlatformSupported)
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

            if (isSuccess && (showOnError || GetCloseProcessDialog()))
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
            if (DesignMode)
                return;

            if (ProcessCallback == null)
            {
                throw new InvalidOperationException("You can't load the form without a ProcessCallback " + DesignMode);
            }

            if (AbortCallback == null)
            {
                Abort.Visible = false;
            }
            RestorePosition("process");
            Start();
        }

        private void FormStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                }
                catch (InvalidOperationException) { }
            }
        }

        private void Start()
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
                }
                catch (InvalidOperationException) { }
            }
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
            }
            catch { }
        }

        private bool GetCloseProcessDialog()
        {

            bool? gcpd = GitCommands.Settings.GlobalCloseProcessDialog;
            if (gcpd.HasValue)
                return gcpd.Value;
            else
                if (string.IsNullOrEmpty(SettingsName))
                    return false;
                else
                {
                    bool? cpd = GitCommands.Settings.GetCloseProcessDialog(SettingsName);
                    if (cpd.HasValue)
                        return cpd.Value;
                    else
                        return false;
                }
        }

        private void RefreshKeepDialogOpen()         
        {
            if (string.IsNullOrEmpty(SettingsName))
                KeepDialogOpen.Visible = false;
            else
            {
                bool? cpd = GitCommands.Settings.GetCloseProcessDialog(SettingsName);
                KeepDialogOpen.SetNullableChecked(!cpd);
                KeepDialogOpen.Visible = true;
            }
        }

        private void KeepDialogOpen_CheckStateChanged(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(SettingsName))
            {
                bool? cpd = !KeepDialogOpen.GetNullableChecked();
                GitCommands.Settings.SetCloseProcessDialog(SettingsName, cpd);
            }
        }
    }
}
