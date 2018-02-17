﻿using System.Drawing;
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
        private Keys _KeyData;
        /// <summary>Gets or sets the KeyData</summary>
        public Keys KeyData
        {
            get => _KeyData;
            set
            {
                _KeyData = value;
                //TODO: do not change text color on already assigned keys, which occur only once
                ForeColor = (HotkeySettingsManager.IsUniqueKey(_KeyData)) ? Color.Red : Color.Black;
                Text = value.ToText() ?? _hotkeyNotSet.Text;
            }
        }
        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // We don't want only a modifier key pressed
            // TODO Further restrict the allowed keys
            if (!keyData.GetKeyCode().IsModifierKey())
                KeyData = keyData;

            // Swallow all keys
            return true;
        }

    }
}
