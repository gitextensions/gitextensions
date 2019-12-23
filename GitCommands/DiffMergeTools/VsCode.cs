using System.Collections.Generic;

namespace GitCommands.DiffMergeTools
{
    internal class VsCode : DiffMergeTool
    {
        /// <inheritdoc />
        public override string DiffCommand => "--wait --diff \"$LOCAL\" \"$REMOTE\"";

        /// <inheritdoc />
        public override string ExeFileName => "code.exe";

        /// <inheritdoc />
        public override string MergeCommand => "--wait \"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "vscode";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            @"Microsoft VS Code\"
        };
    }
}