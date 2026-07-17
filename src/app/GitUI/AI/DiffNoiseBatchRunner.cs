using System.Collections.Concurrent;

namespace GitUI.AI;

/// <summary>
/// Splits the files into batches and classifies them concurrently, reporting progress as each batch completes.
/// Shared by the <see cref="IDiffNoiseClassifier"/> backends.
/// </summary>
internal static class DiffNoiseBatchRunner
{
    internal static async Task<IReadOnlyList<DiffNoiseClassification>> RunAsync(
        IReadOnlyList<DiffFileContent> files,
        int maxCharsPerFile,
        int maxConcurrency,
        Func<IReadOnlyList<DiffFileContent>, CancellationToken, Task<IReadOnlyList<DiffNoiseClassification>>> classifyBatchAsync,
        IProgress<DiffNoiseProgress>? progress,
        CancellationToken cancellationToken)
    {
        List<List<DiffFileContent>> batches = [.. DiffNoisePrompt.CreateBatches(files, maxCharsPerFile)];
        if (batches.Count == 0)
        {
            return [];
        }

        ConcurrentBag<DiffNoiseClassification> results = [];
        int processed = 0;
        int total = files.Count;
        progress?.Report(new DiffNoiseProgress(0, total));

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = Math.Max(1, maxConcurrency),
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(batches, parallelOptions, async (batch, ct) =>
        {
            IReadOnlyList<DiffNoiseClassification> batchResults = await classifyBatchAsync(batch, ct);
            foreach (DiffNoiseClassification result in batchResults)
            {
                results.Add(result);
            }

            int done = Interlocked.Add(ref processed, batch.Count);
            progress?.Report(new DiffNoiseProgress(done, total));
        });

        return [.. results];
    }
}
