using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using GitCommands;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests.Settings
{
    [TestFixture]
    internal sealed class SettingTests
    {
        private const string SettingsFileContent = @"<?xml version=""1.0"" encoding=""utf-8""?><dictionary />";

        private static readonly TempFileCollection _tempFiles = new();
        private static string _settingFilePath;
        private static GitExtSettingsCache _gitExtSettingsCache;
        private static RepoDistSettings _settingContainer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _settingFilePath = _tempFiles.AddExtension(".settings");
            _tempFiles.AddFile(_settingFilePath + ".backup", keepFile: false);

            File.WriteAllText(_settingFilePath, SettingsFileContent);

            _gitExtSettingsCache = GitExtSettingsCache.Create(_settingFilePath);
            _settingContainer = new RepoDistSettings(null, _gitExtSettingsCache, SettingLevel.Unknown);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _gitExtSettingsCache.Dispose();
            ((IDisposable)_tempFiles).Dispose();
        }

        #region Setting

        [Test]
        [TestCaseSource(nameof(CreateCases))]
        public void Should_create_setting<T>(T settingDefault)
            where T : struct
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<T>(settingsPath, settingName, settingDefault);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(settingDefault));
            Assert.That(setting.IsUnset, Is.True);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCaseSource(nameof(SaveCases))]
        public void Should_save_setting<T>(T settingDefault, T value)
            where T : struct
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            T storedValue = default;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, settingDefault);

                setting.Value = value;

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, settingDefault);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.EqualTo(value));
        }

        [Test]
        [TestCaseSource(nameof(SaveCases))]
        public void Should_trigger_updated_event_for_setting<T>(T settingDefault, T value)
            where T : struct
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(SaveCases))]
        public void Should_not_trigger_updated_event_for_setting<T>(T settingDefault, T value)
            where T : struct
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        [TestCaseSource(nameof(CreateCases))]
        public void Should_return_default_value_for_setting_if_value_not_exist<T>(T settingDefault)
            where T : struct
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            T storedValue = default;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, settingDefault);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.EqualTo(settingDefault));
        }

        [Test]
        [TestCaseSource(nameof(CreateCases))]
        public void Should_return_default_value_for_setting_if_value_is_incorrect<T>(T settingDefault)
            where T : struct
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            T storedValue = default;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, settingDefault);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.EqualTo(settingDefault));
        }

        #endregion Setting

        #region String Setting

        [Test]
        [TestCaseSource(nameof(CreateStringCases))]
        public void Should_create_string_setting(string settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault ?? string.Empty));
            Assert.That(setting.Value, Is.EqualTo(settingDefault ?? string.Empty));
            Assert.That(setting.IsUnset, Is.True);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCaseSource(nameof(SaveStringCases))]
        public void Should_save_string_setting(string settingDefault, string value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            string storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, settingDefault);

                setting.Value = value;

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, settingDefault);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.EqualTo(value ?? string.Empty));
        }

        [Test]
        [TestCaseSource(nameof(SaveStringCases))]
        public void Should_trigger_updated_event_for_string_setting(string settingDefault, string value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault ?? string.Empty));
            Assert.That(setting.Value, Is.EqualTo(value ?? string.Empty));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(SaveStringCases))]
        public void Should_not_trigger_updated_event_for_string_setting(string settingDefault, string value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault ?? string.Empty));
            Assert.That(setting.Value, Is.EqualTo(value ?? string.Empty));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        #endregion String Setting

        #region Bool Setting

        [Test]
        public void Should_create_nullable_bool_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<bool>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(false)]
        [TestCase(true)]
        public void Should_save_nullable_bool_setting(bool? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<bool>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Should_trigger_updated_event_for_nullable_bool_setting(bool? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<bool>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase(false)]
        [TestCase(true)]
        public void Should_not_trigger_updated_event_for_nullable_bool_setting(bool? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<bool>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        public void Should_return_default_value_for_nullable_bool_setting_if_value_not_exist()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            bool? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<bool>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        [Test]
        public void Should_return_default_value_for_nullable_bool_setting_if_value_is_incorrect()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            bool? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create<bool>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        #endregion Bool Setting

        #region Char Setting

        [Test]
        public void Should_create_nullable_char_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<char>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(char.MinValue)]
        [TestCase(' ')]
        public void Should_save_nullable_char_setting(char? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<char>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(char.MinValue)]
        [TestCase(' ')]
        public void Should_trigger_updated_event_for_nullable_char_setting(char? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<char>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase(char.MinValue)]
        [TestCase(' ')]
        public void Should_not_trigger_updated_event_for_nullable_char_setting(char? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<char>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        public void Should_return_default_value_for_nullable_char_setting_if_value_not_exist()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            char? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<char>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        [Test]
        public void Should_return_default_value_for_nullable_char_setting_if_value_is_incorrect()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            char? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create<char>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        #endregion Char Setting

        #region Byte Setting

        [Test]
        public void Should_create_nullable_byte_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<byte>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(byte.MinValue)]
        [TestCase(byte.MaxValue)]
        [TestCase(0)]
        public void Should_save_nullable_byte_setting(byte? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<byte>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(byte.MinValue)]
        [TestCase(byte.MaxValue)]
        [TestCase(0)]
        public void Should_trigger_updated_event_for_nullable_byte_setting(byte? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<byte>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase(byte.MinValue)]
        [TestCase(byte.MaxValue)]
        [TestCase(0)]
        public void Should_not_trigger_updated_event_for_nullable_byte_setting(byte? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<byte>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        public void Should_return_default_value_for_nullable_byte_setting_if_value_not_exist()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            byte? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<byte>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        [Test]
        public void Should_return_default_value_for_nullable_byte_setting_if_value_is_incorrect()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            byte? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create<byte>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        #endregion Byte Setting

        #region Int Setting

        [Test]
        public void Should_create_nullable_int_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<int>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [TestCase(0)]
        public void Should_save_nullable_int_setting(int? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<int>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [TestCase(0)]
        public void Should_trigger_updated_event_for_nullable_int_setting(int? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<int>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [TestCase(0)]
        public void Should_not_trigger_updated_event_for_nullable_int_setting(int? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<int>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        public void Should_return_default_value_for_nullable_int_setting_if_value_not_exist()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            int? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<int>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        [Test]
        public void Should_return_default_value_for_nullable_int_setting_if_value_is_incorrect()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            int? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create<int>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        #endregion Int Setting

        #region Float Setting

        [Test]
        public void Should_create_nullable_float_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<float>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase(0f)]
        public void Should_save_nullable_float_setting(float? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<float>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase(0f)]
        public void Should_trigger_updated_event_for_nullable_float_setting(float? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<float>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase(0f)]
        public void Should_not_trigger_updated_event_for_nullable_float_setting(float? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<float>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        public void Should_return_default_value_for_nullable_float_setting_if_value_not_exist()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            float? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<float>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        [Test]
        public void Should_return_default_value_for_nullable_float_setting_if_value_is_incorrect()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            float? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create<float>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        #endregion Float Setting

        #region Enum Setting

        [Test]
        public void Should_create_nullable_enum_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<TestEnum>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(TestEnum.First)]
        [TestCase(TestEnum.Second)]
        public void Should_save_nullable_enum_setting(TestEnum? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<TestEnum>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null)]
        [TestCase(TestEnum.First)]
        [TestCase(TestEnum.Second)]
        public void Should_save_nullable_enum_setting_as_string(TestEnum? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            string storedValue = string.Empty;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<TestEnum>(settingsPath, settingName);

                setting.Value = value;

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                storedValue = setting.Value;
            });

            var isNumber = int.TryParse(storedValue, out _);

            // Assert
            Assert.That(isNumber, Is.False);
        }

        [Test]
        [TestCase(TestEnum.First)]
        [TestCase(TestEnum.Second)]
        public void Should_trigger_updated_event_for_nullable_enum_setting(TestEnum? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<TestEnum>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase(TestEnum.First)]
        [TestCase(TestEnum.Second)]
        public void Should_not_trigger_updated_event_for_nullable_enum_setting(TestEnum? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            var updated = false;

            // Act
            var setting = Setting.Create<TestEnum>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        [Test]
        public void Should_return_default_value_for_nullable_enum_setting_if_value_not_exist()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            TestEnum? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create<TestEnum>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        [Test]
        public void Should_return_default_value_for_nullable_enum_setting_if_value_is_incorrect()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            TestEnum? storedValue = null;

            // Act
            AppSettings.UsingContainer(_settingContainer, () =>
            {
                var setting = Setting.Create(settingsPath, settingName, string.Empty);

                setting.Value = Guid.NewGuid().ToString();

                AppSettings.SaveSettings();
            });

            using TempFileCollection tempFiles = new();
            string filePath = tempFiles.AddExtension(".settings");

            File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

            RepoDistSettings container = new(null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

            AppSettings.UsingContainer(container, () =>
            {
                var setting = Setting.Create<TestEnum>(settingsPath, settingName);

                storedValue = setting.Value;
            });

            // Assert
            Assert.That(storedValue, Is.Null);
        }

        public enum TestEnum
        {
            First,
            Second
        }

        #endregion Enum Setting

        #region Struct Setting

        [Test]
        public void Should_create_nullable_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);

            // Act
            var setting = Setting.Create<TestStruct>(settingsPath, settingName);

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.Null);
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        public void Should_save_nullable_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            TestStruct? value = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            // Act
            var setting = Setting.Create<TestStruct>(settingsPath, settingName);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        public void Should_trigger_updated_event_for_nullable_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            TestStruct? value = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            var updated = false;

            // Act
            var setting = Setting.Create<TestStruct>(settingsPath, settingName);

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.True);
        }

        [Test]
        public void Should_not_trigger_updated_event_for_nullable_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            AppSettingsPath settingsPath = new(pathName);
            TestStruct? value = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            var updated = false;

            // Act
            var setting = Setting.Create<TestStruct>(settingsPath, settingName);

            setting.Value = value;

            setting.Updated += (source, eventArgs) =>
            {
                updated = true;
            };

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.Null);
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
            Assert.That(updated, Is.False);
        }

        public struct TestStruct
        {
            public bool Bool { get; set; }

            public char Char { get; set; }

            public byte Byte { get; set; }

            public int Int { get; set; }

            public float Float { get; set; }
        }

        #endregion Struct Setting

        #region Test Cases

        private static IEnumerable<object[]> CreateCases()
        {
            foreach (var value in Values())
            {
                yield return new object[] { value };
            }
        }

        private static IEnumerable<object[]> SaveCases()
        {
            foreach (var settingDefault in Values())
            {
                foreach (var value in Values())
                {
                    if (settingDefault.GetType() == value.GetType())
                    {
                        yield return new object[] { settingDefault, value };
                    }
                }
            }
        }

        private static IEnumerable<object[]> CreateStringCases()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { "_" };
        }

        private static IEnumerable<object[]> SaveStringCases()
        {
            yield return new object[] { null, null };
            yield return new object[] { null, string.Empty };
            yield return new object[] { null, "_" };

            yield return new object[] { string.Empty, null };
            yield return new object[] { string.Empty, string.Empty };
            yield return new object[] { string.Empty, "_" };

            yield return new object[] { "_", null };
            yield return new object[] { "_", string.Empty };
            yield return new object[] { "_", "_" };
        }

        private static IEnumerable<object> Values()
        {
            yield return false;
            yield return true;

            yield return char.MinValue;
            yield return '_';
            yield return '0';

            yield return sbyte.MinValue;
            yield return sbyte.MaxValue;
            yield return (sbyte)0;
            yield return (sbyte)1;
            yield return (sbyte)-1;

            yield return byte.MinValue;
            yield return byte.MaxValue;
            yield return (byte)1;

            yield return short.MinValue;
            yield return short.MaxValue;
            yield return (short)0;
            yield return (short)1;
            yield return (short)-1;

            yield return ushort.MinValue;
            yield return ushort.MaxValue;
            yield return (ushort)1;

            yield return int.MinValue;
            yield return int.MaxValue;
            yield return 0;
            yield return 1;
            yield return -1;

            yield return uint.MinValue;
            yield return uint.MaxValue;

            yield return long.MinValue;
            yield return long.MaxValue;
            yield return 0L;
            yield return 1L;
            yield return -1L;

            yield return ulong.MinValue;
            yield return ulong.MaxValue;
            yield return 1UL;

            yield return float.MinValue;
            yield return float.MaxValue;
            yield return float.Epsilon;
            yield return float.PositiveInfinity;
            yield return float.NegativeInfinity;
            yield return float.NaN;
            yield return 0F;
            yield return 1F;
            yield return -1F;

            yield return double.MinValue;
            yield return double.MaxValue;
            yield return double.Epsilon;
            yield return double.PositiveInfinity;
            yield return double.NegativeInfinity;
            yield return double.NaN;
            yield return 0D;
            yield return 1D;
            yield return -1D;

            yield return decimal.MinValue;
            yield return decimal.MaxValue;
            yield return decimal.Zero;
            yield return decimal.One;
            yield return decimal.MinusOne;

            yield return DateTime.MinValue;
            yield return DateTime.MaxValue.Day;
            yield return DateTime.Today;

            yield return TestEnum.First;
            yield return TestEnum.Second;

            yield return new TestStruct
            {
                Bool = false,
                Char = char.MinValue,
                Byte = 0,
                Int = 0,
                Float = 0F
            };

            yield return new TestStruct
            {
                Bool = true,
                Char = '_',
                Byte = 1,
                Int = 1,
                Float = 1F
            };
        }

        #endregion Test Cases
    }
}
