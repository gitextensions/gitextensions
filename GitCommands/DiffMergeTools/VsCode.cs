namespace GitCommands.DiffMergeTools
{
    internal class VsCode : DiffMergeTool
    {
        private static readonly string[] Folders = GetFolders();

        /// <inheritdoc />
        public override string DiffCommand => "--wait --diff \"$LOCAL\" \"$REMOTE\"";

        /// <inheritdoc />
        public override string ExeFileName => "Code.exe";

        /// <inheritdoc />
        public override string MergeCommand => "--wait --merge \"$REMOTE\" \"$LOCAL\" \"$BASE\" \"$MERGED\"";

        /// <inheritdoc />
        public override string Name => "vscode";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => Folders;

        private static string[] GetFolders()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return new[]
            {
                Path.Combine(folder, @"Programs\Microsoft VS Code"),
                @"Microsoft VS Code\",
            };
        }
    }
}
