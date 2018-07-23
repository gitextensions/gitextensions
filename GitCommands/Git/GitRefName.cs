using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class GitRefName
    {
        private static readonly Regex _remoteHeadRegex = new Regex("^refs/remotes/[^/]+/HEAD$", RegexOptions.Compiled);
        private static readonly Regex _remoteNameRegex = new Regex("^refs/remotes/([^/]+)", RegexOptions.Compiled);

        /// <summary>"refs/tags/"</summary>
        public static string RefsTagsPrefix { get; } = "refs/tags/";

        /// <summary>"refs/heads/"</summary>
        public static string RefsHeadsPrefix { get; } = "refs/heads/";

        /// <summary>"refs/remotes/"</summary>
        public static string RefsRemotesPrefix { get; } = "refs/remotes/";

        /// <summary>"refs/bisect/"</summary>
        public static string RefsBisectPrefix { get; } = "refs/bisect/";

        /// <summary>"refs/bisect/good"</summary>
        public static string RefsBisectGoodPrefix { get; } = "refs/bisect/good";

        /// <summary>"refs/bisect/bad"</summary>
        public static string RefsBisectBadPrefix { get; } = "refs/bisect/bad";

        /// <summary>"refs/stash"</summary>
        public static string RefsStashPrefix { get; } = "refs/stash";

        /// <summary>"^{}"</summary>
        public static string TagDereferenceSuffix { get; } = "^{}";

        [Pure, NotNull]
        public static string GetRemoteName([NotNull] string refName)
        {
            var match = _remoteNameRegex.Match(refName);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // This method requires the full form of the ref path, which begins with "refs/".
            // The overload which accepts multiple remote names can be used when the format might
            // be abbreviated to "remote/branch".
            Debug.Assert(refName.StartsWith("refs/"), "Must begin with \"refs/\".");

            return string.Empty;
        }

        [Pure, NotNull]
        public static string GetRemoteName([NotNull] string refName, [NotNull, ItemNotNull] IEnumerable<string> remotes)
        {
            if (refName.StartsWith("refs/"))
            {
                return GetRemoteName(refName);
            }

            foreach (var remote in remotes)
            {
                if (refName.StartsWith(remote) && refName.Length > remote.Length && refName[remote.Length] == '/')
                {
                    return remote;
                }
            }

            return string.Empty;
        }

        [Pure, CanBeNull]
        public static string GetFullBranchName([CanBeNull] string branch)
        {
            if (branch == null)
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
            if (GitRevision.Sha1HashRegex.IsMatch(branch))
            {
                return branch;
            }

            return "refs/heads/" + branch;
        }

        [Pure]
        public static bool IsRemoteHead([NotNull] string refName)
        {
            return _remoteHeadRegex.IsMatch(refName);
        }
    }
}