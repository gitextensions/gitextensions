﻿using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    internal sealed class MainThreadScheduler : LocalScheduler
    {
        internal static readonly MainThreadScheduler Instance = new();

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            CancellationDisposable cancellationDisposable = new();
            SingleAssignmentDisposable disposable = new();
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
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                            return t;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current)
                .Unwrap();
        }
    }
}
