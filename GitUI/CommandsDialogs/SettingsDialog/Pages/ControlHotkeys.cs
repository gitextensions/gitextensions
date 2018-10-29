using System;
using System.Windows.Forms;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    /// <summary>
    /// ControlHotkeys enables editing of HotkeySettings
    /// </summary>
    public partial class ControlHotkeys : GitExtensionsControl
    {
        #region Settings
        private HotkeySettings[] _settings;
        private HotkeySettings[] Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                UpdateCombobox(value);
            }
        }
        #endregion

        #region SelectedHotkeySettings
        private HotkeySettings _selectedHotkeySettings;
        private HotkeySettings SelectedHotkeySettings
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
        private HotkeyCommand _selectedHotkeyCommand;
        private HotkeyCommand SelectedHotkeyCommand
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

        public void SaveSettings()
        {
            HotkeySettingsManager.SaveSettings(Settings);
        }

        public void ReloadSettings()
        {
            Settings = HotkeySettingsManager.LoadSettings();
        }

        private void UpdateCombobox(HotkeySettings[] settings)
        {
            SelectedHotkeySettings = null;

            cmbSettings.Items.Clear();
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    cmbSettings.Items.Add(setting);
                }
            }
        }

        private void UpdateListViewItems(HotkeySettings setting)
        {
            SelectedHotkeyCommand = null;

            listMappings.Items.Clear();
            if (setting != null)
            {
                foreach (var cmd in setting.Commands)
                {
                    if (cmd != null)
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

        private void UpdateTextBox(HotkeyCommand command)
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
            var lvi = listMappings.SelectedItems.Count > 0 ? listMappings.SelectedItems[0] : null;
            if (lvi != null)
            {
                var hotkey = lvi.Tag as HotkeyCommand;
                SelectedHotkeyCommand = hotkey;
            }
        }

        private void bApply_Click(object sender, EventArgs e)
        {
            var hotkey = SelectedHotkeyCommand;
            if (hotkey != null)
            {
                // Update the KeyData with the chosen one
                hotkey.KeyData = txtHotkey.KeyData;

                // Refresh the ListView
                UpdateListViewItems(SelectedHotkeySettings);
            }
        }

        private void bClear_Click(object sender, EventArgs e)
        {
            var hotkey = SelectedHotkeyCommand;
            if (hotkey != null)
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
