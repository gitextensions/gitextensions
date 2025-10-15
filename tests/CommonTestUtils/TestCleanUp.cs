using System.ComponentModel;

namespace CommonTestUtils
{
    /// <summary>
    /// This class supports TestCleanUpAttribute. See TestCleanUpAttribute.cs
    /// for technical details.
    /// </summary>
    public static class TestCleanUp
    {
        private static object Sync = new();
        private static OrderedDictionary<int, Action> Waits = new();
        private static List<Action> Actions = new();

        /// <summary>
        /// Registers a clean-up wait action. Called once per site that
        /// needs to perform a synchronous action after the completion
        /// of the test suite. The wait action registration is permanent.
        /// Values of <paramref name="order"/> must be unique. Wait
        /// actions are executed in sequence each time
        /// <see cref="WaitForCompletion"/> is called. The purpose is to
        /// allow consumers of the <see cref="TestCleanUp"/> infrastructure
        /// to ensure that any asynchronous operations they may have
        /// started run to completion before the test process terminates.
        /// </summary>
        /// <param name="order">The position in sequence to run this wait action</param>
        /// <param name="wait">The action to be called.</param>
        public static void RegisterCleanUpWait(int order, Action wait)
        {
            lock (Sync)
            {
                Waits.Add(order, wait);
            }
        }

        /// <summary>
        /// Registers a clean-up action to be performed after the completion
        /// of the current test, including the completion of any
        /// <see cref="NUnit.Framework.ITestAction.AfterTest(NUnit.Framework.Interfaces.ITest)"/>
        /// callbacks. The <see cref="ExecuteQueuedCleanUpActions"/> method is
        /// called by <see cref="TestCleanUpAttribute"/> in order to execute
        /// the current plan of action. Clean-up actions are called only once
        /// and need to be registered each time they are needed.
        /// </summary>
        /// <param name="action">The clean-up action to run after the current test.</param>
        public static void RegisterCleanUpAction(Action action)
        {
            lock (Sync)
            {
                Actions.Add(action);
            }
        }

        /// <summary>
        /// This method supports the <see cref="TestCleanUp"/> infrastructure.
        /// It is called by <see cref="TestCleanUpAttribute"/>, which is an
        /// implementation of NUnit <see cref="NUnit.Framework.ITestAction"/>,
        /// after the end of the current test.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExecuteQueuedCleanUpActions()
        {
            lock (Sync)
            {
                Actions.ForEach(action => action());
                Actions.Clear();
            }
        }

        /// <summary>
        /// This method supports the <see cref="TestCleanUp"/> infrastructure.
        /// It is called by <see cref="TestCleanUpAttribute"/>, which is an
        /// implementation of NUnit <see cref="NUnit.Framework.ITestAction"/>,
        /// after the end of the test suite.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void WaitForCompletion()
        {
            lock (Sync)
            {
                Waits.Values.ForEach(wait => wait());
            }
        }
    }
}
