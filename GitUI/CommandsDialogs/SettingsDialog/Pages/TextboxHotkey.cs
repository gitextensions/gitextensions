using System.Windows.Forms;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public class TextboxHotkey : TextBox
    {
        private readonly TranslationString _hotkeyNotSet =
            new TranslationString("None");

        #region Key
        private Keys _keyData;

        /// <summary>Gets or sets the KeyData</summary>
        public Keys KeyData
        {
            get { return _keyData; }
            set
            {
                _keyData = value;

                // TODO: do not change text color on already assigned keys, which occur only once
                ForeColor = HotkeySettingsManager.IsUniqueKey(_keyData) ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                Text = _keyData.ToText();
            }
        }
        #endregion

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
