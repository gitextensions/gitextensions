using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GitCommandsTests.Helpers
{
    /// <summary>
    /// Simple <see cref="TaskScheduler"/> implementation that owns a single thread
    /// and schedules all work on it. No tasks may execute inline under this scheduler.
    /// </summary>
    /// <remarks>Only really useful for testing purposes.</remarks>
    internal sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task> _queue = new BlockingCollection<Task>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Thread _thread;

        public SingleThreadTaskScheduler()
        {
            _thread = new Thread(() =>
            {
                try
                {
                    foreach (var task in _queue.GetConsumingEnumerable(_cts.Token))
                    {
                        TryExecuteTask(task);
                    }
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            });

            _thread.Start();
        }

        protected override void QueueTask(Task task) => _queue.Add(task);

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

        protected override IEnumerable<Task> GetScheduledTasks() => _queue;

        public void Dispose()
        {
            _cts.Cancel();
            _thread.Join();
            _cts.Dispose();
            _queue.Dispose();
        }
    }
}