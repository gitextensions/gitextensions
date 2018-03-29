using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI;

namespace ResourceManager
{
    public class GitExtensionsFormBase : Form, ITranslate
    {
        /// <summary>indicates whether the <see cref="Form"/> has been translated</summary>
        private bool _translated;

        /// <summary>Creates a new <see cref="GitExtensionsFormBase"/> indicating position restore.</summary>
        public GitExtensionsFormBase()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SetFont();

            ShowInTaskbar = Application.OpenForms.Count <= 0;

            Load += GitExtensionsFormLoad;
        }

        /// <summary>Gets or sets a value that specifies if the hotkeys are used</summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>Gets or sets the hotkeys</summary>
        protected IEnumerable<HotkeyCommand> Hotkeys { get; set; }

        /// <summary>Overridden: Checks if a hotkey wants to handle the key before letting the message propagate</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HotkeysEnabled && Hotkeys != null)
            {
                foreach (var hotkey in Hotkeys)
                {
                    if (hotkey != null && hotkey.KeyData == keyData)
                    {
                        return ExecuteCommand(hotkey.CommandCode);
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected Keys GetShortcutKeys(int commandCode)
        {
            return GetHotkeyCommand(commandCode)?.KeyData ?? Keys.None;
        }

        protected HotkeyCommand GetHotkeyCommand(int commandCode)
        {
            return Hotkeys?.FirstOrDefault(h => h.CommandCode == commandCode);
        }

        /// <summary>Override this method to handle form-specific Hotkey commands.</summary>
        protected virtual bool ExecuteCommand(int command)
        {
            return false;
        }

        protected void SetFont()
        {
            Font = AppSettings.Font;
        }

        /// <summary>Indicates whether this is a valid <see cref="IComponent"/> running in design mode.</summary>
        protected static bool CheckComponent(object value)
        {
            if (value is IComponent component)
            {
                return component.Site != null && component.Site.DesignMode;
            }

            return false;
        }

        /// <summary>Sets <see cref="AutoScaleMode"/>,
        /// restores position, raises the <see cref="Form.Load"/> event,
        /// and .
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            AutoScaleMode = AppSettings.EnableAutoScale
                ? AutoScaleMode.Dpi
                : AutoScaleMode.None;
            base.OnLoad(e);
        }

        protected void GitExtensionsFormLoad(object sender, EventArgs e)
        {
            // find out if the value is a component and is currently in design mode
            var isComponentInDesignMode = CheckComponent(this);

            if (!_translated && !isComponentInDesignMode)
            {
                throw new Exception("The control " + GetType().Name +
                                    " is not translated in the constructor. You need to call Translate() right after InitializeComponent().");
            }
        }

        /// <summary>Translates the <see cref="Form"/>'s fields and properties, including child controls.</summary>
        protected void Translate()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
            _translated = true;
        }

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(Name, this, translation);
        }

        protected void TranslateItem(string itemName, object item)
        {
            var translation = Translator.GetTranslation(AppSettings.CurrentTranslation);

            if (translation.Count == 0)
            {
                return;
            }

            var itemsToTranslate = new[] { (itemName, item) };

            foreach (var pair in translation)
            {
                TranslationUtils.TranslateItemsFromList(Name, pair.Value, itemsToTranslate);
            }
        }
    }
}