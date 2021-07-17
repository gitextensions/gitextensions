using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using ResourceManager.Properties;

namespace ResourceManager
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>
    /// Base class for all Git Extensions forms.
    /// </summary>
    /// <remarks>
    /// Deriving from this class requires a call to <see cref="InitializeComplete"/> at
    /// the end of the constructor. Omitting this call with result in a runtime exception.
    /// </remarks>
    public class GitExtensionsFormBase : Form, ITranslate
    {
        private readonly GitExtensionsControlInitialiser _initialiser;

        /// <summary>Creates a new <see cref="GitExtensionsFormBase"/> indicating position restore.</summary>
        public GitExtensionsFormBase()
        {
            _initialiser = new GitExtensionsControlInitialiser(this);

            if (IsDesignMode)
            {
                return;
            }

            ShowInTaskbar = Application.OpenForms.Count <= 0;
            Icon = Resources.GitExtensionsLogoIcon;
        }

        protected bool IsDesignMode => _initialiser.IsDesignMode;

        #region Hotkeys

        /// <summary>Gets or sets a value that specifies if the hotkeys are used</summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>Gets or sets the hotkeys</summary>
        protected IEnumerable<HotkeyCommand>? Hotkeys { get; set; }

        /// <summary>Overridden: Checks if a hotkey wants to handle the key before letting the message propagate</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!IsDesignMode && HotkeysEnabled && Hotkeys is not null)
            {
                foreach (var hotkey in Hotkeys)
                {
                    if (hotkey is not null && hotkey.KeyData == keyData)
                    {
                        return ExecuteCommand(hotkey.CommandCode).Executed;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected Keys GetShortcutKeys(int commandCode)
        {
            return GetHotkeyCommand(commandCode)?.KeyData ?? Keys.None;
        }

        protected HotkeyCommand? GetHotkeyCommand(int commandCode)
        {
            return Hotkeys?.FirstOrDefault(h => h.CommandCode == commandCode);
        }

        /// <summary>Override this method to handle form-specific Hotkey commands.</summary>
        protected virtual CommandStatus ExecuteCommand(int command)
        {
            return false;
        }

        #endregion

        protected override void WndProc(ref Message m)
        {
            if (!IsDesignMode)
            {
                if (m.Msg == NativeMethods.WM_ACTIVATEAPP && m.WParam != IntPtr.Zero)
                {
                    OnApplicationActivated();
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>Performs post-initialisation tasks such as translation and DPI scaling.</summary>
        /// <remarks>
        /// <para>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</para>
        /// <para>Requiring this extra life-cycle event allows preparing the UI after any call to <c>InitializeComponent</c>,
        /// but before it is show. Both the <see cref="Form.Load"/> and <see cref="Form.Shown"/> events occur too late for
        /// operations that effect layout.</para>
        /// </remarks>
        protected void InitializeComplete()
        {
            _initialiser.InitializeComplete();

            if (IsDesignMode)
            {
                return;
            }

            AutoScaleMode = AppSettings.EnableAutoScale
                ? AutoScaleMode.Dpi
                : AutoScaleMode.None;

            this.AdjustForDpiScaling();
            this.EnableRemoveWordHotkey();
        }

        #region Translation

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

        #endregion

        /// <summary>
        /// Notifies whenever the application becomes active.
        /// </summary>
        protected virtual void OnApplicationActivated()
        {
        }
    }
}
