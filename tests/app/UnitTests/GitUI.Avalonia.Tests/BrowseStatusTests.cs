using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class BrowseStatusTests
{
    private bool _showAheadBehindData;

    [SetUp]
    public void SetUp()
    {
        _showAheadBehindData = AppSettings.ShowAheadBehindData;
        AppSettings.ShowAheadBehindData = true;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.ShowAheadBehindData = _showAheadBehindData;
    }

    [AvaloniaTest]
    public void Repository_state_visualiser_should_cover_the_original_status_matrix()
    {
        RepoStateVisualiser visualiser = new();

        visualiser.Invoke(allChangedFiles: null).Should().Be(RepoStateVisualiser.Unknown);
        visualiser.Invoke([]).Should().Be(RepoStateVisualiser.Clean);
        visualiser.Invoke([CreateStatus(staged: true)]).Should().Be(RepoStateVisualiser.Staged);
        visualiser.Invoke([CreateStatus(), CreateStatus(staged: true)]).Should().Be(RepoStateVisualiser.Mixed);
        visualiser.Invoke([CreateStatus()]).Should().Be(RepoStateVisualiser.Dirty);
        visualiser.Invoke([CreateStatus(tracked: false)]).Should().Be(RepoStateVisualiser.UntrackedOnly);
        visualiser.Invoke([CreateStatus(submodule: true)]).Should().Be(RepoStateVisualiser.DirtySubmodules);
    }

    [AvaloniaTest]
    public void Push_button_should_show_ahead_and_behind_information_and_restore_its_default()
    {
        const string branchName = "main";
        ToolStripPushButton button = new();
        Dictionary<string, AheadBehindData> data = new()
        {
            [branchName] = new AheadBehindData(branchName, "refs/remotes/origin/main", "3", "2"),
        };

        button.DisplayAheadBehindInformation(data, branchName, "Ctrl+Up");

        button.GetTestAccessor().GetButtonText().Should().Be("3↑ 2↓");
        button.GetTestAccessor().IsIconOnly().Should().BeFalse();
        button.Icon.Should().BeSameAs(Images.Unstage);
        ToolTip.GetTip(button)?.ToString().Should().Contain("3 new commit(s) will be pushed")
            .And.Contain("2 commit(s) should be integrated")
            .And.Contain("Ctrl+Up");

        button.DisplayAheadBehindInformation(aheadBehindData: null, branchName, shortcut: string.Empty);

        button.GetTestAccessor().IsIconOnly().Should().BeTrue();
        button.Icon.Should().BeSameAs(Images.Push);
        ToolTip.GetTip(button)?.ToString().Should().Be("Push");
    }

    private static GitItemStatus CreateStatus(
        bool staged = false,
        bool tracked = true,
        bool submodule = false)
        => new("file.txt")
        {
            Staged = staged ? StagedStatus.Index : StagedStatus.WorkTree,
            IsTracked = tracked,
            IsSubmodule = submodule,
        };
}
