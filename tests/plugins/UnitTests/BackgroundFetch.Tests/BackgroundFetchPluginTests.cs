using System.Reactive.Concurrency;
using CommonTestUtils;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Plugins.BackgroundFetch;
using NSubstitute;

namespace BackgroundFetchTests;

public sealed class BackgroundFetchPluginTests
{
    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    private HistoricalScheduler _scheduler = null!;
    private List<TimeSpan> _triggers = null!;
    private int _isGitRunningCallCount;

    private TimeSpan Now => _scheduler.Clock - DateTimeOffset.MinValue;

    [SetUp]
    public void SetUp()
    {
        _scheduler = new HistoricalScheduler(DateTimeOffset.MinValue);
        _triggers = [];
        _isGitRunningCallCount = 0;
    }

    [Test]
    public void PostSettings_should_be_ignored_after_the_plugin_was_unregistered()
    {
        // StartSettingsDialog raises the subscribers of PostSettings from before the settings dialog was shown,
        // so the handler of the plugin can still be called after the plugin has been unregistered
        IGitPluginSettingsContainer settingsContainer = Substitute.For<IGitPluginSettingsContainer>();
        settingsContainer.GetSettingsSource().Returns(new MemorySettings());
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        EventHandler<GitUIPostActionEventArgs>? postSettings = null;
        commands.When(c => c.PostSettings += Arg.Any<EventHandler<GitUIPostActionEventArgs>>())
            .Do(callInfo => postSettings = callInfo.Arg<EventHandler<GitUIPostActionEventArgs>>());

        BackgroundFetchPlugin plugin = new() { SettingsContainer = settingsContainer };
        plugin.Register(commands);
        plugin.Unregister(commands);

        postSettings.Should().NotBeNull();
        postSettings!.Invoking(handler => handler(commands, new GitUIPostActionEventArgs(ownerForm: null, commands, actionDone: true)))
            .Should().NotThrow();
    }

    [Test]
    public void CreateFetchTriggersObservable_should_trigger_on_each_tick_if_git_is_not_running()
    {
        using IDisposable subscription = SubscribeToFetchTriggers(isGitRunningUntil: TimeSpan.Zero);

        _scheduler.AdvanceBy(TimeSpan.FromSeconds(35));

        _triggers.Should().Equal(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30));
    }

    [Test]
    public void CreateFetchTriggersObservable_should_trigger_once_per_tick_as_soon_as_git_has_exited()
    {
        using IDisposable subscription = SubscribeToFetchTriggers(isGitRunningUntil: TimeSpan.FromSeconds(25));

        _scheduler.AdvanceBy(TimeSpan.FromSeconds(45));

        _triggers.Should().Equal(TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(40));
    }

    [Test]
    public void CreateFetchTriggersObservable_should_poll_with_a_single_poller_while_git_keeps_running()
    {
        using IDisposable subscription = SubscribeToFetchTriggers(isGitRunningUntil: TimeSpan.MaxValue);

        TimeSpan duration = TimeSpan.FromHours(1);
        _scheduler.AdvanceBy(duration);

        _triggers.Should().BeEmpty();

        // One check per tick plus one per second. Accumulating pollers would grow quadratically instead.
        int ticks = (int)(duration / _interval);
        int polls = (int)duration.TotalSeconds;
        _isGitRunningCallCount.Should().BeLessThanOrEqualTo(ticks + polls);
    }

    [Test]
    public void CreateFetchTriggersObservable_should_stop_polling_when_disposed()
    {
        IDisposable subscription = SubscribeToFetchTriggers(isGitRunningUntil: TimeSpan.MaxValue);
        _scheduler.AdvanceBy(TimeSpan.FromSeconds(15));
        int callCount = _isGitRunningCallCount;
        callCount.Should().BePositive();

        subscription.Dispose();
        _scheduler.AdvanceBy(TimeSpan.FromMinutes(5));

        _isGitRunningCallCount.Should().Be(callCount);
    }

    [Test]
    public void WaitForRunningGitExitsObservable_should_signal_immediately_and_complete_if_git_is_not_running()
    {
        bool completed = false;
        using IDisposable subscription = BackgroundFetchPlugin
            .WaitForRunningGitExitsObservable(() => IsGitRunning(until: TimeSpan.Zero), _scheduler)
            .Subscribe(_ => _triggers.Add(Now), () => completed = true);

        _triggers.Should().Equal(TimeSpan.Zero);
        completed.Should().BeTrue();
    }

    [Test]
    public void WaitForRunningGitExitsObservable_should_signal_once_and_complete_when_git_has_exited()
    {
        bool completed = false;
        using IDisposable subscription = BackgroundFetchPlugin
            .WaitForRunningGitExitsObservable(() => IsGitRunning(until: TimeSpan.FromSeconds(3)), _scheduler)
            .Subscribe(_ => _triggers.Add(Now), () => completed = true);

        _scheduler.AdvanceBy(TimeSpan.FromSeconds(10));

        _triggers.Should().Equal(TimeSpan.FromSeconds(3));
        completed.Should().BeTrue();
        _isGitRunningCallCount.Should().Be(4);
    }

    private bool IsGitRunning(TimeSpan until)
    {
        _isGitRunningCallCount++;
        return Now < until;
    }

    private IDisposable SubscribeToFetchTriggers(TimeSpan isGitRunningUntil)
        => BackgroundFetchPlugin
            .CreateFetchTriggersObservable(() => IsGitRunning(isGitRunningUntil), _interval, _interval, _scheduler)
            .Subscribe(_ => _triggers.Add(Now));
}
