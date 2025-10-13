namespace CommonTestUtils
{
    // This class supports TestCleanUpAttribute. See TestCleanUpAttribute.cs
    // for technical details.
    public static class TestCleanUp
    {
        private static object Sync = new();
        private static List<Action> Actions = new();

        public static void RegisterCleanUpAction(Action action)
        {
            lock (Sync)
            {
                Actions.Add(action);
            }
        }

        public static void StartQueuedCleanupActions()
        {
            lock (Sync)
            {
                Actions.ForEach(action => action());
                Actions.Clear();
            }
        }
    }
}
