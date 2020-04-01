using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using GitCommands;
using GitUI.Properties;
using GitUI.UserControls;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public partial class FormStatus : GitExtensionsForm
    {
        private readonly bool _useDialogSettings;

        private DispatcherFrameModalController _modalController;

        public FormStatus() : this(true)
        {
        }

        public FormStatus(bool useDialogSettings)
            : this(null, useDialogSettings)
        {
        }

        public FormStatus([CanBeNull] ConsoleOutputControl consoleOutput, bool useDialogSettings)
            : base(true)
        {
            _useDialogSettings = useDialogSettings;

            ConsoleOutput = consoleOutput ?? ConsoleOutputControl.CreateInstance();
            ConsoleOutput.Dock = DockStyle.Fill;
            ConsoleOutput.Terminated += delegate { Close(); }; // This means the control is not visible anymore, no use in keeping. Expected scenario: user hits ESC in the prompt after the git process exits

            InitializeComponent();

            if (_useDialogSettings)
            {
                KeepDialogOpen.Checked = !AppSettings.CloseProcessDialog;
            }
            else
            {
                KeepDialogOpen.Hide();
            }

            InitializeComplete();
        }

        public FormStatus(Action<FormStatus> process, string text)
            : this(new EditboxBasedConsoleOutputControl(), true)
        {
            ProcessCallback = process;
            Text = text;
        }

        protected readonly ConsoleOutputControl ConsoleOutput;
        public Action<FormStatus> ProcessCallback;
        public Action<FormStatus> AbortCallback;
        private bool _errorOccurred;
        private bool _showOnError;

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
                mdiCp.ClassStyle = mdiCp.ClassStyle | NativeMethods.CP_NOCLOSE_BUTTON;
                return mdiCp;
            }
        }

        public bool ErrorOccurred()
        {
            return _errorOccurred;
        }

        public async Task SetProgressAsync(string text)
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
            try
            {
                AppendMessageCrossThread("Done");
                ProgressBar.Visible = false;
                Ok.Enabled = true;
                Ok.Focus();
                AcceptButton = Ok;
                Abort.Enabled = false;
                TaskbarProgress.SetProgress(
                    isSuccess
                        ? TaskbarProgressBarState.Normal
                        : TaskbarProgressBarState.Error,
                    100,
                    100);

                picBoxSuccessFail.Image = isSuccess
                    ? Images.StatusBadgeSuccess
                    : Images.StatusBadgeError;

                _errorOccurred = !isSuccess;

                if (isSuccess && !_showOnError && (_useDialogSettings && AppSettings.CloseProcessDialog))
                {
                    Close();
                }
            }
            catch (ConEmu.WinForms.GuiMacroExecutor.GuiMacroException)
            {
                // Do nothing
            }
            finally
            {
                _modalController?.EndModal(isSuccess);
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

        public void ShowDialogOnError(IWin32Window owner = null)
        {
            KeepDialogOpen.Visible = false;
            Abort.Visible = false;
            _showOnError = true;
            _modalController = new DispatcherFrameModalController(this, owner);
            _modalController.BeginModal();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.OK;
        }

        private void FormStatus_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (_modalController != null)
            {
                return;
            }

            Start();
        }

        private void FormStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
            AfterClosed();
        }

        internal void Start()
        {
            if (ProcessCallback == null)
            {
                throw new InvalidOperationException("You can't load the form without a ProcessCallback");
            }

            if (AbortCallback == null)
            {
                Abort.Visible = false;
            }

            StartPosition = FormStartPosition.CenterParent;

            TaskbarProgress.SetIndeterminate();

            Reset();
            ProcessCallback(this);
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            try
            {
                AbortCallback(this);
                OutputLog.Append(Environment.NewLine + "Aborted");  // TODO: write to display control also, if we pull the function up to this base class
                Done(false);
                DialogResult = DialogResult.Abort;
            }
            catch
            {
            }
        }

        public string GetOutputString()
        {
            return OutputLog.GetString();
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

        internal void AfterClosed()
        {
            TaskbarProgress.Clear();
        }
    }

    internal class DispatcherFrameModalController
    {
        private readonly DispatcherFrame _dispatcherFrame = new DispatcherFrame();
        private readonly FormStatus _formStatus;
        private readonly IWin32Window _owner;

        public DispatcherFrameModalController(FormStatus formStatus, IWin32Window owner)
        {
            _formStatus = formStatus;
            _owner = owner;
        }

        public void BeginModal()
        {
            _formStatus.Start();
            Dispatcher.PushFrame(_dispatcherFrame);
        }

        public void EndModal(bool success)
        {
            if (!success)
            {
                _formStatus.ShowDialog(_owner);
            }
            else
            {
                _formStatus.AfterClosed();
            }

            _dispatcherFrame.Continue = false;
        }
    }
}
