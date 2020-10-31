using System;
using System.Drawing;
using GitCommands;
using GitExtensions.Core.Settings;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace ResourceManager
{
    [UsedImplicitly]
    public abstract class GitPluginBase : IGitPlugin, ITranslate
    {
        public string Description { get; protected set; }
        public string Name { get; protected set; }
        public Image Icon { get; protected set; }

        protected void SetNameAndDescription(string name)
        {
            Name = name;
            Description = name;
        }

        void IDisposable.Dispose()
        {
        }

        // Store settings to use later
        public ISettingsSource Settings => SettingsContainer.GetSettingsSource();

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        protected void Translate()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public virtual void AddTranslationItems(ITranslation translation)
        {
            string name = GetType().Name;
            TranslationUtils.AddTranslationItem(name, this, "Description", translation);
            TranslationUtils.AddTranslationItemsFromFields(name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            string name = GetType().Name;
            TranslationUtils.TranslateProperty(name, this, "Description", translation);
            TranslationUtils.TranslateItemsFromFields(name, this, translation);
        }
    }
}
