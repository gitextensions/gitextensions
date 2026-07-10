using System.CodeDom.Compiler;
using System.Globalization;
using System.Text.Json;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitCommandsTests.Settings;
internal sealed class SettingTests
{
    private const string SettingsFileContent = @"<?xml version=""1.0"" encoding=""utf-8""?><dictionary />";

    private static readonly TempFileCollection _tempFiles = new();
    private static string _settingFilePath = null!;
    private static GitExtSettingsCache _gitExtSettingsCache = null!;
    private static DistributedSettings _settingContainer = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _settingFilePath = _tempFiles.AddExtension(".settings");
        _tempFiles.AddFile(_settingFilePath + ".backup", keepFile: false);

        File.WriteAllText(_settingFilePath, SettingsFileContent);

        _gitExtSettingsCache = GitExtSettingsCache.Create(_settingFilePath);
        _settingContainer = new DistributedSettings(lowerPriority: null, _gitExtSettingsCache, SettingLevel.Unknown);
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
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        ISetting<T> setting = CreateTestSetting(settingsPath, settingName, settingDefault);

        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().Be(settingDefault);
        setting.Value.Should().Be(settingDefault);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCaseSource(nameof(SaveCases))]
    public void Should_save_setting<T>(T settingDefault, T value)
        where T : struct
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        T storedValue = default;
        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<T> setting = CreateTestSetting(settingsPath, settingName, settingDefault);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<T> setting = CreateTestSetting(settingsPath, settingName, settingDefault);

            storedValue = setting.Value;
        });
        storedValue.Should().Be(value);
    }

    [Test]
    [TestCaseSource(nameof(CreateCases))]
    public void Should_return_default_value_for_setting_if_value_not_exist<T>(T settingDefault)
        where T : struct
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        T storedValue = default;
        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<T> setting = CreateTestSetting(settingsPath, settingName, settingDefault);

            storedValue = setting.Value;
        });
        storedValue.Should().Be(settingDefault);
    }

    [Test]
    [TestCaseSource(nameof(CreateCases))]
    public void Should_return_default_value_for_setting_if_value_is_incorrect<T>(T settingDefault)
        where T : struct
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        T storedValue = default;
        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<T> setting = CreateTestSetting(settingsPath, settingName, settingDefault);

            storedValue = setting.Value;
        });
        storedValue.Should().Be(settingDefault);
    }

    #endregion Setting

    #region String Setting

    [Test]
    [TestCaseSource(nameof(CreateStringCases))]
    public void Should_create_string_setting(string settingDefault)
    {
        settingDefault ??= string.Empty;

        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        ISetting<string> setting = Setting.Create(settingsPath, settingName, settingDefault);

        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().Be(settingDefault);
        setting.Value.Should().Be(settingDefault);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCaseSource(nameof(SaveStringCases))]
    public void Should_save_string_setting(string? settingDefault, string? value)
    {
        settingDefault ??= string.Empty;
        value ??= string.Empty;

        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, settingDefault);

            setting.Value.Should().Be(value);
        });
    }

    [Test]
    [TestCaseSource(nameof(SaveStringCases))]
    public void Should_save_nullable_string_setting(string? settingDefault, string? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string?> setting = Setting.CreateNullableString(settingsPath, settingName);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<string?> setting = Setting.CreateNullableString(settingsPath, settingName);

            setting.Value.Should().Be(value);
        });
    }

    #endregion String Setting

    #region Bool Setting

    [Test]
    public void Should_create_nullable_bool_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().BeNull();
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(false)]
    [TestCase(true)]
    public void Should_save_nullable_bool_setting(bool? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    public void Should_return_default_value_for_bool_setting_if_value_not_exist([Values] bool defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    public void Should_return_default_value_for_bool_setting_if_value_is_incorrect([Values] bool defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Should_create_bool_setting(bool defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        ISetting<bool> setting = Setting.Create(settingsPath, settingName, defaultValue);

        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().Be(defaultValue);
        setting.Value.Should().Be(defaultValue);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Should_save_bool_setting(bool defaultValue, bool value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(value);
        });
    }

    [Test]
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    public void Should_read_bool_setting_case_insensitively(string storedValue, bool expected)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = storedValue;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, !expected);

            setting.Value.Should().Be(expected);
        });
    }

    [Test]
    [TestCase("TRUE", false)]
    [TestCase("FALSE", true)]
    [TestCase("1", false)]
    [TestCase("yes", true)]
    public void Should_return_default_for_bool_setting_if_stored_value_is_unrecognized(string storedValue, bool defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = storedValue;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    [TestCase(false, "false")]
    [TestCase(true, "true")]
    public void Should_store_bool_setting_as_lowercase_string(bool value, string expectedStoredString)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<bool> setting = Setting.Create(settingsPath, settingName, !value);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value.Should().Be(expectedStoredString);
        });
    }

    [Test]
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    public void Should_read_nullable_bool_setting_case_insensitively(string storedValue, bool? expected)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = storedValue;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);

            setting.Value.Should().Be(expected);
        });
    }

    [Test]
    [TestCase("TRUE")]
    [TestCase("FALSE")]
    [TestCase("1")]
    [TestCase("yes")]
    public void Should_return_null_for_nullable_bool_setting_if_stored_value_is_unrecognized(string storedValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = storedValue;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);

            setting.Value.Should().BeNull();
        });
    }

    [Test]
    [TestCase(null)]
    [TestCase(false)]
    [TestCase(true)]
    public void Should_save_nullable_bool_setting_as_string(bool? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);

            setting.Value.Should().Be(value);
        });
    }

    [Test]
    [TestCase(false, "false")]
    [TestCase(true, "true")]
    [TestCase(null, null)]
    public void Should_store_nullable_bool_setting_as_lowercase_string(bool? value, string? expectedStoredString)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<bool?> setting = Setting.CreateNullableBool(settingsPath, settingName);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<string?> setting = Setting.CreateNullableString(settingsPath, settingName);

            setting.Value.Should().Be(expectedStoredString);
        });
    }

    #endregion Bool Setting

    #region Char Setting

    [Test]
    public void Should_create_char_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        ISetting<char> setting = CreateTestSetting(settingsPath, settingName, defaultValue: default(char));

        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().Be('\0');
        setting.Value.Should().Be('\0');
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(char.MinValue)]
    [TestCase(' ')]
    [TestCase(char.MaxValue)]
    public void Should_save_nullable_char_setting(char? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<char?> setting = CreateNullableTestSetting<char>(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    public void Should_return_default_value_for_char_setting_if_value_not_exist([Values('\0', ' ', '\uFFFF')] char defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<char> setting = CreateTestSetting(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    public void Should_return_default_value_for_char_setting_if_value_is_incorrect([Values('\0', ' ', '\uFFFF')] char defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<char> setting = CreateTestSetting(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    #endregion Char Setting

    #region Byte Setting

    [Test]
    public void Should_create_nullable_byte_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        ISetting<byte?> setting = CreateNullableTestSetting<byte>(settingsPath, settingName);

        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().BeNull();
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(byte.MinValue)]
    [TestCase(byte.MaxValue)]
    [TestCase(0)]
    public void Should_save_nullable_byte_setting(byte? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<byte?> setting = CreateNullableTestSetting<byte>(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    public void Should_return_default_value_for_byte_setting_if_value_not_exist([Values(0, 1, 255)] byte defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<byte> setting = CreateTestSetting(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    public void Should_return_default_value_for_byte_setting_if_value_is_incorrect([Values(0, 1, 255)] byte defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<byte> setting = CreateTestSetting(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    #endregion Byte Setting

    #region Int Setting

    [Test]
    public void Should_create_nullable_int_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<int?> setting = CreateNullableTestSetting<int>(settingsPath, settingName);
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().BeNull();
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    [TestCase(0)]
    public void Should_save_nullable_int_setting(int? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<int?> setting = CreateNullableTestSetting<int>(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    public void Should_return_default_value_for_int_setting_if_value_not_exist([Values(int.MinValue, -1, 0, 1, int.MaxValue)] int defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<int> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    public void Should_return_default_value_for_int_setting_if_value_is_incorrect([Values(int.MinValue, -1, 0, 1, int.MaxValue)] int defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<int> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    #endregion Int Setting

    #region Float Setting

    [Test]
    public void Should_create_nullable_float_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<float?> setting = CreateNullableTestSetting<float>(settingsPath, settingName);
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().BeNull();
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(float.MinValue)]
    [TestCase(float.MaxValue)]
    [TestCase(0f)]
    public void Should_save_nullable_float_setting(float? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<float?> setting = CreateNullableTestSetting<float>(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    public void Should_return_default_value_for_float_setting_if_value_not_exist([Values(0f, .1f)] float defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<float> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    public void Should_return_default_value_for_float_setting_if_value_is_incorrect([Values(0f, .1f)] float defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<float> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    #endregion Float Setting

    #region Enum Setting

    [Test]
    public void Should_create_nullable_enum_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        ISetting<TestEnum?> setting = Setting.CreateNullableEnum<TestEnum>(settingsPath, settingName);

        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().BeNull();
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(TestEnum.First)]
    [TestCase(TestEnum.Second)]
    public void Should_save_nullable_enum_setting(TestEnum? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<TestEnum?> setting = Setting.CreateNullableEnum<TestEnum>(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    [TestCase(null)]
    [TestCase(TestEnum.First)]
    [TestCase(TestEnum.Second)]
    public void Should_save_nullable_enum_setting_as_string(TestEnum? value)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<TestEnum?> setting = Setting.CreateNullableEnum<TestEnum>(settingsPath, settingName);

            setting.Value = value;

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<string?> setting = Setting.CreateNullableString(settingsPath, settingName);

            string? storedValue = setting.Value;
            storedValue.Should().Be(value?.ToString());
            bool isNumber = int.TryParse(storedValue, out _);
            isNumber.Should().BeFalse();
        });
    }

    [Test]
    public void Should_return_default_value_for_enum_setting_if_value_not_exist([Values] TestEnum defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<TestEnum> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
    }

    [Test]
    public void Should_return_default_value_for_enum_setting_if_value_is_incorrect([Values] TestEnum defaultValue)
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);

        AppSettings.UsingContainer(_settingContainer, () =>
        {
            ISetting<string> setting = Setting.Create(settingsPath, settingName, string.Empty);

            setting.Value = Guid.NewGuid().ToString();

            AppSettings.SaveSettings();
        });

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");

        File.WriteAllText(filePath, File.ReadAllText(_settingFilePath));

        DistributedSettings container = new(lowerPriority: null, GitExtSettingsCache.Create(filePath), SettingLevel.Unknown);

        AppSettings.UsingContainer(container, () =>
        {
            ISetting<TestEnum> setting = Setting.Create(settingsPath, settingName, defaultValue);

            setting.Value.Should().Be(defaultValue);
        });
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
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        ISetting<TestStruct?> setting = CreateNullableTestSetting<TestStruct>(settingsPath, settingName);
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().BeNull();
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
    }

    [Test]
    public void Should_save_nullable_struct_setting()
    {
        string pathName = Guid.NewGuid().ToString();
        string settingName = Guid.NewGuid().ToString();
        AppSettingsPath settingsPath = new(pathName);
        TestStruct? value = new TestStruct
        {
            Bool = false,
            Char = ' ',
            Byte = 0,
            Int = 0,
            Float = 0f
        };
        ISetting<TestStruct?> setting = CreateNullableTestSetting<TestStruct>(settingsPath, settingName);

        setting.Value = value;
        setting.Should().NotBeNull();
        setting.Name.Should().Be(settingName);
        setting.Default.Should().BeNull();
        setting.Value.Should().Be(value);
        setting.FullPath.Should().Be($"{pathName}.{settingName}");
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

    #region Helpers

    private static ISetting<T> CreateTestSetting<T>(SettingsPath path, string name, T defaultValue)
        where T : struct
    {
        Type type = typeof(T);
        return
              type == typeof(string) ? (ISetting<T>)Setting.Create(path, name, (string)(object)defaultValue)
            : type == typeof(bool) ? (ISetting<T>)Setting.Create(path, name, (bool)(object)defaultValue)
            : type == typeof(int) ? (ISetting<T>)Setting.Create(path, name, (int)(object)defaultValue)
            : type == typeof(float) ? (ISetting<T>)Setting.Create(path, name, (float)(object)defaultValue)
            : Setting.Create(
                path,
                name,
                defaultValue,
                read: s =>
                {
                    try
                    {
                        if (typeof(T).IsEnum
                            && Enum.TryParse(typeof(T), s, out object? parsed)
                            && parsed is T enumValue)
                        {
                            return (true, enumValue);
                        }

                        if ((typeof(T).IsPrimitive || typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime))
                            && Convert.ChangeType(s, typeof(T), CultureInfo.InvariantCulture) is T convertedValue)
                        {
                            return (true, convertedValue);
                        }

                        T? deserialized = JsonSerializer.Deserialize<T>(s);
                        return deserialized.HasValue ? (true, deserialized.Value) : default;
                    }
                    catch
                    {
                        return default;
                    }
                },
                store: v =>
                {
                    if (typeof(T).IsEnum)
                    {
                        return v.ToString();
                    }

                    if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime))
                    {
                        return Convert.ToString(v, CultureInfo.InvariantCulture);
                    }

                    return JsonSerializer.Serialize(v);
                });
    }

    private static ISetting<T?> CreateNullableTestSetting<T>(SettingsPath path, string name)
        where T : struct
    {
        Type type = typeof(T);
        return
            type == typeof(string) ? (ISetting<T?>)Setting.CreateNullableString(path, name)
            : type == typeof(bool) ? (ISetting<T?>)Setting.CreateNullableBool(path, name)
            : Setting.Create<T?>(
                path,
                name,
                defaultValue: null,
                read: s =>
                {
                    try
                    {
                        if (typeof(T).IsEnum
                            && Enum.TryParse(typeof(T), s, out object? parsed)
                            && parsed is T enumValue)
                        {
                            return (true, (T?)enumValue);
                        }

                        if ((typeof(T).IsPrimitive || typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime))
                            && Convert.ChangeType(s, typeof(T), CultureInfo.InvariantCulture) is T convertedValue)
                        {
                            return (true, (T?)convertedValue);
                        }

                        T? deserialized = JsonSerializer.Deserialize<T>(s);
                        return deserialized.HasValue ? (true, deserialized) : default;
                    }
                    catch
                    {
                        return default;
                    }
                },
                store: v =>
                {
                    if (!v.HasValue)
                    {
                        return null;
                    }

                    T val = v.Value;

                    if (typeof(T).IsEnum)
                    {
                        return val.ToString();
                    }

                    if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime))
                    {
                        return Convert.ToString(val, CultureInfo.InvariantCulture);
                    }

                    return JsonSerializer.Serialize(val);
                });
    }

    #endregion Helpers

    #region Test Cases

    private static IEnumerable<object[]> CreateCases()
    {
        foreach (object value in Values())
        {
            yield return new object[] { value };
        }
    }

    private static IEnumerable<object[]> SaveCases()
    {
        foreach (object settingDefault in Values())
        {
            foreach (object value in Values())
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
        yield return new object[] { null! };
        yield return new object[] { string.Empty };
        yield return new object[] { "_" };
    }

    private static IEnumerable<object[]> SaveStringCases()
    {
        yield return new object[] { null!, null! };
        yield return new object[] { null!, string.Empty };
        yield return new object[] { null!, "_" };

        yield return new object[] { string.Empty, null! };
        yield return new object[] { string.Empty, string.Empty };
        yield return new object[] { string.Empty, "_" };

        yield return new object[] { "_", null! };
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
