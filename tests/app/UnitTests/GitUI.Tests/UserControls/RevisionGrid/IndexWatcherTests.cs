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
        string refPath = Path.Join(repo.Module.GitCommonDirectory, "refs", "heads", "branch");
        File.WriteAllText(refPath, "");

        indexWatcher.Reset();

        // Notification delivered after Reset for a change made before, e.g. by the fetch that triggered the refresh
        File.SetLastWriteTimeUtc(refPath, DateTime.UtcNow.AddMinutes(-1));
        accessor.RaiseChanged(refPath);
        indexWatcher.IndexChanged.Should().BeFalse();

        File.SetLastWriteTimeUtc(refPath, DateTime.UtcNow.AddMinutes(1));
        accessor.RaiseChanged(refPath);
        indexWatcher.IndexChanged.Should().BeTrue();
    }
}
