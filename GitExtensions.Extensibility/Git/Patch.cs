namespace GitUIPluginInterfaces
{
    public sealed class Patch
    {
        public string Header { get; }

        public string? Index { get; }

        public PatchFileType FileType { get; }

        public string FileNameA { get; }

        public string? FileNameB { get; }

        public PatchChangeType ChangeType { get; }

        public string? Text { get; }

        public Patch(
            string header,
            string? index,
            PatchFileType fileType,
            string fileNameA,
            string? fileNameB,
            PatchChangeType changeType,
            string? text)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Index = index;
            FileType = fileType;
            FileNameA = fileNameA ?? throw new ArgumentNullException(nameof(fileNameA));
            FileNameB = fileNameB;
            ChangeType = changeType;
            Text = text;
        }
    }

    public enum PatchChangeType
    {
        NewFile,
        DeleteFile,
        ChangeFile,
        ChangeFileMode
    }

    public enum PatchFileType
    {
        Binary,
        Text
    }
}
