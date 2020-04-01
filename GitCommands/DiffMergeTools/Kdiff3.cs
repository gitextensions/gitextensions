using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class Kdiff3 : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "kdiff3.exe";

        /// <inheritdoc />
        public override string MergeCommand => "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "kdiff3";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            // regkdiff3path
            @"KDiff3"
        };
    }
}
