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

        public void FileAndForget(Task task, Func<Exception, bool>? fileOnlyIf = null)
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
                    catch (Exception ex) when (fileOnlyIf?.Invoke(ex) ?? true)
                    {
                        await JoinableTaskFactory.SwitchToMainThreadAsync();
                        Application.OnThreadException(ex.Demystify());
                    }
                });
        }

        public void RunAsyncAndForget(Func<Task> asyncAction, Func<Exception, bool>? fileOnlyIf = null)
        {
            FileAndForget(JoinableTaskFactory.RunAsync(asyncAction).Task, fileOnlyIf);
        }

        public void RunAsyncAndForget(Action action, Func<Exception, bool>? fileOnlyIf = null)
        {
            RunAsyncAndForget(() =>
                {
                    action();
                    return Task.CompletedTask;
                },
                fileOnlyIf);
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
