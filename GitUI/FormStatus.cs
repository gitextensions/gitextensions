﻿using System;
using System.Threading;
using System.Windows.Forms;

using GitCommands;

using GitUI.UserControls;

using JetBrains.Annotations;

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

        private readonly bool UseDialogSettings = true;

        public FormStatus(): this(true)
        { }

        public FormStatus(bool useDialogSettings)
            : base(true)
        {
            UseDialogSettings = useDialogSettings;
            ConsoleOutput = ConsoleOutputControl.CreateInstance();
            ConsoleOutput.Dock = DockStyle.Fill;
            ConsoleOutput.Terminated += delegate { Close(); }; // This means the control is not visible anymore, no use in keeping. Expected scenario: user hits ESC in the prompt after the git process exits

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

        protected readonly ConsoleOutputControl ConsoleOutput; // Naming: protected stuff must be CLS-compliant here
        public ProcessStart ProcessCallback;
        public ProcessAbort AbortCallback;
        private bool errorOccurred;
        private bool showOnError;

        /// <summary>
        /// Gets the logged output text. Note that this is a separate string from what you see in the console output control.
        /// For instance, progress messages might be skipped; other messages might be added manually.
        /// </summary>
        [NotNull]
        public readonly FormStatusOutputLog OutputLog = new FormStatusOutputLog();

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
                    int index = text.LastIndexOf('%');
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
                    // Show last progress message in the title, unless it's showin in the control body already
                    if(!ConsoleOutput.IsDisplayingFullProcessOutput)
                      Text = text;
                };
            BeginInvoke(method, this);
        }

        /// <summary>
        /// Adds a message to the console display control ONLY, <see cref="GetOutputString" /> will not list it.
        /// </summary>
        public void AddMessage(string text)
        {
            ConsoleOutput.AppendMessageFreeThreaded(text);
        }

        /// <summary>
        /// Adds a message line to the console display control ONLY, <see cref="GetOutputString" /> will not list it.
        /// </summary>
        public void AddMessageLine(string text)
        {
            AddMessage(text + Environment.NewLine);
        }

        public void Done(bool isSuccess)
        {
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
            OutputLog.Clear();
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
            Reset();
            ProcessCallback(this);
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            try
            {
                AbortCallback(this);
                OutputLog.Append(Environment.NewLine + "Aborted");	// TODO: write to display control also, if we pull the function up to this base class
                Done(false);
                DialogResult = DialogResult.Abort;
            }
            catch { }
        }

        public string GetOutputString()
        {
            return OutputLog.GetString();
        }

      private void KeepDialogOpen_CheckedChanged(object sender, EventArgs e)
      {
          AppSettings.CloseProcessDialog = !KeepDialogOpen.Checked;

          // Maintain the invariant: if changing to "don't keep" and conditions are such that the dialog would have closed in dont-keep mode, then close it
          if((!KeepDialogOpen.Checked /* keep off */) && (Ok.Enabled /* done */) && (!errorOccurred /* and successful */)) /* not checking for UseDialogSettings because checkbox is only visible with True */
              Close();
      }
    }
}
