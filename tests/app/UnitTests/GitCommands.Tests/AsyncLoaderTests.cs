﻿using System.Diagnostics;
using CommonTestUtils;
using GitCommands;
using GitUI;
using Microsoft.VisualStudio.Threading;

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

                ClassicAssert.True(await loadSignal.WaitAsync(1000));

                ClassicAssert.AreEqual(1, started);
                ClassicAssert.AreEqual(0, completed);

                completeSignal.Release();

                await task;

                ClassicAssert.AreEqual(1, started);
                ClassicAssert.AreEqual(1, completed);

                ClassicAssert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Load_performed_on_thread_pool_and_result_handled_via_callers_context()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                Thread callerThread = Thread.CurrentThread;
                Thread loadThread = null;
                Thread continuationThread = null;

                ClassicAssert.False(callerThread.IsThreadPoolThread);

                using AsyncLoader loader = new();
                await loader.LoadAsync(
                    () => loadThread = Thread.CurrentThread,
                    () => continuationThread = Thread.CurrentThread);

                ClassicAssert.True(loadThread.IsThreadPoolThread);
                ClassicAssert.AreNotSame(loadThread, callerThread);
                ClassicAssert.AreNotSame(loadThread, continuationThread);
            });
        }

        [Test]
        public void Cancel_during_load_means_callback_not_fired()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
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

                ClassicAssert.True(await loadSignal.WaitAsync(1000));

                ClassicAssert.AreEqual(1, started);
                ClassicAssert.AreEqual(0, completed);

                _loader.Cancel();
                completeSignal.Release();

                await task;

                ClassicAssert.AreEqual(1, started);
                ClassicAssert.AreEqual(0, completed, "Should not have called the follow-up action");

                ClassicAssert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Cancel_during_delay_means_load_not_fired()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                // Deliberately use a long delay as cancellation should cut it short
                _loader.Delay = TimeSpan.FromMilliseconds(5000);

                int started = 0;
                int completed = 0;
                Task task = _loader.LoadAsync(
                    () => started++,
                    () => completed++);

                await Task.Delay(50);

                ClassicAssert.AreEqual(0, started);
                ClassicAssert.AreEqual(0, completed);

                _loader.Cancel();

                await task;

                ClassicAssert.AreEqual(0, started);
                ClassicAssert.AreEqual(0, completed);

                ClassicAssert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Dispose_during_load_means_callback_not_fired()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
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

                ClassicAssert.True(await loadSignal.WaitAsync(1000));

                ClassicAssert.AreEqual(1, started);
                ClassicAssert.AreEqual(0, completed);

                _loader.Dispose();
                completeSignal.Release();

                await task;

                ClassicAssert.AreEqual(1, started);
                ClassicAssert.AreEqual(0, completed, "Should not have called the follow-up action");

                ClassicAssert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            });
        }

        [Test]
        public void Delay_causes_load_callback_to_be_deferred()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                _loader.Delay = TimeSpan.FromMilliseconds(100);

                Stopwatch sw = Stopwatch.StartNew();

                await _loader.LoadAsync(
                    () => sw.Stop(),
                    () => { });

                ClassicAssert.GreaterOrEqual(sw.Elapsed, _loader.Delay - TimeSpan.FromMilliseconds(10));
            });
        }

        [Test]
        public void Error_raised_via_event_and_marked_as_handled_does_not_fault_task()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
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
                    ClassicAssert.Fail);

                ClassicAssert.AreEqual(1, observed.Count);
                ClassicAssert.AreSame(ex, observed[0]);
            });
        }

        [Test]
        public void Error_raised_via_event_when_no_error_handler_installed_faults_task()
        {
            Exception ex = new();

            JoinableTask loadTask = ThreadHelper.JoinableTaskFactory.RunAsync(() =>
                _loader.LoadAsync(
                    loadContent: () => throw ex,
                    onLoaded: ClassicAssert.Fail));

            Exception oe = ClassicAssert.Throws<Exception>(() => loadTask.Join());
            ClassicAssert.AreSame(oe, ex);
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
                ClassicAssert.Fail));

            Exception oe = ClassicAssert.Throws<Exception>(() => loadTask.Join());

            ClassicAssert.AreEqual(1, observed.Count);
            ClassicAssert.AreSame(ex, observed[0]);
            ClassicAssert.AreSame(oe, observed[0]);
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
            await AssertEx.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(() => { }, () => { }));
            await AssertEx.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(() => 1, i => { }));
            await AssertEx.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(_ => { }, () => { }));
            await AssertEx.ThrowsAsync<ObjectDisposedException>(() => loader.LoadAsync(_ => 1, i => { }));
            ClassicAssert.Throws<ObjectDisposedException>(() => loader.Cancel());
        }
    }
}
