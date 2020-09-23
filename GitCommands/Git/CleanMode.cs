namespace GitCommands
{
    /// <summary>Mode for 'git clean'</summary>
    public enum CleanMode
    {
        /// <summary>Only untracked files not in .gitignore, the default. Git clean without either -x or -X option.</summary>
        OnlyNonIgnored = 0,

        /// <summary>Only files included in any ignore list (.gitignore, $GIT_DIR/info/exclude). Git clean with -X option.</summary>
        OnlyIgnored,

        /// <summary>All files not tracked by Git. Git clean with  -x option.</summary>
        All
    }
}
