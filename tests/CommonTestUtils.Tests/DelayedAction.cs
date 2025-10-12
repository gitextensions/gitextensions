namespace CommonTestUtils.Tests
{
    internal class DelayedAction
    {
        private object _sync = new();
        private List<Action> _actions = new();
        private volatile bool _hasActed = false;

        public DelayedAction(TimeSpan delay)
        {
            Task.Run(() => WaitThenAct(delay));
        }

        private async Task WaitThenAct(TimeSpan delay)
        {
            await Task.Delay(delay);

            lock (_sync)
            {
                _actions.ForEach(action => action());
                _hasActed = true;
            }
        }

        public void Do(Action action)
        {
            lock (_sync)
            {
                if (!_hasActed)
                {
                    _actions.Add(action);
                }
                else
                {
                    action();
                }
            }
        }
    }
}
