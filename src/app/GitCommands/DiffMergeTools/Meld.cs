namespace GitCommands.DiffMergeTools;

internal sealed class Meld : DiffMergeTool
{
    /// <inheritdoc />
    public override string ExeFileName => OperatingSystem.IsWindows() ? "meld.exe" : "meld";

    /// <inheritdoc />
    public override string MergeCommand => "\"$LOCAL\" \"$BASE\" \"$REMOTE\" --output \"$MERGED\"";

    /// <inheritdoc />
    public override string Name => "meld";

    /// <inheritdoc />
    public override IEnumerable<string> SearchPaths => new[]
    {
        @"Meld\",
        @"Meld (x86)\"
    };
}
