namespace CommonTestUtils
{
    internal class SynchronizingCounter
    {
        private readonly object _sync = new();
        private volatile int _count;

        public void Increment()
        {
            lock (_sync)
            {
                Interlocked.Increment(ref _count);
            }
        }

        public void Decrement()
        {
            lock (_sync)
            {
                if (_count == 0)
                {
                    throw new InvalidOperationException("Cannot decrement SynchronizingCounter that is already at 0.");
                }

                Interlocked.Decrement(ref _count);
                Monitor.PulseAll(_sync);
            }
        }

        public void WaitForZero()
        {
            lock (_sync)
            {
                while (_count > 0)
                {
                    Monitor.Wait(_sync);
                }
            }
        }
    }
}
