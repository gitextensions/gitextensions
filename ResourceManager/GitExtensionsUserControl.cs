using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace ResourceManager
{
    /// <summary>Provides translation and hotkey plumbing for GitEx <see cref="UserControl"/>s.</summary>
    public class GitExtensionsControl : UserControl, ITranslate
    {
        public GitExtensionsControl()
        {
            Font = AppSettings.Font;

            Load += GitExtensionsControl_Load;
        }

        public int GetCurrentDeviceDpi()
        {
#if TARGETING_DOTNET47
            int deviceDpi = DeviceDpi;
#else
            int deviceDpi;
            using (Graphics g = CreateGraphics())
            {
                deviceDpi = (int)g.DpiX;
            }
#endif
            return deviceDpi;
        }

        [Browsable(false)] // because we always read from settings
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        private static bool CheckComponent(object value)
        {
            bool isComponentInDesignMode = false;
            var component = value as IComponent;
            ISite site = component?.Site;
            if (site != null && site.DesignMode)
                isComponentInDesignMode = true;

            return isComponentInDesignMode;
        }

        protected virtual void OnRuntimeLoad(EventArgs e)
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!CheckComponent(this))
                OnRuntimeLoad(e);
        }

        private bool translated;

        private void GitExtensionsControl_Load(object sender, EventArgs e)
        {
            // find out if the value is a component and is currently in design mode
            bool isComponentInDesignMode = CheckComponent(this);

            if (!translated && !isComponentInDesignMode)
                throw new Exception("The control " + GetType().Name + " is not translated in the constructor. You need to call Translate() right after InitializeComponent().");
        }

        /// <summary>Translates the <see cref="UserControl"/>'s elements.</summary>
        protected void Translate()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
            translated = true;
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
                return;
            foreach (var pair in translation)
            {
                IEnumerable<Tuple<string, object>> itemsToTranslate = new[] { new Tuple<string, object>(itemName, item) };
                TranslationUtils.TranslateItemsFromList(Name, pair.Value, itemsToTranslate);
            }
        }

        #region Hotkeys

        /// <summary>Gets or sets a value that specifies if the hotkeys are used</summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>Gets or sets the hotkeys</summary>
        protected IEnumerable<HotkeyCommand> Hotkeys { get; set; }

        /// <summary>Checks if a hotkey wants to handle the key before letting the message propagate.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HotkeysEnabled && Hotkeys != null)
                foreach (var hotkey in Hotkeys)
                {
                    if (hotkey != null && hotkey.KeyData == keyData)
                    {
                        return ExecuteCommand(hotkey.CommandCode);
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

        /// <summary>
        /// Override this method to handle form-specific Hotkey commands.
        /// <remarks>This base method does nothing and returns false.</remarks>
        /// </summary>
        /// <param name="command"></param>
        protected virtual bool ExecuteCommand(int command)
        {
            return false;
        }

        #endregion
    }
}
