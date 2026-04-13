using GitCommands;
using GitCommands.Settings;

namespace GitCommandsTests.Settings;
internal sealed class RuntimeSettingTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    private enum Enum
    {
        Foo,
        Bar,
        HardCodedDefault,
        PersistentValue,
        RuntimeValue,
    }

    [Test]
    public void EnumRuntimeSetting()
    {
        SettingsPath rootSettingsPath = new AppSettingsPath(pathName: "");
        const string name = "SettingX";

        ISetting<Enum> persistentSetting = Setting.Create(rootSettingsPath, name, Enum.HardCodedDefault);
        RuntimeSetting<Enum> runtimeSetting = new(persistentSetting);

        runtimeSetting.Value.Should().Be(Enum.HardCodedDefault);
        Setting.IsUnset(persistentSetting).Should().BeTrue();

        runtimeSetting.Value = Enum.PersistentValue;
        runtimeSetting.Value.Should().Be(Enum.PersistentValue);
        Setting.GetRawValue(persistentSetting).Should().Be(Enum.HardCodedDefault);
        Setting.IsUnset(persistentSetting).Should().BeTrue();

        runtimeSetting.Save();
        runtimeSetting.Value.Should().Be(Enum.PersistentValue);
        Setting.GetRawValue(persistentSetting).Should().Be(Enum.PersistentValue);
        Setting.IsUnset(persistentSetting).Should().BeFalse();

        runtimeSetting.Value = Enum.RuntimeValue;
        runtimeSetting.Value.Should().Be(Enum.RuntimeValue);
        Setting.GetRawValue(persistentSetting).Should().Be(Enum.PersistentValue);
        Setting.IsUnset(persistentSetting).Should().BeFalse();

        runtimeSetting.Reload();
        runtimeSetting.Value.Should().Be(Enum.PersistentValue);
        Setting.GetRawValue(persistentSetting).Should().Be(Enum.PersistentValue);
        Setting.IsUnset(persistentSetting).Should().BeFalse();

        runtimeSetting.ResetToDefault();
        runtimeSetting.Value.Should().Be(Enum.HardCodedDefault);
        Setting.GetRawValue(persistentSetting).Should().Be(Enum.PersistentValue);
        Setting.IsUnset(persistentSetting).Should().BeFalse();
    }
}
