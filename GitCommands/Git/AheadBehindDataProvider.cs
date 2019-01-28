using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public interface IAheadBehindDataProvider
    {
        IDictionary<string, AheadBehindData> GetData(string branchName = "");
    }

    public class AheadBehindDataProvider : IAheadBehindDataProvider
    {
        private readonly Func<IExecutable> _getGitExecutable;

        // TODO handle [gone] status to show that remote branch no longer exists
        private readonly Regex _aheadBehindRegEx =
            new Regex(@"^(\[(ahead (?<ahead_p>\d+))?(, )?(behind (?<behind_p>\d+))?\])?::(\[(ahead (?<ahead_u>\d+))?(, )?(behind (?<behind_u>\d+))?\])?::(?<branch>.*)$",
                RegexOptions.Compiled | RegexOptions.Multiline);

        public AheadBehindDataProvider(Func<IExecutable> getGitExecutable)
        {
            _getGitExecutable = getGitExecutable;
        }

        [CanBeNull]
        public IDictionary<string, AheadBehindData> GetData(string branchName = "")
        {
            if (!AppSettings.ShowAheadBehindData)
            {
                return null;
            }

            return GetData(null, branchName);
        }

        // This method is required to facilitate unit tests
        private IDictionary<string, AheadBehindData> GetData(Encoding encoding, string branchName = "")
        {
            if (branchName == null)
            {
                throw new ArgumentException(nameof(branchName));
            }

            if (branchName == "(no branch)")
            {
                return null;
            }

            var aheadBehindGitCommand = new GitArgumentBuilder("for-each-ref")
            {
                "--format=\"%(push:track)::%(upstream:track)::%(refname:short)\"",
                "refs/heads/" + branchName
            };

            var result = GetGitExecutable().GetOutput(aheadBehindGitCommand, outputEncoding: encoding);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            var matches = _aheadBehindRegEx.Matches(result);
            if (matches.Count < 1 || (matches.Count == 1 && (!matches[0].Groups["branch"].Success ||
                                                             !(matches[0].Groups["ahead_p"].Success || matches[0].Groups["ahead_u"].Success ||
                                                               matches[0].Groups["behind_p"].Success || matches[0].Groups["behind_u"].Success))))
            {
                return null;
            }

            var aheadBehindForBranchesData = new Dictionary<string, AheadBehindData>();
            foreach (Match match in matches)
            {
                aheadBehindForBranchesData.Add(match.Groups["branch"].Value,
                    new AheadBehindData
                    {
                        // The information is displayed in the push button, so the push info  is preferred (may differ from upstream)
                        Branch = match.Groups["branch"].Value,
                        AheadCount = match.Groups["ahead_p"].Success ? match.Groups["ahead_p"].Value : match.Groups["ahead_u"].Value,
                        BehindCount = match.Groups["behind_p"].Success ? match.Groups["behind_p"].Value : match.Groups["behind_u"].Value
                    });
            }

            return aheadBehindForBranchesData;
        }

        [NotNull]
        private IExecutable GetGitExecutable()
        {
            var executable = _getGitExecutable();

            if (executable == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IExecutable)}");
            }

            return executable;
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        public readonly struct TestAccessor
        {
            private readonly AheadBehindDataProvider _provider;

            public TestAccessor(AheadBehindDataProvider provider)
            {
                _provider = provider;
            }

            public IDictionary<string, AheadBehindData> GetData(Encoding encoding, string branchName) => _provider.GetData(encoding, branchName);
        }
    }
}
