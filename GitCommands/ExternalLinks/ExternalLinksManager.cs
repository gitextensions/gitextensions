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
        private readonly ISettingsSource _cachedSettings;
        private readonly ExternalLinksManager? _lowerPriority;
        private readonly IExternalLinksStorage _externalLinksStorage = new ExternalLinksStorage();
        private readonly List<ExternalLinkDefinition> _definitions;

        public ExternalLinksManager(RepoDistSettingsSet settingsSet, SettingLevel settingLevel, bool cascade = false)
        {
            if (settingLevel is SettingLevel.Effective && !cascade)
            {
                throw new InvalidOperationException("Effective settings level should be cascading.");
            }

            switch (settingLevel)
            {
                case SettingLevel.Effective:
                case SettingLevel.Local:
                    _cachedSettings = settingsSet.LocalSettings;

                    if (cascade)
                    {
                        _lowerPriority = new ExternalLinksManager(settingsSet, SettingLevel.Distributed, cascade);
                    }

                    break;
                case SettingLevel.Distributed:
                    _cachedSettings = settingsSet.RepoDistSettings;

                    if (cascade)
                    {
                        _lowerPriority = new ExternalLinksManager(settingsSet, SettingLevel.Global, cascade);
                    }

                    break;
                case SettingLevel.Global:
                    _cachedSettings = settingsSet.GlobalSettings;
                    break;
                default:
                    throw new InvalidOperationException("Unknown settings level.");
            }

            _definitions = _externalLinksStorage
                .Load(_cachedSettings)
                .ToList();
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

            if (_lowerPriority is null || _lowerPriority.Contains(definition.Name))
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
        /// Adds the provided definitions at the lowest available level.
        /// </summary>
        /// <param name="definitions">External link definitions.</param>
        public void AddRange(IEnumerable<ExternalLinkDefinition> definitions)
        {
            if (definitions is null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            foreach (var externalLinkDefinition in definitions)
            {
                Add(externalLinkDefinition);
            }
        }

        /// <summary>
        /// Checks if a definition with the supplied name exists in any level of available settings.
        /// </summary>
        /// <param name="definitionName">The name of the definition to find.</param>
        /// <returns><see langword="true"/> if a definition already exists; otherwise <see langword="false"/>.</returns>
        public bool Contains(string? definitionName)
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
