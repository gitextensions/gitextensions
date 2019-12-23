using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class DiffMerge : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "DiffMerge.exe";

        /// <inheritdoc />
        public override string MergeCommand => "-merge -result=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";

        /// <inheritdoc />
        public override string Name => "diffmerge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"SourceGear\Common\DiffMerge\",
            @"SourceGear\DiffMerge\"
        };
    }
}