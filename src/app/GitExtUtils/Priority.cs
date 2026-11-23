using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility;

namespace GitExtUtils;

public static class Priority
{
    /// <summary>
    ///  Returns a dictionary with priorities for the items (e.g. refs, remotes) whose key matches the given list of regexes.
    /// </summary>
    /// <typeparam name="T">The type to prioritize, e.g. IGitRef.</typeparam>
    /// <param name="items">The items to prioritize.</param>
    /// <param name="keySelector">Function in T to get the sort key.</param>
    /// <param name="regexes">String with the priority regexes separated by semicolon.</param>
    /// <returns>Priorities for the items that match a regex.</returns>
    public static IReadOnlyDictionary<T, int> GetPriorityMapping<T>(this IReadOnlyList<T> items, Func<T, string> keySelector, string regexes) where T : class
    {
        return GetPriorityMapping(items, keySelector,
            regexes
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(regex => $"^({regex})$")
                .ToArray());
    }

    /// <summary>
    ///  Returns a dictionary with priorities for the items (e.g. refs, remotes) whose key matches the given list of regexes.
    /// </summary>
    /// <typeparam name="T">The type to prioritize, e.g. IGitRef.</typeparam>
    /// <param name="items">The items to prioritize.</param>
    /// <param name="keySelector">Function in T to get the sort key.</param>
    /// <param name="regexes">Array with the priority regexes.</param>
    /// <returns>Priorities for the items that match a regex.</returns>
    public static IReadOnlyDictionary<T, int> GetPriorityMapping<T>(this IReadOnlyList<T> items, Func<T, string> keySelector, string[] regexes) where T : class
    {
        if (regexes.Length == 0)
        {
            return ReadOnlyDictionary<T, int>.Empty;
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
        int minCacheSize = (regexes.Length * 2) + additionalRegexCacheEntries;
        if (Regex.CacheSize < minCacheSize)
        {
            Regex.CacheSize = minCacheSize;
        }

        Dictionary<T, int> priorityByItem = [];
        foreach (T item in items)
        {
            string key = keySelector(item);
            int currentOrder = 0;
            foreach (string regex in regexes)
            {
                try
                {
                    if (Regex.IsMatch(key, regex, RegexOptions.ExplicitCapture))
                    {
                        priorityByItem[item] = currentOrder;
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

        return priorityByItem;
    }
}
