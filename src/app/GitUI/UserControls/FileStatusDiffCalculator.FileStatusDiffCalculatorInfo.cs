using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI;

partial class FileStatusDiffCalculator
{
    private record struct FileStatusDiffCalculatorInfo(
        IReadOnlyList<GitRevision> Revisions,
        ObjectId? HeadId,
        bool AllowMultiDiff,
        bool FileTreeMode,
        string GrepArguments,
        bool ShowSkipWorktreeFiles,
        UntrackedFilesMode UntrackedFilesMode);
}
