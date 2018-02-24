using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Helpers
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
        public async Task RegularCase()
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
        public async Task Threads()
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
        public async Task CancelDuringLoad()
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
        public async Task CancelDuringDelay()
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
        public async Task DisposeCalledDuringOperation()
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
        public async Task DelayedLoad()
        {
            _loader.Delay = TimeSpan.FromMilliseconds(100);

            var sw = Stopwatch.StartNew();

            await _loader.Load(
                () => sw.Stop(),
                () => { });

            Assert.GreaterOrEqual(sw.Elapsed, _loader.Delay);
        }

        [Test]
        public async Task ErrorEventHandled()
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
        public void ErrorEventUnhandled()
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
        public void UseAfterDispose()
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