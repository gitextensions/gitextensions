using GitCommands;
using ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    abstract class MenuCommandsBase : ITranslate
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

        private IEnumerable<Tuple<string, object>> GetMenuCommandsForTranslationImpl()
        {
            return GetMenuCommandsForTranslation().Select(menu => new Tuple<string, object>(menu.Name, menu));
        }
    }
}
