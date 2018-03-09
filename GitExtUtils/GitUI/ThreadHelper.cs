using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public static class ThreadHelper
    {
        public static JoinableTaskContext JoinableTaskContext
        {
            get;
            internal set;
        }

        public static JoinableTaskFactory JoinableTaskFactory => JoinableTaskContext.Factory;

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
