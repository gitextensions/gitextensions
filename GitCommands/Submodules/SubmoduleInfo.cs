namespace GitCommands.Submodules
{
    /// <summary>Holds submodule information that is gathered asynchronously.</summary>
    public class SubmoduleInfo
    {
        public string Text; // User-friendly display text
        public string Path; // Full path to submodule
        public SubmoduleStatus? Status;
        public bool IsDirty;
        public bool Bold;
    }
}
