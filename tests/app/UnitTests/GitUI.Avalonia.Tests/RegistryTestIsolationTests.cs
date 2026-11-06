using System.Reflection;
using GitCommands;
using Microsoft.Win32;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RegistryTestIsolationTests
{
    [TestCaseSource(nameof(RegistryAtDiscovery))]
    public void AppSettings_should_already_use_test_registry_during_discovery(string? keyName)
    {
        if (OperatingSystem.IsWindows())
        {
            keyName.Should().Contain(RegistryTestIsolation.TestRoot);
            keyName.Should().Be(RegistryTestIsolation.SettingsKeyName);
        }
        else
        {
            keyName.Should().BeNull("portable settings do not open a Windows registry handle");
        }
    }

    [Test]
    public void AppSettings_should_write_SSH_and_startup_settings_to_the_isolated_store()
    {
        string previousSsh = AppSettings.SshPath;
        bool previousCheck = AppSettings.CheckSettings;
        try
        {
            AppSettings.SshPath = "registry-isolation-test.exe";
            AppSettings.CheckSettings = !previousCheck;
            AppSettings.SshPath.Should().Be("registry-isolation-test.exe");
            AppSettings.CheckSettings.Should().Be(!previousCheck);
            if (OperatingSystem.IsWindows())
            {
                FieldInfo field = typeof(AppSettings).GetField("_versionIndependentRegKey", BindingFlags.NonPublic | BindingFlags.Static)!;
                RegistryKey key = (RegistryKey)field.GetValue(null)!;
                key.Name.Should().Be(RegistryTestIsolation.SettingsKeyName);
                key.GetValue("gitssh").Should().Be("registry-isolation-test.exe");
                key.GetValue("CheckSettings").Should().Be(previousCheck ? "false" : "true");
            }
            else
            {
                AppSettings.GetString("gitssh", null).Should().Be("registry-isolation-test.exe");
                AppSettings.GetBool("CheckSettings", previousCheck).Should().Be(!previousCheck);
            }
        }
        finally
        {
            AppSettings.SshPath = previousSsh;
            AppSettings.CheckSettings = previousCheck;
        }
    }

    [Test]
    public void Capture_should_detect_changed_added_removed_and_retyped_values_including_subkeys()
    {
        if (!OperatingSystem.IsWindows())
        {
            RegistryTestIsolation.SettingsKeyName.Should().BeNull();
            return;
        }

        string path = $"{RegistryTestIsolation.TestRoot}\\{Environment.ProcessId}-guard-{Guid.NewGuid():N}";
        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(path, writable: true);
            key.SetValue("value", "%PATH%", RegistryValueKind.ExpandString);
            key.SetValue("removed", new byte[] { 1, 2 }, RegistryValueKind.Binary);
            using RegistryKey child = key.CreateSubKey("child", writable: true);
            child.SetValue("list", new[] { "first", "second" }, RegistryValueKind.MultiString);
            IReadOnlyDictionary<string, string> before = RegistryTestIsolation.Capture(key);
            RegistryTestIsolation.Capture(key).Should().BeEquivalentTo(before);
            key.SetValue("value", "%PATH%", RegistryValueKind.String);
            RegistryTestIsolation.Capture(key).Should().NotBeEquivalentTo(before);
            key.SetValue("value", "%PATH%", RegistryValueKind.ExpandString);
            key.DeleteValue("removed");
            RegistryTestIsolation.Capture(key).Should().NotBeEquivalentTo(before);
            key.SetValue("removed", new byte[] { 1, 2 }, RegistryValueKind.Binary);
            key.SetValue("added", 123, RegistryValueKind.DWord);
            RegistryTestIsolation.Capture(key).Should().NotBeEquivalentTo(before);
            key.DeleteValue("added");
            child.SetValue("list", new[] { "first", "changed" }, RegistryValueKind.MultiString);
            RegistryTestIsolation.Capture(key).Should().NotBeEquivalentTo(before);
            child.SetValue("list", new[] { "first", "second" }, RegistryValueKind.MultiString);
            RegistryTestIsolation.Capture(key).Should().BeEquivalentTo(before);
            RegistryTestIsolation.Capture(null).Should().BeEmpty();
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
        }
    }

    private static IEnumerable<TestCaseData> RegistryAtDiscovery()
    {
        FieldInfo field = typeof(AppSettings).GetField("_versionIndependentRegKey", BindingFlags.NonPublic | BindingFlags.Static)!;
        string? name = OperatingSystem.IsWindows() ? ((RegistryKey?)field.GetValue(null))?.Name : null;
        yield return new TestCaseData(name);
    }
}
