using GitCommands.Settings;

namespace GitCommandsTests.Settings;
public class FileSettingsCacheTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("boo")]
    public void ctor_FileWatcher_Path_should_not_set_if_invalid_dir(string? settingsFilePath)
    {
        new MockFileSettingsCache(settingsFilePath!, false).GetTestAccessor().FileSystemWatcher.Path.Should().BeNullOrEmpty();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("boo")]
    public void ctor_FileWatcher_Filter_should_be_default_if_invalid_dir(string? settingsFilePath)
    {
        new MockFileSettingsCache(settingsFilePath!, false).GetTestAccessor().FileSystemWatcher.Filter.Should().Be("*");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("boo")]
    public void ctor_FileWatcher_EnableRaisingEvents_should_be_false_if_invalid_dir(string? settingsFilePath)
    {
        new MockFileSettingsCache(settingsFilePath!, false).GetTestAccessor().FileSystemWatcher.EnableRaisingEvents.Should().BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    public void SaveImpl_should_throw_if_invalid_path(string? settingsFilePath)
    {
        FileSettingsCache.TestAccessor cache = new MockFileSettingsCache(settingsFilePath!, false).GetTestAccessor();
        cache.SetLastModificationDate(DateTime.Now);
        ((Action)(() => cache.SaveImpl())).Should().Throw<SaveSettingsException>();
    }

    [Test]
    public void SaveImpl_should_create_folder_if_absent()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        string settingsFilePath = Path.Combine(tempPath, "GitExtensions.settings");

        try
        {
            FileSettingsCache.TestAccessor cache = new MockFileSettingsCache(settingsFilePath!, false).GetTestAccessor();
            cache.SetLastModificationDate(DateTime.Now);

            Directory.Exists(tempPath).Should().BeFalse();
            File.Exists(settingsFilePath).Should().BeFalse();

            cache.SaveImpl();

            Directory.Exists(tempPath).Should().BeTrue();
            File.Exists(settingsFilePath).Should().BeTrue();
        }
        finally
        {
            // clean up
            try
            {
                Directory.Delete(tempPath, true);
            }
            catch
            {
                // no-op
            }
        }
    }

    [TestCase(true, ".unreadable")]
    [TestCase(false, ".backup")]
    public void SaveImpl_should_preserve_a_settings_file_which_could_not_be_read(bool readFails, string expectedExtension)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        string settingsFilePath = Path.Combine(tempPath, "GitExtensions.settings");
        const string existingSettings = "the settings which are already stored";
        const string existingBackup = "the last backup which could be read";

        try
        {
            Directory.CreateDirectory(tempPath);
            File.WriteAllText(settingsFilePath, existingSettings);
            File.WriteAllText(settingsFilePath + ".backup", existingBackup);

            ReadableFileSettingsCache cache = new(settingsFilePath, readFails);
            cache.Load();

            // Loading stores the read time in UTC, so the modification has to be dated accordingly
            cache.GetTestAccessor().SetLastModificationDate(DateTime.UtcNow.AddMinutes(1));

            cache.GetTestAccessor().SaveImpl();

            File.ReadAllText(settingsFilePath + expectedExtension).Should().Be(existingSettings);
            File.ReadAllText(settingsFilePath + ".backup").Should()
                .Be(readFails ? existingBackup : existingSettings,
                    "a backup which could be read must not be replaced by a file which could not");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }

    private sealed class ReadableFileSettingsCache : FileSettingsCache
    {
        private readonly bool _readFails;

        public ReadableFileSettingsCache(string settingsFilePath, bool readFails)
            : base(settingsFilePath, autoSave: false)
        {
            _readFails = readFails;
        }

        protected override void ClearImpl()
        {
        }

        protected override string? GetValueImpl(string key) => null;

        protected override void ReadSettings(string fileName)
        {
            if (_readFails)
            {
                throw new InvalidOperationException("the settings file is damaged");
            }
        }

        protected override void SetValueImpl(string key, string? value)
        {
        }

        protected override void WriteSettings(string fileName)
        {
            File.WriteAllText(fileName, "the settings which are written now");
        }
    }

    private class MockFileSettingsCache : FileSettingsCache
    {
        public MockFileSettingsCache(string settingsFilePath, bool autoSave = true)
            : base(settingsFilePath!, autoSave)
        {
        }

        protected override void ClearImpl()
        {
            throw new NotImplementedException();
        }

        protected override string GetValueImpl(string key)
        {
            throw new NotImplementedException();
        }

        protected override void ReadSettings(string fileName)
        {
            throw new NotImplementedException();
        }

        protected override void SetValueImpl(string key, string? value)
        {
            throw new NotImplementedException();
        }

        protected override void WriteSettings(string fileName)
        {
        }
    }
}
