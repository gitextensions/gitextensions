using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using ResourceManager.Translation;
using System.Collections.Generic;

namespace GitUI
{
    public class GitExtensionsControl : UserControl
    {
        public GitExtensionsControl()
        {
            Font = SystemFonts.MessageBoxFont;

            Load += GitExtensionsControl_Load;
        }

        private bool translated;

        private static bool CheckComponent(object value)
        {
            bool isComponentInDesignMode = false;
            var component = value as IComponent;
            if (component != null)
            {
                ISite site = component.Site;
                if ((site != null) && site.DesignMode)
                    isComponentInDesignMode = true;
            }

            return isComponentInDesignMode;
        }

        void GitExtensionsControl_Load(object sender, EventArgs e)
        {
            // find out if the value is a component and is currently in design mode
            bool isComponentInDesignMode = CheckComponent(this);

            if (!translated && !isComponentInDesignMode)
                throw new Exception("The control " + GetType().Name + " is not translated in the constructor. You need to call Translate() right after InitializeComponent().");
        }

        protected void Translate()
        {
            var translator = new Translator(GitCommands.Settings.Translation);
            translator.TranslateControl(this);
            translated = true;
        }

        #region Hotkeys

        /// <summary>Gets or sets a value that specifies if the hotkeys are used</summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>Gets or sets the hotkeys</summary>
        protected IEnumerable<Hotkey.HotkeyCommand> Hotkeys { get; set; }

        /// <summary>Overridden: Checks if a hotkey wants to handle the key before letting the message propagate</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HotkeysEnabled && this.Hotkeys != null)
                foreach (var hotkey in this.Hotkeys)
                {
                    if (hotkey.KeyData == keyData)
                    {
                        return ExecuteCommand(hotkey.CommandCode);
                    }
                }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Override this method to handle form specific Hotkey commands
        /// This base method does nothing
        /// </summary>
        /// <param name="command"></param>
        protected virtual bool ExecuteCommand(int command)
        {
            return false;
        }

        #endregion
    }
}
