using System.Diagnostics;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public class TaskManager
    {
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

        public void FileAndForget(Task task)
        {
            JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                        await task.ConfigureAwait(false);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
                    }
                    catch (OperationCanceledException)
                    {
                        // Do not rethrow these
                    }
                    catch (Exception ex)
                    {
                        await JoinableTaskFactory.SwitchToMainThreadAsync();
                        Application.OnThreadException(ex.Demystify());
                    }
                });
        }

        public void RunAsyncAndForget(Func<Task> asyncAction)
        {
            FileAndForget(JoinableTaskFactory.RunAsync(asyncAction).Task);
        }

        public void RunAsyncAndForget(Action action)
        {
            RunAsyncAndForget(() =>
                {
                    action();
                    return Task.CompletedTask;
                });
        }

        public async Task JoinPendingOperationsAsync(CancellationToken cancellationToken)
        {
            await _joinableTaskCollection.JoinTillEmptyAsync(cancellationToken);
        }

        public void JoinPendingOperations()
        {
            // Note that JoinableTaskContext.Factory must be used to bypass the default behavior of JoinableTaskFactory
            // since the latter adds new tasks to the collection and would therefore never complete.
            JoinableTaskContext.Factory.Run(_joinableTaskCollection.JoinTillEmptyAsync);
        }
    }
}
