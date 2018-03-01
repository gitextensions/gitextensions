using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitCommands
{
    public class AsyncLoader : IDisposable
    {
        private readonly TaskScheduler _continuationTaskScheduler;
        private CancellationTokenSource _cancelledTokenSource;
        public int Delay { get; set; }

        public AsyncLoader()
            : this(DefaultContinuationTaskScheduler)
        {
        }

        public AsyncLoader(TaskScheduler continuationTaskScheduler)
        {
            _continuationTaskScheduler = continuationTaskScheduler;
            Delay = 0;
        }

        private static int _defaultThreadId = -1;
        private static TaskScheduler _DefaultContinuationTaskScheduler;
        public static TaskScheduler DefaultContinuationTaskScheduler
        {
            get
            {
                if (_defaultThreadId == Thread.CurrentThread.ManagedThreadId && _DefaultContinuationTaskScheduler != null)
                {
                    return _DefaultContinuationTaskScheduler;
                }

                return TaskScheduler.FromCurrentSynchronizationContext();
            }

            set
            {
                _defaultThreadId = Thread.CurrentThread.ManagedThreadId;
                _DefaultContinuationTaskScheduler = value;
            }
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
            _cancelledTokenSource?.Dispose();
            _cancelledTokenSource = new CancellationTokenSource();
            var token = _cancelledTokenSource.Token;
            return Task.Factory.StartNew(() =>
                {
                    if (Delay > 0)
                    {
                        token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(Delay));
                    }
                    if (!token.IsCancellationRequested)
                    {
                        loadContent(token);
                    }
                }, token)
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
                            if (!token.IsCancellationRequested)
                            {
                                onLoaded();
                            }
                        }
                        catch (Exception exception)
                        {
                            if (!OnLoadingError(exception))
                                throw;
                        }
                    }, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, _continuationTaskScheduler);
        }

        public Task<T> Load<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            return Load((token) => loadContent(), onLoaded);
        }

        public Task<T> Load<T>(Func<CancellationToken, T> loadContent, Action<T> onLoaded)
        {
            Cancel();
            _cancelledTokenSource?.Dispose();
            _cancelledTokenSource = new CancellationTokenSource();
            var token = _cancelledTokenSource.Token;
            return Task.Factory.StartNew(() =>
                {
                    if (Delay > 0)
                    {
                        token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(Delay));
                    }
                    if (token.IsCancellationRequested)
                    {
                        return default;
                    }
                    return loadContent(token);

                }, token)
                .ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    foreach (var e in task.Exception.InnerExceptions)
                        if (!OnLoadingError(e))
                            throw e;
                    return default;
                }
                try
                {
                    if (!token.IsCancellationRequested)
                    {
                        onLoaded(task.Result);
                    }
                    return task.Result;
                }
                catch (Exception exception)
                {
                    if (!OnLoadingError(exception))
                        throw;
                    return default;
                }
            }, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, _continuationTaskScheduler);
        }

        public void Cancel()
        {
            _cancelledTokenSource?.Cancel();
        }

        private bool OnLoadingError(Exception exception)
        {
            var args = new AsyncErrorEventArgs(exception);
            LoadingError(this, args);
            return args.Handled;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _cancelledTokenSource?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
