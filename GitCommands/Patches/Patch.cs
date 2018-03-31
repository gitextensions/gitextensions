using System;
using JetBrains.Annotations;

namespace GitCommands.Patches
{
    public sealed class Patch
    {
        [NotNull]
        public string Header { get; }

        [CanBeNull]
        public string Index { get; }

        public PatchFileType FileType { get; }

        [NotNull]
        public string FileNameA { get; }

        [CanBeNull]
        public string FileNameB { get; }

        public bool IsCombinedDiff { get; }

        public PatchChangeType ChangeType { get; }

        [CanBeNull]
        public string Text { get; }

        public Patch(
            [NotNull] string header,
            [CanBeNull] string index,
            PatchFileType fileType,
            [NotNull] string fileNameA,
            [CanBeNull] string fileNameB,
            bool isCombinedDiff,
            PatchChangeType changeType,
            [CanBeNull] string text)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Index = index;
            FileType = fileType;
            FileNameA = fileNameA ?? throw new ArgumentNullException(nameof(fileNameA));
            FileNameB = fileNameB;
            IsCombinedDiff = isCombinedDiff;
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