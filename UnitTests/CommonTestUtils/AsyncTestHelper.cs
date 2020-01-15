using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitUI;

namespace CommonTestUtils
{
    public static class AsyncTestHelper
    {
        public static TimeSpan UnexpectedTimeout
        {
            get
            {
                return Debugger.IsAttached ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(1);
            }
        }

        public static void RunAndWaitForPendingOperations(Func<Task> asyncMethod, CancellationToken cancellationToken)
        {
            if (ThreadHelper.JoinableTaskFactory == null)
            {
                throw new InvalidOperationException($"Run & wait is pointless without {nameof(ThreadHelper.JoinableTaskFactory)}.");
            }

            ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations(cancellationToken);
        }

        public static T RunAndWaitForPendingOperations<T>(Func<Task<T>> asyncMethod, CancellationToken cancellationToken)
        {
            if (ThreadHelper.JoinableTaskFactory == null)
            {
                throw new InvalidOperationException($"Run & wait is pointless without {nameof(ThreadHelper.JoinableTaskFactory)}.");
            }

            var result = ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations(cancellationToken);

            return result;
        }

        public static void WaitForPendingOperations(TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    WaitForPendingOperations(cts.Token);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    if (int.TryParse(Environment.GetEnvironmentVariable("GE_TEST_SLEEP_SECONDS_ON_HANG"), out var sleepSeconds) && sleepSeconds > 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(sleepSeconds));
                    }

                    throw;
                }
            }
        }

        public static void WaitForPendingOperations(CancellationToken cancellationToken)
        {
            if (ThreadHelper.JoinableTaskContext == null)
            {
                throw new InvalidOperationException($"Wait is pointless without {nameof(ThreadHelper.JoinableTaskContext)}.");
            }

            // Note that ThreadHelper.JoinableTaskContext.Factory must be used to bypass the default behavior of
            // ThreadHelper.JoinableTaskFactory since the latter adds new tasks to the collection and would therefore
            // never complete.
            ThreadHelper.JoinableTaskContext.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(cancellationToken));
        }
    }
}
