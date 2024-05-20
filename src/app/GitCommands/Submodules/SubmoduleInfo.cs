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
        public string Text { get; }

        /// <summary>
        /// Full path to submodule
        /// </summary>
        public string Path { get; }

        public bool Bold { get; }

        public DetailedSubmoduleInfo? Detailed { get; set; }

        public SubmoduleInfo(string text, string path, bool bold)
        {
            Text = text;
            Path = path;
            Bold = bold;
        }
    }
}
