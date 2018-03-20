using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.Settings;

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
        private readonly IExternalLinksLoader _externalLinksLoader;

        public ConfiguredLinkDefinitionsProvider(IExternalLinksLoader externalLinksLoader)
        {
            _externalLinksLoader = externalLinksLoader;
        }

        /// <summary>
        /// Loads all persisted external link definitions across all setting layers.
        /// </summary>
        public IReadOnlyList<ExternalLinkDefinition> Get(RepoDistSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var cachedSettings = new RepoDistSettings(null, settings.SettingsCache);
            IEnumerable<ExternalLinkDefinition> effective = _externalLinksLoader.Load(cachedSettings);

            if (settings.LowerPriority != null)
            {
                var lowerPriorityLoader = new ConfiguredLinkDefinitionsProvider(_externalLinksLoader);
                effective = effective.Union(lowerPriorityLoader.Get(settings.LowerPriority));
            }

            return effective.ToList();
        }
    }
}