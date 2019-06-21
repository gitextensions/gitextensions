using System;
using System.IO;
using FluentAssertions;
using GitCommands.Settings;
using NUnit.Framework;

namespace GitCommandsTests.Settings
{
    [TestFixture]
    public class FileSettingsCacheTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("C:\\" + "\t")]
        [TestCase("boo")]
        public void ctor_FileWatcher_Path_should_not_set_if_invalid_dir(string settingsFilePath)
        {
            new MockFileSettingsCache(settingsFilePath, false).GetTestAccessor().FileSystemWatcher.Path.Should().BeNullOrEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("C:\\" + "\t")]
        [TestCase("boo")]
        public void ctor_FileWatcher_Filter_should_be_default_if_invalid_dir(string settingsFilePath)
        {
            new MockFileSettingsCache(settingsFilePath, false).GetTestAccessor().FileSystemWatcher.Filter.Should().Be("*.*");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("C:\\" + "\t")]
        [TestCase("boo")]
        public void ctor_FileWatcher_EnableRaisingEvents_should_be_false_if_invalid_dir(string settingsFilePath)
        {
            new MockFileSettingsCache(settingsFilePath, false).GetTestAccessor().FileSystemWatcher.EnableRaisingEvents.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("C:\\" + "\t")]
        [TestCase("boo")]
        public void ctor_CanEnableFileWatcher_should_be_false_if_invalid_dir(string settingsFilePath)
        {
            new MockFileSettingsCache(settingsFilePath, false).GetTestAccessor().CanEnableFileWatcher.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("C:\\" + "\t")]
        public void SaveImpl_should_throw_if_invalid_path(string settingsFilePath)
        {
            var cache = new MockFileSettingsCache(settingsFilePath, false).GetTestAccessor();
            cache.SetLastModificationDate(DateTime.Now);
            ((Action)(() => cache.SaveImpl())).Should().Throw<Exception>();
        }

        private class MockFileSettingsCache : FileSettingsCache
        {
            public MockFileSettingsCache(string settingsFilePath, bool autoSave = true)
                : base(settingsFilePath, autoSave)
            {
            }

            protected override void ClearImpl()
            {
                throw new System.NotImplementedException();
            }

            protected override string GetValueImpl(string key)
            {
                throw new System.NotImplementedException();
            }

            protected override void ReadSettings(string fileName)
            {
                throw new System.NotImplementedException();
            }

            protected override void SetValueImpl(string key, string value)
            {
                throw new System.NotImplementedException();
            }

            protected override void WriteSettings(string fileName)
            {
            }
        }
    }
}