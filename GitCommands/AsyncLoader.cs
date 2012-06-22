using System;
using System.Threading;

namespace GitCommands
{
    public class AsyncLoader
    {
        private readonly SynchronizationContext _syncContext;
        private readonly object _taskLock;
        private ILoadingTask _currentTask;

        public AsyncLoader()
            : this(SynchronizationContext.Current)
        {
        }

        public AsyncLoader(SynchronizationContext syncContext)
        {
            _syncContext = syncContext;
            _taskLock = new object();
            _currentTask = new LoadingTask<object>();
        }

        public event EventHandler<AsyncErrorEventArgs> LoadingError = delegate { };

        /// <summary>
        /// Does something on threadpool, executes continuation on current sync context thread, executes onError if the async request fails.
        /// There does probably exist something like this in the .NET library, but I could not find it. //cocytus
        /// </summary>
        /// <typeparam name="T">Result to be passed from doMe to continueWith</typeparam>
        /// <param name="doMe">The stuff we want to do. Should return whatever continueWith expects.</param>
        /// <param name="continueWith">Do this on original sync context.</param>
        /// <param name="onError">Do this on original sync context if doMe barfs.</param>
        public static void DoAsync<T>(Func<T> doMe, Action<T> continueWith, Action<Exception> onError)
        {
            AsyncLoader loader = new AsyncLoader();
            loader.LoadingError += (object sender, AsyncErrorEventArgs e) => { onError(e.Exception);  };
            loader.Load(doMe, continueWith);
        }    
        
        public void Load(Action loadContent, Action onLoaded)
        { 
            Load(() => { loadContent(); return true; }, (b) => onLoaded() );
        }

        public void Load<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            var newTask = new LoadingTask<T>(_syncContext, loadContent, onLoaded, OnLoadingError);

            lock (_taskLock)
            {
                _currentTask.Cancel();
                _currentTask = newTask;
            }

            newTask.RunAsync();
        }

        public void Cancel()
        {
            lock (_taskLock)
            {
                _currentTask.Cancel();
            }        
        }

        private void OnLoadingError(Exception exception)
        {
            LoadingError(this, new AsyncErrorEventArgs(exception));
        }

        #region Nested type: ILoadingTask

        private interface ILoadingTask
        {
            void RunAsync();

            void Cancel();
        }

        #endregion

        #region Nested type: LoadingTask

        private sealed class LoadingTask<T> : ILoadingTask
        {
            private readonly Func<T> _loadContent;
            private readonly Action<Exception> _onError;
            private readonly Action<T> _onLoaded;
            private readonly SynchronizationContext _syncContext;
            private bool _cancelled;

            public LoadingTask()
            {
                _cancelled = true;
            }

            public LoadingTask(SynchronizationContext syncContext, Func<T> loadContent,
                               Action<T> onLoaded, Action<Exception> onError)
            {
                _syncContext = syncContext;
                _loadContent = loadContent;
                _onLoaded = onLoaded;
                _onError = onError;
            }

            #region ILoadingTask Members

            public void Cancel()
            {
                _cancelled = true;
            }

            public void RunAsync()
            {
                if (_cancelled) return;
                RunOnWorker(
                    () =>
                    {
                        try
                        {
                            if (_cancelled) return;
                            var content = _loadContent();

                            if (_cancelled) return;
                            RunOnUiThread(
                                () =>
                                {
                                    try
                                    {
                                        if (_cancelled) return;
                                        _onLoaded(content);
                                    }
                                    catch (Exception exception)
                                    {
                                        if (_cancelled) return;
                                        _onError(exception);
                                    }
                                });
                        }
                        catch (Exception exception)
                        {
                            if (_cancelled) return;
                            RunOnUiThread(
                                () =>
                                {
                                    if (_cancelled) return;
                                    _onError(exception);
                                });
                        }
                    });
            }

            #endregion

            private static void RunOnWorker(Action action)
            {
                ThreadPool.QueueUserWorkItem(_ => action());
            }

            private void RunOnUiThread(Action action)
            {
                _syncContext.Post(_ => action(), null);
            }
        }

        #endregion
    }

    public class AsyncErrorEventArgs : EventArgs
    {
        public AsyncErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

}
