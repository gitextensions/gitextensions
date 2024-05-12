using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;

namespace GitExtensions.Extensibility.Plugins;

public abstract class GitPluginBase : IGitPlugin, ITranslate
{
    public Guid Id { get; protected set; }

    public string? Description { get; protected set; }
    public string? Name { get; protected set; }
    public Image? Icon { get; protected set; }

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
    }

    void IDisposable.Dispose()
    {
    }

    public bool HasSettings { get; }

    // Store settings to use later
    public SettingsSource Settings
    {
        get
        {
            ArgumentNullException.ThrowIfNull(SettingsContainer);
            return SettingsContainer.GetSettingsSource();
        }
    }

    public IGitPluginSettingsContainer? SettingsContainer { get; set; }

    public virtual IEnumerable<ISetting> GetSettings()
    {
        return Enumerable.Empty<ISetting>();
    }

    public virtual void Register(IGitUICommands gitUiCommands)
    {
        ArgumentNullException.ThrowIfNull(SettingsContainer);
        SettingsContainer.SetSettingsSource(gitUiCommands.Module.GetEffectiveSettings());
    }

    public virtual void Unregister(IGitUICommands gitUiCommands)
    {
        ArgumentNullException.ThrowIfNull(SettingsContainer);
        SettingsContainer.SetSettingsSource(null);
    }

    /// <summary>
    /// Run the plugin Execute method
    /// </summary>
    /// <param name="args">arguments from the UI</param>
    /// <returns>true, if the revision grid need a refresh
    /// false, otherwise </returns>
    public abstract bool Execute(GitUIEventArgs args);

    protected void Translate(string currentTranslation)
    {
        // Description for old plugin setting processing as key
        Description = Name;
        Translator.Translate(this, currentTranslation);
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
        TranslationUtils.TranslateProperty(name, this, nameof(Name), translation);
        TranslationUtils.TranslateItemsFromFields(name, this, translation);
    }
}
