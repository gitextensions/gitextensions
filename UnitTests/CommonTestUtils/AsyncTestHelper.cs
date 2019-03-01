using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitUI;

namespace CommonTestUtils
{
    public static class AsyncTestHelper
    {
        public static void RunAndWaitForPendingOperations(Func<Task> asyncMethod)
        {
            ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations();
        }

        public static T RunAndWaitForPendingOperations<T>(Func<Task<T>> asyncMethod)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

            WaitForPendingOperations();

            return result;
        }

        public static void WaitForPendingOperations()
        {
            try
            {
                ThreadHelper.JoinableTaskContext?.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync());
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
