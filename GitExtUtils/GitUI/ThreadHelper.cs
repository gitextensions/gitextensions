using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public static class ThreadHelper
    {
        private static JoinableTaskContext _joinableTaskContext;
        private static JoinableTaskCollection _joinableTaskCollection;
        private static JoinableTaskFactory _joinableTaskFactory;

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

                if (value == null)
                {
                    _joinableTaskContext = null;
                    _joinableTaskCollection = null;
                    _joinableTaskFactory = null;
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

        public static async Task JoinPendingOperationsAsync()
        {
            await _joinableTaskCollection.JoinTillEmptyAsync();
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

        public static T CompletedOrDefault<T>(this Task<T> task)
        {
            if (!task.IsCompleted)
            {
                return default(T);
            }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return task.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
    }
}
