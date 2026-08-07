namespace GitCommands.DiffMergeTools;

internal sealed class SemanticMerge : DiffMergeTool
{
    private static readonly string[] Folders = GetFolders();

    /// <inheritdoc />
    public override string DiffCommand => "-s \"$LOCAL\" -d \"$REMOTE\"";

    /// <inheritdoc />
    public override string ExeFileName => OperatingSystem.IsWindows() ? "semanticmergetool.exe" : "semanticmergetool";

    /// <inheritdoc />
    public override string MergeCommand => "-s \"$REMOTE\" -d \"$LOCAL\" -b \"$BASE\" -r \"$MERGED\"";

    /// <inheritdoc />
    public override string Name => "semanticmerge";

    /// <inheritdoc />
    public override IEnumerable<string> SearchPaths => Folders;

    private static string[] GetFolders()
    {
        string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return
        [
            Path.Join(folder, @"semanticmerge"),
            Path.Join(folder, @"PlasticSCM4\semanticmerge")
        ];
    }
}
