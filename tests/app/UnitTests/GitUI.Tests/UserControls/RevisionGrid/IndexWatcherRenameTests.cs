using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid;

[TestFixture]
public class IndexWatcherRenameTests
{
    private string _gitDir = null!;

    [SetUp]
    public void SetUp()
    {
        _gitDir = Path.Join(Path.GetTempPath(), "GitExtensionsTests", Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Join(_gitDir, "refs"));
        File.WriteAllText(Path.Join(_gitDir, "index"), "initial");
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_gitDir, recursive: true);
    }

    // Git does not write "index" in place: it writes "index.lock" and renames it over "index".
    [Test]
    public void Should_report_change_when_index_lock_is_renamed_over_index()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(true);
        module.WorkingDirGitDir.Returns(_gitDir);
        module.GitCommonDirectory.Returns(_gitDir);
        IGitUICommands uiCommands = Substitute.For<IGitUICommands>();
        uiCommands.Module.Returns(module);
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Returns(uiCommands);

        using IndexWatcher watcher = new(uiCommandsSource);
        using ManualResetEventSlim changed = new(initialState: false);
        watcher.Changed += (_, e) =>
        {
            if (e.IsIndexChanged)
            {
                changed.Set();
            }
        };

        watcher.Reset();

        string lockFile = Path.Join(_gitDir, "index.lock");
        File.WriteAllText(lockFile, "updated by git");
        File.Move(lockFile, Path.Join(_gitDir, "index"), overwrite: true);

        changed.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
    }
}
