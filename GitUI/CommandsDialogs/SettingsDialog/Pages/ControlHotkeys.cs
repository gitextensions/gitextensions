using System;
using System.Windows.Forms;
using GitUI.Hotkey;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    /// <summary>
    /// ControlHotkeys enables editing of HotkeySettings
    /// </summary>
    public partial class ControlHotkeys : GitExtensionsControl
    {
        #region Properties

        #region Settings
        private HotkeySettings[] _Settings;
        private HotkeySettings[] Settings
        {
            get { return _Settings; }
            set
            {
                _Settings = value;
                UpdateCombobox(value);
            }
        }
        #endregion

        #region SelectedHotkeySettings
        private HotkeySettings _SelectedHotkeySettings;
        private HotkeySettings SelectedHotkeySettings
        {
            get { return _SelectedHotkeySettings; }
            set
            {
                _SelectedHotkeySettings = value;
                UpdateListViewItems(value);
            }
        }

        #endregion

        #region SelectedHotkeyCommand
        private HotkeyCommand _SelectedHotkeyCommand;
        private HotkeyCommand SelectedHotkeyCommand
        {
            get { return _SelectedHotkeyCommand; }
            set
            {
                _SelectedHotkeyCommand = value;
                UpdateTextBox(value);
            }
        }
        #endregion

        #endregion

        public ControlHotkeys()
        {
            InitializeComponent();
            Translate();
        }

        #region Methods

        public void SaveSettings()
        {
            HotkeySettingsManager.SaveSettings(this.Settings);
        }

        public void ReloadSettings()
        {
            this.Settings = HotkeySettingsManager.LoadSettings();
        }

        private void UpdateCombobox(HotkeySettings[] settings)
        {
            this.SelectedHotkeySettings = null;

            this.cmbSettings.Items.Clear();
            if (settings != null)
                foreach (var setting in settings)
                    cmbSettings.Items.Add(setting);
        }

        private void UpdateListViewItems(HotkeySettings setting)
        {
            this.SelectedHotkeyCommand = null;

            this.listMappings.Items.Clear();
            if (setting != null)
                foreach (var cmd in setting.Commands)
                {
                    if (cmd != null)
                        this.listMappings.Items.Add(new ListViewItem(new[] { cmd.Name, cmd.KeyData.ToText() }) { Tag = cmd });
                }
        }

        private void UpdateTextBox(HotkeyCommand command)
        {
            txtHotkey.KeyData = (command != null) ? command.KeyData : Keys.None;
        }

        private void ControlHotkeys_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;
            ReloadSettings();
        }

        private void cmbSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedHotkeySettings = this.cmbSettings.SelectedItem as HotkeySettings;
        }

        private void listMappings_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lvi = this.listMappings.SelectedItems.Count > 0 ? this.listMappings.SelectedItems[0] : null;
            if (lvi != null)
            {
                var hotkey = lvi.Tag as HotkeyCommand;
                this.SelectedHotkeyCommand = hotkey;
            }
        }

        private void bApply_Click(object sender, EventArgs e)
        {
            var hotkey = this.SelectedHotkeyCommand;
            if (hotkey != null)
            {
                // Update the KeyData with the chosen one
                hotkey.KeyData = txtHotkey.KeyData;

                // Refresh the ListView
                UpdateListViewItems(this.SelectedHotkeySettings);
            }
        }

        private void bClear_Click(object sender, EventArgs e)
        {
            var hotkey = this.SelectedHotkeyCommand;
            if (hotkey != null)
            {
                // Update the KeyData with the chosen one
                hotkey.KeyData = Keys.None;
                // Refresh the ListView
                UpdateListViewItems(this.SelectedHotkeySettings);
                txtHotkey.KeyData = hotkey.KeyData;
            }
        }

        private void bResetToDefaults_Click(object sender, EventArgs e)
        {
            this.Settings = HotkeySettingsManager.CreateDefaultSettings();
        }

        #endregion
    }
}
