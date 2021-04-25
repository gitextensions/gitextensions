using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public static class ThreadHelper
    {
        private const int RPC_E_WRONG_THREAD = unchecked((int)0x8001010E);
        private static JoinableTaskContext _joinableTaskContext = null!;
        private static JoinableTaskCollection _joinableTaskCollection = null!;
        private static JoinableTaskFactory _joinableTaskFactory = null!;

        public static JoinableTaskContext JoinableTaskContext
        {
            get
            {
                return _joinableTaskContext;
            }

            internal set
            {
                if (value == _joinableTaskContext)
                {
                    return;
                }

                if (value is null)
                {
                    _joinableTaskContext = null!;
                    _joinableTaskCollection = null!;
                    _joinableTaskFactory = null!;
                }
                else
                {
                    _joinableTaskContext = value;
                    _joinableTaskCollection = value.CreateCollection();
                    _joinableTaskFactory = value.CreateFactory(_joinableTaskCollection);
                }
            }
        }

        public static JoinableTaskFactory JoinableTaskFactory => _joinableTaskFactory;

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

        public static async Task JoinPendingOperationsAsync(CancellationToken cancellationToken)
        {
            await _joinableTaskCollection.JoinTillEmptyAsync(cancellationToken);
        }

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
