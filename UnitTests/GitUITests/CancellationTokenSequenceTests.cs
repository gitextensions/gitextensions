using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public sealed class CancellationTokenSequenceTests
    {
        [Test]
        public void Next_cancels_previous_token()
        {
            var sequence = new CancellationTokenSequence();

            var token1 = sequence.Next();

            Assert.False(token1.IsCancellationRequested);

            var token2 = sequence.Next();

            Assert.True(token1.IsCancellationRequested);
            Assert.False(token2.IsCancellationRequested);
        }

        [Test]
        public void Next_throws_if_disposed()
        {
            var sequence = new CancellationTokenSequence();

            sequence.Dispose();

            Assert.Throws<ObjectDisposedException>(() => sequence.Next());
        }

        [Test]
        public void Cancel_cancels_previous_token()
        {
            var sequence = new CancellationTokenSequence();

            var token = sequence.Next();

            Assert.False(token.IsCancellationRequested);

            sequence.CancelCurrent();

            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public void Cancel_is_idempotent()
        {
            var sequence = new CancellationTokenSequence();

            var token = sequence.Next();

            Assert.False(token.IsCancellationRequested);

            sequence.CancelCurrent();
            sequence.CancelCurrent();
            sequence.CancelCurrent();
            sequence.CancelCurrent();
        }

        [Test]
        public void Cancel_does_not_throw_if_no_token_yet_issued()
        {
            var sequence = new CancellationTokenSequence();

            sequence.CancelCurrent();
        }

        [Test]
        public void Dispose_cancels_previous_token()
        {
            var sequence = new CancellationTokenSequence();

            var token = sequence.Next();

            Assert.False(token.IsCancellationRequested);

            sequence.Dispose();

            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public void Dispose_is_idempotent()
        {
            var sequence = new CancellationTokenSequence();

            sequence.Next();

            sequence.Dispose();
            sequence.Dispose();
            sequence.Dispose();
            sequence.Dispose();
        }

        [Test]
        public void Dispose_does_not_throw_if_no_token_yet_issued()
        {
            var sequence = new CancellationTokenSequence();

            sequence.Dispose();
        }

        [Test]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task Concurrent_callers_to_Next_only_result_in_one_non_cancelled_token_being_issued()
        {
            const int loopCount = 10000;

            var logicalProcessorCount = Environment.ProcessorCount;
            var threadCount = Math.Max(2, logicalProcessorCount);

            using (var sequence = new CancellationTokenSequence())
            using (var barrier = new Barrier(threadCount))
            using (var countdown = new CountdownEvent(loopCount * threadCount))
            {
                var completedCount = 0;

                var completionTokenSource = new CancellationTokenSource();
                var completionToken = completionTokenSource.Token;
                var winnerByIndex = new int[threadCount];

                var tasks = Enumerable
                    .Range(0, threadCount)
                    .Select(i => Task.Run(() => ThreadMethod(i)))
                    .ToList();

                Assert.True(
                    countdown.Wait(TimeSpan.FromSeconds(10)),
                    "Test should have completed within a reasonable amount of time");

                await Task.WhenAll(tasks);

                Assert.AreEqual(loopCount, completedCount);

                await Console.Out.WriteLineAsync("Winner by index: " + string.Join(",", winnerByIndex));

                // Assume hyper threading, so halve logical processors (could use WMI or P/Invoke for more robust answer)
                if (logicalProcessorCount <= 2)
                {
                    Assert.Inconclusive("This test requires more than one physical processor to run.");
                }

                return;

                void ThreadMethod(int i)
                {
                    while (true)
                    {
                        barrier.SignalAndWait();

                        if (completionToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var token = sequence.Next();

                        barrier.SignalAndWait();

                        if (!token.IsCancellationRequested)
                        {
                            Interlocked.Increment(ref completedCount);
                            Interlocked.Increment(ref winnerByIndex[i]);
                        }

                        if (countdown.Signal())
                        {
                            completionTokenSource.Cancel();
                        }
                    }
                }
            }
        }
    }
}