namespace GitExtensions.Extensibility.Git;

/// <summary>Specifies whether to ignore changes to submodules when looking for changes (e.g. via 'git status').</summary>
public enum IgnoreSubmodulesMode
{
    /// <summary>Default is <see cref="All"/> (hides all changes to submodules).</summary>
    Default = 0,

    /// <summary>Consider a submodule modified when it either:
    ///  contains untracked or modified files,
    ///  or its HEAD differs from the commit recorded in the superproject.</summary>
    None,

    /// <summary>Submodules NOT considered dirty when they only contain <i>untracked</i> content
    ///  (but they are still scanned for modified content).</summary>
    Untracked,

    /// <summary>Ignores all changes to the work tree of submodules,
    ///  only changes to the <i>commits</i> stored in the superproject are shown.</summary>
    Dirty,

    /// <summary>Hides all changes to submodules
    ///  (and suppresses the output of submodule summaries when the config option status.submodulesummary is set).</summary>
    All
}
