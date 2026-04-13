using GitUI;

namespace GitUITests;
public sealed class CancellationTokenSequenceTests
{
    [Test]
    public void Next_cancels_previous_token()
    {
        CancellationTokenSequence sequence = new();

        CancellationToken token1 = sequence.Next();

        token1.IsCancellationRequested.Should().BeFalse();

        CancellationToken token2 = sequence.Next();

        token1.IsCancellationRequested.Should().BeTrue();
        token2.IsCancellationRequested.Should().BeFalse();
    }

    [Test]
    public void Next_throws_if_disposed()
    {
        CancellationTokenSequence sequence = new();

        sequence.Dispose();

        ((Action)(() => sequence.Next())).Should().Throw<OperationCanceledException>();
    }

    [Test]
    public void Cancel_cancels_previous_token()
    {
        CancellationTokenSequence sequence = new();

        CancellationToken token = sequence.Next();

        token.IsCancellationRequested.Should().BeFalse();

        sequence.CancelCurrent();

        token.IsCancellationRequested.Should().BeTrue();
    }

    [Test]
    public void Cancel_is_idempotent()
    {
        CancellationTokenSequence sequence = new();

        CancellationToken token = sequence.Next();

        token.IsCancellationRequested.Should().BeFalse();

        sequence.CancelCurrent();
        sequence.CancelCurrent();
        sequence.CancelCurrent();
        sequence.CancelCurrent();
    }

    [Test]
    public void Cancel_does_not_throw_if_no_token_yet_issued()
    {
        CancellationTokenSequence sequence = new();

        sequence.CancelCurrent();
    }

    [Test]
    public void Dispose_cancels_previous_token()
    {
        CancellationTokenSequence sequence = new();

        CancellationToken token = sequence.Next();

        token.IsCancellationRequested.Should().BeFalse();

        sequence.Dispose();

        token.IsCancellationRequested.Should().BeTrue();
    }

    [Test]
    public void Dispose_is_idempotent()
    {
        CancellationTokenSequence sequence = new();

        sequence.Next();

        sequence.Dispose();
        sequence.Dispose();
        sequence.Dispose();
        sequence.Dispose();
    }

    [Test]
    public void Dispose_does_not_throw_if_no_token_yet_issued()
    {
        CancellationTokenSequence sequence = new();

        sequence.Dispose();
    }

    [Test]
    public async Task Concurrent_callers_to_Next_only_result_in_one_non_cancelled_token_being_issued()
    {
        const int loopCount = 1000;

        int logicalProcessorCount = Environment.ProcessorCount;

        // Cap threads to avoid O(n²) CAS contention in Next() on high-core-count machines
        int threadCount = Math.Min(8, Math.Max(2, logicalProcessorCount));

        using CancellationTokenSequence sequence = new();
        using Barrier barrier = new(threadCount);
        int completedCount = 0;
        int[] winnerByIndex = new int[threadCount];

        List<Task> tasks = [.. Enumerable
            .Range(0, threadCount)
            .Select(i => Task.Run(() => ThreadMethod(i)))];

        Task allTasks = Task.WhenAll(tasks);
        Task completed = await Task.WhenAny(allTasks, Task.Delay(TimeSpan.FromSeconds(10)));

        completed.Should().Be(allTasks, "Test should have completed within a reasonable amount of time");

        completedCount.Should().Be(loopCount);

        await Console.Out.WriteLineAsync("Winner by index: " + string.Join(",", winnerByIndex));

        // Assume hyper threading, so halve logical processors (could use WMI or P/Invoke for more robust answer)
        if (logicalProcessorCount <= 2)
        {
            Assert.Inconclusive("This test requires more than one physical processor to run.");
        }

        return;

        void ThreadMethod(int i)
        {
            for (int j = 0; j < loopCount; j++)
            {
                barrier.SignalAndWait();

                CancellationToken token = sequence.Next();

                barrier.SignalAndWait();

                if (!token.IsCancellationRequested)
                {
                    Interlocked.Increment(ref completedCount);
                    Interlocked.Increment(ref winnerByIndex[i]);
                }
            }
        }
    }
}
