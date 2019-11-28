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
                throw new InvalidOperationException("Run & wait is pointless without JoinableTaskContext.");
            }

            ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations(cancellationToken);
        }

        public static T RunAndWaitForPendingOperations<T>(Func<Task<T>> asyncMethod, CancellationToken cancellationToken)
        {
            if (ThreadHelper.JoinableTaskFactory == null)
            {
                throw new InvalidOperationException("Run & wait is pointless without JoinableTaskContext.");
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
                catch (OperationCanceledException ex) when (cts.IsCancellationRequested)
                {
                    Console.WriteLine($"{nameof(WaitForPendingOperations)} cts canceled, ex {ex.Demystify()}");
                }
            }
        }

        public static void WaitForPendingOperations(CancellationToken cancellationToken)
        {
            if (ThreadHelper.JoinableTaskContext == null)
            {
                throw new InvalidOperationException("Wait is pointless without JoinableTaskContext.");
            }

            ThreadHelper.JoinableTaskContext.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(cancellationToken));
        }
    }
}
