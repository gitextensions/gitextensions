using GitUI.Hotkey;
using GitUIPluginInterfaces;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public class TextboxHotkey : TextBox
    {
        private Keys _keyData;

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

        /// <summary>Gets or sets the KeyData.</summary>
        public Keys KeyData
        {
            get => _keyData;
            set
            {
                if (_keyData == value)
                {
                    return;
                }

                _keyData = value;

                if (_keyData != Keys.None)
                {
                    // TODO: do not change text color on already assigned keys, which occur only once
                    ForeColor = HotkeySettingsManager.IsUniqueKey(_keyData) ? Color.Red : SystemColors.WindowText;
                }

                Text = _keyData.ToText();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // We don't want only a modifier key pressed
            // TODO Further restrict the allowed keys
            if (!keyData.GetKeyCode().IsModifierKey())
            {
                KeyData = keyData;
            }

            // Swallow all keys
            return true;
        }
    }
}
