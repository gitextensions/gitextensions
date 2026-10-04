using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AvaloniaThreadingExtensionsTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void InvokeAndForget_should_run_synchronous_UI_work_before_returning()
    {
        Control control = new();
        List<string> order = [];

        order.Add("before");
        control.InvokeAndForget(() => order.Add("action"));
        order.Add("after");

        order.Should().Equal("before", "action", "after");
    }

    [AvaloniaTest]
    public void InvokeAndForget_should_ignore_pre_cancelled_work()
    {
        Control control = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        bool invoked = false;

        control.InvokeAndForget(() => invoked = true, cancellation.Token);

        invoked.Should().BeFalse();
    }

    [AvaloniaTest]
    [Category("P0_6")]
    public async Task LoadAsync_should_load_on_a_worker_and_return_to_the_actual_Avalonia_UI_thread()
    {
        Dispatcher.UIThread.CheckAccess().Should().BeTrue();
        SynchronizationContext? callerContext = SynchronizationContext.Current;
        callerContext.Should().NotBeNull();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext(Thread.CurrentThread, callerContext);
        int callerThreadId = Environment.CurrentManagedThreadId;
        int loadThreadId = 0;
        int callbackThreadId = 0;
        bool loadedOnThreadPool = false;
        bool loadedOnUiThread = true;
        bool callbackOnUiThread = false;

        using AsyncLoader loader = new();
        await loader.LoadAsync(
            () =>
            {
                loadThreadId = Environment.CurrentManagedThreadId;
                loadedOnThreadPool = Thread.CurrentThread.IsThreadPoolThread;
                loadedOnUiThread = Dispatcher.UIThread.CheckAccess();
            },
            () =>
            {
                callbackThreadId = Environment.CurrentManagedThreadId;
                callbackOnUiThread = Dispatcher.UIThread.CheckAccess();
            });

        loadedOnThreadPool.Should().BeTrue();
        loadedOnUiThread.Should().BeFalse();
        loadThreadId.Should().NotBe(callerThreadId);
        callbackThreadId.Should().Be(callerThreadId);
        callbackOnUiThread.Should().BeTrue();
    }
}
