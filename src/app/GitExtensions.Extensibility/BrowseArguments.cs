using GitExtensions.Extensibility.Git;

namespace GitExtensions.Extensibility
{
    public record BrowseArguments
    {
        /// <summary>
        ///  Gets the revision filter to apply to browse.
        /// </summary>
        public string? RevFilter { get; init; }

        /// <summary>
        ///  Gets the path filter to apply to browse.
        /// </summary>
        public string? PathFilter { get; init; }

        /// <summary>
        ///  Gets the currently (last) selected commit id.
        /// </summary>
        public ObjectId? SelectedId { get; init; }

        /// <summary>
        ///  Gets the first selected commit id (as in a diff).
        /// </summary>
        public ObjectId? FirstId { get; init; }

        /// <summary>
        /// If to start in "FileHistory mode", hiding left panel.
        /// </summary>
        public bool IsFileHistoryMode { get; init; }
    }
}
