using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI.Theming;

namespace ResourceManager
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Provides translation and hotkey plumbing for GitEx <see cref="UserControl"/>s.</summary>
    public class GitExtensionsControl : UserControl, ITranslate
    {
        private readonly GitExtensionsControlInitialiser _initialiser;

        protected GitExtensionsControl()
        {
            _initialiser = new GitExtensionsControlInitialiser(this);
        }

        [Browsable(false)] // because we always read from settings
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        protected bool IsDesignMode => _initialiser.IsDesignMode;

        protected virtual void OnRuntimeLoad()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsDesignMode)
            {
                OnRuntimeLoad();
            }
        }

        /// <summary>Performs post-initialisation tasks such as translation.</summary>
        /// <remarks>
        /// <para>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</para>
        /// <para>Requiring this extra life-cycle event allows preparing the UI after any call to <c>InitializeComponent</c>,
        /// but before it is show. The <see cref="UserControl.Load"/> event occurs too late for operations that effect layout.</para>
        /// </remarks>
        protected void InitializeComplete()
        {
            _initialiser.InitializeComplete();

            if (IsDesignMode)
            {
                return;
            }

            this.FixVisualStyle();
        }

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(Name, this, translation);
        }

        #region Hotkeys

        /// <summary>Gets or sets a value that specifies if the hotkeys are used</summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>Gets or sets the hotkeys</summary>
        protected IEnumerable<HotkeyCommand>? Hotkeys { get; set; }

        /// <summary>Checks if a hotkey wants to handle the key before letting the message propagate.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HotkeysEnabled && Hotkeys is not null)
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

        private HotkeyCommand? GetHotkeyCommand(int commandCode)
        {
            return Hotkeys?.FirstOrDefault(h => h.CommandCode == commandCode);
        }

        /// <summary>
        /// Override this method to handle form-specific Hotkey commands.
        /// <remarks>This base method does nothing and returns a <see cref="GitCommands.CommandStatus"/>
        /// with <see cref="GitCommands.CommandStatus.Executed"/> set to <see langword="false"/></remarks>
        /// </summary>
        protected virtual GitCommands.CommandStatus ExecuteCommand(int command)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the given [combination of] key[s] represents a keypress which is used for text input by default.
        /// <remarks>Can be used to ignore hotkeys which would prevent from typing text into an input control if it's focused.</remarks>
        /// </summary>
        /// <param name="multiLine">Should be set to true for multi-line input controls to match keys for vertical movement, too.</param>
        public static bool IsTextEditKey(Keys keys, bool multiLine = false)
        {
            keys &= ~Keys.Shift; // ignore the SHIFT key as modifier
            switch (keys)
            {
                case Keys key when key is
                    (>= Keys.A and <= Keys.Z)
                    or (>= Keys.D0 and <= Keys.D9)
                    or (>= Keys.Oem1 and <= Keys.Oem102):
                case Keys.Space:
                case Keys.Back:
                case Keys.Delete:
                case Keys.Insert:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                    return true;

                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    return multiLine;
            }

            return false;
        }

        #endregion
    }
}
