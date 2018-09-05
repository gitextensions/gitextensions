namespace GitCommands.Submodules
{
    /// <summary>
    /// Contains submodule information that is loaded asynchronously.
    /// </summary>
    public class SubmoduleInfo
    {
        /// <summary>
        /// User-friendly display text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Full path to submodule
        /// </summary>
        public string Path { get; set; }

        public SubmoduleStatus? Status { get; set; }
        public bool IsDirty { get; set; }
        public bool Bold { get; set; }
    }
}
