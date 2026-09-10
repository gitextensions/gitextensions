namespace GitCommands.DiffMergeTools;

internal sealed class Araxis : DiffMergeTool
{
    /// <inheritdoc />
    public override string ExeFileName => OperatingSystem.IsWindows() ? "Compare.exe" : "compare";

    /// <inheritdoc />
    public override string MergeCommand => "/merge /wait /a2 /3 \"$LOCAL\" \"$BASE\" \"$REMOTE\" \"$MERGED\"";

    /// <inheritdoc />
    public override string Name => "araxis";

    /// <inheritdoc />
    public override IEnumerable<string> SearchPaths => new[]
    {
        @"Araxis\"
    };
}
