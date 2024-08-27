using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitUI.Shells;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
    {
        private readonly ShellProvider _shellProvider = new();
        private int _cboTerminalPreviousIndex = -1;

        public FormBrowseRepoSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            cboTerminal.DisplayMember = "Name";
            InitializeComplete();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);
        }

        protected override void OnRuntimeLoad()
        {
            // align 1st columns across all tables
            tlpnlGeneral.AdjustWidthToSize(0, lblDefaultShell, chkUseBrowseForFileHistory, chkUseDiffViewerForBlame, chkShowFindInCommitFilesGitGrep, chkShowConsoleTab, chkShowGpgInformation);
            tlpnlTabs.AdjustWidthToSize(0, lblDefaultShell, chkUseBrowseForFileHistory, chkUseDiffViewerForBlame, chkShowFindInCommitFilesGitGrep, chkShowConsoleTab, chkShowGpgInformation);

            base.OnRuntimeLoad();
        }

        protected override void PageToSettings()
        {
            AppSettings.ShowConEmuTab.Value = chkShowConsoleTab.Checked;
            AppSettings.UseBrowseForFileHistory.Value = chkUseBrowseForFileHistory.Checked;
            AppSettings.UseDiffViewerForBlame.Value = chkUseDiffViewerForBlame.Checked;
            AppSettings.ShowGpgInformation.Value = chkShowGpgInformation.Checked;
            AppSettings.ShowFindInCommitFilesGitGrep.Value = chkShowFindInCommitFilesGitGrep.Checked;
            AppSettings.ConEmuTerminal.Value = ((IShellDescriptor)cboTerminal.SelectedItem).Name.ToLowerInvariant();

            base.PageToSettings();
        }

        protected override void SettingsToPage()
        {
            chkShowConsoleTab.Checked = AppSettings.ShowConEmuTab.Value;
            chkUseBrowseForFileHistory.Checked = AppSettings.UseBrowseForFileHistory.Value;
            chkUseDiffViewerForBlame.Checked = AppSettings.UseDiffViewerForBlame.Value;
            chkShowGpgInformation.Checked = AppSettings.ShowGpgInformation.Value;
            chkShowFindInCommitFilesGitGrep.Checked = AppSettings.ShowFindInCommitFilesGitGrep.Value;

            foreach (IShellDescriptor shell in _shellProvider.GetShells())
            {
                cboTerminal.Items.Add(shell);

                if (string.Equals(shell.Name, AppSettings.ConEmuTerminal.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    cboTerminal.SelectedItem = shell;
                }
            }

            base.SettingsToPage();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(FormBrowseRepoSettingsPage));
        }

        private void cboTerminal_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (!(cboTerminal.SelectedItem is IShellDescriptor shell))
            {
                return;
            }

            if (shell.HasExecutable)
            {
                return;
            }

            MessageBoxes.ShellNotFound(this);
            cboTerminal.SelectedIndex = _cboTerminalPreviousIndex;
        }

        private void cboTerminal_Enter(object sender, EventArgs e)
        {
            _cboTerminalPreviousIndex = cboTerminal.SelectedIndex;
        }
    }
}
