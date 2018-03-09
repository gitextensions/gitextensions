using System;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public class AsyncLoader : IDisposable
    {
        private CancellationTokenSource _cancelledTokenSource;
        public TimeSpan Delay { get; set; }

        public AsyncLoader()
        {
            Delay = TimeSpan.Zero;
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
            return loader.LoadAsync(doMe, continueWith);
        }

        public static Task<T> DoAsync<T>(Func<T> doMe, Action<T> continueWith)
        {
            AsyncLoader loader = new AsyncLoader();
            return loader.LoadAsync(doMe, continueWith);
        }

        public static Task DoAsync(Action doMe, Action continueWith)
        {
            AsyncLoader loader = new AsyncLoader();
            return loader.LoadAsync(doMe, continueWith);
        }

        public Task LoadAsync(Action loadContent, Action onLoaded)
        {
            return LoadAsync((token) => loadContent(), onLoaded);
        }

        public async Task LoadAsync(Action<CancellationToken> loadContent, Action onLoaded)
        {
            Cancel();
            _cancelledTokenSource?.Dispose();
            _cancelledTokenSource = new CancellationTokenSource();
            var token = _cancelledTokenSource.Token;

            try
            {
                if (Delay > TimeSpan.Zero)
                {
                    await Task.Delay(Delay, token).ConfigureAwait(false);
                }
                else
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                }

                if (!token.IsCancellationRequested)
                {
                    loadContent(token);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException && token.IsCancellationRequested)
                {
                    return;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (!OnLoadingError(e))
                {
                    throw;
                }

                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            if (!token.IsCancellationRequested)
            {
                try
                {
                    onLoaded();
                }
                catch (Exception e)
                {
                    if (!OnLoadingError(e))
                    {
                        throw;
                    }
                }
            }
        }

        public Task<T> LoadAsync<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            return LoadAsync((token) => loadContent(), onLoaded);
        }

        public async Task<T> LoadAsync<T>(Func<CancellationToken, T> loadContent, Action<T> onLoaded)
        {
            Cancel();
            _cancelledTokenSource?.Dispose();
            _cancelledTokenSource = new CancellationTokenSource();
            var token = _cancelledTokenSource.Token;

            T result;

            try
            {
                if (Delay > TimeSpan.Zero)
                {
                    await Task.Delay(Delay, token).ConfigureAwait(false);
                }
                else
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                }

                if (token.IsCancellationRequested)
                {
                    result = default(T);
                }
                else
                {
                    result = loadContent(token);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException && token.IsCancellationRequested)
                {
                    return default(T);
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (!OnLoadingError(e))
                {
                    throw;
                }

                return default(T);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!token.IsCancellationRequested)
            {
                try
                {
                    onLoaded(result);
                }
                catch (Exception e)
                {
                    if (!OnLoadingError(e))
                    {
                        throw;
                    }

                    return default(T);
                }
            }

            return result;
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
            {
                _cancelledTokenSource?.Dispose();
            }
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
