using System;
using System.ComponentModel;
using System.Threading;

namespace GitUI
{
    internal class AsyncLoader
    {
        public event EventHandler<ErrorEventArgs> LoadingError = delegate { };

        private readonly ISynchronizeInvoke control;
        private readonly object taskLock;
        private ILoadingTask currentTask;

        public AsyncLoader(ISynchronizeInvoke control)
        {
            this.control = control;
            taskLock = new object();
            currentTask = new LoadingTask<object>();
        }

        public void Load<T>(Func<T> loadContent, Action<T> onLoaded)
        {
            var newTask = new LoadingTask<T>(control, loadContent, onLoaded, OnLoadingError);

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
            private readonly ISynchronizeInvoke control;
            private readonly Func<T> loadContent;
            private readonly Action<T> onLoaded;
            private readonly Action<Exception> onError;
            private bool cancelled;

            public LoadingTask()
            {
                cancelled = true;
            }

            public LoadingTask(
                ISynchronizeInvoke control,
                Func<T> loadContent,
                Action<T> onLoaded,
                Action<Exception> onError)
            {
                this.control = control;
                this.loadContent = loadContent;
                this.onLoaded = onLoaded;
                this.onError = onError;
            }

            public void RunAsync()
            {
                if (cancelled) return;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        if (cancelled) return;
                        T content = loadContent();

                        if (cancelled) return;
                        BeginInvoke(delegate
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
                        BeginInvoke(delegate
                        {
                            if (cancelled) return;
                            onError(exception);
                        });
                    }
                });
            }

            private void BeginInvoke(Action action)
            {
                control.BeginInvoke(action, null);
            }

            public void Cancel()
            {
                cancelled = true;
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

namespace System
{
    // Copy of .NET 3.5 delegates, remove if GitExtensions
    // starts targetting a newer version of .NET

    public delegate void Action();

    public delegate void Action<T>(T arg);

    public delegate T Func<T>();
}