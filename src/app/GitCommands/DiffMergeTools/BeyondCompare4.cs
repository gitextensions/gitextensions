namespace GitCommands.DiffMergeTools;

internal sealed class BeyondCompare4 : DiffMergeTool
{
    /// <inheritdoc />
    public override string ExeFileName => OperatingSystem.IsWindows() ? "bcomp.exe" : "bcompare";

    /// <inheritdoc />
    public override string Name => "bc";

    /// <inheritdoc />
    public override IEnumerable<string> SearchPaths => new[]
    {
        @"Beyond Compare 4 (x86)\",
        @"Beyond Compare 4\"
    };
}
