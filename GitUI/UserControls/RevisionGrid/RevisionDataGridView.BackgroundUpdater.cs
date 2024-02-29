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
            private readonly TaskManager _taskManager;

            private volatile bool _executing;
            private volatile bool _rerunRequested;

            public BackgroundUpdater(TaskManager taskManager, Func<Task> operation, int cooldownMilliseconds)
            {
                _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
                _operation = operation ?? throw new ArgumentNullException(nameof(operation));
                _cooldownMilliseconds = cooldownMilliseconds;
            }

            public void ScheduleExecution()
            {
                lock (_sync)
                {
                    if (!_executing)
                    {
                        // if not running, start it
                        _executing = true;
                        _taskManager.FileAndForget(WrappedOperationAsync);
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
                try
                {
                    await _operation();
                }
                finally
                {
                    await Task.Delay(_cooldownMilliseconds);

                    lock (_sync)
                    {
                        if (_rerunRequested)
                        {
                            _taskManager.FileAndForget(WrappedOperationAsync);
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
}
