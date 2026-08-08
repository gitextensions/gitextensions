using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using GitExtensions.Extensibility;
using GitUIPluginInterfaces;

namespace GitCommands;

public static class GitRefName
{
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

    /// <summary>"refs/sessions/".</summary>
    public static string RefsSessionsPrefix { get; } = "refs/sessions/";

    /// <summary>"^{}".</summary>
    public static string TagDereferenceSuffix { get; } = "^{}";

    [Pure]
    public static string GetRemoteName(string refName)
    {
        if (!refName.StartsWith(RefsRemotesPrefix))
        {
            // This method requires the full form of the ref path, which begins with "refs/".
            // The overload which accepts multiple remote names can be used when the format might
            // be abbreviated to "remote/branch".
            DebugHelpers.Assert(refName.StartsWith("refs/"), "Must begin with \"refs/\".");
            return string.Empty;
        }

        ReadOnlySpan<char> afterPrefix = refName.AsSpan(RefsRemotesPrefix.Length);
        int slash = afterPrefix.IndexOf('/');
        return (slash < 0 ? afterPrefix : afterPrefix[..slash]).ToString();
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
    [return: NotNullIfNotNull(nameof(branch))]
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
        if (GitRevision.Sha1HashRegex.IsMatch(branch))
        {
            return branch;
        }

        return RefsHeadsPrefix + branch;
    }

    [Pure]
    [return: NotNullIfNotNull(nameof(branch))]
    public static string? GetFullRemoteName(string? branch, string remote)
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
        if (GitRevision.Sha1HashRegex.IsMatch(branch))
        {
            return branch;
        }

        return RefsRemotesPrefix + remote + "/" + branch;
    }

    [Pure]
    public static bool IsRemoteHead(string refName)
    {
        const string headSuffix = "/HEAD";
        if (!refName.StartsWith(RefsRemotesPrefix) || !refName.EndsWith(headSuffix))
        {
            return false;
        }

        // The remote name segment between "refs/remotes/" and "/HEAD" must be non-empty and contain no '/'
        int remoteStart = RefsRemotesPrefix.Length;
        int remoteEnd = refName.Length - headSuffix.Length;
        if (remoteEnd <= remoteStart)
        {
            return false;
        }

        return refName.AsSpan()[remoteStart..remoteEnd].IndexOf('/') < 0;
    }
}
