using Microsoft.VisualStudio.Threading;

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

        public AsyncLazy<DetailedSubmoduleInfo> Detailed { get; set; }
        public bool Bold { get; set; }
    }
}
