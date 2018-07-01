using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public void FileAndForgetReportsThreadException()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var ex = new Exception();

                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    ThrowExceptionAsync(ex).FileAndForget();
                    return Task.CompletedTask;
                });

                JoinPendingOperations();
                Assert.AreSame(ex, helper.Exception);
            }
        }

        [Test]
        public void FileAndForgetIgnoresCancellationExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var form = new Form();
                form.Dispose();

                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    YieldOntoControlMainThreadAsync(form).FileAndForget();
                    return Task.CompletedTask;
                });

                JoinPendingOperations();
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        public void FileAndForgetFilterCanAllowExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var ex = new Exception();

                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    ThrowExceptionAsync(ex).FileAndForget(fileOnlyIf: e => e == ex);
                    return Task.CompletedTask;
                });

                JoinPendingOperations();
                Assert.AreSame(ex, helper.Exception);
            }
        }

        [Test]
        public void FileAndForgetFilterCanIgnoreExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var ex = new Exception();

                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    ThrowExceptionAsync(ex).FileAndForget(fileOnlyIf: e => e != ex);
                    return Task.CompletedTask;
                });

                JoinPendingOperations();
                Assert.Null(helper.Exception, helper.Message);
            }
        }

        [Test]
        public void FileAndForgetFilterIgnoresCancellationExceptions()
        {
            using (var helper = new ThreadExceptionHelper())
            {
                var form = new Form();
                form.Dispose();

                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    YieldOntoControlMainThreadAsync(form).FileAndForget(fileOnlyIf: ex => true);
                    return Task.CompletedTask;
                });

                JoinPendingOperations();
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
        {
            await Task.Yield();
        }

        private static void JoinPendingOperations()
        {
            // Since we are testing a FileAndForget method, we need to join all pending operations before continuing.
            // Note that ThreadHelper.JoinableTaskContext.Factory must be used to bypass the default behavior of
            // ThreadHelper.JoinableTaskFactory since the latter adds new tasks to the collection and would therefore
            // never complete.
            ThreadHelper.JoinableTaskContext?.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync());
        }

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
