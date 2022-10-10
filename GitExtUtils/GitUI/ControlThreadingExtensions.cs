﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public static class ControlThreadingExtensions
    {
        private static readonly CancellationToken _preCancelledToken;
        private static readonly ConditionalWeakTable<IComponent, StrongBox<CancellationToken>> _controlDisposed;

        static ControlThreadingExtensions()
        {
            using CancellationTokenSource cts = new();
            cts.Cancel();
            _preCancelledToken = cts.Token;

            _controlDisposed = new ConditionalWeakTable<IComponent, StrongBox<CancellationToken>>();
        }

#pragma warning disable VSTHRD004 // Await SwitchToMainThreadAsync
        public static ControlMainThreadAwaitable SwitchToMainThreadAsync(this ToolStripItem control, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ControlMainThreadAwaitable(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken), disposable: null);
            }

            if (control.IsDisposed)
            {
                return new ControlMainThreadAwaitable(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_preCancelledToken), disposable: null);
            }

            var disposedCancellationToken = ToolStripItemDisposedCancellationFactory.Instance.GetOrCreateCancellationToken(control);
            CancellationTokenSource? cancellationTokenSource = null;
            if (cancellationToken.CanBeCanceled)
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disposedCancellationToken, cancellationToken);
                disposedCancellationToken = cancellationTokenSource.Token;
            }

            var mainThreadAwaiter = ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(disposedCancellationToken);
            return new ControlMainThreadAwaitable(mainThreadAwaiter, cancellationTokenSource);
        }

        public static ControlMainThreadAwaitable SwitchToMainThreadAsync(this Control control, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ControlMainThreadAwaitable(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken), disposable: null);
            }

            if (control.IsDisposed)
            {
                return new ControlMainThreadAwaitable(ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_preCancelledToken), disposable: null);
            }

            var disposedCancellationToken = ControlIsDisposedCancellationFactory.Instance.GetOrCreateCancellationToken(control);
            CancellationTokenSource? cancellationTokenSource = null;
            if (cancellationToken.CanBeCanceled)
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disposedCancellationToken, cancellationToken);
                disposedCancellationToken = cancellationTokenSource.Token;
            }

            var mainThreadAwaiter = ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(disposedCancellationToken);
            return new ControlMainThreadAwaitable(mainThreadAwaiter, cancellationTokenSource);
        }
#pragma warning restore VSTHRD004 // Await SwitchToMainThreadAsync

        public readonly struct ControlMainThreadAwaitable
        {
            private readonly JoinableTaskFactory.MainThreadAwaitable _awaitable;
            private readonly IDisposable? _disposable;

            internal ControlMainThreadAwaitable(JoinableTaskFactory.MainThreadAwaitable awaitable, IDisposable? disposable)
            {
                _awaitable = awaitable;
                _disposable = disposable;
            }

            public ControlMainThreadAwaiter GetAwaiter()
            {
                return new ControlMainThreadAwaiter(_awaitable.GetAwaiter(), _disposable);
            }
        }

        public readonly struct ControlMainThreadAwaiter : INotifyCompletion
        {
            private readonly JoinableTaskFactory.MainThreadAwaiter _awaiter;
            private readonly IDisposable? _disposable;

            internal ControlMainThreadAwaiter(JoinableTaskFactory.MainThreadAwaiter awaiter, IDisposable? disposable)
            {
                _awaiter = awaiter;
                _disposable = disposable;
            }

            public bool IsCompleted => _awaiter.IsCompleted;

            public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

            public void GetResult()
            {
                try
                {
                    _awaiter.GetResult();
                }
                finally
                {
                    _disposable?.Dispose();
                }
            }
        }

        private sealed class ControlIsDisposedCancellationFactory : IsDisposedCancellationFactory<Control>
        {
            public static readonly ControlIsDisposedCancellationFactory Instance = new();

            protected override bool IsDisposed(Control component) => component.IsDisposed;
        }

        private sealed class ToolStripItemDisposedCancellationFactory : IsDisposedCancellationFactory<ToolStripItem>
        {
            public static readonly ToolStripItemDisposedCancellationFactory Instance = new();

            protected override bool IsDisposed(ToolStripItem component) => component.IsDisposed;
        }

        private abstract class IsDisposedCancellationFactory<T>
            where T : class, IComponent
        {
            private readonly ConditionalWeakTable<IComponent, StrongBox<CancellationToken>>.CreateValueCallback _disposedCancellationTokenFactory;

            protected IsDisposedCancellationFactory()
            {
                _disposedCancellationTokenFactory = control =>
                {
                    if (IsDisposed((T)control))
                    {
                        return new StrongBox<CancellationToken>(_preCancelledToken);
                    }

                    CancellationTokenSource cts = new();

                    // Get a copy of the CancellationToken before the source can be disposed. After the source is cancelled
                    // and disposed, the CancellationToken will continue to behave properly, but
                    // CancellationTokenSource.Token will start to throw an ObjectDisposedException.
                    var token = cts.Token;

                    control.Disposed += delegate
                    {
                        CancelAndDispose(cts);
                    };

                    if (IsDisposed((T)control))
                    {
                        // Handle control disposed on another thread while registering event handler
                        CancelAndDispose(cts);
                    }

                    return new StrongBox<CancellationToken>(token);
                };
            }

            public CancellationToken GetOrCreateCancellationToken(T component)
            {
                return _controlDisposed.GetValue(component, _disposedCancellationTokenFactory).Value;
            }

            protected abstract bool IsDisposed(T component);

            private static void CancelAndDispose(CancellationTokenSource cancellationTokenSource)
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
    }
}
