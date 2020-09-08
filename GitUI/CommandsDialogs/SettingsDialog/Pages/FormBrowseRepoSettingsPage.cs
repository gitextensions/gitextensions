using System;
using GitCommands;
using GitUI.Configuring;
using GitUI.Shells;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
    {
        private readonly ShellProvider _shellProvider = new ShellProvider();
        private readonly IShellService _shellService = new ShellService();

        private int _cboTerminalPreviousIndex = -1;

        public FormBrowseRepoSettingsPage()
        {
            InitializeComponent();
            Text = "Browse repository window";
            cboTerminal.DisplayMember = "Name";
            InitializeComplete();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);
        }

        protected override void PageToSettings()
        {
            AppSettings.ShowConEmuTab.Value = chkChowConsoleTab.Checked;
            AppSettings.ShowGpgInformation.Value = chkShowGpgInformation.Checked;

            var selectedShell = (IShell)cboTerminal.SelectedItem;

            if (selectedShell != null)
            {
                _shellService.Default(selectedShell.Name);
            }

            base.PageToSettings();
        }

        protected override void SettingsToPage()
        {
            chkChowConsoleTab.Checked = AppSettings.ShowConEmuTab.Value;
            chkShowGpgInformation.Checked = AppSettings.ShowGpgInformation.Value;

            foreach (var shell in _shellService.List())
            {
                cboTerminal.Items.Add(shell);

                if (shell.Default)
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
            if (!(cboTerminal.SelectedItem is IShell shell))
            {
                return;
            }

            var shellDescriptor = _shellProvider
                .GetShell(shell.Name);

            if (shellDescriptor.HasExecutable)
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
