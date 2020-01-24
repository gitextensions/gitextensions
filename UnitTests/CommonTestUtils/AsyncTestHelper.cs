using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations(cancellationToken);
        }

        public static T RunAndWaitForPendingOperations<T>(Func<Task<T>> asyncMethod, CancellationToken cancellationToken)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations(cancellationToken);

            return result;
        }

        public static void WaitForPendingOperations(TimeSpan timeout)
        {
            // Workaround for tests hanging in conjunction with canceled operations:
            // Process the message loop before and after the wait.
            Application.DoEvents();
            using (var cts = new CancellationTokenSource(timeout))
            {
                WaitForPendingOperations(cts.Token);
            }
            Application.DoEvents();
        }

        public static void WaitForPendingOperations(CancellationToken cancellationToken)
        {
            ThreadHelper.JoinableTaskContext?.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(cancellationToken));
        }
    }
}
