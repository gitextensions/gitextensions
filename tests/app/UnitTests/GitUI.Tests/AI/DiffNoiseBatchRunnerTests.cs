using System.Collections.Concurrent;
using GitUI.AI;

namespace GitUITests.AI;

[TestFixture]
public sealed class DiffNoiseBatchRunnerTests
{
    private sealed class CollectingProgress : IProgress<DiffNoiseProgress>
    {
        private readonly ConcurrentQueue<DiffNoiseProgress> _reports = new();

        public IReadOnlyCollection<DiffNoiseProgress> Reports => _reports;

        public void Report(DiffNoiseProgress value) => _reports.Enqueue(value);
    }

    [Test]
    public async Task RunAsync_aggregates_all_batch_results()
    {
        List<DiffFileContent> files = [.. Enumerable.Range(0, 32).Select(i => new DiffFileContent($"f{i}.cs", "x"))];

        IReadOnlyList<DiffNoiseClassification> result = await DiffNoiseBatchRunner.RunAsync(
            files,
            maxCharsPerFile: 1000,
            maxConcurrency: 4,
            classifyBatchAsync: (batch, _) => Task.FromResult<IReadOnlyList<DiffNoiseClassification>>(
                [.. batch.Select(f => new DiffNoiseClassification(f.Path, DiffNoiseCategory.Imports, null))]),
            progress: null,
            cancellationToken: default);

        result.Should().HaveCount(32);
        result.Select(r => r.Path).Should().BeEquivalentTo(files.Select(f => f.Path));
    }

    [Test]
    public async Task RunAsync_reports_progress_up_to_total()
    {
        List<DiffFileContent> files = [.. Enumerable.Range(0, 20).Select(i => new DiffFileContent($"f{i}.cs", "x"))];
        CollectingProgress progress = new();

        await DiffNoiseBatchRunner.RunAsync(
            files,
            maxCharsPerFile: 1000,
            maxConcurrency: 2,
            classifyBatchAsync: (batch, _) => Task.FromResult<IReadOnlyList<DiffNoiseClassification>>([]),
            progress: progress,
            cancellationToken: default);

        progress.Reports.Should().Contain(new DiffNoiseProgress(0, 20));
        progress.Reports.Select(r => r.ProcessedFiles).Max().Should().Be(20);
        progress.Reports.Should().OnlyContain(r => r.TotalFiles == 20);
    }

    [Test]
    public async Task RunAsync_returns_empty_for_no_files()
    {
        IReadOnlyList<DiffNoiseClassification> result = await DiffNoiseBatchRunner.RunAsync(
            [],
            maxCharsPerFile: 1000,
            maxConcurrency: 4,
            classifyBatchAsync: (_, _) => throw new InvalidOperationException("should not be called"),
            progress: null,
            cancellationToken: default);

        result.Should().BeEmpty();
    }
}
