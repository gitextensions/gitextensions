namespace GitExtensions.Extensibility.Git;

public interface IGitVersion : IComparable<IGitVersion>
{
    bool IsUnknown { get; }
    bool SupportAmendCommits { get; }
    bool SupportGuiMergeTool { get; }
    bool SupportRangeDiffPath { get; }
    bool SupportRangeDiffTool { get; }
    bool SupportRebaseMerges { get; }
    bool SupportStashStaged { get; }
    bool SupportUpdateRefs { get; }

    string ToString();

    public static bool operator >(IGitVersion left, IGitVersion? right) => left.CompareTo(right) > 0;
    public static bool operator <(IGitVersion left, IGitVersion? right) => left.CompareTo(right) < 0;
    public static bool operator >=(IGitVersion left, IGitVersion? right) => left.CompareTo(right) >= 0;
    public static bool operator <=(IGitVersion left, IGitVersion? right) => left.CompareTo(right) <= 0;
}
