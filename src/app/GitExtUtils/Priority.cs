using System.Text.RegularExpressions;
using GitExtensions.Extensibility;

namespace GitExtUtils;

public static class Priority
{
    /// <summary>
    ///  Returns a dictionary with priorities for the references whose key matches the given list of regexes.
    /// </summary>
    /// <typeparam name="T">The type to prioritize, e.g. IGitRef.</typeparam>
    /// <param name="references">The branches or remotes to prioritize.</param>
    /// <param name="keySelector">Function in T to get the sort key.</param>
    /// <param name="regexes">Array with the priority regexes.</param>
    /// <param name="regexList">String with the priority regexes separated by semicolon; used if<paramref name="regexes"/> is null.</param>
    /// <returns>Priorities for the references that match a regex.</returns>
    public static Dictionary<T, int> Priorities<T>(IReadOnlyList<T> references, Func<T, string> keySelector, string[]? regexes = null, string? regexList = null) where T : class
    {
        regexes ??= regexList
            ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(regex => $"^({regex})$")
            .ToArray()
            ?? throw new ArgumentException($"Both {nameof(regexes)} and {nameof(regexList)} are null");

        if (regexes.Length == 0)
        {
            return [];
        }

        // A lot of regexes will push out entries from the static regex cache, increasing the time a lot (several hundred ms).
        // (Switching the regex to the outer loop will decrease the effect but the code structure is simpler this way).
        // The static .NET regex cache must be able to include at least all regexes in the loop,
        // ideally also all common use in a "refresh" loop, so increase the size.
        // A few usages in branches vs remote in this method, CommitInfo adds usage for
        // split length of PrioritizedBranchNames (for remotes) and PrioritizedRemoteNames,
        // a few usages in submodule status processing etc.
        // This check should probably be done in a central location only at startup.
        const int additionalRegexCacheEntries = 10;
        if ((regexes.Length * 2) + additionalRegexCacheEntries > Regex.CacheSize)
        {
            Regex.CacheSize = (regexes.Length * 2) + additionalRegexCacheEntries;
        }

        Dictionary<T, int> priorityByNode = [];
        foreach (T node in references)
        {
            string key = keySelector(node);
            int currentOrder = 0;
            foreach (string regex in regexes)
            {
                try
                {
                    if (Regex.IsMatch(key, regex, RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(0.1)))
                    {
                        priorityByNode[node] = currentOrder;
                        break;
                    }
                }
                catch (RegexParseException)
                {
                    // Invalid regex - ignore
                }

                currentOrder++;
            }
        }

        return priorityByNode;
    }
}
