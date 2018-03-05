using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace ResourceManager
{
    public abstract class GitPluginBase : IGitPlugin, ITranslate
    {
        public string Description { get; protected set; }
        public string Name { get; protected set; }

        protected void SetNameAndDescription(string aName)
        {
            Name = aName;
            Description = aName;
        }

        // Store settings to use later
        public ISettingsSource Settings => SettingsContainer.GetSettingsSource();

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public virtual IEnumerable<ISetting> GetSettings()
        {
            return new List<ISetting>();
        }

        public virtual void Register(IGitUICommands gitUiCommands)
        {
            SettingsContainer.SetSettingsSource(gitUiCommands.GitModule.GetEffectiveSettings());
        }

        public virtual void Unregister(IGitUICommands gitUiCommands)
        {
            SettingsContainer.SetSettingsSource(null);
        }

        public abstract bool Execute(GitUIBaseEventArgs gitUiCommands);

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
