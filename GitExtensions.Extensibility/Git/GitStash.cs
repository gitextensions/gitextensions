using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitUIPluginInterfaces;

/// <summary>Stored local modifications.</summary>
public sealed partial class GitStash
{
    [GeneratedRegex(@"^stash@\{(?<index>\d+)\}: (?<message>.+)$", RegexOptions.ExplicitCapture)]
    private static partial Regex StashRegex();

    public static bool TryParse(string s, [NotNullWhen(returnValue: true)] out GitStash? stash)
    {
        // "stash@{i}: WIP on {branch}: {PreviousCommitMiniSHA} {PreviousCommitMessage}"
        // "stash@{i}: On {branch}: {Message}"
        // "stash@{i}: autostash"

        Match match = StashRegex().Match(s);

        if (!match.Success)
        {
            stash = default;
            return false;
        }

        stash = new GitStash(
            int.Parse(match.Groups["index"].Value),
            match.Groups["message"].Value);

        return true;
    }

    /// <summary>Short description of the commit the stash was based on.</summary>
    public string Message { get; }

    /// <summary>Gets the index of the stash in the list.</summary>
    public int Index { get; }

    public GitStash(int index, string message)
    {
        Index = index;
        Message = message;
    }

    /// <summary>Name of the stash.</summary>
    /// <remarks>"stash@{n}"</remarks>
    public string Name => $"stash@{{{Index}}}";

    public string Summary => Index == -1 ? Message : $"@{{{Index}}}: {Message}";

    public override string ToString() => Message;
}
