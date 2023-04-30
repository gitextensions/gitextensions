using GitCommands;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI.CommandsDialogs.Menus
{
    internal abstract class ToolStripMenuItemEx : ToolStripMenuItem, ITranslate
    {
        private Func<GitUICommands>? _getUICommands;

        /// <summary>
        ///  Gets the current instance of the UI commands.
        /// </summary>
        protected GitUICommands UICommands
            => (_getUICommands ?? throw new InvalidOperationException("The button is not initialized")).Invoke();

        /// <summary>
        ///  Gets the current instance of the git module.
        /// </summary>
        protected GitModule Module
            => UICommands.Module;

        /// <summary>
        ///  Gets the form that is displaying the menu item.
        /// </summary>
        protected static Form? OwnerForm
            => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

        /// <summary>
        ///  Initializes the menu item.
        /// </summary>
        /// <param name="getUICommands">The method that returns the current instance of UI commands.</param>
        public void Initialize(Func<GitUICommands> getUICommands)
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);

            _getUICommands = getUICommands;
        }

        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        protected static string GetShortcutKey(IEnumerable<HotkeyCommand>? hotkeys, int commandCode)
        {
            return (hotkeys?.FirstOrDefault(h => h.CommandCode == commandCode)?.KeyData ?? Keys.None).ToShortcutKeyDisplayString();
        }

        /// <summary>
        ///  Allows reloading/reassigning the configured shortcut key.
        /// </summary>
        //// <param name="hotkeys"></param>
        public virtual void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        {
        }

        /// <summary>
        ///  Allows refreshing the state of the menu item depending on the state of the loaded git repository.
        /// </summary>
        /// <param name="bareRepository"><see lang="true"/> if the current git repository is bare; otherwise, <see lang="false"/>.</param>
        public virtual void RefreshState(bool bareRepository)
        {
        }

        void ITranslate.AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields("FormBrowse", this, translation);
        }

        void ITranslate.TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields("FormBrowse", this, translation);
        }
    }
}
