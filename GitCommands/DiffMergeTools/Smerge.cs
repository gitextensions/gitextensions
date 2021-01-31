using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class Smerge : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "smerge.exe";

        /// <inheritdoc />
        public override string DiffCommand => "mergetool \"$LOCAL\" \"$REMOTE\" -o=\"$MERGED\"";

        /// <inheritdoc />
        public override string MergeCommand => "mergetool \"$BASE\" \"$LOCAL\" \"$REMOTE\" -o=\"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "smerge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"Sublime Merge\"
        };
    }
}
