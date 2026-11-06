using CommonTestUtils;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid;

[Apartment(ApartmentState.STA)]
public class IndexWatcherTests
{
    private static readonly string _gitDirectory = Path.Join(AppContext.BaseDirectory, "repo", ".git");

    [Test]
    public async Task ResetAsync_should_run_the_git_command_in_the_background_and_reset_on_the_main_thread()
    {
        bool? resolvedOnMainThread = null;
        bool? resetOnMainThread = null;
        IGitModule module = CreateModule(() => resolvedOnMainThread ??= ThreadHelper.JoinableTaskContext.IsOnMainThread);
        using IndexWatcher watcher = CreateWatcher(module, out _);
        watcher.Changed += (_, _) => resetOnMainThread = ThreadHelper.JoinableTaskContext.IsOnMainThread;

        ThreadHelper.FileAndForget(() => watcher.ResetAsync(module, CancellationToken.None));
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        resolvedOnMainThread.Should().BeFalse();
        resetOnMainThread.Should().BeTrue();
    }

    [Test]
    public async Task ResetAsync_should_not_reset_if_another_repo_has_been_opened_meanwhile()
    {
        TaskCompletionSource resolving = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim repoOpened = new();
        IGitModule module = CreateModule(() =>
        {
            resolving.TrySetResult();
            repoOpened.Wait(AsyncTestHelper.UnexpectedTimeout);
        });
        using IndexWatcher watcher = CreateWatcher(module, out IGitUICommands uiCommands);
        int resets = 0;
        watcher.Changed += (_, _) => resets++;

        ThreadHelper.FileAndForget(() => watcher.ResetAsync(module, CancellationToken.None));
        await resolving.Task;
        IGitModule otherModule = CreateModule();
        uiCommands.Module.Returns(otherModule);
        repoOpened.Set();
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        resets.Should().Be(0);
    }

    [Test]
    public async Task ResetAsync_should_not_reset_if_cancelled_meanwhile()
    {
        TaskCompletionSource resolving = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim cancelled = new();
        IGitModule module = CreateModule(() =>
        {
            resolving.TrySetResult();
            cancelled.Wait(AsyncTestHelper.UnexpectedTimeout);
        });
        using IndexWatcher watcher = CreateWatcher(module, out _);
        int resets = 0;
        watcher.Changed += (_, _) => resets++;
        using CancellationTokenSource cancellationTokenSource = new();

        ThreadHelper.FileAndForget(() => watcher.ResetAsync(module, cancellationTokenSource.Token));
        await resolving.Task;
        await cancellationTokenSource.CancelAsync();
        cancelled.Set();
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        resets.Should().Be(0);
    }

    private static IGitModule CreateModule(Action? onGitCommonDirectory = null)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(true);
        module.WorkingDirGitDir.Returns(_gitDirectory);
        module.GitCommonDirectory.Returns(_ =>
        {
            onGitCommonDirectory?.Invoke();
            return _gitDirectory;
        });
        return module;
    }

    private static IndexWatcher CreateWatcher(IGitModule module, out IGitUICommands uiCommands)
    {
        uiCommands = Substitute.For<IGitUICommands>();
        uiCommands.Module.Returns(module);
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Returns(uiCommands);
        return new IndexWatcher(uiCommandsSource);
    }
}
