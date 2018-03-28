using System.Text.RegularExpressions;

namespace GitCommands.Git
{
    /// <summary>Stored local modifications.</summary>
    public sealed class GitStash
    {
        private static readonly Regex _regex = new Regex(@"^stash@\{(?<index>\d+)\}: (?<message>.+)$", RegexOptions.Compiled);

        public static bool TryParse(string s, out GitStash stash)
        {
            // "stash@{i}: WIP on {branch}: {PreviousCommitMiniSHA} {PreviousCommitMessage}"
            // "stash@{i}: On {branch}: {Message}"
            // "stash@{i}: autostash"

            var match = _regex.Match(s);

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

        public override string ToString()
        {
            return Message;
        }
    }
}
