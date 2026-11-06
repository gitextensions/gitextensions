using GitExtensions.Extensibility;
using GitExtUtils;

namespace GitCommands.Git;

/// <summary>
///  Lists and deletes the refs which agentic tools leave behind, i.e. the refs hidden by <see cref="AppSettings.SessionRefsFilters"/>.
/// </summary>
public static class SessionRefs
{
    private const string _excludeOption = "--exclude=";

    /// <summary>
    ///  Gets the ref patterns of the <c>--exclude=</c> options in <paramref name="refsFilters"/>.
    /// </summary>
    public static IEnumerable<string> GetPatterns(string refsFilters)
    {
        foreach (string filter in refsFilters.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (filter.StartsWith(_excludeOption, StringComparison.Ordinal))
            {
                string pattern = filter[_excludeOption.Length..].Trim('"');
                if (pattern.Length > 0)
                {
                    yield return pattern;
                }
            }
        }
    }

    /// <summary>
    ///  Gets the full names of the refs matching the <c>--exclude=</c> patterns in <paramref name="refsFilters"/>.
    /// </summary>
    public static IReadOnlyList<string> GetRefs(IExecutable gitExecutable, string refsFilters)
    {
        string[] patterns = [.. GetPatterns(refsFilters)];

        // for-each-ref without any pattern would list all refs
        if (patterns.Length == 0)
        {
            return [];
        }

        GitArgumentBuilder args = new("for-each-ref")
        {
            "--format=%(refname)",
            string.Join(' ', patterns.Select(pattern => pattern.Quote()))
        };

        return gitExecutable.GetOutput(args).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    ///  Gets the arguments to delete the ref with the full name <paramref name="gitRef"/> (<c>git update-ref -d</c>).
    /// </summary>
    public static ArgumentString GetDeleteRefArgs(string gitRef)
    {
        return new GitArgumentBuilder("update-ref")
        {
            "-d",
            gitRef.Quote()
        };
    }
}
