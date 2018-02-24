using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class AsyncLoader : IDisposable
    {
        private static readonly ThreadLocal<TaskScheduler> _defaultScheduler = new ThreadLocal<TaskScheduler>(trackAllValues: false);

        /// <summary>
        /// Gets and sets the default <see cref="TaskScheduler"/> to use for continuations following calls to <c>Load</c> from the current thread.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TaskScheduler.FromCurrentSynchronizationContext"/>.
        /// </remarks>
        [NotNull]
        public static TaskScheduler DefaultContinuationTaskScheduler
        {
            get => _defaultScheduler.IsValueCreated ? _defaultScheduler.Value : TaskScheduler.FromCurrentSynchronizationContext();
            set => _defaultScheduler.Value = value ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Invokes <paramref name="loadContent"/> on the thread pool, then executes <paramref name="continueWith"/> on the current synchronisation context.
        /// </summary>
        /// <remarks>
        /// Note this method does not perform any cancellation of prior loads, nor does it support cancellation upon disposal.
        /// If you require those features, use an instance of <see cref="AsyncLoader"/> instead.
        /// </remarks>
        /// <typeparam name="T">Type of data returned by <paramref name="loadContent"/> and accepted by <paramref name="continueWith"/>.</typeparam>
        /// <param name="loadContent">A function to invoke on the thread pool that returns a value to be passed to <paramref name="continueWith"/>.</param>
        /// <param name="continueWith">An action to invoke on the original synchronisation context with the return value from <paramref name="loadContent"/>.</param>
        /// <param name="onError">
        /// An optional callback for notification of exceptions from <paramref name="loadContent"/>.
        /// Invoked on the original synchronisation context.
        /// Invoked once per exception, so may be called multiple times.
        /// Handlers must set <see cref="AsyncErrorEventArgs.Handled"/> to <c>true</c> to prevent any exceptions being re-thrown and faulting the async operation.
        /// </param>
        public static async void DoAsync<T>(Func<T> loadContent, Action<T> continueWith, Action<AsyncErrorEventArgs> onError = null)
        {
            using (var loader = new AsyncLoader())
            {
                if (onError != null)
                {
                    loader.LoadingError += (_, e) => onError(e);
                }

                await loader.Load(loadContent, continueWith);
            }
        }

        public event EventHandler<AsyncErrorEventArgs> LoadingError;

        [NotNull] private readonly TaskScheduler _continuationTaskScheduler;
        [CanBeNull] private CancellationTokenSource _cancellationTokenSource;
        private int _disposed;

        /// <summary>
        /// Gets and sets an amount of time to delay calling <c>loadContent</c> actions after a call to one of the <c>Load</c> overloads.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TimeSpan.Zero"/>.
        /// </remarks>
        public TimeSpan Delay { get; set; }

        public AsyncLoader(TaskScheduler continuationTaskScheduler = null)
        {
            _continuationTaskScheduler = continuationTaskScheduler ?? DefaultContinuationTaskScheduler;
        }

        public Task Load(Action loadContent, Action onLoaded)
        {
            return Load(token => loadContent(), onLoaded);
        }

        public Task Load(Action<CancellationToken> loadContent, Action onLoaded)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(AsyncLoader));
            }

            // Stop any prior operation
            Cancel();

            // Create a new cancellation object
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            // Copy reference to token (important)
            var token = _cancellationTokenSource.Token;

            return Task.Factory
                .StartNew(Load, token)
                .ContinueWith(
                    Continuation,
                    CancellationToken.None,
                    TaskContinuationOptions.NotOnCanceled,
                    _continuationTaskScheduler);

            void Load()
            {
                // Defer the load operation if requested
                if (Delay > TimeSpan.Zero)
                {
                    token.WaitHandle.WaitOne(Delay);
                }

                // Load content, so long as we haven't already been cancelled
                if (!token.IsCancellationRequested)
                {
                    loadContent(token);
                }
            }

            void Continuation(Task task)
            {
                if (task.IsFaulted)
                {
                    Debug.Assert(task.Exception != null, "Faulted task should have a non-null exception");

                    foreach (var e in task.Exception.InnerExceptions)
                    {
                        if (!OnLoadingError(e))
                        {
                            throw e;
                        }
                    }

                    return;
                }

                try
                {
                    // Invoke continuation unless cancelled
                    if (!token.IsCancellationRequested)
                    {
                        onLoaded();
                    }
                }
                catch (Exception exception)
                {
                    if (!OnLoadingError(exception))
                    {
                        throw;
                    }
                }
            }
        }

        public Task<T> Load<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            return Load(token => loadContent(), onLoaded);
        }

        public Task<T> Load<T>(Func<CancellationToken, T> loadContent, Action<T> onLoaded)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(AsyncLoader));
            }

            // Stop any prior operation
            Cancel();

            // Create a new cancellation object
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            // Copy reference to token (important)
            var token = _cancellationTokenSource.Token;

            return Task.Factory
                .StartNew(Load, token)
                .ContinueWith(
                    Continuation,
                    CancellationToken.None,
                    TaskContinuationOptions.NotOnCanceled,
                    _continuationTaskScheduler);

            T Load()
            {
                // Defer the load operation if requested
                if (Delay > TimeSpan.Zero)
                {
                    token.WaitHandle.WaitOne(Delay);
                }

                // Bail early if cancelled, returning default value for type
                if (token.IsCancellationRequested)
                {
                    return default;
                }

                // Load content
                return loadContent(token);
            }

            T Continuation(Task<T> task)
            {
                if (task.IsFaulted)
                {
                    foreach (var e in task.Exception.InnerExceptions)
                    {
                        if (!OnLoadingError(e))
                        {
                            throw e;
                        }
                    }

                    return default;
                }

                try
                {
                    // Invoke continuation unless cancelled
                    if (!token.IsCancellationRequested)
                    {
                        onLoaded(task.Result);
                    }

                    return task.Result;
                }
                catch (Exception exception)
                {
                    if (!OnLoadingError(exception))
                    {
                        throw;
                    }

                    return default;
                }
            }
        }

        private bool OnLoadingError(Exception exception)
        {
            var args = new AsyncErrorEventArgs(exception);
            LoadingError?.Invoke(this, args);
            return args.Handled;
        }

        public void Cancel()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(AsyncLoader));
            }

            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }

    public sealed class AsyncErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public bool Handled { get; set; } = true;

        public AsyncErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
