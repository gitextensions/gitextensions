namespace GitExtensions.Extensibility.Git;

public interface IGitVersion : IComparable<IGitVersion>
{
    bool IsUnknown { get; }
    bool SupportAmendCommits { get; }
    bool SupportCommentStringConfig { get; }
    bool SupportGuiMergeTool { get; }
    bool SupportNewGitConfigSyntax { get; }
    bool SupportRangeDiffPath { get; }
    bool SupportRangeDiffTool { get; }
    bool SupportRebaseMerges { get; }
    bool SupportStashStaged { get; }
    bool SupportUpdateRefs { get; }

    // TODO bool SupportLsFilesFormat { get; }

    string ToString();

    public static bool operator >(IGitVersion left, IGitVersion? right) => left.CompareTo(right) > 0;
    public static bool operator <(IGitVersion left, IGitVersion? right) => left.CompareTo(right) < 0;
    public static bool operator >=(IGitVersion left, IGitVersion? right) => left.CompareTo(right) >= 0;
    public static bool operator <=(IGitVersion left, IGitVersion? right) => left.CompareTo(right) <= 0;
}
