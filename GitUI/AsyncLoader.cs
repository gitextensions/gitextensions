using System;
using System.Threading;

namespace GitUI
{
    internal class AsyncLoader
    {
        public event EventHandler<ErrorEventArgs> LoadingError = delegate { };

        private readonly SynchronizationContext context;
        private readonly object taskLock;
        private ILoadingTask currentTask;

        public AsyncLoader()
            : this(SynchronizationContext.Current)
        {
        }

        public AsyncLoader(SynchronizationContext context)
        {
            this.context = context;
            taskLock = new object();
            currentTask = new LoadingTask<object>();
        }

        public void Load<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            var newTask = new LoadingTask<T>(context, loadContent, onLoaded, OnLoadingError);

            lock (taskLock)
            {
                currentTask.Cancel();
                currentTask = newTask;
            }

            newTask.RunAsync();
        }

        private void OnLoadingError(Exception exception)
        {
            LoadingError(this, new ErrorEventArgs(exception));
        }

        private interface ILoadingTask
        {
            void RunAsync();

            void Cancel();
        }

        private sealed class LoadingTask<T> : ILoadingTask
        {
            private readonly SynchronizationContext context;
            private readonly Func<T> loadContent;
            private readonly Action<T> onLoaded;
            private readonly Action<Exception> onError;
            private bool cancelled;

            public LoadingTask()
            {
                cancelled = true;
            }

            public LoadingTask(
                SynchronizationContext context,
                Func<T> loadContent,
                Action<T> onLoaded,
                Action<Exception> onError)
            {
                this.context = context;
                this.loadContent = loadContent;
                this.onLoaded = onLoaded;
                this.onError = onError;
            }

            public void Cancel()
            {
                cancelled = true;
            }

            public void RunAsync()
            {
                if (cancelled) return;
                RunOnWorker(delegate
                {
                    try
                    {
                        if (cancelled) return;
                        T content = loadContent();

                        if (cancelled) return;
                        RunOnUI(delegate
                        {
                            try
                            {
                                if (cancelled) return;
                                onLoaded(content);
                            }
                            catch (Exception exception)
                            {
                                if (cancelled) return;
                                onError(exception);
                            }
                        });
                    }
                    catch (Exception exception)
                    {
                        if (cancelled) return;
                        RunOnUI(delegate
                        {
                            if (cancelled) return;
                            onError(exception);
                        });
                    }
                });
            }

            private void RunOnWorker(Action action)
            {
                ThreadPool.QueueUserWorkItem(_ => action());
            }

            private void RunOnUI(Action action)
            {
                context.Post(_ => action(), null);
            }
        }
    }

    internal class ErrorEventArgs : EventArgs
    {
        private readonly Exception exception;

        public ErrorEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        public Exception Exception
        {
            get { return exception; }
        }
    }
}