#nullable enable

using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI;

public class FileStatusWithDescription
{
    public FileStatusWithDescription(GitRevision? firstRev, GitRevision secondRev, string summary, IReadOnlyList<GitItemStatus> statuses, ObjectId? baseA = null, ObjectId? baseB = null, string iconName = nameof(Images.Diff))
    {
        FirstRev = firstRev;
        SecondRev = secondRev ?? throw new ArgumentNullException(nameof(secondRev));
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        Statuses = statuses ?? throw new ArgumentNullException(nameof(statuses));
        BaseA = baseA;
        BaseB = baseB;
        IconName = iconName ?? throw new ArgumentNullException(nameof(iconName));
    }

    public GitRevision? FirstRev { get; }
    public GitRevision SecondRev { get; }
    public string Summary { get; }
    public IReadOnlyList<GitItemStatus> Statuses { get; }
    public ObjectId? BaseA { get; }
    public ObjectId? BaseB { get; }
    public string IconName { get; }
}
