using System;
using GitCommands;
using GitUI.Shells;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
    {
        private readonly ShellProvider _shellProvider = new ShellProvider();
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

            // Bind settings with controls
            AddSettingBinding(AppSettings.ShowConEmuTab, chkChowConsoleTab);
            AddSettingBinding(AppSettings.ConEmuStyle, _NO_TRANSLATE_cboStyle);
            AddSettingBinding(AppSettings.ConEmuFontSize, cboFontSize);
            AddSettingBinding(AppSettings.ShowGpgInformation, chkShowGpgInformation);
        }

        protected override void PageToSettings()
        {
            AppSettings.ConEmuTerminal.Value = ((IShellDescriptor)cboTerminal.SelectedItem).Name.ToLowerInvariant();
            base.PageToSettings();
        }

        protected override void SettingsToPage()
        {
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

        private void chkChowConsoleTab_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxConsoleSettings.Enabled = chkChowConsoleTab.Checked;
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
