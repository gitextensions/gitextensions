using System.ComponentModel;

namespace CommonTestUtils
{
    /// <summary>
    /// This class supports TestCleanUpAttribute. See TestCleanUpAttribute.cs
    /// for technical details.
    /// </summary>
    public static class Epilogue
    {
        private static object Sync = new();
        private static OrderedDictionary<int, Action> AfterSuiteActions = new();
        private static List<Action> AfterTestActions = new();

        /// <summary>
        /// Registers an action to be executed after the completion of
        /// the test suite. Wait actions are executed synchronously in
        /// the sequence indicated by <paramref name="order"/>. Values
        /// of <paramref name="order"/> must be unique.The wait action
        /// registration is permanent. The <see cref="ExecuteAfterTestActions"/> method is
        /// called by <see cref="EpilogueAttribute"/> in order to execute
        /// the current plan of action.
        /// </summary>
        /// <param name="order">The position in sequence to run this action.</param>
        /// <param name="wait">The action to be called.</param>
        public static void RegisterAfterSuiteAction(int order, Action wait)
        {
            lock (Sync)
            {
                AfterSuiteActions.Add(order, wait);
            }
        }

        /// <summary>
        /// Registers an action to be performed after the completion of the
        /// current test, including the completion of any
        /// <see cref="NUnit.Framework.ITestAction.AfterTest(NUnit.Framework.Interfaces.ITest)"/>
        /// callbacks. The <see cref="ExecuteAfterSuiteActions"/> method is
        /// called by <see cref="EpilogueAttribute"/> in order to execute
        /// the current plan of action. Clean-up actions are called only once
        /// and need to be registered each time they are needed.
        /// </summary>
        /// <param name="action">The clean-up action to run after the current test.</param>
        public static void RegisterAfterTestAction(Action action)
        {
            lock (Sync)
            {
                AfterTestActions.Add(action);
            }
        }

        /// <summary>
        /// This method supports the <see cref="Epilogue"/> infrastructure.
        /// It is called by <see cref="EpilogueAttribute"/>, which is an
        /// implementation of NUnit <see cref="NUnit.Framework.ITestAction"/>,
        /// after the end of the current test.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExecuteAfterTestActions()
        {
            lock (Sync)
            {
                AfterTestActions.ForEach(action => action());
                AfterTestActions.Clear();
            }
        }

        /// <summary>
        /// This method supports the <see cref="Epilogue"/> infrastructure.
        /// It is called by <see cref="EpilogueAttribute"/>, which is an
        /// implementation of NUnit <see cref="NUnit.Framework.ITestAction"/>,
        /// after the end of the test suite.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExecuteAfterSuiteActions()
        {
            lock (Sync)
            {
                AfterSuiteActions.Values.ForEach(wait => wait());
            }
        }
    }
}
