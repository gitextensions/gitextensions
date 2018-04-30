using System.Collections.Generic;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public class GetRefsGitCommand
    {
        private readonly IGitModule _gitModule;
        private readonly bool _tags;
        private readonly bool _branches;

        public GetRefsGitCommand(IGitModule gitModule, bool tags = true, bool branches = true)
        {
            _gitModule = gitModule;
            _tags = tags;
            _branches = branches;
        }

        public IReadOnlyList<IGitRef> Execute()
        {
            return ParseRefs(RefList());
        }

        private string RefList()
        {
            if (_tags && _branches)
            {
                return _gitModule.RunGitCmd("show-ref --dereference", GitModule.SystemEncoding);
            }

            if (_tags)
            {
                return _gitModule.RunGitCmd("show-ref --tags", GitModule.SystemEncoding);
            }

            if (_branches)
            {
                return _gitModule.RunGitCmd(@"for-each-ref --sort=-committerdate refs/heads/ --format=""%(objectname) %(refname)""", GitModule.SystemEncoding);
            }

            return "";
        }

        // TODO duplicated code from GitModule! For demo only!
        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> ParseRefs([NotNull] string refList)
        {
            // Parse lines of format:
            //
            // 69a7c7a40230346778e7eebed809773a6bc45268 refs/heads/master
            // 69a7c7a40230346778e7eebed809773a6bc45268 refs/remotes/origin/master
            // 366dfba1abf6cb98d2934455713f3d190df2ba34 refs/tags/2.51
            //
            // Lines may also use \t as a column delimiter, such as output of "ls-remote --heads origin".

            var regex = new Regex(@"^(?<objectid>[0-9a-f]{40})[ \t](?<refname>.+)$", RegexOptions.Multiline);

            var matches = regex.Matches(refList);

            var gitRefs = new List<IGitRef>();
            var headByRemote = new Dictionary<string, GitRef>();

            foreach (Match match in matches)
            {
                var refName = match.Groups["refname"].Value;
                var objectId = match.Groups["objectid"].Value;
                var remoteName = GitRefName.GetRemoteName(refName);
                var head = new GitRef(_gitModule, objectId, refName, remoteName);

                if (GitRefName.IsRemoteHead(refName))
                {
                    headByRemote[remoteName] = head;
                }
                else
                {
                    gitRefs.Add(head);
                }
            }

            // do not show default head if remote has a branch on the same commit
            foreach (var gitRef in gitRefs)
            {
                if (headByRemote.TryGetValue(gitRef.Remote, out var defaultHead) &&
                    gitRef.Guid == defaultHead.Guid)
                {
                    headByRemote.Remove(gitRef.Remote);
                }
            }

            gitRefs.AddRange(headByRemote.Values);

            return gitRefs;
        }
    }
}