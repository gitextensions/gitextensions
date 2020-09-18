using System;
using GitCommands;
using GitCommands.Settings;
using NUnit.Framework;

namespace GitCommandsTests.Settings
{
    [TestFixture]
    internal sealed class SettingTests
    {
        #region String Setting

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Should_create_string_setting(string settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
        [TestCase(null, null)]
        [TestCase(null, "")]
        [TestCase(null, " ")]
        [TestCase("", null)]
        [TestCase("", "")]
        [TestCase("", " ")]
        [TestCase(" ", null)]
        [TestCase(" ", "")]
        [TestCase(" ", " ")]
        public void Should_save_string_setting(string settingDefault, string value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault ?? string.Empty));
            Assert.That(setting.Value, Is.EqualTo(value ?? string.Empty));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(null, "")]
        [TestCase(null, " ")]
        [TestCase("", null)]
        [TestCase("", "")]
        [TestCase("", " ")]
        [TestCase(" ", null)]
        [TestCase(" ", "")]
        [TestCase(" ", " ")]
        public void Should_trigger_updated_event_for_string_setting(string settingDefault, string value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(null, null)]
        [TestCase(null, "")]
        [TestCase(null, " ")]
        [TestCase("", null)]
        [TestCase("", "")]
        [TestCase("", " ")]
        [TestCase(" ", null)]
        [TestCase(" ", "")]
        [TestCase(" ", " ")]
        public void Should_not_trigger_updated_event_for_string_setting(string settingDefault, string value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(false)]
        [TestCase(true)]
        public void Should_create_bool_setting(bool settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Should_save_bool_setting(bool settingDefault, bool value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Should_trigger_updated_event_for_bool_setting(bool settingDefault, bool value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Should_not_trigger_updated_event_for_bool_setting(bool settingDefault, bool value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        public void Should_create_nullable_bool_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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

        #endregion Bool Setting

        #region Char Setting

        [Test]
        [TestCase(char.MinValue)]
        [TestCase(char.MaxValue)]
        [TestCase(' ')]
        public void Should_create_char_setting(char settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        [TestCase(char.MinValue, char.MinValue)]
        [TestCase(char.MinValue, char.MaxValue)]
        [TestCase(char.MinValue, ' ')]
        [TestCase(char.MaxValue, char.MinValue)]
        [TestCase(char.MaxValue, char.MaxValue)]
        [TestCase(char.MaxValue, ' ')]
        [TestCase(' ', char.MinValue)]
        [TestCase(' ', char.MaxValue)]
        [TestCase(' ', ' ')]
        public void Should_save_char_setting(char settingDefault, char value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(char.MinValue, char.MinValue)]
        [TestCase(char.MinValue, char.MaxValue)]
        [TestCase(char.MinValue, ' ')]
        [TestCase(char.MaxValue, char.MinValue)]
        [TestCase(char.MaxValue, char.MaxValue)]
        [TestCase(char.MaxValue, ' ')]
        [TestCase(' ', char.MinValue)]
        [TestCase(' ', char.MaxValue)]
        [TestCase(' ', ' ')]
        public void Should_trigger_updated_event_for_char_setting(char settingDefault, char value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(char.MinValue, char.MinValue)]
        [TestCase(char.MinValue, char.MaxValue)]
        [TestCase(char.MinValue, ' ')]
        [TestCase(char.MaxValue, char.MinValue)]
        [TestCase(char.MaxValue, char.MaxValue)]
        [TestCase(char.MaxValue, ' ')]
        [TestCase(' ', char.MinValue)]
        [TestCase(' ', char.MaxValue)]
        [TestCase(' ', ' ')]
        public void Should_not_trigger_updated_event_for_char_setting(char settingDefault, char value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        public void Should_create_nullable_char_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
        [TestCase(char.MaxValue)]
        [TestCase(' ')]
        public void Should_save_nullable_char_setting(char? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
        [TestCase(char.MaxValue)]
        [TestCase(' ')]
        public void Should_trigger_updated_event_for_nullable_char_setting(char? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(char.MaxValue)]
        [TestCase(' ')]
        public void Should_not_trigger_updated_event_for_nullable_char_setting(char? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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

        #endregion Char Setting

        #region Byte Setting

        [Test]
        [TestCase(byte.MinValue)]
        [TestCase(byte.MaxValue)]
        [TestCase(0)]
        public void Should_create_byte_setting(byte settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        [TestCase(byte.MinValue, byte.MinValue)]
        [TestCase(byte.MinValue, byte.MaxValue)]
        [TestCase(byte.MinValue, 0)]
        [TestCase(byte.MaxValue, byte.MinValue)]
        [TestCase(byte.MaxValue, byte.MaxValue)]
        [TestCase(byte.MaxValue, 0)]
        [TestCase(0, byte.MinValue)]
        [TestCase(0, byte.MaxValue)]
        [TestCase(0, 0)]
        public void Should_save_byte_setting(byte settingDefault, byte value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(byte.MinValue, byte.MinValue)]
        [TestCase(byte.MinValue, byte.MaxValue)]
        [TestCase(byte.MinValue, 0)]
        [TestCase(byte.MaxValue, byte.MinValue)]
        [TestCase(byte.MaxValue, byte.MaxValue)]
        [TestCase(byte.MaxValue, 0)]
        [TestCase(0, byte.MinValue)]
        [TestCase(0, byte.MaxValue)]
        [TestCase(0, 0)]
        public void Should_trigger_updated_event_for_byte_setting(byte settingDefault, byte value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(byte.MinValue, byte.MinValue)]
        [TestCase(byte.MinValue, byte.MaxValue)]
        [TestCase(byte.MinValue, 0)]
        [TestCase(byte.MaxValue, byte.MinValue)]
        [TestCase(byte.MaxValue, byte.MaxValue)]
        [TestCase(byte.MaxValue, 0)]
        [TestCase(0, byte.MinValue)]
        [TestCase(0, byte.MaxValue)]
        [TestCase(0, 0)]
        public void Should_not_trigger_updated_event_for_byte_setting(byte settingDefault, byte value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        public void Should_create_nullable_byte_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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

        #endregion Byte Setting

        #region Int Setting

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [TestCase(0)]
        public void Should_create_int_setting(int settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        [TestCase(int.MinValue, int.MinValue)]
        [TestCase(int.MinValue, int.MaxValue)]
        [TestCase(int.MinValue, 0)]
        [TestCase(int.MaxValue, int.MinValue)]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(int.MaxValue, 0)]
        [TestCase(0, int.MinValue)]
        [TestCase(0, int.MaxValue)]
        [TestCase(0, 0)]
        public void Should_save_int_setting(int settingDefault, int value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(int.MinValue, int.MinValue)]
        [TestCase(int.MinValue, int.MaxValue)]
        [TestCase(int.MinValue, 0)]
        [TestCase(int.MaxValue, int.MinValue)]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(int.MaxValue, 0)]
        [TestCase(0, int.MinValue)]
        [TestCase(0, int.MaxValue)]
        [TestCase(0, 0)]
        public void Should_trigger_updated_event_for_int_setting(int settingDefault, int value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(int.MinValue, int.MinValue)]
        [TestCase(int.MinValue, int.MaxValue)]
        [TestCase(int.MinValue, 0)]
        [TestCase(int.MaxValue, int.MinValue)]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(int.MaxValue, 0)]
        [TestCase(0, int.MinValue)]
        [TestCase(0, int.MaxValue)]
        [TestCase(0, 0)]
        public void Should_not_trigger_updated_event_for_int_setting(int settingDefault, int value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        public void Should_create_nullable_int_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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

        #endregion Int Setting

        #region Float Setting

        [Test]
        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase(0f)]
        public void Should_create_float_setting(float settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        [TestCase(float.MinValue, float.MinValue)]
        [TestCase(float.MinValue, float.MaxValue)]
        [TestCase(float.MinValue, 0f)]
        [TestCase(float.MaxValue, float.MinValue)]
        [TestCase(float.MaxValue, float.MaxValue)]
        [TestCase(float.MaxValue, 0f)]
        [TestCase(0f, float.MinValue)]
        [TestCase(0f, float.MaxValue)]
        [TestCase(0f, 0f)]
        public void Should_save_float_setting(float settingDefault, float value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(float.MinValue, float.MinValue)]
        [TestCase(float.MinValue, float.MaxValue)]
        [TestCase(float.MinValue, 0f)]
        [TestCase(float.MaxValue, float.MinValue)]
        [TestCase(float.MaxValue, float.MaxValue)]
        [TestCase(float.MaxValue, 0)]
        [TestCase(0f, float.MinValue)]
        [TestCase(0f, float.MaxValue)]
        [TestCase(0f, 0f)]
        public void Should_trigger_updated_event_for_float_setting(float settingDefault, float value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(float.MinValue, float.MinValue)]
        [TestCase(float.MinValue, float.MaxValue)]
        [TestCase(float.MinValue, 0f)]
        [TestCase(float.MaxValue, float.MinValue)]
        [TestCase(float.MaxValue, float.MaxValue)]
        [TestCase(float.MaxValue, 0f)]
        [TestCase(0f, float.MinValue)]
        [TestCase(0f, float.MaxValue)]
        [TestCase(0f, 0f)]
        public void Should_not_trigger_updated_event_for_float_setting(float settingDefault, float value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        public void Should_create_nullable_float_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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

        #endregion Float Setting

        #region Enum Setting

        [Test]
        [TestCase(TestEnum.First)]
        [TestCase(TestEnum.Second)]
        public void Should_create_enum_setting(TestEnum settingDefault)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        [TestCase(TestEnum.First, TestEnum.First)]
        [TestCase(TestEnum.First, TestEnum.Second)]
        [TestCase(TestEnum.Second, TestEnum.First)]
        [TestCase(TestEnum.Second, TestEnum.Second)]
        public void Should_save_enum_setting(TestEnum settingDefault, TestEnum value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        [TestCase(TestEnum.First, TestEnum.First)]
        [TestCase(TestEnum.First, TestEnum.Second)]
        [TestCase(TestEnum.Second, TestEnum.First)]
        [TestCase(TestEnum.Second, TestEnum.Second)]
        public void Should_trigger_updated_event_for_enum_setting(TestEnum settingDefault, TestEnum value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        [TestCase(TestEnum.First, TestEnum.First)]
        [TestCase(TestEnum.First, TestEnum.Second)]
        [TestCase(TestEnum.Second, TestEnum.First)]
        [TestCase(TestEnum.Second, TestEnum.Second)]
        public void Should_not_trigger_updated_event_for_enum_setting(TestEnum settingDefault, TestEnum value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
        public void Should_create_nullable_enum_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);

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
        [TestCase(TestEnum.First)]
        [TestCase(TestEnum.Second)]
        public void Should_trigger_updated_event_for_nullable_enum_setting(TestEnum? value)
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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

        public enum TestEnum
        {
            First,
            Second
        }

        #endregion Enum Setting

        #region Struct Setting

        [Test]
        public void Should_create_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
            var settingDefault = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

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
        public void Should_save_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
            var settingDefault = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            var value = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            // Act
            var setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            // Assert
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting.SettingsSource, Is.EqualTo(settingsPath));
            Assert.That(setting.Name, Is.EqualTo(settingName));
            Assert.That(setting.Default, Is.EqualTo(settingDefault));
            Assert.That(setting.Value, Is.EqualTo(value));
            Assert.That(setting.IsUnset, Is.False);
            Assert.That(setting.FullPath, Is.EqualTo($"{pathName}.{settingName}"));
        }

        [Test]
        public void Should_trigger_updated_event_for_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
            var settingDefault = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            var value = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

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
        public void Should_not_trigger_updated_event_for_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);
            var settingDefault = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

            var value = new TestStruct
            {
                Bool = false,
                Char = ' ',
                Byte = 0,
                Int = 0,
                Float = 0f
            };

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
        public void Should_create_nullable_struct_setting()
        {
            // Arrange
            var pathName = Guid.NewGuid().ToString();
            var settingName = Guid.NewGuid().ToString();
            var settingsPath = new AppSettingsPath(pathName);

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
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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
            var settingsPath = new AppSettingsPath(pathName);
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
    }
}
