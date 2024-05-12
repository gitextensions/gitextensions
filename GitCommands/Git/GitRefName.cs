using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public static partial class GitRefName
    {
        [GeneratedRegex(@"^refs/remotes/[^/]+/HEAD$", RegexOptions.ExplicitCapture)]
        private static partial Regex RemoteHeadRegex();
        [GeneratedRegex(@"^refs/remotes/(?<remote>[^/]+)", RegexOptions.ExplicitCapture)]
        private static partial Regex RemoteNameRegex();

        /// <summary>"refs/tags/".</summary>
        public static string RefsTagsPrefix { get; } = "refs/tags/";

        /// <summary>"refs/heads/".</summary>
        public static string RefsHeadsPrefix { get; } = "refs/heads/";

        /// <summary>"refs/remotes/".</summary>
        public static string RefsRemotesPrefix { get; } = "refs/remotes/";

        /// <summary>"refs/bisect/".</summary>
        public static string RefsBisectPrefix { get; } = "refs/bisect/";

        /// <summary>"refs/bisect/good".</summary>
        public static string RefsBisectGoodPrefix { get; } = "refs/bisect/good";

        /// <summary>"refs/bisect/bad".</summary>
        public static string RefsBisectBadPrefix { get; } = "refs/bisect/bad";

        /// <summary>"refs/stash".</summary>
        public static string RefsStashPrefix { get; } = "refs/stash";

        /// <summary>"refs/notes/commits".</summary>
        public static string RefsNotesPrefix { get; } = "refs/notes/commits";

        /// <summary>"^{}".</summary>
        public static string TagDereferenceSuffix { get; } = "^{}";

        [Pure]
        public static string GetRemoteName(string refName)
        {
            Match match = RemoteNameRegex().Match(refName);

            if (match.Success)
            {
                return match.Groups["remote"].Value;
            }

            // This method requires the full form of the ref path, which begins with "refs/".
            // The overload which accepts multiple remote names can be used when the format might
            // be abbreviated to "remote/branch".
            DebugHelpers.Assert(refName.StartsWith("refs/"), "Must begin with \"refs/\".");

            return string.Empty;
        }

        [Pure]
        public static string GetRemoteName(string refName, IEnumerable<string> remotes)
        {
            if (refName.StartsWith("refs/"))
            {
                return GetRemoteName(refName);
            }

            foreach (string remote in remotes)
            {
                if (refName.StartsWith(remote) && refName.Length > remote.Length && refName[remote.Length] == '/')
                {
                    return remote;
                }
            }

            return string.Empty;
        }

        [Pure]
        public static string GetRemoteBranch(string refName)
        {
            if (refName.Length <= GitRefName.RefsRemotesPrefix.Length)
            {
                return string.Empty;
            }

            int startBranch = refName.IndexOf('/', GitRefName.RefsRemotesPrefix.Length);
            if (startBranch < 0)
            {
                return string.Empty;
            }

            return refName[(1 + startBranch)..];
        }

        [Pure]
        [return: NotNullIfNotNull("branch")]
        public static string? GetFullBranchName(string? branch)
        {
            if (branch is null)
            {
                return null;
            }

            branch = branch.Trim();

            if (string.IsNullOrEmpty(branch) || branch.StartsWith("refs/"))
            {
                return branch;
            }

            // If the branch represents a commit hash, return it as-is without appending refs/heads/ (fix issue #2240)
            // NOTE: We can use `String.IsNullOrEmpty(Module.RevParse(srcRev))` instead
            if (GitRevision.Sha1HashRegex().IsMatch(branch))
            {
                return branch;
            }

            return "refs/heads/" + branch;
        }

        [Pure]
        public static bool IsRemoteHead(string refName)
        {
            return RemoteHeadRegex().IsMatch(refName);
        }
    }
}
