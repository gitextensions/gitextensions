namespace GitUI.UiSettings
{
    /// <summary>Specifies whether to automatically prune stale remote-tracking branches.
    /// <remarks>A 'stale' remote-tracking branch is one that has been deleted on the remote repo,
    /// yet still lives in 'refs/remotes/'.</remarks></summary>
    public enum AutoPrune
    {
        /// <summary>Never auto-prune stale remote-tracking branches.</summary>
        Never,
        /// <summary>Only auto-prune stale remote-tracking branches
        ///  if they are merged in an existing remote branch.</summary>
        IfMerged,
        /// <summary>Always auto-prune stale remote-tracking branches.</summary>
        Always
    }
}
