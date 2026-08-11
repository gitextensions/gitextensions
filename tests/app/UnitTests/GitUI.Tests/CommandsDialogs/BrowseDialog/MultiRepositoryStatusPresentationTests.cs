using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

namespace GitUITests.CommandsDialogs.BrowseDialog;

public sealed class MultiRepositoryStatusPresentationTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 11, 16, 30, 0, TimeSpan.FromHours(8));

    [Test]
    public void FormatFetchTimestamp_combines_relative_and_absolute_local_time()
    {
        DateTimeOffset timestamp = Now.AddMinutes(-5);

        string formatted = MultiRepositoryStatusPresentation.FormatFetchTimestamp(timestamp, Now);

        formatted.Should().Be($"5 分钟前（{timestamp.ToLocalTime():yyyy-MM-dd HH:mm}）");
    }

    [TestCase(30, "刚刚")]
    [TestCase(120, "2 分钟前")]
    [TestCase(7_200, "2 小时前")]
    [TestCase(172_800, "2 天前")]
    [TestCase(3_456_000, "1 个月前")]
    [TestCase(63_072_000, "2 年前")]
    public void FormatRelativeTime_uses_confirmed_thresholds(int elapsedSeconds, string expected)
        => MultiRepositoryStatusPresentation.FormatRelativeTime(Now.AddSeconds(-elapsedSeconds), Now).Should().Be(expected);

    [Test]
    public void FormatRelativeTime_treats_future_timestamp_as_just_now()
        => MultiRepositoryStatusPresentation.FormatRelativeTime(Now.AddMinutes(5), Now).Should().Be("刚刚");

    [Test]
    public void GetSynchronizationLabels_reports_diverged_ahead_and_behind()
    {
        MultiRepositoryStatus status = new()
        {
            RepositoryPath = @"C:\repo",
            Upstream = "origin/main",
            Ahead = 2,
            Behind = 3
        };

        MultiRepositoryStatusPresentation.GetSynchronizationLabels(status)
            .Select(label => label.Text)
            .Should().Equal("已分叉", "领先 2", "落后 3");
    }

    [Test]
    public void GetSynchronizationLabels_reports_synchronized_repository()
    {
        MultiRepositoryStatus status = new()
        {
            RepositoryPath = @"C:\repo",
            Upstream = "origin/main",
            Ahead = 0,
            Behind = 0
        };

        MultiRepositoryStatusPresentation.GetSynchronizationLabels(status).Single().Text.Should().Be("已同步");
    }

    [Test]
    public void Error_prefers_fetch_error_without_losing_status_error()
    {
        MultiRepositoryStatus status = new()
        {
            RepositoryPath = @"C:\repo",
            StatusError = "status failed",
            FetchError = "fetch failed"
        };

        status.Error.Should().Be("fetch failed");
        status.StatusError.Should().Be("status failed");
    }
}
