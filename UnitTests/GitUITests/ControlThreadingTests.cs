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
    public class ControlThreadingTests
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
        public void ControlDisposedAfterSwitchOnMainThread()
        {
            var form = new Form();

            var awaitable = form.SwitchToMainThreadAsync();

            form.Dispose();

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
    }
}
