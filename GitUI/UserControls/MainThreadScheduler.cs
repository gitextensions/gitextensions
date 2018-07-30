﻿using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
                    await Task.Delay(normalizedTime, token).ConfigureAwaitRunInline();
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
    }
}
