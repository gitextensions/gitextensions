using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class TortoiseGitMerge : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "TortoiseGitMerge.exe";

        /// <inheritdoc />
        public override string MergeCommand => "-base:\"$BASE\" -mine:\"$LOCAL\" -theirs:\"$REMOTE\" -merged:\"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "tortoisemerge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"TortoiseGit\bin\",
        };
    }
}