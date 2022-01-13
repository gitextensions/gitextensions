using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitCommands.ExternalLinks
{
    // NB: this implementation is stateful
    public sealed class ExternalLinksManager
    {
        private readonly ISettingsSource[] _cachedSettingsList;
        private readonly IExternalLinksStorage _externalLinksStorage = new ExternalLinksStorage();
        private readonly List<List<ExternalLinkDefinition>> _definitionsList = new();

        public ExternalLinksManager(RepoDistSettings settings)
        {
            _cachedSettingsList = settings.Split();

            foreach (ISettingsSource cachedSettings in _cachedSettingsList)
            {
                List<ExternalLinkDefinition> definitions = _externalLinksStorage
                    .Load(cachedSettings)
                    .ToList();

                _definitionsList.Add(definitions);
            }
        }

        /// <summary>
        /// Adds the provided definition at the lowest available level.
        /// </summary>
        /// <param name="definition">External link definition.</param>
        public void Add(ExternalLinkDefinition definition)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            foreach (int index in Enumerable.Range(0, _definitionsList.Count))
            {
                List<ExternalLinkDefinition> definitions = _definitionsList[index];
                List<ExternalLinkDefinition> lowerPriorityDefinitions = null;

                if (index + 1 < _definitionsList.Count)
                {
                    lowerPriorityDefinitions = _definitionsList[index + 1];
                }

                if (lowerPriorityDefinitions is null || Contains(lowerPriorityDefinitions, definition.Name))
                {
                    if (!Contains(definitions, definition.Name))
                    {
                        definitions.Add(definition);
                    }

                    // TODO: else notify the user?
                }
                else
                {
                    lowerPriorityDefinitions.Add(definition);
                }
            }
        }

        /// <summary>
        /// Adds the provided definitions at the lowest available level.
        /// </summary>
        /// <param name="definitions">External link definitions.</param>
        public void AddRange(IEnumerable<ExternalLinkDefinition> definitions)
        {
            if (definitions is null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            foreach (ExternalLinkDefinition externalLinkDefinition in definitions)
            {
                Add(externalLinkDefinition);
            }
        }

        /// <summary>
        /// Loads all settings from all available levels.
        /// </summary>
        /// <returns>A collection of all available definitions.</returns>
        public IReadOnlyList<ExternalLinkDefinition> GetEffectiveSettings()
        {
            IEnumerable<ExternalLinkDefinition> result = Enumerable.Empty<ExternalLinkDefinition>();

            foreach (List<ExternalLinkDefinition> definitions in _definitionsList)
            {
                result = result
                    .Union(definitions);
            }

            return result
                .ToList();
        }

        /// <summary>
        /// Removes the supplied definition.
        /// </summary>
        /// <param name="definition">External link definition.</param>
        public void Remove(ExternalLinkDefinition definition)
        {
            foreach (List<ExternalLinkDefinition> definitions in _definitionsList)
            {
                if (definitions.Remove(definition))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        public void Save()
        {
            foreach (int index in Enumerable.Range(0, _cachedSettingsList.Length))
            {
                ISettingsSource cachedSettings = _cachedSettingsList[index];
                List<ExternalLinkDefinition> definitions = _definitionsList[index];

                _externalLinksStorage.Save(cachedSettings, definitions);
            }
        }

        /// <summary>
        /// Checks if a definition with the supplied name exists in any level of available settings.
        /// </summary>
        /// <param name="definitionName">The name of the definition to find.</param>
        /// <returns><see langword="true"/> if a definition already exists; otherwise <see langword="false"/>.</returns>
        private static bool Contains(IEnumerable<ExternalLinkDefinition> definitions, string? definitionName)
        {
            return definitions.Any(x => x.Name == definitionName);
        }
    }
}
