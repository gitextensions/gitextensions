using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed partial class RevisionDataGridView
    {
        /// <summary>
        /// Coordinates background update executions. Requested reruns only "queue up" upto one.
        /// </summary>
        private class BackgroundUpdater
        {
            private readonly Func<Task> _operation;
            private readonly int _cooldownMilliseconds;
            private readonly object _sync = new();

            private volatile bool _executing;
            private volatile bool _rerunRequested;

            public BackgroundUpdater(Func<Task> operation, int cooldownMilliseconds)
            {
                _operation = operation ?? throw new ArgumentNullException(nameof(operation));
                _cooldownMilliseconds = cooldownMilliseconds;
            }

            public bool IsExecuting => _executing;

            public void ScheduleExcecution()
            {
                lock (_sync)
                {
                    if (!_executing)
                    {
                        // if not running, start it
                        _executing = true;
                        Task.Run(WrappedOperationAsync);
                    }
                    else
                    {
                        // if currently running make sure it runs again
                        _rerunRequested = true;
                    }
                }
            }

            private async Task WrappedOperationAsync()
            {
                await _operation();

                if (_rerunRequested)
                {
                    await Task.Delay(_cooldownMilliseconds);
                }

                lock (_sync)
                {
                    if (_rerunRequested)
                    {
                        Task.Run(WrappedOperationAsync);
                        _rerunRequested = false;
                    }
                    else
                    {
                        _executing = false;
                    }
                }
            }
        }
    }
}
