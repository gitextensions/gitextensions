using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class BeyondCompare4 : DiffMergeTool
    {
        /// <inheritdoc />
        public override string ExeFileName => "bcomp.exe";

        /// <inheritdoc />
        public override string Name => "bc";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"Beyond Compare 4 (x86)\",
            @"Beyond Compare 4\"
        };
    }
}