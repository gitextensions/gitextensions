namespace GitCommands
{
    /// <summary>
    /// Result model for batch processing arguments and count of items for batch progress
    /// </summary>
    public sealed class BatchArgumentItem
    {
        public BatchArgumentItem(ArgumentString argument, int count)
        {
            Argument = argument;
            BatchItemsCount = count;
        }

        /// <summary>
        /// Batch command line argument
        /// </summary>
        public ArgumentString Argument { get; }

        /// <summary>
        /// Count of items in batch, used for batch progress update
        /// </summary>
        public int BatchItemsCount { get; }
    }
}
