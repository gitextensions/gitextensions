using System.Collections.Generic;
using System.Linq;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    internal abstract class MenuCommandsBase : ITranslate
    {
        // for translation category
        protected string TranslationCategoryName { get; set; }

        public void Translate()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(TranslationCategoryName, this, translation);
            TranslationUtils.AddTranslationItemsFromList(TranslationCategoryName, translation, GetMenuCommandsForTranslationImpl());
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(TranslationCategoryName, this, translation);
            TranslationUtils.TranslateItemsFromList(TranslationCategoryName, translation, GetMenuCommandsForTranslationImpl());
        }

        // override and return all commands created by extending class
        protected abstract IEnumerable<MenuCommand> GetMenuCommandsForTranslation();

        private IEnumerable<(string name, object item)> GetMenuCommandsForTranslationImpl()
        {
            return GetMenuCommandsForTranslation().Select(menu => (menu.Name, (object)menu));
        }
    }
}
