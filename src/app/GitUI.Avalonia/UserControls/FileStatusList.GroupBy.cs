using Avalonia.Media;
using GitExtensions.Extensibility.Git;

namespace GitUI;

partial class FileStatusList
{
    private sealed record GroupBy(
        Func<GitItemStatus, GroupKey> GetGroupKey,
        Func<IGrouping<GroupKey, GitItemStatus>, IImage> GetImage,
        Func<IGrouping<GroupKey, GitItemStatus>, string> GetLabel);
}
