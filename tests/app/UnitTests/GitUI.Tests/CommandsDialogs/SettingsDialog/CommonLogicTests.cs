using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.SettingsDialog;
using NSubstitute;

namespace GitUITests.CommandsDialogs.SettingsDialog;

[TestFixture]
public class CommonLogicTests
{
    private string _workingDir;
    private string _gitDir;

    [SetUp]
    public void SetUp()
    {
        _workingDir = Path.Combine(Path.GetTempPath(), $"GitExtensionsTest_{Guid.NewGuid():N}");
        _gitDir = Path.Combine(_workingDir, ".git");
        Directory.CreateDirectory(_gitDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_workingDir))
        {
            Directory.Delete(_workingDir, recursive: true);
        }
    }

    [Test]
    public void Dispose_must_release_the_settings_caches_it_owns()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDir);
        module.GitCommonDirectory.Returns(_gitDir);

        CommonLogic commonLogic = new(module);
        FileSettingsCache[] caches = [.. commonLogic.GetTestAccessor().OwnedSettingsCaches.OfType<FileSettingsCache>()];

        // The settings dialog creates these with useSharedCache: false, i.e. it owns them alone,
        // and each of them watches the directory of its settings file for as long as it lives.
        caches.Should().NotBeEmpty();
        caches.Select(cache => cache.GetTestAccessor().FileSystemWatcher.EnableRaisingEvents)
              .Should().Contain(true, "the repository is watched while the dialog is open");

        commonLogic.Dispose();

        foreach (FileSettingsCache cache in caches)
        {
            cache.GetTestAccessor().FileSystemWatcher.EnableRaisingEvents.Should()
                 .BeFalse("no watcher may outlive the dialog which owns it");
        }
    }
}
