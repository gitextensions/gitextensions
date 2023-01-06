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
