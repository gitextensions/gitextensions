using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class BeyondCompare3 : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "bcomp.exe";

        /// <inheritdoc />
        public override string Name => "bc3";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"Beyond Compare 3 (x86)\",
            @"Beyond Compare 3\"
        };
    }
}