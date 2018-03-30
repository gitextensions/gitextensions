using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using GitUI;
using NUnit.Framework;

namespace GitCommandsTests
{
    [Apartment(ApartmentState.STA)]
    public sealed class AsyncLoaderTests
    {
        private AsyncLoader _loader;

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
        public void Load_should_execute_async_operation_and_callback()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var loadSignal = new SemaphoreSlim(0);
                var completeSignal = new SemaphoreSlim(0);

                var started = 0;
                var completed = 0;
                var task = _loader.LoadAsync(
                    () =>
                    {
                        started++;
                        loadSignal.Release();
                        completeSignal.Wait();
                    },
                    () => completed++);

                Assert.True(await loadSignal.WaitAsync(1000));

                Assert.AreEqual(1, started);
                Assert.AreEqual(0, completed);

                completeSignal.Release();

                await task;

                Assert.AreEqual(1, started);
                Assert.AreEqual(1, completed);

                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Load_performed_on_thread_pool_and_result_handled_via_callers_context()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var callerThread = Thread.CurrentThread;
                Thread loadThread = null;
                Thread continuationThread = null;

                Assert.False(callerThread.IsThreadPoolThread);

                using (var loader = new AsyncLoader())
                {
                    await loader.LoadAsync(
                        () => loadThread = Thread.CurrentThread,
                        () => continuationThread = Thread.CurrentThread);
                }

                Assert.True(loadThread.IsThreadPoolThread);
                Assert.AreNotSame(loadThread, callerThread);
                Assert.AreNotSame(loadThread, continuationThread);
            });
        }

        [Test]
        public void Cancel_during_load_means_callback_not_fired()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var loadSignal = new SemaphoreSlim(0);
                var completeSignal = new SemaphoreSlim(0);

                var started = 0;
                var completed = 0;
                var task = _loader.LoadAsync(
                    () =>
                    {
                        started++;
                        loadSignal.Release();
                        completeSignal.Wait();
                    },
                    () => completed++);

                Assert.True(await loadSignal.WaitAsync(1000));

                Assert.AreEqual(1, started);
                Assert.AreEqual(0, completed);

                _loader.Cancel();
                completeSignal.Release();

                await task;

                Assert.AreEqual(1, started);
                Assert.AreEqual(0, completed, "Should not have called the follow-up action");

                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Cancel_during_delay_means_load_not_fired()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                // Deliberately use a long delay as cancellation should cut it short
                _loader.Delay = TimeSpan.FromMilliseconds(5000);

                var started = 0;
                var completed = 0;
                var task = _loader.LoadAsync(
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
            });
        }

        [Test]
        public void Dispose_during_load_means_callback_not_fired()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var loadSignal = new SemaphoreSlim(0);
                var completeSignal = new SemaphoreSlim(0);

                var started = 0;
                var completed = 0;
                var task = _loader.LoadAsync(
                    () =>
                    {
                        started++;
                        loadSignal.Release();
                        completeSignal.Wait();
                    },
                    () => completed++);

                Assert.True(await loadSignal.WaitAsync(1000));

                Assert.AreEqual(1, started);
                Assert.AreEqual(0, completed);

                _loader.Dispose();
                completeSignal.Release();

                await task;

                Assert.AreEqual(1, started);
                Assert.AreEqual(0, completed, "Should not have called the follow-up action");

                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Delay_causes_load_callback_to_be_deferred()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                _loader.Delay = TimeSpan.FromMilliseconds(100);

                var sw = Stopwatch.StartNew();

                await _loader.LoadAsync(
                    () => sw.Stop(),
                    () => { });

                Assert.GreaterOrEqual(sw.Elapsed, _loader.Delay - TimeSpan.FromMilliseconds(10));
            });
        }

        [Test]
        public void Error_raised_via_event_and_marked_as_handled_does_not_fault_task()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var observed = new List<Exception>();

                _loader.LoadingError += (_, e) =>
                {
                    observed.Add(e.Exception);
                    e.Handled = true;
                };

                var ex = new Exception();

                await _loader.LoadAsync(
                    () => throw ex,
                    Assert.Fail);

                Assert.AreEqual(1, observed.Count);
                Assert.AreSame(ex, observed[0]);
            });
        }

        [Test]
        public void Error_raised_via_event_when_no_error_handler_installed_does_not_fault_task()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var ex = new Exception();

                await _loader.LoadAsync(
                    () => throw ex,
                    Assert.Fail);
            });
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

            var loadTask = ThreadHelper.JoinableTaskFactory.RunAsync(() => _loader.LoadAsync(
                () => throw ex,
                Assert.Fail));

            var oe = Assert.Throws<Exception>(() => loadTask.Join());

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
            Assert.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(() => { }, () => { }));
            Assert.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(() => 1, i => { }));
            Assert.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(_ => { }, () => { }));
            Assert.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(_ => 1, i => { }));
            Assert.Throws<ObjectDisposedException>(() => loader.Cancel());
        }
    }
}