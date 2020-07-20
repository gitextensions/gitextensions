using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using GitUI;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public sealed class ThreadHelperTests
    {
        private static async Task YieldOntoControlMainThreadAsync(Control control)
        {
            await control.SwitchToMainThreadAsync();
        }

        private static async Task ThrowExceptionAsync(Exception ex)
        {
            await Task.Yield();
            throw ex;
        }

        [Test]
        [Ignore("Hangs")]
        public async Task FileAndForgetReportsThreadException()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var ex = new Exception();

                ThrowExceptionAsync(ex).FileAndForget();

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                Assert.AreSame(ex, helper.Exception);
            }
        }

        [Test]
        public async Task FileAndForgetIgnoresCancellationExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var form = new Form();
                form.Dispose();

                YieldOntoControlMainThreadAsync(form).FileAndForget();

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        [Ignore("Hangs")]
        public async Task FileAndForgetFilterCanAllowExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var ex = new Exception();

                ThrowExceptionAsync(ex).FileAndForget(fileOnlyIf: e => e == ex);

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                Assert.AreSame(ex, helper.Exception);
            }
        }

        [Test]
        [Ignore("Hangs")]
        public async Task FileAndForgetFilterCanIgnoreExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var ex = new Exception();

                ThrowExceptionAsync(ex).FileAndForget(fileOnlyIf: e => e != ex);

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        public async Task FileAndForgetFilterIgnoresCancellationExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var form = new Form();
                form.Dispose();

                YieldOntoControlMainThreadAsync(form).FileAndForget(fileOnlyIf: ex => true);

                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        public void ThrowIfNotOnUIThread()
        {
            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            ThreadHelper.ThrowIfNotOnUIThread();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;

                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);

                Assert.Throws<COMException>(() => ThreadHelper.ThrowIfNotOnUIThread());
            });
        }

        [Test]
        public void ThrowIfOnUIThread()
        {
            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            Assert.Throws<COMException>(() => ThreadHelper.ThrowIfOnUIThread());
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;

                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);

                ThreadHelper.ThrowIfOnUIThread();
            });
        }

        [Test]
        public void CompletedResultThrowsIfNotCompleted()
        {
            var tcs = new TaskCompletionSource<int>();
            Assert.Throws<InvalidOperationException>(() => tcs.Task.CompletedResult());
        }

        [Test]
        public void CompletedResultReturnsResultIfCompleted()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(1);
            Assert.AreEqual(1, tcs.Task.CompletedResult());
        }

        [Test]
        public void CompletedResultThrowsIfCancelled()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            var actual = Assert.Throws<AggregateException>(() => tcs.Task.CompletedResult());
            Assert.IsInstanceOf<TaskCanceledException>(actual.InnerException);
        }

        [Test]
        public void CompletedResultThrowsIfFaulted()
        {
            var tcs = new TaskCompletionSource<int>();
            var ex = new Exception();
            tcs.SetException(ex);
            var actual = Assert.Throws<AggregateException>(() => tcs.Task.CompletedResult());
            Assert.AreSame(ex, actual.InnerException);
            Assert.AreEqual(1, actual.InnerExceptions.Count);
        }

        [Test]
        public void CompletedOrDefaultReturnsDefaultIfNotCompleted()
        {
            var tcs = new TaskCompletionSource<int>();
            Assert.AreEqual(0, tcs.Task.CompletedOrDefault());
        }

        [Test]
        public void CompletedOrDefaultReturnsResultIfCompleted()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(1);
            Assert.AreEqual(1, tcs.Task.CompletedOrDefault());
        }

        [Test]
        public void CompletedOrDefaultThrowsIfCancelled()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            var actual = Assert.Throws<AggregateException>(() => tcs.Task.CompletedOrDefault());
            Assert.IsInstanceOf<TaskCanceledException>(actual.InnerException);
        }

        [Test]
        public void CompletedOrDefaultThrowsIfFaulted()
        {
            var tcs = new TaskCompletionSource<int>();
            var ex = new Exception();
            tcs.SetException(ex);
            var actual = Assert.Throws<AggregateException>(() => tcs.Task.CompletedOrDefault());
            Assert.AreSame(ex, actual.InnerException);
            Assert.AreEqual(1, actual.InnerExceptions.Count);
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        public void JoinableTaskFactoryConfiguredForMTA()
        {
            Assert.AreEqual(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
            Assert.Null(SynchronizationContext.Current);
            Assert.NotNull(ThreadHelper.JoinableTaskContext);
            Assert.NotNull(ThreadHelper.JoinableTaskFactory);
            Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        public async Task AllowAwaitForAsynchronousMTATest()
            => await Task.Yield();

        private sealed class ThreadExceptionHelper : IDisposable
        {
            public ThreadExceptionHelper()
            {
                Application.ThreadException += HandleThreadException;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            [CanBeNull]
            public string Message => Exception?.Message;

            public void Dispose()
            {
                Application.ThreadException -= HandleThreadException;
            }

            private void HandleThreadException(object sender, ThreadExceptionEventArgs e)
            {
                Exception = e.Exception;
            }
        }
    }
}
