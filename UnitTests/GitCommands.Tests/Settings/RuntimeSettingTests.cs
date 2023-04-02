using FluentAssertions;
using GitCommands;
using GitCommands.Settings;

namespace GitCommandsTests.Settings
{
    [TestFixture]
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
            runtimeSetting.IsUnset.Should().BeTrue();

            runtimeSetting.Value = Enum.PersistentValue;
            runtimeSetting.Value.Should().Be(Enum.PersistentValue);
            persistentSetting.Value.Should().Be(Enum.HardCodedDefault);
            runtimeSetting.IsUnset.Should().BeTrue();

            runtimeSetting.Save();
            runtimeSetting.Value.Should().Be(Enum.PersistentValue);
            persistentSetting.Value.Should().Be(Enum.PersistentValue);
            runtimeSetting.IsUnset.Should().BeFalse();

            runtimeSetting.Value = Enum.RuntimeValue;
            runtimeSetting.Value.Should().Be(Enum.RuntimeValue);
            persistentSetting.Value.Should().Be(Enum.PersistentValue);
            runtimeSetting.IsUnset.Should().BeFalse();

            runtimeSetting.Reload();
            runtimeSetting.Value.Should().Be(Enum.PersistentValue);
            persistentSetting.Value.Should().Be(Enum.PersistentValue);
            runtimeSetting.IsUnset.Should().BeFalse();

            runtimeSetting.ResetToDefault();
            runtimeSetting.Value.Should().Be(Enum.HardCodedDefault);
            persistentSetting.Value.Should().Be(Enum.PersistentValue);
            runtimeSetting.IsUnset.Should().BeFalse();
        }
    }
}
