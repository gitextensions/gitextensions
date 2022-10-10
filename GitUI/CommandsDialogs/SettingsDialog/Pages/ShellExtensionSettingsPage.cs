using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ShellExtensionSettingsPage : SettingsPageWithHeader
    {
        public ShellExtensionSettingsPage()
        {
            InitializeComponent();
            Text = "Shell extension";
            InitializeComplete();

            // when the dock is set in the designer it causes weird visual artifacts in scaled Windows environments
            _NO_TRANSLATE_chlMenuEntries.Dock = DockStyle.Fill;
        }

        protected override void SettingsToPage()
        {
            for (int i = 0; i < AppSettings.CascadeShellMenuItems.Length; i++)
            {
                _NO_TRANSLATE_chlMenuEntries.SetItemChecked(i, AppSettings.CascadeShellMenuItems[i] == '1');
            }

            cbAlwaysShowAllCommands.Checked = AppSettings.AlwaysShowAllCommands;

            UpdatePreview();
        }

        protected override void PageToSettings()
        {
            string l_CascadeShellMenuItems = "";

            for (int i = 0; i < _NO_TRANSLATE_chlMenuEntries.Items.Count; i++)
            {
                if (_NO_TRANSLATE_chlMenuEntries.GetItemChecked(i))
                {
                    l_CascadeShellMenuItems += "1";
                }
                else
                {
                    l_CascadeShellMenuItems += "0";
                }
            }

            AppSettings.CascadeShellMenuItems = l_CascadeShellMenuItems;
            AppSettings.AlwaysShowAllCommands = cbAlwaysShowAllCommands.Checked;
        }

        private void chlMenuEntries_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            string topLevel = "";
            string cascaded = "";

            for (int i = 0; i < _NO_TRANSLATE_chlMenuEntries.Items.Count; i++)
            {
                if (_NO_TRANSLATE_chlMenuEntries.GetItemChecked(i))
                {
                    cascaded += "       " + _NO_TRANSLATE_chlMenuEntries.Items[i] + "\r\n";
                }
                else
                {
                    topLevel += "GitExt " + _NO_TRANSLATE_chlMenuEntries.Items[i] + "\r\n";
                }
            }

            labelPreview.Text = topLevel + "Git Extensions > \r\n" + cascaded;
        }
    }
}
