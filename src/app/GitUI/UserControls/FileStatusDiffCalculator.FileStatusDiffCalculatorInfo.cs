using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI
{
    partial class FileStatusDiffCalculator
    {
        private struct FileStatusDiffCalculatorInfo
        {
            public IReadOnlyList<GitRevision> Revisions { get; set; }
            public ObjectId? HeadId { get; set; }
            public bool AllowMultiDiff { get; set; }
            public string GrepArguments { get; set; }
        }
    }
}
