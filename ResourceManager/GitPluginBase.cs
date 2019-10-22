using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;
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

        protected GitPluginBase(bool hasSettings)
        {
            HasSettings = hasSettings;
        }

        // required for the TranslationApp to work
        protected GitPluginBase()
            : this(false)
        {
        }

        protected void SetNameAndDescription(string name)
        {
            Name = name;
            Description = name;
        }

        public bool HasSettings { get; }

        // Store settings to use later
        public ISettingsSource Settings => SettingsContainer.GetSettingsSource();

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public virtual IEnumerable<ISetting> GetSettings()
        {
            return Enumerable.Empty<ISetting>();
        }

        public virtual void Register(IGitUICommands gitUiCommands)
        {
            SettingsContainer.SetSettingsSource(gitUiCommands.GitModule.GetEffectiveSettings());
        }

        public virtual void Unregister(IGitUICommands gitUiCommands)
        {
            SettingsContainer.SetSettingsSource(null);
        }

        /// <summary>
        /// Run the plugin Execute method
        /// </summary>
        /// <param name="args">arguments from the UI</param>
        /// <returns>true, if the revision grid need a refresh
        /// false, otherwise </returns>
        public abstract bool Execute(GitUIEventArgs args);

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
