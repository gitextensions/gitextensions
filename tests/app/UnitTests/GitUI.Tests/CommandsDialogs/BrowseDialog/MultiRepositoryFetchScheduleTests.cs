using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

namespace GitUITests.CommandsDialogs.BrowseDialog;

public sealed class MultiRepositoryFetchScheduleTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    [Test]
    public void ShouldFetch_returns_true_when_idle_threshold_is_reached()
    {
        MultiRepositoryFetchSchedule schedule = new();

        schedule.ShouldFetch(Now, enabled: true, IdleThreshold, IdleThreshold, Interval).Should().BeTrue();
    }

    [Test]
    public void ShouldFetch_waits_for_interval_after_attempt()
    {
        MultiRepositoryFetchSchedule schedule = new();
        schedule.MarkAttempt(Now);

        schedule.ShouldFetch(Now.AddMinutes(29), enabled: true, IdleThreshold, IdleThreshold, Interval).Should().BeFalse();
        schedule.ShouldFetch(Now.AddMinutes(30), enabled: true, IdleThreshold, IdleThreshold, Interval).Should().BeTrue();
    }

    [Test]
    public void ShouldFetch_resets_cycle_after_user_activity()
    {
        MultiRepositoryFetchSchedule schedule = new();
        schedule.MarkAttempt(Now);

        schedule.ShouldFetch(Now.AddMinutes(1), enabled: true, TimeSpan.Zero, IdleThreshold, Interval).Should().BeFalse();
        schedule.ShouldFetch(Now.AddMinutes(6), enabled: true, IdleThreshold, IdleThreshold, Interval).Should().BeTrue();
    }

    [Test]
    public void ShouldFetch_returns_false_when_disabled()
    {
        MultiRepositoryFetchSchedule schedule = new();

        schedule.ShouldFetch(Now, enabled: false, TimeSpan.FromHours(1), IdleThreshold, Interval).Should().BeFalse();
    }
}
