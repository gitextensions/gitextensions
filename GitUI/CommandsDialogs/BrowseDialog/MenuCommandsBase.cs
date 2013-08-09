using GitCommands;
using ResourceManager.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    abstract class MenuCommandsBase : ITranslate
    {
        //for translation category
        protected string TranslationCategoryName { get; set; }

        public void Translate()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public virtual void AddTranslationItems(Translation translation)
        {
            TranslationUtl.AddTranslationItemsFromFields(TranslationCategoryName, this, translation);
            TranslationUtl.AddTranslationItemsFromList(TranslationCategoryName, translation, GetMenuCommandsForTranslationImpl());
        }

        public virtual void TranslateItems(Translation translation)
        {
            TranslationUtl.TranslateItemsFromFields(TranslationCategoryName, this, translation);
            TranslationUtl.TranslateItemsFromList(TranslationCategoryName, translation, GetMenuCommandsForTranslationImpl());
        }

        //override and return all commands created by extending class
        protected abstract IEnumerable<MenuCommand> GetMenuCommandsForTranslation();

        private IEnumerable<Tuple<string, object>> GetMenuCommandsForTranslationImpl()
        {
            return GetMenuCommandsForTranslation().Select(menu => new Tuple<string, object>(menu.Name, menu));
        }
    }
}
