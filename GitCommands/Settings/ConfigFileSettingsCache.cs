using System.Diagnostics;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    [DebuggerDisplay("{_byNameMap.Count} cached {" + nameof(SettingsFilePath) + ",nq}")]
    public class ConfigFileSettingsCache : FileSettingsCache
    {
        private Lazy<ConfigFile> _configFile;

        public ConfigFileSettingsCache(string configFileName, bool autoSave)
            : base(configFileName, autoSave)
        {
            _configFile = new Lazy<ConfigFile>(() => new ConfigFile(SettingsFilePath));
        }

        public static ConfigFileSettingsCache FromCache(string settingsFilePath)
        {
            Lazy<ConfigFileSettingsCache> createSettingsCache = new(
                () => new ConfigFileSettingsCache(settingsFilePath, autoSave: true));

            return FromCache(settingsFilePath, createSettingsCache);
        }

        public static ConfigFileSettingsCache Create(string settingsFilePath, bool useSharedCache = true)
        {
            if (useSharedCache)
            {
                return FromCache(settingsFilePath);
            }
            else
            {
                return new ConfigFileSettingsCache(settingsFilePath, autoSave: false);
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

            _configFile = new Lazy<ConfigFile>(() => new ConfigFile(fileName));
        }

        protected override void SetValueImpl(string key, string? value)
        {
            _configFile.Value.SetValue(key, value!); // TODO propagate nullability
        }

        protected override string GetValueImpl(string key)
        {
            return _configFile.Value.GetValue(key, null!); // TODO revisit null suppression
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
