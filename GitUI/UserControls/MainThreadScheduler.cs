using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    internal sealed class MainThreadScheduler : LocalScheduler
    {
        internal static readonly MainThreadScheduler Instance = new MainThreadScheduler();

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var cancellationDisposable = new CancellationDisposable();
            var disposable = new SingleAssignmentDisposable();
            var normalizedTime = Scheduler.Normalize(dueTime);
            var token = cancellationDisposable.Token;
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await WithCancellationAsCompletionAsync(Task.Delay(normalizedTime, token)).ConfigureAwaitRunInline();
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    if (ThreadHelper.JoinableTaskContext.IsOnMainThread)
                    {
                        await Task.Yield();
                    }
                    else
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                    }

                    disposable.Disposable = action(this, state);
                });
            return new CompositeDisposable(cancellationDisposable, disposable);
        }

        private static Task WithCancellationAsCompletionAsync(Task task)
        {
            return task
                .ContinueWith(
                    t =>
                    {
                        if (t.Status == TaskStatus.Canceled)
                        {
                            return Task.CompletedTask;
                        }
                        else
                        {
                            return t;
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current)
                .Unwrap();
        }
    }
}
