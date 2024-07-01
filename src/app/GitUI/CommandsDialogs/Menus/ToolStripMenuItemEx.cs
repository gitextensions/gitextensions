using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using ResourceManager;

namespace GitUI.CommandsDialogs.Menus
{
    internal abstract class ToolStripMenuItemEx : ToolStripMenuItem, ITranslate
    {
        private Func<IGitUICommands>? _getUICommands;

        /// <summary>
        ///  Gets the current instance of the UI commands.
        /// </summary>
        protected IGitUICommands UICommands
            => (_getUICommands ?? throw new InvalidOperationException("The button is not initialized"))();

        /// <summary>
        ///  Gets the current instance of the git module.
        /// </summary>
        protected IGitModule Module => UICommands.Module;

        /// <summary>
        ///  Gets the form that is displaying the menu item.
        /// </summary>
        protected static Form? OwnerForm
            => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

        /// <summary>
        ///  Initializes the menu item.
        /// </summary>
        /// <param name="getUICommands">The method that returns the current instance of UI commands.</param>
        public void Initialize(Func<IGitUICommands> getUICommands)
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);

            _getUICommands = getUICommands;

            OnInitialized();
        }

        /// <summary>
        ///  Allows the menu item to perform any initialization logic.
        /// </summary>
        public virtual void OnInitialized()
        {
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
