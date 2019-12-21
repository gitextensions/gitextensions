using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class P4Merge : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "p4merge.exe";

        /// <inheritdoc />
        public override string MergeCommand => "\"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "p4merge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"Perforce\"
        };
    }
}