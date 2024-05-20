namespace GitCommands.DiffMergeTools
{
    internal class TortoiseGitIDiff : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "TortoiseGitIDiff.exe";

        /// <inheritdoc />
        public override bool IsMergeTool => false;

        /// <inheritdoc />
        public override string DiffCommand => "/left:\"$LOCAL\" /right:\"$REMOTE\" /fit /overlay";

        /// <inheritdoc />
        public override string Name => "TortoiseGitIDiff";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"TortoiseGit\bin\",
        };
    }
}
