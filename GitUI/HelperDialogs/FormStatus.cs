using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUI.UserControls;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI.HelperDialogs
{
    public partial class FormStatus : GitExtensionsDialog
    {
        private readonly bool _useDialogSettings;
        private bool _errorOccurred;

        private protected Action<FormStatus>? ProcessCallback;
        private protected Action<FormStatus>? AbortCallback;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private protected FormStatus()
            : this(commands: null, consoleOutput: null, useDialogSettings: true)
        {
        }

        public FormStatus(GitUICommands? commands, ConsoleOutputControl? consoleOutput, bool useDialogSettings)
            : base(commands, enablePositionRestore: true)
        {
            _useDialogSettings = useDialogSettings;

            ConsoleOutput = consoleOutput ?? ConsoleOutputControl.CreateInstance();
            ConsoleOutput.Terminated += (s, e) =>
            {
                // This means the control is not visible anymore, no use in keeping.
                // Expected scenario: user hits ESC in the prompt after the git process exits
                Close();
            };

            InitializeComponent();

            SetIcon(Images.StatusBadgeWaiting);

            pnlOutput.Controls.Add(ConsoleOutput);
            ConsoleOutput.Dock = DockStyle.Fill;

            if (_useDialogSettings)
            {
                KeepDialogOpen.Checked = !AppSettings.CloseProcessDialog;
            }
            else
            {
                KeepDialogOpen.Hide();
            }

            // work-around the designer bug that can't add controls to FlowLayoutPanel
            ControlsPanel.Controls.Add(Abort);
            ControlsPanel.Controls.Add(Ok);
            ControlsPanel.Controls.Add(KeepDialogOpen);
            AcceptButton = Ok;
            CancelButton = Abort;

            Controls.SetChildIndex(ProgressBar, 1);
            ProgressBar.Dock = DockStyle.Bottom;

            InitializeComplete();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();

                Icon?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // disable the control box close button
                CreateParams mdiCp = base.CreateParams;
                mdiCp.ClassStyle |= NativeMethods.CP_NOCLOSE_BUTTON;
                return mdiCp;
            }
        }

        private protected ConsoleOutputControl ConsoleOutput { get; }

        /// <summary>
        /// Gets the logged output text. Note that this is a separate string from what you see in the console output control.
        /// For instance, progress messages might be skipped; other messages might be added manually.
        /// </summary>
        private protected FormStatusOutputLog OutputLog { get; } = new();

        public bool ErrorOccurred()
        {
            return _errorOccurred;
        }

        public string GetOutputString()
        {
            return OutputLog.GetString();
        }

        public void Retry()
        {
            Reset();
            ProcessCallback?.Invoke(this);
        }

        public static void ShowErrorDialog(IWin32Window owner, string text, params string[] output)
        {
            using FormStatus form = new(commands: null, new EditboxBasedConsoleOutputControl(), useDialogSettings: true);
            form.Text = text;
            if (output?.Length > 0)
            {
                foreach (string line in output)
                {
                    form.AppendMessage(line);
                }
            }

            form.ProgressBar.Visible = false;
            form.KeepDialogOpen.Visible = false;
            form.Abort.Visible = false;

            form.StartPosition = FormStartPosition.CenterParent;

            // We know that an operation (whatever it may have been) has failed, so set the error state.
            form.Done(false);

            form.ShowDialog(owner);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TaskbarProgress.Clear();
            base.OnFormClosed(e);
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            // If the dialog was invoked via ShowErrorDialog, there is no operation to invoke
            // it has already failed (i.e. completed).
            if (!_errorOccurred)
            {
                Start();
            }
        }

        /// <summary>
        /// Adds a message to the console display control ONLY, <see cref="GetOutputString" /> will not list it.
        /// </summary>
        private protected void AppendMessage(string text)
        {
            ConsoleOutput.AppendMessageFreeThreaded(text);
        }

        private protected void Done(bool isSuccess)
        {
            try
            {
                AppendMessage("Done");
                ProgressBar.Visible = false;
                Ok.Enabled = true;
                Ok.Focus();
                AcceptButton = Ok;
                Abort.Enabled = false;
                TaskbarProgress.SetProgress(isSuccess ? TaskbarProgressBarState.Normal : TaskbarProgressBarState.Error, 100, 100);

                Bitmap image = isSuccess ? Images.StatusBadgeSuccess : Images.StatusBadgeError;
                SetIcon(image);

                _errorOccurred = !isSuccess;

                if (isSuccess && (_useDialogSettings && AppSettings.CloseProcessDialog))
                {
                    Close();
                }
            }
            catch (ConEmu.WinForms.GuiMacroExecutor.GuiMacroException)
            {
                // Do nothing
            }
        }

        private protected void Reset()
        {
            SetIcon(Images.StatusBadgeWaiting);
            ConsoleOutput.Reset();
            OutputLog.Clear();
            ProgressBar.Visible = true;
            Ok.Enabled = false;
            ActiveControl = null;
        }

        private void SetIcon(Bitmap image)
        {
            Icon oldIcon = Icon;
            Icon = image.ToIcon();
            oldIcon?.Dispose();
        }

        private protected async Task SetProgressAsync(string text)
        {
            // This has to happen on the UI thread
            await this.SwitchToMainThreadAsync();

            int index = text.LastIndexOf('%');
            if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out var progressValue) && progressValue >= 0)
            {
                ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressBar.Value = Math.Min(100, progressValue);
                TaskbarProgress.SetProgress(TaskbarProgressBarState.Normal, progressValue, 100);
            }

            // Show last progress message in the title, unless it's showing in the control body already
            if (!ConsoleOutput.IsDisplayingFullProcessOutput)
            {
                Text = text;
            }
        }

        private void Start()
        {
            if (ProcessCallback is null)
            {
                throw new InvalidOperationException("You can't load the form without a ProcessCallback");
            }

            if (AbortCallback is null)
            {
                Abort.Visible = false;
            }

            StartPosition = FormStartPosition.CenterParent;
            TaskbarProgress.SetState(TaskbarProgressBarState.Indeterminate);

            Reset();
            ProcessCallback(this);
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            try
            {
                AbortCallback?.Invoke(this);
                OutputLog.Append(Environment.NewLine + "Aborted");  // TODO: write to display control also, if we pull the function up to this base class
                Done(false);
                DialogResult = DialogResult.Abort;
            }
            catch
            {
            }
        }

        private void KeepDialogOpen_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.CloseProcessDialog = !KeepDialogOpen.Checked;

            // Maintain the invariant: if changing to "don't keep" and conditions are such that the dialog would have closed in dont-keep mode, then close it
            // Not checking for UseDialogSettings because checkbox is only visible with True
            if ((!KeepDialogOpen.Checked /* keep off */) && Ok.Enabled /* done */ && (!_errorOccurred /* and successful */))
            {
                Close();
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
