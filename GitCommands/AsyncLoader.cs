using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitCommands
{
    public class AsyncLoader
    {
        private readonly TaskScheduler _taskScheduler;
        private CancellationTokenSource _cancelledTokenSource;

        public AsyncLoader()
            : this(TaskScheduler.FromCurrentSynchronizationContext())
        {
        }

        public AsyncLoader(TaskScheduler taskScheduler)
        {
            _taskScheduler = taskScheduler;
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
        public static Task<T> DoAsync<T>(Func<T> doMe, Action<T> continueWith, Action<AsyncErrorEventArgs> onError)
        {
            AsyncLoader loader = new AsyncLoader();
            loader.LoadingError += (sender, e) => onError(e);
            return loader.Load(doMe, continueWith);
        }

        public static Task<T> DoAsync<T>(Func<T> doMe, Action<T> continueWith)
        {
            AsyncLoader loader = new AsyncLoader();
            return loader.Load(doMe, continueWith);
        }

        public static Task DoAsync(Action doMe, Action continueWith)
        {
            AsyncLoader loader = new AsyncLoader();
            return loader.Load(doMe, continueWith);
        }

        public Task Load(Action loadContent, Action onLoaded)
        { 
            return Load((token) => loadContent(), onLoaded);
        }

        public Task Load(Action<CancellationToken> loadContent, Action onLoaded)
        {
            Cancel();
            _cancelledTokenSource = new CancellationTokenSource();
            var token = _cancelledTokenSource.Token;
            return Task.Factory.StartNew(() => loadContent(token), token)
                .ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    foreach (var e in task.Exception.InnerExceptions)
                        if (!OnLoadingError(e))
                            throw e;
                    return;
                }
                try
                {
                    onLoaded();
                }
                catch (Exception exception)
                {
                    if (!OnLoadingError(exception))
                        throw;
                }
            }, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, _taskScheduler);
        }

        public Task<T> Load<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            return Load((token) => loadContent(), onLoaded);
        }

        public Task<T> Load<T>(Func<CancellationToken, T> loadContent, Action<T> onLoaded)
        {
            Cancel();
            _cancelledTokenSource = new CancellationTokenSource();
            var token = _cancelledTokenSource.Token;
            return Task.Factory.StartNew(() => loadContent(token), token)
                .ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    foreach (var e in task.Exception.InnerExceptions)
                        if (!OnLoadingError(e))
                            throw e;
                    return default(T);
                }
                try
                {
                    onLoaded(task.Result);
                    return task.Result;
                }
                catch (Exception exception)
                {
                    if (!OnLoadingError(exception))
                        throw;
                    return default(T);
                }
            }, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, _taskScheduler);
        }

        public void Cancel()
        {
            if (_cancelledTokenSource != null)
                _cancelledTokenSource.Cancel();
        }

        private bool OnLoadingError(Exception exception)
        {
            var args = new AsyncErrorEventArgs(exception);
            LoadingError(this, args);
            return args.Handled;
        }
    }

    public class AsyncErrorEventArgs : EventArgs
    {
        public AsyncErrorEventArgs(Exception exception)
        {
            Exception = exception;
            Handled = true;
        }

        public Exception Exception { get; private set; }

        public bool Handled { get; set; }
    }
}
