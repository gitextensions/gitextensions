using GitExtensions.Extensibility.Git;

namespace GitUI;

partial class FileStatusList
{
    private record GroupBy(
        Func<GitItemStatus, string> GetGroupKey,
        Func<GitItemStatus, string> GetImageKey,
        Func<string, string> GetLabel);
}
