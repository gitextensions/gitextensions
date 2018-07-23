using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;

namespace GitUI
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets an awaitable that schedules the continuation with a preference to executing synchronously on the callstack that completed the <see cref="Task"/>,
        /// without regard to thread ID or any <see cref="SynchronizationContext"/> that may be applied when the continuation is scheduled or when the antecedent completes.
        /// </summary>
        /// <param name="antecedent">The task to await on.</param>
        /// <returns>An awaitable.</returns>
        /// <remarks>
        /// If there is not enough stack space remaining on the thread that is completing the <paramref name="antecedent"/> <see cref="Task"/>,
        /// the continuation may be scheduled on the threadpool.
        /// </remarks>
        public static ExecuteContinuationSynchronouslyAwaitable ConfigureAwaitRunInline(this Task antecedent)
        {
            Requires.NotNull(antecedent, nameof(antecedent));

            return new ExecuteContinuationSynchronouslyAwaitable(antecedent);
        }

        /// <summary>
        /// Gets an awaitable that schedules the continuation with a preference to executing synchronously on the callstack that completed the <see cref="Task"/>,
        /// without regard to thread ID or any <see cref="SynchronizationContext"/> that may be applied when the continuation is scheduled or when the antecedent completes.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the awaited <see cref="Task"/>.</typeparam>
        /// <param name="antecedent">The task to await on.</param>
        /// <returns>An awaitable.</returns>
        /// <remarks>
        /// If there is not enough stack space remaining on the thread that is completing the <paramref name="antecedent"/> <see cref="Task"/>,
        /// the continuation may be scheduled on the threadpool.
        /// </remarks>
        public static ExecuteContinuationSynchronouslyAwaitable<T> ConfigureAwaitRunInline<T>(this Task<T> antecedent)
        {
            Requires.NotNull(antecedent, nameof(antecedent));

            return new ExecuteContinuationSynchronouslyAwaitable<T>(antecedent);
        }

        /// <summary>
        /// A Task awaitable that has affinity to executing callbacks synchronously on the completing callstack.
        /// </summary>
        public struct ExecuteContinuationSynchronouslyAwaitable
        {
            /// <summary>
            /// The task whose completion will execute the continuation.
            /// </summary>
            private readonly Task _antecedent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecuteContinuationSynchronouslyAwaitable"/> struct.
            /// </summary>
            /// <param name="antecedent">The task whose completion will execute the continuation.</param>
            public ExecuteContinuationSynchronouslyAwaitable(Task antecedent)
            {
                Requires.NotNull(antecedent, nameof(antecedent));
                _antecedent = antecedent;
            }

            /// <summary>
            /// Gets the awaiter.
            /// </summary>
            /// <returns>The awaiter</returns>
            public ExecuteContinuationSynchronouslyAwaiter GetAwaiter() => new ExecuteContinuationSynchronouslyAwaiter(_antecedent);
        }

        /// <summary>
        /// A Task awaiter that has affinity to executing callbacks synchronously on the completing callstack.
        /// </summary>
        public struct ExecuteContinuationSynchronouslyAwaiter : INotifyCompletion
        {
            /// <summary>
            /// The task whose completion will execute the continuation.
            /// </summary>
            private readonly Task _antecedent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecuteContinuationSynchronouslyAwaiter"/> struct.
            /// </summary>
            /// <param name="antecedent">The task whose completion will execute the continuation.</param>
            public ExecuteContinuationSynchronouslyAwaiter(Task antecedent)
            {
                Requires.NotNull(antecedent, nameof(antecedent));
                _antecedent = antecedent;
            }

            /// <summary>
            /// Gets a value indicating whether the antecedent <see cref="Task"/> has already completed.
            /// </summary>
            public bool IsCompleted => _antecedent.IsCompleted;

            /// <summary>
            /// Rethrows any exception thrown by the antecedent.
            /// </summary>
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            public void GetResult() => _antecedent.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            /// <summary>
            /// Schedules a callback to run when the antecedent task completes.
            /// </summary>
            /// <param name="continuation">The callback to invoke.</param>
            public void OnCompleted(Action continuation)
            {
                Requires.NotNull(continuation, nameof(continuation));

                _antecedent.ContinueWith(
                    (_, s) => ((Action)s)(),
                    continuation,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }

        /// <summary>
        /// A Task awaitable that has affinity to executing callbacks synchronously on the completing callstack.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the awaited <see cref="Task"/>.</typeparam>
        public struct ExecuteContinuationSynchronouslyAwaitable<T>
        {
            /// <summary>
            /// The task whose completion will execute the continuation.
            /// </summary>
            private readonly Task<T> _antecedent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecuteContinuationSynchronouslyAwaitable{T}"/> struct.
            /// </summary>
            /// <param name="antecedent">The task whose completion will execute the continuation.</param>
            public ExecuteContinuationSynchronouslyAwaitable(Task<T> antecedent)
            {
                Requires.NotNull(antecedent, nameof(antecedent));
                _antecedent = antecedent;
            }

            /// <summary>
            /// Gets the awaiter.
            /// </summary>
            /// <returns>The awaiter</returns>
            public ExecuteContinuationSynchronouslyAwaiter<T> GetAwaiter() => new ExecuteContinuationSynchronouslyAwaiter<T>(_antecedent);
        }

        /// <summary>
        /// A Task awaiter that has affinity to executing callbacks synchronously on the completing callstack.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the awaited <see cref="Task"/>.</typeparam>
        public struct ExecuteContinuationSynchronouslyAwaiter<T> : INotifyCompletion
        {
            /// <summary>
            /// The task whose completion will execute the continuation.
            /// </summary>
            private readonly Task<T> _antecedent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecuteContinuationSynchronouslyAwaiter{T}"/> struct.
            /// </summary>
            /// <param name="antecedent">The task whose completion will execute the continuation.</param>
            public ExecuteContinuationSynchronouslyAwaiter(Task<T> antecedent)
            {
                Requires.NotNull(antecedent, nameof(antecedent));
                _antecedent = antecedent;
            }

            /// <summary>
            /// Gets a value indicating whether the antecedent <see cref="Task"/> has already completed.
            /// </summary>
            public bool IsCompleted => _antecedent.IsCompleted;

            /// <summary>
            /// Rethrows any exception thrown by the antecedent.
            /// </summary>
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            public T GetResult() => _antecedent.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            /// <summary>
            /// Schedules a callback to run when the antecedent task completes.
            /// </summary>
            /// <param name="continuation">The callback to invoke.</param>
            public void OnCompleted(Action continuation)
            {
                Requires.NotNull(continuation, nameof(continuation));

                _antecedent.ContinueWith(
                    (_, s) => ((Action)s)(),
                    continuation,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
    }
}
