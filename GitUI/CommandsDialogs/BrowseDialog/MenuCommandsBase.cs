using System;
using System.Collections.Generic;
using GitCommands;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    internal abstract class MenuCommandsBase : ITranslate
    {
        // for translation category
        protected abstract string TranslationCategoryName { get; }

        void IDisposable.Dispose()
        {
        }

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
            foreach (MenuCommand menuCommand in GetMenuCommandsForTranslation())
            {
                Validates.NotNull(menuCommand.Name);
                yield return (menuCommand.Name, menuCommand);
            }
        }
    }
}
