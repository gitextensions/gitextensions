using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace GitUI.UserControls
{
    internal sealed class MainThreadScheduler : LocalScheduler
    {
        internal static readonly MainThreadScheduler Instance = new MainThreadScheduler();

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var disposable = new SingleAssignmentDisposable();
            var normalizedTime = Scheduler.Normalize(dueTime);
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await Task.Delay(normalizedTime);
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    disposable.Disposable = action(this, state);
                });
            return disposable;
        }
    }
}
