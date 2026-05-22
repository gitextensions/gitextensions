using System.Runtime.InteropServices;
using CommonTestUtils;
using GitUI;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUITests;
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
        helper.Exception.Should().BeSameAs(ex);
    }

    [Test]
    public async Task FileAndForgetIgnoresCancellationExceptions()
    {
        using ThreadExceptionHelper helper = new();
        Form form = new();
        form.Dispose();

        YieldOntoControlMainThreadAsync(form).FileAndForget();

        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
        helper.Exception.Should().BeNull(helper.Message);
    }

    [Test]
    public async Task ThrowIfNotOnUIThread()
    {
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
#pragma warning disable VSTHRD109 // Switch instead of assert in async methods -- intentionally testing ThrowIfNotOnUIThread
        ThreadHelper.ThrowIfNotOnUIThread();
#pragma warning restore VSTHRD109

        await TaskScheduler.Default;

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();

        ((Action)(() => ThreadHelper.ThrowIfNotOnUIThread())).Should().Throw<COMException>();
    }

    [Test]
    public async Task ThrowIfOnUIThread()
    {
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeTrue();
        ((Action)(() => ThreadHelper.ThrowIfOnUIThread())).Should().Throw<COMException>();

        await TaskScheduler.Default;

        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();

        ThreadHelper.ThrowIfOnUIThread();
    }

    [Test]
    public void CompletedResultThrowsIfNotCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        ((Action)(() => tcs.Task.CompletedResult())).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void CompletedResultReturnsResultIfCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetResult(1);
        tcs.Task.CompletedResult().Should().Be(1);
    }

    [Test]
    public void CompletedResultThrowsIfCancelled()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetCanceled();
        Action act = () => tcs.Task.CompletedResult();
        AggregateException actual = act.Should().Throw<AggregateException>().Which;
        actual.InnerException.Should().BeOfType<TaskCanceledException>();
    }

    [Test]
    public void CompletedResultThrowsIfFaulted()
    {
        TaskCompletionSource<int> tcs = new();
        Exception ex = new();
        tcs.SetException(ex);
        Action act = () => tcs.Task.CompletedResult();
        AggregateException actual = act.Should().Throw<AggregateException>().Which;
        actual.InnerException.Should().BeSameAs(ex);
        actual.InnerExceptions.Count.Should().Be(1);
    }

    [Test]
    public void CompletedOrDefaultReturnsDefaultIfNotCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.Task.CompletedOrDefault().Should().Be(0);
    }

    [Test]
    public void CompletedOrDefaultReturnsResultIfCompleted()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetResult(1);
        tcs.Task.CompletedOrDefault().Should().Be(1);
    }

    [Test]
    public void CompletedOrDefaultThrowsIfCancelled()
    {
        TaskCompletionSource<int> tcs = new();
        tcs.SetCanceled();
        Action act = () => tcs.Task.CompletedOrDefault();
        AggregateException actual = act.Should().Throw<AggregateException>().Which;
        actual.InnerException.Should().BeOfType<TaskCanceledException>();
    }

    [Test]
    public void CompletedOrDefaultThrowsIfFaulted()
    {
        TaskCompletionSource<int> tcs = new();
        Exception ex = new();
        tcs.SetException(ex);
        Action act = () => tcs.Task.CompletedOrDefault();
        AggregateException actual = act.Should().Throw<AggregateException>().Which;
        actual.InnerException.Should().BeSameAs(ex);
        actual.InnerExceptions.Count.Should().Be(1);
    }

    [Test]
    [Apartment(ApartmentState.MTA)]
    public void JoinableTaskFactoryConfiguredForMTA()
    {
        Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.MTA);
        SynchronizationContext.Current.Should().BeNull();
        ThreadHelper.JoinableTaskContext.Should().NotBeNull();
        ThreadHelper.JoinableTaskFactory.Should().NotBeNull();
        ThreadHelper.JoinableTaskContext.IsOnMainThread.Should().BeFalse();
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

        public Exception Exception { get; private set; } = null!;

        [CanBeNull]
        public string Message => Exception?.Message!;

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
