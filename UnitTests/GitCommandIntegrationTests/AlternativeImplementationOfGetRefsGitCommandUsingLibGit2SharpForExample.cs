using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using LibGit2Sharp;

namespace GitCommandIntegrationTests
{
    internal class AlternativeImplementationOfGetRefsGitCommandUsingLibGit2SharpForExample
    {
        private readonly GitModule _gitModule;
        private readonly bool _tags;
        private readonly bool _branches;

        public AlternativeImplementationOfGetRefsGitCommandUsingLibGit2SharpForExample(GitModule gitModule, bool tags, bool branches)
        {
            _gitModule = gitModule;
            _tags = tags;
            _branches = branches;
        }

        public IReadOnlyList<IGitRef> Execute()
        {
            using (var repo = new Repository(_gitModule.WorkingDir))
            {
                return new ReadOnlyCollection<IGitRef>(
                    repo.Refs
                        .Where(FilterFunc())
                        .Select(ToGitRef)
                        .ToList());
            }
        }

        private IGitRef ToGitRef(Reference reference)
        {
            var refName = reference.CanonicalName;
            var objectId = reference.ResolveToDirectReference().TargetIdentifier;
            var remoteName = GitRefName.GetRemoteName(refName);
            return new GitRef(_gitModule, objectId, refName, remoteName);
        }

        private Func<Reference, bool> FilterFunc()
        {
            return r => (_tags && r.IsTag) || (_branches && r.IsLocalBranch);
        }
    }
}