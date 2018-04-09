using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitCommands.Git
{
    public static class DetachedHeadParser
    {
        public static readonly string DetachedBranch = "(no branch)";

        private static readonly string[] DetachedPrefixes = { "(no branch", "(detached from ", "(HEAD detached at " };

        public static bool IsDetachedHead(string branch)
        {
            return DetachedPrefixes.Any(a => branch.StartsWith(a, StringComparison.Ordinal));
        }

        public static bool TryParse(string text, out string sha1)
        {
            sha1 = null;
            if (!IsDetachedHead(text))
            {
                return false;
            }

            var sha1Match = new Regex(@"^\(.* (?<sha1>.*)\)$").Match(text);
            if (!sha1Match.Success)
            {
                return false;
            }

            sha1 = sha1Match.Groups["sha1"].Value;
            return true;
        }
    }
}