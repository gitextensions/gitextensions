using System.Text;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI;

public interface IFindFilePredicateProvider
{
    /// <summary>
    /// Returns the names of files that match the specified search pattern.
    /// </summary>
    /// <param name="searchPattern">The search string to match against the paths of files.</param>
    Func<string?, bool> Get(string searchPattern, string workingDir);
}

public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
{
    private static readonly StringBuilder sb = new(20);

    public Func<string?, bool> Get(string searchPattern, string workingDir)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);
        ArgumentNullException.ThrowIfNull(workingDir);

        string pattern = searchPattern.ToPosixPath();
        string dir = workingDir.ToPosixPath();

        if (pattern.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
        {
            pattern = pattern[dir.Length..].TrimStart('/');
            return fileName => fileName?.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) is true;
        }

        Regex camelHumpsRegex = BuildRegexCamelHumps(pattern);

        // Method Contains have no override with StringComparison parameter
        return fileName => fileName != null && (fileName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) is >= 0 || camelHumpsRegex.IsMatch(fileName));
    }

    private static Regex BuildRegexCamelHumps(string searchPattern)
    {
        sb.Clear();
        foreach (char c in searchPattern)
        {
            if (c == '/' || char.IsUpper(c) || char.IsAsciiDigit(c))
            {
                sb.Append(".*");
            }

            sb.Append(c);
        }

        return new Regex(sb.ToString(), RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking);
    }
}
