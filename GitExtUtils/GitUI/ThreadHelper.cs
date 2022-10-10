using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public static class ThreadHelper
    {
        private const int RPC_E_WRONG_THREAD = unchecked((int)0x8001010E);

        private static TaskManager _taskManager;

        public static JoinableTaskContext JoinableTaskContext
        {
            get => _taskManager?.JoinableTaskContext;
            internal set => _taskManager = value is null ? null : new(value);
        }

        public static JoinableTaskFactory JoinableTaskFactory => _taskManager.JoinableTaskFactory;

        public static TaskManager CreateTaskManager()
            => new(_taskManager.JoinableTaskContext);

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

            Debug.Assert(JoinableTaskContext.IsOnMainThread, "Must be on the UI thread.");
        }

        public static void ThrowIfOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (JoinableTaskContext.IsOnMainThread)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "{0} must be called on a background thread.", callerMemberName);
                throw new COMException(message, RPC_E_WRONG_THREAD);
            }
        }

        public static void FileAndForget(this JoinableTask joinableTask, Func<Exception, bool>? fileOnlyIf = null)
        {
            joinableTask.Task.FileAndForget(fileOnlyIf);
        }

        public static void FileAndForget(this Task task, Func<Exception, bool>? fileOnlyIf = null)
            => _taskManager.FileAndForget(task, fileOnlyIf);

        public static async Task JoinPendingOperationsAsync(CancellationToken cancellationToken)
            => await _taskManager.JoinPendingOperationsAsync(cancellationToken);

        public static void JoinPendingOperations()
            => _taskManager.JoinPendingOperations();

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
}
