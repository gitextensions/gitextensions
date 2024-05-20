namespace GitCommands.Git;

public readonly record struct AheadBehindData(string Branch,  string RemoteRef, string AheadCount, string BehindCount)
{
    // gone: "plumbing" expression, see https://git-scm.com/docs/git-for-each-ref#Documentation/git-for-each-ref.txt-upstream
    public static readonly string Gone = "gone";
    public string ToDisplay()
    {
        return AheadCount == Gone
            ? Gone
            : AheadCount == "0" && string.IsNullOrEmpty(BehindCount)
            ? "0↑↓"
            : (!string.IsNullOrEmpty(AheadCount) && AheadCount != "0"
                ? AheadCount + "↑" + (!string.IsNullOrEmpty(BehindCount) ? " " : string.Empty)
                : string.Empty)
            + (!string.IsNullOrEmpty(BehindCount) ? BehindCount + "↓" : string.Empty);
    }
}
