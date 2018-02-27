using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using GitCommandsTests.Helpers;
using NUnit.Framework;

namespace GitCommandsTests
{
    public sealed class AsyncLoaderTests
    {
        private AsyncLoader _loader;

        [SetUp]
        public void SetUp()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

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
            var loadSignal = new SemaphoreSlim(0);

            var started = 0;
            var completed = 0;
            var task = _loader.Load(
                () =>
                {
                    started++;
                    loadSignal.Release();
                    Thread.Sleep(100);
                },
                () => completed++);

            Assert.True(await loadSignal.WaitAsync(1000));

            Assert.AreEqual(1, started);
            Assert.AreEqual(0, completed);

            await task;

            Assert.AreEqual(1, started);
            Assert.AreEqual(1, completed);

            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public async Task Load_performed_on_thread_pool_and_result_handled_via_callers_context()
        {
            var callerThread = Thread.CurrentThread;
            Thread loadThread = null;
            Thread continuationThread = null;

            Assert.False(callerThread.IsThreadPoolThread);

            using (var scheduler = new SingleThreadTaskScheduler())
            using (var loader = new AsyncLoader(scheduler))
            {
                await loader.Load(
                    () => loadThread = Thread.CurrentThread,
                    () => continuationThread = Thread.CurrentThread);
            }

            Assert.True(loadThread.IsThreadPoolThread);
            Assert.AreNotSame(loadThread, callerThread);
            Assert.AreNotSame(loadThread, continuationThread);
        }

        [Test]
        public async Task Cancel_during_load_means_callback_not_fired()
        {
            var loadSignal = new SemaphoreSlim(0);

            var started = 0;
            var completed = 0;
            var task = _loader.Load(
                () =>
                {
                    started++;
                    loadSignal.Release();
                    Thread.Sleep(100);
                },
                () => completed++);

            Assert.True(await loadSignal.WaitAsync(1000));

            Assert.AreEqual(1, started);
            Assert.AreEqual(0, completed);

            _loader.Cancel();

            await task;

            Assert.AreEqual(1, started);
            Assert.AreEqual(0, completed, "Should not have called the follow-up action");

            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public async Task Cancel_during_delay_means_load_not_fired()
        {
            // Deliberately use a long delay as cancellation should cut it short
            _loader.Delay = TimeSpan.FromMilliseconds(5000);

            var started = 0;
            var completed = 0;
            var task = _loader.Load(
                () => started++,
                () => completed++);

            await Task.Delay(50);

            Assert.AreEqual(0, started);
            Assert.AreEqual(0, completed);

            _loader.Cancel();

            await task;

            Assert.AreEqual(0, started);
            Assert.AreEqual(0, completed);

            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public async Task Dispose_during_load_means_callback_not_fired()
        {
            var loadSignal = new SemaphoreSlim(0);

            var started = 0;
            var completed = 0;
            var task = _loader.Load(
                () =>
                {
                    started++;
                    loadSignal.Release();
                    Thread.Sleep(100);
                },
                () => completed++);

            Assert.True(await loadSignal.WaitAsync(1000));

            Assert.AreEqual(1, started);
            Assert.AreEqual(0, completed);

            _loader.Dispose();

            await task;

            Assert.AreEqual(1, started);
            Assert.AreEqual(0, completed, "Should not have called the follow-up action");

            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public async Task Delay_causes_load_callback_to_be_deferred()
        {
            _loader.Delay = TimeSpan.FromMilliseconds(100);

            var sw = Stopwatch.StartNew();

            await _loader.Load(
                () => sw.Stop(),
                () => { });

            Assert.GreaterOrEqual(sw.Elapsed, _loader.Delay - TimeSpan.FromMilliseconds(10));
        }

        [Test]
        public async Task Error_raised_via_event_and_marked_as_handled_does_not_fault_task()
        {
            var observed = new List<Exception>();

            _loader.LoadingError += (_, e) =>
            {
                observed.Add(e.Exception);
                e.Handled = true;
            };

            var ex = new Exception();

            await _loader.Load(
                () => throw ex,
                Assert.Fail);

            Assert.AreEqual(1, observed.Count);
            Assert.AreSame(ex, observed[0]);
        }

        [Test]
        public void Error_raised_via_event_and_not_marked_as_handled_faults_task()
        {
            var observed = new List<Exception>();

            _loader.LoadingError += (_, e) =>
            {
                observed.Add(e.Exception);
                e.Handled = false;
            };

            var ex = new Exception();

            var oe = Assert.ThrowsAsync<Exception>(() => _loader.Load(
                () => throw ex,
                Assert.Fail));

            Assert.AreEqual(1, observed.Count);
            Assert.AreSame(ex, observed[0]);
            Assert.AreSame(oe, observed[0]);
        }

        [Test]
        public void Using_load_or_cancel_after_dispose_throws()
        {
            var loader = new AsyncLoader();

            // Safe to dispose multiple times
            loader.Dispose();
            loader.Dispose();
            loader.Dispose();

            // Any use after dispose should throw
            Assert.Throws<ObjectDisposedException>(() => loader.Load(() => { }, () => { }));
            Assert.Throws<ObjectDisposedException>(() => loader.Load(_ => { }, () => { }));
            Assert.Throws<ObjectDisposedException>(() => loader.Load(() => 1, i => { }));
            Assert.Throws<ObjectDisposedException>(() => loader.Load(_ => 1, i => { }));
            Assert.Throws<ObjectDisposedException>(() => loader.Cancel());
        }
    }
}