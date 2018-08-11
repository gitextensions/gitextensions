using System;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public sealed class AsyncLoader : IDisposable
    {
        public event EventHandler<AsyncErrorEventArgs> LoadingError;

        private readonly CancellationTokenSequence _cancellationSequence = new CancellationTokenSequence();

        private int _disposed;

        /// <summary>
        /// Gets and sets an amount of time to delay calling <c>loadContent</c> actions after a call to one of the <c>Load</c> overloads.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TimeSpan.Zero"/>.
        /// </remarks>
        public TimeSpan Delay { get; set; }

        public Task LoadAsync(Action loadContent, Action onLoaded)
        {
            return LoadAsync(token => loadContent(), onLoaded);
        }

        public async Task LoadAsync(Action<CancellationToken> loadContent, Action onLoaded)
        {
            await LoadAsync(
                (token) =>
                {
                    loadContent(token);
                    return string.Empty;
                },
                _ => onLoaded());
        }

        public Task<T> LoadAsync<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            return LoadAsync(token => loadContent(), onLoaded);
        }

        [ItemCanBeNull]
        public async Task<T> LoadAsync<T>(Func<CancellationToken, T> loadContent, Action<T> onLoaded)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(AsyncLoader));
            }

            // Stop any prior operation
            Cancel();

            // Create a new cancellation token
            var token = _cancellationSequence.Next();

            T result;

            try
            {
                // Defer the load operation if requested
                if (Delay > TimeSpan.Zero)
                {
                    await Task.Delay(Delay, token).ConfigureAwait(false);
                }
                else
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                }

                // Bail early if cancelled, returning default value for type
                if (token.IsCancellationRequested)
                {
                    result = default;
                }
                else
                {
                    // Load content
                    result = loadContent(token);
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException && token.IsCancellationRequested)
                {
                    return default;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (!OnLoadingError(e))
                {
                    throw;
                }

                return default;
            }

            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }

            // Invoke continuation unless cancelled
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

                    return default;
                }
            }

            return result;
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

            _cancellationSequence.CancelCurrent();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            {
                return;
            }

            _cancellationSequence?.Dispose();
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
