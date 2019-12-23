using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class WinMerge : DiffMergeTool
    {
        /// <inheritdoc />
        public override string DiffCommand => "-e -u \"$LOCAL\" \"$REMOTE\"";

        /// <inheritdoc />
        public override string ExeFileName => "winmergeu.exe";

        /// <inheritdoc />
        public override string MergeCommand => "-e -u  -wl -wr -fm -dl \"Mine: $LOCAL\" -dm \"Merged: $BASE\" -dr \"Theirs: $REMOTE\" \"$LOCAL\" \"$BASE\" \"$REMOTE\" -o \"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "winmerge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"WinMerge\"
        };
    }
}