using CommonTestUtils;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid;

[TestFixture]
public sealed class IndexWatcherTests
{
    [Test]
    public void Reset_should_ignore_late_notifications_of_changes_made_before()
    {
        using GitModuleTestHelper repo = new();
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Module.Returns(repo.Module);
        using IndexWatcher indexWatcher = new(uiCommandsSource);
        IndexWatcher.TestAccessor accessor = indexWatcher.GetTestAccessor();
        // Outside of the watched paths, so that only the injected notifications are handled
        string refPath = Path.Join(repo.TemporaryPath, "branch");
        File.WriteAllText(refPath, "");

        indexWatcher.Reset();

        // Notification delivered after Reset for a change made before, e.g. by the fetch that triggered the refresh
        File.SetLastWriteTimeUtc(refPath, DateTime.UtcNow.AddMinutes(-1));
        accessor.RaiseChanged(refPath);
        indexWatcher.IndexChanged.Should().BeFalse();

        File.SetLastWriteTimeUtc(refPath, DateTime.UtcNow.AddMinutes(1));
        accessor.RaiseChanged(refPath);
        indexWatcher.IndexChanged.Should().BeTrue();

        // A change made just after Reset can get an older time stamp because the file system clock is coarse,
        // hence the cutoff is earlier than the time of the Reset
        indexWatcher.Reset();
        indexWatcher.IndexChanged.Should().BeFalse();
        File.SetLastWriteTimeUtc(refPath, accessor.LastResetUtc.AddTicks(-1));
        accessor.RaiseChanged(refPath);
        indexWatcher.IndexChanged.Should().BeFalse();
        File.SetLastWriteTimeUtc(refPath, accessor.LastResetUtc);
        accessor.RaiseChanged(refPath);
        indexWatcher.IndexChanged.Should().BeTrue();
    }

    [Test]
    public void Notification_of_a_no_longer_existing_file_should_use_the_time_of_its_directory()
    {
        using GitModuleTestHelper repo = new();
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Module.Returns(repo.Module);
        using IndexWatcher indexWatcher = new(uiCommandsSource);
        IndexWatcher.TestAccessor accessor = indexWatcher.GetTestAccessor();

        // Outside of the watched paths, so that only the injected notifications are handled
        string refsPath = Path.Join(repo.TemporaryPath, "refs");
        Directory.CreateDirectory(refsPath);
        string lockPath = Path.Join(refsPath, "stash.lock"); // renamed to "stash" before the notification is handled

        indexWatcher.Reset();

        // Renamed before Reset
        Directory.SetLastWriteTimeUtc(refsPath, DateTime.UtcNow.AddMinutes(-1));
        accessor.RaiseChanged(lockPath);
        indexWatcher.IndexChanged.Should().BeFalse();

        // Renamed after Reset
        Directory.SetLastWriteTimeUtc(refsPath, DateTime.UtcNow.AddMinutes(1));
        accessor.RaiseChanged(lockPath);
        indexWatcher.IndexChanged.Should().BeTrue();
    }
}
