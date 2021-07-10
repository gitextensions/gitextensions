using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface IAheadBehindDataProvider
    {
        IDictionary<string, AheadBehindData>? GetData(string branchName = "");
    }

    public class AheadBehindDataProvider : IAheadBehindDataProvider
    {
        private readonly Func<IExecutable> _getGitExecutable;

        // Parse info about remote branches, see below for explanation
        // This assumes that the Git output is not localised
        private readonly Regex _aheadBehindRegEx =
            new(
                @"^((?<gone_p>gone)|((ahead\s(?<ahead_p>\d+))?(,\s)?(behind\s(?<behind_p>\d+))?)|(?<unk_p>.*?))::
                   ((?<gone_u>gone)|((ahead\s(?<ahead_u>\d+))?(,\s)?(behind\s(?<behind_u>\d+))?)|(?<unk_u>.*?))::
                   (?<remote_p>.*?)::(?<remote_u>.*?)::(?<branch>.*)$",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
        private readonly string _refFormat = @"%(push:track,nobracket)::%(upstream:track,nobracket)::%(push)::%(upstream)::%(refname:short)";

        public AheadBehindDataProvider(Func<IExecutable> getGitExecutable)
        {
            _getGitExecutable = getGitExecutable;
        }

        public IDictionary<string, AheadBehindData>? GetData(string branchName = "")
        {
            if (!AppSettings.ShowAheadBehindData)
            {
                return null;
            }

            return GetData(null, branchName);
        }

        // This method is required to facilitate unit tests
        private IDictionary<string, AheadBehindData>? GetData(Encoding? encoding, string branchName = "")
        {
            if (branchName is null)
            {
                throw new ArgumentException(nameof(branchName));
            }

            if (branchName == DetachedHeadParser.DetachedBranch)
            {
                return null;
            }

            GitArgumentBuilder aheadBehindGitCommand = new("for-each-ref")
            {
                $"--format=\"{_refFormat}\"",
                "refs/heads/" + branchName
            };

            ExecutionResult result = GetGitExecutable().Execute(aheadBehindGitCommand, outputEncoding: encoding);
            if (!result.ExitedSuccessfully || string.IsNullOrEmpty(result.StandardOutput))
            {
                return null;
            }

            var matches = _aheadBehindRegEx.Matches(result.StandardOutput);
            Dictionary<string, AheadBehindData> aheadBehindForBranchesData = new();
            foreach (Match match in matches)
            {
                var branch = match.Groups["branch"].Value;
                var remoteRef = (match.Groups["remote_p"].Success && !string.IsNullOrEmpty(match.Groups["remote_p"].Value))
                    ? match.Groups["remote_p"].Value
                    : match.Groups["remote_u"].Value;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(remoteRef))
                {
                    continue;
                }

#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
                aheadBehindForBranchesData.Add(match.Groups["branch"].Value,
                    new AheadBehindData
                    {
                        // The information is displayed in the push button, so the push info is preferred (may differ from upstream)
                        Branch = branch,
                        RemoteRef = remoteRef,
                        AheadCount =
                            // Prefer push to upstream for the count
                            match.Groups["ahead_p"].Success
                            // Single-line comment should be preceded by blank line
                            ? match.Groups["ahead_p"].Value
                            // If behind is set for push, ahead is null
                            : match.Groups["behind_p"].Success
                            ? string.Empty
                            : match.Groups["ahead_u"].Success
                            ? match.Groups["ahead_u"].Value
                            // No information about the remote branch, it is gone
                            : match.Groups["gone_p"].Success || match.Groups["gone_u"].Success
                            ? AheadBehindData.Gone
                            // If the printout is unknown (translated?), do not assume that there are "0" changes
                            : (match.Groups["unk_p"].Success && !string.IsNullOrWhiteSpace(match.Groups["unk_p"].Value))
                                || (match.Groups["unk_u"].Success && !string.IsNullOrWhiteSpace(match.Groups["unk_u"].Value))
                            ? string.Empty
                            // A remote exists, but "track" does not display the count if ahead/behind match
                            : "0",

                        // Behind do not track '0' or 'gone', only in Ahead
                        BehindCount = match.Groups["behind_p"].Success
                            ? match.Groups["behind_p"].Value
                            : !match.Groups["ahead_p"].Success
                            ? match.Groups["behind_u"].Value
                            : string.Empty
                    });
#pragma warning restore SA1515
            }

            return aheadBehindForBranchesData;
        }

        private IExecutable GetGitExecutable()
        {
            var executable = _getGitExecutable();

            if (executable is null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IExecutable)}");
            }

            return executable;
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly AheadBehindDataProvider _provider;

            public TestAccessor(AheadBehindDataProvider provider)
            {
                _provider = provider;
            }

            public IDictionary<string, AheadBehindData>? GetData(Encoding encoding, string branchName) => _provider.GetData(encoding, branchName);
        }
    }
}
