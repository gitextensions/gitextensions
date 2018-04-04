using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

// This diagnostic is unnecessarily noisy when testing async patterns
#pragma warning disable VSTHRD104 // Offer async methods

namespace GitUITests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ControlThreadingExtensionsTests
    {
        [Test]
        public void ControlSwitchToMainThreadOnMainThread()
        {
            var form = new Form();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync();
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });

            var cancellationTokenSource = new CancellationTokenSource();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });
        }

        [Test]
        public void ControlSwitchToMainThreadOnMainThreadCompletesSynchronously()
        {
            var form = new Form();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);

            var awaiter = form.SwitchToMainThreadAsync().GetAwaiter();
            Assert.True(awaiter.IsCompleted);
            awaiter.GetResult();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);

            var cancellationTokenSource = new CancellationTokenSource();
            awaiter = form.SwitchToMainThreadAsync(cancellationTokenSource.Token).GetAwaiter();
            Assert.True(awaiter.IsCompleted);
            awaiter.GetResult();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
        }

        [Test]
        public void ControlSwitchToMainThreadOnBackgroundThread()
        {
            var form = new Form();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;
                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync();
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });

            var cancellationTokenSource = new CancellationTokenSource();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;
                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });
        }

        [Test]
        public void ControlDisposedBeforeSwitchOnMainThread()
        {
            var form = new Form();
            form.Dispose();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await form.SwitchToMainThreadAsync());
        }

        [Test]
        public void TokenCancelledBeforeSwitchOnMainThread()
        {
            var form = new Form();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token));
        }

        [Test]
        public void ControlDisposedAndTokenCancelledBeforeSwitchOnMainThread()
        {
            var form = new Form();
            form.Dispose();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            var exception = Assert.ThrowsAsync<TaskCanceledException>(async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token));

            // If both conditions are met on entry, the explicit cancellation token is the one used for the exception
            Assert.AreEqual(cancellationTokenSource.Token, exception.CancellationToken);
        }

        [Test]
        public void ControlDisposedAfterSwitchOnMainThread()
        {
            var form = new Form();

            var awaitable = form.SwitchToMainThreadAsync();

            form.Dispose();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await awaitable);
        }

        [Test]
        public void TokenCancelledAfterSwitchOnMainThread()
        {
            var form = new Form();
            var cancellationTokenSource = new CancellationTokenSource();

            var awaitable = form.SwitchToMainThreadAsync(cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await awaitable);
        }

        [Test]
        public void ControlDisposedBeforeSwitchOnBackgroundThread()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;

                var form = new Form();
                form.Dispose();

                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                Assert.ThrowsAsync<TaskCanceledException>(async () => await form.SwitchToMainThreadAsync());
            });
        }

        [Test]
        public void TokenCancelledBeforeSwitchOnBackgroundThread()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;

                var form = new Form();
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.Cancel();

                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                Assert.ThrowsAsync<TaskCanceledException>(async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token));
            });
        }

        [Test]
        public void ControlDisposedAfterSwitchOnBackgroundThread()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;

                var form = new Form();

                var awaitable = form.SwitchToMainThreadAsync();

                form.Dispose();

                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                Assert.ThrowsAsync<TaskCanceledException>(async () => await awaitable);
            });
        }

        [Test]
        public void TokenCancelledAfterSwitchOnBackgroundThread()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;

                var form = new Form();
                var cancellationTokenSource = new CancellationTokenSource();

                var awaitable = form.SwitchToMainThreadAsync(cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                Assert.ThrowsAsync<TaskCanceledException>(async () => await awaitable);
            });
        }
    }
}
