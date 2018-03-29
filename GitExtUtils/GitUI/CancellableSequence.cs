using System;
using System.Threading;

namespace GitUI
{
    /// <summary>
    /// Supports sequences of actions where the commencement of an action cancels any
    /// prior action.
    /// </summary>
    /// <remarks>
    /// <example>
    /// Define an instance of this type, usually as a private readonly field:
    /// <code>
    /// private readonly CancellableSequence _sequence = new CancellableSequence();
    /// </code>
    /// Then use it to generate <see cref="CancellationToken"/> objects for use in asynchronous
    /// operations.
    /// <code>
    /// var token = _sequence.Next();
    /// // Do asynchronous operation using this token.
    /// // Subsequent calls to Next will cancel this token.
    /// </code>
    /// </example>
    /// </remarks>
    public sealed class CancellableSequence : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private int _isDisposed;

        /// <summary>
        /// Issues the <see cref="CancellationToken"/> for use by the next asynchronous operation in the sequence,
        /// and cancels any prior token.
        /// </summary>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This object is disposed.</exception>
        public CancellationToken Next()
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(CancellableSequence));
            }

            CancellationTokenSource prior;
            CancellationTokenSource next;

            do
            {
                prior = _cancellationTokenSource;

                // Cancel any existing action in the sequence
                prior.Cancel();

                // Create a cancellation token for the next action
                next = new CancellationTokenSource();
            }
            while (Interlocked.CompareExchange(ref _cancellationTokenSource, next, prior) == next);

            // The above CompareExchange prevents a race condition:
            //
            // - Two calls to Next occur simultaneously
            // - They both cancel the token that existed before they called
            // - Neither caller's token is cancelled and they both run to completion
            //
            // With this check, the code will loop for one of the callers such that the other is cancelled
            // and double the work is not performed.
            //
            // This pattern is similar in concept to software transactional memory.
            // Observers see atomic operations, though internally there may be intermediate
            // states.

            if (_isDisposed != 0)
            {
                next.Cancel();
            }

            return next.Token;
        }

        /// <summary>
        /// Cancels any current asynchronous operation.
        /// </summary>
        /// <remarks>
        /// It is safe to call this method multiple times, regardless of whether a
        /// new token has been issued or not.
        /// </remarks>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Cancels the current token and disposes this object.
        /// </summary>
        /// <remarks>
        /// Only the first call to this method per instance of this class has any effect.
        /// </remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
            {
                return;
            }

            var cancellationTokenSource = _cancellationTokenSource;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}