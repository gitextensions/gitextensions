using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.Settings;
using GitUIPluginInterfaces;

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
        IReadOnlyList<ExternalLinkDefinition> Get(RepoDistSettings settings);
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
        public IReadOnlyList<ExternalLinkDefinition> Get(RepoDistSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            ISettingsSource[] cachedSettingsList = settings.Split();
            IEnumerable<ExternalLinkDefinition> effective = Enumerable.Empty<ExternalLinkDefinition>();

            foreach (ISettingsSource cachedSettings in cachedSettingsList)
            {
                IReadOnlyList<ExternalLinkDefinition> definitions = _externalLinksStorage.Load(cachedSettings);

                effective = effective
                    .Union(definitions);
            }

            return effective
                .ToList();
        }
    }
}
