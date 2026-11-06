using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using GitCommands;
using Microsoft.Win32;

internal static class RegistryTestIsolation
{
    internal const string TestRoot = @"Software\GitExtensions.Avalonia.Tests";
    private const string OwnerPath = @"Software\GitExtensions";
    private static RegistryKey? _testRoot;
    private static RegistryKey? _settings;
    private static string? _path;
    private static IReadOnlyDictionary<string, string>? _ownerSnapshot;

    internal static string? SettingsKeyName => OperatingSystem.IsWindows() ? _settings?.Name : null;

    [ModuleInitializer]
    internal static void Initialize()
    {
        if (OperatingSystem.IsWindows())
        {
            InitializeWindows();
        }
    }

    internal static void VerifyAndDispose()
    {
        if (!OperatingSystem.IsWindows() || _testRoot is null)
        {
            return;
        }

        try
        {
            using RegistryKey? owner = Registry.CurrentUser.OpenSubKey(OwnerPath, writable: false);
            Capture(owner).Should().BeEquivalentTo(_ownerSnapshot,
                "tests must never modify the owner's registry settings");
            TestContext.Progress.WriteLine("Owner registry values unchanged (including subkeys).");
        }
        finally
        {
            _settings?.Dispose();
            _testRoot.Dispose();
            Registry.CurrentUser.DeleteSubKeyTree(_path!, throwOnMissingSubKey: false);
            _testRoot = null;
        }
    }

    [SupportedOSPlatform("windows")]
    internal static IReadOnlyDictionary<string, string> Capture(RegistryKey? root)
    {
        Dictionary<string, string> snapshot = new(StringComparer.OrdinalIgnoreCase);
        if (root is not null)
        {
            CaptureKey(root, string.Empty, snapshot);
        }

        return snapshot;
    }

    [SupportedOSPlatform("windows")]
    private static void CaptureKey(RegistryKey key, string path, Dictionary<string, string> snapshot)
    {
        snapshot.Add($"key:{path}", string.Empty);
        foreach (string name in key.GetValueNames())
        {
            object? value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            snapshot.Add($"value:{path}\\{name}", $"{key.GetValueKind(name)}:{JsonSerializer.Serialize(value)}");
        }

        foreach (string name in key.GetSubKeyNames())
        {
            using RegistryKey child = key.OpenSubKey(name, writable: false)!;
            CaptureKey(child, $"{path}\\{name}", snapshot);
        }
    }

    [SupportedOSPlatform("windows")]
    private static void InitializeWindows()
    {
        using (RegistryKey? owner = Registry.CurrentUser.OpenSubKey(OwnerPath, writable: false))
        {
            _ownerSnapshot = Capture(owner);
        }

        _path = $"{TestRoot}\\{Environment.ProcessId}-{Guid.NewGuid():N}";
        _testRoot = Registry.CurrentUser.CreateSubKey(_path, writable: true);
        // AppSettings migrates SSH settings in its static constructor, before a SetUpFixture
        // or reflected field assignment can redirect its cached registry handle. Confine that
        // first initialization too, before NUnit evaluates any test-case sources.
        nint currentUser = unchecked((nint)(int)0x80000001);
        ThrowIfFailed(RegOverridePredefKey(currentUser, _testRoot.Handle.DangerousGetHandle()));
        try
        {
            RuntimeHelpers.RunClassConstructor(typeof(AppSettings).TypeHandle);
        }
        finally
        {
            ThrowIfFailed(RegOverridePredefKey(currentUser, nint.Zero));
        }

        FieldInfo field = typeof(AppSettings).GetField("_versionIndependentRegKey", BindingFlags.NonPublic | BindingFlags.Static)!;
        ((RegistryKey?)field.GetValue(null))?.Dispose();
        _settings = _testRoot.CreateSubKey("Settings", writable: true);
        field.SetValue(null, _settings);
    }

    private static void ThrowIfFailed(int error)
    {
        if (error != 0)
        {
            throw new Win32Exception(error);
        }
    }

    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern int RegOverridePredefKey(nint key, nint replacement);
}
