using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class Meld : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "meld.exe";

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
}