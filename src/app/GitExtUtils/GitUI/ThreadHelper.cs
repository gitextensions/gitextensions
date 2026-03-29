#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GitExtensions.Extensibility;
using Microsoft.VisualStudio.Threading;

namespace GitUI;

public static class ThreadHelper
{
    private const int RPC_E_WRONG_THREAD = unchecked((int)0x8001010E);

    private static TaskManager? _taskManager;

    private static TaskManager TaskManager =>
        _taskManager ?? throw new InvalidOperationException($"{nameof(ThreadHelper)}.{nameof(JoinableTaskContext)} has not been initialized.");

    public static bool HasJoinableTaskContext => _taskManager is not null;

    public static JoinableTaskContext JoinableTaskContext
    {
        get => TaskManager.JoinableTaskContext;
        internal set => _taskManager = value is null ? null : new(value);
    }

    public static JoinableTaskFactory JoinableTaskFactory => TaskManager.JoinableTaskFactory;

    internal static void CancelSwitchToMainThread()
        => TaskManager.CancelSwitchToMainThread();

    public static ExclusiveTaskRunner CreateExclusiveTaskRunner()
        => new(TaskManager);

    public static TaskManager CreateTaskManager()
        => new(TaskManager.JoinableTaskContext);

    public static void ThrowIfNotOnUIThread([CallerMemberName] string callerMemberName = "")
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        if (!JoinableTaskContext.IsOnMainThread)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "{0} must be called on the UI thread.", callerMemberName);
            throw new COMException(message, RPC_E_WRONG_THREAD);
        }
    }

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public static void AssertOnUIThread()
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        DebugHelpers.Assert(JoinableTaskContext.IsOnMainThread, "Must be on the UI thread.");
    }

    public static void ThrowIfOnUIThread([CallerMemberName] string callerMemberName = "")
    {
        if (JoinableTaskContext.IsOnMainThread)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "{0} must be called on a background thread.", callerMemberName);
            throw new COMException(message, RPC_E_WRONG_THREAD);
        }
    }

    /// <summary>
    /// Asynchronously run <paramref name="asyncAction"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
    /// </summary>
    public static void FileAndForget(Func<Task> asyncAction)
        => TaskManager.FileAndForget(asyncAction);

    /// <summary>
    /// Asynchronously run <paramref name="action"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
    /// </summary>
    public static void FileAndForget(Action action)
        => TaskManager.FileAndForget(action);

    /// <summary>
    /// Asynchronously run <paramref name="joinableTask"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
    /// </summary>
    public static void FileAndForget(this JoinableTask joinableTask)
        => TaskManager.FileAndForget(joinableTask.Task);

    /// <summary>
    /// Asynchronously run <paramref name="task"/> on a background thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
    /// </summary>
    public static void FileAndForget(this Task task)
        => TaskManager.FileAndForget(task);

    /// <summary>
    /// Asynchronously run <paramref name="asyncAction"/> on the UI thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
    /// </summary>
    public static void InvokeAndForget(this Control control, Func<Task> asyncAction, TaskManager? taskManager = null, CancellationToken cancellationToken = default)
        => (taskManager ?? TaskManager).InvokeAndForget(control, asyncAction, cancellationToken);

    /// <summary>
    /// Asynchronously run <paramref name="action"/> on the UI thread and forward all exceptions to <see cref="Application.OnThreadException"/> except for <see cref="OperationCanceledException"/>, which is ignored.
    /// </summary>
    public static void InvokeAndForget(this Control control, Action action, TaskManager? taskManager = null, CancellationToken cancellationToken = default)
        => InvokeAndForget(control, TaskManager.AsyncAction(action), taskManager, cancellationToken);

    public static async Task JoinPendingOperationsAsync(CancellationToken cancellationToken)
        => await TaskManager.JoinPendingOperationsAsync(cancellationToken);

    public static T CompletedResult<T>(this Task<T> task)
    {
        if (!task.IsCompleted)
        {
            throw new InvalidOperationException();
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        return task.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    public static T? CompletedOrDefault<T>(this Task<T> task)
    {
        if (!task.IsCompleted)
        {
            return default;
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        return task.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }
}
