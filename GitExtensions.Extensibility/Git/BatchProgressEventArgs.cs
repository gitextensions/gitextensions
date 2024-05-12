namespace GitExtensions.Extensibility.Git;

/// <summary>
/// Event arguments for batch progress updating
/// </summary>
public sealed class BatchProgressEventArgs : EventArgs
{
    public BatchProgressEventArgs(int batchItemsProcessed, bool executionResult)
    {
        ProcessedCount = batchItemsProcessed;
        ExecutionResult = executionResult;
    }

    /// <summary>
    /// Number of items processed in this batch event
    /// </summary>
    public int ProcessedCount { get; }

    /// <summary>
    /// Batch execution result
    /// </summary>
    public bool ExecutionResult { get; }
}
