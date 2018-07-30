using System;
using System.Collections.Generic;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public class ConfigFileSettingsCache : FileSettingsCache
    {
        private Lazy<ConfigFile> _configFile;

        public ConfigFileSettingsCache(string configFileName, bool autoSave, bool isLocal)
            : base(configFileName, autoSave)
        {
            _configFile = new Lazy<ConfigFile>(() => new ConfigFile(SettingsFilePath, isLocal));
        }

        public static ConfigFileSettingsCache FromCache(string settingsFilePath, bool isLocal)
        {
            var createSettingsCache = new Lazy<ConfigFileSettingsCache>(
                () => new ConfigFileSettingsCache(settingsFilePath, true, isLocal));

            return FromCache(settingsFilePath, createSettingsCache);
        }

        public static ConfigFileSettingsCache Create(string settingsFilePath, bool isLocal, bool allowCache = true)
        {
            if (allowCache)
            {
                return FromCache(settingsFilePath, isLocal);
            }
            else
            {
                return new ConfigFileSettingsCache(settingsFilePath, false, isLocal);
            }
        }

        protected override void WriteSettings(string fileName)
        {
            _configFile.Value.Save(fileName);
        }

        protected override void ClearImpl()
        {
            ReadSettings(SettingsFilePath);
        }

        protected override void ReadSettings(string fileName)
        {
            if (!_configFile.IsValueCreated)
            {
                return;
            }

            bool local = _configFile.Value.Local;

            _configFile = new Lazy<ConfigFile>(() => new ConfigFile(fileName, local));
        }

        protected override void SetValueImpl(string key, string value)
        {
            _configFile.Value.SetValue(key, value);
        }

        protected override string GetValueImpl(string key)
        {
            return _configFile.Value.GetValue(key, null);
        }

        /// <summary>
        /// Adds the specific configuration section to the .git/config file.
        /// </summary>
        /// <param name="configSection">The configuration section.</param>
        public void AddConfigSection(IConfigSection configSection)
        {
            LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();
                _configFile.Value.AddConfigSection(configSection);

                // mark as dirty so the updated configuration is persisted
                SettingsChanged();
            });
        }

        public IReadOnlyList<string> GetValues(string key)
        {
            return LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();
                return _configFile.Value.GetValues(key);
            });
        }

        public IReadOnlyList<IConfigSection> GetConfigSections()
        {
            return LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();
                return _configFile.Value.ConfigSections;
            });
        }

        /// <summary>
        /// Removes the specific configuration section from the .git/config file.
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section.</param>
        /// <param name="performSave">If <see langword="true"/> the configuration changes will be saved immediately.</param>
        public void RemoveConfigSection(string configSectionName, bool performSave = false)
        {
            LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();
                _configFile.Value.RemoveConfigSection(configSectionName);
                if (performSave)
                {
                    _configFile.Value.Save();
                }
            });
        }
    }
}
