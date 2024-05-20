using CommonTestUtils;
using GitUI;
using Microsoft.VisualStudio.Threading;

// This diagnostic is unnecessarily noisy when testing async patterns

namespace GitUITests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ControlThreadingExtensionsTests
    {
        [Test]
        public void ControlSwitchToMainThreadOnMainThread()
        {
            Form form = new();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync();
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });

            CancellationTokenSource cancellationTokenSource = new();
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
            Form form = new();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);

            ControlThreadingExtensions.ControlMainThreadAwaiter awaiter = form.SwitchToMainThreadAsync().GetAwaiter();
            Assert.True(awaiter.IsCompleted);
            awaiter.GetResult();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);

            CancellationTokenSource cancellationTokenSource = new();
            awaiter = form.SwitchToMainThreadAsync(cancellationTokenSource.Token).GetAwaiter();
            Assert.True(awaiter.IsCompleted);
            awaiter.GetResult();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
        }

        [Test]
        public void ControlSwitchToMainThreadOnBackgroundThread()
        {
            Form form = new();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;
                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync();
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });

            CancellationTokenSource cancellationTokenSource = new();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TaskScheduler.Default;
                Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
                await form.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            });
        }

        [Test]
        public async Task ControlDisposedBeforeSwitchOnMainThread()
        {
            Form form = new();
            form.Dispose();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await form.SwitchToMainThreadAsync());
        }

        [Test]
        public async Task TokenCancelledBeforeSwitchOnMainThread()
        {
            Form form = new();
            CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token));
        }

        [Test]
        public async Task ControlDisposedAndTokenCancelledBeforeSwitchOnMainThread()
        {
            Form form = new();
            form.Dispose();
            CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            OperationCanceledException exception = await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token));

            // If both conditions are met on entry, the explicit cancellation token is the one used for the exception
            Assert.AreEqual(cancellationTokenSource.Token, exception.CancellationToken);
        }

        [Test]
        public async Task ControlDisposedAfterSwitchOnMainThread()
        {
            Form form = new();

            ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync();

            form.Dispose();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await awaitable);
        }

        [Test]
        public async Task TokenCancelledAfterSwitchOnMainThread()
        {
            Form form = new();
            CancellationTokenSource cancellationTokenSource = new();

            ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync(cancellationTokenSource.Token);

            await cancellationTokenSource.CancelAsync();

            Assert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await awaitable);
        }

        [Test]
        public async Task ControlDisposedBeforeSwitchOnBackgroundThread()
        {
            Form form = new();
            form.Dispose();

            await TaskScheduler.Default;

            Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await form.SwitchToMainThreadAsync());
        }

        [Test]
        public async Task TokenCancelledBeforeSwitchOnBackgroundThread()
        {
            Form form = new();

            await TaskScheduler.Default;

            CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync();

            Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await form.SwitchToMainThreadAsync(cancellationTokenSource.Token));
        }

        [Test]
        [Ignore("Hangs")]
        public async Task ControlDisposedAfterSwitchOnBackgroundThread()
        {
            Form form = new();

            await TaskScheduler.Default;

            ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync();
#pragma warning disable VSTHRD103 // Call async methods when in an async method
            ThreadHelper.JoinableTaskFactory.Run(
#pragma warning restore VSTHRD103 // Call async methods when in an async method
                async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    form.Dispose();
                });

            Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await awaitable);
        }

        [Test]
        public async Task TokenCancelledAfterSwitchOnBackgroundThread()
        {
            Form form = new();

            await TaskScheduler.Default;

            CancellationTokenSource cancellationTokenSource = new();

            ControlThreadingExtensions.ControlMainThreadAwaitable awaitable = form.SwitchToMainThreadAsync(cancellationTokenSource.Token);

            await cancellationTokenSource.CancelAsync();

            Assert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await awaitable);
        }
    }
}
