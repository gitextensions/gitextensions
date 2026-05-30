using System.Text;

namespace GitCommands.Git;

public readonly record struct AheadBehindData(string Branch,  string RemoteRef, string AheadCount, string BehindCount)
{
    // gone: "plumbing" expression, see https://git-scm.com/docs/git-for-each-ref#Documentation/git-for-each-ref.txt-upstream
    public static readonly string Gone = "gone";
    public static readonly string GoneSymbol = "✗";

    /// <summary>
    ///  Returns a string representation of the ahead/behind data, with arrows indicating the direction.
    ///  If the branch is gone, it returns "✗" (<see cref="GoneSymbol"/>).
    /// </summary>
    /// <param name="withCounts">If true, the display will include numeric counts.</param>
    /// <param name="reverse">If true, the direction of the arrows is reversed. To be used when displaying the data for a remote branch.</param>
    public string ToDisplay(bool withCounts = true, bool reverse = false)
    {
        if (AheadCount == Gone)
        {
            return GoneSymbol;
        }

        bool isBehind = BehindCount.Length > 0;

        StringBuilder sb = new();

        if (AheadCount == "0" && !isBehind)
        {
            if (withCounts)
            {
                sb.Append('0');
            }

            return sb.Append(reverse ? "↓↑" : "↑↓").ToString();
        }

        if (AheadCount.Length > 0 && AheadCount != "0")
        {
            if (withCounts)
            {
                sb.Append(AheadCount);
            }

            sb.Append(reverse ? '↓' : '↑');
            if (isBehind && withCounts)
            {
                sb.Append(' ');
            }
        }

        if (isBehind)
        {
            if (withCounts)
            {
                sb.Append(BehindCount);
            }

            sb.Append(reverse ? '↑' : '↓');
        }

        return sb.ToString();
    }
}
