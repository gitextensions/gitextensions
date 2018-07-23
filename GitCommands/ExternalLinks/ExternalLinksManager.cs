using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.Settings;

namespace GitCommands.ExternalLinks
{
    // NB: this implementation is stateful
    public sealed class ExternalLinksManager
    {
        private readonly RepoDistSettings _cachedSettings;
        private readonly ExternalLinksManager _lowerPriority;
        private readonly IExternalLinksStorage _externalLinksStorage = new ExternalLinksStorage();
        private readonly List<ExternalLinkDefinition> _definitions;

        public ExternalLinksManager(RepoDistSettings settings)
        {
            _cachedSettings = new RepoDistSettings(null, settings.SettingsCache);
            _definitions = _externalLinksStorage.Load(_cachedSettings).ToList();

            if (settings.LowerPriority != null)
            {
                _lowerPriority = new ExternalLinksManager(settings.LowerPriority);
            }
        }

        /// <summary>
        /// Adds the provided definition at the lowest available level.
        /// </summary>
        /// <param name="definition">External link definition.</param>
        public void Add(ExternalLinkDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (_lowerPriority == null || _lowerPriority.Contains(definition.Name))
            {
                if (!Contains(definition.Name))
                {
                    _definitions.Add(definition);
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority.Add(definition);
            }
        }

        /// <summary>
        /// Checks if a definition with the supplied name exists in any level of available settings.
        /// </summary>
        /// <param name="definitionName">The name of the definition to find.</param>
        /// <returns><see langword="true"/> if a definition already exists; otherwise <see langword="false"/>.</returns>
        public bool Contains(string definitionName)
        {
            return _definitions.Any(linkDef => linkDef.Name == definitionName);
        }

        /// <summary>
        /// Loads all settings from all available levels.
        /// </summary>
        /// <returns>A collection of all available definitions.</returns>
        public IReadOnlyList<ExternalLinkDefinition> GetEffectiveSettings()
        {
            return _definitions
                .Union(_lowerPriority?.GetEffectiveSettings() ?? Enumerable.Empty<ExternalLinkDefinition>())
                .ToList();
        }

        /// <summary>
        /// Removes the supplied definition.
        /// </summary>
        /// <param name="definition">External link definition.</param>
        public void Remove(ExternalLinkDefinition definition)
        {
            if (!_definitions.Remove(definition))
            {
                _lowerPriority?.Remove(definition);
            }
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        public void Save()
        {
            _lowerPriority?.Save();
            _externalLinksStorage.Save(_cachedSettings, _definitions);
        }
    }
}
