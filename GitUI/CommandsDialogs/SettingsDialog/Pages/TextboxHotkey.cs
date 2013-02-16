using System.Windows.Forms;
using GitUI.Hotkey;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public class TextboxHotkey : TextBox
    {
        #region Key
        private Keys _KeyData;
        /// <summary>Gets or sets the KeyData</summary>
        public Keys KeyData
        {
            get { return _KeyData; }
            set
            {
                _KeyData = value;
                //TODO: do not change text color on already assigned keys, which occur only once
                this.ForeColor = (HotkeySettingsManager.IsUniqueKey(_KeyData)) ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                this.Text = value.ToText();
            }
        }
        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // We don't want only a modifier key pressed
            // TODO Further restrict the allowed keys
            if (!keyData.GetKeyCode().IsModifierKey())
                this.KeyData = keyData;

            // Swallow all keys
            return true;
        }

    }
}
