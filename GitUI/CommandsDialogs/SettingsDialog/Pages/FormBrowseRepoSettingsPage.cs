using System;
using GitCommands;
using GitUI.Shells;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
    {
        private readonly ShellProvider _shellProvider = new();
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

            AppSettings.ConEmuTerminal.Value = ((IShellDescriptor)cboTerminal.SelectedItem).Name.ToLowerInvariant();
            base.PageToSettings();
        }

        protected override void SettingsToPage()
        {
            chkChowConsoleTab.Checked = AppSettings.ShowConEmuTab.Value;
            chkShowGpgInformation.Checked = AppSettings.ShowGpgInformation.Value;

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
