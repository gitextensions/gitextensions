namespace GitExtensions.ParityFix;

// parity-scaffolding: Each proposed file carries its proof and every unresolved finding.
internal sealed record FileFix(string Path, string Text, int CommentsRestored, bool OrderChanged,
    IReadOnlyList<string> InitializedFields, IReadOnlyList<string> Unresolved, bool ProofPassed);

// parity-scaffolding: The dry-run and apply commands retain the same reviewable report.
internal sealed record FixRun(IReadOnlyList<FileFix> Files, int SelectedFindings, int CommentFindings, int UnresolvedOperations);
