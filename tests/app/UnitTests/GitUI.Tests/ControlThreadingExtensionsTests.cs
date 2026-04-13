using CommonTestUtils;
using GitUI;
using Microsoft.VisualStudio.Threading;

// This diagnostic is unnecessarily noisy when testing async patterns

namespace GitUITests;
[Apartment(ApartmentState.STA)]
public class ControlThreadingExtensionsTests
{
    [Test]
    public async Task ControlSwitchToMainThreadOnMainThread()
    {
        using Form form = new();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        await form.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
    }

    [Test]
    public async Task ControlSwitchToMainThreadOnMainThread_with_cancellation_token()
    {
        using Form form = new();

        using CancellationTokenSource cancellationTokenSource = new();
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
    }

    [Test]
    public void ControlSwitchToMainThreadOnMainThreadCompletesSynchronously()
    {
        using Form form = new();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();

        ControlThreadingExtensions.ControlMainThreadAwaiter awaiter = form.SwitchToMainThreadAsync().GetAwaiter();
        awaiter.IsCompleted.Should().BeTrue();
        awaiter.GetResult();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();

        using CancellationTokenSource cancellationTokenSource = new();
        awaiter = form.SwitchToMainThreadAsync(cancellationTokenSource.Token).GetAwaiter();
        awaiter.IsCompleted.Should().BeTrue();
        awaiter.GetResult();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
    }

    [Test]
    public async Task ControlSwitchToMainThreadOnBackgroundThread()
    {
        using Form form = new();

        await TaskScheduler.Default;
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
        await form.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
    }

    [Test]
    public async Task ControlSwitchToMainThreadOnBackgroundThread_with_cancellation_token()
    {
        using Form form = new();

        using CancellationTokenSource cancellationTokenSource = new();
        await TaskScheduler.Default;
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
        await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
    }

    [Test]
    public async Task ControlDisposedBeforeSwitchOnMainThread()
    {
        Form form = new();
        form.Dispose();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        Func<Task> act = async () => await form.SwitchToMainThreadAsync();
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task TokenCancelledBeforeSwitchOnMainThread()
    {
        using Form form = new();
        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        Func<Task> act = async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task ControlDisposedAndTokenCancelledBeforeSwitchOnMainThread()
    {
        Form form = new();
        form.Dispose();
        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        Func<Task> act = async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
        OperationCanceledException exception = (await act.Should().ThrowAsync<OperationCanceledException>()).Which;

        // If both conditions are met on entry, the explicit cancellation token is the one used for the exception
        exception.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Test]
    public async Task ControlDisposedAfterSwitchOnMainThread()
    {
        Form form = new();

        ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync();

        form.Dispose();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        Func<Task> act = async () => await awaitable;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task TokenCancelledAfterSwitchOnMainThread()
    {
        using Form form = new();
        using CancellationTokenSource cancellationTokenSource = new();

        ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync(cancellationTokenSource.Token);

        await cancellationTokenSource.CancelAsync();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        Func<Task> act = async () => await awaitable;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task ControlDisposedBeforeSwitchOnBackgroundThread()
    {
        Form form = new();
        form.Dispose();

        await TaskScheduler.Default;

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
        Func<Task> act = async () => await form.SwitchToMainThreadAsync();
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task TokenCancelledBeforeSwitchOnBackgroundThread()
    {
        using Form form = new();

        await TaskScheduler.Default;

        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
        Func<Task> act = async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task ControlDisposedAfterSwitchOnBackgroundThread()
    {
        Form form = new();

        await TaskScheduler.Default;

        ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync();
        await ThreadHelper.JoinableTaskFactory.RunAsync(
            async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                form.Dispose();
            });

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
        Func<Task> act = async () => await awaitable;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task TokenCancelledAfterSwitchOnBackgroundThread()
    {
        using Form form = new();

        await TaskScheduler.Default;

        using CancellationTokenSource cancellationTokenSource = new();

        ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync(cancellationTokenSource.Token);

        await cancellationTokenSource.CancelAsync();

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
        Func<Task> act = async () => await awaitable;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
