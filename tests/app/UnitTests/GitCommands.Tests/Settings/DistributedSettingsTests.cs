using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitCommandsTests.Settings;

/// <summary>
///  Tests for <see cref="DistributedSettings"/> built with the same layering as
///  <c>CommonLogic</c>, where each level uses its own independent settings file.
/// </summary>
internal sealed class DistributedSettingsTests
{
    private const string _emptySettingsFileContent = """<?xml version="1.0" encoding="utf-8"?><dictionary />""";
    private const string _testSettingName = "test.setting";
    private const string _globalValue = "global-value";
    private const string _distributedValue = "distributed-value";
    private const string _localValue = "local-value";

    private string _globalFilePath = null!;
    private string _distributedFilePath = null!;
    private string _localFilePath = null!;
    private GitExtSettingsCache _globalCache = null!;
    private GitExtSettingsCache _distributedCache = null!;
    private GitExtSettingsCache _localCache = null!;
    private DistributedSettings _globalSettings = null!;
    private DistributedSettings _distributedSettings = null!;
    private DistributedSettings _localSettings = null!;
    private DistributedSettings _effectiveSettings = null!;

    [SetUp]
    public void SetUp()
    {
        _globalFilePath = Path.GetTempFileName();
        _distributedFilePath = Path.GetTempFileName();
        _localFilePath = Path.GetTempFileName();

        File.WriteAllText(_globalFilePath, _emptySettingsFileContent);
        File.WriteAllText(_distributedFilePath, _emptySettingsFileContent);
        File.WriteAllText(_localFilePath, _emptySettingsFileContent);

        CreateLayeredSettings();
    }

    [TearDown]
    public void TearDown()
    {
        _globalCache.Dispose();
        _distributedCache.Dispose();
        _localCache.Dispose();
        File.Delete(_globalFilePath);
        File.Delete(_globalFilePath + ".backup");
        File.Delete(_distributedFilePath);
        File.Delete(_distributedFilePath + ".backup");
        File.Delete(_localFilePath);
        File.Delete(_localFilePath + ".backup");
    }

    private void CreateLayeredSettings()
    {
        // Mirror the construction in CommonLogic exactly:
        // Global and each level use their own isolated cache (useSharedCache: false / autoSave: false).
        _globalCache = new GitExtSettingsCache(_globalFilePath, autoSave: false);
        _distributedCache = new GitExtSettingsCache(_distributedFilePath, autoSave: false);
        _localCache = new GitExtSettingsCache(_localFilePath, autoSave: false);

        _globalSettings = new DistributedSettings(lowerPriority: null, _globalCache, SettingLevel.Global);
        _distributedSettings = new DistributedSettings(lowerPriority: null, _distributedCache, SettingLevel.Distributed);
        _localSettings = new DistributedSettings(lowerPriority: null, _localCache, SettingLevel.Local);

        // Effective chain mirrors CommonLogic: Effective (local cache) -> Distributed (distributed cache) -> Global (global cache)
        _effectiveSettings = new DistributedSettings(
            lowerPriority: new DistributedSettings(lowerPriority: _globalSettings, _distributedCache, SettingLevel.Distributed),
            _localCache,
            SettingLevel.Effective);
    }

    private object SaveAndReadBack()
    {
        _effectiveSettings.Save();

        return new
        {
            global = ReadSettingFrom(_globalFilePath),
            distributed = ReadSettingFrom(_distributedFilePath),
            local = ReadSettingFrom(_localFilePath),
        };

        string? ReadSettingFrom(string path)
        {
            using GitExtSettingsCache cache = new(path, autoSave: false);
            return cache.TryGetValue(_testSettingName, out string? value) ? value : null;
        }
    }

    [Test]
    public async Task GetValue_on_effective_should_return_null_when_no_level_has_value()
    {
        _effectiveSettings.GetValue(_testSettingName).Should().BeNull();

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.GetValue_null_when_no_level_has_value");
    }

    [Test]
    public async Task GetValue_on_effective_should_return_global_value_when_only_global_is_set(
        [Values("", _globalValue)] string effectiveValue)
    {
        _globalSettings.SetValue(_testSettingName, effectiveValue);

        _effectiveSettings.GetValue(_testSettingName).Should().Be(effectiveValue);

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName($"DistributedSettingsTests.GetValue_global_when_only_global_is_set_{effectiveValue}");
    }

    [Test]
    public async Task GetValue_on_effective_should_return_distributed_value_overriding_global(
        [Values(null, _globalValue)] string? globalValue,
        [Values("", _distributedValue)] string effectiveValue)
    {
        _globalSettings.SetValue(_testSettingName, globalValue);
        _distributedSettings.SetValue(_testSettingName, effectiveValue);

        _effectiveSettings.GetValue(_testSettingName).Should().Be(effectiveValue);

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName($"DistributedSettingsTests.GetValue_distributed_overriding_global_{globalValue ?? "null"}_{effectiveValue}");
    }

    [Test]
    public async Task GetValue_on_effective_should_return_local_value_overriding_distributed_and_global(
        [Values(null, _globalValue)] string? globalValue,
        [Values(null, _distributedValue)] string? distributedValue,
        [Values("", _localValue)] string effectiveValue)
    {
        _globalSettings.SetValue(_testSettingName, globalValue);
        _distributedSettings.SetValue(_testSettingName, distributedValue);
        _localSettings.SetValue(_testSettingName, effectiveValue);

        _effectiveSettings.GetValue(_testSettingName).Should().Be(effectiveValue);

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName($"DistributedSettingsTests.GetValue_local_overriding_{globalValue ?? "null"}_{distributedValue ?? "null"}_{effectiveValue}");
    }

    [Test(Description = "If this test should fail, it can be removed because SetValue on effective is not used.")]
    public async Task SetValue_on_effective_should_write_to_global_when_no_level_has_value()
    {
        // When no level has an explicit value, effective delegates to global (the lowest level).
        _effectiveSettings.SetValue(_testSettingName, _globalValue);

        _globalSettings.GetValue(_testSettingName).Should().Be(_globalValue);
        _distributedSettings.GetValue(_testSettingName).Should().BeNull();
        _localSettings.GetValue(_testSettingName).Should().BeNull();

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_writes_to_global_when_no_level_has_value");
    }

    [Test(Description = "If this test should fail, it can be removed because SetValue on effective is not used.")]
    public async Task SetValue_on_effective_should_write_to_local_when_distributed_has_a_different_value()
    {
        // When distributed has an explicit value the effective-level write goes to local
        // to override it without touching the distributed (shared) file.
        _distributedSettings.SetValue(_testSettingName, _distributedValue);

        _effectiveSettings.SetValue(_testSettingName, _localValue);

        _localSettings.GetValue(_testSettingName).Should().Be(_localValue);
        _distributedSettings.GetValue(_testSettingName).Should().Be(_distributedValue);
        _globalSettings.GetValue(_testSettingName).Should().BeNull();

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_writes_to_local_when_distributed_differs");
    }

    [Test(Description = "If this test should fail, it can be removed because SetValue on effective is not used.")]
    public async Task SetValue_on_effective_should_not_write_to_local_when_distributed_has_same_value()
    {
        // When distributed already has the same value, no write is needed at the local level.
        _distributedSettings.SetValue(_testSettingName, _distributedValue);

        _effectiveSettings.SetValue(_testSettingName, _distributedValue);

        _localSettings.GetValue(_testSettingName).Should().BeNull();
        _distributedSettings.GetValue(_testSettingName).Should().Be(_distributedValue);

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_no_write_to_local_when_distributed_same");
    }

    [Test]
    public async Task GetValue_on_each_level_should_be_independent()
    {
        _globalSettings.SetValue(_testSettingName, _globalValue);
        _distributedSettings.SetValue(_testSettingName, _distributedValue);
        _localSettings.SetValue(_testSettingName, _localValue);

        _globalSettings.GetValue(_testSettingName).Should().Be(_globalValue);
        _distributedSettings.GetValue(_testSettingName).Should().Be(_distributedValue);
        _localSettings.GetValue(_testSettingName).Should().Be(_localValue);

        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.GetValue_each_level_independent");
    }

    [Test]
    public async Task SetValue_null_on_each_level_should_be_independent()
    {
        _globalSettings.SetValue(_testSettingName, _globalValue);
        _distributedSettings.SetValue(_testSettingName, _distributedValue);
        _localSettings.SetValue(_testSettingName, _localValue);
        _effectiveSettings.Save();

        _localSettings.SetValue(_testSettingName, null);
        _effectiveSettings.GetValue(_testSettingName).Should().Be(_distributedValue);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_each_level_independent.local_null");

        _distributedSettings.SetValue(_testSettingName, null);
        _effectiveSettings.GetValue(_testSettingName).Should().Be(_globalValue);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_each_level_independent.distributed_null");

        _globalSettings.SetValue(_testSettingName, null);
        _effectiveSettings.GetValue(_testSettingName).Should().BeNull();
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_each_level_independent.global_null");
    }

    [Test]
    public async Task SetValue_modified_on_each_level_should_be_independent()
    {
        _globalSettings.SetValue(_testSettingName, _globalValue);
        _distributedSettings.SetValue(_testSettingName, _distributedValue);
        _localSettings.SetValue(_testSettingName, _localValue);
        _effectiveSettings.Save();

        const string globalModified = "global-modified";
        _globalSettings.SetValue(_testSettingName, globalModified);
        _globalSettings.GetValue(_testSettingName).Should().Be(globalModified);
        _effectiveSettings.GetValue(_testSettingName).Should().Be(_localValue);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_each_level_independent.global_modified");

        const string distributedModified = "distributed-modified";
        _distributedSettings.SetValue(_testSettingName, distributedModified);
        _distributedSettings.GetValue(_testSettingName).Should().Be(distributedModified);
        _effectiveSettings.GetValue(_testSettingName).Should().Be(_localValue);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_each_level_independent.distributed_modified");

        const string localModified = "local-modified";
        _localSettings.SetValue(_testSettingName, localModified);
        _localSettings.GetValue(_testSettingName).Should().Be(localModified);
        _effectiveSettings.GetValue(_testSettingName).Should().Be(localModified);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.SetValue_each_level_independent.local_modified");
    }

    [Test]
    public async Task GetValue_on_effective_level_shall_return_inherited_right_after_load()
    {
        _effectiveSettings.GetValue(_testSettingName).Should().BeNull();

        _globalSettings.SetValue(_testSettingName, _globalValue);

        _effectiveSettings.GetValue(_testSettingName).Should().Be(_globalValue);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.GetValue_on_effective.before");

        _globalCache.Dispose();
        _distributedCache.Dispose();
        _localCache.Dispose();
        CreateLayeredSettings();

        _effectiveSettings.GetValue(_testSettingName).Should().Be(_globalValue);
        await Verifier.Verify(SaveAndReadBack())
            .UseFileName("DistributedSettingsTests.GetValue_on_effective.after");
    }
}
