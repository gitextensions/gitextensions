using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public class ExclusiveTaskRunner : IDisposable
    {
        private readonly CancellationTokenSequence _cancellationTokenSequence = new();
        private JoinableTask? _runningTask;
        private readonly TaskManager _taskManager;

        public ExclusiveTaskRunner(TaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public void Dispose()
        {
            _cancellationTokenSequence.Dispose();
            _runningTask = null;
            GC.SuppressFinalize(this);
        }

        public void CancelCurrent()
        {
            _cancellationTokenSequence.CancelCurrent();
        }

        /// <summary>
        /// Exclusivly run <paramref name="asyncAction"/> asynchronously on the current thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// A previous task is cancelled and its termination is awaited before.
        /// </summary>
        public JoinableTask RunDetached(Func<CancellationToken, Task> asyncAction)
        {
            lock (_cancellationTokenSequence)
            {
                CancellationToken cancellationToken = _cancellationTokenSequence.Next();
                JoinableTask? previousTask = _runningTask;
                _runningTask = _taskManager.JoinableTaskFactory.RunAsync(async () =>
                    {
                        // Absolutely await the completion or the cancellation of the previous run, i.e. without cancellation
                        if (previousTask is not null)
                        {
                            await previousTask.JoinAsync();
                        }

                        await TaskManager.HandleExceptionsAsync(() => asyncAction(cancellationToken).WithCancellation(cancellationToken), _taskManager.ReportExceptionOnMainThreadAsync);
                    });
                return _runningTask;
            }
        }
    }
}
