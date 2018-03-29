using System;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public sealed class CancellableSequenceTests
    {
        [Test]
        public void Next_cancels_previous_token()
        {
            var sequence = new CancellableSequence();

            var token1 = sequence.Next();

            Assert.False(token1.IsCancellationRequested);

            var token2 = sequence.Next();

            Assert.True(token1.IsCancellationRequested);
            Assert.False(token2.IsCancellationRequested);
        }

        [Test]
        public void Next_throws_if_disposed()
        {
            var sequence = new CancellableSequence();

            sequence.Dispose();

            Assert.Throws<ObjectDisposedException>(() => sequence.Next());
        }

        [Test]
        public void Cancel_cancels_previous_token()
        {
            var sequence = new CancellableSequence();

            var token = sequence.Next();

            Assert.False(token.IsCancellationRequested);

            sequence.Cancel();

            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public void Cancel_is_idempotent()
        {
            var sequence = new CancellableSequence();

            var token = sequence.Next();

            Assert.False(token.IsCancellationRequested);

            sequence.Cancel();
            sequence.Cancel();
            sequence.Cancel();
            sequence.Cancel();
        }

        [Test]
        public void Cancel_does_not_throw_if_no_token_yet_issued()
        {
            var sequence = new CancellableSequence();

            sequence.Cancel();
        }

        [Test]
        public void Dispose_cancels_previous_token()
        {
            var sequence = new CancellableSequence();

            var token = sequence.Next();

            Assert.False(token.IsCancellationRequested);

            sequence.Dispose();

            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public void Dispose_is_idempotent()
        {
            var sequence = new CancellableSequence();

            sequence.Next();

            sequence.Dispose();
            sequence.Dispose();
            sequence.Dispose();
            sequence.Dispose();
        }

        [Test]
        public void Dispose_does_not_throw_if_no_token_yet_issued()
        {
            var sequence = new CancellableSequence();

            sequence.Dispose();
        }
    }
}