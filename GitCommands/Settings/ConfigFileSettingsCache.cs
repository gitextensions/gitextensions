﻿using GitCommands.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands.Settings
{
    public class ConfigFileSettingsCache : FileSettingsCache
    {
        private Lazy<ConfigFile> _configFile;

        public ConfigFileSettingsCache(string configFileName, bool autoSave, bool aLocal)
            : base(configFileName, autoSave)
        {
            _configFile = new Lazy<ConfigFile>(() =>
                {
                    return new ConfigFile(SettingsFilePath, aLocal);
                });
        }

        public static ConfigFileSettingsCache FromCache(string aSettingsFilePath, bool aLocal)
        {
            Lazy<ConfigFileSettingsCache> createSettingsCache = new Lazy<ConfigFileSettingsCache>(() =>
            {
                return new ConfigFileSettingsCache(aSettingsFilePath, true, aLocal);
            });

            return FileSettingsCache.FromCache(aSettingsFilePath, createSettingsCache);
        }

        public static ConfigFileSettingsCache Create(string aSettingsFilePath, bool aLocal, bool allowCache = true)
        {
            if (allowCache)
                return FromCache(aSettingsFilePath, aLocal);
            else
                return new ConfigFileSettingsCache(aSettingsFilePath, false, aLocal);
        }

        protected override void WriteSettings(string fileName)
        {
            _configFile.Value.Save(fileName);
        }

        protected override void ClearImpl()
        {
            base.ClearImpl();
            ReadSettings(SettingsFilePath);
        }

        protected override void ReadSettings(string fileName)
        {
            if (!_configFile.IsValueCreated)
            {
                return;
            }

            bool local = _configFile.Value.Local;

            _configFile = new Lazy<ConfigFile>(() =>
            {
                return new ConfigFile(fileName, local);
            });
        }

        protected override void SetValueImpl(string key, string value)
        {
            _configFile.Value.SetValue(key, value);
        }

        protected override string GetValueImpl(string key)
        {
            return _configFile.Value.GetValue(key, null);
        }


    }
}
