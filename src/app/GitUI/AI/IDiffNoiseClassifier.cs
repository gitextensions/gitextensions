namespace GitUI.AI;

/// <summary>A single file's unified diff to be classified.</summary>
/// <param name="Path">The file path, used as a stable key to correlate the response.</param>
/// <param name="Diff">The unified diff text of the file.</param>
public sealed record DiffFileContent(string Path, string Diff);

/// <summary>The classification result for a single file.</summary>
/// <param name="Path">The file path, matching the requested <see cref="DiffFileContent.Path"/>.</param>
/// <param name="Category">The category the file's diff was classified into.</param>
/// <param name="Reason">An optional short human-readable justification.</param>
public sealed record DiffNoiseClassification(string Path, DiffNoiseCategory Category, string? Reason);

/// <summary>Progress of a classification run.</summary>
/// <param name="ProcessedFiles">The number of files classified so far.</param>
/// <param name="TotalFiles">The total number of files to classify.</param>
public readonly record struct DiffNoiseProgress(int ProcessedFiles, int TotalFiles);

/// <summary>
/// Classifies file diffs into <see cref="DiffNoiseCategory"/> so that "noise" changes can be filtered
/// away from the diff view. Implementations may call an external AI service.
/// </summary>
public interface IDiffNoiseClassifier
{
    /// <summary>
    /// Classifies each of the supplied file diffs. A file is only assigned a non-<see cref="DiffNoiseCategory.None"/>
    /// category when <em>all</em> of its changes fall into that single category.
    /// </summary>
    /// <param name="files">The file diffs to classify.</param>
    /// <param name="options">Which categories the caller is interested in (used to focus the prompt).</param>
    /// <param name="progress">Optional progress reporter, invoked as batches complete.</param>
    /// <param name="cancellationToken">A token to cancel the (potentially long-running) request.</param>
    /// <returns>One classification per input file. Files that could not be classified are omitted.</returns>
    Task<IReadOnlyList<DiffNoiseClassification>> ClassifyAsync(
        IReadOnlyList<DiffFileContent> files,
        DiffNoiseFilterOptions options,
        IProgress<DiffNoiseProgress>? progress,
        CancellationToken cancellationToken);
}
