using System.Runtime.InteropServices;
using CommonTestUtils;
using GitUI;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUITests;

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
    public async Task FileAndForgetReportsThreadException()
    {
        using ThreadExceptionHelper helper = new();
        Exception ex = new();

        ThrowExceptionAsync(ex).FileAndForget();

        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
        ClassicAssert.AreSame(ex, helper.Exception);
    }

    [Test]
    public async Task FileAndForgetIgnoresCancellationExceptions()
    {
        using ThreadExceptionHelper helper = new();
        Form form = new();
        form.Dispose();

        YieldOntoControlMainThreadAsync(form).FileAndForget();

        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
        ClassicAssert.Null(helper.Exception, helper.Message);
    }

    [Test]
    public void ThrowIfNotOnUIThread()
    {
        ClassicAssert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
        ThreadHelper.ThrowIfNotOnUIThread();
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await TaskScheduler.Default;

            ClassicAssert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);

            ClassicAssert.Throws<COMException>(() => ThreadHelper.ThrowIfNotOnUIThread());
        });
    }

    [Test]
    public void ThrowIfOnUIThread()
    {
        ClassicAssert.True(ThreadHelper.JoinableTaskContext.IsOnMainThread);
        ClassicAssert.Throws<COMException>(() => ThreadHelper.ThrowIfOnUIThread());
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await TaskScheduler.Default;

            ClassicAssert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);

            ThreadHelper.ThrowIfOnUIThread();
        });
    }

    [Test]
    public void CompletedResultThrowsIfNotCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        ClassicAssert.Throws<InvalidOperationException>(() => tcs.Task.CompletedResult());
    }

    [Test]
    public void CompletedResultReturnsResultIfCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetResult(1);
        ClassicAssert.AreEqual(1, tcs.Task.CompletedResult());
    }

    [Test]
    public void CompletedResultThrowsIfCancelled()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetCanceled();
        AggregateException actual = ClassicAssert.Throws<AggregateException>(() => tcs.Task.CompletedResult());
        ClassicAssert.IsInstanceOf<TaskCanceledException>(actual.InnerException);
    }

    [Test]
    public void CompletedResultThrowsIfFaulted()
    {
        TaskCompletionSource<int> tcs = new();
        Exception ex = new();
        tcs.SetException(ex);
        AggregateException actual = ClassicAssert.Throws<AggregateException>(() => tcs.Task.CompletedResult());
        ClassicAssert.AreSame(ex, actual.InnerException);
        ClassicAssert.AreEqual(1, actual.InnerExceptions.Count);
    }

    [Test]
    public void CompletedOrDefaultReturnsDefaultIfNotCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        ClassicAssert.AreEqual(0, tcs.Task.CompletedOrDefault());
    }

    [Test]
    public void CompletedOrDefaultReturnsResultIfCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetResult(1);
        ClassicAssert.AreEqual(1, tcs.Task.CompletedOrDefault());
    }

    [Test]
    public void CompletedOrDefaultThrowsIfCancelled()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetCanceled();
        AggregateException actual = ClassicAssert.Throws<AggregateException>(() => tcs.Task.CompletedOrDefault());
        ClassicAssert.IsInstanceOf<TaskCanceledException>(actual.InnerException);
    }

    [Test]
    public void CompletedOrDefaultThrowsIfFaulted()
    {
        TaskCompletionSource<int> tcs = new();
        Exception ex = new();
        tcs.SetException(ex);
        AggregateException actual = ClassicAssert.Throws<AggregateException>(() => tcs.Task.CompletedOrDefault());
        ClassicAssert.AreSame(ex, actual.InnerException);
        ClassicAssert.AreEqual(1, actual.InnerExceptions.Count);
    }

    [Test]
    [Apartment(ApartmentState.MTA)]
    public void JoinableTaskFactoryConfiguredForMTA()
    {
        ClassicAssert.AreEqual(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
        ClassicAssert.Null(SynchronizationContext.Current);
        ClassicAssert.NotNull(ThreadHelper.JoinableTaskContext);
        ClassicAssert.NotNull(ThreadHelper.JoinableTaskFactory);
        ClassicAssert.False(ThreadHelper.JoinableTaskContext.IsOnMainThread);
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
