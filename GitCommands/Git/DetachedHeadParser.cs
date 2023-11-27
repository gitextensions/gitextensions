using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitCommands.Git
{
    public static partial class DetachedHeadParser
    {
        public static readonly string DetachedBranch = "(no branch)";

        private static readonly string[] DetachedPrefixes = { "(no branch", "(detached from ", "(HEAD detached at " };

        [GeneratedRegex(@"^\(.* (?<sha1>.*)\)$")]
        private static partial Regex ShaRegex();

        public static bool IsDetachedHead(string branch)
        {
            return DetachedPrefixes.Any(a => branch.StartsWith(a, StringComparison.Ordinal));
        }

        public static bool TryParse(string text, [NotNullWhen(returnValue: true)] out string? sha1)
        {
            sha1 = null;
            if (!IsDetachedHead(text))
            {
                return false;
            }

            Match sha1Match = ShaRegex().Match(text);
            if (!sha1Match.Success)
            {
                return false;
            }

            sha1 = sha1Match.Groups["sha1"].Value;
            return true;
        }
    }
}
