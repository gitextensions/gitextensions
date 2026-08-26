using Microsoft.VisualStudio.Threading;

namespace GitUI.CommandsDialogs.RepoHosting;

/// <summary>
/// Preserves the WinForms asynchronous-loader boundary on top of the shared task manager.
/// </summary>
internal sealed class AsyncLoader
{
    private readonly TaskManager _taskManager = GitUI.Compat.DesignTimeTaskManager.Create();

    public event EventHandler<AsyncLoaderErrorEventArgs>? LoadingError;

    public JoinableTaskFactory JoinableTaskFactory => _taskManager.JoinableTaskFactory;

    public void FileAndForget(Func<Task> asyncAction)
    {
        _taskManager.FileAndForget(() => RunAsync(asyncAction));
    }

    public Task JoinPendingOperationsAsync(CancellationToken cancellationToken)
        => _taskManager.JoinPendingOperationsAsync(cancellationToken);

    public void JoinPendingOperations()
        => _taskManager.JoinPendingOperations();

    private async Task RunAsync(Func<Task> asyncAction)
    {
        try
        {
            await asyncAction();
        }
        catch (OperationCanceledException)
        {
            // AsyncLoader ignores cancellation just like TaskManager.FileAndForget.
        }
        catch (Exception ex)
        {
            EventHandler<AsyncLoaderErrorEventArgs>? loadingError = LoadingError;
            if (loadingError is null)
            {
                // Let TaskManager route an unowned exception through the application host.
                throw;
            }

            await JoinableTaskFactory.SwitchToMainThreadAsync();
            loadingError(this, new AsyncLoaderErrorEventArgs(ex));
        }
    }
}

internal sealed class AsyncLoaderErrorEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; } = exception;
}
