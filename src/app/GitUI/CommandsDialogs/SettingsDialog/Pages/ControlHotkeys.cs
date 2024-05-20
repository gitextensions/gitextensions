using GitUI.Hotkey;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    /// <summary>
    /// ControlHotkeys enables editing of HotkeySettings.
    /// </summary>
    internal partial class ControlHotkeys : GitExtensionsControl
    {
        private IReadOnlyList<HotkeySettings>? _settings;

        private IReadOnlyList<HotkeySettings>? Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                UpdateCombobox(value);
            }
        }

        #region SelectedHotkeySettings
        private HotkeySettings? _selectedHotkeySettings;
        private HotkeySettings? SelectedHotkeySettings
        {
            get { return _selectedHotkeySettings; }
            set
            {
                _selectedHotkeySettings = value;
                UpdateListViewItems(value);
            }
        }

        #endregion

        #region SelectedHotkeyCommand
        private HotkeyCommand? _selectedHotkeyCommand;
        private HotkeyCommand? SelectedHotkeyCommand
        {
            get { return _selectedHotkeyCommand; }
            set
            {
                _selectedHotkeyCommand = value;
                UpdateTextBox(value);
            }
        }
        #endregion

        public ControlHotkeys()
        {
            InitializeComponent();
            InitializeComplete();

            cmbSettings.DisplayMember = nameof(HotkeySettings.Name);
        }

        private IHotkeySettingsManager HotkeySettingsManager
        {
            get
            {
                if (this.FindAncestors().OfType<SettingsPageBase>().FirstOrDefault() is not SettingsPageBase settingsPage)
                {
                    throw new InvalidOperationException($"{GetType().Name} must be sited on a {typeof(SettingsPageBase)} control");
                }

                return settingsPage.ServiceProvider.GetRequiredService<IHotkeySettingsManager>();
            }
        }

        public void SaveSettings()
        {
            Validates.NotNull(Settings);

            HotkeySettingsManager.SaveSettings(Settings);
        }

        public void ReloadSettings()
        {
            Settings = HotkeySettingsManager.LoadSettings();
        }

        private void UpdateCombobox(IReadOnlyList<HotkeySettings>? settings)
        {
            SelectedHotkeySettings = null;

            cmbSettings.Items.Clear();
            if (settings is not null)
            {
                foreach (HotkeySettings setting in settings)
                {
                    cmbSettings.Items.Add(setting);
                }
            }
        }

        private void UpdateListViewItems(HotkeySettings? setting)
        {
            SelectedHotkeyCommand = null;

            listMappings.Items.Clear();
            if (setting?.Commands is not null)
            {
                foreach (HotkeyCommand cmd in setting.Commands)
                {
                    if (cmd is not null)
                    {
                        listMappings.Items.Add(
                            new ListViewItem(new[] { cmd.Name, cmd.KeyData.ToText() })
                            {
                                Tag = cmd
                            });
                    }
                }
            }
        }

        private void UpdateTextBox(HotkeyCommand? command)
        {
            txtHotkey.KeyData = command?.KeyData ?? Keys.None;
        }

        private void ControlHotkeys_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            ReloadSettings();
        }

        private void cmbSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedHotkeySettings = cmbSettings.SelectedItem as HotkeySettings;
        }

        private void listMappings_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem lvi = listMappings.SelectedItems.Count > 0 ? listMappings.SelectedItems[0] : null;
            if (lvi is not null)
            {
                HotkeyCommand hotkey = lvi.Tag as HotkeyCommand;
                SelectedHotkeyCommand = hotkey;
            }
        }

        private void bApply_Click(object sender, EventArgs e)
        {
            HotkeyCommand hotkey = SelectedHotkeyCommand;
            if (hotkey is not null)
            {
                // Update the KeyData with the chosen one
                hotkey.KeyData = txtHotkey.KeyData;

                // Refresh the ListView
                UpdateListViewItems(SelectedHotkeySettings);
            }
        }

        private void bClear_Click(object sender, EventArgs e)
        {
            HotkeyCommand hotkey = SelectedHotkeyCommand;
            if (hotkey is not null)
            {
                // Update the KeyData with the chosen one
                hotkey.KeyData = Keys.None;

                // Refresh the ListView
                UpdateListViewItems(SelectedHotkeySettings);
                txtHotkey.KeyData = hotkey.KeyData;
            }
        }

        private void bResetToDefaults_Click(object sender, EventArgs e)
        {
            Settings = HotkeySettingsManager.CreateDefaultSettings();
        }
    }
}
