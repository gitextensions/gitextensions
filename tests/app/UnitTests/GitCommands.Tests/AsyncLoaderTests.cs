using System.Diagnostics;
using CommonTestUtils;
using GitCommands;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitCommandsTests;

[Apartment(ApartmentState.STA)]
public sealed class AsyncLoaderTests
{
    private AsyncLoader _loader = null!;

    [SetUp]
    public void SetUp()
    {
        _loader = new AsyncLoader();
    }

    [TearDown]
    public void TearDown()
    {
        _loader.Dispose();
    }

    [Test]
    public async Task Load_should_execute_async_operation_and_callback()
    {
        SemaphoreSlim loadSignal = new(0);
        SemaphoreSlim completeSignal = new(0);

        int started = 0;
        int completed = 0;
        Task task = _loader.LoadAsync(
            () =>
            {
                started++;
                loadSignal.Release();
                completeSignal.Wait();
            },
            () => completed++);

        (await loadSignal.WaitAsync(1000)).Should().BeTrue();

        started.Should().Be(1);
        completed.Should().Be(0);

        completeSignal.Release();

        await task;

        started.Should().Be(1);
        completed.Should().Be(1);

        task.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Test]
    public async Task Load_performed_on_thread_pool_and_result_handled_via_callers_context()
    {
        Thread callerThread = Thread.CurrentThread;
        Thread? loadThread = null;
        Thread? continuationThread = null;

        callerThread.IsThreadPoolThread.Should().BeFalse();

        using AsyncLoader loader = new();
        await loader.LoadAsync(
            () => loadThread = Thread.CurrentThread,
            () => continuationThread = Thread.CurrentThread);

        loadThread!.IsThreadPoolThread.Should().BeTrue();
        callerThread.Should().NotBeSameAs(loadThread);
        continuationThread.Should().NotBeSameAs(loadThread);
    }

    [Test]
    public async Task Cancel_during_load_means_callback_not_fired()
    {
        SemaphoreSlim loadSignal = new(0);
        SemaphoreSlim completeSignal = new(0);

        int started = 0;
        int completed = 0;
        Task task = _loader.LoadAsync(
            () =>
            {
                started++;
                loadSignal.Release();
                completeSignal.Wait();
            },
            () => completed++);

        (await loadSignal.WaitAsync(1000)).Should().BeTrue();

        started.Should().Be(1);
        completed.Should().Be(0);

        _loader.Cancel();
        completeSignal.Release();

        await task;

        started.Should().Be(1);
        completed.Should().Be(0, "Should not have called the follow-up action");

        task.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Test]
    public async Task Cancel_during_delay_means_load_not_fired()
    {
        // Deliberately use a long delay as cancellation should cut it short
        _loader.Delay = TimeSpan.FromMilliseconds(5000);

        int started = 0;
        int completed = 0;
        Task task = _loader.LoadAsync(
            () => started++,
            () => completed++);

        await Task.Delay(50);

        started.Should().Be(0);
        completed.Should().Be(0);

        _loader.Cancel();

        await task;

        started.Should().Be(0);
        completed.Should().Be(0);

        task.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Test]
    public async Task Dispose_during_load_means_callback_not_fired()
    {
        SemaphoreSlim loadSignal = new(0);
        SemaphoreSlim completeSignal = new(0);

        int started = 0;
        int completed = 0;
        Task task = _loader.LoadAsync(
            () =>
            {
                started++;
                loadSignal.Release();
                completeSignal.Wait();
            },
            () => completed++);

        (await loadSignal.WaitAsync(1000)).Should().BeTrue();

        started.Should().Be(1);
        completed.Should().Be(0);

        _loader.Dispose();
        completeSignal.Release();

        await task;

        started.Should().Be(1);
        completed.Should().Be(0, "Should not have called the follow-up action");

        task.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Test]
    public async Task Delay_causes_load_callback_to_be_deferred()
    {
        _loader.Delay = TimeSpan.FromMilliseconds(200);

        Stopwatch sw = Stopwatch.StartNew();

        await _loader.LoadAsync(
            () => sw.Stop(),
            () => { });

        sw.Elapsed.Should().BeGreaterThanOrEqualTo(_loader.Delay - TimeSpan.FromMilliseconds(20));
    }

    [Test]
    public async Task Error_raised_via_event_and_marked_as_handled_does_not_fault_task()
    {
        List<Exception> observed = [];

        _loader.LoadingError += (_, e) =>
        {
            observed.Add(e.Exception);
            e.Handled = true;
        };

        Exception ex = new();

        await _loader.LoadAsync(
            () => throw ex,
            Assert.Fail);

        observed.Count.Should().Be(1);
        observed[0].Should().BeSameAs(ex);
    }

    [Test]
    public void Error_raised_via_event_when_no_error_handler_installed_faults_task()
    {
        Exception ex = new();

        JoinableTask loadTask = ThreadHelper.JoinableTaskFactory.RunAsync(() =>
            _loader.LoadAsync(
                loadContent: () => throw ex,
                onLoaded: Assert.Fail));

        Action act = () => loadTask.Join();
        Exception oe = act.Should().Throw<Exception>().Which;
        ex.Should().BeSameAs(oe);
    }

    [Test]
    public void Error_raised_via_event_and_not_marked_as_handled_faults_task()
    {
        List<Exception> observed = [];

        _loader.LoadingError += (_, e) =>
        {
            observed.Add(e.Exception);
            e.Handled = false;
        };

        Exception ex = new();

        JoinableTask loadTask = ThreadHelper.JoinableTaskFactory.RunAsync(() => _loader.LoadAsync(
            () => throw ex,
            Assert.Fail));

        Action act = () => loadTask.Join();
        Exception oe = act.Should().Throw<Exception>().Which;

        observed.Count.Should().Be(1);
        observed[0].Should().BeSameAs(ex);
        observed[0].Should().BeSameAs(oe);
    }

    [Test]
    public async Task Using_load_or_cancel_after_dispose_throws()
    {
        AsyncLoader loader = new();

        // Safe to dispose multiple times
        loader.Dispose();
        loader.Dispose();
        loader.Dispose();

        // Any use after dispose should throw
        Func<Task> act1 = async () => await loader.LoadAsync(() => { }, () => { });
        await act1.Should().ThrowAsync<ObjectDisposedException>();
        Func<Task> act2 = async () => await loader.LoadAsync(() => 1, i => { });
        await act2.Should().ThrowAsync<ObjectDisposedException>();
        Func<Task> act3 = async () => await loader.LoadAsync(_ => { }, () => { });
        await act3.Should().ThrowAsync<ObjectDisposedException>();
        Func<Task> act4 = async () => await loader.LoadAsync(_ => 1, i => { });
        await act4.Should().ThrowAsync<ObjectDisposedException>();
        Action act5 = () => loader.Cancel();
        act5.Should().Throw<ObjectDisposedException>();
    }
}
