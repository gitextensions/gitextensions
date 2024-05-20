using System.Diagnostics;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public class TaskManager
    {
        private static readonly CancellationTokenSequence _switchToMainThreadCancellationTokenSequence = new();

        private static CancellationToken _switchToMainThreadCancellationToken = _switchToMainThreadCancellationTokenSequence.Next();

        private readonly JoinableTaskCollection _joinableTaskCollection;

        public TaskManager(JoinableTaskContext joinableTaskContext)
        {
            JoinableTaskContext = joinableTaskContext;
            _joinableTaskCollection = joinableTaskContext.CreateCollection();
            JoinableTaskFactory = joinableTaskContext.CreateFactory(_joinableTaskCollection);
        }

        public JoinableTaskContext JoinableTaskContext { get; init; }

        public JoinableTaskFactory JoinableTaskFactory { get; init; }

        /// <summary>
        /// Handle all exceptions from asynchronous execution of <paramref name="asyncAction"/> by calling <paramref name="handleExceptionAsync"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        internal static async Task HandleExceptionsAsync(Func<Task> asyncAction, Func<Exception, Task> handleExceptionAsync)
        {
            try
            {
                await asyncAction();
            }
            catch (OperationCanceledException)
            {
                // Do not rethrow these
            }
            catch (Exception ex)
            {
                await handleExceptionAsync(ex);
            }
        }

        /// <summary>
        /// Handle all exceptions from synchronous execution of <paramref name="action"/> by calling <paramref name="handleException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        public static void HandleExceptions(Action action, Action<Exception> handleException)
        {
            try
            {
                action();
            }
            catch (OperationCanceledException)
            {
                // Do not rethrow these
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
        }

        internal static Func<Task> AsyncAction(Action action)
        {
            return () =>
                {
                    action();
                    return Task.CompletedTask;
                };
        }

        internal static void CancelSwitchToMainThread()
        {
            _switchToMainThreadCancellationToken = _switchToMainThreadCancellationTokenSequence.Next();
        }

        /// <summary>
        /// Asynchronously run <paramref name="asyncAction"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        public void FileAndForget(Func<Task> asyncAction)
        {
            _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;
                    await HandleExceptionsAsync(asyncAction, ReportExceptionOnMainThreadAsync);
                });
        }

        /// <summary>
        /// Asynchronously run <paramref name="action"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        public void FileAndForget(Action action)
        {
            FileAndForget(AsyncAction(action));
        }

        /// <summary>
        /// Asynchronously run <paramref name="task"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        public void FileAndForget(Task task)
        {
            TimeSpan infiniteTimeout = new(-TimeSpan.TicksPerMillisecond);
            FileAndForget(() => task.WaitAsync(infiniteTimeout));
        }

        /// <summary>
        /// Asynchronously run <paramref name="asyncAction"/> on the UI thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        public void InvokeAndForget(Control control, Func<Task> asyncAction, CancellationToken cancellationToken = default)
        {
            _ = JoinableTaskFactory.RunAsync(() =>
                HandleExceptionsAsync(async () =>
                    {
                        if (!JoinableTaskContext.IsOnMainThread)
                        {
                            await control.SwitchToMainThreadAsync(cancellationToken.CombineWith(_switchToMainThreadCancellationToken).Token);
                        }

                        await asyncAction();
                    },
                    ReportExceptionOnMainThreadAsync));
        }

        /// <summary>
        /// Asynchronously run <paramref name="action"/> on the UI thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
        /// </summary>
        public void InvokeAndForget(Control control, Action action, CancellationToken cancellationToken = default)
        {
            InvokeAndForget(control, AsyncAction(action), cancellationToken);
        }

        public async Task JoinPendingOperationsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _joinableTaskCollection.JoinTillEmptyAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void JoinPendingOperations()
        {
            const int maxWaitMilliseconds = 60_000;
            using CancellationTokenSource cancellationTokenSource = new(maxWaitMilliseconds);

            // Note that JoinableTaskContext.Factory must be used to bypass the default behavior of JoinableTaskFactory
            // since the latter adds new tasks to the collection and would therefore never complete.
            JoinableTaskContext.Factory.Run(() => JoinPendingOperationsAsync(cancellationTokenSource.Token));
        }

        /// <summary>
        /// Forward the exception <paramref name="ex"/> to <see cref="Application.OnThreadException"/> on the main thread.
        /// </summary>
        /// The readability of the callstack is improved by calling <c>ExceptionExtensions.Demystify</c>.
        internal async Task ReportExceptionOnMainThreadAsync(Exception ex)
        {
            try
            {
                if (!JoinableTaskContext.IsOnMainThread)
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(_switchToMainThreadCancellationToken);
                }

                Application.OnThreadException(ex.Demystify());
            }
            catch (Exception exceptionWhileReporting)
            {
                try
                {
                    Trace.TraceError(exceptionWhileReporting.ToString());
                    Trace.TraceError(ex.ToString());
                    Trace.TraceError(ex.StackTrace);
                }
                catch
                {
                    // Give up
                }
            }
        }
    }
}
