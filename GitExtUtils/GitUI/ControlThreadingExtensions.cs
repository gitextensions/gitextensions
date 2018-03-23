using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public static class ControlThreadingExtensions
    {
        private static readonly CancellationToken _preCancelledToken;
        private static readonly ConditionalWeakTable<Control, StrongBox<CancellationToken>> _controlDisposed;
        private static readonly ConditionalWeakTable<Control, StrongBox<CancellationToken>>.CreateValueCallback _controlDisposedCancellationTokenFactory;

        static ControlThreadingExtensions()
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                _preCancelledToken = cts.Token;
            }

            _controlDisposed = new ConditionalWeakTable<Control, StrongBox<CancellationToken>>();
            _controlDisposedCancellationTokenFactory = control =>
            {
                if (control.IsDisposed)
                {
                    return new StrongBox<CancellationToken>(_preCancelledToken);
                }

                var cts = new CancellationTokenSource();

                // Get a copy of the CancellationToken before the source can be disposed. After the source is cancelled
                // and disposed, the CancellationToken will continue to behave properly, but
                // CancellationTokenSource.Token will start to throw an ObjectDisposedException.
                var token = cts.Token;

                control.Disposed += delegate
                {
                    CancelAndDispose(cts);
                };

                if (control.IsDisposed)
                {
                    // Handle control disposed on another thread while registering event handler
                    CancelAndDispose(cts);
                }

                return new StrongBox<CancellationToken>(token);
            };

            return;

            // Local functions
            void CancelAndDispose(CancellationTokenSource cancellationTokenSource)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // This can occur in race conditions
                }

                cancellationTokenSource.Dispose();
            }
        }

        public static ControlMainThreadAwaitable SwitchToMainThreadAsync(this Control control)
        {
            if (control.IsDisposed)
            {
                return new ControlMainThreadAwaitable(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_preCancelledToken), _preCancelledToken);
            }

            var disposedCancellationToken = _controlDisposed.GetValue(control, _controlDisposedCancellationTokenFactory).Value;
            var mainThreadAwaiter = ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(disposedCancellationToken);
            return new ControlMainThreadAwaitable(mainThreadAwaiter, disposedCancellationToken);
        }

        public struct ControlMainThreadAwaitable
        {
            private readonly JoinableTaskFactory.MainThreadAwaitable _awaitable;
            private readonly CancellationToken _cancellationToken;

            internal ControlMainThreadAwaitable(JoinableTaskFactory.MainThreadAwaitable awaitable, CancellationToken cancellationToken)
            {
                _awaitable = awaitable;
                _cancellationToken = cancellationToken;
            }

            public ControlMainThreadAwaiter GetAwaiter()
            {
                return new ControlMainThreadAwaiter(_awaitable.GetAwaiter(), _cancellationToken);
            }
        }

        public struct ControlMainThreadAwaiter : INotifyCompletion
        {
            private readonly JoinableTaskFactory.MainThreadAwaiter _awaiter;
            private readonly CancellationToken _cancellationToken;

            internal ControlMainThreadAwaiter(JoinableTaskFactory.MainThreadAwaiter awaiter, CancellationToken cancellationToken)
            {
                _awaiter = awaiter;
                _cancellationToken = cancellationToken;
            }

            public bool IsCompleted => _awaiter.IsCompleted;

            public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

            public void GetResult()
            {
                _awaiter.GetResult();

                // The default MainThreadAwaiter only throws an exception if we fail to reach the main thread. This call
                // ensures we always cancel the continuation if we somehow reach the UI thread after the control was
                // disposed.
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
