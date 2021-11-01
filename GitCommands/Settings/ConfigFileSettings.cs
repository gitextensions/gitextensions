using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public sealed class ConfigFileSettings : CompositeSettingsContainer<ConfigFileSettingsCache>, IConfigFileSettings, IConfigValueStore
    {
        public ConfigFileSettings(SettingLevel settingLevel, params ConfigFileSettingsCache[] settingsCaches)
            : base(settingsCaches)
        {
            SettingLevel = settingLevel;
        }

        public new string GetValue(string setting)
        {
            return GetString(setting, string.Empty);
        }

        public IReadOnlyList<string> GetValues(string setting)
        {
            return SettingsCaches.First()
                .GetValues(setting);
        }

        public new void SetValue(string setting, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // to remove setting
                value = null;
            }

            SetString(setting, value);
        }

        public void SetPathValue(string setting, string? value)
        {
            // for using unc paths -> these need to be backward slashes
            if (!string.IsNullOrWhiteSpace(value) && !value.StartsWith("\\\\"))
            {
                value = value.ToPosixPath();
            }

            SetValue(setting, value);
        }

        public IReadOnlyList<IConfigSection> GetConfigSections()
        {
            return SettingsCaches.First()
                .GetConfigSections();
        }

        /// <summary>
        /// Adds the specific configuration section to the .git/config file.
        /// </summary>
        /// <param name="configSection">The configuration section.</param>
        public void AddConfigSection(IConfigSection configSection)
        {
            SettingsCaches.First()
                .AddConfigSection(configSection);
        }

        /// <summary>
        /// Removes the specific configuration section from the .git/config file.
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section.</param>
        /// <param name="performSave">If <see langword="true"/> the configuration changes will be saved immediately.</param>
        public void RemoveConfigSection(string configSectionName, bool performSave = false)
        {
            SettingsCaches.First()
                .RemoveConfigSection(configSectionName, performSave);
        }

        [MaybeNull]
        public Encoding FilesEncoding
        {
            get => GetEncoding("i18n.filesEncoding");
            set => SetEncoding("i18n.filesEncoding", value);
        }

        public Encoding? CommitEncoding => GetEncoding("i18n.commitEncoding");

        public Encoding? LogOutputEncoding => GetEncoding("i18n.logoutputencoding");

        private Encoding? GetEncoding(string settingName)
        {
            string encodingName = GetValue(settingName);

            if (string.IsNullOrEmpty(encodingName))
            {
                return null;
            }

            if (AppSettings.AvailableEncodings.TryGetValue(encodingName, out var result))
            {
                return result;
            }

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine(
                    "Unsupported encoding set in git config file: {0}\n" +
                    "Please check the setting {1} in config file.", encodingName, settingName);
                return null;
            }
        }

        private void SetEncoding(string settingName, Encoding? encoding)
        {
            SetValue(settingName, encoding?.WebName);
        }
    }
}
