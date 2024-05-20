using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUIPluginInterfaces;
using ResourceManager.Hotkey;
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
        private IReadOnlyList<HotkeyCommand>? _hotkeys;

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

#if !SUPPORT_THEME_HOOKS
            Load += (s, e) => ((Form)s!).FixVisualStyle();
#endif
        }

        /// <summary>
        /// Checks if the form wants to handle the key and executes that hotkey
        /// (without propagating an unhandled key to the base class function as in <cref>ProcessCmdKey</cref>).
        /// </summary>
        public virtual bool ProcessHotkey(Keys keyData)
        {
            if (IsDesignMode || !HotkeysEnabled)
            {
                return false;
            }

            HotkeyCommand? hotkey = _hotkeys?.FirstOrDefault(hotkey => hotkey?.KeyData == keyData);
            return hotkey is not null && ExecuteCommand(hotkey.CommandCode);
        }

        protected bool IsDesignMode => _initialiser.IsDesignMode;

        #region Hotkeys

        /// <summary>
        ///  Gets or sets a value that specifies if the hotkeys are used.
        /// </summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>
        ///  Gets the currently loaded hotkeys.
        /// </summary>
        protected IReadOnlyList<HotkeyCommand>? Hotkeys => _hotkeys;

        /// <summary>
        ///  Loads hotkeys for the specified configuration setting.
        /// </summary>
        /// <param name="hotkeySettingsName">The setting name.</param>
        protected void LoadHotkeys(string hotkeySettingsName)
        {
            _hotkeys = GetHotkeys(hotkeySettingsName);
        }

        /// <summary>
        ///  Loads hotkeys for the specified configuration setting.
        /// </summary>
        /// <param name="hotkeySettingsName">The setting name.</param>
        protected IReadOnlyList<HotkeyCommand> GetHotkeys(string hotkeySettingsName)
        {
            if (!HotkeysEnabled)
            {
                return [];
            }

            if (!TryGetUICommands(out IGitUICommands commands))
            {
                DebugHelpers.Fail($"{GetType().FullName}: service provider is unavailable.");
                return [];
            }

            return commands.GetRequiredService<IHotkeySettingsLoader>().LoadHotkeys(hotkeySettingsName);
        }

        /// <summary>Overridden: Checks if a hotkey wants to handle the key before letting the message propagate</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return ProcessHotkey(keyData) || base.ProcessCmdKey(ref msg, keyData);
        }

        protected Keys GetShortcutKeys<T>(T commandCode) where T : struct, Enum
            => _hotkeys.GetShortcutKey(commandCode);

        protected string GetShortcutKeyDisplayString<T>(T commandCode) where T : struct, Enum
            => _hotkeys.GetShortcutDisplay(commandCode);

        protected string GetShortcutKeyTooltipString<T>(T commandCode) where T : struct, Enum
            => _hotkeys.GetShortcutToolTip(commandCode);

        /// <summary>Override this method to handle form-specific Hotkey commands.</summary>
        protected virtual bool ExecuteCommand(int command)
        {
            return false;
        }

        #endregion

        /// <summary>
        ///  Attempts to find an instance of <see cref="IGitUICommands"/>.
        /// </summary>
        /// <param name="commands">
        ///  The instance of <see cref="IGitUICommands"/> directly assigned form
        ///  (if the form implements <see cref="IGitModuleForm"/>); <see langword="null"/>, otherwise.
        /// </param>
        /// <returns>
        ///  <see langword="true"/>, if an instance of <see cref="IGitUICommands"/> is found; <see langword="false"/>, otherwise.
        /// </returns>
        public bool TryGetUICommands([NotNullWhen(returnValue: true)] out IGitUICommands? commands)
        {
            if (this is IGitModuleForm control)
            {
                commands = control.UICommands;
                return commands is not null;
            }

            commands = null;
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            if (!IsDesignMode)
            {
                if (m.Msg == NativeMethods.WM_ACTIVATEAPP && m.WParam != IntPtr.Zero)
                {
                    OnApplicationActivated();
                    if (WindowState == FormWindowState.Minimized && Owner is null && AppSettings.WorkaroundActivateFromMinimize)
                    {
                        // Application occasionally requires explicit "restore" in Taskbar.
                        // See https://github.com/gitextensions/gitextensions/pull/10119.
                        Trace.WriteLine("WindowState is unexpectedly Minimized in OnApplicationActivated(), restoring.");
                        WindowState = FormWindowState.Normal;
                    }
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
            IDictionary<string, TranslationFile> translation = Translator.GetTranslation(AppSettings.CurrentTranslation);

            if (translation.Count == 0)
            {
                return;
            }

            (string itemName, object item)[] itemsToTranslate = new[] { (itemName, item) };

            foreach (KeyValuePair<string, TranslationFile> pair in translation)
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
