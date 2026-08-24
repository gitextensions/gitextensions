using GitExtensions.Extensibility.Git;

namespace GitUI;

/// <summary>
///  Extension methods for <see cref="IGitModule"/> that are specific to the UI layer
///  and therefore not part of the extensibility API.
/// </summary>
internal static class GitModuleExtensions
{
    /// <summary>
    ///  Determines whether a git action (rebase, merge, patch) can be automatically continued
    ///  based on the command output and the current repository state.
    /// </summary>
    /// <remarks>
    ///  Auto-continue is safe when rerere resolved all conflicts ("using previous resolution"),
    ///  the operation was not aborted, the repo is still mid-operation, and there are no remaining
    ///  unresolved conflicts that would require user intervention.
    /// </remarks>
    /// <param name="module">The git module.</param>
    /// <param name="commandOutput">The stdout/stderr output of the last git command.</param>
    /// <returns>
    ///  <see langword="true"/> if the action should be automatically continued; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool CanContinueAction(this IGitModule module, string commandOutput)
        => commandOutput.Contains("using previous resolution")
            && !commandOutput.AsSpan().TrimEnd().EndsWith("Aborted")
            && (module.InTheMiddleOfMerge() || module.InTheMiddleOfPatch() || module.InTheMiddleOfRebase())
            && !module.InTheMiddleOfConflictedMerge();
}
