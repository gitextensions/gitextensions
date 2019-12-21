namespace GitCommands.DiffMergeTools
{
    public readonly struct DiffMergeToolConfiguration
    {
        public DiffMergeToolConfiguration(string exeFileName, string path, string diffCommand, string mergeCommand)
        {
            ExeFileName = exeFileName;
            Path = path.ToPosixPath();
            DiffCommand = diffCommand ?? string.Empty;
            MergeCommand = mergeCommand ?? string.Empty;

            FullDiffCommand = string.IsNullOrWhiteSpace(DiffCommand) ? string.Empty : $"\"{Path}\" {DiffCommand}";
            FullMergeCommand = string.IsNullOrWhiteSpace(MergeCommand) ? string.Empty : $"\"{Path}\" {MergeCommand}";
        }

        public string DiffCommand { get; }
        public string MergeCommand { get; }
        public string ExeFileName { get; }
        public string Path { get; }

        public string FullDiffCommand { get; }
        public string FullMergeCommand { get; }
    }
}
