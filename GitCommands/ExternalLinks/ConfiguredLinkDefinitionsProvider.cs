using GitCommands.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitCommands.ExternalLinks
{
    /// <summary>
    /// Provides the ability to retrieves available persisted external link definitions.
    /// </summary>
    public interface IConfiguredLinkDefinitionsProvider
    {
        /// <summary>
        /// Loads all persisted external link definitions across all setting layers.
        /// </summary>
        IReadOnlyList<ExternalLinkDefinition> Get(DistributedSettings settings);
    }

    /// <summary>
    /// Retrieves available persisted external link definitions.
    /// </summary>
    public sealed class ConfiguredLinkDefinitionsProvider : IConfiguredLinkDefinitionsProvider
    {
        private readonly IExternalLinksStorage _externalLinksStorage;

        public ConfiguredLinkDefinitionsProvider(IExternalLinksStorage externalLinksStorage)
        {
            _externalLinksStorage = externalLinksStorage;
        }

        /// <summary>
        /// Loads all persisted external link definitions across all setting layers.
        /// </summary>
        public IReadOnlyList<ExternalLinkDefinition> Get(DistributedSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            DistributedSettings cachedSettings = new(null, settings.SettingsCache, SettingLevel.Unknown);
            IEnumerable<ExternalLinkDefinition>? effective = _externalLinksStorage.Load(cachedSettings);

            Validates.NotNull(effective);

            if (settings.LowerPriority is not null)
            {
                ConfiguredLinkDefinitionsProvider lowerPriorityLoader = new(_externalLinksStorage);
                effective = effective.Union(lowerPriorityLoader.Get(settings.LowerPriority));
            }

            return effective.ToList();
        }
    }
}
