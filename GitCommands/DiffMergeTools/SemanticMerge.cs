using System;
using System.Collections.Generic;
using System.IO;

namespace GitCommands.DiffMergeTools
{
    internal class SemanticMerge : DiffMergeTool
    {
        private static readonly string[] Folders = GetFolders();

        /// <inheritdoc />
        public override string DiffCommand => "-s \"$LOCAL\" -d \"$REMOTE\"";

        /// <inheritdoc />
        public override string ExeFileName => "semanticmergetool.exe";

        /// <inheritdoc />
        public override string MergeCommand => "-s \"$REMOTE\" -d \"$LOCAL\" -b \"$BASE\" -r \"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "semanticmerge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => Folders;

        private static string[] GetFolders()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return new[]
            {
                Path.Combine(folder, @"semanticmerge"),
                Path.Combine(folder, @"PlasticSCM4\semanticmerge")
            };
        }
    }
}