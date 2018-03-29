using System;
using System.Threading;

namespace GitUI
{
    /// <summary>
    /// Supports sequences of actions where the commencement of an action cancels any
    /// prior action.
    /// </summary>
    /// <remarks>
    /// This sequence does not guarantee that prior operations stop executing before
    /// <see cref="Next"/> returns. It only guarantees that their <see cref="CancellationToken"/>
    /// will be cancelled. Operations need to use their tokens in an appropriate manner.
    /// You may still require additional concurrency protections.
    /// </remarks>
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
    public sealed class CancellationTokenSequence : IDisposable
    {
        /// <remarks>
        /// If this field is <c>null</c>, the object has been disposed.
        /// </remarks>
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Issues the <see cref="CancellationToken"/> for use by the next asynchronous operation in the sequence,
        /// and cancels any prior token.
        /// </summary>
        /// <remarks>
        /// <para>This method does not guarantee that prior operations stop executing before
        /// it returns. It only guarantees that their <see cref="CancellationToken"/>
        /// will be cancelled. Operations need to use their tokens in an appropriate manner.
        /// You may still require additional concurrency protections.</para>
        ///
        /// <para>This method is thread-safe.</para>
        /// </remarks>
        /// <returns>A <see cref="CancellationToken"/> to be used by the commencing asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">This object is disposed.</exception>
        public CancellationToken Next()
        {
            var next = new CancellationTokenSource();

            // Make sure to obtain the CancellationToken for the next source before exposing the source,
            // or another thread could dispose of source before we get a chance to access this property.
            var nextToken = next.Token;

            // The following block is identical to Interlocked.Exchange, except no replacement is
            // made if the current field value is null (latch on null).
            var prior = Volatile.Read(ref _cancellationTokenSource);

            while (prior != null)
            {
                var candidate = Interlocked.CompareExchange(ref _cancellationTokenSource, next, prior);

                if (candidate == prior)
                {
                    break;
                }
                else
                {
                    prior = candidate;
                }
            }

            // Detect and handle the case where the sequence was already disposed
            if (prior is null)
            {
                next.Dispose();
                throw new ObjectDisposedException(nameof(CancellationTokenSequence));
            }

            prior.Cancel();
            prior.Dispose();

            return nextToken;
        }

        /// <summary>
        /// Cancels any current asynchronous operation.
        /// </summary>
        /// <remarks>
        /// It is safe to call this method multiple times, regardless of whether a
        /// new token has been issued or not.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This object is disposed.</exception>
        public void CancelCurrent()
        {
            // Calling Next() will cancel the current operation. Ignoring the return value means
            // an unnecessary CancellationTokenSource is allocated, but it will not be leaked or
            // otherwise interfere with the sequence.
            Next();
        }

        /// <summary>
        /// Cancels the current token and disposes this object.
        /// </summary>
        /// <remarks>
        /// Only the first call to this method per instance of this class has any effect.
        /// </remarks>
        public void Dispose()
        {
            var cancellationTokenSource = Interlocked.Exchange(ref _cancellationTokenSource, null);

            if (cancellationTokenSource is null)
            {
                return;
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}